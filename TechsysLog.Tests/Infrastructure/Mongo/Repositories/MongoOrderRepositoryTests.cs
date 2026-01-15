using FluentAssertions;
using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Common.ValueObjects;
using TechsysLog.Infrastructure.Persistence.Mongo.Repositories;

namespace TechsysLog.Tests.Infrastructure.Mongo.Repositories;

public sealed class MongoOrderRepositoryTests : MongoTestBase
{
    public MongoOrderRepositoryTests(MongoTestFixture fixture) : base(fixture) { }

    private static Address Addr(string cep = "01001000")
        => new(cep, "Rua A", "10", "Centro", "São Paulo", "SP");

    [Fact]
    public async Task create_and_get_by_order_number_should_work()
    {
        var repo = new MongoOrderRepository(Ctx);

        var order = new Order("P001", "Notebook", 3500m, Addr());
        await repo.CreateAsync(order, CancellationToken.None);

        var found = await repo.GetByOrderNumberAsync("P001", CancellationToken.None);

        found.Should().NotBeNull();
        found!.OrderNumber.Should().Be("P001");
        found.Description.Should().Be("Notebook");
        found.Value.Should().Be(3500m);
        found.DeliveryAddress.Cep.Should().Be("01001000");
    }

    [Fact]
    public async Task update_should_replace_document_by_order_number()
    {
        var repo = new MongoOrderRepository(Ctx);

        var order = new Order("P001", "Notebook", 3500m, Addr());
        await repo.CreateAsync(order, CancellationToken.None);

        // muda status (Delivery)
        order.MarkAsDelivered();
        await repo.UpdateAsync(order, CancellationToken.None);

        var found = await repo.GetByOrderNumberAsync("P001", CancellationToken.None);

        found.Should().NotBeNull();
        found!.Status.ToString().Should().Be("Delivered");
    }

    [Fact]
    public async Task list_should_return_all_orders()
    {
        var repo = new MongoOrderRepository(Ctx);

        await repo.CreateAsync(new Order("P001", "A", 10m, Addr("11111111")), CancellationToken.None);
        await repo.CreateAsync(new Order("P002", "B", 20m, Addr("22222222")), CancellationToken.None);

        var list = await repo.ListAsync(CancellationToken.None);

        list.Should().HaveCount(2);
        list.Select(x => x.OrderNumber).Should().Contain(new[] { "P001", "P002" });
    }

    [Fact]
    public async Task create_should_enforce_unique_orderNumber_index()
    {
        var repo = new MongoOrderRepository(Ctx);

        await repo.CreateAsync(new Order("P001", "A", 10m, Addr()), CancellationToken.None);

        Func<Task> act = async () =>
            await repo.CreateAsync(new Order("P001", "B", 20m, Addr()), CancellationToken.None);

        await act.Should().ThrowAsync<MongoWriteException>();
    }
}
