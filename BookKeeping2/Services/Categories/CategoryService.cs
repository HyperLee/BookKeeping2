using BookKeeping2.Data;
using BookKeeping2.Data.SeedData;
using BookKeeping2.Models.Categories;
using BookKeeping2.Models.Common;
using BookKeeping2.ViewModels.Categories;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping2.Services.Categories;

/// <summary>
/// EF Core implementation of category management.
/// </summary>
public sealed class CategoryService : ICategoryService
{
    private readonly AppDbContext dbContext;

    /// <summary>
    /// Initializes a category service.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public CategoryService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryListItemViewModel>> ListAsync(bool includeArchived = true, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = dbContext.Categories.AsNoTracking();
        if (!includeArchived)
        {
            query = query.Where(category => !category.IsArchived);
        }

        return await query
            .OrderBy(category => category.Type)
            .ThenBy(category => category.DisplayOrder)
            .ThenBy(category => category.Name)
            .Select(category => new CategoryListItemViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type,
                IsDefault = category.IsDefault,
                IsArchived = category.IsArchived
            })
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryResult> CreateAsync(string name, TransactionType type, string iconKey = "tag", int displayOrder = 0, CancellationToken cancellationToken = default)
    {
        var result = new CategoryResult();
        string trimmed = name.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            result.AddError(nameof(Category.Name), "請輸入分類名稱。");
            return result;
        }

        string normalizedName = DefaultSeedData.NormalizeName(trimmed);
        bool duplicate = await dbContext.Categories.AnyAsync(
            category => category.Type == type && category.NormalizedName == normalizedName,
            cancellationToken);
        if (duplicate)
        {
            result.AddError(nameof(Category.Name), "同類型已有相同名稱的分類。");
            return result;
        }

        DateTimeOffset nowUtc = DateTimeOffset.UtcNow;
        dbContext.Categories.Add(new Category
        {
            Name = trimmed,
            NormalizedName = normalizedName,
            Type = type,
            IconKey = string.IsNullOrWhiteSpace(iconKey) ? "tag" : iconKey.Trim(),
            DisplayOrder = displayOrder,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
