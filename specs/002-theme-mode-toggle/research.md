# Research: 網站主題模式切換

## Decision: 使用 `localStorage` 保存選取的主題模式

**Rationale**: 規格要求偏好範圍是同一瀏覽器與裝置，且僅保存 `light`、`dark` 或 `system`。`localStorage` 符合跨頁與回訪需求，不需要帳號、伺服器 session 或 SQLite schema 變更，也避免把純 UI 偏好混入財務資料庫。

**Alternatives considered**:

- SQLite `AppSetting`: 不符合「同一瀏覽器與裝置」的偏好範圍，且會不必要地新增資料庫寫入與 migration。
- Cookie: 可支援伺服器端預先渲染判斷，但本需求不需要送到伺服器，會增加每次 request payload 與隱私面。
- Session storage: 分頁關閉後失效，不符合回訪沿用偏好。

## Decision: 以 Bootstrap `data-bs-theme` 套用有效主題

**Rationale**: 專案已使用 Bootstrap 5.3.3，內建 `[data-bs-theme=light]` 與 `[data-bs-theme=dark]` 色彩變數。將有效主題寫入 `<html data-bs-theme="light|dark">` 可以讓 Bootstrap 元件、表單、表格、alert、navbar 與自訂 CSS 共享同一主題來源。

**Alternatives considered**:

- 自訂 `body.dark` / `body.light` 類別: 需要重寫大量 Bootstrap 變數與元件狀態，維護成本較高。
- 分離亮色/暗黑 CSS 檔: 增加資產數量與載入順序風險，且容易讓兩份樣式偏離。

## Decision: 首次繪製前在 `_Layout.cshtml` head 內執行最小初始化 script

**Rationale**: FR-015 與 SC-008 要求在目標瀏覽器環境中避免先顯示相反主題。外部 script 需等待額外資源載入，較難保證在 CSS 套用前完成。head 內小型同步 script 可在 Bootstrap CSS 載入前讀取 allow-list localStorage 值、推導有效主題並設定 `document.documentElement.dataset.bsTheme`。若使用者或瀏覽器設定封鎖必要能力，頁面仍需在可執行後儘早 fallback 且不得產生 script error。

**Alternatives considered**:

- 只在 `site.js` 初始化: 會等到 body 後段 script 載入，較容易出現亮暗閃爍。
- 伺服器根據 cookie 輸出主題: 可預先渲染，但本功能不需要把偏好傳給伺服器，且 cookie 不是規格要求的保存方式。

## Decision: 首頁控制項使用語意化 radio group

**Rationale**: 三種模式互斥，原生 radio button 提供鍵盤、螢幕閱讀器與表單語意。首頁可使用 Bootstrap button/radio 樣式或 `form-check`，但底層仍應保留 `fieldset`、`legend`、相同 `name` 與明確 `checked` 狀態。

**Alternatives considered**:

- Select 下拉: 可用但目前狀態不如三選項直觀，且切換成本較高。
- 三個普通 button: 需自行補完整 ARIA selected/pressed 與鍵盤行為，較容易漏掉可及性細節。

## Decision: 使用 `storage` event 同步同站已開啟分頁

**Rationale**: 當任一分頁更新 localStorage，其他同 origin 分頁會收到 `storage` event，可在 2 秒內重新推導並套用有效主題。這是廣泛支援的瀏覽器機制，不需要伺服器、SignalR 或輪詢。

**Alternatives considered**:

- `BroadcastChannel`: API 清楚，但不如 `storage` event 與 localStorage 保存機制自然整合，部分環境支援仍需 fallback。
- 週期性輪詢 localStorage: 可行但浪費資源，也會延遲同步。
- 伺服器推播: 對純本機 UI 偏好過重。

## Decision: 系統模式以 `matchMedia('(prefers-color-scheme: dark)')` 推導

**Rationale**: `matchMedia` 是瀏覽器暴露系統或瀏覽器外觀偏好的標準機制。選取模式為 `system` 時註冊 change listener，系統偏好變更後重新套用有效主題；若無法判斷，依規格 fallback 到亮色外觀。

**Alternatives considered**:

- 只在載入時讀取一次: 不符合使用期間系統偏好變更需同步更新的需求。
- 永遠依系統偏好，不提供明確 light/dark 覆蓋: 不符合三模式需求。

## Decision: 以整合測試驗證 Razor markup，以瀏覽器驗證補足互動與可及性

**Rationale**: `WebApplicationFactory` 已存在，可先測首頁包含控制項、所有其他使用者可瀏覽頁不包含控制項、layout 包含 head 初始化 hook 與 `site.js`。跨分頁同步、首次繪製前主題、系統偏好、1 秒/2 秒時間限制、對比與焦點狀態需要真實瀏覽器能力，因此必須用 Playwright、axe-core 或等效瀏覽器工具補足，手動 QA 只能作為輔助驗證。

**Alternatives considered**:

- 只做手動測試: 無法滿足憲章測試優先要求。
- 只做 xUnit markup 測試: 無法實證 localStorage、media query、首次繪製前套用與 cross-tab 行為。
