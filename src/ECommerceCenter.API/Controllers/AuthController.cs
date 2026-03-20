using ECommerceCenter.Application.Common.Helpers;
using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Common.ApiResponse;
using ECommerceCenter.Application.Features.Auth.Commands.Login;
using ECommerceCenter.Application.Features.Auth.Commands.LoginWithGoogle;
using ECommerceCenter.Application.Features.Auth.Commands.Logout;
using ECommerceCenter.Application.Features.Auth.Commands.RefreshToken;
using ECommerceCenter.Application.Features.Auth.Commands.Register;
using ECommerceCenter.Application.Features.Auth.Commands.ForgotPassword;
using ECommerceCenter.Application.Features.Auth.Commands.SetPassword;
using ECommerceCenter.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceCenter.API.Controllers;

public class AuthController(
    IMediator mediator,
    IWebHostEnvironment env) : AppController(mediator)
{
    private const string RefreshTokenCookieName = "refreshToken";

    // POST /api/Auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var guestSessionId = CartCookieHelper.GetSessionId(Request.Cookies);
        var commandWithSession = command with { GuestSessionId = guestSessionId };

        var result = await Mediator.Send(commandWithSession, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var authResult = result.Value!;
        SetRefreshTokenCookie(authResult.RefreshToken, authResult.RefreshTokenExpiration);
        CartCookieHelper.ClearSessionCookie(Response.Cookies, !env.IsDevelopment());

        return Ok(ApiResponseHandler.Created(
            new LoginResponse(authResult.User, authResult.AccessToken),
            message: "Registration successful."));
    }

    // POST /api/Auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var guestSessionId = CartCookieHelper.GetSessionId(Request.Cookies);
        var commandWithSession = command with { GuestSessionId = guestSessionId };

        var result = await Mediator.Send(commandWithSession, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var authResult = result.Value!;
        SetRefreshTokenCookie(authResult.RefreshToken, authResult.RefreshTokenExpiration);
        CartCookieHelper.ClearSessionCookie(Response.Cookies, !env.IsDevelopment());

        return Ok(ApiResponseHandler.Success(
            new LoginResponse(authResult.User, authResult.AccessToken),
            message: "Login successful."));
    }

    // POST /api/Auth/google
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWithGoogle(
        [FromBody] LoginWithGoogleCommand command,
        CancellationToken cancellationToken)
    {
        var guestSessionId = CartCookieHelper.GetSessionId(Request.Cookies);
        var commandWithSession = command with { GuestSessionId = guestSessionId };

        var result = await Mediator.Send(commandWithSession, cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var authResult = result.Value!;
        SetRefreshTokenCookie(authResult.RefreshToken, authResult.RefreshTokenExpiration);
        CartCookieHelper.ClearSessionCookie(Response.Cookies, !env.IsDevelopment());

        return Ok(ApiResponseHandler.Success(
            new LoginResponse(authResult.User, authResult.AccessToken),
            message: "Google login successful."));
    }

    // POST /api/Auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await Mediator.Send(new LogoutCommand(userId), cancellationToken);

        ClearRefreshTokenCookie();
        return HandleResult(result);
    }

    // POST /api/Auth/refresh-token
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName] ?? string.Empty;
        var result = await Mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);

        if (!result.IsSuccess)
            return HandleResult(result);

        var authResult = result.Value!;
        SetRefreshTokenCookie(authResult.RefreshToken, authResult.RefreshTokenExpiration);

        return Ok(ApiResponseHandler.Success(
            new LoginResponse(authResult.User, authResult.AccessToken),
            message: "Token refreshed."));
    }

    // GET /api/Auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return HandleResult(result);
    }

    // POST /api/Auth/forgot-password
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // POST /api/Auth/set-password
    [HttpPost("set-password")]
    [AllowAnonymous]
    public async Task<IActionResult> SetPassword(
        [FromBody] SetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token, DateTime expiration)
    {
        var isSecure = !env.IsDevelopment();
        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax,
            Expires = expiration
        });
    }

    private void ClearRefreshTokenCookie()
    {
        var isSecure = !env.IsDevelopment();
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax
        });
    }
}
