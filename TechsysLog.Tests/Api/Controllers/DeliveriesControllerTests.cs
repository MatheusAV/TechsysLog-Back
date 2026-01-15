using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Tests.Api.Infrastructure;

namespace TechsysLog.Tests.Api.Controllers;

public sealed class DeliveriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeliveriesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task post_deliveries_should_return_200_ok()
    {
        _factory.DeliveryServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterDeliveryRequest>(), "test-user-1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var req = new RegisterDeliveryRequest("P001", DateTime.UtcNow);
        var resp = await _client.PostAsJsonAsync("/api/v1/deliveries", req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        _factory.DeliveryServiceMock.Verify(x =>
            x.RegisterAsync(It.IsAny<RegisterDeliveryRequest>(), "test-user-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
