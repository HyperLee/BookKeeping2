# Tasks: 網站介面英文語系切換

**Input**: Design documents from `/specs/003-site-language-toggle/`
**Prerequisites**: [plan.md](./plan.md), [spec.md](./spec.md), [research.md](./research.md), [data-model.md](./data-model.md), [contracts/language-ui-contract.md](./contracts/language-ui-contract.md), [quickstart.md](./quickstart.md)

**Tests**: 本功能規格與憲章明確要求測試先行，因此每個故事都包含必須先撰寫並確認失敗的測試任務。

**Organization**: 任務依使用者故事分組，讓每個故事可獨立實作、測試與驗收。

**TDD Gate**: 每個 `Tests for User Story` 區段開始前，必須先向使用者或維護者確認該故事的失敗測試意圖；未取得確認前不得撰寫或執行該故事的測試任務。

## Format: `[ID] [P?] [Story] Description`

- **[P]**: 可平行執行，因為修改不同檔案且不依賴尚未完成的任務
- **[Story]**: 使用者故事標籤，例如 [US1]、[US2]、[US3]
- **Description**: 每個任務都包含具體檔案路徑

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: 建立 localization 與測試檔案骨架，供後續紅綠重構流程使用。

- [X] T001 Create localization marker and option constants skeletons in `BookKeeping2/Localization/SharedResource.cs` and `BookKeeping2/Localization/UiLanguageOptions.cs`
- [X] T002 [P] Create the English resource file skeleton in `BookKeeping2/Resources/SharedResource.en.resx`
- [X] T003 [P] Create the language browser fixture skeleton in `BookKeeping2.Tests/TestSupport/LanguageToggleBrowserFixture.cs`
- [X] T004 [P] Create the provider test file skeleton in `BookKeeping2.Tests/Unit/Localization/UiLanguageRequestCultureProviderTests.cs`
- [X] T005 [P] Create the page integration test file skeleton in `BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs`
- [X] T006 [P] Create the resource completeness test file skeleton in `BookKeeping2.Tests/Integration/StaticAssets/LanguageResourceCompletenessTests.cs`
- [X] T007 [P] Create the browser test file skeleton in `BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: 建立所有故事共用的 request culture、resource lookup 與 Razor localization 基礎。

**Critical**: 此階段完成前，不應開始任何使用者故事的實作。

- [ ] T008 [P] Add failing allow-list, invalid value, default fallback, and fixed culture tests in `BookKeeping2.Tests/Unit/Localization/UiLanguageRequestCultureProviderTests.cs`
- [ ] T009 [P] Add failing resource key, blank value, and placeholder coverage tests in `BookKeeping2.Tests/Integration/StaticAssets/LanguageResourceCompletenessTests.cs`
- [ ] T010 Implement `zh-TW` and `en` language constants, cookie name, cookie lifetime, and html lang mapping in `BookKeeping2/Localization/UiLanguageOptions.cs`
- [ ] T011 Implement the custom Cookie-only request culture provider in `BookKeeping2/Localization/UiLanguageRequestCultureProvider.cs`
- [ ] T012 Register localization services, DataAnnotations display localization, supported UI cultures, the custom provider, and `UseRequestLocalization` ordering in `BookKeeping2/Program.cs`
- [ ] T013 Add `SharedResource` localizer using/import support for Razor Pages in `BookKeeping2/Pages/_ViewImports.cshtml`
- [ ] T014 Populate foundational English resource keys for language names, layout text, DataAnnotations display text, and shared actions while keeping user-facing validation/error correction messages in Traditional Chinese in `BookKeeping2/Resources/SharedResource.en.resx`
- [ ] T015 Run the foundational tests with `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~UiLanguageRequestCultureProviderTests|FullyQualifiedName~LanguageResourceCompletenessTests"` and make provider/resource tests pass in `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: Foundation ready. User story implementation can now proceed in priority order or in parallel with clear file ownership.

---

## Phase 3: User Story 1 - 在首頁切換整站介面語言 (Priority: P1) MVP

**Goal**: 使用者可在首頁選擇繁體中文或 English，選擇後首頁與後續站內頁面以所選語言呈現。

**Independent Test**: 進入首頁，切換 English，瀏覽首頁、交易、分類、帳戶、預算、報表、CSV、隱私權與錯誤頁，確認固定 UI 文字以英文顯示；再切回繁體中文確認站內頁面回到繁中。

### Tests for User Story 1

> Confirm the test intent with the user or maintainer first. Then write these tests and confirm they fail before implementation.

- [ ] T016 [P] [US1] Add failing homepage language control, selected option, home-only control, and English rendering tests in `BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs`
- [ ] T017 [P] [US1] Add failing keyboard language selection, one-second selected-language rendering, and post-navigation language rendering tests in `BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs`
- [ ] T018 [P] [US1] Add failing contract assertions for anti-forgery language form markup and no global text replacement in `BookKeeping2.Tests/Integration/StaticAssets/LanguageResourceCompletenessTests.cs`

### Implementation for User Story 1

- [ ] T019 [US1] Add the non-financial language POST handler, selected language state, and redirect behavior in `BookKeeping2/Pages/Index.cshtml.cs`
- [ ] T020 [US1] Add the accessible homepage language control beside the existing theme control in `BookKeeping2/Pages/Index.cshtml`
- [ ] T021 [US1] Localize `<html lang>`, page title composition, navigation, footer, and shared layout text in `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T022 [P] [US1] Localize homepage headings, summary labels, links, empty states, and table headers in `BookKeeping2/Pages/Index.cshtml`
- [ ] T023 [P] [US1] Localize pagination text and labels in `BookKeeping2/Pages/Shared/_Pagination.cshtml`
- [ ] T024 [P] [US1] Localize account and category page fixed UI text in `BookKeeping2/Pages/Accounts/Index.cshtml` and `BookKeeping2/Pages/Categories/Index.cshtml`
- [ ] T025 [P] [US1] Localize budget and report page fixed UI text in `BookKeeping2/Pages/Budgets/Index.cshtml` and `BookKeeping2/Pages/Reports/Index.cshtml`
- [ ] T026 [P] [US1] Localize transaction list, create, edit, delete, and shared form fixed UI text in `BookKeeping2/Pages/Transactions/Index.cshtml`, `BookKeeping2/Pages/Transactions/Create.cshtml`, `BookKeeping2/Pages/Transactions/Edit.cshtml`, `BookKeeping2/Pages/Transactions/Delete.cshtml`, and `BookKeeping2/Pages/Transactions/_TransactionForm.cshtml`
- [ ] T027 [P] [US1] Localize CSV, privacy, and error page fixed UI text in `BookKeeping2/Pages/Csv/Import.cshtml`, `BookKeeping2/Pages/Csv/Export.cshtml`, `BookKeeping2/Pages/Privacy.cshtml`, and `BookKeeping2/Pages/Error.cshtml`
- [ ] T028 [US1] Add homepage progressive enhancement for the language form without global text replacement in `BookKeeping2/wwwroot/js/site.js`
- [ ] T029 [US1] Add English translations for all US1 layout, homepage, page heading, button, link, table, empty-state, and shared partial keys in `BookKeeping2/Resources/SharedResource.en.resx`
- [ ] T030 [US1] Run `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~LanguageTogglePageTests|FullyQualifiedName~LanguageToggleBrowserTests"` and make US1 tests pass in `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Story 1 is independently testable as the MVP.

---

## Phase 4: User Story 2 - 預設維持繁體中文並保留手動選擇 (Priority: P2)

**Goal**: 沒有有效 Cookie 時一律顯示繁體中文，不依 `Accept-Language` 自動切換；手動選擇會以安全 Cookie 保存並在回訪時套用。

**Independent Test**: 清除或放入無效 `bookkeeping.ui.language` Cookie 後開啟網站，確認繁中 fallback；送出 `Accept-Language: en-US` 仍維持繁中；手動選擇 English 後重新整理與回訪仍顯示英文。

### Tests for User Story 2

> Confirm the test intent with the user or maintainer first. Then write these tests and confirm they fail before implementation.

- [ ] T031 [P] [US2] Add failing missing-cookie, invalid-cookie, and ignored `Accept-Language` integration tests in `BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs`
- [ ] T032 [US2] Add failing cookie attribute and one-year expiry tests for the homepage POST handler in `BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs`
- [ ] T033 [P] [US2] Add failing reload, return visit, and checked selected option browser tests in `BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs`

### Implementation for User Story 2

- [ ] T034 [US2] Harden the homepage language POST handler to allow-list values and write only `bookkeeping.ui.language` with Path, HttpOnly, IsEssential, SameSite, Secure, and one-year expiry in `BookKeeping2/Pages/Index.cshtml.cs`
- [ ] T035 [US2] Ensure request localization ignores `Accept-Language` and falls back to `zh-TW` for missing or invalid Cookie values in `BookKeeping2/Program.cs`
- [ ] T036 [US2] Ensure the homepage language control reflects the resolved current language after reloads and invalid Cookie fallback in `BookKeeping2/Pages/Index.cshtml`
- [ ] T037 [US2] Add English translations for US2 fallback and selected-state keys while preserving Traditional Chinese user-facing validation/error correction messages in `BookKeeping2/Resources/SharedResource.en.resx`
- [ ] T038 [US2] Run `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~LanguageTogglePageTests|FullyQualifiedName~LanguageToggleBrowserTests|FullyQualifiedName~UiLanguageRequestCultureProviderTests"` and make US2 tests pass in `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: User Stories 1 and 2 both work independently.

---

## Phase 5: User Story 3 - 完整覆蓋既有介面文字與可用性 (Priority: P3)

**Goal**: 英文模式涵蓋現有記帳流程的表單標籤、非錯誤狀態、CSV 頁面訊息、圖表標籤與可用性；使用者面向錯誤、驗證與可執行修正提示依憲章維持繁體中文，同時不翻譯使用者資料或改變財務結果。

**Independent Test**: 在英文模式下操作新增交易、交易驗證、分類與帳戶管理、預算、報表、CSV 匯入匯出與 responsive layout，確認可翻譯 UI 為英文、錯誤/驗證修正訊息維持繁體中文、資料原文與 CSV contract 保持不變。

### Tests for User Story 3

> Confirm the test intent with the user or maintainer first. Then write these tests and confirm they fail before implementation.

- [ ] T039 [P] [US3] Add failing DataAnnotations display localization tests and Traditional Chinese validation/error message preservation tests in `BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs`
- [ ] T040 [US3] Add failing account, category, budget, transaction, report, CSV non-error status message localization tests, and custom user-data invariant tests in `BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs`
- [ ] T041 [P] [US3] Add failing CSV export header, transaction type, amount/date, and custom account/category/note preservation tests for English mode in `BookKeeping2.Tests/Integration/Pages/CsvExportPageTests.cs`
- [ ] T042 [P] [US3] Add failing CSV import Traditional Chinese contract, raw value, and persisted data preservation tests for English mode in `BookKeeping2.Tests/Integration/Pages/CsvImportPageTests.cs`
- [ ] T043 [P] [US3] Add failing mobile, tablet, desktop, focus, overflow, theme-coexistence, and one-second selected-language rendering browser tests in `BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs`

### Implementation for User Story 3

- [ ] T044 [US3] Localize DataAnnotations display names while keeping validation/error messages in Traditional Chinese in `BookKeeping2/ViewModels/Accounts/AccountViewModels.cs`, `BookKeeping2/ViewModels/Categories/CategoryViewModels.cs`, `BookKeeping2/ViewModels/Budgets/BudgetViewModels.cs`, `BookKeeping2/ViewModels/Transactions/TransactionFilterInputModel.cs`, and `BookKeeping2/ViewModels/Transactions/TransactionInputModel.cs`
- [ ] T045 [US3] Preserve shared financial validation messages in Traditional Chinese without changing money/date rules in `BookKeeping2/Validation/FinancialValidationMessages.cs`
- [ ] T046 [US3] Localize non-error PageModel status messages while keeping user-facing error/correction messages in Traditional Chinese in `BookKeeping2/Pages/Accounts/Index.cshtml.cs`, `BookKeeping2/Pages/Categories/Index.cshtml.cs`, `BookKeeping2/Pages/Budgets/Index.cshtml.cs`, `BookKeeping2/Pages/Transactions/Create.cshtml.cs`, `BookKeeping2/Pages/Transactions/Edit.cshtml.cs`, and `BookKeeping2/Pages/Transactions/Delete.cshtml.cs`
- [ ] T047 [US3] Localize non-error service result and formatter messages while keeping user-facing errors in Traditional Chinese and audit summaries masked and stable in `BookKeeping2/Services/Accounts/AccountService.cs`, `BookKeeping2/Services/Categories/CategoryService.cs`, `BookKeeping2/Services/Budgets/BudgetService.cs`, `BookKeeping2/Services/Transactions/TransactionService.cs`, and `BookKeeping2/Services/Csv/CsvImportResultFormatter.cs`
- [ ] T048 [US3] Add display-only system label localization helpers for transaction type, account type, budget alert state, and default category labels in `BookKeeping2/Localization/SystemDisplayLocalizer.cs`
- [ ] T049 [US3] Apply display-only localized labels without changing persisted values in `BookKeeping2/ViewModels/Categories/CategoryViewModels.cs`, `BookKeeping2/ViewModels/Transactions/TransactionListItemViewModel.cs`, `BookKeeping2/ViewModels/Transactions/TransactionFormOptionsViewModel.cs`, `BookKeeping2/ViewModels/Accounts/AccountViewModels.cs`, and `BookKeeping2/ViewModels/Budgets/BudgetViewModels.cs`
- [ ] T050 [US3] Localize CSV page UI, non-error import result presentation, and export page copy while preserving CSV parser/exporter fixed Traditional Chinese contract and Traditional Chinese error/validation messages in `BookKeeping2/Pages/Csv/Import.cshtml`, `BookKeeping2/Pages/Csv/Import.cshtml.cs`, `BookKeeping2/Pages/Csv/Export.cshtml`, `BookKeeping2/Services/Csv/CsvExportService.cs`, and `BookKeeping2/Services/Csv/CsvImportParser.cs`
- [ ] T051 [US3] Localize report chart labels and accessible chart text without changing totals in `BookKeeping2/Pages/Reports/Index.cshtml` and `BookKeeping2/wwwroot/js/reports.js`
- [ ] T052 [US3] Add remaining English resource entries for forms, non-error status, enum labels, default categories, charts, CSV page UI, and confirmations while excluding user-facing validation/error correction messages from English resources in `BookKeeping2/Resources/SharedResource.en.resx`
- [ ] T053 [US3] Adjust responsive layout, focus indicators, and long English text behavior for language controls, nav, tables, forms, alerts, and footer in `BookKeeping2/wwwroot/css/site.css`
- [ ] T054 [US3] Run `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~LanguageTogglePageTests|FullyQualifiedName~CsvExportPageTests|FullyQualifiedName~CsvImportPageTests|FullyQualifiedName~LanguageResourceCompletenessTests|FullyQualifiedName~LanguageToggleBrowserTests"` and make US3 tests pass in `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

**Checkpoint**: All user stories are independently functional and covered by targeted tests.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: 完成全功能驗證、回歸檢查與文件一致性確認。

- [ ] T055 [P] Verify no SQLite schema, migration, audit, session, localStorage persistence, server log entry, or telemetry capture was added for language preference in `BookKeeping2/Data/AppDbContext.cs`, `BookKeeping2/Data/Migrations/`, `BookKeeping2/Services/Audit/AuditService.cs`, `BookKeeping2/Program.cs`, `BookKeeping2/Pages/Index.cshtml.cs`, and `BookKeeping2/wwwroot/js/site.js`
- [ ] T056 [P] Verify the manual quickstart scenarios are still accurate after implementation in `specs/003-site-language-toggle/quickstart.md`
- [ ] T057 [P] Run `dotnet build BookKeeping2.slnx` and fix build warnings or errors in `BookKeeping2.slnx`
- [ ] T058 Run `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj` and fix regressions in `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [ ] T059 Inspect the final diff for resource keys, untranslated UI strings, CSV contract preservation, and responsive CSS scope in `BookKeeping2/Resources/SharedResource.en.resx`, `BookKeeping2/Pages/`, `BookKeeping2/Services/Csv/`, and `BookKeeping2/wwwroot/css/site.css`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 Setup**: No dependencies; can start immediately.
- **Phase 2 Foundational**: Depends on Phase 1; blocks all user stories.
- **Phase 3 US1**: Depends on Phase 2; delivers MVP language switching.
- **Phase 4 US2**: Depends on Phase 2 and should be validated after US1 for the complete homepage flow.
- **Phase 5 US3**: Depends on Phase 2 and builds on localized surfaces from US1.
- **Phase 6 Polish**: Depends on all desired user stories being complete.

### User Story Dependencies

- **US1 (P1)**: Can start after Foundational; no dependency on US2 or US3.
- **US2 (P2)**: Can start after Foundational; uses the same homepage handler and culture provider as US1 but remains independently testable through Cookie/default behavior.
- **US3 (P3)**: Can start after Foundational; completes coverage for Traditional Chinese validation/error preservation, non-error service messages, CSV contract, charts, responsive layout, and accessibility.

### Within Each User Story

- Test intent must be confirmed with the user or maintainer before each story's tests are written.
- Tests must be written first and observed failing before implementation.
- Resource and provider foundations must exist before localized Razor/PageModel changes.
- PageModel and service localization should precede browser verification.
- CSV contract tests must pass before any CSV UI localization is considered complete.

---

## Parallel Opportunities

- T002-T007 can run in parallel after T001 ownership is agreed.
- T008 and T009 can run in parallel because they touch different test files.
- US1 tests T016-T018 can run in parallel.
- US1 page localization tasks T022-T027 can run in parallel after T021 establishes layout patterns; coordinate with the T029 resource owner as each page introduces keys.
- US2 tests T031-T033 can run in parallel if file ownership in `LanguageTogglePageTests.cs` and `LanguageToggleBrowserTests.cs` is coordinated.
- US3 tests T039-T043 can run in parallel by assigning different test files or non-overlapping regions.
- US3 implementation T044-T053 can be split by page/service/resource/CSS ownership after shared display-label APIs are defined.
- Polish verification T055-T057 can run in parallel before the full test run T058.

## Parallel Example: User Story 1

```text
Task: "T016 Add failing homepage language control, selected option, home-only control, and English rendering tests in BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs"
Task: "T017 Add failing keyboard language selection, one-second rendering, and post-navigation language rendering tests in BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs"
Task: "T018 Add failing contract assertions for anti-forgery language form markup and no global text replacement in BookKeeping2.Tests/Integration/StaticAssets/LanguageResourceCompletenessTests.cs"
```

```text
Task: "T024 Localize account and category page fixed UI text in BookKeeping2/Pages/Accounts/Index.cshtml and BookKeeping2/Pages/Categories/Index.cshtml"
Task: "T025 Localize budget and report page fixed UI text in BookKeeping2/Pages/Budgets/Index.cshtml and BookKeeping2/Pages/Reports/Index.cshtml"
Task: "T026 Localize transaction pages and shared form fixed UI text in BookKeeping2/Pages/Transactions/"
Task: "T027 Localize CSV, privacy, and error page fixed UI text in BookKeeping2/Pages/Csv/, BookKeeping2/Pages/Privacy.cshtml, and BookKeeping2/Pages/Error.cshtml"
```

## Parallel Example: User Story 2

```text
Task: "T031 Add failing missing-cookie, invalid-cookie, and ignored Accept-Language integration tests in BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs"
Task: "T033 Add failing reload, return visit, and checked selected option browser tests in BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs"
```

```text
Task: "T034 Harden the homepage language POST handler in BookKeeping2/Pages/Index.cshtml.cs"
Task: "T035 Ensure request localization ignores Accept-Language in BookKeeping2/Program.cs"
Task: "T036 Ensure the homepage language control reflects the resolved current language in BookKeeping2/Pages/Index.cshtml"
```

## Parallel Example: User Story 3

```text
Task: "T039 Add failing DataAnnotations display localization and Traditional Chinese validation/error preservation tests in BookKeeping2.Tests/Integration/Pages/LanguageTogglePageTests.cs"
Task: "T041 Add failing CSV export contract tests in BookKeeping2.Tests/Integration/Pages/CsvExportPageTests.cs"
Task: "T042 Add failing CSV import contract tests in BookKeeping2.Tests/Integration/Pages/CsvImportPageTests.cs"
Task: "T043 Add failing responsive and accessibility browser tests in BookKeeping2.Tests/Integration/Browser/LanguageToggleBrowserTests.cs"
```

```text
Task: "T044 Localize DataAnnotations display names and preserve Traditional Chinese validation/error messages in BookKeeping2/ViewModels/"
Task: "T047 Localize non-error service result and formatter messages while preserving Traditional Chinese errors in BookKeeping2/Services/"
Task: "T050 Localize CSV page UI while preserving CSV contract in BookKeeping2/Pages/Csv/ and BookKeeping2/Services/Csv/"
Task: "T053 Adjust responsive layout and focus behavior in BookKeeping2/wwwroot/css/site.css"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 setup.
2. Complete Phase 2 foundational localization provider, resources, and middleware.
3. Complete Phase 3 US1 tests first, observe failure, then implement homepage control and core site rendering.
4. Stop and validate US1 with the targeted test command from T030.

### Incremental Delivery

1. Foundation ready: custom Cookie provider, localization services, shared resource file.
2. US1: language switch is visible on homepage and applies to site pages.
3. US2: default/fallback/persistence behavior is hardened and independently verified.
4. US3: all existing UI surfaces, Traditional Chinese validation/error preservation, CSV UI, charts, responsive layout, and accessibility are completed.
5. Polish: build, full tests, quickstart, and no-persistence/no-schema verification.

### Parallel Team Strategy

1. One engineer owns `Program.cs`, `Localization/`, and resource conventions through Phase 2.
2. One engineer owns Razor Page localization for US1.
3. One engineer owns Cookie/default/persistence tests and handler hardening for US2.
4. One engineer owns validation/error preservation, service/CSV/chart/resource completeness for US3.
5. Browser and CSS verification can run after each story reaches its checkpoint.

## Notes

- `[P]` tasks must not edit the same file concurrently without coordination.
- All user-facing documentation and UI text must remain Traditional Chinese by default, with English added through resources.
- Do not add SQLite schema, migrations, sessions, localStorage, audit events, or server logs for language preference.
- Do not change CSV exported headers or transaction type values: `日期,類型,金額,分類,帳戶,備註`, `收入`, `支出`.
- Do not translate user-entered account names, custom category names, transaction notes, or CSV raw values.
- Keep user-facing validation, error, and actionable correction messages in Traditional Chinese unless the constitution is formally revised.
