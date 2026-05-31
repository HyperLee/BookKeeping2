# Data Model: 定期交易與轉帳確認

本功能新增定期規則與期別處理模型。正式收入/支出仍使用既有 `Transaction`，正式同幣別轉帳仍使用既有 `AccountTransfer`。所有使用者面向文件與 UI 文字維持繁體中文；程式識別名稱可使用英文。

## Shared Rules

- 所有日期使用 `DateOnly` 表示 Asia/Taipei 本地日期。
- 規則開始日可晚於今日；待確認期別與確認後正式紀錄日期不得晚於 `ITaipeiDateService.Today`。
- 金額 domain/view model 使用 `decimal`；SQLite persistence 使用 `long` minor units 與 `MoneyMinorUnitConverter`。
- 幣別使用 `SupportedCurrency` allow-list：`TWD`、`USD`、`JPY`、`EUR`、`GBP`。
- 名稱與備註在保存前使用 `TextInputSanitizer`；稽核摘要使用 `AuditLogMaskingPolicy`。
- 狀態變更必須在 EF Core transaction 中完成。

## Entity: RecurringRule

代表使用者設定的定期收入、支出或轉帳模板。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `Name` | string | Required；trim 後不可空白；max 100；sanitized plain text |
| `RecordKind` | `RecurringRecordKind` | `Income`、`Expense`、`Transfer` |
| `Frequency` | `RecurringFrequency` | `Weekly`、`Monthly`、`Yearly` |
| `StartDate` | DateOnly | Required；規則第一期日期，可為未來 |
| `EndDate` | DateOnly? | Optional；若早於下一期日期，不產生該期 |
| `AnchorDay` | int | 由 `StartDate.Day` 建立；monthly/yearly fallback 用 |
| `AnchorMonth` | int? | Yearly 使用 `StartDate.Month`；weekly/monthly 可為 null |
| `Currency` | string | Required supported currency code；normalized uppercase |
| `Amount` | decimal | Not mapped；> 0，<= 999,999,999.99，最多 2 位小數 |
| `AmountMinorUnits` | long | Persisted integer minor units |
| `CategoryId` | long? | Income/Expense required；分類存在、未封存且 type 符合 `RecordKind` |
| `Category` | Category? | Optional navigation |
| `AccountId` | long? | Income/Expense required；帳戶存在、未封存且幣別符合 `Currency` |
| `Account` | Account? | Optional navigation |
| `FromAccountId` | long? | Transfer required；轉出帳戶存在、未封存、幣別符合 `Currency` |
| `FromAccount` | Account? | Optional navigation |
| `ToAccountId` | long? | Transfer required；轉入帳戶存在、未封存、幣別符合 `Currency`，不得等於 `FromAccountId` |
| `ToAccount` | Account? | Optional navigation |
| `Note` | string? | Sanitized plain text；max 500 |
| `IsActive` | bool | Active rules materialize new due occurrences |
| `IsDeleted` | bool | Soft delete marker；deleted rules hidden from normal management list |
| `DeactivatedAtUtc` | DateTimeOffset? | Set when rule is disabled |
| `DeletedAtUtc` | DateTimeOffset? | Set when rule is soft deleted |
| `CreatedAtUtc` | DateTimeOffset | UTC timestamp |
| `UpdatedAtUtc` | DateTimeOffset | UTC timestamp |
| `LastChangeSummary` | string | Masked latest rule change summary；max 500 |

**Relationships**:

- One `RecurringRule` has many `RecurringOccurrence`.
- Rule account/category foreign keys use restrict delete behavior.
- Rule soft delete does not delete occurrences, transactions or transfers.

**Validation**:

- `EndDate`, when present, must be on or after `StartDate`.
- `Income` and `Expense` require `CategoryId` and `AccountId`; transfer account fields must be null or ignored.
- `Transfer` requires `FromAccountId` and `ToAccountId`; category/account fields for income/expense must be null or ignored.
- Income category must have `TransactionType.Income`; expense category must have `TransactionType.Expense`.
- Account/category choices must be active at rule create/update.
- For transfer, from/to accounts must be different and same currency as `Currency`; negative resulting balance remains allowed at confirmation.

**Indexes**:

- `{ IsDeleted, IsActive, StartDate }` for materialization candidate lookup.
- `{ IsDeleted, Name }` for management list search/sort if needed.
- `{ RecordKind, Frequency }` for diagnostics and tests.

**State transitions**:

```text
Draft input -> Active rule
Active rule -> Updated active rule
Active rule -> Inactive rule
Active/Inactive rule -> SoftDeleted rule
```

停用或刪除後不 materialize 新期別；已存在 pending occurrence 保留，讓使用者逐期確認或略過。

## Entity: RecurringOccurrence

代表某一條定期規則在特定日期的一期待處理事項。Occurrence 在到期日達到或晚於今日時 materialize，並保存當時的規則快照。

| Field | Type | Rules |
|-------|------|-------|
| `Id` | long | Primary key |
| `RecurringRuleId` | long | Required FK |
| `RecurringRule` | RecurringRule | Required navigation |
| `ScheduledDate` | DateOnly | Asia/Taipei local date；materialized 時不得晚於 today |
| `Status` | `RecurringOccurrenceStatus` | `Pending`、`Confirmed`、`Skipped` |
| `RecordKindSnapshot` | `RecurringRecordKind` | Copied from rule |
| `CurrencySnapshot` | string | Copied normalized currency |
| `Amount` | decimal | Not mapped；snapshot amount |
| `AmountMinorUnitsSnapshot` | long | Persisted snapshot amount |
| `CategoryIdSnapshot` | long? | Income/Expense snapshot |
| `CategoryNameSnapshot` | string? | Safe display snapshot；max 100 |
| `AccountIdSnapshot` | long? | Income/Expense snapshot |
| `AccountNameSnapshot` | string? | Safe display snapshot；max 100 |
| `FromAccountIdSnapshot` | long? | Transfer snapshot |
| `FromAccountNameSnapshot` | string? | Safe display snapshot；max 100 |
| `ToAccountIdSnapshot` | long? | Transfer snapshot |
| `ToAccountNameSnapshot` | string? | Safe display snapshot；max 100 |
| `NoteSnapshot` | string? | Sanitized snapshot；max 500 |
| `MaterializedAtUtc` | DateTimeOffset | UTC timestamp |
| `ConfirmedAtUtc` | DateTimeOffset? | Set when confirmed |
| `SkippedAtUtc` | DateTimeOffset? | Set when skipped |
| `GeneratedTransactionId` | long? | Set for confirmed income/expense |
| `GeneratedTransaction` | Transaction? | Optional navigation |
| `GeneratedTransferId` | long? | Set for confirmed transfer |
| `GeneratedTransfer` | AccountTransfer? | Optional navigation |
| `LastActionSummary` | string? | Masked confirm/skip summary；max 500 |

**Relationships**:

- One occurrence belongs to one rule.
- Confirmed income/expense occurrence links to exactly one `Transaction`.
- Confirmed transfer occurrence links to exactly one `AccountTransfer`.
- Skipped occurrence links to no formal record.

**Validation**:

- Unique `{ RecurringRuleId, ScheduledDate }`; same rule cannot create duplicate same-date occurrence.
- `Pending` occurrence has no generated formal record.
- `Confirmed` occurrence must have exactly one generated transaction or transfer depending on `RecordKindSnapshot`.
- `Skipped` occurrence must have no generated formal record.
- Confirmation must re-run formal transaction/transfer validation using adjusted input, not blindly trust the snapshot.
- Adjusted confirmation date cannot be future.

**Indexes**:

- Unique `{ RecurringRuleId, ScheduledDate }`.
- `{ Status, ScheduledDate }` for pending list and homepage count.
- `{ RecurringRuleId, Status, ScheduledDate }` for rule detail/history.
- `{ Status, CurrencySnapshot, ScheduledDate }` for future filtering and performance tests.

**State transitions**:

```text
Materialized -> Pending
Pending -> Confirmed
Pending -> Skipped
Confirmed/Skipped -> terminal in V1
```

V1 不支援將 confirmed/skipped 還原為 pending。若正式交易或轉帳後續被軟刪除，occurrence 仍保持 confirmed，以保留「該期已處理」事實。

## Entity: Transaction

既有正式收入/支出 entity；新增定期來源連結。

| Field | Type | Rules |
|-------|------|-------|
| `RecurringOccurrenceId` | long? | Null for manual/CSV transactions；set when created from confirmed recurring occurrence |
| `RecurringOccurrence` | RecurringOccurrence? | Optional navigation |

**Rules**:

- Unique index on nullable `RecurringOccurrenceId`; SQLite permits multiple nulls, but a non-null occurrence can create only one transaction.
- Existing transaction validation, amount rules, category/account/currency matching, soft delete, budget warning and audit behavior still apply.
- Recurring-created transaction appears in reports, budgets, timeline and CSV exactly like a manual formal transaction after confirmation.
- Pending occurrence never appears as transaction.

## Entity: AccountTransfer

既有正式同幣別轉帳 entity；新增定期來源連結。

| Field | Type | Rules |
|-------|------|-------|
| `RecurringOccurrenceId` | long? | Null for manual/CSV transfers；set when created from confirmed recurring occurrence |
| `RecurringOccurrence` | RecurringOccurrence? | Optional navigation |

**Rules**:

- Unique index on nullable `RecurringOccurrenceId`; a non-null occurrence can create only one transfer.
- Existing transfer validation, same-currency rules, negative balance allowance, soft delete and audit behavior still apply.
- Recurring-created transfer affects account balances and timeline after confirmation, but remains excluded from income, expense, reports, category statistics, trend charts and budget usage.
- Transfer CSV export includes confirmed formal transfer rows through existing transfer export contract; pending occurrence is not exported.

## Entity: RecurringRuleInputModel

Razor Page input model for create/edit recurring rule forms.

| Field | Type | Rules |
|-------|------|-------|
| `Name` | string? | Required；max 100 |
| `RecordKind` | RecurringRecordKind | Required |
| `Frequency` | RecurringFrequency | Required |
| `StartDate` | DateOnly | Required |
| `EndDate` | DateOnly? | Optional；must be >= `StartDate` |
| `Currency` | string? | Required supported currency |
| `Amount` | decimal | Required money rules |
| `CategoryId` | long? | Required for income/expense |
| `AccountId` | long? | Required for income/expense |
| `FromAccountId` | long? | Required for transfer |
| `ToAccountId` | long? | Required for transfer |
| `Note` | string? | Optional；max 500 after sanitization |

**Validation messages**:

- Missing name: `請輸入定期規則名稱。`
- Missing frequency: `請選擇週期。`
- End before start: `結束日不可早於開始日。`
- Missing category/account: reuse existing Traditional Chinese financial validation messages where possible.
- Transfer same account: `轉出帳戶與轉入帳戶不可相同。`
- Transfer currency mismatch: `轉出帳戶、轉入帳戶與轉帳幣別必須一致。`

## Entity: RecurringConfirmationInputModels

Confirmation input models are separate for transaction and transfer flows because required fields differ.

### RecurringTransactionConfirmationInputModel

| Field | Type | Rules |
|-------|------|-------|
| `OccurrenceId` | long | Required pending occurrence |
| `TransactionDate` | DateOnly | Required；default `ScheduledDate`；not future |
| `Currency` | string? | Required supported currency |
| `Amount` | decimal | Required money rules |
| `CategoryId` | long | Required active matching category |
| `AccountId` | long | Required active same-currency account |
| `Note` | string? | Optional sanitized note |

### RecurringTransferConfirmationInputModel

| Field | Type | Rules |
|-------|------|-------|
| `OccurrenceId` | long | Required pending occurrence |
| `TransferDate` | DateOnly | Required；default `ScheduledDate`；not future |
| `Currency` | string? | Required supported currency |
| `Amount` | decimal | Required money rules |
| `FromAccountId` | long | Required active same-currency account |
| `ToAccountId` | long | Required active same-currency account；must differ |
| `Note` | string? | Optional sanitized note |

**Rules**:

- Adjustments only affect the formal record created for this occurrence.
- Confirmation input must not update the parent recurring rule.
- Repeated submit for an already confirmed occurrence returns success or redirects to the generated formal record without creating another record.
- Submitting a skipped occurrence cannot create a formal record; show an actionable Traditional Chinese message.

## Entity: AuditEvent

既有 audit entity；新增 recurring event types.

| Event | EntityType | Severity | Summary |
|-------|------------|----------|---------|
| `RecurringRuleCreated` | `RecurringRule` | Information | Rule kind, frequency, masked amount/currency, safe name |
| `RecurringRuleUpdated` | `RecurringRule` | Information | Masked updated fields summary |
| `RecurringRuleDeactivated` | `RecurringRule` | Information | Safe rule name and stop-new-occurrences summary |
| `RecurringRuleDeleted` | `RecurringRule` | Warning | Safe rule name and soft-delete summary |
| `RecurringOccurrenceConfirmed` | `RecurringOccurrence` | Information | Scheduled date, formal record kind/id, masked amount |
| `RecurringOccurrenceSkipped` | `RecurringOccurrence` | Information | Scheduled date, rule name, safe skip summary |

**Rules**:

- Audit summaries must not expose full sensitive notes.
- Amounts use `AuditLogMaskingPolicy.MaskAmount`.
- Text uses `AuditLogMaskingPolicy.MaskText`.
- Formal transaction/transfer creation should also preserve existing transaction/transfer audit events.

## Migration Notes

- Add `RecurringRules` table with schedule, type-specific references, soft-delete/status metadata and masked summary fields.
- Add `RecurringOccurrences` table with rule FK, scheduled date, status, snapshots, processed timestamps and generated formal record links.
- Add nullable `RecurringOccurrenceId` to `Transactions` and `AccountTransfers`.
- Add unique indexes on `Transactions.RecurringOccurrenceId` and `AccountTransfers.RecurringOccurrenceId`.
- Add indexes listed under `RecurringRule` and `RecurringOccurrence`.
- Add new audit event enum values and update model snapshot.
- Existing data requires no backfill; all existing transactions and transfers have null `RecurringOccurrenceId`.
