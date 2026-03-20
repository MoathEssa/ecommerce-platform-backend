using FluentValidation;

namespace ECommerceCenter.Application.Features.Payments.Commands.CreateRefund;

public class CreateRefundCommandValidator : AbstractValidator<CreateRefundCommand>
{
    public CreateRefundCommandValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(200).When(x => x.Reason is not null);
    }
}
