# Tarefas — Organização de nomenclatura

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria, move ou altera.
- Marque `[x]` só depois de `dotnet test` verde.

> **Esta feature não tem Domínio, Aplicação nem Infraestrutura.** É
> reorganização de nome e de pasta dentro da `DocesCabana.MVC` e dos dois
> projetos de teste, mais uma emenda à constituição (plano §3).

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `010-organizacao-de-nomenclatura` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build` e `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`, registrar o estado inicial (tudo verde antes de começar). Confirmar por busca de texto que só os 4 arquivos do plano §3 referenciam `AdminController`/`Admin/Cadastro` (plano §8, risco 1).

## Fase 2 — Testes (devem falhar)

*Escreva/ajuste, rode, veja vermelho pelo motivo certo. Só então passe para a Fase 3.*

- [ ] **T003** — `DocesCabana.Tests/Units/Controllers/AdminControllerTests.cs`: renomear para `CatalogoControllerTests.cs`, classe `AdminControllerTests` → `CatalogoControllerTests`, `new AdminController(...)` → `new CatalogoController(...)`. Vai falhar a compilar — `CatalogoController` ainda não existe.
- [ ] **T004** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaCadastroProduto.cs`: `$"{urlBase}/Admin/Cadastro"` → `$"{urlBase}/Catalogo/Cadastro"`.
- [ ] **T005** `[P]` — `DocesCabana.Tests.E2E/Fluxos/AreaAdministrativaTests.cs`: as duas ocorrências de `$"{UrlBase}/Admin/Cadastro"` → `$"{UrlBase}/Catalogo/Cadastro"`; acrescentar `Dado_EnderecoAntigoDeCadastroDeProduto_Quando_Acessado_Entao_DeveResponder404` (CA-01), autenticando como administrador antes de acessar `/Admin/Cadastro` e checando `resposta.Status == 404`.
- [ ] **T006** — Confirmar que T003 falha por compilação (`CatalogoController` inexistente) e que o novo teste E2E de T005 falha por 200-em-vez-de-404 (rota antiga ainda existe) — nunca por erro alheio.

## Fase 3 — Renomear e mover

- [ ] **T007** — `git mv DocesCabana.MVC/Controllers/AdminController.cs DocesCabana.MVC/Controllers/CatalogoController.cs`; dentro do arquivo, `class AdminController` → `class CatalogoController`. Corpo do arquivo inalterado além do nome.
- [ ] **T008** — `git mv DocesCabana.MVC/Views/Admin DocesCabana.MVC/Views/Catalogo` (a pasta inteira, com `Cadastro.cshtml` dentro). Nenhum conteúdo de arquivo muda.
- [ ] **T009** `[P]` — `git mv DocesCabana.MVC/Views/Shared/_Carrossel.cshtml DocesCabana.MVC/Views/Home/_Carrossel.cshtml`.
- [ ] **T010** `[P]` — `git mv DocesCabana.MVC/Views/Shared/_Categorias.cshtml DocesCabana.MVC/Views/Home/_Categorias.cshtml`.
- [ ] **T011** — Rodar `dotnet build`: sem erro. Rodar `dotnet test DocesCabana.Tests`: T003 passa.

## Fase 4 — Verificação ao vivo

- [ ] **T012** — Subir a aplicação. Confirmar ao vivo: `/Catalogo/Cadastro` autenticado como administrador abre a tela e cadastra produto normalmente (CA-02); `/Admin/Cadastro` devolve 404 (CA-01); a página inicial renderiza carrossel e categorias sem diferença visual (as duas partials movidas resolvem — plano §8, risco 2).
- [ ] **T013** — Rodar `dotnet test DocesCabana.Tests.E2E` inteiro: T004, T005 e o resto da suíte (inclusive os fluxos que passam pela página inicial e pela área administrativa) verdes.

## Fase 5 — Emenda constitucional

- [ ] **T014** — `.specify/memory/constitution.md`: acrescentar ao Princípio IV a regra de RQ-02 — tela parcial de uso único mora com o controlador dono; `Views/Shared/` é reservado ao que é reaproveitado por mais de uma página. Registrar a emenda **PATCH** no histórico de emendas (Governança), com data e motivo, citando esta feature.

## Fase 6 — Fechamento

- [ ] **T015** — `dotnet build` sem warnings novos; `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E` inteiros verdes.
- [ ] **T016** — Preencher o checklist em `checklist.md`.
- [ ] **T017** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md` (índice, nota de ordem executada).

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RQ-01 | T003, T007, T008 |
| RQ-02 | T009, T010 |
| RQ-03 | T014 |
| CA-01 | T005, T012 |
| CA-02 | T004, T012, T013 |
| CA-03 | T005, T013 |
