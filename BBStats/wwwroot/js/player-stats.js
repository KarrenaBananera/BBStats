(function () {
    function initProfileStatsPanels(root) {
        (root || document).querySelectorAll('.profile-stats-panel').forEach(function (panel) {
            var button = panel.querySelector('.profile-stats-summary');
            if (!button || button.dataset.bound === 'true') {
                return;
            }

            button.dataset.bound = 'true';
            button.addEventListener('click', function () {
                panel.classList.toggle('expanded');
                button.setAttribute(
                    'aria-expanded',
                    panel.classList.contains('expanded') ? 'true' : 'false');
            });
        });
    }

    var container = document.getElementById('player-character-stats');
    if (!container) {
        return;
    }

    var url = container.getAttribute('data-stats-url');
    if (!url) {
        return;
    }

    fetch(url, { credentials: 'same-origin' })
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Failed to load player statistics.');
            }

            return response.text();
        })
        .then(function (html) {
            container.innerHTML = html;
            initProfileStatsPanels(container);
        })
        .catch(function () {
            container.remove();
        });
})();
