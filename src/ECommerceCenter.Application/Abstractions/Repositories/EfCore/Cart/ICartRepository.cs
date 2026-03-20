using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;

public interface ICartRepository : IGenericRepository<CartEntity>
{
    Task<CartEntity?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<CartEntity?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk-deletes all items for the given cart in a single SQL DELETE statement.
    /// </summary>
    Task ClearItemsAsync(int cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lightweight header-only read: resolves the cart by userId or sessionId and
    /// returns just the fields needed to drive the cart read pipeline.
    /// Returns null when no cart exists yet.
    /// </summary>
    Task<(int Id, string? CouponCode, string CurrencyCode)?> GetCartHeaderAsync(
        int? userId, string? sessionId, CancellationToken ct = default);

    /// <summary>
    /// Projects all line-items for a given cart in a single query
    /// (no Include chains — navigation fields are inlined via SELECT).
    /// </summary>
    Task<List<CartItemRow>> GetCartItemsProjectedAsync(int cartId, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated list of all customer carts for the admin dashboard.
    /// Status is derived: "Abandoned" if last activity &gt; 24 h ago, otherwise "Active".
    /// </summary>
    Task<(List<Application.Abstractions.DTOs.Cart.AdminCartListItemDto> Items, int TotalCount)> GetAdminCartsAsync(
        int page, int pageSize, string? search, string? status, CancellationToken ct = default);
}
