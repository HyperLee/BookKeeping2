using Ganss.Xss;

namespace BookKeeping2.Validation;

/// <summary>
/// Sanitizes user-provided text before persistence or display.
/// </summary>
public sealed class TextInputSanitizer
{
    private readonly HtmlSanitizer sanitizer = new();

    /// <summary>
    /// Sanitizes text intended to be stored as plain user-entered content.
    /// </summary>
    /// <param name="input">The input text.</param>
    /// <param name="maxLength">The maximum allowed persisted length.</param>
    /// <returns>Sanitized text, or <see langword="null" /> when the input is blank.</returns>
    public string? SanitizePlainText(string? input, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        string cleaned = sanitizer.Sanitize(input).Trim();
        if (cleaned.Length <= maxLength)
        {
            return cleaned;
        }

        return cleaned[..maxLength];
    }
}
