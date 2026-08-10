# Tarefas — Cadastro de produto pelo administrador

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

> ⚠️ Parcialmente bloqueado: a spec tem uma pendência aberta sobre como um
> usuário vira administrador (seção 10). Só a **T030** depende dessa resposta —
> todas as demais tarefas podem começar.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `001-cadastro-produto-admin` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado inicial verde.
- [ ] **T003** — Inspecionar `DocesCabana.Infrastructure/DependencyInjections/IdentityDependencyInjection.cs` e confirmar se papéis (`AddRoles`) já estão habilitados. Registra o risco 2 do plano.

## Fase 2 — Testes (devem falhar)

- [ ] **T004** `[P]` — `Tests/Units/Entities/SubcategoriaTests.cs`: nome vazio lança, nome com menos de 3 caracteres lança, `CategoriaId` vazio lança, caso válido constrói.
- [ ] **T005** `[P]` — `Tests/Units/Validators/ProdutoDTOValidatorTests.cs`: um caso válido e um inválido para cada uma de RN-01 a RN-04 (CA-03, CA-04, CA-05).
- [ ] **T006** `[P]` — `Tests/Units/Services/ProdutoServiceTests.cs`: adicionar `Dado_ProdutoValido_Quando_Cadastrar_Entao_DeveChamarAdicionarECommit` e `..._Entao_DeveRetornarDtoComIdPreenchido`.
- [ ] **T007** — Rodar `dotnet test` e confirmar que T004–T006 falham pelo motivo esperado.

## Fase 3 — Domínio

- [ ] **T008** `[P]` — `Domain/Entities/Categoria.cs`: `private set`, construtor validante, `protected Ctor()`.
- [ ] **T009** `[P]` — `Domain/Entities/Subcategoria.cs`: mesmo padrão, com `ValidarCategoria` e `ValidarNome`.
- [ ] **T010** — `dotnet test`: T004 passa.

## Fase 4 — Aplicação

- [ ] **T011** `[P]` — `Application/DTOs/SubcategoriaDTO.cs` (`Id`, `Nome`).
- [ ] **T012** `[P]` — `Application/Contracts/Repositories/ISubcategoriaRepository.cs`.
- [ ] **T013** `[P]` — `Application/Contracts/Services/ISubcategoriaService.cs`.
- [ ] **T014** `[P]` — `Application/Mappings/SubcategoriaMapper.cs`.
- [ ] **T015** — `Application/Validators/ProdutoDTOValidator.cs`. Mensagens idênticas às do domínio (`Produto.cs`), para que o usuário veja o mesmo texto por qualquer caminho.
- [ ] **T016** — `Application/Services/SubcategoriaService.cs`.
- [ ] **T017** — `Application/Services/ProdutoService.cs`: injetar `IUnitOfWork`; em `Cadastrar`, chamar `Commit` após `Adicionar` e retornar `ProdutoMapper.ToDTO(produto)`. **Corrige a dívida D-01 e é a causa raiz do RF-02.**
- [ ] **T018** — `dotnet test`: T005 e T006 passam.

## Fase 5 — Infraestrutura

- [ ] **T019** `[P]` — `Infrastructure/DatabaseContext/Configurations/CategoriaConfiguration.cs`.
- [ ] **T020** `[P]` — `Infrastructure/DatabaseContext/Configurations/SubcategoriaConfiguration.cs`: FK para `Categoria` com `DeleteBehavior.Restrict`.
- [ ] **T021** — `Infrastructure/DatabaseContext/DocesCabanaDbContext.cs`: `DbSet<Categoria>`, `DbSet<Subcategoria>`, e FK de `Produto.SubcategoriaId`.
- [ ] **T022** — `Infrastructure/Repositories/SubcategoriaRepository.cs`.
- [ ] **T023** — `Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: registrar `ISubcategoriaRepository` e `ISubcategoriaService`.
- [ ] **T024** — Migration `AddCategoriaSubcategoria`. Conferir o SQL gerado antes de aplicar.
- [ ] **T025** — `Tests/Integration/Repositories/ProdutoRepositoryIntegrationTests.cs`: provar CA-02 — adicionar, commitar, reler e encontrar o produto.

## Fase 6 — Apresentação

- [ ] **T026** — `MVC/Helpers/DbInitializer.cs`: semear categorias e subcategorias **antes** dos produtos, e vincular os produtos da massa inicial a subcategorias reais (risco 1 do plano).
- [ ] **T027** — `MVC/Controllers/AdminController.cs`:
  - GET `Cadastro` carrega as subcategorias e devolve `SelectList` via `ViewBag`
  - POST vira `async Task<IActionResult>` com `[ValidateAntiForgeryToken]`
  - `if (!ModelState.IsValid) return View(dto);` antes de qualquer efeito
  - `await _produtoService.Cadastrar(dto)` — **corrige a dívida D-03**
  - Sucesso: `TempData` com mensagem e `RedirectToAction(nameof(Cadastro))`
- [ ] **T028** — `MVC/Views/Admin/Cadastro.cshtml`: `asp-action="Cadastro"` (**corrige D-04**), `select` de subcategoria com `asp-items` (RF-07), remoção do campo Promoção (**corrige D-05**), exibição da mensagem de sucesso do `TempData`, inclusão de `_ValidationScriptsPartial`.
- [ ] **T029** `[P]` — `MVC/wwwroot/css/pages/cadastro_produto.css`: o arquivo é referenciado pela view e ainda não existe.
- [ ] **T030** — `[BLOQUEADA pela pendência da spec]` `AdminController` com `[Authorize(Roles = "Administrador")]` e semeadura do papel + usuário administrador no `DbInitializer`. **Corrige a dívida D-02.** Senha do administrador via *user secrets*, nunca literal.
- [ ] **T031** — `Tests/Units/Controllers/AdminControllerTests.cs`: `ModelState` inválido devolve `ViewResult` e **não** chama o serviço; válido chama o serviço e devolve `RedirectToActionResult`.

## Fase 7 — Fechamento

- [ ] **T032** — `dotnet test` inteiro verde.
- [ ] **T033** — Subir a aplicação e percorrer CA-01 a CA-07 manualmente.
- [ ] **T034** — Preencher `checklist.md` a partir de `.specify/templates/checklist-template.md`.
- [ ] **T035** — Atualizar a spec para *Implementada*, riscar as dívidas D-01 a D-06 em `specs/000-baseline/spec.md` e atualizar a linha da feature em `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T028 |
| RF-02 | T017, T024, T025 |
| RF-03 | T027, T028 |
| RF-04 | T015, T027, T028 |
| RF-05 | T027, T031 |
| RF-06 | T030 |
| RF-07 | T011–T016, T022, T026, T028 |
| RF-08 | T026, T033 |
| RN-01 a RN-04 | T005, T015 (barreira de entrada) + `Produto.cs` já existente (invariante) |
| RN-05 | já garantido pelo construtor de `Produto` |
| RN-06 | cultura `pt-BR` já configurada; verificar em T033 |

## Dívidas da baseline resolvidas aqui

| Dívida | Tarefa |
|---|---|
| D-01 — escrita sem `IUnitOfWork` | T017 |
| D-02 — área administrativa aberta | T030 |
| D-03 — POST sem `await`, sem `ModelState`, sem antiforgery | T027 |
| D-04 — `asp-action` apontando para ação inexistente | T028 |
| D-05 — campo Promoção com enum errado | T028 |
| D-06 — ausência de `ProdutoDTOValidator` | T015 |
