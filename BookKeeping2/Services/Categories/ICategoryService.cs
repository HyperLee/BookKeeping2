using BookKeeping2.Models.Common;
using BookKeeping2.ViewModels.Categories;

namespace BookKeeping2.Services.Categories;

/// <summary>
/// Manages transaction categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Lists categories.
    /// </summary>
    /// <param name="includeArchived">Whether archived rows should be included.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Category rows.</returns>
    Task<IReadOnlyList<CategoryListItemViewModel>> ListAsync(bool includeArchived = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a category.
    /// </summary>
    /// <param name="name">The category name.</param>
    /// <param name="type">The category type.</param>
    /// <param name="iconKey">The icon key.</param>
    /// <param name="displayOrder">The display order.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<CategoryResult> CreateAsync(string name, TransactionType type, string iconKey = "tag", int displayOrder = 0, CancellationToken cancellationToken = default);
}
