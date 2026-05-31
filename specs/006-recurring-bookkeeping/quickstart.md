# Quickstart: 定期交易與轉帳確認

## 1. 開始前檢查

```powershell
git branch --show-current
dotnet build BookKeeping2.slnx
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~AccountTransfer|FullyQualifiedName~SupportedCurrency|FullyQualifiedName~MoneyMinorUnitConverter"
```

預期分支為 `006-recurring-bookkeeping`。若工作樹已有使用者未提交變更，先確認變更範圍，不要覆寫非本功能檔案。

## 2. 先寫失敗測試

依使用者故事順序建立測試。每個測試任務先列出測試意圖並取得使用者或維護者確認，再撰寫測試、確認測試會失敗，最後才實作。

建議測試檔：

- `BookKeeping2.Tests/Unit/Recurring/RecurrenceDateCalculatorTests.cs`
- `BookKeeping2.Tests/Unit/Recurring/RecurringRuleServiceTests.cs`
- `BookKeeping2.Tests/Unit/Recurring/RecurringOccurrenceMaterializationTests.cs`
- `BookKeeping2.Tests/Unit/Recurring/RecurringConfirmationServiceTests.cs`
- `BookKeeping2.Tests/Unit/Transactions/RecurringTransactionSourceTests.cs`
- `BookKeeping2.Tests/Unit/AccountTransfers/RecurringTransferSourceTests.cs`
- `BookKeeping2.Tests/Integration/Pages/RecurringPagesTests.cs`
- `BookKeeping2.Tests/Integration/Pages/HomeRecurringSummaryTests.cs`
- `BookKeeping2.Tests/Integration/Persistence/RecurringPersistenceTests.cs`
- `BookKeeping2.Tests/Integration/Performance/RecurringOccurrencePerformanceTests.cs`
- `BookKeeping2.Tests/Integration/Browser/RecurringBrowserTests.cs`

Minimum P1 failing tests:

- 建立每月 5 日支出規則，到期後 materialize 一筆 pending occurrence，且正式 `Transactions` 筆數仍為 0。
- 建立每月 10 日收入規則，到期後出現在待確認清單。
- 未到期規則不出現在首頁摘要或待確認清單。
- 缺少幣別、金額、分類、帳戶或週期時拒絕規則保存並顯示繁體中文錯誤。
- 確認 pending 支出後建立一筆正式 transaction，帳戶餘額、月報與預算反映結果。
- 確認前調整本期金額，只影響正式 transaction，不改寫 parent recurring rule。
- 將確認日期改為未來日期時拒絕。
- 同一期快速重送或重新整理重送不建立第二筆 transaction。
- 建立定期同幣別轉帳並確認後，兩個帳戶餘額正確變動，月報與預算不包含該轉帳。
- 轉帳規則或確認時 from/to/currency 不一致時拒絕。

Minimum P2/P3 failing tests:

- 每月 31 日規則在 2 月使用該年 2 月最後一天。
- 跨過三個月後列出三筆獨立 pending occurrence。
- 確認一期、略過一期後，第三期仍 pending。
- 略過同一期後重新整理不再出現。
- 停用規則後不 materialize 新 occurrence，且已確認正式紀錄保留。
- 編輯規則未來預設金額不改寫已 materialized pending occurrence。
- 首頁顯示 pending count 與處理入口；無 pending 時不顯示誤導警告。
- 手機與桌面寬度下規則表單、pending list、確認表單不重疊。

## 3. Persistence 與 domain 實作順序

1. 新增 `RecurringRecordKind`、`RecurringFrequency`、`RecurringOccurrenceStatus` enum。
2. 新增 `RecurringRule`、`RecurringOccurrence` model、entity configuration、`DbSet` 與 migration。
3. 在 `Transaction` 與 `AccountTransfer` 新增 nullable `RecurringOccurrenceId` 與唯一索引。
4. 新增 `RecurrenceDateCalculator`，先讓 weekly/monthly/yearly、月底 fallback、跨年與閏年測試通過。
5. 新增 recurring input/result/view models 與 form options。
6. 實作 `RecurringRuleService` create/update/deactivate/soft delete validation、sanitization、audit。
7. 實作 `RecurringOccurrenceService.MaterializeDueAsync`，使用 unique `{ RecurringRuleId, ScheduledDate }` 保持 idempotent。
8. 抽出或共用既有 transaction/transfer validation，避免定期確認與手動流程規則漂移。
9. 實作 `ConfirmTransactionAsync` 與 `ConfirmTransferAsync`，在單一 EF Core transaction 內建立正式紀錄、更新 occurrence、寫入 recurring audit 與 formal record audit。
10. 實作 `SkipAsync`，確保只更新單一期別且不建立正式紀錄。

## 4. UI 實作順序

1. 建立 `Pages/Recurring/Index`、`Create`、`Edit`、`Delete`。
2. 建立 `Pages/Recurring/Pending` 待確認清單。
3. 建立 `ConfirmTransaction` 與 `ConfirmTransfer` 頁面，預填 occurrence snapshot 並允許本期調整。
4. 建立 `Skip` 確認頁。
5. 在首頁加入 pending summary 與處理入口。
6. 在 shared layout 或既有導覽加入 `定期項目` 入口，保持既有主題與語系功能。
7. 檢查手機與桌面寬度下名稱、金額、方向、錯誤訊息與按鈕不重疊。

## 5. Formal Record Integration

1. 確認 pending occurrence 不出現在交易 timeline、轉帳 timeline、帳戶餘額、報表、預算或 CSV。
2. 確認 income/expense occurrence 後，正式 transaction 應出現在既有交易 timeline、報表、預算與 transaction CSV。
3. 確認 transfer occurrence 後，正式 transfer 應出現在混合 timeline、帳戶餘額與 transfer CSV。
4. 確認 transfer 不進收入、支出、月報、分類統計、趨勢圖或預算使用率。
5. 正式紀錄後續軟刪除沿用既有 transaction/transfer delete flow；occurrence 仍保持 confirmed。

## 6. Targeted Verification

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~RecurrenceDateCalculator|FullyQualifiedName~RecurringRuleService"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~RecurringOccurrence|FullyQualifiedName~RecurringConfirmation"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~RecurringPages|FullyQualifiedName~HomeRecurringSummary"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~RecurringPersistence|FullyQualifiedName~RecurringOccurrencePerformance"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~ReportServiceTests|FullyQualifiedName~BudgetServiceTests|FullyQualifiedName~CsvTransfer"
```

Full verification before completion:

```powershell
dotnet build BookKeeping2.slnx
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --collect:"XPlat Code Coverage"
```

Coverage output must show at least 80% coverage for recurrence date calculation, materialization, confirmation idempotency, formal record creation and money validation paths before completion.

If browser automation is available:

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~RecurringBrowserTests"
```

Browser tests must include mobile/desktop responsive checks and WCAG 2.1 AA core accessibility assertions through axe-core or equivalent DOM assertions. If browser tests cannot run because Chrome or Edge is unavailable, report that exact blocker and still run the non-browser subset.

## 7. Manual Scenario Checklist

- 建立每月房租支出規則，到期後確認 pending 出現且正式交易尚未建立。
- 確認房租 pending，確認交易明細、帳戶餘額、月報與預算更新。
- 調整本期訂閱費金額後確認，確認 parent rule 預設金額未改變。
- 嘗試以未來日期確認，確認顯示繁體中文錯誤且未建立正式紀錄。
- 快速重送同一確認表單，確認只建立一筆正式紀錄。
- 建立每月銀行到信用卡定期轉帳，到期確認後檢查兩帳戶餘額與報表/預算排除。
- 建立每月 31 日規則，檢查 2 月、閏年與跨年日期。
- 讓同一規則跨過三期，逐期確認/略過並確認剩餘 pending 狀態正確。
- 停用規則後確認不再產生新 pending，已確認正式紀錄仍存在。
- 軟刪除規則後確認歷史 occurrence 與正式紀錄可追溯。
- 在首頁有 pending 時看到摘要與處理入口，無 pending 時不顯示誤導警告。
- 在手機與桌面寬度檢查規則列表、待確認清單、確認表單、錯誤訊息與操作按鈕不重疊、鍵盤可操作。
