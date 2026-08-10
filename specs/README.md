# Especificações

Uma pasta por feature, no formato `NNN-slug-em-portugues`, contendo `spec.md`,
`plan.md`, `tasks.md` e, ao final, `checklist.md`.

O nome da pasta é também o nome da branch. Numeração sequencial, nunca reaproveitada.

## Índice

| ID | Feature | Status | Artefatos |
|---|---|---|---|
| [000](./000-baseline/spec.md) | Baseline do sistema | Implementada (parcial) | spec |
| [001](./001-cadastro-produto-admin/spec.md) | Cadastro de produto pelo administrador | Rascunho — 1 pendência aberta | spec · [plan](./001-cadastro-produto-admin/plan.md) · [tasks](./001-cadastro-produto-admin/tasks.md) |

## Backlog sugerido

Derivado das tabelas do [`ModelagemBancoTCC.dbml`](../ModelagemBancoTCC.dbml) que
ainda não têm comportamento. Ordem sugerida por dependência:

| Próximo ID | Feature | Depende de |
|---|---|---|
| 002 | Listagem, edição e exclusão de produto (admin) | 001 |
| 003 | Navegação por categoria e subcategoria | 001 |
| 004 | Controle de estoque | 001 |
| 005 | Lista de favoritos | 000 |
| 006 | Endereço do usuário | 000 |
| 007 | Carrinho e fechamento de pedido | 004, 006 |
| 008 | Pagamento | 007 |
| 009 | Avaliação de produto | 007 |
| 010 | Promoções | 001 |

## Como criar a próxima

```powershell
.\.specify\scripts\nova-feature.ps1 "listagem de produtos admin"
```

O script cria a pasta numerada, copia os templates e opcionalmente cria a branch.
Depois disso, o fluxo é `/especificar` → `/planejar` → `/tarefas` → `/implementar`.

Leia [`.specify/README.md`](../.specify/README.md) antes da primeira vez.
