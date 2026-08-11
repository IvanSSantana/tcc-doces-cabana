# Tarefas — Revisão técnica da base

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

**Específico desta feature:** os blocos A, B, C e E tocam arquivos disjuntos e
podem ser reordenados entre si. O **bloco D (nomenclatura) é o último**, sempre —
renomear arquivo que outro bloco está editando produz conflito e diff ilegível.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `002-revisao-tecnica` a partir de `main`.
- [x] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado inicial
      aqui: **99 testes, 0 falhas, 6 avisos NU1903**. É a linha de base contra a
      qual T046 e T047 comparam.

---

## Fase 2 — Bloco A: autenticação

*RF-01 (login por CPF), RF-02 (bloqueio), RF-04/05/06 (recuperação de senha).*

### Testes — devem falhar

- [x] **T003** — `DocesCabana.Tests/Units/Services/UsuarioServiceLoginTests.cs`
      (criar): CPF sem pontuação autentica; CPF pontuado autentica; e-mail
      continua autenticando; login inexistente devolve `Failed`;
      `PasswordSignInAsync` é chamado com `lockoutOnFailure: true`.
      **Prova CA-01, CA-02, CA-03, CA-04.**
- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Helpers/CpfHelperTests.cs`
      (criar): dígito verificador válido e inválido, onze dígitos repetidos,
      formato com e sem pontuação, entrada vazia. **Prova RN-01.**
- [x] **T005** `[P]` — `DocesCabana.Tests/Units/Helpers/TelefoneHelperTests.cs`
      (criar): DDD válido e inválido, nono dígito presente e ausente, entrada
      com e sem pontuação.
- [x] **T006** `[P]` — `DocesCabana.Tests/Units/Validators/EsqueceuSenhaDTOValidatorTests.cs`
      (criar): login vazio, login malformado, e-mail válido, CPF válido.
      **Prova CA-09 na barreira de entrada.**
- [x] **T007** — `DocesCabana.Tests/Units/Controllers/AutenticacaoControllerTests.cs`:
      substituir os três testes de `EsqueceuSenha`. Login existente grava a
      confirmação em `TempData` e envia e-mail; login inexistente grava **a mesma
      string** e não envia; login malformado (`ModelState` inválido) devolve a
      view sem tocar em `IUsuarioService` — verificar com `Verify(..., Times.Never)`.
      **Prova CA-07, CA-08, CA-09.**
- [x] **T008** — Rodar `dotnet test` e confirmar que T003–T007 falham **pelo
      motivo certo** (asserção, não erro de compilação alheio). Registrar a
      contagem de falhas.

### Implementação

- [x] **T009** — `DocesCabana.Infrastructure/Identity/Services/UsuarioService.cs`:
      extrair `private async Task<Usuario?> ResolverUsuario(string login)` que
      tenta `FindByEmailAsync` e, na ausência, busca por CPF normalizado.
      `BuscarPorLogin` e `RealizarLogin` passam a consumi-lo. `RealizarLogin`
      autentica com `usuario.Email!` — **nunca com o `login` cru** — e passa
      `lockoutOnFailure: true`. Não mutar o parâmetro `login`.
- [x] **T010** — `DocesCabana.Infrastructure/DependencyInjections/IdentityDependencyInjection.cs`:
      `options.Lockout.MaxFailedAccessAttempts = 5`,
      `DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15)`,
      `AllowedForNewUsers = true`. **Implementa RN-02.**
- [x] **T011** — `DocesCabana.MVC/Controllers/AutenticacaoController.cs`: guarda
      `if (!ModelState.IsValid) return View(dto);` no POST de `EsqueceuSenha`;
      mensagem neutra em `TempData["Confirmacao"]` nos dois caminhos, com o texto
      exato da RN-05; remover o campo `ILogger` injetado e nunca lido.
- [x] **T012** — `DocesCabana.MVC/Views/Autenticacao/EsqueceuSenha.cshtml`:
      exibir `TempData["Confirmacao"]` com classe de confirmação, não de erro.
- [x] **T013** — Rodar `dotnet test`: T003–T007 passam, e os testes que já
      existiam continuam passando.

---

## Fase 3 — Bloco B: domínio, produto e validação de entrada

*RF-03 (status preservado), RF-07 (validar antes de atribuir), RQ-06, RQ-11.*

### Testes — devem falhar

- [x] **T014** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`:
      acrescentar — status explícito é preservado; status omitido nasce `Ativo`;
      construtor recusado não deixa `ProdutoId` nem `Status` atribuídos.
      **Prova CA-05, CA-06, RN-03, RN-04.**
- [x] **T015** `[P]` — `DocesCabana.Tests/Units/Entities/UsuarioTests.cs`:
      acrescentar — celular inválido recusa a criação sem ter atribuído
      `UserName` nem `PhoneNumber`. **Prova CA-10.**
- [x] **T016** `[P]` — `DocesCabana.Tests/Units/Mappings/ProdutoMapperTests.cs`
      (criar): ida e volta entidade↔DTO preservando **todos** os campos, com
      atenção ao `Status`; lista vazia; `PromocaoId` nulo.
- [x] **T017** `[P]` — `DocesCabana.Tests/Units/Mappings/UsuarioMapperTests.cs`
      (criar): `CadastroToEntity` normaliza CPF e celular para dígitos; `ToDTO`
      preserva os campos.
- [x] **T018** `[P]` — `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs`:
      acrescentar — `Cadastrar` chama `Adicionar` no repositório e devolve o DTO
      **mapeado da entidade** (com o `ProdutoId` gerado e o `Status` efetivo),
      não o DTO de entrada.
- [x] **T019** `[P]` — `DocesCabana.Tests/Units/Validators/ProdutoDTOValidatorTests.cs`
      (criar): um caso válido e um inválido por regra — nome vazio, nome com 2
      caracteres, preço zero, preço negativo, imagem vazia, imagem relativa,
      imagem com esquema não-http, subcategoria `Guid.Empty`.
- [x] **T020** `[P]` — `DocesCabana.Tests/Units/Validators/RedefinirSenhaDTOValidatorTests.cs`
      (criar): cada `RuleFor` de senha e a igualdade da confirmação.
- [x] **T021** — Rodar `dotnet test` e confirmar que T014–T020 falham pelo motivo
      certo.

### Implementação

- [x] **T022** — `DocesCabana.Domain/Entities/Produto.cs`: acrescentar o parâmetro
      `ProdutoStatus status = ProdutoStatus.Ativo` **antes** de `Guid id = default`;
      mover `ProdutoId` e `Status` para depois do bloco de validação, de modo que
      nenhuma atribuição preceda uma validação.
- [x] **T023** — `DocesCabana.Infrastructure/Identity/Usuario.cs`: mover
      `UserName` e `PhoneNumber` para depois do bloco de validação; promover a
      `Regex` de e-mail a `private static readonly` com `RegexOptions.Compiled`,
      como em `TelefoneHelper` (hoje é recompilada a cada construção); remover os
      comentários fósseis "2. DELEGADO PARA O HELPER" e "3. DELEGADO PARA O HELPER".
- [x] **T024** — `DocesCabana.Application/Mappings/ProdutoMapper.cs` e
      `Services/ProdutoService.cs`: `ToEntity` repassa `dto.Status`; `Cadastrar`
      devolve `ProdutoMapper.ToDTO(produto)`.
- [x] **T025** — `DocesCabana.Application/Validators/ProdutoDTOValidator.cs`
      (criar): espelha as invariantes de `Produto` com mensagens idênticas às do
      domínio. **Resolve a dívida D-06 da baseline.** Registro no contêiner é
      automático pelo assembly scan — nenhuma alteração de DI.
- [x] **T026** — Rodar `dotnet test`: T014–T020 passam.

---

## Fase 4 — Bloco C: remoção das transactions

*RQ-02.*

- [x] **T027** — `DocesCabana.Tests/Integration/DatabaseIntegrationTests.cs` e
      `Integration/Repositories/ProdutoRepositoryIntegrationTests.cs`: os dois
      testes de `IniciarTransacao` e os três usos de `ExecutarEmTransacao` saem.
      Entra um teste de atomicidade: duas entidades adicionadas, uma inválida
      para o banco, um `SalvarAlteracoes` que lança — nenhuma das duas persiste.
      É o comportamento que a transação explícita dava e que o
      `SaveChangesAsync` já dá sozinho.
- [x] **T028** — Remover `DocesCabana.Domain/Contracts/ITransaction.cs` e
      `DocesCabana.Infrastructure/Repositories/TransactionEf.cs`. Em
      `Contracts/IUnitOfWork.cs`, deixar apenas `SalvarAlteracoes` e retirar a
      herança de `IAsyncDisposable`. Em `Repositories/UnitOfWork.cs`, remover
      `IniciarTransacao`, `ExecutarEmTransacao` e `DisposeAsync` — este último
      descartava um `DbContext` de que o `UnitOfWork` não é dono.
- [x] **T029** — Rodar `dotnet test`: verde, e a contagem caiu em exatamente 2
      testes (os de transação explícita), não mais.

---

## Fase 5 — Bloco E: configuração, segredos e dependências

*RQ-01, RQ-07, RQ-08, RQ-09.*

- [x] **T030** — `DocesCabana.MVC/Helpers/DbInitializer.cs` e `Program.cs`:
      separar aplicar migrations de semear; semeadura só quando o ambiente não é
      produção. No `Program.cs`, usar `DbInitializer.Seed(...)` sem o namespace
      completo — o `using` já está no topo.
- [x] **T031** — `DocesCabana.Domain.csproj`: remover o `PackageReference` de
      `Microsoft.Extensions.Identity.Stores` (o projeto não tem um único
      `using Microsoft.*`). `DocesCabana.Infrastructure.csproj`:
      `PackageReference` explícito de `SQLitePCLRaw.lib.e_sqlite3` na **menor**
      versão sem advisory. Verificar com `dotnet list package --vulnerable
      --include-transitive` — a saída deve listar zero pacotes vulneráveis — e
      rodar a suíte de integração, que exercita SQLite de verdade.
      *Não mexer em `FluentValidation.AspNetCore`: 11.3.1 é a última versão
      publicada, o pacote foi descontinuado.*
- [x] **T032** — `git rm --cached DocesCabana.MVC/appsettings.json` (o arquivo
      local permanece intacto com seus placeholders; o `.gitignore`, que já o
      lista, volta a valer). Criar `DocesCabana.MVC/appsettings.Example.json`
      versionado, mesma estrutura, `EmailSettings` com valores vazios. Documentar
      no `README.md`: copiar o exemplo e usar *user secrets* para credenciais
      reais. **Validar clonando o repositório numa pasta temporária e subindo a
      aplicação — prova CA-11.**
- [x] **T033** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/ProdutoConfiguration.cs`:
      remover **apenas** `HasColumnType("INTEGER")` do `Status` — é o único ponto
      que quebra no SQL Server, e como `ProdutoStatus` é `enum : byte` o SQLite
      já gera `INTEGER` sozinho. **Manter** o `HasColumnType("decimal(18,2)")` do
      `Preco`: é válido nos dois providers e dá afinidade `NUMERIC` no SQLite
      (ver plano §5 e §7). **Verificar que o esquema não mudou:**
      `dotnet ef migrations add VerificacaoDialeto --project
      DocesCabana.Infrastructure --startup-project DocesCabana.MVC`, conferir que
      `Up` e `Down` saíram vazios, e então `dotnet ef migrations remove`. Se a
      migration **não** sair vazia, parar e reavaliar antes de seguir.
- [x] **T034** — Rodar `dotnet test`: verde.

---

## Fase 6 — Bloco D: nomenclatura

*RQ-03, RQ-04, RQ-05. Roda por último, sobre suíte verde. Nenhuma tarefa deste
bloco muda comportamento — se um teste quebrar aqui, a renomeação está errada.*

- [ ] **T035** — Inventário antes de renomear: buscar `\.Id\b` e `Telefone` em
      `DocesCabana.MVC/Views/**/*.cshtml` e nos `ViewComponents`. Razor resolve
      propriedade em tempo de compilação de view, então uma referência esquecida
      pode não quebrar o `dotnet build`. Listar as ocorrências aqui antes de T036.
- [ ] **T036** — `DocesCabana.Application/DTOs/ProdutoDTO.cs`: `Id` → `ProdutoId`
      (espelha a entidade); `EstaFavorito` passa de `set` para `init`. Atualizar
      `ProdutoMapper`, as views inventariadas em T035 e os testes afetados.
- [ ] **T037** — `DocesCabana.Application/DTOs/Autenticacao/CadastroDTO.cs`:
      `Telefone` → `Celular`, alinhando com `UsuarioDTO`, com a entidade e com o
      `.dbml`. Atualizar `CadastroDTOValidator`, `UsuarioMapper`,
      `Views/Autenticacao/Cadastro.cshtml` e `CadastroDTOValidatorTests`.
- [ ] **T038** `[P]` — Mover `DTOs/EsqueceuSenhaDTO.cs` e `DTOs/RedefinirSenhaDTO.cs`
      para `DTOs/Autenticacao/` — o namespace que ambos já declaram. Remover a
      propriedade `Id` de cada um; nenhuma é lida em lugar nenhum.
- [ ] **T039** `[P]` — Renomear `Validators/EsqueceuSenhaValidator.cs` para
      `EsqueceuSenhaDTOValidator.cs`, o nome do tipo que ele declara. Extrair
      `ValidarEmailOuCpf` — hoje duplicado byte a byte entre este validator e
      `LoginDTOValidator` — para um único ponto e consumir nos dois.
- [ ] **T040** `[P]` — `DocesCabana.Infrastructure/DependencyInjections/DbContextDependencyInjection.cs`:
      renomear a classe `DatabaseConfig` para `DbContextDependencyInjection`,
      alinhando com o nome do arquivo e com os outros três módulos de DI.
- [ ] **T041** — Renomear ~40 testes para `Dado_/Quando_/Entao_` em
      `Units/Controllers/AutenticacaoControllerTests.cs`,
      `Units/Controllers/HomeControllerTests.cs` e
      `Units/Services/UsuarioServiceTests.cs`. **Só o nome muda** — corpo e
      asserções ficam idênticos.
- [ ] **T042** — Remover `using` não utilizados: `Microsoft.AspNetCore.Authorization`
      em `HomeController.cs`, `Microsoft.Data.Sqlite` em `DatabaseIntegrationTests.cs`,
      `System.Threading.Tasks` e `Xunit` em `InfraestruturaSqliteEmMemoria.cs`
      (ambos implícitos), e o que mais o compilador apontar.

---

## Fase 7 — Documentação e fechamento

- [ ] **T043** — `.specify/memory/constitution.md`: versão **1.1.0**. Princípio IV
      ganha "o nome do arquivo coincide com o nome do tipo que ele declara, e a
      pasta coincide com o namespace". Princípio VI perde a menção a limite de
      transação. Linha no histórico de emendas com data 2026-08-11 e motivo.
- [ ] **T044** `[P]` — `specs/000-baseline/spec.md`: §4.3 passa a dizer SQLite,
      com nota de que SQL Server é o alvo do deploy. §6 marca D-06 como resolvida
      pela `002` e D-01 a D-05 como endereçadas pela `001`; D-07 permanece aberta.
- [ ] **T045** `[P]` — `specs/README.md`: renumerar o backlog sugerido de 002–010
      para 003–011. *(A linha da `002` no índice já foi acrescentada junto com a
      criação da spec.)*
- [ ] **T046** — `dotnet test` inteiro verde. **Conferir: contagem maior que 99**
      (T029 removeu 2, as fases 2 e 3 acrescentam bem mais) e **zero testes fora
      do padrão `Dado_/Quando_/Entao_`** — verificar buscando `public.*Task\s+\w+_`
      que não comece por `Dado_`. **Prova CA-12.**
- [ ] **T047** — `dotnet build` sem nenhum aviso `NU1903`. **Prova CA-13.**
- [ ] **T048** — Fumaça manual, com a aplicação rodando: criar conta; **entrar com
      CPF sem pontuação** (CA-01); sair e **entrar com CPF pontuado** (CA-02);
      sair e entrar com e-mail (CA-03); errar a senha cinco vezes e confirmar o
      bloqueio na sexta (CA-04); solicitar redefinição com login existente e com
      inexistente, conferindo que a mensagem é idêntica e **aparece como
      confirmação** (CA-07, CA-08); enviar `abc` no campo de login e conferir o
      erro junto ao campo (CA-09); abrir a vitrine e conferir que os cards
      renderizam depois da renomeação de `ProdutoId`.
- [ ] **T049** — Preencher `checklist.md`; mudar o status da spec para
      *Implementada*; atualizar a linha da `002` em `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 — login por CPF | T003, T009, T048 |
| RF-02 — bloqueio por tentativas | T003, T009, T010, T048 |
| RF-03 — status preservado | T014, T016, T018, T022, T024 |
| RF-04 — confirmação não é erro | T007, T011, T012, T048 |
| RF-05 — recusa login malformado | T006, T007, T011, T048 |
| RF-06 — mensagem idêntica | T007, T011, T048 |
| RF-07 — validar antes de atribuir | T014, T015, T022, T023 |
| RQ-01 — dependências mortas | T031 |
| RQ-02 — remoção das transactions | T027, T028, T029, T043 |
| RQ-03 — arquivo, tipo e namespace | T035, T036, T038, T039, T040, T042, T043 |
| RQ-04 — um conceito, um nome | T037 |
| RQ-05 — nomenclatura de teste | T041, T046 |
| RQ-06 — validação de entrada do produto | T019, T025, T044 |
| RQ-07 — segredos fora do versionamento | T032 |
| RQ-08 — persistência sem dialeto | T033 |
| RQ-09 — sem vulnerabilidade conhecida | T031, T047 |
| RQ-10 — baseline fiel ao código | T044, T045 |
| RQ-11 — cobertura das lacunas | T004, T005, T016, T017, T018, T020 |
| RN-01 — CPF por dígito verificador | T004, T009 |
| RN-02 — 5 tentativas, 15 minutos | T010 |
| RN-03 — produto nasce Ativo | T014, T022 |
| RN-04 — objeto válido ou inexistente | T014, T015, T022, T023 |
| RN-05 — mensagem neutra | T007, T011 |
| CA-01 a CA-04 | T003, T009, T010, T048 |
| CA-05, CA-06 | T014, T022 |
| CA-07 a CA-09 | T006, T007, T011, T012, T048 |
| CA-10 | T015, T023 |
| CA-11 | T032 |
| CA-12 | T046 |
| CA-13 | T031, T047 |
