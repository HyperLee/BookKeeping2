using BookKeeping2.Services.Categories;
using BookKeeping2.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Categories;

/// <summary>
/// Handles category management.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly ICategoryService categoryService;

    /// <summary>
    /// Initializes a category management page.
    /// </summary>
    /// <param name="categoryService">The category service.</param>
    public IndexModel(ICategoryService categoryService)
    {
        this.categoryService = categoryService;
    }

    /// <summary>
    /// Gets or sets the category input.
    /// </summary>
    [BindProperty]
    public CategoryInputModel Input { get; set; } = new();

    /// <summary>
    /// Gets category rows.
    /// </summary>
    public IReadOnlyList<CategoryListItemViewModel> Categories { get; private set; } = [];

    /// <summary>
    /// Displays categories.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        Categories = await categoryService.ListAsync();
    }

    /// <summary>
    /// Creates a category.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Categories = await categoryService.ListAsync();
            return Page();
        }

        CategoryResult result = await categoryService.CreateAsync(Input.Name, Input.Type, Input.IconKey, Input.DisplayOrder);
        if (!result.Succeeded)
        {
            foreach ((string field, string[] messages) in result.Errors)
            {
                foreach (string message in messages)
                {
                    ModelState.AddModelError($"Input.{field}", message);
                }
            }

            Categories = await categoryService.ListAsync();
            return Page();
        }

        TempData["StatusMessage"] = "分類已新增。";
        return RedirectToPage();
    }
}
