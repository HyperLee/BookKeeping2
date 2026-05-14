(function () {
  const forms = document.querySelectorAll('form[data-disable-on-submit="true"]');
  forms.forEach((form) => {
    form.addEventListener('submit', () => {
      const submitter = form.querySelector('button[type="submit"]');
      if (submitter) {
        submitter.disabled = true;
      }
    });
  });

  const transactionForms = document.querySelectorAll('[data-transaction-form]');
  transactionForms.forEach((form) => {
    const currencySelect = form.querySelector('[data-currency-select]');
    const accountSelect = form.querySelector('[data-account-select]');
    if (!currencySelect || !accountSelect) {
      return;
    }

    const filterAccounts = () => {
      const selectedCurrency = currencySelect.value;
      accountSelect.querySelectorAll('option[data-currency]').forEach((option) => {
        const isMatch = !selectedCurrency || option.dataset.currency === selectedCurrency;
        option.hidden = !isMatch;
        option.disabled = !isMatch && !option.selected;
      });

      const selectedOption = accountSelect.selectedOptions[0];
      if (selectedOption && selectedOption.dataset.currency && selectedOption.dataset.currency !== selectedCurrency) {
        accountSelect.value = '';
      }
    };

    currencySelect.addEventListener('change', filterAccounts);
    filterAccounts();
  });
})();
