/* ── FordEnterRPG — Page Transition Overlay ──
   Injects a full-screen dark overlay that:
   • Reveals content on page load  (slides right, gold leading edge)
   • Covers screen on navigation   (slides in from right, then navigates)
   Works on every page just by including this script.
*/
(function () {

    /* ── Inject styles ── */
    var css = [
        '#pt{position:fixed;inset:0;z-index:99999;pointer-events:none;background:#050402;',
        'will-change:transform;animation:ptReveal 1.1s cubic-bezier(.22,1,.36,1) forwards}',

        /* Gold shimmer on right edge — visible as overlay slides away */
        '#pt::after{content:"";position:absolute;top:0;right:0;width:2px;height:100%;',
        'background:linear-gradient(to bottom,transparent 5%,rgba(200,152,56,.55) 25%,',
        'rgba(255,215,0,.85) 50%,rgba(200,152,56,.55) 75%,transparent 95%);',
        'box-shadow:0 0 12px rgba(200,152,56,.35)}',

        /* Spinner inside overlay (appears after exit covers screen) */
        '#pt .s{position:absolute;top:50%;left:50%;width:30px;height:30px;margin:-15px 0 0 -15px;',
        'border:2px solid #1a1610;border-top-color:#a07830;border-radius:50%;opacity:0}',
        '#pt.x .s{animation:ptSpin .65s linear .85s infinite,ptShow .15s ease .85s forwards}',

        /* Reveal: slides off to the right */
        '@keyframes ptReveal{from{transform:translateX(0)}to{transform:translateX(102%)}}',

        /* Exit: slides in from the right */
        '#pt.x{transform:translateX(102%);',
        'animation:ptCover .85s cubic-bezier(.55,0,.1,1) forwards!important}',
        '@keyframes ptCover{from{transform:translateX(102%)}to{transform:translateX(0)}}',

        '@keyframes ptSpin{to{transform:rotate(360deg)}}',
        '@keyframes ptShow{to{opacity:1}}',
    ].join('');

    var style = document.createElement('style');
    style.textContent = css;
    document.head.appendChild(style);

    /* ── Inject overlay div ── */
    var overlay = document.createElement('div');
    overlay.id = 'pt';
    overlay.innerHTML = '<div class="s"></div>';
    document.body.insertBefore(overlay, document.body.firstChild);

    /* ── Navigation handling ── */
    var navigating = false;

    function goTo(href) {
        if (navigating) return;
        navigating = true;
        overlay.classList.add('x');
        setTimeout(function () { window.location.href = href; }, 900);
    }

    /* Intercept all link clicks — wait for exit animation before navigating */
    document.addEventListener('click', function (e) {
        /* Allow roulette prev/next buttons (data-no-transition) */
        if (e.target.dataset && e.target.dataset.noTransition !== undefined) return;
        if (e.target.id === 'btn-prev' || e.target.id === 'btn-next') return;

        var a = e.target.closest('a[href]');
        if (!a) return;
        var h = a.getAttribute('href') || '';
        if (!h || h[0] === '#' || /^(https?:|mailto:)/.test(h)) return;

        e.preventDefault();
        goTo(h);
    }, true); /* capture phase so it runs before any stopPropagation */

    /* Form submissions — cover screen, then let form submit naturally */
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (form.dataset && form.dataset.noTransition !== undefined) return;
        overlay.classList.add('x');
    });

})();
