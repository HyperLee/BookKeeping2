namespace BookKeeping2.Models.Settings;

/// <summary>
/// Represents a site-level non-secret application setting.
/// </summary>
public sealed class AppSetting
{
    /// <summary>
    /// Gets or sets the setting key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the setting value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latest update timestamp in UTC.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
