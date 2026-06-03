(function () {
    document.querySelectorAll('.match-series .series-summary').forEach(function (button) {
        button.addEventListener('click', function () {
            var series = button.closest('.match-series');
            if (series) {
                series.classList.toggle('expanded');
            }
        });
    });
})();
