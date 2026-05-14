using BookKeeping2.Models.Common;
using Xunit;

namespace BookKeeping2.Tests.Unit.Common;

public sealed class SupportedCurrencyTests
{
    [Theory]
    [InlineData("TWD", "TWD")]
    [InlineData(" twd ", "TWD")]
    [InlineData("usd", "USD")]
    [InlineData("JpY", "JPY")]
    [InlineData(" eur", "EUR")]
    [InlineData("GBP ", "GBP")]
    public void TryNormalize_accepts_supported_codes_case_insensitively_after_trimming(string input, string expectedCode)
    {
        bool normalized = SupportedCurrency.TryNormalize(input, out string? actualCode);

        Assert.True(normalized);
        Assert.Equal(expectedCode, actualCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("AUD")]
    [InlineData("NTD")]
    public void TryNormalize_rejects_blank_or_unsupported_codes(string? input)
    {
        bool normalized = SupportedCurrency.TryNormalize(input, out string? actualCode);

        Assert.False(normalized);
        Assert.Null(actualCode);
    }

    [Fact]
    public void Options_return_supported_display_names_in_contract_order()
    {
        IReadOnlyList<SupportedCurrencyOption> options = SupportedCurrency.Options;

        Assert.Collection(
            options,
            option => AssertOption(option, "TWD", "新台幣"),
            option => AssertOption(option, "USD", "美金"),
            option => AssertOption(option, "JPY", "日幣"),
            option => AssertOption(option, "EUR", "歐元"),
            option => AssertOption(option, "GBP", "英鎊"));
    }

    [Fact]
    public void Normalize_throws_for_unsupported_codes()
    {
        Assert.Throws<ArgumentException>(() => SupportedCurrency.Normalize("AUD"));
    }

    private static void AssertOption(SupportedCurrencyOption option, string expectedCode, string expectedDisplayName)
    {
        Assert.Equal(expectedCode, option.Code);
        Assert.Equal(expectedDisplayName, option.DisplayName);
    }
}
