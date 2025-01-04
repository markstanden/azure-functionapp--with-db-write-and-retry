using System.Data.Common;
using interview;
using interview.Retry;
using interview.Sanitation;
using interview.SqlDbService;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

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
        var sanitisedShipmentId = _sanitation.AlphaNumericsWithSpecialCharacters(
            notification.shipmentId,
            ['-']
        );

        try
        {
            await using var connection = _connector.GetConnection();
            await connection.OpenAsync();

            var rowsAffected = await Task.WhenAll(
                WriteShipmentAsync(connection, sanitisedShipmentId, notification.shipmentDate),
                WriteShipmentLinesAsync(connection, sanitisedShipmentId, notification.shipmentLines)
            );

            var totalRowsAffected = rowsAffected.Sum();

            return new Retryable
            {
                success = totalRowsAffected >= 2,
                message = $"Success: {rowsAffected} rows affected.",
            };
        }
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

    private async Task<int> WriteShipmentAsync(
        DbConnection connection,
        string sanitisedShipmentId,
        DateTime shipmentDate
    )
    {
        await using var command = connection.CreateCommand();
        command.CommandText = CreateShipmentQuery();
        command.Parameters.Add(new SqlParameter("@shipmentId", sanitisedShipmentId));
        command.Parameters.Add(new SqlParameter("@shipmentDate", shipmentDate));

        return await command.ExecuteNonQueryAsync();
    }

    private async Task<int> WriteShipmentLinesAsync(
        DbConnection connection,
        string sanitisedShipmentId,
        IEnumerable<ShipmentLine> shipmentLines
    )
    {
        var rowsAffected = 0;
        foreach (var shipmentLine in shipmentLines)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = CreateShipmentLinesQuery();
            command.Parameters.Add(new SqlParameter("@shipmentId", sanitisedShipmentId));
            command.Parameters.Add(
                new SqlParameter("@sku", _sanitation.AlphaNumericsOnly(shipmentLine.sku))
            );
            command.Parameters.Add(new SqlParameter("@quantity", shipmentLine.quantity));

            rowsAffected += await command.ExecuteNonQueryAsync();
        }

        return rowsAffected;
    }

    public string CreateShipmentQuery() =>
        $"INSERT INTO {_dbName}.{_dbShipmentTableName} (shipmentId, shipmentDate) VALUES (@shipmentId, @shipmentDate)";

    public string CreateShipmentLinesQuery() =>
        $"INSERT INTO {_dbName}.{_dbShipmentLinesTableName} (shipmentId, sku, quantity) VALUES (@shipmentId, @sku, @quantity)";
}
