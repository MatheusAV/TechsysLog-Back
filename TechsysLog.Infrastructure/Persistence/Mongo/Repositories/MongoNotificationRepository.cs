using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;
using TechsysLog.Infrastructure.Persistence.Mongo.Mappers;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Repositories
{
    public sealed class MongoNotificationRepository : INotificationRepository
    {
        private readonly IMongoDbContext _ctx;

        public MongoNotificationRepository(IMongoDbContext ctx) => _ctx = ctx;

        public async Task<List<Notification>> ListByUserAsync(string userId, CancellationToken ct)
        {
            var docs = await _ctx.Notifications
                .Find(x => x.UserId == userId)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync(ct);

            return docs.Select(x => x.ToDomain()).ToList();
        }

        public async Task CreateAsync(Notification notification, CancellationToken ct)
        {
            await _ctx.Notifications.InsertOneAsync(notification.ToDoc(), cancellationToken: ct);
        }

        public async Task MarkAsReadAsync(string notificationId, string userId, CancellationToken ct)
        {
            var update = Builders<Persistence.Mongo.Documents.NotificationDocument>
                .Update.Set(x => x.IsRead, true);

            await _ctx.Notifications.UpdateOneAsync(
                x => x.Id == notificationId && x.UserId == userId,
                update,
                cancellationToken: ct);
        }
    }
}
