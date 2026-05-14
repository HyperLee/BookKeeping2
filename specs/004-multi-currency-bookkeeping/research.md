# Research: 多幣別獨立記帳

## Decision: 保留現有 ASP.NET Core Razor Pages 技術棧，不替換主框架

**Rationale**: 這個功能是既有單人記帳系統的 domain/schema/UI 擴充，不是架構重寫。Razor Pages、PageModel、DataAnnotations、jQuery Validation 與 Bootstrap 已支援現有交易、帳戶、預算、CSV 與報表流程；Microsoft 的 Razor Pages/model validation 指引也符合目前 `ModelState.IsValid`、Tag Helpers 與表單驗證模式。更換為 SPA、API-first 或 Blazor 會擴大重工，且不會直接降低多幣別混算風險。

**Alternatives considered**:

- Blazor 或 SPA frontend: 對本功能沒有必要，會重寫現有頁面與測試。
- Minimal API + frontend app: 會新增 API contract 與前後端狀態同步負擔，超出單一 Razor Pages app 的需求。
- 保持現狀但只在 UI 顯示幣別: 無法保護資料完整性，不能防止帳戶、預算與報表混算。

## Decision: 使用內部固定幣別 allow-list，不新增外部貨幣套件或 `Currencies` 資料表

**Rationale**: 規格固定支援 `TWD`、`USD`、`JPY`、`EUR`、`GBP`，且明確排除匯率與換算。最佳設計是新增內部 helper/value catalog，例如 `SupportedCurrency` 或 `CurrencyCatalog`，集中提供正規化、驗證、顯示名稱與選項排序。資料庫只保存大寫三碼字串 `Currency`，不需要可編輯 currency table，也不需要 ISO currency library。

**Alternatives considered**:

- `Currencies` 資料表: 對固定五種支援值過度設計，也會讓使用者誤以為可自行新增幣別。
- C# enum 直接 persisted: 可行但對 CSV/string input 正規化較不直覺，未來顯示名稱或排序仍需要 helper。
- 外部 money/currency library: 會引入匯率或每幣別小數位等行為暗示，與本規格「所有幣別含 JPY 都最多 2 位小數」不一致。

## Decision: 金額儲存繼續使用 `long` minor units，所有幣別沿用 2 位小數規則

**Rationale**: 現有 `MoneyMinorUnitConverter` 已將 domain/view model 的 `decimal` 金額轉為 SQLite 中的 `long` minor units，避免 SQLite decimal query limitation 對比較與排序造成 client evaluation 風險。規格要求所有支援幣別套用相同金額規則，包含 `JPY` 最多 2 位小數，因此不應為不同幣別引入不同 scale。

**Alternatives considered**:

- SQLite 直接保存 `decimal`: EF Core SQLite 可讀寫 decimal，但官方限制指出部分比較/排序會需要 client evaluation；金額查詢與效能測試不適合改回 decimal column。
- 為 JPY 使用 0 位小數: 違反 FR-022。
- 使用 `double`: 違反憲章與金融精確度要求。

## Decision: EF Core migration 為 existing records 設定 `TWD` default，並加入分幣別索引

**Rationale**: FR-002/FR-024 要求既有或舊匯入資料視為 `TWD` 且金額數值不變。Migration 應為 `Transactions.Currency`、`Budgets.Currency` 加入 required `TEXT` 欄位並預設 `TWD`，確認 `Accounts.Currency` 在 migration/model snapshot 中一致存在且既有帳戶為 `TWD`。EF Core composite indexes 可同時支援唯一性與查詢效率，例如預算唯一範圍 `{ CategoryId, BudgetMonth, Currency }`，交易查詢 `{ IsDeleted, Currency, TransactionDate }`。SQLite migration limitations 可接受這類 AddColumn/CreateIndex/unique index 變更，但需要 persistence tests 驗證 migration、default 值、索引與外鍵。

**Alternatives considered**:

- 執行資料修補但不加 schema 約束: 無法長期防止缺少幣別值。
- 將幣別藏在備註或顯示層: 無法查詢、統計、匯出或驗證。
- 建立跨幣別 materialized summary table: 目前規模不需要，且會增加一致性風險。

## Decision: 交易、帳戶與預算在 server-side validation 強制幣別一致

**Rationale**: 帳戶餘額與預算進度是金融資料結果，不能只靠 UI 過濾。`TransactionService` 和 CSV import 必須驗證交易幣別屬於 allow-list，且選用帳戶存在、未封存並與交易幣別一致。`AccountService` 建立帳戶時必須要求明確幣別，既有帳戶的幣別不可透過 edit flow 修改。`BudgetService` 必須以月份、支出分類與幣別為唯一範圍，支出使用金額只納入同月、同分類、同幣別且未軟刪除交易。

**Alternatives considered**:

- 只在 Razor select 過濾帳戶: CSV、測試或手動 POST 仍可能送入不一致帳戶。
- 允許交易變更幣別但保留原帳戶: 直接破壞帳戶餘額可信度。
- 預算不綁幣別: 會把同分類不同幣別支出混入同一預算。

## Decision: 所有摘要、報表與圖表改成分幣別資料結構

**Rationale**: 規格核心是禁止跨幣別合計。首頁摘要、帳戶餘額、月報、分類統計、趨勢圖與 Chart.js datasets 都應輸出 currency bucket，例如每個 bucket 內有 income、expense、balance、category shares、daily trend。只有單一幣別資料時 UI 可只顯示該幣別，但 view model 仍應讓幣別顯性存在，避免頁面或 chart label 隱含跨幣別總額。

**Alternatives considered**:

- 顯示一組 totals 並在旁標多幣別警告: 仍可能誤導，且違反 FR-007/SC-003。
- 將不同幣別金額相加後只顯示「混合」: 規格明確禁止。
- 以帳戶幣別推導報表幣別: 交易本身需保存幣別，才能符合 CSV 與歷史資料要求。

## Decision: CSV 匯出採七欄新契約，匯入同時支援七欄與舊六欄

**Rationale**: 規格指定新匯出欄位順序為 `日期,類型,幣別,金額,分類,帳戶,備註`，新版匯入亦使用此七欄格式。舊六欄 `日期,類型,金額,分類,帳戶,備註` 必須相容並預設 `TWD`。CsvHelper 仍適合 RFC-style parsing/writing、row-level validation 與逐列處理；現有 formula injection protection 必須保留，並套用到 category/account/note 等文字欄位。

**Alternatives considered**:

- 只接受七欄並拒絕六欄: 破壞既有備份與移轉流程。
- 七欄中幣別空白時自動 `TWD`: 規格要求有幣別欄但值空白應失敗。
- 依帳戶幣別推定 CSV 幣別: FR-032 禁止任意推定，且會掩蓋匯入資料問題。

## Decision: Playwright 作為 UI/響應式驗證補強，axe-core 只作選配測試工具

**Rationale**: 既有 test project 已包含 Playwright，且 repository 已有 browser fixture patterns。多幣別 UI 風險主要是欄位增加後在行動寬度重疊、表單選項與篩選互動錯誤、Chart.js/表格標籤不清楚。Playwright 可直接驗證桌面與行動 viewport、鍵盤操作與文字呈現。axe-core 或等效 DOM assertions 可補強可及性，但不需要納入生產相依或成為功能前置條件。

**Alternatives considered**:

- 只用 WebApplicationFactory HTML 字串測試: 無法驗證行動/桌面視覺與互動。
- 導入完整 visual regression SaaS: 對單人本地 app 過重，且會引入第三方傳輸疑慮。
- 不做 browser tests: 無法滿足 UI 變更的響應式與可及性風險控制。

## Sources

- Microsoft Learn: Model validation in ASP.NET Core MVC and Razor Pages, .NET 10 view: https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0
- Microsoft Learn: Prevent Cross-Site Request Forgery attacks in ASP.NET Core, .NET 10 view: https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0
- Microsoft Learn: EF Core indexes: https://learn.microsoft.com/en-us/ef/core/modeling/indexes
- Microsoft Learn: EF Core transactions: https://learn.microsoft.com/en-us/ef/core/saving/transactions
- Microsoft Learn: EF Core SQLite provider limitations: https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations
- CsvHelper official documentation: https://joshclose.github.io/CsvHelper/
- Playwright .NET browser documentation: https://playwright.dev/dotnet/docs/browsers
