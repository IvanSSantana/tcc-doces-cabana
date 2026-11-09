# Plano — Gestão de produtos do administrador

**ID:** 031-gestao-de-produtos-admin · **Criado em:** 2026-11-09
**Status:** Rascunho

---

## 1. Objetivo

Hoje o administrador só consegue cadastrar produto (`/Admin/Produto/Cadastro`).
Não existe tela para ver, corrigir ou retirar um produto da loja. Esta feature
cria uma página única de gestão em `/Admin/Produto`, com estes elementos:

- uma tabela paginada de produtos;
- um filtro "Todos" / "Inativos";
- um botão "Novo produto" no topo;
- três ações por linha: ver, editar e excluir. Ver, editar e criar abrem em
  modal.

"Excluir" é inativação (soft delete). O produto sai da loja e o histórico de
pedidos continua intacto.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | **Excluir = inativar.** `IProdutoService.Remover` dá lugar a `Inativar(Guid)`, que chama `produto.AlterarStatus(ProdutoStatus.Inativo)` e `SalvarAlteracoes`. Inativar um produto já inativo não faz nada e não lança erro. | Decisão do debate. Além disso, o `DELETE` real nunca funcionaria: `ItemPedido`, `ItemCarrinho`, `Favorito`, `Avaliacao` e `Estoque` referenciam `Produto` com `DeleteBehavior.Restrict`. Vitrine, catálogo, favoritos e carrinho já tratam `Inativo` como fora da loja (`ProdutoRepository.ConstruirConsulta`, `FavoritoService`, `CarrinhoMapper` → `ForaDoCatalogo`), então nada mais precisa mudar. | Manter `Remover` com `DELETE`: falha com erro de FK em qualquer produto que já teve movimento. Manter os dois métodos: `Remover` não tem consumidor e fica como armadilha. |
| D2 | **Filtro com duas opções:** `Todos` (padrão) mostra `Ativo` + `ForaDeEstoque`, e `Inativos` mostra **somente** `Inativo`. Inativos nunca aparecem em `Todos`. | Decisão do debate: inativos não são exibidos por padrão, e a opção "Inativos" mostra só eles. | Mostrar inativos em `Todos` com selo (proposta inicial, rejeitada no debate). |
| D3 | **Ver, editar e novo abrem em modal**, com `<dialog>` nativo, o mesmo recurso do `_ModalLogin`. | Decisão do debate. `<dialog>` já é o padrão da base e resolve foco e `Esc` sem biblioteca. | Páginas separadas para editar e ver; o novo continuaria na página atual de cadastro. Descartado no debate. |
| D4 | **Formulário único para novo e editar** em `Areas/Admin/Views/Produto/_FormularioProduto.cshtml`. A diferença entre os dois é o `ProdutoId` (vazio = novo), que define a ação de destino e o texto do botão. | Evita duplicar as 147 linhas do formulário atual. As regras de campo são as mesmas nos dois casos. | Dois partials quase idênticos. |
| D5 | **O conteúdo do modal vem do servidor como partial via `fetch`:** `GET Cadastro`, `GET Edicao/{id}` e `GET Detalhe/{id}` retornam `PartialView`. O envio também é por `fetch` com `FormData`. Em caso de erro de validação, o servidor responde **400 com o partial preenchido** (`ModelState` intacto) e o JS troca o conteúdo do modal. Em caso de sucesso, o servidor responde **`RedirectToAction(Index)`** com `TempData["Confirmacao"]`. O `fetch` segue o redirect e o JS faz `window.location = resposta.url`. | Mantém o POST-Redirect-Get e o `if (!ModelState.IsValid)` do Princípio VII. As mensagens de campo continuam sendo as do FluentValidation, renderizadas pelo Razor (Princípio III). É o mesmo padrão de `fetch` + partial com `X-Requested-With` do `catalogo.js`. | Renderizar todos os dados na página e montar o modal em JS: a validação do servidor não volta para o campo. POST tradicional que reabre o modal em erro: recarrega a listagem a cada erro e obriga a repassar filtro e página por campo oculto. |
| D6 | **Na edição, a imagem é opcional.** Sem arquivo, a imagem atual é mantida. Com arquivo, ele passa pelo `ImagemParaEnvioDTOValidator` e pelo `IArmazenamentoDeImagem`, exatamente como no cadastro. A imagem antiga **não** é apagada do Supabase. | Corrigir o preço não deveria obrigar a reenviar a foto. Apagar do armazenamento é efeito colateral irreversível, sem pedido no debate. | Imagem obrigatória na edição. Apagar a imagem antiga ao trocar. |
| D7 | **`ProdutoService.Atualizar` é reescrito:** busca a entidade rastreada (`BuscarPorId` usa `FindAsync`) e aplica os métodos de intenção: `AlterarNome`, `AlterarPreco`, `AlterarSubcategoriaId`, `AlterarDescricao`, `AlterarDimensoes`, `AlterarImagem`, `AlterarStatus` e `MarcarComoSemAcucar`/`DesmarcarSemAcucar`. Depois chama `SalvarAlteracoes`, **sem** `_produtoRepository.Atualizar`. | O código atual faz `Update` numa **segunda** instância com a mesma chave, enquanto a primeira já está rastreada. O EF lança "another instance with the same key value is already being tracked". O Princípio II pede mudança de estado por método de intenção. | Manter `ToEntity` + `Update` com `AsNoTracking` no `BuscarPorId`: contorna o EF, mas ignora os métodos de intenção, e mudar o `BuscarPorId` genérico afeta todos os repositórios. |
| D8 | **O modal de edição tem o campo Status**, que permite reativar um produto ou marcá-lo como `ForaDeEstoque`. | Alinhado no debate. Reativar é só uma edição, sem botão extra. | Botão "Reativar" na linha dos inativos. |
| D9 | **Excluir pede confirmação** num `<dialog>` ("Inativar *{nome}*? Ele sai da loja.") e depois envia um `POST Inativar` comum, com antiforgery, sem `fetch`. O redirect volta para `Index` com o **mesmo filtro e página** e uma confirmação em `TempData`. A linha de um produto inativo (filtro `Inativos`) não tem botão de excluir. | Alinhado no debate. Inativar não tem validação de campo para devolver, então não precisa de `fetch`. | `confirm()` do navegador: visual fora do padrão. Excluir sem confirmação. |
| D10 | **Paginação:** 10 por página, ordenação por `Nome`. Página fora do intervalo é ajustada com `Math.Clamp(pagina, 1, totalDePaginas)`, como faz o `CatalogoService`. O resultado usa o `PaginaDeProdutosDTO` existente. | Alinhado no debate (10 por página). O ajuste evita página vazia com URL adulterada. Ordenar por nome é o que um admin procurando um doce espera. | Reusar o `_Paginacao` do catálogo: ele é acoplado a `CatalogoDTO` e a `EnderecoDoCatalogo`. O visual é reaproveitado (mesmas classes `link-paginacao`), mas a marcação fica num partial próprio. |
| D11 | **Colunas da tabela:** Foto, Nome, Preço, Status, Ações. O modal "Ver" mostra todos os campos do produto, com a subcategoria como "Categoria › Subcategoria". | Alinhado no debate (rascunho aprovado). O rótulo da subcategoria sai da mesma lista que `CarregarSubcategorias` já monta, sem consulta nova. | Incluir a subcategoria na tabela: exigiria `Include` e um DTO novo para a listagem, sem pedido. |
| D12 | **Atalho do cabeçalho:** "Cadastrar produto" vira "Produtos" e aponta para `Admin/Produto/Index`. A tela de página inteira `Cadastro.cshtml` deixa de existir. | O cadastro passa a acontecer dentro da gestão (D3/D4). | Manter as duas portas de entrada. |
| D13 | **A consulta da gestão ganha método próprio no repositório**, `BuscarPaginaDaGestao`/`ContarNaGestao`, com um enum novo `FiltroDeGestaoDeProduto { Todos, Inativos }` em `Application/Enums`. | `ConstruirConsulta` do catálogo exclui inativos por regra, e a gestão precisa justamente deles. Misturar os dois públicos num filtro só convida a vazar inativo para a loja. | Reaproveitar `BuscarPaginaDoCatalogo` com uma flag. `BuscarTodos()` + paginação em memória: carrega a tabela inteira a cada página. |

**Fora de escopo:** busca por nome e filtro por categoria na tabela; exclusão
física; apagar imagem antiga do armazenamento; ação em lote; promoções na
gestão (a entidade `Promocao` segue sem uso); controle de estoque (spec `026`).

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma `ProjectReference` nova. O enum de filtro mora em `Application/Enums`, e o repositório o recebe pelo contrato da `Application`, como `FiltroCatalogoDTO` já faz. |
| II | Domínio rico e auto-validante | ✅ OK | `Atualizar` passa a usar os métodos de intenção que já existem (D7). Nenhuma entidade muda. |
| III | Validação nas duas barreiras | ✅ OK | `ProdutoDTOValidator` (entrada) e o construtor/métodos de `Produto` (invariante) continuam valendo para cadastro e edição. A edição sem imagem remove `ImagemUrl` do `ModelState` e preenche a URL atual antes de validar a entidade. |
| IV | Nomenclatura em português | ✅ OK | `Inativar`, `Edicao`, `Detalhe`, `FiltroDeGestaoDeProduto`, `GestaoDeProdutosViewModel`, `_FormularioProduto`, `_DetalheProduto`. Os partials de uso único ficam em `Areas/Admin/Views/Produto/`. |
| V | Testes escritos antes | ✅ OK | Cada passo de comportamento da seção 6 começa pelo teste vermelho. Os E2E existentes são adaptados e nenhum novo é criado (ver seção 7). |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `Inativar` e `Atualizar` mudam a entidade rastreada e chamam `SalvarAlteracoes`. A consulta nova fica no `ProdutoRepository`. Sem migration. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | Os POSTs `Cadastro`, `Edicao` e `Inativar` têm `[ValidateAntiForgeryToken]`, com o token enviado no `FormData`. A classe mantém `[Authorize(Roles = Papeis.Administrador)]`. Sucesso redireciona (D5). Em erro, a resposta é `PartialView` com status 400, que é o equivalente de `return View(dto)` para um modal, sem efeito colateral antes. |
| VIII | Tratamento de erro por camada | ✅ OK | `Inativar`, `Atualizar` e `BuscarProdutoPorId` lançam `KeyNotFoundException` para id inexistente, que o `FilterException` captura. A ação não tem `try/catch`. Falha de envio de imagem vira `ModelState.AddModelError`. |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`

Nenhum arquivo.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/FiltroDeGestaoDeProduto.cs` | criar | `Todos = 0`, `Inativos = 1` (D2, D13). |
| `Contracts/Repositories/IProdutoRepository.cs` | alterar | Acrescenta `BuscarPaginaDaGestao` e `ContarNaGestao` (D13). |
| `Contracts/Services/IProdutoService.cs` | alterar | Remove `Remover`, acrescenta `Inativar` e `ListarParaGestao` (D1, D10). |
| `Services/ProdutoService.cs` | alterar | Implementa `ListarParaGestao` com `TamanhoDaPaginaDaGestao = 10` e ajuste de página. `Inativar` substitui `Remover`. `Atualizar` é reescrito com métodos de intenção (D7). Corrige o typo "Produo" na mensagem ao remover o método antigo. |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/ProdutoRepository.cs` | alterar | `BuscarPaginaDaGestao`/`ContarNaGestao`: `AsNoTracking`. `Todos` → `Status != Inativo`, `Inativos` → `Status == Inativo`. Ordena por `Nome` e aplica `Skip`/`Take`. |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Areas/Admin/Controllers/ProdutoController.cs` | alterar | Ações novas: `GET Index(filtro, pagina)`, `GET Detalhe(id)` (partial), `GET Edicao(id)` (partial), `POST Edicao(dto, imagem?)` e `POST Inativar(id, filtro, pagina)`. `GET Cadastro` passa a retornar o partial do formulário vazio. `POST Cadastro` retorna `PartialView` com status 400 em erro e redireciona para `Index` em sucesso. A validação de imagem vira um método privado compartilhado entre cadastro (obrigatória) e edição (opcional). `CarregarSubcategorias` é reaproveitado também para o rótulo do "Ver". Remove a ação `Error()`, que é inalcançável na area (código morto). |
| `Models/GestaoDeProdutosViewModel.cs` | criar | `PaginaDeProdutosDTO Pagina` + `FiltroDeGestaoDeProduto Filtro`, para a view e a paginação montarem URLs mantendo o filtro. |
| `Areas/Admin/Views/Produto/Index.cshtml` | criar | Cabeçalho com título e "+ Novo produto", abas de filtro (links `?filtro=`), `TempData["Confirmacao"]`, tabela, mensagem vazia por filtro, partial de paginação, `<dialog>` do modal de produto e `<dialog>` de confirmação de inativação (com o form POST + antiforgery). |
| `Areas/Admin/Views/Produto/_FormularioProduto.cshtml` | criar | Conteúdo do formulário atual de `Cadastro.cshtml`, parametrizado por `ProdutoId` (ação, título, texto do botão, `required` da imagem só no novo, miniatura da imagem atual na edição). Mantém os rótulos atuais, dos quais o E2E depende. |
| `Areas/Admin/Views/Produto/_DetalheProduto.cshtml` | criar | Leitura de todos os campos do produto, com o botão "Editar" que troca o conteúdo do modal pelo de edição. |
| `Areas/Admin/Views/Produto/_PaginacaoDaGestao.cshtml` | criar | Marcação de paginação com as classes `link-paginacao` do catálogo, montando `?filtro=&pagina=`. |
| `Areas/Admin/Views/Produto/Cadastro.cshtml` | apagar | Substituído pelo `_FormularioProduto` dentro do modal (D4, D12). |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | "Cadastrar produto" → "Produtos", `asp-action="Index"`, com ícone de lista (D12). |
| `wwwroot/js/pages/gestao-produtos.js` | criar | Abre o `<dialog>` carregando o partial por `fetch` (`X-Requested-With`). Faz o envio por `FormData`: em 400, troca o conteúdo; em `redirected`, navega. Também abre a confirmação de inativação preenchendo nome e id, e fecha com `Esc` ou pelo botão. |
| `wwwroot/css/pages/gestao-produtos.css` | criar | Tabela, abas de filtro, botões de ação e modais, sobre as variáveis e o padrão de `administradores.css` e `cadastro_produto.css`. |

### Testes

| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs` | alterar | Casos de `ListarParaGestao`, `Inativar` e `Atualizar` (seção 7). |
| `DocesCabana.Tests/Units/Controllers/Admin/ProdutoControllerTests.cs` | alterar | Os casos de `Cadastro` passam a afirmar `PartialViewResult` com status 400 e redirecionamento para `Index`. Casos novos para `Index`, `Edicao`, `Detalhe` e `Inativar`. |
| `DocesCabana.Tests/Integration/Repositories/ProdutoGestaoRepositoryIntegrationTests.cs` | criar | Filtro `Todos`/`Inativos`, ordenação, paginação e contagem contra o Postgres descartável. Também prova que `Atualizar` persiste sem o conflito de rastreamento do EF (D7). |
| `DocesCabana.Tests.E2E/Paginas/PaginaCadastroProduto.cs` | alterar | Vira objeto de página da gestão: `Abrir` vai para `/Admin/Produto`, e `AbrirNovo` clica em "+ Novo produto" e espera o modal. O `Formulario` passa a ser localizado dentro do `<dialog>`. |
| `DocesCabana.Tests.E2E/Fluxos/CadastroDeProdutoTests.cs` | alterar | Os dois testes atuais seguem válidos, só que pelo modal. O caminho feliz passa a confirmar o produto na tabela. |
| `DocesCabana.Tests.E2E/Fluxos/AreaAdministrativaTests.cs` | alterar | `/Admin/Produto/Cadastro` → `/Admin/Produto` nas proteções de acesso. |

## 5. Contratos e modelo de dados

```csharp
// Application/Enums/FiltroDeGestaoDeProduto.cs
public enum FiltroDeGestaoDeProduto { Todos = 0, Inativos = 1 }

// Application/Contracts/Repositories/IProdutoRepository.cs  (acréscimos)
Task<List<Produto>> BuscarPaginaDaGestao(FiltroDeGestaoDeProduto filtro, int pagina, int tamanhoDaPagina);
Task<int> ContarNaGestao(FiltroDeGestaoDeProduto filtro);

// Application/Contracts/Services/IProdutoService.cs
Task<PaginaDeProdutosDTO> ListarParaGestao(FiltroDeGestaoDeProduto filtro, int pagina);   // novo
Task Inativar(Guid produtoId);                                                             // novo
// Task Remover(Guid produtoId);                                                           // removido
Task<ProdutoDTO> Atualizar(ProdutoDTO dto);                                                // mesma assinatura, novo corpo

// MVC/Models/GestaoDeProdutosViewModel.cs
public class GestaoDeProdutosViewModel
{
    public PaginaDeProdutosDTO Pagina { get; init; }
    public FiltroDeGestaoDeProduto Filtro { get; init; }
}

// Areas/Admin/Controllers/ProdutoController.cs
[HttpGet]  Task<IActionResult> Index(FiltroDeGestaoDeProduto filtro = Todos, int pagina = 1);
[HttpGet]  Task<IActionResult> Cadastro();                         // PartialView("_FormularioProduto")
[HttpPost] Task<IActionResult> Cadastro(ProdutoDTO dto, IFormFile? imagem);
[HttpGet]  Task<IActionResult> Edicao(Guid id);                    // PartialView("_FormularioProduto", dto)
[HttpPost] Task<IActionResult> Edicao(ProdutoDTO dto, IFormFile? imagem);
[HttpGet]  Task<IActionResult> Detalhe(Guid id);                   // PartialView("_DetalheProduto", dto)
[HttpPost] Task<IActionResult> Inativar(Guid id, FiltroDeGestaoDeProduto filtro, int pagina);
```

Não há migration: o esquema não muda.

## 6. Ordem de execução

- [ ] 1. Criar `FiltroDeGestaoDeProduto` e as assinaturas novas de `IProdutoRepository` e `IProdutoService` (compila com `NotImplementedException`).
- [ ] 2. Teste de integração de `BuscarPaginaDaGestao`/`ContarNaGestao` (vermelho) → implementar no `ProdutoRepository` → verde.
- [ ] 3. Testes de unidade de `ListarParaGestao` (filtro repassado, ajuste de página, tamanho 10, total) (vermelho) → implementar → verde.
- [ ] 4. Testes de unidade de `Inativar` (muda status e salva; já inativo não falha; id inexistente → `KeyNotFoundException`) (vermelho) → substituir `Remover` por `Inativar` → verde.
- [ ] 5. Testes de unidade de `Atualizar` (aplica cada campo na entidade rastreada, não chama `Repository.Atualizar`, salva; id inexistente → `KeyNotFoundException`) e teste de integração que persiste a edição (vermelhos) → reescrever `Atualizar` → verde.
- [ ] 6. Testes de controller para `Index` (repassa filtro/página e retorna view com o view model) (vermelho) → criar `GestaoDeProdutosViewModel`, a ação, `Index.cshtml` e `_PaginacaoDaGestao.cshtml` → verde.
- [ ] 7. Ajustar os testes de controller de `Cadastro` para `PartialViewResult`/400/redirect a `Index` (vermelho) → extrair `_FormularioProduto.cshtml`, adaptar as ações e apagar `Cadastro.cshtml` → verde.
- [ ] 8. Testes de controller para `Edicao` GET/POST (sem imagem mantém a URL atual; imagem inválida → 400; envio falhou → 400; sucesso → `Atualizar` + redirect) (vermelho) → implementar → verde.
- [ ] 9. Testes de controller para `Detalhe` e `Inativar` (redireciona mantendo filtro e página, com `TempData`) (vermelho) → implementar ações e `_DetalheProduto.cshtml` → verde.
- [ ] 10. `gestao-produtos.js` e `gestao-produtos.css`, com os modais de produto e de confirmação.
- [ ] 11. Atalho do cabeçalho → "Produtos" / `Index`.
- [ ] 12. Adaptar os objetos de página e os E2E de `CadastroDeProdutoTests` e `AreaAdministrativaTests` (vermelho com a tela antiga apagada) → verde.
- [ ] 13. `dotnet build` e `dotnet test` completos.
- [ ] 14. Verificação manual (seção 7) e atualização do status em `specs/README.md`.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| `Todos` traz ativos e fora de estoque, nunca inativos; `Inativos` traz só inativos | `Dado_ProdutosDeCadaStatus_Quando_BuscarPaginaDaGestaoComFiltroTodos_Entao_NaoDeveTrazerInativos` / `..._ComFiltroInativos_Entao_DeveTrazerSomenteInativos` (integração) |
| Ordenação por nome, paginação e contagem coerentes | `Dado_VinteEUmProdutos_Quando_BuscarTerceiraPagina_Entao_DeveTrazerUmEmOrdemDeNome` (integração) |
| Página fora do intervalo é ajustada | `Dado_PaginaAlemDaUltima_Quando_ListarParaGestao_Entao_DeveDevolverAUltima` (unidade) |
| Inativar muda o status e salva, sem `DELETE` | `Dado_ProdutoAtivo_Quando_Inativar_Entao_DeveFicarInativoESalvar` (unidade) |
| Inativar é idempotente | `Dado_ProdutoJaInativo_Quando_Inativar_Entao_NaoDeveLancar` (unidade) |
| Id inexistente | `Dado_IdInexistente_Quando_Inativar_Entao_DeveLancarKeyNotFoundException` / idem `Atualizar` (unidade) |
| `Atualizar` usa métodos de intenção na entidade rastreada | `Dado_ProdutoExistente_Quando_Atualizar_Entao_DeveAplicarOsCamposSemChamarRepositorioAtualizar` (unidade) |
| `Atualizar` persiste de verdade (o bug do EF não volta) | `Dado_ProdutoPersistido_Quando_AtualizarPrecoEStatus_Entao_DeveLerOsValoresNovos` (integração) |
| Cadastro e edição com erro devolvem o formulário com o erro no campo | `Dado_ModelStateInvalido_Quando_CadastroPost_Entao_DeveRetornarPartialCom400SemChamarServico` / idem `Edicao` (unidade) |
| Edição sem imagem mantém a atual | `Dado_EdicaoSemArquivo_Quando_EdicaoPost_Entao_DeveAtualizarComAImagemAtualSemChamarArmazenamento` (unidade) |
| Sucesso segue PRG | `Dado_ProdutoValido_Quando_EdicaoPost_Entao_DeveRedirecionarParaIndexComConfirmacao` (unidade) |
| Inativar volta para o mesmo filtro e página | `Dado_FiltroEPagina_Quando_Inativar_Entao_DeveRedirecionarComOsMesmosValores` (unidade) |
| Jornada completa no navegador: modal carregado por `fetch`, antiforgery no `FormData`, upload real e redirect seguido pelo JS | E2E adaptado `Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar`. Ele prova o pipeline `fetch` + antiforgery + multipart + PRG, que o teste de unidade não alcança. Nenhum E2E novo: editar e inativar são regra de negócio já provada em unidade e integração (Princípio V). |
| Visual, foco no modal, `Esc`, estado vazio de cada filtro, confirmação de inativação e o produto sumindo da loja | Verificação manual |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| `fetch` segue o redirect do POST e recebe o HTML de `Index` inteiro. Se o JS não checar `resposta.redirected`, injeta a página dentro do modal. | Médio | O JS navega para `resposta.url` sempre que `redirected` for verdadeiro. Coberto pelo E2E do caminho feliz. |
| Sessão expirada durante o modal: o `[Authorize]` redireciona para o login, e o `fetch` segue esse redirect. | Baixo | O mesmo tratamento de `redirected` leva o admin à tela de login. Nada é salvo. |
| A FluentValidation automática valida `ImagemUrl` no bind. Na edição sem arquivo, a URL atual não vem no form. | Médio | O POST `Edicao` busca o produto, preenche `ImagemUrl` com a atual (`ComImagem`) e remove a chave do `ModelState`, como `Cadastro` já faz. Coberto pelo teste de edição sem imagem. |
| Imagens antigas ficam órfãs no Supabase ao trocar a foto. | Baixo | Aceito (D6). Fica registrado como backlog. |
| Inativar um produto que está em carrinhos. | Baixo | Já tratado: o `CarrinhoMapper` marca o item como `ForaDoCatalogo` e o fechamento bloqueia. Nenhuma mudança necessária. |

Nenhum desvio constitucional.

## 9. Resultado da execução

*Preenchido na execução.*
