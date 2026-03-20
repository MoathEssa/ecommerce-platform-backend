namespace ECommerceCenter.Domain.Enums;

public enum DiscountType
{
    /// <summary>e.g. 10 means 10% off. Capped by MaxDiscountAmount if set.</summary>
    Percentage,
    /// <summary>e.g. 50 means 50 SAR off.</summary>
    FixedAmount
}
