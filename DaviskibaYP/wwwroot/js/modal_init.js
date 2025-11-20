(function () {
    const modal = document.getElementById('authModal');
    if (!modal) return;

    // Кнопки, которые открывают модалку (в шапке и мобильном меню)
    const openBtns = document.querySelectorAll('.js-open-auth');

    // Кнопки закрытия (фон + крестик)
    const closeBtns = modal.querySelectorAll('.js-modal-close');

    // Табы и панели
    const tabButtons = modal.querySelectorAll('.tab-btn');
    const loginPanel = modal.querySelector('#auth-login');
    const registerPanel = modal.querySelector('#auth-register');

    function switchTab(tab) {
        // таб-кнопки
        tabButtons.forEach(btn => {
            const isActive = btn.dataset.tab === tab;
            btn.classList.toggle('active', isActive);
            btn.setAttribute('aria-selected', isActive ? 'true' : 'false');
        });

        if (!loginPanel || !registerPanel) return;

        const isLogin = tab === 'login';

        loginPanel.classList.toggle('active', isLogin);
        loginPanel.setAttribute('aria-hidden', isLogin ? 'false' : 'true');

        registerPanel.classList.toggle('active', !isLogin);
        registerPanel.setAttribute('aria-hidden', !isLogin ? 'false' : 'true');
    }

    function openModal(initialTab) {
        modal.classList.add('open');
        modal.setAttribute('aria-hidden', 'false'); // убираем warning
        document.body.classList.add('noscroll');

        switchTab(initialTab || 'login');
    }

    function closeModal() {
        modal.classList.remove('open');
        modal.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('noscroll');
    }

    // Открытие модалки с нужным табом (login / register)
    openBtns.forEach(btn => {
        btn.addEventListener('click', e => {
            e.preventDefault();
            const tab = btn.dataset.authTab || 'login';
            openModal(tab);
        });
    });

    // Переключение табов внутри модалки
    tabButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            const tab = btn.dataset.tab || 'login';
            switchTab(tab);
        });
    });

    // Закрытие по крестику и клику по фону
    closeBtns.forEach(btn => {
        btn.addEventListener('click', e => {
            e.preventDefault();
            closeModal();
        });
    });

    modal.addEventListener('click', e => {
        if (e.target.classList.contains('modal-backdrop')) {
            closeModal();
        }
    });

    // Закрытие по Esc
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape' && modal.classList.contains('open')) {
            closeModal();
        }
    });
})();
