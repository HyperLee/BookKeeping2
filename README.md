# Open BookKeeping

Open BookKeeping 是單使用者部署的 ASP.NET Core Razor Pages 個人記帳工具。V1 固定使用 TWD 與 Asia/Taipei 本地日期，提供交易 CRUD、分類與帳戶管理、月報、預算追蹤、CSV 匯入匯出、搜尋篩選與遮罩稽核摘要。

目前專案已加入 `specs/002-theme-mode-toggle/` 定義的網站主題模式切換：使用者只在首頁選擇「亮色模式」、「暗黑模式」或「跟隨系統」，整個網站會套用同一有效主題。

## 功能

- 新增、編輯、軟刪除收入與支出交易
- 預設分類 seed、分類與帳戶管理
- 首頁本月摘要、帳戶餘額、預算進度與最近交易
- 月度報表與 Chart.js 趨勢圖
- 每月分類預算與 80%/100% 提醒
- RFC 4180 CSV 匯入/匯出，含特殊字元與公式注入防護
- 明細日期、分類、帳戶、金額與關鍵字篩選
- 網站主題模式切換：亮色、暗黑、跟隨系統

## 主題模式切換

需求來源：[`specs/002-theme-mode-toggle/spec.md`](specs/002-theme-mode-toggle/spec.md)

- 首頁 `/` 顯示三選一主題控制項；其他 Razor Pages 只套用主題，不顯示控制項。
- 主題偏好只保存 `light`、`dark` 或 `system` 到瀏覽器 `localStorage` key `bookkeeping.theme.mode`。
- 共用 layout 會在 Bootstrap CSS 載入前先設定 `<html data-bs-theme>`，降低亮暗主題閃爍。
- `system` 模式使用 `prefers-color-scheme` 推導有效主題，系統偏好變更時會同步更新。
- 同 origin 已開啟分頁透過 `storage` event 在 2 秒內同步主題。
- 主題切換只影響視覺呈現，不新增資料庫 schema、不寫入財務資料、不送出表單，也不呼叫交易、帳戶、分類、預算、CSV 或報表端點。

## 開發需求

- .NET 10 SDK
- PowerShell 或相容 shell
- 可寫入的 SQLite 資料庫目錄，預設為 `BookKeeping2/App_Data/bookkeeping.db`
- Chrome、Edge 或 Playwright Chromium，用於主題模式瀏覽器測試

## 常用命令

從 repository root 執行：

```powershell
dotnet build BookKeeping2/BookKeeping2.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
dotnet run --project BookKeeping2/BookKeeping2.csproj --launch-profile https
```

預設啟動位址：

- `https://localhost:7185`
- `http://localhost:5069`

若只要收集 coverage：

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --collect:"XPlat Code Coverage"
```

## 專案結構

- `BookKeeping2/Pages/`: Razor Pages UI 與共用 layout
- `BookKeeping2/Services/`: 交易、報表、預算、CSV、稽核與共用服務
- `BookKeeping2/Models/`: EF Core 持久化實體
- `BookKeeping2/Data/`: DbContext、migration、entity configuration 與 seed data
- `BookKeeping2/wwwroot/css/site.css`: 共用樣式、主題色彩、對比與焦點狀態
- `BookKeeping2/wwwroot/js/site.js`: 主題模式、成功訊息與表單焦點等瀏覽器端行為
- `BookKeeping2.Tests/`: xUnit 單元測試、整合測試與 Playwright 主題模式測試
- `specs/001-personal-bookkeeping-tool/`: 個人記帳工具 V1 規格、計畫與任務
- `specs/002-theme-mode-toggle/`: 網站主題模式切換規格、計畫、contract、quickstart 與任務

## 工程規則

金額一律使用 `decimal`，SQLite 儲存採 minor units。狀態變更表單使用 antiforgery，CSV 下載不快取，稽核摘要不得記錄完整敏感財務明細。使用者介面與文件以繁體中文為主。

新增功能需從 Spec Kit 文件開始，先撰寫能描述需求的測試，再實作並執行相關 build/test。主題、響應式與可及性變更需檢查手機、平板與桌面寬度，並確認文字、控制項、表格、圖表與焦點狀態不重疊且維持足夠對比。
