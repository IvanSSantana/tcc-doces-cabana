# SDD — Desenvolvimento Orientado a Especificação

Neste projeto a especificação é o artefato principal, e o código é o que se
deriva dela. A ordem é sempre a mesma:

```
   ideia ──► spec.md ──► plan.md ──► tasks.md ──► código
             o quê       como         em que      execução
             e por quê   e onde       ordem
```

A regra que sustenta tudo: **cada etapa só começa quando a anterior não tem mais
ambiguidade**. Especificação com dúvida em aberto não vira plano; plano sem
arquivos nomeados não vira tarefa; tarefa sem teste não vira código.

## O que tem nesta pasta

```
.specify/
├── memory/
│   └── constitution.md      Os 8 princípios inegociáveis do projeto.
│                            Toda spec, plano e tarefa é validada contra ela.
├── templates/
│   ├── spec-template.md     O quê e por quê. Sem detalhe técnico.
│   ├── plan-template.md     Como e onde. Arquivo por arquivo.
│   ├── tasks-template.md    Em que ordem. Uma tarefa, um arquivo, um commit.
│   └── checklist-template.md  Portão de saída antes do merge.
└── scripts/
    └── nova-feature.ps1     Cria specs/NNN-slug já preenchida.
```

E fora daqui:

```
specs/                       Uma pasta por feature.
├── README.md                Índice e backlog.
├── 000-baseline/            O que já existe hoje, incluindo as dívidas conhecidas.
└── 001-cadastro-produto-admin/   Exemplo completo: spec + plan + tasks.

.claude/commands/            Os comandos que executam o fluxo.
```

## Como usar

### 1. Criar a feature

```powershell
.\.specify\scripts\nova-feature.ps1 "controle de estoque"
```

Cria `specs/004-controle-de-estoque/` com os quatro arquivos já preenchidos com
nome, ID e data. Use `-CriarBranch` para já criar a branch de mesmo nome.

> ⚠️ **Ao editar `nova-feature.ps1`, salve como UTF-8 *com BOM*.** O Windows
> PowerShell 5.1 lê script sem BOM como ANSI e corrompe os acentos, quebrando o
> parser com `TerminatorExpectedAtEndOfString`. Os `.md` gerados, ao contrário,
> são gravados sem BOM de propósito.

### 2. Especificar

```
/especificar controle de estoque para os produtos do catálogo
```

Produz a `spec.md`. **Só descreve comportamento** — nada de classe, tabela ou
framework. Toda decisão de negócio que o assistente não pode tomar sozinho vira
uma linha `[NECESSITA ESCLARECIMENTO: ...]`.

Você responde essas pendências, edita a spec, e só então avança. Essa é a etapa
que decide a qualidade de tudo o que vem depois — vale mais tempo aqui do que em
qualquer outra.

### 3. Planejar

```
/planejar 004-controle-de-estoque
```

Produz a `plan.md`: verificação contra a constituição, lista arquivo por arquivo
do que muda em cada camada, contratos, migration, estratégia de teste,
alternativas descartadas e riscos.

O comando se recusa a rodar se sobrou pendência na spec.

### 4. Quebrar em tarefas

```
/tarefas 004-controle-de-estoque
```

Produz a `tasks.md`: tarefas numeradas, cada uma nomeando um arquivo, com os
testes vindo obrigatoriamente antes da implementação e a tabela de
rastreabilidade requisito → tarefa.

### 5. Implementar

```
/implementar 004-controle-de-estoque
```

Executa em ordem, testando a cada fase. Ao final, preenche o checklist e atualiza
o índice.

### 6. Auditar (a qualquer momento)

```
/analisar 004-controle-de-estoque
```

Relata requisito sem tarefa, código fora do escopo, violação constitucional e
critério de aceite não provado. Só lê, não altera.

## Por que a constituição existe

Sem ela, cada spec renegocia as mesmas decisões: onde valida, quem persiste, em
que língua nomeia. A `constitution.md` fixa isso uma vez e vira um teste que todo
plano precisa passar. Quando um plano não passa, ou o plano está errado ou a
constituição precisa de emenda — nunca "dessa vez tudo bem".

Ela foi extraída da arquitetura que o projeto **já tem**, não imposta de fora.
Por isso a baseline lista sete dívidas: são os pontos onde o código atual ainda
não alcançou os próprios princípios.

## Perguntas frequentes

**Preciso passar pelas quatro etapas para uma correção de uma linha?** Não.
Correção de bug e ajuste de estilo vão direto. O fluxo existe para feature — algo
que muda o que o sistema faz.

**A spec pode mudar depois de aprovada?** Pode, e vai. O que não pode é o código
divergir dela em silêncio: mudou o entendimento, atualiza a spec primeiro.

**Onde registro uma dívida que descobri?** Na tabela de dívidas de
`specs/000-baseline/spec.md`, e ela é resolvida por alguma spec futura que a
referencia explicitamente.
