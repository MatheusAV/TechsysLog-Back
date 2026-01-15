using TechsysLog.Application.DTOs.Deliveries;

namespace TechsysLog.Application.Services.Interfaces
{
    public interface IDeliveryService
    {
        Task RegisterAsync(RegisterDeliveryRequest request, string actorUserId, CancellationToken ct);
    }
}
