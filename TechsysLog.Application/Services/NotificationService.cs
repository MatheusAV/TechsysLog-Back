using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Application.Services
{
    public sealed class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<Notification> CreateAsync(string userId, string message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw AppErrors.Validation("UserId é obrigatório.");
            if (string.IsNullOrWhiteSpace(message)) throw AppErrors.Validation("Mensagem é obrigatória.");

            var n = new Notification(userId, message);
            await _repo.CreateAsync(n, ct);
            return n;
        }

        public async Task<List<NotificationResponse>> ListMyAsync(string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw AppErrors.Validation("UserId é obrigatório.");

            var list = await _repo.ListByUserAsync(userId, ct);
            return list.Select(x => new NotificationResponse(x.Id, x.Message, x.IsRead, x.CreatedAt)).ToList();
        }

        public async Task MarkAsReadAsync(string notificationId, string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(notificationId)) throw AppErrors.Validation("NotificationId é obrigatório.");
            if (string.IsNullOrWhiteSpace(userId)) throw AppErrors.Validation("UserId é obrigatório.");

            await _repo.MarkAsReadAsync(notificationId, userId, ct);
        }
    }
}
