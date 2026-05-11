using System.Globalization;
using BookKeeping2.Services.Reports;
using BookKeeping2.Services.Time;
using BookKeeping2.ViewModels.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Reports;

/// <summary>
/// Displays monthly bookkeeping reports.
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly IReportService reportService;
    private readonly ITaipeiDateService dateService;

    /// <summary>
    /// Initializes a new reports page model.
    /// </summary>
    /// <param name="reportService">The report service.</param>
    /// <param name="dateService">The Taipei date service.</param>
    public IndexModel(IReportService reportService, ITaipeiDateService dateService)
    {
        this.reportService = reportService;
        this.dateService = dateService;
    }

    /// <summary>
    /// Gets or sets the selected month query value in yyyy-MM format.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? Month { get; set; }

    /// <summary>
    /// Gets the report data.
    /// </summary>
    public MonthlyReportViewModel Report { get; private set; } = new();

    /// <summary>
    /// Handles the report request.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnGetAsync()
    {
        DateOnly selectedMonth = ResolveSelectedMonth();
        Month = selectedMonth.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        Report = await reportService.GetMonthlyReportAsync(selectedMonth.Year, selectedMonth.Month);
    }

    private DateOnly ResolveSelectedMonth()
    {
        if (string.IsNullOrWhiteSpace(Month))
        {
            DateOnly today = dateService.Today;
            return new DateOnly(today.Year, today.Month, 1);
        }

        if (DateOnly.TryParseExact(
            $"{Month}-01",
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly selectedMonth))
        {
            return new DateOnly(selectedMonth.Year, selectedMonth.Month, 1);
        }

        ModelState.AddModelError(nameof(Month), "月份格式無效。");
        DateOnly fallback = dateService.Today;
        return new DateOnly(fallback.Year, fallback.Month, 1);
    }
}
