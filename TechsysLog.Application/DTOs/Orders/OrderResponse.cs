namespace TechsysLog.Application.DTOs.Orders
{
    public sealed record OrderResponse(
     string OrderNumber,
     string Description,
     decimal Value,
     string Cep,
     string Street,
     string Number,
     string District,
     string City,
     string State,
     string Status,
     DateTime CreatedAt
 );
}
