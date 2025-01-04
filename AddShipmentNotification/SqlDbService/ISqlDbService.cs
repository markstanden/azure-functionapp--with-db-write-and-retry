using interview.Retry;

namespace interview.SqlDbService;

public interface ISqlDbService
{
    public Task<IRetryable> WriteNotificationAsync(ShipmentNotification notification);
}
