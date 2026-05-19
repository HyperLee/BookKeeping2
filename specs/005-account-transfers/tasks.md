# Tasks: 同幣別帳戶轉帳與信用卡繳款

**Input**: Design documents from `/specs/005-account-transfers/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/account-transfer-contract.md`, `quickstart.md`

**Tests**: 本功能規格與 quickstart 明確要求測試優先開發；每個測試任務必須先列出測試意圖並取得使用者或維護者確認，再撰寫並確認失敗，之後才能進入對應實作任務。

**Organization**: Tasks are grouped by user story to enable independent implementation and testing. User-facing UI text and validation messages must use Traditional Chinese (`zh-TW`).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (`US1`, `US2`, `US3`, `US4`)
- Every task includes exact repository paths

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the branch, baseline health, and feature-specific scaffolding before writing failing tests.

- [X] T001 Confirm current branch is `005-account-transfers` and run the baseline commands listed in specs/005-account-transfers/quickstart.md
- [X] T002 [P] Create account transfer source folders in BookKeeping2/Models/AccountTransfers/, BookKeeping2/Services/AccountTransfers/, and BookKeeping2/ViewModels/AccountTransfers/
- [X] T003 [P] Create account transfer test folders in BookKeeping2.Tests/Unit/AccountTransfers/ and BookKeeping2.Tests/Integration/Browser/
- [X] T004 [P] Add reusable account transfer test data helpers to BookKeeping2.Tests/TestSupport/TestDataBuilder.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add the shared persistence model, migration, and audit vocabulary that all user stories depend on.

**CRITICAL**: No user story work can begin until this phase is complete.

### Tests for Foundational Persistence

- [X] T005 [P] Confirm persistence test intent, then add failing persistence tests for AccountTransfers schema, restrict foreign keys, soft-delete columns, submission-token unique index, and query indexes in BookKeeping2.Tests/Integration/Persistence/AccountTransferPersistenceTests.cs

### Implementation for Foundational Persistence

- [X] T006 Add AccountTransfer entity with XML documentation, money minor-unit conversion, one-time submission token, timestamps, soft-delete metadata, and masked summary fields in BookKeeping2/Models/AccountTransfers/AccountTransfer.cs
- [X] T007 [P] Add AccountTransfer EF Core configuration for required fields, max lengths, restrict foreign keys, submission-token unique index, and query indexes in BookKeeping2/Data/EntityConfigurations/AccountTransferConfiguration.cs
- [X] T008 Register DbSet<AccountTransfer> and using statements in BookKeeping2/Data/AppDbContext.cs
- [X] T009 Add transfer audit event types for create, update, delete, transfer CSV import, and transfer CSV export in BookKeeping2/Models/Audit/AuditEventType.cs
- [X] T010 Create EF Core migration and model snapshot updates for AccountTransfers in BookKeeping2/Data/Migrations/
- [X] T011 Run the AccountTransfer persistence test and confirm it passes using BookKeeping2.Tests/Integration/Persistence/AccountTransferPersistenceTests.cs

**Checkpoint**: AccountTransfer persistence exists, is migrated, and can be referenced by story-level tests and services.

---

## Phase 3: User Story 1 - 建立同幣別帳戶轉帳 (Priority: P1) MVP

**Goal**: 使用者可建立、編輯、軟刪除同幣別帳戶轉帳；轉出帳戶扣減、轉入帳戶增加，且負餘額允許。

**Independent Test**: 建立一筆 TWD 1,000 從銀行到現金的轉帳，確認銀行餘額減少、現金餘額增加；同帳戶與跨幣別轉帳被拒絕；負餘額允許；編輯與軟刪除後餘額重新計算。

### Tests for User Story 1

- [X] T012 [P] [US1] Confirm service test intent, then add failing service tests for create validation, same-account rejection, currency mismatch rejection, future-date rejection, note sanitization, submission-token duplicate rapid resubmit, same-content different-token allowance, update, and soft delete in BookKeeping2.Tests/Unit/AccountTransfers/AccountTransferServiceTests.cs
- [X] T013 [P] [US1] Confirm balance test intent, then add failing balance tests for outgoing transfer, incoming transfer, negative balance allowance, edit recalculation, soft-delete exclusion, and 1-second balance reflection after create/edit/delete in BookKeeping2.Tests/Unit/Accounts/AccountTransferBalanceTests.cs
- [X] T014 [P] [US1] Confirm Razor Pages test intent, then add failing Razor Pages tests for Transfers Create/Edit/Delete GET and POST, anti-forgery, SubmissionToken rendering and reuse behavior, validation preservation, success redirects, and deleted transfer NotFound handling in BookKeeping2.Tests/Integration/Pages/AccountTransferPagesTests.cs

### Implementation for User Story 1

- [X] T015 [P] [US1] Create AccountTransferInputModel with hidden SubmissionToken and Traditional Chinese validation messages in BookKeeping2/ViewModels/AccountTransfers/AccountTransferInputModel.cs
- [X] T016 [P] [US1] Create AccountTransferFormOptionsViewModel for active same-currency account selections in BookKeeping2/ViewModels/AccountTransfers/AccountTransferFormOptionsViewModel.cs
- [X] T017 [P] [US1] Create AccountTransferResult for success, not-found, duplicate-resubmit, and field-level validation errors in BookKeeping2/Services/AccountTransfers/AccountTransferResult.cs
- [X] T018 [US1] Define IAccountTransferService contract from specs/005-account-transfers/contracts/account-transfer-contract.md in BookKeeping2/Services/AccountTransfers/IAccountTransferService.cs
- [X] T019 [US1] Implement AccountTransferService validation, sanitization, same-currency rules, submission-token duplicate rapid resubmit detection, same-content different-token allowance, create, update, soft delete, and masked audit events in BookKeeping2/Services/AccountTransfers/AccountTransferService.cs
- [X] T020 [US1] Update AccountService balance aggregation to include non-deleted outgoing and incoming transfer totals without N+1 queries in BookKeeping2/Services/Accounts/AccountService.cs
- [X] T021 [US1] Register IAccountTransferService in dependency injection and add the AccountTransfers namespace in BookKeeping2/Program.cs
- [X] T022 [P] [US1] Create shared transfer form partial with date, currency, amount, from account, to account, note, validation summary, and anti-forgery-compatible fields in BookKeeping2/Pages/Transfers/_TransferForm.cshtml
- [X] T023 [US1] Create transfer create Razor Page and PageModel with preserved validation state and success redirect in BookKeeping2/Pages/Transfers/Create.cshtml and BookKeeping2/Pages/Transfers/Create.cshtml.cs
- [X] T024 [US1] Create transfer edit Razor Page and PageModel with missing/deleted transfer handling and success redirect in BookKeeping2/Pages/Transfers/Edit.cshtml and BookKeeping2/Pages/Transfers/Edit.cshtml.cs
- [X] T025 [US1] Create transfer delete Razor Page and PageModel that confirms direction and performs soft delete only in BookKeeping2/Pages/Transfers/Delete.cshtml and BookKeeping2/Pages/Transfers/Delete.cshtml.cs
- [X] T026 [US1] Add transfer form and validation styles that keep labels, messages, and action buttons readable on mobile and desktop in BookKeeping2/wwwroot/css/site.css
- [X] T027 [US1] Run US1 service, balance, and page tests and fix only US1 files until green using BookKeeping2.Tests/Unit/AccountTransfers/AccountTransferServiceTests.cs, BookKeeping2.Tests/Unit/Accounts/AccountTransferBalanceTests.cs, and BookKeeping2.Tests/Integration/Pages/AccountTransferPagesTests.cs

**Checkpoint**: User Story 1 is independently functional and testable without timeline or CSV work.

---

## Phase 4: User Story 2 - 信用卡繳款不列入支出 (Priority: P1)

**Goal**: 使用者以轉帳表示信用卡繳款時，只調整帳戶餘額，不增加月支出、分類統計或預算使用率。

**Independent Test**: 建立信用卡支出交易與銀行到信用卡帳戶的轉帳，確認銀行與信用卡餘額改變，但月報支出與預算使用率不包含繳款轉帳。

### Tests for User Story 2

- [X] T028 [P] [US2] Confirm report test intent, then add failing report tests proving transfer payments are excluded from monthly expense totals, category totals, trend data, and chart totals in BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs
- [X] T029 [P] [US2] Confirm budget test intent, then add failing budget tests proving transfer payments do not increase any category budget usage or alert state in BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs
- [X] T030 [P] [US2] Confirm homepage summary test intent, then add failing homepage summary tests proving transfer payments do not alter income/expense cards while account balances reflect transfers in BookKeeping2.Tests/Integration/Pages/HomeBudgetSummaryTests.cs

### Implementation for User Story 2

- [X] T031 [US2] Ensure ReportService remains transaction-only for income, expense, category, monthly, trend, and chart totals while AccountTransfer records are ignored in BookKeeping2/Services/Reports/ReportService.cs
- [X] T032 [US2] Ensure BudgetService remains transaction-only for category usage, remaining budget, and alert state while AccountTransfer records are ignored in BookKeeping2/Services/Budgets/BudgetService.cs
- [X] T033 [US2] Ensure homepage AccountBalances use transfer-aware AccountService while monthly income/expense summaries remain transaction-only in BookKeeping2/Pages/Index.cshtml.cs
- [X] T034 [US2] Run US2 report, budget, and homepage tests and fix only US2 files until green using BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs, BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs, and BookKeeping2.Tests/Integration/Pages/HomeBudgetSummaryTests.cs

**Checkpoint**: Credit card payment transfers affect balances but never financial totals for income, expense, reports, or budgets.

---

## Phase 5: User Story 3 - 在明細時間線查看轉帳 (Priority: P2)

**Goal**: 使用者在交易明細時間線中可與收入、支出一起查看轉帳，並清楚辨識方向、金額、備註與可用操作。

**Independent Test**: 建立同一天的收入、支出與轉帳，確認排序、轉帳標示、方向文字、帳戶篩選、幣別篩選、關鍵字、金額篩選與 category filter 排除轉帳都正確。

### Tests for User Story 3

- [X] T035 [P] [US3] Confirm timeline query test intent, then add failing timeline query tests for mixed income/expense/transfer sorting, account filter matching either side, currency/date/keyword/amount filters, category filter excluding transfers, and no deleted transfers in BookKeeping2.Tests/Unit/Transactions/TransactionTimelineQueryTests.cs
- [X] T036 [P] [US3] Confirm transaction timeline page test intent, then add failing transaction timeline page tests for visible `新增轉帳` entry, transfer label `轉帳`, direction text, edit/delete links, and filter form behavior in BookKeeping2.Tests/Integration/Pages/TransactionTimelineTransferTests.cs
- [X] T037 [P] [US3] Confirm performance test intent, then add failing performance tests for 10,000 mixed transaction and transfer rows with one filter under 2 seconds, transfer-aware account balance aggregation after create/edit/delete under 1 second, and no N+1 query pattern in BookKeeping2.Tests/Integration/Performance/TransactionTimelinePerformanceTests.cs and BookKeeping2.Tests/Integration/Performance/AccountTransferPerformanceTests.cs

### Implementation for User Story 3

- [X] T038 [P] [US3] Create TransactionTimelineItemViewModel with record kind, transfer direction fields, amount text, category-null transfer behavior, edit page, and delete page in BookKeeping2/ViewModels/Transactions/TransactionTimelineItemViewModel.cs
- [X] T039 [US3] Update PagedTransactionListViewModel to expose timeline items while preserving pagination metadata in BookKeeping2/ViewModels/Transactions/PagedTransactionListViewModel.cs
- [X] T040 [US3] Update TransactionQueryService to project transactions and non-deleted transfers into one filtered, sorted, paged timeline without N+1 queries in BookKeeping2/Services/Transactions/TransactionQueryService.cs
- [X] T041 [US3] Update ITransactionQueryService XML documentation to describe mixed transaction and transfer timeline semantics in BookKeeping2/Services/Transactions/ITransactionQueryService.cs
- [X] T042 [US3] Update Transactions Index PageModel to keep existing filters and load the mixed timeline from TransactionQueryService in BookKeeping2/Pages/Transactions/Index.cshtml.cs
- [X] T043 [US3] Update Transactions Index Razor view with `新增轉帳` entry, transfer row label, direction display, category-empty transfer rows, and transfer edit/delete links in BookKeeping2/Pages/Transactions/Index.cshtml
- [X] T044 [US3] Add responsive timeline row styles for transfer direction, amount, labels, and action buttons in BookKeeping2/wwwroot/css/site.css
- [X] T045 [US3] Run US3 timeline query, page, and performance tests and fix only US3 files until green using BookKeeping2.Tests/Unit/Transactions/TransactionTimelineQueryTests.cs, BookKeeping2.Tests/Integration/Pages/TransactionTimelineTransferTests.cs, and BookKeeping2.Tests/Integration/Performance/TransactionTimelinePerformanceTests.cs

**Checkpoint**: Transfers are visible in the transaction timeline and independently testable without CSV.

---

## Phase 6: User Story 4 - 轉帳 CSV 匯入與匯出 (Priority: P2)

**Goal**: 使用者可使用獨立轉帳 CSV 格式備份與移轉轉帳紀錄，且不破壞既有交易 CSV 契約。

**Independent Test**: 匯出轉帳 CSV 取得 header `日期,幣別,金額,轉出帳戶,轉入帳戶,備註`；匯入有效與無效混合列時只建立有效轉帳並顯示逐列錯誤；交易 CSV 與轉帳 CSV 不會互相誤判。

### Tests for User Story 4

- [X] T046 [P] [US4] Confirm transfer CSV parser test intent, then add failing transfer CSV parser tests for exact header, transaction-header rejection, empty file, wrong order, field-count errors, row limit, date, currency, amount, account names, and row-level errors in BookKeeping2.Tests/Unit/Csv/CsvTransferImportParserTests.cs
- [X] T047 [P] [US4] Confirm transfer CSV import service test intent, then add failing transfer CSV import service tests for valid row creation, invalid row skipping, mixed commit behavior, same-currency validation, active account validation, 100-row import under 10 seconds, audit batch summaries, and safe raw value previews in BookKeeping2.Tests/Unit/Csv/CsvTransferImportServiceTests.cs
- [X] T048 [P] [US4] Confirm transfer CSV export service test intent, then add failing transfer CSV export service tests for header order, non-deleted transfer filtering, date/id ordering, formula injection protection, file name, 1,000-row export under 5 seconds, and export audit-safe row count in BookKeeping2.Tests/Unit/Csv/CsvTransferExportServiceTests.cs
- [X] T049 [P] [US4] Confirm CSV page test intent, then add failing CSV page tests for separate transfer import/export handlers, preserved transaction CSV contract, validation errors, and download headers in BookKeeping2.Tests/Integration/Pages/CsvTransferPageTests.cs

### Implementation for User Story 4

- [X] T050 [P] [US4] Create CsvTransferRow with exact transfer CSV field mapping in BookKeeping2/Services/Csv/CsvTransferRow.cs
- [X] T051 [US4] Create CsvTransferImportParser with exact header matching, structural validation, existing upload limits, row limit, and transaction-header rejection in BookKeeping2/Services/Csv/CsvTransferImportParser.cs
- [X] T052 [US4] Create CsvTransferImportService that resolves active accounts, generates internal SubmissionToken values, validates transfer rules through AccountTransferService, commits valid rows with import batch/errors, and records audit summaries in BookKeeping2/Services/Csv/CsvTransferImportService.cs
- [X] T053 [US4] Create CsvTransferExportService that exports only non-deleted transfers with formula protection, date/id ordering, transfer file names, and audit-safe result metadata in BookKeeping2/Services/Csv/CsvTransferExportService.cs
- [X] T054 [US4] Update transaction CsvImportParser tests and behavior so transfer CSV headers are rejected by the transaction import path in BookKeeping2/Services/Csv/CsvImportParser.cs and BookKeeping2.Tests/Unit/Csv/CsvImportParserTests.cs
- [X] T055 [US4] Register CsvTransferImportService and CsvTransferExportService in dependency injection in BookKeeping2/Program.cs
- [X] T056 [US4] Add separate transfer CSV upload form, result summary, and handler to the import page without changing transaction import behavior in BookKeeping2/Pages/Csv/Import.cshtml and BookKeeping2/Pages/Csv/Import.cshtml.cs
- [X] T057 [US4] Add separate transfer CSV export form, download handler, cache headers, and audit recording to the export page without changing transaction export behavior in BookKeeping2/Pages/Csv/Export.cshtml and BookKeeping2/Pages/Csv/Export.cshtml.cs
- [X] T058 [US4] Run US4 CSV parser, import, export, and page tests and fix only US4 files until green using BookKeeping2.Tests/Unit/Csv/CsvTransferImportParserTests.cs, BookKeeping2.Tests/Unit/Csv/CsvTransferImportServiceTests.cs, BookKeeping2.Tests/Unit/Csv/CsvTransferExportServiceTests.cs, and BookKeeping2.Tests/Integration/Pages/CsvTransferPageTests.cs

**Checkpoint**: Transfer CSV import/export works independently and existing transaction CSV contracts remain intact.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Verify accessibility, responsive behavior, performance, audit safety, and whole-repo quality after selected user stories are complete.

- [X] T059 [P] Add Playwright browser coverage and WCAG 2.1 AA accessibility assertions for transfer create/edit/delete, timeline transfer rows, and transfer CSV UI at mobile and desktop widths in BookKeeping2.Tests/Integration/Browser/AccountTransferBrowserTests.cs
- [X] T060 [P] Review audit masking and logging so transfer amount, note, import, and export summaries do not expose raw sensitive financial data in BookKeeping2/Services/Audit/AuditLogMaskingPolicy.cs
- [X] T061 [P] Review Traditional Chinese UI text, validation messages, button labels, and CSV summaries for transfer surfaces in BookKeeping2/Pages/Transfers/, BookKeeping2/Pages/Transactions/Index.cshtml, and BookKeeping2/Pages/Csv/
- [X] T062 Run targeted verification commands from specs/005-account-transfers/quickstart.md for AccountTransfer, TransactionTimeline, CsvTransfer, ReportService, BudgetService, persistence, and performance tests
- [X] T063 Run full build, test, and coverage verification for BookKeeping2.slnx and BookKeeping2.Tests/BookKeeping2.Tests.csproj, confirming critical transfer amount, balance, CSV, and query logic coverage is at least 80%
- [X] T064 Update manual verification notes or unresolved blockers in specs/005-account-transfers/quickstart.md if browser automation or environment constraints prevent full validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; starts immediately.
- **Foundational (Phase 2)**: Depends on Setup; blocks all user stories.
- **US1 (Phase 3)**: Depends on Foundational; establishes transfer create/edit/delete and balance behavior for the MVP.
- **US2 (Phase 4)**: Depends on Foundational and uses US1 transfer behavior for realistic payment scenarios.
- **US3 (Phase 5)**: Depends on Foundational and uses US1 transfer persistence/service behavior for timeline rows.
- **US4 (Phase 6)**: Depends on Foundational and US1 service validation because CSV import creates AccountTransfer records through the same rules.
- **Polish (Phase 7)**: Depends on all selected user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: MVP; can start after Foundational.
- **User Story 2 (P1)**: Can start after US1 has transfer service and balance behavior.
- **User Story 3 (P2)**: Can start after Foundational; practically easier after US1 because it displays created transfers.
- **User Story 4 (P2)**: Starts after US1 because import must reuse transfer validation and creation rules.

### Within Each User Story

- For each listed test task, document the intended assertions and obtain user or maintainer confirmation before writing the test.
- Write the confirmed tests and observe them fail before implementation.
- Implement view models and service contracts before services and pages.
- Implement services before Razor Page handlers that call them.
- Complete each story checkpoint before starting lower-priority stories unless parallel staffing is available.

## Parallel Opportunities

- Setup tasks T002, T003, and T004 can run in parallel after T001.
- Foundational test T005 and configuration task T007 can be prepared in parallel, but T006, T008, T009, and T010 must be integrated in order.
- US1 test tasks T012, T013, and T014 can run in parallel; T015, T016, and T017 can run in parallel before service implementation.
- US2 test tasks T028, T029, and T030 can run in parallel because they touch separate test files.
- US3 test tasks T035, T036, and T037 can run in parallel; view model task T038 can run before query and page integration.
- US4 test tasks T046, T047, T048, and T049 can run in parallel; CsvTransferRow T050 can run before parser/import/export services.
- Polish review tasks T059, T060, and T061 can run in parallel after implemented surfaces exist.

---

## Parallel Example: User Story 1

```text
Task: "T012 [P] [US1] Add failing service tests in BookKeeping2.Tests/Unit/AccountTransfers/AccountTransferServiceTests.cs"
Task: "T013 [P] [US1] Add failing balance tests in BookKeeping2.Tests/Unit/Accounts/AccountTransferBalanceTests.cs"
Task: "T014 [P] [US1] Add failing Razor Pages tests in BookKeeping2.Tests/Integration/Pages/AccountTransferPagesTests.cs"
Task: "T015 [P] [US1] Create AccountTransferInputModel in BookKeeping2/ViewModels/AccountTransfers/AccountTransferInputModel.cs"
Task: "T016 [P] [US1] Create AccountTransferFormOptionsViewModel in BookKeeping2/ViewModels/AccountTransfers/AccountTransferFormOptionsViewModel.cs"
Task: "T017 [P] [US1] Create AccountTransferResult in BookKeeping2/Services/AccountTransfers/AccountTransferResult.cs"
```

## Parallel Example: User Story 2

```text
Task: "T028 [P] [US2] Add report exclusion tests in BookKeeping2.Tests/Unit/Reports/ReportServiceTests.cs"
Task: "T029 [P] [US2] Add budget exclusion tests in BookKeeping2.Tests/Unit/Budgets/BudgetServiceTests.cs"
Task: "T030 [P] [US2] Add homepage summary tests in BookKeeping2.Tests/Integration/Pages/HomeBudgetSummaryTests.cs"
```

## Parallel Example: User Story 3

```text
Task: "T035 [P] [US3] Add timeline query tests in BookKeeping2.Tests/Unit/Transactions/TransactionTimelineQueryTests.cs"
Task: "T036 [P] [US3] Add timeline page tests in BookKeeping2.Tests/Integration/Pages/TransactionTimelineTransferTests.cs"
Task: "T037 [P] [US3] Add performance tests in BookKeeping2.Tests/Integration/Performance/TransactionTimelinePerformanceTests.cs"
Task: "T038 [P] [US3] Create TransactionTimelineItemViewModel in BookKeeping2/ViewModels/Transactions/TransactionTimelineItemViewModel.cs"
```

## Parallel Example: User Story 4

```text
Task: "T046 [P] [US4] Add transfer CSV parser tests in BookKeeping2.Tests/Unit/Csv/CsvTransferImportParserTests.cs"
Task: "T047 [P] [US4] Add transfer CSV import service tests in BookKeeping2.Tests/Unit/Csv/CsvTransferImportServiceTests.cs"
Task: "T048 [P] [US4] Add transfer CSV export service tests in BookKeeping2.Tests/Unit/Csv/CsvTransferExportServiceTests.cs"
Task: "T049 [P] [US4] Add CSV page tests in BookKeeping2.Tests/Integration/Pages/CsvTransferPageTests.cs"
Task: "T050 [P] [US4] Create CsvTransferRow in BookKeeping2/Services/Csv/CsvTransferRow.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup.
2. Complete Phase 2: Foundational persistence and audit vocabulary.
3. Complete Phase 3: User Story 1.
4. Stop and validate US1 independently with service, balance, persistence, and transfer page tests.
5. Demo create, edit, same-account validation, cross-currency validation, negative balance allowance, and soft delete.

### Incremental Delivery

1. Deliver US1 as MVP transfer CRUD and balance correctness.
2. Add US2 to prove credit card payments do not affect reports or budgets.
3. Add US3 to expose transfers in the transaction timeline.
4. Add US4 to support transfer CSV backup and restore.
5. Run Phase 7 verification after each delivery increment that will be merged.

### Parallel Team Strategy

1. One developer completes Setup and Foundational with migration ownership.
2. After Foundational, one developer works US1 service/pages while another prepares US2 exclusion tests.
3. Once US1 service contracts stabilize, US3 timeline and US4 CSV can proceed in parallel because they write mostly separate files.
4. Keep Program.cs, AppDbContext.cs, and site.css edits coordinated because multiple stories touch those shared files.

## Notes

- `[P]` tasks must use different files and must not depend on incomplete implementation from another task.
- AccountTransfer data must remain independent from income/expense Transaction records.
- Money exposed in domain and view models must use `decimal`; persistence must use `long` minor units through MoneyMinorUnitConverter.
- Transfer operations that write AccountTransfer, audit events, CSV batches, or row errors must use EF Core transactions.
- Do not add exchange rates, cross-currency transfer behavior, authentication, or credit card billing-cycle logic.
