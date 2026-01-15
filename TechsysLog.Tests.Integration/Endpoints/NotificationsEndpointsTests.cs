using System.Net.Http.Headers;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Tests.Integration.Fixtures;
using TechsysLog.Tests.Integration.Helpers;
using Xunit;

namespace TechsysLog.Tests.Integration.Endpoints;

public sealed class NotificationsEndpointsTests : IClassFixture<MongoFixture>
{
    private readonly MongoFixture _mongo;

    public NotificationsEndpointsTests(MongoFixture mongo) => _mongo = mongo;

    [Fact]
    public async Task Notifications_should_be_listed_and_marked_as_read_async()
    {
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var token = await AuthHelper.RegisterAndLoginAsync(client, $"t{Guid.NewGuid():N}@a.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // criar pedido gera notificação para o usuário
        await client.PostAsJsonAsync("/api/v1/orders",
            new CreateOrderRequest("PED-NOTIF", "Pedido Notif", 20m, "01001000", "10"));

        var notifs = await client.GetFromJsonAsync<List<NotificationResponse>>("/api/v1/notifications/me");
        Assert.NotNull(notifs);
        Assert.NotEmpty(notifs!);

        var first = notifs!.First();
        Assert.False(first.IsRead);

        var r = await client.PutAsync($"/api/v1/notifications/{first.Id}/read", null);
        r.EnsureSuccessStatusCode();

        var notifs2 = await client.GetFromJsonAsync<List<NotificationResponse>>("/api/v1/notifications/me");
        Assert.True(notifs2!.First(x => x.Id == first.Id).IsRead);
    }
}
