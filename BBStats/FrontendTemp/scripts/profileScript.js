(function () {
    document.querySelectorAll('.match-series .series-summary').forEach(function (button) {
        button.addEventListener('click', function () {
            var series = button.closest('.match-series');
            if (series) {
                series.classList.toggle('expanded');
            }
        });
    });

    document.querySelectorAll('form[data-profile-search]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var target = form.getAttribute('data-profile-search');
            if (target) {
                window.location.href = target;
            }
        });
    });
})();
