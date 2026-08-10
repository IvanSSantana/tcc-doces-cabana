---
description: Verifica consistência entre spec, plano, tarefas e código de uma feature
argument-hint: <ID da feature, ex.: 001-cadastro-produto-admin>
---

Você vai auditar a coerência dos artefatos de **$ARGUMENTS**. Este é um comando de
**leitura**: não altere nada, apenas relate.

Leia `.specify/memory/constitution.md` e todos os arquivos de `specs/$ARGUMENTS/`,
e depois o código que eles dizem respeito.

Procure, nesta ordem de gravidade:

1. **Requisito órfão** — `RF-xx` ou `RN-xx` na spec sem tarefa correspondente.
2. **Código órfão** — arquivo alterado pela feature que nenhum requisito pede
   (escopo que cresceu sozinho).
3. **Violação constitucional** — código entregue que fere um princípio sem
   justificativa registrada na seção 9 do plano.
4. **Critério não provado** — `CA-xx` sem teste nem verificação manual registrada.
5. **Deriva plano ↔ código** — o plano diz que muda o arquivo A, o código mudou o
   arquivo B.
6. **Pendência viva** — `[NECESSITA ESCLARECIMENTO]` numa spec marcada como
   aprovada ou implementada.
7. **Detalhe de implementação vazando para a spec** — nome de classe, tabela ou
   framework em `spec.md`.

Relate como uma tabela: gravidade, onde, o que está errado, o que fazer. Ordene
da mais grave para a menos. Se não achar nada, diga isso — não invente achado
para parecer útil.
