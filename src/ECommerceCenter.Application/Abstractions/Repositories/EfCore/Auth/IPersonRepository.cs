using ECommerceCenter.Domain.Entities.Auth;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;

public interface IPersonRepository : IGenericRepository<Person>
{
    Task<Person?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
