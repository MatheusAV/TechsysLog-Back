using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechsysLog.Application.Abstractions.Auth;

namespace TechsysLog.Infrastructure.Auth
{
    public sealed class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(JwtSettings settings)
        {
            _settings = settings;
        }

        public string GenerateToken(string userId, string email, string name)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new("name", name),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
