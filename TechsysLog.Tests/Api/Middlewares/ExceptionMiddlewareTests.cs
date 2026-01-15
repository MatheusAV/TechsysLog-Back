using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using TechsysLog.Application.DTOs.Orders;
using TechsysLog.Application.Exceptions;
using TechsysLog.Tests.Api.Infrastructure;
using Xunit;

namespace TechsysLog.Tests.Api.Middlewares;

public sealed class ExceptionMiddlewareTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExceptionMiddlewareTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task should_return_400_and_error_payload_when_app_exception_validation()
    {
        // Arrange: endpoint protegido (/orders) -> service lança VALIDATION
        _factory.OrderServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(AppErrors.Validation("CEP inválido."));

        var req = new CreateOrderRequest("P001", "Desc", 10m, "bad", "100");

        // Act
        var resp = await _client.PostAsJsonAsync("/api/v1/orders", req);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Code.Should().Be("VALIDATION");
        body.Error.Message.Should().Contain("CEP inválido");
    }

    [Fact]
    public async Task should_return_404_and_error_payload_when_app_exception_not_found()
    {
        _factory.DeliveryServiceMock
            .Setup(x => x.RegisterAsync(It.IsAny<TechsysLog.Application.DTOs.Deliveries.RegisterDeliveryRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(AppErrors.NotFound("Pedido não encontrado."));

        var req = new TechsysLog.Application.DTOs.Deliveries.RegisterDeliveryRequest("P404", DateTime.UtcNow);

        var resp = await _client.PostAsJsonAsync("/api/v1/deliveries", req);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Code.Should().Be("NOT_FOUND");
        body.Error.Message.Should().Contain("Pedido não encontrado");
    }

    [Fact]
    public async Task should_return_409_and_error_payload_when_app_exception_conflict()
    {
        _factory.OrderServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateOrderRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(AppErrors.Conflict("Já existe pedido com este número."));

        var req = new CreateOrderRequest("P001", "Desc", 10m, "01001000", "100");
        var resp = await _client.PostAsJsonAsync("/api/v1/orders", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Code.Should().Be("CONFLICT");
        body.Error.Message.Should().Contain("Já existe pedido");
    }

    [Fact]
    public async Task should_return_500_and_error_payload_when_unexpected_exception()
    {
        _factory.OrderServiceMock
            .Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var resp = await _client.GetAsync("/api/v1/orders");

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var body = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        body.Should().NotBeNull();
        body!.Error.Code.Should().Be("UNEXPECTED");
        body.Error.Message.Should().Be("Erro inesperado.");
    }
}
