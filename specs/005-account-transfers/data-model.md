# Data Model: 同幣別帳戶轉帳與信用卡繳款

本功能新增獨立轉帳資料模型，並調整帳戶餘額與明細時間線的讀取模型。所有 user-facing 文件與 UI 文字維持繁體中文；程式識別名稱可使用英文。

## Entity: AccountTransfer

代表兩個同幣別帳戶間的資金移動。信用卡繳款也以此 entity 表示。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `TransferDate` | DateOnly | Asia/Taipei local date；不得晚於今日 |
| `Currency` | string | Required 3-char supported currency code；必須等於轉出與轉入帳戶幣別 |
| `Amount` | decimal | Not mapped；使用 `MoneyMinorUnitConverter`；> 0，<= 999,999,999.99，最多 2 位小數 |
| `AmountMinorUnits` | long | Persisted integer minor units |
| `FromAccountId` | long | Required；轉出帳戶必須存在且未封存 |
| `FromAccount` | Account | Required navigation |
| `ToAccountId` | long | Required；轉入帳戶必須存在且未封存 |
| `ToAccount` | Account | Required navigation |
| `Note` | string? | Sanitized plain text，max 500 |
| `CreatedAtUtc` | DateTimeOffset | UTC timestamp |
| `UpdatedAtUtc` | DateTimeOffset | UTC timestamp |
| `IsDeleted` | bool | Soft delete marker |
| `DeletedAtUtc` | DateTimeOffset? | Soft delete timestamp |
| `DeletionSummary` | string? | Masked summary including transfer direction and currency |
| `LastChangeSummary` | string | Masked latest change summary |

**Relationships**:

- One transfer references one `FromAccount`.
- One transfer references one `ToAccount`.
- Both foreign keys use restrict delete behavior.
- `Account` navigation collections are optional implementation detail; queries may use explicit joins instead.

**Validation**:

- `FromAccountId` and `ToAccountId` must be different.
- Both accounts must exist and `IsArchived == false` for create/update.
- Both account currencies must equal normalized `Currency`.
- Negative resulting balance is allowed; no insufficient-funds rejection.
- Transfer cannot reference category, budget or transaction type.
- Note is sanitized before persistence.

**Indexes**:

- `{ IsDeleted, TransferDate }` for default timeline sorting/filtering.
- `{ IsDeleted, Currency, TransferDate }` for currency/date filtering.
- `{ IsDeleted, FromAccountId, TransferDate }` for outgoing account filter and balance totals.
- `{ IsDeleted, ToAccountId, TransferDate }` for incoming account filter and balance totals.
- Optional `{ IsDeleted, Currency, FromAccountId, ToAccountId, TransferDate }` if performance tests show compound account/currency filtering needs it.

**State transitions**:

- Create: validates all rules, checks short-window duplicate form resubmit, saves transfer and audit event atomically.
- Edit: may change date, currency, amount, from account, to account and note; validates all rules against updated values and saves audit event atomically.
- Soft delete: sets `IsDeleted`, `DeletedAtUtc`, `DeletionSummary`, saves warning audit event atomically, and excludes the transfer from normal list, account balances and export.

## Entity: Account

既有帳戶 entity；本功能不改變帳戶幣別規則，但帳戶餘額計算納入轉帳。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Existing primary key |
| `Currency` | string | Fixed account currency，create 後不可修改 |
| `OpeningBalanceMinorUnits` | long | Existing persisted opening balance |
| `IsArchived` | bool | Archived accounts are hidden from new transfer selections |

**Balance calculation**:

```text
CurrentBalance =
  OpeningBalance
  + same-account same-currency income transactions
  - same-account same-currency expense transactions
  - non-deleted outgoing transfers
  + non-deleted incoming transfers
```

**Validation**:

- Archived accounts remain visible in historical transfer rows but cannot be selected for new or edited transfers.
- Account balances must not combine different currencies.

## Entity: TransactionTimelineItem

明細時間線讀取模型，用於共同顯示收入、支出與轉帳；不是資料庫表。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Source record id |
| `RecordKind` | enum/string | `Income`、`Expense`、`Transfer` |
| `RecordDate` | DateOnly | Transaction date or transfer date |
| `Currency` | string | Displayed with amount |
| `Amount` | decimal | Positive display amount |
| `PrimaryText` | string | Transaction category/account text or transfer direction |
| `FromAccountName` | string? | Required for transfer rows |
| `ToAccountName` | string? | Required for transfer rows |
| `CategoryName` | string? | Required for income/expense rows, null for transfers |
| `Note` | string? | Sanitized note |
| `EditPage` | string | Transfer rows link to transfer edit; transactions link to transaction edit |
| `DeletePage` | string | Transfer rows link to transfer delete; transactions link to transaction delete |

**Filtering**:

- Date range applies to both transactions and transfers.
- Currency applies to both.
- Account filter applies to `Transaction.AccountId` or transfer `FromAccountId`/`ToAccountId`.
- Category filter applies only to income/expense transactions; when set, transfer rows are excluded.
- Keyword matches note, category name, transaction account name, from account name or to account name.
- Min/max amount applies to both transaction amount and transfer amount.

**Sorting**:

- Default sort: `RecordDate` descending, then stable source kind/id descending.
- Pagination must happen after filtering and sorting.

## Entity: AccountTransferInputModel

Razor Page input model for create/edit transfer forms.

| Field | Type | Rules |
|-------|------|-------|
| `TransferDate` | DateOnly | Required；不得晚於今日 |
| `Currency` | string? | Required；normalized through `SupportedCurrency` |
| `Amount` | decimal | Required；existing money rules |
| `FromAccountId` | long | Required；active account |
| `ToAccountId` | long | Required；active account；must differ from from account |
| `Note` | string? | Optional；max 500 after sanitization |

**Validation messages**:

- Missing date: `請選擇轉帳日期。`
- Future date: reuse `FinancialValidationMessages.DateCannotBeFuture`.
- Missing amount or invalid amount: reuse existing money converter messages where possible.
- Same account: `轉出帳戶與轉入帳戶不可相同。`
- Currency mismatch: `轉出帳戶、轉入帳戶與轉帳幣別必須一致。`
- Missing account: `請選擇轉出帳戶。` / `請選擇轉入帳戶。`

## Entity: TransferCsvRow

轉帳 CSV 匯入與匯出的資料列。

| Field | Type | Rules |
|-------|------|-------|
| `Date` | string | `yyyy-MM-dd` |
| `Currency` | string | Required supported currency code |
| `Amount` | string | Invariant decimal text |
| `FromAccount` | string | Existing active account name for import；formula-protected on export |
| `ToAccount` | string | Existing active account name for import；formula-protected on export |
| `Note` | string? | Sanitized on import；formula-protected on export |

**CSV contract**:

- Header must be exactly `日期,幣別,金額,轉出帳戶,轉入帳戶,備註`.
- Existing transaction CSV headers must not be parsed as transfer CSV.
- Transfer CSV headers must not be parsed as transaction CSV.
- Empty files, wrong header order, wrong field count, files larger than the existing upload limit and rows beyond the existing row limit produce actionable errors.
- Valid rows create transfers; invalid rows do not create transfers and are included in row-level error summaries.

## Entity: CsvImportBatch / CsvImportError

既有 CSV import audit persistence 可重用於轉帳匯入。

**Rules**:

- Batch summary must identify transfer CSV import separately from transaction CSV import.
- Row errors must include row number, field name, reason and safe raw value preview.
- Valid transfer rows and import batch/errors are committed atomically.
- Raw sensitive note text must not be written to audit summaries.

## Entity: AuditEvent

既有稽核 entity；新增轉帳相關 event types。

| Event | EntityType | Severity | Summary |
|-------|------------|----------|---------|
| `TransferCreated` | `AccountTransfer` | Information | Masked amount, currency, from/to account display names, masked note |
| `TransferUpdated` | `AccountTransfer` | Information | Masked updated amount/direction/note |
| `TransferDeleted` | `AccountTransfer` | Warning | Masked deleted transfer summary |
| `TransferCsvImported` | `CsvImportBatch` | Information/Warning | File summary with succeeded/failed counts |
| `TransferCsvExported` | `AccountTransfer` or `CsvExport` | Information | Export row count and date range only |

**Rules**:

- Audit summaries must not expose full sensitive notes.
- Amounts use `AuditLogMaskingPolicy.MaskAmount`.
- Text uses `AuditLogMaskingPolicy.MaskText`.

## Migration Notes

- Add `AccountTransfers` table with required fields, two account foreign keys and soft-delete metadata.
- Add indexes listed under `AccountTransfer`.
- Add `DbSet<AccountTransfer>` to `AppDbContext`.
- Update model snapshot.
- Existing data requires no transfer backfill.
- No changes to existing transaction, budget or account currency columns.
