using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TechsysLog.Realtime.Hubs;

/// <summary>
/// Hub responsável por eventos em tempo real:
/// - Atualização do painel de pedidos (broadcast)
/// - Notificações por usuário (grupo user:{userId})
/// </summary>
[Authorize] // exige JWT na conexão SignalR
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // userId vem do claim "sub" (JwtRegisteredClaimNames.Sub)
        var userId = Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
