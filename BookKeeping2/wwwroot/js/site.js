(function () {
    const storageKey = 'bookkeeping.theme.mode';
    const allowedModes = new Set(['light', 'dark', 'system']);

    function normalizeMode(mode) {
        return allowedModes.has(mode) ? mode : 'system';
    }

    function readMode() {
        try {
            return normalizeMode(window.localStorage.getItem(storageKey));
        } catch {
            return 'system';
        }
    }

    function writeMode(mode) {
        try {
            localStorage.setItem(storageKey, normalizeMode(mode));
        } catch {
            return;
        }
    }

    function getSystemPreferenceQuery() {
        try {
            return window.matchMedia('(prefers-color-scheme: dark)');
        } catch {
            return null;
        }
    }

    function deriveEffectiveTheme(mode) {
        if (mode === 'dark') {
            return 'dark';
        }

        if (mode === 'light') {
            return 'light';
        }

        const systemPreferenceQuery = getSystemPreferenceQuery();
        return systemPreferenceQuery && systemPreferenceQuery.matches ? 'dark' : 'light';
    }

    function applyTheme(mode) {
        const normalizedMode = normalizeMode(mode);
        const focusedElement = document.activeElement;
        const root = document.documentElement;
        root.dataset.themeMode = normalizedMode;
        root.dataset.bsTheme = deriveEffectiveTheme(normalizedMode);
        if (focusedElement instanceof HTMLElement && document.contains(focusedElement)) {
            focusedElement.focus({ preventScroll: true });
        }

        return normalizedMode;
    }

    function syncThemeControl(mode) {
        const control = document.querySelector('[data-theme-mode-control]');
        if (!(control instanceof HTMLElement)) {
            return;
        }

        const normalizedMode = normalizeMode(mode);
        const radios = control.querySelectorAll('input[type="radio"][name="themeMode"]');
        radios.forEach(radio => {
            if (radio instanceof HTMLInputElement) {
                radio.checked = radio.value === normalizedMode;
            }
        });
    }

    let currentMode = applyTheme(readMode());
    syncThemeControl(currentMode);

    const systemPreferenceQuery = getSystemPreferenceQuery();
    function handleSystemPreferenceChange() {
        if (currentMode === 'system') {
            currentMode = applyTheme(currentMode);
            syncThemeControl(currentMode);
        }
    }

    if (systemPreferenceQuery && typeof systemPreferenceQuery.addEventListener === 'function') {
        systemPreferenceQuery.addEventListener('change', handleSystemPreferenceChange);
    } else if (systemPreferenceQuery && typeof systemPreferenceQuery.addListener === 'function') {
        systemPreferenceQuery.addListener(handleSystemPreferenceChange);
    }

    window.addEventListener('storage', event => {
        if (event.key !== storageKey) {
            return;
        }

        currentMode = applyTheme(readMode());
        syncThemeControl(currentMode);
    });

    const themeControl = document.querySelector('[data-theme-mode-control]');
    if (themeControl instanceof HTMLElement) {
        themeControl.addEventListener('change', event => {
            const target = event.target;
            if (!(target instanceof HTMLInputElement) || target.name !== 'themeMode') {
                return;
            }

            const selectedMode = normalizeMode(target.value);
            writeMode(selectedMode);
            currentMode = applyTheme(selectedMode);
            syncThemeControl(currentMode);
        });
    }

    const successAlerts = document.querySelectorAll('.alert-success.alert-dismissible');
    successAlerts.forEach(alert => {
        window.setTimeout(() => {
            if (window.bootstrap) {
                window.bootstrap.Alert.getOrCreateInstance(alert).close();
            }
        }, 4500);
    });

    const invalidField = document.querySelector('.input-validation-error, .is-invalid');
    if (invalidField instanceof HTMLElement) {
        invalidField.focus({ preventScroll: false });
    }
}());
