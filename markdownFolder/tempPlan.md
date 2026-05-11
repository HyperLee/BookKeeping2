# 技術上下文

**語言/版本**: C# 14 / .NET 10.0  
**主要相依性**: ASP.NET Core 10.0（Razor Pages）、Bootstrap 5、jQuery 3.x、jQuery Validation、Chart.js 4.x、Entity Framework Core 10.0（SQLite provider）、Serilog、HtmlSanitizer  
**儲存**: SQLite（透過 Entity Framework Core + Microsoft.EntityFrameworkCore.Sqlite）  
**測試**: xUnit + Moq（單元測試）+ WebApplicationFactory（整合測試）  
**目標平台**: 桌面瀏覽器（Chrome、Edge、Firefox、Safari）+ 行動裝置瀏覽器（響應式）  
**專案類型**: Web（單一 ASP.NET Core Razor Pages 專案 + 測試專案）  
**效能目標**: FCP < 1.5 秒、LCP < 2.5 秒；10,000 筆紀錄查詢 < 3 秒；100 筆月報產生 < 2 秒  
**限制條件**: CSV 匯入 < 5MB / 10,000 筆上限；金額一律使用 `decimal` 型別；V1 單一幣別  
**規模/範圍**: 單人使用、約 8 個頁面、預估年累積 600-2,400 筆紀錄、極端情境 10,000+ 筆


| # | 原則 | 狀態 | 說明 |
|---|------|------|------|
| I | 程式碼品質至上 (NON-NEGOTIABLE) | ✅ 通過 | 採用 C# 14、檔案範圍命名空間、Nullable Reference Types、XML 文件註解、.editorconfig 格式化 |
| II | 測試優先開發 (NON-NEGOTIABLE) | ✅ 通過 | 採用 xUnit + Moq + WebApplicationFactory；金額計算一律使用 `decimal`；每個 User Story 獨立可測試 |
| III | 使用者體驗一致性 | ✅ 通過 | Bootstrap 5 統一設計語言；jQuery Validation 即時驗證；Toast 通知機制；Mobile-first 響應式設計 |
| IV | 效能與延展性 | ✅ 通過 | FCP < 1.5s / LCP < 2.5s 目標；async/await I/O；靜態資源壓縮；Chart.js 輕量圖表 |
| V | 可觀察性與監控 | ✅ 通過 | Serilog 結構化 JSON 日誌；檔案輪替 30 天；關鍵業務操作 Information 級別記錄 |
| VI | 安全優先 | ✅ 通過 | Razor 引擎 HTML 編碼；Anti-Forgery Token；HtmlSanitizer 處理 CSV 匯入文字；HTTPS + HSTS |
| VII | 資料完整性 (NON-NEGOTIABLE) | ✅ 通過 | `decimal` 金額；EF Core 交易原子性；EF Core Migrations 版本化遷移；軟刪除策略 |


**結構決策**: 採用 ASP.NET Core Razor Pages 單一專案架構（非前後端分離）。這與現有專案結構一致，並符合規格書的「單一 Razor Pages 專案」需求。額外建立一個獨立的 xUnit 測試專案。程式碼按關注點分層為 Models、Data、Services、ViewModels、Validation、Pages。