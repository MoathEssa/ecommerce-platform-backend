using ECommerceCenter.Domain.Entities.Reliability;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;

public interface IOutboxMessageRepository : IGenericRepository<OutboxMessage>
{
    Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default);
}
