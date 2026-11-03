(function () {
  "use strict";

  var formulario = document.getElementById("formulario-catalogo");
  var resultado = document.getElementById("resultado-catalogo");

  if (!formulario || !resultado) return;

  var TEMPO_ANTES_DE_MOSTRAR_CARREGANDO_MS = 200;

  function atualizarResultado(url, opcoes) {
    opcoes = opcoes || {};

    var indicadorDeCarregamento = window.setTimeout(function () {
      resultado.classList.add("resultado-catalogo--carregando");
      resultado.setAttribute("aria-busy", "true");
    }, TEMPO_ANTES_DE_MOSTRAR_CARREGANDO_MS);

    var rolagemAntes = window.scrollY;

    return fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } })
      .then(function (resposta) {
        if (!resposta.ok) throw new Error("Resposta não OK: " + resposta.status);
        return resposta.text();
      })
      .then(function (html) {
        var provisorio = document.createElement("div");
        provisorio.innerHTML = html;
        var novoResultado = provisorio.querySelector("#resultado-catalogo");
        if (!novoResultado) throw new Error("Resposta sem #resultado-catalogo.");

        resultado.replaceWith(novoResultado);
        resultado = novoResultado;

        if (!opcoes.semAlterarHistorico) {
          window.history.pushState({ catalogoParcial: true }, "", url);
        }

        resultado.focus({ preventScroll: true });

        if (opcoes.rolarParaOTopo) {
          resultado.scrollIntoView({ block: "start" });
        } else {
          window.scrollTo(0, rolagemAntes);
        }
      })
      .catch(function () {
        window.location.href = url;
      })
      .finally(function () {
        window.clearTimeout(indicadorDeCarregamento);
        resultado.classList.remove("resultado-catalogo--carregando");
        resultado.removeAttribute("aria-busy");
      });
  }

  formulario.addEventListener("submit", function (evento) {
    evento.preventDefault();

    var parametros = new URLSearchParams(new FormData(formulario));
    var url = formulario.action.split("?")[0] + "?" + parametros.toString();

    atualizarResultado(url);
  });

  document.addEventListener("click", function (evento) {
    var link = evento.target.closest(".link-paginacao");
    if (!link || !resultado.contains(link)) return;

    evento.preventDefault();
    atualizarResultado(link.href, { rolarParaOTopo: true });
  });

  window.addEventListener("popstate", function () {
    window.location.reload();
  });
})();
