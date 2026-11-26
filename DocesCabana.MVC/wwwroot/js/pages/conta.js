document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    function formatarCelular(valor) {
        const digitos = valor.replace(/\D/g, "").slice(0, 11);
        let formatado = "";
        if (digitos.length > 0) formatado += "(" + digitos.slice(0, 2);
        if (digitos.length > 2) formatado += ") " + digitos.slice(2, 7);
        if (digitos.length > 7) formatado += "-" + digitos.slice(7, 11);
        return formatado;
    }

    function formatarData(valor) {
        const digitos = valor.replace(/\D/g, "").slice(0, 8);
        let formatado = "";
        if (digitos.length > 0) formatado += digitos.slice(0, 2);
        if (digitos.length > 2) formatado += "/" + digitos.slice(2, 4);
        if (digitos.length > 4) formatado += "/" + digitos.slice(4, 8);
        return formatado;
    }

    function formatarCep(valor) {
        const digitos = valor.replace(/\D/g, "").slice(0, 8);
        let formatado = digitos.slice(0, 5);
        if (digitos.length > 5) formatado += "-" + digitos.slice(5, 8);
        return formatado;
    }

    const campoCelular = document.querySelector("input[name='Celular']");
    if (campoCelular) {
        campoCelular.value = formatarCelular(campoCelular.value);
        campoCelular.addEventListener("input", (e) => { e.target.value = formatarCelular(e.target.value); });
    }

    function formatarCpf(valor) {
        const digitos = valor.replace(/\D/g, "").slice(0, 11);
        let formatado = digitos.slice(0, 3);
        if (digitos.length > 3) formatado += "." + digitos.slice(3, 6);
        if (digitos.length > 6) formatado += "." + digitos.slice(6, 9);
        if (digitos.length > 9) formatado += "-" + digitos.slice(9, 11);
        return formatado;
    }

    const campoCpf = document.querySelector("input[name='CPF']");
    if (campoCpf) {
        campoCpf.value = formatarCpf(campoCpf.value);
        campoCpf.addEventListener("input", (e) => { e.target.value = formatarCpf(e.target.value); });
    }

    const campoDataNascimento = document.querySelector("input[name='DataNascimento']");
    if (campoDataNascimento) {
        campoDataNascimento.addEventListener("input", (e) => { e.target.value = formatarData(e.target.value); });
    }

    const campoCep = document.querySelector("[data-campo-cep]");
    if (!campoCep) return;

    campoCep.addEventListener("input", (e) => { e.target.value = formatarCep(e.target.value); });
    
    async function buscarPorCep() {
        const digitos = campoCep.value.replace(/\D/g, "");
        if (digitos.length !== 8) return;

        try {
            const resposta = await fetch(`https://viacep.com.br/ws/${digitos}/json/`);
            if (!resposta.ok) return;

            const dados = await resposta.json();
            if (dados.erro) return;

            const mapa = { Estado: dados.uf, Cidade: dados.localidade, Bairro: dados.bairro, Rua: dados.logradouro };
            document.querySelectorAll("[data-preenchido-por-cep]").forEach((campo) => {
                const valor = mapa[campo.name];
                if (valor) campo.value = valor;
            });
        } catch {
        }
    }

    campoCep.addEventListener("blur", buscarPorCep);
});
