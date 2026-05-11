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
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --collect "XPlat Code Coverage"
```

關鍵業務邏輯，尤其是金額計算、交易寫入、報表、預算與 CSV 匯入匯出，coverage 必須達 80% 以上。

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
- 主要成功標準時間門檻: 交易反映 1 秒、100 筆月報 2 秒、1,000 筆 CSV 匯出 5 秒、100 筆 CSV 匯入 10 秒、預算提醒 1 秒、10,000 筆篩選 2 秒

## 品質閘門

合併或交付前需完成:

```powershell
dotnet build BookKeeping2/BookKeeping2.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --collect "XPlat Code Coverage"
dotnet list BookKeeping2/BookKeeping2.csproj package --vulnerable --include-transitive
dotnet list BookKeeping2.Tests/BookKeeping2.Tests.csproj package --vulnerable --include-transitive
```

- 公開 API 必須具備 XML 文件註解；具複雜行為或金融計算的 API 必須包含範例。
- 安全掃描不得有未處理的高風險套件弱點。
- 未經使用者明確操作或部署設定允許時，瀏覽器網路面板不得出現第三方傳輸。
- WCAG 2.1 AA 核心檢查需涵蓋鍵盤操作、語意標記與對比度。

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
- 首次使用者流程可在 30 秒內完成第一筆交易新增。
- 新增、編輯或刪除交易後，明細列表與首頁摘要在 1 秒內反映變更。
- 320px 手機寬度與桌面寬度下，文字、表單與圖表不重疊。
- 鍵盤可完成主要流程，表單欄位與錯誤訊息具備語意標記，文字與控制項對比度符合 WCAG 2.1 AA 核心要求。
- CSV 匯出可被試算表開啟，備註逗號、引號、換行與公式風險皆安全。
- 100 筆月報在 2 秒內呈現且統計結果符合測試資料。
- 1,000 筆 CSV 匯出在 5 秒內完成；100 筆標準 CSV 匯入在 10 秒內完成並列出成功/失敗摘要。
- 10,000 筆交易套用單一篩選條件後在 2 秒內呈現結果。
- 開啟瀏覽器網路面板完成主要流程，確認未授權第三方傳輸為 0。

## Phase 11 驗證紀錄（2026-05-11）

| 項目 | 結果 | 證據 |
|------|------|------|
| Web 專案建置與 XML 文件註解 | 通過 | `dotnet build BookKeeping2/BookKeeping2.csproj`，0 warning / 0 error。 |
| 全套自動測試與 coverage | 通過 | `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --no-restore --collect "XPlat Code Coverage"`，41/41 passed；Cobertura line-rate 85.70%，branch-rate 68.46%。 |
| 套件弱點掃描 | 通過 | `dotnet list BookKeeping2/BookKeeping2.csproj package --vulnerable --include-transitive` 與 `dotnet list BookKeeping2.Tests/BookKeeping2.Tests.csproj package --vulnerable --include-transitive` 均回報目前來源未提供任何易受攻擊套件。 |
| CSV 下載 no-store | 通過 | `CsvExportPageTests` 驗證 `Cache-Control` 含 `no-store`、content type 為 `text/csv; charset=utf-8`，並記錄 `CsvExported` 稽核事件。 |
| CSV 匯入/匯出安全 | 通過 | 單元測試覆蓋 RFC 4180 欄位順序、逗號/引號/換行、公式注入前綴、欄位數、檔案大小與錯誤摘要。 |
| 預算與稽核遮罩 | 通過 | 預算警告稽核使用 `MaskAmount`，交易異動使用 `MaskAmount` 與 `MaskText`；CSV 匯入/匯出稽核只記錄筆數與摘要，不記錄完整備註。 |
| 主要效能成功標準 | 通過 | 自動測試覆蓋 100 筆月報 < 2 秒、1,000 筆 CSV 匯出 < 5 秒、100 筆 CSV 匯入 < 10 秒、預算提醒 < 1 秒、10,000 筆篩選 < 2 秒；交易建立與跨頁可見性由整合測試驗證。 |
| 320px/桌面響應式與可及性核心檢查 | 完成程式碼層級檢查 | `site.css` 增加手機寬度容器、表格、按鈕、progress 與 canvas 約束；主要表單使用 label、validation span、語意 table/section/nav 與鍵盤可操作的原生控制項。 |
| 未授權第三方傳輸 | 通過程式碼檢查 | 應用程式腳本未使用 `fetch`、`sendBeacon` 或外部 CDN；Chart.js 已 vendored 至 `wwwroot/lib/chart.js/`，CSP `default-src 'self'`。 |

## 部署注意事項

- V1 不提供站內帳號。若部署於公開網路，必須由反向代理、VPN、Basic Auth 或其他受信任部署層提供存取控制。
- 生產環境必須啟用 HTTPS/HSTS。
- SQLite 資料庫檔案與備份不得提交版本控制。
- connection string 與任何秘密值使用 Secret Manager、環境變數或受控秘密儲存。
- 一般診斷日誌不得含完整金額、完整備註或敏感財務資料。
