namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore;

public interface IEfUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction.
    /// Commits on success; the transaction is rolled back automatically on exception.
    /// No try/catch needed — exceptions propagate to the global exception-handling middleware.
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task ClearTrackedChangesAsync(CancellationToken cancellationToken = default);

}
