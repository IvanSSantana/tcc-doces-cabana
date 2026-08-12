# Especificações

Uma pasta por feature, no formato `NNN-slug-em-portugues`, contendo `spec.md`,
`plan.md`, `tasks.md` e, ao final, `checklist.md`.

O nome da pasta é também o nome da branch. Numeração sequencial, nunca reaproveitada.

## Índice

| ID | Feature | Status | Artefatos |
|---|---|---|---|
| [000](./000-baseline/spec.md) | Baseline do sistema | Implementada (parcial) | spec |
| [001](./001-cadastro-produto-admin/spec.md) | Cadastro de produto pelo administrador | Implementada | spec · [plan](./001-cadastro-produto-admin/plan.md) · [tasks](./001-cadastro-produto-admin/tasks.md) · [checklist](./001-cadastro-produto-admin/checklist.md) |
| [002](./002-revisao-tecnica/spec.md) | Revisão técnica da base | Implementada | spec · [plan](./002-revisao-tecnica/plan.md) · [tasks](./002-revisao-tecnica/tasks.md) · [checklist](./002-revisao-tecnica/checklist.md) |
| [003](./003-modelo-de-dados-completo/spec.md) | Modelo de dados completo | Implementada | spec · [plan](./003-modelo-de-dados-completo/plan.md) · [tasks](./003-modelo-de-dados-completo/tasks.md) · [checklist](./003-modelo-de-dados-completo/checklist.md) |
| 004 | Separar pessoa de credencial | A especificar | — |
| 005 | Papéis e cadastro de administrador | A especificar | — |
| 006 | Testes ponta a ponta em Playwright | A especificar | — |

> **Ordem executada:** `002` → `003` → `001`. A `001` originalmente esperava a
> `004`/`005` para resolver papéis, mas a pendência foi resolvida com o mínimo
> viável embutido na própria `001` (papel `Administrador` + admin semeado) —
> ver a nota de atualização na spec `001`. A `004` (separar pessoa de
> credencial) e a `005` (tela de gestão de administradores) continuam no
> backlog, agora sem bloquear nada além de si mesmas. A `006` (E2E) fica para
> depois delas.

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
