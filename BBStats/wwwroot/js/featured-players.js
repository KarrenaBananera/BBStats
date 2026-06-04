(function () {
    var STORAGE_KEY = 'bbstats.featuredPlayers';

    function loadFeaturedPlayers() {
        try {
            var raw = localStorage.getItem(STORAGE_KEY);
            if (!raw) {
                return [];
            }

            var parsed = JSON.parse(raw);
            if (!Array.isArray(parsed)) {
                return [];
            }

            return parsed.filter(function (player) {
                return player
                    && typeof player.steamId === 'string'
                    && player.steamId.length > 0
                    && typeof player.name === 'string'
                    && player.name.length > 0;
            });
        } catch {
            return [];
        }
    }

    function saveFeaturedPlayers(players) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(players));
        window.dispatchEvent(new CustomEvent('bbstats:featured-players-changed'));
    }

    function isFeatured(steamId) {
        return loadFeaturedPlayers().some(function (player) {
            return player.steamId === steamId;
        });
    }

    function addFeatured(steamId, name) {
        var players = loadFeaturedPlayers().filter(function (player) {
            return player.steamId !== steamId;
        });

        players.unshift({ steamId: steamId, name: name });
        saveFeaturedPlayers(players);
    }

    function removeFeatured(steamId) {
        saveFeaturedPlayers(loadFeaturedPlayers().filter(function (player) {
            return player.steamId !== steamId;
        }));
    }

    function updateStarButton(button, featured) {
        button.classList.toggle('is-featured', featured);
        button.setAttribute(
            'aria-label',
            featured ? 'Remove from featured players' : 'Save as featured player');
        button.title = featured
            ? 'Remove from featured players'
            : 'Save as featured player';

        var icon = button.querySelector('i');
        if (icon) {
            icon.classList.toggle('bi-star', !featured);
            icon.classList.toggle('bi-star-fill', featured);
        }
    }

    function initProfileStar() {
        var button = document.querySelector('.featured-player-toggle');
        if (!button) {
            return;
        }

        var steamId = button.getAttribute('data-steam-id');
        var playerName = button.getAttribute('data-player-name');
        if (!steamId || !playerName) {
            return;
        }

        updateStarButton(button, isFeatured(steamId));

        button.addEventListener('click', function () {
            if (isFeatured(steamId)) {
                removeFeatured(steamId);
                updateStarButton(button, false);
            } else {
                addFeatured(steamId, playerName);
                updateStarButton(button, true);
            }
        });
    }

    function renderFeaturedSection() {
        var section = document.getElementById('featured-players-section');
        var list = document.getElementById('featured-players-list');
        if (!section || !list) {
            return;
        }

        var players = loadFeaturedPlayers();
        list.innerHTML = '';

        if (players.length === 0) {
            section.hidden = true;
            return;
        }

        section.hidden = false;

        players.forEach(function (player) {
            var link = document.createElement('a');
            link.href = '/Search?q=' + encodeURIComponent(player.steamId);
            link.className = 'featured-player-link top-player-link';
            link.textContent = player.name;
            list.appendChild(link);
        });
    }

    function refreshFeaturedUi() {
        renderFeaturedSection();

        var button = document.querySelector('.featured-player-toggle');
        if (!button) {
            return;
        }

        var steamId = button.getAttribute('data-steam-id');
        if (steamId) {
            updateStarButton(button, isFeatured(steamId));
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initProfileStar();
        renderFeaturedSection();
    });

    window.addEventListener('storage', function (event) {
        if (event.key === STORAGE_KEY) {
            refreshFeaturedUi();
        }
    });

    window.addEventListener('bbstats:featured-players-changed', renderFeaturedSection);
})();
