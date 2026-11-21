(() => {
    const formulario = document.querySelector('[data-formulario-cupom]');
    if (!formulario) return;

    const valorMarcado = (seletor) =>
        formulario.querySelector(`${seletor}:checked`)?.value ?? '';

    // Mostra só os campos do tipo e da restrição escolhidos. Sem JS, tudo fica visível.
    // A lista de restrição escondida é desabilitada para não ir no POST: o cupom aceita uma só.
    const atualizarCampos = () => {
        const tipo = valorMarcado('[data-escolha-tipo]');
        const restricao = valorMarcado('[data-escolha-restricao]');

        formulario.querySelectorAll('[data-campo-do-tipo]').forEach((campo) => {
            campo.hidden = !campo.dataset.campoDoTipo.split(' ').includes(tipo);
        });

        formulario.querySelectorAll('[data-campo-da-restricao]').forEach((campo) => {
            const escondido = campo.dataset.campoDaRestricao !== restricao;
            campo.hidden = escondido;
            campo.querySelectorAll('select').forEach((lista) => {
                lista.disabled = escondido;
            });
        });
    };

    formulario.querySelectorAll('[data-escolha-tipo], [data-escolha-restricao]').forEach((opcao) => {
        opcao.addEventListener('change', atualizarCampos);
    });

    atualizarCampos();
})();
