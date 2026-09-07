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

- [ ] **T001** — Criar branch `028-banco-postgres-supabase` a partir de `main` (com a `027` já mergeada).
- [ ] **T002** — Rodar `dotnet build` e as duas suítes; **registrar os números de partida**: quantidade de testes e duração de cada suíte. São eles que dizem, no fim, se o custo ficou dentro do aceitável (spec §10).
- [ ] **T003** — **Antes de qualquer código.** No painel do Supabase, obter a connection string do **pooler em modo Session** (`aws-0-<regiao>.pooler.supabase.com`, porta 5432, `SSL Mode=Require`) e provar a conexão fora da aplicação. Rodar `select version()` e **anotar a versão maior** — ela decide a imagem da T011. Se só o host direto (`db.<ref>.supabase.co`) estiver disponível e ele não responder, é IPv6: usar o pooler (plano §8).

## Fase 2 — O provider

> Ao fim desta fase a **aplicação roda no Postgres e a suíte não compila** —
> os projetos de teste ainda referenciam o SQLite. É esperado, e é por isso que
> a Fase 2 não tem tarefa de "rodar a suíte": ela volta a rodar na Fase 3.

- [ ] **T004** — `DocesCabana.Infrastructure.csproj`: `Microsoft.EntityFrameworkCore.Sqlite` sai, `Npgsql.EntityFrameworkCore.PostgreSQL` entra. Remover o pin de `SQLitePCLRaw.lib.e_sqlite3` — ele existia só pela vulnerabilidade da versão transitiva do SQLite.
- [ ] **T005** — `DependencyInjections/DbContextDependencyInjection.cs`: `UseSqlite` → `UseNpgsql`.
- [ ] **T006** — `Migrations/`: apagar as 14 migrations, seus `.Designer.cs` e o `DocesCabanaDbContextModelSnapshot.cs`; gerar `InitialCreatePostgres`. Conferir no arquivo gerado que `Guid` virou `uuid`, `decimal(18,2)` virou `numeric`, os enums de byte viraram `smallint` e as datas viraram `timestamp with time zone`.
- [ ] **T007** — `appsettings.Example.json`: connection string no formato Npgsql com **senha em branco** (RF-03, CA-03). A real vai para *user secrets* em `ConnectionStrings:DefaultConnection`, ao lado da `SupabaseSettings:ChaveDeServico` que a `027` já pôs lá. Conferir que `appsettings.json` segue fora do versionamento.
- [ ] **T008** — Subir a aplicação apontada para o Supabase e conferir: a migration aplica num banco vazio, a semeadura completa, e o catálogo abre com os cem produtos e suas imagens (RF-04/RF-05, CA-02).

## Fase 3 — A suíte de integração

- [ ] **T009** — `DocesCabana.Tests/Units/Infraestrutura/MensagemDePostgresIndisponivelTests.cs`: a mensagem de pré-requisito ausente cita **o que ligar** e **que a suíte não usa banco da máquina nem do Supabase** (RF-09, CA-07). Ver falhar. É o único comportamento novo da entrega, e o único que tem ciclo vermelho-verde de verdade (plano §11) — por isso a formatação da mensagem vive num método próprio, testável sem desligar o Docker.
- [ ] **T010** — `DocesCabana.Tests.csproj`: provider trocado, `Testcontainers.PostgreSql` adicionado.
- [ ] **T011** — `DocesCabana.Tests/Integration/PostgresDeTeste.cs`: **novo**. Fixture de coleção que sobe um contêiner por execução, com a imagem **fixada na versão anotada na T003**. Captura a falha de daemon ausente e relança com a mensagem da T009.
- [ ] **T012** — `Integration/InfraestruturaSqliteEmMemoria.cs` → `InfraestruturaPostgresDescartavel.cs`: um banco novo por teste dentro do contêiner compartilhado (plano §5). Os três helpers de semeadura (`SemearSubcategoria`, `SemearUsuario`, `SemearAvaliacao`) não mudam de assinatura — se mudarem, o isolamento escolhido foi o errado.
- [ ] **T013** — Rodar `dotnet test DocesCabana.Tests`. **Registrar a duração** e listar o que ficou vermelho. Nenhum dos 60 testes de integração deveria precisar de reescrita; os vermelhos esperados são de ordenação, e são assunto da Fase 4.

## Fase 4 — O que a ordenação revelar

- [ ] **T014** — Para cada teste de ordenação vermelho (ao menos `CatalogoRepositoryIntegrationTests`, que afirma `"100% Cacau"` em primeiro): olhar a ordem nova, decidir se ela é **melhor para uma loja brasileira**, e então atualizar a asserção **com o motivo escrito no teste** — ou, se for pior, fixar a collation explicitamente. Ver a terceira regra não negociável no topo. Registrar a decisão aqui, nesta linha, seja qual for.
- [ ] **T015** — Rodar `dotnet test DocesCabana.Tests`: unidade e integração verdes.

## Fase 5 — A ponta a ponta

- [ ] **T016** — `DocesCabana.Tests.E2E.csproj`: `Npgsql` (só o driver — este projeto não usa EF Core) e `Testcontainers.PostgreSql` entram; `Microsoft.Data.Sqlite` sai. Atualizar o comentário que explica por que o driver está ali.
- [ ] **T017** — `Infraestrutura/AplicacaoEmExecucao.cs`: sobe um contêiner para a coleção; `CaminhoDoBanco` vira `ConexaoDoBanco` e é entregue ao processo filho por `ConnectionStrings__DefaultConnection` — **a mesma linha que hoje aponta para o arquivo SQLite**. Some a pasta temporária do banco (a de e-mails continua).
- [ ] **T018** `[P]` — `Fluxos/CarrinhoTests.cs` (1 acesso) e `Fluxos/PaginaInicialTests.cs` (3 acessos): `SqliteConnection` → `NpgsqlConnection`. **O `UPPER()` dos dois lados some** — `ProdutoId` passa a ser `uuid` e a comparação é direta com o `Guid` como parâmetro. Entram aspas nos identificadores (`"Produto"`, `"Status"`, `"ItemPedido"`) e os parâmetros trocam de `$nome` para `@nome` (plano §7).
- [ ] **T019** — Rodar `dotnet test DocesCabana.Tests.E2E --filter "Categoria!=Externo"`. **Registrar a duração** e comparar com a da T002.

## Fase 6 — Varredura

- [ ] **T020** — `grep -rin "sqlite"` na base inteira (código, testes, `docs/`, `specs/README.md`, `.gitignore`). Cada ocorrência ou vira Postgres, ou vira referência histórica explícita ("era SQLite até a `028`"), ou sai. Atenção especial aos comentários que explicam contornos que deixaram de existir.
- [ ] **T021** `[P]` — Conferir que o aviso `NU1903` (`SQLitePCLRaw.lib.e_sqlite3`) **sumiu** do build — ele vinha da cadeia transitiva do `Microsoft.Data.Sqlite` no projeto de E2E, que não tinha o pin que a `Infrastructure` tinha. Sem SQLite em lugar nenhum, o build fica sem aviso pela primeira vez desde a `020`. Se ainda aparecer, achar de onde vem antes de fechar a fase.
- [ ] **T022** — `docs/arquitetura.md`: as seções que descrevem o banco, as migrations e a infraestrutura de teste. Explicar **por que** a suíte usa contêiner e não o banco da loja, e por que o `UPPER()` dos testes crus deixou de existir.

## Fase 7 — Fechamento

- [ ] **T023** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"` na base inteira.
- [ ] **T024** — Preencher `checklist.md`.
- [ ] **T025** — `specs/README.md`: a linha da feature, e registrar que a `028` fecha o par que a `027` abriu — as duas nasceram do mesmo pedido ("migrar para o Supabase") e foram separadas por não dependerem uma da outra.
- [ ] **T026** `[P]` — `specs/000-baseline/spec.md`: riscar as dívidas que esta entrega resolve, se houver.
- [ ] **T027** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero, **sem credencial do banco no ambiente** — é o estado em que qualquer pessoa clona o projeto (CA-04).
- [ ] **T028** — **CA-01 à mão**, e este é o critério que nenhum teste cobre inteiro: percorrer catálogo, filtro, busca, página de produto, favoritar, carrinho, conta, endereços e meus pedidos, conferindo que **nada mudou**. Qualquer diferença é regressão (RN-01), não "efeito da migração".
- [ ] **T029** — Atualizar o status da spec e do plano, e a linha em `specs/README.md`. Registrar o que **não** foi encerrado, se algo ficar.

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
