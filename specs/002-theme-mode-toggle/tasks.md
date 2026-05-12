# Tasks: 網站主題模式切換

**Input**: Design documents from `specs/002-theme-mode-toggle/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/theme-ui-contract.md`, `quickstart.md`
**Tests**: Required by the feature specification and project constitution. Write each test task first, confirm it fails for the intended reason, then implement.
**Organization**: Tasks are grouped by user story so each story can be implemented and tested as an independent increment.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel because it touches different files and does not depend on incomplete tasks
- **[Story]**: Maps task to a user story, for example `[US1]`
- Setup, Foundational, and Polish tasks do not use story labels

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm the existing ASP.NET Core Razor Pages project and test project are ready before feature work.

- [ ] T001 Run baseline build and test commands for `BookKeeping2/BookKeeping2.csproj` and `BookKeeping2.Tests/BookKeeping2.Tests.csproj`, recording any pre-existing failures in `specs/002-theme-mode-toggle/quickstart.md`
- [ ] T002 Review current layout, homepage, static assets, and UI contract insertion points in `BookKeeping2/Pages/Shared/_Layout.cshtml`, `BookKeeping2/Pages/Index.cshtml`, `BookKeeping2/wwwroot/css/site.css`, `BookKeeping2/wwwroot/js/site.js`, and `specs/002-theme-mode-toggle/contracts/theme-ui-contract.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Add shared test scaffolding used by all theme-mode stories.

**Critical**: No user story implementation should begin until this phase is complete.

- [ ] T003 Create `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs` with `BookKeepingWebApplicationFactory`, route constants for `/`, `/Privacy`, `/Error`, and shared assertions for `[data-theme-mode-control]`
- [ ] T004 [P] Create `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs` with helpers to read `BookKeeping2/Pages/Shared/_Layout.cshtml` and `BookKeeping2/wwwroot/js/site.js`

**Checkpoint**: Test scaffolding exists and story-specific failing tests can now be added.

---

## Phase 3: User Story 1 - 在首頁切換整站主題 (Priority: P1)

**Goal**: 首頁提供亮色、暗黑、跟隨系統三選項，選取後整站套用有效主題，其他分頁不提供控制項。

**Independent Test**: 進入首頁切換任一主題，再前往 `/Privacy` 與 `/Error`，確認頁面外觀一致且只有首頁顯示主題控制項。

### Tests for User Story 1

- [ ] T005 [US1] Add failing homepage markup test for `fieldset[data-theme-mode-control]`, three `themeMode` radio values, and labels「亮色模式」「暗黑模式」「跟隨系統」in `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs`
- [ ] T006 [US1] Add failing non-homepage test verifying `/Privacy` and `/Error` do not render `[data-theme-mode-control]` in `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs`
- [ ] T007 [US1] Add failing layout contract test verifying the pre-paint theme initialization script appears before Bootstrap CSS and `~/js/site.js` remains referenced in `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs`

### Implementation for User Story 1

- [ ] T008 [US1] Add a minimal pre-paint theme initialization script before the Bootstrap stylesheet in `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T009 [US1] Update Bootstrap theme-compatible navbar/link classes and preserve shared static asset references in `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T010 [US1] Add the zh-TW radio `fieldset` theme mode control with stable selectors and labels to `BookKeeping2/Pages/Index.cshtml`
- [ ] T011 [US1] Implement core theme mode selection, current-page application, control checked-state synchronization, and `localStorage` writes in `BookKeeping2/wwwroot/js/site.js`
- [ ] T012 [US1] Add theme control layout, light/dark compatible colors, and visible focus styling in `BookKeeping2/wwwroot/css/site.css`
- [ ] T013 [US1] Run targeted US1 tests in `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs`

**Checkpoint**: User Story 1 is functional and testable independently as the MVP.

---

## Phase 4: User Story 2 - 跟隨系統外觀偏好 (Priority: P2)

**Goal**: 「跟隨系統」模式依 `prefers-color-scheme` 推導有效主題，系統偏好變更時自動更新，明確亮色/暗黑選擇不被系統變更覆蓋。

**Independent Test**: 將模式設為 `system` 後模擬或切換系統亮暗偏好，確認網站於 2 秒內套用相符有效主題；將模式設為 `light` 或 `dark` 時確認系統變更不覆蓋使用者選擇。

### Tests for User Story 2

- [ ] T014 [US2] Add failing script contract test for `matchMedia('(prefers-color-scheme: dark)')`, `system` derivation, and light fallback behavior in `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs`
- [ ] T015 [US2] Add failing script contract test that explicit `light` and `dark` modes bypass system preference changes in `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs`

### Implementation for User Story 2

- [ ] T016 [US2] Extend the pre-paint script to derive `system` mode from `matchMedia('(prefers-color-scheme: dark)')` with light fallback in `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T017 [US2] Extend runtime behavior to subscribe to `prefers-color-scheme` changes only when mode is `system`, leaving explicit `light` and `dark` modes unchanged in `BookKeeping2/wwwroot/js/site.js`
- [ ] T018 [US2] Run targeted US2 tests in `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs`

**Checkpoint**: User Story 2 works independently for system-following behavior.

---

## Phase 5: User Story 3 - 保留偏好並維持可讀性 (Priority: P3)

**Goal**: 同一瀏覽器與裝置保留最近選取模式，無效或不可讀偏好回到 `system`，同站分頁同步，且三種模式在主要頁面保持可讀、可操作、符合對比與焦點要求。

**Independent Test**: 選取主題後重新載入網站確認偏好生效；把保存值改成無效值確認 fallback；開啟兩個同站分頁確認 2 秒內同步；在手機、平板、桌面寬度檢查首頁、隱私權頁、錯誤頁不重疊且焦點可見。

### Tests for User Story 3

- [ ] T019 [US3] Add failing script contract test for allow-list validation, invalid `localStorage` fallback to `system`, and storage key `bookkeeping.theme.mode` in `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs`
- [ ] T020 [US3] Add failing script contract test for `storage` event synchronization and no finance endpoint calls from theme switching code in `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs`
- [ ] T021 [US3] Add failing accessibility markup and focus-style tests for the homepage control and shared styles in `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs`

### Implementation for User Story 3

- [ ] T022 [US3] Harden the pre-paint script with allow-list validation, `localStorage` try/catch handling, `data-theme-mode`, and safe `system` fallback in `BookKeeping2/Pages/Shared/_Layout.cshtml`
- [ ] T023 [US3] Harden runtime behavior for validated reads/writes, `storage` event synchronization, exception fallback, and focus preservation in `BookKeeping2/wwwroot/js/site.js`
- [ ] T024 [US3] Update responsive, contrast, and focus-visible styles for homepage cards, nav, forms, tables, alerts, footer, and theme controls in `BookKeeping2/wwwroot/css/site.css`
- [ ] T025 [US3] Execute manual responsive, persistence, cross-tab, keyboard, and contrast checks from `specs/002-theme-mode-toggle/quickstart.md`
- [ ] T026 [US3] Run targeted US3 tests in `BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs` and `BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs`

**Checkpoint**: All user stories are independently functional and verified.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and constitutional checks across the feature.

- [ ] T027 Run full project build for `BookKeeping2/BookKeeping2.csproj`
- [ ] T028 Run full automated test suite for `BookKeeping2.Tests/BookKeeping2.Tests.csproj`
- [ ] T029 [P] Verify the feature did not add SQLite schema, EF Core migration, cookie, session, server log, or audit persistence changes under `BookKeeping2/Data/`, `BookKeeping2/Migrations/`, and `BookKeeping2/Program.cs`
- [ ] T030 [P] Verify all user-facing theme text remains Traditional Chinese in `BookKeeping2/Pages/Index.cshtml`, `BookKeeping2/Pages/Shared/_Layout.cshtml`, and `BookKeeping2/wwwroot/js/site.js`
- [ ] T031 [P] Verify theme switching code does not submit forms, call finance endpoints, or expose sensitive data in `BookKeeping2/wwwroot/js/site.js`
- [ ] T032 Complete the quickstart validation checklist and update any discovered follow-up notes in `specs/002-theme-mode-toggle/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies.
- **Foundational (Phase 2)**: Depends on Setup and blocks all user story work.
- **US1 (Phase 3)**: Depends on Foundational and is the MVP.
- **US2 (Phase 4)**: Depends on Foundational; can be developed independently but touches shared theme files, so coordinate with US1 edits.
- **US3 (Phase 5)**: Depends on Foundational; can be developed independently but final polish should validate it with US1 and US2.
- **Polish (Phase 6)**: Depends on all desired user stories being complete.

### User Story Dependencies

- **User Story 1 (P1)**: No story dependency after Foundational. Provides the MVP control and site-wide theme application.
- **User Story 2 (P2)**: No story dependency after Foundational, but shares `_Layout.cshtml` and `site.js` with US1.
- **User Story 3 (P3)**: No story dependency after Foundational, but shares `_Layout.cshtml`, `site.js`, and `site.css` with US1 and US2.

### Within Each User Story

- Write failing tests first and confirm they fail for the intended reason.
- Implement layout hooks before runtime JavaScript that depends on them.
- Implement runtime JavaScript before final CSS and manual browser checks.
- Run targeted tests before moving to the next story checkpoint.

### Parallel Opportunities

- T004 can run in parallel with T003 after Setup because it creates a separate test file.
- T029, T030, and T031 can run in parallel during Polish because they inspect different concerns.
- Different user stories can be assigned in parallel after Phase 2 if contributors coordinate edits to `_Layout.cshtml`, `site.js`, and `site.css`.

---

## Parallel Example: User Story Work

```text
Task: "Add failing homepage/non-homepage/layout tests in BookKeeping2.Tests/Integration/Pages/ThemeModePageTests.cs"
Task: "Add failing script contract tests in BookKeeping2.Tests/Integration/StaticAssets/ThemeModeScriptContractTests.cs"
Task: "Update responsive and contrast styles in BookKeeping2/wwwroot/css/site.css"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 only.
3. Stop and validate homepage switching, non-homepage absence of the control, pre-paint hook placement, and targeted US1 tests.
4. Demo or review the MVP before adding system preference and persistence hardening.

### Incremental Delivery

1. Setup plus Foundational creates the test harness.
2. US1 delivers visible homepage switching and site-wide application.
3. US2 adds `system` behavior and system preference change handling.
4. US3 adds persistence hardening, cross-tab synchronization, responsive checks, and accessibility validation.
5. Polish confirms build, tests, data integrity, security, and quickstart coverage.

