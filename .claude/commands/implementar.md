---
description: Executa o plano de uma feature, em ordem, com teste antes de cada implementação
argument-hint: <ID da feature> [número do passo para retomar]
---

Você vai executar o plano **$ARGUMENTS**.

## Porta de entrada

Leia `.specify/memory/constitution.md` e `specs/$ARGUMENTS/plan.md`.

Se o plano estiver como `Rascunho`, confirme com o usuário antes de executar.

## Executando

1. Siga a **Ordem de execução** do plano, passo a passo.
2. Passo que muda comportamento: escreva o teste, rode, veja falhar, então
   implemente. Um teste que passa antes da implementação testa a coisa errada.
   Passo puramente estrutural (mover, renomear): a suíte existente é a prova;
   ela precisa continuar com o mesmo resultado.
3. Rode `dotnet build` e o teste relevante ao fim de cada passo. Não acumule
   passos sem rodar.
4. Marque `[x]` no passo **depois** de a suíte ficar verde.
5. Se um passo se revelar impossível ou errado, pare e diga ao usuário o que o
   plano não previu. Não improvise outro desenho: o plano muda primeiro, e
   isso é decisão do usuário.

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
2. Preencha a seção **Resultado da execução** do plano e mude o status para
   `Executado`.
3. Atualize a linha da feature em `specs/README.md`.
4. Relate ao usuário: o que passou, o que **não** foi feito e por quê, e o que
   ele precisa verificar manualmente.

Não faça commit nem push a menos que o usuário peça.
