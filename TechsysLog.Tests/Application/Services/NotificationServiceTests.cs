using FluentAssertions;
using Moq;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Tests.Application.Services;

public sealed class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _repo = new();

    private NotificationService CreateSut()
        => new(_repo.Object);

    // -------------------------
    // CreateAsync
    // -------------------------

    [Fact]
    public async Task create_async_should_throw_validation_when_userId_is_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.CreateAsync("", "msg", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("UserId");

        _repo.Verify(x => x.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_throw_validation_when_message_is_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.CreateAsync("user1", "", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Mensagem");

        _repo.Verify(x => x.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_persist_notification_and_return_entity()
    {
        // Arrange
        var sut = CreateSut();
        Notification? captured = null;

        _repo.Setup(x => x.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .Callback<Notification, CancellationToken>((n, _) => captured = n)
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.CreateAsync("user1", "Olá", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("user1");
        result.Message.Should().Be("Olá");
        result.IsRead.Should().BeFalse();

        captured.Should().NotBeNull();
        captured!.UserId.Should().Be("user1");
        captured.Message.Should().Be("Olá");

        _repo.Verify(x => x.CreateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------
    // ListMyAsync
    // -------------------------

    [Fact]
    public async Task list_my_async_should_throw_validation_when_userId_is_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.ListMyAsync("", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("UserId");

        _repo.Verify(x => x.ListByUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task list_my_async_should_map_notifications_to_response()
    {
        // Arrange
        var sut = CreateSut();

        var n1 = new Notification("user1", "Msg 1");
        var n2 = new Notification("user1", "Msg 2");
        n2.MarkAsRead();

        _repo.Setup(x => x.ListByUserAsync("user1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification> { n1, n2 });

        // Act
        var list = await sut.ListMyAsync("user1", CancellationToken.None);

        // Assert
        list.Should().HaveCount(2);
        list[0].Message.Should().Be("Msg 1");
        list[0].IsRead.Should().BeFalse();

        list[1].Message.Should().Be("Msg 2");
        list[1].IsRead.Should().BeTrue();

        _repo.Verify(x => x.ListByUserAsync("user1", It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------------------------
    // MarkAsReadAsync
    // -------------------------

    [Fact]
    public async Task mark_as_read_async_should_throw_validation_when_notificationId_is_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.MarkAsReadAsync("", "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("NotificationId");

        _repo.Verify(x => x.MarkAsReadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task mark_as_read_async_should_throw_validation_when_userId_is_empty()
    {
        var sut = CreateSut();

        var act = async () => await sut.MarkAsReadAsync("n1", "", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("UserId");

        _repo.Verify(x => x.MarkAsReadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task mark_as_read_async_should_call_repository()
    {
        // Arrange
        var sut = CreateSut();

        _repo.Setup(x => x.MarkAsReadAsync("n1", "user1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.MarkAsReadAsync("n1", "user1", CancellationToken.None);

        // Assert
        _repo.Verify(x => x.MarkAsReadAsync("n1", "user1", It.IsAny<CancellationToken>()), Times.Once);
    }
}
