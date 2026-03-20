using ECommerceCenter.Domain.Entities.Auth;
using System.Security.Claims;

namespace ECommerceCenter.Application.Abstractions.Identity;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    int GetRefreshTokenExpirationDays();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
