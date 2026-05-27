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


// ── Enemy slot machine (auto-play on page load after each turn) ───────────────
(function () {
    var overlay = document.getElementById('enemy-slot-overlay');
    if (!overlay) return;

    if (typeof enemySlotTurn === 'undefined' || enemySlotTurn <= 0) return;
    if (typeof battleStatus !== 'undefined' && battleStatus !== 'Active') return;

    var storageKey = 'es_' + (typeof battleId !== 'undefined' ? battleId : 0) + '_' + enemySlotTurn;
    if (localStorage.getItem(storageKey)) return;

    setTimeout(function () {
        localStorage.setItem(storageKey, '1');
        runEnemySlot(
            typeof enemySlotSkill !== 'undefined' ? enemySlotSkill : 'Ataque',
            typeof enemySlotHit   !== 'undefined' ? enemySlotHit   : true
        );
    }, 500);

    function runEnemySlot(skillName, isHit) {
        var strip  = document.getElementById('enemy-slot-strip');
        var label  = document.getElementById('enemy-slot-name');
        var result = document.getElementById('enemy-slot-result');
        var BLOCK_H = 80, COUNT = 32;

        if (label)  label.textContent = skillName;
        if (result) { result.style.opacity = '0'; result.textContent = ''; }

        strip.innerHTML = '';
        strip.style.transition = 'none';
        strip.style.transform  = 'translateY(0)';

        for (var i = 0; i < COUNT; i++) {
            var el = document.createElement('div');
            el.className = 'slot-block';
            var g = (i === COUNT - 2) ? isHit : Math.random() >= 0.15;
            el.classList.add(g ? 'green' : 'red');
            el.textContent = g ? '✓  ACERTO' : '✗  ERRO';
            strip.appendChild(el);
        }

        overlay.style.display = 'flex';
        var finalY = (COUNT - 2 - 1) * BLOCK_H;

        requestAnimationFrame(function () {
            requestAnimationFrame(function () {
                var dur = (2200 + Math.random() * 800).toFixed(0);
                strip.style.transition = 'transform ' + dur + 'ms cubic-bezier(0.04, 0.82, 0.18, 1)';
                strip.style.transform  = 'translateY(-' + finalY + 'px)';
            });
        });

        var settleDur = 2700 + Math.random() * 300;
        setTimeout(function () {
            var center = strip.children[COUNT - 2];
            center.style.boxShadow = isHit
                ? '0 0 22px #66dd88, inset 0 0 20px rgba(102,221,136,.25)'
                : '0 0 22px #ff6666, inset 0 0 20px rgba(255,102,102,.25)';

            if (result) {
                result.textContent  = isHit ? '✓  Acertou!' : '✗  Errou o ataque!';
                result.style.color  = isHit ? '#66dd88' : '#ff6666';
                result.style.opacity = '1';
            }

            setTimeout(function () { overlay.style.display = 'none'; }, 1000);
        }, settleDur);
    }
})();


// ── 3D Dice initiative overlay ─────────────────────────────────────────────────
(function () {
    var diceOverlay = document.getElementById('dice-overlay');
    if (!diceOverlay) return;

    /* only show dice on brand-new battles (turn 0) */
    if (typeof battleTurnCount === 'undefined' || battleTurnCount !== 0) return;
    if (typeof battleStatus !== 'undefined' && battleStatus !== 'Active') return;

    var storageKey = 'dice_' + (typeof battleId !== 'undefined' ? battleId : 0);
    if (localStorage.getItem(storageKey)) return;

    diceOverlay.style.display = 'flex';

    /* ── Three.js scene ── */
    var scene    = new THREE.Scene();
    var camera   = new THREE.PerspectiveCamera(50, 1, 0.1, 100);
    camera.position.set(0, 0, 4.5);

    var renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
    renderer.setSize(280, 280);
    renderer.setClearColor(0x000000, 0);
    renderer.shadowMap.enabled = true;

    var wrap = document.getElementById('dice-canvas-wrap');
    wrap.appendChild(renderer.domElement);

    /* lights */
    scene.add(new THREE.AmbientLight(0xffffff, 0.45));
    var dirL = new THREE.DirectionalLight(0xffd700, 0.9);
    dirL.position.set(4, 6, 5);
    scene.add(dirL);
    var fillL = new THREE.DirectionalLight(0xffa040, 0.3);
    fillL.position.set(-4, -2, 3);
    scene.add(fillL);

    /* load dice OBJ + MTL */
    var dice = null;
    var rotX = 0, rotY = 0, rotZ = 0;
    var velX = 0, velY = 0, velZ = 0;
    var spinning = false;

    var mtlLoader = new THREE.MTLLoader();
    mtlLoader.load('/models/dice.mtl', function (mats) {
        mats.preload();

        /* override colours to gold/black for medieval feel */
        if (mats.materials['white']) {
            mats.materials['white'].color.setHex(0xc8a020);
            mats.materials['white'].shininess = 120;
        }
        if (mats.materials['black']) {
            mats.materials['black'].color.setHex(0x100c00);
            mats.materials['black'].shininess = 60;
        }

        var objLoader = new THREE.OBJLoader();
        objLoader.setMaterials(mats);
        objLoader.load('/models/dice.obj', function (obj) {
            dice = obj;

            /* centre and scale */
            var box = new THREE.Box3().setFromObject(dice);
            var cen = box.getCenter(new THREE.Vector3());
            dice.position.sub(cen);
            var sz = box.getSize(new THREE.Vector3());
            var mx = Math.max(sz.x, sz.y, sz.z);
            dice.scale.setScalar(2.2 / mx);

            scene.add(dice);
        });
    }, undefined, function () {
        /* fallback if MTL fails: plain gold box */
        var geo = new THREE.BoxGeometry(1.8, 1.8, 1.8);
        var mat = new THREE.MeshPhongMaterial({ color: 0xc8a020, shininess: 100 });
        dice = new THREE.Mesh(geo, mat);
        scene.add(dice);
    });

    /* idle slow spin */
    function animate() {
        requestAnimationFrame(animate);
        if (dice) {
            if (spinning) {
                rotX += velX; rotY += velY; rotZ += velZ;
                velX *= 0.97; velY *= 0.97; velZ *= 0.97;
                if (Math.abs(velY) < 0.003) { spinning = false; onSettled(); }
            } else {
                rotX += 0.005; rotY += 0.008; rotZ += 0.003;
            }
            dice.rotation.set(rotX, rotY, rotZ);
        }
        renderer.render(scene, camera);
    }
    animate();

    var onSettled = null;

    /* Roll button */
    var rollBtn     = document.getElementById('dice-roll-btn');
    var continueBtn = document.getElementById('dice-continue-btn');
    var vsRow       = document.getElementById('dice-vs-row');
    var resultMsg   = document.getElementById('dice-result-msg');
    var playerScore = document.getElementById('dice-player-score');
    var enemyScore  = document.getElementById('dice-enemy-score');

    rollBtn.addEventListener('click', function () {
        rollBtn.disabled = true;
        rollBtn.textContent = 'Rolando…';

        /* fast random tumble */
        velX = (0.25 + Math.random() * 0.2) * (Math.random() < 0.5 ? 1 : -1);
        velY = (0.28 + Math.random() * 0.25);
        velZ = (0.15 + Math.random() * 0.15) * (Math.random() < 0.5 ? 1 : -1);
        spinning = true;

        var playerRoll = Math.floor(Math.random() * 6) + 1;
        var enemyRoll;
        do { enemyRoll = Math.floor(Math.random() * 6) + 1; } while (enemyRoll === playerRoll);

        onSettled = function () {
            /* show results */
            vsRow.style.display = 'flex';
            playerScore.textContent = playerRoll;
            enemyScore.textContent  = enemyRoll;

            var playerFirst = playerRoll > enemyRoll;
            resultMsg.textContent = playerFirst
                ? '⚔ Tu ages primeiro nesta batalha!'
                : '⚠ O inimigo tem iniciativa!';
            resultMsg.style.color   = playerFirst ? '#66dd88' : '#ff9966';
            resultMsg.style.display = 'block';
            continueBtn.style.display = 'inline-block';
            rollBtn.style.display     = 'none';
        };
    });

    continueBtn.addEventListener('click', function () {
        localStorage.setItem(storageKey, '1');
        diceOverlay.style.display = 'none';
    });
})();
