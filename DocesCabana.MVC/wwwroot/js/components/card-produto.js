// Funções interativas para o Card de Produto

function alterarQuantidade(botao, delta) {
    const controls = botao.closest('.controles-card');
    if (!controls) return;

    const qtySpan = controls.querySelector('.valor-quantidade-card');
    if (!qtySpan) return;

    let currentQty = parseInt(qtySpan.textContent) || 1;
    let newQty = currentQty + delta;

    if (newQty < 1) newQty = 1;

    qtySpan.textContent = newQty;
}

function alternarFavorito(botao) {
    const icon = botao.querySelector('svg');
    if (!icon) {
        return;
    }

    const isFavorited = botao.querySelector('[data-prefix="fas"]') ? true : false;

    if (isFavorited) {
        icon.classList.add('fa-regular');
        icon.classList.remove('fa-solid');
    } else {
        icon.classList.add('fa-solid');
        icon.classList.remove('fa-regular');
    }
}

function adicionarAoCarrinho(produtoId, botao) {
    const card = botao.closest('.card-produto');
    if (!card) return;

    const qtySpan = card.querySelector('.valor-quantidade-card');
    const quantidade = qtySpan ? parseInt(qtySpan.textContent) : 1;

    // Feedback visual temporário ao adicionar
    const originalText = botao.textContent;
    botao.textContent = 'Adicionado!';
    botao.style.backgroundColor = '#006b52';

    setTimeout(() => {
        botao.textContent = originalText;
        botao.style.backgroundColor = '';
    }, 1500);
}
