using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.ProductCategories.Commands.SetProductCategories;

public record SetProductCategoriesCommand(
    int ProductId,
    int? CategoryId) : IRequest<Result>;
