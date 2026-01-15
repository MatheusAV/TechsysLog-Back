using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.Services.Interfaces;

namespace TechsysLog.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet("me")]
    public async Task<ActionResult<List<NotificationResponse>>> My(CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub")!;
        var list = await _notifications.ListMyAsync(userId, ct);
        return Ok(list);
    }

    [HttpPut("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] string notificationId, CancellationToken ct)
    {
        var userId = User.FindFirstValue("sub")!;
        await _notifications.MarkAsReadAsync(notificationId, userId, ct);
        return Ok(new { ok = true });
    }
}
