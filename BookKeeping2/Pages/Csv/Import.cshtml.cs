using BookKeeping2.Services.Csv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BookKeeping2.Pages.Csv;

/// <summary>
/// Handles transaction CSV import uploads.
/// </summary>
public sealed class ImportModel : PageModel
{
    private readonly ICsvImportService csvImportService;
    private readonly CsvTransferImportService csvTransferImportService;

    /// <summary>
    /// Initializes a CSV import page.
    /// </summary>
    /// <param name="csvImportService">The CSV import service.</param>
    /// <param name="csvTransferImportService">The transfer CSV import service.</param>
    public ImportModel(ICsvImportService csvImportService, CsvTransferImportService csvTransferImportService)
    {
        this.csvImportService = csvImportService;
        this.csvTransferImportService = csvTransferImportService;
    }

    /// <summary>
    /// Gets or sets the uploaded CSV file.
    /// </summary>
    [BindProperty]
    public IFormFile? Upload { get; set; }

    /// <summary>
    /// Gets or sets the uploaded transfer CSV file.
    /// </summary>
    [BindProperty]
    public IFormFile? TransferUpload { get; set; }

    /// <summary>
    /// Gets the latest import result.
    /// </summary>
    public CsvImportResult? Result { get; private set; }

    /// <summary>
    /// Gets the latest transfer import result.
    /// </summary>
    public CsvTransferImportResult? TransferResult { get; private set; }

    /// <summary>
    /// Gets the current seven-column CSV header contract.
    /// </summary>
    public string CurrencyCsvHeader => CsvImportParser.SevenColumnHeaderText;

    /// <summary>
    /// Gets the legacy six-column CSV header accepted as TWD.
    /// </summary>
    public string LegacyCsvHeader => CsvImportParser.LegacyHeaderText;

    /// <summary>
    /// Gets the transfer CSV header contract.
    /// </summary>
    public string TransferCsvHeader => CsvTransferImportParser.HeaderText;

    /// <summary>
    /// Displays the upload form.
    /// </summary>
    public void OnGet()
    {
    }

    /// <summary>
    /// Imports the uploaded CSV file.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (Upload is null || Upload.Length == 0)
        {
            ModelState.AddModelError(nameof(Upload), "請選擇 CSV 檔案。");
            return Page();
        }

        await using var memoryStream = new MemoryStream();
        await Upload.CopyToAsync(memoryStream);
        Result = await csvImportService.ImportAsync(new CsvImportCommand(Upload.FileName, memoryStream.ToArray()));
        return Page();
    }

    /// <summary>
    /// Imports the uploaded transfer CSV file.
    /// </summary>
    /// <returns>The post result.</returns>
    public async Task<IActionResult> OnPostTransferAsync()
    {
        if (TransferUpload is null || TransferUpload.Length == 0)
        {
            ModelState.AddModelError(nameof(TransferUpload), "請選擇轉帳 CSV 檔案。");
            return Page();
        }

        await using var memoryStream = new MemoryStream();
        await TransferUpload.CopyToAsync(memoryStream);
        TransferResult = await csvTransferImportService.ImportAsync(new CsvImportCommand(TransferUpload.FileName, memoryStream.ToArray()));
        return Page();
    }
}
