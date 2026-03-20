using ECommerceCenter.Domain.Entities.Payments;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;

public interface IRefundRepository : IGenericRepository<Refund>
{
    Task<Refund?> GetByProviderRefundIdAsync(string providerRefundId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Refund>> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalRefundedAmountAsync(int orderId, CancellationToken cancellationToken = default);
}
