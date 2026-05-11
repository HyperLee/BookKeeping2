<!--
Sync Impact Report
Version change: unratified template → 1.0.0
Modified principles:
- Template placeholders -> I. 程式碼品質至上
- Template placeholders -> II. 測試優先開發
- Template placeholders -> III. 使用者體驗一致性
- Template placeholders -> IV. 效能與延展性
- Template placeholders -> V. 可觀察性與監控
- Template placeholders -> VI. 安全優先
- Template placeholders -> VII. 資料完整性
Added sections:
- 技術標準
- 開發工作流程
- 治理規則
Removed sections:
- Generic placeholder sections from the initial Spec Kit constitution template
Templates requiring updates:
- ✅ updated: .specify/templates/plan-template.md
- ✅ updated: .specify/templates/spec-template.md
- ✅ updated: .specify/templates/tasks-template.md
- ✅ updated: docs/readme-template.md
- ✅ reviewed: .specify/templates/commands/*.md was not present
Follow-up TODOs: None
-->
# BookKeeping 記帳系統憲章

## 核心原則

### I. 程式碼品質至上 (NON-NEGOTIABLE)

所有程式碼 MUST 符合以下品質標準：

- **可維護性**: 程式碼 MUST 清晰、可讀，重要設計決策 MUST 文件化。
- **C# 最佳實踐**: MUST 使用 C# 14 可用的語言特性、檔案範圍命名空間、
  模式匹配與 Nullable Reference Types；null 判斷 MUST 使用 `is null` 或
  `is not null`。
- **命名規範**: 公開成員、型別與方法 MUST 使用 PascalCase；私有欄位 MUST
  使用 camelCase；介面名稱 MUST 以 `I` 開頭。
- **XML 文件註解**: 所有公開 API MUST 具備 XML 文件註解；具複雜行為或金融
  計算的 API MUST 包含 `<example>` 與 `<code>` 區段。
- **錯誤處理**: MUST 明確處理邊界情況，例外訊息 MUST 能協助定位錯誤來源，
  且不得洩露敏感資料。
- **程式碼格式化**: MUST 遵循 `.editorconfig` 定義的格式規範。

**理由**: 記帳系統處理金融資料，程式碼品質直接影響資料正確性、維護成本與
長期可靠性。品質要求屬於交付條件，不得以開發速度作為豁免理由。

### II. 測試優先開發 (NON-NEGOTIABLE)

所有功能與修正 MUST 遵循測試優先開發流程：

- **紅-綠-重構週期**: MUST 先撰寫測試，取得使用者或維護者對測試意圖的確認，
  觀察測試失敗，再實作功能、通過測試並進行重構。
- **關鍵路徑測試**: 金額計算、帳目分類、收支統計、報表產生、交易記錄與
  資料匯入匯出 MUST 有單元測試覆蓋。
- **整合測試**: Razor Pages 頁面渲染、表單處理、驗證流程、資料存取層與
  重要使用者流程 MUST 有整合測試。
- **測試命名**: 測試 MUST 遵循既有檔案命名風格與大小寫規範；測試內容不得依賴
  `Arrange`、`Act`、`Assert` 註解來補足可讀性。
- **相依性隔離**: 外部服務、時間、檔案系統與資料存取 SHOULD 使用 mock、
  fake 或測試替身隔離，避免非必要的非決定性。
- **獨立可測試**: 每個使用者故事 MUST 可獨立測試，並作為可交付的 MVP 增量。
- **財務精確度測試**: 涉及金額的運算 MUST 使用 `decimal`，測試 MUST 驗證
  捨入規則、精確度與邊界金額。

**理由**: 記帳系統的計算錯誤會破壞使用者信任並造成財務風險。測試先行讓需求、
計算規則與回歸風險在實作前被明確化。

### III. 使用者體驗一致性

所有使用者面向功能 MUST 維持一致且可操作的體驗：

- **UI/UX 標準化**: MUST 使用一致的設計語言、Bootstrap 5 元件與
  `wwwroot/css/site.css` 中的自訂樣式規範。
- **回應式設計**: 所有頁面 MUST 在手機、平板與桌面寬度下保持可讀、可操作，
  且不得出現內容重疊。
- **錯誤訊息**: 使用者面向錯誤訊息 MUST 使用繁體中文，並說明可執行的修正方式。
- **驗證回饋**: 表單 MUST 提供即時或提交後的清楚驗證回饋；Razor Pages 表單
  SHOULD 使用 Data Annotations 搭配 jQuery Validation。
- **無障礙設計**: 使用者介面 MUST 滿足 WCAG 2.1 AA 的核心可及性要求，
  包含鍵盤操作、語意標記與對比度。
- **使用者故事優先級**: 規格 MUST 以業務價值排序使用者故事，使用 P1、P2、
  P3 等優先級表示。
- **記帳體驗**: 記帳流程 MUST 簡潔直覺，常用分類與金額輸入 MUST 減少不必要
  步驟並防止誤輸入。

**理由**: 記帳工具的價值取決於日常使用是否低摩擦。一致的互動、明確的驗證與
可及性要求能降低放棄使用與錯誤輸入的機率。

### IV. 效能與延展性

系統 MUST 在功能設計與實作時保留效能與成長空間：

- **頁面載入時間**: 主要頁面 SHOULD 達成首次內容繪製 FCP 小於 1.5 秒，
  最大內容繪製 LCP 小於 2.5 秒；未達標時 MUST 在計畫中記錄原因與改善路徑。
- **靜態資源最佳化**: CSS、JavaScript 與圖片 MUST 維持合理大小；第三方資源
  MUST 透過 `wwwroot/lib/` 或受控 CDN 管理。
- **記憶體管理**: 使用檔案、串流、資料庫連線或其他非受控資源時 MUST 正確
  實作釋放流程，必要時使用 `IDisposable` 或 `await using`。
- **非同步程式設計**: I/O 密集操作 MUST 使用 async/await，且不得阻塞 UI 或
  ASP.NET Core request thread。
- **快取策略**: 靜態或低變化率資產 SHOULD 使用 ASP.NET Core Static Assets
  管線與適當快取策略。
- **效能監控**: 重要功能 SHOULD 使用 Application Insights 或等效工具追蹤
  延遲、錯誤率與資源用量。
- **報表產生效率**: 帳目查詢與統計報表 MUST 在合理時間內完成；大量交易彙總
  MUST 避免阻塞互動流程。

**理由**: 當交易資料累積後，查詢、統計與報表效能會直接影響可用性。早期建立
效能邊界可降低後續重工成本。

### V. 可觀察性與監控

系統 MUST 提供可診斷、可稽核的執行資訊：

- **結構化日誌**: MUST 使用 `ILogger`、Serilog 或等效提供者記錄結構化日誌。
- **日誌層級**: MUST 正確使用 Trace、Debug、Information、Warning、Error、
  Critical，且不得以高層級日誌掩蓋一般流程。
- **遙測收集**: SHOULD 依部署規模整合 Application Insights 或等效工具，收集
  關鍵使用流程與效能指標。
- **關鍵事件記錄**: 安全事件、登入相關事件與帳目異動操作 MUST 被記錄。
- **錯誤追蹤**: 關鍵錯誤 MUST 可被追蹤；生產環境 SHOULD 對高嚴重度錯誤設定告警。
- **財務操作稽核**: 帳目新增、修改、刪除與匯出 MUST 保留稽核軌跡，至少包含
  操作者、時間、操作類型與變更摘要；敏感金額與個資不得不必要地明文寫入日誌。

**理由**: 財務資料系統需要可追溯性。可觀察性讓問題診斷、使用者支援與合規稽核
有足夠證據，不依賴猜測。

### VI. 安全優先

安全性 MUST 內建於每個功能與部署流程：

- **輸入驗證**: 所有使用者輸入 MUST 伺服器端驗證；用戶端驗證只能作為輔助。
- **XSS 防護**: MUST 使用 Razor 預設 HTML 編碼；任何未編碼輸出 MUST 有明確
  審查理由。
- **CSRF 防護**: 所有狀態改變表單 MUST 使用 Anti-Forgery Token。
- **敏感資料保護**: 金鑰、連線字串與憑證 MUST 使用 Secret Manager、環境變數
  或受控祕密管理服務，不得提交到版本控制。
- **HTTPS Only**: 生產環境 MUST 強制 HTTPS 並啟用 HSTS。
- **Content Security Policy**: 生產環境 SHOULD 設定 CSP 標頭；若暫不設定，
  MUST 在計畫中記錄風險與補齊時程。
- **金融資料防護**: 帳目金額、財務摘要與個人財務資訊 MUST 按敏感資料處理，
  不得在日誌、錯誤頁或遙測中不必要地明文暴露。

**理由**: 記帳系統保存個人財務資訊。安全控制必須在設計階段成為預設條件，
不能依靠事後補強。

### VII. 資料完整性 (NON-NEGOTIABLE)

記帳系統 MUST 以財務資料正確性與完整性作為核心交付標準：

- **金額精確度**: 所有貨幣與金額運算 MUST 使用 `decimal`；`float` 與 `double`
  禁止用於貨幣計算。
- **交易原子性**: 涉及多筆帳目或跨資料表一致性的操作 MUST 在同一交易中完成，
  並確保全部成功或全部回滾。
- **資料驗證**: 帳目金額 MUST 為正數；分類 MUST 存在；日期 MUST 合理，除預算
  或排程功能外不得建立未來實際交易。
- **收支完整性**: 系統 MUST 能驗證收支記錄完整性，並在需要時提供對帳或一致性
  檢查能力。
- **備份與復原**: 系統 MUST 提供資料匯出機制，讓使用者能備份自己的帳目資料。
- **刪除保護**: 帳目刪除 SHOULD 採用軟刪除或可追溯策略；若採硬刪除，MUST 在
  規格與風險評估中說明理由。

**理由**: 使用者信任建立在財務資料可信與可復原之上。資料完整性問題會直接破壞
系統價值，且修復成本高。

## 技術標準

### 技術堆疊

- **Framework**: ASP.NET Core 10.0
- **語言**: C# 14，啟用 Nullable Reference Types 與 implicit usings
- **前端**: Razor Pages、Bootstrap 5、jQuery、jQuery Validation
- **靜態資源**: `MapStaticAssets` 與 `WithStaticAssets` 管線
- **日誌**: `ILogger`、Serilog 或相容的結構化日誌提供者
- **測試**: xUnit、Moq、WebApplicationFactory；必要時補充 Playwright 或等效 UI 測試

### 專案結構

- **關注點分離**: Pages、Models、Services 與資料存取 MUST 保持清楚分層。
- **Razor Pages 慣例**: 頁面 MUST 放置於 `BookKeeping2/Pages/`，並遵循 ASP.NET
  Core Razor Pages 慣例。
- **共用佈局**: 站台佈局 MUST 透過 `Pages/Shared/_Layout.cshtml` 與
  `_ViewStart.cshtml` 統一。
- **設定管理**: MUST 使用 `appsettings.json` 搭配環境特定設定檔；敏感設定不得
  寫入這些檔案。
- **相依性注入**: 應用服務 MUST 透過 ASP.NET Core 內建 DI 容器管理生命週期。
- **靜態檔案組織**: CSS、JavaScript 與第三方函式庫 MUST 分別放置於
  `wwwroot/css/`、`wwwroot/js/` 與 `wwwroot/lib/`。

### 資源管理

- **CSS**: 共用樣式 MUST 放置於 `wwwroot/css/site.css`；頁面專屬樣式 SHOULD
  使用 CSS Isolation。
- **JavaScript**: 共用指令碼 MUST 放置於 `wwwroot/js/site.js`；頁面專屬指令碼
  MUST 清楚限定作用範圍。
- **第三方函式庫**: Bootstrap、jQuery、jQuery Validation 等前端函式庫 MUST
  透過 `wwwroot/lib/` 或明確版本鎖定的套件來源管理。

## 開發工作流程

### 文件語言要求

所有使用者面向文件 MUST 使用繁體中文 zh-TW：

- 功能規格 `spec.md`
- 實作計畫 `plan.md`
- 研究文件 `research.md`
- 資料模型 `data-model.md`
- 快速入門指南 `quickstart.md`
- 任務清單 `tasks.md`
- README 與使用者指南

程式碼內部識別名稱 MAY 使用英文；程式碼註解 MAY 使用英文或中文。Git commit
訊息 SHOULD 使用英文或團隊既定格式，但不得影響使用者面向文件的繁體中文要求。

### 功能開發流程

1. **規格定義**: 在 `/specs/###-feature-name/spec.md` 以繁體中文定義使用者故事、
   驗收標準、資料完整性要求與安全限制。
2. **計畫制定**: 使用 `/speckit.plan` 產生 `plan.md`、`research.md`、
   `data-model.md` 與 `quickstart.md`，並在 Constitution Check 中逐項驗證。
3. **憲章檢查**: 設計 MUST 符合本憲章所有原則，特別是資料完整性、金額精確度、
   測試先行、安全與稽核。
4. **測試先行**: 每個故事或修正 MUST 先撰寫失敗測試，確認測試能捕捉需求。
5. **實作**: 按使用者故事優先級實作，避免跨故事耦合破壞獨立交付。
6. **測試通過**: 合併前 MUST 通過相關單元測試、整合測試與必要的手動驗證。
7. **程式碼審查**: 審查 MUST 確認品質、資料完整性、安全、效能與憲章合規。

### 品質閘門

每個 Pull Request 或等效變更 MUST 通過：

- 所有相關自動化測試通過，包含單元與整合測試。
- 關鍵業務邏輯，尤其是金額計算，測試覆蓋率 MUST 達 80% 以上。
- 無編譯警告、格式錯誤或 linter 錯誤。
- 公開 API 的 XML 文件註解完整。
- 安全性掃描不得有未處理的高風險漏洞。
- 金額相關運算使用 `decimal`，不得使用 `float` 或 `double`。
- 至少一位維護者或團隊成員完成審查。

## 治理規則

### 憲章優先級

本憲章優先於所有其他開發實踐與指南。當規格、計畫、任務、README、AGENTS.md、
工具輸出或個人偏好與本憲章衝突時，MUST 以本憲章為準，除非憲章經正式程序修訂。

### 修訂程序

修訂本憲章 MUST 完成以下步驟：

1. **提案文件**: 說明修訂理由、影響範圍、替代方案與遷移風險。
2. **團隊審查**: 至少三分之二團隊成員或指定維護者同意。
3. **遷移計畫**: 評估對既有程式碼、規格、測試與文件的影響，並列出時程。
4. **版本更新**: 按語意化版本規則更新版本號。
5. **相依文件更新**: 同步更新 Spec Kit 模板、README 模板、AGENTS.md 與相關指南。

### 版本控制規則

- **MAJOR**: 移除或重新定義核心原則，或引入不相容的治理變更。
- **MINOR**: 新增原則、章節，或實質擴充既有指導。
- **PATCH**: 釐清說明、文字修正、格式修正或非語意的細化。

### 合規審查

- 所有 Pull Request 或等效變更 MUST 驗證憲章合規性。
- 每季度 SHOULD 進行憲章遵循審計；若無團隊流程，維護者 MUST 在重大版本前執行。
- 任何複雜度增加 MUST 在計畫中記錄業務價值理由與被拒絕的簡化替代方案。

### 執行指引

開發期間 MAY 參考 `AGENTS.md`、`.agents/skills/`、`.specify/templates/` 與未來新增的
`.github/instructions/csharp.instructions.md` 取得具體實作指引；這些檔案 MUST 與本憲章
保持一致，且不得降低本憲章要求。

**版本**: 1.0.0 | **批准日期**: 2026-02-20 | **最後修訂**: 2026-05-11
