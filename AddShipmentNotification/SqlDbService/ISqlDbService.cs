using interview;
using interview.Sanitation;

public interface ISqlDbService<T>
{
    public Task<bool> WriteNotification(ShipmentNotification notification, ISanitation sanitation);
}
