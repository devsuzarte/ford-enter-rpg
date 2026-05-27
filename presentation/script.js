/* ============================================================
   FordEnterRPG Presentation — script.js
   ============================================================ */

const TOTAL = 9;
let current = 0;
let busy    = false;

const curtainTop   = document.querySelector('.curtain--top');
const curtainBot   = document.querySelector('.curtain--bot');
const progressFill = document.getElementById('progressFill');
const navDotsEl    = document.getElementById('navDots');
const prevBtn      = document.getElementById('prevBtn');
const nextBtn      = document.getElementById('nextBtn');
const goRestart    = document.getElementById('goRestart');
const slides       = document.querySelectorAll('.slide');

/* ── Bootstrap ─────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
    buildNavDots();
    splitTitleLetters();
    initParticles();
    initKeyboard();
    initTouch();

    prevBtn.addEventListener('click', () => slideChange(-1));
    nextBtn.addEventListener('click', () => slideChange(1));
    goRestart.addEventListener('click', () => goTo(0));

    showSlide(0);
});

/* ── Nav Dots ───────────────────────────────────────────────── */
function buildNavDots() {
    for (let i = 0; i < TOTAL; i++) {
        const dot = document.createElement('button');
        dot.className = 'nav-dot';
        dot.setAttribute('aria-label', `Slide ${i + 1}`);
        dot.addEventListener('click', () => goTo(i));
        navDotsEl.appendChild(dot);
    }
}

function updateDots(idx) {
    navDotsEl.querySelectorAll('.nav-dot').forEach((d, i) => {
        d.classList.toggle('active', i === idx);
    });
}

/* ── Show Slide (instant, no curtain) ───────────────────────── */
function showSlide(idx) {
    slides.forEach((s, i) => s.classList.toggle('active', i === idx));
    updateDots(idx);
    progressFill.style.width = `${((idx + 1) / TOTAL) * 100}%`;
    prevBtn.disabled = idx === 0;
    nextBtn.disabled = idx === TOTAL - 1;
    animateSlide(slides[idx]);
}

/* ── Animate Slide Content ──────────────────────────────────── */
function animateSlide(slide) {
    const items = slide.querySelectorAll('[data-animate]');

    /* reset all animated items */
    items.forEach(el => {
        el.classList.remove('visible');
    });

    /* reset rarity bar fills (instant, no transition) */
    slide.querySelectorAll('.rarity-bar-fill').forEach(bar => {
        bar.style.transition = 'none';
        bar.style.width = '0';
        /* force reflow so transition re-applies later */
        void bar.offsetHeight;
        bar.style.transition = '';
    });

    /* reset title letters */
    const titleEl = slide.querySelector('.title-main.js-split');
    if (titleEl) {
        titleEl.querySelectorAll('.letter').forEach(l => l.classList.remove('visible'));
    }

    /* stagger items in */
    items.forEach((el, i) => {
        setTimeout(() => {
            el.classList.add('visible');

            /* animate rarity bars when their row becomes visible */
            el.querySelectorAll('.rarity-bar-fill').forEach(bar => {
                const w = parseFloat(bar.dataset.width) || 0;
                setTimeout(() => { bar.style.width = w + '%'; }, 180);
            });
        }, 220 + i * 155);
    });

    /* title letter-by-letter animation */
    if (titleEl) {
        const letters = titleEl.querySelectorAll('.letter');
        letters.forEach((l, j) => {
            setTimeout(() => l.classList.add('visible'), 130 + j * 62);
        });
    }
}

/* ── Navigate With Curtain ──────────────────────────────────── */
function slideChange(dir) {
    const next = current + dir;
    if (busy || next < 0 || next >= TOTAL) return;
    goTo(next);
}

function goTo(idx) {
    if (busy || idx === current) return;
    busy = true;

    /* close curtains */
    curtainTop.classList.add('closed');
    curtainBot.classList.add('closed');

    setTimeout(() => {
        current = idx;
        showSlide(current);

        /* slight hold then open curtains */
        setTimeout(() => {
            curtainTop.classList.remove('closed');
            curtainBot.classList.remove('closed');
            setTimeout(() => { busy = false; }, 400);
        }, 60);
    }, 380);
}

/* ── Split Title Letters ────────────────────────────────────── */
function splitTitleLetters() {
    const el = document.querySelector('.title-main.js-split');
    if (!el) return;
    const text = el.textContent;
    el.innerHTML = text
        .split('')
        .map(ch => ch === ' ' ? ' ' : `<span class="letter">${ch}</span>`)
        .join('');
}

/* ── Keyboard Navigation ────────────────────────────────────── */
function initKeyboard() {
    document.addEventListener('keydown', e => {
        switch (e.key) {
            case 'ArrowRight':
            case 'ArrowDown':
            case ' ':
                e.preventDefault();
                slideChange(1);
                break;
            case 'ArrowLeft':
            case 'ArrowUp':
                e.preventDefault();
                slideChange(-1);
                break;
            case 'Home':
                e.preventDefault();
                goTo(0);
                break;
            case 'End':
                e.preventDefault();
                goTo(TOTAL - 1);
                break;
        }
    });
}

/* ── Touch / Swipe Navigation ───────────────────────────────── */
function initTouch() {
    let startX = 0;
    document.addEventListener('touchstart', e => {
        startX = e.touches[0].clientX;
    }, { passive: true });
    document.addEventListener('touchend', e => {
        const dx = startX - e.changedTouches[0].clientX;
        if (Math.abs(dx) > 55) slideChange(dx > 0 ? 1 : -1);
    }, { passive: true });
}

/* ── Particle System ────────────────────────────────────────── */
function initParticles() {
    const canvas = document.getElementById('particles');
    const ctx    = canvas.getContext('2d');
    let W, H;

    function resize() {
        W = canvas.width  = window.innerWidth;
        H = canvas.height = window.innerHeight;
    }
    resize();
    window.addEventListener('resize', resize);

    const COLORS  = ['#c9a227', '#f5d060', '#a855c8', '#5599ff', '#e8e4d0'];
    const COUNT   = 90;
    const particles = [];

    for (let i = 0; i < COUNT; i++) {
        particles.push(makeParticle(true));
    }

    function makeParticle(randomY) {
        return {
            x:       Math.random() * (W || window.innerWidth),
            y:       randomY ? Math.random() * (H || window.innerHeight) : (H || window.innerHeight) + 5,
            r:       Math.random() * 1.6 + 0.4,
            vx:      (Math.random() - 0.5) * 0.25,
            vy:      -(Math.random() * 0.45 + 0.1),
            alpha:   Math.random() * 0.45 + 0.08,
            color:   COLORS[Math.floor(Math.random() * COLORS.length)],
            life:    randomY ? Math.random() : 0,
            maxLife: Math.random() * 0.5 + 0.5,
        };
    }

    function tick() {
        ctx.clearRect(0, 0, W, H);

        for (let i = 0; i < particles.length; i++) {
            const p = particles[i];
            p.x += p.vx;
            p.y += p.vy;
            p.life += 0.0028;

            if (p.life >= p.maxLife || p.y < -8) {
                particles[i] = makeParticle(false);
                continue;
            }

            const t = Math.sin((p.life / p.maxLife) * Math.PI);
            ctx.globalAlpha = t * p.alpha;
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
            ctx.fillStyle = p.color;
            ctx.fill();
        }

        ctx.globalAlpha = 1;
        requestAnimationFrame(tick);
    }

    tick();
}
