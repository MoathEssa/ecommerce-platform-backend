using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Catalog;

public class CategoryRepository(AppDbContext context)
    : GenericRepository<Category>(context), ICategoryRepository
{
    public async Task<Category?> GetBySlugAsync(
        string slug, CancellationToken cancellationToken = default)
        => await Context.Set<Category>()
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public async Task<bool> SlugExistsAsync(
        string slug, int? excludeId, CancellationToken cancellationToken = default)
        => await Context.Set<Category>()
            .AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId.Value), cancellationToken);

    public async Task<bool> HasChildrenAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Set<Category>()
            .AnyAsync(c => c.ParentId == id, cancellationToken);

    public async Task<bool> HasProductAssignmentsAsync(int id, CancellationToken cancellationToken = default)
        => await Context.Products
            .AnyAsync(p => p.CategoryId == id, cancellationToken);

    public async Task<List<Category>> GetAllActiveAsync(CancellationToken ct = default)
        => await Context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
        => await Context.Set<Category>()
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ToListAsync(ct);

    public async Task<List<CatalogSearchRow>> SearchByNameAsync(
        string term, int limit, CancellationToken ct = default)
        => await Context.Set<Category>()
            .AsNoTracking()
            .Where(c => c.IsActive && c.Name.StartsWith(term))
            .OrderBy(c => c.Name)
            .Take(limit)
            .Select(c => new CatalogSearchRow(c.Name, c.Slug))
            .ToListAsync(ct);

    public async Task<bool> SortOrderExistsAmongSiblingsAsync(
        int? parentId, int sortOrder, int? excludeId, CancellationToken ct = default)
        => await Context.Set<Category>()
            .AnyAsync(
                c => c.ParentId == parentId
                  && c.SortOrder == sortOrder
                  && (excludeId == null || c.Id != excludeId.Value),
                ct);

    public async Task<long> NextSlugSuffixAsync(CancellationToken ct = default)
    {
        var conn = Context.Database.GetDbConnection();
        var wasOpen = conn.State == ConnectionState.Open;
        if (!wasOpen) await conn.OpenAsync(ct);
        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NEXT VALUE FOR slug_suffix_seq";
            if (Context.Database.CurrentTransaction is { } tx)
                cmd.Transaction = tx.GetDbTransaction();
            var result = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt64(result);
        }
        finally
        {
            if (!wasOpen) await conn.CloseAsync();
        }
    }
}
