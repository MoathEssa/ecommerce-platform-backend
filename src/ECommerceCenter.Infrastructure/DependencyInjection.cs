using System.Text;
using ECommerceCenter.Application.Abstractions.Identity;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Auth;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Cart;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Catalog;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Inventory;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Orders;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Payments;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Abstractions.Services;
using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.Settings;
using ECommerceCenter.Domain.Entities.Auth;
using ECommerceCenter.Infrastructure.Data;
using ECommerceCenter.Infrastructure.Data.Repositories;
using ECommerceCenter.Infrastructure.Data.Repositories.Auth;
using ECommerceCenter.Infrastructure.Data.Repositories.Cart;
using ECommerceCenter.Infrastructure.Data.Repositories.Catalog;
using ECommerceCenter.Infrastructure.Data.Repositories.Coupons;
using ECommerceCenter.Infrastructure.Data.Repositories.Inventory;
using ECommerceCenter.Infrastructure.Data.Repositories.Orders;
using ECommerceCenter.Infrastructure.Data.Repositories.Payments;
using ECommerceCenter.Infrastructure.Data.Repositories.Reliability;
using ECommerceCenter.Infrastructure.Identity;
using ECommerceCenter.Infrastructure.Services;
using ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping;
using ECommerceCenter.Infrastructure.Suppliers;
using ECommerceCenter.Infrastructure.Workers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core ────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ── SMTP / Email ───────────────────────────────────────────────────
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
        services.AddTransient<IEmailService, SmtpEmailService>();

        // ── ASP.NET Identity ───────────────────────────────────────────────
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // Token lifetime for password-reset / email-confirm tokens
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromHours(24));

        // ── JWT Bearer ─────────────────────────────────────────────────────
        var jwtSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);

        var secretKey = jwtSection.Get<JwtSettings>()!.SecretKey;

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // No grace period — strict expiry
                };
            });

        // ── Unit of Work ───────────────────────────────────────────────────
        services.AddScoped<IEfUnitOfWork, EfUnitOfWork>();

        // ── Generic Repository ─────────────────────────────────────────────
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // ── Auth Repositories ──────────────────────────────────────────────
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();

        // ── Catalog Repositories ───────────────────────────────────────────
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();

        // ── Store Settings ─────────────────────────────────────────────────────
        services.Configure<StoreSettings>(configuration.GetSection(StoreSettings.SectionName));

        // ── Image Storage (Azure Blob) ─────────────────────────────────────────
        services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
        services.AddHttpClient();
        services.AddScoped<IImageStorageService, AzureBlobImageStorageService>();

        // ── Inventory Repositories ─────────────────────────────────────────
        services.AddScoped<IInventoryItemRepository, InventoryItemRepository>();


        // ── Order Repositories ─────────────────────────────────────────────
        services.AddScoped<IOrderRepository, OrderRepository>();

        // ── Payment Repositories ───────────────────────────────────────────
        services.AddScoped<IPaymentAttemptRepository, PaymentAttemptRepository>();
        services.AddScoped<IPaymentProviderEventRepository, PaymentProviderEventRepository>();

        // ── Reliability Repositories ───────────────────────────────────────
        services.AddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

        // ── Cart Repositories ──────────────────────────────────────────────
        services.AddScoped<ICartRepository, CartRepository>();

        // ── Coupon Evaluator ───────────────────────────────────────────────
        services.AddScoped<ICouponEvaluator, CouponEvaluator>();

        // ── Coupon Repositories ────────────────────────────────────────────
        services.AddScoped<ICouponRepository, CouponRepository>();

        // ── Refund Repository ──────────────────────────────────────────────
        services.AddScoped<IRefundRepository, RefundRepository>();

        // ── Audit Log Repository ───────────────────────────────────────────
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // ── Inventory Adjustment Repository ────────────────────────────────
        services.AddScoped<IInventoryAdjustmentRepository, InventoryAdjustmentRepository>();

        // ── Coupon Read Service ────────────────────────────────────────────

        // ── Stripe / Payment ───────────────────────────────────────────────
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.AddScoped<IStripePaymentService, StripePaymentService>();

        // ── Dashboard ──────────────────────────────────────────────────────
        services.AddScoped<IDashboardService, DashboardService>();

        // ── MediatR handlers in Infrastructure (e.g. Stripe webhook) ──────────
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // ── Identity Services ──────────────────────────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFirebaseTokenVerifier, FirebaseTokenVerifier>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHttpContextAccessor();

        // ── CJDropshipping HttpClient ──────────────────────────────────────
        services.Configure<CjDropshippingSettings>(configuration.GetSection(CjDropshippingSettings.SectionName));
        services.AddHttpClient("CjDropshipping", client =>
        {
            client.BaseAddress = new Uri("https://developers.cjdropshipping.com/api2.0/");
        });

        // ── Supplier Auth Services ─────────────────────────────────────────
        services.AddScoped<ISupplierAuthService, CjDropshippingAuthService>();
        services.AddScoped<SupplierAuthServiceFactory>();

        // ── CJ Product / Category Services ────────────────────────────────
        services.AddScoped<ICjAccessTokenProvider, CjAccessTokenProvider>();
        services.AddScoped<ICjProductService, CjDropshippingProductService>();
        services.AddScoped<ICjFreightService, CjDropshippingFreightService>();

        // ── Background Workers ─────────────────────────────────────────────
        // services.AddHostedService<OutboxPublisherWorker>(); // disabled
        // services.AddHostedService<IdempotencyCleanupWorker>();
        // services.AddHostedService<CartCleanupWorker>();

        return services;
    }
}
