using System.Security.Cryptography;
using System.Text;

namespace BookKeeping2.Services.Audit;

/// <summary>
/// Creates minimal masked audit summaries for sensitive financial values.
/// </summary>
public sealed class AuditLogMaskingPolicy
{
    /// <summary>
    /// Masks an amount for logs and audit summaries.
    /// </summary>
    /// <param name="amount">The amount to mask.</param>
    /// <returns>A non-sensitive amount band.</returns>
    public string MaskAmount(decimal amount)
    {
        decimal absolute = Math.Abs(amount);
        return absolute switch
        {
            < 1_000m => "TWD <1K",
            < 10_000m => "TWD 1K-10K",
            < 100_000m => "TWD 10K-100K",
            _ => "TWD >=100K"
        };
    }

    /// <summary>
    /// Masks free text with a stable hash preview.
    /// </summary>
    /// <param name="value">The sensitive text.</param>
    /// <returns>A masked text summary.</returns>
    public string MaskText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "空白";
        }

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"hash:{Convert.ToHexString(hash)[..12]}";
    }
}
