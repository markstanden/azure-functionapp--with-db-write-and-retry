using interview.Sanitation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace interview.SqlDbService;

public class SqlDbService<T> : ISqlDbService<T>
{
    private readonly string _connectionString;
    private readonly string _dbName;
    private readonly string _dbShipmentLinesTableName;
    private readonly string _dbShipmentTableName;
    private readonly ILogger<T> _logger;

    public SqlDbService(
        string connectionString,
        ILogger<T> logger,
        string dbName = "dbo",
        string dbShipmentTableName = "markShipment",
        string dbShipmentLinesTableName = "markShipment_Line"
    )
    {
        _connectionString = connectionString;
        _logger = logger;
        _dbName = dbName;
        _dbShipmentTableName = dbShipmentTableName;
        _dbShipmentLinesTableName = dbShipmentLinesTableName;
    }

    public async Task<bool> WriteNotification(
        ShipmentNotification notification,
        ISanitation sanitation
    )
    {
        try
        {
            // Await using disposes of connections on exiting code block
            await using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var rowsAffected = 0;
                var sanitisedShipmentId = sanitation.AlphaNumericsWithSpecialCharacters(
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
                            sanitation.AlphaNumericsOnly(shipmentLine.sku)
                        );
                        sqlCommand.Parameters.AddWithValue("@quantity", shipmentLine.quantity);

                        rowsAffected += await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return rowsAffected > 0;
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError($"Failed to write to DB: {sqlEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
