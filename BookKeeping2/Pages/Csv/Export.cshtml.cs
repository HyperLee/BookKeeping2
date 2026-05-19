using BookKeeping2.Models.Audit;
using BookKeeping2.Models.AccountTransfers;
using BookKeeping2.Models.Transactions;
using BookKeeping2.Services.Audit;
using BookKeeping2.Services.Csv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Csv;

/// <summary>
/// Handles transaction CSV export.
/// </summary>
public sealed class ExportModel : PageModel
{
    private readonly ICsvExportService csvExportService;
    private readonly CsvTransferExportService csvTransferExportService;
    private readonly IAuditService auditService;

    /// <summary>
    /// Initializes the CSV export page.
    /// </summary>
    /// <param name="csvExportService">The CSV export service.</param>
    /// <param name="csvTransferExportService">The transfer CSV export service.</param>
    /// <param name="auditService">The audit service.</param>
    public ExportModel(ICsvExportService csvExportService, CsvTransferExportService csvTransferExportService, IAuditService auditService)
    {
        this.csvExportService = csvExportService;
        this.csvTransferExportService = csvTransferExportService;
        this.auditService = auditService;
    }

    /// <summary>
    /// Gets or sets the optional inclusive start date.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the optional inclusive end date.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets the CSV header contract used by downloads.
    /// </summary>
    public string CurrencyCsvHeader => CsvExportService.HeaderText;

    /// <summary>
    /// Gets the transfer CSV header contract used by downloads.
    /// </summary>
    public string TransferCsvHeader => CsvTransferExportService.HeaderText;

    /// <summary>
    /// Displays the export form.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Downloads the CSV file.
    /// </summary>
    /// <returns>The file download response.</returns>
    public async Task<IActionResult> OnGetDownloadAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            ModelState.AddModelError(nameof(EndDate), "結束日期不可早於起始日期。");
            return Page();
        }

        CsvExportResult result = await csvExportService.ExportAsync(new CsvExportOptions
        {
            StartDate = StartDate,
            EndDate = EndDate
        });

        Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";
        await auditService.RecordAsync(
            AuditEventType.CsvExported,
            nameof(Transaction),
            null,
            $"匯出交易 CSV，筆數 {result.RowCount}",
            cancellationToken: HttpContext.RequestAborted);

        return File(result.Content, "text/csv; charset=utf-8", result.FileName);
    }

    /// <summary>
    /// Downloads the transfer CSV file.
    /// </summary>
    /// <returns>The file download response.</returns>
    public async Task<IActionResult> OnGetTransferDownloadAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            ModelState.AddModelError(nameof(EndDate), "結束日期不可早於起始日期。");
            return Page();
        }

        CsvExportResult result = await csvTransferExportService.ExportAsync(new CsvExportOptions
        {
            StartDate = StartDate,
            EndDate = EndDate
        });

        Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Expires = "0";
        await auditService.RecordAsync(
            AuditEventType.TransferCsvExported,
            nameof(AccountTransfer),
            null,
            $"匯出轉帳 CSV，筆數 {result.RowCount}",
            cancellationToken: HttpContext.RequestAborted);

        return File(result.Content, "text/csv; charset=utf-8", result.FileName);
    }
}
