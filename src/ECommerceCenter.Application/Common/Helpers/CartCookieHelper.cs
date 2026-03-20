using Microsoft.AspNetCore.Http;

namespace ECommerceCenter.Application.Common.Helpers;

/// <summary>
/// Encapsulates all guest cart session-cookie operations.
/// Keeping this out of the controllers prevents code duplication across
/// <c>CartController</c> and <c>AuthController</c>.
/// </summary>
public static class CartCookieHelper
{
    public const string CookieName = "cartSessionId";

    /// <summary>Returns the current session id from the request, or <c>null</c> if none is set.</summary>
    public static string? GetSessionId(IRequestCookieCollection cookies)
        => cookies[CookieName];

    /// <summary>
    /// Returns the existing session id, or generates a new one and writes it to the response.
    /// Call this on guest cart mutations so the cookie is always present when the response is sent.
    /// </summary>
    public static string EnsureSessionId(
        IRequestCookieCollection requestCookies,
        IResponseCookies responseCookies,
        bool isSecure = true)
    {
        var existing = requestCookies[CookieName];
        if (!string.IsNullOrWhiteSpace(existing))
            return existing;

        var newId = Guid.NewGuid().ToString();
        responseCookies.Append(CookieName, newId, BuildOptions(isSecure, DateTimeOffset.UtcNow.AddDays(30)));
        return newId;
    }

    /// <summary>Removes the cart session cookie (called on login / register to prevent cart leakage).</summary>
    public static void ClearSessionCookie(IResponseCookies responseCookies, bool isSecure = true)
        => responseCookies.Delete(CookieName, BuildOptions(isSecure));

    private static CookieOptions BuildOptions(bool isSecure, DateTimeOffset? expires = null) => new()
    {
        HttpOnly = true,
        Secure = isSecure,
        SameSite = isSecure ? SameSiteMode.Strict : SameSiteMode.Lax,
        Expires = expires
    };
}
