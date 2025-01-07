using System.Data.Common;
using interview.Models.Domain;
using interview.Services.Retry;
using interview.Services.Validation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace interview.Services.Database;

public class SqlDbService : ISqlDbService
{
    private readonly IDbConnector _connector;
    private readonly string _dbSchema;
    private readonly string _dbShipmentLinesTableName;
    private readonly string _dbShipmentTableName;
    private readonly ILogger<ISqlDbService> _logger;
    private readonly ISanitation _sanitation;

    public SqlDbService(
        IDbConnector connector,
        ISanitation sanitation,
        ILogger<ISqlDbService> logger,
        string dbSchema,
        string dbShipmentTableName,
        string dbShipmentLinesTableName
    )
    {
        _connector = connector;
        _sanitation = sanitation;
        _logger = logger;
        _dbSchema = dbSchema;
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
        // It should not be possible to trigger these guards as we are already checking
        // within the caller, but is good practice in case the implementation changes.
        if (notification is null || notification.shipmentLines is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        // Check now that there is at least one item in the shipping lines array.
        if (notification.shipmentLines.Length < 1)
        {
            const string error = "Shipping Notification lines is empty.";
            _logger.LogWarning($"Aborting DB Write - {error}");
            throw new ArgumentException(error, nameof(notification));
        }

        var sanitisedShipmentId = _sanitation.AlphaNumericsWithSpecialCharacters(
            notification.shipmentId,
            ['-']
        );

        // We know how many rows to expect to be written, it is a single
        // row for the shipment table and a line each for the shipment lines.
        var expectedRows = 1 + (notification.shipmentLines.Length);

        try
        {
            // await using here will dispose of the connection once
            // the thread leaves the _parent scope_ (and await async completion).
            await using var connection = _connector.GetConnection();
            await connection.OpenAsync();

            int rowsAffected = new[]
            {
                await WriteShipmentAsync(
                    connection,
                    sanitisedShipmentId,
                    notification.shipmentDate
                ),
                await WriteShipmentLinesAsync(
                    connection,
                    sanitisedShipmentId,
                    notification.shipmentLines
                ),
            }.Sum();

            return new Retryable
            {
                success = rowsAffected == expectedRows,
                message = $"{rowsAffected} rows affected.",
            };
        }
        catch (DbException dbEx)
        {
            _logger.LogError($"Failed to write to DB: {dbEx.Message}");
            return new Retryable { success = false, message = dbEx.Message };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Failed to establish connection to DB:\nMessage: {ex.Message}\nError: {ex.Data}"
            );
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
        command.CommandText = CreateShipmentInsertQuery(_dbSchema, _dbShipmentTableName);
        command.Parameters.Add(new SqlParameter("@shipmentId", sanitisedShipmentId));
        command.Parameters.Add(new SqlParameter("@shipmentDate", shipmentDate));

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Takes all the provided shipment lines and synchronously adds
    /// them to the DB using the provided connection
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="sanitisedShipmentId"></param>
    /// <param name="shipmentLines"></param>
    /// <returns></returns>
    public async Task<int> WriteShipmentLinesAsync(
        DbConnection connection,
        string sanitisedShipmentId,
        IEnumerable<ShipmentLine> shipmentLines
    )
    {
        return await shipmentLines
        // We can't use a Task.WhenAll(shipmentLineTasks).Sum() as the connection cannot handle
        // multiple concurrent async requests, so we must add the lines synchronously.  The reducer below
        // awaits the result of each addition before starting the next DB request.
        .Aggregate(
            // Accumulator init is first param
            Task.FromResult(0),
            // reducer is second param
            async (affectedLineAcc, shipmentLine) =>
                await affectedLineAcc
                + await WriteShipmentLineAsync(connection, sanitisedShipmentId, shipmentLine)
        );
    }

    /// <summary>
    /// Writes a single shipmentLine to the Database using the provided connection
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="sanitisedShipmentId"></param>
    /// <param name="shipmentLine"></param>
    /// <returns></returns>
    public async Task<int> WriteShipmentLineAsync(
        DbConnection connection,
        string sanitisedShipmentId,
        ShipmentLine shipmentLine
    )
    {
        await using var command = connection.CreateCommand();
        command.CommandText = CreateShipmentLineInsertQuery(_dbSchema, _dbShipmentLinesTableName);
        command.Parameters.Add(new SqlParameter("@shipmentId", sanitisedShipmentId));
        command.Parameters.Add(
            new SqlParameter("@sku", _sanitation.AlphaNumericsOnly(shipmentLine.sku))
        );
        command.Parameters.Add(new SqlParameter("@quantity", shipmentLine.quantity));

        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// SQL query to insert a single shipment into the shipment table
    /// </summary>
    /// <param name="schema">The table schema prefix</param>
    /// <param name="table">The DB table to add the record to</param>
    /// <returns>SQL Query string with the parameters @shipmentId and @shipmentDate</returns>
    public string CreateShipmentInsertQuery(string schema, string table) =>
        $"INSERT INTO {schema}.{table} (shipmentId, shipmentDate) VALUES (@shipmentId, @shipmentDate)";

    /// <summary>
    /// SQL query to insert a single shipment line into the shipmentLines table
    /// </summary>
    /// <param name="schema">The table schema prefix</param>
    /// <param name="table">The DB table to add the record to</param>
    /// <returns>SQL Query string with the parameters @shipmentId, @sku and @quantity</returns>
    public string CreateShipmentLineInsertQuery(string schema, string table) =>
        $"INSERT INTO {schema}.{table} (shipmentId, sku, quantity) VALUES (@shipmentId, @sku, @quantity)";
}
