using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;
using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Orders;

public class OrderRepository(AppDbContext context)
    : GenericRepository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        => await Context.Set<Order>()
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await Context.Set<Order>()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
}
