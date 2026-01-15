using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TechsysLog.Application.DTOs.Users;
using TechsysLog.Application.Services.Interfaces;

namespace TechsysLog.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUserService _users;

    public AuthController(IUserService users) => _users = users;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var id = await _users.RegisterAsync(request, ct);
        return Created("", new { userId = id });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var res = await _users.LoginAsync(request, ct);
        return Ok(res);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            email = User.FindFirstValue(JwtRegisteredClaimNames.Email),
            name = User.FindFirstValue("name")
        });
    }
}
