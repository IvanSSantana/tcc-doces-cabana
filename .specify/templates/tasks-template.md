# Tarefas — [NOME DA FEATURE]

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com outras `[P]` vizinhas (arquivos distintos,
  sem dependência entre si). Sem `[P]` significa: termine a anterior primeiro.
- Toda tarefa nomeia **o arquivo exato** que ela cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `[NNN-slug]` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build` e `dotnet test` e registrar o estado inicial (tudo verde antes de começar).

## Fase 2 — Testes (devem falhar)

*Escreva, rode, veja vermelho. Só então passe para a Fase 3.*

- [ ] **T003** `[P]` — `DocesCabana.Tests/Units/Entities/XTests.cs`: testes para RN-01, RN-02.
- [ ] **T004** `[P]` — `DocesCabana.Tests/Units/Validators/XDTOValidatorTests.cs`: caso válido e inválido por regra.
- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Services/XServiceTests.cs`: RF-01 com `Mock<IXRepository>`.
- [ ] **T006** — Confirmar que T003–T005 falham pelo motivo certo (e não por erro de compilação alheio).

## Fase 3 — Domínio

- [ ] **T007** — `DocesCabana.Domain/Entities/X.cs`: entidade com `private set`, construtor validante e `protected Ctor()`.
- [ ] **T008** `[P]` — `DocesCabana.Domain/Enums/XStatus.cs`, se aplicável.
- [ ] **T009** — Rodar `dotnet test`: testes de entidade (T003) passam.

## Fase 4 — Aplicação

- [ ] **T010** `[P]` — `DocesCabana.Application/DTOs/XDTO.cs`.
- [ ] **T011** `[P]` — `DocesCabana.Application/Contracts/Services/IXService.cs`.
- [ ] **T012** `[P]` — `DocesCabana.Application/Contracts/Repositories/IXRepository.cs`.
- [ ] **T013** — `DocesCabana.Application/Mappings/XMapper.cs`: `ToDTO` e `ToEntity`.
- [ ] **T014** — `DocesCabana.Application/Validators/XDTOValidator.cs`.
- [ ] **T015** — `DocesCabana.Application/Services/XService.cs`, com commit via `IUnitOfWork`.
- [ ] **T016** — Rodar `dotnet test`: T004 e T005 passam.

## Fase 5 — Infraestrutura

- [ ] **T017** — `DocesCabana.Infrastructure/Repositories/XRepository.cs`.
- [ ] **T018** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/XConfiguration.cs` e `DbSet` no contexto.
- [ ] **T019** — Migration: `dotnet ef migrations add [Nome] --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`.
- [ ] **T020** — Registrar no contêiner em `DependencyInjections/ApplicationDependencyInjection.cs`.
- [ ] **T021** — `DocesCabana.Tests/Integration/Repositories/XRepositoryIntegrationTests.cs`.

## Fase 6 — Apresentação

- [ ] **T022** — `DocesCabana.MVC/Controllers/XController.cs`: `[ValidateAntiForgeryToken]`, `await`, guarda de `ModelState`, redirecionamento no sucesso.
- [ ] **T023** `[P]` — `DocesCabana.MVC/Views/X/Y.cshtml`, com `asp-action` apontando para o nome real da ação.
- [ ] **T024** `[P]` — `DocesCabana.MVC/wwwroot/css/pages/x.css`.
- [ ] **T025** — `DocesCabana.Tests/Units/Controllers/XControllerTests.cs`.

## Fase 7 — Fechamento

- [ ] **T026** — `dotnet test` inteiro verde.
- [ ] **T027** — Executar a aplicação e percorrer manualmente cada critério de aceite da spec.
- [ ] **T028** — Preencher o checklist em `checklist.md`.
- [ ] **T029** — Atualizar o status da spec para *Implementada* e a linha da feature em `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T003, T007, T015 |
| RN-01 | T003, T007 |
| CA-01 | T005, T025, T027 |
