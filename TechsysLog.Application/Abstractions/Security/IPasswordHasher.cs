namespace TechsysLog.Application.Abstractions.Security
{
    public interface IPasswordHasher
    {
        string Hash(string plainPassword);
        bool Verify(string plainPassword, string passwordHash);
    }
}
