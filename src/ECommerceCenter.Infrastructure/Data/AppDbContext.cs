using ECommerceCenter.Domain.Entities.Auth;
using ECommerceCenter.Domain.Entities.Cart;
using ECommerceCenter.Domain.Entities.Catalog;
using ECommerceCenter.Domain.Entities.Coupons;
using ECommerceCenter.Domain.Entities.Inventory;
using ECommerceCenter.Domain.Entities.Orders;
using ECommerceCenter.Domain.Entities.Payments;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Entities.Shipping;
using ECommerceCenter.Domain.Entities.Suppliers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerceCenter.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>(options)
{
    // ── Auth ──────────────────────────────────────────────────────────────
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Person> Persons { get; set; } = null!;

    // ── Catalog ───────────────────────────────────────────────────────────
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    // ── Inventory ─────────────────────────────────────────────────────────
    public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
    public DbSet<InventoryAdjustment> InventoryAdjustments { get; set; } = null!;

    // ── Orders ────────────────────────────────────────────────────────────
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<OrderAddress> OrderAddresses { get; set; } = null!;
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } = null!;

    // ── Payments ──────────────────────────────────────────────────────────
    public DbSet<PaymentAttempt> PaymentAttempts { get; set; } = null!;
    public DbSet<PaymentProviderEvent> PaymentProviderEvents { get; set; } = null!;
    public DbSet<Refund> Refunds { get; set; } = null!;

    // ── Shipping ──────────────────────────────────────────────────────────
    public DbSet<Shipment> Shipments { get; set; } = null!;
    // ── Suppliers ────────────────────────────────────────────────────
    public DbSet<SupplierCredential> SupplierCredentials { get; set; } = null!;
    // ── Reliability ───────────────────────────────────────────────────────
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;


    // ── Cart ──────────────────────────────────────────────────────────────
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<CartItem> CartItems { get; set; } = null!;

    // ── Coupons ───────────────────────────────────────────────────────────
    public DbSet<Coupon> Coupons { get; set; } = null!;
    public DbSet<CouponApplicableCategory> CouponApplicableCategories { get; set; } = null!;
    public DbSet<CouponApplicableProduct> CouponApplicableProducts { get; set; } = null!;
    public DbSet<CouponApplicableVariant> CouponApplicableVariants { get; set; } = null!;
    public DbSet<CouponUsage> CouponUsages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
            entity.Ignore(rt => rt.IsExpired);
            entity.Ignore(rt => rt.IsActive);

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.IsActive).HasDefaultValue(true);
        });

        // All IEntityTypeConfiguration<T> classes in this assembly are picked up automatically.
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── DB Sequences ───────────────────────────────────────────────────
        // slug_suffix_seq: used to generate collision-free slug suffixes (e.g. "my-product-42")
        builder.HasSequence<long>("slug_suffix_seq").StartsAt(2).IncrementsBy(1);

        // variant_sku_seq: used to auto-generate SKUs (e.g. "SKU-000001")
        builder.HasSequence<long>("variant_sku_seq").StartsAt(1).IncrementsBy(1);
    }
}
