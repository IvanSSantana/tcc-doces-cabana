# Tarefas — Modelo de dados completo

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

**Específico desta feature:** os grupos A a D são quatro commits independentes.
A **migration é uma só**, criada no grupo E depois que as dez entidades e as dez
configurações existirem — gerar uma por grupo produziria quatro migrations que só
fazem sentido juntas.

**Regra de navegação, válida em todas as tarefas:** relacionamento entre
entidades do domínio é propriedade de navegação **anulável**, declarada do filho
para o pai. Referência a `Usuario` é `Guid` puro, sem navegação. Nenhuma coleção.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `003-modelo-de-dados-completo` a partir de `main`,
      com a `002` já integrada.
- [ ] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado inicial
      aqui: **152 testes, 0 falhas, 0 avisos**. É a linha de base contra a qual a
      T056 compara.
- [ ] **T003** — `DocesCabana.Domain/Helpers/CepHelper.cs` e
      `DocesCabana.Tests/Units/Helpers/CepHelperTests.cs`: `ApenasDigitos` e
      `FormatoValido` (exatamente 8 dígitos), espelhando `CpfHelper`. Teste
      primeiro. **Implementa RN-13.**

---

## Fase 2 — Grupo A: catálogo

*`Categoria` e `Subcategoria`. É o grupo que destrava a `001` e o que quebra a
massa inicial, por isso vem primeiro.*

### Testes — devem falhar

- [ ] **T004** `[P]` — `DocesCabana.Tests/Units/Entities/CategoriaTests.cs`:
      nome vazio, nome com 2 caracteres, nome com 101 caracteres, caso válido.
      **Prova RN-01.**
- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Entities/SubcategoriaTests.cs`:
      `CategoriaId` vazio, nome inválido nas mesmas faixas, caso válido.
      **Prova RN-02.**
- [ ] **T006** — Rodar `dotnet test` e confirmar que T004–T005 falham por
      ausência do tipo, não por outro motivo.

### Implementação

- [ ] **T007** `[P]` — `DocesCabana.Domain/Entities/Categoria.cs`: `private set`,
      construtor validante, `protected Ctor()`, `AlterarNome`.
- [ ] **T008** — `DocesCabana.Domain/Entities/Subcategoria.cs`: idem, mais a
      navegação anulável `Categoria? Categoria` sobre `CategoriaId`.
- [ ] **T009** `[P]` — `DocesCabana.Infrastructure/DatabaseContext/Configurations/CategoriaConfiguration.cs`.
- [ ] **T010** `[P]` — `.../Configurations/SubcategoriaConfiguration.cs`, com a
      chave estrangeira para `Categoria` e `DeleteBehavior.Restrict`.
- [ ] **T011** — `DocesCabana.Infrastructure/DatabaseContext/DocesCabanaDbContext.cs`:
      `DbSet<Categoria>` e `DbSet<Subcategoria>`.
- [ ] **T012** — `DocesCabana.Domain/Entities/Produto.cs`: navegação anulável
      `Subcategoria? Subcategoria` sobre o `SubcategoriaId` que já existe.
      Nenhuma coluna nova. **RQ-10.**
- [ ] **T013** — Rodar `dotnet test`: T004–T005 passam.

---

## Fase 3 — Grupo B: promoção e estoque

### Testes — devem falhar

- [ ] **T014** `[P]` — `DocesCabana.Tests/Units/Entities/PromocaoTests.cs`: nome
      vazio e nome longo demais; data de fim anterior à de início; tipo
      *Percentual* com valor 0, 101 e 100 (limite válido); tipo *ValorFixo* com
      valor 0 e negativo; `EstaVigente` dentro do período, fora do período, e com
      a promoção desativada. **Prova RN-06 a RN-10, CA-05, CA-06, CA-07.**
- [ ] **T015** `[P]` — `DocesCabana.Tests/Units/Entities/EstoqueTests.cs`:
      `ProdutoId` vazio; quantidade inicial negativa; `Adicionar` soma;
      `Retirar` dentro do saldo subtrai; `Retirar` além do saldo lança
      `InvalidOperationException` **e deixa a quantidade intacta**.
      **Prova RN-04, RN-05, CA-04.**
- [ ] **T016** — Rodar `dotnet test` e confirmar o vermelho.

### Implementação

- [ ] **T017** `[P]` — `DocesCabana.Domain/Entities/Promocao.cs`: `Ativar`,
      `Desativar`, `AlterarPeriodo` e `EstaVigente(DateTime referencia)` — a data
      vem por parâmetro, nunca do relógio, para que a vigência seja testável.
- [ ] **T018** `[P]` — `DocesCabana.Domain/Entities/Estoque.cs`: navegação
      anulável `Produto? Produto`; `Adicionar` e `Retirar`.
- [ ] **T019** `[P]` — `.../Configurations/PromocaoConfiguration.cs`, com
      `Valor` em `HasColumnType("decimal(18,2)")` e `Tipo` **sem**
      `HasColumnType` (lição da `002`).
- [ ] **T020** `[P]` — `.../Configurations/EstoqueConfiguration.cs`: 1:1 com
      `Produto` por chave compartilhada —
      `HasOne(e => e.Produto).WithOne().HasForeignKey<Estoque>(e => e.ProdutoId)`.
- [ ] **T021** — `DocesCabanaDbContext.cs`: `DbSet<Promocao>`, `DbSet<Estoque>`.
- [ ] **T022** — `DocesCabana.Domain/Entities/Produto.cs`: navegação anulável
      `Promocao? Promocao` sobre o `PromocaoId` que já existe.
- [ ] **T023** — Rodar `dotnet test`: T014–T015 passam.

---

## Fase 4 — Grupo C: relacionamento com usuário

*`Endereco`, `Favorito` e `Avaliacao`. Os três referenciam usuário por `Guid`
puro — é aqui que a RQ-02 aparece na prática.*

### Testes — devem falhar

- [ ] **T024** `[P]` — `DocesCabana.Tests/Units/Entities/EnderecoTests.cs`:
      `UsuarioId` vazio; CEP com 7 e com 9 dígitos; CEP pontuado válido; estado,
      cidade, bairro e rua vazios; número 0 e negativo; complemento nulo aceito.
      **Prova RN-12 a RN-14.**
- [ ] **T025** `[P]` — `DocesCabana.Tests/Units/Entities/FavoritoTests.cs`:
      `ProdutoId` vazio, `UsuarioId` vazio, caso válido. **Prova RN-15.**
- [ ] **T026** `[P]` — `DocesCabana.Tests/Units/Entities/AvaliacaoTests.cs`:
      identificadores vazios; nota 0, 6, 1 e 5; comentário nulo aceito e
      comentário com 256 caracteres recusado. **Prova RN-16 a RN-18, CA-08.**
- [ ] **T027** — Rodar `dotnet test` e confirmar o vermelho.

### Implementação

- [ ] **T028** `[P]` — `DocesCabana.Domain/Entities/Endereco.cs`, usando
      `CepHelper` da T003. `UsuarioId` é `Guid` puro, **sem navegação**.
- [ ] **T029** `[P]` — `DocesCabana.Domain/Entities/Favorito.cs`: navegação
      `Produto? Produto`; `UsuarioId` sem navegação.
- [ ] **T030** `[P]` — `DocesCabana.Domain/Entities/Avaliacao.cs`: idem.
- [ ] **T031** `[P]` — `.../Configurations/EnderecoConfiguration.cs`, com FK para
      a tabela de usuário declarada por `HasOne<Usuario>().WithMany()` —
      o relacionamento existe no banco sem existir na entidade.
- [ ] **T032** `[P]` — `.../Configurations/FavoritoConfiguration.cs`: chave
      primária composta `HasKey(f => new { f.ProdutoId, f.UsuarioId })`, que é o
      que impede o par duplicado da RN-15.
- [ ] **T033** `[P]` — `.../Configurations/AvaliacaoConfiguration.cs`.
- [ ] **T034** — `DocesCabanaDbContext.cs`: `DbSet<Endereco>`, `DbSet<Favorito>`,
      `DbSet<Avaliacao>`.
- [ ] **T035** — Rodar `dotnet test`: T024–T026 passam.

---

## Fase 5 — Grupo D: compra

*`Pedido`, `ItemPedido` e `Pagamento`. **Sem método de transição de estado** —
RQ-04. Se alguma tarefa aqui pedir `Cancelar` ou `Aprovar`, ela está fora do
escopo desta spec.*

### Testes — devem falhar

- [ ] **T036** `[P]` — `DocesCabana.Tests/Units/Entities/PedidoTests.cs`:
      `UsuarioId` e `EnderecoEntregaId` vazios; valor negativo; e o caso válido
      confirmando que nasce com status *Pendente*, pagamento não aprovado e data
      preenchida. **Prova RN-19 a RN-21, CA-09.**
- [ ] **T037** `[P]` — `DocesCabana.Tests/Units/Entities/ItemPedidoTests.cs`:
      identificadores vazios; quantidade 0 e negativa; preço unitário 0 e
      negativo; caso válido. **Prova RN-22, CA-10.**
- [ ] **T038** `[P]` — `DocesCabana.Tests/Units/Entities/PagamentoTests.cs`:
      `PedidoId` vazio; valor 0 e negativo; caso válido confirmando status
      *Pendente* e `DataPagamento` nula. **Prova RN-23 a RN-25.**
- [ ] **T039** — Rodar `dotnet test` e confirmar o vermelho.

### Implementação

- [ ] **T040** `[P]` — `DocesCabana.Domain/Entities/Pedido.cs`: navegação
      `Endereco? EnderecoEntrega`; `UsuarioId` sem navegação; **nenhuma coleção
      de itens** e nenhum método de transição.
- [ ] **T041** `[P]` — `DocesCabana.Domain/Entities/ItemPedido.cs`: navegações
      `Pedido? Pedido` e `Produto? Produto`.
- [ ] **T042** `[P]` — `DocesCabana.Domain/Entities/Pagamento.cs`: navegação
      `Pedido? Pedido`.
- [ ] **T043** `[P]` — `.../Configurations/PedidoConfiguration.cs`,
      `ItemPedidoConfiguration.cs` e `PagamentoConfiguration.cs`. `Pagamento` é
      1:1 com `Pedido`; os valores monetários usam `decimal(18,2)`.
- [ ] **T044** — `DocesCabanaDbContext.cs`: `DbSet<Pedido>`, `DbSet<ItemPedido>`,
      `DbSet<Pagamento>`.
- [ ] **T045** — Rodar `dotnet test`: T036–T038 passam. As dez entidades existem.

---

## Fase 6 — Grupo E: persistência e massa inicial

- [ ] **T046** — **Apagar o banco local** `DocesCabana.MVC/docescabana.db` (e os
      arquivos `-shm`/`-wal`). Ele tem produtos com `SubcategoriaId` órfão, e a
      chave estrangeira nova **não aplica** sobre eles. O banco não é versionado
      e é descartável. Registrar o passo no `README.md`.
- [ ] **T047** — Criar a migration:
      `dotnet ef migrations add AddRemainingDomainEntities --project
      DocesCabana.Infrastructure --startup-project DocesCabana.MVC`.
      Conferir no arquivo gerado que só há criação de tabela e chave estrangeira —
      **nenhuma alteração em `Produto`**. As navegações da T012 e da T022 assentam
      sobre colunas que já existem; se aparecer `AlterColumn` em `Produto`, parar
      e reavaliar.
- [ ] **T048** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: reescrever `Semear`
      na ordem categorias → subcategorias → produtos, com **identificadores
      fixos** (não `Guid.NewGuid()`), para que testes e E2E possam referenciar
      categoria conhecida. Três categorias — Salgados, Doces, Adega — com duas
      subcategorias cada, conforme o plano §5. Os seis produtos existentes passam
      a apontar para *Doces de Tacho*. **Implementa RF-02, CA-02.**
- [ ] **T049** — `DocesCabana.Tests/Integration/Repositories/ModeloDeDadosIntegrationTests.cs`:
      produto com subcategoria inexistente é recusado (**CA-03**); `Favorito`
      recusa o mesmo par duas vezes; `Estoque` recusa segundo registro para o
      mesmo produto; navegação vem `null` sem `Include` e preenchida com
      `Include`.
- [ ] **T050** — Rodar `dotnet test`: verde, incluindo os testes de integração
      novos.
- [ ] **T051** — Subir a aplicação, confirmar que o banco é recriado e semeado
      sem erro, e que a vitrine exibe os seis produtos com preço em formato
      brasileiro. **Prova CA-01.**

---

## Fase 7 — Documentação e fechamento

- [ ] **T052** `[P]` — `ModelagemBancoTCC.dbml`: remover `Promocao_Produto_FK`;
      renomear `Produto_Pedido_FK` para `ItemPedido` e `Favoritos` para
      `Favorito`; `Promocao.Valor` de `smallint` para `decimal(18,2)`; e corrigir
      `Usuario`, que ainda lista `Senha varchar(255)` embora o sistema use o
      `PasswordHash` do Identity desde a segunda migration. **RQ-06.**
- [ ] **T053** `[P]` — `specs/000-baseline/spec.md`: a §5 deixa de listar como
      "modelado no papel" tudo o que passou a existir; sobra apenas a nota de que
      as features que consomem essas tabelas continuam pendentes. A dívida D-07
      (`Endereco` sem entidade) é marcada como **resolvida** por esta spec.
- [ ] **T054** `[P]` — `specs/README.md`: status da `003` para *Implementada*.
- [ ] **T055** — Conferir `.dbml` contra
      `Migrations/DocesCabanaDbContextModelSnapshot.cs`: tabelas, colunas e
      relacionamentos correspondem, e nenhum nome `*_FK` sobrou. **Prova CA-11.**
- [ ] **T056** — `dotnet build` sem avisos novos e `dotnet test` inteiro verde,
      com contagem maior que os 152 da T002. **Prova CA-12.**
- [ ] **T057** — Preencher `checklist.md` e atualizar o status da spec.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 — vitrine não regride | T048, T051 |
| RF-02 — produtos em subcategorias reais | T048, T049 |
| RF-03 — produto órfão recusado | T049 |
| RQ-01 — as dez tabelas existem | T007–T045, T047 |
| RQ-02 — sem navegação para `Usuario` | T028, T029, T030, T031, T040 |
| RQ-03 — domínio rico | T007, T008, T017, T018, T028–T030, T040–T042 |
| RQ-04 — invariantes enxutas onde não há consumidor | T040, T041, T042 |
| RQ-05 — nomes corrigidos | T041, T043, T052 |
| RQ-06 — `.dbml` em dia | T052, T055 |
| RQ-07 — uma migration | T047 |
| RQ-08 — sem sintaxe presa a provider | T019, T043 |
| RQ-09 — cobertura de invariante e relacionamento | T004–T006, T014–T016, T024–T027, T036–T039, T049 |
| RQ-10 — navegação entre entidades do domínio | T008, T012, T018, T022, T029, T030, T040, T041, T042 |
| RQ-11 — sem coleção | T040 |
| RN-01, RN-02 | T004, T005, T007, T008 |
| RN-03 | T049 |
| RN-04, RN-05 | T015, T018, T020 |
| RN-06 a RN-10 | T014, T017 |
| RN-11 | *(já satisfeita por `Produto.PromocaoId`; T022 só acrescenta a navegação)* |
| RN-12 a RN-14 | T003, T024, T028 |
| RN-15 | T025, T029, T032 |
| RN-16 a RN-18 | T026, T030 |
| RN-19 a RN-21 | T036, T040 |
| RN-22 | T037, T041 |
| RN-23 a RN-25 | T038, T042, T043 |
| CA-01 | T051 |
| CA-02 | T048, T049 |
| CA-03 | T049 |
| CA-04 | T015, T018 |
| CA-05, CA-06, CA-07 | T014, T017 |
| CA-08 | T026, T030 |
| CA-09 | T036, T040 |
| CA-10 | T037, T041 |
| CA-11 | T052, T055 |
| CA-12 | T056 |
