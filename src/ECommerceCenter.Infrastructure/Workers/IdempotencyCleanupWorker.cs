using ECommerceCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Workers;

/// <summary>
/// Runs daily at 03:00 UTC. Batch-deletes expired IdempotencyKeys
/// (1000 at a time) to prevent unbounded table growth.
/// </summary>
public class IdempotencyCleanupWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<IdempotencyCleanupWorker> logger) : BackgroundService
{
    private const int BatchSize = 1000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("IdempotencyCleanupWorker started.");

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
                logger.LogError(ex, "IdempotencyCleanupWorker encountered an error.");
            }
        }

        logger.LogInformation("IdempotencyCleanupWorker stopped.");
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var totalDeleted = 0;
        int deleted;

        do
        {
            deleted = await context.IdempotencyKeys
                .Where(k => k.ExpiresAt != null && k.ExpiresAt < DateTime.UtcNow)
                .Take(BatchSize)
                .ExecuteDeleteAsync(ct);

            totalDeleted += deleted;
        } while (deleted == BatchSize && !ct.IsCancellationRequested);

        if (totalDeleted > 0)
            logger.LogInformation(
                "IdempotencyCleanupWorker purged {Count} expired idempotency keys.", totalDeleted);
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddHours(3); // 03:00 UTC today
        if (nextRun <= now)
            nextRun = nextRun.AddDays(1); // 03:00 UTC tomorrow
        return nextRun - now;
    }
}
