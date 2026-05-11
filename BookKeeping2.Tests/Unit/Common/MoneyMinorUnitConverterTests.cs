using BookKeeping2.Services.Common;
using Xunit;

namespace BookKeeping2.Tests.Unit.Common;

public sealed class MoneyMinorUnitConverterTests
{
    [Theory]
    [InlineData("1", 100)]
    [InlineData("150.25", 15025)]
    [InlineData("999999999.99", 99999999999)]
    public void ToMinorUnits_converts_valid_twd_amounts_exactly(string amountText, long expectedMinorUnits)
    {
        decimal amount = decimal.Parse(amountText);

        long actual = MoneyMinorUnitConverter.ToMinorUnits(amount);

        Assert.Equal(expectedMinorUnits, actual);
        Assert.Equal(amount, MoneyMinorUnitConverter.FromMinorUnits(actual));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.001")]
    [InlineData("1000000000")]
    public void ToMinorUnits_rejects_invalid_transaction_amounts(string amountText)
    {
        decimal amount = decimal.Parse(amountText);

        Assert.ThrowsAny<Exception>(() => MoneyMinorUnitConverter.ToMinorUnits(amount));
    }
}
