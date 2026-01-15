using FluentAssertions;
using System.Net;
using TechsysLog.Application.Abstractions.Cep;
using TechsysLog.Application.Exceptions;
using TechsysLog.Infrastructure.Cep;
using TechsysLog.Tests.Infrastructure.Http;
using Xunit;

namespace TechsysLog.Tests.Infrastructure.Cep;

public sealed class ViaCepServiceTests
{
    [Fact]
    public async Task get_address_by_cep_async_should_throw_validation_when_cep_not_8_digits()
    {
        var http = new HttpClient(new FakeHttpMessageHandler(_ => FakeHttpMessageHandler.Json("{}")));
        var sut = new ViaCepService(http);

        var act = async () => await sut.GetAddressByCepAsync("123", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("CEP inválido");
    }

    [Fact]
    public async Task get_address_by_cep_async_should_throw_validation_when_provider_returns_erro_true()
    {
        var http = new HttpClient(new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json("""{"erro": true}""")
        ));
        var sut = new ViaCepService(http);

        var act = async () => await sut.GetAddressByCepAsync("01001000", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("VALIDATION");
        ex.Which.Message.Should().Contain("CEP não encontrado");
    }

    [Fact]
    public async Task get_address_by_cep_async_should_return_mapped_address_when_ok()
    {
        var json = """
        {
          "cep": "01001-000",
          "logradouro": "Praça da Sé",
          "bairro": "Sé",
          "localidade": "São Paulo",
          "uf": "SP"
        }
        """;

        var http = new HttpClient(new FakeHttpMessageHandler(req =>
        {
            req.RequestUri!.ToString().Should().Contain("/ws/01001000/json/");
            return FakeHttpMessageHandler.Json(json);
        }));

        var sut = new ViaCepService(http);

        var result = await sut.GetAddressByCepAsync("01001-000", CancellationToken.None);

        result.Cep.Should().Be("01001-000");
        result.Street.Should().Be("Praça da Sé");
        result.District.Should().Be("Sé");
        result.City.Should().Be("São Paulo");
        result.State.Should().Be("SP");
    }

    [Fact]
    public async Task get_address_by_cep_async_should_throw_502_when_http_client_fails()
    {
        var http = new HttpClient(new FakeHttpMessageHandler(_ => throw new HttpRequestException("fail")));
        var sut = new ViaCepService(http);

        var act = async () => await sut.GetAddressByCepAsync("01001000", CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Code.Should().Be("CEP_PROVIDER_ERROR");
        ex.Which.HttpStatusHint.Should().Be(502);
    }
}
