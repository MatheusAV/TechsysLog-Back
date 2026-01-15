using FluentAssertions;
using Moq;
using TechsysLog.Application.Abstractions.Cep;
using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Application.Exceptions;
using TechsysLog.Application.Services;
using TechsysLog.Application.Services.Interfaces;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Domain.Repositories;

namespace TechsysLog.Tests.Application.Services;

public sealed class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICepService> _cepService = new();
    private readonly Mock<INotificationService> _notificationService = new();
    private readonly Mock<INotificationPublisher> _publisher = new();

    private OrderService CreateSut()
        => new(_orderRepo.Object, _cepService.Object, _notificationService.Object, _publisher.Object);

    [Fact]
    public async Task create_async_should_throw_validation_when_orderNumber_is_empty()
    {
        var sut = CreateSut();
        var req = new CreateOrderRequest("", "desc", 10m, "01001000", "100");

        var act = async () => await sut.CreateAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Número do pedido");

        _orderRepo.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_throw_validation_when_value_is_invalid()
    {
        var sut = CreateSut();
        var req = new CreateOrderRequest("P001", "desc", 0m, "01001000", "100");

        var act = async () => await sut.CreateAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Valor");

        _orderRepo.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_throw_validation_when_cep_is_empty()
    {
        var sut = CreateSut();
        var req = new CreateOrderRequest("P001", "desc", 10m, "", "100");

        var act = async () => await sut.CreateAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("CEP");

        _orderRepo.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_throw_validation_when_address_number_is_empty()
    {
        var sut = CreateSut();
        var req = new CreateOrderRequest("P001", "desc", 10m, "01001000", "");

        var act = async () => await sut.CreateAsync(req, "user1", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("Número do endereço");

        _orderRepo.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_throw_conflict_when_orderNumber_already_exists()
    {
        // Arrange
        var sut = CreateSut();
        var req = new CreateOrderRequest("P001", "desc", 10m, "01001000", "100");

        _orderRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Order("P001", "old", 10m,
                new TechsysLog.Domain.Common.ValueObjects.Address("01001000", "Rua", "1", "Centro", "SP", "SP")));

        // Act
        var act = async () => await sut.CreateAsync(req, "user1", CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("CONFLICT");

        _cepService.Verify(x => x.GetAddressByCepAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _orderRepo.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationService.Verify(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisher.Verify(x => x.PublishToAllAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task create_async_should_create_order_and_notify_and_publish_when_valid()
    {
        // Arrange
        var sut = CreateSut();
        var req = new CreateOrderRequest("P001", "Notebook", 3500m, "01001000", "100");
        var actorUserId = "user1";

        _orderRepo
            .Setup(x => x.GetByOrderNumberAsync("P001", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        _cepService
            .Setup(x => x.GetAddressByCepAsync("01001000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CepAddressResult(
                Cep: "01001-000",
                Street: "Praça da Sé",
                District: "Sé",
                City: "São Paulo",
                State: "SP"
            ));

        // NotificationService retorna entidade Notification (Domain)
        var createdNotification = new Notification(actorUserId, "Pedido P001 cadastrado.");
        _notificationService
            .Setup(x => x.CreateAsync(actorUserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdNotification);

        _orderRepo
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publisher
            .Setup(x => x.PublishToAllAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publisher
            .Setup(x => x.PublishToUserAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.CreateAsync(req, actorUserId, CancellationToken.None);

        // Assert (1) Persiste pedido com endereço resolvido
        _orderRepo.Verify(x => x.CreateAsync(It.Is<Order>(o =>
            o.OrderNumber == "P001" &&
            o.Description == "Notebook" &&
            o.Value == 3500m &&
            o.DeliveryAddress.Cep == "01001-000" &&
            o.DeliveryAddress.Street == "Praça da Sé" &&
            o.DeliveryAddress.Number == "100" &&
            o.DeliveryAddress.District == "Sé" &&
            o.DeliveryAddress.City == "São Paulo" &&
            o.DeliveryAddress.State == "SP"
        ), It.IsAny<CancellationToken>()), Times.Once);

        // Assert (2) Notificação criada
        _notificationService.Verify(x => x.CreateAsync(actorUserId,
            It.Is<string>(msg => msg.Contains("Pedido P001 cadastrado")),
            It.IsAny<CancellationToken>()), Times.Once);

        // Assert (3) Realtime publish
        _publisher.Verify(x => x.PublishToAllAsync(It.Is<object>(p => p.ToString()!.Contains("ORDER_CREATED")), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(x => x.PublishToUserAsync(actorUserId, It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task list_async_should_map_orders_to_response_correctly()
    {
        // Arrange
        var sut = CreateSut();

        var order1 = new Order("P001", "Desc", 10m,
            new TechsysLog.Domain.Common.ValueObjects.Address("01001000", "Rua A", "10", "Bairro", "Cidade", "SP"));

        var order2 = new Order("P002", "Desc2", 20m,
            new TechsysLog.Domain.Common.ValueObjects.Address("22222222", "Rua B", "20", "Bairro2", "Cidade2", "RJ"));

        _orderRepo
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order1, order2 });

        // Act
        var list = await sut.ListAsync(CancellationToken.None);

        // Assert
        list.Should().HaveCount(2);

        list[0].OrderNumber.Should().Be("P001");
        list[0].Cep.Should().Be("01001000");
        list[0].Street.Should().Be("Rua A");
        list[0].Number.Should().Be("10");
        list[0].State.Should().Be("SP");

        list[1].OrderNumber.Should().Be("P002");
        list[1].Cep.Should().Be("22222222");
        list[1].Street.Should().Be("Rua B");
        list[1].Number.Should().Be("20");
        list[1].State.Should().Be("RJ");
    }
}
