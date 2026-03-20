using ECommerceCenter.Domain.Entities.Payments;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;

public interface IPaymentProviderEventRepository : IGenericRepository<PaymentProviderEvent>
{
    /// <summary>Returns true if the event was already processed — used for webhook dedupe.</summary>
    Task<bool> ExistsByProviderEventIdAsync(string provider, string eventId, CancellationToken cancellationToken = default);
}
