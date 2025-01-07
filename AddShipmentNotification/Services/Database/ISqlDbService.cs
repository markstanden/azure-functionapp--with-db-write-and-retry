using interview.Models.Domain;
using interview.Services.Retry;

namespace interview.Services.Database;

public interface ISqlDbService
{
    public Task<IRetryable> WriteNotificationAsync(ShipmentNotification notification);
}
