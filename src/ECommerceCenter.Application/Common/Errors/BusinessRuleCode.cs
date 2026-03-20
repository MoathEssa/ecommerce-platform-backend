namespace ECommerceCenter.Application.Common.Errors;

/// <summary>
/// Typed business-rule violation codes used across the application.
/// Each member maps to a unique <c>BUSINESS_RULE_{NAME}</c> error code.
/// </summary>
public enum BusinessRuleCode
{
    // ── Cart ────────────────────────────────────────────────────────────────
    ProductUnavailable,
    OutOfStock,
    InsufficientStock,
    EmptyCart,

    // ── Coupon ──────────────────────────────────────────────────────────────
    NoCouponApplied,
    CouponNotFound,
    CouponNotStarted,
    CouponExpired,
    CouponUsageLimitReached,
    CouponPerUserLimitReached,
    CouponAlreadyApplied,
    CouponNotApplicable,
    MinOrderAmountNotMet,

    // ── Catalog / Categories ────────────────────────────────────────────────
    MaxCategoryDepthExceeded,
    CategoryHasChildren,
    CategoryHasProducts,
    DuplicateSortOrderAmongSiblings,

    // ── Catalog / Products ──────────────────────────────────────────────────
    InvalidStatusTransition,
    NoActiveVariants,

    // ── Checkout ─────────────────────────────────────────────────────────────
    MissingIdempotencyKey,
    IdempotencyConflict,
    CheckoutItemsEmpty,
    VariantNotFound,
    ProductNotActive,
    CouponDoesNotApply,
    NegativeOrderTotal,
    StockReservationFailed,
    CurrencyMismatch,
    ExceedsMaxQuantity,

    // ── Supplier / Import 
    SupplierCategoryNotMapped,
    SupplierProductAlreadyImported,

    // ── Payments / Refunds ───────────────────────────────────────────────────
    OrderNotRefundable,
    RefundAmountExceedsRemaining,
    NoSucceededPayment,

    // ── Inventory Admin ─────────────────────────────────────────────────────
    InventoryBelowZero,
    AdjustmentDeltaZero,

    // ── Coupon Admin ────────────────────────────────────────────────────────
    CouponCodeExists,
    CouponInvalidPercentage,
    InvalidCouponDates
}
