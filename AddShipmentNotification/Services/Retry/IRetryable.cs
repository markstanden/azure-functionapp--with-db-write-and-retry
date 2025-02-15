namespace AddShipmentNotification.Services.Retry;

public interface IRetryable
{
    public bool success { get; init; }
    public string message { get; init; }
}
