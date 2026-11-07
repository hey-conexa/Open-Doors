
// ESTADO DE SESSÃO (simulado via localStorage) 
function getSession() {
  try {
    const raw = localStorage.getItem('od-session');
    return raw ? JSON.parse(raw) : null;
  } catch (e) { return null; }
}

function setSession(data) {
  try { localStorage.setItem('od-session', JSON.stringify(data)); } catch (e) {}
}

function clearSession() {
  try { localStorage.removeItem('od-session'); } catch (e) {}
}

// LOGOUT 
function logoutStudent() {
  clearSession();
  closeProfileDrawer();
  renderProfileBtn();
}

// ABRIR / FECHAR GAVETA 
function toggleProfileDrawer() {
  const drawer  = document.getElementById('profileDrawer');
  const overlay = document.getElementById('profileOverlay');
  const btn     = document.getElementById('profileBtn');
  if (!drawer) return;
  const isOpen  = drawer.classList.contains('open');

  if (isOpen) {
    closeProfileDrawer();
  } else {
    drawer.classList.add('open');
    overlay.classList.add('open');
    btn.classList.add('profile-btn--active');
    btn.setAttribute('aria-expanded', 'true');

    const firstFocusable = drawer.querySelector('a, button');
    if (firstFocusable) setTimeout(() => firstFocusable.focus(), 50);
  }
}

function closeProfileDrawer() {
  const drawer  = document.getElementById('profileDrawer');
  const overlay = document.getElementById('profileOverlay');
  const btn     = document.getElementById('profileBtn');

  drawer?.classList.remove('open');
  overlay?.classList.remove('open');
  btn?.classList.remove('profile-btn--active');
  btn?.setAttribute('aria-expanded', 'false');

  btn?.focus();
}

document.addEventListener('keydown', e => {
  if (e.key === 'Escape') closeProfileDrawer();
});

document.addEventListener('keydown', e => {
  const drawer = document.getElementById('profileDrawer');
  if (!drawer?.classList.contains('open')) return;
  if (e.key !== 'Tab') return;

  const focusables = Array.from(
    drawer.querySelectorAll('a, button, [tabindex]:not([tabindex="-1"])')
  ).filter(el => !el.disabled);

  if (!focusables.length) return;
  const first = focusables[0];
  const last  = focusables[focusables.length - 1];

  if (e.shiftKey && document.activeElement === first) {
    e.preventDefault();
    last.focus();
  } else if (!e.shiftKey && document.activeElement === last) {
    e.preventDefault();
    first.focus();
  }
});

function renderProfileBtn() {
  const session    = getSession();
  const btnLabel   = document.getElementById('profileBtnLabel');
  const guestDiv   = document.getElementById('drawerGuest');
  const studentDiv = document.getElementById('drawerStudent');
  const companyDiv = document.getElementById('drawerCompany');
  const btn        = document.getElementById('profileBtn');

  if (session?.type === 'student') {
    const firstName = session.name?.split(' ')[0] || 'Estudante';
    const initial   = firstName.charAt(0).toUpperCase();

    if (btnLabel)   btnLabel.textContent = firstName;
    if (btn)        btn.setAttribute('aria-label', `Perfil de ${firstName} — abrir menu`);
    if (document.getElementById('drawerAvatar'))
      document.getElementById('drawerAvatar').textContent = initial;
    if (document.getElementById('drawerUserName'))
      document.getElementById('drawerUserName').textContent = session.name || 'Estudante';

    if (guestDiv)   guestDiv.style.display   = 'none';
    if (studentDiv) studentDiv.style.display = 'block';
    if (companyDiv) companyDiv.style.display = 'none';

  } else if (session?.type === 'company') {
    const shortName = session.name?.split(' ')[0] || 'Empresa';
    const initial   = shortName.charAt(0).toUpperCase();

    if (btnLabel)   btnLabel.textContent = shortName;
    if (btn)        btn.setAttribute('aria-label', `Painel da empresa ${shortName} — abrir menu`);
    if (document.getElementById('drawerAvatar'))
      document.getElementById('drawerAvatar').textContent = initial;
    if (document.getElementById('drawerUserName'))
      document.getElementById('drawerUserName').textContent = session.name || 'Empresa';

    if (guestDiv)   guestDiv.style.display   = 'none';
    if (studentDiv) studentDiv.style.display = 'none';
    if (companyDiv) companyDiv.style.display = 'block';

  } else {
    if (btnLabel)   btnLabel.textContent = 'Entrar';
    if (btn)        btn.setAttribute('aria-label', 'Entrar ou criar conta — abrir menu');
    if (guestDiv)   guestDiv.style.display   = 'block';
    if (studentDiv) studentDiv.style.display = 'none';
    if (companyDiv) companyDiv.style.display = 'none';
  }
}

function logoutCompany() {
  clearSession();
  closeProfileDrawer();
  renderProfileBtn();
}

document.addEventListener('DOMContentLoaded', () => {
  renderProfileBtn();

  const btn = document.getElementById('profileBtn');
  if (btn) btn.setAttribute('aria-expanded', 'false');
});

function loginAsStudent(name) {
  setSession({
    type: 'student',
    name: name || 'Estudante',
    id: null,
    email: null
  });
}
