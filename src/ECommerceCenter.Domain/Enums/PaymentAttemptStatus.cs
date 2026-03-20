namespace ECommerceCenter.Domain.Enums;

public enum PaymentAttemptStatus
{
    Created,
    RequiresAction,
    Succeeded,
    Failed,
    Canceled
}
