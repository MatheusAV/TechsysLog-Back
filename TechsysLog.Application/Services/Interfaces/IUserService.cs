using TechsysLog.Application.DTOs.Users;

namespace TechsysLog.Application.Services.Interfaces
{

    public interface IUserService
    {
        Task<string> RegisterAsync(RegisterUserRequest request, CancellationToken ct);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
    }
}
