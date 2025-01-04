using System.Data.Common;
using interview.Retry;
using interview.Sanitation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace interview.SqlDbService;

public class SqlDbService : ISqlDbService
{
    private readonly IDbConnector _connector;
    private readonly string _dbName;
    private readonly string _dbShipmentLinesTableName;
    private readonly string _dbShipmentTableName;
    private readonly ILogger<ISqlDbService> _logger;
    private readonly ISanitation _sanitation;

    public SqlDbService(
        IDbConnector connector,
        ISanitation sanitation,
        ILogger<ISqlDbService> logger,
        string dbName,
        string dbShipmentTableName,
        string dbShipmentLinesTableName
    )
    {
        _connector = connector;
        _sanitation = sanitation;
        _logger = logger;
        _dbName = dbName;
        _dbShipmentTableName = dbShipmentTableName;
        _dbShipmentLinesTableName = dbShipmentLinesTableName;
    }

    /// <summary>
    /// Writes a notification to the DB
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    public async Task<IRetryable> WriteNotificationAsync(ShipmentNotification notification)
    {
        try
        {
            await using var connection = _connector.GetConnection();
            await connection.OpenAsync();

            var rowsAffected = 0;
            var sanitisedShipmentId = _sanitation.AlphaNumericsWithSpecialCharacters(
                notification.shipmentId,
                ['-']
            );

            await using (var command = connection.CreateCommand())
            {
                command.CommandText = CreateShipmentQuery();
                command.Parameters.Add(new SqlParameter("@shipmentId", sanitisedShipmentId));
                command.Parameters.Add(
                    new SqlParameter("@shipmentDate", notification.shipmentDate)
                );

                rowsAffected += await command.ExecuteNonQueryAsync();
            }

            foreach (var shipmentLine in notification.shipmentLines)
            {
                await using (var command = connection.CreateCommand())
                {
                    command.CommandText = CreateShipmentLinesQuery();
                    command.Parameters.Add(new SqlParameter("@shipmentId", sanitisedShipmentId));
                    command.Parameters.Add(
                        new SqlParameter("@sku", _sanitation.AlphaNumericsOnly(shipmentLine.sku))
                    );
                    command.Parameters.Add(new SqlParameter("@quantity", shipmentLine.quantity));

                    rowsAffected += await command.ExecuteNonQueryAsync();
                }
            }

            return new Retryable
            {
                success = rowsAffected > 0,
                message = $"Success: {rowsAffected} rows affected.",
            };
        }
        //Common parent error class allows for catching of both SqlException and SqliteExceptions
        catch (DbException dbEx)
        {
            _logger.LogError($"Failed to write to DB: {dbEx.Message}");
            return new Retryable { success = false, message = dbEx.Message };
        }
        catch (Exception ex)
        {
            return new Retryable { success = false, message = ex.Message };
        }
    }

    public string CreateShipmentQuery()
    {
        return $"INSERT INTO {_dbName}.{_dbShipmentTableName} (shipmentId, shipmentDate) VALUES (@shipmentId, @shipmentDate)";
    }

    public string CreateShipmentLinesQuery()
    {
        return $"INSERT INTO {_dbName}.{_dbShipmentLinesTableName} (shipmentId, sku, quantity) VALUES (@shipmentId, @sku, @quantity)";
    }
}
