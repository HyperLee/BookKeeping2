using System.Text;
using BookKeeping2.Services.Csv;
using Xunit;

namespace BookKeeping2.Tests.Unit.Csv;

public sealed class CsvTransferImportParserTests
{
    [Fact]
    public void Parse_accepts_exact_transfer_header_and_rejects_transaction_headers_wrong_order_and_empty_files()
    {
        var parser = new CsvTransferImportParser();
        CsvTransferImportResult valid = parser.Parse(new CsvImportCommand(
            "transfers.csv",
            Encoding.UTF8.GetBytes("日期,幣別,金額,轉出帳戶,轉入帳戶,備註\r\n2026-05-01,TWD,1000,銀行,現金,提款")));
        CsvTransferImportResult transactionHeader = parser.Parse(new CsvImportCommand(
            "transactions.csv",
            Encoding.UTF8.GetBytes("日期,類型,幣別,金額,分類,帳戶,備註\r\n2026-05-01,支出,TWD,100,餐飲,現金,午餐")));
        CsvTransferImportResult wrongOrder = parser.Parse(new CsvImportCommand(
            "wrong.csv",
            Encoding.UTF8.GetBytes("日期,金額,幣別,轉出帳戶,轉入帳戶,備註\r\n2026-05-01,1000,TWD,銀行,現金,提款")));
        CsvTransferImportResult empty = parser.Parse(new CsvImportCommand("empty.csv", []));

        CsvTransferRow row = Assert.Single(valid.Rows);
        Assert.Equal("2026-05-01", row.Date);
        Assert.Equal("TWD", row.Currency);
        Assert.Equal("銀行", row.FromAccount);
        Assert.Equal("現金", row.ToAccount);
        Assert.Contains(transactionHeader.Errors, error => error.FieldName == "標題列");
        Assert.Contains(wrongOrder.Errors, error => error.FieldName == "標題列");
        Assert.Contains(empty.Errors, error => error.Reason.Contains("空檔案", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_reports_field_count_and_row_limit_errors()
    {
        var parser = new CsvTransferImportParser(maximumDataRows: 1);
        string csv = "日期,幣別,金額,轉出帳戶,轉入帳戶,備註\r\n"
            + "2026-05-01,TWD,1000,銀行,現金,提款\r\n"
            + "2026-05-02,TWD,1000,銀行\r\n"
            + "2026-05-03,TWD,1000,銀行,現金,超過上限";

        CsvTransferImportResult result = parser.Parse(new CsvImportCommand("rows.csv", Encoding.UTF8.GetBytes(csv)));

        Assert.Single(result.Rows);
        Assert.Contains(result.Errors, error => error.RowNumber == 3 && error.Reason.Contains("欄位數量", StringComparison.Ordinal));
        Assert.Contains(result.Errors, error => error.RowNumber == 4 && error.Reason.Contains("有效資料列", StringComparison.Ordinal));
    }
}
