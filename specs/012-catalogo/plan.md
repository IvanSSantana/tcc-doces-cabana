# Plano Técnico — Catálogo

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-18
**Revisado em:** 2026-08-19 · **Status:** Executado

---

## 1. Resumo da abordagem

Um `CatalogoController` público na raiz — nome liberado pela `011` — cuja ação
`Index` recebe a categoria pelo caminho do endereço e o resto por *query
string*, no estilo que a `008` já usa. Um `CatalogoService` compõe o
`CatalogoDTO` da tela inteira (barra lateral, categoria atual, marcações,
ordenação, página e grade) a partir de um `ICategoriaService` novo e de um
método novo no `IProdutoRepository`. A tela é uma única `<form method="get">`
que envolve barra lateral e seletor de ordenação, submetendo-se ao mudar, com
botão de fallback em `<noscript>`; a paginação e os links de categoria são
âncoras comuns. Nada depende de JavaScript (RF-22).

Uma migration, de uma coluna: `Produto.SemAcucar`. Ela é o que permite fundir
"Doces Caseiros" e "Doces Zero" numa categoria só sem perder a informação —
"Barras" e "Potes" existiam nas duas listas, e sem a marcação as versões zero
se tornariam indistinguíveis das comuns.

Três efeitos colaterais deliberados, todos derivados de requisito: o cabeçalho
passa a listar as categorias reais consultando o `ICategoriaService` (RF-03); os
três controles do card ficam desabilitados (RF-24), o que significa apagar o
JavaScript que fingia funcionar; e `BuscarTodosProdutos` passa a filtrar produto
inativo, corrigindo o defeito da seção 10 da spec.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` nova. Contratos em `Application`, implementação em `Infrastructure` |
| II | Domínio rico e auto-validante | ⬜ OK | `Produto` ganha `SemAcucar` com `private set` e um método de intenção (`MarcarComoSemAcucar` / `DesmarcarSemAcucar`), não um setter público |
| III | Validação nas duas barreiras | ⬜ OK (parcial) | `SemAcucar` é booleano: não há formato a validar. Os parâmetros de filtro são saneados no controller (categoria inexistente → 404, ordenação inválida → padrão, página fora do intervalo → limite), não validados como formulário |
| IV | Nomenclatura em português | ⬜ OK | `CatalogoService`, `OrdenacaoCatalogo`, `FiltroCatalogoDTO`, `PaginaDeProdutosDTO`, `BarraLateral`. `CatalogoController` público não colide com `Areas.Admin.ProdutoController` |
| V | Testes escritos antes | ⬜ OK | Fase 2 inteira vermelha antes da Fase 3 |
| VI | Repositório + commit via UnitOfWork | ⬜ OK | `ICategoriaRepository` novo e método novo no `IProdutoRepository`. A feature de catálogo só lê, então não chama `IUnitOfWork` — não por omissão, por não haver o que gravar. O cadastro de produto, que grava `SemAcucar`, já passa pelo `IUnitOfWork` existente. **Uma migration**: `AddProdutoSemAcucar` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK (parcial) | Não existe `POST` no catálogo — filtro é consulta, e `GET` é o método certo para algo que precisa ser recarregável e compartilhável. A ausência de `[Authorize]` é requisito: o catálogo é público |
| VIII | Tratamento de erro por camada | ⬜ OK | Aplicação lança `KeyNotFoundException` para categoria inexistente; o `FilterException` da `008` traduz para 404. Nenhum `try/catch` em ação |

## 3. Direção visual

A referência visual manda. O que segue é a leitura dela em decisões
executáveis.

### Menu suspenso do cabeçalho

Estado fechado: a categoria em texto claro com uma seta para baixo, sobre o
verde da faixa. Ao abrir, **a categoria vira coral e a seta aponta para cima**,
e a faixa de fundo dela passa a bege — o item parece uma aba presa ao painel.
Um painel bege ocupa a largura do conteúdo abaixo da faixa; dentro dele, um
cartão coral de cantos arredondados, alinhado sob a categoria aberta, lista as
subcategorias em texto claro, uma por linha.

```
┌──────────────────────────────────────────────────────┐
│  Doces ⌃  │ Empório ⌄  Adega ⌄  Souvenir ⌄  [Favoritos]│  ← faixa verde
├───────────┴──────────────────────────────────────────┤
│ ┌───────────────┐                                    │
│ │  Barras       │                                    │  ← painel bege
│ │  Bolachas     │   (cartão coral)                   │
│ │  Box          │                                    │
│ └───────────────┘                                    │
└──────────────────────────────────────────────────────┘
```

Abre por foco e por passagem de mouse, sem JavaScript: `:hover` e
`:focus-within` sobre o item, de modo que teclado alcance o mesmo que o mouse.

### Barra lateral

Bloco claro à esquerda com "Categorias" em verde, "Todos" seguido das quatro
categorias, a atual com fundo destacado. Abaixo, separada por um fio, a lista de
caixas de seleção: as oito principais visíveis e as demais dentro de um
`<details>` rotulado "Ver todas" — elemento nativo, que abre e fecha sem
JavaScript e já é anunciado corretamente por leitor de tela. A caixa "Sem
açúcar" fica num terceiro bloco, separada, porque é filtro de outra natureza
(RN-04).

### Paginação

Centralizada abaixo da grade: seta anterior, números, seta seguinte, com a
página atual em coral sólido. Sem "primeira/última" — com três páginas por
categoria não há distância a percorrer.

## 4. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Produto.cs` | alterar | `bool SemAcucar` com `private set`, parâmetro opcional no construtor (por último, para não quebrar chamadas posicionais) e método de intenção para alternar |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/OrdenacaoCatalogo.cs` | criar | `MaisVendidos`, `MelhorAvaliados`, `MenorPreco`, `MaiorPreco`, `NomeAZ` |
| `DTOs/CategoriaDTO.cs` | criar | `CategoriaId`, `Nome`, `Apelido`, `Subcategorias` |
| `DTOs/PaginaDeProdutosDTO.cs` | criar | `Itens`, `PaginaAtual`, `TotalDePaginas`, `TotalDeItens` |
| `DTOs/CatalogoDTO.cs` | criar | Compõe a tela: `Categorias`, `CategoriaAtual`, `SubcategoriasMarcadas`, `ApenasSemAcucar`, `Ordenacao`, `Pagina` |
| `DTOs/ProdutoDTO.cs` | alterar | `SemAcucar` |
| `Contracts/Repositories/ICategoriaRepository.cs` | criar | `BuscarTodasComSubcategorias` |
| `Contracts/Repositories/IProdutoRepository.cs` | alterar | `BuscarPaginaDoCatalogo(filtro, pagina, tamanho)` e `ContarNoCatalogo(filtro)` |
| `Contracts/Services/ICategoriaService.cs` | criar | `ListarComSubcategorias`, `BuscarPorApelido` |
| `Contracts/Services/ICatalogoService.cs` | criar | `Montar(...)` devolvendo `CatalogoDTO` |
| `Mappings/CategoriaMapper.cs` | criar | `ToDTO`, com as subcategorias aninhadas e o apelido calculado |
| `Mappings/ProdutoMapper.cs` | alterar | `SemAcucar` nos dois sentidos |
| `Servicos/Apelido.cs` | criar | Função pura: nome → apelido (minúsculas, sem acento, espaço vira hífen) |
| `Services/CategoriaService.cs` | criar | Consome `ICategoriaRepository`; casa apelido em memória |
| `Services/CatalogoService.cs` | criar | Compõe o `CatalogoDTO`; `KeyNotFoundException` para apelido desconhecido; limita a página ao intervalo válido |
| `Services/ProdutoService.cs` | alterar | `BuscarTodosProdutos` deixa de devolver produto inativo — defeito da spec §10 |
| `Validators/ProdutoDTOValidator.cs` | — | Nada: booleano não tem formato a validar |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/CategoriaRepository.cs` | criar | `Include(c => c.Subcategorias)`, `AsNoTracking` |
| `Repositories/ProdutoRepository.cs` | alterar | `BuscarPaginaDoCatalogo` com filtro, ordenação, `Skip`/`Take`; `ContarNoCatalogo` com o mesmo filtro |
| `DatabaseContext/Configurations/ProdutoConfiguration.cs` | alterar | Mapear `SemAcucar` com padrão `false` |
| `Migrations/` | **criar** | `AddProdutoSemAcucar` — uma coluna, não nula, padrão `false` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar os dois repositórios e os dois serviços novos |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/CatalogoController.cs` | criar | `Index` `GET`, pública, categoria pelo caminho, resto por *query string* |
| `Program.cs` | alterar | Rota `Catalogo/{apelido?}` antes da padrão |
| `Views/Catalogo/Index.cshtml` | criar | Trilha, formulário `GET` envolvendo barra lateral e ordenação, grade, paginação, estado vazio |
| `Views/Catalogo/_BarraLateral.cshtml` | criar | Categorias, caixas de subcategoria com `<details>`, caixa de sem açúcar |
| `Views/Catalogo/_Paginacao.cshtml` | criar | Controles numerados, preservando filtro e ordenação em cada link |
| `wwwroot/css/pages/catalogo.css` | criar | Tokens escopados em `.pagina-catalogo`; grade de 3 colunas colapsando |
| `ViewComponents/Header.cs` | alterar | Injeta `ICategoriaService` para alimentar o menu (RF-03) |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Os 4 `href="#"` viram as categorias reais, cada uma com menu suspenso de até 8 subcategorias |
| `wwwroot/css/components/header.css` | alterar | Estados do menu suspenso conforme §3 |
| `Views/Home/_Categorias.cshtml` | alterar | Os blocos ligam ao catálogo da categoria (RF-05) |
| `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | Os três controles com `disabled` e rótulo de indisponível (RF-24) |
| `wwwroot/css/components/card-produto.css` | alterar | Estado desabilitado dos três |
| `wwwroot/js/components/card-produto.js` | alterar | **Apagar** as três funções — o teatro da spec §10 |
| `Areas/Admin/Views/Produto/Cadastro.cshtml` | alterar | Campo de sem açúcar (RF-29) |
| `Areas/Admin/Controllers/ProdutoController.cs` | alterar | Seletor de subcategoria qualificado por categoria (RF-28) |
| `Helpers/DbInitializer.cs` | alterar | Taxonomia real e 100 produtos, 25 por categoria |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/ProdutoTests.cs` | alterar | `SemAcucar` nasce `false`; método de intenção alterna |
| `Units/Services/CatalogoServiceTests.cs` | criar | Filtro, soma de subcategorias, sem açúcar, paginação, limites, apelido desconhecido |
| `Units/Services/ProdutoServiceTests.cs` | alterar | `BuscarTodosProdutos` não devolve inativo |
| `Units/Controllers/CatalogoControllerTests.cs` | criar | `ViewResult`, padrões de parâmetro, ausência de autorização |
| `Integration/Repositories/CatalogoRepositoryIntegrationTests.cs` | criar | Ordenações, filtro combinado e paginação contra SQLite |
| `E2E/Paginas/PaginaCatalogo.cs` | criar | Objeto de página |
| `E2E/Fluxos/CatalogoTests.cs` | criar | Os critérios de navegação, filtro, ordenação, paginação e responsividade |

## 5. Contratos

```csharp
public enum OrdenacaoCatalogo
{
    MaisVendidos,      // anunciada, não oferecida (RF-16/RN-07)
    MelhorAvaliados,
    MenorPreco,
    MaiorPreco,
    NomeAZ,            // padrão (RF-17)
}

public record FiltroCatalogoDTO(
    Guid? CategoriaId,
    IReadOnlyCollection<Guid> SubcategoriaIds,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao);

public interface ICategoriaService
{
    Task<List<CategoriaDTO>> ListarComSubcategorias();
    Task<CategoriaDTO?> BuscarPorApelido(string apelido);
}

public interface ICatalogoService
{
    Task<CatalogoDTO> Montar(string? apelidoDaCategoria, FiltroCatalogoDTO filtro, int pagina);
}

// Acréscimos ao IProdutoRepository existente
Task<List<Produto>> BuscarPaginaDoCatalogo(FiltroCatalogoDTO filtro, int pagina, int tamanhoDaPagina);
Task<int> ContarNoCatalogo(FiltroCatalogoDTO filtro);
```

Rotas resultantes:

```
/Catalogo
/Catalogo/doces
/Catalogo/doces?subcategorias={guid}&subcategorias={guid}
/Catalogo/doces?semAcucar=true&ordenacao=MenorPreco&pagina=2
```

O apelido é derivado do nome, não guardado: `Apelido.De("Empório")` devolve
`"emporio"`. `BuscarPorApelido` percorre as quatro categorias que a tela já
carrega — sem consulta extra, sem coluna, sem migration por esse motivo.

## 6. Modelo de dados

- **Entidade alterada:** `Produto` ganha `SemAcucar` (booleano, não nulo, padrão
  `false`).
- **Relacionamentos:** nenhum novo. `Categoria → Subcategoria → Produto`
  permanece como está.
- **Migration:** `dotnet ef migrations add AddProdutoSemAcucar --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`
- **Impacto em dados existentes:** nenhum — produtos existentes nascem `false`,
  que é o valor correto para todos eles.

### Seed

| Categoria | Subcategorias | Produtos |
|---|---|---|
| Doces | Barras, Bolachas / Rosquinhas, Box, Combos, Compotas, Cappuccino, Latas, Palhas, Potes, Quindim, Raspa de Tachos, Sorvetes | 25 |
| Empório | Café, Cappuccino, Charcutaria, Croissant, Desidratados, Geleias, Manteiga, Mel, Molho, Risotto | 25 |
| Adega | Cachaça, Licor, Licor Caseiro, Vinhos | 25 |
| Souvenir | Bijuterias, Canecas, Chaveiros, Kits, Pelúcia | 25 |

Dos 25 de Doces, dez marcados como sem açúcar, concentrados nas subcategorias
que vinham de "Doces Zero" (Barras, Potes, Cappuccino, Sorvetes, Combos). Ao
menos um produto inativo e um fora de estoque, para CA-20 e CA-21 terem o que
exercitar.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/ProdutoTests.cs` | `SemAcucar` e seu método de intenção |
| Unidade — serviço | `Units/Services/CatalogoServiceTests.cs` | RF-07, RF-12, RF-13, RF-14, RF-21, RF-25, RF-27 com repositórios mockados |
| Unidade — serviço | `Units/Services/ProdutoServiceTests.cs` | O defeito da §10: inativo fora da vitrine |
| Unidade — controller | `Units/Controllers/CatalogoControllerTests.cs` | `ViewResult`, padrões, saneamento de parâmetro |
| Integração | `Integration/Repositories/CatalogoRepositoryIntegrationTests.cs` | Filtro combinado, ordenações e paginação em SQLite — em especial que a soma das páginas devolve cada produto uma vez (CA-16) |
| E2E | `E2E/Fluxos/CatalogoTests.cs` | Navegação, menu suspenso, filtro, ordenação, paginação, sem JavaScript, 375px |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_Visitante_Quando_AbrirOCatalogo_Entao_DeveListarAPrimeiraPagina` |
| CA-02 | `Dado_Visitante_Quando_EscolherCategoriaNoCabecalho_Entao_DeveFiltrarPelaCategoria` |
| CA-03 | `Dado_CatalogoDeEmporio_Quando_OlharOEndereco_Entao_DeveConterOApelidoLegivel` |
| CA-04 | `Dado_CategoriaComDozeSubcategorias_Quando_AbrirOMenu_Entao_DeveMostrarOito` |
| CA-05 | `Dado_CategoriaComQuatroSubcategorias_Quando_AbrirOMenu_Entao_DeveMostrarAsQuatro` |
| CA-06 | `Dado_UmaSubcategoriaMarcada_Quando_MarcarASegunda_Entao_DeveSomarOsProdutosDasDuas` |
| CA-07 | `Dado_SubcategoriasMarcadas_Quando_DesmarcarTodas_Entao_DeveVoltarACategoriaInteira` |
| CA-08 | `Dado_MaisDeOitoSubcategorias_Quando_AcionarVerTodas_Entao_DeveRevelarSemRecarregar` |
| CA-09 | `Dado_CatalogoDeDoces_Quando_MarcarSemAcucar_Entao_DeveListarSoOsMarcados` |
| CA-10 | `Dado_SemAcucarMarcado_Quando_MarcarTambemUmaSubcategoria_Entao_DeveCombinarOsDois` |
| CA-11 | `Dado_CatalogoCompleto_Quando_OlharABarraLateral_Entao_NaoDeveHaverCaixaDeSubcategoria` |
| CA-12 | `Dado_Catalogo_Quando_OrdenarPorMenorPreco_Entao_DeveListarDoMaisBaratoAoMaisCaro` |
| CA-13 | `Dado_SeletorDeOrdenacao_Quando_TentarEscolherMaisVendidos_Entao_DeveEstarIndisponivel` |
| CA-14 | `Dado_OrdenacaoEscolhida_Quando_TrocarDeCategoriaEDePagina_Entao_DevePreservarAOrdenacao` |
| CA-15 | `Dado_CategoriaComMaisDeDozeProdutos_Quando_IrParaASegundaPagina_Entao_DeveMostrarOutrosProdutos` |
| CA-16 | `Dado_TodasAsPaginasDeUmaCategoria_Quando_Percorridas_Entao_CadaProdutoApareceUmaVez` |
| CA-17 | `Dado_PaginaAlemDoTotal_Quando_Solicitada_Entao_DeveMostrarAUltimaValida` |
| CA-18 | `Dado_Catalogo_Quando_ClicarNumProduto_Entao_DeveAbrirAPaginaDele` |
| CA-19 | `Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_DevemEstarDesabilitados` |
| CA-20 | `Dado_ProdutoInativo_Quando_AbrirCatalogoEVitrine_Entao_NaoDeveAparecerEmNenhum` |
| CA-21 | `Dado_ProdutoForaDeEstoque_Quando_AbrirOCatalogo_Entao_DeveAparecerSinalizado` |
| CA-22 | `Dado_FiltrosSemResultado_Quando_Aplicados_Entao_DeveMostrarMensagemPropria` |
| CA-23 | `Dado_ApelidoInexistente_Quando_AbrirOCatalogo_Entao_DeveResponder404` |
| CA-24 | `Dado_CappuccinoEmDuasCategorias_Quando_AbrirOSeletorDoCadastro_Entao_DeveQualificarPorCategoria` |
| CA-25 | `Dado_JavaScriptDesligado_Quando_FiltrarOrdenarEPaginar_Entao_TudoDeveFuncionar` |
| CA-26 | `Dado_TelaDe375px_Quando_AbrirOCatalogo_Entao_NaoDeveHaverRolagemHorizontal` |

CA-26 mede o `scrollWidth` de `.pagina-catalogo`, não do documento inteiro — o
cabeçalho compartilhado já estoura a 375px por conta própria, achado registrado
no checklist da `009` e ainda não corrigido.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Coluna de apelido em `Categoria` | Existiria para evitar um laço sobre quatro itens que a tela já carregou. A comparação precisa ser em memória de qualquer forma, porque não há jeito portátil de pedir ao SQL "ache o nome sem acento e em minúsculas" |
| Manter "Doces Caseiros" e "Doces Zero" separadas | "Zero" descreve o produto, não onde ele fica. E "Barras" e "Potes" existiam nas duas: separadas, a loja mantém dois nomes para a mesma coisa; fundidas sem marcação, perde-se a informação |
| "Zero" como subcategoria dentro de Doces | Não resolve: um produto seria "Barras" **ou** "Zero", nunca os dois. É a mesma limitação que impediria sem glúten e sem lactose depois |
| Contador desnormalizado para "mais vendidos" | Precisa incrementar no fechamento **e** decrementar no cancelamento — fonte clássica de número corrompido em silêncio. E um contador não sabe *quando* a venda foi, inviabilizando a janela de 90 dias |
| Rolagem infinita ou "carregar mais" | Nenhuma posição da lista teria endereço próprio, e a primeira exige JavaScript sem alternativa. Páginas numeradas são compartilháveis e voltam no histórico |
| Filtrar no navegador, carregando tudo | Cem produtos hoje, centenas depois. E quebraria o padrão renderizado no servidor das specs 008 e 009 |
| "Ver todas" com JavaScript | `<details>` faz o mesmo nativamente, sem script, e já é anunciado por leitor de tela |
| Corrigir o card mais tarde, junto com o carrinho | Deixaria doze botões dizendo "Adicionado!" sem adicionar nada. A `009` já estabeleceu que corrigir o que mente é pré-requisito |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Seed não reaplica em banco existente.** `DbInitializer` semeia só quando a tabela está vazia; a taxonomia nova não chega a quem já tem `docescabana.db` | Alta | Médio | A tarefa de seed começa apagando o banco de desenvolvimento; T0nn confirma as quatro categorias ao vivo. Mesmo comportamento de toda mudança de seed desde a `003` |
| **Paginação com ordenação instável.** Ordem que empata faz produto aparecer em duas páginas ou em nenhuma | Alta | Alto | RN-05 obriga ordem sem empate; toda ordenação recebe `Nome` como desempate final. CA-16 é o teste que trava isso, e roda contra SQLite de verdade |
| **`Skip`/`Take` sem `OrderBy` determinístico** produz resultado indefinido no provedor | Média | Alto | Mesma mitigação: nunca paginar sem ordenação completa. O teste de integração percorre todas as páginas e compara o conjunto |
| **Menu suspenso sem JavaScript preso aberto no toque.** `:hover` em tela sensível ao toque se comporta de forma imprevisível | Alta | Médio | Em telas estreitas o menu vira lista expansível com `<details>`, sem depender de `:hover`; `:focus-within` cobre teclado |
| **Colisão de apelido** entre duas categorias | Baixa | Médio | Quatro nomes escolhidos a dedo. Um teste de unidade garante que os apelidos das categorias semeadas continuam distintos |
| **Cabeçalho consulta categorias em toda tela do site** | Certa | Baixo | Quatro registros com subcategorias, sem rastreamento. Não justifica cache agora |
| **Vazamento de token CSS** | Média | Médio | Tokens em `.pagina-catalogo`, nunca em `:root` — disciplina da `008` e da `009` |
| **Seletor `section` sem escopo em `header.css`** afetando a grade | Média | Médio | Achado da `009`: `section { ... }` vale para toda `<section>`. Usar `<div>` onde não houver marco semântico, e resetar em `catalogo.css` |

## 10. Desvios constitucionais justificados

*Nenhum.*
