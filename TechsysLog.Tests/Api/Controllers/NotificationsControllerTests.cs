using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Tests.Api.Infrastructure;

namespace TechsysLog.Tests.Api.Controllers;

public sealed class NotificationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public NotificationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task get_notifications_me_should_return_200_and_list()
    {
        _factory.NotificationServiceMock
            .Setup(x => x.ListMyAsync("test-user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<NotificationResponse>
            {
                new("n1","msg",false,DateTime.UtcNow)
            });

        var resp = await _client.GetAsync("/api/v1/notifications/me");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        body.Should().NotBeNull();
        body!.Should().HaveCount(1);
        body[0].Id.Should().Be("n1");
    }

    [Fact]
    public async Task put_notifications_read_should_return_200_ok()
    {
        _factory.NotificationServiceMock
            .Setup(x => x.MarkAsReadAsync("n1", "test-user-1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var resp = await _client.PutAsync("/api/v1/notifications/n1/read", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        _factory.NotificationServiceMock.Verify(x =>
            x.MarkAsReadAsync("n1", "test-user-1", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
