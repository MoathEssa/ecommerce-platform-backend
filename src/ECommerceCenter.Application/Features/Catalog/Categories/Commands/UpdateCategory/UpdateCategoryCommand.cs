using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    int Id,
    string Name,
    string? Slug,
    string? Description,
    string? ImageUrl,
    int? ParentId,
    int SortOrder,
    bool IsActive) : IRequest<Result<AdminCategoryDto>>;
