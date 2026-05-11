# Open BookKeeping

Open BookKeeping 是單使用者部署的 ASP.NET Core Razor Pages 個人記帳工具。V1 固定使用 TWD 與 Asia/Taipei 本地日期，提供交易 CRUD、分類與帳戶管理、月報、預算追蹤、CSV 匯入匯出、搜尋篩選與遮罩稽核摘要。

## 功能

- 新增、編輯、軟刪除收入與支出交易
- 預設分類 seed、分類與帳戶管理
- 首頁本月摘要、帳戶餘額、預算進度與最近交易
- 月度報表與 Chart.js 趨勢圖
- 每月分類預算與 80%/100% 提醒
- RFC 4180 CSV 匯入/匯出，含特殊字元與公式注入防護
- 明細日期、分類、帳戶、金額與關鍵字篩選

## 開發需求

- .NET 10 SDK
- PowerShell 或相容 shell
- 可寫入的 SQLite 資料庫目錄

## 常用命令

```powershell
dotnet build BookKeeping2/BookKeeping2.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
dotnet run --project BookKeeping2/BookKeeping2.csproj --launch-profile https
```

預設啟動位址：

- `https://localhost:7185`
- `http://localhost:5069`

## 專案結構

- `BookKeeping2/Pages/`: Razor Pages UI
- `BookKeeping2/Services/`: 交易、報表、預算、CSV、稽核與共用服務
- `BookKeeping2/Models/`: EF Core 持久化實體
- `BookKeeping2/Data/`: DbContext、migration、entity configuration 與 seed data
- `BookKeeping2.Tests/`: xUnit 單元與整合測試
- `specs/001-personal-bookkeeping-tool/`: Spec Kit 規格、計畫、任務與驗證紀錄

## 工程規則

金額一律使用 `decimal`，SQLite 儲存採 minor units。狀態變更表單使用 antiforgery，CSV 下載不快取，稽核摘要不得記錄完整敏感財務明細。使用者介面與文件以繁體中文為主。
