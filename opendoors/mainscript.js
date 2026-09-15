window.addEventListener('load', () => {

    const prefersReduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // CAROUSEL APOIADORES
    
    if (!prefersReduced) {
        document.querySelectorAll('.carousel-imagens').forEach(track => {
            const velocidade = 1;

            const wrapper = track.parentElement;
            const larguraWrapper = wrapper.offsetWidth;
            const imagensOriginais = Array.from(track.querySelectorAll('img'));

            while (track.scrollWidth < larguraWrapper * 3) {
                imagensOriginais.forEach(img => {
                    track.appendChild(img.cloneNode(true));
                });
            }

            const larguraItem = imagensOriginais[0].offsetWidth + 20;
            let posicao = 0;
            let pausado = false;

            function moverCarousel() {
                if (!pausado) {
                    posicao -= velocidade;
                    const primeiraImagem = track.querySelector('img');
                    if (Math.abs(posicao) >= larguraItem) {
                        track.appendChild(primeiraImagem);
                        posicao += larguraItem;
                    }
                    track.style.transform = `translateX(${posicao}px)`;
                }
                requestAnimationFrame(moverCarousel);
            }

            const wrapper2 = track.closest('.carousel-wrapper');
            if (wrapper2) {
                wrapper2.addEventListener('mouseenter', () => { pausado = true; });
                wrapper2.addEventListener('mouseleave', () => { pausado = false; });
            }

            requestAnimationFrame(moverCarousel);
        });
    }


    // MURAL

    (function () {
        const track  = document.getElementById('muralTrack');
        const dots   = document.querySelectorAll('.mural-dot');
        const thumbs = document.querySelectorAll('.mural-thumb');
        const pauseBtn = document.getElementById('muralPauseBtn');

        if (!track) return;

        let current = 0;
        const total = dots.length;
        let timer;
        let isPaused = prefersReduced; // se reduzido, começa pausado

        function muralGoTo(n) {
            current = (n + total) % total;
            track.style.transform = `translateX(-${current * 100}%)`;
            dots.forEach((d, i)  => d.classList.toggle('mural-dot--active',   i === current));
            thumbs.forEach((t, i) => t.classList.toggle('mural-thumb--active', i === current));
            if (!isPaused) {
                clearInterval(timer);
                timer = setInterval(() => muralGoTo(current + 1), 4000);
            }
        }

        function muralPause() {
            isPaused = !isPaused;
            if (isPaused) {
                clearInterval(timer);
                if (pauseBtn) {
                    pauseBtn.setAttribute('aria-label', 'Retomar carrossel');
                    pauseBtn.innerHTML = '&#9654;';
                }
            } else {
                timer = setInterval(() => muralGoTo(current + 1), 4000);
                if (pauseBtn) {
                    pauseBtn.setAttribute('aria-label', 'Pausar carrossel');
                    pauseBtn.innerHTML = '&#9646;&#9646;';
                }
            }
        }

        window.muralGoTo = muralGoTo;
        window.muralMove = function (dir) { muralGoTo(current + dir); };
        window.muralPause = muralPause;

        if (pauseBtn) {
            pauseBtn.addEventListener('click', muralPause);
        }

        if (!isPaused) {
            timer = setInterval(() => muralGoTo(current + 1), 4000);
        }
    })();

});
