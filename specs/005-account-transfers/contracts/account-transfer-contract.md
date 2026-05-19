# Contract: 同幣別帳戶轉帳

此 contract 定義使用者可見 UI、Razor Page handler、CSV 與服務邊界。它不是外部 HTTP API；本專案仍是單一 Razor Pages web app。

## UI Contract

### 交易明細時間線

**Surface**: `Pages/Transactions/Index`

- Page must provide a visible `新增轉帳` entry that routes to `/Transfers/Create`.
- Timeline must show income, expense and transfer rows sorted together by date.
- Transfer rows must display:
  - label text `轉帳`
  - direction text `{轉出帳戶} -> {轉入帳戶}`
  - amount text `{Currency} {Amount:N2}`
  - note when present
  - edit/delete links for the transfer record
- Transfer rows must not display category text.
- Account filter includes transfer rows where the selected account is either from account or to account.
- Category filter excludes transfer rows.
- Currency, date, keyword and amount filters apply to transfer rows.
- Mobile and desktop layouts must keep labels, amount, direction, buttons and validation messages readable without overlap.

### 新增轉帳

**Surface**: `GET /Transfers/Create`, `POST /Transfers/Create`

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

- POST must require anti-forgery token.
- Successful create redirects to transaction timeline or transfer details/edit target consistent with existing transaction pages.
- Validation errors re-render the form with selected values preserved.
- If same content is submitted repeatedly within the configured short window, the service returns success without creating a duplicate transfer.

### 編輯轉帳

**Surface**: `GET /Transfers/Edit/{id}`, `POST /Transfers/Edit/{id}`

Rules:

- Editable fields: date, currency, amount, from account, to account and note.
- POST must require anti-forgery token.
- Missing or deleted transfer returns NotFound or existing project equivalent.
- Save validates all create rules against updated values.
- Successful save records a masked audit event and redirects back to the timeline.

### 刪除轉帳

**Surface**: `GET /Transfers/Delete/{id}`, `POST /Transfers/Delete/{id}`

Rules:

- Confirmation page shows date, currency amount and direction.
- POST must require anti-forgery token.
- Delete is soft delete only.
- Deleted transfers are excluded from normal timeline, account balances and transfer CSV export.
- Successful delete records a warning-level masked audit event.

## Service Contract

### `IAccountTransferService`

Expected operations:

```csharp
Task<AccountTransferInputModel?> GetForEditAsync(long id, CancellationToken cancellationToken = default);
Task<AccountTransferFormOptionsViewModel> GetFormOptionsAsync(string? currency = null, CancellationToken cancellationToken = default);
Task<AccountTransferResult> CreateAsync(AccountTransferInputModel input, CancellationToken cancellationToken = default);
Task<AccountTransferResult> UpdateAsync(long id, AccountTransferInputModel input, CancellationToken cancellationToken = default);
Task<AccountTransferResult> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
```

Result semantics:

- `Succeeded == true` means the requested action is complete.
- Validation errors are keyed to input property names when possible.
- Duplicate rapid resubmit returns success without inserting a second transfer.
- Service never performs currency conversion.
- Service never updates reports, budgets or categories.

### Account balance contract

`IAccountService.GetBalanceSummariesAsync` must include transfer effects:

- Outgoing transfer decreases the from account balance.
- Incoming transfer increases the to account balance.
- Deleted transfers are ignored.
- Different currencies are never combined.
- Negative balances are allowed.

### Timeline query contract

`ITransactionQueryService.SearchAsync` or its successor must return a paged timeline containing both transaction and transfer items.

Rules:

- No N+1 account/category/transfer queries.
- Transfer rows are excluded when category filter is set.
- Account filter matches either transfer side.
- Reports and budgets must continue querying `Transactions` only, unless explicitly projecting transfer rows for display without totals.

## Transfer CSV Contract

### Header

The transfer CSV header must be exactly:

```csv
日期,幣別,金額,轉出帳戶,轉入帳戶,備註
```

Transaction CSV remains exactly:

```csv
日期,類型,幣別,金額,分類,帳戶,備註
```

Legacy transaction CSV remains accepted only by the transaction import path:

```csv
日期,類型,金額,分類,帳戶,備註
```

### Import rows

Rules:

- Date must be `yyyy-MM-dd` and not later than Asia/Taipei today.
- Currency must be one of `TWD`, `USD`, `JPY`, `EUR`, `GBP`.
- Amount uses invariant decimal text and existing money limits.
- From account and to account must resolve by normalized account name.
- Both accounts must be active, different and same currency as the row.
- Valid rows create transfers.
- Invalid rows do not create transfers and must be reported with row number, field and reason.
- Mixed valid/invalid files commit valid transfers and persist row-level error summary.
- Transaction CSV headers must not be accepted by transfer import.

### Export rows

Rules:

- Export only non-deleted transfers.
- Header order must match the transfer header exactly.
- Rows are ordered by transfer date ascending, then id ascending.
- Account names and notes must be protected against spreadsheet formula injection.
- Export file name should clearly identify transfers, for example `transfers-yyyyMMdd-HHmmss.csv`.
- Export audit summary records row count and filter range, not raw note content.

## Persistence Contract

`AccountTransfers` table:

- `Id` primary key.
- `TransferDate`, `Currency`, `AmountMinorUnits`, `FromAccountId`, `ToAccountId`, `CreatedAtUtc`, `UpdatedAtUtc`, `IsDeleted`, `LastChangeSummary` are required.
- `Note`, `DeletedAtUtc`, `DeletionSummary` are optional.
- `Currency` max length 3 and required.
- `Note`, `DeletionSummary`, `LastChangeSummary` max length 500.
- `FromAccountId` and `ToAccountId` foreign keys reference `Accounts.Id` with restrict delete behavior.
- No unique index on transfer content, because same-content independent transfers are allowed.
