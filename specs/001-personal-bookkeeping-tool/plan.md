# 實作計畫: Open BookKeeping - 開源個人記帳理財工具

**分支**: `001-personal-bookkeeping-tool` | **日期**: 2026-05-11 | **規格**: [spec.md](./spec.md)  
**輸入**: `/specs/001-personal-bookkeeping-tool/spec.md` 的功能規格，並採用 `markdownFolder/tempPlan.md` 的技術棧大綱

**說明**: 此計畫由 `/speckit-plan` 產生。所有使用者面向文件與 UI 文字使用繁體中文 zh-TW。

## 摘要

本功能將現有 ASP.NET Core Razor Pages starter 擴充為單使用者部署的個人記帳工具。V1 固定使用 TWD 與 Asia/Taipei 本地日期，提供交易 CRUD、資料持久化、分類與帳戶管理、月報圖表、預算追蹤、CSV 匯入匯出、搜尋篩選、首頁摘要與稽核摘要。

技術方法採單一 Razor Pages Web 專案加獨立 xUnit 測試專案。應用層分為 Razor Pages、ViewModels/InputModels、Services、Data 與 Models；持久化使用 EF Core 10 + SQLite；金額在領域與服務 API 一律使用 `decimal`，SQLite 儲存採縮放整數 minor units 以避免 SQLite 對 `decimal` 比較/排序的限制；CSV 採 RFC 4180 相容格式並防範公式注入；關鍵財務異動以 Serilog/`ILogger` 產生遮罩稽核摘要。

## 技術上下文

**語言與版本**: C# 14 / .NET 10 / ASP.NET Core 10.0  
**主要相依**: Razor Pages、Bootstrap 5、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0 SQLite provider、Serilog、HtmlSanitizer、CsvHelper  
**儲存**: SQLite，透過 EF Core `DbContext`、Migrations、唯一索引、外鍵與交易控制；資料檔路徑由設定管理，避免提交實際資料庫  
**測試**: xUnit + Moq 單元測試、`WebApplicationFactory` 整合測試；必要時補充 Playwright 或等效 UI 驗證  
**目標平台**: 桌面瀏覽器 Chrome、Edge、Firefox、Safari 與行動裝置瀏覽器  
**專案型態**: Web，單一 ASP.NET Core Razor Pages 專案 + `BookKeeping2.Tests` 測試專案  
**效能目標**: FCP < 1.5 秒、LCP < 2.5 秒；10,000 筆交易篩選 < 2 秒；10,000 筆紀錄查詢 < 3 秒；100 筆月報產生 < 2 秒；1,000 筆 CSV 匯出 < 5 秒；100 筆 CSV 匯入 < 10 秒  
**限制**: CSV 匯入 < 5 MB / 10,000 有效資料列；金額 API 一律使用 `decimal`；V1 單一幣別 TWD；實際交易日期不得晚於 Asia/Taipei 今日；狀態變更表單使用 Anti-Forgery Token；使用者面向文字為繁體中文；生產環境 HTTPS/HSTS；未授權第三方傳輸為 0  
**規模與範圍**: 單人使用、約 8 個主要頁面、年累積 600-2,400 筆紀錄、極端情境 10,000+ 筆交易；V1 不包含站內帳號、角色、多帳本、多幣別或帳戶間轉帳

## Constitution Check

*GATE: Phase 0 research 前通過；Phase 1 design 後已重新檢查。*

| 原則 | 狀態 | 設計承諾 |
|------|------|----------|
| 程式碼品質 | 通過 | 使用 C# 14、Nullable Reference Types、檔案範圍命名空間、`.editorconfig`、清楚分層與必要 XML 文件註解；公開服務與複雜金融計算 API 需含範例。 |
| 測試優先 | 通過 | 建立 `BookKeeping2.Tests` 後先撰寫失敗測試；交易、分類、帳戶、餘額、報表、預算、CSV、日期與安全驗證均有單元/整合測試策略。 |
| 資料完整性 | 通過 | 領域與服務金額使用 `decimal`；SQLite 儲存用 minor units；交易、匯入與跨資料表異動包在 EF Core transaction；交易刪除採軟刪除與稽核摘要。 |
| 安全優先 | 通過 | 伺服器端驗證為權威；Razor 預設編碼；POST 表單使用 antiforgery；CSV/備註經 HtmlSanitizer 或拒絕策略；生產 HTTPS/HSTS；規劃 CSP header。 |
| 使用者體驗 | 通過 | Bootstrap 5、既有 `site.css`、繁體中文欄位層級錯誤、toast/alert 回饋、鍵盤操作與 320px 手機寬度無重疊。 |
| 效能與延展性 | 通過 | 交易列表分頁、索引、async EF 查詢、預先彙總查詢、Chart.js 輕量資料、匯入串流處理與靜態資源管線。 |
| 可觀察性與稽核 | 通過 | Serilog/`ILogger` 結構化事件；新增/編輯/刪除、匯入/匯出、預算警告與寫入失敗記錄遮罩摘要，不記錄完整備註或敏感明細。 |
| 文件語言 | 通過 | `spec.md`、`plan.md`、`research.md`、`data-model.md`、`quickstart.md`、`contracts/*` 使用繁體中文 zh-TW。 |

### Post-Design Constitution Check

Phase 1 設計後重新檢查結果仍為通過。唯一需在 tasks 階段明確安排的是建立測試專案、補齊 CSP/安全 header、以及處理 constitution 私有欄位 camelCase 與 `.editorconfig` `_camelCase` 的既有不一致；實作不得在未決定前進行大規模私有欄位命名翻修。

## 專案結構

### 文件（本功能）

```text
specs/001-personal-bookkeeping-tool/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── csv-format.md
│   └── ui-pages.md
└── tasks.md              # Phase 2 由 /speckit-tasks 建立
```

### 原始碼（repository root）

```text
BookKeeping2/
├── Data/
│   ├── AppDbContext.cs
│   ├── EntityConfigurations/
│   ├── Migrations/
│   └── SeedData/
├── Models/
│   ├── Accounts/
│   ├── Budgets/
│   ├── Categories/
│   ├── CsvImports/
│   ├── Transactions/
│   └── Audit/
├── Services/
│   ├── Accounts/
│   ├── Budgets/
│   ├── Categories/
│   ├── Csv/
│   ├── Reports/
│   ├── Transactions/
│   └── Time/
├── ViewModels/
├── Validation/
├── Pages/
│   ├── Accounts/
│   ├── Budgets/
│   ├── Categories/
│   ├── Csv/
│   ├── Reports/
│   ├── Transactions/
│   ├── Shared/
│   └── Index.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── Program.cs
└── appsettings*.json

BookKeeping2.Tests/
├── Unit/
│   ├── Accounts/
│   ├── Budgets/
│   ├── Categories/
│   ├── Csv/
│   ├── Reports/
│   └── Transactions/
├── Integration/
│   ├── Pages/
│   └── Persistence/
└── TestSupport/
```

**結構決策**: Razor Page 的 `PageModel` 僅負責 HTTP input/output、ModelState 與導覽；交易、帳戶、分類、報表、預算與 CSV 規則放在注入服務中；EF Core 實體設定集中於 `Data/EntityConfigurations`；測試以服務單元測試覆蓋規則，以 `WebApplicationFactory` 覆蓋頁面渲染、表單 POST、驗證與 SQLite 持久化流程。

## Phase 0: Research 結果

輸出: [research.md](./research.md)

已解析決策包含 Razor Pages 架構、EF Core SQLite、金額儲存策略、Asia/Taipei 日期邊界、交易原子性、軟刪除與稽核、CSV 格式與公式注入防護、Chart.js 報表、測試分層、日誌遮罩與部署安全；沒有未解的澄清項目。

## Phase 1: Design & Contracts 結果

輸出:

- [data-model.md](./data-model.md)
- [contracts/ui-pages.md](./contracts/ui-pages.md)
- [contracts/csv-format.md](./contracts/csv-format.md)
- [quickstart.md](./quickstart.md)

設計涵蓋交易、分類、帳戶、預算、CSV 匯入批次/錯誤、稽核事件與應用設定；契約涵蓋主要 Razor Pages 路由、表單欄位、驗證回饋、CSV 欄位格式、匯出安全處理與匯入錯誤摘要。

## 複雜度追蹤

| 違反項目 | 必要原因 | 拒絕較簡方案的原因 |
|----------|----------|--------------------|
| 無憲章違反 | 無 | 無 |

## Phase 2 停止點

本 `/speckit-plan` 執行到規劃輸出為止，不建立 `tasks.md`。下一步應執行 `/speckit-tasks` 產生依賴排序的 TDD 任務，且第一批任務必須先建立 `BookKeeping2.Tests` 與測試基礎設施。
