---
description: Executa as tarefas de uma feature, em ordem, com teste antes de cada implementação
argument-hint: <ID da feature> [Txxx para retomar de uma tarefa específica]
---

Você vai implementar a feature **$ARGUMENTS**.

## Porta de entrada

Leia, nesta ordem: `.specify/memory/constitution.md`,
`specs/$ARGUMENTS/spec.md`, `specs/$ARGUMENTS/plan.md`, `specs/$ARGUMENTS/tasks.md`.

Se houver tarefa marcada como bloqueada por pendência da spec, **execute todas as
demais** e deixe a bloqueada por último, avisando o usuário. Não invente a
resposta que falta para destravá-la.

## Executando

1. Trabalhe em ordem numérica. Tarefas `[P]` vizinhas podem ir juntas.
2. **Escreva o teste, rode, veja falhar, então implemente.** Não pule o passo de
   ver falhar: um teste que passa antes da implementação está testando a coisa
   errada.
3. Rode `dotnet test` ao fim de cada fase. Não acumule fases sem rodar.
4. Marque `[x]` na tarefa em `tasks.md` **depois** de a suíte ficar verde.
5. Se uma tarefa se revelar impossível ou errada, pare e diga ao usuário o que o
   plano não previu. Não improvise um desenho diferente no meio da execução — o
   plano é que precisa mudar, e isso é decisão dele.

## Enquanto escreve código

- Siga o padrão do arquivo vizinho, não o seu padrão preferido.
- Português em tudo que for de negócio.
- Entidade nova: `private set`, construtor validante, `protected Ctor()`.
- Escrita no banco: `IUnitOfWork` sempre. Sem commit, nada foi salvo.
- POST: `[ValidateAntiForgeryToken]`, `await`, guarda de `ModelState`,
  redirecionamento no sucesso.
- Teste nomeado `Dado_..._Quando_..._Entao_...`.

## Ao terminar

1. `dotnet build` e `dotnet test` completos.
2. Preencha `specs/$ARGUMENTS/checklist.md`.
3. Atualize o status da spec e a linha em `specs/README.md`.
4. Se a feature resolveu dívidas da baseline, risque-as em
   `specs/000-baseline/spec.md`.
5. Relate ao usuário: o que passou, o que **não** foi feito e por quê, e o que
   ele precisa verificar manualmente (os critérios de aceite que nenhum teste
   automatizado cobre).

Não faça commit nem push a menos que o usuário peça.
