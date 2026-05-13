using Microsoft.AspNetCore.Localization;

namespace BookKeeping2.Localization;

/// <summary>
/// Resolves the request UI culture from the site language cookie only.
/// </summary>
public sealed class UiLanguageRequestCultureProvider : RequestCultureProvider
{
    /// <inheritdoc />
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        string? cookieValue = httpContext.Request.Cookies[UiLanguageOptions.CookieName];
        string uiCultureName = UiLanguageOptions.NormalizeUiCulture(cookieValue);
        ProviderCultureResult result = new(UiLanguageOptions.DefaultCultureName, uiCultureName);

        return Task.FromResult<ProviderCultureResult?>(result);
    }
}
