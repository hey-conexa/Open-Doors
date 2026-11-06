/* Lógica da Área do Estudante e Teste Vocacional — Escala Likert */

let currentSession = null
let currentStudent = null

const HABILIDADES_OPCOES = [
    'Comunicacao', 'Trabalho em equipe', 'Lideranca', 'Organizacao', 'Resolucao de problemas',
    'Pensamento analitico', 'Criatividade', 'Atendimento ao cliente', 'Negociacao', 'Proatividade',
    'Gestao do tempo', 'Pacote Office', 'Excel', 'Power BI', 'Word',
    'Google Workspace', 'Canva', 'Marketing digital', 'Redacao', 'Oratoria',
    'Vendas', 'Gestao de projetos', 'Administracao', 'Financeiro', 'Contabilidade basica',
    'Logistica', 'Recursos humanos', 'Ingles', 'Espanhol', 'Programacao',
    'HTML/CSS', 'JavaScript', 'Banco de dados', 'Analise de dados', 'Suporte tecnico',
    'Design grafico', 'Edicao de video', 'Fotografia', 'Planejamento', 'Empatia'
];

// ==============================================================
// SESSÃO
// ==============================================================
document.addEventListener('DOMContentLoaded', async () => {
    try {
        // Token próprio do backend (salvo no login)
        const auth = getAuth();
        if (!auth || auth.tipoUsuario !== 'Estudante') {
            clearAuth();
            window.location.href = '../Acesso/AcessoEstudantes.html';
            return;
        }

        const raw     = localStorage.getItem('od-session')
        const session = raw ? JSON.parse(raw) : null

        if (!session || session.type !== 'student' || session.id !== auth.contaId) {
            clearAuth()
            window.location.href = '../Acesso/AcessoEstudantes.html'
            return
        }
        closeModal()
        currentSession = session

        await refreshStudentCache(session.id)

        const name    = session.name || 'Estudante'
        const initial = name.charAt(0).toUpperCase()

        document.getElementById('heroName').textContent   = `Olá, ${name.split(' ')[0]}!`
        document.getElementById('heroAvatar').textContent = initial
        updateHeroAvatar()

        atualizarProgressoPerfil()

        if (session.id) {
            try {
                const res = await authFetch(`${API}/api/testes-vocacionais/estudante/${session.id}`)
                if (res.ok) {
                    const teste = await res.json()
                    if (teste && teste.analisadoIa) {
                        session.temTesteVocacional = true
                        localStorage.setItem('od-session', JSON.stringify(session))
                        const badge = document.getElementById('voc-badge')
                        if (badge) { badge.textContent = 'Concluído ✓'; badge.className = 'me-badge me-badge--ok'; }
                        const introBtn = document.querySelector('#voc-intro .me-btn--primary')
                        if (introBtn) introBtn.textContent = 'Refazer teste'
                    }
                }
            } catch (e) { console.warn('Erro ao verificar teste:', e); }
        }

        // ---- Carregar recomendações de vagas ----
        await carregarRecomendadoVagas(session.id)

        // ---- Carregar candidaturas reais ----
        carregarCandidaturas(session.id).catch(() => {});

        // Bind start button
        const startBtn = document.getElementById('vocStartBtn');
        if (startBtn) startBtn.onclick = vocStart;

        bindProfileActions();
        refreshProfileCardsUI();

    } catch (e) { console.warn('Erro ao carregar sessão:', e); }
})

async function doLogout() {
    clearAuth();
    window.location.href = '../OpenDoors.html';
}

async function refreshStudentCache(estudanteId) {
    if (!estudanteId) return;
    try {
        const res = await authFetch(`${API}/api/estudantes/${estudanteId}`);
        if (!res.ok) return;
        currentStudent = await res.json();
    } catch (err) {
        console.warn('Nao foi possivel carregar dados do estudante:', err);
    }
}

function getModalEls() {
    return {
        backdrop: document.getElementById('meModalBackdrop'),
        title: document.getElementById('meModalTitle'),
        body: document.getElementById('meModalBody'),
        footer: document.getElementById('meModalFooter'),
        close: document.getElementById('meModalClose')
    };
}

function closeModal() {
    const { backdrop, body, footer } = getModalEls();
    if (!backdrop) return;
    backdrop.hidden = true;
    if (body) body.innerHTML = '';
    if (footer) footer.innerHTML = '';
}

function updateHeroAvatar() {
    const heroAvatar = document.getElementById('heroAvatar');
    if (!heroAvatar) return;

    const nome = (currentStudent?.nome || currentSession?.name || 'Estudante').trim();
    const initial = nome.charAt(0).toUpperCase();
    const foto = currentStudent?.fotoPerfilUrl;

    if (foto) {
        heroAvatar.style.backgroundImage = `url("${foto}")`;
        heroAvatar.style.backgroundSize = 'cover';
        heroAvatar.style.backgroundPosition = 'center';
        heroAvatar.textContent = '';
        heroAvatar.classList.add('me-hero__avatar--photo');
        return;
    }

    heroAvatar.style.backgroundImage = '';
    heroAvatar.textContent = initial || '?';
    heroAvatar.classList.remove('me-hero__avatar--photo');
}

function showModal({ title, bodyHtml = '', saveLabel = 'Salvar', onSave }) {
    const { backdrop, title: titleEl, body, footer, close } = getModalEls();
    if (!backdrop || !titleEl || !body || !footer || !close) return;

    titleEl.textContent = title;
    body.innerHTML = bodyHtml;
    footer.innerHTML = '';

    const cancelBtn = document.createElement('button');
    cancelBtn.type = 'button';
    cancelBtn.className = 'me-btn me-btn--ghost';
    cancelBtn.textContent = onSave ? 'Cancelar' : 'Fechar';
    cancelBtn.onclick = closeModal;
    footer.appendChild(cancelBtn);

    if (onSave) {
        const saveBtn = document.createElement('button');
        saveBtn.type = 'button';
        saveBtn.className = 'me-btn me-btn--primary';
        saveBtn.textContent = saveLabel;
        saveBtn.onclick = async () => {
            saveBtn.disabled = true;
            saveBtn.textContent = 'Salvando...';
            try {
                await onSave();
                closeModal();
            } catch (err) {
                alert(err?.message || 'Nao foi possivel salvar.');
                saveBtn.disabled = false;
                saveBtn.textContent = saveLabel;
            }
        };
        footer.appendChild(saveBtn);
    }

    close.onclick = closeModal;
    backdrop.onclick = (e) => {
        if (e.target === backdrop) closeModal();
    };
    backdrop.hidden = false;
}

function attachDropzone(inputId, zoneId, labelId, { acceptPrefix = '', maxSizeMb = 5 } = {}) {
    const input = document.getElementById(inputId);
    const zone = document.getElementById(zoneId);
    const label = document.getElementById(labelId);
    if (!input || !zone) return null;

    const maxBytes = maxSizeMb * 1024 * 1024;
    const setLabel = (file) => {
        if (!label) return;
        label.textContent = file ? `Arquivo selecionado: ${file.name}` : 'Nenhum arquivo selecionado';
    };

    const validate = (file) => {
        if (!file) return false;
        if (acceptPrefix && !file.type.startsWith(acceptPrefix)) {
            alert('Formato de arquivo invalido.');
            return false;
        }
        if (file.size > maxBytes) {
            alert(`Arquivo muito grande. Limite: ${maxSizeMb}MB.`);
            return false;
        }
        return true;
    };

    const setFile = (file) => {
        if (!validate(file)) return;
        const dt = new DataTransfer();
        dt.items.add(file);
        input.files = dt.files;
        setLabel(file);
    };

    input.addEventListener('change', () => setLabel(input.files?.[0] || null));
    zone.addEventListener('click', () => input.click());
    zone.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            input.click();
        }
    });
    zone.addEventListener('dragover', (e) => {
        e.preventDefault();
        zone.classList.add('is-dragover');
    });
    zone.addEventListener('dragleave', () => zone.classList.remove('is-dragover'));
    zone.addEventListener('drop', (e) => {
        e.preventDefault();
        zone.classList.remove('is-dragover');
        const file = e.dataTransfer?.files?.[0];
        if (file) setFile(file);
    });

    return {
        getFile: () => input.files?.[0] || null,
        setLabel,
    };
}

function toStudentPayload(st) {
    return {
        id: st.id,
        nome: st.nome || '',
        email: st.email || '',
        telefone: st.telefone || null,
        cpf: st.cpf || null,
        dataNascimento: st.dataNascimento || null,
        cidade: st.cidade || null,
        estado: st.estado || null,
        fotoPerfilUrl: st.fotoPerfilUrl || null,
        instituicao: st.instituicao || null,
        curso: st.curso || null,
        semestre: st.semestre ?? null,
        turno: st.turno || null,
        previsaoConclusao: st.previsaoConclusao || null,
        curriculoUrl: st.curriculoUrl || null,
        habilidadesExtraidas: st.habilidadesExtraidas || [],
        temCurriculo: st.temCurriculo ?? false,
        temTesteVocacional: st.temTesteVocacional ?? false,
        status: st.status || 'ativo'
    };
}

async function saveStudentPatch(patch) {
    if (!currentSession?.id) throw new Error('Sessao invalida.');
    if (!currentStudent) await refreshStudentCache(currentSession.id);
    if (!currentStudent) throw new Error('Nao foi possivel carregar estudante.');

    const merged = { ...currentStudent, ...patch };
    const payload = toStudentPayload(merged);
    const res = await authFetch(`${API}/api/estudantes/${currentSession.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });
    if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        throw new Error(err.erro || 'Falha ao salvar dados do estudante.');
    }

    currentStudent = await res.json();
    if (currentSession) {
        currentSession.name = currentStudent.nome || currentSession.name;
        localStorage.setItem('od-session', JSON.stringify(currentSession));
    }
    updateHeroAvatar();
    refreshProfileCardsUI();
}

function atualizarProgressoPerfil() {
    const hasFoto       = !!currentStudent?.fotoPerfilUrl;
    const hasCurriculo  = !!(currentStudent?.curriculoUrl || currentStudent?.temCurriculo);
    const hasHabilidades = Array.isArray(currentStudent?.habilidadesExtraidas) && currentStudent.habilidadesExtraidas.length > 0;
    const hasTeste      = !!(currentSession?.temTesteVocacional || currentStudent?.temTesteVocacional);

    // 25% por cada item: foto, currículo, habilidades, teste vocacional
    const pct = [hasFoto, hasCurriculo, hasHabilidades, hasTeste].filter(Boolean).length * 25;

    document.getElementById('progressPct').textContent  = pct + '%';
    document.getElementById('progressFill').style.width = pct + '%';
    document.querySelector('[role="progressbar"]')?.setAttribute('aria-valuenow', pct);
}

function refreshProfileCardsUI() {
    const hasFoto       = !!currentStudent?.fotoPerfilUrl;
    const hasCurriculo  = !!(currentStudent?.curriculoUrl || currentStudent?.temCurriculo);
    const hasHabilidades = Array.isArray(currentStudent?.habilidadesExtraidas) && currentStudent.habilidadesExtraidas.length > 0;

    const btnFoto = document.getElementById('btnFotoPerfil');
    if (btnFoto) btnFoto.textContent = hasFoto ? 'Editar' : 'Adicionar';

    const btnCurriculo = document.getElementById('btnCurriculo');
    if (btnCurriculo) btnCurriculo.textContent = hasCurriculo ? 'Atualizar' : 'Enviar';

    const btnHab = document.getElementById('btnHabilidades');
    if (btnHab) btnHab.textContent = hasHabilidades ? 'Editar' : 'Adicionar';

    const pendencias = [hasFoto, hasCurriculo, hasHabilidades].filter(v => !v).length;
    const pendenciaBadge = document.querySelector('#perfil .me-badge--warn');
    if (pendenciaBadge) pendenciaBadge.textContent = `${pendencias} pendencias`;

    atualizarProgressoPerfil();
}

function bindProfileActions() {
    const btnEditar = document.getElementById('btnEditarPerfil');
    if (btnEditar) btnEditar.onclick = abrirModalDadosPessoais;

    const btnFoto = document.getElementById('btnFotoPerfil');
    if (btnFoto) btnFoto.onclick = abrirModalFoto;

    const btnCurriculo = document.getElementById('btnCurriculo');
    if (btnCurriculo) btnCurriculo.onclick = abrirModalCurriculo;

    const btnHabilidades = document.getElementById('btnHabilidades');
    if (btnHabilidades) btnHabilidades.onclick = abrirModalHabilidades;

    const cfgDados = document.getElementById('cfgDadosPessoais');
    if (cfgDados) cfgDados.onclick = (e) => { e.preventDefault(); abrirModalDadosPessoais(); };

    const cfgSeg = document.getElementById('cfgSeguranca');
    if (cfgSeg) cfgSeg.onclick = (e) => { e.preventDefault(); abrirModalSeguranca(); };

    const cfgNotif = document.getElementById('cfgNotificacoes');
    if (cfgNotif) cfgNotif.onclick = (e) => { e.preventDefault(); abrirModalNotificacoes(); };
}

function abrirModalFoto() {
    showModal({
        title: 'Foto de perfil',
        bodyHtml: `
            <div class="me-form-grid">
                <label for="fotoPerfilFileInput">Escolher arquivo</label>
                <div id="fotoPerfilDropzone" class="me-dropzone" role="button" tabindex="0" aria-label="Escolher ou arrastar foto de perfil">
                    <p>Escolher arquivo ou arraste o arquivo ate aqui</p>
                    <input id="fotoPerfilFileInput" type="file" accept="image/*">
                </div>
                <p id="fotoPerfilFileLabel" class="me-helper">Nenhum arquivo selecionado</p>
                <p class="me-helper">Formatos aceitos: JPG, PNG e WebP (maximo 2MB).</p>
            </div>
        `,
        onSave: async () => {
            const file = document.getElementById('fotoPerfilFileInput')?.files?.[0];
            if (!file) throw new Error('Selecione uma imagem para salvar.');

            const foto = await new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onload = () => resolve(reader.result);
                reader.onerror = () => reject(new Error('Nao foi possivel ler a imagem.'));
                reader.readAsDataURL(file);
            });

            await saveStudentPatch({ fotoPerfilUrl: foto });
        }
    });

    const dropApi = attachDropzone('fotoPerfilFileInput', 'fotoPerfilDropzone', 'fotoPerfilFileLabel', {
        acceptPrefix: 'image/',
        maxSizeMb: 2
    });

    const existingFoto = currentStudent?.fotoPerfilUrl;
    if (dropApi && existingFoto && /^https?:\/\//i.test(existingFoto)) {
        dropApi.setLabel({ name: 'Foto atual mantida (sera substituida se enviar novo arquivo)' });
    }
}

function abrirModalCurriculo() {
    showModal({
        title: 'Curriculo',
        bodyHtml: `
            <div class="me-form-grid">
                <label for="curriculoFileInput">Envie seu curriculo em PDF</label>
                <div id="curriculoDropzone" class="me-dropzone" role="button" tabindex="0" aria-label="Escolher ou arrastar curriculo">
                    <p>Escolher arquivo ou arraste o arquivo ate aqui</p>
                    <input id="curriculoFileInput" type="file" accept="application/pdf">
                </div>
                <p id="curriculoFileLabel" class="me-helper">Nenhum arquivo selecionado</p>
                <p class="me-helper">Se enviar PDF, a IA tenta extrair habilidades automaticamente.</p>
            </div>
        `,
        onSave: async () => {
            const fileInput = document.getElementById('curriculoFileInput');
            const file = fileInput?.files?.[0];

            if (!file) throw new Error('Selecione um curriculo em PDF para salvar.');

            if (file && currentSession?.id) {
                const fd = new FormData();
                fd.append('estudanteId', currentSession.id);
                fd.append('curriculo', file);
                const res = await authFetch(`${API}/api/ia/analisar-curriculo`, { method: 'POST', body: fd });
                if (!res.ok) {
                    const err = await res.json().catch(() => ({}));
                    throw new Error(err.erro || 'Nao foi possivel analisar o curriculo.');
                }
                // Usa a URL retornada pela API (o backend já salvou no banco, mas atualizamos o estado local)
                const resultado = await res.json().catch(() => ({}));
                if (resultado.curriculoUrl) {
                    await saveStudentPatch({ curriculoUrl: resultado.curriculoUrl, temCurriculo: true });
                    return; // já salvo com URL correta
                }
            }

            await saveStudentPatch({ temCurriculo: true });
            gerarScoresParaVagas(currentSession.id)
                .then(r => carregarRecomendadoVagas(currentSession.id, r))
                .catch(() => {});
        }
    });

    attachDropzone('curriculoFileInput', 'curriculoDropzone', 'curriculoFileLabel', {
        acceptPrefix: 'application/pdf',
        maxSizeMb: 10
    });
}

function abrirModalHabilidades() {
    const selecionadas = new Set(currentStudent?.habilidadesExtraidas || []);
    const checkboxes = HABILIDADES_OPCOES.map((habilidade, idx) => `
        <label class="me-skill-option" for="habilidade_${idx}">
            <input id="habilidade_${idx}" type="checkbox" value="${habilidade}" ${selecionadas.has(habilidade) ? 'checked' : ''}>
            <span>${habilidade}</span>
        </label>
    `).join('');

    showModal({
        title: 'Habilidades',
        bodyHtml: `
            <div class="me-form-grid">
                <p class="me-helper">Selecione as habilidades que voce ja possui.</p>
                <div class="me-skills-grid">${checkboxes}</div>
            </div>
        `,
        onSave: async () => {
            const habilidades = Array.from(document.querySelectorAll('.me-skills-grid input[type="checkbox"]:checked'))
                .map(input => input.value);
            await saveStudentPatch({ habilidadesExtraidas: habilidades });
            gerarScoresParaVagas(currentSession.id)
                .then(r => carregarRecomendadoVagas(currentSession.id, r))
                .catch(() => {});
        }
    });
}

function abrirModalDadosPessoais() {
    showModal({
        title: 'Dados pessoais',
        bodyHtml: `
            <div class="me-form-grid">
                <label for="nomeInput">Nome</label>
                <input id="nomeInput" type="text" value="${currentStudent?.nome || ''}">
                <label for="emailInput">E-mail</label>
                <input id="emailInput" type="email" value="${currentStudent?.email || ''}">
                <label for="telefoneInput">Telefone</label>
                <input id="telefoneInput" type="text" value="${currentStudent?.telefone || ''}">
                <label for="cidadeInput">Cidade</label>
                <input id="cidadeInput" type="text" value="${currentStudent?.cidade || ''}">
                <label for="estadoInput">Estado (UF)</label>
                <input id="estadoInput" type="text" maxlength="2" value="${currentStudent?.estado || ''}">
            </div>
        `,
        onSave: async () => {
            const nome = document.getElementById('nomeInput')?.value?.trim();
            const email = document.getElementById('emailInput')?.value?.trim();
            if (!nome || !email) throw new Error('Nome e e-mail sao obrigatorios.');

            await saveStudentPatch({
                nome,
                email,
                telefone: document.getElementById('telefoneInput')?.value?.trim() || null,
                cidade: document.getElementById('cidadeInput')?.value?.trim() || null,
                estado: document.getElementById('estadoInput')?.value?.trim()?.toUpperCase() || null
            });

            const heroName = document.getElementById('heroName');
            if (heroName) heroName.textContent = `Olá, ${nome.split(' ')[0]}!`;
            const heroAvatar = document.getElementById('heroAvatar');
            if (heroAvatar) heroAvatar.textContent = nome.charAt(0).toUpperCase();
        }
    });
}

function abrirModalSeguranca() {
    showModal({
        title: 'Seguranca',
        bodyHtml: `
            <div class="me-form-grid">
                <label for="senhaAtualInput">Senha atual</label>
                <input id="senhaAtualInput" type="password" placeholder="Sua senha atual">
                <label for="novaSenhaInput">Nova senha</label>
                <input id="novaSenhaInput" type="password" placeholder="Minimo 8 caracteres">
                <label for="confirmaSenhaInput">Confirmar nova senha</label>
                <input id="confirmaSenhaInput" type="password" placeholder="Repita a nova senha">
            </div>
        `,
        saveLabel: 'Alterar senha',
        onSave: async () => {
            const atual = document.getElementById('senhaAtualInput')?.value || '';
            const nova = document.getElementById('novaSenhaInput')?.value || '';
            if (!atual) throw new Error('Digite a senha atual.');
            const conf = document.getElementById('confirmaSenhaInput')?.value || '';
            if (!nova) throw new Error('Digite a nova senha.');
            if (nova.length < 8) throw new Error('A nova senha precisa ter ao menos 8 caracteres.');
            if (nova !== conf) throw new Error('Confirmacao de senha nao confere.');

            await apiAlterarSenha(atual, nova);
        }
    });
}

function abrirModalNotificacoes() {
    const storageKey = `od-notif-${currentSession?.id || 'student'}`;
    const current = JSON.parse(localStorage.getItem(storageKey) || '{"vagas":true,"status":true,"newsletter":false}');
    showModal({
        title: 'Notificacoes',
        bodyHtml: `
            <div class="me-form-grid">
                <label><input type="checkbox" id="notifVagas" ${current.vagas ? 'checked' : ''}> Receber novas vagas por e-mail</label>
                <label><input type="checkbox" id="notifStatus" ${current.status ? 'checked' : ''}> Receber atualizacao de candidaturas</label>
                <label><input type="checkbox" id="notifNewsletter" ${current.newsletter ? 'checked' : ''}> Receber newsletter semanal</label>
            </div>
        `,
        saveLabel: 'Salvar preferencias',
        onSave: async () => {
            const prefs = {
                vagas: !!document.getElementById('notifVagas')?.checked,
                status: !!document.getElementById('notifStatus')?.checked,
                newsletter: !!document.getElementById('notifNewsletter')?.checked
            };
            localStorage.setItem(storageKey, JSON.stringify(prefs));
        }
    });
}

// ==============================================================
// TESTE VOCACIONAL — Escala Likert 7 pontos
// ==============================================================

let vocPerguntas = [];
let vocCurrent   = 0;
let vocAnswers   = {}; // { index: { perguntaId, pergunta, resposta: 1-7 } }

const LIKERT_LABELS = [
    { val: 1, label: 'Concordo totalmente',  color: '#2ecc71' },
    { val: 2, label: 'Concordo',             color: '#58d68d' },
    { val: 3, label: 'Concordo parcialmente',color: '#a9dfbf' },
    { val: 4, label: 'Neutro',               color: '#bdc3c7' },
    { val: 5, label: 'Discordo parcialmente',color: '#c9a7d8' },
    { val: 6, label: 'Discordo',             color: '#9b59b6' },
    { val: 7, label: 'Discordo totalmente',  color: '#7d3c98' },
];

async function vocStart() {
    document.getElementById('voc-intro').style.display = 'none';
    document.getElementById('voc-test').style.display  = 'block';

    vocCurrent = 0;
    vocAnswers = {};

    try {
        const res = await authFetch(`${API}/api/perguntas-teste`);
        if (!res.ok) throw new Error('Erro ao carregar perguntas');
        vocPerguntas = await res.json();

        if (!vocPerguntas || vocPerguntas.length === 0) {
            document.getElementById('voc-test').innerHTML =
                '<p style="color:var(--error);padding:2rem">Nenhuma pergunta disponível no momento.</p>';
            return;
        }

        document.querySelectorAll('.voc-total').forEach(el => el.textContent = vocPerguntas.length);
        vocRenderQuestion();
    } catch (err) {
        console.error(err);
        document.getElementById('voc-test').innerHTML =
            '<p style="color:var(--error);padding:2rem">Erro ao carregar o teste. Verifique sua conexão.</p>';
    }
}

window.vocStart = vocStart;

function mostrarProgressoIA(atual, total) {
    const container = document.querySelector('.me-vagas');
    if (!container) return;
    const pct = Math.round((atual / total) * 100);
    container.innerHTML = `
        <div class="ia-progresso">
            <div class="ia-progresso__icone"><svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M12 2a3 3 0 013 3v6H9V5a3 3 0 013-3z"/><circle cx="9" cy="16" r="1" fill="currentColor"/><circle cx="15" cy="16" r="1" fill="currentColor"/><line x1="12" y1="2" x2="12" y2="5"/></svg></div>
            <p class="ia-progresso__texto">Analisando vagas com IA… <strong>${atual} de ${total}</strong></p>
            <div class="ia-progresso__barra-bg">
                <div class="ia-progresso__barra-fill" style="width:${pct}%"></div>
            </div>
            <p class="ia-progresso__pct">${pct}%</p>
        </div>
    `;
}

async function gerarScoresParaVagas(estudanteId) {
    try {
        const vagasRes = await authFetch(`${API}/api/vagas/abertas`);
        if (!vagasRes.ok) return [];

        const vagas = await vagasRes.json();
        if (!Array.isArray(vagas) || vagas.length === 0) return [];

        const vagasParaScore = vagas.slice(0, 20);
        const recomendacoes = [];
        const total = vagasParaScore.filter(v => v?.id).length;
        let processadas = 0;

        mostrarProgressoIA(0, total);

        for (const vaga of vagasParaScore) {
            if (!vaga?.id) continue;

            try {
                const scoreRes = await authFetch(`${API}/api/ia/gerar-score`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ estudanteId, vagaId: vaga.id })
                });

                processadas++;
                mostrarProgressoIA(processadas, total);

                if (!scoreRes.ok) continue;

                const score = await scoreRes.json();
                recomendacoes.push({
                    ...vaga,
                    scoreTotal: score.scoreTotal,
                    scoreCurriculo: score.scoreCurriculo,
                    scoreVocacional: score.scoreVocacional,
                    scoreHabilidades: score.scoreHabilidades,
                    justificativa: score.justificativa
                });
            } catch (err) {
                processadas++;
                mostrarProgressoIA(processadas, total);
                console.warn(`Falha ao gerar score da vaga ${vaga.id}:`, err);
            }
        }

        recomendacoes.sort((a, b) => (b.scoreTotal || 0) - (a.scoreTotal || 0));
        return recomendacoes.slice(0, 5);
    } catch (err) {
        console.warn('Falha ao preparar recomendacoes de vagas:', err);
        return [];
    }
}

function vocRenderQuestion() {
    const q     = vocPerguntas[vocCurrent];
    const total = vocPerguntas.length;
    const pct   = Math.round((vocCurrent / total) * 100);

    const qNumEl = document.getElementById('vocQNum');
    if (qNumEl) qNumEl.textContent = vocCurrent + 1;

    const progressFillEl = document.getElementById('vocProgressFill');
    if (progressFillEl) progressFillEl.style.width = pct + '%';

    document.getElementById('vocProgressBar')?.setAttribute('aria-valuenow', pct);

    const cat = q.categoria || 'geral';
    const catLabel = cat.startsWith('RIASEC') ? `Holland Code — ${cat.replace('RIASEC_', '')}`
                 : cat.startsWith('BigFive') ? `Big Five — ${cat.replace('BigFive_', '')}`
                 : 'Perfil vocacional';
    const qTypeEl = document.getElementById('vocQType');
    if (qTypeEl) qTypeEl.textContent = catLabel;

    const qTextEl = document.getElementById('vocQText');
    if (qTextEl) qTextEl.textContent = q.pergunta;

    const btnBackEl = document.getElementById('vocBtnBack');
    if (btnBackEl) btnBackEl.style.visibility = vocCurrent > 0 ? 'visible' : 'hidden';

    const btnNext = document.getElementById('vocBtnNext');
    const isLast  = vocCurrent === total - 1;
    if (!btnNext) return;
    btnNext.textContent = isLast ? 'Ver resultado →' : 'Próxima →';
    btnNext.disabled    = !(vocCurrent in vocAnswers);

    const container = document.getElementById('vocOptions');
    if (!container) return;
    container.innerHTML = '';
    container.className = 'voc-options voc-options--likert';

    const labelLeft = document.createElement('span');
    labelLeft.className   = 'voc-likert-label voc-likert-label--left';
    labelLeft.textContent = 'Concordo';
    container.appendChild(labelLeft);

    const circlesWrap = document.createElement('div');
    circlesWrap.className   = 'voc-likert-circles';
    circlesWrap.setAttribute('role', 'radiogroup');
    circlesWrap.setAttribute('aria-label', 'Escala de concordância');

    const selectedVal = vocAnswers[vocCurrent]?.resposta ?? null;

    LIKERT_LABELS.forEach(item => {
        const btn = document.createElement('button');
        btn.type      = 'button';
        btn.className = 'voc-likert-circle';
        btn.setAttribute('role', 'radio');
        btn.setAttribute('aria-checked', selectedVal === item.val ? 'true' : 'false');
        btn.setAttribute('aria-label', item.label);
        btn.dataset.val = item.val;

        const dist  = Math.abs(item.val - 4);
        const size  = 28 + dist * 8;
        btn.style.cssText = `
            width:${size}px; height:${size}px;
            border-radius:50%;
            border: 2.5px solid ${item.color};
            background: ${selectedVal === item.val ? item.color : 'transparent'};
            cursor: pointer;
            transition: background 0.15s, transform 0.1s;
            flex-shrink: 0;
        `;

        btn.addEventListener('click', () => {
            vocAnswers[vocCurrent] = {
                perguntaId: q.id,
                pergunta:   q.pergunta,
                resposta:   item.val,
                categoria:  q.categoria || 'geral'
            };
            circlesWrap.querySelectorAll('.voc-likert-circle').forEach(b => {
                const v     = parseInt(b.dataset.val);
                const lItem = LIKERT_LABELS[v - 1];
                const d     = Math.abs(v - 4);
                const s     = 28 + d * 8;
                b.style.background = v === item.val ? lItem.color : 'transparent';
                b.setAttribute('aria-checked', v === item.val ? 'true' : 'false');
            });
            document.getElementById('vocBtnNext').disabled = false;
        });

        circlesWrap.appendChild(btn);
    });

    container.appendChild(circlesWrap);

    const labelRight = document.createElement('span');
    labelRight.className   = 'voc-likert-label voc-likert-label--right';
    labelRight.textContent = 'Discordo';
    container.appendChild(labelRight);
}

function vocNext() {
    if (!(vocCurrent in vocAnswers)) return;
    if (vocCurrent === vocPerguntas.length - 1) {
        vocSubmit();
    } else {
        vocCurrent++;
        vocRenderQuestion();
        document.getElementById('vocQuestionWrap')?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}

function vocBack() {
    if (vocCurrent > 0) {
        vocCurrent--;
        vocRenderQuestion();
    }
}

// ==============================================================
// ENVIO
// ==============================================================

async function vocSubmit() {
    const session = JSON.parse(localStorage.getItem('od-session') || '{}');
    if (!session.id) {
        alert('Sessão expirada. Faça login novamente.');
        window.location.href = '../Acesso/AcessoEstudantes.html';
        return;
    }

    const qTypeEl = document.getElementById('vocQType');
    const qTextEl = document.getElementById('vocQText');
    const optionsEl = document.getElementById('vocOptions');
    const btnBack = document.getElementById('vocBtnBack');
    const btnNext = document.getElementById('vocBtnNext');

    if (qTypeEl) qTypeEl.textContent = 'Processando';
    if (qTextEl) qTextEl.textContent = '⏳ Analisando suas respostas com IA...';
    if (optionsEl) {
        optionsEl.innerHTML = '<p style="color:var(--text-muted);padding:1rem 0;text-align:center">Isso pode levar alguns segundos.</p>';
    }
    if (btnBack) {
        btnBack.disabled = true;
        btnBack.style.visibility = 'hidden';
    }
    if (btnNext) {
        btnNext.disabled = true;
        btnNext.textContent = 'Processando...';
    }

    try {
        const respostas = Object.values(vocAnswers).map(r => ({
            perguntaId: r.perguntaId,
            pergunta:   r.pergunta,
            resposta:   String(r.resposta),
            categoria:  r.categoria || 'geral'
        }));

        const saveRes = await authFetch(`${API}/api/testes-respostas`, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body:    JSON.stringify({ estudanteId: session.id, respostas })
        });
        if (!saveRes.ok) {
            const err = await saveRes.json().catch(() => ({}));
            throw new Error(err.erro || 'Erro ao salvar respostas');
        }

        const iaRes = await authFetch(`${API}/api/ia/analisar-teste`, {
            method:  'POST',
            headers: { 'Content-Type': 'application/json' },
            body:    JSON.stringify({ estudanteId: session.id, respostas })
        });
        if (!iaRes.ok) {
            const err = await iaRes.json().catch(() => ({}));
            throw new Error(err.erro || 'Erro na análise da IA');
        }
        const resultado = await iaRes.json();
        session.temTesteVocacional = true;
        localStorage.setItem('od-session', JSON.stringify(session));
        vocShowResult(resultado);

        // Atualiza recomendacoes em segundo plano para nao travar a tela de resultado
        gerarScoresParaVagas(session.id)
            .then((fallbackRecomendacoes) => carregarRecomendadoVagas(session.id, fallbackRecomendacoes))
            .catch((err) => console.warn('Falha ao atualizar recomendacoes apos teste:', err));
    } catch (err) {
        console.error(err);
        if (qTypeEl) qTypeEl.textContent = 'Erro';
        if (qTextEl) qTextEl.textContent = '❌ Não foi possível concluir o teste agora.';
        if (optionsEl) {
            optionsEl.innerHTML = `
                <p style="color:var(--error);padding:1rem 0;text-align:center">${err.message}</p>
                <div style="text-align:center">
                    <button onclick="vocStart()" class="me-btn me-btn--primary">Tentar novamente</button>
                </div>`;
        }
        if (btnNext) {
            btnNext.disabled = false;
            btnNext.textContent = 'Próxima →';
        }
    }
}

// ==============================================================
// RESULTADO
// ==============================================================

function vocShowResult(resultado) {
    document.getElementById('voc-test').style.display   = 'none';
    document.getElementById('voc-result').style.display = 'block';

    document.getElementById('vocResultDesc').textContent =
        resultado.descricaoPerfil || 'Suas respostas revelam um perfil versátil com múltiplas afinidades.';

    const areasContainer = document.getElementById('vocAreas');
    areasContainer.innerHTML = '';

    (resultado.areasSugeridas || []).forEach((area, idx) => {
        const card = document.createElement('div');
        card.className = 'voc-area-card' + (idx === 0 ? ' voc-area-card--top' : '');
        card.setAttribute('role', 'listitem');
        card.innerHTML = `
            <div class="voc-area-card__header">
                <strong>${area}</strong>
                ${idx === 0 ? '<span class="me-badge me-badge--ok">Melhor match</span>' : ''}
            </div>`;
        areasContainer.appendChild(card);
    });

    if (resultado.pontosFortes?.length > 0) {
        const div = document.createElement('div');
        div.style.marginTop = '16px';
        div.innerHTML = `<p style="font-weight:600;margin-bottom:8px">Seus pontos fortes:</p>
            <ul style="padding-left:20px;color:var(--text-muted)">
                ${resultado.pontosFortes.map(p => `<li>${p}</li>`).join('')}
            </ul>`;
        areasContainer.appendChild(div);
    }

    const badge = document.getElementById('voc-badge');
    if (badge) { badge.textContent = 'Concluído ✓'; badge.className = 'me-badge me-badge--ok'; }

    document.getElementById('voc-result').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function vocRestart() {
    document.getElementById('voc-result').style.display = 'none';
    vocStart();
}

// ==============================================================
// RECOMENDAÇÕES DE VAGAS
// ==============================================================

async function carregarRecomendadoVagas(estudanteId, fallbackCards = []) {
    try {
        const resp = await authFetch(`${API}/api/vagas/recomendadas/${estudanteId}`);
        if (!resp.ok) throw new Error('Erro ao buscar recomendações');
        const cardsApi = await resp.json();
        const cards = Array.isArray(cardsApi) && cardsApi.length > 0 ? cardsApi : fallbackCards;

        const container = document.querySelector('.me-vagas');
        if (!container) return;

        if (cards.length === 0) {
    container.innerHTML = '<p class="me-vaga__desc">Nenhuma vaga recomendada no momento.</p>';
} else {
    container.innerHTML = ''; // limpa cards padrão
}

        cards.forEach(card => {
            const article = document.createElement('article');
            article.className = 'me-vaga';
            article.setAttribute('role', 'listitem');
            if (card.id) article.dataset.vagaId = card.id;

            // Logomarca genérica
            const header = document.createElement('div');
            header.className = 'me-vaga__header';
            const logo = document.createElement('div');
            logo.className = 'me-vaga__logo';
            logo.setAttribute('aria-hidden', 'true');
            logo.innerHTML = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>';
            header.appendChild(logo);

            const info = document.createElement('div');
            info.innerHTML = `
                <h3 class="me-vaga__title">${card.titulo}</h3>
                <p class="me-vaga__company">${card.empresa || card.empresaId || 'Empresa'}</p>
                <span class="me-badge me-badge--new" aria-label="Vaga nova">${card.status || 'Nova'}</span>
            `;
            header.appendChild(info);

            // Tags
            const tagsDiv = document.createElement('div');
            tagsDiv.className = 'me-vaga__tags';
            tagsDiv.setAttribute('aria-label', 'Características da vaga');
            const tags = [
                `Área: ${card.area}`,
                `Cidade: ${card.cidade}`,
                `Score: ${card.scoreTotal}%`
            ];
            tags.forEach(t => {
                const span = document.createElement('span');
                span.className = 'me-tag';
                span.textContent = t;
                tagsDiv.appendChild(span);
            });

            // Descrição curta
            const desc = document.createElement('p');
            desc.className = 'me-vaga__desc';
            desc.textContent = card.descricao || 'Vaga em destaque para seu perfil.';

            // Footer com distânci­a e botão
            const footer = document.createElement('div');
            footer.className = 'me-vaga__footer';
            const dateSpan = document.createElement('span');
            dateSpan.className = 'me-vaga__date';
            dateSpan.innerHTML = '<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true"><rect x="3" y="4" width="18" height="18" rx="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>';
            dateSpan.appendChild(document.createTextNode('Publicada há 2 dias'));
            footer.appendChild(dateSpan);

            const btn = document.createElement('button');
            btn.className = 'me-btn me-btn--primary me-btn--sm';
            btn.setAttribute('aria-label', `Candidatar-se à vaga de ${card.titulo}`);
            btn.textContent = 'Candidatar-se';
            btn.onclick = () => candidatarSe(btn, card);
            footer.appendChild(btn);

            // Assemble
            article.appendChild(header);
            article.appendChild(tagsDiv);
            article.appendChild(desc);
            article.appendChild(footer);

            container.appendChild(article);
        });
    } catch (err) {
        console.error(err);
        const container = document.querySelector('.me-vagas');
        if (!container) return;

        if (Array.isArray(fallbackCards) && fallbackCards.length > 0) {
            container.innerHTML = '';
            fallbackCards.forEach(card => {
                const article = document.createElement('article');
                article.className = 'me-vaga';
                article.setAttribute('role', 'listitem');
                if (card.id) article.dataset.vagaId = card.id;

                const header = document.createElement('div');
                header.className = 'me-vaga__header';
                const logo = document.createElement('div');
                logo.className = 'me-vaga__logo';
                logo.setAttribute('aria-hidden', 'true');
                logo.innerHTML = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>';
                header.appendChild(logo);

                const info = document.createElement('div');
                info.innerHTML = `
                    <h3 class="me-vaga__title">${card.titulo}</h3>
                    <p class="me-vaga__company">${card.empresa || card.empresaId || 'Empresa'}</p>
                    <span class="me-badge me-badge--new" aria-label="Vaga nova">${card.status || 'Nova'}</span>
                `;
                header.appendChild(info);

                const tagsDiv = document.createElement('div');
                tagsDiv.className = 'me-vaga__tags';
                ['Área: ' + (card.area || '-'), 'Cidade: ' + (card.cidade || '-'), 'Score: ' + ((card.scoreTotal ?? 0) + '%')].forEach(t => {
                    const span = document.createElement('span');
                    span.className = 'me-tag';
                    span.textContent = t;
                    tagsDiv.appendChild(span);
                });

                const desc = document.createElement('p');
                desc.className = 'me-vaga__desc';
                desc.textContent = card.descricao || card.justificativa || 'Vaga em destaque para seu perfil.';

                const fbFooter = document.createElement('div');
                fbFooter.className = 'me-vaga__footer';
                const fbBtn = document.createElement('button');
                fbBtn.className = 'me-btn me-btn--primary me-btn--sm';
                fbBtn.setAttribute('aria-label', `Candidatar-se à vaga de ${card.titulo}`);
                fbBtn.textContent = 'Candidatar-se';
                fbBtn.onclick = () => candidatarSe(fbBtn, card);
                fbFooter.appendChild(fbBtn);

                article.appendChild(header);
                article.appendChild(tagsDiv);
                article.appendChild(desc);
                article.appendChild(fbFooter);
                container.appendChild(article);
            });
            return;
        }

        container.innerHTML = `<p style="color:var(--error)">Não foi possível carregar vagas recomendadas.</p>`;
    }
}

// ─── CANDIDAR-SE A UMA VAGA ──────────────────────────────────────────────────

async function candidatarSe(btn, card) {
    if (!currentSession?.id) return;

    // Impede duplo clique
    btn.disabled = true;
    const original = btn.textContent;
    btn.textContent = 'Enviando…';

    try {
        const res = await authFetch(`${API}/api/candidaturas`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                estudanteId: currentSession.id,
                vagaId: card.id || card.vagaId,
                empresaId: card.empresaId || '00000000-0000-0000-0000-000000000000',
                status: 'pendente'
            })
        });

        if (res.status === 409 || res.status === 400) {
            const err = await res.json().catch(() => ({}));
            // Verifica se é duplicata
            if (err?.erro?.toLowerCase().includes('duplica') || err?.erro?.toLowerCase().includes('já')) {
                btn.textContent = 'Já enviada';
                btn.classList.remove('me-btn--primary');
                btn.classList.add('me-btn--secondary');
                return;
            }
        }

        if (!res.ok) throw new Error('Falha ao candidatar');

        btn.textContent = 'Enviada ✓';
        btn.classList.remove('me-btn--primary');
        btn.classList.add('me-btn--secondary');
        btn.disabled = true;

        // Atualiza a seção de candidaturas em tempo real
        await carregarCandidaturas(currentSession.id);

    } catch (err) {
        console.error('Erro ao candidatar:', err);
        btn.textContent = original;
        btn.disabled = false;
        alert('Não foi possível enviar a candidatura. Tente novamente.');
    }
}

// ─── CARREGAR CANDIDATURAS DO ESTUDANTE ──────────────────────────────────────

async function carregarCandidaturas(estudanteId) {
    const lista = document.getElementById('candidaturasLista');
    const empty = document.getElementById('candidaturasEmpty');
    if (!lista) return;

    try {
        const res = await authFetch(`${API}/api/candidaturas/estudante/${estudanteId}`);
        if (!res.ok) throw new Error('Erro ao carregar candidaturas');
        const candidaturas = await res.json();

        // Remove itens anteriores (exceto o empty state)
        Array.from(lista.querySelectorAll('.me-candidatura')).forEach(el => el.remove());

        if (!candidaturas.length) {
            if (empty) empty.style.display = '';
            return;
        }

        if (empty) empty.style.display = 'none';

        // Enriquecer com título da vaga (busca em lote por IDs únicos)
        const vagaIds = [...new Set(candidaturas.map(c => c.vagaId).filter(Boolean))];
        const vagaMap = {};
        await Promise.all(vagaIds.map(async id => {
            try {
                const r = await authFetch(`${API}/api/vagas/${id}`);
                if (r.ok) vagaMap[id] = await r.json();
            } catch (_) {}
        }));

        candidaturas.forEach(c => {
            const vaga = vagaMap[c.vagaId];
            const titulo = vaga?.titulo || `Vaga #${c.vagaId}`;
            const empresa = vaga?.empresa || vaga?.empresaId || '—';
            const data = c.criadoEm ? `Enviada ${formatarDataRelativa(c.criadoEm)}` : 'Candidatura enviada';

            const statusInfo = mapearStatus(c.status);

            const item = document.createElement('div');
            item.className = 'me-candidatura';
            item.setAttribute('role', 'listitem');
            item.innerHTML = `
                <div class="me-candidatura__info">
                    <strong>${titulo}</strong>
                    <span>${empresa} · ${data}</span>
                </div>
                <span class="me-status ${statusInfo.cls}" aria-label="Status: ${statusInfo.label}">${statusInfo.label}</span>
            `;
            lista.appendChild(item);
        });

        // Marca botões de vagas já candidatadas como "Enviada ✓"
        const vagasJaAplicadas = new Set(candidaturas.map(c => c.vagaId));
        document.querySelectorAll('.me-vaga').forEach(card => {
            const btn = card.querySelector('.me-btn--primary');
            if (!btn) return;
            // Tenta achar o vagaId pelo aria-label do botão ou pelo card dataset
            const vagaId = card.dataset.vagaId ? parseInt(card.dataset.vagaId) : null;
            if (vagaId && vagasJaAplicadas.has(vagaId)) {
                btn.textContent = 'Enviada ✓';
                btn.classList.remove('me-btn--primary');
                btn.classList.add('me-btn--secondary');
                btn.disabled = true;
            }
        });

    } catch (err) {
        console.warn('Erro ao carregar candidaturas:', err);
        if (empty) {
            empty.style.display = '';
            empty.querySelector('p').textContent = 'Não foi possível carregar suas candidaturas.';
        }
    }
}

function mapearStatus(status) {
    switch ((status || '').toLowerCase()) {
        case 'pendente':    return { cls: 'me-status--review',    label: 'Pendente' };
        case 'em_analise':
        case 'em análise':  return { cls: 'me-status--review',    label: 'Em análise' };
        case 'entrevista':  return { cls: 'me-status--interview', label: 'Entrevista' };
        case 'aprovado':    return { cls: 'me-status--ok',        label: 'Aprovado' };
        case 'reprovado':
        case 'encerrada':   return { cls: 'me-status--closed',    label: 'Encerrada' };
        default:            return { cls: 'me-status--review',    label: status || 'Em análise' };
    }
}

function formatarDataRelativa(isoDate) {
    try {
        const diff = Date.now() - new Date(isoDate).getTime();
        const dias = Math.floor(diff / 86400000);
        if (dias === 0) return 'hoje';
        if (dias === 1) return 'há 1 dia';
        if (dias < 7)  return `há ${dias} dias`;
        if (dias < 14) return 'há 1 semana';
        if (dias < 30) return `há ${Math.floor(dias / 7)} semanas`;
        return `há ${Math.floor(dias / 30)} meses`;
    } catch (_) { return ''; }
}

// ─── VER TODAS AS VAGAS ──────────────────────────────────────────────────────

async function verTodasVagas() {
    // Busca candidaturas já feitas para marcar botões corretamente
    let jaCandidata = new Set();
    try {
        const rc = await authFetch(`${API}/api/candidaturas/estudante/${currentSession?.id}`);
        if (rc.ok) {
            const cands = await rc.json();
            cands.forEach(c => jaCandidata.add(String(c.vagaId)));
        }
    } catch (_) {}

    showModal({
        title: 'Todas as vagas abertas',
        bodyHtml: `<div id="todasVagasLista" style="min-height:80px;display:flex;align-items:center;justify-content:center;">
            <span style="color:var(--gray-400);font-size:0.9rem">Carregando vagas…</span>
        </div>`
        // sem onSave → modal só exibe botão "Cancelar" (funciona como "Fechar")
    });

    try {
        const res = await authFetch(`${API}/api/vagas/abertas`);
        if (!res.ok) throw new Error();
        const vagas = await res.json();

        const container = document.getElementById('todasVagasLista');
        if (!container) return;

        if (!vagas.length) {
            container.innerHTML = `<p style="color:var(--gray-400);text-align:center;padding:24px 0">Nenhuma vaga aberta no momento.</p>`;
            return;
        }

        container.style.cssText = 'display:flex;flex-direction:column;gap:12px;';
        container.innerHTML = '';

        vagas.forEach(vaga => {
            const jaCandidatou = jaCandidata.has(String(vaga.id));
            const metaParts = [
                vaga.area   ? `Área: ${vaga.area}`        : null,
                vaga.cidade ? `Cidade: ${vaga.cidade}`     : null,
                vaga.nivel  ? vaga.nivel                   : null,
                vaga.bolsa  ? `R$ ${Number(vaga.bolsa).toLocaleString('pt-BR')}/mês` : null,
            ].filter(Boolean);

            const card = document.createElement('div');
            card.style.cssText = `
                display:flex;flex-direction:column;gap:8px;
                padding:14px 16px;border-radius:10px;
                background:var(--surface);border:1px solid var(--border);
            `;
            card.innerHTML = `
                <div style="display:flex;justify-content:space-between;align-items:flex-start;gap:8px;">
                    <div>
                        <div style="font-weight:600;font-size:0.95rem">${vaga.titulo}</div>
                        <div style="font-size:0.78rem;color:var(--gray-400);margin-top:2px">${vaga.empresaId || ''}</div>
                    </div>
                    <button class="me-btn ${jaCandidatou ? 'me-btn--secondary' : 'me-btn--primary'} me-btn--sm"
                            style="flex-shrink:0;font-size:0.78rem;padding:5px 12px"
                            ${jaCandidatou ? 'disabled' : ''}
                            data-vaga-id="${vaga.id}">
                        ${jaCandidatou ? 'Enviada ✓' : 'Candidatar-se'}
                    </button>
                </div>
                ${metaParts.length ? `
                <div style="display:flex;flex-wrap:wrap;gap:5px;">
                    ${metaParts.map(p => `<span class="me-badge" style="font-size:0.72rem">${p}</span>`).join('')}
                </div>` : ''}
                ${vaga.descricao ? `<p style="font-size:0.82rem;color:var(--gray-500);margin:0;line-height:1.5">${vaga.descricao.substring(0, 140)}${vaga.descricao.length > 140 ? '…' : ''}</p>` : ''}
            `;

            // Evento no botão de candidatura
            const btn = card.querySelector('button[data-vaga-id]');
            btn.addEventListener('click', async () => {
                await candidatarSe(btn, vaga);
                jaCandidata.add(String(vaga.id));
            });

            container.appendChild(card);
        });

    } catch (_) {
        const container = document.getElementById('todasVagasLista');
        if (container) container.innerHTML = `<p style="color:var(--error);text-align:center;padding:24px 0">Erro ao carregar vagas. Tente novamente.</p>`;
    }
}
