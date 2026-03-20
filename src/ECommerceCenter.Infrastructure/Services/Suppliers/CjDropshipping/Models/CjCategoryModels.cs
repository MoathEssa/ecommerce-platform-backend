namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;

/// <summary>Root response for GET v1/product/getCategory</summary>
internal sealed record CjCategoryListResponse(
    int Code,
    bool Result,
    string Message,
    List<CjFirstLevelCategory>? Data,
    string RequestId);

/// <summary>Top-level CJ category group. Carries a UUID ID.</summary>
internal sealed record CjFirstLevelCategory(
    string CategoryFirstId,
    string CategoryFirstName,
    List<CjSecondLevelCategory> CategoryFirstList);

/// <summary>Second-level CJ category group. Carries a UUID ID.</summary>
internal sealed record CjSecondLevelCategory(
    string CategorySecondId,
    string CategorySecondName,
    List<CjThirdLevelCategory> CategorySecondList);

/// <summary>
/// Third / leaf CJ category — carries a UUID <c>CategoryId</c> used when
/// querying CJ products by category or importing into the store.
/// </summary>
internal sealed record CjThirdLevelCategory(
    string CategoryId,
    string CategoryName);
