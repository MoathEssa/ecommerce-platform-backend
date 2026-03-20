using ECommerceCenter.Application.Abstractions.Repositories.EfCore;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Coupons;
using ECommerceCenter.Application.Abstractions.Repositories.EfCore.Reliability;
using ECommerceCenter.Application.Common.ResultPattern;
using ECommerceCenter.Domain.Entities.Reliability;
using ECommerceCenter.Domain.Enums;
using MediatR;

namespace ECommerceCenter.Application.Features.Coupons.Commands.DeactivateCoupon;

public class DeactivateCouponCommandHandler(
    ICouponRepository couponRepository,
    IAuditLogRepository auditLogRepository,
    IEfUnitOfWork unitOfWork) : IRequestHandler<DeactivateCouponCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await couponRepository.GetByIdAsync(request.Id, cancellationToken);
        if (coupon is null)
            return Result.NotFound("Coupon", request.Id);

        coupon.IsActive = false;
        coupon.UpdatedAt = DateTime.UtcNow;
        couponRepository.Update(coupon);

        await auditLogRepository.AddAsync(new AuditLog
        {
            ActorId = request.ActorId,
            ActorType = ActorType.Admin,
            Action = "Coupon.Deactivate",
            EntityType = "Coupon",
            EntityId = coupon.Id,
            BeforeJson = $"{{\"isActive\":true}}",
            AfterJson = $"{{\"isActive\":false}}"
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
