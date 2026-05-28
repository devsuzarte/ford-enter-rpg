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

        var layout = document.getElementById('battle-layout');
        if (layout) layout.style.opacity = '0';

        var missChance = parseFloat(form.dataset.missChance || '0') / 100;
        var isHit = Math.random() >= missChance;

        /* store isHit on the form so the server uses the same result */
        var isHitInput = form.querySelector('input[name="isHit"]');
        if (isHitInput) isHitInput.value = isHit ? 'true' : 'false';

        if (slotLabel)  slotLabel.textContent = skillName || '';
        if (slotResult) { slotResult.style.opacity = '0'; slotResult.textContent = ''; }

        buildStrip(missChance, isHit);
        slotOverlay.style.display = 'flex';

        /* Mark that we've played the post-reload slot animations for this turn
           so the server-side replay doesn't run again after reload. */
        try {
            if (typeof battleId !== 'undefined' && typeof battleTurnCount !== 'undefined') {
                localStorage.setItem('es_' + battleId + '_' + battleTurnCount, '1');
            }
        } catch (e) { /* ignore storage errors */ }

        var finalY = (COUNT - 2 - 1) * BLOCK_H;

        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                /* randomise the easing feel slightly */
                var dur = (2200 + Math.random() * 800).toFixed(0);
                slotStrip.style.transition = 'transform ' + dur + 'ms cubic-bezier(0.04, 0.82, 0.18, 1)';
                slotStrip.style.transform  = 'translateY(-' + finalY + 'px)';
            });
        });

        var settleDur = 2700 + Math.random() * 300;
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

            var continueBtn = document.getElementById('slot-continue-btn');
            if (continueBtn) {
                continueBtn.style.display = 'inline-block';
                continueBtn.onclick = function () {
                    continueBtn.style.display = 'none';
                    slotOverlay.style.display = 'none';
                    centerBlock.style.boxShadow = '';
                    var loader = document.getElementById('loader');
                    if (loader) loader.style.display = 'flex';
                    if (pendingForm) { pendingForm.submit(); pendingForm = null; }
                };
            } else {
                setTimeout(function () {
                    slotOverlay.style.display = 'none';
                    centerBlock.style.boxShadow = '';
                    var loader = document.getElementById('loader');
                    if (loader) loader.style.display = 'flex';
                    if (pendingForm) { pendingForm.submit(); pendingForm = null; }
                }, 900);
            }
        }, settleDur);
    }

    document.querySelectorAll('.skill-form').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            /* Determine if the player acts first this turn */
            var goesFirst;
            if (typeof battleTurnCount !== 'undefined' && battleTurnCount === 0) {
                /* First turn: use the dice overlay result stored in the hidden input */
                var pgfInput = form.querySelector('input[name="playerGoesFirst"]');
                goesFirst = pgfInput ? pgfInput.value !== 'false' : true;
            } else {
                /* Subsequent turns: use the stored battle value from the server */
                goesFirst = typeof battlePlayerGoesFirst !== 'undefined' ? battlePlayerGoesFirst : true;
            }

            if (!goesFirst) {
                /* Enemy goes first — still show the player's slot animation before submitting
                   so the player sees their animation even when outcome is known. */
                var missChance = parseFloat(form.dataset.missChance || '0') / 100;
                var isHit = Math.random() >= missChance;
                var isHitInput = form.querySelector('input[name="isHit"]');
                if (isHitInput) isHitInput.value = isHit ? 'true' : 'false';
                var skillName = form.querySelector('.skill-name');
                runSlot(form, skillName ? skillName.textContent.trim() : '');
                return;
            }

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

    /* Weights: Epic 4x, Rare 2x, Common 1x */
    /* Weights: Common 4x, Rare 2x, Epic 1x (epic is rarest) */
    var SEGMENTS = [
        { label: 'Epico',  rarity: 'Epic',   color: '#3a2a00', rim: '#ffd700', weight: 1, textColor: '#ffd700' },
        { label: 'Raro',   rarity: 'Rare',   color: '#0d3a5c', rim: '#56b4e9', weight: 2, textColor: '#56b4e9' },
        { label: 'Comum',  rarity: 'Common', color: '#252520', rim: '#888877', weight: 4, textColor: '#bbbb99' }
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

            /* fix upside-down text: flip labels in the lower half of the canvas */
            var textAngle = mid + Math.PI / 2;
            if (Math.sin(mid) > 0) textAngle += Math.PI;
            ctx.rotate(textAngle);

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

        /* vary both rotations and duration for randomness */
        var extraSpins   = 7 + Math.random() * 9;
        var totalRotation = Math.PI * 2 * extraSpins;
        var duration      = 3400 + Math.random() * 2000;
        var startTime     = null;

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
        /* pointer is at the top (canvas angle −π/2).
           Normalize to wheel-layout frame [0, 2π): */
        var hit = ((-finalAngle) % (Math.PI * 2) + Math.PI * 2) % (Math.PI * 2);

        var found = SEGMENTS[SEGMENTS.length - 1];
        var acc = 0;
        for (var i = 0; i < SEGMENTS.length; i++) {
            acc += (SEGMENTS[i].weight / TOTAL_W) * Math.PI * 2;
            if (hit < acc) { found = SEGMENTS[i]; break; }
        }

        /* pass the rarity to the server via the hidden input */
        if (pendingForm) {
            var rarityInput = pendingForm.querySelector('input[name="rarity"]');
            if (rarityInput) rarityInput.value = found.rarity;
        }

        if (wheelResult) {
            wheelResult.style.color   = found.rim;
            wheelResult.textContent   = found.label + '!';
            wheelResult.style.opacity = '1';
        }

        setTimeout(function () {
            wheelOverlay.style.display = 'none';
            if (pendingForm) {
                pendingForm.dataset.noTransition = '';
                pendingForm.submit();
                pendingForm = null;
            }
        }, 2000);
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
