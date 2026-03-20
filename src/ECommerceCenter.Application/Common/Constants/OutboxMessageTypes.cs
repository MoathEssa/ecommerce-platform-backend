namespace ECommerceCenter.Application.Common.Constants;

/// <summary>
/// Centralised string constants for <c>OutboxMessage.Type</c> values.
/// Each constant must match exactly what the outbox dispatcher uses to fan-out the event.
/// </summary>
public static class OutboxMessageTypes
{
    // ── Checkout ─────────────────────────────────────────────────────────────
    public const string OrderPlaced           = "OrderPlaced";
    public const string PaymentIntentCreated  = "PaymentIntentCreated";

    // ── Webhook (payment lifecycle) ───────────────────────────────────────────
    public const string PaymentSucceeded          = "PaymentSucceeded";
    public const string OrderPaid                 = "OrderPaid";
    public const string InventoryCommitted        = "InventoryCommitted";
    public const string PaymentFailed             = "PaymentFailed";
    public const string PaymentAfterCancellation  = "PaymentAfterCancellation";

    // ── Refunds ────────────────────────────────────────────────────────────────
    public const string RefundIssued    = "RefundIssued";
    public const string RefundCompleted = "RefundCompleted";
    public const string RefundFailed    = "RefundFailed";

    // ── Order lifecycle ────────────────────────────────────────────────────────
    public const string OrderCancelled = "OrderCancelled";
}
