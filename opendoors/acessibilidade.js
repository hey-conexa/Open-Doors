/**
 * acessibilidade.js
 * Barra de acessibilidade + integração VLibras
 * Funcionalidades: tamanho de fonte, alto contraste, dislexia, VLibras
 */

(function () {
    'use strict';

    // ── Configurações ──────────────────────────────────────────
    const FONT_SIZES   = ['normal', 'grande', 'muito-grande'];
    const FONT_LABELS  = ['A', 'A+', 'A++'];
    const STORAGE_KEY  = 'od-acessibilidade';

    // ── Carregar preferências salvas ───────────────────────────
    function carregarPrefs() {
        try {
            return JSON.parse(localStorage.getItem(STORAGE_KEY)) || {};
        } catch { return {}; }
    }

    function salvarPrefs(prefs) {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(prefs));
    }

    // ── Aplicar preferências ao <html> ─────────────────────────
    function aplicarPrefs(prefs) {
        const html = document.documentElement;

        // Fonte
        html.removeAttribute('data-font-size');
        if (prefs.fontSize && prefs.fontSize !== 'normal') {
            html.setAttribute('data-font-size', prefs.fontSize);
        }

        // Alto contraste
        html.classList.toggle('alto-contraste', !!prefs.altoContraste);

        // Fonte para dislexia
        html.classList.toggle('fonte-dislexia', !!prefs.dislexia);
    }

    // ── Criar barra de acessibilidade ──────────────────────────
    function criarBarra() {
        const prefs = carregarPrefs();
        aplicarPrefs(prefs);

        const barra = document.createElement('div');
        barra.id = 'acessibilidade-barra';
        barra.setAttribute('role', 'toolbar');
        barra.setAttribute('aria-label', 'Barra de acessibilidade');
        barra.innerHTML = `
            <button id="acc-toggle" class="acc-toggle-btn" aria-expanded="false" aria-controls="acc-painel" title="Opções de acessibilidade">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
                    <circle cx="12" cy="5" r="1.5"/>
                    <path d="M12 7v5m0 0l-3 5m3-5l3 5M9 9h6"/>
                </svg>
                <span class="acc-sr-only">Acessibilidade</span>
            </button>

            <div id="acc-painel" class="acc-painel" hidden>
                <p class="acc-titulo">Acessibilidade</p>

                <div class="acc-grupo">
                    <span class="acc-label">Tamanho do texto</span>
                    <div class="acc-fonte-btns">
                        ${FONT_SIZES.map((size, i) => `
                            <button class="acc-btn-fonte ${prefs.fontSize === size || (!prefs.fontSize && size === 'normal') ? 'ativo' : ''}"
                                    data-size="${size}"
                                    aria-label="Texto ${FONT_LABELS[i]}"
                                    aria-pressed="${prefs.fontSize === size || (!prefs.fontSize && size === 'normal')}">
                                ${FONT_LABELS[i]}
                            </button>
                        `).join('')}
                    </div>
                </div>

                <div class="acc-grupo">
                    <span class="acc-label">Visualização</span>
                    <button id="acc-contraste" class="acc-btn-toggle ${prefs.altoContraste ? 'ativo' : ''}" aria-pressed="${!!prefs.altoContraste}">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><circle cx="12" cy="12" r="9"/><path d="M12 3v18"/></svg>
                        Alto contraste
                    </button>
                    <button id="acc-dislexia" class="acc-btn-toggle ${prefs.dislexia ? 'ativo' : ''}" aria-pressed="${!!prefs.dislexia}">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M4 7h16M4 12h10M4 17h13"/></svg>
                        Fonte dislexia
                    </button>
                </div>

                <div class="acc-grupo">
                    <span class="acc-label">LIBRAS</span>
                    <button id="acc-libras" class="acc-btn-toggle" aria-label="Ativar intérprete de LIBRAS">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><path d="M7 11l5-5 5 5M7 17l5-5 5 5" transform="rotate(90,12,12)"/><circle cx="12" cy="12" r="10"/></svg>
                        Ativar VLibras
                    </button>
                </div>

                <button id="acc-redefinir" class="acc-btn-redefinir">Redefinir tudo</button>
            </div>
        `;

        document.body.appendChild(barra);
        inicializarEventos(prefs);
    }

    // ── Eventos da barra ───────────────────────────────────────
    function inicializarEventos(prefs) {
        const toggle = document.getElementById('acc-toggle');
        const painel = document.getElementById('acc-painel');

        // Abrir/fechar painel
        toggle.addEventListener('click', () => {
            const aberto = !painel.hidden;
            painel.hidden = aberto;
            toggle.setAttribute('aria-expanded', String(!aberto));
        });

        // Fechar ao pressionar Escape
        document.addEventListener('keydown', e => {
            if (e.key === 'Escape' && !painel.hidden) {
                painel.hidden = true;
                toggle.setAttribute('aria-expanded', 'false');
                toggle.focus();
            }
        });

        // Tamanho de fonte
        document.querySelectorAll('.acc-btn-fonte').forEach(btn => {
            btn.addEventListener('click', () => {
                const size = btn.dataset.size;
                prefs.fontSize = size;
                salvarPrefs(prefs);
                aplicarPrefs(prefs);

                document.querySelectorAll('.acc-btn-fonte').forEach(b => {
                    b.classList.toggle('ativo', b.dataset.size === size);
                    b.setAttribute('aria-pressed', String(b.dataset.size === size));
                });
            });
        });

        // Alto contraste
        document.getElementById('acc-contraste').addEventListener('click', function () {
            prefs.altoContraste = !prefs.altoContraste;
            salvarPrefs(prefs);
            aplicarPrefs(prefs);
            this.classList.toggle('ativo', prefs.altoContraste);
            this.setAttribute('aria-pressed', String(prefs.altoContraste));
        });

        // Fonte dislexia
        document.getElementById('acc-dislexia').addEventListener('click', function () {
            prefs.dislexia = !prefs.dislexia;
            salvarPrefs(prefs);
            aplicarPrefs(prefs);
            this.classList.toggle('ativo', prefs.dislexia);
            this.setAttribute('aria-pressed', String(prefs.dislexia));
        });

        // VLibras — ativa o widget e fecha o painel
        document.getElementById('acc-libras').addEventListener('click', () => {
            ativarVLibras();
            painel.hidden = true;
            toggle.setAttribute('aria-expanded', 'false');
        });

        // Redefinir
        document.getElementById('acc-redefinir').addEventListener('click', () => {
            prefs = {};
            salvarPrefs(prefs);
            aplicarPrefs(prefs);

            document.querySelectorAll('.acc-btn-fonte').forEach(b => {
                const isNormal = b.dataset.size === 'normal';
                b.classList.toggle('ativo', isNormal);
                b.setAttribute('aria-pressed', String(isNormal));
            });
            document.getElementById('acc-contraste').classList.remove('ativo');
            document.getElementById('acc-contraste').setAttribute('aria-pressed', 'false');
            document.getElementById('acc-dislexia').classList.remove('ativo');
            document.getElementById('acc-dislexia').setAttribute('aria-pressed', 'false');
        });
    }

    // ── VLibras — inicializa sempre ao carregar a página ──────
    function inicializarVLibras() {
        // Container exigido pelo widget
        const vw = document.createElement('div');
        vw.setAttribute('vw', '');
        vw.className = 'enabled';
        vw.innerHTML = `
            <div vw-access-button class="active"></div>
            <div vw-plugin-wrapper>
                <div class="vw-plugin-top-wrapper"></div>
            </div>
        `;
        document.body.appendChild(vw);

        const script = document.createElement('script');
        script.src = 'https://vlibras.gov.br/app/vlibras-plugin.js';
        script.onload = () => {
            new window.VLibras.Widget('https://vlibras.gov.br/app');
        };
        document.body.appendChild(script);
    }

    function ativarVLibras() {
        // O botão nativo do VLibras fica dentro do container [vw-access-button]
        const btn = document.querySelector('[vw-access-button]');
        if (btn) {
            btn.click();
        }
    }

    // ── Init ───────────────────────────────────────────────────
    function init() {
        criarBarra();
        inicializarVLibras();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
