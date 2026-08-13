# Tarefas — Gestão de administradores

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

**Específico desta feature:** a T003 muda a casa de uma constante usada por
`[Authorize(Roles = ...)]`. O **valor** da string não muda — só o lugar onde ela
mora. Se o valor mudar por acidente, a área administrativa fica inacessível para
todo mundo, inclusive para quem estiver testando.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `005-gestao-de-administradores` a partir de `main`
      (com a `004` já integrada).
- [x] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado
      inicial verde. É a linha de base da T020.

## Fase 2 — A constante de papel muda de casa

*Sem isto, um serviço da infraestrutura não consegue enxergar o nome do papel.*

- [x] **T003** — `DocesCabana.Domain/Papeis.cs` (criar):
      `public const string Administrador = "Administrador";` — **o mesmo valor
      literal de hoje**. Remover `PapelAdministrador` de
      `MVC/Helpers/DbInitializer.cs` e apontar `MVC/Controllers/AdminController.cs`
      e o próprio `DbInitializer` para `Papeis.Administrador`.
- [x] **T004** — Rodar `dotnet test`: verde, sem mudança de contagem. É
      refatoração pura.

## Fase 3 — Testes (devem falhar)

- [x] **T005** `[P]` — `Tests/Units/Services/AdministradorServiceTests.cs`
      (criar): `ListarAdministradores` compõe nome (do `Usuario`) e e-mail (da
      `ContaDeAcesso`) de cada administrador; `CadastrarAdministrador` repassa
      `Papeis.Administrador` para `CadastrarUsuario`. **Prova RF-01, RF-03, RN-04.**
- [x] **T006** `[P]` — `Tests/Units/Services/UsuarioServiceCadastroTests.cs`:
      acrescentar — com `papel` informado, `AddToRoleAsync` é chamado; se ele
      falhar, a conta criada é apagada. **Prova RN-05, CA-05.**
- [x] **T007** `[P]` — `Tests/Units/Controllers/AdministradorControllerTests.cs`
      (criar): `Index` devolve a lista; `Cadastro` POST com `ModelState`
      inválido devolve `ViewResult` e **não** chama o serviço; POST válido chama
      o serviço e devolve `RedirectToActionResult` com `TempData` preenchido.
      **Prova RF-06, RF-07, CA-02.**
- [x] **T008** — Rodar `dotnet test` e confirmar que T005–T007 falham pelo
      motivo esperado.

## Fase 4 — Serviço

- [x] **T009** — `Infrastructure/Identity/Services/IUsuarioService.cs` e
      `UsuarioService.cs`: acrescentar o parâmetro opcional
      `string? papel = null` a `CadastrarUsuario` e atribuir o papel **dentro**
      do bloco que a `004` já compensa (plano §4). Quem chama hoje não muda.
- [x] **T010** `[P]` — `Infrastructure/Identity/Services/IAdministradorService.cs`
      (criar).
- [x] **T011** — `Infrastructure/Identity/Services/AdministradorService.cs`
      (criar): `ListarAdministradores` usa `GetUsersInRoleAsync` e completa os
      nomes pelo `IUsuarioRepository`; `CadastrarAdministrador` delega a
      `CadastrarUsuario(dto, Papeis.Administrador)`.
- [x] **T012** — `Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`:
      registrar `IAdministradorService`.
- [x] **T013** — Rodar `dotnet test`: T005 e T006 passam.

## Fase 5 — Apresentação

- [x] **T014** — `MVC/Controllers/AdministradorController.cs` (criar):
      `[Authorize(Roles = Papeis.Administrador)]` na classe;
      `Index` lista; `Cadastro` GET devolve o formulário; `Cadastro` POST com
      `[ValidateAntiForgeryToken]`, guarda de `ModelState`, e no sucesso
      `TempData["Confirmacao"]` + `RedirectToAction(nameof(Index))`.
- [x] **T015** `[P]` — `MVC/Views/Administrador/Index.cshtml` (criar): tabela de
      nome e e-mail, mensagem de confirmação do `TempData`, link para o
      cadastro.
- [x] **T016** `[P]` — `MVC/Views/Administrador/Cadastro.cshtml` (criar):
      formulário espelhando `Views/Autenticacao/Cadastro.cshtml`, com
      `asp-validation-for` em cada campo e `_ValidationScriptsPartial`.
- [x] **T017** `[P]` — `MVC/wwwroot/css/pages/administradores.css` (criar).
- [x] **T018** — `MVC/Views/Shared/Components/Header/Default.cshtml`: link para a
      gestão dentro de `@if (User.IsInRole(Papeis.Administrador))`. **Prova RF-09.**
- [x] **T019** — Rodar `dotnet test`: T007 passa.

## Fase 6 — Fechamento

- [x] **T020** — `dotnet build` sem avisos novos e `dotnet test` verde, com
      contagem maior que a da T002.
- [ ] **T021** — Fumaça manual, com a aplicação rodando:
      - Entrar como o administrador semeado e abrir a gestão: ele consta na
        lista (**CA-01**).
      - Cadastrar um administrador novo: confirmação aparece e ele entra na
        lista (**CA-02**).
      - Sair, entrar com a conta recém-criada e abrir `/Admin/Cadastro`: nenhum
        acesso negado (**CA-03**).
      - Tentar cadastrar com e-mail já usado (**CA-04**) e com CPF já usado —
        neste, conferir depois que o e-mail da tentativa **não** entra no
        sistema (**CA-05**).
      - Senha "senha123": erro no campo Senha (**CA-06**).
      - Sair e acessar a gestão sem login: vai para o login (**CA-07**).
      - Entrar como cliente comum: acesso negado (**CA-08**) e nenhum link no
        cabeçalho (**CA-09**).
- [ ] **T022** — Preencher `checklist.md`.
- [ ] **T023** — Atualizar a spec para *Implementada* e a linha da `005` em
      `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 — listar | T005, T011, T014, T015 |
| RF-02 — formulário | T016 |
| RF-03 — nasce com acesso administrativo | T005, T009, T011 |
| RF-04 — mesmas regras do cadastro de cliente | T016 (reusa `CadastroDTOValidator`) |
| RF-05 — recusa e-mail ou CPF repetidos | T006, T009 |
| RF-06 — confirmação após sucesso | T007, T014, T015 |
| RF-07 — nada gravado com campo inválido | T007, T014 |
| RF-08 — acesso negado a quem não é admin | T014, T021 |
| RF-09 — link escondido | T018, T021 |
| RN-01, RN-02 | T016 (`CadastroDTO` e seu validator, já existentes) |
| RN-03 — e-mail e CPF únicos no sistema | T006, T009 |
| RN-04 — lista completa | T005, T011 |
| RN-05 — nada pela metade | T006, T009 |
| CA-01 | T005, T021 |
| CA-02 | T007, T021 |
| CA-03 | T021 |
| CA-04 | T006, T021 |
| CA-05 | T006, T021 |
| CA-06 | `CadastroDTOValidatorTests` (já existe), T021 |
| CA-07, CA-08, CA-09 | T021 |
