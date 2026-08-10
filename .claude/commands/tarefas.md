---
description: Quebra um plano aprovado em tarefas executáveis e ordenadas
argument-hint: <ID da feature, ex.: 001-cadastro-produto-admin>
---

Você vai quebrar o plano da feature **$ARGUMENTS** em tarefas.

## Porta de entrada

Leia `specs/$ARGUMENTS/spec.md` e `specs/$ARGUMENTS/plan.md`.

Se o plano não existe ou está marcado como Rascunho sem revisão, avise e pare.

## Escrevendo

Preencha `specs/$ARGUMENTS/tasks.md` a partir de
`.specify/templates/tasks-template.md`.

Regras de quebra:

- Uma tarefa nomeia **um arquivo exato** e cabe em um commit. Se você não
  consegue dizer qual arquivo muda, a tarefa está grande demais.
- **Teste antes de implementação, sempre** (Princípio V). A fase de testes vem
  antes da fase de domínio, e existe uma tarefa explícita de "confirmar que os
  testes falham pelo motivo certo" — sem ela, um teste que passa por engano
  passa despercebido.
- Ordem por dependência de camada: Domínio → Aplicação → Infraestrutura →
  Apresentação. Uma camada de fora não é implementada antes da de dentro.
- Marque `[P]` só quando as tarefas tocam arquivos diferentes e nenhuma depende
  do resultado da outra.
- Intercale tarefas de `dotnet test` ao fim de cada fase. Descobrir a quebra três
  fases depois custa muito mais caro.
- Toda tarefa que altera esquema traz o comando `dotnet ef migrations add`
  completo, com `--project` e `--startup-project`.
- Preencha a tabela de rastreabilidade: todo `RF-xx` e `CA-xx` da spec precisa
  aparecer em pelo menos uma tarefa. Requisito sem tarefa é requisito esquecido.
- Se a feature resolve dívidas listadas em `specs/000-baseline/spec.md`, aponte
  qual tarefa resolve qual dívida.

## Ao terminar

Diga ao usuário quantas tarefas saíram, quais estão bloqueadas por pendência, e
qual é a primeira. Não implemente nada.
