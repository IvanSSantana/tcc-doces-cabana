// Atualização sem recarga do catálogo (spec 014, RF-01 a RF-06).
//
// Intercepta a troca de filtro/ordenação (submit do formulário) e a
// paginação (clique nos links de página), busca só o bloco do resultado via
// fetch e troca o conteúdo — sem tocar cabeçalho, barra lateral ou rodapé.
// Trocar de categoria continua navegação comum de propósito: a barra lateral
// inteira muda de conteúdo nesse caso, e reconstruí-la também arrancaria o
// foco do controle recém-usado sem nada em troca (spec §10).
//
// Sem JavaScript, nada aqui roda: o formulário usa requestSubmit() nos
// onchange (dispara o evento "submit" de verdade, ao contrário de
// submit()), e sem um listener para interceptar esse evento ele segue seu
// caminho normal — submissão comum, página inteira, exatamente como sempre
// funcionou (RF-05).
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
        // Substitui o elemento inteiro, não só o innerHTML — o próprio
        // #resultado-catalogo vem no HTML devolvido pelo servidor, então
        // isso troca também o wrapper, mantendo os dois lados idênticos.
        var provisorio = document.createElement("div");
        provisorio.innerHTML = html;
        var novoResultado = provisorio.querySelector("#resultado-catalogo");
        if (!novoResultado) throw new Error("Resposta sem #resultado-catalogo.");

        resultado.replaceWith(novoResultado);
        resultado = novoResultado;

        if (!opcoes.semAlterarHistorico) {
          window.history.pushState({ catalogoParcial: true }, "", url);
        }

        // RF-18: o foco vai para o resultado, nunca fica solto no início do
        // documento — é o que faz paginar (e filtrar) por teclado continuar
        // usável depois da troca.
        resultado.focus({ preventScroll: true });

        if (opcoes.rolarParaOTopo) {
          resultado.scrollIntoView({ block: "start" });
        } else {
          window.scrollTo(0, rolagemAntes);
        }
      })
      .catch(function () {
        // RF-06/CA-08: a atualização parcial falhou — em vez de deixar a
        // tela presa ou incompleta, entrega o resultado pedido do jeito que
        // sempre funcionou, a navegação inteira.
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

    // Monta o endereço a partir do próprio formulário — nunca à mão. Assim
    // só existe uma regra de serialização de filtro, e o endereço que o
    // histórico guarda é garantidamente o mesmo que o formulário produziria
    // (spec 014, plano §9, risco 3).
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

  // O botão voltar do navegador desfaz a última troca (RF-02/CA-03) — via
  // recarga completa, não busca parcial. As caixas de subcategoria e "sem
  // açúcar" vivem na barra lateral, fora de #resultado-catalogo; refazer só
  // o resultado deixaria essas caixas com o estado marcado de antes do
  // "voltar", porque nada as sincroniza de volta com a URL. A recarga
  // resolve isso de graça, sem duplicar em JavaScript a lógica de estado
  // que o Razor já sabe fazer.
  window.addEventListener("popstate", function () {
    window.location.reload();
  });
})();
