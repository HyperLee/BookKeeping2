using System.Xml.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace BookKeeping2.Tests.Integration.StaticAssets;

public sealed partial class LanguageResourceCompletenessTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string EnglishResourcePath = Path.Combine(RepositoryRoot, "BookKeeping2", "Resources", "SharedResource.en.resx");

    private static readonly string[] FoundationalKeys =
    [
        "繁體中文",
        "English",
        "介面語言",
        "Open BookKeeping",
        "首頁",
        "交易",
        "分類",
        "帳戶",
        "預算",
        "報表",
        "CSV",
        "隱私權",
        "新增",
        "儲存",
        "取消",
        "名稱",
        "金額",
        "日期"
    ];

    [Fact]
    public void English_resource_contains_foundational_shared_keys()
    {
        Dictionary<string, string> values = ReadEnglishResource();

        foreach (string key in FoundationalKeys)
        {
            Assert.True(values.ContainsKey(key), $"Missing English resource key: {key}");
        }
    }

    [Fact]
    public void English_resource_values_are_not_blank_or_internal_placeholders()
    {
        Dictionary<string, string> values = ReadEnglishResource();

        foreach ((string key, string value) in values)
        {
            Assert.False(string.IsNullOrWhiteSpace(value), $"English resource value is blank: {key}");
            Assert.DoesNotContain("TODO", value, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("TBD", value, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("{{", value, StringComparison.Ordinal);
            Assert.DoesNotContain("}}", value, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Parameterized_english_resource_values_keep_matching_placeholders()
    {
        Dictionary<string, string> values = ReadEnglishResource();

        foreach ((string key, string value) in values)
        {
            var keyPlaceholders = PlaceholderRegex().Matches(key).Select(match => match.Value).Order().ToArray();
            var valuePlaceholders = PlaceholderRegex().Matches(value).Select(match => match.Value).Order().ToArray();

            Assert.Equal(keyPlaceholders, valuePlaceholders);
        }
    }

    private static Dictionary<string, string> ReadEnglishResource()
    {
        XDocument document = XDocument.Load(EnglishResourcePath);
        return document.Root!
            .Elements("data")
            .Where(element => element.Attribute("name")?.Value is not null)
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);
    }

    [GeneratedRegex(@"\{[0-9]+\}")]
    private static partial Regex PlaceholderRegex();

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
