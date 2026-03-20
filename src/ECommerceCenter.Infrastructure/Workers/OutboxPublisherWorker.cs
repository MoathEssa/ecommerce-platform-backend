using ECommerceCenter.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Workers;

/// <summary>
/// Polls the OutboxMessages table every 5 seconds, processes undelivered events,
/// and marks them as processed. Uses UPDLOCK/READPAST for concurrency safety.
/// Dead-letters messages after 10 failed attempts.
/// </summary>
public class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private const int BatchSize = 50;
    private const int MaxRetries = 10;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxPublisherWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxPublisherWorker encountered an error.");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("OutboxPublisherWorker stopped.");
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Use raw SQL with UPDLOCK, READPAST for concurrency safety
        var messages = await context.OutboxMessages
            .FromSqlRaw(
                """
                SELECT TOP({0}) *
                FROM OutboxMessages WITH (UPDLOCK, READPAST)
                WHERE ProcessedAt IS NULL
                ORDER BY OccurredAt ASC
                """, BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                // Route event to handlers based on Type.
                // In an MVP, this is where you'd publish to a message broker,
                // send emails, update analytics, etc.
                // For now, we log and mark as processed.
                logger.LogInformation(
                    "Processing outbox message {Id}: Type={Type}", message.Id, message.Type);

                message.ProcessedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.LastError = ex.Message;

                if (message.Attempts >= MaxRetries)
                {
                    // Dead-letter: mark as processed but with error
                    message.ProcessedAt = DateTime.UtcNow;
                    logger.LogWarning(
                        "Dead-lettering outbox message {Id} after {Attempts} attempts: {Error}",
                        message.Id, message.Attempts, ex.Message);
                }
                else
                {
                    logger.LogWarning(
                        "Outbox message {Id} failed (attempt {Attempts}): {Error}",
                        message.Id, message.Attempts, ex.Message);
                }
            }
        }

        await context.SaveChangesAsync(ct);

        logger.LogInformation("OutboxPublisherWorker processed {Count} messages.", messages.Count);
    }
}
