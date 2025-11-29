window.addEventListener('DOMContentLoaded', function () {
    const grid = document.getElementById('festivals-grid');
    const paginationList = document.getElementById('festivals-pagination');

    const searchInput = document.getElementById('filter-search');
    const citySelect = document.getElementById('filter-city');
    const dateSelect = document.getElementById('filter-date');

    const config = window.gastroFestCatalogConfig || {};
    const isAdmin = !!config.isAdmin; // флаг админа из Razor

    if (!grid) {
        console.error('Не найден контейнер #festivals-grid');
        return;
    }

    // Элементы модалки
    const modal = document.getElementById('festival-delete-modal');
    const modalTitle = document.getElementById('festival-delete-modal-title');
    const modalConfirm = document.getElementById('festival-delete-modal-confirm');
    const modalCancel = document.getElementById('festival-delete-modal-cancel');
    let modalFormToSubmit = null;

    // Данные и пагинация
    let allFestivals = [];
    let filteredFestivals = [];
    let timerId = null;
    let currentPage = 1;
    const pageSize = 6; // сколько карточек на странице

    // ===== Загрузка данных один раз =====
    fetch('/Festivals/GetAllJson')
        .then(function (r) {
            if (!r.ok) {
                throw new Error('HTTP ' + r.status);
            }
            return r.json();
        })
        .then(function (data) {
            allFestivals = data || [];
            applyFilters(); // первый рендер
        })
        .catch(function (err) {
            console.error('Ошибка при загрузке фестивалей', err);
            grid.innerHTML = '<p>Ошибка загрузки фестивалей.</p>';
        });

    // ===== Применение фильтров =====
    function applyFilters() {
        if (!allFestivals || allFestivals.length === 0) {
            grid.innerHTML = '<p>Фестивали не найдены.</p>';
            if (paginationList) paginationList.innerHTML = '';
            return;
        }

        const term = (searchInput && searchInput.value || '').toLowerCase().trim();
        const city = (citySelect && citySelect.value) || '';
        const dateFilter = (dateSelect && dateSelect.value) || '';

        const now = new Date();
        const firstCurrentMonth = new Date(now.getFullYear(), now.getMonth(), 1);
        const firstNextMonth = new Date(now.getFullYear(), now.getMonth() + 1, 1);
        const firstAfterNext = new Date(now.getFullYear(), now.getMonth() + 2, 1);

        filteredFestivals = allFestivals.filter(function (f) {
            // поиск по названию
            if (term && !(f.title || '').toLowerCase().includes(term)) {
                return false;
            }

            // фильтр по городу
            if (city === 'Москва') {
                if (f.city !== 'Москва') return false;
            } else if (city === 'Санкт-Петербург') {
                if (f.city !== 'Санкт-Петербург') return false;
            } else if (city === 'other') {
                if (f.city === 'Москва' || f.city === 'Санкт-Петербург') return false;
            }

            // фильтр по датам
            if (dateFilter) {
                const start = new Date(f.startDate);

                if (dateFilter === 'currentMonth') {
                    if (!(start >= firstCurrentMonth && start < firstNextMonth)) {
                        return false;
                    }
                } else if (dateFilter === 'nextMonth') {
                    if (!(start >= firstNextMonth && start < firstAfterNext)) {
                        return false;
                    }
                }
            }

            return true;
        });

        currentPage = 1;
        renderCurrentPage();
    }

    // Отрисовать текущую страницу и пагинацию
    function renderCurrentPage() {
        if (!filteredFestivals || filteredFestivals.length === 0) {
            grid.innerHTML = '<p>По заданным условиям фестивали не найдены.</p>';
            if (paginationList) paginationList.innerHTML = '';
            return;
        }

        const totalItems = filteredFestivals.length;
        const totalPages = Math.ceil(totalItems / pageSize);

        if (currentPage > totalPages) {
            currentPage = totalPages;
        }

        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = startIndex + pageSize;
        const itemsForPage = filteredFestivals.slice(startIndex, endIndex);

        renderFestivals(itemsForPage);
        renderPagination(totalPages);
    }

    // ===== Рендер карточек =====
    function renderFestivals(items) {
        if (!items || items.length === 0) {
            grid.innerHTML = '<p>По заданным условиям фестивали не найдены.</p>';
            return;
        }

        const html = items.map(function (f) {
            const cityCountry = [f.city, f.country].filter(Boolean).join(', ');
            const start = new Date(f.startDate);
            const end = new Date(f.endDate);

            const formatDate = function (d) {
                return d.toLocaleDateString('ru-RU', {
                    day: '2-digit',
                    month: '2-digit',
                    year: 'numeric'
                });
            };

            return (
                '<article class="festival-card">' +
                '  <a href="/Festivals/Details/' + f.id + '" class="festival-card__link">' +
                (f.coverUrl
                    ? '    <div class="festival-card__img">' +
                    '      <img src="' + encodeHtmlAttr(f.coverUrl) + '" alt="' + escapeHtml(f.title || '') + '">' +
                    '    </div>'
                    : '') +
                '    <div class="festival-card__body">' +
                '      <h3 class="festival-card__title">' + escapeHtml(f.title || '') + '</h3>' +
                (cityCountry
                    ? '      <p class="festival-card__city">' + escapeHtml(cityCountry) + '</p>'
                    : '') +
                '      <p class="festival-card__dates">' +
                '        ' + formatDate(start) + ' — ' + formatDate(end) +
                '      </p>' +
                (f.description
                    ? '      <p class="festival-card__desc">' + escapeHtml(f.description) + '</p>'
                    : '') +
                '    </div>' +
                '  </a>' +
                (isAdmin
                    ? '  <div class="festival-card__admin" data-id="' + f.id +
                    '" data-title="' + escapeHtml(f.title || '') + '">' +
                    '    <a href="/Festivals/Edit/' + f.id +
                    '" class="btn btn-outline btn-sm">Редактировать</a>' +
                    '  </div>'
                    : '') +
                '</article>'
            );
        }).join('');

        grid.innerHTML = html;
        setupAdminDeleteForms();
    }

    // ===== Рендер пагинации =====
    function renderPagination(totalPages) {
        if (!paginationList) return;

        if (totalPages <= 1) {
            paginationList.innerHTML = '';
            return;
        }

        let html = '';

        // Назад
        if (currentPage > 1) {
            html +=
                '<li class="pagination__item">' +
                '  <a href="#" class="pagination__link pagination__link--prev" data-page="' + (currentPage - 1) + '">← Назад</a>' +
                '</li>';
        }

        for (let i = 1; i <= totalPages; i++) {
            html +=
                '<li class="pagination__item ' + (i === currentPage ? 'pagination__item--active' : '') + '">' +
                '  <a href="#" class="pagination__link" data-page="' + i + '">' + i + '</a>' +
                '</li>';
        }

        // Вперёд
        if (currentPage < totalPages) {
            html +=
                '<li class="pagination__item">' +
                '  <a href="#" class="pagination__link pagination__link--next" data-page="' + (currentPage + 1) + '">Вперёд →</a>' +
                '</li>';
        }

        paginationList.innerHTML = html;
    }


    if (paginationList) {
        paginationList.addEventListener('click', function (e) {
            const link = e.target.closest('a.pagination__link');
            if (!link) return;

            e.preventDefault();

            const page = parseInt(link.getAttribute('data-page'), 10);
            if (!isNaN(page) && page !== currentPage) {
                currentPage = page;
                renderCurrentPage();
            }
        });
    }

    // ===== Вставка форм удаления и модалка =====
    function setupAdminDeleteForms() {
        if (!isAdmin) return;

        const template = document.getElementById('festival-delete-form-template');
        if (!template) return;

        const adminBlocks = grid.querySelectorAll('.festival-card__admin[data-id]');
        adminBlocks.forEach(function (block) {
            const id = block.getAttribute('data-id');

            const form = template.cloneNode(true);
            form.removeAttribute('id');
            form.style.display = 'inline';
            form.action = '/Festivals/Delete/' + id;

            const btn = form.querySelector('button');
            if (btn) {
                btn.textContent = 'Удалить';
                btn.className = 'btn btn-outline btn-sm js-btn-delete';
            }

            block.appendChild(form);
        });
    }

    function openDeleteModal(title, form) {
        if (!modal) {
            if (form && confirm('Удалить фестиваль «' + title + '»?')) {
                form.submit();
            }
            return;
        }

        modalFormToSubmit = form || null;
        if (modalTitle) {
            modalTitle.textContent = title || '';
        }
        modal.classList.add('is-open');
    }

    function closeDeleteModal() {
        if (!modal) return;
        modal.classList.remove('is-open');
        modalFormToSubmit = null;
    }

    if (isAdmin && grid) {
        grid.addEventListener('click', function (e) {
            const btn = e.target.closest('.js-btn-delete');
            if (!btn) return;

            e.preventDefault();

            const adminBlock = btn.closest('.festival-card__admin');
            if (!adminBlock) return;

            const form = adminBlock.querySelector('form');
            const title = adminBlock.getAttribute('data-title') || '';

            openDeleteModal(title, form);
        });
    }

    if (modalCancel) {
        modalCancel.addEventListener('click', function () {
            closeDeleteModal();
        });
    }

    if (modalConfirm) {
        modalConfirm.addEventListener('click', function () {
            if (modalFormToSubmit) {
                modalFormToSubmit.submit();
            }
        });
    }

    if (modal) {
        modal.addEventListener('click', function (e) {
            if (e.target === modal) {
                closeDeleteModal();
            }
        });
    }

    // ===== Хелперы =====
    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function encodeHtmlAttr(str) {
        return escapeHtml(str);
    }

    // ===== События фильтров =====
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            if (timerId) clearTimeout(timerId);
            timerId = setTimeout(applyFilters, 300);
        });
    }

    if (citySelect) {
        citySelect.addEventListener('change', applyFilters);
    }

    if (dateSelect) {
        dateSelect.addEventListener('change', applyFilters);
    }
});
