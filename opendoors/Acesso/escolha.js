/* Tela única de acesso: o usuário escolhe Estudante ou Empresa */

// TEMA (mesma chave das outras telas)
function toggleTheme() {
  const html = document.documentElement;
  const next = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
  html.setAttribute('data-theme', next);
  try { localStorage.setItem('od-theme', next); } catch (e) {}
}

(function () {
  try {
    const saved = localStorage.getItem('od-theme');
    if (saved) document.documentElement.setAttribute('data-theme', saved);
  } catch (e) {}
})();

// Voltando de uma tela de login para cá, a transição desliza no sentido contrário
window.addEventListener('pagereveal', e => {
  if (!e.viewTransition) return;
  const veioDe = window.navigation?.activation?.from?.url || '';
  if (/Acesso(Estudantes|Empresas)\.html/.test(veioDe)) e.viewTransition.types.add('voltar');
});

const semMovimento = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

// O outro cartão desliza para fora; depois abre a tela do perfil escolhido.
// #cadastro (botão "Criar minha conta" da homepage) é repassado para abrir direto no cadastro.
function escolherPerfil(perfil) {
  const opcoes = document.querySelector('.escolha-opcoes');
  const card = opcoes.querySelector(`[data-perfil="${perfil}"]`);
  if (!card || opcoes.classList.contains('escolhido')) return;

  const destino = card.getAttribute('href') + (location.hash === '#cadastro' ? '#cadastro' : '');
  // Tira o #perfil do endereço: ao voltar para cá, a escolha não se repete sozinha
  history.replaceState(null, '', location.pathname);
  opcoes.classList.add('escolhido');
  opcoes.dataset.escolha = perfil;

  setTimeout(() => { location.href = destino; }, semMovimento ? 0 : 480);
}

document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('.escolha-card').forEach(card => {
    card.addEventListener('click', e => {
      // Ctrl/⌘+clique e botão do meio continuam abrindo em nova aba
      if (e.ctrlKey || e.metaKey || e.shiftKey || e.button !== 0) return;
      e.preventDefault();
      escolherPerfil(card.dataset.perfil);
    });
  });

  escolherPeloEndereco();
});

// Atalhos da homepage ("Sou Estudante" / "Sou Empresa") já chegam com o perfil escolhido
function escolherPeloEndereco() {
  const perfil = location.hash.slice(1);
  if (perfil === 'estudante' || perfil === 'empresa') {
    setTimeout(() => escolherPerfil(perfil), semMovimento ? 0 : 250);
  }
}
window.addEventListener('hashchange', escolherPeloEndereco);

// Ao voltar pelo botão do navegador (página restaurada do cache), os cartões voltam ao lugar
window.addEventListener('pageshow', e => {
  if (!e.persisted) return;
  const opcoes = document.querySelector('.escolha-opcoes');
  opcoes.classList.remove('escolhido');
  delete opcoes.dataset.escolha;
});
