# Plano Técnico — Modelo de dados completo

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-12
**Status:** Rascunho

---

## 1. Resumo da abordagem

Dez entidades novas no `DocesCabana.Domain`, dez configurações no
`DocesCabana.Infrastructure`, uma migration e uma massa inicial reescrita. O
trabalho é feito em quatro grupos, cada um fechando em `dotnet test` verde:
**(A)** catálogo — `Categoria`, `Subcategoria`; **(B)** catálogo estendido —
`Promocao`, `Estoque`; **(C)** relacionamento com usuário — `Endereco`,
`Favorito`, `Avaliacao`; **(D)** compra — `Pedido`, `ItemPedido`, `Pagamento`.

A migration é criada uma vez só, ao final do grupo D, para não gerar quatro
migrations que ninguém vai querer ler separadas. A massa inicial é reescrita
junto do grupo A, porque é ela que quebra primeiro: o `DbInitializer` semeia hoje
produtos com `SubcategoriaId` aleatório, e no instante em que a chave estrangeira
existir esses produtos passam a ser órfãos.

Relacionamento entre entidades do domínio é expresso por **propriedade de
navegação do filho para o pai** (RQ-10, RQ-11). Referência a **usuário** é a
exceção: fica como `Guid` puro, porque `Usuario` vive na `Infrastructure` e
navegar até ele inverteria a dependência (RQ-02). Não há coleção em entidade
nenhuma nesta entrega — coleção implica agregado, e agregado é decisão da spec
que definir a regra.

Como o projeto não usa carregamento tardio (o pacote de proxies não está
instalado), toda navegação vem `null` a menos que a consulta peça `Include`
explicitamente. Isso é intencional: obriga quem consulta a declarar o que
precisa, em vez de disparar consulta escondida.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Todas as entidades novas nascem no `Domain`. Nenhuma `ProjectReference` nova. A RQ-02 é o que mantém isso de pé: `Endereco`, `Favorito`, `Avaliacao` e `Pedido` referenciam usuário por `Guid`, nunca por navegação — navegar até `Usuario` obrigaria o `Domain` a conhecer a `Infrastructure`. Os demais relacionamentos, todos internos ao `Domain`, usam navegação normal (RQ-10). |
| II | Domínio rico e auto-validante | ✅ OK | As dez entidades com `private set`, construtor que valida antes de atribuir e `protected Ctor()`. A RQ-04 limita o comportamento das seis sem consumidor, mas não a validação: essas também recusam dado inválido. |
| III | Validação nas duas barreiras | ⬜ n/a | Não há barreira de entrada porque não há formulário nesta feature. As barreiras aparecem nas specs que criarem tela. |
| IV | Nomenclatura em português | ✅ OK | A RQ-05 corrige três nomes vindos do `.dbml`. Enums e vocabulário já existentes são reaproveitados sem tradução nova. |
| V | Testes escritos antes | ✅ OK | Uma classe de teste de invariante por entidade, escrita e vermelha antes da entidade existir. |
| VI | Repositório + commit via UnitOfWork | ✅ OK | Uma migration versionada com nome em inglês. Configurações em `DatabaseContext/Configurations/`, uma por entidade, sem Data Annotation. Nenhum repositório novo: `IRepository<T>` já está registrado genericamente e resolve `IRepository<Subcategoria>` sem código. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ n/a | Nenhum controller é tocado. |
| VIII | Tratamento de erro por camada | ✅ OK | Entidades lançam `ArgumentException` / `ArgumentNullException` na construção e `InvalidOperationException` em operação inválida — `Estoque.Retirar` além do saldo é o único caso desta entrega. |

Nenhuma emenda constitucional necessária.

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Categoria.cs` | criar | RN-01 |
| `Entities/Subcategoria.cs` | criar | RN-02 |
| `Entities/Estoque.cs` | criar | RN-04, RN-05 — único com comportamento além do construtor: `Adicionar` e `Retirar` |
| `Entities/Promocao.cs` | criar | RN-06 a RN-10 — `Ativar`, `Desativar`, `EstaVigente`, `AlterarPeriodo` |
| `Entities/Endereco.cs` | criar | RN-12 a RN-14 |
| `Entities/Favorito.cs` | criar | RN-15 — junção com chave composta, sem comportamento |
| `Entities/Avaliacao.cs` | criar | RN-16 a RN-18 |
| `Entities/Pedido.cs` | criar | RN-19 a RN-21 — **sem** transição de status (RQ-04) |
| `Entities/ItemPedido.cs` | criar | RN-22 |
| `Entities/Pagamento.cs` | criar | RN-23 a RN-25 — **sem** `Aprovar`/`Estornar` (RQ-04) |
| `Helpers/CepHelper.cs` | criar | RN-13 — `ApenasDigitos` e `FormatoValido`, espelhando `CpfHelper` |
| `Entities/Produto.cs` | alterar | RQ-10 — navegações `Subcategoria` e `Promocao` sobre as chaves que já existem. Nenhuma coluna nova, nenhuma mudança de esquema |

Os cinco enums ficam **inalterados**. `Produto.PromocaoId` e `AplicarPromocao` já
expressam a RN-11 corretamente.

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `DatabaseContext/Configurations/*Configuration.cs` | criar (10) | Uma por entidade, com chave, obrigatoriedade, tamanho e chave estrangeira |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | Dez `DbSet<>` novos |
| `Migrations/` | criar | `AddRemainingDomainEntities` |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/DbInitializer.cs` | alterar | Semear categorias e subcategorias **antes** dos produtos, e ligar os seis produtos a uma subcategoria real |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/{Categoria,Subcategoria,Estoque,Promocao,Endereco,Favorito,Avaliacao,Pedido,ItemPedido,Pagamento}Tests.cs` | criar (10) | Invariantes RN-01 a RN-25 |
| `Units/Helpers/CepHelperTests.cs` | criar | RN-13 |
| `Integration/Repositories/ModeloDeDadosIntegrationTests.cs` | criar | Cada relacionamento recusa referência órfã; chave composta de `Favorito` recusa par duplicado |

### Documentação

| Arquivo | Ação | O quê |
|---|---|---|
| `ModelagemBancoTCC.dbml` | alterar | RQ-06 — três renomeações, remoção de `Promocao_Produto_FK`, `Promocao.Valor` para `decimal(18,2)`, e `Usuario` refletindo o Identity real |
| `specs/000-baseline/spec.md` | alterar | §5 deixa de listar como "modelado no papel" o que passou a existir |
| `specs/README.md` | alterar | Índice e renumeração do backlog |

## 4. Contratos

Nenhuma interface pública muda. As dez entidades são tipos novos; os construtores
seguem o padrão de `Produto` — parâmetros obrigatórios primeiro, `Guid id = default`
por último para o mapeador e para os testes.

```csharp
// Assinaturas representativas — o padrão se repete nas dez
public Categoria(string nome, Guid id = default);
public Subcategoria(Guid categoriaId, string nome, Guid id = default);
public Estoque(Guid produtoId, short quantidade);
public Promocao(string nome, PromocaoTipo tipo, decimal valor,
                DateTime dataInicio, DateTime dataFim,
                string? descricao = null, Guid id = default);
```

Dois métodos merecem nota:

```csharp
// Estoque — o único ponto desta entrega que lança InvalidOperationException
public void Retirar(short quantidade);   // recusa se deixar o saldo negativo

// Promocao — recebe a data de referência em vez de ler o relógio,
// para que a vigência seja testável sem congelar o tempo
public bool EstaVigente(DateTime referencia);
```

## 5. Modelo de dados

### Entidades e colunas

**Legenda:** `↦` relacionamento com propriedade de navegação (ambos no domínio);
`→` relacionamento só por identificador, sem navegação (alvo na infraestrutura).

| Entidade | Chave | Colunas |
|---|---|---|
| `Categoria` | `CategoriaId` | `Nome` (obrigatório, 100) |
| `Subcategoria` | `SubcategoriaId` | `CategoriaId` ↦ Categoria, `Nome` (obrigatório, 100) |
| `Estoque` | `ProdutoId` | ↦ Produto (1:1, chave compartilhada), `Quantidade` (`short`) |
| `Promocao` | `PromocaoId` | `Nome` (255), `Descricao` (255, opcional), `Tipo` (`PromocaoTipo`), `Valor` (`decimal(18,2)`), `DataInicio`, `DataFim`, `Ativa` |
| `Endereco` | `EnderecoId` | `UsuarioId` **→** Usuario, `Estado` (100), `Cidade` (150), `Bairro` (255), `CEP` (8), `Rua` (255), `Numero` (`int`), `Complemento` (100, opcional) |
| `Favorito` | `(ProdutoId, UsuarioId)` | `ProdutoId` ↦ Produto, `UsuarioId` **→** Usuario |
| `Avaliacao` | `AvaliacaoId` | `UsuarioId` **→** Usuario, `ProdutoId` ↦ Produto, `Comentario` (255, opcional), `Nota` (`byte`), `UpVote` (`bool`) |
| `Pedido` | `PedidoId` | `UsuarioId` **→** Usuario, `EnderecoEntregaId` ↦ Endereco, `PagamentoAprovado` (`bool`), `Valor` (`decimal(18,2)`), `Status` (`PedidoStatus`), `Data` |
| `ItemPedido` | `ItemPedidoId` | `PedidoId` ↦ Pedido, `ProdutoId` ↦ Produto, `Quantidade` (`short`), `PrecoUnitario` (`decimal(18,2)`) |
| `Pagamento` | `PagamentoId` | `PedidoId` ↦ Pedido (1:1), `Metodo` (`MetodoPagamento`), `Status` (`PagamentoStatus`), `Valor` (`decimal(18,2)`), `DataPagamento` (opcional) |

`Produto` deixa de ser inalterado e ganha duas navegações: `Subcategoria` (que já
tinha o `SubcategoriaId`) e `Promocao` (que já tinha o `PromocaoId`, opcional).
Nenhuma coluna nova — só a navegação sobre chave que já existe, o que **não gera
mudança de esquema**.

Quatro referências ficam sem navegação, todas apontando para `Usuario`:
`Endereco.UsuarioId`, `Favorito.UsuarioId`, `Avaliacao.UsuarioId` e
`Pedido.UsuarioId`. A spec `004` remove essa exceção.

### Divergências deliberadas em relação ao `.dbml`

| `.dbml` | Fica | Motivo |
|---|---|---|
| `Produto_Pedido_FK` | `ItemPedido` | A tabela guarda quantidade e preço unitário — é um conceito de negócio, não uma FK. RQ-05 |
| `Promocao_Produto_FK` | *(removida)* | RN-11: uma promoção por produto. `Produto.PromocaoId` já cobre |
| `Favoritos` | `Favorito` | Singular, como as demais. RQ-05 |
| `Promocao.Valor smallint` | `decimal(18,2)` | `smallint` não representa R$ 4,50 |
| `Favoritos` com índice em (ProdutoId, UsuarioId) | Chave **primária** composta | É junção pura; índice único e PK coincidem, e a PK impede o par duplicado da RN-15 |
| `Usuario.Senha varchar(255)` | *(não existe)* | O sistema usa `PasswordHash` do Identity desde a segunda migration; o `.dbml` está desatualizado |

Tipos monetários seguem `HasColumnType("decimal(18,2)")`, o mesmo de
`Produto.Preco` — a `002` verificou que é válido tanto no SQLite quanto no SQL
Server. Enums não recebem `HasColumnType`: o provider mapeia `byte` sozinho, e
foi justamente uma anotação `"INTEGER"` fixa que a `002` removeu.

### Massa inicial

Três categorias, espelhando o que o cabeçalho já exibe, com **identificadores
fixos** — não `Guid.NewGuid()` — para que testes e E2E possam referenciá-los:

| Categoria | Subcategorias |
|---|---|
| Salgados | Salgados Assados, Salgados Fritos |
| Doces | Doces de Tacho, Doces Caseiros |
| Adega | Vinhos, Destilados |

Os seis produtos existentes passam a apontar para **Doces de Tacho**. A ordem de
semeadura vira: categorias → subcategorias → produtos.

### Migration

```
dotnet ef migrations add AddRemainingDomainEntities \
  --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC
```

- **Impacto em dados existentes:** o banco local de desenvolvimento
  (`docescabana.db`, não versionado) tem produtos com `SubcategoriaId` órfão.
  Aplicar a chave estrangeira sobre eles **falha**. O banco é descartável: a
  tarefa correspondente o apaga antes de aplicar a migration.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/CategoriaTests.cs` | RN-01 |
| Unidade — entidade | `Units/Entities/SubcategoriaTests.cs` | RN-02 |
| Unidade — entidade | `Units/Entities/EstoqueTests.cs` | RN-04, RN-05 |
| Unidade — entidade | `Units/Entities/PromocaoTests.cs` | RN-06 a RN-10 |
| Unidade — entidade | `Units/Entities/EnderecoTests.cs` | RN-12 a RN-14 |
| Unidade — entidade | `Units/Entities/FavoritoTests.cs` | RN-15 |
| Unidade — entidade | `Units/Entities/AvaliacaoTests.cs` | RN-16 a RN-18 |
| Unidade — entidade | `Units/Entities/PedidoTests.cs` | RN-19 a RN-21 |
| Unidade — entidade | `Units/Entities/ItemPedidoTests.cs` | RN-22 |
| Unidade — entidade | `Units/Entities/PagamentoTests.cs` | RN-23 a RN-25 |
| Unidade — auxiliar | `Units/Helpers/CepHelperTests.cs` | RN-13 |
| Integração | `Integration/Repositories/ModeloDeDadosIntegrationTests.cs` | Cada FK recusa órfão; `Favorito` recusa par duplicado; `Estoque` recusa segundo registro para o mesmo produto |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | verificação manual, mais `VitrineE2E` quando a `005` existir |
| CA-02 | `Dado_MassaInicial_Quando_ConsultarSubcategoriaDoProduto_Entao_DeveExistirComCategoria` |
| CA-03 | `Dado_ProdutoComSubcategoriaInexistente_Quando_Salvar_Entao_DeveRecusar` |
| CA-04 | `Dado_EstoqueComTresUnidades_Quando_RetirarCinco_Entao_DeveLancarInvalidOperationException` |
| CA-05 | `Dado_DatasInvertidas_Quando_CriarPromocao_Entao_DeveLancarArgumentException` |
| CA-06 | `Dado_PercentualForaDaFaixa_Quando_CriarPromocao_Entao_DeveLancarArgumentException` |
| CA-07 | `Dado_PromocaoAtivaNoPeriodo_Quando_EstaVigente_Entao_DeveRetornarTrue` |
| CA-08 | `Dado_NotaForaDaFaixa_Quando_CriarAvaliacao_Entao_DeveLancarArgumentException` |
| CA-09 | `Dado_DadosValidos_Quando_CriarPedido_Entao_DeveNascerPendente` |
| CA-10 | `Dado_QuantidadeZero_Quando_CriarItemPedido_Entao_DeveLancarArgumentException` |
| CA-11 | conferência manual do `.dbml` contra o snapshot, tarefa de fechamento |
| CA-12 | `dotnet test`, tarefa de fechamento |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Proibir navegação em **todo** o modelo, inclusive entre entidades do domínio | Era o desenho inicial, e estava errado: a restrição vale só onde o alvo é `Usuario`, que mora na `Infrastructure`. Generalizar a proibição empobrecia consultas simples — `Subcategoria` → `Categoria` não tem razão nenhuma para ser `Guid` solto — sem nenhum ganho arquitetural. Corrigido pela RQ-10. |
| Expor coleções (`Categoria.Subcategorias`, `Pedido.Itens`) | Coleção implica decidir quem gerencia a vida dos filhos, e isso é decisão de agregado. A spec de pedido é que define se o total é calculado a partir dos itens e o que acontece ao remover um. Navegação só do filho para o pai (RQ-11) entrega o que as features próximas precisam sem antecipar essa decisão. |
| Separar pessoa de credencial já nesta spec | Eliminaria a exceção da RQ-02 de vez, mas exige migration movendo colunas de `Usuario` e mexe em `UsuarioService`, `UsuarioMapper` e em toda a bateria de testes de autenticação que a `002` acabou de deixar verde. Misturar isso com a criação de dez tabelas produz uma entrega difícil de revisar e de reverter. Vira a spec `004`, imediatamente depois desta. |
| `Pedido` como raiz de agregado com coleção de `ItemPedido` | É o desenho correto **quando existir a spec de pedido**, que é quem define se o total é calculado ou informado, e o que acontece ao remover item. Modelar o agregado antes disso é adivinhar a regra — exatamente o que a RQ-04 proíbe. `ItemPedido` fica plano e a spec de carrinho promove o agregado. |
| Uma migration por grupo de entidades | Quatro migrations que só fazem sentido juntas. Uma só, com nome descritivo, é mais legível no histórico. |
| Manter os nomes `*_FK` do `.dbml` | Ferem o Princípio IV, e `Produto_Pedido_FK` descreve mal uma tabela que guarda quantidade e preço. Corrigir o `.dbml` mantém as duas fontes honestas. |
| Manter `Promocao_Produto_FK` como N:N | Exigiria uma regra de desempate — qual desconto vale quando duas promoções pegam o mesmo produto — que não existe em lugar nenhum e teria de ser inventada aqui. |
| Criar `ICategoriaRepository`, `IPromocaoRepository` etc. | `IRepository<T>` já está registrado genericamente e resolve qualquer entidade. Interface específica só quando uma feature precisar de consulta específica. |
| Gerar `Guid` aleatório na massa inicial | Impede teste e E2E de referenciarem categoria conhecida. Identificadores fixos custam nada e destravam os dois. |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Migration falhar por produto órfão no banco local | **Alta** | Médio | O banco é descartável e não versionado; a tarefa apaga `docescabana.db` antes de aplicar. Documentado no README |
| Semear produto antes de existir a subcategoria | Alta | Alto | A ordem de semeadura vira explícita: categorias → subcategorias → produtos. Teste de integração cobre |
| Modelar errado uma das seis entidades sem consumidor | Média | Médio | A RQ-04 limita o dano: sem método de transição, o que existe é validação de campo, que raramente muda. O agregado de pedido é explicitamente adiado |
| Diff de 10 entidades ficar ilegível | Média | Baixo | Quatro grupos, quatro commits, cada um verde |
| `Estoque` com chave compartilhada confundir o EF | Baixa | Médio | Configuração explícita de 1:1 com `HasOne().WithOne().HasForeignKey<Estoque>()`; teste de integração prova que um segundo estoque para o mesmo produto é recusado |
| Redundância `ProdutoStatus.ForaDeEstoque` × `Quantidade == 0` gerar dado inconsistente | Média | Baixo | Reconhecida na spec §9 e adiada para a spec de estoque; nesta entrega nada escreve nos dois |
| Navegação vir `null` por falta de `Include` e estourar `NullReferenceException` em produção | **Alta** | Médio | Toda navegação é declarada anulável (`Subcategoria?`), o que faz o compilador cobrar a verificação. Teste de integração prova as duas metades: sem `Include` vem `null`, com `Include` vem preenchida. A alternativa — proxies de carregamento tardio — dispara consulta escondida e não será instalada |
| Adicionar navegação a `Produto` gerar migration indesejada | Média | Médio | A navegação assenta sobre `SubcategoriaId` e `PromocaoId`, que já existem. Tarefa de verificação gera migration e confere que sai vazia, como a `002` fez com o dialeto |

## 9. Desvios constitucionais justificados

Nenhum.
