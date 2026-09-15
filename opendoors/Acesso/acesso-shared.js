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
