# UI Contract: 網站主題模式切換

## Scope

此 contract 定義 Razor Pages 使用者介面與瀏覽器端狀態的 observable behavior。沒有新增 HTTP API、資料庫 contract 或外部服務 contract。

## Global Theme Contract

- 每個 Razor Page 都必須透過共用 layout 在 `<html>` 上套用 `data-bs-theme="light"` 或 `data-bs-theme="dark"`。
- 可選擇在 `<html>` 上同步 `data-theme-mode="light|dark|system"`，用於測試與 CSS hook；此值不得取代 `data-bs-theme`。
- 若偏好不存在、無效或 localStorage 無法讀取，選取模式必須視為 `system`。
- 若 `system` 無法判斷系統偏好，有效主題必須 fallback 為 `light`。
- 首次繪製前初始化必須出現在 Bootstrap CSS 前，並且只能讀取 allow-list 主題偏好與 `matchMedia`。

## Local Storage Contract

| Key | Allowed Values | Invalid Handling |
|-----|----------------|------------------|
| `bookkeeping.theme.mode` | `light`, `dark`, `system` | 忽略該值並視為 `system` |

- 寫入只允許在首頁控制項變更時發生。
- 寫入值只能是選取模式，不得包含有效主題、時間戳、財務資料或使用者識別資訊。
- 其他同 origin 分頁收到 `storage` event 後，必須在 2 秒內重新套用最新有效主題。

## Home Page Control Contract

首頁 `/` 必須渲染一組互斥的三選項控制項：

```html
<fieldset data-theme-mode-control>
  <legend>主題模式</legend>
  <input type="radio" name="themeMode" value="light">
  <input type="radio" name="themeMode" value="dark">
  <input type="radio" name="themeMode" value="system">
</fieldset>
```

Observable requirements:

- 控制項只出現在首頁。
- 控制項可用滑鼠、觸控與鍵盤操作。
- 三個選項的使用者面向文字必須包含「亮色模式」、「暗黑模式」、「跟隨系統」。
- 目前選取模式必須透過 `checked` 狀態或等效可及性狀態清楚標示。
- 切換後目前頁面必須立即套用有效主題，成功標準目標為 1 秒內。

## Non-Home Page Contract

`/Privacy`、`/Error` 與其他站內 Razor Pages:

- 必須套用 `<html data-bs-theme>` 推導出的有效主題。
- 不得渲染 `[data-theme-mode-control]` 或任何主題選擇 radio/select/button group。
- 不得因頁面不同而覆寫使用者已選定的主題模式。

## Runtime JavaScript Contract

`wwwroot/js/site.js` 必須提供下列行為：

- 初始化目前頁面的主題狀態，並同步首頁控制項的 selected 狀態。
- 監聽首頁 radio change，驗證值後寫入 localStorage 並套用主題。
- 監聽 `storage` event，當 `bookkeeping.theme.mode` 改變時重新讀取、推導與套用主題。
- 當模式為 `system` 時監聽 `prefers-color-scheme` change，系統偏好變更後重新套用有效主題。
- localStorage 或 matchMedia 發生例外時不得讓頁面 script 中斷；必須 fallback 到 `system` / `light`。

## Accessibility Contract

- 控制項必須有可見群組標籤，且每個選項都有可點擊 label。
- 焦點指示在亮色與暗黑主題下都必須可見。
- 主題切換不得讓目前焦點元素消失或被不可見樣式覆蓋。
- 主要文字、連結、按鈕、表單、表格、alert、驗證訊息與 footer/nav 在 light/dark 有效主題下都必須符合 WCAG 2.2 AA 對比。

## Data Integrity Contract

- 主題切換不得送出任何財務表單。
- 主題切換不得呼叫交易、分類、帳戶、預算、CSV 或報表相關 endpoint。
- 主題切換不得修改 SQLite 檔案或 EF Core tracked data。
