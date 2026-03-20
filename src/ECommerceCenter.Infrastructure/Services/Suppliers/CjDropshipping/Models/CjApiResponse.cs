using System.Text.Json.Serialization;

namespace ECommerceCenter.Infrastructure.Services.Suppliers.CjDropshipping.Models;

/// <summary>Generic wrapper that CJ wraps every API response in.</summary>
internal sealed record CjApiResponse<T>(
    [property: JsonPropertyName("code")] int Code,
    [property: JsonPropertyName("result")] bool Result,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("requestId")] string RequestId);

/// <summary>The token payload returned by getAccessToken and refreshAccessToken.</summary>
internal sealed record CjTokenData(
    [property: JsonPropertyName("openId")] long? OpenId,
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("accessTokenExpiryDate")] DateTimeOffset AccessTokenExpiryDate,
    [property: JsonPropertyName("refreshToken")] string RefreshToken,
    [property: JsonPropertyName("refreshTokenExpiryDate")] DateTimeOffset RefreshTokenExpiryDate,
    [property: JsonPropertyName("createDate")] DateTimeOffset CreateDate);
