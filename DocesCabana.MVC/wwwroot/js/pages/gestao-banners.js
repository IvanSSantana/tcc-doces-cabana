(() => {
    const formulario = document.querySelector('[data-formulario-banner]');
    if (!formulario) return;

    // Mostra só o seletor do destino escolhido. Sem JS, os três ficam visíveis e vale o do rádio marcado.
    const atualizarCampos = () => {
        const destino = formulario.querySelector('[data-escolha-destino]:checked')?.value ?? '';

        formulario.querySelectorAll('[data-campo-do-destino]').forEach((campo) => {
            campo.hidden = campo.dataset.campoDoDestino !== destino;
        });
    };

    formulario.querySelectorAll('[data-escolha-destino]').forEach((opcao) => {
        opcao.addEventListener('change', atualizarCampos);
    });

    atualizarCampos();
})();
