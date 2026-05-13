using System.Globalization;
using BookKeeping2.Localization;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// Formats CSV import results for UI and audit summaries.
/// </summary>
public static class CsvImportResultFormatter
{
    /// <summary>
    /// Formats a concise result summary.
    /// </summary>
    /// <param name="result">The import result.</param>
    /// <returns>The Traditional Chinese summary.</returns>
    public static string Format(CsvImportResult result)
    {
        string summary = $"成功 {result.SucceededRows} 筆，失敗 {result.FailedRows} 筆";
        if (result.CreatedCategories.Count > 0)
        {
            summary += $"，自動新增分類：{string.Join("、", result.CreatedCategories)}";
        }

        return summary;
    }

    /// <summary>
    /// Formats a concise result summary for the current UI culture.
    /// </summary>
    /// <param name="result">The import result.</param>
    /// <returns>The display summary.</returns>
    public static string FormatForCurrentUi(CsvImportResult result)
    {
        if (!string.Equals(CultureInfo.CurrentUICulture.Name, UiLanguageOptions.EnglishUiCultureName, StringComparison.OrdinalIgnoreCase))
        {
            return Format(result);
        }

        string summary = $"Succeeded {result.SucceededRows} rows, failed {result.FailedRows} rows";
        if (result.CreatedCategories.Count > 0)
        {
            summary += $", automatically created categories: {string.Join(", ", result.CreatedCategories)}";
        }

        return summary;
    }

    /// <summary>
    /// Formats one row-level error.
    /// </summary>
    /// <param name="error">The import error.</param>
    /// <returns>The formatted error text.</returns>
    public static string FormatError(CsvImportErrorDetail error)
    {
        string message = error.RowNumber > 0
            ? $"第 {error.RowNumber} 行匯入失敗：{error.Reason}"
            : $"匯入失敗：{error.Reason}";
        if (!string.IsNullOrWhiteSpace(error.RawValuePreview))
        {
            message += $"（原始值：{error.RawValuePreview}）";
        }

        return message;
    }
}
