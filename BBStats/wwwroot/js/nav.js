(function () {
    function setDropdownOpen(dropdown, isOpen) {
        dropdown.classList.toggle('is-open', isOpen);

        var toggle = dropdown.querySelector('.nav-dropdown-toggle');
        if (toggle) {
            toggle.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        }
    }

    function closeAllDropdowns(except) {
        document.querySelectorAll('[data-nav-dropdown].is-open').forEach(function (dropdown) {
            if (dropdown !== except) {
                setDropdownOpen(dropdown, false);
            }
        });
    }

    document.querySelectorAll('[data-nav-dropdown]').forEach(function (dropdown) {
        var toggle = dropdown.querySelector('.nav-dropdown-toggle');
        if (!toggle) {
            return;
        }

        toggle.addEventListener('click', function (e) {
            e.stopPropagation();

            var willOpen = !dropdown.classList.contains('is-open');
            if (willOpen) {
                closeAllDropdowns(dropdown);
            }

            setDropdownOpen(dropdown, willOpen);
        });
    });

    document.addEventListener('click', function (e) {
        if (e.target.closest('[data-nav-dropdown]')) {
            return;
        }

        closeAllDropdowns();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Escape') {
            return;
        }

        closeAllDropdowns();
    });

    document.querySelectorAll('form.header-search').forEach(function (form) {
        form.addEventListener('submit', function (event) {
            var input = form.querySelector('input[name="q"]');
            if (!input) {
                return;
            }

            input.value = input.value.trim();
            if (!input.value) {
                event.preventDefault();
            }
        });
    });
})();
