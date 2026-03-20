namespace ECommerceCenter.Domain.Enums;

/// <summary>Who triggered an action — used in OrderStatusHistory and AuditLog.</summary>
public enum ActorType
{
    Admin,
    User,
    System
}
