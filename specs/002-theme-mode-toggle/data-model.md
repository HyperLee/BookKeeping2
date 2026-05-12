# Data Model: 網站主題模式切換

本功能沒有 SQLite schema 變更，且不新增 EF Core entity。以下模型描述瀏覽器端 UI 狀態與驗證規則。

## ThemeMode

使用者可保存的主題模式。

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `value` | string | Yes | 只能是 `light`、`dark`、`system` |
| `label` | string | Yes | 使用者面向文字必須為繁體中文：亮色模式、暗黑模式、跟隨系統 |
| `description` | string | No | 可提供輔助說明，但不得取代中文 label |

### Validation

- 從 UI、localStorage 或任何 script 讀到的值都必須經 allow-list 檢查。
- 無值、無效值或 localStorage 無法讀取時，模式視為 `system`。
- 系統不得保存 `effectiveTheme`、時間戳、歷史紀錄或任何可識別使用者的資料。

## EffectiveTheme

網站實際呈現的 Bootstrap 主題。

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `value` | string | Yes | 只能是 `light` 或 `dark` |
| `sourceMode` | string | Yes | 來自目前 ThemeMode |
| `systemPrefersDark` | boolean/null | Conditional | 只有 `sourceMode = system` 時用於推導 |

### Derivation

- `ThemeMode = light` -> `EffectiveTheme = light`
- `ThemeMode = dark` -> `EffectiveTheme = dark`
- `ThemeMode = system` 且 `matchMedia('(prefers-color-scheme: dark)').matches = true` -> `EffectiveTheme = dark`
- `ThemeMode = system` 且無法判斷系統偏好 -> `EffectiveTheme = light`

### State Transitions

```text
missing/invalid preference
  -> ThemeMode system
  -> EffectiveTheme from matchMedia or light fallback

user selects light
  -> persist "light"
  -> apply data-bs-theme="light"
  -> notify current page control state
  -> other tabs receive storage event and apply light

user selects dark
  -> persist "dark"
  -> apply data-bs-theme="dark"
  -> notify current page control state
  -> other tabs receive storage event and apply dark

user selects system
  -> persist "system"
  -> apply effective theme from matchMedia
  -> listen for system preference changes
  -> other tabs receive storage event and apply derived effective theme
```

## UserThemePreference

同一瀏覽器與裝置上的保存值。

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `storageKey` | string | Yes | `bookkeeping.theme.mode` |
| `mode` | ThemeMode value | Yes | 實際保存值只能是 `light`、`dark` 或 `system` |

### Persistence Rules

- 儲存在 `window.localStorage`。
- 寫入前必須驗證 mode。
- 讀取失敗時不得中斷頁面載入，必須回到 `system`。
- 不得寫入 SQLite、cookie、session、server log 或 audit event。

## PrimaryPage

本功能的主題、響應式與可及性驗證範圍。

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `route` | string | Yes | `/`、`/Privacy`、`/Error` |
| `rendersThemeControl` | boolean | Yes | 只有 `/` 為 true |
| `requiresThemeApplication` | boolean | Yes | 三個頁面都為 true |

### Validation

- 首頁必須顯示主題模式控制項與目前選取狀態。
- 隱私權頁與錯誤頁不得顯示主題模式控制項。
- 三個頁面在 `light`、`dark`、`system` 模式下，手機、平板、桌面寬度不得有文字或控制項重疊。
- 主要內容、導覽、按鈕、連結、表單、驗證訊息與焦點狀態需符合 WCAG 2.2 AA。
