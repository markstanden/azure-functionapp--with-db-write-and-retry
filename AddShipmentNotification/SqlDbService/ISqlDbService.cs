namespace interview.SqlDbService;

public interface ISqlDbService
{
    public Task<bool> WriteNotification(ShipmentNotification notification);
}
