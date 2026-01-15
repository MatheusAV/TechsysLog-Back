namespace TechsysLog.Application.Abstractions.Realtime
{
    public interface INotificationPublisher
    {
        Task PublishToUserAsync(string userId, object payload, CancellationToken ct);
        Task PublishToAllAsync(object payload, CancellationToken ct);
    }
}
