/* Lógica exclusiva da página AcessoEmpresas */

// ──────────────────────────────────────────────
// TABS
// ──────────────────────────────────────────────
function switchTab(tab) {
  document.querySelectorAll('.tab').forEach(t => {
    t.classList.remove('active');
    t.setAttribute('aria-selected', 'false');
  });
  document.querySelectorAll('.screen').forEach(s => s.classList.remove('active'));

  document.getElementById(`tab-${tab}`).classList.add('active');
  document.getElementById(`tab-${tab}`).setAttribute('aria-selected', 'true');
  document.getElementById(`screen-${tab}`).classList.add('active');
  clearErrors();
}

// ──────────────────────────────────────────────
// LOGIN — /api/login (JWT do backend)
// ──────────────────────────────────────────────

// Busca o perfil da empresa logada e guarda a sessão local
async function carregarSessaoEmpresa(contaId) {
  const res = await authFetch(`${API}/api/empresas/${contaId}`);
  if (!res.ok) throw new Error('Perfil de empresa não encontrado. Cadastre-se primeiro.');
  const empresa = await res.json();

  localStorage.setItem('od-session', JSON.stringify({
    type:  'company',
    id:    empresa.id,
    name:  empresa.razaoSocial || empresa.nomeFantasia,
    email: empresa.email
  }));
  return empresa;
}

async function companyLogin() {
  const email = document.getElementById('login-email').value.trim();
  const pw    = document.getElementById('login-pw').value;

  clearErrors();
  if (!email || !pw) return showError('login-email', 'Preencha e-mail e senha.');
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
    return showError('login-email', 'E-mail inválido.');

  const btn = document.querySelector('#screen-login .btn-primary');
  btn.disabled = true;
  btn.textContent = 'Verificando...';
  btn.setAttribute('aria-busy', 'true');

  try {
    let auth;
    try {
      auth = await apiLogin(email, pw);
    } catch (err) {
      if (err.status) return showError('login-email', err.status === 401 ? 'E-mail ou senha incorretos.' : err.message);
      throw err;
    }

    if (auth.tipoUsuario !== 'Empresa') {
      clearAuth();
      return showError('login-email', 'Esta conta é de estudante. Use o acesso para estudantes.');
    }

    try {
      await carregarSessaoEmpresa(auth.contaId);
    } catch (err) {
      clearAuth();
      return showError('login-email', err.message);
    }

    redirectToPanel();

  } catch (err) {
    showError('login-email', 'Erro ao conectar com o servidor. Tente novamente.');
    console.error(err);
  } finally {
    btn.disabled = false;
    btn.textContent = 'Entrar';
    btn.removeAttribute('aria-busy');
  }
}

// ──────────────────────────────────────────────
// CADASTRO
// ──────────────────────────────────────────────
async function companyRegister() {
  const name    = document.getElementById('co-name').value.trim();
  const cnpj    = document.getElementById('co-cnpj').value.trim();
  const setor   = document.getElementById('co-setor').value.trim();
  const contact = document.getElementById('co-contact').value.trim();
  const role    = document.getElementById('co-role').value.trim();
  const email   = document.getElementById('co-email').value.trim();
  const phone   = document.getElementById('co-phone').value.trim();
  const cep     = document.getElementById('co-cep').value.trim();
  const city    = document.getElementById('co-city').value.trim();
  const pw      = document.getElementById('co-pw').value;
  const pwc     = document.getElementById('co-pwc').value;
  const terms   = document.getElementById('co-terms').checked;

  clearErrors();

  if (!name)    return showError('co-name',    'Campo obrigatório.');
  if (!cnpj || cnpj.replace(/\D/g,'').length < 14)
                return showError('co-cnpj',    'CNPJ inválido.');
  if (!contact) return showError('co-contact', 'Campo obrigatório.');
  if (!role)    return showError('co-role',    'Campo obrigatório.');
  if (!email)   return showError('co-email',   'Campo obrigatório.');
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
                return showError('co-email',   'E-mail inválido.');
  if (!phone)   return showError('co-phone',   'Campo obrigatório.');
  if (cep.replace(/\D/g,'').length < 8)
                return showError('co-cep',     'CEP inválido.');
  if (!pw)      return showError('co-pw',      'Campo obrigatório.');
  if (pw.length < 8)   return showError('co-pw',  'Mínimo 8 caracteres.');
  if (pw !== pwc)      return showError('co-pwc', 'As senhas não coincidem.');
  if (!terms)   return showError('co-terms-wrap', 'Aceite os termos para continuar.');

  const btn = document.querySelector('#screen-register .btn-primary');
  if (btn) { btn.disabled = true; btn.textContent = 'Criando conta...'; btn.setAttribute('aria-busy', 'true'); }

  try {
    // 1. Cria perfil + login da empresa no backend (o Id da conta é gerado pela API)
    const [localidade, uf] = city.includes(' — ') ? city.split(' — ') : [city, ''];
    const payload = {
      razaoSocial:      name,
      cnpj:             cnpj.replace(/\D/g,''),
      email:            email,
      senha:            pw,
      telefone:         phone,
      cep:              cep.replace(/\D/g,''),
      cidade:           localidade.trim(),
      estado:           uf.trim(),
      setor:            setor || null,
      responsavelNome:  contact,
      responsavelCargo: role,
      status:           'ativa'
    };

    const res = await fetch(`${API}/api/empresas`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json', 'ngrok-skip-browser-warning': '1' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      const errBody = await res.json().catch(() => ({}));
      if (res.status === 409) return showError('co-email', 'Este e-mail já possui uma conta. Tente fazer login.');
      throw new Error(errBody.erro || `Erro ${res.status} ao salvar perfil da empresa`);
    }

    // 2. Entra com a conta recém-criada para obter o token
    const auth = await apiLogin(email, pw);
    await carregarSessaoEmpresa(auth.contaId);

    // Mostra sucesso e redireciona
    document.getElementById('s-company').textContent = name;
    document.getElementById('s-contact').textContent = contact;
    document.getElementById('s-email').textContent   = email;
    document.getElementById('s-city').textContent    = city || cep;
    showScreen('screen-success');

    setTimeout(redirectToPanel, 2000);

  } catch (err) {
    showError('co-email', `Erro ao criar conta: ${err.message}`);
    console.error(err);
  } finally {
    if (btn) { btn.disabled = false; btn.textContent = 'Criar conta'; btn.removeAttribute('aria-busy'); }
  }
}

// ──────────────────────────────────────────────
// REDIRECT
// ──────────────────────────────────────────────
function redirectToPanel() {
  window.location.href = '../Areas/MenuEmpresa.html';
}

// ──────────────────────────────────────────────
// BUSCA DE CEP
// ──────────────────────────────────────────────
async function fetchCEP() {
  const cep = document.getElementById('co-cep').value.replace(/\D/g, '');
  if (cep.length !== 8) return;

  const cityInput = document.getElementById('co-city');
  const spinner   = document.getElementById('cep-spinner-co');

  cityInput.value = 'Buscando...';
  if (spinner) spinner.style.display = 'block';

  try {
    const res  = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
    const data = await res.json();

    if (!data.erro) {
      cityInput.value = `${data.localidade} — ${data.uf}`;
      cityInput.setAttribute('readonly', '');
    } else {
      cityInput.value = '';
      cityInput.placeholder = 'Preencha manualmente';
      cityInput.removeAttribute('readonly');
      showError('co-cep', 'CEP não encontrado.');
    }
  } catch (e) {
    cityInput.value = '';
    cityInput.placeholder = 'Preencha manualmente';
    cityInput.removeAttribute('readonly');
  } finally {
    if (spinner) spinner.style.display = 'none';
  }
}

// ──────────────────────────────────────────────
// MÁSCARAS
// ──────────────────────────────────────────────
function maskCNPJ(el) {
  let v = el.value.replace(/\D/g, '').slice(0, 14);
  if      (v.length > 12) v = v.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d+)/, '$1.$2.$3/$4-$5');
  else if (v.length >  8) v = v.replace(/(\d{2})(\d{3})(\d{3})(\d+)/, '$1.$2.$3/$4');
  else if (v.length >  5) v = v.replace(/(\d{2})(\d{3})(\d+)/, '$1.$2.$3');
  else if (v.length >  2) v = v.replace(/(\d{2})(\d+)/, '$1.$2');
  el.value = v;
}

function togglePw(id, btn) {
  const input = document.getElementById(id);
  const isText = input.type === 'text';
  input.type = isText ? 'password' : 'text';
  btn.setAttribute('aria-label', isText ? 'Mostrar senha' : 'Ocultar senha');
}

function checkPwStrength(pw, barsId, labelId) {
  const bars  = document.querySelectorAll(`#${barsId} .pw-bar`);
  const label = document.getElementById(labelId);
  let score = 0;
  if (pw.length >= 8)          score++;
  if (/[A-Z]/.test(pw))        score++;
  if (/[0-9]/.test(pw))        score++;
  if (/[^A-Za-z0-9]/.test(pw)) score++;

  const levels = ['', 'Fraca', 'Razoável', 'Boa', 'Forte'];
  const colors = ['', '#ef4444', '#f59e0b', '#3b82f6', '#22c55e'];

  bars.forEach((b, i) => {
    b.className = 'pw-bar';
    b.style.background = i < score ? colors[score] : '';
  });
  if (label) {
    label.textContent = pw.length > 0 ? levels[score] || 'Fraca' : '';
    label.style.color = colors[score] || '';
  }
}

// Verifica sessão já existente ao carregar
document.addEventListener('DOMContentLoaded', () => {
  try {
    const auth  = getAuth();
    const local = localStorage.getItem('od-session');
    if (auth && local) {
      const s = JSON.parse(local);
      if (s.type === 'company') redirectToPanel();
    }
  } catch (_) {}
});
