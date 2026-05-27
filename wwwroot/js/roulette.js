/* ── Slot machine (skill selection) ────────────────────────────────────────── */
(function () {
    var slotOverlay = document.getElementById('slot-overlay');
    var slotStrip   = document.getElementById('slot-strip');
    var slotLabel   = document.getElementById('slot-skill-name');
    var slotResult  = document.getElementById('slot-result');
    if (!slotOverlay || !slotStrip) return;

    var pendingForm = null;
    var BLOCK_H = 80;
    var COUNT   = 32;

    function buildStrip(missChance, resultIsHit) {
        slotStrip.innerHTML = '';
        slotStrip.style.transition = 'none';
        slotStrip.style.transform  = 'translateY(0)';

        for (var i = 0; i < COUNT; i++) {
            var el = document.createElement('div');
            el.className = 'slot-block';

            var isGreen;
            if (i === COUNT - 2) {
                isGreen = resultIsHit;
            } else {
                isGreen = Math.random() >= missChance;
            }

            el.classList.add(isGreen ? 'green' : 'red');
            el.textContent = isGreen ? '✓  ACERTO' : '✗  ERRO';
            slotStrip.appendChild(el);
        }
    }

    function runSlot(form, skillName) {
        pendingForm = form;
        var missChance = parseFloat(form.dataset.missChance || '0') / 100;
        var isHit = Math.random() >= missChance;

        if (slotLabel)  slotLabel.textContent = skillName || '';
        if (slotResult) { slotResult.style.opacity = '0'; slotResult.textContent = ''; }

        buildStrip(missChance, isHit);
        slotOverlay.style.display = 'flex';

        /* centre block = index (COUNT-2); scroll so it sits in middle of 3-block window */
        var finalY = (COUNT - 2 - 1) * BLOCK_H;

        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                slotStrip.style.transition = 'transform 2.6s cubic-bezier(0.04, 0.82, 0.18, 1)';
                slotStrip.style.transform  = 'translateY(-' + finalY + 'px)';
            });
        });

        setTimeout(function () {
            var centerBlock = slotStrip.children[COUNT - 2];
            centerBlock.style.boxShadow = isHit
                ? '0 0 22px #66dd88, inset 0 0 20px rgba(102,221,136,.25)'
                : '0 0 22px #ff6666, inset 0 0 20px rgba(255,102,102,.25)';
            centerBlock.style.transition = 'box-shadow .25s';

            if (slotResult) {
                slotResult.textContent  = isHit ? '✓  ACERTO!' : '✗  ERRO! Ataque falhou!';
                slotResult.style.color  = isHit ? '#66dd88' : '#ff6666';
                slotResult.style.opacity = '1';
            }

            setTimeout(function () {
                slotOverlay.style.display = 'none';
                centerBlock.style.boxShadow = '';
                var loader = document.getElementById('loader');
                if (loader) loader.style.display = 'flex';
                if (pendingForm) { pendingForm.submit(); pendingForm = null; }
            }, 900);
        }, 2700);
    }

    document.querySelectorAll('.skill-form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var skillName = form.querySelector('.skill-name');
            runSlot(form, skillName ? skillName.textContent.trim() : '');
        });
    });
})();


/* ── Roulette wheel (skill reward) ─────────────────────────────────────────── */
(function () {
    var wheelOverlay = document.getElementById('wheel-overlay');
    var canvas       = document.getElementById('wheel-canvas');
    var wheelResult  = document.getElementById('wheel-result');
    if (!wheelOverlay || !canvas) return;

    /* segments ordered so the arc layout matches the visual pointer at the top.
       Drawing starts at (rotation - π/2), i.e. the 12 o'clock position when rotation=0. */
    var SEGMENTS = [
        { label: 'Epico',  color: '#7a5e00', rim: '#ffd700', weight: 4, textColor: '#ffd700' },
        { label: 'Raro',   color: '#0d3a5c', rim: '#56b4e9', weight: 2, textColor: '#56b4e9' },
        { label: 'Comum',  color: '#252535', rim: '#888899', weight: 1, textColor: '#aaaacc' }
    ];
    var TOTAL_W = SEGMENTS.reduce(function (s, x) { return s + x.weight; }, 0);

    var pendingForm = null;
    var ctx = canvas.getContext('2d');
    var W   = canvas.width;
    var H   = canvas.height;
    var CX  = W / 2;
    var CY  = H / 2;
    var R   = Math.min(CX, CY) - 8;

    function drawWheel(rotation) {
        ctx.clearRect(0, 0, W, H);
        /* segments start from rotation - π/2 so that segment[0] begins at the top when rotation=0 */
        var start = rotation - Math.PI / 2;

        SEGMENTS.forEach(function (seg) {
            var sweep = (seg.weight / TOTAL_W) * Math.PI * 2;

            ctx.beginPath();
            ctx.moveTo(CX, CY);
            ctx.arc(CX, CY, R, start, start + sweep);
            ctx.closePath();
            ctx.fillStyle = seg.color;
            ctx.fill();

            ctx.beginPath();
            ctx.moveTo(CX, CY);
            ctx.arc(CX, CY, R, start, start + sweep);
            ctx.closePath();
            ctx.strokeStyle = seg.rim;
            ctx.lineWidth = 2;
            ctx.stroke();

            var mid = start + sweep / 2;
            var tx  = CX + Math.cos(mid) * R * 0.62;
            var ty  = CY + Math.sin(mid) * R * 0.62;
            ctx.save();
            ctx.translate(tx, ty);
            ctx.rotate(mid + Math.PI / 2);
            ctx.fillStyle = seg.textColor;
            ctx.font = 'bold 13px Segoe UI';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            ctx.fillText(seg.label, 0, 0);
            ctx.restore();

            start += sweep;
        });

        /* inner circle */
        ctx.beginPath();
        ctx.arc(CX, CY, 18, 0, Math.PI * 2);
        ctx.fillStyle = '#0d0d1a';
        ctx.fill();
        ctx.strokeStyle = '#3a3a5a';
        ctx.lineWidth = 2;
        ctx.stroke();

        /* spoke lines */
        start = rotation - Math.PI / 2;
        SEGMENTS.forEach(function (seg) {
            ctx.beginPath();
            ctx.moveTo(CX, CY);
            ctx.lineTo(CX + Math.cos(start) * R, CY + Math.sin(start) * R);
            ctx.strokeStyle = '#0d0d1a';
            ctx.lineWidth = 2;
            ctx.stroke();
            start += (seg.weight / TOTAL_W) * Math.PI * 2;
        });
    }

    function spinWheel(form) {
        pendingForm = form;
        if (wheelResult) { wheelResult.style.opacity = '0'; wheelResult.textContent = ''; }
        wheelOverlay.style.display = 'flex';

        var totalRotation = Math.PI * 2 * (9 + Math.random() * 6);
        var duration = 4200;
        var startTime = null;

        function easeOutQuart(t) { return 1 - Math.pow(1 - t, 4); }

        function step(ts) {
            if (!startTime) startTime = ts;
            var progress = Math.min((ts - startTime) / duration, 1);
            var angle = totalRotation * easeOutQuart(progress);
            drawWheel(angle);
            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                showResult(angle);
            }
        }

        drawWheel(0);
        requestAnimationFrame(step);
    }

    function showResult(finalAngle) {
        /* The wheel draws segment[0] starting at (rotation - π/2).
           The pointer is at the top = canvas angle -π/2.
           Angle of pointer within the segment layout:
             offset = (-π/2) - (finalAngle - π/2) = -finalAngle
           Normalized to [0, 2π): */
        var hit = ((-finalAngle) % (Math.PI * 2) + Math.PI * 2) % (Math.PI * 2);

        var found = SEGMENTS[SEGMENTS.length - 1];
        var acc = 0;
        for (var i = 0; i < SEGMENTS.length; i++) {
            acc += (SEGMENTS[i].weight / TOTAL_W) * Math.PI * 2;
            if (hit < acc) { found = SEGMENTS[i]; break; }
        }

        if (wheelResult) {
            wheelResult.style.color   = found.rim;
            wheelResult.textContent   = found.label + '!';
            wheelResult.style.opacity = '1';
        }

        setTimeout(function () {
            wheelOverlay.style.display = 'none';
            var loader = document.getElementById('loader');
            if (loader) loader.style.display = 'flex';
            if (pendingForm) { pendingForm.submit(); pendingForm = null; }
        }, 1100);
    }

    var skillForm = document.getElementById('skill-reward-form');
    if (skillForm) {
        skillForm.addEventListener('submit', function (e) {
            e.preventDefault();
            spinWheel(skillForm);
        });
    }

    drawWheel(0);
})();
