using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Enums;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Payments;

public class RefundRepository(AppDbContext context)
    : GenericRepository<Refund>(context), IRefundRepository
{
    public async Task<Refund?> GetByProviderRefundIdAsync(
        string providerRefundId,
        CancellationToken cancellationToken = default)
        => await Context.Set<Refund>()
            .FirstOrDefaultAsync(r => r.ProviderRefundId == providerRefundId, cancellationToken);

    public async Task<IEnumerable<Refund>> GetByOrderIdAsync(
        int orderId,
        CancellationToken cancellationToken = default)
        => await Context.Set<Refund>()
            .Where(r => r.OrderId == orderId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalRefundedAmountAsync(
        int orderId,
        CancellationToken cancellationToken = default)
        => await Context.Set<Refund>()
            .Where(r => r.OrderId == orderId && r.Status == RefundStatus.Succeeded)
            .SumAsync(r => r.Amount, cancellationToken);
}
