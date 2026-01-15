using FluentAssertions;
using Moq;
using TechsysLog.Application.Abstractions.Auth;
using TechsysLog.Application.Abstractions.Security;
using TechsysLog.Application.DTOs.Users;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Tests.Application.Services;

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokenService = new();

    private UserService CreateSut()
        => new(_userRepo.Object, _hasher.Object, _tokenService.Object);

    // -------------------------
    // RegisterAsync
    // -------------------------

    [Fact]
    public async Task register_async_should_create_user_when_email_not_exists()
    {
        // Arrange
        var sut = CreateSut();
        var request = new RegisterUserRequest("Matheus", "matheus@teste.com", "123");

        _userRepo
            .Setup(x => x.GetByEmailAsync("matheus@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _hasher
            .Setup(x => x.Hash("123"))
            .Returns("HASHED");

        _userRepo
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var userId = await sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        userId.Should().NotBeNullOrWhiteSpace();

        _userRepo.Verify(x => x.GetByEmailAsync("matheus@teste.com", It.IsAny<CancellationToken>()), Times.Once);
        _hasher.Verify(x => x.Hash("123"), Times.Once);
        _userRepo.Verify(x => x.CreateAsync(It.Is<User>(u =>
            u.Name == "Matheus" &&
            u.Email == "matheus@teste.com" &&
            u.PasswordHash == "HASHED"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task register_async_should_throw_validation_when_name_is_empty()
    {
        var sut = CreateSut();
        var request = new RegisterUserRequest("", "a@a.com", "123");

        var act = async () => await sut.RegisterAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Nome");
    }

    [Fact]
    public async Task register_async_should_throw_validation_when_email_is_empty()
    {
        var sut = CreateSut();
        var request = new RegisterUserRequest("Matheus", "", "123");

        var act = async () => await sut.RegisterAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("E-mail");
    }

    [Fact]
    public async Task register_async_should_throw_validation_when_password_is_empty()
    {
        var sut = CreateSut();
        var request = new RegisterUserRequest("Matheus", "a@a.com", "");

        var act = async () => await sut.RegisterAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Senha");
    }

    [Fact]
    public async Task register_async_should_throw_conflict_when_email_already_exists()
    {
        // Arrange
        var sut = CreateSut();
        var request = new RegisterUserRequest("Matheus", "matheus@teste.com", "123");

        _userRepo
            .Setup(x => x.GetByEmailAsync("matheus@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User("Outro", "matheus@teste.com", "X"));

        // Act
        var act = async () => await sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("CONFLICT");

        _userRepo.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _hasher.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
    }

    // -------------------------
    // LoginAsync
    // -------------------------

    [Fact]
    public async Task login_async_should_return_token_when_credentials_are_valid()
    {
        // Arrange
        var sut = CreateSut();
        var request = new LoginRequest("matheus@teste.com", "123");

        var user = new User("Matheus", "matheus@teste.com", "HASHED");

        _userRepo
            .Setup(x => x.GetByEmailAsync("matheus@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasher
            .Setup(x => x.Verify("123", "HASHED"))
            .Returns(true);

        _tokenService
            .Setup(x => x.GenerateToken(It.IsAny<string>(), "matheus@teste.com", "Matheus"))
            .Returns("JWT_TOKEN");

        // Act
        var result = await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        result.Email.Should().Be("matheus@teste.com");
        result.Name.Should().Be("Matheus");
        result.AccessToken.Should().Be("JWT_TOKEN");

        _hasher.Verify(x => x.Verify("123", "HASHED"), Times.Once);
        _tokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), "matheus@teste.com", "Matheus"), Times.Once);
    }

    [Fact]
    public async Task login_async_should_throw_validation_when_email_is_empty()
    {
        var sut = CreateSut();
        var request = new LoginRequest("", "123");

        var act = async () => await sut.LoginAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("E-mail");
    }

    [Fact]
    public async Task login_async_should_throw_validation_when_password_is_empty()
    {
        var sut = CreateSut();
        var request = new LoginRequest("a@a.com", "");

        var act = async () => await sut.LoginAsync(request, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Senha");
    }

    [Fact]
    public async Task login_async_should_throw_unauthorized_when_user_not_found()
    {
        // Arrange
        var sut = CreateSut();
        var request = new LoginRequest("x@x.com", "123");

        _userRepo
            .Setup(x => x.GetByEmailAsync("x@x.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = async () => await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("UNAUTHORIZED");

        _hasher.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task login_async_should_throw_unauthorized_when_password_invalid()
    {
        // Arrange
        var sut = CreateSut();
        var request = new LoginRequest("matheus@teste.com", "wrong");

        var user = new User("Matheus", "matheus@teste.com", "HASHED");

        _userRepo
            .Setup(x => x.GetByEmailAsync("matheus@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _hasher
            .Setup(x => x.Verify("wrong", "HASHED"))
            .Returns(false);

        // Act
        var act = async () => await sut.LoginAsync(request, CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("UNAUTHORIZED");

        _tokenService.Verify(x => x.GenerateToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
