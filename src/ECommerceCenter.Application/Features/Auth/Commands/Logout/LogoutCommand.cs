using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(int UserId) : IRequest<Result>;
