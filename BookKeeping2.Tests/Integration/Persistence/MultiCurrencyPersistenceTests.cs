using BookKeeping2.Data;
using BookKeeping2.Models.Accounts;
using BookKeeping2.Models.Budgets;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace BookKeeping2.Tests.Integration.Persistence;

public sealed class MultiCurrencyPersistenceTests
{
    [Fact]
    public void Ef_model_defines_required_currency_columns_with_twd_defaults()
    {
        using AppDbContext context = CreateContext();

        AssertCurrencyProperty(context.Model, typeof(Transaction), "Transactions");
        AssertCurrencyProperty(context.Model, typeof(Budget), "Budgets");
        AssertCurrencyProperty(context.Model, typeof(Account), "Accounts");
    }

    [Fact]
    public void Ef_model_defines_currency_indexes_for_transactions_and_budgets()
    {
        using AppDbContext context = CreateContext();
        IEntityType transactionType = context.Model.FindEntityType(typeof(Transaction))!;
        IEntityType budgetType = context.Model.FindEntityType(typeof(Budget))!;

        Assert.Contains(transactionType.GetIndexes(), index => HasProperties(index, "IsDeleted", "Currency", "TransactionDate"));
        Assert.Contains(transactionType.GetIndexes(), index => HasProperties(index, "IsDeleted", "Currency", "CategoryId", "TransactionDate"));
        Assert.Contains(transactionType.GetIndexes(), index => HasProperties(index, "IsDeleted", "Currency", "AccountId", "TransactionDate"));
        Assert.Contains(budgetType.GetIndexes(), index => index.IsUnique && HasProperties(index, "CategoryId", "BudgetMonth", "Currency"));
    }

    [Fact]
    public void Migration_history_contains_multi_currency_migration()
    {
        using AppDbContext context = CreateContext();

        Assert.Contains("20260514000000_AddMultiCurrencyBookkeeping", context.Database.GetMigrations());
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        return new AppDbContext(options);
    }

    private static void AssertCurrencyProperty(IModel model, Type entityType, string tableName)
    {
        IProperty currency = model.FindEntityType(entityType)!.FindProperty("Currency")!;

        Assert.NotNull(currency);
        Assert.Equal("TEXT", currency.GetColumnType());
        Assert.Equal(3, currency.GetMaxLength());
        Assert.False(currency.IsNullable);
        Assert.Equal("TWD", currency.GetDefaultValue());
        Assert.Equal(tableName, currency.DeclaringType.GetTableName());
    }

    private static bool HasProperties(IIndex index, params string[] propertyNames)
    {
        return index.Properties.Select(property => property.Name).SequenceEqual(propertyNames);
    }
}
