# 資料模型: Open BookKeeping

**日期**: 2026-05-11  
**範圍**: V1 單使用者、單帳本、TWD、Asia/Taipei 日期

## 模型原則

- 金額在 C# 領域模型與服務 API 一律使用 `decimal`。
- SQLite 儲存金額使用 `long` minor units，輸入最多 2 位小數，轉換失敗或超出範圍時拒絕。
- 實際交易日期使用 `DateOnly`，不得晚於 Asia/Taipei 本地今日。
- 所有狀態變更保存 `CreatedAtUtc`、`UpdatedAtUtc` 或等效時間戳。
- 一般查詢排除軟刪除交易與封存項目；報表同樣排除軟刪除交易。
- 正規化名稱為 trim 後忽略大小寫的比較值，建議儲存 uppercase invariant 形式。
- V1 採最後完成儲存為準；保留更新摘要，不實作細緻 merge。

## Entity: Transaction

代表一筆收入或支出。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。若使用 SQLite integer key，對外 route 不暴露敏感資訊。 |
| `TransactionDate` | `DateOnly` | 是 | Asia/Taipei 本地日期，不得晚於今日。 |
| `Type` | `TransactionType` | 是 | `Income` 或 `Expense`。 |
| `Amount` | `decimal` | 是 | 領域/API 欄位，必須 > 0，最多 2 位小數。 |
| `AmountMinorUnits` | `long` | 是 | SQLite 儲存欄位，`Amount * 100`。 |
| `CategoryId` | FK | 是 | 必須引用同類型分類。 |
| `AccountId` | FK | 是 | 必須引用有效帳戶。 |
| `Note` | `string?` | 否 | 最大長度由實作任務定義，輸入需伺服器端清理或拒絕注入風險。 |
| `CreatedAtUtc` | `DateTimeOffset` | 是 | 建立時間。 |
| `UpdatedAtUtc` | `DateTimeOffset` | 是 | 最後更新時間。 |
| `IsDeleted` | `bool` | 是 | 軟刪除旗標。 |
| `DeletedAtUtc` | `DateTimeOffset?` | 否 | 軟刪除時間。 |
| `DeletionSummary` | `string?` | 否 | 遮罩稽核摘要，不含完整備註。 |
| `LastChangeSummary` | `string` | 是 | 新增/編輯摘要，避免敏感明文。 |

**關聯**

- `Transaction` belongs to one `Category`
- `Transaction` belongs to one `Account`
- `Transaction` has many `AuditEvent`

**索引**

- `(IsDeleted, TransactionDate)`
- `(IsDeleted, CategoryId, TransactionDate)`
- `(IsDeleted, AccountId, TransactionDate)`
- `(IsDeleted, AmountMinorUnits)`

**狀態轉換**

```text
Draft input -> Validated -> Persisted -> Updated
Persisted/Updated -> SoftDeleted
SoftDeleted -> hidden from normal list/report
```

## Entity: Category

代表收入或支出分類。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。 |
| `Name` | `string` | 是 | trim 後不可空白。 |
| `NormalizedName` | `string` | 是 | 同一 `Type` 內唯一。 |
| `Type` | `TransactionType` | 是 | `Income` 或 `Expense`。 |
| `IconKey` | `string` | 是 | 圖示或辨識標記，限定允許清單或安全文字。 |
| `DisplayOrder` | `int` | 是 | 列表排序。 |
| `IsDefault` | `bool` | 是 | 預設分類不可破壞。 |
| `IsArchived` | `bool` | 是 | 已被交易引用時以封存取代刪除。 |
| `CreatedAtUtc` | `DateTimeOffset` | 是 | 建立時間。 |
| `UpdatedAtUtc` | `DateTimeOffset` | 是 | 更新時間。 |

**預設支出分類**

餐飲、交通、娛樂、購物、居住、醫療、教育、其他

**預設收入分類**

薪資、獎金、投資收益、其他收入

**關聯與刪除規則**

- `Category` has many `Transaction`
- `Category` has many `Budget` when `Type = Expense`
- 被交易引用時不得刪除；可封存避免新交易選取。
- 未被引用且非預設分類可刪除。

## Entity: Account

代表現金、銀行、信用卡或電子支付等資金來源。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。 |
| `Name` | `string` | 是 | trim 後不可空白。 |
| `NormalizedName` | `string` | 是 | 全域唯一。 |
| `Type` | `AccountType` | 是 | `Cash`、`Bank`、`CreditCard`、`EWallet`、`Other`。 |
| `IconKey` | `string` | 是 | 圖示或辨識標記。 |
| `OpeningBalance` | `decimal` | 是 | 領域/API 欄位，可為 0 或正負值，最多 2 位小數。 |
| `OpeningBalanceMinorUnits` | `long` | 是 | SQLite 儲存欄位。 |
| `Currency` | `string` | 是 | 固定 `TWD`。 |
| `DisplayOrder` | `int` | 是 | 首頁與設定排序。 |
| `IsArchived` | `bool` | 是 | 已被交易引用時不得硬刪除。 |
| `CreatedAtUtc` | `DateTimeOffset` | 是 | 建立時間。 |
| `UpdatedAtUtc` | `DateTimeOffset` | 是 | 更新時間。 |

**餘額計算**

```text
CurrentBalance =
  OpeningBalance
  + sum(Income transactions for account)
  - sum(Expense transactions for account)
```

一般餘額排除 `IsDeleted = true` 的交易。

## Entity: Budget

代表特定支出分類在某年月的預算。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。 |
| `CategoryId` | FK | 是 | 必須引用 `Expense` 分類。 |
| `BudgetMonth` | `YearMonth` 或 `DateOnly` | 是 | 使用該月第一天表示月份，Asia/Taipei 月份。 |
| `Amount` | `decimal` | 是 | > 0，最多 2 位小數。 |
| `AmountMinorUnits` | `long` | 是 | SQLite 儲存欄位。 |
| `CreatedAtUtc` | `DateTimeOffset` | 是 | 建立時間。 |
| `UpdatedAtUtc` | `DateTimeOffset` | 是 | 更新時間。 |

**唯一性**

- `(CategoryId, BudgetMonth)` 唯一。

**衍生值**

- `SpentAmount`: 該月份、該分類、未刪除支出交易總額。
- `UsageRate`: `SpentAmount / Amount`。
- `RemainingAmount`: `Amount - SpentAmount` when >= 0。
- `OverspentAmount`: `SpentAmount - Amount` when > 0。

**提醒狀態**

```text
Normal: UsageRate < 80%
NearLimit: UsageRate >= 80% and <= 100%
Exceeded: UsageRate > 100%
```

## Entity: CsvImportBatch

代表一次 CSV 匯入摘要。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。 |
| `OriginalFileName` | `string` | 是 | 僅保留安全檔名，不包含路徑。 |
| `ImportedAtUtc` | `DateTimeOffset` | 是 | 處理時間。 |
| `TotalRows` | `int` | 是 | 不含標題列。 |
| `SucceededRows` | `int` | 是 | 成功建立交易數。 |
| `FailedRows` | `int` | 是 | 失敗列數。 |
| `CreatedCategoryCount` | `int` | 是 | 自動建立分類數。 |
| `Summary` | `string` | 是 | 使用者可讀摘要，不含敏感完整內容。 |

**關聯**

- `CsvImportBatch` has many `CsvImportError`
- `CsvImportBatch` may reference created `Transaction` IDs through audit summary or join table if implementation需要。

## Entity: CsvImportError

代表 CSV 匯入單列失敗原因。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。 |
| `CsvImportBatchId` | FK | 是 | 所屬批次。 |
| `RowNumber` | `int` | 是 | 使用者看到的 CSV 行號，標題列為第 1 行。 |
| `FieldName` | `string?` | 否 | 錯誤欄位。 |
| `Reason` | `string` | 是 | 繁體中文錯誤原因。 |
| `RawValuePreview` | `string?` | 否 | 最小揭露預覽，需截斷/遮罩。 |

## Entity: AuditEvent

代表關鍵業務事件與異動摘要。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Id` | `Guid` 或 `long` | 是 | 主鍵。 |
| `OccurredAtUtc` | `DateTimeOffset` | 是 | 事件時間。 |
| `EventType` | `AuditEventType` | 是 | `TransactionCreated`、`TransactionUpdated`、`TransactionDeleted`、`CsvImported`、`CsvExported`、`BudgetWarningTriggered`、`UnexpectedError` 等。 |
| `EntityType` | `string` | 是 | 受影響實體類型。 |
| `EntityId` | `string?` | 否 | 受影響實體 ID。 |
| `Summary` | `string` | 是 | 遮罩摘要，不含完整備註或敏感資料。 |
| `Severity` | `string` | 是 | `Information`、`Warning`、`Error`。 |
| `CorrelationId` | `string?` | 否 | 對應 request/log correlation。 |

## Entity: AppSetting

代表 V1 固定或可調整的站台設定。

| 欄位 | 型別 | 必填 | 規則 |
|------|------|------|------|
| `Key` | `string` | 是 | 主鍵，例如 `Currency`、`TimeZone`、`BudgetWarningThreshold`。 |
| `Value` | `string` | 是 | 值。 |
| `UpdatedAtUtc` | `DateTimeOffset` | 是 | 更新時間。 |

**初始值**

- `Currency = TWD`
- `TimeZone = Asia/Taipei`
- `BudgetWarningThreshold = 0.8`

## Validation Rules

- 金額: 必填、正數、最多 2 位小數、可轉換 minor units、不得 overflow。
- 交易日期: 必填、有效 `YYYY-MM-DD` 或 UI 日期輸入、不得晚於 Asia/Taipei 今日。
- 分類: 必須存在、未封存、類型符合交易類型。
- 帳戶: 必須存在、未封存。
- 分類名稱: trim 後不可空白，同一類型 normalized name 唯一。
- 帳戶名稱: trim 後不可空白，normalized name 全域唯一。
- 預算: 僅支出分類，金額 > 0，同分類同月份唯一。
- CSV: 固定六欄、檔案 < 5 MB、有效資料列 <= 10,000、逐列驗證、錯誤列不建立交易。
- 備註與文字欄位: 拒絕或清理 HTML/script 風險；匯出時防公式注入。

## 交易與查詢行為

- 一般明細、首頁、報表、預算計算預設 `IsDeleted = false`。
- 搜尋備註使用 sanitized/plain text 欄位。
- 金額範圍篩選使用 minor units 欄位。
- 日期範圍為包含起訖日期。
- 多分頁同時儲存同一筆交易時，最後完成儲存的版本為目前資料，並建立異動摘要。
