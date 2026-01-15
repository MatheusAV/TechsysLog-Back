using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;
using TechsysLog.Infrastructure.Persistence.Mongo.Mappers;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Repositories
{
    public sealed class MongoOrderRepository : IOrderRepository
    {
        private readonly IMongoDbContext _ctx;

        public MongoOrderRepository(IMongoDbContext ctx) => _ctx = ctx;

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct)
        {
            var doc = await _ctx.Orders.Find(x => x.OrderNumber == orderNumber).FirstOrDefaultAsync(ct);
            return doc is null ? null : doc.ToDomain();
        }

        public async Task<List<Order>> ListAsync(CancellationToken ct)
        {
            var docs = await _ctx.Orders.Find(_ => true).ToListAsync(ct);
            return docs.Select(d => d.ToDomain()).ToList();
        }

        public async Task CreateAsync(Order order, CancellationToken ct)
        {
            await _ctx.Orders.InsertOneAsync(order.ToDoc(), cancellationToken: ct);
        }

        public async Task UpdateAsync(Order order, CancellationToken ct)
        {
            var doc = order.ToDoc();
            await _ctx.Orders.ReplaceOneAsync(x => x.OrderNumber == doc.OrderNumber, doc, cancellationToken: ct);
        }
    }
}
