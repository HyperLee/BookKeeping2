# Tasks: 定期交易與轉帳確認

**Input**: Design documents from `/specs/006-recurring-bookkeeping/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/recurring-bookkeeping-contract.md`, `quickstart.md`

**Tests**: 本功能依憲章與 `quickstart.md` 採測試優先。每個測試任務都必須先列出測試意圖並取得使用者或維護者確認，再撰寫測試並確認測試失敗。

**Organization**: 任務依使用者故事分組，讓每個故事能獨立實作、測試與展示。

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 確認分支、既有基線與 recurring 功能檔案位置。

- [ ] T001 Run baseline branch/build/test checks listed in `specs/006-recurring-bookkeeping/quickstart.md`
- [ ] T002 [P] Create recurring source folders under `BookKeeping2/Models/Recurring/`, `BookKeeping2/ViewModels/Recurring/`, `BookKeeping2/Services/Recurring/`, and `BookKeeping2/Pages/Recurring/`
- [ ] T003 [P] Create recurring test folders under `BookKeeping2.Tests/Unit/Recurring/` and confirm existing integration folders under `BookKeeping2.Tests/Integration/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立所有故事共用的 persistence、正式紀錄來源連結、稽核 enum 與服務契約。

**Critical**: No user story work can begin until this phase is complete.

### Tests for Foundational Infrastructure

- [ ] T004 [P] Present foundational persistence test intent, then add failing schema/index/migration tests in `BookKeeping2.Tests/Integration/Persistence/RecurringPersistenceTests.cs`
- [ ] T005 [P] Present transaction source-link test intent, then add failing tests for nullable unique `RecurringOccurrenceId` behavior in `BookKeeping2.Tests/Unit/Transactions/RecurringTransactionSourceTests.cs`
- [ ] T006 [P] Present transfer source-link test intent, then add failing tests for nullable unique `RecurringOccurrenceId` behavior in `BookKeeping2.Tests/Unit/AccountTransfers/RecurringTransferSourceTests.cs`

### Implementation for Foundational Infrastructure

- [ ] T007 [P] Add `RecurringRecordKind` enum with XML documentation in `BookKeeping2/Models/Recurring/RecurringRecordKind.cs`
- [ ] T008 [P] Add `RecurringFrequency` enum with XML documentation in `BookKeeping2/Models/Recurring/RecurringFrequency.cs`
- [ ] T009 [P] Add `RecurringOccurrenceStatus` enum with XML documentation in `BookKeeping2/Models/Recurring/RecurringOccurrenceStatus.cs`
- [ ] T010 Add `RecurringRule` entity with money minor-unit conversion, soft-delete fields, schedule anchors, and XML documentation in `BookKeeping2/Models/Recurring/RecurringRule.cs`
- [ ] T011 Add `RecurringOccurrence` entity with snapshots, terminal status fields, generated record links, and XML documentation in `BookKeeping2/Models/Recurring/RecurringOccurrence.cs`
- [ ] T012 Add nullable `RecurringOccurrenceId` and navigation property to `BookKeeping2/Models/Transactions/Transaction.cs`
- [ ] T013 Add nullable `RecurringOccurrenceId` and navigation property to `BookKeeping2/Models/AccountTransfers/AccountTransfer.cs`
- [ ] T014 Register `DbSet<RecurringRule>` and `DbSet<RecurringOccurrence>` in `BookKeeping2/Data/AppDbContext.cs`
- [ ] T015 [P] Add recurring rule EF configuration, FKs, max lengths, and lookup indexes in `BookKeeping2/Data/EntityConfigurations/RecurringRuleConfiguration.cs`
- [ ] T016 [P] Add recurring occurrence EF configuration, snapshot constraints, unique `{ RecurringRuleId, ScheduledDate }`, and status/date indexes in `BookKeeping2/Data/EntityConfigurations/RecurringOccurrenceConfiguration.cs`
- [ ] T017 Update transaction EF configuration with nullable recurring source FK and unique index in `BookKeeping2/Data/EntityConfigurations/TransactionConfiguration.cs`
- [ ] T018 Update transfer EF configuration with nullable recurring source FK and unique index in `BookKeeping2/Data/EntityConfigurations/AccountTransferConfiguration.cs`
- [ ] T019 Create EF migration and model snapshot updates for recurring tables and source links in `BookKeeping2/Data/Migrations/`
- [ ] T020 Add recurring audit event values with XML documentation in `BookKeeping2/Models/Audit/AuditEventType.cs`
- [ ] T021 [P] Add reusable recurring service result type in `BookKeeping2/Services/Recurring/RecurringResult.cs`
- [ ] T022 [P] Add recurring rule service interface matching the contract in `BookKeeping2/Services/Recurring/IRecurringRuleService.cs`
- [ ] T023 [P] Add recurring occurrence service interface matching the contract in `BookKeeping2/Services/Recurring/IRecurringOccurrenceService.cs`
- [ ] T024 Run foundational test filters covering `BookKeeping2.Tests/Integration/Persistence/RecurringPersistenceTests.cs`, `BookKeeping2.Tests/Unit/Transactions/RecurringTransactionSourceTests.cs`, and `BookKeeping2.Tests/Unit/AccountTransfers/RecurringTransferSourceTests.cs`

**Checkpoint**: Foundation ready; user story implementation can start.

---

## Phase 3: User Story 1 - 建立定期收入與支出規則 (Priority: P1) MVP

**Goal**: 使用者可建立收入/支出定期規則，到期後看到待確認項目，且確認前不建立正式交易。

**Independent Test**: 建立每月 5 日 TWD 房租支出規則，將 Asia/Taipei 今日推進到到期後，首頁或定期頁出現 pending occurrence，且 `Transactions` 仍無正式紀錄。

### Tests for User Story 1

- [ ] T025 [P] [US1] Present date-calculation test intent, then add failing weekly/monthly/yearly due-date tests in `BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs`
- [ ] T026 [P] [US1] Present rule-service test intent, then add failing income/expense create and validation tests in `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`
- [ ] T027 [P] [US1] Present materialization test intent, then add failing pending occurrence tests for due and not-yet-due rules in `BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs`
- [ ] T028 [P] [US1] Present recurring page test intent, then add failing Razor Pages tests for create/list/pending income and expense flows in `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

### Implementation for User Story 1

- [ ] T029 [US1] Implement deterministic weekly/monthly/yearly date generation without future occurrence creation in `BookKeeping2/Services/Recurring/RecurrenceDateCalculator.cs`
- [ ] T030 [P] [US1] Add rule input/list/pending/form option view models in `BookKeeping2/ViewModels/Recurring/RecurringRuleInputModel.cs`, `BookKeeping2/ViewModels/Recurring/RecurringRuleListViewModel.cs`, `BookKeeping2/ViewModels/Recurring/RecurringRuleListItemViewModel.cs`, `BookKeeping2/ViewModels/Recurring/RecurringOccurrenceListItemViewModel.cs`, and `BookKeeping2/ViewModels/Recurring/RecurringFormOptionsViewModel.cs`
- [ ] T031 [US1] Implement income/expense create, update-for-future defaults, list, form options, validation, sanitization, and masked audit in `BookKeeping2/Services/Recurring/RecurringRuleService.cs`
- [ ] T032 [US1] Implement due occurrence materialization and pending list queries for income/expense rules in `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T033 [US1] Register recurring services and date calculator in dependency injection in `BookKeeping2/Program.cs`
- [ ] T034 [US1] Add recurring rule list page with income/expense rows and entry actions in `BookKeeping2/Pages/Recurring/Index.cshtml` and `BookKeeping2/Pages/Recurring/Index.cshtml.cs`
- [ ] T035 [US1] Add recurring create page with income/expense fields, server validation, anti-forgery, and preserved values in `BookKeeping2/Pages/Recurring/Create.cshtml` and `BookKeeping2/Pages/Recurring/Create.cshtml.cs`
- [ ] T036 [US1] Add pending list page that materializes due income/expense occurrences before listing in `BookKeeping2/Pages/Recurring/Pending.cshtml` and `BookKeeping2/Pages/Recurring/Pending.cshtml.cs`
- [ ] T037 [US1] Add `定期項目` navigation entry while preserving theme/language behavior in `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T038 [US1] Run US1 test filters covering `BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs`, `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`, `BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs`, and `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

**Checkpoint**: US1 is independently demonstrable as the MVP.

---

## Phase 4: User Story 2 - 確認到期項目後建立正式交易 (Priority: P1)

**Goal**: 使用者可確認 pending 收入/支出項目，必要時調整本期內容，並只建立一筆正式交易。

**Independent Test**: 以 seeded pending 支出 occurrence 調整本期金額後確認，檢查正式 transaction、帳戶餘額、月報與預算反映結果，且 parent recurring rule 預設金額未變。

### Tests for User Story 2

- [ ] T039 [P] [US2] Present transaction-confirmation service test intent, then add failing confirmation, adjustment, future-date rejection, and idempotency tests in `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs`
- [ ] T040 [P] [US2] Present transaction confirmation page test intent, then add failing GET/POST anti-forgery and validation tests in `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`
- [ ] T041 [P] [US2] Present formal transaction boundary test intent, then add failing source-link, report, budget, and CSV behavior tests in `BookKeeping2.Tests/Unit/Transactions/RecurringTransactionSourceTests.cs`

### Implementation for User Story 2

- [ ] T042 [P] [US2] Add transaction confirmation input and confirmation view models in `BookKeeping2/ViewModels/Recurring/RecurringTransactionConfirmationInputModel.cs` and `BookKeeping2/ViewModels/Recurring/RecurringOccurrenceConfirmationViewModel.cs`
- [ ] T043 [US2] Extract or centralize reusable transaction validation rules used by manual and recurring confirmations in `BookKeeping2/Services/Transactions/TransactionValidationRules.cs` and `BookKeeping2/Services/Transactions/TransactionService.cs`
- [ ] T044 [US2] Implement `ConfirmTransactionAsync` with one EF Core transaction, recurring audit, formal transaction audit, budget warning preservation, and duplicate-submit protection in `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T045 [US2] Add transaction confirmation page with snapshot defaults, adjustable fields, anti-forgery, and Traditional Chinese validation messages in `BookKeeping2/Pages/Recurring/ConfirmTransaction.cshtml` and `BookKeeping2/Pages/Recurring/ConfirmTransaction.cshtml.cs`
- [ ] T046 [US2] Verify and adjust confirmed transaction integration while keeping pending occurrences excluded in `BookKeeping2/Services/Reports/ReportService.cs`, `BookKeeping2/Services/Budgets/BudgetService.cs`, and `BookKeeping2/Services/Csv/CsvExportService.cs`
- [ ] T047 [US2] Run US2 test filters covering `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs`, `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`, and `BookKeeping2.Tests/Unit/Transactions/RecurringTransactionSourceTests.cs`

**Checkpoint**: US2 confirms income/expense occurrences without duplicate formal transactions.

---

## Phase 5: User Story 3 - 建立與確認定期同幣別轉帳 (Priority: P1)

**Goal**: 使用者可建立定期同幣別轉帳規則，到期後確認為正式 transfer，並保持轉帳排除於收入、支出、報表與預算。

**Independent Test**: 以 seeded 或 UI 建立每月 TWD 銀行到信用卡轉帳 occurrence，確認後檢查兩個帳戶餘額正確變動，月報與預算不包含該轉帳。

### Tests for User Story 3

- [ ] T048 [P] [US3] Present transfer-rule test intent, then add failing transfer rule validation tests in `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`
- [ ] T049 [P] [US3] Present transfer-confirmation service test intent, then add failing same-currency, same-account, negative-balance-allowed, and idempotency tests in `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs`
- [ ] T050 [P] [US3] Present transfer page and exclusion test intent, then add failing recurring transfer page/report/budget tests in `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

### Implementation for User Story 3

- [ ] T051 [US3] Extend recurring rule input, form options, and rule service validation for transfer fields in `BookKeeping2/ViewModels/Recurring/RecurringRuleInputModel.cs`, `BookKeeping2/ViewModels/Recurring/RecurringFormOptionsViewModel.cs`, and `BookKeeping2/Services/Recurring/RecurringRuleService.cs`
- [ ] T052 [US3] Extend occurrence materialization to snapshot transfer direction and safe account names in `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T053 [US3] Extract or centralize reusable transfer validation rules used by manual and recurring confirmations in `BookKeeping2/Services/AccountTransfers/AccountTransferValidationRules.cs` and `BookKeeping2/Services/AccountTransfers/AccountTransferService.cs`
- [ ] T054 [US3] Implement `ConfirmTransferAsync` with one EF Core transaction, source-link idempotency, masked recurring audit, and existing transfer audit behavior in `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T055 [US3] Add transfer confirmation page with snapshot defaults, adjustable fields, anti-forgery, and Traditional Chinese validation messages in `BookKeeping2/Pages/Recurring/ConfirmTransfer.cshtml` and `BookKeeping2/Pages/Recurring/ConfirmTransfer.cshtml.cs`
- [ ] T056 [US3] Update recurring create/list/pending UI to show transfer fields and transfer direction correctly in `BookKeeping2/Pages/Recurring/Create.cshtml`, `BookKeeping2/Pages/Recurring/Index.cshtml`, and `BookKeeping2/Pages/Recurring/Pending.cshtml`
- [ ] T057 [US3] Verify and adjust transfer integration while keeping transfers excluded from reports and budgets in `BookKeeping2/Services/Csv/CsvTransferExportService.cs`, `BookKeeping2/Services/Reports/ReportService.cs`, and `BookKeeping2/Services/Budgets/BudgetService.cs`
- [ ] T058 [US3] Run US3 test filters covering `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`, `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs`, and `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

**Checkpoint**: US3 supports recurring same-currency transfers as formal transfers only after confirmation.

---

## Phase 6: User Story 4 - 處理多期到期與略過項目 (Priority: P2)

**Goal**: 系統列出所有到期未處理期數，使用者可逐期確認或略過，且略過不影響後續期數。

**Independent Test**: 建立每月支出規則後跨過三個月，清單列出三期；確認一期、略過一期後，第三期仍 pending。

### Tests for User Story 4

- [ ] T059 [P] [US4] Present multi-period materialization test intent, then add failing tests for three due periods and independent pending state in `BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs`
- [ ] T060 [P] [US4] Present skip-flow test intent, then add failing service and Razor Pages tests for skip terminal state and refresh behavior in `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs` and `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

### Implementation for User Story 4

- [ ] T061 [US4] Update `GetDueDates` and `MaterializeDueAsync` to create every due missing occurrence through today while skipping existing dates in `BookKeeping2/Services/Recurring/RecurrenceDateCalculator.cs` and `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T062 [US4] Implement `SkipAsync` with terminal status transition, no formal record creation, and masked audit in `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T063 [US4] Add skip confirmation page with anti-forgery and safe summary in `BookKeeping2/Pages/Recurring/Skip.cshtml` and `BookKeeping2/Pages/Recurring/Skip.cshtml.cs`
- [ ] T064 [US4] Update pending list view and page model to remove confirmed/skipped rows while preserving other pending rows in `BookKeeping2/ViewModels/Recurring/RecurringPendingListViewModel.cs` and `BookKeeping2/Pages/Recurring/Pending.cshtml.cs`
- [ ] T065 [US4] Run US4 test filters covering `BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs`, `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs`, and `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

**Checkpoint**: US4 supports backlog catch-up and per-occurrence skip.

---

## Phase 7: User Story 5 - 管理定期規則狀態與日期邊界 (Priority: P2)

**Goal**: 使用者可查看、編輯、停用或軟刪除規則，並正確處理月底、跨年與閏年日期。

**Independent Test**: 建立每月 31 日規則驗證 2 月最後一天；停用規則後下一期不再 materialize，既有正式紀錄保留。

### Tests for User Story 5

- [ ] T066 [P] [US5] Present edge-date test intent, then add failing monthly 29/30/31, leap-year, and yearly fallback tests in `BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs`
- [ ] T067 [P] [US5] Present rule lifecycle test intent, then add failing edit/deactivate/soft-delete and existing-pending-snapshot tests in `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`
- [ ] T068 [P] [US5] Present rule management page test intent, then add failing edit/deactivate/delete Razor Pages tests in `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

### Implementation for User Story 5

- [ ] T069 [US5] Complete monthly/yearly last-day fallback, inclusive end-date, leap-year, and cross-year logic in `BookKeeping2/Services/Recurring/RecurrenceDateCalculator.cs`
- [ ] T070 [US5] Implement rule update, deactivate, and soft-delete semantics without mutating existing occurrences or formal records in `BookKeeping2/Services/Recurring/RecurringRuleService.cs`
- [ ] T071 [US5] Add recurring edit page with future-default updates only in `BookKeeping2/Pages/Recurring/Edit.cshtml` and `BookKeeping2/Pages/Recurring/Edit.cshtml.cs`
- [ ] T072 [US5] Add recurring soft-delete confirmation page and deactivate post handler in `BookKeeping2/Pages/Recurring/Delete.cshtml`, `BookKeeping2/Pages/Recurring/Delete.cshtml.cs`, and `BookKeeping2/Pages/Recurring/Index.cshtml.cs`
- [ ] T073 [US5] Update recurring rule list to show next due date, active/inactive state, edit, deactivate, and delete actions in `BookKeeping2/Pages/Recurring/Index.cshtml`
- [ ] T074 [US5] Run US5 test filters covering `BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs`, `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`, and `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`

**Checkpoint**: US5 manages recurring rules safely and preserves history.

---

## Phase 8: User Story 6 - 首頁提醒與可用性 (Priority: P3)

**Goal**: 首頁顯示待確認摘要與處理入口；定期頁面在手機、平板、桌面與主題切換下可讀可操作。

**Independent Test**: 建立多筆到期項目後開啟首頁，摘要顯示 pending count 與入口；切換主題/語系並在手機與桌面寬度檢查沒有重疊。

### Tests for User Story 6

- [ ] T075 [P] [US6] Present homepage summary test intent, then add failing pending-count and no-warning-empty-state tests in `BookKeeping2.Tests/Integration/Pages/HomeRecurringSummaryTests.cs`
- [ ] T076 [P] [US6] Present responsive/browser test intent, then add failing mobile/desktop/theme/accessibility checks in `BookKeeping2.Tests/Integration/Browser/RecurringBrowserTests.cs`

### Implementation for User Story 6

- [ ] T077 [P] [US6] Add homepage summary view model in `BookKeeping2/ViewModels/Recurring/RecurringHomeSummaryViewModel.cs`
- [ ] T078 [US6] Implement `GetHomeSummaryAsync` with bounded materialization and safe summary data in `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`
- [ ] T079 [US6] Add homepage pending summary and processing link without misleading empty warning in `BookKeeping2/Pages/Index.cshtml` and `BookKeeping2/Pages/Index.cshtml.cs`
- [ ] T080 [US6] Add responsive recurring surface styles and focus/contrast fixes in `BookKeeping2/wwwroot/css/site.css`
- [ ] T081 [US6] Add English resource entries for recurring general UI labels while keeping zh-TW source text in `BookKeeping2/Resources/SharedResource.en.resx`
- [ ] T082 [US6] Run US6 test filters covering `BookKeeping2.Tests/Integration/Pages/HomeRecurringSummaryTests.cs` and `BookKeeping2.Tests/Integration/Browser/RecurringBrowserTests.cs`

**Checkpoint**: US6 makes pending recurring work visible and usable.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: 效能、安全、文件與完整驗證。

- [ ] T083 [P] Present performance test intent, then add recurring 100-rule/500-occurrence tests in `BookKeeping2.Tests/Integration/Performance/RecurringOccurrencePerformanceTests.cs`
- [ ] T084 [P] Add or update language/static contract coverage for recurring labels and validation messages in `BookKeeping2.Tests/Integration/StaticAssets/LanguageResourceCompletenessTests.cs`
- [ ] T085 [P] Update recurring test data helpers for reusable rule, occurrence, transaction, and transfer setup in `BookKeeping2.Tests/TestSupport/TestDataBuilder.cs`
- [ ] T086 Verify no pending occurrence leaks into reports, budgets, timelines, or CSV before confirmation using tests in `BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs`, `BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs`, `BookKeeping2.Tests/Unit/Csv/CsvExportServiceTests.cs`, and `BookKeeping2.Tests/Unit/Csv/CsvTransferExportServiceTests.cs`
- [ ] T087 Run targeted verification commands listed in `specs/006-recurring-bookkeeping/quickstart.md`
- [ ] T088 Run full build, full test suite, and coverage commands for `BookKeeping2.slnx` and `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [ ] T089 Inspect migration, model snapshot, recurring pages, and generated diff for schema/UI/doc consistency in `BookKeeping2/Data/Migrations/`, `BookKeeping2/Pages/Recurring/`, and `specs/006-recurring-bookkeeping/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: No dependencies.
- **Phase 2 Foundational**: Depends on Phase 1 and blocks all user stories.
- **Phase 3 US1**: Depends on Phase 2 and is the MVP.
- **Phase 4 US2**: Depends on Phase 2; may use seeded pending occurrences for independent tests and integrates naturally after US1.
- **Phase 5 US3**: Depends on Phase 2; may use seeded transfer occurrences for independent tests and integrates naturally after US1.
- **Phase 6 US4**: Depends on Phase 2 and the occurrence service behavior from US1/US2/US3 when implemented sequentially.
- **Phase 7 US5**: Depends on Phase 2 and can be tested with seeded rules/occurrences.
- **Phase 8 US6**: Depends on Phase 2 and benefits from US1 pending list behavior.
- **Phase 9 Polish**: Depends on all desired stories being complete.

### User Story Dependencies

- **US1 (P1)**: First deliverable MVP; no dependency on other stories after foundation.
- **US2 (P1)**: Can be implemented after foundation with seeded pending transaction occurrences; for product flow, pair with US1.
- **US3 (P1)**: Can be implemented after foundation with seeded pending transfer occurrences; for product flow, pair with US1.
- **US4 (P2)**: Uses the occurrence service and is safest after US1 plus either US2 or US3.
- **US5 (P2)**: Can run after foundation but should be validated against US1 materialization.
- **US6 (P3)**: Should follow at least US1 so homepage summary has real pending data.

### Within Each User Story

- Tests must be written first and observed failing before implementation.
- Models/view models before services.
- Services before Razor Pages.
- Core service behavior before report/budget/CSV integration checks.
- Story tests must pass before moving to the next story in a sequential delivery flow.

---

## Parallel Opportunities

- Setup folder creation tasks T002-T003 can run in parallel.
- Foundational tests T004-T006 can run in parallel after test intent approval.
- Foundational enum/config/interface tasks marked [P] can run in parallel when they touch different files.
- Each story's tests marked [P] can run in parallel before implementation.
- US2 and US3 can be developed in parallel after Phase 2 if seeded occurrence fixtures are used and merge coordination is done around `RecurringOccurrenceService.cs`.
- Polish tasks T083-T085 can run in parallel after the related story surfaces exist.

## Parallel Example: User Story 1

```bash
Task: "T025 Add failing date-calculation tests in BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs"
Task: "T026 Add failing income/expense rule-service tests in BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs"
Task: "T027 Add failing materialization tests in BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs"
Task: "T028 Add failing recurring page tests in BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs"
```

## Parallel Example: User Story 2

```bash
Task: "T039 Add failing transaction confirmation service tests in BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs"
Task: "T040 Add failing transaction confirmation page tests in BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs"
Task: "T041 Add failing formal transaction boundary tests in BookKeeping2.Tests/Unit/Transactions/RecurringTransactionSourceTests.cs"
```

## Parallel Example: User Story 3

```bash
Task: "T048 Add failing transfer rule validation tests in BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs"
Task: "T049 Add failing transfer confirmation tests in BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs"
Task: "T050 Add failing recurring transfer page/report/budget tests in BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs"
```

## Parallel Example: User Story 4

```bash
Task: "T059 Add failing multi-period materialization tests in BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs"
Task: "T060 Add failing skip service and Razor Pages tests in BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs and BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs"
```

## Parallel Example: User Story 5

```bash
Task: "T066 Add failing date-boundary tests in BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs"
Task: "T067 Add failing rule lifecycle tests in BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs"
Task: "T068 Add failing rule management page tests in BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs"
```

## Parallel Example: User Story 6

```bash
Task: "T075 Add failing homepage summary tests in BookKeeping2.Tests/Integration/Pages/HomeRecurringSummaryTests.cs"
Task: "T076 Add failing responsive/browser tests in BookKeeping2.Tests/Integration/Browser/RecurringBrowserTests.cs"
```

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 setup.
2. Complete Phase 2 foundation.
3. Complete Phase 3 US1.
4. Stop and validate US1 independently with the specified unit and page tests.

### P1 Completion

1. Add US2 to confirm recurring income/expense occurrences into formal transactions.
2. Add US3 to support recurring same-currency transfers.
3. Validate that confirmed transactions and transfers affect only the intended formal surfaces.

### Incremental Delivery

1. Add US4 for backlog catch-up and skip.
2. Add US5 for lifecycle management and date edge cases.
3. Add US6 for homepage reminder and responsive/browser polish.
4. Finish Phase 9 cross-cutting verification before claiming feature completion.

### Team Parallel Strategy

1. Complete setup and foundation together.
2. After Phase 2, assign US2 and US3 to separate implementers using seeded occurrence tests.
3. Coordinate edits to `BookKeeping2/Services/Recurring/RecurringOccurrenceService.cs`, `BookKeeping2/Pages/Recurring/Pending.cshtml`, and `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`.
4. Merge story increments only after each story's independent tests pass.

## Notes

- `[P]` tasks touch different files or can run without waiting for another incomplete task in the same phase.
- `[US1]` through `[US6]` labels map directly to user stories in `specs/006-recurring-bookkeeping/spec.md`.
- State-changing Razor Pages tasks must preserve anti-forgery protection.
- All user-facing UI text and validation messages must be Traditional Chinese unless an existing localization resource requires a translated English entry.
- Currency and money behavior must continue to use `decimal` externally and minor units in SQLite.
