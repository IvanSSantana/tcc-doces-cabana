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
| [003](./003-modelo-de-dados-completo/spec.md) | Modelo de dados completo | Rascunho | spec · [plan](./003-modelo-de-dados-completo/plan.md) · [tasks](./003-modelo-de-dados-completo/tasks.md) |
| 004 | Separar pessoa de credencial | A especificar | — |
| 005 | Papéis e cadastro de administrador | A especificar | — |
| 006 | Testes ponta a ponta em Playwright | A especificar | — |

> **Ordem de execução:** `002` → `003` → `004` → `005` → `001` → `006`.
>
> A `002` (feita) preparou a base. A `003` cria as dez tabelas que faltam — sem
> elas a `001` não tem subcategoria para oferecer numa lista nem promoção de
> verdade para vincular. A `004` separa o dado de negócio do usuário da
> credencial do Identity, removendo a exceção à direção de dependência que a
> constituição hoje tolera. A `005` resolve a pendência de autorização da `001`.
> Só então a `001` fecha, e a `006` valida tudo pela interface.

## Backlog

Derivado das tabelas do [`ModelagemBancoTCC.dbml`](../ModelagemBancoTCC.dbml) que
ainda não têm comportamento. **Sem número** — o número é atribuído quando a spec
é criada, para que a chegada de uma feature nova não renumere a lista inteira.
Ordem sugerida por dependência:

| Feature | Depende de |
|---|---|
| Listagem, edição e exclusão de produto (admin) | 001 |
| Navegação por categoria e subcategoria | 003 |
| Controle de estoque | 003 |
| Lista de favoritos | 003 |
| Endereço do usuário | 003 |
| Carrinho e fechamento de pedido | estoque, endereço |
| Pagamento | carrinho |
| Avaliação de produto | carrinho |
| Promoções na vitrine | 003 |

## Como criar a próxima

```powershell
.\.specify\scripts\nova-feature.ps1 "listagem de produtos admin"
```

O script cria a pasta numerada, copia os templates e opcionalmente cria a branch.
Depois disso, o fluxo é `/especificar` → `/planejar` → `/tarefas` → `/implementar`.

Leia [`.specify/README.md`](../.specify/README.md) antes da primeira vez.
