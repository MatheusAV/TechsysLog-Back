using TechsysLog.Domain.Common.Entities;

namespace TechsysLog.Domain.Repositories
{
    public interface IDeliveryRepository
    {
        Task<Delivery?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct);
        Task CreateAsync(Delivery delivery, CancellationToken ct);
    }
}
