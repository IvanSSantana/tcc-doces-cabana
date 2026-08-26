// Carrinho sem recarga (spec 017, Fase 8, RF-06/RF-09/RF-19/CA-21). Três
// caminhos batem na mesma resposta (_ItensDoCarrinho): acrescentar
// (cartão/página do produto, pelo formulário compartilhado do layout),
// alterar quantidade e remover (só existem na própria tela do carrinho, um
// <form> por linha). Uma função só trata a resposta dos três — o bloco
// devolvido é sempre o mesmo; a diferença mora em montar o pedido.
(function () {
  "use strict";

  var MINIMO = 1;
  var MAXIMO = 99;

  function atualizarBolha(totalDeItens) {
    document.querySelectorAll("[data-bolha-carrinho]").forEach(function (bolha) {
      bolha.textContent = String(totalDeItens);
      bolha.hidden = totalDeItens === 0;
    });
  }

  function tratarResposta(resposta) {
    if (!resposta.ok) throw new Error("Resposta não OK: " + resposta.status);
    return resposta.text();
  }

  function aplicarBloco(html) {
    var documentoAnalisado = new DOMParser().parseFromString(html, "text/html");
    var blocoNovo = documentoAnalisado.querySelector("#itens-carrinho");
    if (!blocoNovo) return;

    atualizarBolha(parseInt(blocoNovo.getAttribute("data-total-itens"), 10) || 0);

    // A troca do bloco inteiro só faz sentido na própria tela do carrinho —
    // nas demais páginas (cartão, produto) não existe #itens-carrinho para
    // substituir, e não deveria existir: as três ações devolvem sempre o
    // carrinho inteiro, mas só a tela do carrinho tem onde colocá-lo.
    var blocoAtual = document.querySelector("#itens-carrinho");
    if (blocoAtual) blocoAtual.replaceWith(blocoNovo);
  }

  function enviar(formulario, dados) {
    fetch(formulario.action, {
      method: "POST",
      headers: { "X-Requested-With": "XMLHttpRequest" },
      body: dados,
    })
      .then(tratarResposta)
      .then(aplicarBloco)
      .catch(function () {
        // Falha de rede não é crítica o bastante para navegação cheia
        // (mesmo critério do favorito, spec 015) — o carrinho simplesmente
        // não muda na tela; a pessoa tenta de novo.
      });
  }

  // Cotação de frete (spec 020) é GET, não POST — cotar não muda estado
  // nenhum (plano §4, "por que GET"). A URL carrega o cep na query, para o
  // caminho sem script continuar funcionando por navegação comum.
  function enviarConsulta(formulario, dados) {
    var url = formulario.action + "?" + new URLSearchParams(dados).toString();
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then(tratarResposta)
      .then(aplicarBloco)
      .catch(function () {
        // Mesmo critério de enviar(): falha de rede não trava a tela.
      });
  }

  document.addEventListener("submit", function (evento) {
    var formulario = evento.target;
    var botao = evento.submitter;

    if (formulario.id === "formulario-carrinho") {
      if (!botao) return;
      evento.preventDefault();

      // FormData(form, submitter) — segundo argumento inclui o name/value
      // do botão que submeteu, exatamente como o navegador faria sozinho
      // (mesmo recurso do favorito.js).
      var dados = new FormData(formulario, botao);

      // Só o cartão precisa de reforço manual: a quantidade dele vive num
      // <span>, não num campo do formulário (plano §3) — um hidden
      // compartilhado por form= levaria a quantidade de TODOS os cartões
      // da grade, não só a do que foi clicado. A página do produto já manda
      // a dela sozinha, porque o <input> é form=-associado a este mesmo
      // formulário.
      var cartao = botao.closest(".card-produto");
      if (cartao) {
        var valorNoCartao = cartao.querySelector("[data-quantidade-valor]");
        if (valorNoCartao) dados.set("quantidade", valorNoCartao.textContent.trim());
      }

      enviar(formulario, dados);
      return;
    }

    if (
      formulario.classList.contains("formulario-quantidade-carrinho") ||
      formulario.classList.contains("formulario-remover-carrinho") ||
      formulario.classList.contains("formulario-esvaziar-carrinho")
    ) {
      evento.preventDefault();
      enviar(formulario, new FormData(formulario, botao));

      // O diálogo (spec 021) não tem por que continuar aberto enquanto a
      // resposta chega — falha de rede não é crítica o bastante para travar
      // a tela (mesmo critério do enviar() acima); o carrinho simplesmente
      // não muda, e a pessoa tenta de novo pelo link.
      var dialogoDoFormulario = formulario.closest("dialog");
      if (dialogoDoFormulario) dialogoDoFormulario.close();
      return;
    }

    if (formulario.classList.contains("formulario-frete-carrinho")) {
      evento.preventDefault();
      enviarConsulta(formulario, new FormData(formulario));
    }
  });

  // Diálogo de confirmação para esvaziar (spec 021, RF-11). Sem JavaScript,
  // o link de "Esvaziar carrinho" navega para /Carrinho/ConfirmarEsvaziar —
  // uma página própria com a mesma pergunta (RN-04). Com script, o mesmo
  // link abre este diálogo inline, e a navegação nunca acontece.
  document.addEventListener("click", function (evento) {
    var abrirConfirmacao = evento.target.closest("[data-abrir-confirmacao-esvaziar]");
    if (abrirConfirmacao) {
      var dialogo = document.querySelector("#dialogo-esvaziar-carrinho");
      if (dialogo && typeof dialogo.showModal === "function") {
        evento.preventDefault();
        dialogo.showModal();
      }
      return;
    }

    var fecharDialogo = evento.target.closest("[data-fechar-dialogo-esvaziar]");
    if (fecharDialogo) {
      var dialogoAberto = evento.target.closest("dialog");
      if (dialogoAberto) dialogoAberto.close();
    }
  });

  // Os ± do cartão ficam fora do <form> (type="button") — só ajustam o
  // número que o clique em "Adicionar" lê depois. Sem script, eles não
  // fazem nada, e o botão acrescenta uma unidade: o comportamento honesto
  // de um controle de grade (plano §3), já garantido pelo default do
  // controlador quando "quantidade" não vem na requisição.
  function ajustarQuantidadeDoCartao(botao, delta) {
    var cartao = botao.closest(".card-produto");
    if (!cartao) return;

    var valorNoCartao = cartao.querySelector("[data-quantidade-valor]");
    if (!valorNoCartao) return;

    var atual = parseInt(valorNoCartao.textContent, 10) || MINIMO;
    valorNoCartao.textContent = String(Math.min(MAXIMO, Math.max(MINIMO, atual + delta)));
  }

  document.addEventListener("click", function (evento) {
    var botaoMais = evento.target.closest("[data-quantidade-mais]");
    if (botaoMais && botaoMais.closest(".card-produto")) {
      ajustarQuantidadeDoCartao(botaoMais, 1);
      return;
    }

    var botaoMenos = evento.target.closest("[data-quantidade-menos]");
    if (botaoMenos && botaoMenos.closest(".card-produto")) {
      ajustarQuantidadeDoCartao(botaoMenos, -1);
    }
  });
})();
