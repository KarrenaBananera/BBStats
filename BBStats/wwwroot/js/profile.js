(function () {
    document.querySelectorAll('.match-series .series-summary').forEach(function (button) {
        button.addEventListener('click', function (event) {
            if (event.target.closest('a')) {
                return;
            }

            var series = button.closest('.match-series');
            if (series) {
                series.classList.toggle('expanded');
                button.setAttribute(
                    'aria-expanded',
                    series.classList.contains('expanded') ? 'true' : 'false');
            }
        });
    });
})();
