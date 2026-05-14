# Contract: 多幣別獨立記帳

## Scope

本契約定義固定幣別值、資料庫遷移、交易/帳戶/預算驗證、報表與摘要呈現、CSV 匯入匯出、稽核摘要、安全與效能要求。它不定義匯率、換匯、跨幣別換算、多使用者、多帳本或第三方金融資料同步。

## Supported Currency Contract

Allowed persisted currency codes:

```text
TWD
USD
JPY
EUR
GBP
```

Rules:

- Accept input case-insensitively after trimming surrounding whitespace.
- Store uppercase code only.
- Reject null, empty, whitespace-only or unsupported values.
- Do not infer currency from browser language, OS locale, location, account name, date or category.
- Legacy records or legacy six-column CSV rows without a currency column are `TWD`.
- Seven-column CSV rows with an empty currency field are invalid and must not default.

Display names:

| Code | zh-TW display |
|------|---------------|
| `TWD` | 新台幣 |
| `USD` | 美金 |
| `JPY` | 日幣 |
| `EUR` | 歐元 |
| `GBP` | 英鎊 |

Visible monetary values may use code-only display such as `USD 100.00` if the surrounding UI is clear and accessible.

## No Conversion Contract

The system must not provide:

- exchange rates
- automatic or manual conversion
- foreign exchange records
- cross-currency total assets
- cross-currency total income
- cross-currency total expense
- cross-currency balance
- any hidden equivalent of mixed-currency arithmetic

All calculations are valid only within the same currency.

## Persistence Contract

Required persisted fields:

- `Transactions.Currency` required `TEXT`, max length 3, normalized uppercase.
- `Accounts.Currency` required `TEXT`, max length 3, normalized uppercase.
- `Budgets.Currency` required `TEXT`, max length 3, normalized uppercase.

Migration requirements:

- Existing rows in `Transactions`, `Accounts` and `Budgets` must become `TWD`.
- Existing numeric amounts and minor units must not change.
- Existing soft-delete state, audit summaries, categories and account names must not change.
- Budget uniqueness must become `{ CategoryId, BudgetMonth, Currency }`.
- Transaction indexes must support currency/date/category/account filtering.

SQLite constraints:

- Use EF Core migration and entity configuration.
- Validate behavior with SQLite integration tests, not only in-memory LINQ tests.
- If SQLite requires table rebuild for an operation, migration must preserve data and foreign keys.

## Transaction Form Contract

Create and edit transaction forms must:

- Render a required currency control with exactly the supported currency options.
- Default only new blank forms to `TWD` if product flow needs a default; posted data still must be validated.
- Preserve existing transaction currency on edit.
- Allow changing transaction currency only when the selected account has the same currency.
- Filter or clearly mark account options by currency; server-side validation remains authoritative.
- Show every visible transaction amount with currency in list, details, edit confirmation and delete confirmation.
- Use anti-forgery protection for state-changing POSTs.

Server-side validation must reject:

- unsupported currency
- missing currency
- category not matching transaction type
- account missing, archived or currency-mismatched
- future actual transaction date
- invalid amount rules

Duplicate submission detection must include currency so `TWD 100.00` and `USD 100.00` are distinct.

## Account Contract

Account creation must:

- Require explicit supported currency.
- Normalize and persist uppercase currency.
- Preserve global unique account name rule across all currencies.
- Allow opening balance in the selected account currency using existing amount rules.

Account updates must:

- Not allow currency changes after creation.
- Continue allowing non-currency editable fields only if existing account workflows support them.

Account summaries must:

- Calculate balance from opening balance plus non-deleted same-account, same-currency transactions.
- Display account currency with balances.
- Not display a single total across accounts of different currencies.

## Budget Contract

Budget forms must:

- Require month, expense category, currency and amount.
- Allow same month/category with different currencies.
- Reject duplicate same month/category/currency budgets.

Budget progress must:

- Include only non-deleted expense transactions matching the same month, category and currency.
- Keep usage rate, remaining amount, overspent amount and alert state per currency.
- Ensure a transaction in one currency never changes another currency's budget warning.

## Summary And Report Contract

Homepage summary, monthly report, category statistics and trend chart must:

- Group income, expense, balance, category totals, percentages and trend points by currency.
- Omit currencies with no relevant data unless a form/control is listing supported options.
- Show single-currency months as one clear currency bucket.
- Show multi-currency months as separate buckets.
- Never show cross-currency total income, total expense, balance, asset or budget.

Category share percentages:

- Numerator is same-currency category expense.
- Denominator is same-currency total expense.
- Same category may appear in multiple currency buckets.

Chart.js datasets:

- Labels or legends must include currency when multiple currencies are present.
- Tooltip/accessibility text must identify currency.
- Datasets must not combine values from different currencies.

## Transaction List Filter Contract

Transaction list filters must:

- Add optional currency filter with supported currency options.
- Keep date, category, account, amount and keyword filters working.
- When no currency filter is applied, display each row's currency clearly.
- Preserve pagination and page size behavior.
- Exclude soft-deleted rows.

Performance expectation:

- 10,000 transaction filtering with currency and existing filters must complete under 2 seconds in the test environment used for existing performance checks.

## CSV Contract

### Export

Header order must be exactly:

```text
日期,類型,幣別,金額,分類,帳戶,備註
```

Row rules:

- `日期`: `yyyy-MM-dd`
- `類型`: `收入` or `支出`
- `幣別`: original transaction currency code
- `金額`: original amount, no conversion
- `分類`, `帳戶`, `備註`: preserve original text after existing formula-injection protection
- Exclude soft-deleted transactions

### Import

Accepted headers:

```text
日期,類型,幣別,金額,分類,帳戶,備註
日期,類型,金額,分類,帳戶,備註
```

Seven-column rows:

- Currency field is required.
- Currency must normalize to a supported code.
- Blank currency is a row error.

Six-column legacy rows:

- Currency is `TWD`.

All rows:

- Date, type, amount, category and account validation continue to apply.
- Account currency must match transaction currency.
- Unsupported currency and account mismatch must be reported with row-level reason.
- Success count, failure count and created category count must remain accurate.
- CSV import persistence must stay atomic for created valid transactions and import batch/error records.

## Audit And Logging Contract

Audit summaries for transaction changes, CSV import and budget warnings must include enough currency context to identify affected currency, for example `USD` in a masked amount summary.

Prohibited:

- Raw sensitive financial details beyond existing masked summary policy.
- Logging raw CSV content.
- Logging unsupported raw currency values beyond safe row error previews.
- Sending financial or currency data to third parties.

## UI Accessibility And Layout Contract

All changed pages must:

- Use Traditional Chinese user-facing text.
- Keep visible focus indicators.
- Associate validation messages with fields.
- Avoid overlap at mobile and desktop widths.
- Avoid horizontal overflow from new currency labels.
- Use table/card layouts where currency remains close to amount.
- Keep theme and language toggle behavior intact.

Manual/browser verification must include at least:

- Transaction create/edit/list at mobile and desktop width.
- Account creation/list.
- Budget list/progress.
- Monthly report with multiple currencies.
- CSV import/export pages.

## Security Contract

- Treat currency as untrusted input.
- Validate only through allow-list.
- Use server-side validation as authoritative.
- Preserve Razor HTML encoding.
- Preserve `UseBookKeepingSecurityHeaders()`, HTTPS redirection and production HSTS.
- Preserve anti-forgery for all state-changing forms.
- Preserve CsvHelper parsing/writing and formula injection protection.

## Compatibility Contract

Existing TWD data must remain usable after migration:

- Existing transactions show `TWD`.
- Existing accounts show `TWD`.
- Existing budgets show `TWD`.
- Existing reports still work as single-currency TWD reports.
- Existing six-column CSV imports still work and create `TWD` transactions.

Backward-incompatible CSV change:

- New exports are seven-column only because the feature must preserve currency.
- Import remains backward compatible with six-column files.

## Implementation Verification Notes

- 2026-05-14: Tasks T001-T004 reviewed this contract against the requested stories. CSV, report, account, budget, transaction UI and validation requirements are all represented by explicit tests in `tasks.md`.
- 2026-05-14: Shared test data helpers will expose stable supported-currency constants before adding entity builders so test cases can name currency intent consistently.
