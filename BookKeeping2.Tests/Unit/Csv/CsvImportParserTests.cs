using System.Diagnostics;
using System.Text;
using BookKeeping2.Services.Csv;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvImportParserTests
{
    [Fact]
    public void Parse_validates_header_field_count_empty_file_size_and_parses_100_rows_under_ten_seconds()
    {
        var parser = new CsvImportParser();
        string validCsv = "日期,類型,金額,分類,帳戶,備註\r\n"
            + string.Join("\r\n", Enumerable.Range(1, 100).Select(i => $"2026-02-{((i - 1) % 28) + 1:00},支出,100,餐飲,現金,測試 {i}"));
        Stopwatch stopwatch = Stopwatch.StartNew();

        CsvImportResult validResult = parser.Parse(new CsvImportCommand("valid.csv", Encoding.UTF8.GetBytes(validCsv)));
        CsvImportResult badHeader = parser.Parse(new CsvImportCommand("bad-header.csv", Encoding.UTF8.GetBytes("日期,金額\r\n2026-02-01,100")));
        CsvImportResult badFieldCount = parser.Parse(new CsvImportCommand("bad-fields.csv", Encoding.UTF8.GetBytes("日期,類型,金額,分類,帳戶,備註\r\n2026-02-01,支出,100")));
        CsvImportResult empty = parser.Parse(new CsvImportCommand("empty.csv", []));
        CsvImportResult tooLarge = parser.Parse(new CsvImportCommand("large.csv", new byte[(5 * 1024 * 1024) + 1]));

        stopwatch.Stop();
        Assert.Equal(100, validResult.Rows.Count);
        Assert.Empty(validResult.Errors);
        Assert.Contains(badHeader.Errors, error => error.RowNumber == 1 && error.Reason.Contains("標題列", StringComparison.Ordinal));
        Assert.Contains(badFieldCount.Errors, error => error.Reason.Contains("欄位數量不正確", StringComparison.Ordinal));
        Assert.Contains(empty.Errors, error => error.Reason.Contains("空檔案", StringComparison.Ordinal));
        Assert.Contains(tooLarge.Errors, error => error.Reason.Contains("檔案大小", StringComparison.Ordinal));
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void Parse_accepts_seven_column_currency_header_normalizes_currency_and_rejects_blank_currency()
    {
        var parser = new CsvImportParser();
        string csv = "日期,類型,幣別,金額,分類,帳戶,備註\r\n"
            + "2026-02-01,支出, usd ,100,餐飲,美元現金,午餐\r\n"
            + "2026-02-02,支出, ,100,餐飲,美元現金,空白幣別";

        CsvImportResult result = parser.Parse(new CsvImportCommand("seven-column.csv", Encoding.UTF8.GetBytes(csv)));

        CsvImportRow row = Assert.Single(result.Rows);
        Assert.Equal("USD", row.Currency);
        Assert.False(row.IsLegacyFormat);
        Assert.Contains(result.Errors, error => error.RowNumber == 3 && error.FieldName == "幣別" && error.Reason.Contains("幣別不可空白", StringComparison.Ordinal));
    }
}
