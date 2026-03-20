using ECommerceCenter.Application.Abstractions.DTOs.Catalog;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Title,
    string? Slug,
    string? Description,
    string? Brand) : IRequest<Result<AdminProductCreatedDto>>;
