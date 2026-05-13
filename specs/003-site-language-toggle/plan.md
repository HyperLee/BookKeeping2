# Implementation Plan: 網站介面英文語系切換

**Branch**: `003-site-language-toggle` | **Date**: 2026-05-13 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-site-language-toggle/spec.md`

## Summary

在首頁新增介面語言選擇，讓使用者可在預設繁體中文與手動選擇的 English 之間切換。後續所有可直接瀏覽的 Razor Pages 由伺服器依第一方必要 Cookie 輸出對應語言的可翻譯介面文字，且不依瀏覽器或作業系統語言自動改成英文。技術上使用 ASP.NET Core localization middleware、共用 `.resx` 資源、DataAnnotations display localization、首頁非財務 POST handler 與 allow-list Cookie；使用者面向錯誤、驗證與可執行修正提示依憲章維持繁體中文。語言切換只影響 UI 文字，不新增 SQLite schema、不改 CSV 檔案契約、不改使用者原始資料與財務計算。

## Technical Context

**Language/Version**: C# 14 / .NET 10 / ASP.NET Core 10.0；前端僅在首頁語言控制項需要時使用瀏覽器原生 JavaScript 作為漸進增強  
**Primary Dependencies**: Razor Pages、ASP.NET Core localization middleware、`IStringLocalizer<SharedResource>`/ResourceManager `.resx`、DataAnnotations display localization、Bootstrap 5.3.3、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0 SQLite provider、Serilog、HtmlSanitizer、CsvHelper
**Storage**: 現有財務資料維持 SQLite + EF Core `AppDbContext`、`InitialCreate` migration、唯一索引、外鍵與交易控制；本功能不新增資料庫 schema。介面語言偏好僅以伺服器可讀第一方必要 Cookie `bookkeeping.ui.language` 保存 `zh-TW` 或 `en`，期限 1 年，SameSite=Lax，HTTPS 連線設定 Secure，不寫入 SQLite、Session、localStorage、稽核或伺服器日誌。  
**Testing**: xUnit + Moq 單元測試、`WebApplicationFactory` Razor Pages 整合測試、coverlet coverage；語言切換、Cookie allow-list、忽略 Accept-Language、資源完整性、DataAnnotations display text、繁體中文驗證/錯誤訊息保留、CSV 契約保留、資料不變性、首頁控制項唯一性、1 秒內呈現所選語言、響應式與鍵盤操作需補充 Playwright 真實瀏覽器測試，並以 axe-core 或等效 DOM/可及性斷言驗證主要頁面。
**Target Platform**: 桌面瀏覽器 Chrome、Edge、Firefox、Safari 與行動裝置瀏覽器  
**Project Type**: Web，單一 ASP.NET Core Razor Pages 專案 + `BookKeeping2.Tests` 測試專案  
**Performance Goals**: 沒有有效 Cookie 的首次請求必須直接輸出繁體中文；有有效 Cookie 的頁面請求必須由伺服器在回應中輸出所選語言；首頁切換後與後續站內頁面在 1 秒內呈現所選語言；無效 Cookie 或瀏覽器能力受限時不得產生 script error，並安全回到繁體中文。  
**Constraints**: 語言控制項只出現在首頁；預設一律繁體中文且不得依瀏覽器語言自動切換英文；使用者手動選擇英文後才顯示英文可翻譯 UI；使用者面向錯誤、驗證與可執行修正提示維持繁體中文；符合 WCAG 2.2 AA 對比與可見焦點指示；語言切換不得提交財務表單、修改資料、刪除資料、重新計算財務結果、改變 CSV 匯入匯出格式，且不得翻譯使用者自行輸入或匯入的資料原文。
**Scale/Scope**: 語言、響應式與可及性驗證範圍為所有目前可由使用者直接瀏覽的 Razor Pages：首頁、隱私權頁、錯誤頁、帳戶、分類、預算、交易清單、新增交易、編輯交易、刪除交易、CSV 匯入、CSV 匯出與報表；shared partial、validation partial、pagination partial 與 transaction form partial 透過其宿主頁面驗證。

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

- **I. 程式碼品質至上**: PASS。採用 ASP.NET Core 既有 localization patterns，新增 helper/service 時需維持檔案範圍 namespace、nullable-safe code、公開 API XML 文件註解與既有命名風格。
- **II. 測試優先開發**: PASS。後續 tasks 必須先取得使用者或維護者對測試意圖的確認，再新增失敗測試，涵蓋首頁控制項、Cookie 行為、所有主要頁面的英文輸出、無效 Cookie fallback、忽略 Accept-Language、DataAnnotations display、繁體中文驗證/錯誤訊息保留、CSV 契約、使用者資料原文與財務結果保留，再實作語言切換。
- **III. 使用者體驗一致性**: PASS。控制項使用 Bootstrap 表單元件與 fieldset/legend 或等效語意結構，只在首頁出現；繁中與英文文案都需在手機、平板與桌面不重疊、可鍵盤操作、焦點清楚；使用者面向錯誤與可執行修正訊息依憲章維持繁體中文。
- **IV. 效能與延展性**: PASS。語言由伺服器在 request pipeline 中解析，避免全站 client-side text replacement；資源檔由 ResourceManager 快取，不增加資料庫或網路 round-trip。
- **V. 可觀察性與監控**: PASS。本功能不新增財務事件與稽核紀錄；不得記錄語言偏好值。既有 Serilog/ILogger 保持不輸出敏感財務資料。
- **VI. 安全優先**: PASS。Cookie 採 allow-list 驗證、SameSite=Lax、HTTPS Secure、HttpOnly 與必要 Cookie；首頁語言設定使用非財務 POST handler 與 anti-forgery，不用 GET 改狀態，不輸出 raw HTML。
- **VII. 資料完整性**: PASS。無 SQLite schema、migration、財務模型或 CSV 檔案格式變更；語言切換不得改變交易、分類、帳戶、預算、匯入批次、報表結果、金額、日期或計算規則。

**Gate Result**: PASS，無未解決釐清事項，無需 Complexity Tracking 豁免。

## Project Structure

### Documentation (this feature)

```text
specs/003-site-language-toggle/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── language-ui-contract.md
└── tasks.md              # Phase 2 output from /speckit-tasks, not created here
```

### Source Code (repository root)

```text
BookKeeping2/
├── Program.cs                         # localization service/middleware registration
├── Localization/
│   ├── SharedResource.cs              # shared resource marker for UI strings
│   ├── UiLanguageOptions.cs           # supported language/cookie constants
│   └── UiLanguageRequestCultureProvider.cs
├── Resources/
│   └── SharedResource.en.resx         # English translations for existing zh-TW UI literals
├── Pages/
│   ├── Index.cshtml                   # homepage language control plus existing theme control
│   ├── Index.cshtml.cs                # non-financial language POST handler
│   ├── Shared/
│   │   ├── _Layout.cshtml             # localized nav/footer/title and html lang
│   │   └── _Pagination.cshtml         # localized pagination text
│   ├── Transactions/
│   │   └── _TransactionForm.cshtml    # localized shared transaction form labels/options
│   └── [Accounts|Budgets|Categories|Csv|Reports|Transactions]/
│       └── *.cshtml, *.cshtml.cs      # localized fixed UI strings and status messages
├── ViewModels/
│   └── **/*.cs                        # DataAnnotations display text localized; validation/error text remains zh-TW
├── Services/
│   └── **/*.cs                        # non-error service result messages localized at display boundary or via shared localizer
└── wwwroot/
    ├── css/site.css                   # responsive/focus fixes for longer English text if needed
    └── js/site.js                     # only homepage progressive enhancement if needed; no global text replacement

BookKeeping2.Tests/
├── Unit/
│   └── Localization/
│       └── UiLanguageRequestCultureProviderTests.cs
├── Integration/
│   ├── Pages/
│   │   └── LanguageTogglePageTests.cs
│   ├── StaticAssets/
│   │   └── LanguageResourceCompletenessTests.cs
│   └── Browser/
│       └── LanguageToggleBrowserTests.cs
└── TestSupport/
    └── LanguageToggleBrowserFixture.cs # may reuse ThemeModeBrowserFixture pattern if practical
```

**Structure Decision**: 採用既有單一 ASP.NET Core Razor Pages 專案與 `BookKeeping2.Tests` 測試專案。語言切換是站台層 UI localization feature，不建立新資料庫表、不新增廣泛 repository abstraction，也不以 client-side 全站替換文字。共用文字以 `SharedResource` 聚合，讓 layout、Razor Pages、PageModel、DataAnnotations display text 與使用者可見非錯誤服務訊息可逐步接入同一資源來源；使用者面向錯誤、驗證與可執行修正提示維持繁體中文，使用者自行輸入的帳戶、分類、備註與 CSV 原始內容維持原文。

## Phase 0 Research

Phase 0 completed in [research.md](./research.md). 所有技術未知項目已以規格、既有專案結構、使用者提供的技術上下文與 ASP.NET Core 官方 localization 指引解析，沒有待釐清標記。

## Phase 1 Design

Phase 1 design artifacts:

- [data-model.md](./data-model.md)
- [contracts/language-ui-contract.md](./contracts/language-ui-contract.md)
- [quickstart.md](./quickstart.md)

### Post-Design Constitution Check

- **I. 程式碼品質至上**: PASS。設計使用框架 localization middleware、資源檔與既有 Razor Pages 結構，未引入不必要的跨層抽象。
- **II. 測試優先開發**: PASS。quickstart 與 tasks 明確列出需先取得測試意圖確認、再建立失敗測試與手動驗證，涵蓋三個使用者故事與主要邊界情況。
- **III. 使用者體驗一致性**: PASS。UI contract 限制控制項位置、目前選取狀態、鍵盤操作、`html lang`、長英文文字、繁體中文錯誤/驗證訊息保留與 responsive 驗證。
- **IV. 效能與延展性**: PASS。語言解析在 request pipeline 完成，資源 lookup 使用 ResourceManager，避免全站 JavaScript 掃描或資料庫查詢。
- **V. 可觀察性與監控**: PASS。無新增遙測或稽核資料；語言 Cookie 不記錄到日誌。
- **VI. 安全優先**: PASS。Cookie allow-list、anti-forgery POST、HttpOnly/SameSite/Secure、Razor encoding 與不本地化 HTML 的限制均寫入 contract。
- **VII. 資料完整性**: PASS。data-model 明確標示語言偏好不進 SQLite，CSV 檔案欄位與交易類型固定繁中，使用者資料原文不被翻譯。

**Post-Design Gate Result**: PASS。

## Complexity Tracking

無憲章違規或需豁免的複雜度增加。
