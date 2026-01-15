using TechsysLog.Domain.Common.Entities;

namespace TechsysLog.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct);
        Task<User?> GetByIdAsync(string id, CancellationToken ct);
        Task CreateAsync(User user, CancellationToken ct);
    }
}
