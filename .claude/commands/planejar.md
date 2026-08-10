---
description: Gera o plan.md técnico a partir de uma spec aprovada
argument-hint: <ID da feature, ex.: 001-cadastro-produto-admin>
---

Você vai produzir o plano técnico da feature **$ARGUMENTS**.

## Porta de entrada

Leia `specs/$ARGUMENTS/spec.md`.

**Se sobrar qualquer `[NECESSITA ESCLARECIMENTO]` na spec, pare.** Liste as
pendências ao usuário e não escreva o plano. Planejar sobre ambiguidade é
inventar requisito, e o custo disso aparece só na implementação.

## Antes de desenhar

1. Leia `.specify/memory/constitution.md`.
2. Leia o código real de cada camada que a feature toca. Não planeje contra a sua
   memória da estrutura — abra os arquivos. Em particular:
   - `DocesCabana.Domain/Entities/` para o padrão de entidade rica
   - `DocesCabana.Application/Services/` e `Contracts/` para o padrão de serviço
   - `DocesCabana.Infrastructure/Repositories/` e `DatabaseContext/`
   - `DocesCabana.MVC/Controllers/` para o padrão de ação
3. Verifique se algo que a feature precisa **já existe** e pode ser reusado. O
   plano que mais economiza é o que descobre que metade já está pronta.

## Escrevendo

Preencha `specs/$ARGUMENTS/plan.md` a partir de
`.specify/templates/plan-template.md`.

- Comece pela **Verificação Constitucional**. Um ❌ é um sinal de que a abordagem
  está errada, não de que a constituição atrapalha. Só registre desvio na seção 9
  se você conseguir escrever por que a alternativa conforme foi descartada.
- Liste **arquivo por arquivo** o que muda. "Ajustar a camada de aplicação" não é
  plano; `Application/Services/ProdutoService.cs: injetar IUnitOfWork` é.
- Só assinaturas de contrato — implementação é tarefa, não plano.
- Mapeie cada critério de aceite da spec para o teste que vai prová-lo.
- Preencha "Alternativas descartadas". Um plano sem alternativa considerada
  geralmente é o primeiro caminho que passou pela cabeça.

## Ao terminar

Apresente ao usuário: a abordagem em 3 frases, os desvios constitucionais (se
houver) e os riscos de impacto alto. Não gere `tasks.md` nem escreva código.
