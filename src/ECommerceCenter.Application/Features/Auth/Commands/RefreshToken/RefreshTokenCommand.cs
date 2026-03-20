using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<AuthResultInternal>>;
