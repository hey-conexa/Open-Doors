/* ============================================================
   Assistente de Carreira — agente de IA do Open Doors

   Chama POST /api/ia/agente-carreira (o aluno é identificado pelo token).
   Depende de api-client.js (constante API e função authFetch).
   Não altera o menu-estudante.js.

   Segurança: todo texto vindo da IA entra na página com textContent,
   nunca com innerHTML, porque o conteúdo depende do currículo enviado.
   ============================================================ */
(function () {
    'use strict';

    const btnGerar    = document.getElementById('agBtnGerar');
    const statusEl    = document.getElementById('agStatus');
    const resultadoEl = document.getElementById('agResultado');

    if (!btnGerar || !statusEl || !resultadoEl) return;

    const TEXTO_BOTAO_INICIAL = btnGerar.textContent;
    let jaGerou = false;

    // ------------------------------------------------------------
    // Helpers de DOM e de dados
    // ------------------------------------------------------------
    function el(tag, className, text) {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null && text !== '') node.textContent = text;
        return node;
    }

    // Texto limpo ou '' (a IA pode devolver campos vazios ou de tipo inesperado)
    function texto(v) {
        if (typeof v === 'string') return v.trim();
        if (typeof v === 'number') return String(v);
        return '';
    }

    function listaDeTextos(v) {
        return (Array.isArray(v) ? v : []).map(texto).filter(Boolean);
    }

    function listaDeObjetos(v) {
        return (Array.isArray(v) ? v : []).filter(x => x && typeof x === 'object');
    }

    function capitalizar(s) {
        return s ? s.charAt(0).toUpperCase() + s.slice(1) : s;
    }

    // ------------------------------------------------------------
    // Montagem dos blocos de resultado
    // ------------------------------------------------------------
    function card({ rotulo, titulo, textos = [], tags = [] }) {
        const c = el('article', 'ag-card');
        c.setAttribute('role', 'listitem');

        if (rotulo) c.appendChild(el('span', 'ag-card__rotulo', rotulo));
        if (titulo) c.appendChild(el('h4', 'ag-card__titulo', titulo));
        textos.filter(Boolean).forEach(t => c.appendChild(el('p', 'ag-card__texto', t)));

        if (tags.length) {
            const wrap = el('div', 'ag-tags');
            tags.forEach(t => wrap.appendChild(el('span', 'me-tag', t)));
            c.appendChild(wrap);
        }
        return c;
    }

    function grade(cards, rotuloAcessivel) {
        const g = el('div', 'ag-grid');
        g.setAttribute('role', 'list');
        g.setAttribute('aria-label', rotuloAcessivel);
        cards.forEach(c => g.appendChild(c));
        return g;
    }

    function bloco(titulo, conteudo) {
        const secao = el('section', 'ag-bloco');
        secao.appendChild(el('h3', 'ag-bloco__titulo', titulo));
        secao.appendChild(conteudo);
        return secao;
    }

    function blocoResumo(d) {
        const wrap = el('div', 'ag-resumo');

        const resumo = texto(d.resumo);
        if (resumo) wrap.appendChild(el('p', 'ag-resumo__texto', resumo));

        const fortes = listaDeTextos(d.pontosFortes);
        if (fortes.length) {
            wrap.appendChild(el('h3', 'ag-bloco__titulo', 'Seus pontos fortes'));
            const tags = el('div', 'ag-tags');
            fortes.forEach(f => tags.appendChild(el('span', 'me-tag', f)));
            wrap.appendChild(tags);
        }
        return wrap;
    }

    function renderResultado(d) {
        resultadoEl.replaceChildren();

        const resumo = blocoResumo(d);
        if (resumo.childNodes.length) resultadoEl.appendChild(resumo);

        const melhorias = listaDeObjetos(d.melhoriasCurriculo).map(m => card({
            rotulo: 'Ajuste no currículo',
            titulo: texto(m.problema),
            textos: [texto(m.sugestao)]
        }));
        if (melhorias.length)
            resultadoEl.appendChild(bloco('Como melhorar seu currículo', grade(melhorias, 'Melhorias no currículo')));

        const cursos = listaDeObjetos(d.cursos).map(c => card({
            rotulo: capitalizar(texto(c.nivel)),
            titulo: texto(c.tema),
            textos: [texto(c.motivo)]
        }));
        if (cursos.length)
            resultadoEl.appendChild(bloco('O que estudar', grade(cursos, 'Temas para estudar')));

        const projetos = listaDeObjetos(d.projetosPortfolio).map(p => card({
            titulo: texto(p.titulo),
            textos: [
                texto(p.descricao),
                texto(p.demonstra) ? 'O que isso mostra: ' + texto(p.demonstra) : ''
            ],
            tags: listaDeTextos(p.tecnologias)
        }));
        if (projetos.length)
            resultadoEl.appendChild(bloco('Projetos para o seu portfólio', grade(projetos, 'Projetos de portfólio')));

        const conteudos = listaDeObjetos(d.conteudosParaPublicar).map(c => card({
            rotulo: capitalizar(texto(c.formato)),
            titulo: texto(c.titulo),
            textos: [
                texto(c.ideia),
                texto(c.porQue) ? 'Por que ajuda: ' + texto(c.porQue) : ''
            ]
        }));
        if (conteudos.length)
            resultadoEl.appendChild(bloco('Conteúdos para publicar', grade(conteudos, 'Ideias de conteúdo para publicar')));

        const passos = listaDeTextos(d.proximosPassos);
        if (passos.length) {
            const ol = el('ol', 'ag-lista');
            passos.forEach(p => ol.appendChild(el('li', '', p)));
            resultadoEl.appendChild(bloco('Seus próximos passos', ol));
        }

        // A IA pode devolver tudo vazio: nesse caso não mostramos um resultado em branco
        if (!resultadoEl.childNodes.length) return false;

        resultadoEl.hidden = false;
        resultadoEl.setAttribute('tabindex', '-1');
        resultadoEl.setAttribute('aria-label', 'Sugestões do assistente de carreira');

        const reduzirMovimento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        resultadoEl.scrollIntoView({ behavior: reduzirMovimento ? 'auto' : 'smooth', block: 'start' });
        resultadoEl.focus({ preventScroll: true });
        return true;
    }

    // ------------------------------------------------------------
    // Mensagens de status
    // ------------------------------------------------------------
    function mostrarStatus(mensagem, tipo) {
        statusEl.replaceChildren();
        if (!mensagem) return;

        const box = el('p', 'ag-status__msg' + (tipo ? ' ag-status__msg--' + tipo : ''), mensagem);
        if (tipo === 'erro') box.setAttribute('role', 'alert');
        statusEl.appendChild(box);
    }

    // Sem currículo enviado: avisa e oferece abrir o envio que já existe na página
    function mostrarSemCurriculo(mensagem) {
        mostrarStatus(mensagem || 'Envie seu currículo para receber sugestões.', 'aviso');

        const btnCurriculo = document.getElementById('btnCurriculo');
        if (btnCurriculo) {
            const acao = el('button', 'me-btn me-btn--secondary me-btn--sm', 'Enviar currículo');
            acao.type = 'button';
            acao.addEventListener('click', () => btnCurriculo.click());
            statusEl.appendChild(acao);
        }
    }

    function setCarregando(carregando) {
        btnGerar.disabled = carregando;
        btnGerar.setAttribute('aria-busy', carregando ? 'true' : 'false');
        btnGerar.textContent = carregando
            ? 'Analisando seu currículo…'
            : (jaGerou ? 'Gerar novamente' : TEXTO_BOTAO_INICIAL);
    }

    // ------------------------------------------------------------
    // Chamada à API
    // ------------------------------------------------------------
    async function gerar() {
        setCarregando(true);
        resultadoEl.hidden = true;
        mostrarStatus('A IA está lendo o seu currículo. Isso pode levar até 30 segundos.', 'info');

        try {
            const res = await authFetch(`${API}/api/ia/agente-carreira`, { method: 'POST' });
            const dados = await res.json().catch(() => ({}));

            if (res.status === 422) {
                mostrarSemCurriculo(dados.erro);
                return;
            }
            if (!res.ok) {
                throw new Error(dados.erro || 'Não foi possível gerar as sugestões agora.');
            }

            if (renderResultado(dados)) {
                jaGerou = true;
                mostrarStatus('Sugestões prontas. Veja abaixo.', 'ok');
            } else {
                mostrarStatus('A IA não devolveu sugestões desta vez. Tente gerar novamente.', 'aviso');
            }
        } catch (e) {
            // fetch() rejeita com TypeError quando não há conexão com o servidor
            const semRede = !!e && e.name === 'TypeError';
            mostrarStatus(
                semRede ? 'Não foi possível falar com o servidor. Verifique sua conexão.' : e.message,
                'erro'
            );
        } finally {
            setCarregando(false);
        }
    }

    btnGerar.addEventListener('click', gerar);
})();