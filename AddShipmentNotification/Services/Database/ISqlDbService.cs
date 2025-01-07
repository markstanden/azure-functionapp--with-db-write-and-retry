using interview.Models.Domain;
using interview.Retry;

namespace interview.Services.Database;

public interface ISqlDbService
{
    public Task<IRetryable> WriteNotificationAsync(ShipmentNotification notification);
}
