# Especificação — [NOME DA FEATURE]

**ID:** `[NNN-slug-da-feature]` · **Branch:** `[NNN-slug-da-feature]`
**Criada em:** [AAAA-MM-DD] · **Status:** Rascunho | Em revisão | Aprovada | Implementada

---

## ⚠️ Regras de preenchimento

Esta spec descreve **o quê** e **por quê**. Nunca **como**.

- Proibido nesta seção: nome de classe, tabela, coluna, framework, endpoint,
  biblioteca, estrutura de pasta. Isso é assunto da `plan.md`.
- Escreva para quem entende de doces, não de C#.
- Toda ambiguidade que você não pode resolver sozinho vira uma linha marcada
  `[NECESSITA ESCLARECIMENTO: pergunta objetiva]`. **Não adivinhe.** Uma spec com
  marcações pendentes não avança para a `plan`.

---

## 1. Contexto e problema

*Qual é a situação hoje e o que dói nela? 2 a 4 frases. Se a feature existe para
atender um requisito do TCC, diga qual.*

## 2. Objetivo

*Uma frase. Se não couber em uma frase, provavelmente são duas features.*

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | |
| Cliente autenticado | |
| Administrador da loja | |

## 4. Histórias de usuário

> **HU-01** — Como **[perfil]**, quero **[ação]** para que **[benefício]**.
>
> **HU-02** — ...

## 5. Requisitos funcionais

*Numerados, testáveis, um comportamento observável por linha. Se não dá para
escrever um teste que prove, o requisito está vago demais.*

- **RF-01** — O sistema DEVE ...
- **RF-02** — O sistema DEVE ...
- **RF-03** — O sistema NÃO DEVE ...

## 6. Regras de negócio

*Invariantes e restrições do domínio. É daqui que saem as validações da entidade.*

- **RN-01** — ...
- **RN-02** — ...

## 7. Critérios de aceite

*No formato Dado/Quando/Então — vira teste quase palavra por palavra
(Princípio V da constituição). Cubra o caminho feliz **e** os caminhos de erro.*

### CA-01 — [nome do cenário]
- **Dado** que ...
- **Quando** ...
- **Então** ...

### CA-02 — [cenário de erro]
- **Dado** que ...
- **Quando** ...
- **Então** ...

## 8. Fora de escopo

*O que alguém poderia razoavelmente supor que está incluído, mas não está.
Esta seção evita mais retrabalho do que qualquer outra.*

- ...

## 9. Dependências

- **Depende de:** [spec ou capacidade que precisa existir antes]
- **Bloqueia:** [o que espera esta feature]

## 10. Pendências

- [ ] `[NECESSITA ESCLARECIMENTO: ...]`

---

## Checklist de qualidade da spec

Marque tudo antes de pedir revisão. Item não marcado é motivo para devolver a spec.

- [ ] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [ ] Todo requisito funcional é verificável por um teste
- [ ] Todo requisito tem ao menos um critério de aceite correspondente
- [ ] Os caminhos de erro estão especificados, não só o caminho feliz
- [ ] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [ ] A seção "Fora de escopo" foi preenchida de verdade
- [ ] Não restam marcações `[NECESSITA ESCLARECIMENTO]`
- [ ] Nada aqui conflita com `.specify/memory/constitution.md`
