(function () {
  "use strict";

  var ID_DO_FORMULARIO = "formulario-favorito";

  var CHAVE_DA_INTENCAO = "doces-cabana:favoritar-pendente";

  function alternarIcone(botao, favoritado) {
    var iconeAtual = botao.querySelector("svg, i");
    var novoIcone = document.createElement("i");
    novoIcone.className = favoritado ? "fa-solid fa-heart favoritado" : "fa-regular fa-heart";

    if (iconeAtual) {
      iconeAtual.replaceWith(novoIcone);
    } else {
      botao.appendChild(novoIcone);
    }

    botao.setAttribute("aria-label", favoritado ? "Desfavoritar" : "Favoritar");
  }

  document.addEventListener("submit", function (evento) {
    if (evento.target.id !== ID_DO_FORMULARIO) return;

    var formulario = evento.target;
    var botao = evento.submitter;
    if (!botao) return;

    evento.preventDefault();

    var dados = new FormData(formulario, botao);

    fetch(formulario.action, {
      method: "POST",
      headers: { "X-Requested-With": "XMLHttpRequest" },
      body: dados,
    })
      .then(function (resposta) {
        if (resposta.status === 401) {
          favoritarComoVisitante(botao);
          return null;
        }

        if (!resposta.ok) throw new Error("Resposta não OK: " + resposta.status);
        return resposta.json();
      })
      .then(function (corpo) {
        if (!corpo) return;

        alternarIcone(botao, corpo.favoritado);

        var paginaDeFavoritos = document.querySelector(".pagina-favoritos");
        if (paginaDeFavoritos && !corpo.favoritado) {
          var cartao = botao.closest(".card-produto");
          if (cartao) cartao.remove();

          var grade = paginaDeFavoritos.querySelector(".grade-produtos");
          if (grade && grade.children.length === 0) {
            grade.hidden = true;
            var mensagemVazia = paginaDeFavoritos.querySelector(".favoritos-vazio");
            if (mensagemVazia) mensagemVazia.hidden = false;
          }
        }
      })
      .catch(function () {
      });
  });

  function favoritarComoVisitante(botao) {
    try {
      window.sessionStorage.setItem(CHAVE_DA_INTENCAO, botao.value);
    } catch (erro) {
    }

    var linkEntrar = document.querySelector("#modal-login .botao-entrar");
    if (linkEntrar) {
      var url = new URL(linkEntrar.href, window.location.origin);
      url.searchParams.set("returnUrl", window.location.pathname + window.location.search);
      linkEntrar.href = url.toString();
    }

    if (window.abrirModal) window.abrirModal();
  }

  function concluirFavoritoPendente() {
    var produtoId;
    try {
      produtoId = window.sessionStorage.getItem(CHAVE_DA_INTENCAO);
    } catch (erro) {
      return;
    }
    if (!produtoId) return;

    var formulario = document.getElementById(ID_DO_FORMULARIO);
    if (!formulario) return;

    var dados = new FormData(formulario);
    dados.set("produtoId", produtoId);

    fetch(formulario.action, {
      method: "POST",
      headers: { "X-Requested-With": "XMLHttpRequest" },
      body: dados,
    })
      .then(function (resposta) {
        if (resposta.status === 401) return null;

        try { window.sessionStorage.removeItem(CHAVE_DA_INTENCAO); } catch (erro) { }

        return resposta.ok ? resposta.json() : null;
      })
      .then(function (corpo) {
        if (!corpo) return;

        var botaoNaTela = document.querySelector('.botao-favorito-card[value="' + produtoId + '"]');
        if (botaoNaTela) alternarIcone(botaoNaTela, corpo.favoritado);
      })
      .catch(function () {});
  }

  document.addEventListener("DOMContentLoaded", concluirFavoritoPendente);
})();
