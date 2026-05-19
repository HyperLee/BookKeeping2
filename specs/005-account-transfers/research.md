# Research: 同幣別帳戶轉帳與信用卡繳款

## Decision: 以獨立 `AccountTransfer` entity 表示轉帳

**Rationale**: 規格明確要求轉帳不得以一筆支出加一筆收入模擬。獨立 table 可讓轉帳影響帳戶餘額，但天然排除於收入、支出、分類統計、月報與預算使用率，也能獨立支援 CSV 契約與稽核事件。

**Alternatives considered**:

- 在 `TransactionType` 加入 `Transfer`: 拒絕。既有 `TransactionType` 被分類、報表與預算邏輯視為收入/支出，加入第三態會提高誤算風險。
- 建立兩筆 linked transactions: 拒絕。會污染報表/預算，且編輯、刪除與稽核需要維持兩筆資料同步。

## Decision: 帳戶餘額維持即時計算，不儲存 derived balance

**Rationale**: 目前 `AccountService.GetBalanceSummariesAsync` 已從 opening balance 與未刪除交易 totals 計算餘額。延伸為 opening balance + income - expense - outgoing transfers + incoming transfers，可避免儲存餘額與明細不同步。新增/編輯/刪除轉帳後，頁面重新查詢即可反映餘額。

**Alternatives considered**:

- 在 `Accounts` 儲存目前餘額並於每次操作更新: 拒絕。需要額外一致性修復機制，且軟刪除/編輯轉帳更容易造成 drift。
- 建立帳戶流水 ledger snapshot: 拒絕。對目前單人、10,000+ 筆規模過重，規格沒有對帳快照需求。

## Decision: 同幣別規則沿用 `SupportedCurrency` 與 `Account.Currency`

**Rationale**: 多幣別基礎已存在，支援 `TWD`、`USD`、`JPY`、`EUR`、`GBP`，並已規定 trim、大小寫不敏感、儲存大寫代碼與兩位小數金額規則。轉帳只需驗證 transfer currency、from account currency、to account currency 三者完全一致。

**Alternatives considered**:

- 新增 `Currencies` table: 拒絕。支援幣別固定且沒有匯率或管理 UI。
- 由轉出帳戶自動推導幣別且不送出欄位: 部分拒絕。UI 可在選帳戶後輔助同步，但 server contract 仍保留幣別欄位，讓 CSV、稽核與錯誤訊息清楚可驗證。

## Decision: 混合明細時間線使用讀取端 projection

**Rationale**: 轉帳寫入端應保持與交易分離；明細頁需要共同排序、篩選與分頁。`TransactionQueryService` 可投影收入/支出與轉帳為同一個 timeline view model，先套用日期、幣別、帳戶、金額與關鍵字篩選，再依日期與穩定 id 排序。帳戶篩選對交易比對 `AccountId`，對轉帳比對 `FromAccountId` 或 `ToAccountId`。

**Alternatives considered**:

- 將所有明細搬到單一 ledger table: 拒絕。會大幅重構既有交易模型、migration 與報表，不符合本功能範圍。
- UI 分成交易列表與轉帳列表: 拒絕。規格要求與收入、支出一起顯示在交易明細時間線。

## Decision: 轉帳 CSV 使用獨立 header 與專用 parser/service

**Rationale**: 規格要求交易 CSV 與轉帳 CSV 不得互相誤判。轉帳 CSV header 固定為 `日期,幣別,金額,轉出帳戶,轉入帳戶,備註`，使用專用 parser 與 import/export service，保留 CsvHelper、row-level errors、檔案大小/筆數限制、文字清理與 formula injection protection。

**Alternatives considered**:

- 在既有交易 CSV 增加 `類型=轉帳`: 拒絕。會破壞既有交易 CSV 契約並讓分類/帳戶欄位語意混亂。
- 自動偵測沒有明確模式的 CSV: 拒絕。使用 exact header contract 才能避免誤判。

## Decision: 轉帳新增、編輯、刪除與 CSV 匯入使用 EF Core transaction

**Rationale**: 轉帳、稽核、CSV batch/error persistence 需要一致。新增/編輯/刪除轉帳與對應 audit event 必須在同一 EF Core transaction；CSV 匯入需要有效列建立、錯誤摘要、batch 與 audit 一致提交。帳戶餘額是查詢時計算，不在 transaction 中另行更新帳戶餘額欄位。

**Alternatives considered**:

- 分開儲存轉帳與稽核: 拒絕。會造成財務操作缺少可追溯摘要。
- CSV 有錯就整批 rollback: 拒絕。規格要求有效列建立、無效列略過並顯示逐列錯誤摘要。

## Decision: 軟刪除轉帳並排除一般查詢與 CSV 匯出

**Rationale**: 金融資料依既有交易模式採可追溯策略。`IsDeleted`、`DeletedAtUtc`、`DeletionSummary` 保留摘要；一般明細、帳戶餘額與轉帳 CSV 匯出只包含未刪除轉帳。

**Alternatives considered**:

- 硬刪除轉帳: 拒絕。違反財務資料可追溯偏好，且規格要求保留可追溯摘要。
- 以反向轉帳取代刪除: 拒絕。規格要求軟刪除後等同反轉原本餘額影響，不要求新增反向紀錄。

## Decision: 快速重送只防止同一次表單短時間重複

**Rationale**: 規格允許相同內容由使用者分次建立的獨立轉帳。沿用交易服務的短時間 duplicate check pattern：在幾秒內遇到日期、幣別、金額、轉出帳戶、轉入帳戶、備註完全相同的未刪除轉帳，視為同一次快速重送並回傳成功，不建立第二筆。

**Alternatives considered**:

- 建立唯一索引防止相同內容: 拒絕。會禁止合法的重複轉帳。
- 完全依賴 anti-forgery: 拒絕。anti-forgery 不防止雙擊或網路重送造成的重複提交。

## Decision: 新增轉帳頁面放在 `Pages/Transfers`，入口放在交易明細

**Rationale**: 轉帳表單是獨立流程，放在 `Pages/Transfers` 可避免把交易 create/edit model 複雜化；入口仍從 `Pages/Transactions/Index` 提供「新增轉帳」，並在時間線中顯示轉帳列，符合使用者查帳 workflow。

**Alternatives considered**:

- 將轉帳表單塞進 `Transactions/Create`: 拒絕。交易欄位包含分類與收入/支出類型，轉帳欄位包含轉出/轉入帳戶，混用會增加驗證與 UI 複雜度。
- 建立完全分離的轉帳管理頁且不出現在交易明細: 拒絕。規格要求交易明細時間線能看到轉帳。

## Decision: 不新增生產相依套件

**Rationale**: 既有 EF Core SQLite、CsvHelper、HtmlSanitizer、Bootstrap、jQuery Validation、Serilog 與測試工具足以實作資料模型、CSV、安全、UI 與驗證。axe-core 類 DOM/assertion 可作為測試輔助，不作為生產依賴。

**Alternatives considered**:

- 引入 money/currency library: 拒絕。沒有匯率、換算或小數位差異需求，且現有 `MoneyMinorUnitConverter` 已定義規則。
- 引入 client-side table/grid 套件: 拒絕。現有 Razor Pages + Bootstrap 表格/分頁足以支援規模與互動需求。
