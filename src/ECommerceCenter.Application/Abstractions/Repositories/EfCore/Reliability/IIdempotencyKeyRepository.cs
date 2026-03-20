using ECommerceCenter.Domain.Entities.Reliability;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;

public interface IIdempotencyKeyRepository : IGenericRepository<IdempotencyKey>
{
    Task<IdempotencyKey?> GetByKeyAndRouteAsync(string key, string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically inserts <paramref name="placeholder"/> (StatusCode = 0) outside any transaction.
    /// Returns <c>null</c> when the current request successfully claimed the key.
    /// Returns the existing record when (Key, Route) already exists so the caller can
    /// determine whether the earlier request is still in-progress or already completed.
    /// </summary>
    Task<IdempotencyKey?> TryReserveAsync(IdempotencyKey placeholder, CancellationToken cancellationToken = default);
}
