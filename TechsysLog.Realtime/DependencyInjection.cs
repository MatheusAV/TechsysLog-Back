using Microsoft.Extensions.DependencyInjection;
using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Realtime.Publishers;

namespace TechsysLog.Realtime;

public static class DependencyInjection
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();
        return services;
    }
}
