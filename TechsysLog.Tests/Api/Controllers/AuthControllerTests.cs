using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Users;
using TechsysLog.Tests.Api.Infrastructure;

namespace TechsysLog.Tests.Api.Controllers;

public sealed class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task post_register_should_return_201_created()
    {
        _factory.UserServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<RegisterUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("user-id-1");

        var req = new RegisterUserRequest("Matheus", "m@t.com", "123");
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/register", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task post_login_should_return_200_and_auth_response()
    {
        _factory.UserServiceMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthResponse("u1", "Matheus", "m@t.com", "TOKEN"));

        var req = new LoginRequest("m@t.com", "123");
        var resp = await _client.PostAsJsonAsync("/api/v1/auth/login", req);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().Be("TOKEN");
    }
}
