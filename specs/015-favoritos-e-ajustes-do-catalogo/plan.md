# Plano Técnico — Favoritos e ajustes do catálogo

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-21
**Status:** Executado

---

## 1. Resumo da abordagem

**Favoritos é um vertical completo sem esquema novo.** `Favorito` já existe no
domínio, com chave composta `(ProdutoId, UsuarioId)` que *já* impede o par
duplicado, e a tabela foi criada pela migration `AddRemainingDomainEntities`
(spec 003). Entram apenas contrato de repositório, repositório, serviço,
controlador e telas. **Nenhuma migration.**

**O coração é um botão de envio dentro de um formulário.** Sem JavaScript ele
posta, alterna e redireciona de volta — POST-Redirect-Get, como o Princípio VII
pede. Com JavaScript, um script intercepta, posta por `fetch` e troca o ícone no
lugar. É o mesmo desenho da `014`: o caminho degradado **é** o código real, não
uma promessa paralela.

**A intenção do visitante viaja no navegador.** Ao clicar sem estar autenticado,
o script guarda o produto pretendido em `sessionStorage`, abre o modal e
acrescenta o endereço atual como retorno. Depois do login o navegador volta ao
catálogo, o script vê a intenção pendente e a conclui. Isso exige que o login
saiba voltar — e hoje ele não sabe.

**O login ganha endereço de retorno.** `AutenticacaoController.Login` termina
sempre em `RedirectToAction("Index", "Home")`; passa a aceitar `returnUrl`,
guardado por `Url.IsLocalUrl` contra redirecionamento para fora do site (RN-04).

**O cartão do catálogo muda quase só por CSS.** `display: contents` no bloco de
ações dissolve o agrupamento, e o cartão vira uma grade de duas colunas: imagem
e nome atravessam as duas, preço fica numa e o seletor na outra, e o botão volta
a atravessar. A marcação muda em dois pontos apenas — o rótulo do botão, que
vira parâmetro do componente, e o `ToUpper()` do nome, que sai da view para o
CSS, onde apresentação deveria estar desde o início.

**Os dois ajustes menores não precisam de script.** A trilha é `text-transform`
mais uma classe no item final. O "Ver todas" continua sendo `<details>` nativo:
a coluna vira flexível e o `<summary>` recebe `order: 1`, o que o desloca para
depois do conteúdo revelado; o rótulo alterna por `[open]`. O piso sem-JavaScript
da `014` sai intacto.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` tocada. `IFavoritoRepository` e `IFavoritoService` ficam em `Application/Contracts`; a implementação do repositório, em `Infrastructure` |
| II | Domínio rico e auto-validante | ⬜ OK | `Favorito` não muda: já valida as duas chaves no construtor e não tem estado mutável — favorito existe ou não existe |
| III | Validação nas duas barreiras | ⬜ OK (parcial) | Não há formulário com campos a validar: favoritar carrega um identificador, não dados digitados. `returnUrl` é guardado na borda web por `Url.IsLocalUrl` — é defesa contra endereço hostil, não validação de entrada do usuário. Ver §10 |
| IV | Nomenclatura em português | ⬜ OK | `IFavoritoRepository`, `FavoritoService`, `FavoritoController`, `favorito.js`, `AlternarFavorito` |
| V | Testes escritos antes | ⬜ OK | Cada frente tem fase vermelha própria |
| VI | Repositório + commit via UnitOfWork | ⬜ OK | `FavoritoService` grava por `IFavoritoRepository` e fecha com `IUnitOfWork.SalvarAlteracoes`. Sem migration: a tabela existe desde a `003` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK | `[HttpPost]` + `[ValidateAntiForgeryToken]` no alternar; `[Authorize]` na lista. PRG no caminho sem script. Ver §10 sobre o caminho assíncrono |
| VIII | Tratamento de erro por camada | ⬜ OK | Produto inexistente ou fora do catálogo público vira `KeyNotFoundException` na aplicação, capturada pelo filtro global |

## 3. Direção visual

A referência manda. Abaixo, a leitura dela em decisões verificáveis, ao lado do
que a `014` deixou.

```
HOJE (pós-014)                     REFERÊNCIA
┌───────────────────┐              ┌───────────────────┐
│        ♡ (hover)  │              │ ┌───────────────┐ │
│   ┌───────────┐   │              │ │  fundo cinza  │ │  imagem sobre
│   │  imagem   │   │              │ │    imagem   ♡ │ │  painel próprio
│   └───────────┘   │              │ └───────────────┘ │
│   NOME EM CAIXA   │              │  Nome em caixa    │  caixa normal
│       ALTA        │              │  normal, 2 linhas │
│      R$ 19,90     │              │ R$ 19,99  [− 1 +] │  mesma linha
│  [− 1 +] [Adic.]  │              │ ┌───────────────┐ │
│                   │              │ │Adicionar ao 🛒│ │  faixa larga
└───────────────────┘              │ └───────────────┘ │  na base
                                   └───────────────────┘
```

| Decisão | Hoje | Depois |
|---|---|---|
| Fundo do cartão | transparente; borda só no *hover* | branco com borda visível em repouso |
| Fundo da imagem | herdado do cartão | painel cinza próprio, cantos arredondados no topo |
| Nome | caixa alta, via Razor | caixa normal, transformação no CSS |
| Preço e seletor | linhas separadas | mesma linha, preço à esquerda |
| Botão de carrinho | pequeno, ao lado do seletor | faixa larga na base, cor de destaque, com ícone |
| Coração | *hover*, canto do cartão | sobre a imagem; *hover* no ponteiro fino, fixo no grosso |
| Trilha | caixa baixa, tudo igual | caixa alta, último item em destaque |

Nenhuma cor nova entra: o botão usa `--cor-destaque`, que já é o laranja do
tema, e o mesmo laranja passa a marcar o fim da trilha. Nenhuma fonte nova.

**A distinção entre ponteiro fino e grosso é `@media (hover: hover)`**, não
largura de tela: um tablet largo tem tela grande e nenhum *hover*. Medir a
capacidade certa é o que faz RF-05 valer no aparelho real, não só no
redimensionamento da janela.

## 4. Impacto por camada

### `DocesCabana.Domain`

Nenhum arquivo.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Contracts/Repositories/IFavoritoRepository.cs` | **criar** | `BuscarPorUsuario`, `Buscar(produtoId, usuarioId)`, `IdsPorUsuario` |
| `Contracts/Services/IFavoritoService.cs` | **criar** | `Alternar`, `ListarDoUsuario` |
| `Services/FavoritoService.cs` | **criar** | Regra do interruptor (RN-01), recusa de produto fora do catálogo público, commit por `IUnitOfWork` |
| `Mappings/ProdutoMapper.cs` | alterar | Sobrecarga que recebe o conjunto de identificadores favoritados e preenche `EstaFavorito` |
| `Contracts/Services/ICatalogoService.cs` | alterar | `Montar` passa a receber o identificador de quem vê (opcional) |
| `Services/CatalogoService.cs` | alterar | Busca os favoritos da página de uma vez e repassa ao mapeador |

`ProdutoDTO.EstaFavorito` **já existe** e nunca foi preenchido por ninguém —
esta feature é a primeira a usá-lo. Nenhum campo novo no DTO.

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/FavoritoRepository.cs` | **criar** | Consulta por usuário com `Include` do produto; verificação de par |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registro do repositório e do serviço |

**Nenhuma migration.** A tabela `Favorito` foi criada pela migration
`20260812114935_AddRemainingDomainEntities`, com a chave composta que garante
RN-01 no banco.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/FavoritoController.cs` | **criar** | `Index` com `[Authorize]`; `Alternar` em `POST` com antiforgery, respondendo redirecionamento ou estado conforme o pedido |
| `Controllers/CatalogoController.cs` | alterar | Repassa o identificador de quem vê ao serviço |
| `Controllers/AutenticacaoController.cs` | alterar | `Login` aceita e honra `returnUrl`, guardado por `Url.IsLocalUrl` |
| `Views/Autenticacao/Login.cshtml` | alterar | Campo oculto que devolve o `returnUrl` no envio |
| `Views/Favorito/Index.cshtml` | **criar** | Grade dos favoritos e estado vazio |
| `ViewComponents/CardProduto.cs` | alterar | Parâmetro do rótulo do botão de carrinho |
| `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | Coração vira botão de envio em formulário; nome perde `ToUpper()`; rótulo parametrizado |
| `Views/Shared/Components/VitrineProdutos/Default.cshtml` | alterar | Deixa de passar `estaFavorito`, que o componente não recebe (RF-27) |
| `Views/Catalogo/Index.cshtml` | alterar | Classe no último item da trilha |
| `Views/Catalogo/_BarraLateral.cshtml` | alterar | Dois rótulos no controle de revelar, alternados por estado |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Remove o `<script>` para arquivo inexistente e o `<dialog>` vazio; acrescenta atalho para o cadastro de produto |
| `wwwroot/js/components/favorito.js` | **criar** | Interceptação, `fetch`, troca do ícone, intenção pendente do visitante |
| `wwwroot/css/components/card-produto.css` | alterar | Base ganha `text-transform` do nome; nada mais muda para o carrossel |
| `wwwroot/css/pages/catalogo.css` | alterar | Desenho do cartão no catálogo, trilha, controle de revelar |
| `wwwroot/css/pages/favoritos.css` | **criar** | Grade e estado vazio |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Services/FavoritoServiceTests.cs` | **criar** | Interruptor liga e desliga; produto inexistente; produto fora do catálogo público |
| `Units/Controllers/AutenticacaoControllerTests.cs` | alterar/criar | Retorno local é honrado; retorno externo é descartado |
| `Integration/Repositories/FavoritoIntegrationTests.cs` | **criar** | Chave composta recusa par repetido; lista por usuário não vaza a de outro |
| `E2E/Paginas/PaginaFavoritos.cs` | **criar** | Objeto de página da lista |
| `E2E/Paginas/PaginaCatalogo.cs` | alterar | Localizadores do coração e do controle de revelar |
| `E2E/Fluxos/FavoritosTests.cs` | **criar** | CA-01 a CA-15 |
| `E2E/Fluxos/CatalogoTests.cs` | alterar | CA-16 a CA-22 |

## 5. Contratos

```csharp
public interface IFavoritoRepository
{
    Task<List<Favorito>> BuscarPorUsuario(Guid usuarioId);
    Task<Favorito?> Buscar(Guid produtoId, Guid usuarioId);
    Task<HashSet<Guid>> IdsPorUsuario(Guid usuarioId, IEnumerable<Guid> produtoIds);
}

public interface IFavoritoService
{
    // Devolve o estado resultante: true = passou a favorito.
    Task<bool> Alternar(Guid produtoId, Guid usuarioId);
    Task<List<ProdutoDTO>> ListarDoUsuario(Guid usuarioId);
}

// Passa a saber quem está vendo, para preencher EstaFavorito (RF-02).
// Nulo = visitante, e aí nenhum cartão vem marcado.
Task<CatalogoDTO> Montar(string? apelidoDaCategoria, FiltroCatalogoDTO filtro, int pagina, Guid? usuarioId = null);
```

`IdsPorUsuario` recebe os identificadores da página e devolve só os favoritados
entre eles: **uma consulta por página**, não uma por cartão. Com doze cartões, a
diferença entre uma e treze idas ao banco é a diferença entre uma grade e um
problema.

## 6. Modelo de dados

Nenhuma mudança. A tabela `Favorito` existe desde a spec `003`, com chave
primária composta `(ProdutoId, UsuarioId)` — que é, ela própria, a garantia de
RN-01 no nível que nenhum caminho de código contorna.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade | `FavoritoServiceTests` | RN-01 (interruptor), recusa de produto inexistente e de produto fora do catálogo público |
| Unidade | `AutenticacaoControllerTests` | RF-13 e RF-14 — retorno local honrado, externo descartado |
| Integração | `FavoritoIntegrationTests` | RN-01 no banco e RN-02 (lista de um não traz a de outro) |
| E2E | `FavoritosTests` | O resto: só o navegador sabe se a página recarregou, se o ícone mudou no lugar e se a intenção sobreviveu ao login |
| E2E | `CatalogoTests` | Cartão, trilha, revelar/recolher, e a não-regressão do carrossel |

Mapeamento critério → teste:

| Critério | Teste |
|---|---|
| CA-01, CA-02 | `Dado_ProdutoNaoFavoritado_Quando_Favoritar_Entao_DeveMarcarEDesmarcarNoMesmoControle` |
| CA-03 | `Dado_ProdutoFavoritado_Quando_RecarregarOCatalogo_Entao_DeveContinuarMarcado` |
| CA-04 | `Dado_ClienteAutenticado_Quando_Favoritar_Entao_NaoDeveRecarregarAPagina` |
| CA-05 | `Dado_JavaScriptDesligado_Quando_Favoritar_Entao_DeveFuncionarEVoltarAListagem` |
| CA-06 | `Dado_TelaSensivelAoToque_Quando_AbrirOCatalogo_Entao_OControleDeveEstarVisivel` |
| CA-07 | `Dado_Visitante_Quando_TentarFavoritar_Entao_DeveSerConvidadoAEntrarSemGravar` |
| CA-08 | `Dado_VisitanteQueTentouFavoritar_Quando_Entrar_Entao_OProdutoDeveEstarFavoritado` |
| CA-09 | `Dado_ProdutosFavoritados_Quando_AbrirALista_Entao_DeveMostrarExatamenteEles` |
| CA-10 | `Dado_FavoritoQueSaiuDoCatalogo_Quando_AbrirALista_Entao_NaoDeveAparecerEVoltaSeReativado` |
| CA-11 | `Dado_ListaDeFavoritos_Quando_Desfavoritar_Entao_DeveSairDaListaSemRecarregar` |
| CA-12 | `Dado_NenhumFavorito_Quando_AbrirALista_Entao_DeveOferecerCaminhoParaOCatalogo` |
| CA-13 | `Dado_Visitante_Quando_AbrirALista_Entao_DeveSerLevadoAEntrar` |
| CA-14 | `Dado_LoginAPartirDoCatalogo_Quando_Entrar_Entao_DeveVoltarAoCatalogo` |
| CA-15 | `Dado_RetornoExterno_Quando_Entrar_Entao_DeveIrParaAPaginaInicial` |
| CA-16 | `Dado_CatalogoAberto_Quando_MedirOCartao_Entao_DeveSeguirOArranjoDaReferencia` |
| CA-17 | `Dado_CatalogoAberto_Quando_LerONomeDoProduto_Entao_NaoDeveEstarTodoEmMaiusculas` |
| CA-18 | `Dado_PaginaInicial_Quando_OlharOCarrossel_Entao_NaoDeveTerRegredido` (já existe) |
| CA-19 | `Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_DevemEstarDesabilitados` (já existe) |
| CA-20 | `Dado_CatalogoDeCategoria_Quando_OlharATrilha_Entao_DeveEstarEmCaixaAltaComUltimoDestacado` |
| CA-21 | `Dado_MaisDeOitoSubcategorias_Quando_Revelar_Entao_OControleDeveIrParaOFimEOferecerRecolher` |
| CA-22 | `Dado_JavaScriptDesligado_Quando_RevelarERecolher_Entao_DeveFuncionarNosDoisSentidos` |
| CA-23 | `Dado_TelasDaLoja_Quando_ObservarAsRequisicoes_Entao_NenhumaDeveTerminarEmNaoEncontrado` |
| CA-24 | `Dado_Administrador_Quando_ProcurarOCadastroDeProduto_Entao_DeveHaverCaminhoDeNavegacao` |

**CA-23 usa observação de respostas do navegador** (`Page.Response`), acumulando
os códigos de cada requisição e falhando se alguma voltar 404. É o único jeito
de provar que o `<script>` morto sumiu — o olho não vê um arquivo que não
carregou.

**CA-08 é o teste mais frágil desta feature** e o que mais importa acertar:
envolve três navegações e um estado guardado no navegador. Ele é escrito por
último, depois de CA-07 e CA-14 estarem verdes, para que uma falha nele signifique
"a intenção não sobreviveu", e não "o convite ou o retorno não funcionam".

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Guardar a intenção do visitante no servidor (sessão ou `TempData`) | Funcionaria também sem script, mas acopla a tela de login ao favorito: o controlador de autenticação passaria a conhecer um domínio que não é dele. Como o convite em si já é recurso de script, o ganho é pequeno e o acoplamento é permanente |
| Completar a intenção por um endereço tipo `?favoritar={id}` | Seria um `GET` que altera estado — exatamente o que o Princípio VII proíbe, e o que faria um pré-carregador de navegador favoritar sozinho |
| `[Authorize]` no alternar, deixando o Identity redirecionar | O pedido assíncrono receberia o HTML da tela de login com código 200 e não teria como distinguir sucesso de "precisa entrar". Verificar autenticação na própria ação permite responder 401, que o script entende |
| Componente próprio para o cartão do catálogo | Duas marcações do mesmo produto para manter em sincronia. O arranjo da referência cabe em CSS sobre a marcação atual |
| `text-transform: lowercase` para desfazer a caixa alta do nome | Destruiria as maiúsculas legítimas — "Doce de Leite Aviação" viraria tudo minúsculo. A caixa tem de sair da view, não ser desfeita depois |
| Script para mover o "Ver todas" para o fim | `order` em coluna flexível faz o mesmo sem uma linha de JavaScript, e sem quebrar o `<details>` nativo que a `012` escolheu de propósito |
| Largura de tela para decidir se o coração fica fixo | Tablet largo tem tela grande e nenhum *hover*. `@media (hover: hover)` mede a capacidade certa |
| Unificar o cartão do carrossel com o do catálogo | Oferecido e descartado pelo responsável (spec §10). Revogaria a RF-11 da `014` e o teste que a protege |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **`display: contents` quebra o cartão em navegador antigo**, achatando o arranjo | Baixa | Alto | O teste de CA-16 mede as caixas resolvidas no navegador de verdade, não a regra CSS. Se o arranjo não se formar, ele falha |
| **O cartão do carrossel muda sem querer**: base e catálogo compartilham arquivo, e um seletor mal escopado vaza | Média | Alto | Toda regra nova do desenho mora em `catalogo.css` sob a grade do catálogo; `card-produto.css` só recebe a transformação de caixa. O teste de não-regressão da `014` roda em tarefa própria |
| **A intenção pendente dispara na hora errada** — em outra aba, depois de muito tempo, ou para produto que sumiu | Média | Médio | `sessionStorage` é por aba e morre com ela; a intenção é limpa assim que consumida; produto que não existe mais devolve erro tratado e a intenção é descartada |
| **Favoritar o mesmo produto duas vezes em corrida** (duplo clique, aba dupla) | Média | Baixo | A chave composta recusa no banco; o serviço lê antes de gravar e o segundo pedido apenas desfaz. O pior caso é o estado final ser o oposto do esperado, nunca dado corrompido |
| **`returnUrl` vira trampolim para outro site** | Baixa | Alto | `Url.IsLocalUrl` antes de qualquer redirecionamento, com teste dedicado (CA-15). É requisito, não zelo |
| **A consulta de favoritos multiplica as idas ao banco** se feita por cartão | Média | Médio | `IdsPorUsuario` recebe os doze identificadores e responde uma vez. O contrato foi desenhado para tornar o erro difícil |
| **Remover o `<script>` morto quebra algo que dependia dele** | Muito baixa | Médio | O arquivo não existe: nada pode depender do que nunca carregou. A função `abrirModal` vem de outro arquivo, carregado pelo layout |

## 10. Desvios constitucionais justificados

**Princípio VII — o caminho assíncrono não redireciona.**

POST-Redirect-Get existe para que recarregar a página depois de um envio não
repita o envio. Um pedido assíncrono não cria entrada no histórico do
navegador: não há o que recarregar, e portanto não há o que repetir. O caminho
sem script — que é o que um navegador pode de fato recarregar — **redireciona
normalmente**. O princípio é atendido onde ele tem efeito.

**Princípio III — não há validador de entrada para favoritar.**

O princípio pede a regra nas duas barreiras. Favoritar não recebe dados
digitados: recebe um identificador que o próprio sistema imprimiu no cartão.
Não há formato a conferir nem mensagem de campo a devolver. O que existe de
verificável — o produto existe? está disponível ao público? — é invariante de
aplicação, e vive no serviço, que lança `KeyNotFoundException` como o Princípio
VIII manda.

A guarda de `returnUrl` **não** é validação de entrada no sentido do princípio:
é defesa da borda web contra endereço hostil, e mora onde o redirecionamento
acontece, porque é lá que o risco existe.
