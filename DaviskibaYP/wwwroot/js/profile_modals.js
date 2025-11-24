(function () {
    const body = document.body;

    const editBtn = document.getElementById('js-open-edit');
    const passBtn = document.getElementById('js-open-password');

    const editModal = document.getElementById('profileEditModal');
    const passModal = document.getElementById('profilePasswordModal');

    function openModal(modal) {
        if (!modal) return;
        modal.classList.add('profile-modal--open');
        body.classList.add('profile-modal-open');
    }

    function closeModal(modal) {
        if (!modal) return;
        modal.classList.remove('profile-modal--open');
        body.classList.remove('profile-modal-open');
    }

    if (editBtn && editModal) {
        editBtn.addEventListener('click', function () {
            openModal(editModal);
        });
    }

    if (passBtn && passModal) {
        passBtn.addEventListener('click', function () {
            openModal(passModal);
        });
    }

    // Закрытие по крестику и по клику на фон
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('js-profile-modal-close')) {
            const modal = e.target.closest('.profile-modal');
            closeModal(modal);
        }
    });

    // Закрытие по Esc
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            if (editModal && editModal.classList.contains('profile-modal--open')) {
                closeModal(editModal);
            }
            if (passModal && passModal.classList.contains('profile-modal--open')) {
                closeModal(passModal);
            }
        }
    });
})();
