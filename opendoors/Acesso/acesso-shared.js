/* Funções compartilhadas pelas páginas de acesso */

// TEMA 
function toggleTheme() {
  const html  = document.documentElement;
  const isDark = html.getAttribute('data-theme') === 'dark';
  const next  = isDark ? 'light' : 'dark';
  html.setAttribute('data-theme', next);
  try { localStorage.setItem('od-theme', next); } catch (e) {}
}

(function () {
  try {
    const saved = localStorage.getItem('od-theme');
    if (saved) document.documentElement.setAttribute('data-theme', saved);
  } catch (e) {}
})();

// VIEW ROUTER 
function showScreen(id) {
  document.querySelectorAll('.screen, .success-screen').forEach(s => {
    s.classList.remove('active');
  });
  const el = document.getElementById(id);
  if (el) el.classList.add('active');
}

// MÁSCARAS 
function maskPhone(el) {
  let v = el.value.replace(/\D/g, '').slice(0, 11);
  if      (v.length > 10) v = v.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  else if (v.length >  6) v = v.replace(/(\d{2})(\d{4,5})(\d*)/, '($1) $2-$3');
  else if (v.length >  2) v = v.replace(/(\d{2})(\d+)/, '($1) $2');
  el.value = v;
}

function maskCEP(el) {
  let v = el.value.replace(/\D/g, '').slice(0, 8);
  if (v.length > 5) v = v.replace(/(\d{5})(\d+)/, '$1-$2');
  el.value = v;
}

function maskCPF(el) {
  let v = el.value.replace(/\D/g, '').slice(0, 11);
  if      (v.length > 9) v = v.replace(/(\d{3})(\d{3})(\d{3})(\d{1,2})/, '$1.$2.$3-$4');
  else if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d+)/, '$1.$2.$3');
  else if (v.length > 3) v = v.replace(/(\d{3})(\d+)/, '$1.$2');
  el.value = v;
}

// ERROS INLINE 
function showError(fieldId, msg) {
  const field = document.getElementById(fieldId);
  if (!field) { alert(msg); return; }

  const existing = field.parentElement.querySelector('.field-error');
  if (existing) existing.remove();

  const err = document.createElement('span');
  err.className   = 'field-error';
  err.textContent = msg;
  err.setAttribute('role', 'alert');
  err.setAttribute('aria-live', 'polite');
  err.style.cssText = 'font-size:11px;color:var(--error);margin-top:4px;display:block;';

  const wrapper = field.closest('.select-wrap') || field.closest('.search-wrap') || field.closest('.cep-wrap');
  const container = wrapper ? wrapper.parentElement : field.parentElement;
  container.appendChild(err);

  field.style.borderColor = 'var(--error)';
  field.setAttribute('aria-invalid', 'true');
  field.focus();
}

function clearErrors() {
  document.querySelectorAll('.field-error').forEach(e => e.remove());
  document.querySelectorAll('input, select').forEach(i => {
    i.style.borderColor = '';
    i.removeAttribute('aria-invalid');
  });
}

// Limpa erro ao digitar
document.addEventListener('DOMContentLoaded', () => {
  document.querySelectorAll('input').forEach(input => {
    input.addEventListener('input', () => {
      const err = input.parentElement.querySelector('.field-error');
      if (err) err.remove();
      input.style.borderColor = '';
      input.removeAttribute('aria-invalid');
    });
  });
});

// ==============================================================
// LOGIN COM GOOGLE (Google Identity Services)
// ==============================================================
// Cada página define onGoogleCredential(credential). Os botões do Google
// são desenhados nos elementos .google-slot; elementos [data-google-area]
// somem quando o Google não está configurado no backend (Google:ClientId).
let googleConfigurado = false;

function carregarScriptGoogle() {
  return new Promise((resolve, reject) => {
    if (window.google?.accounts?.id) return resolve();
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.onload = resolve;
    script.onerror = reject;
    document.head.appendChild(script);
  });
}

async function iniciarLoginGoogle() {
  const clientId = await apiGoogleClientId();
  if (!clientId) {
    document.querySelectorAll('[data-google-area]').forEach(el => { el.style.display = 'none'; });
    return;
  }

  try {
    await carregarScriptGoogle();
  } catch (e) {
    console.warn('Não foi possível carregar o login com Google:', e);
    return;
  }

  google.accounts.id.initialize({
    client_id: clientId,
    callback: resp => onGoogleCredential(resp.credential),
    ux_mode: 'popup'
  });

  document.querySelectorAll('.google-slot').forEach(slot => {
    google.accounts.id.renderButton(slot, {
      theme: document.documentElement.getAttribute('data-theme') === 'dark' ? 'filled_black' : 'outline',
      size: 'large', text: 'continue_with', shape: 'rectangular', locale: 'pt-BR', width: 240
    });
    slot.style.display = '';
  });
  document.querySelectorAll('.google-fallback').forEach(btn => { btn.style.display = 'none'; });
  googleConfigurado = true;
}

document.addEventListener('DOMContentLoaded', () => { iniciarLoginGoogle(); });

// Cadastro iniciado pelo Google: preenche nome/e-mail e esconde os campos de senha
let googleCredentialCadastro = null;

function prepararCadastroGoogle(credential, { nome, email }, campos) {
  googleCredentialCadastro = credential;

  const nomeEl  = document.getElementById(campos.nome);
  const emailEl = document.getElementById(campos.email);
  if (nomeEl && !nomeEl.value) nomeEl.value = nome || '';
  if (emailEl) { emailEl.value = email; emailEl.readOnly = true; }

  campos.senhaContainers.forEach(el => { if (el) el.style.display = 'none'; });

  let aviso = document.getElementById('google-cadastro-aviso');
  if (!aviso) {
    aviso = document.createElement('div');
    aviso.id = 'google-cadastro-aviso';
    aviso.className = 'info-box';
    aviso.setAttribute('role', 'status');
    const fields = document.querySelector('#screen-register .fields');
    fields.insertBefore(aviso, fields.firstChild);
  }
  aviso.textContent = `Você está se cadastrando com o Google (${email}). Complete os dados abaixo — não é preciso criar senha.`;
}

// ==============================================================
// ESQUECI MINHA SENHA — código de 6 dígitos por e-mail
// ==============================================================
// Cada página define mostrarSucessoRedefinicao().
async function sendResetCode(isResend = false) {
  const emailEl = document.getElementById('forgot-email');
  const email = emailEl.value.trim();
  clearErrors();
  if (!email) return showError('forgot-email', 'Informe o e-mail cadastrado.');
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return showError('forgot-email', 'E-mail inválido.');

  const btn = document.querySelector('#screen-forgot .btn-primary');
  if (btn) { btn.disabled = true; btn.textContent = 'Enviando...'; }

  try {
    await apiEsqueciSenha(email);
    document.getElementById('reset-email-target').textContent = email;
    if (isResend) {
      alert('Se o e-mail estiver cadastrado, um novo código foi enviado. Verifique sua caixa de entrada e o spam.');
    } else {
      document.querySelectorAll('.reset-digit').forEach(d => { d.value = ''; });
      document.getElementById('reset-otp-error').textContent = '';
      showScreen('screen-reset');
      document.querySelector('.reset-digit')?.focus();
    }
  } catch (err) {
    if (isResend) alert(err.message);
    else showError('forgot-email', err.message || 'Erro ao enviar o e-mail. Tente novamente.');
    console.error(err);
  } finally {
    if (btn) { btn.disabled = false; btn.textContent = 'Enviar código'; }
  }
}

// Pula para o próximo dígito; colar o código inteiro preenche todos
function resetOtpNext(el, idx) {
  const digits = document.querySelectorAll('.reset-digit');
  const valor = el.value.replace(/\D/g, '');
  if (valor.length > 1) {
    valor.slice(0, digits.length - idx).split('').forEach((d, i) => { digits[idx + i].value = d; });
    digits[Math.min(idx + valor.length, digits.length) - 1].focus();
    return;
  }
  el.value = valor;
  if (valor && digits[idx + 1]) digits[idx + 1].focus();
  document.getElementById('reset-otp-error').textContent = '';
}

function checkResetStrength(input) {
  const pw = input.value;
  let score = 0;
  if (pw.length >= 8)          score++;
  if (/[A-Z]/.test(pw))        score++;
  if (/[0-9]/.test(pw))        score++;
  if (/[^A-Za-z0-9]/.test(pw)) score++;
  const cls   = ['', 'weak', 'weak', 'fair', 'strong'][score];
  const texto = ['', 'Senha fraca', 'Senha fraca', 'Senha razoável', 'Senha forte'][score];
  document.querySelectorAll('#reset-pw-meter .pw-bar').forEach((bar, i) => {
    bar.className = 'pw-bar';
    if (i < score) bar.classList.add(cls);
  });
  const label = document.getElementById('reset-pw-label');
  if (label) label.textContent = texto;
}

async function confirmReset() {
  clearErrors();
  const otpErr = document.getElementById('reset-otp-error');
  otpErr.textContent = '';

  const email  = document.getElementById('forgot-email').value.trim();
  const codigo = Array.from(document.querySelectorAll('.reset-digit')).map(d => d.value).join('');
  const pw     = document.getElementById('reset-pw').value;
  const pwc    = document.getElementById('reset-pwc').value;

  if (!/^\d{6}$/.test(codigo)) { otpErr.textContent = 'Digite os 6 dígitos do código.'; return; }
  if (pw.length < 8) return showError('reset-pw', 'Mínimo 8 caracteres.');
  if (pw !== pwc)    return showError('reset-pwc', 'As senhas não coincidem.');

  const btn = document.querySelector('#screen-reset .btn-primary');
  if (btn) { btn.disabled = true; btn.textContent = 'Redefinindo...'; }

  try {
    await apiRedefinirSenha(email, codigo, pw);
    ['reset-pw', 'reset-pwc'].forEach(id => { document.getElementById(id).value = ''; });
    mostrarSucessoRedefinicao();
  } catch (err) {
    otpErr.textContent = err.message || 'Não foi possível redefinir a senha.';
    otpErr.setAttribute('role', 'alert');
  } finally {
    if (btn) { btn.disabled = false; btn.textContent = 'Redefinir senha'; }
  }
}
