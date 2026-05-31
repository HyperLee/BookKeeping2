# Implementation Plan: 定期交易與轉帳確認

**Branch**: `006-recurring-bookkeeping` | **Date**: 2026-05-31 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-recurring-bookkeeping/spec.md`

## Summary

新增定期收入、支出與同幣別轉帳規則，系統只在 Asia/Taipei 到期後列出待確認項目，使用者逐期確認或略過後才建立正式交易或轉帳。技術上沿用目前 ASP.NET Core Razor Pages、EF Core SQLite、Bootstrap、jQuery Validation、Chart.js、Serilog、HtmlSanitizer、CsvHelper、xUnit、WebApplicationFactory、Playwright 與既有時間/稽核/金額工具；新增定期規則與期別處理資料模型、定期服務、待確認 UI、首頁摘要、正式紀錄來源連結與遮罩稽核事件。第一版不引入背景排程、自動入帳、通知、外部金融 API、進階 recurrence 套件或新前端框架。

## Technical Context

**Language/Version**: C# 14 / .NET 10 / ASP.NET Core 10.0，nullable reference types 與 implicit usings 維持啟用。

**Primary Dependencies**: Razor Pages、ASP.NET Core localization middleware、Bootstrap 5.3、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0 SQLite provider、Serilog.AspNetCore、HtmlSanitizer、CsvHelper。現有生產相依足以支援本功能；不新增 Quartz、Hangfire、NCrontab、RRULE library、SPA framework 或外部金融套件。必要 UI/可及性驗證可延續 Playwright 與 axe-core 或等效測試工具作為測試相依。

**Storage**: SQLite + EF Core `AppDbContext`、entity configuration、migration、唯一索引、外鍵與交易控制。新增 `RecurringRules` 與 `RecurringOccurrences` tables，並在 `Transactions`、`AccountTransfers` 加入 nullable `RecurringOccurrenceId` 來源欄位與唯一索引，讓同一期確認在快速重送或競態下仍不能建立第二筆正式紀錄。金額繼續以 `long` minor units 儲存，domain/view model 對外使用 `decimal`；幣別沿用 `SupportedCurrency` 固定清單；資料檔路徑仍由設定管理且不得提交實際 SQLite 檔案。

**Testing**: xUnit + Moq 單元測試、`WebApplicationFactory` Razor Pages 整合測試、EF Core SQLite persistence tests、coverlet coverage。新增 recurrence date generator、rule validation、occurrence materialization、confirmation idempotency、transaction/transfer formal record creation、report/budget/CSV exclusion-before-confirmation、audit masking、migration、Razor Pages、首頁摘要、performance 與 browser/responsive tests。Playwright 用於待確認清單、確認表單、規則管理、首頁摘要在手機與桌面寬度下的互動與不重疊驗證；若瀏覽器不可用，回報 blocker 並仍執行非瀏覽器測試。

**Target Platform**: 桌面瀏覽器 Chrome、Edge、Firefox、Safari 與行動裝置瀏覽器。

**Project Type**: Web，單一 ASP.NET Core Razor Pages 專案 + `BookKeeping2.Tests` 測試專案。

**Performance Goals**: 保留既有效能目標：FCP < 1.5 秒、LCP < 2.5 秒；10,000 筆收入、支出與轉帳混合明細篩選 < 2 秒；10,000 筆紀錄帳戶餘額查詢 < 3 秒；100 筆月報產生 < 2 秒；1,000 筆正式 CSV 匯出 < 5 秒；100 筆正式 CSV 匯入 < 10 秒。新增目標：100 筆定期規則與 500 筆歷史處理結果時，首頁待確認摘要與待確認清單 < 2 秒；單次開啟清單 materialize 最多 12 期補登的常見情境 < 1 秒；recurrence 查詢與 materialization 不得造成 N+1 account/category lookups。

**Constraints**: 第一版只支援收入、支出與同幣別帳戶轉帳；週期只支援每週、每月、每年與選填結束日；不支援每日、每 N 天、工作日、節假日調整、批次全選確認或自動入帳。規則可有未來開始日，但待確認項目、確認後正式交易日期與正式轉帳日期不得晚於 Asia/Taipei 今日。所有金額 API 使用 `decimal`，金額 > 0、最多 2 位小數且 <= `999,999,999.99`；幣別必須是 `TWD`、`USD`、`JPY`、`EUR`、`GBP` 且不做匯率或跨幣別合計。狀態變更表單使用 Anti-Forgery Token；規則名稱與備註需安全處理；使用者面向文字與錯誤修正提示維持繁體中文；生產環境 HTTPS/HSTS；未授權第三方傳輸為 0。

**Scale/Scope**: 單人使用、目前主要頁面包含首頁、交易、轉帳、帳戶、分類、預算、報表、CSV、隱私權與錯誤頁。一般年累積 600-2,400 筆正式紀錄，極端情境 10,000+ 筆正式收入/支出/轉帳混合紀錄；本功能新增定期規則管理、待確認清單、逐期確認/略過、首頁摘要、稽核、schema migration 與測試。仍不包含登入、角色、多帳本、跨裝置同步、第三方金融服務、跨幣別轉帳、匯率、電子發票或通知。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

- **I. 程式碼品質至上**: PASS。設計沿用既有 Razor Pages、Models、ViewModels、Services、Data 分層；新增公開型別、服務介面與複雜 date generator 需維持 C# 14、nullable-safe code、公開 API XML 文件註解與既有命名風格。
- **II. 測試優先開發**: PASS。後續 tasks 必須先取得使用者或維護者對測試意圖的確認，再撰寫失敗測試，涵蓋週期日期、月底 fallback、跨年/閏年、多期補登、確認前無正式影響、確認 idempotency、略過、停用、軟刪除、正式交易/轉帳沿用既有規則、報表/預算/CSV 邊界與 UI 響應式。
- **III. 使用者體驗一致性**: PASS。定期功能使用 Bootstrap 表單、表格/清單與既有主題/語系模式；首頁摘要、規則列表、待確認清單與確認表單在手機與桌面不得重疊，錯誤訊息以繁體中文指出修正方式。
- **IV. 效能與延展性**: PASS。以資料庫索引支援到期清單與狀態查詢；occurrence materialization 以 bounded service flow 與批次查詢完成，不引入背景工作或外部服務造成不可測延遲。
- **V. 可觀察性與監控**: PASS。規則建立/更新/停用/刪除、期別確認/略過與正式紀錄建立結果記錄遮罩稽核摘要；不記錄完整備註、未遮罩金額或敏感財務明細。
- **VI. 安全優先**: PASS。所有狀態變更使用 anti-forgery；所有輸入伺服器端驗證；名稱與備註經 `TextInputSanitizer`；CSV 仍只處理正式交易/轉帳並保留 formula injection protection；不新增第三方金融傳輸。
- **VII. 資料完整性**: PASS。待確認項目在確認前不影響正式財務資料；確認時以 EF Core transaction 同步建立正式紀錄、更新 occurrence 狀態與寫入稽核；`RecurringOccurrenceId` 唯一索引防止同一期重複正式紀錄；金額繼續 `decimal` 對外與 minor units persistence。

**Gate Result**: PASS。規格已完成釐清並通過 `checklists/requirements.md`；無未解決釐清事項。

## Project Structure

### Documentation (this feature)

```text
specs/006-recurring-bookkeeping/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── recurring-bookkeeping-contract.md
├── checklists/
│   └── requirements.md
└── tasks.md              # Phase 2 output from /speckit-tasks, not created here
```

### Source Code (repository root)

```text
BookKeeping2/
├── Models/
│   ├── Recurring/
│   │   ├── RecurringRule.cs
│   │   ├── RecurringOccurrence.cs
│   │   ├── RecurringRecordKind.cs
│   │   ├── RecurringFrequency.cs
│   │   └── RecurringOccurrenceStatus.cs
│   ├── Transactions/Transaction.cs          # nullable RecurringOccurrenceId source link
│   ├── AccountTransfers/AccountTransfer.cs  # nullable RecurringOccurrenceId source link
│   └── Audit/AuditEventType.cs              # recurring audit events
├── Data/
│   ├── AppDbContext.cs                      # DbSet<RecurringRule>, DbSet<RecurringOccurrence>
│   ├── EntityConfigurations/
│   │   ├── RecurringRuleConfiguration.cs
│   │   ├── RecurringOccurrenceConfiguration.cs
│   │   ├── TransactionConfiguration.cs
│   │   └── AccountTransferConfiguration.cs
│   └── Migrations/
│       └── [timestamp]_AddRecurringBookkeeping.cs
├── ViewModels/
│   └── Recurring/
│       ├── RecurringRuleInputModel.cs
│       ├── RecurringRuleListItemViewModel.cs
│       ├── RecurringOccurrenceListItemViewModel.cs
│       ├── RecurringTransactionConfirmationInputModel.cs
│       ├── RecurringTransferConfirmationInputModel.cs
│       └── RecurringFormOptionsViewModel.cs
├── Services/
│   ├── Recurring/
│   │   ├── IRecurringRuleService.cs
│   │   ├── IRecurringOccurrenceService.cs
│   │   ├── RecurringRuleService.cs
│   │   ├── RecurringOccurrenceService.cs
│   │   ├── RecurrenceDateCalculator.cs
│   │   └── RecurringResult.cs
│   ├── Transactions/
│   │   └── [shared validation helper if extracted]
│   └── AccountTransfers/
│       └── [shared validation helper if extracted]
├── Pages/
│   ├── Recurring/
│   │   ├── Index.cshtml(.cs)                # rules + pending entry points
│   │   ├── Create.cshtml(.cs)
│   │   ├── Edit.cshtml(.cs)
│   │   ├── Delete.cshtml(.cs)
│   │   ├── Pending.cshtml(.cs)
│   │   ├── ConfirmTransaction.cshtml(.cs)
│   │   ├── ConfirmTransfer.cshtml(.cs)
│   │   └── Skip.cshtml(.cs)
│   ├── Index.cshtml(.cs)                    # homepage pending summary
│   └── Shared/_Layout.cshtml                # navigation entry if needed
└── wwwroot/
    └── css/site.css                         # responsive/focus fixes for recurring surfaces if needed

BookKeeping2.Tests/
├── Unit/
│   ├── Recurring/
│   │   ├── RecurrenceDateCalculatorTests.cs
│   │   ├── RecurringRuleServiceTests.cs
│   │   ├── RecurringOccurrenceMaterializationTests.cs
│   │   └── RecurringConfirmationServiceTests.cs
│   ├── Transactions/RecurringTransactionSourceTests.cs
│   └── AccountTransfers/RecurringTransferSourceTests.cs
├── Integration/
│   ├── Pages/RecurringPagesTests.cs
│   ├── Pages/HomeRecurringSummaryTests.cs
│   ├── Persistence/RecurringPersistenceTests.cs
│   ├── Performance/RecurringOccurrencePerformanceTests.cs
│   └── Browser/RecurringBrowserTests.cs
└── TestSupport/
    └── TestDataBuilder.cs                   # recurring helpers
```

**Structure Decision**: 採用既有單一 ASP.NET Core Razor Pages 專案與 `BookKeeping2.Tests` 測試專案。定期規則與待確認期別是新的 persistence boundary；正式收入/支出仍使用 `Transactions`，正式轉帳仍使用 `AccountTransfers`，不新增第三種正式帳務紀錄。規則到期不靠背景排程，而是在首頁與定期頁讀取時由 service materialize 已到期且未處理的 occurrence，保留每期快照，確保規則後續編輯不會靜默改寫已存在待確認項目。確認流程在同一 EF Core transaction 內建立正式紀錄、設定 occurrence 狀態與寫入稽核；必要時抽出既有交易/轉帳驗證小型 helper，避免 recurring confirmation 與手動流程規則漂移。

## Phase 0 Research

Phase 0 completed in [research.md](./research.md)。使用者提供的既有技術上下文已評估：保留現有 ASP.NET Core/Razor Pages/EF Core SQLite/CsvHelper/Bootstrap/Chart.js/Serilog/HtmlSanitizer/xUnit/Playwright 組合，不替換主技術；修正過時範圍為目前已具備多幣別與同幣別轉帳；補強點是 recurrence date calculator、持久化 occurrence snapshot、formal record source unique indexes、確認 idempotency、首頁待確認摘要、稽核與 performance/browser 測試。

## Phase 1 Design

Phase 1 design artifacts:

- [data-model.md](./data-model.md)
- [contracts/recurring-bookkeeping-contract.md](./contracts/recurring-bookkeeping-contract.md)
- [quickstart.md](./quickstart.md)

### Implementation Session Check

- 2026-05-31: 確認目前分支為 `006-recurring-bookkeeping`，active feature spec 位於 `specs/006-recurring-bookkeeping/spec.md`。
- 2026-05-31: 既有程式碼已包含多幣別與同幣別轉帳實作，包括 `SupportedCurrency`、`AccountTransfer`、`IAccountTransferService`、轉帳 CSV、混合交易明細與相關測試；本功能計畫以這些現有邊界為基礎。
- 2026-05-31: `.specify/scripts` 未提供獨立 agent context update 腳本；依 `speckit-plan` 指示以手動更新 `AGENTS.md` SPECKIT marker 指向本 plan。

### Post-Design Constitution Check

- **I. 程式碼品質至上**: PASS。data model、contract 與 quickstart 使用既有分層；新增 helper 只限 recurrence/date/validation reuse 的具體需要。
- **II. 測試優先開發**: PASS。quickstart 明確要求先列測試意圖、取得確認、撰寫失敗測試，再實作；測試矩陣覆蓋 P1/P2/P3、資料完整性、安全、UI、效能與 persistence。
- **III. 使用者體驗一致性**: PASS。UI contract 指定繁體中文文字、鍵盤可操作、手機/桌面不重疊、既有主題與語系功能不被破壞。
- **IV. 效能與延展性**: PASS。data model 指定狀態/日期/規則索引，contract 限制 materialization 與列表查詢避免 N+1，保留 100 規則/500 處理結果 < 2 秒目標。
- **V. 可觀察性與監控**: PASS。稽核事件覆蓋規則變更與期別處理，摘要只保留遮罩金額、方向、類型與安全文字。
- **VI. 安全優先**: PASS。anti-forgery、server-side validation、sanitizer、Razor encoding、CSV formula protection 與無第三方金融傳輸均寫入 contract。
- **VII. 資料完整性**: PASS。確認前無正式影響、確認原子性、formal source unique indexes、軟刪除/停用邊界、同幣別轉帳驗證與報表/預算/CSV 分界均納入設計。

**Post-Design Gate Result**: PASS。

## Complexity Tracking

無憲章違規或需豁免的複雜度增加。
