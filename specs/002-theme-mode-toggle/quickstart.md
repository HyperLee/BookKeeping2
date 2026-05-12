# Quickstart: 網站主題模式切換

## Prerequisites

- .NET 10 SDK
- 可執行桌面瀏覽器 Chrome、Edge、Firefox 或 Safari
- 若後續任務加入 UI 自動化，需安裝 Playwright/axe-core 對應測試工具

## Build And Test

```powershell
dotnet build BookKeeping2/BookKeeping2.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
```

若需要 coverage:

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --collect:"XPlat Code Coverage"
```

## Run Locally

```powershell
dotnet run --project BookKeeping2/BookKeeping2.csproj --launch-profile https
```

Launch profiles:

- HTTPS: `https://localhost:7185`
- HTTP: `http://localhost:5069`

## Manual Verification Checklist

1. 開啟首頁 `/`，確認只有首頁顯示「亮色模式」、「暗黑模式」、「跟隨系統」三個選項。
2. 選擇「暗黑模式」，確認首頁 1 秒內切換為暗黑外觀，重新整理後仍維持暗黑模式。
3. 前往 `/Privacy` 與 `/Error`，確認頁面套用同一有效主題且不顯示主題選擇控制項。
4. 回首頁選擇「亮色模式」，確認 localStorage key `bookkeeping.theme.mode` 的值為 `light`，且頁面有效主題為 light。
5. 選擇「跟隨系統」，使用瀏覽器開發者工具或作業系統外觀設定切換 light/dark，確認 2 秒內更新有效主題。
6. 開啟兩個同站分頁，在其中一個首頁變更主題，確認另一個分頁 2 秒內同步。
7. 將 localStorage key 改成無效值，例如 `invalid`，重新整理後確認回到 `system` 行為，且無 script error。
8. 在手機、平板、桌面寬度檢查首頁、隱私權頁、錯誤頁，確認文字、控制項、導覽、footer 不重疊。
9. 以鍵盤 Tab / Shift+Tab / Arrow keys 操作首頁控制項，確認焦點可見且可切換。
10. 檢查亮色與暗黑主題下主要文字、連結、按鈕、表單、alert、驗證訊息與焦點狀態符合 WCAG 2.2 AA。

## Expected Implementation Order

1. 先新增會失敗的整合測試，覆蓋首頁控制項、非首頁不顯示控制項、layout 初始化 hook 與靜態資源引用。
2. 若採 UI 自動化，先新增會失敗的 Playwright/axe 測試，覆蓋主題切換、跨分頁同步、系統偏好與可及性。
3. 實作 head 初始化 script、首頁 radio group、`site.js` runtime 行為與 `site.css` 主題樣式。
4. 跑完整 build/test，再執行手動或自動瀏覽器驗證。
