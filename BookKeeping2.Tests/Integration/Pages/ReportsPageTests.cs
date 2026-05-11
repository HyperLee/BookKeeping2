using System.Net;
using BookKeeping2.Tests.TestSupport;
using Xunit;

namespace BookKeeping2.Tests.Integration.Pages;

public sealed class ReportsPageTests
{
    [Fact]
    public async Task Reports_page_shows_blank_state_when_month_has_no_transactions()
    {
        await using BookKeepingWebApplicationFactory factory = new();
        HttpClient client = factory.CreateClient();

        string page = await client.GetStringAsync("/Reports?Month=2026-03");

        Assert.Contains("本月尚無紀錄", WebUtility.HtmlDecode(page));
        Assert.Contains("新增交易", WebUtility.HtmlDecode(page));
    }
}
