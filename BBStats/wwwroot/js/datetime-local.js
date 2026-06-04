(function () {
    function parseUtcDate(value) {
        var trimmed = value.trim();
        if (!trimmed) {
            return null;
        }

        if (trimmed.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(trimmed)) {
            return new Date(trimmed);
        }

        return new Date(trimmed + 'Z');
    }

    function formatLocalDatetimes(root) {
        (root || document).querySelectorAll('time.local-datetime[datetime]').forEach(function (element) {
            var value = element.getAttribute('datetime');
            if (!value) {
                return;
            }

            var date = parseUtcDate(value);
            if (!date || Number.isNaN(date.getTime())) {
                return;
            }

            element.textContent = new Intl.DateTimeFormat(undefined, {
                dateStyle: 'short',
                timeStyle: 'short'
            }).format(date);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function () {
            formatLocalDatetimes();
        });
    } else {
        formatLocalDatetimes();
    }
})();
