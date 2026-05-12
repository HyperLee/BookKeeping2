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
