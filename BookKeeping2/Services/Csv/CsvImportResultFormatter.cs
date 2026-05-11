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
    /// Formats one row-level error.
    /// </summary>
    /// <param name="error">The import error.</param>
    /// <returns>The formatted error text.</returns>
    public static string FormatError(CsvImportErrorDetail error)
    {
        return error.RowNumber > 0
            ? $"第 {error.RowNumber} 行匯入失敗：{error.Reason}"
            : $"匯入失敗：{error.Reason}";
    }
}
