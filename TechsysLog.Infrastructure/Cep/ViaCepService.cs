using System.Net.Http.Json;
using TechsysLog.Application.Abstractions.Cep;
using TechsysLog.Application.Exceptions;

namespace TechsysLog.Infrastructure.Cep
{
    public sealed class ViaCepService : ICepService
    {
        private readonly HttpClient _http;

        public ViaCepService(HttpClient http) => _http = http;

        private sealed class ViaCepResponse
        {
            public string? Cep { get; set; }
            public string? Logradouro { get; set; }
            public string? Bairro { get; set; }
            public string? Localidade { get; set; }
            public string? Uf { get; set; }
            public bool? Erro { get; set; }
        }

        public async Task<CepAddressResult> GetAddressByCepAsync(string cep, CancellationToken ct)
        {
            var normalized = new string(cep.Where(char.IsDigit).ToArray());
            if (normalized.Length != 8) throw AppErrors.Validation("CEP inválido (deve conter 8 dígitos).");

            var url = $"https://viacep.com.br/ws/{normalized}/json/";

            ViaCepResponse? resp;
            try
            {
                resp = await _http.GetFromJsonAsync<ViaCepResponse>(url, cancellationToken: ct);
            }
            catch
            {
                throw new AppException("CEP_PROVIDER_ERROR", "Falha ao consultar serviço de CEP.", 502);
            }

            if (resp is null || resp.Erro == true)
                throw AppErrors.Validation("CEP não encontrado.");

            return new CepAddressResult(
                Cep: resp.Cep ?? normalized,
                Street: resp.Logradouro ?? "",
                District: resp.Bairro ?? "",
                City: resp.Localidade ?? "",
                State: resp.Uf ?? ""
            );
        }
    }
}
