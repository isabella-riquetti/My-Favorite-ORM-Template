using Base.Data.Context;
using Base.Data.Helper;
using Base.Data.Models;
using Base.Data.Repository._BulkBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Data.Bulk
{
    public class BulkWorker<TEntity> : IBulkWorker<TEntity> where TEntity : ModelBase
    {
        private readonly IBaseContext _context;

        public BulkWorker(IBaseContext context)
        {
            _context = context;
        }

        public OperationResult BulkAddWithTransaction<T>(T entity, SqlTransaction sqlTransaction)
        {
            var tableName = ((IModelBase)entity).TableName;
            var primaryKey = ((IModelBase)entity).PrimaryKey;

            OperationResult operationResult;

            var entityDatatable = new[] { entity }.ToList().ToDataTable(tableName, new[] { primaryKey });
            operationResult = BulkInsertWithTransaction(entityDatatable, sqlTransaction);

            if (!operationResult.Success)
                sqlTransaction.Rollback();

            return operationResult;
        }

        public OperationResult BulkAddRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction)
        {
            if (!entities.Any())
                return new OperationResult()
                {
                    Message = "Lista vazia",
                    Success = true
                };

            var tableName = ((IModelBase)entities.First()).TableName;
            var primaryKey = ((IModelBase)entities.First()).PrimaryKey;

            OperationResult operationResult;

            var entityDatatable = entities.ToList().ToDataTable(tableName, new[] { primaryKey });
            operationResult = BulkInsertWithTransaction(entityDatatable, sqlTransaction);

            if (!operationResult.Success)
                sqlTransaction.Rollback();

            return operationResult;
        }

        public OperationResult BulkEditWithTransaction<T>(T entity, SqlTransaction sqlTransaction)
        {
            var tableName = ((IModelBase)entity).TableName;
            var primaryKey = ((IModelBase)entity).PrimaryKey;

            OperationResult operationResult;

            var entityDatatable = new[] { entity }.ToList().ToDataTable(tableName, new[] { primaryKey });
            operationResult = UpdateUsingDataTableWithTransaction(entityDatatable, sqlTransaction);

            if (!operationResult.Success)
                sqlTransaction.Rollback();

            return operationResult;
        }

        public OperationResult BulkEditRangeWithTransaction<T>(IEnumerable<T> entities, SqlTransaction sqlTransaction)
        {
            if (!entities.Any())
                return new OperationResult()
                {
                    Message = "Lista vazia",
                    Success = true
                };

            var tableName = ((IModelBase)entities.First()).TableName;
            var primaryKey = ((IModelBase)entities.First()).PrimaryKey;

            OperationResult operationResult;

            var entityDatatable = entities.ToList().ToDataTable(tableName, new[] { primaryKey });
            operationResult = UpdateUsingDataTableWithTransaction(entityDatatable, sqlTransaction);

            if (!operationResult.Success)
                sqlTransaction.Rollback();

            return operationResult;
        }

        public OperationResult BulkDeleteWithTransaction<T>(T entity, Guid idToRemove, SqlTransaction sqlTransaction)
        {
            var tableName = ((IModelBase)entity).TableName;
            var primaryKey = ((IModelBase)entity).PrimaryKey;

            OperationResult operationResult;
            var count = 0;

            operationResult = DeleteByIdsUsingDataTableWithTransaction(primaryKey, new[] { idToRemove }.ToList(), tableName, sqlTransaction);

            if (!operationResult.Success)
                sqlTransaction.Rollback();

            return operationResult;
        }

        public OperationResult BulkDeleteRangeWithTransaction<T>(IEnumerable<T> entities, List<Guid> idsToRemove, SqlTransaction sqlTransaction)
        {
            if (!entities.Any())
                return new OperationResult()
                {
                    Message = "Lista vazia",
                    Success = true
                };

            var tableName = ((IModelBase)entities.First()).TableName;
            var primaryKey = ((IModelBase)entities.First()).PrimaryKey;

            OperationResult operationResult;
            var count = 0;

            operationResult = DeleteByIdsUsingDataTableWithTransaction(primaryKey, idsToRemove, tableName, sqlTransaction);

            if (!operationResult.Success)
                sqlTransaction.Rollback();

            return operationResult;
        }

        public OperationResult BulkAddRange<T>(IEnumerable<T> entities, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            if (!entities.Any())
                return new OperationResult()
                {
                    Message = "Lista vazia",
                    Success = true
                };

            var tableName = ((IModelBase)entities.First()).TableName;
            var primaryKey = ((IModelBase)entities.First()).PrimaryKey;

            var entityDatatable = entities.ToList().ToDataTable(tableName, new[] { primaryKey });
            return BulkInsert(entityDatatable, timeOut, connectionStringName);
        }
        public OperationResult BulkEdit<T>(T entity, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            var tableName = ((IModelBase)entity).TableName;
            var primaryKey = ((IModelBase)entity).PrimaryKey;

            using (var connection = new BaseDatabase(connectionStringName, timeOut).Connection)
            {
                connection.Open();

                var sqlTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                try
                {
                    OperationResult operationResult;

                    var entityDatatable = new[] { entity }.ToList().ToDataTable(tableName, new[] { primaryKey });
                    operationResult = UpdateUsingDataTableWithTransaction(entityDatatable, sqlTransaction);

                    if (operationResult.Success)
                        sqlTransaction.Commit();
                    else if (sqlTransaction != null)
                        sqlTransaction.Rollback();

                    return operationResult;
                }
                catch (Exception ex)
                {
                    if (sqlTransaction != null)
                        sqlTransaction.Rollback();

                    return new OperationResult(false, ex.Message);
                }
            }
        }
        public OperationResult BulkEditRange<T>(IEnumerable<T> entities, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            if (!entities.Any())
                return new OperationResult()
                {
                    Message = "Lista vazia",
                    Success = true
                };

            var tableName = ((IModelBase)entities.First()).TableName;
            var primaryKey = ((IModelBase)entities.First()).PrimaryKey;

            using (var connection = new BaseDatabase(connectionStringName, timeOut).Connection)
            {
                connection.Open();

                var sqlTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                try
                {
                    OperationResult operationResult;

                    var entityDatatable = entities.ToList().ToDataTable(tableName, new[] { primaryKey });
                    operationResult = UpdateUsingDataTableWithTransaction(entityDatatable, sqlTransaction);

                    if (operationResult.Success)
                        sqlTransaction.Commit();
                    else if (sqlTransaction != null)
                        sqlTransaction.Rollback();

                    return operationResult;
                }
                catch (Exception ex)
                {
                    if (sqlTransaction != null)
                        sqlTransaction.Rollback();

                    return new OperationResult(false, ex.Message);
                }
            }
        }

        public OperationResult BulkDeleteRange<T>(IEnumerable<T> entities, List<Guid> idsToRemove, int timeOut = 20000,
          string connectionStringName = "BaseContext")
        {
            if (!entities.Any())
                return new OperationResult()
                {
                    Message = "Lista vazia",
                    Success = true
                };

            using (var connection = new BaseDatabase(connectionStringName, timeOut).Connection)
            {
                connection.Open();

                var sqlTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                if (!entities.Any())
                    return new OperationResult()
                    {
                        Message = "Lista vazia",
                        Success = true
                    };

                var tableName = ((IModelBase)entities.First()).TableName;
                var primaryKey = ((IModelBase)entities.First()).PrimaryKey;

                OperationResult operationResult;
                var count = 0;

                operationResult = DeleteByIdsUsingDataTableWithTransaction(primaryKey, idsToRemove, tableName, sqlTransaction);

                if (operationResult.Success)
                    sqlTransaction.Commit();
                else if (sqlTransaction != null)
                    sqlTransaction.Rollback();

                return operationResult;
            }
        }

        public OperationResult BulkDelete<T>(T entity, Guid idToRemove, int timeOut = 20000,
            string connectionStringName = "BaseContext")
        {
            using (var connection = new BaseDatabase(connectionStringName, timeOut).Connection)
            {
                connection.Open();

                var sqlTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);

                var tableName = ((IModelBase)entity).TableName;
                var primaryKey = ((IModelBase)entity).PrimaryKey;

                OperationResult operationResult;
                var count = 0;

                operationResult = DeleteByIdsUsingDataTableWithTransaction(primaryKey, new[] { idToRemove }.ToList(), tableName, sqlTransaction);

                if (operationResult.Success)
                    sqlTransaction.Commit();
                else if (sqlTransaction != null)
                    sqlTransaction.Rollback();

                return operationResult;
            }
        }

        public OperationResult BulkAdd<T>(T entity, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            var tableName = ((IModelBase)entity).TableName;
            var primaryKey = ((IModelBase)entity).PrimaryKey;

            var entityDatatable = new[] { entity }.ToList().ToDataTable(tableName, new[] { primaryKey });
            return BulkInsert(entityDatatable, timeOut, connectionStringName);
        }

        private OperationResult DeleteByIdsUsingDataTableWithTransaction(string field, List<Guid> ids, string tableName, SqlTransaction transaction)
        {
            try
            {
                OperationResult operationResult;
                var query = string.Empty;

                var objectTempToDelete = ids.Select(id => new Models.ObjectTempToDelete
                {
                    ObjectTempToDeleteId = id
                }).ToList();

                var temporaryTableName = $"tempdelete_{Guid.NewGuid().ToString("N").ToUpper()}";

                var deleteDatatable = objectTempToDelete.ToList().ToDataTable(temporaryTableName, new[] { "ObjectTempToDeleteId" });
                var queryTempTable = CreateTemporaryTableForUpdateCreationQuery(deleteDatatable, temporaryTableName);
                operationResult = ExecuteNonQueryWithTransaction(queryTempTable, transaction);

                if (operationResult.Success)
                {
                    operationResult = BulkInsertWithTransaction(deleteDatatable, transaction);

                    var queryDelete = $@"DELETE originTab
                                  FROM {tableName} as originTab
                                       inner join {temporaryTableName} as tempTable 
                                          on tempTable.ObjectTempToDeleteId=originTab.{field};";

                    operationResult = ExecuteNonQueryWithTransaction(queryDelete, transaction);

                    ExecuteNonQueryWithTransaction(string.Concat("DROP TABLE ", temporaryTableName, " ;"), transaction);
                }

                return operationResult;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }

        private OperationResult BulkInsert(DataTable internalTable, int timeOut = 20000, string connectionStringName = "BaseContext")
        {
            using (var connection = new BaseDatabase(connectionStringName, timeOut).Connection)
            {
                var bulkSql = new SqlBulkCopy
                    (
                        connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers,
                        null
                    );
                try
                {
                    bulkSql.DestinationTableName = internalTable.TableName;
                    bulkSql.BulkCopyTimeout = timeOut;
                    foreach (DataColumn coluna in internalTable.Columns)
                    {
                        if ((coluna.Table.TableName == "ExtraCards" ||
                            coluna.Table.TableName == "OrderItems" ||
                            coluna.Table.TableName == "OrderItemsDeleted")
                            && (coluna.Caption == "Monthly" || coluna.Caption == "Formatting"))
                            continue;

                        bulkSql.ColumnMappings.Add(coluna.ColumnName, coluna.ColumnName);
                    }

                    connection.Open();
                    bulkSql.WriteToServer(internalTable);
                    connection.Close();

                    return new OperationResult();
                }
                catch (Exception ex)
                {
                    return new OperationResult(false, ex.Message);
                }
            }
        }

        private OperationResult BulkInsertWithTransaction(DataTable internalTable, SqlTransaction transaction)
        {
            var bulkSql = new SqlBulkCopy
                    (
                        transaction.Connection,
                        SqlBulkCopyOptions.TableLock |
                        SqlBulkCopyOptions.FireTriggers,
                        transaction
                    );
            try
            {
                bulkSql.DestinationTableName = internalTable.TableName;
                bulkSql.BulkCopyTimeout = transaction.Connection.ConnectionTimeout;
                foreach (DataColumn coluna in internalTable.Columns)
                {
                    if ((coluna.Table.TableName == "ExtraCards" || coluna.Table.TableName == "OrderItems" || coluna.Table.TableName == "OrderItemsDeleted") && (coluna.Caption == "Monthly" || coluna.Caption == "Formatting"))
                        continue;

                    bulkSql.ColumnMappings.Add(coluna.ColumnName, coluna.ColumnName);
                }

                //bulkSql.NotifyAfter = 10000;
                //bulkSql.SqlRowsCopied += new SqlRowsCopiedEventHandler(s_SqlRowsCopied);
                bulkSql.WriteToServer(internalTable);

                return new OperationResult();
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
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

        public void BulkEdit()
        {
            throw new NotImplementedException();
        }

        private OperationResult UpdateUsingDataTableWithTransaction(DataTable internalTable, SqlTransaction transaction)
        {
            //OpenConnection();
            try
            {
                OperationResult operationResult;

                var temporaryTableName = string.Concat("temp_", Guid.NewGuid().ToString("N").ToUpper());
                var temporaryTableCreationQuery = CreateTemporaryTableForUpdateCreationQuery(internalTable, temporaryTableName);

                operationResult = ExecuteNonQueryWithTransaction(temporaryTableCreationQuery, transaction);

                if (operationResult.Success)
                {
                    var originalTableName = internalTable.TableName;
                    internalTable.TableName = temporaryTableName;

                    operationResult = BulkInsertWithTransaction(internalTable, transaction);

                    if (operationResult.Success)
                    {
                        internalTable.TableName = originalTableName;
                        var queryForUpdate = CreateQueryToUpdate(internalTable, temporaryTableName);
                        operationResult = ExecuteNonQueryWithTransaction(queryForUpdate, transaction);

                        if (operationResult.Success)
                            return ExecuteNonQueryWithTransaction(string.Concat("DROP TABLE ", temporaryTableName, " ;"), transaction);

                        return operationResult;
                    }
                }

                return operationResult;
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
            //CloseConnection();
        }

        private OperationResult ExecuteNonQueryWithTransaction(string queryText, SqlTransaction tran)
        {
            try
            {
                new SqlCommand(queryText, tran.Connection, tran).ExecuteNonQuery();
                return new OperationResult();
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message);
            }
        }
    }
}
