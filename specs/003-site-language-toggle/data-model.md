# Data Model: 網站介面英文語系切換

本功能不新增 SQLite schema，也不修改既有 financial/domain entities。以下模型描述 request、Cookie、資源與顯示層資料契約。

## Entity: InterfaceLanguage

使用者可選擇的介面語言。

| Field | Type | Rules |
|-------|------|-------|
| `Code` | string | allow-list: `zh-TW`, `en` |
| `DisplayName` | string | 首頁控制項顯示文字：`繁體中文`、`English` |
| `HtmlLang` | string | `<html lang>` 值：`zh-Hant-TW`、`en` |
| `IsDefault` | bool | `zh-TW` 為唯一預設 |
| `UICultureName` | string | request `CurrentUICulture`：`zh-TW` 或 `en` |
| `CultureName` | string | 固定 `zh-TW`，保留既有日期、數字、金額格式 |

**Validation**:

- 任何非 `zh-TW` 或 `en` 的值都不得套用。
- 缺少或無效語言時必須回到 `zh-TW`。
- 不得依 `Accept-Language`、瀏覽器語言或作業系統語言推導英文。

## Entity: UserLanguagePreferenceCookie

同一瀏覽器與裝置上的使用者手動選擇。

| Field | Type | Rules |
|-------|------|-------|
| `Name` | string | `bookkeeping.ui.language` |
| `Value` | string | allow-list: `zh-TW`, `en` |
| `Path` | string | `/` |
| `ExpiresUtc` | DateTimeOffset | 寫入時間起算 1 年 |
| `SameSite` | enum | `Lax` |
| `Secure` | bool | HTTPS 連線下為 `true` |
| `HttpOnly` | bool | `true`，前端不需讀取 |
| `IsEssential` | bool | `true`，必要 Cookie |

**Validation**:

- Cookie value 只保存最後一次手動選擇的介面語言，不保存有效語言、切換歷史、頁面、使用者資料或財務資料。
- 無效 Cookie 必須被忽略並安全回到 `zh-TW`。
- Cookie 不得寫入 SQLite、Session、localStorage、audit events 或 server logs。

## Entity: RequestCultureResolution

每個 HTTP request 的語言解析結果。

| Field | Type | Rules |
|-------|------|-------|
| `CookieValue` | string? | 原始 Cookie value，僅用於 allow-list 判斷，不記錄 |
| `ResolvedLanguage` | InterfaceLanguage | `zh-TW` 或 `en` |
| `CurrentCulture` | CultureInfo | 固定 `zh-TW` |
| `CurrentUICulture` | CultureInfo | 依 `ResolvedLanguage` 對應 |
| `WasFallback` | bool | 缺少或無效 Cookie 時為 `true` |

**State transitions**:

- No cookie -> `zh-TW`, `WasFallback=true`
- Invalid cookie -> `zh-TW`, `WasFallback=true`
- `bookkeeping.ui.language=zh-TW` -> `zh-TW`, `WasFallback=false`
- `bookkeeping.ui.language=en` -> `en`, `WasFallback=false`
- Homepage POST `uiLanguage=en` -> response writes cookie -> redirect -> next request renders English UI
- Homepage POST `uiLanguage=zh-TW` -> response writes cookie -> redirect -> next request renders Traditional Chinese UI

## Entity: TranslatableUiString

系統提供給使用者閱讀或操作的固定文字。

| Field | Type | Rules |
|-------|------|-------|
| `DefaultText` | string | 繁體中文文字，可作為 resource key |
| `EnglishText` | string | `SharedResource.en.resx` 中的英文翻譯 |
| `Context` | string | 頁面、partial、PageModel、non-error service result 或 DataAnnotations display context |
| `AllowsHtml` | bool | 預設 `false`；本功能不本地化 HTML markup |
| `IsRequired` | bool | 主要頁面、DataAnnotations display、非錯誤狀態與確認訊息均為 required；錯誤/驗證修正訊息依憲章維持繁體中文 |

**Validation**:

- 英文模式不得顯示空白文字、未替換 placeholder、內部 key 或未完成混合語言系統訊息。
- Razor 預設 HTML encoding 必須維持；資源字串不應包含 HTML。
- 若文字含參數，所有參數必須保留並由 Razor/localizer 正常編碼。

## Entity: SystemDisplayLabel

系統定義的 enum、狀態與預設分類顯示名稱。

| Field | Type | Rules |
|-------|------|-------|
| `SourceKind` | enum | `TransactionType`, `AccountType`, `BudgetAlertState`, `DefaultCategory`, `CategoryStatus`, other system labels |
| `CanonicalValue` | string | 程式或資料庫中的既有值，例如 `Expense`、`餐飲` |
| `TraditionalChineseText` | string | 既有顯示文字 |
| `EnglishText` | string | 英文顯示文字 |
| `IsPersisted` | bool | `false` for translated display text |

**Validation**:

- 預設分類可依 `IsDefault` 與 canonical name 在顯示層翻譯。
- 使用者建立或匯入的分類名稱不得翻譯，即使文字剛好等於預設分類名稱。
- 匯出 CSV 時交易類型值仍為 `收入` 或 `支出`，不得改成 `Income` 或 `Expense`。

## Entity: UserDataOriginal

使用者自行建立、編輯或匯入的資料原文。

| Field | Type | Rules |
|-------|------|-------|
| `AccountName` | string | 永遠原文顯示 |
| `UserCategoryName` | string | 永遠原文顯示 |
| `TransactionNote` | string? | 永遠原文顯示，仍套用既有 sanitization/encoding |
| `CsvRawContent` | string | 依既有 parser/formatter 處理，不翻譯 |
| `ReportData` | decimal/date/category values | 金額、日期與計算結果不因語言改變 |

**Validation**:

- 語言切換不得修改、刪除、重新計算或重寫任何使用者資料原文。
- 語言切換不得產生 audit event 或 last-change summary。

## Entity: PageLanguageCoverage

必須驗證的可直接瀏覽頁面範圍。

| Page | Route | Notes |
|------|-------|-------|
| 首頁 | `/` | 唯一顯示語言控制項；同時保留既有主題控制項 |
| 隱私權頁 | `/Privacy` | 套用語言，不顯示語言控制項 |
| 錯誤頁 | `/Error` | page chrome localizes without exposing internals; user-facing error text remains zh-TW |
| 帳戶 | `/Accounts` | 表單、表格、狀態訊息、account type labels |
| 分類 | `/Categories` | 表單、狀態、transaction type labels、default/custom distinction |
| 預算 | `/Budgets` | 表單、進度、alert labels |
| 交易清單 | `/Transactions` | filter labels、table、pagination、actions |
| 新增交易 | `/Transactions/Create` | form labels/options localize; validation/error text remains zh-TW |
| 編輯交易 | `/Transactions/Edit/{id}` | form labels/options localize; validation/error text remains zh-TW |
| 刪除交易 | `/Transactions/Delete/{id}` | confirmation text and actions |
| CSV 匯入 | `/Csv/Import` | page text and non-error status localize; error/validation text and file contract remain zh-TW |
| CSV 匯出 | `/Csv/Export` | page text localizes; downloaded CSV contract remains zh-TW |
| 報表 | `/Reports` | headings/chart labels/empty states localize; totals unchanged |
