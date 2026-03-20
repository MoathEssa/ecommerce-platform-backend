using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Catalog;

public class ProductImageRepository(AppDbContext context)
    : GenericRepository<ProductImage>(context), IProductImageRepository
{
    public async Task<List<ProductImage>> GetByProductIdAsync(
        int productId, CancellationToken cancellationToken = default)
        => await Context.Set<ProductImage>()
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.SortOrder).ThenBy(i => i.Id)
            .ToListAsync(cancellationToken);

    public async Task<ProductImage?> GetByIdAndProductAsync(
        int imageId, int productId, CancellationToken cancellationToken = default)
        => await Context.Set<ProductImage>()
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, cancellationToken);

    public async Task<int> GetMaxSortOrderAsync(
        int productId, CancellationToken cancellationToken = default)
    {
        var max = await Context.Set<ProductImage>()
            .Where(i => i.ProductId == productId)
            .MaxAsync(i => (int?)i.SortOrder, cancellationToken);
        return max ?? -1;
    }

    public async Task AddRangeAsync(List<ProductImage> images, CancellationToken cancellationToken = default)
        => await Context.Set<ProductImage>().AddRangeAsync(images, cancellationToken);

    public void RemoveRange(List<ProductImage> images)
        => Context.Set<ProductImage>().RemoveRange(images);
}
