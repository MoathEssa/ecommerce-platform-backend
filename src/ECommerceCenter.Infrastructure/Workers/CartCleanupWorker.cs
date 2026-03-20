using ECommerceCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Workers;

/// <summary>
/// Runs daily at 04:00 UTC. Removes guest carts (UserId IS NULL)
/// with no activity for 7+ days, in batches of 100.
/// Authenticated user carts are never auto-deleted.
/// </summary>
public class CartCleanupWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<CartCleanupWorker> logger) : BackgroundService
{
    private const int BatchSize = 100;
    private static readonly TimeSpan AbandonedThreshold = TimeSpan.FromDays(7);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CartCleanupWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            await Task.Delay(delay, stoppingToken);

            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CartCleanupWorker encountered an error.");
            }
        }

        logger.LogInformation("CartCleanupWorker stopped.");
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var cutoff = DateTime.UtcNow - AbandonedThreshold;
        var totalDeleted = 0;
        int deleted;

        do
        {
            // Guest carts: UserId IS NULL, last activity > 7 days ago
            // CartItems cascade-delete with the Cart parent
            deleted = await context.Carts
                .Where(c => c.UserId == null && (c.UpdatedAt ?? c.CreatedAt) < cutoff)
                .Take(BatchSize)
                .ExecuteDeleteAsync(ct);

            totalDeleted += deleted;
        } while (deleted == BatchSize && !ct.IsCancellationRequested);

        if (totalDeleted > 0)
            logger.LogInformation(
                "CartCleanupWorker cleaned up {Count} abandoned guest carts.", totalDeleted);
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(4); // 04:00 UTC today
        if (nextRun <= now)
            nextRun = nextRun.AddDays(1); // 04:00 UTC tomorrow
        return nextRun - now;
    }
}
