# Plano Técnico — Página do produto

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-14
**Status:** Rascunho

---

## 1. Resumo da abordagem

A página é uma tela de leitura servida por um controller novo, `ProdutoController`,
em `GET /Produto/Detalhes/{id}`. Ela é montada por um único DTO composto —
`ProdutoDetalheDTO` — que junta o produto, o caminho de navegação, o resumo das
avaliações e a página de avaliações já ordenada, para que a view não faça conta
nem consulta. Ordenação e "Ver mais" viajam na *query string* e redesenham a
página inteira no servidor; o único JavaScript da tela é o seletor de quantidade.
O voto de útil é um `POST` com antiforgery que volta por redirecionamento para a
mesma página, com a mesma ordenação e a mesma quantidade de avaliações abertas
(Princípio VII).

Três mudanças de modelo sustentam a tela: `Produto` ganha `Descricao`; `Avaliacao`
ganha `DataCriacao` e perde o campo `UpVote`, que era um `bool` solto e não
consegue contar votos; e nasce `VotoUtil`, a entidade que registra quem marcou o
quê. A contagem de úteis passa a ser derivada dessa tabela, nunca um contador
gravado — assim ela não diverge.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma `ProjectReference` nova. `VotoUtil` é entidade de domínio pura; a navegação de `Avaliacao` até `Usuario` só é possível porque a `004` traz `Usuario` para o domínio — esta feature **depende** dela, e não abre exceção nova |
| II | Domínio rico e auto-validante | ✅ OK | `Avaliacao.AlternarVotoUtil` é método de intenção e guarda RN-06 e RN-07 dentro da entidade; `Produto.AlterarDescricao` valida RN-01 |
| III | Validação nas duas barreiras | ✅ OK | Descrição: `ProdutoDTOValidator` (mensagem no campo) **e** construtor de `Produto` (invariante). Voto: `[Authorize]` + verificação de autoria na entidade |
| IV | Nomenclatura em português | ✅ OK | `ProdutoDetalheDTO`, `AvaliacaoService`, `VotoUtil`, `OrdenacaoAvaliacao`, rota `/Produto/Detalhes`. Média e data formatadas em `pt-BR` |
| V | Testes escritos antes | ✅ OK | Fase 2 das tarefas é inteiramente de teste vermelho |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `IAvaliacaoRepository` novo; o voto grava via `IUnitOfWork.SalvarAlteracoes` no `AvaliacaoService` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `VotarUtil` é `[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Authorize]`, `async Task<IActionResult>` e redireciona. `Detalhes` é `GET` público e não muda estado |
| VIII | Tratamento de erro por camada | ✅ OK | Produto ausente ou inativo → `KeyNotFoundException` na `Application`; voto na própria avaliação → `InvalidOperationException` no domínio. Ambos caem no `FilterException`, sem `try/catch` no controller |

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Produto.cs` | alterar | `string? Descricao { get; private set; }`, parâmetro opcional no construtor, `AlterarDescricao(string?)` e `ValidarDescricao` (RN-01, máx. 4000). Anulável para não invalidar os produtos já cadastrados |
| `Entities/Avaliacao.cs` | alterar | Ganha `DateTime DataCriacao`; **perde** `bool UpVote`; ganha `_votos` privado, `IReadOnlyCollection<VotoUtil> Votos`, `int TotalUteis`, `bool MarcadaComoUtilPor(Guid)` e `bool AlternarVotoUtil(Guid)` — que lança se o votante for o autor (RN-07) |
| `Entities/VotoUtil.cs` | **criar** | `AvaliacaoId` + `UsuarioId` (chave composta), construtor validante, navegação `Avaliacao?` |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/OrdenacaoAvaliacao.cs` | **criar** | `Relevantes`, `MaisRecentes`, `MaiorNota`, `MenorNota`. Pasta nova: ordenação é escolha de consulta, não invariante — não pertence a `Domain/Enums` |
| `DTOs/ProdutoDTO.cs` | alterar | Ganha `Descricao` |
| `DTOs/ProdutoDetalheDTO.cs` | **criar** | Produto + `SubcategoriaNome` + `Resumo` + `ResumoAvaliacoesDTO` + `PaginaAvaliacoesDTO` |
| `DTOs/AvaliacaoDTO.cs` | **criar** | `AvaliacaoId`, `AutorNome`, `Nota`, `Comentario`, `DataCriacao`, `TotalUteis`, `MarcadaPeloUsuarioAtual`, `EhDoUsuarioAtual` |
| `DTOs/ResumoAvaliacoesDTO.cs` | **criar** | `Media` (`decimal?`), `Total`, `Distribuicao` (`IReadOnlyDictionary<byte,int>`, sempre com as cinco chaves) |
| `DTOs/PaginaAvaliacoesDTO.cs` | **criar** | `Itens`, `Ordenacao`, `Exibindo`, `Total`, `TemMais` |
| `Contracts/Repositories/IProdutoRepository.cs` | alterar | `BuscarDetalhePorId` — o `BuscarPorId` genérico não traz a subcategoria |
| `Contracts/Repositories/IAvaliacaoRepository.cs` | **criar** | Consulta paginada e ordenada, contagem por nota e busca com votos carregados |
| `Contracts/Services/IProdutoService.cs` | alterar | `BuscarDetalhe(...)` |
| `Contracts/Services/IAvaliacaoService.cs` | **criar** | Resumo, listagem e alternância de voto |
| `Services/ProdutoService.cs` | alterar | `BuscarDetalhe` compõe produto + avaliações; lança `KeyNotFoundException` para inexistente **e** para inativo (RF-04) |
| `Services/AvaliacaoService.cs` | **criar** | Média (RN-03), histograma (RN-04), ordenação (RN-05) e voto com commit |
| `Mappings/ProdutoMapper.cs` | alterar | Mapear `Descricao` nos dois sentidos |
| `Mappings/AvaliacaoMapper.cs` | **criar** | `ToDTO(Avaliacao, Guid? usuarioAtual)` |
| `Mappings/ProdutoDetalheMapper.cs` | **criar** | Monta o DTO composto e calcula o resumo de 160 caracteres (RN-02) |
| `Validators/ProdutoDTOValidator.cs` | alterar | `MaximumLength(4000)` na descrição, com a mensagem do RN-01 |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/ProdutoRepository.cs` | alterar | `BuscarDetalhePorId` com `Include(p => p.Subcategoria)` |
| `Repositories/AvaliacaoRepository.cs` | **criar** | `Include(a => a.Usuario)` e `Include(a => a.Votos)`; ordenação traduzida para `OrderBy`; contagem por nota com `GroupBy` no banco |
| `DatabaseContext/Configurations/ProdutoConfiguration.cs` | alterar | `Descricao` opcional, `HasMaxLength(4000)` |
| `DatabaseContext/Configurations/AvaliacaoConfiguration.cs` | alterar | Remove `UpVote`; `DataCriacao` obrigatória; `HasMany(a => a.Votos)` com `OnDelete(Cascade)` — apagada a avaliação, os votos dela vão junto |
| `DatabaseContext/Configurations/VotoUtilConfiguration.cs` | **criar** | Tabela `VotoUtil`, chave composta `(AvaliacaoId, UsuarioId)`, FK para `Usuario` com `Restrict` |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | `DbSet<VotoUtil>` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar `IAvaliacaoRepository` e `IAvaliacaoService` |
| `Migrations/` | criar | `AddProdutoDescricaoAndAvaliacaoVotes` |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/ProdutoController.cs` | **criar** | `Detalhes` (GET público) e `VotarUtil` (POST autenticado) |
| `Views/Produto/Detalhes.cshtml` | **criar** | Caminho de navegação, bloco de compra, seção de descrição |
| `Views/Produto/_BlocoAvaliacoes.cshtml` | **criar** | Média, histograma, ordenação, lista e "Ver mais" |
| `Views/Produto/_CartaoAvaliacao.cshtml` | **criar** | Um cartão de avaliação, com o formulário do voto |
| `ViewComponents/EstrelasNota.cs` | **criar** | Recebe nota e tamanho |
| `Views/Shared/Components/EstrelasNota/Default.cshtml` | **criar** | Fileira de estrelas em SVG com preenchimento fracionário |
| `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | Imagem e nome viram link para a página do produto (RF-01) |
| `Views/Admin/Cadastro.cshtml` | alterar | `textarea` de descrição (RF-11) |
| `wwwroot/css/pages/produto.css` | **criar** | Seção 6 |
| `wwwroot/css/components/estrelas-nota.css` | **criar** | Estilo da fileira de estrelas, reaproveitável |
| `wwwroot/js/pages/produto.js` | **criar** | Seletor de quantidade (RN-10) |
| `Helpers/DbInitializer.cs` | alterar | Descrição nos produtos semeados e avaliações de exemplo, para a tela ter conteúdo em desenvolvimento |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/ProdutoTests.cs` | alterar | RN-01 |
| `Units/Entities/AvaliacaoTests.cs` | alterar | RN-06, RN-07, RN-08, RN-09 |
| `Units/Entities/VotoUtilTests.cs` | **criar** | Construtor validante |
| `Units/Mappings/ProdutoDetalheMapperTests.cs` | **criar** | RN-02, incluindo o corte no fim da palavra e o texto curto sem reticências |
| `Units/Services/ProdutoServiceTests.cs` | alterar | RF-03 e RF-04 |
| `Units/Services/AvaliacaoServiceTests.cs` | **criar** | RN-03, RN-04, RN-05, RF-14, RF-15, RF-16, RF-19 |
| `Units/Validators/ProdutoDTOValidatorTests.cs` | alterar | RN-01 nas duas pontas |
| `Units/Controllers/ProdutoControllerTests.cs` | **criar** | RF-03, RF-17, RF-20, RF-21 |
| `Integration/Repositories/AvaliacaoRepositoryIntegrationTests.cs` | **criar** | Ordenação e contagem por nota rodando em SQLite |
| `Integration/InfraestruturaSqliteEmMemoria.cs` | alterar | `SemearAvaliacao` |

## 4. Contratos

```csharp
// Domain/Entities/Avaliacao.cs
public IReadOnlyCollection<VotoUtil> Votos { get; }
public int TotalUteis { get; }
public bool MarcadaComoUtilPor(Guid usuarioId);
public bool AlternarVotoUtil(Guid usuarioId);   // true = marcou, false = desmarcou

// Domain/Entities/VotoUtil.cs
public VotoUtil(Guid avaliacaoId, Guid usuarioId);

// Domain/Entities/Produto.cs
public void AlterarDescricao(string? descricao);

// Application/Contracts/Repositories/IAvaliacaoRepository.cs
public interface IAvaliacaoRepository : IRepository<Avaliacao>
{
    Task<IEnumerable<Avaliacao>> BuscarPorProduto(
        Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade);

    Task<int> ContarPorProduto(Guid produtoId);

    Task<IReadOnlyDictionary<byte, int>> ContarPorNota(Guid produtoId);

    Task<Avaliacao?> BuscarComVotos(Guid avaliacaoId);
}

// Application/Contracts/Services/IAvaliacaoService.cs
public interface IAvaliacaoService
{
    Task<ResumoAvaliacoesDTO> ResumirPorProduto(Guid produtoId);

    Task<PaginaAvaliacoesDTO> ListarPorProduto(
        Guid produtoId, OrdenacaoAvaliacao ordenacao, int quantidade, Guid? usuarioAtual);

    Task<Guid> AlternarVotoUtil(Guid avaliacaoId, Guid usuarioId);   // devolve o ProdutoId
}

// Application/Contracts/Services/IProdutoService.cs
Task<ProdutoDetalheDTO> BuscarDetalhe(
    Guid id, OrdenacaoAvaliacao ordenacao, int avaliacoesExibidas, Guid? usuarioAtual);

// Application/Contracts/Repositories/IProdutoRepository.cs
Task<Produto?> BuscarDetalhePorId(Guid id);
```

Rotas, pela rota convencional já configurada no `Program.cs`:

```
GET  /Produto/Detalhes/{id}?ordenacao=Relevantes&exibir=5
POST /Produto/VotarUtil        → 302 para a linha acima, com âncora #avaliacoes
```

`exibir` é saneado no controller: mínimo 5, máximo 100, arredondado para baixo em
múltiplos de 5. `ordenacao` inválida cai em `Relevantes`.

## 5. Modelo de dados

**`Produto`** — coluna nova:

| Campo | Tipo | Obrigatório |
|---|---|---|
| `Descricao` | `nvarchar(4000)` | não |

**`Avaliacao`** — uma coluna a mais, uma a menos:

| Campo | Tipo | Obrigatório | Observação |
|---|---|---|---|
| `DataCriacao` | `datetime2` | sim | Gravada em UTC, exibida em `pt-BR` |
| ~~`UpVote`~~ | ~~`bit`~~ | — | **Removida.** Um `bool` na avaliação não responde "quantas pessoas acharam útil" nem "eu já marquei" — as duas perguntas que a tela faz. Substituída pela tabela `VotoUtil` |

**`VotoUtil`** — tabela nova:

| Campo | Tipo | Obrigatório |
|---|---|---|
| `AvaliacaoId` | `uniqueidentifier` | sim, PK composta |
| `UsuarioId` | `uniqueidentifier` | sim, PK composta |

- **Relacionamentos:** `Avaliacao 1—N VotoUtil`, exclusão em cascata; `Usuario 1—N VotoUtil`, exclusão restrita. A chave composta é o que garante o RN-06 no banco, e não só no código.
- **Migration:** `dotnet ef migrations add AddProdutoDescricaoAndAvaliacaoVotes --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`
- **Impacto em dados existentes:** produtos existentes ficam com `Descricao` nula, o que a tela trata (RF-08). `Avaliacao` ainda não tem linha em produção, então descartar `UpVote` não perde dado. O `ModelagemBancoTCC.dbml` precisa acompanhar as três mudanças.

## 6. Direção visual

A referência visual manda na tela; o que segue fixa o que ela deixa em aberto,
sempre a partir do que o site já tem em `site.css`.

### Paleta

Tokens escopados em `.pagina-produto`, para não vazar para o resto do site:

| Token | Valor | Onde |
|---|---|---|
| `--produto-fundo` | `#FDF5F1` | Fundo da página. Um creme quente no lugar do `#FAFAFA` geral — é a única tela de leitura longa da loja, e o papel morno cansa menos que o branco |
| `--produto-tinta` | `#1E1E1E` | Corpo de texto |
| `--produto-tinta-suave` | `#4A4A4A` | Data, contagens, rótulo do histograma |
| `--produto-verde` | `var(--cor-primaria)` | **Só** rótulo de seção e o atalho "Ver mais detalhes" |
| `--produto-vermelho` | `var(--cor-destaque)` | Estrelas, número da média e o botão de carrinho |
| `--produto-regua` | `#D9704F` | As duas réguas de 1px que dividem a página em três faixas |
| `--produto-borda-cartao` | `#DAD4CE` | Contorno do cartão de avaliação |

### Tipografia

Nenhuma família nova: Inter é a voz do site e a página é de leitura. A
personalidade vem da escala, não de mais uma fonte.

| Papel | Ajuste |
|---|---|
| Nome do produto | Inter 300, 34px/1.15, centralizado na coluna — leve e arejado, para contrastar com o peso do botão logo abaixo |
| Rótulo de seção | Inter 700, 16px, verde |
| Corpo | Inter 400, 16px/1.6, medida máxima de 68 caracteres |
| Nota média | Inter 800, 64px, vermelho — o único número grande da página |
| Botão de carrinho | Inter 700, 15px, caixa alta, `letter-spacing: .08em` |
| Data e contagem | Inter 400, 13px, tinta suave |

`Nothing You Could Do`, a manuscrita da marca, **fica de fora**. Ela é a voz dos
títulos de vitrine ("Mais Vendidos", "Categorias") e do selo de favoritos; usá-la
numa tela de texto denso gastaria o único gesto festivo que a marca tem.

### Layout

```
HOME  ›  Doces

┌──────────────────────┐   ┌───────────────────────────────┐
│                      │   │        PÉ DE MOLEQUE          │
│                      │   │        DOCE DE MATAR          │
│        imagem        │   │                               │
│       quadrada       │   │  Características do Produto   │
│                      │   │  resumo em até 160 caracteres │
│                      │   │  Ver mais detalhes ↓          │
└──────────────────────┘   │                               │
                           │  R$ 29,99       [ − 1 + ]     │
                           │  [ ADICIONAR AO CARRINHO 🛒 ] │
                           └───────────────────────────────┘
──────────────────────────── régua ────────────────────────────
Características do Pé de Moleque Doce de Matar
texto corrido, uma coluna, medida máxima de 68 caracteres
──────────────────────────── régua ────────────────────────────
┌────────────────┐   Ordenar por:          [ Relevantes ▾ ]
│ 4,5 ★★★★⯨      │   ┌────────────────────────────────────┐
│ 983 avaliações │   │ Zeca Pagodinho        26 mar. 2026 │
│ 5★ ▇▇▇▇▇▇▇▇    │   │ ★★★★★                              │
│ 4★ ▇▇▇         │   │ comentário                         │
│ 3★ ▇▇          │   │ [ 👍 Útil (3) ]                    │
│ 2★ ▇           │   └────────────────────────────────────┘
│ 1★ ▇           │   …
└────────────────┘            Ver mais
```

Grade de duas colunas (`1fr 1fr` no bloco de compra, `280px 1fr` no de
avaliações), colapsando para coluna única abaixo de 900px. Sem galeria, a imagem
recebe a coluna inteira, limitada a 460px e centralizada, para não virar um
retângulo esticado.

### Elemento-assinatura

**A fileira de estrelas.** Em vez do ícone de meia-estrela do Font Awesome, que
só sabe desenhar 0, 0,5 ou 1, um único *view component* desenha a fileira em SVG
e preenche a estrela parcial por gradiente: 4,5 vira meia estrela de verdade, e
4,2 vira 42% da quinta. A mesma peça serve à média grande, à nota de cada
avaliação e, depois, ao formulário de escrever avaliação. É o detalhe que a
página inteira gira em torno, então é onde vale gastar precisão.

### Movimento e acessibilidade

Um só movimento: `scroll-behavior: smooth` no atalho "Ver mais detalhes",
desligado sob `prefers-reduced-motion`. Nada mais anima além das transições de
`hover` que o site já usa.

- A nota é lida como texto por leitor de tela (`4,5 de 5 estrelas`); o SVG é
  decorativo (`aria-hidden`).
- Cada faixa do histograma é uma linha de lista com o valor em texto.
- O botão de útil usa `aria-pressed` e o foco visível herdado do `site.css`.
- A tela funciona sem JavaScript: só o seletor de quantidade depende dele, e ele
  não bloqueia nada — o carrinho ainda não existe.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/ProdutoTests.cs` | RN-01 |
| Unidade — entidade | `Units/Entities/AvaliacaoTests.cs` | RN-06, RN-07, RN-08, RN-09 |
| Unidade — entidade | `Units/Entities/VotoUtilTests.cs` | Construtor validante |
| Unidade — mapper | `Units/Mappings/ProdutoDetalheMapperTests.cs` | RN-02 |
| Unidade — serviço | `Units/Services/AvaliacaoServiceTests.cs` | RN-03, RN-04, RN-05 e a paginação |
| Unidade — serviço | `Units/Services/ProdutoServiceTests.cs` | RF-03, RF-04 |
| Unidade — validator | `Units/Validators/ProdutoDTOValidatorTests.cs` | RN-01 |
| Unidade — controller | `Units/Controllers/ProdutoControllerTests.cs` | RF-03, RF-17, RF-21 e o saneamento de `exibir` |
| Integração | `Integration/Repositories/AvaliacaoRepositoryIntegrationTests.cs` | Ordenação e contagem por nota no banco |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_ProdutoAtivo_Quando_BuscarDetalhe_Entao_DeveTrazerNomePrecoEResumo` |
| CA-02 | verificação manual, tarefa de fechamento |
| CA-03 | `Dado_ProdutoSemDescricao_Quando_MontarDetalhe_Entao_ResumoDeveSerNulo` |
| CA-04 | `Dado_IdInexistente_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException` |
| CA-05 | `Dado_ProdutoInativo_Quando_BuscarDetalhe_Entao_DeveLancarKeyNotFoundException` |
| CA-06 | `Dado_ProdutoForaDeEstoque_Quando_AbrirDetalhes_Entao_DeveIndicarCompraIndisponivel` |
| CA-07 | `Dado_AvaliacoesComNotasVariadas_Quando_ResumirPorProduto_Entao_DeveCalcularMediaEDistribuicao` |
| CA-08 | `Dado_ProdutoSemAvaliacao_Quando_ResumirPorProduto_Entao_MediaDeveSerNula` |
| CA-09 | `Dado_OitoAvaliacoes_Quando_ListarDez_Entao_TemMaisDeveSerFalso` |
| CA-10 | `Dado_OrdenacaoMaisRecentes_Quando_ListarPorProduto_Entao_DeveTrazerAMaisNovaPrimeiro` |
| CA-11 | `Dado_UsuarioSemVoto_Quando_AlternarVotoUtil_Entao_DeveIncrementarTotalUteis` |
| CA-12 | `Dado_UsuarioJaVotou_Quando_AlternarVotoUtil_Entao_DeveRemoverOVoto` |
| CA-13 | verificação manual, tarefa de fechamento (o `[Authorize]` cobre o servidor) |
| CA-14 | `Dado_AutorDaAvaliacao_Quando_AlternarVotoUtil_Entao_DeveLancarInvalidOperationException` |
| CA-15 | `Dado_DescricaoPreenchida_Quando_Cadastrar_Entao_DevePersistirADescricao` |
| CA-16 | verificação manual, tarefa de fechamento |

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Manter `UpVote` como `bool` na avaliação | Não conta votos nem sabe de quem é o voto. Ficaria mentindo em "Útil (3)" e permitindo votar infinitas vezes |
| Guardar um contador `TotalUteis` na avaliação | Um número gravado diverge da tabela de votos na primeira falha parcial. Derivar de `VotoUtil` custa um `Count` e nunca mente |
| Voto e "Ver mais" por `fetch` devolvendo JSON | Exigiria endpoint de API, estado no cliente e antiforgery manual. A tela é de leitura: recarregar no servidor mantém tudo em uma barreira só (Princípio VII) e funciona sem JavaScript |
| Dois campos no produto, um resumo e uma descrição longa | Dobra o trabalho do administrador para exibir o mesmo texto duas vezes. Uma descrição só, cortada em 160 caracteres na exibição, entrega a mesma tela |
| Reaproveitar `ProdutoDTO` na view, buscando avaliações direto na `.cshtml` | Colocaria consulta e cálculo de média dentro da view. O DTO composto mantém a view burra |
| Nova entidade de imagens para a galeria da referência | Fora de escopo por decisão da loja: sem galeria nesta entrega |
| Rota amigável `/produto/{slug}` | Exigiria campo de *slug* único no produto e tratamento de colisão. A rota convencional já resolve, e URL bonita não é requisito do TCC |
| `estrela.svg` de `wwwroot/images/shapes` para as notas | É a estrela arredondada usada como forma de fundo das categorias, e não lê como estrela de avaliação em 14px |

## 9. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| A `004` não estar implementada quando esta feature começar | **Alta** | **Alto** | A `004` é pré-requisito declarado; sem ela, `Avaliacao` não navega até `Usuario` e o nome do autor não sai. Não iniciar a Fase 3 antes de a `004` estar verde |
| Contar votos por avaliação virar consulta N+1 na lista | Média | Médio | `Include(a => a.Votos)` na consulta paginada, e a lista nunca passa de 100 itens pelo saneamento de `exibir` |
| Histograma dividir por zero em produto sem avaliação | Média | Baixo | RN-03 e RN-04 têm teste dedicado; a tela troca o bloco inteiro pelo estado vazio (RF-18) |
| `exibir` na *query string* virar carga arbitrária | Média | Médio | Saneado no controller: mínimo 5, máximo 100 |
| A régua e o creme desta página vazarem para o resto do site | Baixa | Baixo | Todos os tokens ficam escopados em `.pagina-produto`; o CSS é ligado só nesta view, como já se faz em `cadastro_produto.css` |
| Datas gravadas em hora local e exibidas erradas | Baixa | Médio | `DataCriacao` sempre em UTC na gravação, formatada na view |

## 10. Desvios constitucionais justificados

Nenhum.
