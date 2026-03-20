namespace ECommerceCenter.Application.Abstractions.Identity;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
