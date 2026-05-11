namespace BookKeeping2.Validation;

/// <summary>
/// Centralizes user-facing Traditional Chinese validation messages.
/// </summary>
public static class FinancialValidationMessages
{
    /// <summary>
    /// Message for missing amount.
    /// </summary>
    public const string AmountRequired = "請輸入金額。";

    /// <summary>
    /// Message for invalid amount range.
    /// </summary>
    public const string AmountMustBePositive = "金額必須大於 0。";

    /// <summary>
    /// Message for amount precision violations.
    /// </summary>
    public const string AmountPrecision = "金額最多只能有 2 位小數。";

    /// <summary>
    /// Message for unsupported amount magnitude.
    /// </summary>
    public const string AmountTooLarge = "金額不可超過 TWD 999,999,999.99。";

    /// <summary>
    /// Message for future transaction dates.
    /// </summary>
    public const string DateCannotBeFuture = "交易日期不可晚於今天。";

    /// <summary>
    /// Message for invalid category selection.
    /// </summary>
    public const string CategoryRequired = "請選擇有效分類。";

    /// <summary>
    /// Message for invalid account selection.
    /// </summary>
    public const string AccountRequired = "請選擇有效帳戶。";
}
