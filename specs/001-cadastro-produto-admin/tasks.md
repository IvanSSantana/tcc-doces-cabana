# Tarefas — Cadastro de produto pelo administrador

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

> Reescrita em 2026-08-12: a versão original previa criar `Categoria`/`Subcategoria`
> aqui — já existem, criadas pela `003-modelo-de-dados-completo`. A pendência de
> papéis que bloqueava a T030 original está resolvida (mínimo viável inline,
> tela completa fica para a `005`). Sem tarefas bloqueadas nesta versão.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `001-cadastro-produto-admin` a partir de `main`
      (com `003` já integrada).
- [x] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado
      inicial verde: **227 testes, 0 falhas, 0 avisos**.

## Fase 2 — Testes (devem falhar)

- [x] **T003** `[P]` — `Tests/Units/Services/SubcategoriaServiceTests.cs`:
      `BuscarTodasSubcategorias` mapeia a lista do repositório para
      `SubcategoriaDTO`.
- [x] **T004** `[P]` — `Tests/Units/Services/ProdutoServiceTests.cs`: acrescentar
      `Dado_ProdutoValido_Quando_Cadastrar_Entao_DeveChamarSalvarAlteracoes` —
      verifica que `IUnitOfWork.SalvarAlteracoes` é chamado após `Adicionar`.
- [x] **T005** `[P]` — `Tests/Units/Controllers/AdminControllerTests.cs`
      (criar): GET carrega subcategorias; POST com `ModelState` inválido
      devolve `ViewResult` e não chama `IProdutoService`; POST válido chama o
      serviço e devolve `RedirectToActionResult` com `TempData` preenchido.
- [x] **T006** — Rodar `dotnet test` e confirmar que T003–T005 falham pelo
      motivo esperado.

## Fase 3 — Aplicação

- [x] **T007** `[P]` — `Application/DTOs/SubcategoriaDTO.cs`:
      `SubcategoriaId`, `Nome`.
- [x] **T008** `[P]` — `Application/Contracts/Repositories/ISubcategoriaRepository.cs`.
- [x] **T009** `[P]` — `Application/Contracts/Services/ISubcategoriaService.cs`.
- [x] **T010** — `Application/Mappings/SubcategoriaMapper.cs`: `ToDTO`.
- [x] **T011** — `Application/Services/SubcategoriaService.cs`.
- [x] **T012** — `Application/Services/ProdutoService.cs`: `Cadastrar` passa a
      chamar `_unitOfWork.SalvarAlteracoes()` após `Adicionar`. Injetar
      `IUnitOfWork` no construtor. **Corrige D-01, causa raiz do RF-02.**
- [x] **T013** — Rodar `dotnet test`: T003–T004 passam.

## Fase 4 — Infraestrutura

- [x] **T014** — `Infrastructure/Repositories/SubcategoriaRepository.cs`:
      `Repository<Subcategoria>`, análogo a `ProdutoRepository`.
- [x] **T015** — `Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`:
      registrar `ISubcategoriaRepository` e `ISubcategoriaService`.
- [x] **T016** — `MVC/Helpers/DbInitializer.cs`: semear o papel
      `Administrador` (via `RoleManager<IdentityRole<Guid>>`) e um usuário
      administrador, condicionado a `!IsProduction()`. Senha do
      `dotnet user-secrets` (`Admin:SenhaInicial`), nunca literal.

## Fase 5 — Apresentação

- [x] **T017** — `MVC/Controllers/AdminController.cs`:
      - Injetar `ISubcategoriaService`.
      - GET `Cadastro`: carrega subcategorias, expõe via `ViewBag.Subcategorias`
        (`SelectList`).
      - POST vira `async Task<IActionResult>` com `[ValidateAntiForgeryToken]`.
      - `if (!ModelState.IsValid) return View(dto);` antes de qualquer efeito.
      - `await _produtoService.Cadastrar(dto)`. **Corrige D-03.**
      - Sucesso: `TempData["Confirmacao"]` + `RedirectToAction(nameof(Cadastro))`.
      - `[Authorize(Roles = "Administrador")]` na classe. **Corrige D-02.**
- [x] **T018** — `MVC/Views/Admin/Cadastro.cshtml`:
      - `asp-action="Cadastro"` (corrige D-04).
      - `<select asp-for="SubcategoriaId" asp-items="ViewBag.Subcategorias">`
        (RF-07).
      - Remover o campo Promoção e o `<select>` de `PromocaoTipo` (corrige D-05).
      - Exibir `TempData["Confirmacao"]`.
- [x] **T019** `[P]` — `MVC/wwwroot/css/pages/cadastro_produto.css`: a view já
      referencia este arquivo, que ainda não existe.
- [x] **T020** — Rodar `dotnet test`: T005 passa.

## Fase 6 — Fechamento

- [x] **T021** — `dotnet test` inteiro verde, contagem maior que 227.
- [ ] **T022** — Subir a aplicação e percorrer manualmente:
      - CA-01: logar como admin, cadastrar produto válido, ver confirmação e
        o produto na vitrine.
      - CA-02: reiniciar a aplicação, confirmar que o produto persiste.
      - CA-03, CA-04, CA-05: nome curto, preço zero, imagem inválida — erro no
        campo certo.
      - CA-06: acessar `/Admin/Cadastro` sem login — redireciona para Login.
      - CA-07: logar como cliente comum — acesso negado.
- [ ] **T023** — Preencher `checklist.md`.
- [ ] **T024** — Atualizar a spec para *Implementada*; riscar D-01 a D-06 em
      `specs/000-baseline/spec.md`; atualizar a linha da `001` em
      `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T018 |
| RF-02 | T012, T022 (CA-02) |
| RF-03 | T017, T018 |
| RF-04 | T017, T018 (validator já existe desde a `002`) |
| RF-05 | T017, T005 |
| RF-06 | T016, T017 |
| RF-07 | T007–T011, T014, T018 |
| RF-08 | T022 |
| RN-01 a RN-04 | `ProdutoDTOValidator` (já existe, `002`) + `Produto.cs` (já existe) |
| RN-05 | já garantido pelo construtor de `Produto` desde a `002` |
| RN-06 | cultura `pt-BR` já configurada |

## Dívidas da baseline resolvidas aqui

| Dívida | Tarefa |
|---|---|
| D-01 — escrita sem `IUnitOfWork` | T012 |
| D-02 — área administrativa aberta | T017 |
| D-03 — POST sem `await`, sem `ModelState`, sem antiforgery | T017 |
| D-04 — `asp-action` apontando para ação inexistente | T018 |
| D-05 — campo Promoção com enum errado | T018 |
| D-06 — ausência de `ProdutoDTOValidator` | já resolvida pela `002`, confirmada aqui |
