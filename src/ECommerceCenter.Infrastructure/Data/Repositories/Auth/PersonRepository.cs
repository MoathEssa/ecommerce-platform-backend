using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Auth;

public class PersonRepository(AppDbContext context)
    : GenericRepository<Person>(context), IPersonRepository
{
    public async Task<Person?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
        => await Context.Persons
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
}
