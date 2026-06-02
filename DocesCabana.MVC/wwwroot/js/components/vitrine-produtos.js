// Script de controle interativo para a Vitrine Carrossel de Produtos

function getCarouselState(element) {
    const container = element.closest('.vitrine-carrossel');
    const track = container.querySelector('.trilha-carrossel');
    const slides = container.querySelectorAll('.item-carrossel');
    const dots = container.querySelectorAll('.ponto-indicador');
    
    // Calcula as larguras e gaps reais dos slides
    const slideWidth = slides.length > 0 ? slides[0].getBoundingClientRect().width : 0;
    
    // Obtém o gap do flexbox (padrão 20px se não detectado)
    let gap = 20;
    if (slides.length > 1) {
        const rect1 = slides[0].getBoundingClientRect();
        const rect2 = slides[1].getBoundingClientRect();
        gap = rect2.left - rect1.right;
    }
    
    // Define a visibilidade responsiva
    let visibleSlides = 4;
    const width = window.innerWidth;
    if (width <= 480) visibleSlides = 1;
    else if (width <= 768) visibleSlides = 2;
    else if (width <= 1024) visibleSlides = 3;
    
    // Máximo de scroll possível
    const maxIndex = Math.max(0, slides.length - visibleSlides);
    let currentIndex = parseInt(container.dataset.currentIndex) || 0;
    
    return { container, track, slides, dots, slideWidth, gap, visibleSlides, maxIndex, currentIndex };
}

function updateCarouselPosition(state) {
    const { container, track, dots, slideWidth, gap, currentIndex, maxIndex } = state;
    
    // Limita o índice entre 0 e o máximo de scroll
    let idx = Math.min(Math.max(0, currentIndex), maxIndex);
    container.dataset.currentIndex = idx;
    
    // Calcula o deslocamento
    const amountToMove = idx * (slideWidth + gap);
    track.style.transform = `translateX(-${amountToMove}px)`;
    
    // Habilita/Desabilita setas conforme limites de scroll
    const prevBtn = container.querySelector('.seta-carrossel.prev');
    const nextBtn = container.querySelector('.seta-carrossel.next');
    
    if (prevBtn) {
        if (idx === 0) {
            prevBtn.style.opacity = '0.4';
            prevBtn.style.cursor = 'default';
        } else {
            prevBtn.style.opacity = '1';
            prevBtn.style.cursor = 'pointer';
        }
    }
    
    if (nextBtn) {
        if (idx === maxIndex) {
            nextBtn.style.opacity = '0.4';
            nextBtn.style.cursor = 'default';
        } else {
            nextBtn.style.opacity = '1';
            nextBtn.style.cursor = 'pointer';
        }
    }
    
    // Atualiza a classe ativa nos dots
    dots.forEach((dot, dIdx) => {
        if (dIdx === idx) {
            dot.classList.add('active');
        } else {
            dot.classList.remove('active');
        }
    });
}

function carouselSlideLeft(btn) {
    const state = getCarouselState(btn);
    if (state.currentIndex > 0) {
        state.currentIndex--;
        updateCarouselPosition(state);
    }
}

function carouselSlideRight(btn) {
    const state = getCarouselState(btn);
    if (state.currentIndex < state.maxIndex) {
        state.currentIndex++;
        updateCarouselPosition(state);
    }
}

function carouselGoToSlide(dot, index) {
    const state = getCarouselState(dot);
    state.currentIndex = index;
    updateCarouselPosition(state);
}

// Inicialização e gerenciamento de redimensionamento
document.addEventListener('DOMContentLoaded', () => {
    function initCarousels() {
        document.querySelectorAll('.vitrine-carrossel').forEach(container => {
            if (!container.dataset.initialized) {
                container.dataset.currentIndex = 0;
                container.dataset.initialized = 'true';
            }
            
            const track = container.querySelector('.trilha-carrossel');
            if (track) {
                const dots = container.querySelectorAll('.ponto-indicador');
                
                function adjustDots() {
                    const state = getCarouselState(track);
                    
                    // Oculta dots redundantes (que estariam fora de limites por falta de slides)
                    dots.forEach((dot, idx) => {
                        if (idx > state.maxIndex) {
                            dot.style.display = 'none';
                        } else {
                            dot.style.display = '';
                        }
                    });
                    
                    updateCarouselPosition(state);
                }
                
                // Trata redimensionamento para recalcular widths e visibilidade
                window.addEventListener('resize', adjustDots);
                
                // Pequeno atraso para aguardar o render completo do browser e calcular larguras certas
                setTimeout(adjustDots, 150);
            }
        });
    }

    initCarousels();
    
    // Registra re-inicialização caso novos elementos HTML surjam via AJAX
    window.initVitrineCarousels = initCarousels;
});
