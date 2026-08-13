# Tarefas — Separar pessoa de credencial

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

**Específico desta feature:** o **bloco B (renomeação) é commit isolado**, sem
nenhuma mudança de comportamento junto. Renomear 21 arquivos e mexer em lógica
no mesmo diff é o jeito mais rápido de esconder uma quebra.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `004-separar-pessoa-de-credencial` a partir de
      `main` (com `001` e `003` já integradas).
- [ ] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado
      inicial: **233 testes, 0 falhas, 0 avisos**. É a linha de base contra a
      qual a T040 compara.

---

## Fase 2 — Bloco A: as duas classes

### Testes — devem falhar

- [x] **T003** `[P]` — `DocesCabana.Tests/Units/Entities/UsuarioTests.cs`:
      reescrever para o `Usuario` do **domínio**. Nome vazio; CPF inválido,
      com dígitos repetidos e pontuado (deve normalizar); celular inválido e
      pontuado (deve normalizar); data futura e anterior a 120 anos;
      `UsuarioId` vazio. **Os casos de e-mail saem daqui** — vão para a T004.
      **Prova RN-01 a RN-04.**
- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Entities/ContaDeAcessoTests.cs`
      (criar): e-mail vazio, e-mail malformado, e-mail válido. São os casos que
      saíram da T003. **Prova RN-06.**
- [x] **T005** — Rodar `dotnet test` e confirmar que T003–T004 falham por
      ausência de tipo, não por outro motivo.

### Implementação

- [x] **T006** — `DocesCabana.Domain/Entities/Usuario.cs` (criar): `private set`,
      construtor validante que **normaliza CPF e celular para dígitos** usando
      `CpfHelper` e `TelefoneHelper`, `protected Ctor()`, e
      `AtualizarDados(nome, celular, dataNascimento)`.
- [x] **T007** — `DocesCabana.Infrastructure/Identity/Usuario.cs` → renomear
      arquivo e classe para `ContaDeAcesso.cs` / `ContaDeAcesso`. Remover
      `Nome`, `CPF` e `DataNascimento`; **manter** a validação de e-mail
      (`EmailRegex`) e o construtor `ContaDeAcesso(string email)`; acrescentar a
      navegação `Usuario? Usuario` — infraestrutura pode referenciar domínio.
- [x] **T008** — Rodar `dotnet test`: T003–T004 passam. O resto do projeto
      ainda não compila — é esperado, o bloco B conserta.

---

## Fase 3 — Bloco B: renomeação, sem mudança de comportamento

*Um commit só, mecânico. Se algum teste mudar de resultado aqui, a renomeação
está errada.*

- [x] **T009** — Trocar `Usuario` por `ContaDeAcesso` onde o tipo referido é o
      do Identity, nos 16 pontos de `UserManager<>`, `SignInManager<>` e
      `IdentityDbContext<>`:
      `Infrastructure/DatabaseContext/DocesCabanaDbContext.cs`,
      `DependencyInjections/IdentityDependencyInjection.cs`,
      `Identity/Services/{IUsuarioService,UsuarioService}.cs`,
      `Identity/Mappings/UsuarioMapper.cs`,
      `MVC/Controllers/AutenticacaoController.cs`,
      `MVC/Helpers/DbInitializer.cs`.
- [x] **T010** — Trocar o mesmo tipo nos testes:
      `Units/Services/{UsuarioServiceTests,UsuarioServiceLoginTests}.cs`,
      `Units/Mappings/UsuarioMapperTests.cs`,
      `Units/Controllers/AutenticacaoControllerTests.cs`,
      `Integration/{InfraestruturaSqliteEmMemoria,DatabaseIntegrationTests}.cs`.
- [x] **T011** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/`:
      renomear `UsuarioConfiguration.cs` para `ContaDeAcessoConfiguration.cs`,
      apontando para `ContaDeAcesso` e mantendo, por ora, só o `ToTable`.
- [x] **T012** — Rodar `dotnet build`. Ainda haverá erro nos pontos que
      dependem de `Nome`/`CPF`/`DataNascimento` na conta — mapear quais são
      antes de seguir; são exatamente os que o bloco C resolve.

---

## Fase 4 — Bloco C: o serviço compõe as duas metades

### Testes — devem falhar

- [x] **T013** `[P]` — `DocesCabana.Tests/Units/Services/UsuarioServiceCadastroTests.cs`
      (criar): cadastro válido cria as duas metades com o **mesmo** `Guid`; e —
      o caso que importa — quando a gravação do `Usuario` falha, a conta já
      criada é **apagada** via `UserManager.DeleteAsync`.
      **Prova RN-08, CA-01, CA-04.**
- [x] **T014** `[P]` — `DocesCabana.Tests/Units/Mappings/UsuarioMapperTests.cs`:
      ajustar para a assinatura nova — o DTO traz e-mail da `ContaDeAcesso` e
      nome, CPF, celular e nascimento do `Usuario`.
- [x] **T015** `[P]` — `DocesCabana.Tests/Units/Services/UsuarioServiceLoginTests.cs`:
      ajustar o caminho de CPF para passar pelo `IUsuarioRepository` em vez de
      `_userManager.Users`. **Preserva CA-02 e CA-03.**
- [x] **T016** — Rodar `dotnet test` e confirmar o vermelho pelo motivo certo.

### Implementação

- [x] **T017** `[P]` — `DocesCabana.Application/Contracts/Repositories/IUsuarioRepository.cs`
      (criar): `IRepository<Usuario>` mais `Task<Usuario?> BuscarPorCpf(string cpf)`.
- [x] **T018** `[P]` — `DocesCabana.Infrastructure/Repositories/UsuarioRepository.cs`
      (criar).
- [x] **T019** — `DocesCabana.Infrastructure/Identity/Mappings/UsuarioMapper.cs`:
      `ToDTO(Usuario usuario, ContaDeAcesso conta)`. `CadastroToEntity` deixa de
      existir na forma atual — o serviço passa a construir as duas metades.
- [x] **T020** — `DocesCabana.Infrastructure/Identity/Services/UsuarioService.cs`:
      - `CadastrarUsuario`: cria a `ContaDeAcesso`, depois o `Usuario`, e
        **compensa com `DeleteAsync` se a segunda metade falhar** (plano §4).
      - `BuscarPorLogin` e `ResolverUsuario`: CPF passa a vir do
        `IUsuarioRepository`; e-mail continua no `UserManager`.
      - `BuscarUsuarioPorId` e `AlterarDadosUsuario`: leem e gravam o `Usuario`
        do domínio; a gravação usa `IUnitOfWork`.
      - Injetar `IUsuarioRepository` e `IUnitOfWork`.
- [x] **T021** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`:
      registrar `IUsuarioRepository`.
- [x] **T022** — Rodar `dotnet test`: T013–T015 passam.

---

## Fase 5 — Bloco D: a navegação que motivou a spec

### Testes — devem falhar

- [ ] **T023** — `DocesCabana.Tests/Integration/Repositories/ModeloDeDadosIntegrationTests.cs`:
      acrescentar — `Endereco` consultado com `Include(e => e.Usuario)` traz o
      nome do usuário; consultado sem `Include`, a navegação vem `null`.
      **Prova CA-05.**
- [ ] **T024** — Rodar `dotnet test` e confirmar o vermelho.

### Implementação

- [ ] **T025** `[P]` — `DocesCabana.Domain/Entities/{Endereco,Favorito,Avaliacao,Pedido}.cs`:
      acrescentar `Usuario? Usuario` sobre o `UsuarioId` que já existe. Remover
      o comentário que explicava por que a navegação não existia — ele deixa de
      ser verdade. **Encerra a RQ-02 da spec `003`.**
- [ ] **T026** `[P]` — `.../Configurations/{Endereco,Favorito,Avaliacao,Pedido}Configuration.cs`:
      `HasOne<Usuario>()` (sem navegação, apontando para o Identity) vira
      `HasOne(x => x.Usuario)` apontando para o domínio.
- [ ] **T027** — `.../Configurations/UsuarioConfiguration.cs`: reescrever para o
      `Usuario` do domínio — tabela `Usuario`, `Nome` (255), `CPF` (11) com
      índice único, `Celular` (11), `DataNascimento` como `date`, e a chave
      estrangeira 1:1 para `ContaDeAcesso` por chave compartilhada
      (`HasOne<ContaDeAcesso>().WithOne(c => c.Usuario).HasForeignKey<Usuario>(u => u.UsuarioId)`).
- [ ] **T028** — `DocesCabana.Infrastructure/DatabaseContext/DocesCabanaDbContext.cs`:
      `DbSet<Usuario>` do domínio.
- [ ] **T029** — Rodar `dotnet test`: T023 passa.

---

## Fase 6 — Bloco E: persistência, massa inicial e fechamento

- [ ] **T030** — **Apagar o banco local** `DocesCabana.MVC/docescabana.db` (e os
      arquivos `-shm`/`-wal`). As contas existentes são perdidas — esperado e
      documentado (plano §5); o administrador é recriado na subida.
- [ ] **T031** — Criar a migration:
      `dotnet ef migrations add SepararPessoaDeCredencial --project
      DocesCabana.Infrastructure --startup-project DocesCabana.MVC`.
      Conferir no arquivo gerado que a tabela `ContaDeAcesso` existe, que a
      tabela `Usuario` tem só as cinco colunas do domínio, e que as chaves
      estrangeiras de `Endereco`, `Favorito`, `Avaliacao` e `Pedido` apontam
      para `Usuario`, não para `ContaDeAcesso`.
- [ ] **T032** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: o administrador
      semeado passa a ser criado em duas metades, com o mesmo `Guid`.
- [ ] **T033** — `DocesCabana.Tests/Integration/InfraestruturaSqliteEmMemoria.cs`:
      `SemearUsuario` cria as duas metades e devolve o `Guid` compartilhado.
      Ajustar `DatabaseIntegrationTests` para o CPF único vir do domínio.
- [ ] **T034** — Busca textual por `PhoneNumber` em todo o projeto: nenhuma
      escrita nem leitura deve sobrar fora do que o Identity faz internamente.
      **Prova RQ-07.**
- [ ] **T035** — Rodar `dotnet test`: verde.

---

## Fase 7 — Documentação e fechamento

- [ ] **T036** — `.specify/memory/constitution.md`: versão **1.2.0**. Reescrever
      a exceção do Princípio I — o motivo passa a ser a dependência de
      `UserManager`/`SignInManager`, e não mais a herança da entidade — e
      registrar que entidades de domínio referenciam `Usuario` por navegação
      normal. Linha no histórico com data 2026-08-12 e motivo.
- [ ] **T037** `[P]` — `specs/003-modelo-de-dados-completo/spec.md`: anotar na
      RQ-02 que a limitação foi encerrada por esta spec.
- [ ] **T038** `[P]` — `ModelagemBancoTCC.dbml`: acrescentar `ContaDeAcesso` e
      deixar `Usuario` com nome, CPF, celular e nascimento — as duas metades,
      como o banco passou a ser.
- [ ] **T039** `[P]` — `specs/README.md`: status da `004` para *Implementada*.
- [ ] **T040** — `dotnet build` sem avisos novos e `dotnet test` verde, com
      contagem maior ou igual aos 233 da T002. **Prova CA-07.**
- [ ] **T041** — Fumaça manual, com a aplicação rodando: criar conta; entrar
      com e-mail (CA-02); sair e entrar com CPF, com e sem pontuação (CA-03);
      solicitar redefinição de senha e concluir com a senha nova (CA-06);
      tentar criar segunda conta com o **mesmo CPF** e outro e-mail, e depois
      confirmar que esse e-mail **não** entra no sistema — prova que a conta
      órfã foi desfeita (CA-04).
- [ ] **T042** — Preencher `checklist.md` e atualizar o status da spec.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 — cadastro sem regressão | T013, T020, T041 |
| RF-02 — login por e-mail e CPF | T015, T020, T041 |
| RF-03 — recusa duplicidade | T013, T020 |
| RF-04 — nenhuma metade órfã | T013, T020, T041 |
| RF-05 — redefinição de senha | T041 |
| RQ-01 — dado de negócio no domínio | T003, T006 |
| RQ-02 — conta só com credencial | T004, T007 |
| RQ-03 — domínio fica com o termo `Usuario` | T006, T007, T009, T010 |
| RQ-04 — navegação nas quatro entidades | T023, T025, T026 |
| RQ-05 — exceção do Princípio I reescrita | T036 |
| RQ-06 — uma migration | T031 |
| RQ-07 — `PhoneNumber` fora de uso | T034 |
| RQ-08 — invariantes seguem cobertas | T003, T004, T023 |
| RN-01 a RN-04 | T003, T006 |
| RN-05 — CPF único | T027, T033 |
| RN-06 — e-mail da conta | T004, T007 |
| RN-07 — 1:1 por chave compartilhada | T027, T031 |
| RN-08 — compensação | T013, T020 |
| CA-01 | T013, T041 |
| CA-02, CA-03 | T015, T041 |
| CA-04 | T013, T041 |
| CA-05 | T023, T025, T026 |
| CA-06 | T041 |
| CA-07 | T040 |
