(function () {
    document.querySelectorAll('.match-series .series-summary').forEach(function (button) {
        button.addEventListener('click', function () {
            var series = button.closest('.match-series');
            if (series) {
                series.classList.toggle('expanded');
            }
        });
    });

    document.querySelectorAll('.series-download-set').forEach(function (link) {
        link.addEventListener('click', function (event) {
            event.preventDefault();
            event.stopPropagation();

            var raw = link.getAttribute('data-replay-urls');
            if (!raw) {
                return;
            }

            var urls = raw.split('|').filter(Boolean);
            urls.forEach(function (url, index) {
                window.setTimeout(function () {
                    var link = document.createElement('a');
                    link.href = url;
                    link.download = '';
                    link.rel = 'noopener';
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, index * 400);
            });
        });
    });
})();
