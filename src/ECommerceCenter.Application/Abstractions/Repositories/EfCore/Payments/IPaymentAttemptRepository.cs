using ECommerceCenter.Domain.Entities.Payments;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;

public interface IPaymentAttemptRepository : IGenericRepository<PaymentAttempt>
{
    Task<PaymentAttempt?> GetByProviderIntentIdAsync(string provider, string providerIntentId, CancellationToken cancellationToken = default);
}
