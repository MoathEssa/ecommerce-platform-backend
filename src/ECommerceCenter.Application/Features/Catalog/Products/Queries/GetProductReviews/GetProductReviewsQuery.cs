using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Queries.GetProductReviews;

public record GetProductReviewsQuery(
    string Slug,
    int Page = 1,
    int PageSize = 10)
    : IRequest<Result<ReviewListDto>>;
