using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Commands.SetPassword;

public record SetPasswordCommand(
    int UserId,
    string Token,
    string Password,
    string ConfirmPassword
) : IRequest<Result<string>>;
