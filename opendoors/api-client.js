/* Cliente da API Open Doors — autenticação com o JWT próprio do backend */

// Rodando localmente usa a API local; fora disso, o endereço público da API
const API = ['localhost', '127.0.0.1'].includes(window.location.hostname)
  ? 'http://localhost:5089'
  : 'https://monotype-sudoku-arousal.ngrok-free.dev';

const AUTH_KEY = 'od-auth';

// Token salvo após o login: { token, expiraEm, contaId, tipoUsuario }
function getAuth() {
  try {
    const auth = JSON.parse(localStorage.getItem(AUTH_KEY) || 'null');
    if (!auth?.token) return null;
    if (auth.expiraEm && new Date(auth.expiraEm) <= new Date()) {
      clearAuth();
      return null;
    }
    return auth;
  } catch (e) {
    return null;
  }
}

function setAuth(auth) {
  try { localStorage.setItem(AUTH_KEY, JSON.stringify(auth)); } catch (e) {}
}

function clearAuth() {
  try {
    localStorage.removeItem(AUTH_KEY);
    localStorage.removeItem('od-session');
  } catch (e) {}
}

function loginPageFor(tipoUsuario) {
  return tipoUsuario === 'Empresa'
    ? '../Acesso/AcessoEmpresas.html'
    : '../Acesso/AcessoEstudantes.html';
}

// POST /api/login — guarda o token e devolve { token, expiraEm, contaId, tipoUsuario }
async function apiLogin(email, senha) {
  const res = await fetch(`${API}/api/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', 'ngrok-skip-browser-warning': '1' },
    body: JSON.stringify({ email, senha })
  });

  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    const error = new Error(err.erro || `Erro ${res.status}`);
    error.status = res.status;
    throw error;
  }

  const auth = await res.json();
  setAuth(auth);
  return auth;
}

// fetch com o token; se a sessão expirou (401), volta para a tela de login
async function authFetch(url, options = {}) {
  const auth = getAuth();
  const res = await fetch(url, {
    ...options,
    headers: {
      ...(options.headers || {}),
      'ngrok-skip-browser-warning': '1',
      ...(auth ? { 'Authorization': `Bearer ${auth.token}` } : {})
    }
  });

  if (res.status === 401 && auth) {
    clearAuth();
    window.location.href = loginPageFor(auth.tipoUsuario);
  }
  return res;
}

// PUT /api/login/senha
async function apiAlterarSenha(senhaAtual, novaSenha) {
  const res = await authFetch(`${API}/api/login/senha`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ senhaAtual, novaSenha })
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.erro || 'Não foi possível alterar a senha.');
  }
}
