# 任務: Open BookKeeping - 開源個人記帳理財工具

**輸入**: `/specs/001-personal-bookkeeping-tool/` 內的設計文件  
**前置文件**: `plan.md`、`spec.md`、`research.md`、`data-model.md`、`contracts/ui-pages.md`、`contracts/csv-format.md`、`quickstart.md`  
**測試要求**: 本專案憲章要求 TDD。每個使用者故事 MUST 先建立會失敗的測試，再進行實作。  
**組織方式**: 任務依使用者故事分組，確保每個故事可獨立實作、測試與交付。  
**語言要求**: 使用者面向文件、UI 文字、驗證訊息與錯誤回饋 MUST 使用繁體中文 zh-TW。

## 格式: `[ID] [P?] [Story] 描述`

- **[P]**: 可平行執行，代表不同檔案且無相依關係
- **[Story]**: 對應使用者故事，例如 US1、US2、US3
- 描述 MUST 包含精確檔案路徑

## 路徑慣例

- **Web 專案**: `BookKeeping2/`
- **Razor Pages**: `BookKeeping2/Pages/`
- **模型**: `BookKeeping2/Models/`
- **服務**: `BookKeeping2/Services/`
- **靜態資源**: `BookKeeping2/wwwroot/css/`、`BookKeeping2/wwwroot/js/`
- **測試專案**: `BookKeeping2.Tests/`
- **單元測試**: `BookKeeping2.Tests/Unit/`
- **整合測試**: `BookKeeping2.Tests/Integration/`

## Phase 1: Setup（共用基礎）

**目的**: 建立功能所需的專案、測試與工具基礎。

- [X] T001 更新 `BookKeeping2/BookKeeping2.csproj`，加入 EF Core SQLite、EF Core Design、Serilog.AspNetCore、Ganss.Xss、CsvHelper 套件參考，啟用 XML 文件輸出與公開 API 文件註解警告檢查，並在 `.editorconfig` 確認私有欄位命名規則與憲章差異的實作決策
- [X] T002 建立 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`，加入 xUnit、Moq、Microsoft.AspNetCore.Mvc.Testing、Microsoft.EntityFrameworkCore.Sqlite、coverlet.collector 套件參考
- [X] T003 [P] 建立 SQLite 測試資料庫 fixture 於 `BookKeeping2.Tests/TestSupport/SqliteTestDatabase.cs`
- [X] T004 [P] 建立 Razor Pages 整合測試 factory 於 `BookKeeping2.Tests/TestSupport/BookKeepingWebApplicationFactory.cs`
- [X] T005 [P] 建立可固定 Asia/Taipei 今日的測試時間服務於 `BookKeeping2.Tests/TestSupport/FakeTaipeiDateService.cs`
- [X] T006 更新 `BookKeeping2.slnx`，納入 `BookKeeping2/BookKeeping2.csproj` 與 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [X] T007 建立測試共用資料產生器於 `BookKeeping2.Tests/TestSupport/TestDataBuilder.cs`

---

## Phase 2: Foundational（阻塞性前置）

**目的**: 完成所有使用者故事共用且不可延後的資料、驗證、安全與稽核基礎。

**CRITICAL**: 此階段完成前不得開始任何使用者故事實作。

- [X] T008 建立交易與稽核列舉於 `BookKeeping2/Models/Common/TransactionType.cs`、`BookKeeping2/Models/Accounts/AccountType.cs`、`BookKeeping2/Models/Audit/AuditEventType.cs`
- [X] T009 建立金額 minor units 轉換工具於 `BookKeeping2/Services/Common/MoneyMinorUnitConverter.cs`
- [X] T010 建立 Asia/Taipei 日期服務介面與實作於 `BookKeeping2/Services/Time/ITaipeiDateService.cs`、`BookKeeping2/Services/Time/TaipeiDateService.cs`
- [X] T011 [P] 建立交易實體於 `BookKeeping2/Models/Transactions/Transaction.cs`
- [X] T012 [P] 建立分類實體於 `BookKeeping2/Models/Categories/Category.cs`
- [X] T013 [P] 建立帳戶實體於 `BookKeeping2/Models/Accounts/Account.cs`
- [X] T014 [P] 建立預算實體於 `BookKeeping2/Models/Budgets/Budget.cs`
- [X] T015 [P] 建立 CSV 匯入批次與錯誤實體於 `BookKeeping2/Models/CsvImports/CsvImportBatch.cs`、`BookKeeping2/Models/CsvImports/CsvImportError.cs`
- [X] T016 [P] 建立稽核事件與應用設定實體於 `BookKeeping2/Models/Audit/AuditEvent.cs`、`BookKeeping2/Models/Settings/AppSetting.cs`
- [X] T017 建立 EF Core DbContext 與 DbSet 於 `BookKeeping2/Data/AppDbContext.cs`
- [X] T018 建立 EF Core entity configuration 於 `BookKeeping2/Data/EntityConfigurations/TransactionConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/CategoryConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/AccountConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/BudgetConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/CsvImportBatchConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/CsvImportErrorConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/AuditEventConfiguration.cs`、`BookKeeping2/Data/EntityConfigurations/AppSettingConfiguration.cs`
- [X] T019 建立預設分類與站台設定 seed 於 `BookKeeping2/Data/SeedData/DefaultSeedData.cs`、`BookKeeping2/Data/SeedData/DatabaseInitializer.cs`
- [X] T020 建立 EF Core migration design-time factory、`InitialCreate` migration 與初始 migration snapshot 於 `BookKeeping2/Data/AppDbContextFactory.cs`、`BookKeeping2/Data/Migrations/*_InitialCreate.cs`、`BookKeeping2/Data/Migrations/AppDbContextModelSnapshot.cs`
- [X] T021 更新 SQLite connection string 與資料庫路徑設定於 `BookKeeping2/appsettings.json`、`BookKeeping2/appsettings.Development.json`
- [X] T022 建立共用驗證訊息與輸入安全 helper 於 `BookKeeping2/Validation/FinancialValidationMessages.cs`、`BookKeeping2/Validation/TextInputSanitizer.cs`
- [X] T023 建立稽核服務介面與遮罩策略於 `BookKeeping2/Services/Audit/IAuditService.cs`、`BookKeeping2/Services/Audit/AuditService.cs`、`BookKeeping2/Services/Audit/AuditLogMaskingPolicy.cs`
- [X] T024 建立安全標頭註冊 extension 於 `BookKeeping2/Services/Security/SecurityHeadersExtensions.cs`
- [X] T025 更新 DI、DbContext、初始化、安全標頭與服務註冊於 `BookKeeping2/Program.cs`

**Checkpoint**: Foundation ready。使用者故事可依優先級開始實作。

---

## Phase 3: User Story 1 - 快速新增與維護交易紀錄 (Priority: P1)

**Goal**: 使用者可新增、編輯、軟刪除收入與支出交易，且交易立即反映在明細列表。

**Independent Test**: 建立測試用分類與帳戶後，新增 TWD 150 餐飲支出，確認列表顯示正確欄位；再編輯為 TWD 200 並軟刪除，確認一般列表排除且稽核摘要存在。

### Tests for User Story 1（必須先寫且先失敗）

- [X] T026 [P] [US1] 撰寫交易金額、日期、分類與帳戶驗證單元測試於 `BookKeeping2.Tests/Unit/Transactions/TransactionServiceValidationTests.cs`
- [X] T027 [P] [US1] 撰寫 minor units 精度與 overflow 單元測試於 `BookKeeping2.Tests/Unit/Common/MoneyMinorUnitConverterTests.cs`
- [X] T028 [P] [US1] 撰寫新增、編輯、軟刪除交易頁面整合測試，並驗證明細列表與首頁摘要 1 秒內反映變更，於 `BookKeeping2.Tests/Integration/Pages/TransactionPagesTests.cs`
- [X] T029 [P] [US1] 撰寫重複提交與 antiforgery 整合測試於 `BookKeeping2.Tests/Integration/Pages/TransactionFormSecurityTests.cs`
- [X] T030 [US1] 執行交易相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 1

- [X] T031 [P] [US1] 建立交易輸入模型於 `BookKeeping2/ViewModels/Transactions/TransactionInputModel.cs`
- [X] T032 [P] [US1] 建立交易列表與表單選項 ViewModel 於 `BookKeeping2/ViewModels/Transactions/TransactionListItemViewModel.cs`、`BookKeeping2/ViewModels/Transactions/TransactionFormOptionsViewModel.cs`
- [X] T033 [US1] 建立交易服務介面與結果型別於 `BookKeeping2/Services/Transactions/ITransactionService.cs`、`BookKeeping2/Services/Transactions/TransactionResult.cs`
- [X] T034 [US1] 實作交易新增、編輯、軟刪除、查詢與稽核邏輯於 `BookKeeping2/Services/Transactions/TransactionService.cs`
- [X] T035 [US1] 實作交易明細 PageModel 於 `BookKeeping2/Pages/Transactions/Index.cshtml.cs`
- [X] T036 [US1] 實作交易明細 Razor Page 與刪除入口於 `BookKeeping2/Pages/Transactions/Index.cshtml`
- [X] T037 [US1] 實作新增交易 Razor Page 與 PageModel 於 `BookKeeping2/Pages/Transactions/Create.cshtml`、`BookKeeping2/Pages/Transactions/Create.cshtml.cs`
- [X] T038 [US1] 實作編輯交易 Razor Page 與 PageModel 於 `BookKeeping2/Pages/Transactions/Edit.cshtml`、`BookKeeping2/Pages/Transactions/Edit.cshtml.cs`
- [X] T039 [US1] 實作刪除確認 Razor Page 與 PageModel 於 `BookKeeping2/Pages/Transactions/Delete.cshtml`、`BookKeeping2/Pages/Transactions/Delete.cshtml.cs`
- [X] T040 [US1] 加入防重複提交與欄位回饋腳本於 `BookKeeping2/wwwroot/js/transactions.js`
- [X] T041 [US1] 執行並修正交易相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 1 必須可獨立運作、獨立測試並可展示。

---

## Phase 4: User Story 2 - 資料持久保存與跨瀏覽器一致 (Priority: P1)

**Goal**: 同一站台實例使用 SQLite 保存交易、分類、帳戶、預算與設定，不依賴瀏覽器快取或 localStorage。

**Independent Test**: 建立交易與自訂分類後重新建立測試 client，確認資料仍存在；兩個 client 先後儲存同一交易時，最後完成儲存版本成為目前資料且保留異動摘要。

### Tests for User Story 2（必須先寫且先失敗）

- [X] T042 [P] [US2] 撰寫 SQLite 持久化與重新啟動資料保留整合測試於 `BookKeeping2.Tests/Integration/Persistence/SqlitePersistenceTests.cs`
- [X] T043 [P] [US2] 撰寫跨 client 單一帳本資料一致整合測試，並驗證 V1 無站內帳號、角色或每使用者資料隔離行為於 `BookKeeping2.Tests/Integration/Persistence/CrossBrowserConsistencyTests.cs`
- [X] T044 [P] [US2] 撰寫最後儲存版本與異動摘要單元測試於 `BookKeeping2.Tests/Unit/Transactions/LastWriteWinsAuditTests.cs`
- [X] T045 [US2] 執行持久化相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 2

- [X] T046 [US2] 建立資料庫路徑與初始化選項於 `BookKeeping2/Data/BookKeepingDbOptions.cs`
- [X] T047 [US2] 實作資料庫啟動初始化服務於 `BookKeeping2/Data/DatabaseStartupService.cs`
- [X] T048 [US2] 更新 SQLite connection string 與環境設定讀取於 `BookKeeping2/appsettings.json`、`BookKeeping2/appsettings.Development.json`
- [X] T049 [US2] 補齊交易寫入 transaction scope 與最後儲存異動摘要於 `BookKeeping2/Services/Transactions/TransactionService.cs`
- [X] T050 [US2] 更新首頁資料讀取使用持久化來源於 `BookKeeping2/Pages/Index.cshtml.cs`
- [X] T051 [US2] 執行並修正持久化相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 2 必須證明清除瀏覽器快取或換瀏覽器後資料仍由站台資料庫提供。

---

## Phase 5: User Story 3 - 管理分類與帳戶 (Priority: P1)

**Goal**: 使用者可管理收入/支出分類與帳戶，並能用交易計算各帳戶目前餘額。

**Independent Test**: 首次啟動確認預設分類存在；新增「寵物」支出分類與一個帳戶後，交易表單可選取，帳戶餘額依收入與支出更新。

### Tests for User Story 3（必須先寫且先失敗）

- [X] T052 [P] [US3] 撰寫分類 normalized name 唯一性、預設分類與封存規則單元測試於 `BookKeeping2.Tests/Unit/Categories/CategoryServiceTests.cs`
- [X] T053 [P] [US3] 撰寫帳戶 normalized name 唯一性與餘額計算單元測試於 `BookKeeping2.Tests/Unit/Accounts/AccountServiceTests.cs`
- [X] T054 [P] [US3] 撰寫分類與帳戶管理頁面整合測試於 `BookKeeping2.Tests/Integration/Pages/CategoryAndAccountPagesTests.cs`
- [X] T055 [US3] 執行分類與帳戶相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 3

- [X] T056 [P] [US3] 建立分類管理 ViewModel 於 `BookKeeping2/ViewModels/Categories/CategoryViewModels.cs`
- [X] T057 [P] [US3] 建立帳戶管理 ViewModel 於 `BookKeeping2/ViewModels/Accounts/AccountViewModels.cs`
- [X] T058 [US3] 建立分類服務介面與結果型別於 `BookKeeping2/Services/Categories/ICategoryService.cs`、`BookKeeping2/Services/Categories/CategoryResult.cs`
- [X] T059 [US3] 實作分類新增、編輯、刪除限制與封存邏輯於 `BookKeeping2/Services/Categories/CategoryService.cs`
- [X] T060 [US3] 建立帳戶服務介面與餘額摘要型別於 `BookKeeping2/Services/Accounts/IAccountService.cs`、`BookKeeping2/Services/Accounts/AccountBalanceSummary.cs`
- [X] T061 [US3] 實作帳戶新增、編輯、封存與目前餘額計算於 `BookKeeping2/Services/Accounts/AccountService.cs`
- [X] T062 [US3] 實作分類管理 Razor Page 與 PageModel 於 `BookKeeping2/Pages/Categories/Index.cshtml`、`BookKeeping2/Pages/Categories/Index.cshtml.cs`
- [X] T063 [US3] 實作帳戶管理 Razor Page 與 PageModel 於 `BookKeeping2/Pages/Accounts/Index.cshtml`、`BookKeeping2/Pages/Accounts/Index.cshtml.cs`
- [X] T064 [US3] 將分類與帳戶選項整合到交易表單載入流程於 `BookKeeping2/Services/Transactions/TransactionFormOptionsService.cs`
- [X] T065 [US3] 執行並修正分類與帳戶相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 3 必須可獨立展示分類、帳戶與餘額摘要。

---

## Phase 6: User Story 4 - 查看月度摘要與視覺化報表 (Priority: P2)

**Goal**: 使用者可查看指定月份收入、支出、結餘、支出分類佔比與趨勢圖，無資料時顯示清楚空白狀態。

**Independent Test**: 建立一個月內多分類、多日期交易，報表總額、百分比、趨勢資料與測試資料完全一致；無資料月份顯示「本月尚無紀錄」。

### Tests for User Story 4（必須先寫且先失敗）

- [ ] T066 [P] [US4] 撰寫月報總額、分類佔比、跨年月份歸屬與 100 筆月報 2 秒內完成單元測試於 `BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs`
- [ ] T067 [P] [US4] 撰寫報表頁面與空白狀態整合測試於 `BookKeeping2.Tests/Integration/Pages/ReportsPageTests.cs`
- [ ] T068 [US4] 執行報表相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 4

- [ ] T069 [P] [US4] 建立報表 ViewModel 於 `BookKeeping2/ViewModels/Reports/MonthlyReportViewModel.cs`
- [ ] T070 [US4] 建立報表服務介面與圖表資料型別於 `BookKeeping2/Services/Reports/IReportService.cs`、`BookKeeping2/Services/Reports/ReportChartPoint.cs`
- [ ] T071 [US4] 實作伺服器端月報彙總、分類佔比與趨勢計算於 `BookKeeping2/Services/Reports/ReportService.cs`
- [ ] T072 [US4] 實作報表 PageModel 與月份輸入驗證於 `BookKeeping2/Pages/Reports/Index.cshtml.cs`
- [ ] T073 [US4] 實作報表 Razor Page 與空白狀態於 `BookKeeping2/Pages/Reports/Index.cshtml`
- [ ] T074 [P] [US4] 加入 Chart.js 4.x vendored asset 於 `BookKeeping2/wwwroot/lib/chart.js/chart.umd.min.js`
- [ ] T075 [US4] 加入報表圖表初始化腳本於 `BookKeeping2/wwwroot/js/reports.js`
- [ ] T076 [US4] 執行並修正報表相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 4 必須可獨立驗證所有報表數值來自同一批交易資料。

---

## Phase 7: User Story 5 - 設定與追蹤分類預算 (Priority: P2)

**Goal**: 使用者可為支出分類設定每月預算，系統顯示使用率、剩餘/超支金額與 80%/100% 提醒。

**Independent Test**: 設定餐飲每月 TWD 5,000 預算，新增支出至 80% 與超過 100%，確認提醒與首頁進度正確。

### Tests for User Story 5（必須先寫且先失敗）

- [ ] T077 [P] [US5] 撰寫預算使用率、剩餘金額、超支金額、月份重算與 1 秒內提醒狀態計算單元測試於 `BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs`
- [ ] T078 [P] [US5] 撰寫預算管理頁面整合測試於 `BookKeeping2.Tests/Integration/Pages/BudgetsPageTests.cs`
- [ ] T079 [P] [US5] 撰寫首頁預算進度與 1 秒內提醒呈現整合測試於 `BookKeeping2.Tests/Integration/Pages/HomeBudgetSummaryTests.cs`
- [ ] T080 [US5] 執行預算相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 5

- [ ] T081 [P] [US5] 建立預算 ViewModel 於 `BookKeeping2/ViewModels/Budgets/BudgetViewModels.cs`
- [ ] T082 [US5] 建立預算服務介面與提醒狀態型別於 `BookKeeping2/Services/Budgets/IBudgetService.cs`、`BookKeeping2/Services/Budgets/BudgetAlertState.cs`
- [ ] T083 [US5] 實作預算 CRUD、支出彙總、80%/100% 提醒與稽核事件於 `BookKeeping2/Services/Budgets/BudgetService.cs`
- [ ] T084 [US5] 實作預算管理 PageModel 於 `BookKeeping2/Pages/Budgets/Index.cshtml.cs`
- [ ] T085 [US5] 實作預算管理 Razor Page 與提醒 UI 於 `BookKeeping2/Pages/Budgets/Index.cshtml`
- [ ] T086 [US5] 更新首頁 PageModel 顯示預算進度、帳戶餘額與最近交易於 `BookKeeping2/Pages/Index.cshtml.cs`
- [ ] T087 [US5] 更新首頁 Razor Page 顯示本月收入、支出、結餘、預算進度與空白狀態於 `BookKeeping2/Pages/Index.cshtml`
- [ ] T088 [US5] 執行並修正預算相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 5 必須可獨立展示預算設定、提醒與首頁進度。

---

## Phase 8: User Story 6 - 匯出交易資料為 CSV (Priority: P2)

**Goal**: 使用者可依日期範圍匯出未刪除交易為 RFC 4180 相容 CSV，並防範試算表公式注入。

**Independent Test**: 建立 100 筆交易與含逗號、引號、換行、公式風險的備註後匯出，確認欄位、筆數、日期範圍、特殊字元與安全前綴正確。

### Tests for User Story 6（必須先寫且先失敗）

- [ ] T089 [P] [US6] 撰寫 CSV 匯出欄位順序、日期範圍、未刪除交易規則與 1,000 筆匯出 5 秒內完成單元測試於 `BookKeeping2.Tests/Unit/Csv/CsvExportServiceTests.cs`
- [ ] T090 [P] [US6] 撰寫 CSV 特殊字元與公式注入防護單元測試於 `BookKeeping2.Tests/Unit/Csv/CsvExportSecurityTests.cs`
- [ ] T091 [P] [US6] 撰寫 CSV 匯出頁面下載回應整合測試於 `BookKeeping2.Tests/Integration/Pages/CsvExportPageTests.cs`
- [ ] T092 [US6] 執行 CSV 匯出相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 6

- [ ] T093 [P] [US6] 建立 CSV row 與匯出選項型別於 `BookKeeping2/Services/Csv/CsvTransactionRow.cs`、`BookKeeping2/Services/Csv/CsvExportOptions.cs`
- [ ] T094 [US6] 建立 CSV 匯出服務介面於 `BookKeeping2/Services/Csv/ICsvExportService.cs`
- [ ] T095 [US6] 實作 CsvHelper 匯出、RFC 4180 寫入與公式注入前綴於 `BookKeeping2/Services/Csv/CsvExportService.cs`
- [ ] T096 [US6] 實作 CSV 匯出 PageModel、no-store 下載回應與稽核事件於 `BookKeeping2/Pages/Csv/Export.cshtml.cs`
- [ ] T097 [US6] 實作 CSV 匯出 Razor Page 與日期範圍表單於 `BookKeeping2/Pages/Csv/Export.cshtml`
- [ ] T098 [US6] 執行並修正 CSV 匯出相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 6 必須可獨立下載安全且可被試算表解析的 CSV。

---

## Phase 9: User Story 7 - 匯入標準 CSV 交易資料 (Priority: P3)

**Goal**: 使用者可匯入固定六欄 CSV，系統逐列驗證，有效列建立交易，錯誤列跳過並顯示行號與原因。

**Independent Test**: 匯入含 10 筆有效交易與數筆錯誤列的標準 CSV，確認成功列建立、錯誤列未建立、不存在分類自動建立且摘要完整。

### Tests for User Story 7（必須先寫且先失敗）

- [ ] T099 [P] [US7] 撰寫 CSV 標題列、欄位數、空檔案、檔案大小限制與 100 筆標準 CSV 10 秒內解析單元測試於 `BookKeeping2.Tests/Unit/Csv/CsvImportParserTests.cs`
- [ ] T100 [P] [US7] 撰寫 CSV 逐列驗證、自動建立分類、部分失敗與 100 筆匯入 10 秒內完成單元測試於 `BookKeeping2.Tests/Unit/Csv/CsvImportServiceTests.cs`
- [ ] T101 [P] [US7] 撰寫 CSV 匯入頁面、錯誤摘要與 antiforgery 整合測試於 `BookKeeping2.Tests/Integration/Pages/CsvImportPageTests.cs`
- [ ] T102 [US7] 執行 CSV 匯入相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 7

- [ ] T103 [P] [US7] 建立 CSV 匯入命令與結果型別於 `BookKeeping2/Services/Csv/CsvImportCommand.cs`、`BookKeeping2/Services/Csv/CsvImportResult.cs`
- [ ] T104 [US7] 建立 CSV 匯入服務介面於 `BookKeeping2/Services/Csv/ICsvImportService.cs`
- [ ] T105 [US7] 實作 CSV row parser、標題驗證與錯誤原因產生於 `BookKeeping2/Services/Csv/CsvImportParser.cs`
- [ ] T106 [US7] 實作 CSV 匯入 transaction、逐列驗證、分類自動建立與批次摘要於 `BookKeeping2/Services/Csv/CsvImportService.cs`
- [ ] T107 [US7] 實作匯入結果格式化與遮罩預覽於 `BookKeeping2/Services/Csv/CsvImportResultFormatter.cs`
- [ ] T108 [US7] 實作 CSV 匯入 PageModel 與上傳限制於 `BookKeeping2/Pages/Csv/Import.cshtml.cs`
- [ ] T109 [US7] 實作 CSV 匯入 Razor Page 與錯誤摘要 UI 於 `BookKeeping2/Pages/Csv/Import.cshtml`
- [ ] T110 [US7] 執行並修正 CSV 匯入相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 7 必須可獨立處理有效列、錯誤列與自動分類摘要。

---

## Phase 10: User Story 8 - 搜尋與篩選交易明細 (Priority: P3)

**Goal**: 使用者可依關鍵字、日期範圍、分類、帳戶與金額範圍搜尋或篩選交易，且大量資料仍可用。

**Independent Test**: 建立至少 50 筆不同日期、分類、帳戶與備註交易後，分別套用分類、日期、金額與關鍵字篩選，確認結果只包含符合所有條件的交易。

### Tests for User Story 8（必須先寫且先失敗）

- [ ] T111 [P] [US8] 撰寫交易查詢條件 AND 邏輯、日期與金額範圍單元測試於 `BookKeeping2.Tests/Unit/Transactions/TransactionQueryServiceTests.cs`
- [ ] T112 [P] [US8] 撰寫交易明細篩選頁面整合測試於 `BookKeeping2.Tests/Integration/Pages/TransactionFilterPageTests.cs`
- [ ] T113 [P] [US8] 撰寫 10,000 筆資料篩選效能整合測試於 `BookKeeping2.Tests/Integration/Performance/TransactionQueryPerformanceTests.cs`
- [ ] T114 [US8] 執行搜尋篩選相關測試、確認因尚未實作而失敗，並取得使用者或維護者對測試意圖確認於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

### Implementation for User Story 8

- [ ] T115 [P] [US8] 建立交易篩選輸入與分頁 ViewModel 於 `BookKeeping2/ViewModels/Transactions/TransactionFilterInputModel.cs`、`BookKeeping2/ViewModels/Transactions/PagedTransactionListViewModel.cs`
- [ ] T116 [US8] 建立交易查詢服務介面與查詢條件型別於 `BookKeeping2/Services/Transactions/ITransactionQueryService.cs`、`BookKeeping2/Services/Transactions/TransactionQuery.cs`
- [ ] T117 [US8] 實作交易查詢、分頁、關鍵字搜尋與索引友善排序於 `BookKeeping2/Services/Transactions/TransactionQueryService.cs`
- [ ] T118 [US8] 更新交易明細 PageModel 支援篩選、分頁與欄位驗證於 `BookKeeping2/Pages/Transactions/Index.cshtml.cs`
- [ ] T119 [US8] 更新交易明細 Razor Page 加入篩選表單與結果摘要於 `BookKeeping2/Pages/Transactions/Index.cshtml`
- [ ] T120 [US8] 建立共用分頁 partial 於 `BookKeeping2/Pages/Shared/_Pagination.cshtml`
- [ ] T121 [US8] 執行並修正搜尋篩選相關測試直到通過於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 8 必須可獨立驗證複合篩選與大量資料可用性。

---

## Phase 11: Polish & Cross-Cutting Concerns

**目的**: 完成跨故事品質、文件、安全、效能與回應式驗證。

- [ ] T122 [P] 更新快速入門與手動驗證清單於 `specs/001-personal-bookkeeping-tool/quickstart.md`
- [ ] T123 [P] 更新使用者文件或 README 連結於 `docs/readme-template.md`
- [ ] T124 [P] 調整回應式版面、320px 寬度與圖表容器樣式於 `BookKeeping2/wwwroot/css/site.css`
- [ ] T125 [P] 統一 toast、alert 與可關閉錯誤訊息互動於 `BookKeeping2/wwwroot/js/site.js`
- [ ] T126 檢查主要導覽入口（首頁、明細、新增、報表、設定或更多功能）與所有使用者面向 UI 文字、驗證訊息為繁體中文於 `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T127 檢查敏感財務資料遮罩、CSV 稽核與預算警告稽核於 `BookKeeping2/Services/Audit/AuditLogMaskingPolicy.cs`
- [ ] T128 檢查 CSP、HTTPS/HSTS、antiforgery、no-store 下載回應與安全 header 風險於 `BookKeeping2/Services/Security/SecurityHeadersExtensions.cs`，並將套件弱點掃描結果記錄於 `specs/001-personal-bookkeeping-tool/quickstart.md`
- [ ] T129 執行全套測試、收集 coverage，確認關鍵業務邏輯測試覆蓋率達 80% 以上並修正失敗於 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [ ] T130 執行 web 專案建置、XML 文件註解檢查並修正警告或錯誤於 `BookKeeping2/BookKeeping2.csproj`
- [ ] T131 依 quickstart 完成 320px 手機、常見桌面寬度與 30 秒內首次新增交易手動驗證紀錄於 `specs/001-personal-bookkeeping-tool/quickstart.md`
- [ ] T132 [P] 執行 WCAG 2.1 AA 核心可及性驗證，涵蓋鍵盤操作、語意標記與對比度，並記錄結果於 `specs/001-personal-bookkeeping-tool/quickstart.md`
- [ ] T133 [P] 執行隱私與未授權第三方傳輸檢查，確認未經使用者明確操作或部署設定允許時外部網路請求為 0，並記錄結果於 `specs/001-personal-bookkeeping-tool/quickstart.md`
- [ ] T134 執行成功標準時間量測，涵蓋 SC-002、SC-003、SC-004、SC-005、SC-007 與 SC-008，並記錄量測資料於 `specs/001-personal-bookkeeping-tool/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 無相依，可立即開始
- **Foundational (Phase 2)**: 依賴 Setup 完成，阻塞所有使用者故事
- **User Stories (Phase 3+)**: 依賴 Foundational 完成，之後依 P1 -> P2 -> P3 交付；不同故事可在避免同檔衝突時平行
- **Polish (Phase 11)**: 依賴目標使用者故事完成

### User Story Dependencies

- **User Story 1 (P1)**: Foundational 後即可開始；測試可用 fixture 建立既有分類與帳戶
- **User Story 2 (P1)**: Foundational 後即可開始；驗證持久化與跨 client 一致性
- **User Story 3 (P1)**: Foundational 後即可開始；提供分類與帳戶管理頁面，讓 US1 成為完整可用流程
- **User Story 4 (P2)**: 需要交易資料來源；可在 US1/US2 完成後交付
- **User Story 5 (P2)**: 需要支出分類與交易資料；可在 US1/US3 完成後交付
- **User Story 6 (P2)**: 需要交易查詢資料來源；可在 US1/US2 完成後交付
- **User Story 7 (P3)**: 需要交易、分類、帳戶與 CSV 基礎；可在 US1/US3/US6 後交付
- **User Story 8 (P3)**: 需要交易列表基礎；可在 US1/US2 完成後交付

### Within Each User Story

- 測試 MUST 先撰寫並先失敗
- ViewModel/InputModel before Services when service contracts need input shape
- Services before Razor Pages
- 驗證、錯誤處理、日誌與稽核不得延後到故事完成後才補
- 每個故事完成後 MUST 獨立驗證，再進入下一個優先級

### Parallel Opportunities

- T003、T004、T005、T007 可在 T002 建立測試專案後平行
- T011、T012、T013、T014、T015、T016 可在 T008、T009、T010 後平行
- 每個故事的 `[P]` 測試任務可平行撰寫，但同一故事的實作任務應依序完成以避免同檔衝突
- US4、US5、US6 在 P1 故事完成後可由不同開發者平行處理
- US7 與 US8 在相關 P1/P2 基礎完成後可平行處理
- T122、T123、T124、T125、T126、T127、T128、T132、T133 可在目標故事完成後平行；T134 必須在相關效能測試與 UI 流程完成後執行

---

## Parallel Examples

### User Story 1

```text
Task: "T026 撰寫交易驗證單元測試於 BookKeeping2.Tests/Unit/Transactions/TransactionServiceValidationTests.cs"
Task: "T027 撰寫 minor units 單元測試於 BookKeeping2.Tests/Unit/Common/MoneyMinorUnitConverterTests.cs"
Task: "T028 撰寫交易頁面整合測試於 BookKeeping2.Tests/Integration/Pages/TransactionPagesTests.cs"
```

### User Story 3

```text
Task: "T052 撰寫分類服務單元測試於 BookKeeping2.Tests/Unit/Categories/CategoryServiceTests.cs"
Task: "T053 撰寫帳戶服務單元測試於 BookKeeping2.Tests/Unit/Accounts/AccountServiceTests.cs"
Task: "T054 撰寫分類與帳戶頁面整合測試於 BookKeeping2.Tests/Integration/Pages/CategoryAndAccountPagesTests.cs"
```

### User Story 4/5/6 After P1

```text
Task: "T066 撰寫報表服務單元測試於 BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs"
Task: "T077 撰寫預算服務單元測試於 BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs"
Task: "T089 撰寫 CSV 匯出服務單元測試於 BookKeeping2.Tests/Unit/Csv/CsvExportServiceTests.cs"
```

### User Story 7/8 After CSV Export And Transaction List

```text
Task: "T099 撰寫 CSV 匯入 parser 單元測試於 BookKeeping2.Tests/Unit/Csv/CsvImportParserTests.cs"
Task: "T111 撰寫交易查詢服務單元測試於 BookKeeping2.Tests/Unit/Transactions/TransactionQueryServiceTests.cs"
Task: "T113 撰寫 10,000 筆資料效能測試於 BookKeeping2.Tests/Integration/Performance/TransactionQueryPerformanceTests.cs"
```

---

## Implementation Strategy

### MVP First（可展示 P1 基礎記帳）

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. 完成 Phase 4: User Story 2
5. 完成 Phase 5: User Story 3
6. 停下並驗證交易 CRUD、持久化、分類/帳戶與首頁基本摘要

**MVP scope**: 本規格的實用 MVP 應包含 US1 + US2 + US3，因為交易輸入需要可靠持久化、分類與帳戶才能完整展示。

### Incremental Delivery

1. P1: 完成 US1、US2、US3，交付可用記帳核心
2. P2: 加入 US4 報表、US5 預算、US6 CSV 匯出
3. P3: 加入 US7 CSV 匯入與 US8 搜尋篩選
4. 每個故事完成時通過自己的單元、整合與必要手動驗證

### Parallel Team Strategy

1. 團隊共同完成 Setup 與 Foundational
2. P1 完成後，報表、預算、CSV 匯出可分派給不同開發者
3. P3 可由 CSV 匯入與交易查詢兩條工作線平行推進
4. 合併前執行 `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj`、coverage 80% 門檻檢查、套件弱點掃描、可及性/隱私驗證與 `dotnet build BookKeeping2/BookKeeping2.csproj`

---

## Notes

- `[P]` 任務代表不同檔案且無相依
- `[Story]` 標籤只用於使用者故事階段
- 測試失敗原因 MUST 對應尚未實作的需求
- 金額相關程式碼 MUST 使用 `decimal`，SQLite 儲存使用 minor units
- UI、文件、驗證訊息與使用者回饋 MUST 使用繁體中文 zh-TW
- 狀態改變表單 MUST 使用 antiforgery，CSV 下載 MUST 避免快取敏感內容
- 稽核與日誌 MUST 遮罩敏感財務資料，不得記錄完整備註或不必要的完整金額
