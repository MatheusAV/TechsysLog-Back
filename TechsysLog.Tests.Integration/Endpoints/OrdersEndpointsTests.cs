using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Tests.Integration.Fixtures;
using TechsysLog.Tests.Integration.Helpers;
using Xunit;

namespace TechsysLog.Tests.Integration.Endpoints;

public sealed class OrdersEndpointsTests : IClassFixture<MongoFixture>
{
    private readonly MongoFixture _mongo;

    public OrdersEndpointsTests(MongoFixture mongo) => _mongo = mongo;

    [Fact]
    public async Task Create_and_list_orders_should_work_with_jwt_async()
    {
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var token = await AuthHelper.RegisterAndLoginAsync(client, $"t{Guid.NewGuid():N}@a.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // CEP válido (ViaCEP real). Se quiser 100% offline, eu troco para stub via DI.
        var create = new CreateOrderRequest("PED-001", "Pedido 1", 99.90m, "01001000", "10");
        var r1 = await client.PostAsJsonAsync("/api/v1/orders", create);
        r1.EnsureSuccessStatusCode();

        var list = await client.GetFromJsonAsync<List<OrderResponse>>("/api/v1/orders");
        Assert.NotNull(list);
        Assert.Contains(list!, o => o.OrderNumber == "PED-001" && o.Status == "Created");
    }
}
