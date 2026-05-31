# Research: 定期交易與轉帳確認

## Decision: 保留現有 ASP.NET Core Razor Pages 技術棧，不新增生產排程或 recurrence 套件

**Rationale**: 本功能第一版只支援每週、每月、每年與結束日，且明確要求到期後由使用者確認才建立正式交易或轉帳。現有 ASP.NET Core Razor Pages、EF Core SQLite、Bootstrap、jQuery Validation、Serilog、HtmlSanitizer、CsvHelper、`ITaipeiDateService` 與測試基礎已能覆蓋表單、資料完整性、稽核、安全處理與 UI 驗證。引入 Quartz、Hangfire、Cron/RRULE library 或 SPA framework 會增加部署、測試與資料完整性複雜度，卻不會提供第一版必要價值。

**Alternatives considered**:

- Quartz/Hangfire 背景排程：拒絕。規格排除自動入帳與通知，背景工作會讓「確認前不得建立正式紀錄」更難保證。
- RRULE/NCrontab 類 recurrence library：拒絕。第一版規則簡單，且月底 fallback、Asia/Taipei 今日限制與逐期確認需要明確可測的本地邏輯。
- Client-side only calculation：拒絕。到期清單、重複防護與正式紀錄建立都屬財務資料完整性，必須由伺服器端掌控。

## Decision: 使用伺服器端 deterministic `RecurrenceDateCalculator`

**Rationale**: 規則只需 weekly/monthly/yearly。以 `DateOnly` 和 `ITaipeiDateService.Today` 計算可避免時區時間點與 DST 問題。每月 29、30、31 日遇到較短月份使用該月最後一天；同一 fallback 也套用於 yearly 的 2 月 29 日，讓閏年與非閏年結果可預期。規則開始日可在未來，因為它是排程模板；但 materialized occurrence 與確認後正式紀錄日期不得晚於 Asia/Taipei 今日。

**Alternatives considered**:

- 儲存下一期日期並每次加一週/月/年：部分採用但不作唯一真相。若規則編輯、停用或 occurrence materialization 失敗，只靠下一期日期容易漏期；date calculator 應能從規則與已處理期別重新推導。
- 使用 `DateTimeOffset` 作 recurrence key：拒絕。使用者需求以本地日為單位，正式交易與轉帳也使用 `DateOnly`。
- 年度 2 月 29 日在非閏年略過：拒絕。規格要求月底 fallback 與閏年可預期；使用 2 月最後一天較符合既有 monthly fallback。

## Decision: 持久化 `RecurringOccurrence` 快照，而不是每次動態投影全部待確認項目

**Rationale**: 規格要求已存在待確認項目不得被規則後續編輯靜默改寫，且多期到期需要逐期確認或略過。持久化 occurrence 可保存到期當下的預設金額、幣別、分類、帳戶、轉帳方向與備註快照，支援 pending/confirmed/skipped 狀態、稽核、重複防護與未來 performance index。首頁與定期頁讀取前由 service materialize 到今日為止尚未存在的 due occurrences。

**Alternatives considered**:

- 完全動態產生 pending list：拒絕。規則修改後會改變過去 pending 預設值，且略過狀態仍需另行持久化。
- 預先產生所有未來期數：拒絕。結束日可能很遠或未設定，會建立大量未到期資料，且規格要求未到期不列待確認。
- 只保存最後處理日期：拒絕。無法表示略過單一期、確認其中一期後保留其他期，且無法追溯單期期別。

## Decision: 以 formal record `RecurringOccurrenceId` 唯一索引防止重複確認

**Rationale**: 既有交易快速重送以短時間內容比對防重，轉帳使用 `SubmissionToken`。定期確認需要比內容比對更強的資料庫級保證，因為使用者可調整本期內容，且同一期必須只能建立一筆正式交易或轉帳。新增 nullable `RecurringOccurrenceId` 到 `Transactions` 與 `AccountTransfers`，並建立唯一索引，可允許一般手動紀錄保持 null 且不受影響，同時讓同一期競態重送無法產生第二筆正式紀錄。

**Alternatives considered**:

- 只檢查 occurrence 狀態是否 pending：拒絕。兩個快速請求可能在狀態更新前都讀到 pending。
- 只使用表單 submission token：拒絕。確認表單可重新開啟或調整內容，唯一 business key 應是 occurrence。
- 在 occurrence 上只存 generated record id：必要但不足。若 formal record 寫入成功而 occurrence 更新遇到競態，仍需要 formal table unique index 作最後防線。

## Decision: 確認流程在單一 EF Core transaction 內直接建立正式紀錄、更新 occurrence 與記錄稽核

**Rationale**: 規格要求正式交易或轉帳、帳戶餘額、報表、預算與稽核在確認後一致，且財務跨表操作必須原子化。確認服務應重用或抽出既有交易/轉帳驗證邏輯，然後在同一 `AppDbContext` transaction 內建立 `Transaction` 或 `AccountTransfer`、設定 occurrence 為 confirmed、寫入 recurring audit 與既有 formal record audit。對 expense transaction 仍需觸發既有 budget warning audit。

**Alternatives considered**:

- 呼叫既有 `TransactionService.CreateAsync` 或 `AccountTransferService.CreateAsync` 後再更新 occurrence：拒絕。既有服務各自開啟 transaction，無法自然保證 occurrence 狀態與 formal record 原子一致。
- 先更新 occurrence 再建立正式紀錄：拒絕。正式紀錄失敗會留下已確認但無正式紀錄的狀態。
- 完全複製既有 validation code：拒絕。容易造成手動與定期流程規則漂移；若實作時需要，應抽出小型 validation helper。

## Decision: 停用/刪除規則停止新 materialization，但不靜默移除已存在 pending occurrence

**Rationale**: 規格明確要求停用後不產生新的待確認項目，且已建立正式交易或轉帳不能被自動刪除或改寫。對已 materialized 的 pending occurrence，較安全的行為是保留給使用者逐期確認或略過，避免因停用規則而靜默遺失已到期但未處理的補登事項。刪除規則採軟刪除，以保留 occurrence 與 formal record 的稽核追溯。

**Alternatives considered**:

- 停用時自動略過所有 pending occurrence：拒絕。這是財務處理決策，應由使用者逐期執行。
- 刪除規則時硬刪除 pending occurrence：拒絕。會破壞 audit trail，也可能移除使用者仍需確認的到期項目。
- 停用後仍 materialize 結束日前所有期數：拒絕。與「停用後不再產生」衝突。

## Decision: 定期功能不新增 CSV 規則匯入匯出

**Rationale**: 規格排除定期規則 CSV 匯入匯出。確認後建立的正式交易或轉帳沿用既有 CSV 契約；pending occurrence 在確認前不得出現在正式交易/轉帳匯出，也不得影響報表、預算或帳戶餘額。

**Alternatives considered**:

- 新增 recurring rule CSV：拒絕。已明確排除，且會增加驗證、錯誤回報與安全處理範圍。
- 在正式 CSV 匯出 pending occurrence：拒絕。pending 不是正式財務紀錄。

## Decision: 首頁摘要以服務查詢 pending count，不在 layout 或 client script 推導

**Rationale**: 首頁摘要需要準確反映已到期且尚未處理的項目，並可能先觸發 materialization。此邏輯屬伺服器端資料完整性，不應由 layout 或 JavaScript 掃描本地狀態。首頁 PageModel 可呼叫 recurring occurrence service 取得 count 與少量摘要；其他頁面不必承擔全站查詢成本。

**Alternatives considered**:

- 全站 layout 顯示待確認 badge：拒絕第一版。每頁查詢與 materialization 增加成本，規格只要求首頁摘要與定期頁。
- localStorage/client-side reminder：拒絕。不能作為財務到期狀態來源。

## Decision: 測試補強以 existing xUnit/WebApplicationFactory/Playwright 為主

**Rationale**: 既有測試專案已覆蓋 service、persistence、Razor Pages、static/browser/performance patterns。定期功能風險集中在日期、idempotency、資料完整性、狀態轉換與 responsive UI；使用現有測試架構可最快建立回歸保護。axe-core 或等效可及性檢查只作測試工具，不加入生產 bundle。

**Alternatives considered**:

- 只做單元測試：拒絕。確認流程跨 EF Core、Razor Page handlers、anti-forgery、formal record 建立與稽核，需要整合測試。
- 只做手動 UI 驗證：拒絕。首頁摘要與確認表單是核心流程，需 browser coverage。
