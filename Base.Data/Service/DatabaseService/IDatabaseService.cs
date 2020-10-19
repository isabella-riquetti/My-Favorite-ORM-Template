using System.Collections.Generic;
using System.Data;

namespace Base.Data.Service.DatabaseService
{
    public interface IDatabaseService
    {
        DataTable ConvertToInternalTable<T>(List<T> listaParaConverter, string nomeDaTabela);

        string GetTypeName(string text);
    }
}
