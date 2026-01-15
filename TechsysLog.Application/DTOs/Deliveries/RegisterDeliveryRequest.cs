namespace TechsysLog.Application.DTOs.Deliveries
{
    public sealed record RegisterDeliveryRequest(
     string OrderNumber,
     DateTime? DeliveredAt
 );
}
