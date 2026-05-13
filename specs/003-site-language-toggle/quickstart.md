# Quickstart: 網站介面英文語系切換

## Prerequisites

- Branch: `003-site-language-toggle`
- Spec: `specs/003-site-language-toggle/spec.md`
- Plan: `specs/003-site-language-toggle/plan.md`
- App project: `BookKeeping2/BookKeeping2.csproj`
- Test project: `BookKeeping2.Tests/BookKeeping2.Tests.csproj`

## Implementation Order

1. Confirm each user story's test intent with the user or maintainer, then add failing tests:
   - `LanguageTogglePageTests` for homepage control, non-home absence, selected language rendering, invalid Cookie fallback, ignored `Accept-Language`, Traditional Chinese validation/error preservation and data invariants.
   - `UiLanguageRequestCultureProviderTests` for allow-list, default, invalid and culture/UI culture resolution.
   - `LanguageResourceCompletenessTests` for English resource coverage and no blank/internal-key output.
   - CSV tests proving exported headers, transaction type values, raw values, amount/date values and persisted data remain `日期,類型,金額,分類,帳戶,備註`, `收入`, `支出` in English mode.
   - Playwright tests for keyboard selection, one-second selected-language rendering, reload/return persistence, mobile/desktop layout and focus visibility.
2. Register localization services and request localization middleware.
3. Add `Localization/` helper types and `Resources/SharedResource.en.resx`.
4. Add homepage language POST handler and localized language control.
5. Localize layout, pages, partials, PageModel non-error messages, DataAnnotations display names and user-facing non-error service result messages; keep validation, error and actionable correction messages in Traditional Chinese.
6. Add display-only localization for system enum labels and default category names.
7. Verify CSV parser/exporter data contract remains unchanged.
8. Run targeted and full verification.

## Expected Manual Behavior

### Default Traditional Chinese

1. Clear `bookkeeping.ui.language`.
2. Open `/`.
3. Confirm the page, layout navigation and homepage summaries are Traditional Chinese.
4. Set browser preferred language to English or send `Accept-Language: en-US`.
5. Confirm the site still renders Traditional Chinese until the user manually chooses English.

### Switch To English

1. On `/`, choose `English`.
2. Confirm the homepage reloads or re-renders in English.
3. Visit `/Transactions`, `/Transactions/Create`, `/Categories`, `/Accounts`, `/Budgets`, `/Reports`, `/Csv/Import`, `/Csv/Export`, `/Privacy` and `/Error`.
4. Confirm fixed UI text is English and the language control appears only on `/`.
5. Confirm account names, user-created category names, notes and imported CSV values remain original.

### Switch Back To Traditional Chinese

1. On `/`, choose `繁體中文`.
2. Confirm the homepage and later navigation return to Traditional Chinese.
3. Confirm the Cookie value is `zh-TW`.

### Invalid Cookie

1. Manually set `bookkeeping.ui.language=fr` or another unsupported value.
2. Open `/`.
3. Confirm the UI safely falls back to Traditional Chinese.
4. Confirm no server error, script error, audit event or financial data change occurs.

### CSV Contract

1. Switch to English.
2. Export CSV from `/Csv/Export`.
3. Confirm file headers remain:

   ```text
   日期,類型,金額,分類,帳戶,備註
   ```

4. Confirm transaction type values remain `收入` and `支出`.
5. Import a valid Traditional Chinese CSV and confirm the page UI/status is English while imported data values remain unchanged.

### Responsive And Accessibility

1. Test `/` at mobile and desktop widths.
2. Confirm the language control is keyboard operable and has visible focus.
3. Confirm longer English text does not overlap, truncate important labels or cause horizontal overflow.
4. Confirm `<html lang>` is `zh-Hant-TW` in Traditional Chinese mode and `en` in English mode.
5. Confirm existing theme mode behavior still works in both languages.

## Commands

Run from repository root.

```powershell
dotnet build BookKeeping2.slnx
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj
```

Targeted commands while developing:

```powershell
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~LanguageTogglePageTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~UiLanguageRequestCultureProviderTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~LanguageResourceCompletenessTests"
dotnet test BookKeeping2.Tests/BookKeeping2.Tests.csproj --filter "FullyQualifiedName~LanguageToggleBrowserTests"
```

If browser tests cannot run because Chrome or Edge is unavailable in the environment, record the exact blocker and still run the non-browser subset.
