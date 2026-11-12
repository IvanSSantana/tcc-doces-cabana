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

    var blocoAtual = document.querySelector("#itens-carrinho");
    if (blocoAtual) blocoAtual.replaceWith(blocoNovo);
  }

  var temporizadorDoAviso = null;
  var DURACAO_DO_AVISO_MS = 3500;

  function obterAviso() {
    var aviso = document.querySelector("#aviso-carrinho");
    if (aviso) return aviso;

    aviso = document.createElement("div");
    aviso.id = "aviso-carrinho";
    aviso.className = "aviso-carrinho";
    aviso.setAttribute("role", "status");
    aviso.setAttribute("aria-live", "polite");
    aviso.innerHTML =
      '<i class="fa-solid fa-circle-check aviso-carrinho__icone" aria-hidden="true"></i>' +
      '<div class="aviso-carrinho__texto">' +
      '<strong class="aviso-carrinho__titulo">Adicionado ao carrinho</strong>' +
      '<span class="aviso-carrinho__produto" data-aviso-produto></span>' +
      "</div>" +
      '<a class="aviso-carrinho__link" href="/Carrinho">Ver carrinho</a>' +
      '<button type="button" class="aviso-carrinho__fechar" data-fechar-aviso aria-label="Fechar aviso">&times;</button>';
    document.body.appendChild(aviso);
    return aviso;
  }

  function esconderAviso() {
    var aviso = document.querySelector("#aviso-carrinho");
    if (aviso) aviso.classList.remove("aviso-carrinho--visivel");
  }

  function mostrarAviso(nomeDoProduto, quantidade) {
    var aviso = obterAviso();
    var descricao = aviso.querySelector("[data-aviso-produto]");
    var prefixo = quantidade > 1 ? quantidade + "x " : "";
    descricao.textContent = nomeDoProduto ? prefixo + nomeDoProduto : "";
    descricao.hidden = !nomeDoProduto;

    // força reflow para reiniciar a animação quando o aviso já estava visível
    aviso.classList.remove("aviso-carrinho--visivel");
    void aviso.offsetWidth;
    aviso.classList.add("aviso-carrinho--visivel");

    clearTimeout(temporizadorDoAviso);
    temporizadorDoAviso = setTimeout(esconderAviso, DURACAO_DO_AVISO_MS);
  }

  function obterNomeDoProduto(botao) {
    var cartao = botao.closest(".card-produto");
    var elementoNome = cartao
      ? cartao.querySelector(".nome-card")
      : document.querySelector(".nome-produto");
    return elementoNome ? elementoNome.textContent.trim() : "";
  }

  function enviar(formulario, dados, aoConcluir) {
    fetch(formulario.action, {
      method: "POST",
      headers: { "X-Requested-With": "XMLHttpRequest" },
      body: dados,
    })
      .then(tratarResposta)
      .then(function (html) {
        aplicarBloco(html);
        if (aoConcluir) aoConcluir();
      })
      .catch(function () {
      });
  }

  function enviarConsulta(formulario, dados) {
    var url = formulario.action + "?" + new URLSearchParams(dados).toString();
    fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then(tratarResposta)
      .then(aplicarBloco)
      .catch(function () {
      });
  }

  document.addEventListener("submit", function (evento) {
    var formulario = evento.target;
    var botao = evento.submitter;

    if (formulario.id === "formulario-carrinho") {
      if (!botao) return;
      evento.preventDefault();

      var dados = new FormData(formulario, botao);

      var cartao = botao.closest(".card-produto");
      if (cartao) {
        var valorNoCartao = cartao.querySelector("[data-quantidade-valor]");
        if (valorNoCartao) dados.set("quantidade", valorNoCartao.textContent.trim());
      }

      var nomeDoProduto = obterNomeDoProduto(botao);
      var quantidade = parseInt(dados.get("quantidade"), 10) || 1;
      enviar(formulario, dados, function () {
        mostrarAviso(nomeDoProduto, quantidade);
      });
      return;
    }

    if (
      formulario.classList.contains("formulario-quantidade-carrinho") ||
      formulario.classList.contains("formulario-remover-carrinho") ||
      formulario.classList.contains("formulario-esvaziar-carrinho")
    ) {
      evento.preventDefault();
      enviar(formulario, new FormData(formulario, botao));

      var dialogoDoFormulario = formulario.closest("dialog");
      if (dialogoDoFormulario) dialogoDoFormulario.close();
      return;
    }

    if (formulario.classList.contains("formulario-frete-carrinho")) {
      evento.preventDefault();
      enviarConsulta(formulario, new FormData(formulario));
      return;
    }

    if (formulario.classList.contains("formulario-endereco") && formulario.closest("#itens-carrinho")) {
      evento.preventDefault();
      enviar(formulario, new FormData(formulario));
    }
  });

  document.addEventListener("click", function (evento) {
    var link = evento.target.closest("[data-navegacao-fechamento]");
    if (!link) return;

    evento.preventDefault();
    fetch(link.href, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then(tratarResposta)
      .then(aplicarBloco)
      .catch(function () {
      });
  });

  document.addEventListener("click", function (evento) {
    if (evento.target.closest("[data-fechar-aviso]")) {
      clearTimeout(temporizadorDoAviso);
      esconderAviso();
    }
  });

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
