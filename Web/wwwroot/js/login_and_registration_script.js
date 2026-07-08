// Вкладки модалки (переключение "Войти"/"Зарегистрироваться")
(function () {
    const modal = document.getElementById('authModal');
    if (!modal) return;

    const tabs = modal.querySelectorAll('.tab-btn');
    const panels = modal.querySelectorAll('.auth-panel');

    function switchTab(key) {
        tabs.forEach(b => {
            const on = b.dataset.tab === key;
            b.classList.toggle('active', on);
            b.setAttribute('aria-selected', on ? 'true' : 'false');
        });
        panels.forEach(p => p.classList.toggle('active', p.id === `auth-${key}`));
    }

    tabs.forEach(b => b.addEventListener('click', () => switchTab(b.dataset.tab)));
})();

// Вспомогательные утилиты
async function postJson(url, data) {
    const resp = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    return await resp.json();
}

function showErrors(container, errorsObj) {
    if (!container) return;
    container.innerHTML = '';
    if (!errorsObj || Object.keys(errorsObj).length === 0) {
        container.style.display = 'none';
        return;
    }
    const ul = document.createElement('ul');
    for (const key in errorsObj) {
        const li = document.createElement('li');
        li.textContent = errorsObj[key];
        ul.appendChild(li);
    }
    container.appendChild(ul);
    container.style.display = 'block';
}

function closeAuthModal() {
    const modal = document.getElementById('authModal');
    if (!modal) return;
    modal.classList.remove('open');
    document.body.classList.remove('noscroll');
}

// Открыть модалку по .js-open-auth
(function () {
    const modal = document.getElementById('authModal');
    if (!modal) return;

    const openBtns = document.querySelectorAll('.js-open-auth');
    const closeBtns = modal.querySelectorAll('.js-modal-close');

    openBtns.forEach(b => b.addEventListener('click', e => {
        e.preventDefault();
        modal.classList.add('open');
        document.body.classList.add('noscroll');
    }));

    closeBtns.forEach(b => b.addEventListener('click', closeAuthModal));
    modal.addEventListener('click', e => {
        if (e.target.classList.contains('modal-backdrop')) closeAuthModal();
    });
    document.addEventListener('keydown', e => {
        if (e.key === 'Escape' && modal.classList.contains('open')) closeAuthModal();
    });
})();

// Submit: LOGIN
const loginForm = document.getElementById('auth-login');
if (loginForm) {
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const payload = {
            Email: loginForm.querySelector('[name="Email"]').value,
            Password: loginForm.querySelector('[name="Password"]').value
        };

        const res = await postJson('/Account/Login', payload);
        const errorsBox = document.getElementById('loginErrors');
        errorsBox.innerHTML = '';

        if (!res || res.success !== true) {
            if (res && res.errors) {
                for (const msg of Object.values(res.errors)) {
                    const p = document.createElement('p');
                    p.textContent = msg;
                    errorsBox.appendChild(p);
                }
            } else {
                const p = document.createElement('p');
                p.textContent = 'Ошибка входа';
                errorsBox.appendChild(p);
            }
            return;
        }

        // успех — просто обновляем страницу
        window.location.reload();
    });
}
const registerForm = document.getElementById('auth-register');
if (registerForm) {
    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const payload = {
            Name: registerForm.querySelector('[name="Name"]').value,
            Email: registerForm.querySelector('[name="Email"]').value,
            Password: registerForm.querySelector('[name="Password"]').value,
            ConfirmPassword: registerForm.querySelector('[name="ConfirmPassword"]').value
        };

        const res = await postJson('/Account/Register', payload);
        const errorsBox = document.getElementById('registerErrors');
        errorsBox.innerHTML = '';

        if (!res || res.success !== true) {
            if (res && res.errors) {
                for (const msg of Object.values(res.errors)) {
                    const p = document.createElement('p');
                    p.textContent = msg;
                    errorsBox.appendChild(p);
                }
            } else {
                const p = document.createElement('p');
                p.textContent = 'Ошибка регистрации';
                errorsBox.appendChild(p);
            }
            return;
        }

        // успех — пользователь уже залогинен (SignInAsync в контроллере)
        window.location.reload();
    });
}