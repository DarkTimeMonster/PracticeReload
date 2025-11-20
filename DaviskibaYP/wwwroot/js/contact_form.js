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
        // здесь дальше можешь раскидать ошибки по span-ам
        return;
    }

    alert(data.message);
    this.reset();
});
