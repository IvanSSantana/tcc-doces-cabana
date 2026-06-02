// Funções interativas para o Card de Produto

function alterarQuantidade(btn, delta) {
    const controls = btn.closest('.controles-card');
    if (!controls) return;

    const qtySpan = controls.querySelector('.valor-quantidade-card');
    if (!qtySpan) return;

    let currentQty = parseInt(qtySpan.textContent) || 1;
    let newQty = currentQty + delta;

    if (newQty < 1) newQty = 1;

    qtySpan.textContent = newQty;
}

function toggleFavorite(btn) {
    const icon = btn.querySelector('svg');
    if (!icon) {
        console.error("Erro: Ícone de favorito não encontrado.")
        return;
    }

    const isFavorited = btn.querySelector('[data-prefix="fas"]') ? true : false;
    console.debug("O botão está favoritado: " + isFavorited)

    if (isFavorited) {
        icon.classList.add('fa-regular');
        icon.classList.remove('fa-solid');
    } else {
        console.debug("Debug: Favoritando botão")
        icon.classList.add('fa-solid');
        icon.classList.remove('fa-regular');
    }
}

function adicionarAoCarrinho(produtoId, btn) {
    const card = btn.closest('.card-produto');
    if (!card) return;

    const qtySpan = card.querySelector('.valor-quantidade-card');
    const quantidade = qtySpan ? parseInt(qtySpan.textContent) : 1;

    console.log(`Adicionado ao carrinho: Produto ID: ${produtoId}, Quantidade: ${quantidade}`);

    // Feedback visual temporário ao adicionar
    const originalText = btn.textContent;
    btn.textContent = 'Adicionado!';
    btn.style.backgroundColor = '#006b52';

    setTimeout(() => {
        btn.textContent = originalText;
        btn.style.backgroundColor = '';
    }, 1500);
}
