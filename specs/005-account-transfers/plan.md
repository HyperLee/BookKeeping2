# Implementation Plan: 同幣別帳戶轉帳與信用卡繳款

**Branch**: `005-account-transfers` | **Date**: 2026-05-19 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-account-transfers/spec.md`

## Summary

新增獨立的同幣別帳戶轉帳能力，讓使用者能記錄銀行、現金、電子錢包與信用卡帳戶間的資金移動，而不把轉帳誤算為收入或支出。技術上沿用現有 ASP.NET Core Razor Pages、EF Core SQLite、CsvHelper、Bootstrap、jQuery Validation、Serilog、HtmlSanitizer 與 xUnit/Playwright 測試堆疊；新增 `AccountTransfer` persistence model、轉帳服務、轉帳 Razor Pages、混合明細時間線投影、轉帳 CSV 匯入匯出服務與稽核事件。帳戶餘額維持即時計算，不新增匯率、換匯、信用卡帳單週期或第三方金融整合。

## Technical Context

**Language/Version**: C# 14 / .NET 10 / ASP.NET Core 10.0，nullable reference types 與 implicit usings 維持啟用。

**Primary Dependencies**: Razor Pages、Bootstrap 5.3、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0 SQLite provider、Serilog.AspNetCore、HtmlSanitizer、CsvHelper。現有技術棧足以支援本功能；不新增生產相依套件。

**Storage**: SQLite + EF Core `AppDbContext`、entity configuration、migration、唯一索引、外鍵與交易控制。新增 `AccountTransfers` table，金額繼續以 `long` minor units 儲存，domain/view model 對外使用 `decimal`；每筆 create 使用不可猜測的一次性 `SubmissionToken`，成功建立後保存並以唯一索引防止同一表單重送建立第二筆。幣別沿用現有 `SupportedCurrency` 與帳戶 `Currency` 欄位，不新增 `Currencies` table，不回填交易資料；既有收入/支出 CSV 契約保持不變，轉帳使用獨立 CSV 格式。

**Testing**: xUnit + Moq 單元測試、`WebApplicationFactory` 整合測試、EF Core SQLite persistence tests、coverlet coverage。新增轉帳服務、帳戶餘額、混合明細查詢、CSV 匯入匯出、migration、Razor Pages、稽核、報表/預算排除與效能測試；關鍵轉帳金額、餘額、CSV 與查詢邏輯 coverage 必須達 80% 以上。Playwright 用於轉帳表單、明細時間線、CSV 操作在桌面與行動寬度下的 UI 驗證；轉帳 UI 必須以 axe-core 或等效 DOM/accessibility assertions 驗證 WCAG 2.1 AA 核心要求，相關工具僅作為測試相依。

**Target Platform**: 桌面瀏覽器 Chrome、Edge、Firefox、Safari 與行動裝置瀏覽器。

**Project Type**: Web，單一 ASP.NET Core Razor Pages 專案 + `BookKeeping2.Tests` 測試專案。

**Performance Goals**: 保留既有效能目標：FCP < 1.5 秒、LCP < 2.5 秒；10,000 筆收入、支出與轉帳混合明細套用單一篩選條件後 < 2 秒；10,000 筆紀錄帳戶餘額查詢 < 3 秒；100 筆月報產生 < 2 秒且排除轉帳；1,000 筆轉帳 CSV 匯出 < 5 秒；100 筆轉帳 CSV 匯入 < 10 秒。轉帳 grouping 與帳戶餘額彙總必須在資料庫查詢或 bounded in-memory projection 中完成，不得造成帳戶或轉帳 N+1 查詢。

**Constraints**: 只支援同幣別轉帳；支援幣別固定沿用 `TWD`、`USD`、`JPY`、`EUR`、`GBP`；輸入 trim 並大小寫不敏感，儲存大寫代碼；所有幣別包含 `JPY` 都沿用最多 2 位小數、金額大於 0、上限 `999,999,999.99`；轉帳日期不得晚於 Asia/Taipei 今日；轉出與轉入帳戶必須存在、未封存、不同帳戶且同幣別；允許轉出後負餘額；狀態變更表單使用 Anti-Forgery Token；使用者面向文字為繁體中文；生產環境 HTTPS/HSTS；未授權第三方傳輸為 0；不得提供匯率、換算、換匯紀錄、跨幣別總額或信用卡帳單週期。

**Scale/Scope**: 單人使用、目前約 8 個主要頁面；一般年累積 600-2,400 筆交易，極端情境 10,000+ 筆收入/支出/轉帳混合紀錄。本功能涵蓋轉帳 CRUD、帳戶餘額、明細時間線、CSV 匯入匯出、首頁帳戶餘額摘要、稽核摘要、報表/預算排除與相關測試；不包含站內帳號、角色、多帳本、跨幣別轉帳、匯率管理、信用卡帳單模型或跨裝置同步。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

- **I. 程式碼品質至上**: PASS。設計沿用現有 Razor Pages、Models、ViewModels、Services、Data 分層；新增公開型別與服務介面需維持 C# 14、nullable-safe code、公開 API XML 文件註解與既有命名風格。
- **II. 測試優先開發**: PASS。後續 tasks 必須先取得使用者或維護者對測試意圖的確認，再撰寫失敗測試，涵蓋同幣別驗證、同帳戶拒絕、負餘額允許、帳戶餘額、報表/預算排除、軟刪除、CSV、稽核、快速重送與 UI 響應式。
- **III. 使用者體驗一致性**: PASS。轉帳入口放在交易明細，轉帳表單使用 Bootstrap 表單元件；時間線必須清楚顯示「轉出帳戶 -> 轉入帳戶」與幣別金額；錯誤訊息使用繁體中文且可修正。
- **IV. 效能與延展性**: PASS。新增轉帳查詢索引與帳戶彙總查詢；混合時間線先套用日期、幣別、帳戶、金額與關鍵字篩選，再排序分頁，避免跨表 N+1。
- **V. 可觀察性與監控**: PASS。新增、更新、刪除、轉帳 CSV 匯入匯出都記錄遮罩稽核摘要；不在日誌、錯誤訊息或稽核摘要中暴露完整敏感備註或不必要明細。
- **VI. 安全優先**: PASS。所有轉帳與 CSV 欄位伺服器端驗證；狀態變更維持 anti-forgery；備註使用 `TextInputSanitizer`；CSV 匯出保留 formula injection protection；不新增外部金融傳輸。
- **VII. 資料完整性**: PASS。轉帳為獨立紀錄，不以收入/支出模擬；金額維持 `decimal` 對外與 `long` minor units persistence；新增/編輯/刪除與 CSV 匯入使用 EF Core transaction；軟刪除後帳戶餘額與一般匯出排除。

**Gate Result**: PASS。規格已明確限定同幣別轉帳、信用卡繳款作為轉帳、報表/預算排除與獨立 CSV；無未解決釐清事項。

## Project Structure

### Documentation (this feature)

```text
specs/005-account-transfers/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── account-transfer-contract.md
└── tasks.md              # Phase 2 output from /speckit-tasks, not created here
```

### Source Code (repository root)

```text
BookKeeping2/
├── Models/
│   ├── AccountTransfers/
│   │   └── AccountTransfer.cs
│   ├── Accounts/Account.cs              # optional outgoing/incoming navigation only if useful
│   ├── Audit/AuditEventType.cs          # transfer and transfer CSV audit events
│   └── Common/SupportedCurrency.cs      # reused, no new currency table
├── Data/
│   ├── AppDbContext.cs                  # DbSet<AccountTransfer>
│   ├── EntityConfigurations/
│   │   └── AccountTransferConfiguration.cs
│   └── Migrations/
│       └── [timestamp]_AddAccountTransfers.cs
├── ViewModels/
│   ├── AccountTransfers/
│   │   ├── AccountTransferInputModel.cs
│   │   └── AccountTransferFormOptionsViewModel.cs
│   └── Transactions/
│       ├── TransactionFilterInputModel.cs
│       └── TransactionTimelineItemViewModel.cs
├── Services/
│   ├── AccountTransfers/
│   │   ├── IAccountTransferService.cs
│   │   ├── AccountTransferService.cs
│   │   └── AccountTransferResult.cs
│   ├── Accounts/AccountService.cs       # include transfer totals in balance summaries
│   ├── Csv/
│   │   ├── CsvTransferRow.cs
│   │   ├── CsvTransferImportParser.cs
│   │   ├── CsvTransferImportService.cs
│   │   └── CsvTransferExportService.cs
│   └── Transactions/
│       └── TransactionQueryService.cs   # mixed income/expense/transfer timeline projection
├── Pages/
│   ├── Transfers/
│   │   ├── Create.cshtml(.cs)
│   │   ├── Edit.cshtml(.cs)
│   │   └── Delete.cshtml(.cs)
│   ├── Transactions/Index.cshtml(.cs)   # timeline, filters, "新增轉帳" entry
│   ├── Csv/Import.cshtml(.cs)           # separate transfer CSV import handler/form
│   ├── Csv/Export.cshtml(.cs)           # separate transfer CSV export handler/form
│   └── Index.cshtml(.cs)                # account balances reflect transfers
└── wwwroot/
    └── css/site.css                     # responsive fixes for transfer rows/forms if needed

BookKeeping2.Tests/
├── Unit/
│   ├── AccountTransfers/AccountTransferServiceTests.cs
│   ├── Accounts/AccountTransferBalanceTests.cs
│   ├── Csv/CsvTransferImportParserTests.cs
│   ├── Csv/CsvTransferImportServiceTests.cs
│   ├── Csv/CsvTransferExportServiceTests.cs
│   └── Transactions/TransactionTimelineQueryTests.cs
├── Integration/
│   ├── Pages/AccountTransferPagesTests.cs
│   ├── Pages/TransactionTimelineTransferTests.cs
│   ├── Pages/CsvTransferPageTests.cs
│   ├── Persistence/AccountTransferPersistenceTests.cs
│   ├── Performance/TransactionTimelinePerformanceTests.cs
│   └── Browser/AccountTransferBrowserTests.cs
└── TestSupport/
    └── TestDataBuilder.cs               # transfer/account helpers
```

**Structure Decision**: 採用既有單一 ASP.NET Core Razor Pages 專案與 `BookKeeping2.Tests` 測試專案。轉帳是獨立財務紀錄，新增 `AccountTransfer` table 與專用服務，不修改 `TransactionType` 來加入 `Transfer`，避免轉帳誤進收入/支出、報表、分類統計與預算邏輯。時間線使用讀取端 projection 顯示交易與轉帳，寫入端仍分離。帳戶餘額不儲存 derived balance，而是在 `AccountService` 以交易 totals 加上轉出/轉入 transfer totals 即時計算。

## Phase 0 Research

Phase 0 completed in [research.md](./research.md)。使用者提供的多幣別技術棧已評估：保留 ASP.NET Core/Razor Pages/EF Core SQLite/CsvHelper/Bootstrap/Chart.js/Serilog/HtmlSanitizer/xUnit/Playwright 組合，不替換主技術；補強點是獨立轉帳資料模型、轉帳查詢索引、混合時間線投影、專用轉帳 CSV contract、稽核事件與帳戶餘額彙總查詢。

## Phase 1 Design

Phase 1 design artifacts:

- [data-model.md](./data-model.md)
- [contracts/account-transfer-contract.md](./contracts/account-transfer-contract.md)
- [quickstart.md](./quickstart.md)

### Implementation Session Check

- 2026-05-19: 確認目前分支為 `005-account-transfers`，feature artifacts 與現有專案結構一致。
- 2026-05-19: 多幣別基礎已存在於 `SupportedCurrency`、`Account.Currency`、`Transaction.Currency`、`Budget.Currency` 與 20260514000000 migration；本功能不新增幣別清單或匯率規則。
- 2026-05-19: `.gitignore` 已存在；未偵測 Docker、npm、ESLint、Prettier、Terraform 或 Helm 設定需要新增 ignore file。

### Post-Design Constitution Check

- **I. 程式碼品質至上**: PASS。data model 與 contract 使用既有分層，未新增不必要套件或 repository abstraction。
- **II. 測試優先開發**: PASS。quickstart 明確要求先取得測試意圖確認，再寫失敗測試；測試矩陣涵蓋 P1/P2 user stories、migration、CSV、稽核、帳戶餘額、報表/預算排除、UI、可及性、coverage 與效能。
- **III. 使用者體驗一致性**: PASS。UI contract 指定繁體中文欄位、轉帳方向、錯誤訊息、手機/桌面不重疊與鍵盤可操作。
- **IV. 效能與延展性**: PASS。contract 與 data model 指定轉帳索引、混合時間線篩選順序與 bounded projection，保留 10,000+ 筆效能目標。
- **V. 可觀察性與監控**: PASS。稽核摘要包含必要轉帳方向與幣別，但仍遮罩金額與備註。
- **VI. 安全優先**: PASS。所有輸入透過 server-side validation、sanitizer、anti-forgery 與 CsvHelper；CSV 匯出使用 formula injection protection。
- **VII. 資料完整性**: PASS。轉帳獨立紀錄、同幣別驗證、軟刪除、交易邊界與帳戶餘額重新計算都已納入設計。

**Post-Design Gate Result**: PASS。

## Complexity Tracking

無憲章違規或需豁免的複雜度增加。
