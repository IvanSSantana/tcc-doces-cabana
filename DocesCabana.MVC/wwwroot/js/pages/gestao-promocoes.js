(() => {
    const formulario = document.querySelector('[data-formulario-promocao]');
    if (!formulario) return;

    const normalizar = (texto) =>
        texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();

    const valorMarcado = (seletor) =>
        formulario.querySelector(`${seletor}:checked`)?.value ?? '';

    // Mostra só os campos do tipo e da abrangência escolhidos. Sem JS, tudo fica visível.
    const atualizarCampos = () => {
        const tipo = valorMarcado('[data-escolha-tipo]');
        const abrangencia = valorMarcado('[data-escolha-abrangencia]');

        formulario.querySelectorAll('[data-campo-do-tipo]').forEach((campo) => {
            campo.hidden = !campo.dataset.campoDoTipo.split(' ').includes(tipo);
        });

        formulario.querySelectorAll('[data-campo-da-abrangencia]').forEach((campo) => {
            campo.hidden = campo.dataset.campoDaAbrangencia !== abrangencia;
        });
    };

    formulario.querySelectorAll('[data-escolha-tipo], [data-escolha-abrangencia]').forEach((opcao) => {
        opcao.addEventListener('change', atualizarCampos);
    });

    atualizarCampos();

    // Busca de produtos por nome, feita no navegador.
    const caixaDeBusca = formulario.querySelector('[data-busca-de-produtos]');
    const campoDeBusca = caixaDeBusca?.querySelector('input');
    if (!caixaDeBusca || !campoDeBusca) return;

    caixaDeBusca.hidden = false;

    campoDeBusca.addEventListener('input', () => {
        const termo = normalizar(campoDeBusca.value);

        formulario.querySelectorAll('[data-grupo-de-produtos]').forEach((grupo) => {
            let algumVisivel = false;

            grupo.querySelectorAll('[data-nome-do-produto]').forEach((opcao) => {
                const visivel = termo === '' || normalizar(opcao.dataset.nomeDoProduto).includes(termo);
                opcao.hidden = !visivel;
                algumVisivel ||= visivel;
            });

            grupo.hidden = !algumVisivel;
        });
    });
})();
