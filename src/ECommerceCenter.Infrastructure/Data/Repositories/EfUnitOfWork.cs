using ECommerceCenter.Application.Abstractions.Repositories.EfCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories;

public class EfUnitOfWork(AppDbContext context) : IEfUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await transaction.CommitAsync(cancellationToken);


    }

      public Task ClearTrackedChangesAsync(CancellationToken cancellationToken = default)
    {
        context.ChangeTracker.Clear();
        return Task.CompletedTask;
    }
}
