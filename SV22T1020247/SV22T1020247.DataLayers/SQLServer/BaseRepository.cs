using System.Data;
using Microsoft.Data.SqlClient;

namespace SV22T1020247.DataLayers.SqlServer
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        protected BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected IDbConnection OpenConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
