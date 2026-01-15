namespace TechsysLog.Application.Abstractions.Cep
{
    public sealed record CepAddressResult(
     string Cep,
     string Street,
     string District,
     string City,
     string State
 );

    public interface ICepService
    {
        Task<CepAddressResult> GetAddressByCepAsync(string cep, CancellationToken ct);
    }
}
