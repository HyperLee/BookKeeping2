# Data Model: 多幣別獨立記帳

本功能會修改既有 financial/domain entities，新增必要幣別欄位與分幣別 view model。所有 user-facing 文件與 UI 文字維持繁體中文；程式識別名稱可使用英文。

## Entity: SupportedCurrency

固定支援的記帳幣別 allow-list，不建立資料庫表。

| Field | Type | Rules |
|-------|------|-------|
| `Code` | string | 必須為 `TWD`、`USD`、`JPY`、`EUR`、`GBP` 之一 |
| `DisplayName` | string | `新台幣`、`美金`、`日幣`、`歐元`、`英鎊` |
| `SortOrder` | int | UI 選項固定排序，建議 `TWD`, `USD`, `JPY`, `EUR`, `GBP` |
| `IsDefaultForLegacyData` | bool | 只有 `TWD` 為 true |

**Validation**:

- Input must be trimmed before validation.
- Matching is case-insensitive.
- Persisted value must be uppercase code.
- Empty, null or unsupported values are rejected unless importing legacy six-column CSV, where the missing currency column means `TWD`.
- Browser language, OS locale, location or date must not infer currency.

## Entity: Transaction

使用者建立的收入或支出交易。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `TransactionDate` | DateOnly | Asia/Taipei local date; actual transaction cannot be later than today |
| `Type` | TransactionType | `Income` or `Expense` |
| `Currency` | string | Required 3-char supported currency code; default `TWD` only for existing migrated rows |
| `Amount` | decimal | Not mapped; uses `MoneyMinorUnitConverter`; > 0, <= 999,999,999.99, max 2 decimals |
| `AmountMinorUnits` | long | Persisted integer minor units |
| `CategoryId` | long | Required; category type must match transaction type |
| `AccountId` | long | Required; account must be active and same currency as transaction |
| `Note` | string? | Sanitized plain text, max 500 |
| `CreatedAtUtc` | DateTimeOffset | UTC timestamp |
| `UpdatedAtUtc` | DateTimeOffset | UTC timestamp |
| `IsDeleted` | bool | Soft delete marker |
| `DeletedAtUtc` | DateTimeOffset? | Soft delete timestamp |
| `DeletionSummary` | string? | Masked summary including necessary currency context |
| `LastChangeSummary` | string | Masked summary including necessary currency context |

**Relationships**:

- Many transactions belong to one `Category`.
- Many transactions belong to one `Account`.

**Indexes**:

- `{ IsDeleted, Currency, TransactionDate }` for date and currency filtering.
- `{ IsDeleted, Currency, CategoryId, TransactionDate }` for category/report grouping.
- `{ IsDeleted, Currency, AccountId, TransactionDate }` for account balances.
- Existing amount/date indexes may remain if still used by filters.

**State transitions**:

- Create: currency is required and normalized; selected account must match currency.
- Edit: currency may change only if the saved account is also changed or already matches new currency.
- Soft delete: transaction is excluded from normal list, summary, account balance, budget progress, report and export.

## Entity: Account

記錄交易的帳戶，每個帳戶固定屬於一種幣別。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `Name` | string | Required, max 100 |
| `NormalizedName` | string | Required, unique globally |
| `Type` | AccountType | Existing account type enum |
| `IconKey` | string | Required, max 50 |
| `OpeningBalance` | decimal | Not mapped; may be zero or positive/negative as existing rule allows |
| `OpeningBalanceMinorUnits` | long | Persisted integer minor units |
| `Currency` | string | Required supported currency code; explicitly selected on create |
| `DisplayOrder` | int | Existing ordering |
| `IsArchived` | bool | Hidden from new selections when true |
| `CreatedAtUtc` | DateTimeOffset | UTC timestamp |
| `UpdatedAtUtc` | DateTimeOffset | UTC timestamp |

**Relationships**:

- One account has many transactions.

**Validation**:

- Account name remains globally unique, not unique per currency.
- New account creation rejects missing/blank/unsupported currency.
- Account currency is immutable after creation.
- Account balance uses opening balance plus same-account, same-currency, non-deleted transactions.
- UI must not show a single combined total across accounts with different currencies.

## Entity: Category

使用者管理的收入或支出分類。本功能不為分類新增幣別欄位。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `Name` | string | Existing required name |
| `NormalizedName` | string | Existing normalized name |
| `Type` | TransactionType | Category type controls income/expense usage |
| `IsArchived` | bool | Existing archive behavior |

**Validation**:

- Category can be shared by all supported currencies.
- Category must not be auto-duplicated or renamed by currency.
- Category totals and percentages must be grouped by currency at report time.

## Entity: Budget

針對單一月份、支出分類與幣別的預算設定。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `CategoryId` | long | Required expense category |
| `BudgetMonth` | DateOnly | First day of the month |
| `Currency` | string | Required supported currency code |
| `Amount` | decimal | Not mapped; > 0, <= 999,999,999.99, max 2 decimals |
| `AmountMinorUnits` | long | Persisted integer minor units |
| `CreatedAtUtc` | DateTimeOffset | UTC timestamp |
| `UpdatedAtUtc` | DateTimeOffset | UTC timestamp |

**Relationships**:

- Many budgets reference one expense `Category`.

**Indexes**:

- Unique `{ CategoryId, BudgetMonth, Currency }`.
- Optional `{ BudgetMonth, Currency }` for monthly list queries if profiling shows benefit.

**Validation**:

- Category must exist, be active and have `Type == Expense`.
- Same month/category/currency cannot duplicate an active budget.
- Same month/category with different currency is allowed.
- Spent/remaining/overspent/alert status only use same month, category and currency expense transactions.

## Entity: CurrencyAmount

Display-layer amount with explicit currency. This is a value shape, not a persisted table.

| Field | Type | Rules |
|-------|------|-------|
| `Currency` | string | Supported currency code |
| `Amount` | decimal | Existing domain decimal amount |
| `MinorUnits` | long | Optional internal projection for efficient grouping |
| `DisplayText` | string | Must include currency code or adjacent accessible currency label |

**Validation**:

- Any visible transaction, account, budget, report, summary or CSV amount must let the user identify its currency.
- Different currencies must never be added together.

## Entity: AccountBalanceSummary

分幣別帳戶餘額呈現。

| Field | Type | Rules |
|-------|------|-------|
| `AccountId` | long | Existing account id |
| `Name` | string | Account name |
| `Type` | AccountType | Existing type |
| `Currency` | string | Account currency |
| `OpeningBalance` | decimal | Optional display if needed |
| `CurrentBalance` | decimal | Opening balance plus same-account/same-currency non-deleted transactions |
| `IsArchived` | bool | Existing archive state |

**Validation**:

- A `USD` transaction cannot affect a `TWD` account balance.
- Account summary pages must not show a cross-currency total balance.

## Entity: MonthlyCurrencyReport

單一月份中某一幣別的報表 bucket。

| Field | Type | Rules |
|-------|------|-------|
| `Month` | DateOnly | First day of reported month |
| `Currency` | string | Supported currency code |
| `TotalIncome` | decimal | Same-currency income only |
| `TotalExpense` | decimal | Same-currency expense only |
| `Balance` | decimal | `TotalIncome - TotalExpense` within same currency |
| `CategoryShares` | IReadOnlyList<CategoryCurrencyShare> | Expense category shares within same currency |
| `TrendPoints` | IReadOnlyList<ReportCurrencyChartPoint> | Daily income/expense points within same currency |

**Validation**:

- Months with no transactions retain existing empty state and do not need to list all currencies.
- Single-currency months may show only that currency.
- Multi-currency months must show separate buckets and no cross-currency totals.

## Entity: CategoryCurrencyShare

分類支出佔比，限定同一幣別。

| Field | Type | Rules |
|-------|------|-------|
| `Currency` | string | Supported currency code |
| `CategoryName` | string | Original category name |
| `Amount` | decimal | Same-currency category expense total |
| `Percentage` | decimal | Percentage of same-currency monthly expense |

**Validation**:

- Percentage denominator is same-currency monthly expense only.
- Same category can appear once per currency bucket.

## Entity: CsvImportRow

CSV 匯入交易資料中的單列資料。

| Field | Type | Rules |
|-------|------|-------|
| `RowNumber` | int | Physical row number |
| `Date` | string | `yyyy-MM-dd` |
| `Type` | string | `收入` or `支出` |
| `Currency` | string | Seven-column CSV: required and validated; six-column CSV: implicit `TWD` |
| `Amount` | string | Invariant decimal text |
| `Category` | string | Original text, trimmed for validation |
| `Account` | string | Must reference existing active account with matching currency |
| `Note` | string? | Sanitized before persistence |

**Validation**:

- Accepted headers:
  - New: `日期,類型,幣別,金額,分類,帳戶,備註`
  - Legacy: `日期,類型,金額,分類,帳戶,備註`
- New format with blank currency fails row validation.
- Unsupported currency fails row validation.
- Account currency mismatch fails row validation.
- Row-level errors include user-understandable reason and safe raw value preview.

## Entity: CsvExportRow

CSV 匯出交易資料中的單列資料。

| Field | Type | Rules |
|-------|------|-------|
| `Date` | string | `yyyy-MM-dd` |
| `Type` | string | `收入` or `支出` |
| `Currency` | string | Original transaction currency |
| `Amount` | string | Original transaction amount, no conversion |
| `Category` | string | Formula-protected category name |
| `Account` | string | Formula-protected account name |
| `Note` | string | Formula-protected note |

**Validation**:

- Header order must be exactly `日期,類型,幣別,金額,分類,帳戶,備註`.
- Soft-deleted transactions are excluded.
- Amounts are not converted or combined.

## Migration Notes

- Existing transactions without currency become `TWD` and keep the same `AmountMinorUnits`.
- Existing budgets without currency become `TWD` and keep the same `AmountMinorUnits`.
- Existing accounts without currency become `TWD`; current code already has `Account.Currency`, but migration and model snapshot must be verified.
- Existing unique budget index `{ CategoryId, BudgetMonth }` must be replaced by `{ CategoryId, BudgetMonth, Currency }`.
- New required currency columns must have explicit default/backfill for existing SQLite rows.
