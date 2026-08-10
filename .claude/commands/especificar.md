---
description: Cria ou refina a spec.md de uma feature a partir de uma descrição em linguagem natural
argument-hint: <descrição da feature, ou o ID de uma spec existente para refinar>
---

Você vai produzir a especificação de: **$ARGUMENTS**

## Antes de escrever

1. Leia `.specify/memory/constitution.md` inteira. Ela manda em tudo que vem depois.
2. Leia `specs/000-baseline/spec.md` para saber o que já existe. Não especifique
   comportamento que já está pronto, e não assuma pronto o que está na lista de
   dívidas.
3. Leia `specs/README.md` para ver o índice e o backlog.
4. Se `$ARGUMENTS` for o ID de uma feature existente (`NNN-...`), leia a `spec.md`
   dela e refine em vez de criar do zero.
5. Explore o código relevante antes de escrever. Uma spec escrita sem olhar o
   código descreve um sistema imaginário.

## Escrevendo

Crie a pasta com `.\.specify\scripts\nova-feature.ps1 "<descrição>"` e preencha a
`spec.md` gerada, seguindo o template.

Regras que não se negociam:

- **O quê e por quê, nunca como.** Se você escreveu o nome de uma classe, tabela,
  framework ou rota, isso pertence à `plan.md`, não aqui.
- **Não adivinhe.** Toda decisão que exige conhecimento do negócio que você não
  tem vira `[NECESSITA ESCLARECIMENTO: pergunta objetiva]`. Preencher com um
  palpite plausível é o pior resultado possível: parece decidido e não está.
- Todo requisito precisa ser verificável por um teste. "O sistema deve ser rápido"
  não é requisito; "a vitrine deve carregar em até 2 segundos" é.
- Cubra os caminhos de erro, não só o feliz.
- Escreva as mensagens ao usuário no texto final, em português.
- Preencha "Fora de escopo" de verdade — é a seção que mais evita retrabalho.

## Ao terminar

- Marque o checklist ao final da spec com honestidade.
- Adicione a linha da feature no índice de `specs/README.md`.
- Apresente ao usuário, em uma lista curta, **apenas as marcações
  `[NECESSITA ESCLARECIMENTO]`** que sobraram. Elas são a única coisa que ele
  precisa decidir agora.
- Não gere `plan.md` nem escreva código. Pare aqui.
