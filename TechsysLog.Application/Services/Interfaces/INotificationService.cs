using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Domain.Common.Entities;

namespace TechsysLog.Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> CreateAsync(string userId, string message, CancellationToken ct);
        Task<List<NotificationResponse>> ListMyAsync(string userId, CancellationToken ct);
        Task MarkAsReadAsync(string notificationId, string userId, CancellationToken ct);
    }
}
