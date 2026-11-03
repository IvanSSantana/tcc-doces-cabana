# Fluxo de trabalho — debate, plano, execução

O projeto trabalha em três etapas. O plano é o único artefato escrito: ele guarda
as decisões do debate e o desenho técnico, e o código é derivado dele.

```
   debate ──────► plan.md ──────────► código
   conversa,      decisões + como      execução
   sem arquivo    e onde, em ordem
```

A regra que sustenta tudo: **a execução só começa quando o plano não tem mais
decisão em aberto**.

> Até a feature `028`, o projeto usou o SDD clássico (`spec.md` → `plan.md` →
> `tasks.md` → `checklist.md`). Essas pastas continuam em `specs/` como
> histórico. A partir da `029`, cada pasta tem só o `plan.md`.

## O que tem nesta pasta

```
.specify/
├── memory/
│   └── constitution.md      Os 8 princípios inegociáveis do projeto.
│                            Todo plano é validado contra ela.
├── templates/
│   └── plan-template.md     Decisões, impacto arquivo por arquivo, ordem de
│                            execução, testes, riscos e resultado.
└── scripts/
    └── nova-feature.ps1     Cria specs/NNN-slug/plan.md já preenchido.
```

E fora daqui:

```
specs/                       Uma pasta por feature, com o índice em README.md.
.claude/commands/            /planejar e /implementar.
```

## Como usar

### 1. Debater

Converse com o assistente sobre o problema: o que muda, por quê, o que fica de
fora, quais alternativas existem. Nenhum arquivo é criado nesta etapa. Ela
termina quando não sobra decisão de negócio em aberto.

### 2. Planejar

```
/planejar controle de estoque para os produtos do catálogo
```

Cria `specs/NNN-slug/plan.md` com: as decisões do debate (com motivo e
alternativa descartada), a verificação constitucional, o impacto arquivo por
arquivo, os contratos, a ordem de execução, os testes e os riscos.

Para criar só a pasta:

```powershell
.\.specify\scripts\nova-feature.ps1 "controle de estoque"
```

Use `-CriarBranch` para já criar a branch de mesmo nome.

> ⚠️ **Ao editar `nova-feature.ps1`, salve como UTF-8 *com BOM*.** O Windows
> PowerShell 5.1 lê script sem BOM como ANSI e corrompe os acentos, quebrando o
> parser com `TerminatorExpectedAtEndOfString`. O `plan.md` gerado, ao
> contrário, é gravado sem BOM de propósito.

Revise o plano e mude o status para `Aprovado`.

### 3. Executar

```
/implementar 029-controle-de-estoque
```

Segue a ordem de execução do plano, testando a cada passo, marca os passos
concluídos e preenche o resultado da execução.

## Por que a constituição existe

Sem ela, cada plano renegocia as mesmas decisões: onde valida, quem persiste, em
que língua nomeia. A `constitution.md` fixa isso uma vez e vira um teste que todo
plano precisa passar. Quando um plano não passa, ou o plano está errado ou a
constituição precisa de emenda — nunca "dessa vez tudo bem".

## Perguntas frequentes

**Preciso de plano para uma correção de uma linha?** Não. Correção pequena de
bug e ajuste de estilo vão direto do debate para o código.

**O plano pode mudar depois de aprovado?** Pode. O que não pode é o código
divergir dele em silêncio: mudou o entendimento, atualiza o plano primeiro.

**Onde registro uma dívida que descobri?** Na tabela de dívidas de
`specs/000-baseline/spec.md`. Ela é resolvida por um plano futuro que a
referencia explicitamente.
