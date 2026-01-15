using FluentAssertions;
using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Infrastructure.Persistence.Mongo.Repositories;

namespace TechsysLog.Tests.Infrastructure.Mongo.Repositories;

public sealed class MongoDeliveryRepositoryTests : MongoTestBase
{
    public MongoDeliveryRepositoryTests(MongoTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task create_and_get_by_order_number_should_work()
    {
        var repo = new MongoDeliveryRepository(Ctx);

        var delivery = new Delivery("P001", DateTime.UtcNow);
        await repo.CreateAsync(delivery, CancellationToken.None);

        var found = await repo.GetByOrderNumberAsync("P001", CancellationToken.None);

        found.Should().NotBeNull();
        found!.OrderNumber.Should().Be("P001");
    }

    [Fact]
    public async Task create_should_enforce_one_delivery_per_order_index()
    {
        var repo = new MongoDeliveryRepository(Ctx);

        await repo.CreateAsync(new Delivery("P001", DateTime.UtcNow), CancellationToken.None);

        Func<Task> act = async () =>
            await repo.CreateAsync(new Delivery("P001", DateTime.UtcNow.AddMinutes(1)), CancellationToken.None);

        await act.Should().ThrowAsync<MongoWriteException>();
    }
}
