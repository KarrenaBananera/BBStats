(function () {
    function formatLocalDatetimes() {
        document.querySelectorAll('time.local-datetime[datetime]').forEach(function (element) {
            var value = element.getAttribute('datetime');
            if (!value) {
                return;
            }

            var date = new Date(value);
            if (Number.isNaN(date.getTime())) {
                return;
            }

            element.textContent = new Intl.DateTimeFormat(undefined, {
                dateStyle: 'short',
                timeStyle: 'short'
            }).format(date);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', formatLocalDatetimes);
    } else {
        formatLocalDatetimes();
    }
})();
