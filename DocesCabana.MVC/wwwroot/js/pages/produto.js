// Seletor de quantidade da página do produto (RN-10: inteiro entre 1 e 99,
// começa em 1). É o único JavaScript da tela — o resto funciona sem ele
// (plano §6, "Movimento e acessibilidade").
document.addEventListener("DOMContentLoaded", function () {
    const seletor = document.querySelector("[data-seletor-quantidade]");
    if (!seletor) return;

    const valorSpan = seletor.querySelector("[data-quantidade-valor]");
    const botaoMenos = seletor.querySelector("[data-quantidade-menos]");
    const botaoMais = seletor.querySelector("[data-quantidade-mais]");

    const MINIMO = 1;
    const MAXIMO = 99;

    function obterQuantidade() {
        return parseInt(valorSpan.textContent, 10) || MINIMO;
    }

    function definirQuantidade(valor) {
        const limitada = Math.min(MAXIMO, Math.max(MINIMO, valor));
        valorSpan.textContent = String(limitada);
    }

    botaoMenos.addEventListener("click", function () {
        definirQuantidade(obterQuantidade() - 1);
    });

    botaoMais.addEventListener("click", function () {
        definirQuantidade(obterQuantidade() + 1);
    });
});
