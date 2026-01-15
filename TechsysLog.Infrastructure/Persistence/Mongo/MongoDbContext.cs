using MongoDB.Driver;
using TechsysLog.Infrastructure.Persistence.Mongo.Documents;

namespace TechsysLog.Infrastructure.Persistence.Mongo
{
    public interface IMongoDbContext
    {
        IMongoCollection<UserDocument> Users { get; }
        IMongoCollection<OrderDocument> Orders { get; }
        IMongoCollection<DeliveryDocument> Deliveries { get; }
        IMongoCollection<NotificationDocument> Notifications { get; }
    }

    public sealed class MongoDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _db;

        public MongoDbContext(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _db = client.GetDatabase(settings.DatabaseName);

            Users = _db.GetCollection<UserDocument>("users");
            Orders = _db.GetCollection<OrderDocument>("orders");
            Deliveries = _db.GetCollection<DeliveryDocument>("deliveries");
            Notifications = _db.GetCollection<NotificationDocument>("notifications");
        }

        public IMongoCollection<UserDocument> Users { get; }
        public IMongoCollection<OrderDocument> Orders { get; }
        public IMongoCollection<DeliveryDocument> Deliveries { get; }
        public IMongoCollection<NotificationDocument> Notifications { get; }
    }
}
