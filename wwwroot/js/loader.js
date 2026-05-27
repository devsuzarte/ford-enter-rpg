(function () {
    var o = document.getElementById('loader');
    if (!o) return;
    function show() { o.style.display = 'flex'; }
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form.dataset.noLoader) return;
        show();
    });
    document.addEventListener('click', function (e) {
        if (e.target.id === 'btn-prev' || e.target.id === 'btn-next') return;
        var a = e.target.closest('a[href]');
        if (!a) return;
        var h = a.getAttribute('href') || '';
        if (!h || h[0] === '#' || /^(https?:|mailto:)/.test(h)) return;
        show();
    });
})();
