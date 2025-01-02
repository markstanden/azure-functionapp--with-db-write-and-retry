using interview.Retry;
using interview.Sanitation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace interview.SqlDbService;

public class SqlDbService : ISqlDbService
{
    private readonly string _connectionString;
    private readonly string _dbName;
    private readonly string _dbShipmentLinesTableName;
    private readonly string _dbShipmentTableName;
    private readonly ILogger<SqlDbService> _logger;
    private readonly ISanitation _sanitation;

    public SqlDbService(
        string connectionString,
        ISanitation sanitation,
        ILogger<SqlDbService> logger,
        string dbName,
        string dbShipmentTableName,
        string dbShipmentLinesTableName
    )
    {
        _connectionString = connectionString;
        _sanitation = sanitation;
        _logger = logger;
        _dbName = dbName;
        _dbShipmentTableName = dbShipmentTableName;
        _dbShipmentLinesTableName = dbShipmentLinesTableName;
    }

    public async Task<IRetryable> WriteNotification(ShipmentNotification notification)
    {
        try
        {
            // Await using disposes of connections on exiting code block
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var rowsAffected = 0;
                var sanitisedShipmentId = _sanitation.AlphaNumericsWithSpecialCharacters(
                    notification.shipmentId,
                    ['-']
                );

                await connection.OpenAsync();

                var shipmentQueryString =
                    $"INSERT INTO {_dbName}.{_dbShipmentTableName} (shipmentId, shipmentDate) VALUES (@shipmentId, @shipmentDate)";
                await using (
                    SqlCommand sqlCommand = new SqlCommand(shipmentQueryString, connection)
                )
                {
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.AddWithValue("@shipmentId", sanitisedShipmentId);
                    sqlCommand.Parameters.AddWithValue("@shipmentDate", notification.shipmentDate);

                    rowsAffected += await sqlCommand.ExecuteNonQueryAsync();
                }

                var shipmentLinesQueryString =
                    $"INSERT INTO {_dbName}.{_dbShipmentLinesTableName} (shipmentId, sku, quantity) VALUES (@shipmentId, @sku, @quantity)";
                foreach (var shipmentLine in notification.shipmentLines)
                {
                    await using (
                        SqlCommand sqlCommand = new SqlCommand(shipmentLinesQueryString, connection)
                    )
                    {
                        sqlCommand.Parameters.Clear();
                        sqlCommand.Parameters.AddWithValue("@shipmentId", sanitisedShipmentId);
                        sqlCommand.Parameters.AddWithValue(
                            "@sku",
                            _sanitation.AlphaNumericsOnly(shipmentLine.sku)
                        );
                        sqlCommand.Parameters.AddWithValue("@quantity", shipmentLine.quantity);

                        rowsAffected += await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new Retryable
                {
                    success = rowsAffected > 0,
                    message = $"Success: {rowsAffected} rows affected.",
                };
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError($"Failed to write to DB: {sqlEx.Message}");
            return new Retryable { success = false, message = sqlEx.Message };
        }
        catch (Exception ex)
        {
            return new Retryable { success = false, message = ex.Message };
        }
    }
}
