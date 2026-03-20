using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(int Id) : IRequest<Result>;
