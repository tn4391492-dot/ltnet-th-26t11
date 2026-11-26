using System.Configuration;
using System.Data.SqlClient;

namespace DAL
{
    public static class DbConnectionHelper
    {
        public static SqlConnection GetConnection()
        {
            string connStr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return new SqlConnection(connStr);
        }
    }
}