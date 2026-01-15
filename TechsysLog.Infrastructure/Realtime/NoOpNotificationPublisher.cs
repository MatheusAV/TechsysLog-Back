using TechsysLog.Application.Abstractions.Realtime;

namespace TechsysLog.Infrastructure.Realtime
{
    public sealed class NoOpNotificationPublisher : INotificationPublisher
    {
        public Task PublishToUserAsync(string userId, object payload, CancellationToken ct) => Task.CompletedTask;
        public Task PublishToAllAsync(object payload, CancellationToken ct) => Task.CompletedTask;
    }
}
