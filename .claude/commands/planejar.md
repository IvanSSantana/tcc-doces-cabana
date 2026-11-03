---
description: Gera o plano (com as decisões do debate) de uma feature ou correção
argument-hint: <descrição da feature, ou o ID de um plano existente para revisar>
---

Você vai produzir o plano de: **$ARGUMENTS**

O fluxo do projeto tem três etapas: **debate** (conversa, sem arquivo) →
**planejamento** (este comando, gera `plan.md`) → **execução** (`/implementar`).
O plano é o único artefato: ele guarda as decisões do debate e o desenho técnico.

## Antes de escrever

1. Leia `.specify/memory/constitution.md`.
2. Releia o debate desta conversa. Toda decisão tomada nele entra na seção
   **Decisões** do plano, com o motivo.
3. Se `$ARGUMENTS` for o ID de uma pasta existente (`NNN-...`), leia o
   `plan.md` dela e revise em vez de criar outro.
4. Leia o código real de cada camada que a mudança toca. Não planeje contra a
   sua memória da estrutura. Verifique o que já existe e pode ser reusado.
5. **Pendência de negócio que o debate não fechou: pare e pergunte.** Não
   preencha com palpite. Decisão técnica que o código responde, decida e
   registre na tabela de decisões.

## Escrevendo

Crie a pasta com `.\.specify\scripts\nova-feature.ps1 "<descrição curta>"` e
preencha o `plan.md` gerado, seguindo o template.

- Comece pela **Verificação constitucional**. Um ❌ indica abordagem errada.
  Só registre desvio na seção 8 se conseguir dizer por que a alternativa
  conforme foi descartada.
- Liste **arquivo por arquivo** o que muda, com caminho exato.
- Só assinaturas de contrato. Implementação é execução, não plano.
- A **Ordem de execução** substitui a antiga lista de tarefas: passos curtos,
  em ordem de dependência, com teste antes da implementação quando há mudança
  de comportamento.
- Preencha "Alternativa descartada" em cada decisão relevante.
- Adicione a linha da feature em `specs/README.md` com status `Planejada`.

## Ao terminar

Apresente ao usuário: a abordagem em 3 frases, os desvios constitucionais (se
houver) e os riscos de impacto alto. Não escreva código.
