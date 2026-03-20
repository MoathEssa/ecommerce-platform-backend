using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Suppliers;
using ECommerceCenter.Domain.Entities.Suppliers;
using ECommerceCenter.Infrastructure.Suppliers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ECommerceCenter.Infrastructure.Workers;

/// <summary>
/// Runs every 6 hours. Finds supplier credentials whose access token will expire within 48 hours
/// and proactively refreshes them so the rest of the system always has a valid token available.
/// CJ's 5-minute rate limit on getAccessToken is not a concern here because this worker
/// only ever calls refreshAccessToken, never the initial authentication endpoint.
/// </summary>
public sealed class SupplierTokenRefreshWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<SupplierTokenRefreshWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private const int RefreshWithinHours = 48;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SupplierTokenRefreshWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshExpiringSoonAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SupplierTokenRefreshWorker encountered an unexpected error.");
            }

            await Task.Delay(Interval, stoppingToken);
        }

        logger.LogInformation("SupplierTokenRefreshWorker stopped.");
    }

    private async Task RefreshExpiringSoonAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<ISupplierCredentialRepository>();
        var factory    = scope.ServiceProvider.GetRequiredService<SupplierAuthServiceFactory>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IEfUnitOfWork>();

        var expiring = await repository.GetAllExpiringSoonAsync(RefreshWithinHours, ct);

        if (expiring.Count == 0)
        {
            logger.LogDebug("No supplier tokens expiring within {Hours}h.", RefreshWithinHours);
            return;
        }

        logger.LogInformation(
            "{Count} supplier token(s) expiring within {Hours}h — refreshing.",
            expiring.Count, RefreshWithinHours);

        foreach (var credential in expiring)
        {
            await RefreshOneAsync(credential, factory, repository, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task RefreshOneAsync(
        SupplierCredential credential,
        SupplierAuthServiceFactory factory,
        ISupplierCredentialRepository repository,
        CancellationToken ct)
    {
        try
        {
            if (credential.RefreshToken is null || credential.IsRefreshTokenExpired)
            {
                logger.LogWarning(
                    "Supplier {Supplier} refresh token is missing or expired — manual re-authentication required.",
                    credential.SupplierType);
                return;
            }

            var service = factory.GetService(credential.SupplierType);
            var result  = await service.RefreshAccessTokenAsync(credential.RefreshToken, ct);

            credential.AccessToken            = result.AccessToken;
            credential.AccessTokenExpiryDate  = result.AccessTokenExpiryDate;
            credential.RefreshToken           = result.RefreshToken;
            credential.RefreshTokenExpiryDate = result.RefreshTokenExpiryDate;
            credential.LastRefreshedAt        = DateTime.UtcNow;

            await repository.UpsertAsync(credential, ct);

            logger.LogInformation(
                "Supplier {Supplier} token refreshed successfully. New expiry: {Expiry}",
                credential.SupplierType, result.AccessTokenExpiryDate);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to refresh token for supplier {Supplier}. Will retry on next cycle.",
                credential.SupplierType);
        }
    }
}
