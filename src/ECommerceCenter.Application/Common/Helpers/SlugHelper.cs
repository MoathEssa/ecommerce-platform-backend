using System.Text.RegularExpressions;

namespace ECommerceCenter.Application.Common.Helpers;

public static class SlugHelper
{
    private static readonly Regex NonAlphaNumeric = new(@"[^a-z0-9\s-]", RegexOptions.Compiled);
    private static readonly Regex MultipleWhitespaceOrHyphen = new(@"[\s-]+", RegexOptions.Compiled);

    /// <summary>
    /// Generates a URL-friendly slug from any string.
    /// "iPhone 15 Pro!" → "iphone-15-pro"
    /// </summary>
    public static string Generate(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        slug = NonAlphaNumeric.Replace(slug, "");
        slug = MultipleWhitespaceOrHyphen.Replace(slug, "-");
        return slug.Trim('-');
    }
}
