using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.UpdateVariant;

public record UpdateVariantCommand(
    int ProductId,
    int VariantId,
    string Sku,
    Dictionary<string, string> Options,
    decimal BasePrice,
    string CurrencyCode,
    bool IsActive) : IRequest<Result<AdminProductVariantDto>>;
