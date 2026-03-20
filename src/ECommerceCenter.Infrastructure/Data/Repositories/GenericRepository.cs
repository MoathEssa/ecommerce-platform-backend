using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ECommerceCenter.Infrastructure.Data.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T>
    where T : class
{
    protected readonly AppDbContext Context = context;

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Set<T>().FindAsync([id], cancellationToken);

    public async Task<IEnumerable<T>> FindAllAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Context.Set<T>();

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await Context.Set<T>().AnyAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await Context.Set<T>().AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => Context.Set<T>().Update(entity);

    public void Delete(T entity)
        => Context.Set<T>().Remove(entity);
}
