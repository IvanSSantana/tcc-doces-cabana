# Plano Técnico — Catálogo

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-18
**Status:** Rascunho

---

## 1. Resumo da abordagem

Um `CatalogoController` público na raiz — nome liberado pela `011` — com uma
ação `Index` que recebe categoria, subcategorias marcadas e ordenação por
*query string*, no mesmo estilo que a `008` usa na página do produto. Um
`CatalogoService` compõe o `CatalogoDTO` da tela inteira (barra lateral,
categoria atual, marcações, ordenação e grade) a partir de um `ICategoriaService`
novo e de um método novo no `IProdutoRepository`. Nenhuma mudança de esquema:
`Categoria` e `Subcategoria` existem desde a `003` e só recebem dados novos no
seed. A tela é uma única `<form method="get">` que envolve barra lateral e
seletor de ordenação, submetendo-se ao mudar — com botão de fallback em
`<noscript>`, como a `008` já faz.

Três efeitos colaterais deliberados, todos derivados de requisito: o cabeçalho
passa a listar as categorias reais consultando o `ICategoriaService` (RF-02); os
três controles do card ficam desabilitados (RF-17), o que significa apagar o
JavaScript que fingia funcionar; e `BuscarTodosProdutos` passa a filtrar produto
inativo, corrigindo o defeito da seção 10 da spec — a vitrine da home lista
inativos hoje e leva o cliente a um 404.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` nova. Contratos novos em `Application`, implementação em `Infrastructure` |
| II | Domínio rico e auto-validante | n/a | Nenhuma entidade nova ou alterada — `Categoria`, `Subcategoria` e `Produto` já são o que precisam ser |
| III | Validação nas duas barreiras | n/a | Nenhuma entrada de usuário gravada. Os parâmetros de filtro são saneados no controller (categoria inexistente → 404, ordenação inválida → padrão), não validados como formulário |
| IV | Nomenclatura em português | ⬜ OK | `CatalogoService`, `OrdenacaoCatalogo`, `FiltroCatalogoDTO`, `BarraLateral`. `CatalogoController` público não colide com `Areas.Admin.ProdutoController` |
| V | Testes escritos antes | ⬜ OK | Fase 2 inteira vermelha antes da Fase 3 |
| VI | Repositório + commit via UnitOfWork | ⬜ OK (parcial) | `ICategoriaRepository` novo e método novo no `IProdutoRepository`. Nenhuma escrita: a feature só lê, então `IUnitOfWork` não é chamado — não por omissão, por não haver o que gravar. **Sem migration**: nenhuma mudança de esquema, só dados de seed |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | n/a (parcial) | Não existe `POST` nesta feature — o filtro é `GET`, que é o correto para uma consulta que deve poder ser compartilhada e recarregada. A ausência de `[Authorize]` é requisito: o catálogo é público |
| VIII | Tratamento de erro por camada | ⬜ OK | Aplicação lança `KeyNotFoundException` para categoria inexistente; o `FilterException` da `008` traduz para 404. Nenhum `try/catch` em ação |

## 3. Impacto por camada

### `DocesCabana.Domain`

Nenhum arquivo.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/OrdenacaoCatalogo.cs` | criar | `MaisVendidos`, `MelhorAvaliados`, `MenorPreco`, `MaiorPreco`, `NomeAZ` |
| `DTOs/CategoriaDTO.cs` | criar | `CategoriaId`, `Nome`, `Subcategorias` |
| `DTOs/CatalogoDTO.cs` | criar | Compõe a tela: `Categorias`, `CategoriaAtual`, `SubcategoriasMarcadas`, `Ordenacao`, `Produtos` |
| `Contracts/Repositories/ICategoriaRepository.cs` | criar | `BuscarTodasComSubcategorias`, `BuscarPorId` |
| `Contracts/Repositories/IProdutoRepository.cs` | alterar | `BuscarParaCatalogo(Guid? categoriaId, IReadOnlyCollection<Guid> subcategoriaIds, OrdenacaoCatalogo ordenacao)` |
| `Contracts/Services/ICategoriaService.cs` | criar | `ListarComSubcategorias`, `BuscarPorId` |
| `Contracts/Services/ICatalogoService.cs` | criar | `Montar(...)` devolvendo `CatalogoDTO` |
| `Mappings/CategoriaMapper.cs` | criar | `ToDTO`, com as subcategorias aninhadas |
| `Services/CategoriaService.cs` | criar | Consome `ICategoriaRepository` |
| `Services/CatalogoService.cs` | criar | Compõe o `CatalogoDTO`; lança `KeyNotFoundException` para categoria inexistente (RF-05) |
| `Services/ProdutoService.cs` | alterar | `BuscarTodosProdutos` deixa de devolver produto inativo — defeito da spec §10 |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/CategoriaRepository.cs` | criar | `Include(c => c.Subcategorias)`, `AsNoTracking` |
| `Repositories/ProdutoRepository.cs` | alterar | `BuscarParaCatalogo` com o filtro e as quatro ordenações possíveis |
| `Repositories/ProdutoRepository.cs` | alterar | `BuscarTodos` passa a excluir inativo, ou o serviço filtra — decidido na tarefa, o efeito é o mesmo |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar os dois repositórios e os dois serviços novos |
| `Migrations/` | **nada** | Nenhuma mudança de esquema |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/CatalogoController.cs` | criar | `Index` `GET`, pública, sem parâmetro obrigatório |
| `Views/Catalogo/Index.cshtml` | criar | Trilha, `<form method="get">` envolvendo barra lateral e ordenação, grade, estado vazio |
| `Views/Catalogo/_BarraLateral.cshtml` | criar | "Todos" + categorias + caixas de subcategoria |
| `wwwroot/css/pages/catalogo.css` | criar | Tokens escopados em `.pagina-catalogo`; grade de 3 colunas colapsando |
| `ViewComponents/Header.cs` | alterar | Injeta `ICategoriaService` para alimentar o menu (RF-02) |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Os 4 `href="#"` viram as categorias reais, cada uma ligando ao catálogo dela |
| `Views/Home/_Categorias.cshtml` | alterar | Os 4 blocos ligam ao catálogo da categoria (RF-03) — hoje apontam para uma ação que nunca existiu |
| `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | Os três controles ganham `disabled` e rótulo de indisponível (RF-17) |
| `wwwroot/css/components/card-produto.css` | alterar | Estado desabilitado dos três controles |
| `wwwroot/js/components/card-produto.js` | alterar | **Apagar** `adicionarAoCarrinho` e `alternarFavorito` — o teatro documentado na `012`. `alterarQuantidade` também sai, junto do controle |
| `Helpers/DbInitializer.cs` | alterar | Taxonomia nova: 6 categorias, subcategorias redistribuídas, "Salgados" migra para Padaria |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Services/CatalogoServiceTests.cs` | criar | Filtro, ordenação, categoria inexistente, estado vazio |
| `Units/Services/ProdutoServiceTests.cs` | alterar | Teste novo: `BuscarTodosProdutos` não devolve inativo |
| `Units/Controllers/CatalogoControllerTests.cs` | criar | `Index` devolve `ViewResult`; parâmetros ausentes usam o padrão |
| `Integration/Repositories/CatalogoRepositoryIntegrationTests.cs` | criar | As quatro ordenações e o filtro contra SQLite de verdade |
| `E2E/Paginas/PaginaCatalogo.cs` | criar | Objeto de página |
| `E2E/Fluxos/CatalogoTests.cs` | criar | CA-01 a CA-17 |

## 4. Contratos

```csharp
public enum OrdenacaoCatalogo
{
    MaisVendidos,      // anunciada, não oferecida (RF-13/RN-05)
    MelhorAvaliados,   // padrão (RF-14)
    MenorPreco,
    MaiorPreco,
    NomeAZ,
}

public interface ICategoriaService
{
    Task<List<CategoriaDTO>> ListarComSubcategorias();
    Task<CategoriaDTO?> BuscarPorId(Guid categoriaId);
}

public interface ICatalogoService
{
    Task<CatalogoDTO> Montar(
        Guid? categoriaId,
        IReadOnlyCollection<Guid> subcategoriaIds,
        OrdenacaoCatalogo ordenacao);
}

public interface ICategoriaRepository
{
    Task<List<Categoria>> BuscarTodasComSubcategorias();
    Task<Categoria?> BuscarPorId(Guid categoriaId);
}

// Acréscimo ao IProdutoRepository existente
Task<List<Produto>> BuscarParaCatalogo(
    Guid? categoriaId,
    IReadOnlyCollection<Guid> subcategoriaIds,
    OrdenacaoCatalogo ordenacao);
```

Rotas resultantes, sem configuração nova:

```
/Catalogo
/Catalogo?categoria={guid}
/Catalogo?categoria={guid}&subcategorias={guid}&subcategorias={guid}
/Catalogo?categoria={guid}&ordenacao=MenorPreco
```

## 5. Modelo de dados

Nenhuma mudança de esquema. **Sem migration.**

Só dados de seed em `DbInitializer`:

| Categoria | Subcategorias |
|---|---|
| Doces | Doces de Tacho, Doces Caseiros, Doces Zero |
| Padaria | Salgados Assados, Salgados Fritos |
| Adega | Vinhos, Destilados |
| Empório | Geleias e Conservas, Cafés e Chás |
| Bomboniere | Chocolates, Balas e Gomas |
| Souvenir | Lembrancinhas, Cestas |

"Salgados" deixa de ser categoria; suas duas subcategorias passam para Padaria.
Os 6 produtos semeados ficam onde estão (Doces de Tacho), então nenhum fica
órfão. **Impacto em dados existentes:** o banco de desenvolvimento precisa ser
recriado para a taxonomia nova valer — ver risco na seção 8.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — serviço | `Units/Services/CatalogoServiceTests.cs` | RF-05, RF-10, RF-11, RF-18, RF-20 com repositórios mockados |
| Unidade — serviço | `Units/Services/ProdutoServiceTests.cs` | O defeito da §10: inativo fora da vitrine |
| Unidade — controller | `Units/Controllers/CatalogoControllerTests.cs` | `ViewResult`, padrões de parâmetro, ausência de autorização |
| Integração | `Integration/Repositories/CatalogoRepositoryIntegrationTests.cs` | As ordenações e o filtro em SQLite de verdade — em especial "Melhor avaliados" com produto sem avaliação (RN-04), que é a consulta mais frágil |
| E2E | `E2E/Fluxos/CatalogoTests.cs` | Os critérios de navegação, filtro, ordenação e responsividade |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_Visitante_Quando_AbrirOCatalogo_Entao_DeveListarTodosOsProdutosDisponiveis` |
| CA-02 | `Dado_Visitante_Quando_EscolherCategoriaNoCabecalho_Entao_DeveFiltrarPelaCategoria` |
| CA-03 | `Dado_PaginaInicial_Quando_ClicarNumaCategoria_Entao_DeveAbrirOCatalogoDela` |
| CA-04 | `Dado_CategoriaComSubcategorias_Quando_MarcarUma_Entao_DeveMostrarSoOsProdutosDela` |
| CA-05 | `Dado_UmaSubcategoriaMarcada_Quando_MarcarASegunda_Entao_DeveSomarOsProdutosDasDuas` |
| CA-06 | `Dado_SubcategoriasMarcadas_Quando_DesmarcarTodas_Entao_DeveVoltarACategoriaInteira` |
| CA-07 | `Dado_CatalogoCompleto_Quando_OlharABarraLateral_Entao_NaoDeveHaverCaixaDeSubcategoria` |
| CA-08 | `Dado_Catalogo_Quando_OrdenarPorMenorPreco_Entao_DeveListarDoMaisBaratoAoMaisCaro` |
| CA-09 | `Dado_SeletorDeOrdenacao_Quando_TentarEscolherMaisVendidos_Entao_DeveEstarIndisponivel` |
| CA-10 | `Dado_OrdenacaoEscolhida_Quando_TrocarDeCategoria_Entao_DevePreservarAOrdenacao` |
| CA-11 | `Dado_Catalogo_Quando_ClicarNumProduto_Entao_DeveAbrirAPaginaDele` |
| CA-12 | `Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_DevemEstarDesabilitados` |
| CA-13 | `Dado_ProdutoInativo_Quando_AbrirOCatalogoDaCategoriaDele_Entao_NaoDeveAparecer` |
| CA-14 | `Dado_ProdutoForaDeEstoque_Quando_AbrirOCatalogo_Entao_DeveAparecerSinalizado` |
| CA-15 | `Dado_CategoriaSemProduto_Quando_AbrirOCatalogoDela_Entao_DeveMostrarMensagemPropria` |
| CA-16 | `Dado_CategoriaInexistente_Quando_AbrirOCatalogo_Entao_DeveResponder404` |
| CA-17 | `Dado_TelaDe375px_Quando_AbrirOCatalogo_Entao_NaoDeveHaverRolagemHorizontal` |

CA-17 mede o `scrollWidth` de `.pagina-catalogo`, não do documento inteiro —
o cabeçalho compartilhado já estoura a 375px por conta própria, achado
registrado no checklist da `009` e ainda não corrigido.

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Filtrar no navegador, carregando o catálogo inteiro uma vez | Transformaria a tela num aplicativo de página única, sem endereço compartilhável por filtro, e desalinharia do resto do site, que é renderizado no servidor |
| Categoria como segmento de rota (`/Catalogo/{id}`) | O catálogo completo não tem categoria, então o segmento seria opcional — e um segmento opcional no meio da rota confunde mais do que a *query string* resolve. `008` já mistura os dois estilos; aqui tudo em *query string* é mais simples |
| `POST` para aplicar o filtro | Filtro é consulta: precisa ser recarregável, compartilhável e voltar no histórico do navegador. `GET` é o método certo, e por isso a ausência de antiforgery não é desvio |
| Guardar a nota média desnormalizada em `Produto` | Ganho de desempenho irrelevante com este volume, ao custo de um campo que precisa ser mantido sincronizado a cada avaliação nova |
| Corrigir o card mais tarde, junto com o carrinho | Deixaria 12 botões dizendo "Adicionado!" sem adicionar nada. A `009` já estabeleceu que corrigir o que mente é pré-requisito, não escopo extra |

## 8. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Seed não reaplica em banco existente.** `DbInitializer` semeia só quando encontra a tabela vazia; a taxonomia nova não chega a quem já tem `docescabana.db` | Alta | Médio | A tarefa de seed começa apagando o banco de desenvolvimento e subindo do zero; T0nn confirma as 6 categorias ao vivo. Comportamento é o mesmo de toda mudança de seed desde a `003` |
| **Ordenação por nota média com produto sem avaliação.** Um `JOIN` ingênuo descarta o produto sem avaliação em vez de mandá-lo para o fim (RN-04) | Alta | Alto | Subconsulta com média anulável, ordenada com os nulos por último. É o caso que o teste de integração cobre explicitamente — não confiar no comportamento padrão do provedor |
| **`OrdenacaoCatalogo` chegando inválida pela URL** (`?ordenacao=Lixo`) | Média | Baixo | O ligador de modelo do ASP.NET devolve o valor padrão do enum, que é `MaisVendidos` — justamente a indisponível. O controller sanea explicitamente para `MelhorAvaliados`, e há teste de unidade para isso |
| **Cabeçalho passa a consultar categorias em toda tela do site** | Certa | Baixo | São 6 registros com suas subcategorias, consultados sem rastreamento. Não justifica cache agora; se justificar, vira melhoria própria |
| **Vazamento de token CSS** | Média | Médio | Tokens em `.pagina-catalogo`, nunca em `:root` — mesma disciplina da `008` e da `009` |
| **Seletor de elemento sem escopo em `header.css`** afetando a grade | Média | Médio | Achado da `009`: `section { ... }` vale para toda `<section>`. A tela usa `<div>` onde não precisar de marco semântico, e `catalogo.css` reseta o que herdar |

## 9. Desvios constitucionais justificados

*Nenhum.*
