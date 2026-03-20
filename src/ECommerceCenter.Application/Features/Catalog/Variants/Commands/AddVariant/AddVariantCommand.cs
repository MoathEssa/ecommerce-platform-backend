using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Commands.AddVariant;

public record AddVariantCommand(
    int ProductId,
    Dictionary<string, string> Options,
    decimal BasePrice,
    string CurrencyCode,
    bool IsActive,
    int InitialStock) : IRequest<Result<AdminVariantCreatedDto>>;
