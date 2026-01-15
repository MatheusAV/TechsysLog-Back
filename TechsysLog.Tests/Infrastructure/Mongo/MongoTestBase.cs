using TechsysLog.Infrastructure.Persistence.Mongo;

namespace TechsysLog.Tests.Infrastructure.Mongo;

public abstract class MongoTestBase : IClassFixture<MongoTestFixture>
{
    protected readonly MongoTestFixture Fixture;
    protected readonly IMongoDbContext Ctx;

    protected MongoTestBase(MongoTestFixture fixture)
    {
        Fixture = fixture;
        Ctx = fixture.CreateContext();
        // índices devem existir em todo teste
        Fixture.EnsureIndexesAsync(Ctx).GetAwaiter().GetResult();
    }
}
