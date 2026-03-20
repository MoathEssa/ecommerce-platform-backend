using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string? Slug,
    string? Description,
    string? ImageUrl,
    int? ParentId,
    int SortOrder,
    bool IsActive,
    string? ExternalCategoryId = null,
    int? Supplier = null) : IRequest<Result<AdminCategoryDto>>;
