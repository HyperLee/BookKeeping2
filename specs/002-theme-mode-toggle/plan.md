# Implementation Plan: 網站主題模式切換

**Branch**: `002-theme-mode-toggle` | **Date**: 2026-05-12 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-theme-mode-toggle/spec.md`

## Summary

在首頁新增「亮色模式」、「暗黑模式」、「跟隨系統」三選一控制項，並讓整個 Razor Pages 網站在首次繪製前、頁面切換後、系統偏好變更後與同站分頁同步時都套用一致的有效主題。技術上以 Bootstrap 5.3 的 `data-bs-theme` 作為主題切換入口，使用瀏覽器 `localStorage` 保存同一瀏覽器與裝置上的選取模式，並以 `matchMedia('(prefers-color-scheme: dark)')` 與 `storage` event 推導及同步有效主題。

## Technical Context

**Language/Version**: C# 14 / .NET 10 / ASP.NET Core 10.0；前端使用瀏覽器原生 JavaScript  
**Primary Dependencies**: Razor Pages、Bootstrap 5.3.3、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0 SQLite provider、Serilog、HtmlSanitizer、CsvHelper  
**Storage**: 現有財務資料使用 SQLite + EF Core `AppDbContext`、`InitialCreate` migration、唯一索引、外鍵與交易控制；本功能不新增資料庫 schema，主題偏好僅以瀏覽器 `localStorage` 保存 `light`、`dark` 或 `system`  
**Testing**: xUnit + Moq 單元測試、`WebApplicationFactory` 整合測試、coverlet coverage；主題互動、跨分頁同步、系統偏好、首次繪製前主題與可及性驗證必須補充 Playwright、axe-core 或等效真實瀏覽器工具
**Target Platform**: 桌面瀏覽器 Chrome、Edge、Firefox、Safari 與行動裝置瀏覽器  
**Project Type**: Web，單一 ASP.NET Core Razor Pages 專案 + `BookKeeping2.Tests` 測試專案  
**Performance Goals**: 在目標瀏覽器環境中於首次繪製前套用有效主題；首頁切換與站內後續頁面在 1 秒內呈現一致有效主題；同站已開啟分頁與系統偏好變更在 2 秒內同步；瀏覽器能力被封鎖時不得產生 script error 且需儘早套用安全 fallback
**Constraints**: 主題控制項只出現在首頁；使用者面向文字使用繁體中文；符合 WCAG 2.2 AA 對比與可見焦點指示；主題切換不得修改、刪除或重新計算任何財務資料；不得保存有效主題或切換歷史，只保存選取模式  
**Scale/Scope**: 主題、響應式與可及性驗證範圍為所有目前可由使用者直接瀏覽的 Razor Pages：首頁、隱私權頁、錯誤頁、帳戶、分類、預算、交易清單、新增交易、編輯交易、刪除交易、CSV 匯入、CSV 匯出與報表；shared partial 與 validation partial 只透過其宿主頁面驗證。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

- **I. 程式碼品質至上**: PASS。變更範圍限制在 `_Layout.cshtml`、`Index.cshtml`、`site.css`、`site.js` 與對應測試；若新增公開 helper 或 service，必須補 XML 文件註解。
- **II. 測試優先開發**: PASS。先新增失敗測試確認首頁控制項、所有其他使用者可瀏覽頁不顯示控制項、layout 主題初始化 hook、靜態資源引用、瀏覽器端主題行為與可及性結構；每個故事進入實作前必須由使用者或維護者確認測試意圖與預期失敗原因。
- **III. 使用者體驗一致性**: PASS。使用 Bootstrap 5.3 主題機制與現有 `site.css`，控制項文字為繁體中文，所有目前可由使用者直接瀏覽的 Razor Pages 的焦點狀態、響應式版面與 WCAG 2.2 AA 對比列入驗證。
- **IV. 效能與延展性**: PASS。首次繪製前的主題推導必須是小型同步 head script；互動邏輯放在 `site.js`，避免額外伺服器 request 與資料庫存取。
- **V. 可觀察性與監控**: PASS。本功能不處理伺服器端財務事件；不記錄主題偏好與財務資料。若未來加遙測，必須只記錄匿名 UI 事件且不得包含財務內容。
- **VI. 安全優先**: PASS。主題偏好僅為 allow-list 字串；不輸出使用者 HTML；不新增狀態改變 HTTP form。首次繪製前 inline script 使用現有 CSP 能力，內容需保持最小化且不讀取敏感資料。
- **VII. 資料完整性**: PASS。無資料庫 schema 或財務模型變更；測試與手動驗證需確認主題切換不提交表單、不重算金額、不修改交易、分類、帳戶、預算或報表資料。

**Gate Result**: PASS，無未解決釐清事項，無需 Complexity Tracking 豁免。

## Project Structure

### Documentation (this feature)

```text
specs/002-theme-mode-toggle/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── theme-ui-contract.md
└── tasks.md              # Phase 2 output from /speckit-tasks, not created here
```

### Source Code (repository root)

```text
BookKeeping2/
├── Pages/
│   ├── Index.cshtml                  # 首頁主題模式控制項
│   └── Shared/
│       └── _Layout.cshtml            # 首次繪製前主題初始化與共用資源引用
└── wwwroot/
    ├── css/
    │   └── site.css                  # 主題色彩、對比、焦點與響應式調整
    └── js/
        └── site.js                   # 主題偏好保存、推導、同步與首頁控制項行為

BookKeeping2.Tests/
├── Integration/
│   ├── Pages/
│   │   └── ThemeModePageTests.cs         # Razor Pages markup/layout contract tests
│   ├── StaticAssets/
│   │   └── ThemeModeScriptContractTests.cs
│   └── Browser/
│       └── ThemeModeBrowserTests.cs      # Playwright/axe or equivalent browser behavior tests
└── TestSupport/
    └── ThemeModeBrowserFixture.cs
```

**Structure Decision**: 採用既有單一 ASP.NET Core Razor Pages 專案與 `BookKeeping2.Tests` 測試專案。主題是站台層 UI 行為，不建立新的 domain service 或資料庫結構；真實瀏覽器行為以 Playwright/axe-core 或等效工具放在測試專案的 `Integration/Browser/` 與 `TestSupport/` 中，避免把瀏覽器驗證混入產品程式碼。

## Phase 0 Research

Phase 0 completed in [research.md](./research.md). 所有技術未知項目已以規格、既有專案結構與使用者提供的技術上下文解析，沒有待釐清標記。

## Phase 1 Design

Phase 1 design artifacts:

- [data-model.md](./data-model.md)
- [contracts/theme-ui-contract.md](./contracts/theme-ui-contract.md)
- [quickstart.md](./quickstart.md)

### Post-Design Constitution Check

- **I. 程式碼品質至上**: PASS。設計未新增不必要抽象，使用現有 Razor Pages、Bootstrap 與 static assets。
- **II. 測試優先開發**: PASS。quickstart 與後續 tasks 必須先落實失敗測試、確認測試意圖與預期失敗原因，再實作主題行為。
- **III. 使用者體驗一致性**: PASS。UI contract 明確限制控制項位置、鍵盤操作、目前狀態標示、跨頁一致性與可及性。
- **IV. 效能與延展性**: PASS。無伺服器 round-trip；首次繪製前 script 與 runtime script 的責任已分離。
- **V. 可觀察性與監控**: PASS。無新增日誌需求，避免不必要紀錄使用者偏好。
- **VI. 安全優先**: PASS。localStorage 值採 allow-list 驗證，無 raw HTML，無新增 CSRF surface。
- **VII. 資料完整性**: PASS。資料模型明確標示主題偏好不進 SQLite，且不得影響財務資料。

**Post-Design Gate Result**: PASS。

## Complexity Tracking

無憲章違規或需豁免的複雜度增加。
