// Script de controle interativo para a Vitrine Carrossel de Produtos

function pegarEstadoCarrossel(elemento) {
    const container = elemento.closest('.vitrine-carrossel');
    const trilha = container.querySelector('.trilha-carrossel');
    const itens = container.querySelectorAll('.item-carrossel');
    const pontos = container.querySelectorAll('.ponto-indicador');
    
    const larguraItem = itens.length > 0 ? itens[0].getBoundingClientRect().width : 0;
    
    let gap = 20;
    if (itens.length > 1) {
        const rect1 = itens[0].getBoundingClientRect();
        const rect2 = itens[1].getBoundingClientRect();
        gap = rect2.left - rect1.right;
    }
    
    let itensVisiveis = 4;
    const larguraTela = window.innerWidth;
    if (larguraTela <= 480) itensVisiveis = 1;
    else if (larguraTela <= 768) itensVisiveis = 2;
    else if (larguraTela <= 1024) itensVisiveis = 3;
    
    const indiceMaximo = Math.max(0, itens.length - itensVisiveis);
    let indiceAtual = parseInt(container.dataset.indiceAtual) || 0;
    
    return { container, trilha, itens, pontos, larguraItem, gap, itensVisiveis, indiceMaximo, indiceAtual };
}

function atualizarPosicaoCarrossel(estado) {
    const { container, trilha, pontos, larguraItem, gap, indiceAtual, indiceMaximo } = estado;
    
    let idx = Math.min(Math.max(0, indiceAtual), indiceMaximo);
    container.dataset.indiceAtual = idx;
    
    const quantidadeMover = idx * (larguraItem + gap);
    trilha.style.transform = `translateX(-${quantidadeMover}px)`;
    
    const botaoAnterior = container.querySelector('.seta-carrossel.prev');
    const botaoProximo = container.querySelector('.seta-carrossel.next');
    
    if (botaoAnterior) {
        if (idx === 0) {
            botaoAnterior.style.opacity = '0.4';
            botaoAnterior.style.cursor = 'default';
        } else {
            botaoAnterior.style.opacity = '1';
            botaoAnterior.style.cursor = 'pointer';
        }
    }
    
    if (botaoProximo) {
        if (idx === indiceMaximo) {
            botaoProximo.style.opacity = '0.4';
            botaoProximo.style.cursor = 'default';
        } else {
            botaoProximo.style.opacity = '1';
            botaoProximo.style.cursor = 'pointer';
        }
    }
    
    pontos.forEach((ponto, indicePonto) => {
        if (indicePonto === idx) {
            ponto.classList.add('active');
        } else {
            ponto.classList.remove('active');
        }
    });
}

function irItemAnterior(botao) {
    const estado = pegarEstadoCarrossel(botao);
    if (estado.indiceAtual > 0) {
        estado.indiceAtual--;
        atualizarPosicaoCarrossel(estado);
    }
}

function irProximoItem(botao) {
    const estado = pegarEstadoCarrossel(botao);
    if (estado.indiceAtual < estado.indiceMaximo) {
        estado.indiceAtual++;
        atualizarPosicaoCarrossel(estado);
    }
}

function irParaItem(ponto, indice) {
    const estado = pegarEstadoCarrossel(ponto);
    estado.indiceAtual = indice;
    atualizarPosicaoCarrossel(estado);
}

document.addEventListener('DOMContentLoaded', () => {
    function inicializarCarrosseis() {
        document.querySelectorAll('.vitrine-carrossel').forEach(container => {
            if (!container.dataset.inicializado) {
                container.dataset.indiceAtual = 0;
                container.dataset.inicializado = 'true';
            }
            
            const trilha = container.querySelector('.trilha-carrossel');
            if (trilha) {
                const pontos = container.querySelectorAll('.ponto-indicador');
                
                function ajustarPontos() {
                    const estado = pegarEstadoCarrossel(trilha);
                    
                    pontos.forEach((ponto, indicePonto) => {
                        if (indicePonto > estado.indiceMaximo) {
                            ponto.style.display = 'none';
                        } else {
                            ponto.style.display = '';
                        }
                    });
                    
                    atualizarPosicaoCarrossel(estado);
                }
                
                window.addEventListener('resize', ajustarPontos);
                setTimeout(ajustarPontos, 150);
            }
        });
    }

    inicializarCarrosseis();
    window.inicializarVitrineCarrosseis = inicializarCarrosseis;
});
