using MongoDB.Driver;
using TechsysLog.Infrastructure.Persistence.Mongo.Documents;

namespace TechsysLog.Infrastructure.Persistence.Mongo
{
    public interface IMongoIndexInitializer
    {
        Task EnsureIndexesAsync(CancellationToken ct);
    }

    public sealed class MongoIndexInitializer : IMongoIndexInitializer
    {
        private readonly IMongoDbContext _ctx;

        public MongoIndexInitializer(IMongoDbContext ctx) => _ctx = ctx;

        public async Task EnsureIndexesAsync(CancellationToken ct)
        {
            // Users: email único
            var existing = await _ctx.Users.Indexes.ListAsync(cancellationToken: ct);
            var existingIndexes = await existing.ToListAsync(ct);

            var hasEmailIndex = existingIndexes.Any(i =>
                i.TryGetValue("name", out var name) && name.AsString == "ux_users_email");

            if (!hasEmailIndex)
            {
                var emailIndex = new CreateIndexModel<UserDocument>(
                    Builders<UserDocument>.IndexKeys.Ascending(x => x.Email),
                    new CreateIndexOptions { Unique = true, Name = "ux_users_email" });

                await _ctx.Users.Indexes.CreateOneAsync(emailIndex, cancellationToken: ct);
            }

            // Orders: orderNumber único
            var orderNumberIndex = new CreateIndexModel<OrderDocument>(
                Builders<OrderDocument>.IndexKeys.Ascending(x => x.OrderNumber),
                new CreateIndexOptions { Unique = true, Name = "ux_orders_orderNumber" });

            await _ctx.Orders.Indexes.CreateOneAsync(orderNumberIndex, cancellationToken: ct);

            // Deliveries: 1 entrega por pedido
            var deliveryOrderIndex = new CreateIndexModel<DeliveryDocument>(
                Builders<DeliveryDocument>.IndexKeys.Ascending(x => x.OrderNumber),
                new CreateIndexOptions { Unique = true, Name = "ux_deliveries_orderNumber" });

            await _ctx.Deliveries.Indexes.CreateOneAsync(deliveryOrderIndex, cancellationToken: ct);

            // Notifications: consulta rápida por userId e createdAt
            var notifIndex = new CreateIndexModel<NotificationDocument>(
                Builders<NotificationDocument>.IndexKeys
                    .Ascending(x => x.UserId)
                    .Descending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "ix_notifications_user_createdAt" });

            await _ctx.Notifications.Indexes.CreateOneAsync(notifIndex, cancellationToken: ct);
        }
    }
}
