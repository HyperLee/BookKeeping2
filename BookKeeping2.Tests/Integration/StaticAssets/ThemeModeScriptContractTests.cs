using Xunit;

namespace BookKeeping2.Tests.Integration.StaticAssets;

public sealed partial class ThemeModeScriptContractTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string LayoutPath = Path.Combine(RepositoryRoot, "BookKeeping2", "Pages", "Shared", "_Layout.cshtml");
    private static readonly string SiteScriptPath = Path.Combine(RepositoryRoot, "BookKeeping2", "wwwroot", "js", "site.js");

    private static string ReadLayout()
    {
        return File.ReadAllText(LayoutPath);
    }

    private static string ReadSiteScript()
    {
        return File.ReadAllText(SiteScriptPath);
    }

    [Fact]
    public void Pre_paint_script_derives_system_mode_from_preferred_color_scheme_with_light_fallback()
    {
        string layout = ReadLayout();

        Assert.Contains("matchMedia('(prefers-color-scheme: dark)')", layout, StringComparison.Ordinal);
        Assert.Contains("system", layout, StringComparison.Ordinal);
        Assert.Contains("light", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void Runtime_script_updates_system_mode_from_preferred_color_scheme_only_when_selected()
    {
        string script = ReadSiteScript();

        Assert.Contains("matchMedia('(prefers-color-scheme: dark)')", script, StringComparison.Ordinal);
        Assert.Contains("addEventListener('change'", script, StringComparison.Ordinal);
        Assert.Contains("currentMode === 'system'", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Runtime_script_validates_storage_key_and_writes_only_allow_list_modes()
    {
        string script = ReadSiteScript();

        Assert.Contains("bookkeeping.theme.mode", script, StringComparison.Ordinal);
        Assert.Contains("normalizeMode", script, StringComparison.Ordinal);
        Assert.Contains("localStorage.setItem(storageKey, normalizeMode(mode))", script, StringComparison.Ordinal);
    }

    [Fact]
    public void Runtime_script_does_not_submit_forms_or_call_finance_endpoints()
    {
        string script = ReadSiteScript();

        Assert.DoesNotContain(".submit(", script, StringComparison.Ordinal);
        Assert.DoesNotContain("fetch(", script, StringComparison.Ordinal);
        Assert.DoesNotContain("XMLHttpRequest", script, StringComparison.Ordinal);
        Assert.DoesNotContain("/Transactions", script, StringComparison.Ordinal);
        Assert.DoesNotContain("/Accounts", script, StringComparison.Ordinal);
        Assert.DoesNotContain("/Budgets", script, StringComparison.Ordinal);
        Assert.DoesNotContain("/Categories", script, StringComparison.Ordinal);
        Assert.DoesNotContain("/Csv", script, StringComparison.Ordinal);
        Assert.DoesNotContain("/Reports", script, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "BookKeeping2.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate repository root from test output directory.");
    }
}
