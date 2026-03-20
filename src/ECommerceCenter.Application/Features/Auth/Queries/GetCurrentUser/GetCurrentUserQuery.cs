using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<Result<CurrentUserResponse>>;
