using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Reliability;

public class OutboxMessageRepository(AppDbContext context)
    : GenericRepository<OutboxMessage>(context), IOutboxMessageRepository
{
    public async Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
        => await Context.Set<OutboxMessage>()
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
}
