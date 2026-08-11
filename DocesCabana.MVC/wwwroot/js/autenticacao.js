// --- Lógica de Mostrar/Ocultar Senha (Login e Cadastro) ---
function toggleSenha(idInput, idOlho) {
    const input = document.getElementById(idInput);
    const olho = document.getElementById(idOlho);

    if (input) {
        if (input.type === "password") {
            input.type = "text";
            if (olho) olho.classList.remove("fa-eye-slash");
            if (olho) olho.classList.add("fa-eye");
        } else {
            input.type = "password";
            if (olho) olho.classList.add("fa-eye-slash");
            if (olho) olho.classList.remove("fa-eye");
        }
    }
}

document.addEventListener("DOMContentLoaded", function () {


    // --- Lógica de Máscaras dos Campos (Cadastro) ---
    const inputCelular = document.getElementById("Celular");
    const inputData = document.getElementById("DataNascimento");
    const inputCpf = document.getElementById("CPF");

    function formatarTelefone(value) {
        const digitos = value.replace(/\D/g, "").slice(0, 11);
        let formatado = "";
        if (digitos.length > 0) {
            formatado += "(" + digitos.slice(0, 2);
        }
        if (digitos.length > 2) {
            formatado += ") " + digitos.slice(2, 7);
        }
        if (digitos.length > 7) {
            formatado += "-" + digitos.slice(7, 11);
        }
        return formatado;
    }

    function formatarDataNascimento(value) {
        const digitos = value.replace(/\D/g, "").slice(0, 8);
        let formatado = "";
        if (digitos.length > 0) {
            formatado += digitos.slice(0, 2);
        }
        if (digitos.length > 2) {
            formatado += "/" + digitos.slice(2, 4);
        }
        if (digitos.length > 4) {
            formatado += "/" + digitos.slice(4, 8);
        }
        return formatado;
    }

    function formatarCPF(value) {
        const digitos = value.replace(/\D/g, "").slice(0, 11);
        let formatado = "";
        if (digitos.length > 0) {
            formatado += digitos.slice(0, 3);
        }
        if (digitos.length > 3) {
            formatado += "." + digitos.slice(3, 6);
        }
        if (digitos.length > 6) {
            formatado += "." + digitos.slice(6, 9);
        }
        if (digitos.length > 9) {
            formatado += "-" + digitos.slice(9, 11);
        }
        return formatado;
    }

    if (inputCelular) {
        inputCelular.addEventListener("input", function (e) {
            e.target.value = formatarTelefone(e.target.value);
        });
    }

    if (inputData) {
        inputData.addEventListener("input", function (e) {
            e.target.value = formatarDataNascimento(e.target.value);
        });
    }

    if (inputCpf) {
        inputCpf.addEventListener("input", function (e) {
            e.target.value = formatarCPF(e.target.value);
        });
    }

    // --- Validação em tempo real da senha (Cadastro) ---
    const inputSenhaCadastro = document.getElementById("input-senha-cadastro");
    const containerRequisitos = document.getElementById("requisitos-senha");

    if (inputSenhaCadastro && containerRequisitos) {
        const reqComprimento = document.getElementById("req-comprimento");
        const reqMaiuscula = document.getElementById("req-maiuscula");
        const reqMinuscula = document.getElementById("req-minuscula");
        const reqNumero = document.getElementById("req-numero");
        const reqEspecial = document.getElementById("req-especial");

        inputSenhaCadastro.addEventListener("focus", function () {
            containerRequisitos.style.display = "block";
        });

        inputSenhaCadastro.addEventListener("input", function () {
            const senha = inputSenhaCadastro.value;

            // 1. Comprimento (mínimo 6 caracteres)
            const temComprimento = senha.length >= 6;
            atualizarRequisito(reqComprimento, temComprimento);

            // 2. Letra Maiúscula
            const temMaiuscula = /[A-Z]/.test(senha);
            atualizarRequisito(reqMaiuscula, temMaiuscula);

            // 3. Letra Minúscula
            const temMinuscula = /[a-z]/.test(senha);
            atualizarRequisito(reqMinuscula, temMinuscula);

            // 4. Número
            const temNumero = /[0-9]/.test(senha);
            atualizarRequisito(reqNumero, temNumero);

            // 5. Caractere Especial
            const temEspecial = /[^A-Za-z0-9]/.test(senha);
            atualizarRequisito(reqEspecial, temEspecial);

            // Se tiver todos os requisitos corretos, muda a cor do fundo e a borda
            if (temComprimento && temMaiuscula && temMinuscula && temNumero && temEspecial) {
                containerRequisitos.style.backgroundColor = "#E9F7EF";
                containerRequisitos.style.borderColor = "#D4EFDF";
            } else {
                containerRequisitos.style.backgroundColor = "#FDF2F2";
                containerRequisitos.style.borderColor = "#FDE8E8";
            }
        });

        function atualizarRequisito(elemento, valido) {
            if (valido) {
                elemento.classList.remove("mensagem-erro");
                elemento.classList.add("mensagem-sucesso");
            } else {
                elemento.classList.remove("mensagem-sucesso");
                elemento.classList.add("mensagem-erro");
            }
        }
    }
});
