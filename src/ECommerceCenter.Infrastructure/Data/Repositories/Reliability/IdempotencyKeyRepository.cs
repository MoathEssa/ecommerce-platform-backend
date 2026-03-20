using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Reliability;

public class IdempotencyKeyRepository(AppDbContext context)
    : GenericRepository<IdempotencyKey>(context), IIdempotencyKeyRepository
{
    public async Task<IdempotencyKey?> GetByKeyAndRouteAsync(
        string key,
        string route,
        CancellationToken cancellationToken = default)
        => await Context.Set<IdempotencyKey>()
            .FirstOrDefaultAsync(k => k.Key == key && k.Route == route, cancellationToken);

    public async Task<IdempotencyKey?> TryReserveAsync(
        IdempotencyKey placeholder,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // AddAsync marks the entity as Added; SaveChangesAsync commits it immediately,
            // outside any Unit-of-Work transaction, so concurrent requests see it right away.
            Context.Set<IdempotencyKey>().Add(placeholder);
            await Context.SaveChangesAsync(cancellationToken);
            return null; // this request claimed the key
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException { Number: 2627 or 2601 })
        {
            // Unique constraint on (Key, Route) fired — another request owns this key.
            // Detach the untracked placeholder and reload whatever is actually in the DB.
            Context.Entry(placeholder).State = EntityState.Detached;
            return await GetByKeyAndRouteAsync(placeholder.Key, placeholder.Route, cancellationToken);
        }
    }
}
