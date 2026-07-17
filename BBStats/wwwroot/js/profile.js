(function () {
    // Accordion toggle using event delegation
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

    // AJAX Pagination for matches
    var matchesContainer = document.getElementById('matches-container');
    if (matchesContainer) {
        matchesContainer.addEventListener('click', async function (event) {
            var link = event.target.closest('.pagination-container a');
            if (!link) return;
            
            event.preventDefault();
            var url = link.href;
            url += (url.includes('?') ? '&' : '?') + 'handler=Matches';
            
            try {
                matchesContainer.style.opacity = '0.5'; // Optional loading state
                var response = await fetch(url);
                if (response.ok) {
                    var html = await response.text();
                    matchesContainer.innerHTML = html;
                    matchesContainer.style.opacity = '1';
                    // Scroll to top of matches container
                    matchesContainer.closest('.secondLevelDiv').scrollIntoView({ behavior: 'smooth', block: 'start' });
                } else {
                    window.location.href = link.href; // Fallback
                }
            } catch (error) {
                console.error('Failed to load matches:', error);
                window.location.href = link.href; // Fallback
            }
        });
    }
})();
