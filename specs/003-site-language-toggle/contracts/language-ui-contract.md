# UI Contract: 網站介面英文語系切換

## Scope

本契約定義網站介面語言選擇、Cookie、server-rendered UI、CSV contract、使用者資料原文與可及性驗證要求。它不定義任何新增資料庫 schema、外部 API 或跨裝置同步。

## Language Values

| User-facing option | Submitted value | Cookie value | `html lang` | UI culture |
|--------------------|-----------------|--------------|-------------|------------|
| `繁體中文` | `zh-TW` | `zh-TW` | `zh-Hant-TW` | `zh-TW` |
| `English` | `en` | `en` | `en` | `en` |

Default when missing or invalid: `zh-TW`.

Formatting culture: `zh-TW` for both UI languages, so existing date, number, amount and TWD presentation remains stable unless a specific page already formats with an explicit pattern.

## Cookie Contract

Cookie name: `bookkeeping.ui.language`

Allowed values:

- `zh-TW`
- `en`

Required attributes:

- `Path=/`
- Expires or Max-Age equivalent to 1 year from the manual selection
- `SameSite=Lax`
- `Secure=true` on HTTPS requests
- `HttpOnly=true`
- `IsEssential=true`

Invalid values:

- Empty, missing, malformed or unsupported values must be ignored.
- Invalid values must not throw exceptions.
- The rendered UI must fall back to Traditional Chinese.

Prohibited storage:

- SQLite tables
- EF Core migrations
- Session
- `localStorage`
- audit events
- server logs
- telemetry containing the selected language value

## Request Culture Contract

The request culture resolver must:

- Read only `bookkeeping.ui.language`.
- Validate the value with an allow-list.
- Resolve `CurrentUICulture` to `zh-TW` or `en`.
- Keep `CurrentCulture` at `zh-TW`.
- Ignore `Accept-Language`, browser language, operating system language and device language.
- Fall back to `zh-TW` without surfacing internal errors.

`UseRequestLocalization` must run before Razor Pages render localized content.

## Homepage Control Contract

The language control appears only on `BookKeeping2/Pages/Index.cshtml`.

Required behavior:

- Shows the label/group name for interface language.
- Shows both options: `繁體中文` and `English`.
- Clearly marks the currently selected option.
- Is operable by mouse, touch and keyboard.
- Uses semantic grouping, such as `fieldset`/`legend`, or an equivalent accessible pattern.
- Submits to a dedicated non-financial homepage handler.
- Uses anti-forgery protection.
- Writes only the language Cookie.
- Redirects or reloads to render the selected language.
- Does not submit transaction, category, account, budget, CSV import/export or report forms.

Non-home pages:

- Must apply the selected language.
- Must not render the language control.

JavaScript:

- May auto-submit the homepage language form as progressive enhancement.
- Must not perform global text replacement.
- Must not be required for a user to change language.
- Must not read or write financial data.
- Must not produce script errors when browser capabilities are unavailable.

## Rendering Contract

All currently user-facing fixed interface text must localize:

- layout navigation and footer
- page titles and headings
- section titles
- form labels
- buttons and links
- table headers
- helper text
- empty states
- success messages, non-error status messages and confirmation messages
- pagination text
- chart labels and accessible chart names
- system enum/status labels

User-facing validation messages, error messages and actionable correction hints remain Traditional Chinese under the current constitution. English mode must keep those messages associated with the relevant fields or alerts, and must not render internal keys, blanks or placeholders.
- default category display names

English mode must not render:

- blank labels
- internal resource keys
- unreplaced placeholders
- incomplete mixed-language system messages

Razor HTML encoding remains the baseline. Resource strings must localize text, not HTML markup.

## User Data Contract

The following data must remain unchanged and displayed as original user data:

- account names
- user-created category names
- transaction notes
- CSV raw values and previews
- imported category names created from CSV data
- report amounts and computed totals
- transaction dates and persisted values

System-defined labels may be translated only at display time. Translation must not be persisted.

## CSV Contract

CSV import/export file format remains unchanged in both UI languages.

Exported headers:

```text
日期,類型,金額,分類,帳戶,備註
```

Transaction type values in CSV:

```text
收入
支出
```

CSV parser expected headers and accepted transaction type values remain Traditional Chinese. English mode localizes only the CSV page UI and non-error import/export status. User-facing CSV validation, error and correction messages remain Traditional Chinese.

## Accessibility And Layout Contract

Both languages must satisfy:

- visible focus for the language control and all major actions
- keyboard path to select and submit language
- no content overlap at mobile, tablet and desktop widths
- no horizontal overflow caused by longer English words
- table and form controls remain readable
- alert, validation and error text remains associated with the relevant control
- `html lang` reflects the resolved UI language

Browser verification must include at least one desktop viewport and one mobile viewport. The existing theme mode behavior must continue to work in both languages.

## Security Contract

- Treat Cookie values as untrusted input.
- Use allow-list validation only.
- Use anti-forgery protection for the language POST.
- Do not emit raw HTML from resource strings.
- Do not log raw Cookie values, resolved language preference values or user financial data.
- Preserve `UseBookKeepingSecurityHeaders()`, HTTPS redirection and production HSTS behavior.

## Performance Contract

- First request without valid Cookie renders Traditional Chinese directly.
- Requests with valid Cookie render selected language directly in the server response.
- Homepage language switch and following page views should show the selected language within 1 second under normal local app conditions.
- Localization must not query SQLite for fixed UI strings.
