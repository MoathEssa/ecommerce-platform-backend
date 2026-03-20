using System.Web;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ECommerceCenter.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    IConfiguration configuration)
    : IRequestHandler<ForgotPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // SECURITY: Always return the same message to prevent user enumeration
        const string safeMessage = "If an account with that email exists, a password reset link has been sent.";

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result<string>.Success(safeMessage);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var frontendUrl = configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:3000";
        var encodedToken = HttpUtility.UrlEncode(token);
        var resetLink = $"{frontendUrl}/auth/reset-password?userId={user.Id}&token={encodedToken}";

        var emailBody = $"""
            <h2>Password Reset — ECommerce Center</h2>
            <p>You requested a password reset. Click the link below:</p>
            <p><a href="{resetLink}">Reset Your Password</a></p>
            <p>This link will expire in 24 hours.</p>
            <p>If you did not request this, please ignore this email.</p>
            """;

        await emailService.SendAsync(
            request.Email,
            "Reset Your Password — ECommerce Center",
            emailBody,
            cancellationToken);

        return Result<string>.Success(safeMessage);
    }
}
