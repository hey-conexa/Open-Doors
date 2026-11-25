/* ============================================================
   Página "Sobre nós e Contato" (SobreContato.html)

   PARA EDITAR: preencha as duas listas abaixo.
   - EQUIPE: a seção "Quem faz o Open Doors" só aparece quando tem alguém.
   - CANAIS: sem canais, a página mostra "serão publicados em breve".
   ============================================================ */

// Integrantes da equipe Conexa — { nome, funcao, foto }
// foto: quadrada (320x320) em images/equipe/; sem foto, aparecem as iniciais
const EQUIPE = [
    { nome: 'Isaac Lima',        funcao: 'Frontend', foto: 'images/equipe/isaac-lima.jpg' },
    { nome: 'Isaac de Medeiros', funcao: 'Backend',  foto: 'images/equipe/isaac-medeiros.jpg' },
    { nome: 'David Gabriel',     funcao: 'Frontend', foto: 'images/equipe/david-gabriel.jpg' },
    { nome: 'Nicolas Zemella',   funcao: 'Backend',  foto: 'images/equipe/nicolas-zemella.jpg' },
];

// Canais de contato. tipo: 'email' | 'telefone' | 'whatsapp' | 'instagram' | 'linkedin' | 'github' | 'site'
// valor: o que aparece na tela; link: para onde o clique leva (no e-mail e no telefone é montado sozinho)
const CANAIS = [
    { tipo: 'email',    valor: 'hey.conexa@gmail.com' },
    { tipo: 'telefone', valor: '(11) 9 2488-7209', link: 'tel:+5511924887209' },
    { tipo: 'whatsapp', valor: '(11) 9 2488-7209', link: 'https://wa.me/5511924887209' },
    // Outros exemplos:
    // { tipo: 'email',     valor: 'contato@exemplo.com' },
    // { tipo: 'whatsapp',  valor: '(11) 99999-9999', link: 'https://wa.me/5511999999999' },
    // { tipo: 'instagram', valor: '@opendoors', link: 'https://instagram.com/opendoors' },
];

(function () {
    'use strict';

    const ROTULOS = {
        email: 'E-mail', telefone: 'Telefone', whatsapp: 'WhatsApp', instagram: 'Instagram',
        linkedin: 'LinkedIn', github: 'GitHub', site: 'Site'
    };

    // Ícones em traço (mesmo estilo dos outros ícones do site)
    const ICONES = {
        email:     '<path d="M4 4h16a2 2 0 012 2v12a2 2 0 01-2 2H4a2 2 0 01-2-2V6a2 2 0 012-2z"/><polyline points="22 6 12 13 2 6"/>',
        telefone:  '<path d="M22 16.9v3a2 2 0 01-2.2 2 19.8 19.8 0 01-8.6-3.1 19.5 19.5 0 01-6-6A19.8 19.8 0 012.1 4.2 2 2 0 014.1 2h3a2 2 0 012 1.7c.1.9.4 1.8.7 2.7a2 2 0 01-.5 2.1L8 9.8a16 16 0 006 6l1.3-1.3a2 2 0 012.1-.4c.9.3 1.8.6 2.7.7a2 2 0 011.7 2z"/>',
        whatsapp:  '<path d="M21 11.5a8.4 8.4 0 01-12.4 7.4L3 21l2.1-5.5A8.4 8.4 0 1121 11.5z"/>',
        instagram: '<rect x="2" y="2" width="20" height="20" rx="5"/><circle cx="12" cy="12" r="4"/><line x1="17.5" y1="6.5" x2="17.51" y2="6.5"/>',
        linkedin:  '<path d="M16 8a6 6 0 016 6v7h-4v-7a2 2 0 00-4 0v7h-4v-7a6 6 0 016-6z"/><rect x="2" y="9" width="4" height="12"/><circle cx="4" cy="4" r="2"/>',
        github:    '<path d="M9 19c-5 1.5-5-2.5-7-3m14 6v-3.9a3.4 3.4 0 00-.9-2.6c3.1-.3 6.4-1.5 6.4-7A5.4 5.4 0 0020 4.8 5 5 0 0019.9 1S18.7.7 16 2.5a13.4 13.4 0 00-7 0C6.3.7 5.1 1 5.1 1A5 5 0 005 4.8a5.4 5.4 0 00-1.5 3.7c0 5.4 3.3 6.6 6.4 7a3.4 3.4 0 00-.9 2.6V22"/>',
        site:      '<circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 014 10 15.3 15.3 0 01-4 10 15.3 15.3 0 01-4-10 15.3 15.3 0 014-10z"/>'
    };

    function el(tag, className, text) {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text) node.textContent = text;
        return node;
    }

    function linkDoCanal(c) {
        if (c.link) return c.link;
        if (c.tipo === 'email') return `mailto:${c.valor}`;
        if (c.tipo === 'telefone') return `tel:${String(c.valor).replace(/[^\d+]/g, '')}`;
        return null;
    }

    function iniciais(nome) {
        return nome.trim().split(/\s+/).filter(Boolean)
            .map((p, i, arr) => (i === 0 || i === arr.length - 1) ? p[0] : '')
            .join('').toUpperCase();
    }

    function montarEquipe() {
        const pessoas = EQUIPE.filter(p => p && p.nome && p.nome.trim());
        if (!pessoas.length) return;
        const lista = document.getElementById('listaEquipe');
        pessoas.forEach(p => {
            const li = el('li', 'sc-pessoa');
            const avatar = el('span', 'sc-pessoa__avatar', iniciais(p.nome));
            avatar.setAttribute('aria-hidden', 'true');
            if (p.foto) {
                const img = el('img');
                img.src = p.foto;
                img.alt = '';           // o nome já está escrito ao lado
                img.loading = 'lazy';
                img.width = 64; img.height = 64;
                img.addEventListener('error', () => img.remove()); // sem a foto, ficam as iniciais
                avatar.appendChild(img);
            }
            const texto = el('span', 'sc-pessoa__texto');
            texto.appendChild(el('strong', '', p.nome.trim()));
            if (p.funcao) texto.appendChild(el('span', '', p.funcao));
            li.append(avatar, texto);
            lista.appendChild(li);
        });
        document.getElementById('equipe').hidden = false;
    }

    function montarCanais() {
        const canais = CANAIS.filter(c => c && c.valor && String(c.valor).trim());
        document.getElementById('canaisVazio').hidden = canais.length > 0;
        if (!canais.length) return;
        const lista = document.getElementById('listaCanais');
        canais.forEach(c => {
            const href = linkDoCanal(c);
            const li = el('li');
            const card = el(href ? 'a' : 'div', 'sc-canal');
            if (href) {
                card.href = href;
                if (/^https?:/i.test(href)) { card.target = '_blank'; card.rel = 'noopener noreferrer'; }
            }
            const icone = el('span', 'sc-canal__icone');
            icone.setAttribute('aria-hidden', 'true');
            icone.innerHTML = `<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">${ICONES[c.tipo] || ICONES.site}</svg>`;
            const texto = el('span', 'sc-canal__texto');
            texto.appendChild(el('span', 'sc-canal__rotulo', ROTULOS[c.tipo] || c.rotulo || 'Contato'));
            texto.appendChild(el('strong', '', String(c.valor).trim()));
            card.append(icone, texto);
            li.appendChild(card);
            lista.appendChild(li);
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        montarEquipe();
        montarCanais();
    });
})();
