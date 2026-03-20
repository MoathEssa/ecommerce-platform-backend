using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Catalog.Products.Commands.ChangeProductStatus;

public record ChangeProductStatusCommand(int Id, int Status) : IRequest<Result>;
