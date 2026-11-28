(function () {
    document.addEventListener('click', function (event) {
        var button = event.target.closest('.match-series .series-summary');
        if (!button) return;

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

    var matchesContainer = document.getElementById('matches-container');
    var filterForm = document.getElementById('match-filters');
    if (!matchesContainer) {
        return;
    }

    var debounceMs = 1500;
    var debounceTimer = 0;
    var requestSeq = 0;
    var lastQueryKey = null;

    function fieldValue(name) {
        if (!filterForm) {
            return '';
        }

        var field = filterForm.elements.namedItem(name);
        if (!field || typeof field.value !== 'string') {
            return '';
        }

        return field.value.trim();
    }

    function readBound(name) {
        var raw = fieldValue(name);
        return /^-?\d+$/.test(raw) ? raw : '';
    }

    function filterParams(page) {
        var params = new URLSearchParams();
        var minElo = readBound('minElo');
        var maxElo = readBound('maxElo');
        var opponent = fieldValue('opponent');
        var opponentCharacter = fieldValue('opponentCharacter');

        if (minElo) params.set('minElo', minElo);
        if (maxElo) params.set('maxElo', maxElo);
        if (opponent) params.set('opponent', opponent);
        if (opponentCharacter) params.set('opponentCharacter', opponentCharacter);
        if (page > 1) params.set('matchPage', String(page));
        return params;
    }

    function baseMatchesPath() {
        var path = filterForm
            ? filterForm.getAttribute('data-matches-path')
            : window.location.pathname;
        path = (path || '').split('?')[0].replace(/\/$/, '');
        return path.replace(/\/\d+$/, '');
    }

    function matchesPath(page) {
        var path = baseMatchesPath();
        return page > 1 ? path + '/' + page : path;
    }

    function visibleUrl(params) {
        var page = Number(params.get('matchPage') || '1');
        var path = matchesPath(page > 0 ? page : 1);
        var visible = new URLSearchParams(params);
        visible.delete('matchPage');
        var qs = visible.toString();
        return qs ? path + '?' + qs : path;
    }

    function pageFromLink(link) {
        if (link.dataset.matchPage) {
            var fromData = Number(link.dataset.matchPage);
            if (fromData > 0) {
                return fromData;
            }
        }

        try {
            var parts = new URL(link.href, window.location.origin).pathname.split('/').filter(Boolean);
            var last = parts[parts.length - 1];
            var parsed = Number(last);
            if (String(parsed) === last && parsed > 0) {
                return parsed;
            }
        } catch (error) {
            return 1;
        }

        return 1;
    }

    async function loadMatches(page, fallbackUrl, scrollToList) {
        var params = filterParams(page);
        var key = params.toString();
        if (key === lastQueryKey && !scrollToList) {
            return;
        }

        var seq = ++requestSeq;
        var requestParams = new URLSearchParams(params);
        requestParams.delete('matchPage');
        requestParams.set('handler', 'Matches');
        var url = matchesPath(page) + '?' + requestParams.toString();

        try {
            matchesContainer.style.opacity = '0.5';
            var response = await fetch(url, { credentials: 'same-origin' });
            if (seq !== requestSeq) {
                return;
            }

            if (!response.ok) {
                throw new Error('Failed to load matches');
            }

            matchesContainer.innerHTML = await response.text();
            if (seq !== requestSeq) {
                return;
            }

            lastQueryKey = key;
            if (typeof window.formatLocalDatetimes === 'function') {
                window.formatLocalDatetimes(matchesContainer);
            }

            if (filterForm) {
                history.replaceState(null, '', visibleUrl(params));
            }

            matchesContainer.style.opacity = '1';
            if (scrollToList) {
                var block = matchesContainer.closest('.secondLevelDiv');
                if (block) {
                    block.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }
        } catch (error) {
            if (seq !== requestSeq) {
                return;
            }

            matchesContainer.style.opacity = '1';
            if (fallbackUrl) {
                window.location.href = fallbackUrl;
            }
        }
    }

    matchesContainer.addEventListener('click', function (event) {
        var link = event.target.closest('.pagination-container a');
        if (!link) return;

        event.preventDefault();
        window.clearTimeout(debounceTimer);
        loadMatches(pageFromLink(link), link.href, true);
    });

    if (!filterForm) {
        return;
    }

    var initialPage = Number(filterForm.getAttribute('data-current-page') || '1');
    lastQueryKey = filterParams(initialPage > 0 ? initialPage : 1).toString();

    function scheduleFilter() {
        window.clearTimeout(debounceTimer);
        debounceTimer = window.setTimeout(function () {
            loadMatches(1, visibleUrl(filterParams(1)), false);
        }, debounceMs);
    }

    filterForm.addEventListener('input', function (event) {
        if (event.target && event.target.name === 'opponentCharacter') {
            return;
        }

        scheduleFilter();
    });

    filterForm.addEventListener('search', function (event) {
        if (event.target && event.target.name === 'opponent') {
            scheduleFilter();
        }
    });

    filterForm.addEventListener('change', function (event) {
        if (!event.target || event.target.name !== 'opponentCharacter') {
            return;
        }

        window.clearTimeout(debounceTimer);
        loadMatches(1, visibleUrl(filterParams(1)), false);
    });

    filterForm.addEventListener('submit', function (event) {
        event.preventDefault();
        window.clearTimeout(debounceTimer);
        loadMatches(1, visibleUrl(filterParams(1)), false);
    });

    filterForm.addEventListener('wheel', function (event) {
        if (event.target && event.target.type === 'number' && document.activeElement === event.target) {
            event.preventDefault();
            event.target.blur();
        }
    });
})();
