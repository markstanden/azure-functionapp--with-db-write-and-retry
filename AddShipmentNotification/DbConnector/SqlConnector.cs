using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace interview.SqlDbService;

public class SqlConnector : IDbConnector
{
    private readonly string _connectionString;

    public SqlConnector(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbConnection GetConnection() => new SqlConnection(_connectionString);
}
