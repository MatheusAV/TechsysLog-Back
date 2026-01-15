using TechsysLog.Domain.Common.Entities;

namespace TechsysLog.Domain.Repositories
{

    public interface INotificationRepository
    {
        Task<List<Notification>> ListByUserAsync(string userId, CancellationToken ct);
        Task CreateAsync(Notification notification, CancellationToken ct);
        Task MarkAsReadAsync(string notificationId, string userId, CancellationToken ct);
    }
}
