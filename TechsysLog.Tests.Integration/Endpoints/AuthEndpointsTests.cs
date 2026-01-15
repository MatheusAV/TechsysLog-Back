using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TechsysLog.Application.DTOs.Users;
using TechsysLog.Tests.Integration.Fixtures;
using Xunit;

namespace TechsysLog.Tests.Integration.Endpoints;

public sealed class AuthEndpointsTests : IClassFixture<MongoFixture>
{
    private readonly MongoFixture _mongo;

    public AuthEndpointsTests(MongoFixture mongo)
    {
        _mongo = mongo;
    }

    [Fact]
    public async Task Register_should_create_user_and_return_201_async()
    {
        // Arrange
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var request = new RegisterUserRequest(
            Name: "Auth Tester",
            Email: $"auth_{Guid.NewGuid():N}@test.com",
            Password: "123456"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(payload);
        Assert.True(payload!.ContainsKey("userId"));
        Assert.False(string.IsNullOrWhiteSpace(payload["userId"]));
    }

    [Fact]
    public async Task Register_should_fail_when_email_already_exists_async()
    {
        // Arrange
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var email = $"dup_{Guid.NewGuid():N}@test.com";

        var request = new RegisterUserRequest(
            Name: "Duplicate User",
            Email: email,
            Password: "123456"
        );

        // Primeiro cadastro
        var r1 = await client.PostAsJsonAsync("/api/v1/auth/register", request);
        r1.EnsureSuccessStatusCode();

        // Act (segundo cadastro com mesmo e-mail)
        var r2 = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, r2.StatusCode);

        var error = await r2.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CONFLICT", error.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task Login_should_return_jwt_when_credentials_are_valid_async()
    {
        // Arrange
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var email = $"login_{Guid.NewGuid():N}@test.com";

        var register = new RegisterUserRequest(
            Name: "Login Tester",
            Email: email,
            Password: "123456"
        );

        await client.PostAsJsonAsync("/api/v1/auth/register", register);

        var login = new LoginRequest(
            Email: email,
            Password: "123456"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", login);

        // Assert
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));
        Assert.Equal(email, auth.Email);
        Assert.False(string.IsNullOrWhiteSpace(auth.UserId));
    }

    [Fact]
    public async Task Login_should_fail_when_password_is_invalid_async()
    {
        // Arrange
        await using var factory = new ApiFactory(_mongo.ConnectionString);
        var client = factory.CreateClient();

        var email = $"wrongpwd_{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterUserRequest("Wrong Pwd", email, "123456"));

        var login = new LoginRequest(
            Email: email,
            Password: "wrong-password"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", login);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("UNAUTHORIZED", error.GetProperty("error").GetProperty("code").GetString());
    }
}
