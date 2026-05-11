(function () {
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
