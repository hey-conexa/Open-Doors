/* Lógica exclusiva da página AcessoEstudantes */

// LOGIN SOCIAL
function socialLogin(provider) {
  alert(`Redirecionando para autenticação via ${provider}.\n(Integração real requer o SDK do provedor.)`);
}

// MOSTRAR / OCULTAR SENHA
function togglePw(inputId, btn) {
  const input    = document.getElementById(inputId);
  const isHidden = input.type === 'password';
  input.type = isHidden ? 'text' : 'password';
  btn.querySelector('.icon-eye').style.display     = isHidden ? 'none'  : 'block';
  btn.querySelector('.icon-eye-off').style.display = isHidden ? 'block' : 'none';
  btn.setAttribute('aria-label', isHidden ? 'Ocultar senha' : 'Mostrar senha');
}

// FORÇA DA SENHA
function checkStrength(input, barsSelector, labelId) {
  const pw = input.value;
  let score = 0;
  if (pw.length >= 8)          score++;
  if (/[A-Z]/.test(pw))        score++;
  if (/[0-9]/.test(pw))        score++;
  if (/[^A-Za-z0-9]/.test(pw)) score++;

  const bars  = document.querySelectorAll(barsSelector || '.pw-bar');
  const label = document.getElementById(labelId || 'pw-label');
  const states = [
    { cls: '',       text: '' },
    { cls: 'weak',   text: 'Senha fraca' },
    { cls: 'weak',   text: 'Senha fraca' },
    { cls: 'fair',   text: 'Senha razoável' },
    { cls: 'strong', text: 'Senha forte' },
  ];
  const s = states[score];
  bars.forEach((bar, i) => {
    bar.className = 'pw-bar';
    if (i < score) bar.classList.add(s.cls);
  });
  if (label) label.textContent = s.text;
}

// ==============================================================
// LOGIN — /api/login (JWT do backend)
// ==============================================================

// Busca o perfil do estudante logado e guarda a sessão local
async function carregarSessaoEstudante(contaId) {
  const res = await authFetch(`${API}/api/estudantes/${contaId}`);
  if (!res.ok) throw new Error('Perfil de estudante não encontrado.');
  const estudante = await res.json();

  localStorage.setItem('od-session', JSON.stringify({
    type:               'student',
    id:                 estudante.id,
    name:               estudante.nome,
    email:              estudante.email,
    curso:              estudante.curso,
    instituicao:        estudante.instituicao,
    temTesteVocacional: estudante.temTesteVocacional
  }));
  return estudante;
}

async function studentLogin() {
  const email = document.getElementById('login-email').value.trim();
  const pw    = document.getElementById('login-pw').value;
  clearErrors();

  if (!email || !pw)
    return showError('login-email', 'Preencha e-mail e senha.');
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

    if (auth.tipoUsuario !== 'Estudante') {
      clearAuth();
      return showError('login-email', 'Esta conta é de empresa. Use o acesso para empresas.');
    }

    try {
      await carregarSessaoEstudante(auth.contaId);
    } catch (err) {
      clearAuth();
      return showError('login-email', err.message);
    }

    showSuccess('Login realizado!', 'Identidade verificada com sucesso. Bem-vindo(a) de volta ao Open Doors!', 'login');

  } catch (err) {
    showError('login-email', 'Erro ao conectar com o servidor. Tente novamente.');
    console.error(err);
  } finally {
    btn.disabled = false;
    btn.textContent = 'Entrar';
    btn.removeAttribute('aria-busy');
  }
}

function loginAsStudent() {
  setTimeout(() => {
    window.location.href = window.location.origin + '/PIM-3-Open-Doors-main/PIM%20-%20Front%20End/Areas/MenuEstudante.html';
  }, 1500);
}

// CEP (ViaCEP)
async function fetchCEP(rawVal) {
  const cep = rawVal.replace(/\D/g, '');
  if (cep.length !== 8) return;
  const spinner = document.getElementById('cep-spinner');
  if (spinner) spinner.style.display = 'block';
  try {
    const res  = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
    const data = await res.json();
    if (!data.erro) {
      document.getElementById('reg-logradouro').value = data.logradouro || '';
      document.getElementById('reg-bairro').value     = data.bairro     || '';
      document.getElementById('reg-cidade').value     = data.localidade || '';
      document.getElementById('reg-estado').value     = data.uf         || '';
      ['reg-cidade', 'reg-estado'].forEach(id => {
        const el = document.getElementById(id);
        el.readOnly = !!el.value;
      });
    } else {
      ['reg-cidade', 'reg-estado', 'reg-logradouro', 'reg-bairro'].forEach(id => {
        const el = document.getElementById(id);
        if (el) { el.value = ''; el.readOnly = false; }
      });
      showError('reg-cep', 'CEP não encontrado. Preencha o endereço manualmente.');
    }
  } catch (e) {
    ['reg-cidade', 'reg-estado', 'reg-logradouro', 'reg-bairro'].forEach(id => {
      const el = document.getElementById(id);
      if (el) { el.value = ''; el.readOnly = false; el.placeholder = 'Preencha manualmente'; }
    });
    showError('reg-cep', 'Não foi possível buscar o CEP. Preencha o endereço manualmente.');
  } finally {
    if (spinner) spinner.style.display = 'none';
  }
}

// BUSCA — INSTITUIÇÃO
const INSTITUICOES = [
  'USP – Universidade de São Paulo','UNICAMP – Universidade Estadual de Campinas',
  'UNESP – Universidade Estadual Paulista','UFRJ – Universidade Federal do Rio de Janeiro',
  'UFMG – Universidade Federal de Minas Gerais','UFSC – Universidade Federal de Santa Catarina',
  'UFRGS – Universidade Federal do Rio Grande do Sul','UnB – Universidade de Brasília',
  'UFBA – Universidade Federal da Bahia','UFC – Universidade Federal do Ceará',
  'UFPE – Universidade Federal de Pernambuco','UFPR – Universidade Federal do Paraná',
  'UFSCar – Universidade Federal de São Carlos','UNIFESP – Universidade Federal de São Paulo',
  'FGV – Fundação Getulio Vargas','PUC-SP – Pontifícia Universidade Católica de São Paulo',
  'PUC-Rio – Pontifícia Universidade Católica do Rio de Janeiro',
  'Mackenzie – Universidade Presbiteriana Mackenzie',
  'FATEC – Faculdade de Tecnologia do Estado de São Paulo',
  'SENAC – Centro Universitário Senac','SENAI – Serviço Nacional de Aprendizagem Industrial',
  'Anhanguera – Universidade Anhanguera','Estácio de Sá',
  'Uninter – Centro Universitário Internacional','UNIP – Universidade Paulista',
  'Cruzeiro do Sul – Universidade Cruzeiro do Sul',
  'FIAP – Faculdade de Informática e Administração Paulista',
  'INSPER – Instituto de Ensino e Pesquisa',
  'ITA – Instituto Tecnológico de Aeronáutica',
  'IME – Instituto Militar de Engenharia',
];

function searchInst(query) {
  const dropdown = document.getElementById('inst-dropdown');
  const q = query.trim().toLowerCase();
  if (!q) { dropdown.classList.remove('open'); return; }
  const matches = INSTITUICOES.filter(i => i.toLowerCase().includes(q)).slice(0, 8);
  renderDropdown(dropdown, matches, 'reg-instituicao', 'inst-dropdown');
}

// BUSCA — CURSO
const CURSOS = [
  'Administração','Agronomia','Análise e Desenvolvimento de Sistemas',
  'Arquitetura e Urbanismo','Automação Industrial','Biologia','Biomedicina',
  'Ciência da Computação','Ciências Contábeis','Ciências Econômicas',
  'Comunicação Social','Design Gráfico','Design de Interiores','Direito',
  'Educação Física','Enfermagem','Engenharia Civil','Engenharia de Computação',
  'Engenharia de Controle e Automação','Engenharia de Produção',
  'Engenharia de Software','Engenharia Elétrica','Engenharia Mecânica',
  'Engenharia Química','Estética e Cosmética','Farmácia','Física','Gastronomia',
  'Gestão de RH','Gestão Financeira','Gestão de TI',
  'Informática para Internet','Jornalismo','Letras','Logística','Marketing',
  'Matemática','Medicina','Medicina Veterinária','Nutrição','Odontologia',
  'Pedagogia','Psicologia','Publicidade e Propaganda','Química','Radiologia',
  'Relações Internacionais','Segurança do Trabalho','Serviço Social',
  'Sistemas de Informação','Turismo',
];

function searchCurso(query) {
  const dropdown = document.getElementById('curso-dropdown');
  const q = query.trim().toLowerCase();
  if (!q) { dropdown.classList.remove('open'); return; }
  const matches = CURSOS.filter(c => c.toLowerCase().includes(q)).slice(0, 8);
  renderDropdown(dropdown, matches, 'reg-curso', 'curso-dropdown');
}

function renderDropdown(dropdown, items, inputId, dropdownId) {
  dropdown.innerHTML = '';
  if (!items.length) {
    const li = document.createElement('li');
    li.className   = 'no-result';
    li.textContent = 'Nenhum resultado encontrado';
    li.setAttribute('role', 'option');
    li.setAttribute('aria-disabled', 'true');
    dropdown.appendChild(li);
  } else {
    items.forEach((item, i) => {
      const li = document.createElement('li');
      li.textContent = item;
      li.setAttribute('role', 'option');
      li.setAttribute('id', `${dropdownId}-opt-${i}`);
      li.addEventListener('mousedown', () => {
        document.getElementById(inputId).value = item;
        dropdown.classList.remove('open');
      });
      dropdown.appendChild(li);
    });
  }
  dropdown.classList.add('open');
  dropdown.setAttribute('role', 'listbox');
}

// ==============================================================
// CADASTRO — perfil + login no backend
// ==============================================================
async function studentRegister() {
  const name         = document.getElementById('reg-name').value.trim();
  const email        = document.getElementById('reg-email').value.trim();
  const cpf          = document.getElementById('reg-cpf').value.trim();
  const dob          = document.getElementById('reg-dob').value;
  const phone        = document.getElementById('reg-phone').value.trim();
  const cep          = document.getElementById('reg-cep').value.trim();
  const logradouro   = document.getElementById('reg-logradouro').value.trim();
  const numero       = document.getElementById('reg-numero').value.trim();
  const bairro       = document.getElementById('reg-bairro').value.trim();
  const cidade       = document.getElementById('reg-cidade').value.trim();
  const estado       = document.getElementById('reg-estado').value.trim();
  const escolaridade = document.getElementById('reg-escolaridade').value;
  const instituicao  = document.getElementById('reg-instituicao').value.trim();
  const curso        = document.getElementById('reg-curso').value.trim();
  const semestres    = document.getElementById('reg-semestres').value;
  const turno        = document.getElementById('reg-turno').value;
  const conclusao    = document.getElementById('reg-conclusao').value;
  const pw           = document.getElementById('reg-pw').value;
  const pwc          = document.getElementById('reg-pwc').value;
  const terms        = document.getElementById('reg-terms').checked;

  clearErrors();

  if (!name)  return showError('reg-name',  'Nome obrigatório.');
  if (!email) return showError('reg-email', 'E-mail obrigatório.');
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return showError('reg-email', 'E-mail inválido.');
  if (cpf.replace(/\D/g, '').length !== 11) return showError('reg-cpf',   'CPF inválido.');
  if (!dob)   return showError('reg-dob',   'Data de nascimento obrigatória.');
  if (!phone) return showError('reg-phone', 'Telefone obrigatório.');
  if (cep.replace(/\D/g,'').length !== 8) return showError('reg-cep', 'CEP inválido.');
  if (!logradouro) return showError('reg-logradouro', 'Logradouro obrigatório.');
  if (!numero)     return showError('reg-numero',     'Número obrigatório.');
  if (!bairro)     return showError('reg-bairro',     'Bairro obrigatório.');
  if (!cidade)     return showError('reg-cidade',     'Cidade obrigatória.');
  if (!estado)     return showError('reg-estado',     'Estado obrigatório.');
  if (!escolaridade) return showError('reg-escolaridade', 'Selecione seu nível de escolaridade.');
  if (!instituicao)  return showError('reg-instituicao',  'Informe sua instituição de ensino.');
  if (!curso)        return showError('reg-curso',        'Informe seu curso.');
  if (!semestres)    return showError('reg-semestres',    'Selecione a duração do curso.');
  if (!turno)        return showError('reg-turno',        'Selecione o turno.');
  if (!conclusao)    return showError('reg-conclusao',    'Informe a data de conclusão.');
  if (pw.length < 8) return showError('reg-pw',  'Mínimo 8 caracteres.');
  if (pw !== pwc)    return showError('reg-pwc',  'As senhas não coincidem.');
  if (!terms)        return showError('reg-terms-field', 'Aceite os termos para continuar.');

  const btn = document.querySelector('#screen-register .btn-primary');
  btn.disabled = true;
  btn.textContent = 'Criando conta...';
  btn.setAttribute('aria-busy', 'true');

  try {
    // 1. Cria perfil + login no backend (o Id da conta é gerado pela API)
    const payload = {
      nome:              name,
      email:             email,
      senha:             pw,
      cpf:               cpf.replace(/\D/g, ''),
      telefone:          phone,
      dataNascimento:    dob,
      cidade:            cidade,
      estado:            estado,
      instituicao:       instituicao,
      curso:             curso,
      semestre:          parseInt(semestres) || null,
      turno:             turno,
      previsaoConclusao: conclusao,
      temCurriculo:      false,
      temTesteVocacional: false,
      status:            'ativo'
    };

    const res = await fetch(`${API}/api/estudantes`, {
      method:  'POST',
      headers: { 'Content-Type': 'application/json', 'ngrok-skip-browser-warning': '1' },
      body:    JSON.stringify(payload)
    });

    if (!res.ok) {
      const err = await res.json().catch(() => ({}));
      if (res.status === 409) return showError('reg-email', 'Este e-mail já possui uma conta.');
      throw new Error(err.erro || `Erro ${res.status}`);
    }

    // 2. Entra com a conta recém-criada para obter o token
    const auth = await apiLogin(email, pw);
    await carregarSessaoEstudante(auth.contaId);

    showSuccess(
      `Bem-vindo(a), ${name.split(' ')[0]}!`,
      'Conta criada com sucesso.',
      'register'
    );

  } catch (err) {
    showError('reg-email', `Erro ao criar conta: ${err.message}`);
    console.error(err);
  } finally {
    btn.disabled = false;
    btn.textContent = 'Criar conta';
    btn.removeAttribute('aria-busy');
  }
}

// ==============================================================
// REDEFINIÇÃO DE SENHA
// ==============================================================
// O backend ainda não envia e-mails, então a recuperação por e-mail não está disponível.
// Com a conta logada, a senha pode ser trocada em "Segurança" no painel.
function sendResetCode() {
  clearErrors();
  showError('forgot-email',
    'A recuperação de senha por e-mail ainda não está disponível. Entre em contato com o suporte do Open Doors.');
}

function showSuccess(title, msg, flow) {
  document.getElementById('success-title').textContent = title;
  document.getElementById('success-msg').textContent   = msg;
  document.getElementById('success-actions-login').style.display    = 'none';
  document.getElementById('success-actions-register').style.display = 'none';
  document.getElementById('success-actions-reset').style.display    = 'none';
  const which = flow === 'login' ? 'success-actions-login'
      : flow === 'reset' ? 'success-actions-reset'
      : 'success-actions-register';
  document.getElementById(which).style.display = 'flex';
  showScreen('screen-success');
}

// EVENT LISTENERS
document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('select').forEach(sel => {
    sel.addEventListener('change', () => {
      const err = sel.closest('.field')?.querySelector('.field-error');
      if (err) err.remove();
      sel.style.borderColor = '';
      sel.removeAttribute('aria-invalid');
    });
  });

  document.addEventListener('click', e => {
    if (!e.target.closest('#inst-search-wrap'))
      document.getElementById('inst-dropdown').classList.remove('open');
    if (!e.target.closest('#curso-search-wrap'))
      document.getElementById('curso-dropdown').classList.remove('open');
  });

  ['inst-dropdown', 'curso-dropdown'].forEach(ddId => {
    const inputId = ddId === 'inst-dropdown' ? 'reg-instituicao' : 'reg-curso';
    const input   = document.getElementById(inputId);
    if (!input) return;
    input.addEventListener('keydown', e => {
      const dd    = document.getElementById(ddId);
      const items = dd.querySelectorAll('li:not(.no-result)');
      if (!dd.classList.contains('open') || !items.length) return;
      const active = dd.querySelector('li.active');
      let idx = Array.from(items).indexOf(active);
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        if (active) active.classList.remove('active');
        idx = (idx + 1) % items.length;
        items[idx].classList.add('active');
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        if (active) active.classList.remove('active');
        idx = (idx - 1 + items.length) % items.length;
        items[idx].classList.add('active');
      } else if (e.key === 'Enter' && active) {
        e.preventDefault();
        input.value = active.textContent;
        dd.classList.remove('open');
      } else if (e.key === 'Escape') {
        dd.classList.remove('open');
      }
    });
  });
});
