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

    // Determine whether any slot animations will play this page load
    var _storageKey = 'es_' + (typeof battleId !== 'undefined' ? battleId : 0)
                    + '_' + (typeof battleTurnCount !== 'undefined' ? battleTurnCount : 0);

    var _enemyNeeded  = typeof enemySlotTurn !== 'undefined' && enemySlotTurn > 0;
    var _playerNeeded = typeof playerSlotHit !== 'undefined' && playerSlotHit !== null
                     && typeof playerSlotSkill !== 'undefined' && playerSlotSkill !== '';
    var _willAnimate  = (_enemyNeeded || _playerNeeded)
        && !localStorage.getItem(_storageKey);

    if (_willAnimate) {
        var _layout = document.getElementById('battle-layout');
        if (_layout) _layout.style.opacity = '0';

        document.addEventListener('allAnimationsDone', function () {
            if (_layout) {
                _layout.style.transition = 'opacity .4s';
                _layout.style.opacity = '1';
            }
            revealEvents(slides[cur]);
        }, { once: true });
    }
})();


// ── Post-reload slot animations (enemy first, optional player after) ───────────
(function () {
    var enemyOverlay = document.getElementById('enemy-slot-overlay');
    if (!enemyOverlay) return;

    var enemyNeeded  = typeof enemySlotTurn !== 'undefined' && enemySlotTurn > 0;
    var playerNeeded = typeof playerSlotHit !== 'undefined' && playerSlotHit !== null
                    && typeof playerSlotSkill !== 'undefined' && playerSlotSkill !== '';

    if (!enemyNeeded && !playerNeeded) return;

    var storageKey = 'es_' + (typeof battleId !== 'undefined' ? battleId : 0)
                   + '_' + (typeof battleTurnCount !== 'undefined' ? battleTurnCount : 0);
    if (localStorage.getItem(storageKey)) return;

    setTimeout(function () {
        localStorage.setItem(storageKey, '1');

        function afterEnemy() {
            if (playerNeeded) {
                runPlayerSlot(playerSlotSkill, playerSlotHit, function () {
                    document.dispatchEvent(new Event('allAnimationsDone'));
                });
            } else {
                document.dispatchEvent(new Event('allAnimationsDone'));
            }
        }

        if (enemyNeeded) {
            runEnemySlot(
                typeof enemySlotSkill !== 'undefined' ? enemySlotSkill : 'Ataque',
                typeof enemySlotHit   !== 'undefined' ? enemySlotHit   : true,
                afterEnemy
            );
        } else {
            runPlayerSlot(playerSlotSkill, playerSlotHit, function () {
                document.dispatchEvent(new Event('allAnimationsDone'));
            });
        }
    }, 500);

    function runEnemySlot(skillName, isHit, onDone) {
        var strip       = document.getElementById('enemy-slot-strip');
        var label       = document.getElementById('enemy-slot-name');
        var result      = document.getElementById('enemy-slot-result');
        var continueBtn = document.getElementById('enemy-slot-continue-btn');
        var BLOCK_H = 80, COUNT = 32;

        if (label)       label.textContent = skillName;
        if (result)      { result.style.opacity = '0'; result.textContent = ''; }
        if (continueBtn) continueBtn.style.display = 'none';

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

        enemyOverlay.style.display = 'flex';
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
                result.textContent   = isHit ? '✓  Acertou!' : '✗  Errou o ataque!';
                result.style.color   = isHit ? '#66dd88' : '#ff6666';
                result.style.opacity = '1';
            }

            if (continueBtn) {
                continueBtn.style.display = 'inline-block';
                continueBtn.onclick = function () {
                    continueBtn.style.display = 'none';
                    enemyOverlay.style.display = 'none';
                    if (onDone) onDone();
                };
            } else {
                setTimeout(function () {
                    enemyOverlay.style.display = 'none';
                    if (onDone) onDone();
                }, 1000);
            }
        }, settleDur);
    }

    function runPlayerSlot(skillName, isHit, onDone) {
        var playerOverlay = document.getElementById('slot-overlay');
        var strip       = document.getElementById('slot-strip');
        var label       = document.getElementById('slot-skill-name');
        var result      = document.getElementById('slot-result');
        var continueBtn = document.getElementById('slot-continue-btn');
        var BLOCK_H = 80, COUNT = 32;

        if (!playerOverlay || !strip) { if (onDone) onDone(); return; }

        if (label)       label.textContent = skillName;
        if (result)      { result.style.opacity = '0'; result.textContent = ''; }
        if (continueBtn) continueBtn.style.display = 'none';

        strip.innerHTML = '';
        strip.style.transition = 'none';
        strip.style.transform  = 'translateY(0)';

        for (var i = 0; i < COUNT; i++) {
            var el = document.createElement('div');
            el.className = 'slot-block';
            var g = (i === COUNT - 2) ? isHit : Math.random() >= 0.20;
            el.classList.add(g ? 'green' : 'red');
            el.textContent = g ? '✓  ACERTO' : '✗  ERRO';
            strip.appendChild(el);
        }

        playerOverlay.style.display = 'flex';
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
            center.style.transition = 'box-shadow .25s';

            if (result) {
                result.textContent   = isHit ? '✓  ACERTO!' : '✗  ERRO! Ataque falhou!';
                result.style.color   = isHit ? '#66dd88' : '#ff6666';
                result.style.opacity = '1';
            }

            if (continueBtn) {
                continueBtn.style.display = 'inline-block';
                continueBtn.onclick = function () {
                    continueBtn.style.display = 'none';
                    playerOverlay.style.display = 'none';
                    center.style.boxShadow = '';
                    if (onDone) onDone();
                };
            } else {
                setTimeout(function () {
                    playerOverlay.style.display = 'none';
                    center.style.boxShadow = '';
                    if (onDone) onDone();
                }, 900);
            }
        }, settleDur);
    }
})();


// ── 3D Dice initiative overlay (two dice) ─────────────────────────────────────
(function () {
    var diceOverlay = document.getElementById('dice-overlay');
    if (!diceOverlay) return;

    if (typeof battleTurnCount === 'undefined' || battleTurnCount !== 0) return;
    if (typeof battleStatus !== 'undefined' && battleStatus !== 'Active') return;

    var storageKey = 'dice_' + (typeof battleId !== 'undefined' ? battleId : 0);
    if (localStorage.getItem(storageKey)) return;

    diceOverlay.style.display = 'flex';

    var diceState = [
        { obj: null, pivot: null, rotX: 0, rotY: 0, rotZ: 0, velX: 0, velY: 0, velZ: 0,
          spinning: false, settled: false, snapping: false, targetRot: null, posX: -1.8 },
        { obj: null, pivot: null, rotX: 0, rotY: 0, rotZ: 0, velX: 0, velY: 0, velZ: 0,
          spinning: false, settled: false, snapping: false, targetRot: null, posX:  1.8 }
    ];
    var settledCount = 0;
    var playerRoll, enemyRoll;
    var threeOk = false;

    try {
        var scene  = new THREE.Scene();
        var camera = new THREE.PerspectiveCamera(50, 2, 0.1, 100);
        camera.position.set(0, 0, 5.5);

        var renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
        renderer.setSize(520, 260);
        renderer.setClearColor(0x000000, 0);

        var wrap = document.getElementById('dice-canvas-wrap');
        renderer.domElement.style.maxWidth = '100%';
        renderer.domElement.style.height   = 'auto';
        wrap.appendChild(renderer.domElement);

        scene.add(new THREE.AmbientLight(0xffffff, 0.45));
        var dirL = new THREE.DirectionalLight(0xffd700, 0.9);
        dirL.position.set(4, 6, 5);
        scene.add(dirL);
        var dirL2 = new THREE.DirectionalLight(0xffa040, 0.3);
        dirL2.position.set(-4, -2, 3);
        scene.add(dirL2);

        function applyMaterials(mats) {
            if (mats && mats.materials) {
                if (mats.materials['white']) {
                    mats.materials['white'].color.setHex(0xc8a020);
                    mats.materials['white'].shininess = 120;
                }
                if (mats.materials['black']) {
                    mats.materials['black'].color.setHex(0x100c00);
                    mats.materials['black'].shininess = 60;
                }
            }
        }

        function centreAndScale(obj) {
            var box = new THREE.Box3().setFromObject(obj);
            var cen = box.getCenter(new THREE.Vector3());
            obj.position.sub(cen);
            var sz  = box.getSize(new THREE.Vector3());
            obj.scale.setScalar(1.9 / Math.max(sz.x, sz.y, sz.z));
        }

        function loadGoldBoxes() {
            var mat = new THREE.MeshPhongMaterial({ color: 0xc8a020, shininess: 100 });
            diceState.forEach(function (ds) {
                var mesh = new THREE.Mesh(new THREE.BoxGeometry(1.8, 1.8, 1.8), mat.clone());
                var piv  = new THREE.Group(); piv.position.x = ds.posX; piv.add(mesh);
                ds.obj   = mesh;
                ds.pivot = piv;
                scene.add(piv);
            });
        }

        var mtlLoader = new THREE.MTLLoader();
        mtlLoader.load('/models/dice.mtl', function (mats) {
            mats.preload();
            applyMaterials(mats);

            new THREE.OBJLoader().setMaterials(mats).load('/models/dice.obj', function (obj) {
                centreAndScale(obj);

                var d0 = obj;
                var d1 = obj.clone();

                d1.traverse(function (child) {
                    if (child.isMesh && d0.children.length) {
                        var src = d0.children.find(function(c) { return c.isMesh && c.name === child.name; });
                        if (src) child.material = src.material;
                    }
                });

                diceState[0].obj = d0;
                diceState[1].obj = d1;

                var pivot0 = new THREE.Group(); pivot0.position.x = -1.8; pivot0.add(d0); scene.add(pivot0);
                var pivot1 = new THREE.Group(); pivot1.position.x =  1.8; pivot1.add(d1); scene.add(pivot1);
                diceState[0].pivot = pivot0;
                diceState[1].pivot = pivot1;
            }, undefined, loadGoldBoxes);
        }, undefined, loadGoldBoxes);

        var faceRot = {
            1: [ Math.PI / 2,  0,           0 ],
            2: [ 0,             0,           0 ],
            3: [ 0,            -Math.PI / 2, 0 ],
            4: [ 0,             Math.PI / 2, 0 ],
            5: [ 0,             Math.PI,     0 ],
            6: [-Math.PI / 2,  0,           0 ]
        };

        function lerpAngle(cur, tgt, t) {
            var d = tgt - cur;
            while (d >  Math.PI) d -= Math.PI * 2;
            while (d < -Math.PI) d += Math.PI * 2;
            return cur + d * t;
        }

        function animateDie(ds) {
            if (ds.spinning) {
                ds.rotX += ds.velX; ds.rotY += ds.velY; ds.rotZ += ds.velZ;
                ds.velX *= 0.975; ds.velY *= 0.975; ds.velZ *= 0.975;

                if (Math.abs(ds.velY) < 0.003 && !ds.settled) {
                    ds.spinning = false;
                    ds.settled  = true;
                    settledCount++;

                    var roll   = ds === diceState[0] ? playerRoll : enemyRoll;
                    var target = faceRot[roll] || [0, 0, 0];
                    ds.targetRot = target;
                    ds.snapping  = true;
                }
            }

            if (ds.snapping) {
                var t = 0.08;
                ds.rotX = lerpAngle(ds.rotX, ds.targetRot[0], t);
                ds.rotY = lerpAngle(ds.rotY, ds.targetRot[1], t);
                ds.rotZ = lerpAngle(ds.rotZ, ds.targetRot[2], t);
            } else if (!ds.spinning) {
                ds.rotY += 0.008;
            }

            if (ds.obj) ds.obj.rotation.set(ds.rotX, ds.rotY, ds.rotZ);
        }

        function animate() {
            requestAnimationFrame(animate);
            diceState.forEach(animateDie);
            renderer.render(scene, camera);
        }
        animate();

        threeOk = true;
    } catch (e) {
        console.warn('Three.js dice init failed:', e);
    }

    var rollBtn     = document.getElementById('dice-roll-btn');
    var continueBtn = document.getElementById('dice-continue-btn');
    var vsRow       = document.getElementById('dice-vs-row');
    var resultMsg   = document.getElementById('dice-result-msg');
    var playerScore = document.getElementById('dice-player-score');
    var enemyScore  = document.getElementById('dice-enemy-score');

    function launch(ds, delay) {
        setTimeout(function () {
            ds.rotX = Math.random() * Math.PI * 2;
            ds.rotY = Math.random() * Math.PI * 2;
            ds.rotZ = Math.random() * Math.PI * 2;
            ds.velX = (0.20 + Math.random() * 0.18) * (Math.random() < 0.5 ? 1 : -1);
            ds.velY = (0.26 + Math.random() * 0.22);
            ds.velZ = (0.12 + Math.random() * 0.14) * (Math.random() < 0.5 ? 1 : -1);
            ds.spinning = true;
            ds.settled  = false;
            ds.snapping = false;
        }, delay);
    }

    function doRoll() {
        rollBtn.disabled = true;
        rollBtn.textContent = 'Rolando…';
        resultMsg.style.display = 'none';

        settledCount = 0;
        diceState.forEach(function (ds) { ds.settled = false; ds.snapping = false; });

        playerRoll = Math.floor(Math.random() * 6) + 1;
        enemyRoll  = Math.floor(Math.random() * 6) + 1;

        if (threeOk) {
            launch(diceState[0], 0);
            launch(diceState[1], 180);
        } else {
            setTimeout(function () { settledCount = 2; }, 900);
        }

        var check = setInterval(function () {
            if (settledCount < 2) return;
            clearInterval(check);

            setTimeout(function () {
                if (playerRoll === enemyRoll) {
                    resultMsg.textContent  = '⚖ Empate! Rolando novamente…';
                    resultMsg.style.color  = '#d4a84b';
                    resultMsg.style.display = 'block';
                    setTimeout(doRoll, 1500);
                } else {
                    vsRow.style.display     = 'flex';
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

                    document.querySelectorAll('.skill-form input[name="playerGoesFirst"]').forEach(function (inp) {
                        inp.value = playerFirst ? 'true' : 'false';
                    });
                }
            }, 500);
        }, 100);
    }

    rollBtn.addEventListener('click', doRoll);

    continueBtn.addEventListener('click', function () {
        localStorage.setItem(storageKey, '1');
        diceOverlay.style.display = 'none';
    });
})();
