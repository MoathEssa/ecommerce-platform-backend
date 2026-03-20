using ECommerceCenter.Application.Abstractions.Services.Suppliers;
using ECommerceCenter.Application.Common.ResultPattern;
using MediatR;

namespace ECommerceCenter.Application.Features.Checkout.Queries.CalculateFreight;

public record FreightItemBody(int VariantId, int Quantity);

public record CalculateFreightRequestBody(
    IReadOnlyList<FreightItemBody> Items,
    string EndCountryCode,
    string? Zip);

public record CalculateFreightQuery(
    IReadOnlyList<FreightItemBody> Items,
    string EndCountryCode,
    string? Zip) : IRequest<Result<List<FreightOptionDto>>>;
