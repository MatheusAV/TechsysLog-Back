using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Tests.Api.Infrastructure;

namespace TechsysLog.Tests.Api.Controllers;

public sealed class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task post_orders_should_return_201_created()
    {
        _factory.OrderServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), "test-user-1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var req = new CreateOrderRequest("P001", "Desc", 10m, "01001000", "100");
        var resp = await _client.PostAsJsonAsync("/api/v1/orders", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        _factory.OrderServiceMock.Verify(x =>
            x.CreateAsync(It.IsAny<CreateOrderRequest>(), "test-user-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task get_orders_should_return_200_and_list()
    {
        _factory.OrderServiceMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderResponse>
            {
                new("P001","Desc",10m,"01001000","Rua","100","Bairro","Cidade","SP","Created", DateTime.UtcNow)
            });

        var resp = await _client.GetAsync("/api/v1/orders");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<List<OrderResponse>>();
        body.Should().NotBeNull();
        body!.Should().HaveCount(1);
        body[0].OrderNumber.Should().Be("P001");
    }
}
