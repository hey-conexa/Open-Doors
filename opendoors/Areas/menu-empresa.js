/* Painel da Empresa — Open Doors */

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

let currentSession = null;
let currentEmpresa = null;

// ─── INIT ────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', async () => {
    try {
        // Token próprio do backend (salvo no login)
        const auth = getAuth();
        if (!auth || auth.tipoUsuario !== 'Empresa') {
            clearAuth();
            window.location.href = '../Acesso/AcessoEmpresas.html';
            return;
        }

        const raw = localStorage.getItem('od-session');
        const session = raw ? JSON.parse(raw) : null;
        if (!session || session.type !== 'company' || session.id !== auth.contaId) {
            clearAuth();
            window.location.href = '../Acesso/AcessoEmpresas.html';
            return;
        }

        currentSession = session;

        // Drawer nav
        const name = session.name || 'Empresa';
        document.getElementById('drawerUserName').textContent = name;
        document.getElementById('drawerAvatar').textContent   = name.charAt(0).toUpperCase();
        document.getElementById('profileBtnLabel').textContent = name.split(' ')[0];

        // Carrega dados em paralelo
        await carregarPerfil(session.id);
        await carregarVagas(session.id);
        atualizarStatCandidaturas(session.id);

    } catch (e) {
        console.warn('Erro ao inicializar painel:', e);
    }
});

// ─── PERFIL ──────────────────────────────────────────────────────────────────

async function carregarPerfil(empresaId) {
    try {
        const res = await authFetch(`${API}/api/empresas/${empresaId}`);
        if (!res.ok) return;
        currentEmpresa = await res.json();

        document.getElementById('heroName').textContent = currentEmpresa.nomeFantasia || currentEmpresa.razaoSocial || '—';
        document.getElementById('heroSub').textContent  = [
            currentEmpresa.setor,
            currentEmpresa.cidade && currentEmpresa.estado
                ? `${currentEmpresa.cidade}, ${currentEmpresa.estado}`
                : currentEmpresa.cidade
        ].filter(Boolean).join(' · ') || 'Sem informações adicionais';

        document.getElementById('statContratacoes').textContent = currentEmpresa.totalContratacoes ?? 0;
    } catch (e) {
        console.warn('Erro ao carregar perfil:', e);
    }
}

// ─── VAGAS ───────────────────────────────────────────────────────────────────

async function carregarVagas(empresaId) {
    const lista = document.getElementById('vagasLista');
    const empty = document.getElementById('vagasEmpty');
    if (!lista) return;

    try {
        const res = await authFetch(`${API}/api/vagas/empresa/${empresaId}`);
        if (!res.ok) throw new Error('Erro ao carregar vagas');
        const vagas = await res.json();

        // Remove cards anteriores
        Array.from(lista.querySelectorAll('.em-vaga')).forEach(el => el.remove());

        const ativas = vagas.filter(v => v.status === 'aberta').length;
        document.getElementById('statVagas').textContent = ativas;

        if (!vagas.length) {
            if (empty) empty.style.display = '';
            return;
        }
        if (empty) empty.style.display = 'none';

        vagas.forEach(vaga => lista.appendChild(criarCardVaga(vaga)));

    } catch (e) {
        console.warn('Erro ao carregar vagas:', e);
    }
}

async function atualizarStatCandidaturas(empresaId) {
    try {
        const res = await authFetch(`${API}/api/candidaturas/empresa/${empresaId}`);
        if (!res.ok) return;
        const candidaturas = await res.json();
        const el = document.getElementById('statCandidaturas');
        if (el) el.textContent = candidaturas.length;
    } catch (e) { /* silencioso */ }
}

function criarCardVaga(vaga) {
    const isAberta = vaga.status === 'aberta';
    const card = document.createElement('div');
    card.className = 'em-vaga';
    card.setAttribute('role', 'listitem');
    card.dataset.vagaId = vaga.id;

    const metaParts = [
        vaga.area,
        vaga.cidade,
        vaga.modalidade,
        vaga.bolsa ? `R$ ${Number(vaga.bolsa).toLocaleString('pt-BR')}/mês` : null
    ].filter(Boolean);

    card.innerHTML = `
        <div class="em-vaga__main">
            <div class="em-vaga__title">${vaga.titulo}</div>
            <div class="em-vaga__meta">
                ${metaParts.map(p => `<span>${p}</span>`).join('')}
            </div>
        </div>
        <span class="em-badge ${isAberta ? 'em-badge--open' : 'em-badge--closed'}">
            ${isAberta ? 'Aberta' : 'Encerrada'}
        </span>
        <button class="em-cand-badge" id="badge-vaga-${vaga.id}"
                onclick="verCandidatos(${vaga.id},'${escHtml(vaga.titulo)}')"
                title="Ver candidatos" aria-label="Ver candidatos desta vaga">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 00-3-3.87"/><path d="M16 3.13a4 4 0 010 7.75"/></svg>
            <span id="badge-count-${vaga.id}">…</span>
        </button>
        <div class="em-vaga__actions">
            <button class="em-btn em-btn--ghost em-btn--sm" onclick="abrirModalVaga(${vaga.id})" aria-label="Editar vaga ${escHtml(vaga.titulo)}">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M11 4H4a2 2 0 00-2 2v14a2 2 0 002 2h14a2 2 0 002-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 013 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                Editar
            </button>
            <button class="em-btn ${isAberta ? 'em-btn--danger' : 'em-btn--ghost'} em-btn--sm"
                    onclick="${isAberta ? `fecharVaga(${vaga.id})` : `reabrirVaga(${vaga.id})`}"
                    aria-label="${isAberta ? 'Encerrar' : 'Reabrir'} vaga ${escHtml(vaga.titulo)}">
                ${isAberta ? 'Encerrar' : 'Reabrir'}
            </button>
        </div>
    `;

    // Busca contagem real de candidatos em background
    authFetch(`${API}/api/candidaturas/vaga/${vaga.id}`)
        .then(r => r.ok ? r.json() : [])
        .then(cands => {
            const n = Array.isArray(cands) ? cands.length : 0;
            const el = document.getElementById(`badge-count-${vaga.id}`);
            if (el) el.textContent = n;
            const badge = document.getElementById(`badge-vaga-${vaga.id}`);
            if (badge) badge.classList.toggle('em-cand-badge--has', n > 0);
        })
        .catch(() => {
            const el = document.getElementById(`badge-count-${vaga.id}`);
            if (el) el.textContent = '0';
        });

    return card;
}

async function fecharVaga(vagaId) {
    if (!confirm('Encerrar esta vaga? Ela ficará visível mas não receberá novas candidaturas.')) return;
    try {
        const res = await authFetch(`${API}/api/vagas/${vagaId}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify('encerrada')
        });
        if (!res.ok) throw new Error();
        await carregarVagas(currentSession.id);
    } catch {
        alert('Erro ao encerrar a vaga. Tente novamente.');
    }
}

async function reabrirVaga(vagaId) {
    try {
        const res = await authFetch(`${API}/api/vagas/${vagaId}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify('aberta')
        });
        if (!res.ok) throw new Error();
        await carregarVagas(currentSession.id);
    } catch {
        alert('Erro ao reabrir a vaga. Tente novamente.');
    }
}

// ─── CANDIDATURAS DA EMPRESA ─────────────────────────────────────────────────

async function carregarCandidaturasEmpresa(empresaId) {
    const lista = document.getElementById('candidaturasLista');
    const empty = document.getElementById('candidaturasEmpty');
    if (!lista) return;

    try {
        const res = await authFetch(`${API}/api/candidaturas/empresa/${empresaId}`);
        if (!res.ok) throw new Error();
        const candidaturas = await res.json();

        Array.from(lista.querySelectorAll('.em-candidatura')).forEach(el => el.remove());

        document.getElementById('statCandidaturas').textContent = candidaturas.length;

        if (!candidaturas.length) {
            if (empty) empty.style.display = '';
            return;
        }
        if (empty) empty.style.display = 'none';

        // Enriquecer com dados das vagas e dos estudantes em paralelo
        const vagaIds      = [...new Set(candidaturas.map(c => c.vagaId).filter(Boolean))];
        const estudanteIds = [...new Set(candidaturas.map(c => c.estudanteId).filter(Boolean))];
        const vagaMap = {};
        const estMap  = {};

        await Promise.all([
            ...vagaIds.map(async id => {
                try {
                    const r = await authFetch(`${API}/api/vagas/${id}`);
                    if (r.ok) vagaMap[id] = await r.json();
                } catch (_) {}
            }),
            ...estudanteIds.map(async id => {
                try {
                    const r = await authFetch(`${API}/api/estudantes/${id}`);
                    if (r.ok) estMap[id] = await r.json();
                } catch (_) {}
            })
        ]);

        // Ordena por mais recente
        candidaturas.sort((a, b) => new Date(b.criadoEm) - new Date(a.criadoEm));

        candidaturas.slice(0, 20).forEach(c => {
            const vaga    = vagaMap[c.vagaId];
            const est     = estMap[c.estudanteId] || {};
            const vagaTit = vaga?.titulo || `Vaga #${c.vagaId}`;
            const data    = c.criadoEm ? formatarDataRelativa(c.criadoEm) : '—';
            const status  = mapearStatus(c.status);
            const nome    = est.nome || 'Candidato';
            const inicial = nome.charAt(0).toUpperCase();
            const foto    = est.fotoPerfilUrl;

            const avatarHtml = foto
                ? `<img src="${escHtml(foto)}" alt="${escHtml(nome)}"
                        class="em-candidatura__avatar"
                        style="object-fit:cover"
                        onerror="this.outerHTML='<div class=\\'em-candidatura__avatar\\'>${inicial}</div>'">`
                : `<div class="em-candidatura__avatar">${inicial}</div>`;

            const subInfo = [est.curso, est.instituicao].filter(Boolean).join(' · ');

            const item = document.createElement('div');
            item.className = 'em-candidatura';
            item.setAttribute('role', 'listitem');
            item.innerHTML = `
                ${avatarHtml}
                <div class="em-candidatura__info">
                    <div class="em-candidatura__name">${escHtml(nome)}</div>
                    <div class="em-candidatura__meta">
                        ${subInfo ? `<span>${escHtml(subInfo)}</span> · ` : ''}
                        <strong style="color:var(--text)">${escHtml(vagaTit)}</strong>
                        · ${data}
                    </div>
                </div>
                <span class="em-status ${status.cls}">${status.label}</span>
                <div style="display:flex;gap:6px;flex-shrink:0;flex-wrap:wrap">
                    <button class="em-btn em-btn--ghost em-btn--sm" onclick="verPerfilCandidato('${c.estudanteId}')" title="Ver perfil completo">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/></svg>
                        Perfil
                    </button>
                    <button class="em-btn em-btn--ghost em-btn--sm" onclick="atualizarStatusCandidatura(${c.id},'entrevista')" title="Chamar para entrevista">Entrevista</button>
                    <button class="em-btn em-btn--ghost em-btn--sm" onclick="atualizarStatusCandidatura(${c.id},'aprovado')" title="Aprovar candidato">Aprovar</button>
                </div>
            `;
            lista.appendChild(item);
        });

        if (candidaturas.length > 20) {
            const more = document.createElement('div');
            more.style.cssText = 'text-align:center;padding:12px;font-size:0.8rem;color:var(--gray-400)';
            more.textContent = `+ ${candidaturas.length - 20} candidaturas. Use o painel completo para ver todas.`;
            lista.appendChild(more);
        }

    } catch (e) {
        console.warn('Erro ao carregar candidaturas:', e);
    }
}

async function atualizarStatusCandidatura(candidaturaId, novoStatus) {
    try {
        const res = await authFetch(`${API}/api/candidaturas/${candidaturaId}/status`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(novoStatus)
        });
        if (!res.ok) throw new Error();
        await carregarCandidaturasEmpresa(currentSession.id);
    } catch {
        alert('Erro ao atualizar status. Tente novamente.');
    }
}

// ─── MODAL VER CANDIDATOS ────────────────────────────────────────────────────

async function verCandidatos(vagaId, vagaTitulo) {
    abrirModal(`Candidatos — ${vagaTitulo}`);
    const body = document.getElementById('emModalBody');
    body.innerHTML = '<p style="color:var(--gray-400);font-size:0.88rem">Carregando...</p>';

    try {
        const res = await authFetch(`${API}/api/candidaturas/vaga/${vagaId}`);
        if (!res.ok) throw new Error();
        const candidaturas = await res.json();

        if (!candidaturas.length) {
            body.innerHTML = `
                <div style="text-align:center;padding:32px;color:var(--gray-400)">
                    <p style="font-size:0.9rem;font-weight:500">Nenhuma candidatura recebida para esta vaga ainda.</p>
                </div>`;
            document.getElementById('emModalFooter').innerHTML = `<button class="em-btn em-btn--ghost" onclick="fecharModal()">Fechar</button>`;
            return;
        }

        // Busca perfis e scores em paralelo
        const perfilMap = {};
        const scoreMap  = {};

        const [_, matchesRes] = await Promise.all([
            Promise.all(candidaturas.map(async c => {
                if (!c.estudanteId) return;
                try {
                    const r = await authFetch(`${API}/api/estudantes/${c.estudanteId}`);
                    if (r.ok) perfilMap[c.estudanteId] = await r.json();
                } catch (_) {}
            })),
            authFetch(`${API}/api/matches/vaga/${vagaId}`).catch(() => null)
        ]);

        if (matchesRes?.ok) {
            const matches = await matchesRes.json().catch(() => []);
            matches.forEach(m => { scoreMap[m.estudanteId] = m; });
        }

        // Ordena por score decrescente
        candidaturas.sort((a, b) => {
            const sa = scoreMap[a.estudanteId]?.scoreTotal ?? -1;
            const sb = scoreMap[b.estudanteId]?.scoreTotal ?? -1;
            return sb - sa;
        });

        body.innerHTML = `
            <p style="font-size:0.82rem;color:var(--gray-400);margin-bottom:14px">${candidaturas.length} candidatura${candidaturas.length !== 1 ? 's' : ''} recebida${candidaturas.length !== 1 ? 's' : ''}</p>
            <div style="display:flex;flex-direction:column;gap:12px" id="candidatosVaga"></div>`;

        const container = body.querySelector('#candidatosVaga');

        candidaturas.forEach(c => {
            const status  = mapearStatus(c.status);
            const data    = c.criadoEm ? formatarDataRelativa(c.criadoEm) : '—';
            const est     = perfilMap[c.estudanteId] || {};
            const score   = scoreMap[c.estudanteId];
            const nome    = est.nome || 'Candidato';
            const inicial = nome.charAt(0).toUpperCase();
            const foto    = est.fotoPerfilUrl;

            const avatarHtml = foto
                ? `<img src="${escHtml(foto)}" alt="${escHtml(nome)}"
                        style="width:42px;height:42px;border-radius:50%;object-fit:cover;flex-shrink:0"
                        onerror="this.outerHTML='<div style=\\'width:42px;height:42px;border-radius:50%;background:var(--primary);color:#fff;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:1rem;flex-shrink:0\\'>${inicial}</div>'">`
                : `<div style="width:42px;height:42px;border-radius:50%;background:var(--primary);color:#fff;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:1rem;flex-shrink:0">${inicial}</div>`;

            const subInfo = [est.curso, est.instituicao, est.cidade].filter(Boolean).join(' · ');

            // Score badge
            // Container sempre existe — polling atualiza mesmo quando score ainda não existe
            let scoreBadgeHtml = `<div id="score-block-${c.estudanteId}" style="border:1px solid var(--border);border-radius:8px;padding:10px 12px;background:var(--surface);font-size:0.78rem;color:var(--gray-400)">Score ainda não calculado — atualizando…</div>`;
            if (score) {
                const pct   = Math.round(Number(score.scoreTotal ?? 0));
                const color = pct >= 80 ? '#22c55e' : pct >= 60 ? '#3b82f6' : pct >= 40 ? '#f59e0b' : '#ef4444';
                const label = pct >= 80 ? 'Ótimo match' : pct >= 60 ? 'Bom match' : pct >= 40 ? 'Match razoável' : 'Match baixo';

                // Mini barras de sub-scores
                const subScores = [
                    { label: 'Currículo',   val: Math.round(Number(score.scoreCurriculo  ?? 0)) },
                    { label: 'Vocacional',  val: Math.round(Number(score.scoreVocacional ?? 0)) },
                    { label: 'Habilidades', val: Math.round(Number(score.scoreHabilidades ?? 0)) },
                ].map(s => `
                    <div style="display:flex;align-items:center;gap:6px;font-size:0.72rem;color:var(--gray-400)">
                        <span style="min-width:68px">${s.label}</span>
                        <div style="flex:1;height:4px;border-radius:99px;background:var(--border);overflow:hidden">
                            <div style="width:${s.val}%;height:100%;background:${color};border-radius:99px"></div>
                        </div>
                        <span style="min-width:28px;text-align:right;color:var(--text)">${s.val}%</span>
                    </div>`).join('');

                // Sobrescreve o placeholder com os dados reais (o id já está no placeholder)
                scoreBadgeHtml = `
                    <div id="score-block-${c.estudanteId}" style="border:1px solid var(--border);border-radius:8px;padding:10px 12px;background:var(--surface)">
                        <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px">
                            <span style="font-size:1.1rem;font-weight:800;color:${color}">${pct}%</span>
                            <span style="font-size:0.75rem;font-weight:600;color:${color}">${label}</span>
                            ${score.justificativa ? `<span style="font-size:0.72rem;color:var(--gray-400);flex:1;text-align:right;overflow:hidden;text-overflow:ellipsis;white-space:nowrap" title="${escHtml(score.justificativa)}">${escHtml(score.justificativa.substring(0,60))}…</span>` : ''}
                        </div>
                        ${subScores}
                    </div>`;
            }

            const item = document.createElement('div');
            item.style.cssText = 'display:flex;flex-direction:column;gap:10px;padding:14px;border:1px solid var(--border);border-radius:10px;';
            item.innerHTML = `
                <div style="display:flex;align-items:center;gap:12px;flex-wrap:wrap">
                    ${avatarHtml}
                    <div style="flex:1;min-width:0">
                        <div style="font-size:0.92rem;font-weight:600;color:var(--text)">${escHtml(nome)}</div>
                        ${subInfo ? `<div style="font-size:0.76rem;color:var(--gray-400);margin-top:2px">${escHtml(subInfo)}</div>` : ''}
                        <div style="font-size:0.74rem;color:var(--gray-400)">Candidatura enviada ${data}</div>
                    </div>
                    <span class="em-status ${status.cls}">${status.label}</span>
                </div>

                ${scoreBadgeHtml}

                <div style="display:flex;align-items:center;gap:8px;flex-wrap:wrap">
                    <button class="em-btn em-btn--ghost em-btn--sm" onclick="verPerfilCandidato('${c.estudanteId}')">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/></svg>
                        Ver perfil
                    </button>
                    <button class="em-btn em-btn--ghost em-btn--sm" onclick="atualizarStatusCandidatura(${c.id},'entrevista')">Entrevista</button>
                    <button class="em-btn em-btn--ghost em-btn--sm" onclick="atualizarStatusCandidatura(${c.id},'aprovado')">Aprovar</button>
                </div>`;

            container.appendChild(item);
        });

        // Footer com botão Recalcular e Fechar
        document.getElementById('emModalFooter').innerHTML = `
            <button class="em-btn em-btn--ghost em-btn--sm" id="btnRecalcularScores"
                    onclick="recalcularScoresModal(${vagaId})"
                    title="Buscar scores atualizados agora">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"/><polyline points="1 20 1 14 7 14"/><path d="M3.51 9a9 9 0 0114.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0020.49 15"/></svg>
                Atualizar scores
            </button>
            <button class="em-btn em-btn--ghost" onclick="fecharModal()">Fechar</button>`;

        // Polling leve: atualiza scores a cada 20s enquanto modal estiver aberto
        _scorePollingVagaId = vagaId;
        if (_scorePollingTimer) clearInterval(_scorePollingTimer);
        _scorePollingTimer = setInterval(() => recalcularScoresModal(vagaId, true), 20_000);

    } catch (e) {
        body.innerHTML = `<p style="color:var(--error)">Erro ao carregar candidatos.</p>`;
    }
}

// Polling interno do modal de scores
let _scorePollingTimer = null;
let _scorePollingVagaId = null;

async function recalcularScoresModal(vagaId, silencioso = false) {
    // Se o modal foi fechado ou trocou de vaga, para o polling
    if (document.getElementById('emModalBackdrop')?.hidden || _scorePollingVagaId !== vagaId) {
        clearInterval(_scorePollingTimer);
        _scorePollingTimer = null;
        return;
    }

    const btn = document.getElementById('btnRecalcularScores');
    if (!silencioso && btn) { btn.disabled = true; btn.textContent = 'Recalculando…'; }

    try {
        // Coleta todos os estudanteIds visíveis no modal
        const blocos = document.querySelectorAll('[id^="score-block-"]');
        const estudanteIds = Array.from(blocos).map(el => el.id.replace('score-block-', ''));

        // Dispara regeneração de score para cada candidato em paralelo
        if (!silencioso && estudanteIds.length) {
            await Promise.all(estudanteIds.map(estudanteId =>
                authFetch(`${API}/api/ia/gerar-score`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ estudanteId, vagaId: Number(vagaId) })
                }).catch(() => {})
            ));
        }

        // Busca scores atualizados
        const r = await authFetch(`${API}/api/matches/vaga/${vagaId}`);
        if (!r.ok) return;
        const matches = await r.json();

        matches.forEach(m => {
            const container = document.getElementById(`score-block-${m.estudanteId}`);
            if (!container) return;

            const pct   = Math.round(Number(m.scoreTotal ?? 0));
            const color = pct >= 80 ? '#22c55e' : pct >= 60 ? '#3b82f6' : pct >= 40 ? '#f59e0b' : '#ef4444';
            const label = pct >= 80 ? 'Ótimo match' : pct >= 60 ? 'Bom match' : pct >= 40 ? 'Match razoável' : 'Match baixo';

            const subScores = [
                { label: 'Currículo',   val: Math.round(Number(m.scoreCurriculo  ?? 0)) },
                { label: 'Vocacional',  val: Math.round(Number(m.scoreVocacional ?? 0)) },
                { label: 'Habilidades', val: Math.round(Number(m.scoreHabilidades ?? 0)) },
            ];

            container.innerHTML = `
                <div style="display:flex;align-items:center;gap:8px;margin-bottom:8px">
                    <span style="font-size:1.1rem;font-weight:800;color:${color}">${pct}%</span>
                    <span style="font-size:0.75rem;font-weight:600;color:${color}">${label}</span>
                    ${m.justificativa ? `<span style="font-size:0.72rem;color:var(--gray-400);flex:1;text-align:right;overflow:hidden;text-overflow:ellipsis;white-space:nowrap" title="${escHtml(m.justificativa)}">${escHtml(m.justificativa.substring(0,60))}…</span>` : ''}
                </div>
                ${subScores.map(s => `
                    <div style="display:flex;align-items:center;gap:6px;font-size:0.72rem;color:var(--gray-400)">
                        <span style="min-width:68px">${s.label}</span>
                        <div style="flex:1;height:4px;border-radius:99px;background:var(--border);overflow:hidden">
                            <div style="width:${s.val}%;height:100%;background:${color};border-radius:99px;transition:width 0.4s ease"></div>
                        </div>
                        <span style="min-width:28px;text-align:right;color:var(--text)">${s.val}%</span>
                    </div>`).join('')}`;
        });
    } catch (_) {}

    if (!silencioso && btn) {
        btn.disabled = false;
        btn.innerHTML = `<svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="23 4 23 10 17 10"/><polyline points="1 20 1 14 7 14"/><path d="M3.51 9a9 9 0 0114.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0020.49 15"/></svg> Atualizar scores`;
    }
}

async function verPerfilCandidato(estudanteId) {
    if (!estudanteId) return;

    // Abre novo modal sobre o atual
    const backdrop = document.getElementById('emModalBackdrop');
    const title    = document.getElementById('emModalTitle');
    const body     = document.getElementById('emModalBody');
    const footer   = document.getElementById('emModalFooter');

    title.textContent = 'Carregando perfil…';
    body.innerHTML    = '<p style="color:var(--gray-400);font-size:0.88rem;padding:16px 0">Buscando dados do candidato…</p>';
    footer.innerHTML  = `<button class="em-btn em-btn--ghost" onclick="fecharModal()">Fechar</button>`;

    try {
        const res = await authFetch(`${API}/api/estudantes/${estudanteId}`);
        if (!res.ok) throw new Error('Perfil não encontrado');
        const est = await res.json();

        const nome    = est.nome || 'Candidato';
        const inicial = nome.charAt(0).toUpperCase();
        const foto    = est.fotoPerfilUrl;

        const avatarHtml = foto
            ? `<img src="${escHtml(foto)}" alt="${escHtml(nome)}"
                    style="width:56px;height:56px;border-radius:50%;object-fit:cover"
                    onerror="this.style.display='none'">`
            : `<div style="width:56px;height:56px;border-radius:50%;background:var(--primary);color:#fff;display:flex;align-items:center;justify-content:center;font-weight:700;font-size:1.4rem">${inicial}</div>`;

        const habs = Array.isArray(est.habilidadesExtraidas) && est.habilidadesExtraidas.length
            ? est.habilidadesExtraidas.map(h => `<span style="
                    font-size:0.74rem;padding:3px 10px;border-radius:99px;
                    background:color-mix(in srgb,var(--primary) 12%,transparent);
                    color:var(--primary);border:1px solid color-mix(in srgb,var(--primary) 25%,transparent)
                ">${escHtml(h)}</span>`).join('')
            : '<span style="color:var(--gray-400);font-size:0.82rem">Nenhuma habilidade cadastrada</span>';

        title.textContent = `Perfil — ${nome}`;
        body.innerHTML = `
            <div style="display:flex;align-items:center;gap:14px;margin-bottom:18px">
                ${avatarHtml}
                <div>
                    <div style="font-size:1rem;font-weight:700">${escHtml(nome)}</div>
                    ${est.curso       ? `<div style="font-size:0.8rem;color:var(--gray-400)">${escHtml(est.curso)}${est.semestre ? ` · ${est.semestre}º sem.` : ''}</div>` : ''}
                    ${est.instituicao ? `<div style="font-size:0.78rem;color:var(--gray-400)">${escHtml(est.instituicao)}</div>` : ''}
                    ${est.cidade      ? `<div style="font-size:0.78rem;color:var(--gray-400)">${escHtml(est.cidade)}${est.estado ? `, ${est.estado}` : ''}</div>` : ''}
                </div>
            </div>

            <div style="margin-bottom:16px">
                <div style="font-size:0.78rem;font-weight:600;text-transform:uppercase;letter-spacing:.05em;color:var(--gray-400);margin-bottom:8px">Habilidades</div>
                <div style="display:flex;flex-wrap:wrap;gap:6px">${habs}</div>
            </div>

            <div>
                <div style="font-size:0.78rem;font-weight:600;text-transform:uppercase;letter-spacing:.05em;color:var(--gray-400);margin-bottom:8px">Currículo</div>
                ${est.curriculoUrl || est.temCurriculo
                    ? `<button type="button" onclick="abrirCurriculo('${escHtml(est.id)}', this)"
                            class="em-btn em-btn--ghost em-btn--sm">
                            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/></svg>
                            Abrir currículo (PDF)
                        </button>`
                    : '<span style="color:var(--gray-400);font-size:0.82rem">Currículo não enviado ainda</span>'
                }
            </div>`;

    } catch (e) {
        body.innerHTML = `<p style="color:var(--error)">Não foi possível carregar o perfil.</p>`;
    }
}

// Baixa o PDF do currículo pela API (precisa do token) e abre em outra aba
async function abrirCurriculo(estudanteId, btn) {
    // Abre a aba já no clique: navegadores bloqueiam abas abertas depois de um await
    const aba = window.open('', '_blank');
    if (btn) btn.disabled = true;
    try {
        const res = await authFetch(`${API}/api/estudantes/${estudanteId}/curriculo`);
        if (!res.ok) throw new Error(res.status === 404 ? 'Este estudante ainda não enviou currículo.' : `Erro ${res.status}`);
        const url = URL.createObjectURL(await res.blob());
        if (aba) {
            aba.location.href = url;
        } else {
            // Abas bloqueadas pelo navegador: baixa o arquivo em vez de sair do painel
            const link = document.createElement('a');
            link.href = url;
            link.download = 'curriculo.pdf';
            document.body.appendChild(link);
            link.click();
            link.remove();
        }
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
    } catch (e) {
        if (aba) aba.close();
        alert(e.message || 'Não foi possível abrir o currículo.');
    } finally {
        if (btn) btn.disabled = false;
    }
}

// ─── MODAL NOVA / EDITAR VAGA ────────────────────────────────────────────────

async function abrirModalVaga(vagaId = null) {
    let vaga = null;

    if (vagaId) {
        try {
            const res = await authFetch(`${API}/api/vagas/${vagaId}`);
            if (res.ok) vaga = await res.json();
        } catch (_) {}
    }

    abrirModal(vaga ? 'Editar vaga' : 'Publicar nova vaga');

    const body   = document.getElementById('emModalBody');
    const footer = document.getElementById('emModalFooter');

    const v = (field) => vaga?.[field] ?? '';

    body.innerHTML = `
        <form class="em-form" id="vagaForm" onsubmit="return false">
            <div class="em-form__row">
                <div class="em-field" style="grid-column:1/-1">
                    <label for="vf-titulo">Título da vaga *</label>
                    <input type="text" id="vf-titulo" placeholder="Ex: Estágio em Marketing Digital" value="${escHtml(v('titulo'))}" required>
                </div>
            </div>
            <div class="em-form__row">
                <div class="em-field">
                    <label for="vf-area">Área</label>
                    <select id="vf-area">
                        ${opcoesArea(v('area'))}
                    </select>
                </div>
                <div class="em-field">
                    <label for="vf-nivel">Nível</label>
                    <select id="vf-nivel">
                        <option value="">Selecione</option>
                        ${['Estágio','Jovem Aprendiz','Trainee','Júnior'].map(n => `<option value="${n}" ${v('nivel') === n ? 'selected' : ''}>${n}</option>`).join('')}
                    </select>
                </div>
            </div>
            <div class="em-form__row">
                <div class="em-field">
                    <label for="vf-modalidade">Modalidade</label>
                    <select id="vf-modalidade">
                        <option value="">Selecione</option>
                        ${['Presencial','Remoto','Híbrido'].map(m => `<option value="${m}" ${v('modalidade') === m ? 'selected' : ''}>${m}</option>`).join('')}
                    </select>
                </div>
                <div class="em-field">
                    <label for="vf-cargaHoraria">Carga horária</label>
                    <input type="text" id="vf-cargaHoraria" placeholder="Ex: 6h/dia, 30h/semana" value="${escHtml(v('cargaHoraria'))}">
                </div>
            </div>
            <div class="em-form__row">
                <div class="em-field">
                    <label for="vf-cidade">Cidade</label>
                    <input type="text" id="vf-cidade" placeholder="Ex: São Paulo" value="${escHtml(v('cidade'))}">
                </div>
                <div class="em-field">
                    <label for="vf-bolsa">Bolsa (R$)</label>
                    <input type="number" id="vf-bolsa" placeholder="Ex: 1200" min="0" step="50" value="${v('bolsa') ?? ''}">
                </div>
            </div>
            <div class="em-field">
                <label for="vf-descricao">Descrição da vaga</label>
                <textarea id="vf-descricao" placeholder="Descreva as atividades, requisitos e benefícios...">${escHtml(v('descricao'))}</textarea>
            </div>
            <div class="em-field">
                <label>Habilidades requeridas</label>
                <div class="em-skills-grid" id="vf-habilidades" role="group" aria-label="Habilidades requeridas">
                    ${HABILIDADES_OPCOES.map((h, i) => {
                        const checked = Array.isArray(vaga?.habilidadesRequeridas) && vaga.habilidadesRequeridas.includes(h) ? 'checked' : '';
                        return `<label class="em-skill-option" for="vf-hab-${i}">
                            <input type="checkbox" id="vf-hab-${i}" value="${escHtml(h)}" ${checked}>
                            <span>${escHtml(h)}</span>
                        </label>`;
                    }).join('')}
                </div>
                <span class="em-field-hint">Selecione as habilidades que o candidato ideal deve ter</span>
            </div>
            <div class="em-form__row">
                <div class="em-field">
                    <label for="vf-vagasDisp">Vagas disponíveis</label>
                    <input type="number" id="vf-vagasDisp" placeholder="1" min="1" value="${v('vagasDisponiveis') || 1}">
                </div>
                <div class="em-field">
                    <label for="vf-expira">Expira em</label>
                    <input type="date" id="vf-expira" value="${v('expiraEm') ? v('expiraEm').substring(0,10) : ''}">
                </div>
            </div>
            <span id="vaga-form-error" style="font-size:12px;color:var(--error);display:none"></span>
        </form>`;

    footer.innerHTML = `
        <button class="em-btn em-btn--ghost" onclick="fecharModal()">Cancelar</button>
        <button class="em-btn em-btn--primary" id="btnSalvarVaga" onclick="salvarVaga(${vagaId || 'null'})">
            ${vaga ? 'Salvar alterações' : 'Publicar vaga'}
        </button>`;
}

async function salvarVaga(vagaId) {
    const btn = document.getElementById('btnSalvarVaga');
    const errEl = document.getElementById('vaga-form-error');

    const titulo = document.getElementById('vf-titulo').value.trim();
    if (!titulo) {
        errEl.textContent = 'O título é obrigatório.';
        errEl.style.display = 'block';
        document.getElementById('vf-titulo').focus();
        return;
    }
    errEl.style.display = 'none';

    const habilidades = Array.from(
        document.querySelectorAll('#vf-habilidades input[type="checkbox"]:checked')
    ).map(cb => cb.value);

    const expiraRaw = document.getElementById('vf-expira').value;

    const payload = {
        empresaId:           currentSession.id,
        titulo,
        area:                document.getElementById('vf-area').value || null,
        nivel:               document.getElementById('vf-nivel').value || null,
        modalidade:          document.getElementById('vf-modalidade').value || null,
        cargaHoraria:        document.getElementById('vf-cargaHoraria').value.trim() || null,
        cidade:              document.getElementById('vf-cidade').value.trim() || null,
        bolsa:               parseFloat(document.getElementById('vf-bolsa').value) || null,
        descricao:           document.getElementById('vf-descricao').value.trim() || null,
        habilidadesRequeridas: habilidades,
        vagasDisponiveis:    parseInt(document.getElementById('vf-vagasDisp').value) || 1,
        expiraEm:            expiraRaw ? new Date(expiraRaw).toISOString() : null,
        status:              'aberta'
    };

    btn.disabled = true;
    btn.textContent = 'Salvando...';

    try {
        const url    = vagaId ? `${API}/api/vagas/${vagaId}` : `${API}/api/vagas`;
        const method = vagaId ? 'PUT' : 'POST';
        const res = await authFetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const err = await res.json().catch(() => ({}));
            throw new Error(err.erro || `Erro ${res.status}`);
        }

        fecharModal();
        await carregarVagas(currentSession.id);

    } catch (e) {
        errEl.textContent = e.message || 'Erro ao salvar a vaga.';
        errEl.style.display = 'block';
        btn.disabled = false;
        btn.textContent = vagaId ? 'Salvar alterações' : 'Publicar vaga';
    }
}

// ─── HELPERS DO MODAL ────────────────────────────────────────────────────────

function abrirModal(titulo) {
    document.getElementById('emModalTitle').textContent = titulo;
    document.getElementById('emModalBody').innerHTML    = '';
    document.getElementById('emModalFooter').innerHTML  = '';
    document.getElementById('emModalBackdrop').hidden   = false;
    document.getElementById('emModalBackdrop').addEventListener('click', onBackdropClick);
    document.addEventListener('keydown', onEscModal);
}

function fecharModal() {
    document.getElementById('emModalBackdrop').hidden = true;
    document.getElementById('emModalBackdrop').removeEventListener('click', onBackdropClick);
    document.removeEventListener('keydown', onEscModal);
    // Para o polling de scores ao fechar o modal
    if (_scorePollingTimer) { clearInterval(_scorePollingTimer); _scorePollingTimer = null; }
    _scorePollingVagaId = null;
}

function onBackdropClick(e) {
    if (e.target === document.getElementById('emModalBackdrop')) fecharModal();
}
function onEscModal(e) {
    if (e.key === 'Escape') fecharModal();
}

// ─── DRAWER NAV ──────────────────────────────────────────────────────────────

function toggleProfileDrawer() {
    const drawer  = document.getElementById('profileDrawer');
    const overlay = document.getElementById('profileOverlay');
    const btn     = document.getElementById('profileBtn');
    const isOpen  = drawer.classList.contains('open');
    drawer.classList.toggle('open', !isOpen);
    overlay.classList.toggle('open', !isOpen);
    btn.setAttribute('aria-expanded', String(!isOpen));
}

function closeProfileDrawer() {
    document.getElementById('profileDrawer').classList.remove('open');
    document.getElementById('profileOverlay').classList.remove('open');
    document.getElementById('profileBtn').setAttribute('aria-expanded', 'false');
}

async function doLogout() {
    clearAuth();
    window.location.href = '../OpenDoors.html';
}

// ─── HELPERS GERAIS ──────────────────────────────────────────────────────────

function mapearStatus(status) {
    switch ((status || '').toLowerCase()) {
        case 'pendente':    return { cls: 'em-status--pending',   label: 'Pendente' };
        case 'em_analise':
        case 'em análise':  return { cls: 'em-status--pending',   label: 'Em análise' };
        case 'entrevista':  return { cls: 'em-status--interview', label: 'Entrevista' };
        case 'aprovado':    return { cls: 'em-status--approved',  label: 'Aprovado' };
        default:            return { cls: 'em-status--closed',    label: status || 'Pendente' };
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

function escHtml(str) {
    return String(str || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}

function opcoesArea(selected) {
    const areas = [
        'Administração','Comercial / Vendas','Comunicação / Marketing',
        'Contabilidade / Finanças','Design / Criação','Educação',
        'Engenharia','Gestão de Projetos','Jurídico','Logística',
        'Recursos Humanos','Saúde','Tecnologia da Informação','Outros'
    ];
    return `<option value="">Selecione</option>` +
        areas.map(a => `<option value="${a}" ${selected === a ? 'selected' : ''}>${a}</option>`).join('');
}

// ─── POLLING DE CANDIDATURAS ─────────────────────────────────────────────────
// Verifica novas candidaturas a cada 30 segundos sem precisar recarregar a página

let _pollingInterval   = null;
let _ultimoTotalCand   = 0;
let _badgeEl           = null;

function iniciarPolling(empresaId) {
    if (_pollingInterval) return; // já rodando

    _badgeEl = document.getElementById('pollingBadge');

    _pollingInterval = setInterval(async () => {
        try {
            const res = await authFetch(`${API}/api/candidaturas/empresa/${empresaId}`);
            if (!res.ok) return;
            const candidaturas = await res.json();
            const total = candidaturas.length;

            // Atualiza stat geral
            const statEl = document.getElementById('statCandidaturas');
            if (statEl) statEl.textContent = total;

            if (total > _ultimoTotalCand) {
                const novas = total - _ultimoTotalCand;
                _ultimoTotalCand = total;

                // Atualiza badges de cada vaga
                await carregarVagas(empresaId);

                // Exibe notificação flutuante
                mostrarNotificacaoCandidatura(novas);
            }
        } catch (_) { /* silencioso — não interrompe o usuário */ }
    }, 30_000); // 30 segundos
}

function pararPolling() {
    if (_pollingInterval) {
        clearInterval(_pollingInterval);
        _pollingInterval = null;
    }
}

function mostrarNotificacaoCandidatura(qtd) {
    // Atualiza badge no header da seção "Candidaturas recebidas"
    const badge = document.getElementById('pollingBadge');
    if (!badge) return;
    badge.textContent = `+${qtd} nova${qtd > 1 ? 's' : ''}`;
    badge.hidden = false;

    // Pisca o badge por 8 segundos e some
    setTimeout(() => { badge.hidden = true; }, 8_000);
}

// Inicia o polling após o carregamento inicial (aguarda 5s para não duplicar)
document.addEventListener('DOMContentLoaded', () => {
    setTimeout(() => {
        if (currentSession?.id) {
            // Salva contagem atual antes de começar a vigiar
            const statEl = document.getElementById('statCandidaturas');
            _ultimoTotalCand = parseInt(statEl?.textContent || '0', 10);
            iniciarPolling(currentSession.id);
        }
    }, 5_000);
});

// Para o polling quando o usuário sai da aba (economiza recursos)
document.addEventListener('visibilitychange', () => {
    if (document.hidden) {
        pararPolling();
    } else if (currentSession?.id) {
        // Volta à aba — busca imediatamente e reinicia polling
        carregarCandidaturasEmpresa(currentSession.id);
        iniciarPolling(currentSession.id);
    }
});
