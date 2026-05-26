(function () {
    const campoCpf = document.getElementById('campo-cpf');
    const campoTelefone = document.getElementById('campo-telefone');
    const formulario = document.getElementById('form-cadastro');

    if (campoCpf) {
        campoCpf.addEventListener('input', function () {
            campoCpf.value = formatarCpf(campoCpf.value);
        });
    }

    if (campoTelefone) {
        campoTelefone.addEventListener('input', function () {
            campoTelefone.value = formatarTelefone(campoTelefone.value);
        });
    }

    if (formulario) {
        formulario.addEventListener('submit', function () {
            if (campoCpf) {
                campoCpf.value = apenasDigitos(campoCpf.value);
            }
            if (campoTelefone) {
                campoTelefone.value = apenasDigitos(campoTelefone.value);
            }
        });
    }

    function apenasDigitos(valor) {
        return valor.replace(/\D/g, '');
    }

    function formatarCpf(valor) {
        const digitos = apenasDigitos(valor).slice(0, 11);

        if (digitos.length <= 3) return digitos;
        if (digitos.length <= 6) return `${digitos.slice(0, 3)}.${digitos.slice(3)}`;
        if (digitos.length <= 9) return `${digitos.slice(0, 3)}.${digitos.slice(3, 6)}.${digitos.slice(6)}`;
        return `${digitos.slice(0, 3)}.${digitos.slice(3, 6)}.${digitos.slice(6, 9)}-${digitos.slice(9)}`;
    }

    function formatarTelefone(valor) {
        const digitos = apenasDigitos(valor).slice(0, 11);

        if (digitos.length <= 2) return digitos.length ? `(${digitos}` : '';
        if (digitos.length <= 7) return `(${digitos.slice(0, 2)}) ${digitos.slice(2)}`;
        return `(${digitos.slice(0, 2)}) ${digitos.slice(2, 7)}-${digitos.slice(7)}`;
    }
})();
