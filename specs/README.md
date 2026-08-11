# Especificações

Uma pasta por feature, no formato `NNN-slug-em-portugues`, contendo `spec.md`,
`plan.md`, `tasks.md` e, ao final, `checklist.md`.

O nome da pasta é também o nome da branch. Numeração sequencial, nunca reaproveitada.

## Índice

| ID | Feature | Status | Artefatos |
|---|---|---|---|
| [000](./000-baseline/spec.md) | Baseline do sistema | Implementada (parcial) | spec |
| [001](./001-cadastro-produto-admin/spec.md) | Cadastro de produto pelo administrador | Rascunho — 1 pendência aberta | spec · [plan](./001-cadastro-produto-admin/plan.md) · [tasks](./001-cadastro-produto-admin/tasks.md) |
| [002](./002-revisao-tecnica/spec.md) | Revisão técnica da base | Implementada | spec · [plan](./002-revisao-tecnica/plan.md) · [tasks](./002-revisao-tecnica/tasks.md) · [checklist](./002-revisao-tecnica/checklist.md) |

> **Ordem de execução:** a `002` vem **antes** da `001`. A `001` grava produto
> através da unidade de trabalho que a `002` simplifica, precisa da validação de
> entrada que a `002` cria, e depende da correção de status que a `002` faz.

## Backlog sugerido

Derivado das tabelas do [`ModelagemBancoTCC.dbml`](../ModelagemBancoTCC.dbml) que
ainda não têm comportamento. Ordem sugerida por dependência:

| Próximo ID | Feature | Depende de |
|---|---|---|
| 003 | Listagem, edição e exclusão de produto (admin) | 001 |
| 004 | Navegação por categoria e subcategoria | 001 |
| 005 | Controle de estoque | 001 |
| 006 | Lista de favoritos | 000 |
| 007 | Endereço do usuário | 000 |
| 008 | Carrinho e fechamento de pedido | 005, 007 |
| 009 | Pagamento | 008 |
| 010 | Avaliação de produto | 008 |
| 011 | Promoções | 001 |

## Como criar a próxima

```powershell
.\.specify\scripts\nova-feature.ps1 "listagem de produtos admin"
```

O script cria a pasta numerada, copia os templates e opcionalmente cria a branch.
Depois disso, o fluxo é `/especificar` → `/planejar` → `/tarefas` → `/implementar`.

Leia [`.specify/README.md`](../.specify/README.md) antes da primeira vez.
