using ECommerceCenter.Application.Abstractions.DTOs.Auth;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ECommerceCenter.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    IApplicationUserRepository userRepository,
    UserManager<ApplicationUser> userManager)
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    public async Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
            return Result<CurrentUserResponse>.Unauthorized("User is not authenticated.");

        var user = await userRepository.GetByIdWithPersonAsync(
            currentUserService.UserId.Value, cancellationToken);

        if (user is null)
            return Result<CurrentUserResponse>.NotFound("User", currentUserService.UserId.Value);

        var roles = await userManager.GetRolesAsync(user);

        var personDto = user.Person is not null
            ? new PersonDto(
                user.Person.FirstName,
                user.Person.LastName,
                user.Person.Phone,
                user.Person.DateOfBirth,
                user.Person.AvatarUrl,
                user.Person.Gender)
            : null;

        var response = new CurrentUserResponse(
            user.Id,
            user.Email!,
            roles,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt,
            personDto);

        return Result<CurrentUserResponse>.Success(response);
    }
}
