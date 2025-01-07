using AddShipmentNotification.Models.Domain;
using AddShipmentNotification.Services.Retry;

namespace AddShipmentNotification.Services.Database;

public interface ISqlDbService
{
    public Task<IRetryable> WriteNotificationAsync(ShipmentNotification notification);
}
