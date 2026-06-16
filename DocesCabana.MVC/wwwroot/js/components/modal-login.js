function abrirModal() {
    const modal = document.getElementById('modal-login');
    modal.showModal();

    document.querySelector('.botao-fechar').blur();
}

function fecharModal() {
    const modal = document.getElementById('modal-login');
    modal.close();
}