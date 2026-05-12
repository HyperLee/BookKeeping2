# Quickstart: 網站主題模式切換

## Prerequisites

- .NET 10 SDK
- 可執行桌面瀏覽器 Chrome、Edge、Firefox 或 Safari
- Playwright、axe-core 或等效瀏覽器驗證工具，用於主題互動、跨分頁同步、首次繪製前套用、系統偏好與 WCAG 驗證

## Build And Test

```powershell
dotnet build BookKeeping2/BookKeeping2.csproj
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
```

### Baseline Verification

- 2026-05-12 Phase 1 baseline: `dotnet build BookKeeping2/BookKeeping2.csproj` 通過，0 warnings、0 errors。
- 2026-05-12 Phase 1 baseline: `dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj` 通過，41/41 tests passed；沒有既有失敗需要記錄。

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
3. 前往所有目前可由使用者直接瀏覽的站內頁面，確認各頁在 1 秒內套用同一有效主題且不顯示主題選擇控制項：`/Privacy`、`/Error`、`/Accounts`、`/Categories`、`/Budgets`、`/Transactions`、`/Transactions/Create`、具測試資料 id 的 `/Transactions/Edit/{id}` 與 `/Transactions/Delete/{id}`、`/Csv/Import`、`/Csv/Export`、`/Reports`。
4. 回首頁選擇「亮色模式」，確認 localStorage key `bookkeeping.theme.mode` 的值為 `light`，且頁面有效主題為 light。
5. 選擇「跟隨系統」，使用瀏覽器開發者工具或作業系統外觀設定切換 light/dark，確認 2 秒內更新有效主題。
6. 開啟兩個同站分頁，在其中一個首頁變更主題，確認另一個分頁 2 秒內同步。
7. 將 localStorage key 改成無效值，例如 `invalid`，重新整理後確認回到 `system` 行為，且無 script error。
8. 在手機、平板、桌面寬度檢查所有目前可由使用者直接瀏覽的站內頁面，確認文字、控制項、導覽、表單、表格、圖表與 footer 不重疊。
9. 以鍵盤 Tab / Shift+Tab / Arrow keys 操作首頁控制項，確認焦點可見且可切換。
10. 檢查亮色與暗黑主題下所有目前可由使用者直接瀏覽的站內頁面的主要文字、連結、按鈕、表單、表格、圖表、alert、驗證訊息與焦點狀態符合 WCAG 2.2 AA。
11. 以真實瀏覽器或等效工具驗證目標瀏覽器環境中首次繪製前已套用有效主題；若封鎖網站儲存或外觀偏好查詢，確認頁面無 script error 且套用安全 fallback。

## Expected Implementation Order

1. 先新增會失敗的整合測試，覆蓋首頁控制項、所有非首頁使用者可瀏覽頁不顯示控制項、layout 初始化 hook 與靜態資源引用。
2. 先新增會失敗的 Playwright/axe 或等效瀏覽器測試，覆蓋主題切換、首次繪製前套用、站內後續頁面 1 秒一致、跨分頁同步、系統偏好、fallback 與可及性。
3. 將失敗測試的測試意圖與預期失敗原因交由使用者或維護者確認後，才實作主題行為。
4. 實作 head 初始化 script、首頁 radio group、`site.js` runtime 行為與 `site.css` 主題樣式。
5. 跑完整 build/test，再執行手動或自動瀏覽器驗證。
