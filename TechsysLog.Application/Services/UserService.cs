using TechsysLog.Application.Abstractions.Auth;
using TechsysLog.Application.Abstractions.Security;
using TechsysLog.Application.DTOs.Users;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;
namespace TechsysLog.Application.Services
{
    public sealed class UserService : IUserService
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository users, IPasswordHasher hasher, ITokenService tokenService)
        {
            _users = users;
            _hasher = hasher;
            _tokenService = tokenService;
        }

        public async Task<string> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Name)) throw AppErrors.Validation("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(request.Email)) throw AppErrors.Validation("E-mail é obrigatório.");
            if (string.IsNullOrWhiteSpace(request.Password)) throw AppErrors.Validation("Senha é obrigatória.");

            var existing = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
            if (existing is not null) throw AppErrors.Conflict("E-mail já cadastrado.");

            var hash = _hasher.Hash(request.Password);
            var user = new User(request.Name, request.Email, hash);

            await _users.CreateAsync(user, ct);
            return user.Id;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) throw AppErrors.Validation("E-mail é obrigatório.");
            if (string.IsNullOrWhiteSpace(request.Password)) throw AppErrors.Validation("Senha é obrigatória.");

            var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
            if (user is null) throw AppErrors.Unauthorized("Credenciais inválidas.");

            var ok = _hasher.Verify(request.Password, user.PasswordHash);
            if (!ok) throw AppErrors.Unauthorized("Credenciais inválidas.");

            var token = _tokenService.GenerateToken(user.Id, user.Email, user.Name);
            return new AuthResponse(user.Id, user.Name, user.Email, token);
        }
    }
}
