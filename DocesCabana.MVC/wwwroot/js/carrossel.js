// Carrossel - Navegação e Autoplay

document.addEventListener("DOMContentLoaded", () => {
    const containerIndicadores = document.querySelector('.carrossel-indicadores');
    const slides = document.querySelectorAll('.carrossel-item');
    const botaoAnterior = document.querySelector('.seta-anterior');
    const botaoProximo = document.querySelector('.seta-proximo');

    // Se o carrossel não estiver presente na página, interrompe a execução
    if (!slides.length || !containerIndicadores) return;

    let indiceAtivo = 0;
    const tempoAutoplay = 5000;
    let temporizadorAutoplay;

    // Geração dinâmica das bolinhas indicadoras
    slides.forEach((_, index) => {
        const ponto = document.createElement('span');
        ponto.classList.add('ponto-indicador');

        // Define a primeira bolinha como ativa inicialmente
        if (index === 0) {
            ponto.classList.add('ativo');
        }

        // Evento de clique para ir ao slide correspondente
        ponto.addEventListener('click', () => {
            mostrarSlide(index);
            reiniciarAutoplay();
        });

        containerIndicadores.appendChild(ponto);
    });

    const pontos = containerIndicadores.querySelectorAll('.ponto-indicador');

    // Função principal de transição dos slides e bolinhas
    function mostrarSlide(indice) {
        // Remove a classe ativo de todos os elementos
        slides.forEach(slide => slide.classList.remove('ativo'));
        pontos.forEach(ponto => ponto.classList.remove('ativo'));

        // Validação e loop circular dos índices
        if (indice >= slides.length) {
            indiceAtivo = 0;
        } else if (indice < 0) {
            indiceAtivo = slides.length - 1;
        } else {
            indiceAtivo = indice;
        }

        // Ativa o slide e indicador atual
        slides[indiceAtivo].classList.add('ativo');
        if (pontos[indiceAtivo]) {
            pontos[indiceAtivo].classList.add('ativo');
        }
    }

    // Funções auxiliares para navegação
    function proximoSlide() {
        mostrarSlide(indiceAtivo + 1);
    }

    function slideAnterior() {
        mostrarSlide(indiceAtivo - 1);
    }

    // Temporização do Autoplay
    function iniciarAutoplay() {
        temporizadorAutoplay = setInterval(proximoSlide, tempoAutoplay);
    }

    // Reinicia o cronômetro para evitar pulos imediatos ao clicar manualmente
    function reiniciarAutoplay() {
        clearInterval(temporizadorAutoplay);
        iniciarAutoplay();
    }

    // Configuração dos escutadores de clique nas setas
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