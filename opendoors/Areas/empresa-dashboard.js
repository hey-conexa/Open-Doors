/* ============================================================
   Painel de dados da empresa (estilo BI) — MenuEmpresa.html

   Lê da API: vagas da empresa, candidaturas recebidas, matches
   (scores da IA) por vaga e o perfil dos candidatos (habilidades).
   Filtros (período e vaga) valem para todos os indicadores e gráficos.

   Gráficos desenhados à mão (SVG/HTML), sem biblioteca externa.
   Nomes vindos da API (vagas, habilidades) entram com textContent.
   Depende de api-client.js (API, authFetch, getAuth).
   ============================================================ */
(function () {
    'use strict';

    const raiz = document.getElementById('dashboard');
    if (!raiz) return;

    const MAX_CANDIDATOS_PERFIL = 80;   // perfis buscados para o gráfico de habilidades
    const DIA = 86400000;

    const ETAPAS = [
        { chave: 'recebidas',  rotulo: 'Recebidas',  status: null },
        { chave: 'analise',    rotulo: 'Em análise', status: ['em_analise', 'em análise', 'entrevista', 'aprovado'] },
        { chave: 'entrevista', rotulo: 'Entrevista', status: ['entrevista', 'aprovado'] },
        { chave: 'aprovadas',  rotulo: 'Aprovadas',  status: ['aprovado'] }
    ];

    const estado = {
        vagas: [],
        candidaturas: [],
        matches: new Map(),     // `${vagaId}|${estudanteId}` -> match
        perfis: new Map(),      // estudanteId -> estudante
        periodo: '30',
        vaga: 'todas',
        carregando: false
    };

    // ------------------------------------------------------------
    // Utilidades
    // ------------------------------------------------------------
    const $ = (sel, ctx = raiz) => ctx.querySelector(sel);
    const fmtInt = n => Number(n || 0).toLocaleString('pt-BR');
    const fmtPct = n => `${Math.round(n)}%`;
    const normalizar = s => String(s || '').trim().toLowerCase();

    function el(tag, className, text) {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined && text !== null) node.textContent = text;
        return node;
    }

    function svgEl(tag, attrs = {}) {
        const node = document.createElementNS('http://www.w3.org/2000/svg', tag);
        Object.entries(attrs).forEach(([k, v]) => node.setAttribute(k, v));
        return node;
    }

    // O backend grava em UTC numa coluna sem fuso ("2026-11-24T22:10:00"):
    // sem o "Z", o navegador leria como horário local e erraria o dia
    function data(iso) {
        if (!iso) return null;
        const texto = String(iso);
        const comFuso = /([zZ]|[+-]\d{2}:?\d{2})$/.test(texto) ? texto : texto + 'Z';
        const d = new Date(comFuso);
        return isNaN(d) ? null : d;
    }

    // Data da candidatura. Candidaturas antigas ficaram sem criado_em (a API não
    // preenchia); nelas usa a última atualização, que existe quando o status mudou
    function dataCandidatura(c) {
        return data(c.criadoEm) || data(c.atualizadoEm);
    }

    function inicioDoDia(d) {
        const x = new Date(d);
        x.setHours(0, 0, 0, 0);
        return x;
    }

    function media(lista) {
        return lista.length ? lista.reduce((a, b) => a + b, 0) / lista.length : null;
    }

    // ------------------------------------------------------------
    // Tooltip único (valor em destaque, rótulo depois)
    // ------------------------------------------------------------
    const dica = el('div', 'dash-dica');
    dica.setAttribute('role', 'tooltip');
    dica.hidden = true;
    document.body.appendChild(dica);

    function mostrarDica(alvo, valor, rotulo, extra) {
        dica.replaceChildren(el('strong', '', valor), el('span', '', rotulo));
        if (extra) dica.appendChild(el('small', '', extra));
        dica.hidden = false;
        const r = alvo.getBoundingClientRect();
        const d = dica.getBoundingClientRect();
        let x = r.left + r.width / 2 - d.width / 2;
        x = Math.max(8, Math.min(x, window.innerWidth - d.width - 8));
        let y = r.top - d.height - 8;
        if (y < 8) y = r.bottom + 8;
        dica.style.left = `${x + window.scrollX}px`;
        dica.style.top = `${y + window.scrollY}px`;
    }
    function esconderDica() { dica.hidden = true; }

    function ligarDica(alvo, valor, rotulo, extra) {
        alvo.tabIndex = 0;
        alvo.setAttribute('aria-label', `${rotulo}: ${valor}${extra ? '. ' + extra : ''}`);
        alvo.addEventListener('pointerenter', () => mostrarDica(alvo, valor, rotulo, extra));
        alvo.addEventListener('pointerleave', esconderDica);
        alvo.addEventListener('focus', () => mostrarDica(alvo, valor, rotulo, extra));
        alvo.addEventListener('blur', esconderDica);
    }

    // ------------------------------------------------------------
    // Tabela de dados (a mesma informação dos gráficos, sem depender do mouse)
    // ------------------------------------------------------------
    function preencherTabela(card, cabecalho, linhas) {
        const det = card.querySelector('.dash-tabela');
        if (!det) return;
        const tabela = el('table');
        const thead = el('thead');
        const trh = el('tr');
        cabecalho.forEach(c => trh.appendChild(el('th', '', c)));
        thead.appendChild(trh);
        const tbody = el('tbody');
        linhas.forEach(l => {
            const tr = el('tr');
            l.forEach(c => tr.appendChild(el('td', '', c)));
            tbody.appendChild(tr);
        });
        tabela.append(thead, tbody);
        det.querySelector('table')?.remove();
        det.appendChild(tabela);
        det.hidden = linhas.length === 0;
    }

    function vazio(alvo, msg) {
        alvo.replaceChildren(el('p', 'dash-vazio', msg));
    }

    // ------------------------------------------------------------
    // Colunas verticais (SVG): série temporal e histograma
    // ------------------------------------------------------------
    function escalaBonita(max) {
        if (max <= 4) return { topo: Math.max(max, 1) <= 4 ? 4 : max, passo: 1 };
        const bruto = max / 4;
        const mag = Math.pow(10, Math.floor(Math.log10(bruto)));
        const passo = [1, 2, 2.5, 5, 10].map(m => m * mag).find(p => p >= bruto);
        return { topo: passo * Math.ceil(max / passo), passo };
    }

    function colunas(alvo, pontos, { rotuloSerie, formato = fmtInt, extra } = {}) {
        const largura = Math.max(alvo.clientWidth, 260);
        const altura = 220;
        const m = { t: 12, r: 8, b: 28, l: 36 };
        const w = largura - m.l - m.r;
        const h = altura - m.t - m.b;
        const max = Math.max(...pontos.map(p => p.valor), 0);
        const { topo, passo } = escalaBonita(max);

        const svg = svgEl('svg', { width: largura, height: altura, viewBox: `0 0 ${largura} ${altura}`, role: 'img' });
        svg.setAttribute('aria-label', `${rotuloSerie}. Veja a tabela de dados abaixo para os valores.`);

        // Grade e eixo Y
        for (let v = 0; v <= topo + 1e-9; v += passo) {
            const y = m.t + h - (v / topo) * h;
            svg.appendChild(svgEl('line', { x1: m.l, x2: largura - m.r, y1: y, y2: y, class: v === 0 ? 'dash-base' : 'dash-linha' }));
            const t = svgEl('text', { x: m.l - 8, y: y + 4, 'text-anchor': 'end', class: 'dash-eixo' });
            t.textContent = formato(v);
            svg.appendChild(t);
        }

        const banda = w / pontos.length;
        const larg = Math.min(24, Math.max(3, banda - 2));
        // Rótulos do eixo X: só os que cabem
        const cabe = Math.max(1, Math.ceil(52 / banda));

        pontos.forEach((p, i) => {
            const x0 = m.l + i * banda;
            const bh = topo ? (p.valor / topo) * h : 0;
            const x = x0 + (banda - larg) / 2;
            const y = m.t + h - bh;

            if (bh > 0) {
                const r = Math.min(4, bh, larg / 2);
                // Ponta arredondada em cima, quadrada na base
                const d = `M${x},${m.t + h} V${y + r} Q${x},${y} ${x + r},${y} H${x + larg - r} Q${x + larg},${y} ${x + larg},${y + r} V${m.t + h} Z`;
                svg.appendChild(svgEl('path', { d, class: 'dash-marca' }));
            }

            if (i % cabe === 0) {
                const t = svgEl('text', { x: x0 + banda / 2, y: altura - 8, 'text-anchor': 'middle', class: 'dash-eixo' });
                t.textContent = p.rotuloCurto || p.rotulo;
                svg.appendChild(t);
            }

            // Área de toque: a banda inteira, maior que a coluna
            const alvoHit = svgEl('rect', { x: x0, y: m.t, width: banda, height: h, class: 'dash-hit' });
            svg.appendChild(alvoHit);
            ligarDica(alvoHit, formato(p.valor), p.rotulo, extra ? extra(p) : '');
        });

        alvo.replaceChildren(svg);
    }

    // ------------------------------------------------------------
    // Barras horizontais (HTML): funil, por vaga, composição, habilidades
    // ------------------------------------------------------------
    function barras(alvo, itens, { max, formato = fmtInt, classeCor, classeLista } = {}) {
        const topo = max ?? Math.max(...itens.map(i => i.valor), 1);
        const lista = el('div', 'dash-barras' + (classeLista ? ` ${classeLista}` : ''));
        itens.forEach((item, idx) => {
            const linha = el('div', 'dash-barra');
            const rotulo = el('span', 'dash-barra__rotulo', item.rotulo);
            rotulo.title = item.rotulo;
            const trilho = el('span', 'dash-barra__trilho');
            const fill = el('span', 'dash-barra__fill' + (classeCor ? ` ${classeCor(idx)}` : ''));
            fill.style.width = `${topo ? Math.max(0, (item.valor / topo) * 100) : 0}%`;
            trilho.appendChild(fill);
            // valorTexto: quando o número sozinho engana (ex.: 100% de apenas 1 candidato)
            const valor = el('span', 'dash-barra__valor', item.valorTexto || formato(item.valor));
            linha.append(rotulo, trilho, valor);
            ligarDica(linha, formato(item.valor), item.rotulo, item.extra || '');
            lista.appendChild(linha);
        });
        alvo.replaceChildren(lista);
    }

    // ------------------------------------------------------------
    // Recorte dos dados conforme os filtros
    // ------------------------------------------------------------
    function recorte() {
        const agora = new Date();
        const dias = estado.periodo === 'tudo' ? null : Number(estado.periodo);
        const inicio = dias ? new Date(inicioDoDia(agora).getTime() - (dias - 1) * DIA) : null;

        const daVaga = c => estado.vaga === 'todas' || String(c.vagaId) === estado.vaga;
        const noPeriodo = (c, ini, fim) => {
            if (!ini) return true;
            const d = dataCandidatura(c);
            return !!d && d >= ini && d < fim;
        };

        const fimAtual = new Date(inicioDoDia(agora).getTime() + DIA);
        const atuais = estado.candidaturas.filter(c => daVaga(c) && noPeriodo(c, inicio, fimAtual));
        const anteriores = dias
            ? estado.candidaturas.filter(c => daVaga(c) && noPeriodo(c, new Date(inicio.getTime() - dias * DIA), inicio))
            : null;
        const vagas = estado.vaga === 'todas' ? estado.vagas : estado.vagas.filter(v => String(v.id) === estado.vaga);
        const semData = dias ? estado.candidaturas.filter(c => daVaga(c) && !dataCandidatura(c)).length : 0;

        return { atuais, anteriores, vagas, inicio, dias, semData };
    }

    function matchDe(c) {
        return estado.matches.get(`${c.vagaId}|${c.estudanteId}`) || null;
    }

    // ------------------------------------------------------------
    // Indicadores (KPI)
    // ------------------------------------------------------------
    function indicadores({ atuais, anteriores, dias }) {
        const total = atuais.length;
        const scores = atuais.map(matchDe).filter(Boolean).map(m => Number(m.scoreTotal)).filter(n => !isNaN(n));
        const mediaScore = media(scores);
        const chegaram = etapa => atuais.filter(c => ETAPAS[etapa].status.includes(normalizar(c.status))).length;
        const entrevista = chegaram(2);
        const aprovadas = chegaram(3);

        $('#kpiCandidaturas').textContent = fmtInt(total);
        const delta = $('#kpiCandidaturasDelta');
        if (anteriores && anteriores.length + total > 0) {
            const dif = total - anteriores.length;
            const pct = anteriores.length ? Math.round((dif / anteriores.length) * 100) : null;
            const sinal = dif > 0 ? '▲' : dif < 0 ? '▼' : '■';
            delta.textContent = `${sinal} ${dif > 0 ? '+' : ''}${pct === null ? fmtInt(dif) : pct + '%'} vs. ${dias} dias anteriores`;
            delta.className = 'dash-kpi__delta ' + (dif > 0 ? 'dash-kpi__delta--sobe' : dif < 0 ? 'dash-kpi__delta--desce' : '');
        } else {
            delta.textContent = dias ? 'Sem candidaturas para comparar' : 'Desde a primeira vaga';
            delta.className = 'dash-kpi__delta';
        }

        $('#kpiScore').textContent = mediaScore === null ? '—' : fmtPct(mediaScore);
        $('#kpiScoreSub').textContent = scores.length
            ? `Média de ${fmtInt(scores.length)} candidatura${scores.length > 1 ? 's' : ''} avaliada${scores.length > 1 ? 's' : ''} pela IA`
            : 'Nenhuma candidatura avaliada pela IA';

        $('#kpiEntrevista').textContent = total ? fmtPct((entrevista / total) * 100) : '—';
        $('#kpiEntrevistaSub').textContent = `${fmtInt(entrevista)} de ${fmtInt(total)} chegaram à entrevista`;

        $('#kpiAprovadas').textContent = fmtInt(aprovadas);
        $('#kpiAprovadasSub').textContent = total ? `${fmtPct((aprovadas / total) * 100)} das candidaturas` : 'Nenhuma candidatura no período';
    }

    // ------------------------------------------------------------
    // Gráficos
    // ------------------------------------------------------------
    // Ranking das candidaturas pela compatibilidade calculada pela IA.
    // Cada linha abre o perfil do candidato (verPerfilCandidato, do menu-empresa.js).
    const RANKING_INICIAL = 10;
    let rankingCompleto = false;

    function ranking({ atuais }) {
        const card = $('#cardRanking');
        const alvo = $('.dash-grafico', card);
        const vagaPorId = new Map(estado.vagas.map(v => [v.id, v]));

        const linhas = atuais
            .map(c => ({ c, m: matchDe(c) }))
            .filter(x => x.m && !isNaN(Number(x.m.scoreTotal)))
            .map(({ c, m }) => {
                const perfil = estado.perfis.get(String(c.estudanteId));
                return {
                    estudanteId: c.estudanteId,
                    nome: perfil?.nome || (estado.perfis.size ? 'Candidato' : 'Carregando…'),
                    curso: [perfil?.curso, perfil?.instituicao].filter(Boolean).join(' · '),
                    vaga: vagaPorId.get(c.vagaId)?.titulo || `Vaga ${c.vagaId}`,
                    status: c.status,
                    score: Number(m.scoreTotal)
                };
            })
            .sort((a, b) => b.score - a.score);

        const semScore = atuais.length - linhas.length;
        $('.dash-card__sub', card).textContent = linhas.length
            ? `${fmtInt(linhas.length)} candidatura${linhas.length > 1 ? 's' : ''} avaliada${linhas.length > 1 ? 's' : ''} pela IA`
              + (semScore ? ` · ${fmtInt(semScore)} ainda sem score` : '') + ' · clique para ver o candidato'
            : 'Clique em um candidato para ver o perfil';

        if (!linhas.length) {
            vazio(alvo, atuais.length
                ? 'As candidaturas do período ainda não têm score de compatibilidade.'
                : 'Nenhuma candidatura no período.');
            preencherTabela(card, [], []);
            return;
        }

        const visiveis = rankingCompleto ? linhas : linhas.slice(0, RANKING_INICIAL);
        const lista = el('ol', 'dash-ranking');
        visiveis.forEach((l, i) => {
            const li = el('li');
            const btn = el('button', 'dash-ranking__item');
            btn.type = 'button';
            btn.setAttribute('aria-label', `${i + 1}º lugar: ${l.nome}, ${fmtPct(l.score)} compatível com ${l.vaga}. Ver perfil do candidato`);

            btn.appendChild(el('span', 'dash-ranking__pos', `${i + 1}º`));
            const quem = el('span', 'dash-ranking__quem');
            quem.appendChild(el('strong', '', l.nome));
            quem.appendChild(el('span', '', l.curso ? `${l.vaga} · ${l.curso}` : l.vaga));
            btn.appendChild(quem);

            const trilho = el('span', 'dash-barra__trilho dash-ranking__trilho');
            const fill = el('span', 'dash-barra__fill');
            fill.style.width = `${Math.max(0, Math.min(100, l.score))}%`;
            trilho.appendChild(fill);
            btn.appendChild(trilho);

            btn.appendChild(el('span', 'dash-ranking__pct', fmtPct(l.score)));
            btn.addEventListener('click', () => {
                if (typeof verPerfilCandidato === 'function') verPerfilCandidato(l.estudanteId);
            });
            li.appendChild(btn);
            lista.appendChild(li);
        });

        alvo.replaceChildren(lista);
        if (linhas.length > RANKING_INICIAL) {
            const mais = el('button', 'em-btn em-btn--ghost em-btn--sm dash-ranking__mais',
                rankingCompleto ? 'Mostrar só os 10 primeiros' : `Ver todos (${fmtInt(linhas.length)})`);
            mais.type = 'button';
            mais.addEventListener('click', () => { rankingCompleto = !rankingCompleto; ranking({ atuais }); });
            alvo.appendChild(mais);
        }

        preencherTabela(card, ['Posição', 'Candidato', 'Vaga', 'Compatibilidade'],
            linhas.map((l, i) => [`${i + 1}º`, l.nome, l.vaga, fmtPct(l.score)]));
    }

    function funil({ atuais }) {
        const card = $('#cardFunil');
        const alvo = $('.dash-grafico', card);
        if (!atuais.length) { vazio(alvo, 'Nenhuma candidatura no período.'); preencherTabela(card, [], []); return; }

        const contagens = ETAPAS.map((e, i) => i === 0
            ? atuais.length
            : atuais.filter(c => e.status.includes(normalizar(c.status))).length);
        const itens = ETAPAS.map((e, i) => ({
            rotulo: e.rotulo,
            valor: contagens[i],
            extra: i === 0 ? 'Todas as candidaturas do período'
                 : `${fmtPct(contagens[i - 1] ? (contagens[i] / contagens[i - 1]) * 100 : 0)} da etapa anterior`
        }));
        barras(alvo, itens, { max: contagens[0], classeCor: i => `dash-funil-${i + 1}` });
        preencherTabela(card, ['Etapa', 'Candidaturas', 'Da etapa anterior'],
            itens.map((it, i) => [it.rotulo, fmtInt(it.valor), i === 0 ? '—' : it.extra.split(' ')[0]]));
    }

    function distribuicaoScore({ atuais }) {
        const card = $('#cardDistribuicao');
        const alvo = $('.dash-grafico', card);
        const scores = atuais.map(matchDe).filter(Boolean).map(m => Number(m.scoreTotal)).filter(n => !isNaN(n));
        if (!scores.length) { vazio(alvo, 'Nenhuma candidatura avaliada pela IA ainda.'); preencherTabela(card, [], []); return; }

        const faixas = [0, 20, 40, 60, 80].map(ini => {
            const fim = ini + 20;
            const valor = scores.filter(s => s >= ini && (fim === 100 ? s <= 100 : s < fim)).length;
            return { rotulo: `${ini}–${fim}% de compatibilidade`, rotuloCurto: `${ini}–${fim}`, valor };
        });
        colunas(alvo, faixas, { rotuloSerie: 'Candidatos por faixa de compatibilidade',
                               extra: f => `${fmtPct((f.valor / scores.length) * 100)} dos avaliados` });
        preencherTabela(card, ['Faixa', 'Candidatos'], faixas.map(f => [f.rotulo, fmtInt(f.valor)]));
    }

    function composicaoScore({ atuais }) {
        const card = $('#cardComposicao');
        const alvo = $('.dash-grafico', card);
        const ms = atuais.map(matchDe).filter(Boolean);
        if (!ms.length) { vazio(alvo, 'Nenhuma candidatura avaliada pela IA ainda.'); preencherTabela(card, [], []); return; }

        const partes = [
            { campo: 'scoreVocacional',  rotulo: 'Perfil vocacional', peso: '40%' },
            { campo: 'scoreHabilidades', rotulo: 'Habilidades',       peso: '35%' },
            { campo: 'scoreCurriculo',   rotulo: 'Currículo',         peso: '25%' }
        ].map(p => {
            // Componente sem nota (null) fica fora da média, em vez de contar como zero
            const vals = ms.map(m => m[p.campo]).filter(v => v !== null && v !== undefined)
                           .map(Number).filter(n => !isNaN(n));
            return { rotulo: p.rotulo, valor: media(vals) ?? 0, extra: `Média dos candidatos · peso ${p.peso} no score final` };
        });
        barras(alvo, partes, { max: 100, formato: fmtPct });
        preencherTabela(card, ['Componente', 'Média', 'Peso no score'],
            partes.map((p, i) => [p.rotulo, fmtPct(p.valor), ['40%', '35%', '25%'][i]]));
    }

    function porVaga({ atuais, vagas }) {
        const card = $('#cardPorVaga');
        const alvo = $('.dash-grafico', card);
        if (!vagas.length) { vazio(alvo, 'Nenhuma vaga publicada.'); preencherTabela(card, [], []); return; }

        const itens = vagas.map(v => {
            const cs = atuais.filter(c => c.vagaId === v.id);
            const sc = cs.map(matchDe).filter(Boolean).map(m => Number(m.scoreTotal));
            const md = media(sc);
            return {
                rotulo: v.titulo || `Vaga ${v.id}`,
                valor: cs.length,
                extra: `${v.status === 'aberta' ? 'Aberta' : 'Encerrada'}${md === null ? '' : ` · compatibilidade média ${fmtPct(md)}`}`
            };
        }).sort((a, b) => b.valor - a.valor);

        const visiveis = itens.slice(0, 8);
        if (itens.length > 8) {
            visiveis.push({ rotulo: `Outras ${itens.length - 8} vagas`, valor: itens.slice(8).reduce((s, i) => s + i.valor, 0), extra: 'Soma das demais vagas' });
        }
        barras(alvo, visiveis);
        preencherTabela(card, ['Vaga', 'Candidaturas', 'Situação'], itens.map(i => [i.rotulo, fmtInt(i.valor), i.extra]));
    }

    function habilidades({ atuais, vagas }) {
        const card = $('#cardHabilidades');
        const alvo = $('.dash-grafico', card);

        // Para cada habilidade exigida: quantos candidatos das vagas que a exigem a têm no perfil
        const porHab = new Map(); // nome normalizado -> { nome, candidatos:Set, com:Set }
        vagas.forEach(v => {
            const reqs = (v.habilidadesRequeridas || []).filter(Boolean);
            if (!reqs.length) return;
            const candidatos = atuais.filter(c => c.vagaId === v.id).map(c => String(c.estudanteId));
            reqs.forEach(h => {
                const k = normalizar(h);
                if (!porHab.has(k)) porHab.set(k, { nome: h.trim(), candidatos: new Set(), com: new Set() });
                const reg = porHab.get(k);
                candidatos.forEach(id => {
                    const perfil = estado.perfis.get(id);
                    if (!perfil) return;  // perfil não carregado: fica fora da conta
                    reg.candidatos.add(id);
                    if ((perfil.habilidadesExtraidas || []).some(x => normalizar(x) === k)) reg.com.add(id);
                });
            });
        });

        const itens = [...porHab.values()]
            .filter(r => r.candidatos.size > 0)
            .map(r => ({ rotulo: r.nome, valor: (r.com.size / r.candidatos.size) * 100,
                         valorTexto: `${fmtPct((r.com.size / r.candidatos.size) * 100)} · ${r.com.size}/${r.candidatos.size}`,
                         extra: `${fmtInt(r.com.size)} de ${fmtInt(r.candidatos.size)} candidatos têm essa habilidade`
                                + (r.candidatos.size < 5 ? ' (poucos candidatos: leia com cuidado)' : '') }))
            .sort((a, b) => b.valor - a.valor);

        if (!itens.length) {
            vazio(alvo, vagas.some(v => v.habilidadesRequeridas?.length)
                ? 'Ainda não há candidatos nas vagas com habilidades exigidas.'
                : 'Cadastre habilidades exigidas nas vagas para ver este gráfico.');
            preencherTabela(card, [], []);
            return;
        }
        barras(alvo, itens.slice(0, 10), { max: 100, formato: fmtPct, classeLista: 'dash-barras--base' });
        preencherTabela(card, ['Habilidade', 'Candidatos que têm', 'Detalhe'], itens.map(i => [i.rotulo, fmtPct(i.valor), i.extra]));
    }

    // ------------------------------------------------------------
    // Render geral
    // ------------------------------------------------------------
    function render() {
        const r = recorte();
        const aviso = $('#dashAviso');
        aviso.hidden = !r.semData;
        aviso.textContent = r.semData
            ? `${fmtInt(r.semData)} candidatura${r.semData > 1 ? 's' : ''} sem data de envio não entra${r.semData > 1 ? 'm' : ''} nos filtros de 7, 30 ou 90 dias (aparece${r.semData > 1 ? 'm' : ''} em "Tudo").`
            : '';
        indicadores(r);
        ranking(r);
        funil(r);
        distribuicaoScore(r);
        composicaoScore(r);
        porVaga(r);
        habilidades(r);
    }

    // ------------------------------------------------------------
    // Carga de dados
    // ------------------------------------------------------------
    async function json(url) {
        try {
            const res = await authFetch(url);
            return res.ok ? await res.json() : null;
        } catch (_) { return null; }
    }

    // Busca em lotes pequenos para não disparar dezenas de requisições ao mesmo tempo
    async function emLotes(itens, tamanho, fn) {
        for (let i = 0; i < itens.length; i += tamanho) {
            await Promise.all(itens.slice(i, i + tamanho).map(fn));
        }
    }

    async function carregar(empresaId) {
        if (estado.carregando) return;
        estado.carregando = true;
        raiz.classList.add('dash--carregando');
        $('#dashAtualizado').textContent = 'Atualizando…';

        try {
            const [vagas, candidaturas] = await Promise.all([
                json(`${API}/api/vagas/empresa/${empresaId}`),
                json(`${API}/api/candidaturas/empresa/${empresaId}`)
            ]);
            if (!vagas || !candidaturas) throw new Error('sem dados');

            estado.vagas = vagas;
            estado.candidaturas = candidaturas;
            preencherFiltroVagas();

            const matches = new Map();
            await emLotes(vagas, 6, async v => {
                const ms = await json(`${API}/api/matches/vaga/${v.id}`);
                (ms || []).forEach(m => matches.set(`${m.vagaId}|${m.estudanteId}`, m));
            });
            estado.matches = matches;
            render();

            // Perfis por último: são o passo mais pesado e só alimentam o gráfico de habilidades
            // Os de maior compatibilidade primeiro: são os que aparecem no topo do ranking
            const melhorScore = new Map();
            candidaturas.forEach(c => {
                const sc = Number(matches.get(`${c.vagaId}|${c.estudanteId}`)?.scoreTotal ?? -1);
                const id = String(c.estudanteId);
                melhorScore.set(id, Math.max(melhorScore.get(id) ?? -1, sc));
            });
            const ids = [...melhorScore.keys()]
                .sort((a, b) => melhorScore.get(b) - melhorScore.get(a))
                .filter(id => !estado.perfis.has(id))
                .slice(0, MAX_CANDIDATOS_PERFIL);
            await emLotes(ids, 6, async id => {
                const p = await json(`${API}/api/estudantes/${id}`);
                if (p) estado.perfis.set(id, p);
            });
            render();

            $('#dashAtualizado').textContent = `Atualizado às ${new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}`;
        } catch (_) {
            $('#dashAtualizado').textContent = 'Não foi possível carregar os dados';
        } finally {
            estado.carregando = false;
            raiz.classList.remove('dash--carregando');
        }
    }

    function preencherFiltroVagas() {
        const sel = $('#dashFiltroVaga');
        const atual = estado.vaga;
        sel.replaceChildren(new Option('Todas as vagas', 'todas'));
        estado.vagas.forEach(v => sel.appendChild(new Option(v.titulo || `Vaga ${v.id}`, String(v.id))));
        sel.value = estado.vagas.some(v => String(v.id) === atual) ? atual : 'todas';
        estado.vaga = sel.value;
    }

    // ------------------------------------------------------------
    // Eventos
    // ------------------------------------------------------------
    raiz.querySelectorAll('[data-periodo]').forEach(btn => {
        btn.addEventListener('click', () => {
            estado.periodo = btn.dataset.periodo;
            raiz.querySelectorAll('[data-periodo]').forEach(b => b.setAttribute('aria-pressed', String(b === btn)));
            render();
        });
    });
    $('#dashFiltroVaga').addEventListener('change', e => { estado.vaga = e.target.value; render(); });

    // Redesenha as colunas quando a largura muda (os SVGs têm a largura do cartão)
    let ultimaLargura = 0;
    new ResizeObserver(entradas => {
        const w = Math.round(entradas[0].contentRect.width);
        if (Math.abs(w - ultimaLargura) < 8 || !estado.vagas.length) return;
        ultimaLargura = w;
        render();
    }).observe(raiz);

    window.addEventListener('scroll', esconderDica, { passive: true });

    document.addEventListener('DOMContentLoaded', () => {
        const auth = getAuth();
        if (!auth || auth.tipoUsuario !== 'Empresa') return;
        $('#dashAtualizar').addEventListener('click', () => carregar(auth.contaId));
        carregar(auth.contaId);
    });

    // Permite que o menu-empresa.js peça atualização depois de mudar status, publicar vaga etc.
    window.atualizarDashboard = () => {
        const auth = getAuth();
        if (auth) carregar(auth.contaId);
    };
})();
