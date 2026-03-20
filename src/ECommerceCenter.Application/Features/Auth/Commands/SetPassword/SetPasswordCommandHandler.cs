using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECommerceCenter.Application.Features.Auth.Commands.SetPassword;

public class SetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<SetPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        SetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            return Result<string>.NotFound("User", request.UserId);

        // ResetPasswordAsync validates the token and sets the new password hash
        var result = await userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result<string>.ValidationError(errors);
        }

        // Mark email as confirmed — user proved inbox access via the reset link
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        return Result<string>.Success("Password set successfully. You can now log in.");
    }
}
