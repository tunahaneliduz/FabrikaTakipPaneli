// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    var toggle = document.getElementById('themeToggle');
    if (!toggle) {
        return;
    }

    toggle.addEventListener('click', function () {
        var current = document.documentElement.getAttribute('data-bs-theme');
        var next = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-bs-theme', next);
        localStorage.setItem('theme', next);
    });
})();

(function () {
    var sidebar = document.getElementById('appSidebar');
    var trigger = document.getElementById('sidebarTrigger');
    var closeButton = document.getElementById('sidebarClose');
    var backdrop = document.getElementById('sidebarBackdrop');

    if (!sidebar || !trigger || !backdrop) {
        return;
    }

    function setSidebar(open) {
        document.body.classList.toggle('sidebar-open', open);
        trigger.setAttribute('aria-expanded', open ? 'true' : 'false');
        backdrop.setAttribute('aria-hidden', open ? 'false' : 'true');
    }

    trigger.addEventListener('click', function () { setSidebar(true); });
    backdrop.addEventListener('click', function () { setSidebar(false); });
    if (closeButton) {
        closeButton.addEventListener('click', function () { setSidebar(false); });
    }

    sidebar.addEventListener('click', function (event) {
        if (event.target.closest('a') && window.innerWidth < 992) {
            setSidebar(false);
        }
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            setSidebar(false);
        }
    });
})();
