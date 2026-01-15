using TechsysLog.Application.Abstractions.Security;

namespace TechsysLog.Infrastructure.Security
{
    public sealed class BcryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string plainPassword)
            => BCrypt.Net.BCrypt.HashPassword(plainPassword);

        public bool Verify(string plainPassword, string passwordHash)
            => BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
    }
}
