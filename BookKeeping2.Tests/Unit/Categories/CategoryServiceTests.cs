using BookKeeping2.Data;
using BookKeeping2.Data.SeedData;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.Services.Categories;
using BookKeeping2.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BookKeeping2.Tests.Unit.Categories;

public sealed class CategoryServiceTests
{
    [Fact]
    public async Task CreateAsync_rejects_duplicate_normalized_name_within_same_type()
    {
        await using SqliteTestDatabase database = new();
        await using var context = new AppDbContext(database.CreateOptions<AppDbContext>());
        await context.Database.EnsureCreatedAsync();
        var service = new CategoryService(context);

        CategoryResult first = await service.CreateAsync(" 寵物 ", TransactionType.Expense);
        CategoryResult duplicate = await service.CreateAsync("寵物", TransactionType.Expense);

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Contains(nameof(Category.Name), duplicate.Errors.Keys);
    }

    [Fact]
    public void DefaultSeedData_contains_required_income_and_expense_categories()
    {
        Assert.Contains("餐飲", DefaultSeedData.ExpenseCategoryNames);
        Assert.Contains("薪資", DefaultSeedData.IncomeCategoryNames);
    }
}
