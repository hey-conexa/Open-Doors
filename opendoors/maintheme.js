/* tema claro/escuro da página principal*/

(function () {
    try {
        const saved = localStorage.getItem('od-theme');
        if (saved) document.documentElement.setAttribute('data-theme', saved);
    } catch (e) {}
})();

function toggleTheme() {
    const html   = document.documentElement;
    const isDark = html.getAttribute('data-theme') === 'dark';
    const next   = isDark ? 'light' : 'dark';
    html.setAttribute('data-theme', next);
    try { localStorage.setItem('od-theme', next); } catch (e) {}
}
