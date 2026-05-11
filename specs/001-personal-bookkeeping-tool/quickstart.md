# 快速入門: Open BookKeeping

**日期**: 2026-05-11  
**功能分支**: `001-personal-bookkeeping-tool`

## 前置條件

- .NET 10 SDK
- 可寫入的本機 SQLite 資料庫目錄
- PowerShell 7 或 Windows PowerShell

## 還原與建置

從 repository root 執行:

```powershell
dotnet restore BookKeeping2/BookKeeping2.csproj
dotnet build BookKeeping2/BookKeeping2.csproj
```

目前 `BookKeeping2.slnx` 尚未包含 web project；在 solution 更新前不要用 `.slnx` 作為主要驗證單位。

## 執行應用程式

```powershell
dotnet run --project BookKeeping2/BookKeeping2.csproj --launch-profile https
```

預期位址:

- HTTPS: `https://localhost:7185`
- HTTP: `http://localhost:5069`

## 測試專案

本 feature 的第一批實作任務必須建立 `BookKeeping2.Tests`。建立後以此命令驗證:

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
```

測試範圍至少包含:

- 交易新增、編輯、軟刪除與重複提交防護
- 金額 `decimal` 與 minor units 轉換
- Asia/Taipei 今日與月份歸屬
- 分類與帳戶 normalized name 唯一性
- 帳戶餘額計算
- 月報與分類佔比
- 預算 80% 與 100% 提醒
- CSV 匯入、匯出、特殊字元與公式注入防護
- Razor Pages 表單驗證與 antiforgery

## 開發順序建議

1. 建立 `BookKeeping2.Tests`、測試基礎設施與 SQLite test database fixture。
2. 建立 EF Core Data/Models/Migrations 與 seed 預設分類。
3. 先以失敗測試覆蓋 P1 交易、持久化、分類與帳戶。
4. 實作 P1 Razor Pages 與服務層，確認單元與整合測試通過。
5. 接著實作 P2 報表、預算與 CSV 匯出。
6. 最後實作 P3 CSV 匯入與搜尋篩選。

## 手動驗證清單

- 首次啟動後可看到預設收入/支出分類。
- 新增一筆 TWD 150 餐飲支出後，明細與首頁摘要更新。
- 編輯金額為 200 後，報表與帳戶餘額同步更新。
- 刪除交易後，一般列表與報表不顯示該交易。
- 清除瀏覽器快取或換瀏覽器後，資料仍存在。
- 320px 手機寬度與桌面寬度下，文字、表單與圖表不重疊。
- CSV 匯出可被試算表開啟，備註逗號、引號、換行與公式風險皆安全。

## 部署注意事項

- V1 不提供站內帳號。若部署於公開網路，必須由反向代理、VPN、Basic Auth 或其他受信任部署層提供存取控制。
- 生產環境必須啟用 HTTPS/HSTS。
- SQLite 資料庫檔案與備份不得提交版本控制。
- connection string 與任何秘密值使用 Secret Manager、環境變數或受控秘密儲存。
- 一般診斷日誌不得含完整金額、完整備註或敏感財務資料。
