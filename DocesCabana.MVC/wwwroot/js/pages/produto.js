document.addEventListener("DOMContentLoaded", function () {
    iniciarSeletorDeQuantidade();
    iniciarModalDeAvaliacao();
});

function iniciarSeletorDeQuantidade() {
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
}

function iniciarModalDeAvaliacao() {
    const modal = document.getElementById("modal-avaliacao");
    const conteudo = document.getElementById("conteudo-modal-avaliacao");
    if (!modal || !conteudo) return;

    const CABECALHO_AJAX = { "X-Requested-With": "XMLHttpRequest" };
    const MENSAGEM_DE_FALHA = "Não foi possível enviar sua avaliação agora. Tente novamente.";

    function mostrarFalha() {
        let aviso = conteudo.querySelector("[data-falha-avaliacao]");
        if (!aviso) {
            aviso = document.createElement("p");
            aviso.className = "mensagem-erro exibicao-bloco";
            aviso.setAttribute("data-falha-avaliacao", "");
            aviso.setAttribute("role", "alert");
            const formulario = conteudo.querySelector("[data-formulario-avaliacao]");
            if (formulario) formulario.prepend(aviso);
        }
        aviso.textContent = MENSAGEM_DE_FALHA;
    }

    function atualizarContador() {
        const campo = conteudo.querySelector("[data-comentario-avaliacao]");
        const contador = conteudo.querySelector("[data-contador-comentario]");
        if (!campo || !contador) return;
        contador.textContent = campo.value.length + "/" + (campo.maxLength > 0 ? campo.maxLength : 255);
    }

    function enviar(formulario) {
        const botao = formulario.querySelector('button[type="submit"]');
        if (botao) botao.disabled = true;

        fetch(formulario.action, {
            method: "POST",
            body: new FormData(formulario),
            headers: CABECALHO_AJAX
        }).then(function (resposta) {
            if (resposta.redirected) {
                // O fetch descarta o fragmento da URL do redirect; recolocamos a âncora do bloco.
                window.location = resposta.url + "#avaliacoes";
                return null;
            }
            return resposta.text().then(function (html) {
                if (resposta.status === 400) {
                    conteudo.innerHTML = html;
                    atualizarContador();
                    return null;
                }
                throw new Error("Resposta inesperada: " + resposta.status);
            });
        }).catch(function () {
            if (botao) botao.disabled = false;
            mostrarFalha();
        });
    }

    document.addEventListener("click", function (evento) {
        if (evento.target.closest("[data-abrir-modal-avaliacao]")) {
            if (!modal.open) modal.showModal();
            return;
        }

        if (evento.target.closest("[data-fechar-modal-avaliacao]")) {
            modal.close();
            return;
        }

        // Clique no fundo escurecido (o alvo é o próprio <dialog>) fecha o modal.
        if (evento.target === modal) modal.close();
    });

    conteudo.addEventListener("submit", function (evento) {
        const formulario = evento.target.closest("[data-formulario-avaliacao]");
        if (!formulario) return;

        evento.preventDefault();
        if (!formulario.reportValidity()) return;

        enviar(formulario);
    });

    conteudo.addEventListener("input", function (evento) {
        if (evento.target.closest("[data-comentario-avaliacao]")) atualizarContador();
    });
}
