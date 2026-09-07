# Tarefas — Banco de dados no Postgres do Supabase

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`… — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Marque `[x]` só depois de `dotnet test` verde.

> **Três coisas não são negociáveis.**
>
> **A T003 vem antes de qualquer código.** Provar a conexão com o Supabase
> fora da aplicação, na mão, antes de trocar uma linha. Se o host direto não
> responder (IPv6), descobrir isso depois de já ter apagado as migrations é
> descobrir no pior momento possível. A `027` acertou nisso com a `T003` dela;
> repetir o acerto é de graça.
>
> **A imagem do contêiner tem a mesma versão maior do Postgres que o Supabase.**
> Sem isso a suíte valida um motor e a loja roda outro — que é **exatamente o
> defeito que esta entrega existe para fechar**, reintroduzido pela porta dos
> fundos e mais difícil de enxergar da segunda vez.
>
> **Teste de ordenação vermelho não se conserta trocando o valor esperado.**
> Quando um `Assert` de ordem alfabética falhar, a saída nova **não** é a
> resposta certa por ser a nova. Olhe a ordem, decida se ela é melhor para uma
> loja brasileira, e registre o motivo na T014. Atualizar a asserção no
> automático transforma a maior entrega de fidelidade desta spec em teatro.

---

## Fase 1 — Preparação, e a prova do serviço externo

- [x] **T001** — Criar branch `028-banco-postgres-supabase` a partir de `main` (com a `027` já mergeada).
- [x] **T002** — `dotnet build`: êxito, 2 avisos (os `NU1903` que a Fase 6 deve derrubar). Números de partida: `DocesCabana.Tests` 679 testes / 5s no total, dos quais `Integration/` é 60 testes / 1s. `DocesCabana.Tests.E2E` (`Categoria!=Externo`) é 185 testes / ~6min22s — medido na sessão da `027`, não refeito aqui por não tocar banco novo ainda.
- [x] **T003** — **Confirmado exatamente o que o plano previu.** `db.mjnlzsucdsxqahabsniy.supabase.co` só tem registro `AAAA` (IPv6 puro, sem `A`) e a conexão TCP na porta 5432 deu timeout desta máquina. O pooler em modo Session (`aws-0-us-east-2.pooler.supabase.com:5432`, `SSL Mode=Require`, usuário `postgres.mjnlzsucdsxqahabsniy`) conecta. `select version()`: **PostgreSQL 17.6** — decide a imagem da T011 (`postgres:17-alpine`). Connection string em *user secrets* (`ConnectionStrings:DefaultConnection`), ao lado de `SupabaseSettings:ChaveDeServico`.

## Fase 2 — O provider

> Ao fim desta fase a **aplicação roda no Postgres e a suíte não compila** —
> os projetos de teste ainda referenciam o SQLite. É esperado, e é por isso que
> a Fase 2 não tem tarefa de "rodar a suíte": ela volta a rodar na Fase 3.

- [x] **T004** — `DocesCabana.Infrastructure.csproj`: `Microsoft.EntityFrameworkCore.Sqlite` sai, `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0` entra (versão estável, resolvida pelo próprio NuGet — não pré-lançamento). Removido o pin de `SQLitePCLRaw.lib.e_sqlite3`.
- [x] **T005** — `DependencyInjections/DbContextDependencyInjection.cs`: `UseSqlite` → `UseNpgsql`.
- [x] **T006** — `Migrations/`: as 14 antigas apagadas; `InitialCreatePostgres` gerada com `dotnet ef migrations add` — **não precisou de conexão viva**, só do modelo. Conferido no arquivo: `Guid` → `uuid`, `decimal(18,2)`/`decimal(10,3)` → `numeric(18,2)`/`numeric(10,3)`, enums de byte → `smallint`, datas → `timestamp with time zone`. Zero `migrationBuilder.Sql` sobrevivente.
- [x] **T007** — `appsettings.Example.json`: connection string no formato Npgsql, com host/usuário como placeholder e **senha em branco** (RF-03, CA-03). A real vai para *user secrets* em `ConnectionStrings:DefaultConnection` — ainda não configurada (depende da T003). `appsettings.json` segue fora do versionamento; localmente ajustado para um Postgres de rascunho só para a T006 gerar a migration sem erro de parse.
- [x] **T008** — Subida a aplicação apontada para o Supabase (schema `public` limpo antes, para partir de banco vazio de verdade): `InitialCreatePostgres` aplicou sem erro, o administrador foi semeado, a home e o `/Catalogo` abriram com **99 produtos** e as imagens do Supabase em rodízio (RF-04/RF-05, CA-02). Achado no caminho, sem relação com o Postgres: duas instâncias concorrentes do `dotnet run` (sobra de sessões anteriores não encerradas) colidiram tentando aplicar a mesma migration ao mesmo tempo (`relation "Categoria" already exists"`) — resolvido matando os processos e recriando o schema do zero antes de rodar uma instância só.

## Fase 3 — A suíte de integração

- [x] **T009** — `DocesCabana.Tests/Units/Infraestrutura/MensagemDePostgresIndisponivelTests.cs`: a mensagem de pré-requisito ausente cita **o que ligar** e **que a suíte não usa banco da máquina nem do Supabase** (RF-09, CA-07). Viu falhar (`CS0103`, tipo não existe), depois `MensagemDePostgresIndisponivel.Montar` implementado em `Integration/` (perto de quem vai consumir, `PostgresDeTeste`). 3 testes verdes.
- [x] **T010** — `DocesCabana.Tests.csproj`: `Microsoft.EntityFrameworkCore.Sqlite` saiu; `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0` e `Testcontainers.PostgreSql 4.15.0` (versão resolvida pelo NuGet, fixada) entraram.
- [x] **T011** — `DocesCabana.Tests/Integration/PostgresDeTeste.cs`, imagem `postgres:17-alpine`. **Desvio deliberado do desenho original, registrado aqui:** não é um `ICollectionFixture` do xUnit — isso obrigaria as 60 classes de teste a ganhar `[Collection(...)]` e injeção de construtor, contra a promessa do plano §5 ("nenhum dos 60 testes precisa ser reescrito"). É um singleton estático, iniciado sob demanda e protegido por semáforo contra corrida entre classes rodando em paralelo; o Ryuk do próprio Testcontainers derruba o contêiner ao fim do processo. **Achado rodando com o Docker desligado de verdade:** o Testcontainers valida o endpoint do Docker já no `.Build()`, não só no `.StartAsync()` — o primeiro `try/catch` só cobria o segundo, e o teste da T009 não disparava no caso mais comum. Corrigido antes de seguir.
- [x] **T012** — `Integration/InfraestruturaSqliteEmMemoria.cs` → `InfraestruturaPostgresDescartavel.cs`. Os três helpers de semeadura não mudaram de assinatura. A referência ao nome antigo nas 10 classes de teste que herdam da base (mecânica, `sed` num nome de tipo — nenhuma asserção nem lógica tocada) e no comentário de `PedidoServiceTests.cs`.
- [x] **T013** — `dotnet test DocesCabana.Tests`: **682/682 em 14s** (30s na primeira execução, com o pull da imagem). Um vermelho apareceu no meio do caminho e foi corrigido antes de fechar a fase — ver nota abaixo. Nenhum teste de ordenação (Fase 4) ficou vermelho.

> ⚠️ **Achado da T013, fora do que a Fase 4 antecipava:** `CatalogoRepositoryIntegrationTests` tinha um `ExecuteSqlRawAsync("UPDATE Produto SET NomeNormalizado = '' WHERE ProdutoId = {0}", ...)` sem aspas nos identificadores — mesma classe de defeito que o plano já previa para os quatro acessos crus do E2E (T018), só que num teste da própria suíte de integração, que a varredura anterior não tinha olhado. Corrigido com aspas duplas nos identificadores.

## Fase 4 — O que a ordenação revelar

- [x] **T014** — **Nenhum teste de ordenação ficou vermelho** — inclusive `Dado_ProdutosComAcentoDigitoEMaiuscula_...` que afirma `"100% Cacau"` em primeiro (`CatalogoRepositoryIntegrationTests`). A imagem `postgres:17-alpine` usa collation `C` (ordenação por ponto de código) por padrão, que para os dados testados coincide com a ordem que o SQLite já dava. Não há asserção para revisar nem collation para fixar — a regra não negociável não chegou a ser exercitada porque nada divergiu. Registrado aqui como achado válido, não como tarefa pulada.
- [x] **T015** — `dotnet test DocesCabana.Tests`: unidade e integração verdes (682/682).

## Fase 5 — A ponta a ponta

- [x] **T016** — `DocesCabana.Tests.E2E.csproj`: `Npgsql 10.0.3` (só o driver — este projeto não usa EF Core) e `Testcontainers.PostgreSql 4.15.0` entram; `Microsoft.Data.Sqlite` sai. Comentário atualizado.
- [x] **T017** — `Infraestrutura/AplicacaoEmExecucao.cs`: sobe um contêiner Postgres (`postgres:17-alpine`) só para a coleção inteira — o E2E já compartilha uma única instância da aplicação (`ColecaoE2E`), não precisa do isolamento por teste que a `PostgresDeTeste` dá à suíte de integração. `CaminhoDoBanco` virou `ConexaoDoBanco`, entregue ao processo filho por `ConnectionStrings__DefaultConnection` — a mesma linha que apontava para o arquivo SQLite. Mesmo achado da T011 (validação no `.Build()`, não só no `.StartAsync()`) corrigido aqui também.
- [x] **T018** `[P]` — `Fluxos/CarrinhoTests.cs` (1 acesso) e `Fluxos/PaginaInicialTests.cs` (3 acessos): `SqliteConnection` → `NpgsqlConnection`, com `Aplicacao.ConexaoDoBanco`. O `UPPER()` dos dois lados sumiu; identificadores entre aspas; parâmetros `$nome` → `@nome`.
- [x] **T019** — `dotnet test DocesCabana.Tests.E2E --filter "Categoria!=Externo"`: **185/185, 5min44s** (era ~6min22s na T002 — dentro do esperado; a maior parte do tempo é navegador, não banco).

## Fase 6 — Varredura

- [x] **T020** — Varredura completa. Especs `001`–`023` (histórico do que era verdade então) deixadas como estão de propósito — não são o alvo desta tarefa. Vivos, corrigidos: `constitution.md` (Princípio V, emenda 1.4.1 → 1.4.2, PATCH), `plan-template.md`, `README.md` raiz (instruções de user secrets agora incluem a connection string), `DocesCabana.Tests.E2E/README.md`, comentários em `Produto.cs`, `ProdutoConfiguration.cs` (dois, incluindo o que previa esta troca desde a `002`), `ProdutoRepository.cs`, `DbInitializer.cs` (quatro pontos, incluindo duas referências ao nome antigo da classe de teste que o `sed` da T012 não pegou por estarem fora da lista de arquivos), `TextoHelperTests.cs`, `CatalogoRepositoryIntegrationTests.cs` (o comentário do teste de curinga — achado que só existia por eu ter conferido, e não assumido, que o EF Core escapa `%`/`_` também no Postgres). `docs/arquitetura.md` ganhou a subseção 6.14. `.gitignore` perdeu as três entradas de `docescabana.db*`, que não batem com nada desde que o SQLite saiu.
- [x] **T021** `[P]` — Confirmado: `dotnet build` da solução inteira dá **0 Aviso(s), 0 Erro(s)**. O `NU1903` some pela primeira vez desde a `020`.
- [x] **T022** — `docs/arquitetura.md`: §6.14 nova (por que contêiner e não o banco da loja; por que o `UPPER()` sumiu; por que `PostgresDeTeste` não é `ICollectionFixture`; o achado do `.Build()` vs `.StartAsync()`), mais os pontos ao longo do §6.5, §6.9 e §7.8 corrigidos na T020.

## Fase 7 — Fechamento

- [x] **T023** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"`, restritos aos arquivos tocados por esta entrega. Todas as referências corretas — nenhuma estranha.
- [x] **T024** — Preenchido `checklist.md`.
- [x] **T025** — `specs/README.md`: linha da feature, "Ordem executada" e parágrafo do resumo atualizados. A observação de que a `028` fecha o par da `027` já estava escrita desde a criação da spec (§"A cadeia da loja").
- [x] **T026** `[P]` — `specs/000-baseline/spec.md`: nenhuma das dívidas D-01..D-07 é sobre o provider de banco — nada para riscar na tabela. Mas §4.3 ("Infraestrutura — pronto") descrevia SQLite como atual e SQL Server como alvo futuro do deploy — desatualizado agora. Atualizado para Postgres, seguindo o precedente que a própria `002` deixou no mesmo parágrafo.
- [x] **T027** — `dotnet build`: **0 Aviso(s), 0 Erro(s)**. `dotnet test DocesCabana.Tests`: **682/682**. `dotnet test DocesCabana.Tests.E2E --filter "Categoria!=Externo"`: **185/185** (T019). Nenhum dos dois projetos de teste lê `ConnectionStrings:DefaultConnection` do `appsettings`/user secrets da MVC — cada um sobe seu próprio contêiner Postgres, independente de qualquer credencial configurada na máquina. É garantia estrutural (arquitetura dos fixtures), não só resultado observado desta execução — verificado que nenhuma classe de teste referencia `IConfiguration` da MVC (CA-04, RF-06).
- [x] **T028** — Percorrido contra o Postgres real do Supabase (não um contêiner de teste): home, catálogo, filtro por subcategoria, ordenação, busca por termo — inclusive **sem acento** ("cachaca" achou "Cachaça", 7 produtos, confirmando `NomeNormalizado` intacto no motor novo — RN-02 da `016`) —, página de detalhe do produto, conta, endereços, meus pedidos (a compra semeada aparece como "Entregue"). Login, favoritar (escrita + releitura) e carrinho (escrita + releitura) testados de ponta a ponta com `INSERT` de verdade. Nada mudou de comportamento (RN-01, CA-01). Os efeitos colaterais da verificação (favorito, item de carrinho) foram desfeitos depois.
- [x] **T029** — `spec.md` e `plan.md`: status "Implementada"/"Implementado". `specs/README.md` já atualizado na T025. **Nada ficou pendente** — as 29 tarefas fecharam, incluindo a manual (T028) e a verificação de que build e as duas suítes seguem verdes do zero (T027). Não há débito registrado para o backlog.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T004, T005, T007, T008 |
| RF-02 | T028 |
| RF-03 | T007 |
| RF-04 | T006, T008 |
| RF-05 | T008 |
| RF-06 | T011, T017, T027 |
| RF-07 | T011, T012 |
| RF-08 | T012, T013 |
| RF-09 | T009, T011 |
| RN-01 | T028 |
| RN-02 | T027 |
| RN-03 | T007 |
| RN-04 | T011, T012, T014 |
| RN-05 | T009, T011 |
| CA-01 | T028 |
| CA-02 | T008 |
| CA-03 | T007 |
| CA-04 | T027 |
| CA-05 | T011, T012 |
| CA-06 | T012, T013 |
| CA-07 | T009 |
| CA-08 | T014 |
