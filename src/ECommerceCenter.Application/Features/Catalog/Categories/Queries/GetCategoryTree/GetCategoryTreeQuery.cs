using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Queries.GetCategoryTree;

public record GetCategoryTreeQuery(bool Flat = false) : IRequest<Result<List<CategoryTreeDto>>>;
