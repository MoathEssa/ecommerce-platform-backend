using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RefreshTokenEntity = ECommerceCenter.Domain.Entities.Auth.RefreshToken;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ICartRepository cartRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, Result<AuthResultInternal>>
{
    private sealed class IdentityException(string message) : Exception(message);

    public async Task<Result<AuthResultInternal>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Password != request.ConfirmPassword)
            return Result<AuthResultInternal>.ValidationError("Passwords do not match.");

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return Result<AuthResultInternal>.Duplicate("Email", request.Email);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            IsActive = true,
            EmailConfirmed = false,
            Person = new Person
            {
                FirstName = string.Empty,
                LastName = string.Empty
            }
        };

        AuthResultInternal? authResult = null;

        
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var createResult = await userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new IdentityException(errors);
                }

                var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
                
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    throw new IdentityException(errors);
                }

                var roles = await userManager.GetRolesAsync(user);

                var accessToken = tokenService.GenerateAccessToken(user, roles);
                var refreshToken = tokenService.GenerateRefreshToken();
                var refreshTokenExpiration = DateTime.UtcNow.AddDays(tokenService.GetRefreshTokenExpirationDays());

                user.RefreshTokens.Add(new RefreshTokenEntity
                {
                    Token = refreshToken,
                    UserId = user.Id,
                    ExpiresAt = refreshTokenExpiration,
                    CreatedAt = DateTime.UtcNow
                });

                // Merge guest cart into the newly created user's cart
                if (!string.IsNullOrWhiteSpace(request.GuestSessionId))
                    await MergeGuestCartAsync(user.Id, request.GuestSessionId, ct);

                await unitOfWork.SaveChangesAsync(ct);

                var authResponse = new AuthResponse(
                    user.Id,
                    user.Email!,
                    roles,
                    user.Person.FirstName,
                    user.Person.LastName);

                authResult = new AuthResultInternal(authResponse, accessToken, refreshToken, refreshTokenExpiration);
            }, cancellationToken);
        

        return Result<AuthResultInternal>.Success(authResult!);
    }

    private async Task MergeGuestCartAsync(int userId, string guestSessionId, CancellationToken ct)
    {
        var guestCart = await cartRepository.GetBySessionIdAsync(guestSessionId, ct);
        if (guestCart is null || guestCart.Items.Count == 0)
            return;

        // New user has no cart yet — transfer the guest cart
        guestCart.UserId = userId;
        guestCart.SessionId = null;
        guestCart.UpdatedAt = DateTime.UtcNow;
    }
}
