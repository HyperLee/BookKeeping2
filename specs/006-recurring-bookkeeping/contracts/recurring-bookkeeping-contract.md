# Contract: 定期交易與轉帳確認

此 contract 定義使用者可見 UI、Razor Page handler、服務邊界、persistence 與既有正式紀錄整合。它不是外部 HTTP API；本專案仍是單一 Razor Pages web app。

## UI Contract

### 首頁待確認摘要

**Surface**: `Pages/Index`

- PageModel must materialize due occurrences up to Asia/Taipei today before computing the summary.
- When pending occurrences exist, homepage shows:
  - pending count
  - earliest due date
  - entry link text equivalent to `處理定期項目`
- When no pending occurrences exist, homepage must not show overdue or warning wording.
- Summary must not expose full sensitive notes.
- Existing theme and language controls must remain functional; recurring summary must be readable in light/dark mode and at mobile/desktop widths.

### 定期規則管理

**Surface**: `GET /Recurring`, `GET /Recurring/Create`, `POST /Recurring/Create`, `GET /Recurring/Edit/{id}`, `POST /Recurring/Edit/{id}`, `GET /Recurring/Delete/{id}`, `POST /Recurring/Delete/{id}`

Rule list must show:

- rule name
- record kind: `收入`、`支出`、`轉帳`
- frequency: `每週`、`每月`、`每年`
- default currency amount
- category/account or transfer direction
- next due date when determinable
- active/inactive state
- actions for edit, disable and delete

Create/edit fields:

| UI Label | Input |
|----------|-------|
| `規則名稱` | `Name` |
| `紀錄類型` | `RecordKind` |
| `週期` | `Frequency` |
| `開始日` | `StartDate` |
| `結束日` | `EndDate` |
| `幣別` | `Currency` |
| `金額` | `Amount` |
| `分類` | `CategoryId` for income/expense |
| `帳戶` | `AccountId` for income/expense |
| `轉出帳戶` | `FromAccountId` for transfer |
| `轉入帳戶` | `ToAccountId` for transfer |
| `備註` | `Note` |

Rules:

- POST handlers must require anti-forgery token.
- Validation errors re-render the form with selected values preserved.
- Record kind determines visible/required fields; server-side validation remains authoritative.
- Editing a rule changes future materialization only; already materialized pending occurrences must not be silently changed.
- Delete is soft delete. It must stop new materialization and keep history/formal records.
- Disable stops new materialization but does not silently confirm, skip, or delete existing pending occurrences.

### 待確認清單

**Surface**: `GET /Recurring/Pending`

- PageModel must materialize due occurrences up to Asia/Taipei today before listing.
- List includes all `Pending` occurrences with `ScheduledDate <= today`.
- Each row must show scheduled date, rule name, record kind, currency amount, category/account or transfer direction, and safe note preview when present.
- Rows must be independent; confirming or skipping one row must not change other pending rows.
- Sort order: scheduled date ascending, then rule display order/name, then occurrence id.
- Mobile and desktop layouts must keep date, amount, direction/category, status and action buttons readable without overlap.

### 確認收入/支出期別

**Surface**: `GET /Recurring/ConfirmTransaction/{id}`, `POST /Recurring/ConfirmTransaction/{id}`

Fields:

| UI Label | Input |
|----------|-------|
| `交易日期` | `TransactionDate` |
| `幣別` | `Currency` |
| `金額` | `Amount` |
| `分類` | `CategoryId` |
| `帳戶` | `AccountId` |
| `備註` | `Note` |

Rules:

- GET loads pending occurrence snapshot as defaults.
- POST must require anti-forgery token.
- User may adjust date, currency, amount, category, account and note for this occurrence.
- Confirmation must create one formal `Transaction` and mark the occurrence confirmed in one transaction.
- Adjusted transaction date cannot be later than Asia/Taipei today.
- Formal transaction must use existing transaction validation, audit, reports, budgets, account balance and CSV behavior.
- Parent recurring rule must not be updated by confirmation adjustments.
- Repeated confirmation submit must not create a second transaction.

### 確認轉帳期別

**Surface**: `GET /Recurring/ConfirmTransfer/{id}`, `POST /Recurring/ConfirmTransfer/{id}`

Fields:

| UI Label | Input |
|----------|-------|
| `轉帳日期` | `TransferDate` |
| `幣別` | `Currency` |
| `金額` | `Amount` |
| `轉出帳戶` | `FromAccountId` |
| `轉入帳戶` | `ToAccountId` |
| `備註` | `Note` |

Rules:

- GET loads pending occurrence snapshot as defaults.
- POST must require anti-forgery token.
- User may adjust date, currency, amount, from account, to account and note for this occurrence.
- Confirmation must create one formal `AccountTransfer` and mark the occurrence confirmed in one transaction.
- Adjusted transfer date cannot be later than Asia/Taipei today.
- Formal transfer must use existing transfer validation, audit, account balance, timeline and transfer CSV behavior.
- Transfer must remain excluded from reports, category statistics, trend charts and budget usage.
- Repeated confirmation submit must not create a second transfer.

### 略過期別

**Surface**: `GET /Recurring/Skip/{id}`, `POST /Recurring/Skip/{id}`

Rules:

- Confirmation page shows scheduled date, rule name, kind and safe summary.
- POST must require anti-forgery token.
- Skip changes only this occurrence to `Skipped`.
- Skip must not create formal transaction or transfer.
- Skipped occurrence must not reappear in pending list after refresh.
- Skipping one occurrence must not stop future occurrence materialization for the parent rule.

## Service Contract

### `IRecurringRuleService`

Expected operations:

```csharp
Task<RecurringRuleListViewModel> ListAsync(CancellationToken cancellationToken = default);
Task<RecurringRuleInputModel?> GetForEditAsync(long id, CancellationToken cancellationToken = default);
Task<RecurringFormOptionsViewModel> GetFormOptionsAsync(RecurringRecordKind? kind = null, string? currency = null, CancellationToken cancellationToken = default);
Task<RecurringResult> CreateAsync(RecurringRuleInputModel input, CancellationToken cancellationToken = default);
Task<RecurringResult> UpdateAsync(long id, RecurringRuleInputModel input, CancellationToken cancellationToken = default);
Task<RecurringResult> DeactivateAsync(long id, CancellationToken cancellationToken = default);
Task<RecurringResult> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
```

Result semantics:

- `Succeeded == true` means the requested rule action is complete.
- Validation errors are keyed to input property names when possible.
- Service never creates formal transactions or transfers.
- Service records masked audit events for create/update/deactivate/delete.

### `IRecurringOccurrenceService`

Expected operations:

```csharp
Task<int> MaterializeDueAsync(DateOnly? throughDate = null, CancellationToken cancellationToken = default);
Task<RecurringPendingListViewModel> ListPendingAsync(CancellationToken cancellationToken = default);
Task<RecurringOccurrenceConfirmationViewModel?> GetForConfirmationAsync(long id, CancellationToken cancellationToken = default);
Task<RecurringResult> ConfirmTransactionAsync(long id, RecurringTransactionConfirmationInputModel input, CancellationToken cancellationToken = default);
Task<RecurringResult> ConfirmTransferAsync(long id, RecurringTransferConfirmationInputModel input, CancellationToken cancellationToken = default);
Task<RecurringResult> SkipAsync(long id, CancellationToken cancellationToken = default);
Task<RecurringHomeSummaryViewModel> GetHomeSummaryAsync(CancellationToken cancellationToken = default);
```

Rules:

- `MaterializeDueAsync` uses `ITaipeiDateService.Today` by default and must not create future occurrences.
- Materialization must be idempotent using unique `{ RecurringRuleId, ScheduledDate }`.
- Confirmation must be idempotent using formal record `RecurringOccurrenceId` unique indexes.
- Confirmation of income/expense creates `Transaction`; confirmation of transfer creates `AccountTransfer`.
- Skip and confirm are terminal in V1.
- Service never performs currency conversion.

### `RecurrenceDateCalculator`

Expected operations:

```csharp
IReadOnlyList<DateOnly> GetDueDates(RecurringRule rule, DateOnly throughDate, IReadOnlySet<DateOnly> existingDates);
DateOnly? GetNextDueDate(RecurringRule rule, DateOnly afterDate);
```

Rules:

- Weekly uses 7-day increments from `StartDate`.
- Monthly uses `AnchorDay`; if target month lacks the day, use the month last day.
- Yearly uses `AnchorMonth` + `AnchorDay`; if target year/month lacks the day, use that month last day.
- End date is inclusive: occurrence is generated only when `ScheduledDate <= EndDate`.
- Existing dates are skipped so materialization is idempotent.

## Persistence Contract

### `RecurringRules` table

- Required: `Id`, `Name`, `RecordKind`, `Frequency`, `StartDate`, `AnchorDay`, `Currency`, `AmountMinorUnits`, `IsActive`, `IsDeleted`, `CreatedAtUtc`, `UpdatedAtUtc`, `LastChangeSummary`.
- Optional: `EndDate`, `AnchorMonth`, type-specific FK fields, `Note`, `DeactivatedAtUtc`, `DeletedAtUtc`.
- `Name` max 100.
- `Currency` max 3.
- `Note` and `LastChangeSummary` max 500.
- FKs to `Categories` and `Accounts` use restrict delete behavior.
- Indexes support active rule lookup and management list sorting.

### `RecurringOccurrences` table

- Required: `Id`, `RecurringRuleId`, `ScheduledDate`, `Status`, snapshots for record kind/currency/amount, `MaterializedAtUtc`.
- Optional: snapshot FK/name/note fields, `ConfirmedAtUtc`, `SkippedAtUtc`, `GeneratedTransactionId`, `GeneratedTransferId`, `LastActionSummary`.
- Unique index on `{ RecurringRuleId, ScheduledDate }`.
- Index on `{ Status, ScheduledDate }`.
- Index on `{ RecurringRuleId, Status, ScheduledDate }`.
- Index on `{ Status, CurrencySnapshot, ScheduledDate }`.

### Formal record source links

- `Transactions.RecurringOccurrenceId` nullable FK with unique index.
- `AccountTransfers.RecurringOccurrenceId` nullable FK with unique index.
- Existing rows remain null.
- Deleting or soft deleting formal records does not delete occurrences.

## Formal Data Boundary Contract

- Pending occurrences do not affect account balances, transaction timeline, monthly reports, category statistics, trend charts, budgets or CSV export.
- Confirmed income/expense occurrences affect all existing transaction surfaces exactly like manual transactions.
- Confirmed transfer occurrences affect transfer timeline/account balances/transfer CSV exactly like manual transfers and remain excluded from income/expense reports and budgets.
- Skipped occurrences have no formal financial effect.

## Security And Audit Contract

- All POST handlers require anti-forgery token.
- Server-side validation is required for every rule and confirmation field.
- Razor default HTML encoding remains baseline; no raw user-controlled HTML.
- Rule names and notes are sanitized before persistence.
- CSV formula injection protection remains on formal CSV export.
- No raw sensitive notes or unmasked financial details in logs, audit summaries, validation exception details or telemetry.
- No cookies, sessions, localStorage, browser locale, OS locale or third-party calls may determine currency or due state.
