using ECommerceCenter.Application.Abstractions.DTOs.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using CartEntity = ECommerceCenter.Domain.Entities.Cart.Cart;

namespace ECommerceCenter.Infrastructure.Data.Repositories.Cart;

public class CartRepository(AppDbContext context)
    : GenericRepository<CartEntity>(context), ICartRepository
{
    public async Task<CartEntity?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => await Context.Set<CartEntity>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public async Task<CartEntity?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
        => await Context.Set<CartEntity>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId, cancellationToken);

    public async Task ClearItemsAsync(int cartId, CancellationToken cancellationToken = default)
        => await Context.CartItems
            .Where(i => i.CartId == cartId)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task<(int Id, string? CouponCode, string CurrencyCode)?> GetCartHeaderAsync(
        int? userId, string? sessionId, CancellationToken ct = default)
    {
        IQueryable<CartEntity> query = Context.Set<CartEntity>().AsNoTracking();

        if (userId.HasValue)
            query = query.Where(c => c.UserId == userId.Value);
        else if (!string.IsNullOrWhiteSpace(sessionId))
            query = query.Where(c => c.SessionId == sessionId);
        else
            return null;

        var row = await query
            .Select(c => new { c.Id, c.CouponCode, c.CurrencyCode })
            .FirstOrDefaultAsync(ct);

        return row is null ? null : (row.Id, row.CouponCode, row.CurrencyCode);
    }

    public async Task<List<CartItemRow>> GetCartItemsProjectedAsync(int cartId, CancellationToken ct = default)
        => await Context.CartItems
            .AsNoTracking()
            .Where(i => i.CartId == cartId)
            .Select(i => new CartItemRow(
                i.Id,
                i.VariantId,
                i.Quantity,
                i.Variant.IsActive,
                i.Variant.Sku!,
                i.Variant.OptionsJson,
                i.Variant.BasePrice,
                i.Variant.ProductId,
                i.Variant.Product.Title,
                i.Variant.Product.Slug,
                i.Variant.Product.Status,
                i.Variant.InventoryItem != null
                    ? i.Variant.InventoryItem.OnHand
                    : 0,
                i.Variant.Images
                    .OrderBy(img => img.SortOrder)
                    .Select(img => img.Url)
                    .FirstOrDefault()
                ?? i.Variant.Product.Images
                    .Where(img => img.VariantId == null)
                    .OrderBy(img => img.SortOrder)
                    .Select(img => img.Url)
                    .FirstOrDefault()))
            .ToListAsync(ct);

    public async Task<(List<AdminCartListItemDto> Items, int TotalCount)> GetAdminCartsAsync(
        int page, int pageSize, string? search, string? status, CancellationToken ct = default)
    {
        var abandonedCutoff = DateTime.UtcNow.AddHours(-24);

        var query = Context.Carts
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.UserId,
                UserEmail = c.UserId != null
                    ? Context.Users.Where(u => u.Id == c.UserId).Select(u => u.Email).FirstOrDefault()
                    : null,
                UserName = c.UserId != null
                    ? Context.Persons.Where(p => p.UserId == c.UserId)
                        .Select(p => p.FirstName + " " + p.LastName)
                        .FirstOrDefault()
                    : null,
                c.SessionId,
                ItemCount = c.Items.Sum(i => i.Quantity),
                Subtotal  = c.Items.Sum(i => (decimal?)i.Variant.BasePrice * i.Quantity) ?? 0m,
                c.CurrencyCode,
                c.CouponCode,
                c.CreatedAt,
                c.UpdatedAt,
                LastActivity = c.UpdatedAt ?? c.CreatedAt,
            });

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                (c.UserEmail != null && c.UserEmail.ToLower().Contains(term)) ||
                (c.UserName  != null && c.UserName.ToLower().Contains(term))  ||
                (c.SessionId != null && c.SessionId.ToLower().Contains(term)));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (status.Equals("Abandoned", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.LastActivity < abandonedCutoff);
            else if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.LastActivity >= abandonedCutoff);
        }

        var totalCount = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(c => c.LastActivity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(c =>
        {
            var lastActivity = c.UpdatedAt ?? c.CreatedAt;
            var derivedStatus = lastActivity < abandonedCutoff ? "Abandoned" : "Active";

            return new AdminCartListItemDto(
                c.Id,
                c.UserId,
                c.UserEmail,
                c.UserName?.Trim(),
                c.SessionId,
                c.ItemCount,
                Math.Round(c.Subtotal, 2),
                0m,   // DiscountTotal: coupon evaluation is deferred to detail view
                Math.Round(c.Subtotal, 2),
                c.CurrencyCode,
                c.CouponCode,
                derivedStatus,
                c.CreatedAt,
                c.UpdatedAt);
        }).ToList();

        return (items, totalCount);
    }
}
