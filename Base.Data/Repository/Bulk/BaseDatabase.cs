using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data.Repository._BulkBase
{
    public class BaseDatabase : IDisposable
    {
        public string Server { get; private set; }
        public string Catalog { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public int Timeout { get; private set; }
        private string ConnectionString { get; set; }
        public SqlConnection Connection { get; set; }

        public bool disposed;

        public BaseDatabase(string connectionStringName = "BaseContext", int timeOut = 20000)
        {
            var config = GetDbConfig(connectionStringName);
            Server = config["data source"];
            Catalog = config["initial catalog"];
            Password = config["password"];
            Timeout = timeOut;
            Username = config["user id"];
            UpdateConnectionParams();
        }

        public void UpdateConnectionParams(string server, string catalog, string username, string password, int timeout)
        {
            Server = server;
            Catalog = catalog;
            Username = username;
            Password = password;
            Timeout = timeout;

            ValidateParameters();

            ConnectionString = $"Data Source={Server};Initial Catalog={Catalog};User={Username};Password={Password};Connection Timeout = {Timeout};";
            Connection = new SqlConnection(ConnectionString);
        }

        public void UpdateConnectionParams()
        {
            UpdateConnectionParams(Server, Catalog, Username, Password, Timeout);
        }

        private Dictionary<string, string> GetDbConfig(string connectionString)
        {
            var config = new Dictionary<string, string>();

            var defaultConnection = ConfigurationManager.ConnectionStrings[connectionString]?.ConnectionString;
            var azureFunctionsConnection = Environment.GetEnvironmentVariable(connectionString);
            var apoloConnection = GetCoreConnectionString();

            var connection = defaultConnection ?? azureFunctionsConnection ?? apoloConnection;

            if (connection == null)
                return null;

            var keyValue = connection.Split(';');

            foreach (var item in keyValue)
            {
                var keyValueSplit = item.Split('=');
                if (keyValueSplit.Length < 2)
                    continue;
                if (keyValueSplit[0] == "provider connection string" && keyValueSplit.Length > 2)
                    config.Add(keyValueSplit[1].Replace("\"", "").ToLower(), keyValueSplit[2]);
                else
                    config.Add(keyValueSplit[0].ToLower(), keyValueSplit[1]);
            }
            return config;
        }

        private string GetCoreConnectionString()
        {
            var appSettingsPath = Directory.GetCurrentDirectory() + "/appsettings.json";

            if (File.Exists(appSettingsPath))
            {
                var builder = new ConfigurationBuilder().AddJsonFile(appSettingsPath);
                var configuration = builder.Build();
                return configuration.GetConnectionString("BaseContext");
            }

            return "";
        }

        private void ValidateParameters()
        {
            if (string.IsNullOrWhiteSpace(Server))
                throw new Exception("Parâmetro Server não pode ser nulo ou vazio.");

            if (string.IsNullOrWhiteSpace(Catalog))
                throw new Exception("Parâmetro Catalog não pode ser nulo ou vazio.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new Exception("Parâmetro Username não pode ser nulo ou vazio.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new Exception("Parâmetro Password não pode ser nulo ou vazio.");

            if (Timeout < 30)
                throw new Exception("Parâmetro Timeout não pode ser menor que 30.");
        }

        public static object ChangeType(object value, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (value == null)
                    return null;

                type = Nullable.GetUnderlyingType(type);
            }

            return Convert.ChangeType(value, type);
        }

        public bool BulkInsert(DataTable internalTable)
        {
            var bulkSql = new SqlBulkCopy
                    (
                        Connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers |
                        SqlBulkCopyOptions.UseInternalTransaction,
                        null
                    );
            try
            {
                bulkSql.DestinationTableName = internalTable.TableName;
                bulkSql.BulkCopyTimeout = Timeout;
                foreach (DataColumn coluna in internalTable.Columns)
                {
                    if ((coluna.Table.TableName == "ExtraCards" || coluna.Table.TableName == "OrderItems" || coluna.Table.TableName == "OrderItemsDeleted") && (coluna.Caption == "Monthly" || coluna.Caption == "Formatting"))
                        continue;

                    bulkSql.ColumnMappings.Add(coluna.ColumnName, coluna.ColumnName);
                }
                OpenConnection();

                bulkSql.WriteToServer(internalTable);
                CloseConnection();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public OperationResult BulkInsertWithTransaction(DataTable internalTable, SqlTransaction transaction, bool? returnException = false)
        {
            var bulkSql = new SqlBulkCopy
                    (
                        Connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers,
                        transaction
                    );
            try
            {
                bulkSql.DestinationTableName = internalTable.TableName;
                bulkSql.BulkCopyTimeout = Timeout;
                foreach (DataColumn coluna in internalTable.Columns)
                {
                    if ((coluna.Table.TableName == "ExtraCards" || coluna.Table.TableName == "OrderItems" || coluna.Table.TableName == "OrderItemsDeleted") && (coluna.Caption == "Monthly" || coluna.Caption == "Formatting"))
                        continue;

                    bulkSql.ColumnMappings.Add(coluna.ColumnName, coluna.ColumnName);
                }

                bulkSql.NotifyAfter = 10000;
                bulkSql.SqlRowsCopied += new SqlRowsCopiedEventHandler(s_SqlRowsCopied);
                bulkSql.WriteToServer(internalTable);

                return new OperationResult();
            }
            catch (Exception ex)
            {
                if (!returnException.GetValueOrDefault())
                    return new OperationResult(false, ex.Message);

                throw ex;
            }
        }

        static void s_SqlRowsCopied(Object remetente, SqlRowsCopiedEventArgs e)
        {
            Console.WriteLine("- Copiado {0} linhas.", e.RowsCopied);
        }

        public void OpenConnection()
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        public void CloseConnection()
        {
            if (Connection.State != ConnectionState.Closed)
                Connection.Close();
        }

        public DataTable CreateInternalTableWithQuery(string query, List<string> keys, string tableName)
        {
            try
            {
                var command = new SqlCommand(query, Connection);
                Connection.Open();
                var result = command.ExecuteReader();
                var internalTableReturn = new DataTable();

                var coluna = 0;
                var estruturaDaTabela = result.GetSchemaTable();
                if (estruturaDaTabela == null)
                    return null;
                foreach (DataRow r in estruturaDaTabela.Rows)
                {
                    if (!internalTableReturn.Columns.Contains(r["ColumnName"].ToString()))
                    {
                        var tipoDaColuna = result.GetDataTypeName(coluna);
                        switch (tipoDaColuna)
                        {
                            case "uniqueidentifier":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(Guid));
                                break;
                            case "int":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(int));
                                break;
                            case "date":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(DateTime));
                                break;
                            case "datetime":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(DateTime));
                                break;
                            case "decimal":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(decimal));
                                break;
                            case "float":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(float));
                                break;
                            case "varbinary":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(byte[]));
                                break;
                            case "bit":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(bool));
                                break;
                            default:
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(string));
                                break;
                        }
                        internalTableReturn.Columns[r["ColumnName"].ToString()].Unique = Convert.ToBoolean(r["IsUnique"]);
                        internalTableReturn.Columns[r["ColumnName"].ToString()].AllowDBNull = Convert.ToBoolean(r["AllowDBNull"]);
                        internalTableReturn.Columns[r["ColumnName"].ToString()].ReadOnly = Convert.ToBoolean(r["IsReadOnly"]);
                    }
                    coluna++;
                }
                while (result.Read())
                {

                    var newLine = internalTableReturn.NewRow();
                    for (var i = 0; i < internalTableReturn.Columns.Count; i++)
                    {
                        var columnType = internalTableReturn.Columns[i].DataType;
                        if (columnType.Name == "Byte[]")
                        {
                            if (!result.IsDBNull(i))
                            {
                                var file = result.GetSqlBytes(i).Buffer;
                                newLine[i] = file;
                            }
                        }
                        else
                        {
                            newLine[i] = result.GetValue(i);
                        }
                    }
                    internalTableReturn.Rows.Add(newLine);
                }
                CloseConnection();

                if (keys != null && keys.Any())
                {
                    var keysDataColumns = new DataColumn[keys.Count()];
                    for (var i = 0; i < keys.Count(); i++)
                    {
                        keysDataColumns[i] = internalTableReturn.Columns[keys[i]];
                    }
                    internalTableReturn.PrimaryKey = keysDataColumns;
                }

                if (!string.IsNullOrEmpty(tableName))
                    internalTableReturn.TableName = tableName;

                return internalTableReturn;
            }
            catch (Exception)
            {
                CloseConnection();
                return null;
            }
        }

        public DataTable CreateInternalTableWithQuery(string query, List<string> keys, string tableName, Dictionary<string, object> parameters)
        {
            try
            {
                var command = new SqlCommand(query, Connection);
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
                }
                Connection.Open();
                var result = command.ExecuteReader();
                var internalTableReturn = new DataTable();

                var coluna = 0;
                var estruturaDaTabela = result.GetSchemaTable();
                if (estruturaDaTabela == null)
                    return null;
                foreach (DataRow r in estruturaDaTabela.Rows)
                {
                    if (!internalTableReturn.Columns.Contains(r["ColumnName"].ToString()))
                    {
                        var tipoDaColuna = result.GetDataTypeName(coluna);
                        switch (tipoDaColuna)
                        {
                            case "uniqueidentifier":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(Guid));
                                break;
                            case "int":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(int));
                                break;
                            case "date":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(DateTime));
                                break;
                            case "datetime":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(DateTime));
                                break;
                            case "decimal":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(decimal));
                                break;
                            case "float":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(float));
                                break;
                            case "varbinary":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(byte[]));
                                break;
                            case "bit":
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(bool));
                                break;
                            default:
                                internalTableReturn.Columns.Add(r["ColumnName"].ToString(), typeof(string));
                                break;
                        }
                        internalTableReturn.Columns[r["ColumnName"].ToString()].Unique = Convert.ToBoolean(r["IsUnique"]);
                        internalTableReturn.Columns[r["ColumnName"].ToString()].AllowDBNull = Convert.ToBoolean(r["AllowDBNull"]);
                        internalTableReturn.Columns[r["ColumnName"].ToString()].ReadOnly = Convert.ToBoolean(r["IsReadOnly"]);
                    }
                    coluna++;
                }
                while (result.Read())
                {

                    var newLine = internalTableReturn.NewRow();
                    for (var i = 0; i < internalTableReturn.Columns.Count; i++)
                    {
                        var columnType = internalTableReturn.Columns[i].DataType;
                        if (columnType.Name == "Byte[]")
                        {
                            if (!result.IsDBNull(i))
                            {
                                var file = result.GetSqlBytes(i).Buffer;
                                newLine[i] = file;
                            }
                        }
                        else
                        {
                            newLine[i] = result.GetValue(i);
                        }
                    }
                    internalTableReturn.Rows.Add(newLine);
                }
                CloseConnection();
                if (keys != null && keys.Any())
                {
                    var keysDataColumns = new DataColumn[keys.Count()];
                    for (var i = 0; i < keys.Count(); i++)
                    {
                        keysDataColumns[i] = internalTableReturn.Columns[keys[i]];
                    }
                    internalTableReturn.PrimaryKey = keysDataColumns;
                }

                if (!string.IsNullOrEmpty(tableName))
                    internalTableReturn.TableName = tableName;

                return internalTableReturn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string CreateQueryToUpdate(DataTable internalTable, string temporaryTableName)
        {
            var fromTo = $"UPDATE {internalTable.TableName } SET ";
            for (var coluna = 0; coluna < internalTable.Columns.Count; coluna++)
            {
                if (internalTable.TableName == "OrderItems" && (internalTable.Columns[coluna].ColumnName == "Monthly" || internalTable.Columns[coluna].ColumnName == "Formatting"))
                    continue;

                if (coluna == 0)
                {
                    fromTo += string.Concat(" ", internalTable.TableName, ".", internalTable.Columns[coluna].ColumnName, " = ", temporaryTableName, ".", internalTable.Columns[coluna].ColumnName);
                    continue;
                }
                fromTo += string.Concat(",", internalTable.TableName, ".", internalTable.Columns[coluna].ColumnName, " = ", temporaryTableName, ".", internalTable.Columns[coluna].ColumnName);
            }
            fromTo += $" FROM {internalTable.TableName } INNER JOIN { temporaryTableName} ON ";
            for (var i = 0; i < internalTable.PrimaryKey.Count(); i++)
            {
                if (i != 0)
                    fromTo += " AND ";
                fromTo += string.Concat(temporaryTableName, ".", internalTable.PrimaryKey[i].ColumnName, " = ", internalTable.TableName, ".", internalTable.PrimaryKey[i].ColumnName);
            }
            return fromTo;
        }

        private string CreateTemporaryTableForUpdateCreationQuery(DataTable internalTable, string temporaryTableName)
        {
            var fromTo = $"CREATE TABLE {temporaryTableName} (";
            for (var column = 0; column < internalTable.Columns.Count; column++)
            {
                if (column != 0)
                    fromTo += ",";
                fromTo += string.Concat(" ", internalTable.Columns[column].ColumnName, " ", TypeDefine(internalTable.Columns[column].DataType, internalTable.PrimaryKey.Any(i => i.ColumnName == internalTable.Columns[column].ColumnName)));
            }
            fromTo += ") ";

            var clusterIndex = $"CREATE UNIQUE CLUSTERED INDEX {temporaryTableName} ON {temporaryTableName} (";

            for (var i = 0; i < internalTable.PrimaryKey.Count(); i++)
            {
                if (i < internalTable.PrimaryKey.Count() - 1)
                    clusterIndex += internalTable.PrimaryKey[i].ColumnName + ",";
                else
                    clusterIndex += internalTable.PrimaryKey[i].ColumnName + ");";
            }
            if (internalTable.PrimaryKey.Any())
                fromTo += clusterIndex;

            return fromTo;
        }

        private string TypeDefine(Type columnType, bool key)
        {
            switch (columnType.Name)
            {
                case "Guid":
                    return "uniqueidentifier";
                case "Int":
                    return "int";
                case "Int32":
                    return "int";
                case "DateTime":
                    return "datetime";
                case "Decimal":
                    return "decimal(18, 2)";
                case "TimeSpan":
                    return "time(7)";
                case "Double":
                    return "float";
                default:
                    return (key) ? "varchar(450)" : "varchar(max)";
            }
        }

        private string CreateQueryToDelete(string field, string id, string tableName)
        {
            var fromTo = new StringBuilder();

            fromTo.Append(String.Concat("DELETE FROM ", tableName, " WHERE ", field, " ='", id, "'"));

            return fromTo.ToString();
        }

        public OperationResult UpdateUsingDataTable(DataTable internalTable)
        {
            try
            {
                OpenConnection();

                OperationResult operationResult;

                var temporaryTableName = string.Concat("temp_", Guid.NewGuid().ToString("N").ToUpper());
                var temporaryTableCreationQuery = CreateTemporaryTableForUpdateCreationQuery(internalTable, temporaryTableName);
                operationResult = ExecuteNonQuery(temporaryTableCreationQuery);

                if (operationResult.Success)
                {
                    var originalTableName = internalTable.TableName;
                    internalTable.TableName = temporaryTableName;
                    operationResult = BulkInsertWithoutClosingConnection(internalTable);
                    if (operationResult.Success)
                    {
                        internalTable.TableName = originalTableName;
                        var queryForUpdate = CreateQueryToUpdate(internalTable, temporaryTableName);
                        operationResult = ExecuteNonQuery(queryForUpdate);
                    }

                    if (operationResult.Success)
                        operationResult = ExecuteNonQuery(string.Concat("DROP TABLE ", temporaryTableName, " ;"));

                    CloseConnection();
                    return operationResult;
                }

                return operationResult;
            }
            catch (Exception ex)
            {
                CloseConnection();
                return new OperationResult(false, ex.Message);
            }
        }

        public OperationResult BulkInsertWithoutClosingConnection(DataTable internalTable, bool? returnException = false)
        {
            var bulkSql = new SqlBulkCopy(Connection);
            try
            {
                bulkSql.DestinationTableName = internalTable.TableName;
                foreach (DataColumn coluna in internalTable.Columns)
                {
                    if ((coluna.Table.TableName == "ExtraCards" || coluna.Table.TableName == "OrderItems" || coluna.Table.TableName == "OrderItemsDeleted") && (coluna.Caption == "Monthly" || coluna.Caption == "Formatting"))
                        continue;

                    bulkSql.ColumnMappings.Add(coluna.ColumnName, coluna.ColumnName);
                }

                bulkSql.WriteToServer(internalTable);
                return new OperationResult();
            }
            catch (Exception ex)
            {
                if (!returnException.GetValueOrDefault())
                    return new OperationResult(false, ex.Message);

                throw ex;
            }
        }

        public OperationResult ExecuteNonQuery(string queryText)

        {
            try
            {
                new SqlCommand(queryText, Connection).ExecuteNonQuery();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        public OperationResult UpdateUsingDataTableWithTransaction(DataTable internalTable, SqlTransaction transaction)
        {
            try
            {
                var temporaryTableName = string.Concat("temp_", Guid.NewGuid().ToString("N").ToUpper());
                var temporaryTableCreationQuery = CreateTemporaryTableForUpdateCreationQuery(internalTable, temporaryTableName);
                ExecuteNonQueryWithTransaction(temporaryTableCreationQuery, transaction);

                var originalTableName = internalTable.TableName;
                internalTable.TableName = temporaryTableName;
                var filledTemporaryTable = BulkInsertWithTransaction(internalTable, transaction);
                if (filledTemporaryTable.Success)
                {
                    internalTable.TableName = originalTableName;
                    var queryForUpdate = CreateQueryToUpdate(internalTable, temporaryTableName);
                    ExecuteNonQueryWithTransaction(queryForUpdate, transaction);
                    ExecuteNonQueryWithTransaction(string.Concat("DROP TABLE ", temporaryTableName, " ;"), transaction);
                    return new OperationResult();
                }

                return filledTemporaryTable;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
            //CloseConnection();
        }

        private bool ExecuteNonQueryWithTransaction(string queryText, SqlTransaction tran)
        {
            try
            {
                new SqlCommand(queryText, Connection, tran).ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeleteByIdUsingDataTableWithTransaction(string field, string id, string tableName, SqlTransaction transaction, bool openAndCloseConnection = true)
        {
            try
            {
                if (openAndCloseConnection)
                    OpenConnection();

                var queryForDelete = CreateQueryToDelete(field, id, tableName);
                ExecuteNonQueryWithTransaction(queryForDelete, transaction);

                if (openAndCloseConnection)
                    CloseConnection();

                return true;
            }
            catch (Exception ex)
            {
                if (openAndCloseConnection)
                    CloseConnection();
                return false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    CloseConnection();
                    Connection.Dispose();
                }
            }

            Server = null;
            Catalog = null;
            Username = null;
            Password = null;
            Timeout = 0;
            ConnectionString = null;
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
