document.getElementById('messageForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const payload = {
        name: this.querySelector('[name="Name"]').value,
        email: this.querySelector('[name="Email"]').value,
        message: this.querySelector('[name="Message"]').value
    };

    const res = await fetch('/Contacts/Send', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    const data = await res.json();

    if (!data.success) {
        console.log('Ошибки валидации:', data.errors);
        // тут, если захочешь, можно раскидать ошибки по полям
        return;
    }

    // тут success === true
    let text = data.message || 'Ваше сообщение успешно отправлено.';

    if (data.userEmailSent === false) {
        text += '\n\nОднако не удалось отправить письмо на указанный вами email. ' +
            'Возможно, адрес введён с ошибкой или не существует.';
    } else {
        text += '\n\nНа вашу почту отправлено письмо-подтверждение.';
    }

    alert(text);
    this.reset();
});
