
document.addEventListener("DOMContentLoaded", () => {
    const containerIndicadores = document.querySelector('.carrossel-indicadores');
    const slides = document.querySelectorAll('.carrossel-item');
    const botaoAnterior = document.querySelector('.seta-anterior');
    const botaoProximo = document.querySelector('.seta-proximo');

    if (!slides.length || !containerIndicadores) return;

    let indiceAtivo = 0;
    const tempoAutoplay = 5000;
    let temporizadorAutoplay;

    slides.forEach((_, index) => {
        const ponto = document.createElement('span');
        ponto.classList.add('ponto-indicador');

        if (index === 0) {
            ponto.classList.add('ativo');
        }

        ponto.addEventListener('click', () => {
            mostrarSlide(index);
            reiniciarAutoplay();
        });

        containerIndicadores.appendChild(ponto);
    });

    const pontos = containerIndicadores.querySelectorAll('.ponto-indicador');

    function mostrarSlide(indice) {
        slides.forEach(slide => slide.classList.remove('ativo'));
        pontos.forEach(ponto => ponto.classList.remove('ativo'));

        if (indice >= slides.length) {
            indiceAtivo = 0;
        } else if (indice < 0) {
            indiceAtivo = slides.length - 1;
        } else {
            indiceAtivo = indice;
        }

        slides[indiceAtivo].classList.add('ativo');
        if (pontos[indiceAtivo]) {
            pontos[indiceAtivo].classList.add('ativo');
        }
    }

    function proximoSlide() {
        mostrarSlide(indiceAtivo + 1);
    }

    function slideAnterior() {
        mostrarSlide(indiceAtivo - 1);
    }

    function iniciarAutoplay() {
        temporizadorAutoplay = setInterval(proximoSlide, tempoAutoplay);
    }

    function reiniciarAutoplay() {
        clearInterval(temporizadorAutoplay);
        iniciarAutoplay();
    }

    if (botaoProximo) {
        botaoProximo.addEventListener('click', () => {
            proximoSlide();
            reiniciarAutoplay();
        });
    }

    if (botaoAnterior) {
        botaoAnterior.addEventListener('click', () => {
            slideAnterior();
            reiniciarAutoplay();
        });
    }

    iniciarAutoplay();
});