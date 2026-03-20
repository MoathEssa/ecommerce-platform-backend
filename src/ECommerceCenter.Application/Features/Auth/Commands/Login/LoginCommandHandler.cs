using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using ECommerceCenter.Domain.Entities.Cart;
using MediatR;
using Microsoft.AspNetCore.Identity;
using RefreshTokenEntity = ECommerceCenter.Domain.Entities.Auth.RefreshToken;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationUserRepository userRepository,
    ITokenService tokenService,
    ICartRepository cartRepository,
    IEfUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, Result<AuthResultInternal>>
{
    public async Task<Result<AuthResultInternal>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Load with Person so we can include name in the response
        var user = await userRepository.GetByEmailWithPersonAsync(request.Email, cancellationToken);
        if (user is null)
            return Result<AuthResultInternal>.Unauthorized("Invalid email or password.");

        if (!user.IsActive)
            return Result<AuthResultInternal>.Forbidden("Your account has been deactivated.");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Result<AuthResultInternal>.Unauthorized("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);

        var accessToken = tokenService.GenerateAccessToken(user, roles);
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(tokenService.GetRefreshTokenExpirationDays());

        // Writes span AspNetUsers (LastLoginAt), RefreshTokens, and Carts (guest merge) —
        // wrap in an explicit transaction so all three tables are committed atomically.
        AuthResultInternal? authResult = null;

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            user.RefreshTokens.Add(new RefreshTokenEntity
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = refreshTokenExpiration,
                CreatedAt = DateTime.UtcNow
            });

            user.LastLoginAt = DateTime.UtcNow;

            // Merge guest cart into user cart (if a guest session id was passed along with the login request)
            if (!string.IsNullOrWhiteSpace(request.GuestSessionId))
                await MergeGuestCartAsync(user.Id, request.GuestSessionId, ct);

            await unitOfWork.SaveChangesAsync(ct);

            var authResponse = new AuthResponse(
                user.Id,
                user.Email!,
                roles,
                user.Person?.FirstName,
                user.Person?.LastName);

            authResult = new AuthResultInternal(authResponse, accessToken, refreshToken, refreshTokenExpiration);
        }, cancellationToken);

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
            // Transfer: adopt the guest cart as the user cart
            guestCart.UserId = userId;
            guestCart.SessionId = null;
            guestCart.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Merge: move guest items into user cart
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

            // Transfer coupon if user cart has none
            if (userCart.CouponCode is null && guestCart.CouponCode is not null)
                userCart.CouponCode = guestCart.CouponCode;

            userCart.UpdatedAt = DateTime.UtcNow;

            // Delete guest cart
            cartRepository.Delete(guestCart);
        }
    }
}
