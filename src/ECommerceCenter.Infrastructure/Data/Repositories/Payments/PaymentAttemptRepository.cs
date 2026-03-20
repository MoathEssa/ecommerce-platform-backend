using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Payments;

public class PaymentAttemptRepository(AppDbContext context)
    : GenericRepository<PaymentAttempt>(context), IPaymentAttemptRepository
{
    public async Task<PaymentAttempt?> GetByProviderIntentIdAsync(
        string provider,
        string providerIntentId,
        CancellationToken cancellationToken = default)
        => await Context.Set<PaymentAttempt>()
            .FirstOrDefaultAsync(
                p => p.Provider == provider && p.ProviderIntentId == providerIntentId,
                cancellationToken);
}
