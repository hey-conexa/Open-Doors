/* ============================================================
   Página "Currículo" da área do estudante (Curriculo.html)

   - Envia o PDF para POST /api/ia/analisar-curriculo e mostra a análise
   - Abre o PDF guardado (GET /api/estudantes/{id}/curriculo)
   Depende de api-client.js (API, authFetch) e de menu-estudante.js
   (sessão, attachDropzone, refreshStudentCache, refreshProfileCardsUI).

   Todo texto vindo da IA entra na página com textContent, nunca innerHTML.
   ============================================================ */
(function () {
    'use strict';

    const TAMANHO_MAXIMO_MB = 5; // mesmo limite do backend

    const statusEl   = document.getElementById('cvStatus');
    const btnVer     = document.getElementById('cvBtnVer');
    const btnEnviar  = document.getElementById('cvBtnEnviar');
    const avisoEl    = document.getElementById('cvAviso');
    const analiseEl  = document.getElementById('cvAnalise');
    if (!statusEl || !btnEnviar || !analiseEl) return;

    let estudanteId = null;
    let dropzone = null;

    function el(tag, className, text) {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text) node.textContent = text;
        return node;
    }

    function textos(lista) {
        return Array.isArray(lista)
            ? lista.filter(v => typeof v === 'string' && v.trim()).map(v => v.trim())
            : [];
    }

    function aviso(msg, tipo = '') {
        avisoEl.textContent = msg || '';
        avisoEl.className = 'cv-aviso' + (tipo ? ` cv-aviso--${tipo}` : '');
    }

    // ------------------------------------------------------------
    // Situação do currículo (enviado ou não)
    // ------------------------------------------------------------
    function mostrarSituacao(estudante) {
        const temCurriculo = !!(estudante?.temCurriculo || estudante?.curriculoUrl);
        statusEl.replaceChildren(
            el('strong', '', temCurriculo ? 'Currículo enviado' : 'Nenhum currículo enviado ainda'),
            el('span', '', temCurriculo
                ? 'Empresas veem este PDF quando você se candidata. Envie um novo para substituir.'
                : 'Envie seu currículo em PDF para a IA identificar suas habilidades e melhorar suas recomendações de vagas.')
        );
        statusEl.classList.toggle('cv-status--ok', temCurriculo);
        if (btnVer) btnVer.hidden = !temCurriculo;
        btnEnviar.textContent = temCurriculo ? 'Enviar novo e analisar' : 'Enviar e analisar';
    }

    // ------------------------------------------------------------
    // Análise
    // ------------------------------------------------------------
    function bloco(titulo, conteudo) {
        const sec = el('div', 'cv-bloco');
        sec.appendChild(el('h3', 'cv-bloco__titulo', titulo));
        sec.appendChild(conteudo);
        return sec;
    }

    function chips(lista) {
        const wrap = el('div', 'cv-chips');
        wrap.setAttribute('role', 'list');
        lista.forEach(item => {
            const chip = el('span', 'me-tag', item);
            chip.setAttribute('role', 'listitem');
            wrap.appendChild(chip);
        });
        return wrap;
    }

    // dados: resposta de analisar-curriculo, ou só { habilidades } vindas do perfil salvo
    function mostrarAnalise(dados, { completa }) {
        const habilidades = textos(dados?.habilidades);
        const areas       = textos(dados?.areasAtuacao);
        const nivel       = typeof dados?.nivelExperiencia === 'string' ? dados.nivelExperiencia.trim() : '';
        const experiencias = Array.isArray(dados?.experiencias) ? dados.experiencias : [];

        analiseEl.replaceChildren();

        if (!habilidades.length && !areas.length && !nivel && !experiencias.length) {
            const vazio = el('div', 'me-candidaturas__empty');
            vazio.appendChild(el('p', '', 'A análise aparece aqui depois que você enviar o currículo.'));
            analiseEl.appendChild(vazio);
            return;
        }

        if (completa && (nivel || areas.length)) {
            const resumo = el('div', 'cv-resumo');
            if (nivel) {
                const item = el('div', 'cv-resumo__item');
                item.appendChild(el('span', '', 'Nível de experiência'));
                item.appendChild(el('strong', '', nivel.charAt(0).toUpperCase() + nivel.slice(1)));
                resumo.appendChild(item);
            }
            if (areas.length) {
                const item = el('div', 'cv-resumo__item');
                item.appendChild(el('span', '', 'Áreas de atuação'));
                item.appendChild(el('strong', '', areas.join(' · ')));
                resumo.appendChild(item);
            }
            analiseEl.appendChild(resumo);
        }

        if (habilidades.length) {
            analiseEl.appendChild(bloco(`Habilidades identificadas (${habilidades.length})`, chips(habilidades)));
        }

        if (completa && experiencias.length) {
            const lista = el('ul', 'cv-experiencias');
            experiencias.forEach(exp => {
                const cargo   = typeof exp?.cargo === 'string' ? exp.cargo.trim() : '';
                const empresa = typeof exp?.empresa === 'string' ? exp.empresa.trim() : '';
                const periodo = typeof exp?.periodo === 'string' ? exp.periodo.trim() : '';
                const desc    = typeof exp?.descricao === 'string' ? exp.descricao.trim() : '';
                if (!cargo && !empresa && !desc) return;

                const li = el('li', 'cv-experiencia');
                li.appendChild(el('strong', '', [cargo, empresa].filter(Boolean).join(' — ')));
                if (periodo) li.appendChild(el('span', 'cv-experiencia__periodo', periodo));
                if (desc) li.appendChild(el('p', '', desc));
                lista.appendChild(li);
            });
            if (lista.children.length) analiseEl.appendChild(bloco('Experiências', lista));
        }

        if (!completa) {
            analiseEl.appendChild(el('p', 'me-helper',
                'Nível, áreas de atuação e experiências aparecem logo depois de cada envio.'));
        }
    }

    // ------------------------------------------------------------
    // Envio
    // ------------------------------------------------------------
    async function enviar() {
        const file = dropzone?.getFile();
        if (!file) { aviso('Escolha o seu currículo em PDF primeiro.', 'erro'); return; }
        if (file.type !== 'application/pdf') { aviso('O arquivo precisa ser um PDF.', 'erro'); return; }
        if (file.size > TAMANHO_MAXIMO_MB * 1024 * 1024) {
            aviso(`O arquivo passa de ${TAMANHO_MAXIMO_MB} MB. Envie um PDF menor.`, 'erro');
            return;
        }

        const textoBotao = btnEnviar.textContent;
        btnEnviar.disabled = true;
        btnEnviar.textContent = 'Analisando...';
        aviso('A IA está lendo o seu currículo. Isso leva alguns segundos.');

        try {
            const fd = new FormData();
            fd.append('estudanteId', estudanteId);
            fd.append('curriculo', file);
            const res = await authFetch(`${API}/api/ia/analisar-curriculo`, { method: 'POST', body: fd });
            const dados = await res.json().catch(() => ({}));
            if (!res.ok) throw new Error(dados.erro || 'Não foi possível analisar o currículo.');

            mostrarAnalise(dados, { completa: true });
            aviso('Currículo enviado e analisado.', 'ok');

            // Atualiza o perfil local (habilidades, temCurriculo) e a situação da página
            await refreshStudentCache(estudanteId);
            mostrarSituacao(currentStudent || { temCurriculo: true });
            refreshProfileCardsUI();
            document.getElementById('cvAnaliseTitulo')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
        } catch (err) {
            aviso(err.message || 'Não foi possível analisar o currículo.', 'erro');
            btnEnviar.textContent = textoBotao;
        } finally {
            btnEnviar.disabled = false;
            if (btnEnviar.textContent === 'Analisando...') btnEnviar.textContent = textoBotao;
        }
    }

    // Abre o PDF guardado numa nova aba (a rota exige o token, por isso não é um link direto)
    async function verPdf() {
        const aba = window.open('', '_blank');
        try {
            const res = await authFetch(`${API}/api/estudantes/${estudanteId}/curriculo`);
            if (!res.ok) throw new Error();
            const url = URL.createObjectURL(await res.blob());
            if (aba) aba.location.href = url; else window.location.href = url;
            setTimeout(() => URL.revokeObjectURL(url), 60_000);
        } catch (_) {
            aba?.close();
            aviso('Não foi possível abrir o currículo agora.', 'erro');
        }
    }

    document.addEventListener('estudante-pronto', (e) => {
        estudanteId = e.detail.session.id;
        const estudante = e.detail.estudante;

        dropzone = attachDropzone('cvFileInput', 'cvDropzone', 'cvFileLabel', {
            acceptPrefix: 'application/pdf',
            maxSizeMb: TAMANHO_MAXIMO_MB
        });

        mostrarSituacao(estudante);
        mostrarAnalise({ habilidades: estudante?.habilidadesExtraidas }, { completa: false });

        btnEnviar.addEventListener('click', enviar);
        btnVer?.addEventListener('click', verPdf);
    });
})();
