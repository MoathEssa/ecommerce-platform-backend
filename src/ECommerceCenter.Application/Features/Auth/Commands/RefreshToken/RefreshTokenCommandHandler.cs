using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RefreshTokenEntity = ECommerceCenter.Domain.Entities.Auth.RefreshToken;

namespace ECommerceCenter.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResultInternal>>
{
    public async Task<Result<AuthResultInternal>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var existingToken = await refreshTokenRepository.GetValidTokenWithUserAsync(
            request.RefreshToken, cancellationToken);

        if (existingToken is null)
            return Result<AuthResultInternal>.Unauthorized("Invalid or expired refresh token.");

        var user = existingToken.User;

        if (!user.IsActive)
            return Result<AuthResultInternal>.Forbidden("Your account has been deactivated.");

        // Revoke old token
        refreshTokenRepository.RevokeToken(existingToken, "Replaced by new token");

        var roles = await userManager.GetRolesAsync(user);

        var newAccessToken = tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = tokenService.GenerateRefreshToken();
        var newRefreshTokenExpiration = DateTime.UtcNow.AddDays(tokenService.GetRefreshTokenExpirationDays());

        user.RefreshTokens.Add(new RefreshTokenEntity
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = newRefreshTokenExpiration,
            CreatedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResponse = new AuthResponse(user.Id, user.Email!, roles);
        var result = new AuthResultInternal(authResponse, newAccessToken, newRefreshToken, newRefreshTokenExpiration);

        return Result<AuthResultInternal>.Success(result);
    }
}
