using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// Parses fixed six-column transaction CSV files.
/// </summary>
public sealed class CsvImportParser
{
    /// <summary>
    /// Maximum allowed upload size in bytes.
    /// </summary>
    public const int MaximumFileBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Maximum allowed valid data rows.
    /// </summary>
    public const int MaximumDataRows = 10_000;

    private static readonly string[] ExpectedHeaders = ["日期", "類型", "金額", "分類", "帳戶", "備註"];

    /// <summary>
    /// Parses the uploaded CSV into rows and structural errors.
    /// </summary>
    /// <param name="command">The import command.</param>
    /// <returns>The parse result.</returns>
    public CsvImportResult Parse(CsvImportCommand command)
    {
        var result = new CsvImportResult { FileName = SanitizeFileName(command.FileName) };
        if (command.Content.Length == 0)
        {
            result.AddError(0, "檔案", "空檔案");
            result.FailedRows = result.Errors.Count;
            result.Summary = CsvImportResultFormatter.Format(result);
            return result;
        }

        if (command.Content.Length > MaximumFileBytes)
        {
            result.AddError(0, "檔案", "檔案大小不可超過 5 MB");
            result.FailedRows = result.Errors.Count;
            result.Summary = CsvImportResultFormatter.Format(result);
            return result;
        }

        string csvText = Encoding.UTF8.GetString(command.Content);
        if (string.IsNullOrWhiteSpace(csvText))
        {
            result.AddError(0, "檔案", "空檔案");
            result.FailedRows = result.Errors.Count;
            result.Summary = CsvImportResultFormatter.Format(result);
            return result;
        }

        try
        {
            using var stringReader = new StringReader(csvText);
            using var csvReader = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                BadDataFound = null,
                MissingFieldFound = null
            });

            if (!csvReader.Read())
            {
                result.AddError(0, "檔案", "空檔案");
                return Complete(result);
            }

            string[] header = csvReader.Parser.Record ?? [];
            if (!ExpectedHeaders.SequenceEqual(header))
            {
                result.AddError(1, "標題列", "標題列必須為：日期,類型,金額,分類,帳戶,備註");
                return Complete(result);
            }

            int rowNumber = 1;
            while (csvReader.Read())
            {
                rowNumber++;
                string[] record = csvReader.Parser.Record ?? [];
                if (record.Length == 1 && string.IsNullOrWhiteSpace(record[0]))
                {
                    continue;
                }

                result.TotalRows++;
                if (record.Length != ExpectedHeaders.Length)
                {
                    result.AddError(rowNumber, "欄位", "欄位數量不正確");
                    continue;
                }

                if (result.Rows.Count >= MaximumDataRows)
                {
                    result.AddError(rowNumber, "檔案", "有效資料列不可超過 10,000 筆");
                    continue;
                }

                result.Rows.Add(new CsvImportRow
                {
                    RowNumber = rowNumber,
                    Date = record[0],
                    Type = record[1],
                    Amount = record[2],
                    Category = record[3],
                    Account = record[4],
                    Note = record[5]
                });
            }
        }
        catch (CsvHelperException)
        {
            result.AddError(0, "檔案", "CSV 格式無效");
        }

        if (result.Rows.Count == 0 && result.Errors.Count == 0)
        {
            result.AddError(0, "檔案", "無有效資料");
        }

        return Complete(result);
    }

    private static CsvImportResult Complete(CsvImportResult result)
    {
        result.FailedRows = result.Errors.Count;
        result.Summary = CsvImportResultFormatter.Format(result);
        return result;
    }

    private static string SanitizeFileName(string fileName)
    {
        string safeName = Path.GetFileName(fileName);
        return string.IsNullOrWhiteSpace(safeName) ? "import.csv" : safeName;
    }
}
