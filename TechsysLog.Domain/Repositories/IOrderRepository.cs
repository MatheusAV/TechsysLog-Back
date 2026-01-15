using TechsysLog.Domain.Common.Entities;

namespace TechsysLog.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct);
        Task<List<Order>> ListAsync(CancellationToken ct);
        Task CreateAsync(Order order, CancellationToken ct);
        Task UpdateAsync(Order order, CancellationToken ct);
    }
}
