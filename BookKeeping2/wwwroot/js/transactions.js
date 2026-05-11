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
})();
