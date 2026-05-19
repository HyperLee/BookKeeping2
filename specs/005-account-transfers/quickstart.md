# Quickstart: 同幣別帳戶轉帳與信用卡繳款

## 1. 開始前檢查

```powershell
git branch --show-current
dotnet build BookKeeping2.slnx
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~SupportedCurrencyTests|FullyQualifiedName~MoneyMinorUnitConverterTests"
```

預期分支為 `005-account-transfers`。若工作樹已有使用者未提交變更，先確認變更範圍，不要覆寫非本功能檔案。

## 2. 先寫失敗測試

依使用者故事順序建立測試。每個測試任務先列出測試意圖並取得使用者或維護者確認，再撰寫測試、確認測試會失敗，最後才實作。

建議測試檔：

- `BookKeeping2.Tests/Unit/AccountTransfers/AccountTransferServiceTests.cs`
- `BookKeeping2.Tests/Unit/Accounts/AccountTransferBalanceTests.cs`
- `BookKeeping2.Tests/Unit/Transactions/TransactionTimelineQueryTests.cs`
- `BookKeeping2.Tests/Unit/Csv/CsvTransferImportParserTests.cs`
- `BookKeeping2.Tests/Unit/Csv/CsvTransferImportServiceTests.cs`
- `BookKeeping2.Tests/Unit/Csv/CsvTransferExportServiceTests.cs`
- `BookKeeping2.Tests/Integration/Pages/AccountTransferPagesTests.cs`
- `BookKeeping2.Tests/Integration/Pages/TransactionTimelineTransferTests.cs`
- `BookKeeping2.Tests/Integration/Pages/CsvTransferPageTests.cs`
- `BookKeeping2.Tests/Integration/Persistence/AccountTransferPersistenceTests.cs`
- `BookKeeping2.Tests/Integration/Performance/TransactionTimelinePerformanceTests.cs`

Minimum P1 failing tests:

- 建立 TWD 1,000 從銀行到現金，銀行餘額減少、現金餘額增加。
- 轉出與轉入帳戶相同時拒絕。
- 轉出與轉入帳戶幣別不同時拒絕。
- 轉出後負餘額允許。
- 同一 `SubmissionToken` 快速重送不建立第二筆；相同內容但不同 `SubmissionToken` 可建立獨立轉帳。
- 信用卡繳款轉帳不增加月支出與預算使用率。
- 編輯轉帳後以更新後內容重新計算相關帳戶餘額。
- 軟刪除轉帳後一般查詢、餘額與 CSV 匯出排除。

## 3. Persistence 與服務實作順序

1. 新增 `AccountTransfer` model、entity configuration、`DbSet` 與 migration。
2. 新增 `AccountTransferInputModel`、form options、result type 與 service interface。
3. 實作 `AccountTransferService` validation、SubmissionToken duplicate rapid resubmit、same-content different-token allowance、create/update/soft delete、audit transaction boundary。
4. 調整 `AccountService.GetBalanceSummariesAsync`，加入 outgoing/incoming transfer totals。
5. 調整或擴充交易明細 query，回傳收入、支出與轉帳混合時間線。
6. 確認 `ReportService`、`BudgetService` 與首頁月收入支出摘要仍只使用 income/expense transactions。

## 4. UI 實作順序

1. 建立 `Pages/Transfers/Create`、`Edit`、`Delete`。
2. 在 `Pages/Transactions/Index` 加入 `新增轉帳` 入口。
3. 在明細時間線加入轉帳列樣式與方向文字。
4. 確認篩選條件套用到轉帳列：日期、幣別、帳戶、關鍵字、金額。
5. 確認 category filter 排除轉帳列。
6. 手機與桌面寬度檢查表單、錯誤訊息、方向文字與按鈕不重疊。

## 5. CSV 實作順序

1. 新增 transfer CSV row、parser、import service、export service。
2. 在 CSV 頁面提供獨立轉帳匯入與匯出操作。
3. 確認轉帳 CSV header 為 `日期,幣別,金額,轉出帳戶,轉入帳戶,備註`。
4. 確認交易 CSV header 不被轉帳匯入接受。
5. 確認轉帳 CSV header 不被交易匯入接受。
6. 匯出時對帳戶名稱與備註做 formula injection protection。

## 6. Targeted Verification

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~AccountTransfer"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~TransactionTimeline|FullyQualifiedName~CsvTransfer"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~ReportServiceTests|FullyQualifiedName~BudgetServiceTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~AccountTransferPersistenceTests|FullyQualifiedName~TransactionTimelinePerformanceTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~AccountTransferPerformanceTests|FullyQualifiedName~CsvTransferImportServiceTests|FullyQualifiedName~CsvTransferExportServiceTests"
```

Full verification before completion:

```powershell
dotnet build BookKeeping2.slnx
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --collect:"XPlat Code Coverage"
```

Coverage output must show at least 80% coverage for critical transfer amount, balance, CSV, and query logic before completion.

If browser automation is available:

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~AccountTransferBrowserTests"
```

Browser tests must include mobile/desktop responsive checks and WCAG 2.1 AA accessibility assertions through axe-core or equivalent DOM assertions. If browser tests cannot run because Chrome or Edge is unavailable, report that exact blocker and still run the non-browser subset.

## 7. Manual Scenario Checklist

- 建立兩個 TWD 帳戶，新增 TWD 1,000 從銀行到現金，確認餘額變化。
- 建立銀行到信用卡帳戶的繳款轉帳，確認月報與預算不增加支出。
- 嘗試同帳戶轉帳，確認錯誤訊息為繁體中文且指出不可相同。
- 快速重送同一新增表單，確認同一 `SubmissionToken` 不建立第二筆；重新開啟表單後相同內容可建立獨立轉帳。
- 嘗試 TWD 帳戶轉 USD 帳戶，確認錯誤訊息指出幣別必須一致。
- 用低餘額帳戶轉出較大金額，確認可建立且餘額可為負。
- 編輯轉帳金額與帳戶，確認舊帳戶與新帳戶餘額都正確。
- 軟刪除轉帳，確認時間線、餘額與轉帳 CSV 匯出都排除該筆。
- 匯入包含有效列與無效列的轉帳 CSV，確認有效列建立、無效列列出行號與原因。
- 匯出轉帳 CSV，確認 header、排序、內容與公式保護。
- 在手機寬度與桌面寬度檢查轉帳表單與時間線列不重疊、鍵盤可操作，且通過 WCAG 2.1 AA 核心可及性檢查。
