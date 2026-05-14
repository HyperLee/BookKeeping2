using BookKeeping2.Models.Common;
using BookKeeping2.Tests.TestSupport;
using Xunit;

namespace BookKeeping2.Tests.Unit.TestSupport;

public sealed class TestDataBuilderTests
{
    [Fact]
    public void CreateAccount_allows_currency_to_be_specified()
    {
        var account = TestDataBuilder.CreateAccount(currency: TestDataBuilder.UsdCurrency);

        Assert.Equal(TestDataBuilder.UsdCurrency, account.Currency);
    }

    [Fact]
    public void CreateTransaction_allows_currency_to_be_specified()
    {
        var category = TestDataBuilder.CreateCategory(TransactionType.Expense);
        var account = TestDataBuilder.CreateAccount(currency: TestDataBuilder.EurCurrency);
        var transaction = TestDataBuilder.CreateTransaction(category, account, currency: TestDataBuilder.EurCurrency);

        Assert.Equal(TestDataBuilder.EurCurrency, transaction.Currency);
        Assert.Same(category, transaction.Category);
        Assert.Same(account, transaction.Account);
    }

    [Fact]
    public void CreateBudget_allows_currency_to_be_specified()
    {
        var category = TestDataBuilder.CreateCategory(TransactionType.Expense);
        var budget = TestDataBuilder.CreateBudget(category, currency: TestDataBuilder.GbpCurrency);

        Assert.Equal(TestDataBuilder.GbpCurrency, budget.Currency);
        Assert.Same(category, budget.Category);
    }
}
