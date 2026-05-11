namespace BookKeeping2.Services.Csv;

/// <summary>
/// Represents generated CSV content and metadata.
/// </summary>
/// <param name="Content">The UTF-8 CSV bytes.</param>
/// <param name="RowCount">The number of exported transaction rows.</param>
/// <param name="FileName">The suggested download file name.</param>
public sealed record CsvExportResult(byte[] Content, int RowCount, string FileName);
