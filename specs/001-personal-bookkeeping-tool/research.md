# Phase 0 研究: Open BookKeeping

**日期**: 2026-05-11  
**來源**: [spec.md](./spec.md)、`markdownFolder/tempPlan.md`、`.specify/memory/constitution.md`

## 結論摘要

本功能採 ASP.NET Core 10 Razor Pages + EF Core 10 SQLite + Bootstrap 5 的單體 Web 架構。所有金額在 C# 領域模型與服務 API 中維持 `decimal`，SQLite 端以縮放整數欄位保存 minor units，避免 `double` 造成精度風險，也避免 SQLite provider 對 `decimal` 非等值查詢需要 client evaluation 的限制。V1 使用單使用者部署模型，資料持久化於站台實例的 SQLite 資料庫，不使用瀏覽器 localStorage 作為主要資料來源。

## 決策

### Decision: 保留 Razor Pages 單體架構

**Rationale**: 現有專案已是 Razor Pages starter，功能以表單、列表、報表與 CSV 上傳下載為主。Razor Pages 能讓每個使用者故事以頁面資料夾獨立交付，並能直接使用 Tag Helpers、DataAnnotations、jQuery Validation 與 `WebApplicationFactory` 做整合測試。

**Alternatives considered**:

- MVC Controllers + Views: 對本功能沒有明顯收益，會增加路由與 controller/action 分散度。
- SPA + API: 對單使用者 V1 過重，且增加前後端契約、CSRF/CORS、建置與測試複雜度。

### Decision: 使用 EF Core 10 + SQLite 作為持久化層

**Rationale**: SQLite 符合單人部署、跨瀏覽器一致與低維運成本需求。EF Core 提供 Migrations、LINQ 查詢、外鍵、交易與 `WebApplicationFactory` 整合測試能力；資料模型可從 SQLite 遷移到其他 relational provider。

**Alternatives considered**:

- JSON/檔案儲存: 難以可靠處理索引、查詢、跨資料表一致性與同時寫入。
- 瀏覽器 localStorage/IndexedDB: 不符合跨瀏覽器一致與清除快取後資料仍存在的需求。
- SQL Server/PostgreSQL: 適合多使用者或大型部署，但 V1 單人部署成本較高。

### Decision: 金額領域型別使用 `decimal`，SQLite 儲存使用 minor units

**Rationale**: 憲章禁止用 `float` 或 `double` 處理貨幣。SQLite provider 對 `decimal` 非等值比較與排序有限制，因此交易、預算與初始餘額在 C# API 使用 `decimal`，資料庫欄位保存 `long AmountMinorUnits`、`long OpeningBalanceMinorUnits`、`long BudgetAmountMinorUnits`。輸入驗證限制最多 2 位小數並轉換為 minor units；顯示與計算再轉回 `decimal`。

**Alternatives considered**:

- EF Core decimal 直接映射 SQLite: 可讀寫與等值查詢，但範圍篩選、排序與彙總會有 provider 限制。
- 轉換為 `double`: 違反憲章，且有精度風險。
- 文字儲存 decimal: 不利排序、範圍查詢與索引。

### Decision: 日期使用 `DateOnly`，今日與月份邊界由 Asia/Taipei time service 決定

**Rationale**: 交易日期、預算月份、月報與 CSV 日期不需要時間戳；`DateOnly` 清楚表達本地日曆日期。服務層注入 `ITaipeiDateService` 或 `TimeProvider` 包裝，測試可固定現在時間；部署環境時區不得影響交易是否為未來日期。

**Alternatives considered**:

- 使用 UTC `DateTime` 表示交易日: 容易在時區換日產生月份歸屬錯誤。
- 使用伺服器本地時區: 部署到非台灣時區時會違反規格。

### Decision: 跨資料表異動使用 EF Core transaction

**Rationale**: 新增/編輯/刪除交易會影響交易表、帳戶餘額摘要、預算狀態與稽核事件；CSV 匯入會批次建立交易與分類。這些操作必須全部成功或全部回滾，以符合資料完整性原則。

**Alternatives considered**:

- 每個 repository 各自 `SaveChangesAsync`: 寫入失敗時可能留下部分資料。
- 不保存衍生摘要，只即時計算: 可降低一致性風險，但仍需交易保存交易與稽核；帳戶餘額與預算可優先即時計算並用索引優化。

### Decision: 交易刪除採軟刪除，分類/帳戶採封存或受限刪除

**Rationale**: 交易刪除仍需可追溯且一般列表/報表排除。分類被交易引用時不得刪除；帳戶被交易引用時也不得破壞歷史資料。未被引用的自訂分類可刪除；已被引用的項目應提供封存/停用，避免新交易選取但保留歷史可讀性。

**Alternatives considered**:

- 硬刪除交易: 不符合規格與稽核需求。
- 允許 cascade delete 分類/帳戶: 會破壞歷史交易脈絡。

### Decision: CSV 以 RFC 4180 相容格式，並防範公式注入

**Rationale**: 匯入匯出必須處理逗號、引號、換行與固定六欄位。採用 CsvHelper 作為 CSV parser/writer，避免手刻易錯字串切割。匯出時若文字欄位以 `=`, `+`, `-`, `@`, tab 或 CR/LF 可形成試算表公式風險，必須加安全前綴或採明確轉義策略；匯入時要列出行號與欄位級錯誤。

**Alternatives considered**:

- `string.Split(',')`: 無法處理引號、換行與逗號，拒絕。
- `TextFieldParser`: 可作為結構化 parser，但 writer、mapping 與測試慣例不如 CsvHelper 直接。
- JSON 匯出: 對備份有用，但規格明確要求 CSV。

### Decision: 報表使用伺服器彙總資料 + Chart.js 呈現

**Rationale**: 報表數值必須可由交易資料驗證。伺服器端使用 EF Core 彙總 income/expense/category/day/month 資料，Razor Page 只輸出最小圖表資料給 Chart.js。這能讓報表計算以單元測試驗證，並避免前端重複實作財務規則。

**Alternatives considered**:

- 前端載入所有交易後彙總: 10,000 筆資料時效能與資料暴露面較差。
- 純表格無圖表: 不符合視覺化報表需求。

### Decision: 測試分層為服務單元測試 + Razor Pages 整合測試

**Rationale**: 金額、日期、分類唯一性、CSV 與報表可由服務單元測試快速覆蓋；頁面表單、ModelState、antiforgery、資料持久化與導覽透過 `WebApplicationFactory` 覆蓋。每個 P1/P2/P3 user story 必須可獨立測試。

**Alternatives considered**:

- 只做手動測試: 違反憲章測試優先。
- 只做 UI 端到端測試: 速度慢且難以覆蓋所有金融邊界。

### Decision: 日誌使用遮罩摘要與事件分類

**Rationale**: 需要追蹤新增、編輯、刪除交易、CSV 匯入匯出、預算警告與寫入失敗，但不得把完整金額、完整備註或私人財務細節寫入一般診斷紀錄。稽核摘要保留事件類型、時間、實體 ID、類型、遮罩金額區間或 hash 摘要。

**Alternatives considered**:

- 完整日誌: 診斷方便但違反隱私與憲章。
- 完全不記錄: 無法支援問題診斷與稽核。

### Decision: 生產部署補上安全 header 與受信任存取模型

**Rationale**: V1 不提供站內帳號，因此部署層必須限制公開存取。應用仍保留 HTTPS/HSTS、CSRF、Razor HTML encoding、輸入驗證、CSP header 與 secrets 管理要求，避免單使用者模型被誤解為可公開部署。

**Alternatives considered**:

- 在 V1 內建完整登入/角色: 超出 spec 範圍，會推高交付成本。
- 只依賴網路隔離、不做應用層安全: 不符合憲章。

## 參考資料

- Microsoft Learn: ASP.NET Core Razor Pages with .NET 10
- Microsoft Learn: ASP.NET Core antiforgery
- Microsoft Learn: EF Core SQLite provider and provider limitations
- IETF RFC 4180: Common Format and MIME Type for CSV Files
