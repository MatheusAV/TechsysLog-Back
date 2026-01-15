using FluentAssertions;
using Moq;
using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Application.DTOs.Deliveries;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Common.ValueObjects;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Tests.Application.Services;

public sealed class DeliveryServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IDeliveryRepository> _deliveryRepo = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<INotificationPublisher> _publisher = new();

    private DeliveryService CreateSut()
        => new(_orderRepo.Object, _deliveryRepo.Object, _notificationService.Object, _publisher.Object);

    [Fact]
    public async Task register_async_should_throw_validation_when_orderNumber_is_empty()
    {
        var sut = CreateSut();
        var req = new RegisterDeliveryRequest("", DateTime.UtcNow);

        var act = async () => await sut.RegisterAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Número do pedido");

        _deliveryRepo.Verify(x => x.CreateAsync(It.IsAny<Delivery>(), It.IsAny<CancellationToken>()), Times.Never);
        _orderRepo.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task register_async_should_throw_not_found_when_order_does_not_exist()
    {
        var sut = CreateSut();
        var req = new RegisterDeliveryRequest("P001", DateTime.UtcNow);

        _orderRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = async () => await sut.RegisterAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("NOT_FOUND");

        _deliveryRepo.Verify(x => x.GetByOrderNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _deliveryRepo.Verify(x => x.CreateAsync(It.IsAny<Delivery>(), It.IsAny<CancellationToken>()), Times.Never);
        _orderRepo.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task register_async_should_throw_conflict_when_delivery_already_registered()
    {
        var sut = CreateSut();
        var req = new RegisterDeliveryRequest("P001", DateTime.UtcNow);

        var order = new Order("P001", "Desc", 10m,
            new Address("01001000", "Rua A", "10", "Bairro", "Cidade", "SP"));

        _orderRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _deliveryRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Delivery("P001", DateTime.UtcNow));

        var act = async () => await sut.RegisterAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("CONFLICT");

        _orderRepo.Verify(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _deliveryRepo.Verify(x => x.CreateAsync(It.IsAny<Delivery>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.PublishToAllAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task register_async_should_mark_order_as_delivered_create_delivery_notify_and_publish()
    {
        // Arrange
        var sut = CreateSut();
        var actorUserId = "user1";
        var deliveredAt = new DateTime(2026, 01, 14, 12, 0, 0, DateTimeKind.Utc);
        var req = new RegisterDeliveryRequest("P001", deliveredAt);

        var order = new Order("P001", "Desc", 10m,
            new Address("01001000", "Rua A", "10", "Bairro", "Cidade", "SP"));

        _orderRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _deliveryRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        _orderRepo
            .Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _deliveryRepo
            .Setup(x => x.CreateAsync(It.IsAny<Delivery>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var createdNotification = new Notification(actorUserId, "Entrega registrada para o pedido P001.");
        _notificationService
            .Setup(x => x.CreateAsync(actorUserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdNotification);

        _publisher
            .Setup(x => x.PublishToAllAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publisher
            .Setup(x => x.PublishToUserAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.RegisterAsync(req, actorUserId, CancellationToken.None);

        // Assert (1) Pedido atualizado para Delivered
        _orderRepo.Verify(x => x.UpdateAsync(It.Is<Order>(o =>
            o.OrderNumber == "P001" &&
            o.Status.ToString() == "Delivered"
        ), It.IsAny<CancellationToken>()), Times.Once);

        // Assert (2) Delivery criada com data informada
        _deliveryRepo.Verify(x => x.CreateAsync(It.Is<Delivery>(d =>
            d.OrderNumber == "P001" &&
            d.DeliveredAt == deliveredAt
        ), It.IsAny<CancellationToken>()), Times.Once);

        // Assert (3) Notificação criada
        _notificationService.Verify(x => x.CreateAsync(actorUserId,
            It.Is<string>(msg => msg.Contains("Entrega registrada")),
            It.IsAny<CancellationToken>()), Times.Once);

        // Assert (4) Realtime publish
        _publisher.Verify(x => x.PublishToAllAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(x => x.PublishToUserAsync(actorUserId, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task register_async_should_use_utcnow_when_deliveredAt_is_null()
    {
        // Arrange
        var sut = CreateSut();
        var actorUserId = "user1";
        var req = new RegisterDeliveryRequest("P001", null);

        var order = new Order("P001", "Desc", 10m,
            new Address("01001000", "Rua A", "10", "Bairro", "Cidade", "SP"));

        _orderRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _deliveryRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        _orderRepo.Setup(x => x.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Delivery? captured = null;
        _deliveryRepo.Setup(x => x.CreateAsync(It.IsAny<Delivery>(), It.IsAny<CancellationToken>()))
            .Callback<Delivery, CancellationToken>((d, _) => captured = d)
            .Returns(Task.CompletedTask);

        _notificationService.Setup(x => x.CreateAsync(actorUserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Notification(actorUserId, "Entrega registrada"));

        _publisher.Setup(x => x.PublishToAllAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publisher.Setup(x => x.PublishToUserAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.RegisterAsync(req, actorUserId, CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.DeliveredAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
    }
}
