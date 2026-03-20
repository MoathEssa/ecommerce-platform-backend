using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Domain.Entities.Catalog;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;

public interface IProductImageRepository : IGenericRepository<ProductImage>
{
    Task<List<ProductImage>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);

    Task<ProductImage?> GetByIdAndProductAsync(int imageId, int productId, CancellationToken cancellationToken = default);

    Task<int> GetMaxSortOrderAsync(int productId, CancellationToken cancellationToken = default);

    Task AddRangeAsync(List<ProductImage> images, CancellationToken cancellationToken = default);

    void RemoveRange(List<ProductImage> images);
}
