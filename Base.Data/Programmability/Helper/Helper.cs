using System.Data.SqlClient;
using System.Linq;

namespace Base.Data.Programmability.Helper
{
    public class Helper
    {
        public static string GetSqlFormat(string methodName, object[] parameter)
        {
            foreach (var item in parameter.Select((value, i) => new { i, value }))
            {
                var sqlParameter = (SqlParameter)item.value;
                var parameterName = sqlParameter.ParameterName;
                parameter[item.i] = string.Concat("@", parameterName);
            }

            return string.Concat(methodName, " ", string.Join(",", parameter));
        }
    }
}
