// Seletor de quantidade da página do produto (RN-10/RN-02: inteiro entre 1 e
// 99, começa em 1). O campo em si é um <input type="number"> de verdade
// (spec 017, RF-02) — funciona sem script nenhum; os ± são só um atalho.
document.addEventListener("DOMContentLoaded", function () {
    const seletor = document.querySelector("[data-seletor-quantidade]");
    if (!seletor) return;

    const campo = seletor.querySelector("[data-quantidade-valor]");
    const botaoMenos = seletor.querySelector("[data-quantidade-menos]");
    const botaoMais = seletor.querySelector("[data-quantidade-mais]");

    const MINIMO = 1;
    const MAXIMO = 99;

    function obterQuantidade() {
        return parseInt(campo.value, 10) || MINIMO;
    }

    function definirQuantidade(valor) {
        const limitada = Math.min(MAXIMO, Math.max(MINIMO, valor));
        campo.value = String(limitada);
    }

    botaoMenos.addEventListener("click", function () {
        definirQuantidade(obterQuantidade() - 1);
    });

    botaoMais.addEventListener("click", function () {
        definirQuantidade(obterQuantidade() + 1);
    });
});
