// Header compact + burger + корректная высота шапки (без перехвата якорей)
(function () {
    const headerBar = document.getElementById('headerBar');
    const headerSpace = document.getElementById('headerSpacer');
    const toggle = document.getElementById('navToggle');
    const menu = document.getElementById('mainNav');
    if (!headerBar) return;

    const headerH = () => headerBar.offsetHeight || 80;
    function syncHeader() {
        const h = headerH();
        document.documentElement.style.setProperty('--header-height', h + 'px');
        if (headerSpace) headerSpace.style.height = h + 'px';
    }

    // компакт-тень при скролле
    let compact = false;
    function onScroll() {
        const y = window.scrollY || 0;
        const next = y > 48;
        if (next !== compact) {
            compact = next;
            headerBar.classList.toggle('is-compact', compact);
            syncHeader();
        }
    }

    // бургер
    if (toggle && menu) {
        toggle.addEventListener('click', () => {
            const opened = menu.classList.toggle('is-open');
            toggle.setAttribute('aria-expanded', String(opened));
        });
        // закрывать меню после выбора пункта на мобилке
        menu.querySelectorAll('a').forEach(a => {
            a.addEventListener('click', () => {
                if (window.matchMedia('(max-width: 991.98px)').matches) {
                    menu.classList.remove('is-open');
                    toggle.setAttribute('aria-expanded', 'false');
                }
            });
        });
    }

    // init
    function init() { syncHeader(); onScroll(); }
    document.addEventListener('DOMContentLoaded', init);
    window.addEventListener('load', init);
    window.addEventListener('resize', syncHeader);
    window.addEventListener('scroll', onScroll, { passive: true });
})();
