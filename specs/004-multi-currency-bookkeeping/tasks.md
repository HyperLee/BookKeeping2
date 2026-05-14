# Tasks: 多幣別獨立記帳

**Input**: Design documents from `/specs/004-multi-currency-bookkeeping/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/multi-currency-contract.md`, `quickstart.md`
**Tests**: 本功能規格、plan 與 quickstart 明確要求測試優先；每個故事的測試意圖必須先取得使用者或維護者確認，再撰寫測試、確認測試失敗，最後實作。
**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when it touches different files and has no dependency on incomplete tasks
- **[Story]**: User story label (`US1` through `US5`) for story-phase tasks only
- Every task includes exact repository file paths

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 確認 feature artifacts、現有測試基礎與共享測試資料形狀，避免後續故事重複建立 fixture。

- [X] T001 確認多幣別需求、各故事測試意圖、TDD 順序與排除範圍一致，取得使用者或維護者對測試意圖的確認，並記錄於 `specs/004-multi-currency-bookkeeping/spec.md`
- [X] T002 [P] 檢查多幣別技術決策與檔案配置是否仍符合目前專案結構，更新 `specs/004-multi-currency-bookkeeping/plan.md`
- [X] T003 [P] 檢查 CSV、報表、帳戶、預算與 UI 契約是否涵蓋所有故事測試案例，更新 `specs/004-multi-currency-bookkeeping/contracts/multi-currency-contract.md`
- [X] T004 [P] 擴充多幣別測試資料建構器介面與 helper 命名規劃，準備修改 `BookKeeping2.Tests/TestSupport/TestDataBuilder.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立所有故事共用的幣別 allow-list、schema、migration、驗證訊息與測試資料基礎。

**CRITICAL**: 此階段完成前，不應開始任何 user story implementation。

### Tests First

- [X] T005 [P] 新增 `SupportedCurrencyTests`，覆蓋 trim、大小寫不敏感、上層大寫正規化、五種顯示名稱、空白與不支援值拒絕於 `BookKeeping2.Tests/Unit/Common/SupportedCurrencyTests.cs`
- [X] T006 [P] 新增 migration/model snapshot 測試，先驗證 `Transactions.Currency`、`Budgets.Currency`、`Accounts.Currency`、預設 `TWD`、交易複合索引與預算唯一索引於 `BookKeeping2.Tests/Integration/Persistence/MultiCurrencyPersistenceTests.cs`
- [X] T007 [P] 更新測試資料 builder 測試需求，讓 seeded transaction/account/budget 可指定幣別於 `BookKeeping2.Tests/TestSupport/TestDataBuilder.cs`

### Implementation

- [X] T008 實作固定五種幣別 allow-list、正規化、顯示名稱與選項排序於 `BookKeeping2/Models/Common/SupportedCurrency.cs`
- [X] T009 更新多幣別驗證與使用者訊息常數，確保繁體中文錯誤文案可重用於 `BookKeeping2/Validation/FinancialValidationMessages.cs`
- [X] T010 在交易模型新增 `Currency`、更新 XML docs 與金額說明不再寫死 TWD 於 `BookKeeping2/Models/Transactions/Transaction.cs`
- [X] T011 在預算模型新增 `Currency`、更新 XML docs 與金額說明不再寫死 TWD 於 `BookKeeping2/Models/Budgets/Budget.cs`
- [X] T012 更新帳戶模型 XML docs，說明 account currency 固定、開戶餘額屬於 account currency 於 `BookKeeping2/Models/Accounts/Account.cs`
- [X] T013 更新交易、帳戶與預算 EF Core configuration 的 required/max length/default/index/unique index 規則於 `BookKeeping2/Data/EntityConfigurations/TransactionConfiguration.cs`, `BookKeeping2/Data/EntityConfigurations/AccountConfiguration.cs`, `BookKeeping2/Data/EntityConfigurations/BudgetConfiguration.cs`
- [X] T014 新增 EF Core migration，backfill 既有交易、帳戶與預算為 `TWD` 且保持 minor units 不變於 `BookKeeping2/Data/Migrations/20260514000000_AddMultiCurrencyBookkeeping.cs`
- [X] T015 更新 EF Core model snapshot 與預設 seed data 幣別，確保 schema 與測試 fixture 一致於 `BookKeeping2/Data/Migrations/AppDbContextModelSnapshot.cs`, `BookKeeping2/Data/SeedData/DefaultSeedData.cs`, `BookKeeping2.Tests/TestSupport/TestDataBuilder.cs`

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel.

---

## Phase 3: User Story 1 - 記錄不同幣別交易 (Priority: P1) MVP

**Goal**: 使用者可以新增、編輯、篩選並查看含 `TWD`、`USD`、`JPY`、`EUR`、`GBP` 的交易，每筆金額都清楚伴隨幣別，且帳戶幣別必須一致。

**Independent Test**: 建立 `TWD 100.00` 與 `USD 100.00` 相同日期/分類支出，列表顯示兩筆獨立交易且不合計；編輯幣別時同幣別帳戶驗證生效。

### Tests for User Story 1

- [X] T016 [P] [US1] 擴充交易服務驗證測試，先覆蓋必填/不支援幣別、帳戶幣別不一致、同日期分類金額不同幣別可共存、金額大於 `0`/上限 `999,999,999.99`/最多 2 位小數且包含 `JPY`、duplicate detection 包含幣別、audit summary 包含幣別與名稱/備註/分類/帳戶文字不被翻譯或改寫於 `BookKeeping2.Tests/Unit/Transactions/TransactionServiceValidationTests.cs`
- [X] T017 [P] [US1] 擴充交易查詢測試，先覆蓋幣別 filter 與既有日期/分類/帳戶/金額/關鍵字/pagination 並存於 `BookKeeping2.Tests/Unit/Transactions/TransactionQueryServiceTests.cs`
- [X] T018 [P] [US1] 擴充交易頁面整合測試，先覆蓋 create/edit/list/delete confirmation 與既有交易詳細資料載入流程顯示幣別、invalid POST 拒絕、anti-forgery 保留，以及未依瀏覽器語言、所在地、日期或其他環境訊號推定幣別於 `BookKeeping2.Tests/Integration/Pages/TransactionPagesTests.cs`
- [X] T019 [P] [US1] 新增多幣別交易表單瀏覽器測試，先覆蓋桌面與行動 viewport 的幣別控制、帳戶選項、焦點與無重疊於 `BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs`

### Implementation for User Story 1

- [X] T020 [US1] 擴充交易 input/filter/options/list view models，加入 `Currency`、幣別選項、列表顯示文字與 filter 狀態於 `BookKeeping2/ViewModels/Transactions/TransactionInputModel.cs`, `BookKeeping2/ViewModels/Transactions/TransactionFilterInputModel.cs`, `BookKeeping2/ViewModels/Transactions/TransactionFormOptionsViewModel.cs`, `BookKeeping2/ViewModels/Transactions/TransactionListItemViewModel.cs`, `BookKeeping2/ViewModels/Transactions/PagedTransactionListViewModel.cs`
- [X] T021 [US1] 更新交易 form options，提供支援幣別清單並依幣別呈現/篩選 account options 於 `BookKeeping2/Services/Transactions/TransactionFormOptionsService.cs`
- [X] T022 [US1] 更新交易 create/edit validation、currency normalization、同幣別帳戶驗證、duplicate detection 與 audit summary 於 `BookKeeping2/Services/Transactions/TransactionService.cs`
- [X] T023 [US1] 更新交易查詢 service，加入 optional currency filter、投影 currency、保留 soft-delete 排除與 pagination 於 `BookKeeping2/Services/Transactions/TransactionQueryService.cs`
- [X] T024 [US1] 更新交易 query object，加入 currency filter 並保持既有 filter 相容於 `BookKeeping2/Services/Transactions/TransactionQuery.cs`
- [X] T025 [US1] 更新交易共用表單與 create/edit/delete/list Razor markup，加入 required currency control、幣別驗證訊息與每筆金額相鄰幣別於 `BookKeeping2/Pages/Transactions/_TransactionForm.cshtml`, `BookKeeping2/Pages/Transactions/Create.cshtml`, `BookKeeping2/Pages/Transactions/Edit.cshtml`, `BookKeeping2/Pages/Transactions/Delete.cshtml`, `BookKeeping2/Pages/Transactions/Index.cshtml`
- [X] T026 [US1] 更新交易 PageModel binding、invalid post reload options、filter persistence 與 edit currency/account mismatch handling 於 `BookKeeping2/Pages/Transactions/TransactionFormPageModel.cs`, `BookKeeping2/Pages/Transactions/Create.cshtml.cs`, `BookKeeping2/Pages/Transactions/Edit.cshtml.cs`, `BookKeeping2/Pages/Transactions/Index.cshtml.cs`, `BookKeeping2/Pages/Transactions/Delete.cshtml.cs`
- [X] T027 [US1] 更新交易頁面 client-side enhancement，讓幣別切換時帳戶選項提示一致且不取代 server-side validation 於 `BookKeeping2/wwwroot/js/transactions.js`
- [X] T028 [US1] 更新交易表格與表單 responsive/focus CSS，確保金額與幣別相鄰且手機無水平溢位於 `BookKeeping2/wwwroot/css/site.css`

**Checkpoint**: User Story 1 should be fully functional and testable independently.

---

## Phase 4: User Story 2 - 查看分幣別摘要與報表 (Priority: P1)

**Goal**: 首頁摘要、帳戶餘額、月報、分類統計與趨勢圖只在同幣別內計算，並以分幣別 bucket 呈現。

**Independent Test**: 使用 seeded `TWD` 與 `USD` 同月收入/支出資料，首頁與月報分別顯示各幣別收入、支出、結餘、分類佔比與趨勢，不顯示跨幣別總額。

### Tests for User Story 2

- [X] T029 [P] [US2] 擴充報表服務測試，先覆蓋 monthly totals、category shares、trend points 依幣別分組且不跨幣別合計於 `BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs`
- [X] T030 [P] [US2] 擴充首頁預算/摘要整合測試，先覆蓋多幣別摘要 bucket、單幣別月份與空白月份顯示於 `BookKeeping2.Tests/Integration/Pages/HomeBudgetSummaryTests.cs`
- [X] T031 [P] [US2] 擴充報表頁整合測試，先覆蓋月報、分類統計與趨勢 chart payload 不含跨幣別總額於 `BookKeeping2.Tests/Integration/Pages/ReportsPageTests.cs`
- [X] T032 [P] [US2] 擴充交易查詢效能測試，先覆蓋 10,000 筆交易 currency filter 與 grouping 目標於 `BookKeeping2.Tests/Integration/Performance/TransactionQueryPerformanceTests.cs`

### Implementation for User Story 2

- [X] T033 [US2] 重構報表 view model 為 currency bucket、category share 與 trend point 結構於 `BookKeeping2/ViewModels/Reports/MonthlyReportViewModel.cs`
- [X] T034 [US2] 更新報表 chart point 結構，讓 dataset/tooltip/legend 可識別幣別於 `BookKeeping2/Services/Reports/ReportChartPoint.cs`
- [X] T035 [US2] 更新 report service 查詢與 projection，所有 income/expense/balance/category share/trend 均依 `Currency` 分組於 `BookKeeping2/Services/Reports/ReportService.cs`
- [X] T036 [US2] 更新首頁 PageModel 摘要查詢，輸出分幣別收入、支出、結餘與預算狀態於 `BookKeeping2/Pages/Index.cshtml.cs`
- [X] T037 [US2] 更新首頁 Razor markup，移除跨幣別總額呈現並以繁體中文分幣別區塊顯示於 `BookKeeping2/Pages/Index.cshtml`
- [X] T038 [US2] 更新報表 PageModel 與 Razor markup，月報、分類統計與空白狀態依幣別呈現於 `BookKeeping2/Pages/Reports/Index.cshtml.cs`, `BookKeeping2/Pages/Reports/Index.cshtml`
- [X] T039 [US2] 更新 Chart.js 初始化，legend、dataset labels、tooltip 與 accessibility text 包含幣別且不混合資料於 `BookKeeping2/wwwroot/js/reports.js`

**Checkpoint**: User Story 2 should be fully functional and testable independently with seeded multi-currency data.

---

## Phase 5: User Story 3 - 以帳戶幣別防止混算 (Priority: P1)

**Goal**: 每個帳戶建立時必須明確選擇支援幣別，建立後不可修改，帳戶餘額只由同帳戶同幣別交易影響且不得顯示合併餘額。

**Independent Test**: 建立 `TWD` 與 `USD` 帳戶後，新增 `USD` 交易只能使用 `USD` 帳戶；帳戶列表分別顯示 `TWD 10,000.00` 與 `USD 500.00`，不顯示 `10,500.00`。

### Tests for User Story 3

- [X] T040 [P] [US3] 擴充帳戶服務測試，先覆蓋建立帳戶必填幣別、全域名稱唯一、拒絕修改幣別與同幣別餘額計算於 `BookKeeping2.Tests/Unit/Accounts/AccountServiceTests.cs`
- [X] T041 [P] [US3] 擴充帳戶頁整合測試，先覆蓋新增帳戶幣別控制、空白幣別拒絕、列表顯示幣別與不顯示合併餘額於 `BookKeeping2.Tests/Integration/Pages/CategoryAndAccountPagesTests.cs`
- [X] T042 [P] [US3] 擴充多幣別帳戶瀏覽器測試，先覆蓋桌面與行動建立帳戶、鍵盤操作、visible focus 與金額幣別不重疊於 `BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs`

### Implementation for User Story 3

- [X] T043 [US3] 擴充 account input/list/balance view models，加入 required `Currency`、display text 與分幣別餘額欄位於 `BookKeeping2/ViewModels/Accounts/AccountViewModels.cs`
- [X] T044 [US3] 更新 account service 建立流程，要求明確幣別、正規化儲存、保持全域名稱唯一並拒絕 unsupported/blank currency 於 `BookKeeping2/Services/Accounts/AccountService.cs`
- [X] T045 [US3] 更新 account service 編輯流程，禁止修改既有帳戶 `Currency` 並保留可編輯欄位既有行為於 `BookKeeping2/Services/Accounts/AccountService.cs`
- [X] T046 [US3] 更新帳戶餘額 projection，opening balance 加上同帳戶同幣別未刪除交易，並移除跨幣別 total shape 於 `BookKeeping2/Services/Accounts/AccountBalanceSummary.cs`, `BookKeeping2/Services/Accounts/AccountService.cs`
- [X] T047 [US3] 更新帳戶 PageModel binding、validation summary 與 invalid post reload 行為於 `BookKeeping2/Pages/Accounts/Index.cshtml.cs`
- [X] T048 [US3] 更新帳戶 Razor markup，新增幣別 select、帳戶列表幣別 badge/label 與繁體中文錯誤訊息於 `BookKeeping2/Pages/Accounts/Index.cshtml`
- [X] T049 [US3] 更新交易 form account option 顯示，讓帳戶名稱旁可辨識帳戶幣別於 `BookKeeping2/ViewModels/Transactions/TransactionFormOptionsViewModel.cs`, `BookKeeping2/Pages/Transactions/_TransactionForm.cshtml`
- [X] T050 [US3] 更新帳戶與交易相關 responsive/focus CSS，確保幣別 badge、餘額與控制項手機不溢位於 `BookKeeping2/wwwroot/css/site.css`

**Checkpoint**: User Story 3 should be fully functional and testable independently.

---

## Phase 6: User Story 4 - 依幣別管理月預算 (Priority: P2)

**Goal**: 同月份同分類可建立不同幣別預算，同月份同分類同幣別不得重複，預算進度、剩餘、超支與提醒只使用相同幣別支出交易。

**Independent Test**: 建立 `2026-05` 餐飲 `TWD` 與 `USD` 預算，新增 `USD` 餐飲支出只更新 `USD` 預算；再次建立 `USD` 餐飲預算被拒絕。

### Tests for User Story 4

- [X] T051 [P] [US4] 擴充預算服務測試，先覆蓋 month/category/currency unique、同分類不同幣別允許、金額大於 `0`/上限 `999,999,999.99`/最多 2 位小數且包含 `JPY`、spent/remaining/alert 只用同幣別交易於 `BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs`
- [X] T052 [P] [US4] 擴充預算頁整合測試，先覆蓋幣別控制、重複預算錯誤、列表進度與多幣別顯示於 `BookKeeping2.Tests/Integration/Pages/BudgetsPageTests.cs`
- [X] T053 [P] [US4] 擴充多幣別預算瀏覽器測試，先覆蓋桌面與行動建立預算、進度顯示、焦點與無水平溢位於 `BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs`

### Implementation for User Story 4

- [X] T054 [US4] 擴充 budget input/list/progress view models，加入 `Currency`、currency options 與 display text 於 `BookKeeping2/ViewModels/Budgets/BudgetViewModels.cs`
- [X] T055 [US4] 更新 budget service 建立與更新 validation，依 month/category/currency 檢查唯一性並允許不同幣別共存於 `BookKeeping2/Services/Budgets/BudgetService.cs`
- [X] T056 [US4] 更新 budget service 進度計算，spent/remaining/overspent/alert 只納入同月同分類同幣別未刪除支出交易於 `BookKeeping2/Services/Budgets/BudgetService.cs`
- [X] T057 [US4] 更新 budget result/error code，新增幣別相關錯誤訊息與重複提示於 `BookKeeping2/Services/Budgets/BudgetResult.cs`
- [X] T058 [US4] 更新預算 PageModel binding、query month reload、invalid post reload options 與 model errors 於 `BookKeeping2/Pages/Budgets/Index.cshtml.cs`
- [X] T059 [US4] 更新預算 Razor markup，新增幣別 select、每筆預算金額與使用進度幣別標籤於 `BookKeeping2/Pages/Budgets/Index.cshtml`
- [X] T060 [US4] 更新首頁預算摘要，依幣別呈現預算警示且不把不同幣別混入單一提醒於 `BookKeeping2/Pages/Index.cshtml.cs`, `BookKeeping2/Pages/Index.cshtml`
- [X] T061 [US4] 更新預算與首頁摘要 responsive/focus CSS，確保幣別與金額相鄰且不遮擋於 `BookKeeping2/wwwroot/css/site.css`

**Checkpoint**: User Story 4 should be fully functional and testable independently.

---

## Phase 7: User Story 5 - 匯入匯出多幣別資料 (Priority: P2)

**Goal**: 匯出七欄包含原始幣別；匯入支援新版七欄與既有六欄，七欄空白/不支援幣別與帳戶幣別不一致都以 row-level error 回報。

**Independent Test**: 匯出 header 為 `日期,類型,幣別,金額,分類,帳戶,備註`；匯入 `EUR` matching account 成功、`AUD` 失敗、七欄空白幣別失敗、六欄 legacy 預設 `TWD`、`USD` row 搭配 `TWD` account 失敗。

### Tests for User Story 5

- [X] T062 [P] [US5] 擴充 CSV parser 測試，先覆蓋七欄 header、六欄 legacy header、malformed header、七欄空白幣別與正規化於 `BookKeeping2.Tests/Unit/Csv/CsvImportParserTests.cs`
- [X] T063 [P] [US5] 擴充 CSV import service 測試，先覆蓋支援幣別成功、legacy rows 預設 `TWD`、unsupported currency、帳戶幣別不一致、金額規則錯誤、匯入文字原文保留與 row-level errors 於 `BookKeeping2.Tests/Unit/Csv/CsvImportServiceTests.cs`
- [X] T064 [P] [US5] 擴充 CSV export service 測試，先覆蓋七欄 header、原始 amount/currency、soft-delete 排除與 formula protection 保留於 `BookKeeping2.Tests/Unit/Csv/CsvExportServiceTests.cs`
- [X] T065 [P] [US5] 擴充 CSV 匯入匯出頁整合測試，先覆蓋成功/失敗摘要、繁體中文錯誤訊息與下載內容於 `BookKeeping2.Tests/Integration/Pages/CsvImportPageTests.cs`, `BookKeeping2.Tests/Integration/Pages/CsvExportPageTests.cs`
- [X] T066 [P] [US5] 擴充 CSV 瀏覽器測試，先覆蓋匯入/匯出頁桌面與行動布局、幣別欄位說明與可操作性於 `BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs`

### Implementation for User Story 5

- [X] T067 [US5] 更新 CSV row/command/result shapes，加入 `Currency`、legacy row indicator 與 safe row error context 於 `BookKeeping2/Services/Csv/CsvTransactionRow.cs`, `BookKeeping2/Services/Csv/CsvImportCommand.cs`, `BookKeeping2/Services/Csv/CsvImportResult.cs`
- [X] T068 [US5] 更新 CSV parser，接受七欄與六欄 header、七欄幣別必填、六欄預設 `TWD`、幣別 trim/case-insensitive normalization 於 `BookKeeping2/Services/Csv/CsvImportParser.cs`
- [X] T069 [US5] 更新 CSV import service，驗證交易幣別與帳戶幣別一致、逐列錯誤原因、成功/失敗筆數與 atomic persistence 於 `BookKeeping2/Services/Csv/CsvImportService.cs`
- [X] T070 [US5] 更新 CSV export service，輸出七欄 header、原始幣別與未換算金額，保留 formula injection protection 與 soft-delete 排除於 `BookKeeping2/Services/Csv/CsvExportService.cs`
- [X] T071 [US5] 更新 CSV import/export PageModel，讓結果摘要與下載 metadata 包含幣別 contract 且不記錄原始敏感 CSV 於 `BookKeeping2/Pages/Csv/Import.cshtml.cs`, `BookKeeping2/Pages/Csv/Export.cshtml.cs`
- [X] T072 [US5] 更新 CSV import/export Razor markup，顯示七欄格式、legacy 六欄相容說明與繁體中文 row-level error 於 `BookKeeping2/Pages/Csv/Import.cshtml`, `BookKeeping2/Pages/Csv/Export.cshtml`
- [X] T073 [US5] 更新 CSV result formatter，讓不支援幣別、空白幣別與帳戶幣別不一致的錯誤訊息安全且可理解於 `BookKeeping2/Services/Csv/CsvImportResultFormatter.cs`

**Checkpoint**: User Story 5 should be fully functional and testable independently.

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: 完成跨故事驗證、文件、效能、UI responsive 與 build/test。

- [ ] T074 [P] 更新 quickstart 實作檢核結果與手動驗證紀錄於 `specs/004-multi-currency-bookkeeping/quickstart.md`
- [ ] T075 [P] 更新多幣別 contract 中已實作的相容性與限制備註於 `specs/004-multi-currency-bookkeeping/contracts/multi-currency-contract.md`
- [ ] T076 [P] 檢查所有新增或修改 public types/members XML documentation，修正 `BookKeeping2/Models/Common/SupportedCurrency.cs`
- [ ] T077 執行 targeted unit tests 並修正失敗，使用測試專案 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [ ] T078 執行 persistence/page/performance integration tests 並修正失敗，明確驗證 10,000 筆交易分幣別篩選 < 2 秒、10,000 筆紀錄分幣別查詢 < 3 秒、100 筆月報分幣別產生 < 2 秒、1,000 筆 CSV 匯出 < 5 秒、100 筆 CSV 匯入 < 10 秒，使用測試專案 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [ ] T079 執行 Playwright browser tests；驗證建立新幣別帳戶、建立同幣別交易、在首頁摘要看到該幣別獨立統計的 3 分鐘主要流程，以及主要頁面 FCP < 1.5 秒、LCP < 2.5 秒；若 Chrome/Edge 不可用，記錄 exact blocker 於 `specs/004-multi-currency-bookkeeping/quickstart.md`
- [ ] T080 執行 full build/test 驗證並確認無跨幣別總額 regression，使用 `BookKeeping2.slnx` 與 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Stories (Phase 3+)**: Depend on Foundational completion
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 記錄不同幣別交易 (P1)**: Can start after Foundational; uses seeded same-currency accounts and is MVP scope
- **US2 查看分幣別摘要與報表 (P1)**: Can start after Foundational with seeded multi-currency transactions; does not require US1 UI to be complete
- **US3 以帳戶幣別防止混算 (P1)**: Can start after Foundational; account create/edit UI is independently testable
- **US4 依幣別管理月預算 (P2)**: Can start after Foundational; uses seeded transactions/accounts/categories and integrates with US2 homepage summary after both are available
- **US5 匯入匯出多幣別資料 (P2)**: Can start after Foundational; uses parser/import/export contracts and seeded accounts/categories

### Within Each User Story

- Test intent must be approved by the user or maintainer before writing tests; tests must then be written first and confirmed failing before implementation.
- View models/entities before services.
- Services before PageModels and Razor markup.
- Core behavior before browser/responsive polish.
- Each story should reach its checkpoint before moving to lower-priority scope.

### Parallel Opportunities

- Setup review tasks T002-T004 can run in parallel.
- Foundational tests T005-T007 can run in parallel before shared implementation.
- After T015, US1, US2, US3, US4 and US5 can start in parallel if different owners coordinate shared files.
- Within each story, test files marked `[P]` can be created in parallel.
- CSS/browser test tasks should be coordinated because `BookKeeping2/wwwroot/css/site.css` and `MultiCurrencyBrowserTests.cs` are shared across stories.

---

## Parallel Example: User Story 1

```text
Task: "T016 [US1] 擴充交易服務驗證測試 in BookKeeping2.Tests/Unit/Transactions/TransactionServiceValidationTests.cs"
Task: "T017 [US1] 擴充交易查詢測試 in BookKeeping2.Tests/Unit/Transactions/TransactionQueryServiceTests.cs"
Task: "T018 [US1] 擴充交易頁面整合測試 in BookKeeping2.Tests/Integration/Pages/TransactionPagesTests.cs"
Task: "T019 [US1] 新增多幣別交易表單瀏覽器測試 in BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs"
```

## Parallel Example: User Story 2

```text
Task: "T029 [US2] 擴充報表服務測試 in BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs"
Task: "T030 [US2] 擴充首頁預算/摘要整合測試 in BookKeeping2.Tests/Integration/Pages/HomeBudgetSummaryTests.cs"
Task: "T031 [US2] 擴充報表頁整合測試 in BookKeeping2.Tests/Integration/Pages/ReportsPageTests.cs"
Task: "T032 [US2] 擴充交易查詢效能測試 in BookKeeping2.Tests/Integration/Performance/TransactionQueryPerformanceTests.cs"
```

## Parallel Example: User Story 3

```text
Task: "T040 [US3] 擴充帳戶服務測試 in BookKeeping2.Tests/Unit/Accounts/AccountServiceTests.cs"
Task: "T041 [US3] 擴充帳戶頁整合測試 in BookKeeping2.Tests/Integration/Pages/CategoryAndAccountPagesTests.cs"
Task: "T042 [US3] 擴充多幣別帳戶瀏覽器測試 in BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs"
```

## Parallel Example: User Story 4

```text
Task: "T051 [US4] 擴充預算服務測試 in BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs"
Task: "T052 [US4] 擴充預算頁整合測試 in BookKeeping2.Tests/Integration/Pages/BudgetsPageTests.cs"
Task: "T053 [US4] 擴充多幣別預算瀏覽器測試 in BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs"
```

## Parallel Example: User Story 5

```text
Task: "T062 [US5] 擴充 CSV parser 測試 in BookKeeping2.Tests/Unit/Csv/CsvImportParserTests.cs"
Task: "T063 [US5] 擴充 CSV import service 測試 in BookKeeping2.Tests/Unit/Csv/CsvImportServiceTests.cs"
Task: "T064 [US5] 擴充 CSV export service 測試 in BookKeeping2.Tests/Unit/Csv/CsvExportServiceTests.cs"
Task: "T065 [US5] 擴充 CSV 匯入匯出頁整合測試 in BookKeeping2.Tests/Integration/Pages/CsvImportPageTests.cs and BookKeeping2.Tests/Integration/Pages/CsvExportPageTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1) with failing tests first.
3. Validate create/edit/list/filter transaction behavior independently.
4. Stop for review before reports/accounts/budgets/CSV expansion.

### Incremental Delivery

1. Foundation: supported currency catalog, schema, migration, shared validation and test data.
2. MVP: US1 transaction create/edit/list/filter with currency.
3. P1 completion: US2 reports/summary and US3 account currency guardrails.
4. P2 expansion: US4 budgets and US5 CSV import/export.
5. Polish: responsive/browser/performance/full test verification.

### Parallel Team Strategy

1. One owner completes Phase 2 shared schema and helper work.
2. Separate owners can then take US1, US2, US3, US4 and US5.
3. Coordinate changes to shared files: `BookKeeping2/wwwroot/css/site.css`, `BookKeeping2.Tests/Integration/Browser/MultiCurrencyBrowserTests.cs`, `BookKeeping2.Tests/TestSupport/TestDataBuilder.cs`.

---

## Notes

- `[P]` tasks use different files or are documentation/test work that can proceed independently.
- All calculations must remain same-currency only; do not add exchange rates, conversion, foreign exchange records or cross-currency totals.
- Use `decimal` in domain/view models and existing `long` minor-unit persistence.
- User-facing UI text and documentation must remain Traditional Chinese.
- Keep soft-deleted transactions out of lists, summaries, reports, budgets and CSV export.
