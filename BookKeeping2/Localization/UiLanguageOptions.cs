namespace BookKeeping2.Localization;

/// <summary>
/// Defines supported interface language constants for the site UI.
/// </summary>
public static class UiLanguageOptions
{
    /// <summary>
    /// The first-party cookie name that stores the selected UI language.
    /// </summary>
    public const string CookieName = "bookkeeping.ui.language";

    /// <summary>
    /// The fixed formatting culture used for dates, numbers, and money.
    /// </summary>
    public const string DefaultCultureName = "zh-TW";

    /// <summary>
    /// The default UI culture used when no valid language cookie exists.
    /// </summary>
    public const string DefaultUiCultureName = "zh-TW";

    /// <summary>
    /// The English UI culture name.
    /// </summary>
    public const string EnglishUiCultureName = "en";

    /// <summary>
    /// The language preference cookie lifetime.
    /// </summary>
    public static readonly TimeSpan CookieLifetime = TimeSpan.FromDays(365);

    /// <summary>
    /// Gets all supported UI language codes.
    /// </summary>
    public static readonly IReadOnlySet<string> SupportedUiCultures = new HashSet<string>(StringComparer.Ordinal)
    {
        DefaultUiCultureName,
        EnglishUiCultureName
    };

    /// <summary>
    /// Determines whether the supplied UI culture is supported.
    /// </summary>
    /// <param name="uiCultureName">The UI culture name to validate.</param>
    /// <returns><see langword="true" /> when the UI culture can be used.</returns>
    public static bool IsSupportedUiCulture(string? uiCultureName)
    {
        return uiCultureName is not null && SupportedUiCultures.Contains(uiCultureName);
    }

    /// <summary>
    /// Resolves unsupported values to the default UI culture.
    /// </summary>
    /// <param name="uiCultureName">The requested UI culture name.</param>
    /// <returns>A supported UI culture name.</returns>
    public static string NormalizeUiCulture(string? uiCultureName)
    {
        return IsSupportedUiCulture(uiCultureName) ? uiCultureName! : DefaultUiCultureName;
    }

    /// <summary>
    /// Gets the HTML language attribute value for a UI culture.
    /// </summary>
    /// <param name="uiCultureName">The resolved UI culture name.</param>
    /// <returns>The HTML language value for the UI culture.</returns>
    public static string GetHtmlLang(string? uiCultureName)
    {
        return NormalizeUiCulture(uiCultureName) == EnglishUiCultureName ? "en" : "zh-Hant-TW";
    }
}
