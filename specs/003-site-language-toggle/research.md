# Research: 網站介面英文語系切換

## Decision: 使用 ASP.NET Core localization middleware 與 `.resx` 資源

**Rationale**: 本專案是 Razor Pages 伺服器端輸出 UI。ASP.NET Core 官方 localization 指引將多語系分成三件事：讓內容可本地化、提供支援文化的資源、為每個 request 選擇 culture。`IStringLocalizer`/ResourceManager 可在執行期依 `CurrentUICulture` 查找字串；DataAnnotations localization 可覆蓋表單欄位顯示名稱，但使用者面向驗證、錯誤與可執行修正提示依目前憲章維持繁體中文。現有預設語言已是繁體中文，因此以繁中 UI literal 作為預設 key，新增 `SharedResource.en.resx` 提供英文翻譯，可避免先建立大量 `zh-TW` 資源檔，同時保留預設繁中。

**Alternatives considered**:

- 手寫 dictionary/localization service：較難覆蓋 DataAnnotations display 與 Razor Pages 標準流程，也容易和框架欄位顯示名稱脫節。
- 全站 client-side text replacement：有閃爍、可及性、SEO、無 JS fallback 與表單欄位顯示不一致風險，且無法自然處理伺服器產生的繁體中文錯誤訊息。
- `IViewLocalizer` 為每頁建立資源：適合 view-specific text，但本功能需要 layout、partial、PageModel、DataAnnotations display 與 non-error service result 共用字串；以 `SharedResource` 較符合目前小型站台。

## Decision: 語言偏好使用自訂 allow-listed Cookie provider

**Rationale**: 規格要求預設繁體中文、不得依瀏覽器語言自動改英文，且偏好必須用伺服器可讀 Cookie 保存。設計採 `bookkeeping.ui.language` Cookie，允許值僅 `zh-TW` 與 `en`；缺少、空白或無效值一律 fallback 到繁體中文。Request localization provider list 只使用此自訂 provider 與 default request culture，不使用 `AcceptLanguageHeaderRequestCultureProvider`，因此瀏覽器或作業系統語言不會自動影響 UI。

**Alternatives considered**:

- ASP.NET Core 預設 `.AspNetCore.Culture` Cookie：框架支援完善，但 cookie value 格式為 culture/uiculture pair，不符合本規格「僅保存介面語言值」的簡潔 allow-list。
- localStorage：適合既有主題模式，但伺服器無法在產生 Razor Pages 前讀取，不能可靠套用到所有伺服器端 UI 文字。
- Session/SQLite：超出規格，會增加 persistence surface，也違反不新增 schema 與不保存切換歷史。

## Decision: 固定 `CurrentCulture` 為 `zh-TW`，只依偏好切換 `CurrentUICulture`

**Rationale**: 本功能目標是「介面文字」語言，不是改變金額、日期或 CSV 資料格式。ASP.NET Core 區分 `SupportedCultures` 與 `SupportedUICultures`：前者影響日期、數字、貨幣等 culture-dependent formatting，後者影響資源字串查找。為降低財務呈現回歸風險，自訂 provider 對繁中與英文都維持 `Culture=zh-TW`，並依 Cookie 將 `UICulture` 設為 `zh-TW` 或 `en`。既有明確格式如 `yyyy-MM-dd` 與金額 `N0` 保持穩定。

**Alternatives considered**:

- 英文模式同時設 `Culture=en-US`：能讓日期數字依英文文化呈現，但本規格未要求本地化資料格式，且可能改變既有金額/日期顯示。
- 完全不使用 request culture，只在 Razor 注入 dictionary：會失去 DataAnnotations display 與 Razor Pages 標準 localization pipeline。

## Decision: 首頁使用非財務 POST handler 寫入 Cookie

**Rationale**: 語言選擇是狀態變更，不應以 GET 修改 Cookie。首頁是唯一顯示控制項的位置，可用獨立 form POST 到 named handler，搭配 anti-forgery，驗證 allow-list 後由伺服器設定 1 年 Cookie，並 redirect 回首頁以呈現所選語言。JavaScript 可作為漸進增強，在 radio/select 改變時送出同一個非財務 form；無 JavaScript 時仍可用提交按鈕完成。

**Alternatives considered**:

- Client-side JavaScript 直接寫 Cookie 並 reload：可少一次 server handler，但較難保證 Secure/HttpOnly 設定，且把 allow-list 寫入前端。
- 在每頁提供語言切換：違反規格，控制項只能出現在首頁。
- 用現有財務表單承載語言值：會產生誤提交與資料完整性風險。

## Decision: 使用者資料原文與 CSV 檔案契約不本地化

**Rationale**: 規格明確要求不得翻譯或改寫使用者自行輸入或匯入的內容。帳戶名稱、使用者建立分類、交易備註、CSV 原始內容與資料庫保存值保持原文。系統定義列舉標籤與預設分類可在顯示層映射到英文，但不寫回資料庫。CSV 匯入匯出的欄位標題與交易類型值固定維持既有繁體中文格式，英文模式僅翻譯 CSV 頁面文字與非錯誤狀態；CSV 驗證、錯誤與修正提示依憲章維持繁體中文。

**Alternatives considered**:

- 將預設分類資料庫值改成英文或新增雙語欄位：需要 schema/data migration，且會混淆既有 CSV 與使用者資料。
- 英文模式輸出英文 CSV headers/type values：違反既有 CSV contract，也會破壞匯入相容性。

## Decision: 測試以整合測試 + 資源完整性 + 真實瀏覽器驗證分層

**Rationale**: 語言切換橫跨 request pipeline、Razor markup、DataAnnotations display、服務回傳訊息、CSV 格式與 responsive UI。單一測試型態無法有效覆蓋。設計使用 `WebApplicationFactory` 驗證 Cookie、fallback、HTML 輸出、繁體中文驗證/錯誤保留與 CSV contract；用資源完整性測試確認英文模式不出現未翻譯 key 或空白文字；用 Playwright 真實瀏覽器驗證首頁控制項、鍵盤操作、1 秒內呈現所選語言、重新整理/回訪、行動與桌面寬度、焦點狀態與內容不重疊。

**Alternatives considered**:

- 只做 unit tests：無法證明 Razor Pages、middleware 與 browser rendering 正確。
- 只做 snapshot tests：容易脆弱，且不直接驗證 cookie/security/CSV 契約。
- 完全手動檢查：無法滿足回歸保護與憲章測試先行要求。

## Sources

- Microsoft Learn: Globalization and localization in ASP.NET Core, .NET 10 view: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-10.0
- Microsoft Learn: `Microsoft.AspNetCore.Localization` namespace, .NET 10 view: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.localization?view=aspnetcore-10.0
