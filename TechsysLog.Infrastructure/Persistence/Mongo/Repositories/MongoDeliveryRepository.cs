using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;
using TechsysLog.Infrastructure.Persistence.Mongo.Mappers;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Repositories
{
    public sealed class MongoDeliveryRepository : IDeliveryRepository
    {
        private readonly IMongoDbContext _ctx;

        public MongoDeliveryRepository(IMongoDbContext ctx) => _ctx = ctx;

        public async Task<Delivery?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct)
        {
            var doc = await _ctx.Deliveries.Find(x => x.OrderNumber == orderNumber).FirstOrDefaultAsync(ct);
            return doc is null ? null : doc.ToDomain();
        }

        public async Task CreateAsync(Delivery delivery, CancellationToken ct)
        {
            await _ctx.Deliveries.InsertOneAsync(delivery.ToDoc(), cancellationToken: ct);
        }
    }
}
