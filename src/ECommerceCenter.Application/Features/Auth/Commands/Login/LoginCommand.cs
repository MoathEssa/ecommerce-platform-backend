using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? GuestSessionId = null)
    : IRequest<Result<AuthResultInternal>>;
