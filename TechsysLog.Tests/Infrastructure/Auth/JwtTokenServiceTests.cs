using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TechsysLog.Infrastructure.Auth;

namespace TechsysLog.Tests.Infrastructure.Auth;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void generate_token_should_include_expected_claims_and_valid_signature()
    {
        var settings = new JwtSettings
        {
            Issuer = "techsyslog",
            Audience = "techsyslog",
            SecretKey = "THIS_IS_A_TEST_SECRET_KEY_WITH_32+_CHARS",
            ExpirationMinutes = 60
        };

        var sut = new JwtTokenService(settings);

        var token = sut.GenerateToken("u1", "a@a.com", "Matheus");

        token.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var validations = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = settings.Issuer,
            ValidAudience = settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(5)
        };

        var principal = handler.ValidateToken(token, validations, out var validatedToken);

        validatedToken.Should().BeOfType<JwtSecurityToken>();

        principal.FindFirst("sub")!.Value.Should().Be("u1");
        principal.FindFirst("email")!.Value.Should().Be("a@a.com");
        principal.FindFirst("name")!.Value.Should().Be("Matheus");
        principal.FindFirst(JwtRegisteredClaimNames.Jti)!.Value.Should().NotBeNullOrWhiteSpace();
    }
}
