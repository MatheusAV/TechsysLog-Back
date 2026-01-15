namespace TechsysLog.Application.DTOs.Orders
{
    public sealed record CreateOrderRequest(
      string OrderNumber,
      string Description,
      decimal Value,
      string Cep,
      string Number // número do endereço (complemento do CEP)
  );
}
