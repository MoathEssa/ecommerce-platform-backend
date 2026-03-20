using ECommerceCenter.Domain.Entities.Orders;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
