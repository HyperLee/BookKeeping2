# Quickstart: 多幣別獨立記帳

## Prerequisites

- Branch: `004-multi-currency-bookkeeping`
- Spec: `specs/004-multi-currency-bookkeeping/spec.md`
- Plan: `specs/004-multi-currency-bookkeeping/plan.md`
- App project: `BookKeeping2/BookKeeping2.csproj`
- Test project: `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

## Implementation Order

1. Confirm each user story's test intent with the user or maintainer, then add failing tests.
2. Add an internal supported-currency helper and unit tests for normalization, display values and rejection.
3. Update models/configurations/migration for `Transaction.Currency`, `Budget.Currency` and verified `Account.Currency`; backfill existing data to `TWD`.
4. Update account creation flow to require currency and prevent account currency edits.
5. Update transaction input/list/query/service flow for currency selection, filtering, duplicate detection and account currency validation.
6. Update budget input/list/service flow for month/category/currency uniqueness and same-currency progress calculations.
7. Update report/homepage/account summary services and view models to return currency buckets instead of cross-currency totals.
8. Update CSV parser/import/export for seven-column contract plus six-column legacy import.
9. Update Razor Pages and CSS/JS only where needed to display currency clearly and keep responsive layout stable.
10. Run targeted tests, browser checks and full verification.

## Test Checklist

Add or update tests before implementation:

- `SupportedCurrencyTests`: trims, case-insensitive match, uppercase persistence, supported display names, unsupported rejection.
- `AccountServiceTests`: account creation requires currency, defaults only legacy migration data to `TWD`, rejects duplicate account name across currencies, prevents changing currency after creation.
- `TransactionServiceValidationTests`: create/edit require supported currency, reject mismatched account currency, allow same category/amount/date with different currency, duplicate detection includes currency, audit summary includes currency context.
- `TransactionQueryServiceTests`: optional currency filter works with existing filters and pagination.
- `BudgetServiceTests`: unique month/category/currency, same category different currencies allowed, spent/remaining/alert use same currency only.
- `ReportServiceTests`: monthly totals, category shares and trend points are grouped by currency with no cross-currency totals.
- `CsvImportParserTests`: accepts seven-column header, accepts six-column legacy header, rejects malformed headers, rejects blank currency in seven-column rows.
- `CsvImportServiceTests`: imports supported currency, defaults legacy rows to `TWD`, rejects unsupported currency, rejects account currency mismatch, preserves row-level errors.
- `CsvExportServiceTests`: outputs `日期,類型,幣別,金額,分類,帳戶,備註`, preserves original amount/currency and formula protection.
- `MultiCurrencyPersistenceTests`: migration/model snapshot contains required currency columns, default `TWD`, composite budget unique index and transaction currency indexes.
- `MultiCurrencyPageTests`: pages render currency controls/labels, reject invalid POSTs, never show cross-currency totals.
- `MultiCurrencyBrowserTests`: desktop/mobile create account, create transaction, view summary/report/CSV with no overlap and visible focus.
- `TransactionQueryPerformanceTests`: 10,000 transaction currency filter remains under target.

## Expected Manual Behavior

### Create Accounts

1. Open `/Accounts`.
2. Create one `TWD` account and one `USD` account with different names.
3. Try creating an account without currency.
4. Confirm the form rejects it with a Traditional Chinese message.
5. Confirm account list shows each balance with its own currency and no combined total.

### Create Transactions

1. Open `/Transactions/Create`.
2. Create a `TWD 100.00` expense using the TWD account.
3. Create a `USD 100.00` expense using the USD account.
4. Confirm transaction list shows both rows separately and each amount includes currency.
5. Try posting a `USD` transaction with the TWD account.
6. Confirm the server rejects it and no transaction is persisted.

### Edit Transaction Currency

1. Edit an existing `TWD` transaction.
2. Change currency to `USD` while keeping a TWD account.
3. Confirm validation requires choosing a USD account.
4. Choose a USD account and save.
5. Confirm the list, details, account balance and report use the new currency.

### Budgets

1. Open `/Budgets?Month=2026-05`.
2. Create `2026-05` + `餐飲` + `TWD` budget.
3. Create `2026-05` + `餐飲` + `USD` budget.
4. Confirm both can exist.
5. Add a USD expense for `餐飲`.
6. Confirm only the USD budget progress changes.
7. Attempt another `2026-05` + `餐飲` + `USD` budget.
8. Confirm duplicate is rejected.

### Reports And Summary

1. Seed or create same-month `TWD` and `USD` income/expense rows.
2. Open `/` and `/Reports?Year=2026&Month=5`.
3. Confirm income, expense, balance, category shares and trend chart are separated by currency.
4. Confirm the UI never shows `TWD 100.00 + USD 100.00 = 200.00`.
5. Confirm months without transactions keep the existing empty state.

### CSV Export

1. Open `/Csv/Export`.
2. Export transactions.
3. Confirm the downloaded CSV header is:

   ```text
   日期,類型,幣別,金額,分類,帳戶,備註
   ```

4. Confirm each row contains the original transaction currency and no converted amount.
5. Confirm formula injection protection still prefixes dangerous category/account/note values.

### CSV Import

1. Import a seven-column file containing `EUR` rows and matching EUR account.
2. Confirm rows import as `EUR`.
3. Import a seven-column file containing `AUD`.
4. Confirm row-level unsupported currency error.
5. Import a seven-column row with blank currency.
6. Confirm row-level blank currency error.
7. Import a legacy six-column file.
8. Confirm created transactions are `TWD`.
9. Import a `USD` row referencing a `TWD` account.
10. Confirm row-level account currency mismatch error.

### Responsive And Accessibility

1. Test `/Transactions`, `/Accounts`, `/Budgets`, `/Reports`, `/Csv/Import` and `/Csv/Export` at mobile and desktop widths.
2. Confirm currency controls are keyboard operable.
3. Confirm visible focus remains clear.
4. Confirm amount and currency labels remain adjacent and readable.
5. Confirm existing theme and language controls still behave correctly.

## Commands

Run from repository root.

```powershell
dotnet build BookKeeping2.slnx
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
```

Targeted commands while developing:

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~SupportedCurrencyTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~AccountServiceTests|FullyQualifiedName~TransactionServiceValidationTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~BudgetServiceTests|FullyQualifiedName~ReportServiceTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~CsvImport|FullyQualifiedName~CsvExport"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~MultiCurrencyPersistenceTests|FullyQualifiedName~MultiCurrencyPageTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~MultiCurrencyBrowserTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~TransactionQueryPerformanceTests"
```

If browser tests cannot run because Chrome or Edge is unavailable in the environment, record the exact blocker and still run the non-browser subset.

## Implementation Verification Results

2026-05-14 final verification on branch `004-multi-currency-bookkeeping`:

- Targeted unit tests passed: `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~BookKeeping2.Tests.Unit"` -> 69 passed, 0 failed.
- Persistence/page/performance integration tests passed: `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~BookKeeping2.Tests.Integration.Persistence|FullyQualifiedName~BookKeeping2.Tests.Integration.Pages|FullyQualifiedName~BookKeeping2.Tests.Integration.Performance"` -> 40 passed, 0 failed.
- Playwright browser tests passed: `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~BookKeeping2.Tests.Integration.Browser"` -> 22 passed, 0 failed. Chrome/Edge executable was available; no browser blocker was encountered.
- Full solution build passed: `dotnet build BookKeeping2.slnx` -> 0 warnings, 0 errors.
- Full automated test suite passed after final browser flow and documentation updates: `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj` -> 140 passed, 0 failed.

Manual/browser checklist status:

- The primary browser flow is covered by `MultiCurrencyBrowserTests.Primary_multi_currency_flow_creates_account_transaction_and_home_summary_under_performance_targets`: create a USD account, create a USD expense with that account, verify the homepage shows an independent USD bucket and USD account balance, and assert homepage FCP < 1.5 seconds and LCP < 2.5 seconds.
- CSV browser pages are covered at 390px and 1280px with visible seven-column currency contract text and no horizontal overflow.
- Account, transaction and budget currency controls are covered at 390px and 1280px with no horizontal overflow.
