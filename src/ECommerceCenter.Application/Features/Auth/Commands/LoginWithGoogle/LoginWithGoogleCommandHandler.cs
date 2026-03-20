using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.Constants;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using ECommerceCenter.Domain.Entities.Cart;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RefreshTokenEntity = ECommerceCenter.Domain.Entities.Auth.RefreshToken;

namespace ECommerceCenter.Application.Features.Auth.Commands.LoginWithGoogle;

public class LoginWithGoogleCommandHandler(
    IFirebaseTokenVerifier firebaseTokenVerifier,
    UserManager<ApplicationUser> userManager,
    IApplicationUserRepository userRepository,
    ITokenService tokenService,
    ICartRepository cartRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<LoginWithGoogleCommand, Result<AuthResultInternal>>
{
    private sealed class IdentityException(string message) : Exception(message);

    public async Task<Result<AuthResultInternal>> Handle(
        LoginWithGoogleCommand request,
        CancellationToken cancellationToken)
    {
        var firebaseIdentity = await firebaseTokenVerifier.VerifyIdTokenAsync(
            request.IdToken,
            cancellationToken);

        if (firebaseIdentity is null)
            return Result<AuthResultInternal>.Unauthorized("Invalid Google token.");

        if (!firebaseIdentity.EmailVerified)
            return Result<AuthResultInternal>.Forbidden("Google account email is not verified.");

        AuthResultInternal? authResult = null;
        try
        {
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                var user = await userRepository.GetByEmailWithPersonAsync(firebaseIdentity.Email, ct);

                if (user is null)
                {
                    var (firstName, lastName) = SplitName(firebaseIdentity.DisplayName);

                    user = new ApplicationUser
                    {
                        UserName = firebaseIdentity.Email,
                        Email = firebaseIdentity.Email,
                        IsActive = true,
                        EmailConfirmed = true,
                        Person = new Person
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            AvatarUrl = firebaseIdentity.PhotoUrl
                        }
                    };

                    var createResult = await userManager.CreateAsync(user);
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
                }

                if (!user.IsActive)
                    return;

                // Keep profile fresh from Google if missing locally
                if (user.Person is not null && string.IsNullOrWhiteSpace(user.Person.AvatarUrl))
                    user.Person.AvatarUrl = firebaseIdentity.PhotoUrl;

                var roles = await userManager.GetRolesAsync(user);
                if (!roles.Contains(Roles.User))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, Roles.User);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                        throw new IdentityException(errors);
                    }

                    roles = await userManager.GetRolesAsync(user);
                }

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

                user.LastLoginAt = DateTime.UtcNow;

                if (!string.IsNullOrWhiteSpace(request.GuestSessionId))
                    await MergeGuestCartAsync(user.Id, request.GuestSessionId, ct);

                await unitOfWork.SaveChangesAsync(ct);

                var authResponse = new AuthResponse(
                    user.Id,
                    user.Email!,
                    roles,
                    user.Person?.FirstName,
                    user.Person?.LastName);

                authResult = new AuthResultInternal(
                    authResponse,
                    accessToken,
                    refreshToken,
                    refreshTokenExpiration);
            }, cancellationToken);
        }
        catch (IdentityException ex)
        {
            return Result<AuthResultInternal>.InvalidOperation(ex.Message);
        }

        if (authResult is null)
            return Result<AuthResultInternal>.Forbidden("Your account has been deactivated.");

        return Result<AuthResultInternal>.Success(authResult!);
    }

    private async Task MergeGuestCartAsync(int userId, string guestSessionId, CancellationToken ct)
    {
        var guestCart = await cartRepository.GetBySessionIdAsync(guestSessionId, ct);
        if (guestCart is null || guestCart.Items.Count == 0)
            return;

        var userCart = await cartRepository.GetByUserIdAsync(userId, ct);

        if (userCart is null)
        {
            guestCart.UserId = userId;
            guestCart.SessionId = null;
            guestCart.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            foreach (var guestItem in guestCart.Items)
            {
                var existing = userCart.Items.FirstOrDefault(i => i.VariantId == guestItem.VariantId);
                if (existing is not null)
                    existing.Quantity = Math.Max(existing.Quantity, guestItem.Quantity);
                else
                    userCart.Items.Add(new CartItem
                    {
                        CartId = userCart.Id,
                        VariantId = guestItem.VariantId,
                        Quantity = guestItem.Quantity,
                        CreatedAt = DateTime.UtcNow
                    });
            }

            if (userCart.CouponCode is null && guestCart.CouponCode is not null)
                userCart.CouponCode = guestCart.CouponCode;

            userCart.UpdatedAt = DateTime.UtcNow;
            cartRepository.Delete(guestCart);
        }
    }

    private static (string FirstName, string LastName) SplitName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return (string.Empty, string.Empty);

        var parts = displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return (parts[0], string.Empty);

        return (parts[0], string.Join(' ', parts.Skip(1)));
    }
}
