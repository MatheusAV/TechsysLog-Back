using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;
using TechsysLog.Infrastructure.Persistence.Mongo.Mappers;

namespace TechsysLog.Infrastructure.Persistence.Mongo.Repositories
{

    public sealed class MongoUserRepository : IUserRepository
    {
        private readonly IMongoDbContext _ctx;

        public MongoUserRepository(IMongoDbContext ctx) => _ctx = ctx;

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
        {
            var doc = await _ctx.Users.Find(x => x.Email == email).FirstOrDefaultAsync(ct);
            return doc is null ? null : doc.ToDomain();
        }

        public async Task<User?> GetByIdAsync(string id, CancellationToken ct)
        {
            var doc = await _ctx.Users.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
            return doc is null ? null : doc.ToDomain();
        }

        public async Task CreateAsync(User user, CancellationToken ct)
        {
            await _ctx.Users.InsertOneAsync(user.ToDoc(), cancellationToken: ct);
        }
    }
}
