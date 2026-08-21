// Favoritar sem recarga (spec 015, RF-01 a RF-06).
//
// O coração não fica dentro de um <form> próprio: o cartão pode estar
// dentro do <form method="get"> do catálogo, e HTML não aceita form
// aninhado (o navegador ignora e submete o de fora). Cada botão se associa
// a #formulario-favorito — que vive fora de qualquer form da página — pelo
// atributo form= do HTML5. Sem JavaScript, isso já basta: o botão posta
// normalmente e o servidor redireciona de volta (POST-Redirect-Get). Com
// JavaScript, este script intercepta o envio, posta por fetch e troca só o
// ícone do botão que foi clicado, sem recarregar nada.
(function () {
  "use strict";

  var ID_DO_FORMULARIO = "formulario-favorito";

  // Guardado no próprio navegador, não no servidor (spec 015, plano §8): não
  // acopla a tela de login ao favorito, e evita um GET que altera estado.
  // Consequência assumida: sem JavaScript não há intenção nenhuma — a
  // pessoa favorita de novo depois de entrar, o que é coerente, já que o
  // convite em si é recurso de script.
  var CHAVE_DA_INTENCAO = "doces-cabana:favoritar-pendente";

  function alternarIcone(botao, favoritado) {
    // O kit da FontAwesome (_Footer.cshtml) converte <i> em <svg> assim que
    // a página carrega e apaga a tag original — trocar classe num <i> que
    // já não existe não faz nada. A troca certa é substituir o ícone atual
    // (svg ou i, o que houver) por um <i> novo com as classes certas; o
    // observador de mutações do próprio kit o converte de novo sozinho.
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
    // evento.submitter é o botão que de fato disparou o envio — é dele que
    // vem o produtoId, já que o hidden de returnUrl é o único campo que
    // mora dentro do próprio #formulario-favorito.
    var botao = evento.submitter;
    if (!botao) return;

    evento.preventDefault();

    // FormData(form, submitter) — segundo argumento inclui o name/value do
    // botão que submeteu, exatamente como o navegador faria sozinho.
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

        // Na lista de favoritos (spec 015, RF-10), desfavoritar tira o
        // cartão na hora — a lista existe justamente para mostrar "o que
        // está favoritado", então um cartão desfavoritado não pertence
        // mais a ela, diferente do catálogo, onde o produto continua ali.
        var paginaDeFavoritos = document.querySelector(".pagina-favoritos");
        if (paginaDeFavoritos && !corpo.favoritado) {
          var cartao = botao.closest(".card-produto");
          if (cartao) cartao.remove();

          // Achado de verificação ao vivo: a mensagem de "nenhum favorito"
          // só existia quando o servidor já renderizava a lista vazia — ao
          // esvaziar por aqui, ela nunca aparecia. Os dois blocos vivem na
          // marcação o tempo todo (RF-11); só alterna o hidden.
          var grade = paginaDeFavoritos.querySelector(".grade-produtos");
          if (grade && grade.children.length === 0) {
            grade.hidden = true;
            var mensagemVazia = paginaDeFavoritos.querySelector(".favoritos-vazio");
            if (mensagemVazia) mensagemVazia.hidden = false;
          }
        }
      })
      .catch(function () {
        // Falha de rede em favoritar não é crítica o bastante para
        // justificar navegação cheia (diferente do catálogo, spec 014,
        // RF-06) — o botão simplesmente não muda.
      });
  });

  // RF-06/RF-07: visitante que tenta favoritar é convidado a entrar, e o
  // produto pretendido fica favoritado assim que ele termina de entrar —
  // sem precisar clicar de novo.
  function favoritarComoVisitante(botao) {
    try {
      window.sessionStorage.setItem(CHAVE_DA_INTENCAO, botao.value);
    } catch (erro) {
      // sessionStorage indisponível (aba anônima com bloqueio, por
      // exemplo): sem onde guardar, a intenção simplesmente não sobrevive
      // ao login — a pessoa favorita de novo, o que não é um erro.
    }

    var linkEntrar = document.querySelector("#modal-login .botao-entrar");
    if (linkEntrar) {
      var url = new URL(linkEntrar.href, window.location.origin);
      url.searchParams.set("returnUrl", window.location.pathname + window.location.search);
      linkEntrar.href = url.toString();
    }

    if (window.abrirModal) window.abrirModal();
  }

  // Roda em toda página (favorito.js está no layout) — se houver intenção
  // pendente, tenta concluí-la. Antes do login, o servidor devolve 401 e a
  // intenção fica guardada para a próxima carga; depois do login, o
  // redirecionamento de retorno (RF-13) traz de volta para cá e ela se
  // resolve sozinha.
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
        if (resposta.status === 401) return null; // ainda não entrou — tenta de novo na próxima carga

        // Sucesso ou erro definitivo (produto que sumiu, por exemplo): a
        // intenção não serve mais para nada, então some daqui pra frente —
        // sem isso, um ID inválido tentaria de novo para sempre.
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
