using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string? GuestSessionId = null)
    : IRequest<Result<AuthResultInternal>>;
