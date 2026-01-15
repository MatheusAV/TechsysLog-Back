using TechsysLog.Application.DTOs.Orders;

namespace TechsysLog.Application.Services.Interfaces
{

    public interface IOrderService
    {
        Task CreateAsync(CreateOrderRequest request, string actorUserId, CancellationToken ct);
        Task<List<OrderResponse>> ListAsync(CancellationToken ct);
    }
}
