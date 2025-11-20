(function () {
 
    const el = document.getElementById('map');
    if (!el) return;
    if (typeof L === 'undefined') return;

    const lat = parseFloat(el.dataset.lat || '55.7558');
    const lng = parseFloat(el.dataset.lng || '37.6176');
    const zoom = parseInt(el.dataset.zoom || '12', 10);

    if (!el.style.height) el.style.height = '360px';

    const map = L.map(el, { scrollWheelZoom: false }).setView([lat, lng], zoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    L.marker([lat, lng]).addTo(map).bindPopup('GastroFest').openPopup();

    setTimeout(() => map.invalidateSize(), 300);
})();
