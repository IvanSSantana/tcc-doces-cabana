# Tarefas — Área administrativa

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria, move ou altera.
- Marque `[x]` só depois de `dotnet test` verde.

> **Esta feature não tem Domínio, Aplicação nem Infraestrutura.** É
> reorganização de rota e de pasta dentro da `DocesCabana.MVC` e dos dois
> projetos de teste (plano §3).

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `011-area-administrativa` a partir de `main`.
- [x] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 311 e 28 verdes, herdados da `010`).

## Fase 2 — Testes (devem falhar)

*Ajuste, rode, veja vermelho pelo motivo certo. Só então passe para a Fase 3.*

- [x] **T003** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaCadastroProduto.cs`: `/Catalogo/Cadastro` → `/Admin/Produto/Cadastro`.
- [x] **T004** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaAdministradores.cs`: `/Administrador` → `/Admin/Administrador` e `/Administrador/Cadastro` → `/Admin/Administrador/Cadastro`.
- [x] **T005** — `DocesCabana.Tests.E2E/Fluxos/AreaAdministrativaTests.cs`: apontar as 4 rotas para o prefixo novo; estender o teste de 404 da `010` para cobrir as **duas** rotas antigas (CA-03); acrescentar `Dado_Administrador_Quando_UsarOAtalhoDoCabecalho_Entao_DeveChegarNaGestao` (CA-06) e `Dado_AdministradorNaAreaAdministrativa_Quando_ClicarNaPoliticaDoRodape_Entao_DeveSairDaArea` (CA-07).
- [x] **T006** — `DocesCabana.Tests.E2E/Fluxos/LoginTests.cs`: rota e `ReturnUrl` esperado (`%2FAdmin%2FAdministrador`).
- [x] **T007** — Confirmar que T003–T006 falham por 404/rota inexistente, e não por erro de compilação alheio.

## Fase 3 — Criar a area

- [x] **T008** — `DocesCabana.MVC/Program.cs`: registrar `MapControllerRoute` de area com o padrão `{area:exists}/{controller=Home}/{action=Index}/{id?}` **antes** da rota padrão (plano §8, risco 4).
- [x] **T009** `[P]` — `DocesCabana.MVC/Areas/Admin/Views/_ViewImports.cshtml` (criar): cópia do `Views/_ViewImports.cshtml` da raiz. Sem ele nenhum tag helper funciona nas telas movidas (plano §8, risco 2).
- [x] **T010** `[P]` — `DocesCabana.MVC/Areas/Admin/Views/_ViewStart.cshtml` (criar): `Layout = "_Layout"` (plano §8, risco 3).

## Fase 4 — Mover as telas

- [x] **T011** — `git mv DocesCabana.MVC/Controllers/CatalogoController.cs DocesCabana.MVC/Areas/Admin/Controllers/ProdutoController.cs`; classe `CatalogoController` → `ProdutoController` (RQ-04); namespace para `DocesCabana.MVC.Areas.Admin.Controllers`; acrescentar `[Area("Admin")]`. Corpo inalterado.
- [x] **T012** — `git mv DocesCabana.MVC/Controllers/AdministradorController.cs DocesCabana.MVC/Areas/Admin/Controllers/AdministradorController.cs`; mesmo tratamento, sem renomear.
- [x] **T013** `[P]` — `git mv DocesCabana.MVC/Views/Catalogo DocesCabana.MVC/Areas/Admin/Views/Produto` — a pasta de views acompanha o nome do controlador.
- [x] **T014** `[P]` — `git mv DocesCabana.MVC/Views/Administrador DocesCabana.MVC/Areas/Admin/Views/Administrador`.
- [x] **T015** — `git mv` dos dois testes de controller para `DocesCabana.Tests/Units/Controllers/Admin/`, renomeando `CatalogoControllerTests` → `ProdutoControllerTests`; ajustar `using` e o tipo instanciado. Asserções inalteradas. A subpasta evita colidir com `Units/Controllers/ProdutoControllerTests.cs`, do controlador público.
- [x] **T016** — Rodar `dotnet build` e `dotnet test DocesCabana.Tests`: verde.

## Fase 5 — Consertar os links

- [x] **T017** — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: `asp-area="Admin"` no atalho de administradores (RF-04); `asp-area=""` nos links de cliente (logo, conta, sair) para que não herdem a area (RF-05).
- [x] **T018** `[P]` — `DocesCabana.MVC/Views/Shared/_Footer.cshtml`: `asp-area=""` nos links institucionais (CA-07).
- [x] **T019** `[P]` — `DocesCabana.MVC/Views/Shared/_ModalLogin.cshtml`: `asp-area=""` nos links de autenticação e política.

## Fase 6 — Verificação ao vivo

- [x] **T020** — Subir a aplicação e conferir ao vivo, autenticado como administrador: `/Admin/Produto/Cadastro` cadastra produto (CA-01); `/Admin/Administrador` lista e cadastra (CA-02); `/Catalogo/Cadastro` e `/Administrador` devolvem 404 (CA-03); o atalho do cabeçalho chega na gestão (CA-06); o rodapé leva à política de dentro da área administrativa (CA-07); as duas telas administrativas continuam com cabeçalho e rodapé (risco 3).
- [x] **T021** — Rodar `dotnet test DocesCabana.Tests.E2E` inteiro: verde, incluindo os fluxos herdados da `007`.

## Fase 7 — Emenda constitucional

- [x] **T022** — `.specify/memory/constitution.md`: acrescentar ao Princípio IV a ressalva de que a unicidade de nome de classe é escopada por *area* — `Admin/Produto` e `/Produto` são telas de públicos distintos, separadas pelo framework. Registrar como emenda **PATCH** (1.4.0 → 1.4.1) no histórico: corrige o alcance de uma regra introduzida na 1.4.0, não cria regra nova.

## Fase 8 — Fechamento

- [x] **T023** — `dotnet build` sem warnings novos; as duas suítes verdes.
- [x] **T024** — Preencher o checklist em `checklist.md`.
- [x] **T025** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RQ-01 | T008, T011, T012 |
| RQ-02 | T011, T012, T013, T014 |
| RQ-03 | T011, T012, T020 |
| RQ-04 | T011, T013, T015, T022 |
| RF-01 | T005, T020 |
| RF-02 | T015, T016, T020 |
| RF-03 | T005, T006, T021 |
| RF-04 | T017, T020 |
| RF-05 | T017, T018, T019, T020 |
| CA-01 | T003, T020, T021 |
| CA-02 | T004, T020, T021 |
| CA-03 | T005, T020 |
| CA-04 | T005, T021 |
| CA-05 | T005, T021 |
| CA-06 | T005, T017, T020 |
| CA-07 | T005, T018, T020 |
