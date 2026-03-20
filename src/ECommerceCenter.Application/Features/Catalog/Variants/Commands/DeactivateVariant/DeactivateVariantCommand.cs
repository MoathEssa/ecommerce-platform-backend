using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.DeactivateVariant;

public record DeactivateVariantCommand(int ProductId, int VariantId) : IRequest<Result>;
