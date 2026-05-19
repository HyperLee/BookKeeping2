using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace BookKeeping2.Services.Csv;

/// <summary>
/// Parses transfer CSV files using the fixed six-column transfer contract.
/// </summary>
public sealed class CsvTransferImportParser
{
    /// <summary>
    /// Header text for the transfer CSV format.
    /// </summary>
    public const string HeaderText = "日期,幣別,金額,轉出帳戶,轉入帳戶,備註";

    private static readonly string[] Headers = HeaderText.Split(',');
    private readonly int maximumDataRows;

    /// <summary>
    /// Initializes a transfer CSV parser.
    /// </summary>
    /// <param name="maximumDataRows">The maximum allowed parsed data rows.</param>
    public CsvTransferImportParser(int maximumDataRows = CsvImportParser.MaximumDataRows)
    {
        this.maximumDataRows = maximumDataRows;
    }

    /// <summary>
    /// Parses the uploaded CSV into transfer rows and structural errors.
    /// </summary>
    /// <param name="command">The import command.</param>
    /// <returns>The parse result.</returns>
    public CsvTransferImportResult Parse(CsvImportCommand command)
    {
        var result = new CsvTransferImportResult { FileName = SanitizeFileName(command.FileName) };
        if (command.Content.Length == 0)
        {
            result.AddError(0, "檔案", "空檔案");
            return Complete(result);
        }

        if (command.Content.Length > CsvImportParser.MaximumFileBytes)
        {
            result.AddError(0, "檔案", "檔案大小不可超過 5 MB");
            return Complete(result);
        }

        string csvText = Encoding.UTF8.GetString(command.Content);
        if (string.IsNullOrWhiteSpace(csvText))
        {
            result.AddError(0, "檔案", "空檔案");
            return Complete(result);
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
            if (!Headers.SequenceEqual(header))
            {
                result.AddError(1, "標題列", $"標題列必須為：{HeaderText}");
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
                if (record.Length != Headers.Length)
                {
                    result.AddError(rowNumber, "欄位", "欄位數量不正確");
                    continue;
                }

                if (result.Rows.Count >= maximumDataRows)
                {
                    result.AddError(rowNumber, "檔案", $"有效資料列不可超過 {maximumDataRows:N0} 筆");
                    continue;
                }

                result.Rows.Add(new CsvTransferRow
                {
                    RowNumber = rowNumber,
                    Date = record[0],
                    Currency = record[1],
                    Amount = record[2],
                    FromAccount = record[3],
                    ToAccount = record[4],
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

    private static CsvTransferImportResult Complete(CsvTransferImportResult result)
    {
        result.FailedRows = result.Errors.Count;
        result.Summary = FormatSummary(result);
        return result;
    }

    internal static string FormatSummary(CsvTransferImportResult result)
    {
        return $"轉帳匯入成功 {result.SucceededRows} 筆，失敗 {result.FailedRows} 筆";
    }

    private static string SanitizeFileName(string fileName)
    {
        string safeName = Path.GetFileName(fileName);
        return string.IsNullOrWhiteSpace(safeName) ? "transfer-import.csv" : safeName;
    }
}
