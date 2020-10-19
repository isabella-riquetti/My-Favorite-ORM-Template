using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Base.Data.Service.DatabaseService
{
    public class DatabaseService : IDatabaseService
    {
        public string GetTypeName(string text)
        {
            if (text.IndexOf("Guid", 0, StringComparison.Ordinal) != -1)
                return "Guid";

            if (text.IndexOf("Int", 0, StringComparison.Ordinal) != -1)
                return "int";

            if (text.IndexOf("Decimal", 0, StringComparison.Ordinal) != -1)
                return "decimal";

            if (text.IndexOf("DateTime", 0, StringComparison.Ordinal) != -1)
                return "DateTime";

            return "String";
        }

        public DataTable ConvertToInternalTable<T>(List<T> listaParaConverter, string nomeDaTabela)
        {
            var tabelaInterna = new DataTable(typeof(T).Name);
            var vetorDePropriedades = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var propriedade in vetorDePropriedades)
            {

                var nomeDoTipo = GetTypeName(propriedade.PropertyType.FullName);

                switch (nomeDoTipo)
                {
                    case "Guid":
                        tabelaInterna.Columns.Add(propriedade.Name, typeof(Guid));
                        break;
                    case "int":
                        tabelaInterna.Columns.Add(propriedade.Name, typeof(int));
                        break;
                    case "DateTime":
                        tabelaInterna.Columns.Add(propriedade.Name, typeof(DateTime));
                        break;
                    case "decimal":
                        tabelaInterna.Columns.Add(propriedade.Name, typeof(decimal));
                        break;
                    default:
                        tabelaInterna.Columns.Add(propriedade.Name, typeof(string));
                        break;
                }

            }
            foreach (var itemDaLista in listaParaConverter)
            {
                // Cria um vetor baseado no numero de propiedades da lista que quer converter
                var valores = new object[vetorDePropriedades.Length];

                for (var i = 0; i < vetorDePropriedades.Length; i++)
                {
                    //Passa os valores da lista para o vetor a inserir na tabela interna
                    valores[i] = vetorDePropriedades[i].GetValue(itemDaLista, null);
                }
                // Insere os valores na tabelaInterna como uma nova linha
                tabelaInterna.Rows.Add(valores);
            }
            if (string.IsNullOrEmpty(nomeDaTabela))
                return tabelaInterna;
            tabelaInterna.TableName = nomeDaTabela;
            return tabelaInterna;
        }
    }
}
