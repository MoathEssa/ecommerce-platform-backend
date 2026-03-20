using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Commands.LoginWithGoogle;

public record LoginWithGoogleCommand(string IdToken, string? GuestSessionId = null)
    : IRequest<Result<AuthResultInternal>>;
