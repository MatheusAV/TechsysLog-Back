using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Users;
using LoginRequest = TechsysLog.Application.DTOs.Users.LoginRequest;

namespace TechsysLog.Tests.Integration.Helpers;

public static class AuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string email)
    {
        var register = new RegisterUserRequest("Tester", email, "123456");
        var r1 = await client.PostAsJsonAsync("/api/v1/auth/register", register);
        r1.EnsureSuccessStatusCode();

        var login = new LoginRequest(email, "123456");
        var r2 = await client.PostAsJsonAsync("/api/v1/auth/login", login);
        r2.EnsureSuccessStatusCode();

        var auth = await r2.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }
}
