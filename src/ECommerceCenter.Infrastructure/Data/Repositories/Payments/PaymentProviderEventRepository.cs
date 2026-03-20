using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Payments;

public class PaymentProviderEventRepository(AppDbContext context)
    : GenericRepository<PaymentProviderEvent>(context), IPaymentProviderEventRepository
{
    public async Task<bool> ExistsByProviderEventIdAsync(
        string provider,
        string eventId,
        CancellationToken cancellationToken = default)
        => await Context.Set<PaymentProviderEvent>()
            .AnyAsync(e => e.Provider == provider && e.EventId == eventId, cancellationToken);
}
