// Área de conta (spec 018): máscaras de celular/data (dados pessoais) e CEP
// (endereço), mais a busca automática por CEP. Tudo aqui é conveniência —
// RN-07: falha, demora ou CEP inexistente nunca impedem o cadastro, os
// campos já nascem preenchíveis à mão (RF-19/CA-21).
document.addEventListener("DOMContentLoaded", function () {
    "use strict";

    function formatarTelefone(valor) {
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

    // ── Dados pessoais ───────────────────────────────────────────────────
    const campoCelular = document.querySelector("input[name='Celular']");
    if (campoCelular) {
        campoCelular.addEventListener("input", (e) => { e.target.value = formatarTelefone(e.target.value); });
    }

    const campoDataNascimento = document.querySelector("input[name='DataNascimento']");
    if (campoDataNascimento) {
        campoDataNascimento.addEventListener("input", (e) => { e.target.value = formatarData(e.target.value); });
    }

    // ── Endereço: máscara e busca por CEP ────────────────────────────────
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
            // ViaCEP devolve 200 com { erro: true } para CEP bem formado
            // mas inexistente — não é falha de rede, mas o resultado é o
            // mesmo: os campos continuam como estavam (RN-07).
            if (dados.erro) return;

            const mapa = { Estado: dados.uf, Cidade: dados.localidade, Bairro: dados.bairro, Rua: dados.logradouro };
            document.querySelectorAll("[data-preenchido-por-cep]").forEach((campo) => {
                // Só sobrescreve o que o CEP de fato trouxe — um campo que
                // a resposta não incluiu (raro, mas o formato não garante
                // todos) não é apagado.
                const valor = mapa[campo.name];
                if (valor) campo.value = valor;
            });
        } catch {
            // Falha de rede, timeout, serviço fora do ar: os campos
            // continuam vazios e preenchíveis à mão — não é erro do
            // usuário, não há mensagem alarmante (RN-07/CA-20).
        }
    }

    campoCep.addEventListener("blur", buscarPorCep);
});
