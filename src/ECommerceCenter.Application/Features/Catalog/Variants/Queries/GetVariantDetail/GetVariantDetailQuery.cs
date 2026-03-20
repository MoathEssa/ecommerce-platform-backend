using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Variants.Queries.GetVariantDetail;

public record GetVariantDetailQuery(string ProductSlug, int VariantId)
    : IRequest<Result<VariantDetailDto>>;
