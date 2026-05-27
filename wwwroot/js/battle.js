(function () {
    // ── Turn carousel ──────────────────────────────────────────────────────────
    var slides = document.querySelectorAll('.turn-slide');
    var total  = slides.length;
    if (!total) return;
    var cur = total - 1;

    var btnPrev = document.getElementById('btn-prev');
    var btnNext = document.getElementById('btn-next');
    var indic   = document.getElementById('turn-indicator');

    function revealEvents(slide) {
        var rows = slide.querySelectorAll('.ev-row');
        rows.forEach(function (r) {
            r.classList.remove('ev-visible');
            r.style.transitionDelay = '0s';
        });
        rows.forEach(function (r, i) {
            setTimeout(function () {
                r.style.transitionDelay = '0s';
                r.classList.add('ev-visible');
            }, i * 380);
        });
    }

    function show(i) {
        slides.forEach(function (s) { s.style.display = 'none'; });
        slides[i].style.display = 'block';
        if (btnPrev) btnPrev.disabled = (i === 0);
        if (btnNext) btnNext.disabled = (i === total - 1);
        if (indic) indic.textContent = 'Turno ' + slides[i].dataset.turn + ' / ' + total;
        revealEvents(slides[i]);
    }

    if (btnPrev) {
        btnPrev.addEventListener('click', function (e) {
            e.stopPropagation();
            if (cur > 0) show(--cur);
        });
    }
    if (btnNext) {
        btnNext.addEventListener('click', function (e) {
            e.stopPropagation();
            if (cur < total - 1) show(++cur);
        });
    }

    show(cur);
})();
