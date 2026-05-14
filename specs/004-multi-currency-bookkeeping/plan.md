# Implementation Plan: 多幣別獨立記帳

**Branch**: `004-multi-currency-bookkeeping` | **Date**: 2026-05-14 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-multi-currency-bookkeeping/spec.md`

## Summary

將既有單一 TWD 記帳擴充為固定五種幣別 `TWD`、`USD`、`JPY`、`EUR`、`GBP` 的獨立記帳模型。交易、帳戶、預算、首頁摘要、月報、分類統計、趨勢圖、CSV 匯入匯出與稽核摘要都必須清楚保留幣別，且所有收入、支出、結餘、帳戶餘額、分類比例與預算進度只能在相同幣別內計算。技術上沿用現有 ASP.NET Core Razor Pages、EF Core SQLite、CsvHelper、Bootstrap、jQuery Validation、Serilog 與測試堆疊，不新增匯率服務、外部金融 API、第三方資料傳輸或多幣別換算套件；新增內部幣別 allow-list/validation helper、EF Core migration、分幣別 view model 與 contract tests。

## Technical Context

**Language/Version**: C# 14 / .NET 10 / ASP.NET Core 10.0，nullable reference types 與 implicit usings 維持啟用。
**Primary Dependencies**: Razor Pages、Bootstrap 5.3、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0 SQLite provider、Serilog.AspNetCore、HtmlSanitizer、CsvHelper。現有技術棧足以支援本功能；不新增生產相依套件。
**Storage**: SQLite + EF Core `AppDbContext`、entity configuration、migration、唯一索引、外鍵與交易控制。新增或校正 `Currency` 欄位於交易、帳戶與預算資料模型；既有資料與舊 CSV 預設 `TWD`。金額繼續以 `long` minor units 儲存，domain/view model 對外使用 `decimal`。
**Testing**: xUnit + Moq 單元測試、`WebApplicationFactory` 整合測試、EF Core SQLite persistence tests、coverlet coverage；新增多幣別服務、CSV、migration、Razor Pages、報表、預算、帳戶餘額與效能測試。Playwright 用於交易/帳戶/預算/報表/CSV 的桌面與行動 UI 驗證；axe-core 或等效 DOM/accessibility assertions 可作為可及性補強，但不是必需生產相依。
**Target Platform**: 桌面瀏覽器 Chrome、Edge、Firefox、Safari 與行動裝置瀏覽器。
**Project Type**: Web，單一 ASP.NET Core Razor Pages 專案 + `BookKeeping2.Tests` 測試專案。
**Performance Goals**: 保留既有效能目標：FCP < 1.5 秒、LCP < 2.5 秒；10,000 筆交易分幣別篩選 < 2 秒；10,000 筆紀錄分幣別查詢 < 3 秒；100 筆月報分幣別產生 < 2 秒；1,000 筆 CSV 匯出 < 5 秒；100 筆 CSV 匯入 < 10 秒。分幣別 grouping 必須在資料庫查詢或 bounded in-memory projection 中完成，不得造成跨幣別 N+1 查詢。
**Constraints**: 固定支援 `TWD`、`USD`、`JPY`、`EUR`、`GBP`；輸入 trim 並大小寫不敏感，儲存大寫代碼；所有幣別包含 `JPY` 都沿用最多 2 位小數、金額大於 0、上限 `999,999,999.99`；實際交易日期不得晚於 Asia/Taipei 今日；狀態變更表單使用 Anti-Forgery Token；使用者面向文字為繁體中文；生產環境 HTTPS/HSTS；未授權第三方傳輸為 0；不得提供匯率、換算、換匯紀錄或跨幣別總額。
**Scale/Scope**: 單人使用、目前約 8 個主要頁面；一般年累積 600-2,400 筆交易，極端情境 10,000+ 筆交易；本功能涵蓋交易、帳戶、預算、首頁摘要、月報/分類/趨勢、CSV 匯入匯出、稽核摘要與相關測試。仍不包含站內帳號、角色、多帳本、帳戶間轉帳、匯率管理或跨裝置同步。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

- **I. 程式碼品質至上**: PASS。設計沿用現有分層與 EF Core/Razor Pages patterns，新增幣別 helper、模型欄位、view model 與服務調整時需維持 C# 14、nullable-safe code、公開 API XML 文件註解與既有命名風格。
- **II. 測試優先開發**: PASS。後續 tasks 必須先取得使用者或維護者對測試意圖的確認，再撰寫失敗測試，涵蓋幣別 allow-list、帳戶幣別固定、交易帳戶幣別一致、分幣別報表/預算、CSV 七欄與六欄相容、migration default `TWD`、軟刪除排除、稽核摘要與 UI 顯示。
- **III. 使用者體驗一致性**: PASS。新增幣別欄位與篩選使用 Bootstrap 表單元件；金額旁必須顯示幣別；多幣別摘要不得暗示可合併；手機與桌面不得重疊或水平溢位。
- **IV. 效能與延展性**: PASS。使用 `Currency` 欄位與複合索引支援分幣別篩選與 grouping；避免匯率或外部服務造成延遲與不可測性。
- **V. 可觀察性與監控**: PASS。保留既有 Serilog/ILogger 與 audit events；多幣別稽核摘要只記錄必要幣別識別，不明文暴露不必要財務細節。
- **VI. 安全優先**: PASS。所有幣別輸入視為不可信並用 allow-list 驗證；狀態變更維持 anti-forgery；CSV 仍使用 CsvHelper 與 formula injection protection；不新增第三方金融資料傳輸。
- **VII. 資料完整性**: PASS。金額維持 `decimal` 對外與 `long` minor units persistence；交易、帳戶、預算的幣別一致性在 server-side validation 與 EF schema 層保護；跨紀錄更新維持 EF Core transaction；刪除仍採軟刪除。

**Gate Result**: PASS。規格已明確授權多幣別，覆寫既有 V1 單一 TWD 限制；無未解決釐清事項。

## Project Structure

### Documentation (this feature)

```text
specs/004-multi-currency-bookkeeping/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── multi-currency-contract.md
└── tasks.md              # Phase 2 output from /speckit-tasks, not created here
```

### Source Code (repository root)

```text
BookKeeping2/
├── Models/
│   ├── Common/
│   │   └── SupportedCurrency.cs       # or equivalent internal allow-list/value helper
│   ├── Transactions/Transaction.cs    # Currency, XML docs, not mapped amount docs updated
│   ├── Accounts/Account.cs            # Currency required and immutable after create
│   └── Budgets/Budget.cs              # Currency for monthly category budget scope
├── Data/
│   ├── EntityConfigurations/
│   │   ├── TransactionConfiguration.cs
│   │   ├── AccountConfiguration.cs
│   │   └── BudgetConfiguration.cs
│   └── Migrations/
│       └── [timestamp]_AddMultiCurrencyBookkeeping.cs
├── ViewModels/
│   ├── Accounts/AccountViewModels.cs
│   ├── Budgets/BudgetViewModels.cs
│   ├── Reports/MonthlyReportViewModel.cs
│   └── Transactions/*.cs
├── Services/
│   ├── Accounts/AccountService.cs
│   ├── Budgets/BudgetService.cs
│   ├── Csv/*.cs
│   ├── Reports/ReportService.cs
│   └── Transactions/*.cs
├── Pages/
│   ├── Accounts/Index.cshtml(.cs)
│   ├── Budgets/Index.cshtml(.cs)
│   ├── Csv/Import.cshtml(.cs)
│   ├── Csv/Export.cshtml(.cs)
│   ├── Reports/Index.cshtml(.cs)
│   ├── Transactions/*.cshtml(.cs)
│   └── Index.cshtml(.cs)
└── wwwroot/
    ├── css/site.css                   # responsive/focus fixes for currency labels if needed
    ├── js/site.js                     # shared site behavior remains theme/global only
    └── js/transactions.js             # transaction form account filtering enhancement only

BookKeeping2.Tests/
├── Unit/
│   ├── Accounts/AccountServiceTests.cs
│   ├── Budgets/BudgetServiceTests.cs
│   ├── Common/SupportedCurrencyTests.cs
│   ├── Csv/*.cs
│   ├── Reports/ReportServiceTests.cs
│   └── Transactions/*.cs
├── Integration/
│   ├── Pages/MultiCurrencyPageTests.cs
│   ├── Persistence/MultiCurrencyPersistenceTests.cs
│   ├── Performance/TransactionQueryPerformanceTests.cs
│   └── Browser/MultiCurrencyBrowserTests.cs
└── TestSupport/
    └── TestDataBuilder.cs             # helpers for seeded multi-currency data
```

**Structure Decision**: 採用既有單一 ASP.NET Core Razor Pages 專案與 `BookKeeping2.Tests` 測試專案。幣別是固定 business concept，不建立新專案、不引入 repository abstraction、不新增外部 money/currency library，也不新增 `Currencies` 資料表。持久化使用短字串 `Currency` 搭配內部 allow-list helper，讓 SQLite migration、CSV、UI select options 與 server-side validation 共用同一來源。既有 `Account.Currency` 可沿用但必須補齊建立時明確選擇、不可修改與 migration/snapshot 一致性；`Transaction` 與 `Budget` 需新增 `Currency`，所有摘要與查詢以幣別分組。

## Phase 0 Research

Phase 0 completed in [research.md](./research.md)。使用者提供的技術棧已評估：保留現有 ASP.NET Core/Razor Pages/EF Core SQLite/CsvHelper/Bootstrap/Chart.js/Serilog/HtmlSanitizer/xUnit/Playwright 組合，不替換主技術；補強點是內部幣別 allow-list、EF Core schema migration、複合索引、分幣別 view model 與測試矩陣。

## Phase 1 Design

Phase 1 design artifacts:

- [data-model.md](./data-model.md)
- [contracts/multi-currency-contract.md](./contracts/multi-currency-contract.md)
- [quickstart.md](./quickstart.md)

### Implementation Session Check

- 2026-05-14: 確認目前分支為 `004-multi-currency-bookkeeping`，feature artifacts 與現有專案結構一致。
- 2026-05-14: `.gitignore` 已涵蓋 .NET `bin/`、`obj/`、使用者檔、SQLite 本機資料庫、環境檔、日誌、coverage、IDE 與通用暫存檔；未偵測 Docker、ESLint、Prettier、npm publishing、Terraform 或 Helm 設定需要新增 ignore file。
- 2026-05-14: 測試資料 helper 將以 `TwdCurrency`、`UsdCurrency`、`JpyCurrency`、`EurCurrency`、`GbpCurrency` 常數命名，後續再擴充 account、transaction 與 budget helper 參數以指定幣別。

### Post-Design Constitution Check

- **I. 程式碼品質至上**: PASS。data model 使用既有 domain/service/view model 分層，沒有新增不必要套件或跨層抽象。
- **II. 測試優先開發**: PASS。quickstart 明確要求先寫失敗測試，並列出 P1/P2 user stories、migration、CSV、報表、預算、帳戶與 UI 驗證。
- **III. 使用者體驗一致性**: PASS。UI contract 要求每個可見金額伴隨幣別，禁止跨幣別總額，並要求響應式與鍵盤操作驗證。
- **IV. 效能與延展性**: PASS。contract 與 data model 指定 `Currency` 複合索引與分幣別 grouping，保留 10,000+ 筆查詢效能目標。
- **V. 可觀察性與監控**: PASS。稽核摘要新增必要幣別資訊，但仍遵守遮罩與不暴露敏感財務細節。
- **VI. 安全優先**: PASS。所有幣別值透過 allow-list；CSV 支援新舊格式但保留 CsvHelper、row-level errors、formula protection 與 anti-forgery。
- **VII. 資料完整性**: PASS。帳戶幣別固定、交易帳戶幣別一致、預算唯一範圍含幣別、舊資料 default `TWD`、軟刪除排除與 transaction boundaries 都已納入設計。

**Post-Design Gate Result**: PASS。

## Complexity Tracking

無憲章違規或需豁免的複雜度增加。
