using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.BulkChangeProductStatus;

public record BulkChangeProductStatusCommand(List<int> Ids, int Status) : IRequest<Result<int>>;
