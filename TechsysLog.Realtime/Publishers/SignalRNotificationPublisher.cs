using Microsoft.AspNetCore.SignalR;
using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Realtime.Hubs;

namespace TechsysLog.Realtime.Publishers;

public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public Task PublishToUserAsync(string userId, object payload, CancellationToken ct)
        => _hub.Clients.Group($"user:{userId}")
            .SendAsync("event", payload, cancellationToken: ct);

    public Task PublishToAllAsync(object payload, CancellationToken ct)
        => _hub.Clients.All
            .SendAsync("event", payload, cancellationToken: ct);
}
