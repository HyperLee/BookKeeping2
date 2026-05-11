---

description: "Task list template for feature implementation"
---

# 任務: [FEATURE NAME]

**輸入**: `/specs/[###-feature-name]/` 內的設計文件
**前置文件**: plan.md 必填、spec.md 必填、research.md、data-model.md、contracts/
**測試要求**: 本專案憲章要求 TDD。每個使用者故事 MUST 先建立會失敗的測試，
再進行實作。若某項測試不適用，MUST 在任務中寫明原因。
**組織方式**: 任務依使用者故事分組，確保每個故事可獨立實作、測試與交付。

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

<!--
  ============================================================================
  IMPORTANT:
  下列任務是範例。/speckit-tasks MUST 依 spec.md、plan.md、data-model.md
  與 contracts/ 產生實際任務，並移除不適用的範例。

  任務 MUST:
  - 依 P1、P2、P3 使用者故事分組
  - 每個故事先列測試任務，再列實作任務
  - 包含資料完整性、安全、稽核、效能與繁體中文文件要求
  - 避免同一檔案在平行任務中被多人同時修改
  ============================================================================
-->

## Phase 1: Setup（共用基礎）

**目的**: 建立功能所需的專案、測試與工具基礎

- [ ] T001 確認 `BookKeeping2/BookKeeping2.csproj` 的 target framework 與 nullable 設定符合計畫
- [ ] T002 建立或更新 `BookKeeping2.Tests/BookKeeping2.Tests.csproj`，加入 xUnit、Moq、WebApplicationFactory
- [ ] T003 [P] 設定測試資料與共用 fixture 於 `BookKeeping2.Tests/TestSupport/`
- [ ] T004 [P] 確認 `.editorconfig`、格式化與建置命令可用

---

## Phase 2: Foundational（阻塞性前置）

**目的**: 完成所有使用者故事共用且不可延後的基礎能力

**CRITICAL**: 此階段完成前不得開始任何使用者故事實作。

- [ ] T005 建立或更新核心資料模型於 `BookKeeping2/Models/`，確保金額使用 `decimal`
- [ ] T006 建立或更新服務介面與 DI 註冊於 `BookKeeping2/Services/` 與 `BookKeeping2/Program.cs`
- [ ] T007 建立伺服器端驗證規則，涵蓋金額、分類、日期與必要欄位
- [ ] T008 設定 Anti-Forgery、錯誤處理與安全標頭策略
- [ ] T009 建立結構化日誌與帳目異動稽核基礎
- [ ] T010 定義資料匯出、備份或復原策略的最小可行支援

**Checkpoint**: Foundation ready。使用者故事可依優先級開始實作。

---

## Phase 3: User Story 1 - [Title] (Priority: P1)

**Goal**: [簡述此故事交付的使用者價值]

**Independent Test**: [描述此故事如何單獨驗證]

### Tests for User Story 1（必須先寫且先失敗）

- [ ] T011 [P] [US1] 撰寫金額、日期或分類規則的單元測試於 `BookKeeping2.Tests/Unit/[Feature]Tests.cs`
- [ ] T012 [P] [US1] 撰寫 Razor Page 或使用者流程整合測試於 `BookKeeping2.Tests/Integration/[Feature]Tests.cs`
- [ ] T013 [US1] 執行測試並確認失敗原因符合尚未實作的需求

### Implementation for User Story 1

- [ ] T014 [P] [US1] 建立或更新模型於 `BookKeeping2/Models/[Entity].cs`
- [ ] T015 [US1] 實作服務邏輯於 `BookKeeping2/Services/[Service].cs`
- [ ] T016 [US1] 實作 Razor Page 與 PageModel 於 `BookKeeping2/Pages/[Feature]/`
- [ ] T017 [US1] 加入繁體中文驗證訊息與錯誤處理
- [ ] T018 [US1] 加入結構化日誌與帳目異動稽核
- [ ] T019 [US1] 執行相關測試並確認通過

**Checkpoint**: User Story 1 必須可獨立運作、獨立測試並可展示。

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [簡述此故事交付的使用者價值]

**Independent Test**: [描述此故事如何單獨驗證]

### Tests for User Story 2（必須先寫且先失敗）

- [ ] T020 [P] [US2] 撰寫單元測試於 `BookKeeping2.Tests/Unit/[Feature]Tests.cs`
- [ ] T021 [P] [US2] 撰寫整合測試於 `BookKeeping2.Tests/Integration/[Feature]Tests.cs`
- [ ] T022 [US2] 執行測試並確認失敗原因符合尚未實作的需求

### Implementation for User Story 2

- [ ] T023 [P] [US2] 建立或更新模型於 `BookKeeping2/Models/[Entity].cs`
- [ ] T024 [US2] 實作服務邏輯於 `BookKeeping2/Services/[Service].cs`
- [ ] T025 [US2] 實作 Razor Page 與 PageModel 於 `BookKeeping2/Pages/[Feature]/`
- [ ] T026 [US2] 整合 US1 元件時保持 US2 可獨立測試
- [ ] T027 [US2] 執行相關測試並確認通過

**Checkpoint**: User Story 1 與 User Story 2 必須都能獨立運作。

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [簡述此故事交付的使用者價值]

**Independent Test**: [描述此故事如何單獨驗證]

### Tests for User Story 3（必須先寫且先失敗）

- [ ] T028 [P] [US3] 撰寫單元測試於 `BookKeeping2.Tests/Unit/[Feature]Tests.cs`
- [ ] T029 [P] [US3] 撰寫整合測試於 `BookKeeping2.Tests/Integration/[Feature]Tests.cs`
- [ ] T030 [US3] 執行測試並確認失敗原因符合尚未實作的需求

### Implementation for User Story 3

- [ ] T031 [P] [US3] 建立或更新模型於 `BookKeeping2/Models/[Entity].cs`
- [ ] T032 [US3] 實作服務邏輯於 `BookKeeping2/Services/[Service].cs`
- [ ] T033 [US3] 實作 Razor Page 與 PageModel 於 `BookKeeping2/Pages/[Feature]/`
- [ ] T034 [US3] 執行相關測試並確認通過

**Checkpoint**: 所有目標使用者故事必須可獨立運作並維持既有故事不回歸。

---

[依需要新增更多使用者故事階段，維持相同結構]

---

## Phase N: Polish & Cross-Cutting Concerns

**目的**: 完成跨故事品質、文件與合規工作

- [ ] TXXX [P] 更新繁體中文文件於 `docs/` 或 `specs/[###-feature-name]/`
- [ ] TXXX 執行程式碼清理與重構，維持 `.editorconfig` 格式
- [ ] TXXX 補齊關鍵業務邏輯測試覆蓋率，尤其是金額計算
- [ ] TXXX 執行安全檢查，確認無敏感資料明文日誌、缺少 CSRF 或高風險漏洞
- [ ] TXXX 驗證回應式版面、繁體中文錯誤訊息與基本可及性
- [ ] TXXX 驗證查詢、報表或大量資料情境的效能目標
- [ ] TXXX 執行 `quickstart.md` 驗證流程

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: 無相依，可立即開始
- **Foundational (Phase 2)**: 依賴 Setup 完成，阻塞所有使用者故事
- **User Stories (Phase 3+)**: 依賴 Foundational 完成，之後可依優先級或人力平行處理
- **Polish (Final Phase)**: 依賴所有目標使用者故事完成

### User Story Dependencies

- **User Story 1 (P1)**: Foundational 後即可開始，不依賴其他故事
- **User Story 2 (P2)**: Foundational 後即可開始；若整合 US1，仍 MUST 可獨立測試
- **User Story 3 (P3)**: Foundational 後即可開始；若整合 US1/US2，仍 MUST 可獨立測試

### Within Each User Story

- 測試 MUST 先撰寫並先失敗
- Models before Services
- Services before Razor Pages
- 驗證、錯誤處理、日誌與稽核不得延後到故事完成後才補
- 每個故事完成後 MUST 獨立驗證，再進入下一個優先級

### Parallel Opportunities

- 標記 [P] 的 Setup 任務可平行執行
- Foundational 中不同檔案且無相依的任務可平行執行
- 單一故事的測試任務可平行執行
- 不同故事可由不同開發者平行處理，但不得修改同一檔案造成衝突

---

## Parallel Example: User Story 1

```text
Task: "撰寫金額規則單元測試於 BookKeeping2.Tests/Unit/[Feature]Tests.cs"
Task: "撰寫 Razor Page 整合測試於 BookKeeping2.Tests/Integration/[Feature]Tests.cs"
Task: "建立模型於 BookKeeping2/Models/[Entity].cs"
```

---

## Implementation Strategy

### MVP First（僅 User Story 1）

1. 完成 Phase 1: Setup
2. 完成 Phase 2: Foundational
3. 完成 Phase 3: User Story 1
4. 停下並驗證 User Story 1 的獨立測試、資料完整性與 UX
5. 若驗證通過，再展示或部署

### Incremental Delivery

1. 完成 Setup 與 Foundational
2. 加入 User Story 1，測試並展示 MVP
3. 加入 User Story 2，測試並展示增量
4. 加入 User Story 3，測試並展示增量
5. 每個故事都不得破壞先前故事

### Parallel Team Strategy

1. 團隊共同完成 Setup 與 Foundational
2. Foundational 完成後，依使用者故事分派不同檔案範圍
3. 每個故事完成時各自通過測試，再整合驗證

---

## Notes

- [P] 任務代表不同檔案且無相依
- [Story] 標籤用於追蹤任務與使用者故事
- 測試失敗原因 MUST 對應尚未實作的需求
- 每個任務描述 MUST 使用精確路徑
- 避免含糊任務、同檔案衝突、破壞故事獨立性的跨故事相依
