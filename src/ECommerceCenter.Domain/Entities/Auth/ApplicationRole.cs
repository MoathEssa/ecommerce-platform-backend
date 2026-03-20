using Microsoft.AspNetCore.Identity;

namespace ECommerceCenter.Domain.Entities.Auth;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
