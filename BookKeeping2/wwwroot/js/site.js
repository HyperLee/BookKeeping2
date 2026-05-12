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
            window.localStorage.setItem(storageKey, mode);
        } catch {
            return;
        }
    }

    function deriveEffectiveTheme(mode) {
        return mode === 'dark' ? 'dark' : 'light';
    }

    function applyTheme(mode) {
        const normalizedMode = normalizeMode(mode);
        const root = document.documentElement;
        root.dataset.themeMode = normalizedMode;
        root.dataset.bsTheme = deriveEffectiveTheme(normalizedMode);
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

    const currentMode = applyTheme(readMode());
    syncThemeControl(currentMode);

    const themeControl = document.querySelector('[data-theme-mode-control]');
    if (themeControl instanceof HTMLElement) {
        themeControl.addEventListener('change', event => {
            const target = event.target;
            if (!(target instanceof HTMLInputElement) || target.name !== 'themeMode') {
                return;
            }

            const selectedMode = normalizeMode(target.value);
            writeMode(selectedMode);
            applyTheme(selectedMode);
            syncThemeControl(selectedMode);
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
