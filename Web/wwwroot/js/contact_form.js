const form = document.getElementById('messageForm');
const statusBox = document.getElementById('contactStatus');

if (form && statusBox) {
    form.addEventListener('submit', async function (e) {
        e.preventDefault();

        // Сбрасываем прошлый статус
        statusBox.textContent = '';
        statusBox.className = 'form-status';

        // Сбрасываем подсветку ошибок полей
        form.querySelectorAll('.input-error').forEach(el => {
            el.classList.remove('input-error');
        });

        const payload = {
            name: form.querySelector('[name="Name"]').value,
            email: form.querySelector('[name="Email"]').value,
            message: form.querySelector('[name="Message"]').value
        };

        try {
            const res = await fetch('/Contacts/Send', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            const data = await res.json();

            // Валидация на сервере
            if (!data.success) {
                statusBox.classList.add('form-status--error');

                if (data.errors) {
                    // Подсветить поля с ошибками
                    Object.keys(data.errors).forEach(key => {
                        const input = form.querySelector(`[name="${key}"]`);
                        if (input) {
                            input.classList.add('input-error');
                        }
                    });

                    const firstError = Object.values(data.errors)[0];
                    statusBox.textContent = firstError || 'Исправьте ошибки в форме.';
                } else {
                    statusBox.textContent = 'Исправьте ошибки в форме.';
                }

                return;
            }

            // Успех
            statusBox.classList.add('form-status--ok');
            statusBox.textContent = data.message || 'Сообщение успешно отправлено.';

            form.reset();

            // Скрыть статус через 5 секунд (не обязательно)
            setTimeout(() => {
                statusBox.textContent = '';
                statusBox.className = 'form-status';
            }, 5000);

        } catch (err) {
            console.error(err);
            statusBox.classList.add('form-status--error');
            statusBox.textContent = 'Произошла ошибка при отправке. Попробуйте ещё раз позже.';
        }
    });
}
