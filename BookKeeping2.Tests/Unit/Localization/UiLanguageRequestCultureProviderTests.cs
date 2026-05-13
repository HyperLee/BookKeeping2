using BookKeeping2.Localization;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BookKeeping2.Tests.Unit.Localization;

public sealed class UiLanguageRequestCultureProviderTests
{
    [Theory]
    [InlineData("en", "en")]
    [InlineData("zh-TW", "zh-TW")]
    public async Task DetermineProviderCultureResult_accepts_allow_listed_cookie_values(string cookieValue, string expectedUiCulture)
    {
        UiLanguageRequestCultureProvider provider = new();
        DefaultHttpContext context = new();
        context.Request.Headers.Cookie = $"{UiLanguageOptions.CookieName}={cookieValue}";

        var result = await provider.DetermineProviderCultureResult(context);

        Assert.NotNull(result);
        Assert.Equal(UiLanguageOptions.DefaultCultureName, result.Cultures.Single().Value);
        Assert.Equal(expectedUiCulture, result.UICultures.Single().Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fr")]
    [InlineData("en-US")]
    [InlineData("<script>")]
    public async Task DetermineProviderCultureResult_falls_back_to_default_for_missing_or_invalid_values(string? cookieValue)
    {
        UiLanguageRequestCultureProvider provider = new();
        DefaultHttpContext context = new();
        if (cookieValue is not null)
        {
            context.Request.Headers.Cookie = $"{UiLanguageOptions.CookieName}={cookieValue}";
        }

        var result = await provider.DetermineProviderCultureResult(context);

        Assert.NotNull(result);
        Assert.Equal(UiLanguageOptions.DefaultCultureName, result.Cultures.Single().Value);
        Assert.Equal(UiLanguageOptions.DefaultUiCultureName, result.UICultures.Single().Value);
    }

    [Fact]
    public async Task DetermineProviderCultureResult_ignores_accept_language_header()
    {
        UiLanguageRequestCultureProvider provider = new();
        DefaultHttpContext context = new();
        context.Request.Headers.AcceptLanguage = "en-US,en;q=0.9";

        var result = await provider.DetermineProviderCultureResult(context);

        Assert.NotNull(result);
        Assert.Equal(UiLanguageOptions.DefaultCultureName, result.Cultures.Single().Value);
        Assert.Equal(UiLanguageOptions.DefaultUiCultureName, result.UICultures.Single().Value);
    }

    [Fact]
    public void Supported_languages_define_cookie_lifetime_and_html_lang_mapping()
    {
        Assert.Equal("bookkeeping.ui.language", UiLanguageOptions.CookieName);
        Assert.Equal(TimeSpan.FromDays(365), UiLanguageOptions.CookieLifetime);
        Assert.Equal("zh-Hant-TW", UiLanguageOptions.GetHtmlLang("zh-TW"));
        Assert.Equal("en", UiLanguageOptions.GetHtmlLang("en"));
        Assert.Equal("zh-Hant-TW", UiLanguageOptions.GetHtmlLang("unsupported"));
    }
}
