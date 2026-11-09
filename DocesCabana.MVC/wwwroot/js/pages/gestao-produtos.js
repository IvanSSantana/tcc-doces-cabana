(function () {
  "use strict";

  var resultado = document.getElementById("resultado-gestao");
  var modalProduto = document.getElementById("modal-produto");
  var conteudo = document.getElementById("conteudo-modal-produto");
  var modalInativar = document.getElementById("modal-inativar");

  if (!resultado || !modalProduto || !conteudo || !modalInativar) return;

  var CABECALHO_AJAX = { "X-Requested-With": "XMLHttpRequest" };
  var TEMPO_ANTES_DE_MOSTRAR_CARREGANDO_MS = 200;

  /* Listagem: filtro e paginação trocam só a tabela, sem recarregar a página */

  function atualizarResultado(url) {
    var indicadorDeCarregamento = window.setTimeout(function () {
      resultado.classList.add("resultado-gestao--carregando");
      resultado.setAttribute("aria-busy", "true");
    }, TEMPO_ANTES_DE_MOSTRAR_CARREGANDO_MS);

    return fetch(url, { headers: CABECALHO_AJAX })
      .then(function (resposta) {
        if (resposta.redirected) throw new Error("Redirecionado");
        if (!resposta.ok) throw new Error("Resposta não OK: " + resposta.status);
        return resposta.text();
      })
      .then(function (html) {
        var provisorio = document.createElement("div");
        provisorio.innerHTML = html;
        var novoResultado = provisorio.querySelector("#resultado-gestao");
        if (!novoResultado) throw new Error("Resposta sem #resultado-gestao.");

        resultado.replaceWith(novoResultado);
        resultado = novoResultado;

        window.history.pushState({ gestaoParcial: true }, "", url);
        resultado.focus({ preventScroll: true });
      })
      .catch(function () {
        window.location.href = url;
      })
      .finally(function () {
        window.clearTimeout(indicadorDeCarregamento);
        resultado.classList.remove("resultado-gestao--carregando");
        resultado.removeAttribute("aria-busy");
      });
  }

  /* Modal de produto: ver, editar e cadastrar */

  function carregarNoModal(url) {
    return fetch(url, { headers: CABECALHO_AJAX }).then(function (resposta) {
      if (resposta.redirected) {
        window.location = resposta.url;
        return null;
      }
      if (!resposta.ok) throw new Error("Resposta não OK: " + resposta.status);
      return resposta.text();
    }).then(function (html) {
      if (html === null) return;
      conteudo.innerHTML = html;
      if (!modalProduto.open) modalProduto.showModal();
    }).catch(function () {
      conteudo.innerHTML = '<p class="mensagem-erro exibicao-bloco">Não foi possível carregar agora. Tente novamente.</p>';
      if (!modalProduto.open) modalProduto.showModal();
    });
  }

  function enviarFormulario(formulario) {
    var botao = formulario.querySelector('button[type="submit"]');
    if (botao) botao.disabled = true;

    fetch(formulario.action, {
      method: "POST",
      body: new FormData(formulario),
      headers: CABECALHO_AJAX
    }).then(function (resposta) {
      if (resposta.redirected) {
        window.location = resposta.url;
        return null;
      }
      return resposta.text().then(function (html) {
        if (resposta.status === 400) {
          conteudo.innerHTML = html;
          return null;
        }
        throw new Error("Resposta inesperada: " + resposta.status);
      });
    }).catch(function () {
      if (botao) botao.disabled = false;
      var aviso = formulario.querySelector(".resumo-erros");
      if (!aviso) {
        aviso = document.createElement("div");
        aviso.className = "resumo-erros";
        formulario.insertBefore(aviso, formulario.querySelector(".rodape-modal-produto"));
      }
      aviso.innerHTML = '<span class="mensagem-erro exibicao-bloco">Não foi possível salvar agora. Tente novamente.</span>';
    });
  }

  document.addEventListener("click", function (evento) {
    var linkDaListagem = evento.target.closest(".link-paginacao-gestao, .aba-filtro");
    if (linkDaListagem && resultado.contains(linkDaListagem)) {
      if (linkDaListagem.tagName !== "A" || evento.ctrlKey || evento.metaKey || evento.shiftKey) return;
      evento.preventDefault();
      atualizarResultado(linkDaListagem.href);
      return;
    }

    var abrir = evento.target.closest("[data-abrir-modal]");
    if (abrir) {
      evento.preventDefault();
      carregarNoModal(abrir.getAttribute("data-abrir-modal"));
      return;
    }

    var inativar = evento.target.closest("[data-inativar-id]");
    if (inativar) {
      document.getElementById("inativar-id").value = inativar.getAttribute("data-inativar-id");
      document.getElementById("inativar-nome").textContent = inativar.getAttribute("data-inativar-nome");
      document.getElementById("inativar-filtro").value = resultado.getAttribute("data-filtro");
      document.getElementById("inativar-pagina").value = resultado.getAttribute("data-pagina");
      modalInativar.showModal();
      return;
    }

    var fechar = evento.target.closest("[data-fechar-modal]");
    if (fechar) {
      var dialogo = fechar.closest("dialog");
      if (dialogo) dialogo.close();
      return;
    }

    if (evento.target === modalProduto) modalProduto.close();
    if (evento.target === modalInativar) modalInativar.close();
  });

  conteudo.addEventListener("submit", function (evento) {
    var formulario = evento.target.closest("[data-formulario-produto]");
    if (!formulario) return;

    evento.preventDefault();
    enviarFormulario(formulario);
  });

  modalProduto.addEventListener("close", function () {
    conteudo.innerHTML = "";
  });

  window.addEventListener("popstate", function () {
    window.location.reload();
  });
})();
