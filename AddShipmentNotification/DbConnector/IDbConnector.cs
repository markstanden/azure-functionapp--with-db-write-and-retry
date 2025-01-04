using System.Data.Common;

namespace interview.SqlDbService;

/// <summary>
/// Interface allows for dependency injection of connection,
/// allowing for separate connections to be used in main app (SqlConnection) and test code (SqliteConnection)
/// </summary>
public interface IDbConnector
{
    /// <summary>
    /// Opens connection to the DB
    /// </summary>
    /// <returns></returns>
    DbConnection GetConnection();
}
