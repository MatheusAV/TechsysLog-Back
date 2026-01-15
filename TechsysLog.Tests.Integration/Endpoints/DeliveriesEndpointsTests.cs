using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Tests.Integration.Fixtures;
using TechsysLog.Tests.Integration.Helpers;
using Xunit;

namespace TechsysLog.Tests.Integration.Endpoints;

public sealed class DeliveriesEndpointsTests : IClassFixture<MongoFixture>
{
    private readonly MongoFixture _mongo;

    public DeliveriesEndpointsTests(MongoFixture mongo) => _mongo = mongo;

    [Fact]
    public async Task Register_delivery_should_set_order_delivered_async()
    {
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var token = await AuthHelper.RegisterAndLoginAsync(client, $"t{Guid.NewGuid():N}@a.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/v1/orders",
            new CreateOrderRequest("PED-777", "Pedido 777", 10m, "01001000", "1"));

        var r = await client.PostAsJsonAsync("/api/v1/deliveries",
            new RegisterDeliveryRequest("PED-777", DateTime.UtcNow));

        r.EnsureSuccessStatusCode();

        var list = await client.GetFromJsonAsync<List<OrderResponse>>("/api/v1/orders");
        var order = list!.Single(x => x.OrderNumber == "PED-777");
        Assert.Equal("Delivered", order.Status);
    }
}
