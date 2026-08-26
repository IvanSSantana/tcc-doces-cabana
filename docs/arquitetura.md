# Como o Doces Cabana funciona por dentro

Guia de leitura da base. Explica **o que existe, por que existe assim, e como
cada peça funciona por dentro** — não é auditoria nem lista de tarefas.

Escrito em 2026-08-22, com a `016` recém-implementada. Quando o código mudar,
este arquivo deveria mudar junto: ele vive no repositório justamente para
aparecer no `git status` pedindo atualização.

> **O que este documento não é.** Não é uma caça sistemática a defeitos. A
> seção 8 registra o que apareceu durante a leitura, mas para varredura
> completa existem os comandos `/analisar` e `/code-review`.

---

## Índice

1. [As quatro camadas e a regra da seta](#1-as-quatro-camadas-e-a-regra-da-seta)
2. [Como uma requisição vira tela](#2-como-uma-requisição-vira-tela)
3. [Areas: por que `Admin` é separada](#3-areas-por-que-admin-é-separada)
4. [Os quatro ViewComponents, por dentro](#4-os-quatro-viewcomponents-por-dentro)
5. [Página a página](#5-página-a-página)
6. [Os algoritmos que sustentam as telas](#6-os-algoritmos-que-sustentam-as-telas)
7. [Padrões que se repetem](#7-padrões-que-se-repetem)
8. [Onde o erro é tratado](#8-onde-o-erro-é-tratado)
9. [Dívidas conhecidas e achados desta leitura](#9-dívidas-conhecidas-e-achados-desta-leitura)

---

## 1. As quatro camadas e a regra da seta

O projeto é uma Clean Architecture de quatro projetos. A regra única é: **as
referências só apontam para dentro.**

```
     ┌─────────────────┐
     │  DocesCabana.   │
     │      MVC        │  controllers, views, wwwroot, filtros
     └────────┬────────┘
              │
      ┌───────┴────────┐
      ▼                ▼
┌───────────┐   ┌──────────────┐
│Application│   │Infrastructure│  EF Core, Identity, SMTP
│           │◄──┤              │
│ DTOs,     │   │ implementa os│
│ serviços, │   │ contratos da │
│ contratos │   │ Application  │
└─────┬─────┘   └──────┬───────┘
      │                │
      ▼                ▼
   ┌──────────────────────┐
   │   DocesCabana.Domain │  entidades, enums, helpers
   │  não referencia nada │
   └──────────────────────┘
```

| Projeto | Conhece | O que mora nele |
|---|---|---|
| `Domain` | **ninguém** | `Entities/`, `Enums/`, `Helpers/`, `Contracts/`, `Papeis.cs` |
| `Application` | `Domain` | `DTOs/`, `Services/`, `Contracts/`, `Mappings/`, `Validators/`, `Servicos/` |
| `Infrastructure` | `Application`, `Domain` | `Repositories/`, `DatabaseContext/`, `Identity/`, `Migrations/`, `DependencyInjections/` |
| `MVC` | `Application` + os módulos de DI da `Infrastructure` | `Controllers/`, `Views/`, `Areas/`, `ViewComponents/`, `Filters/`, `Helpers/`, `wwwroot/` |

**Como conferir em dez segundos:** abra o `.csproj` e olhe os
`<ProjectReference>`. Se uma tarefa exige uma referência nova, ela viola o
Princípio I até prova em contrário.

### A única exceção, e por que ela é exceção

`IUsuarioService` **não** mora na `Application` como os outros serviços. Ele
vive em `Infrastructure/Identity/Services/` porque sua implementação depende de
`UserManager<ContaDeAcesso>` e `SignInManager` — tipos do ASP.NET Identity, que
a `Application` não pode enxergar sem quebrar a seta.

Os controladores dependem de `IUsuarioService` diretamente. Isso é aceito e
está registrado na constituição; **nenhuma outra exceção é permitida sem emenda
constitucional.**

Repare na distinção que a emenda 1.2.0 fez: a exceção é sobre **onde o serviço
mora**, não sobre o domínio se relacionar com `Usuario`. A entidade `Usuario` é
de domínio puro — quem guarda a credencial é `ContaDeAcesso`, em
`Infrastructure/Identity`, e as duas compartilham o mesmo `Guid`. Qualquer
entidade referencia `Usuario` por navegação normal.

### Por que `TextoHelper` desceu para o `Domain`

Caso concreto de como a regra da seta decide desenho. Na `016`, a busca passou
a precisar comparar texto sem acento — e quem precisava normalizar era a
entidade `Produto` (para gravar `NomeNormalizado`). Mas o normalizador vivia
dentro de `Application/Servicos/Apelido.cs`, e `Produto` não pode enxergar a
`Application`.

A saída conforme foi **mover** o normalizador para
`Domain/Helpers/TextoHelper.cs` — ele usa só BCL (`System.Globalization`,
`System.Text`), então cabe no `Domain` — e fazer o `Apelido` passar a consumi-lo
de lá. A seta continua apontando para dentro; nada foi duplicado.

---

## 2. Como uma requisição vira tela

### 2.1 O `Program.cs`, na ordem em que executa

O arquivo tem duas metades que se leem diferente. A primeira **registra** coisas
no contêiner; a segunda **monta o pipeline**, e ali a ordem das linhas é a ordem
em que cada requisição passa por elas.

```
REGISTRO (antes de builder.Build())
  AddControllersWithViews  ── registra FilterException e FiltroFusaoDeCarrinho
                              (spec 017) como filtros globais, e troca a
                              mensagem de erro de DataNascimento
  AddDatabaseConfiguration ── DbContext + SQLite
  AddIdentityConfiguration ── Identity, política de senha, bloqueio, cookie
  AddApplicationServices…  ── todos os repositórios e serviços (escopo)
  AddFluentValidation…     ── varre o assembly e registra todo *Validator
  AddSession               ── carrinho de visitante (spec 017); em memória,
                              por processo — some ao reiniciar a aplicação

PARTIDA (uma vez, ao subir)
  DbInitializer.Migrar                     ── aplica migrations pendentes
  DbInitializer.PreencherNomesNormalizados ── conserta linhas antigas (016)
  DbInitializer.Semear                     ── só fora de produção

PIPELINE (a cada requisição, nesta ordem)
  UseExceptionHandler / UseHsts   ── só fora de Development
  UseHttpsRedirection
  UseStatusCodePagesWithReExecute ── 4xx/5xx sem corpo → /Home/NaoEncontrado
  UseRouting                      ── decide qual rota casou
  UseSession                      ── carrinho de visitante (spec 017,
                                     plano §9, risco 1): entra logo aqui —
                                     antes dela, a sessão lida devolve vazio
                                     sem erro nenhum, em silêncio
  UseAuthentication               ── quem é você
  UseAuthorization                ── você pode
  UseRequestLocalization          ── pt-BR fixo: vírgula decimal, dd/MM/yyyy
  MapStaticAssets
  MapControllerRoute × 3          ── area, catalogo, default
```

### 2.2 As três rotas, e por que a ordem entre elas importa

Esta é a parte que mais confunde quem chega. São três rotas e **a ordem não é
arbitrária** — cada uma existe porque a seguinte interpretaria o endereço
errado.

```csharp
// 1ª — area
pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
```
Sem ela, `/Admin/Produto` cairia na rota padrão e seria lido como **controller
`Admin`, ação `Produto`** — que não existe. O `:exists` é uma restrição: só casa
se `Admin` for de fato uma area registrada.

```csharp
// 2ª — catalogo
pattern: "Catalogo/{apelido?}"
defaults: controller = "Catalogo", action = "Index"
```
Sem ela, `/Catalogo/doces` seria lido como **controller `Catalogo`, ação
`doces`**. O segundo segmento aqui é o apelido da categoria, não uma ação — é
o que faz `/Catalogo/emporio` funcionar.

```csharp
// 3ª — default
pattern: "{controller=Home}/{action=Index}/{id?}"
```
O resto. `/` → `Home/Index`. `/Produto/Detalhes/{guid}` → o `id` no terceiro
segmento.

### 2.3 O caminho de uma requisição, do clique à tela

Exemplo real: alguém abre `/Catalogo/doces?subcategorias=barras&ordenacao=MenorPreco`.

```
1. UseRouting casa com a rota "catalogo"
      apelido = "doces"

2. CatalogoController.Index(apelido: "doces",
                            subcategorias: ["barras"],
                            ordenacao: MenorPreco)
      ── o ligador de modelo preenche os parâmetros a partir da query string
      ── SanearOrdenacao() recusa MaisVendidos (RN-07) e devolve o padrão
      ── monta CriteriosDoCatalogoDTO (apelidos, sem identificador nenhum)
      ── lê UsuarioAtualId do ClaimTypes.NameIdentifier (nulo = visitante)

3. CatalogoService.Montar(criterios, pagina, usuarioId)
      ── carrega todas as categorias com subcategorias
      ── casa "doces" com uma categoria; se não casar → KeyNotFoundException
      ── resolve "barras" contra as subcategorias DAQUELA categoria
      ── normaliza o termo de busca, se houver
      ── monta FiltroCatalogoDTO — agora sim com Guids

4. ProdutoRepository.ContarNoCatalogo + BuscarPaginaDoCatalogo(filtro, …)
      ── ConstruirConsulta compõe os Where
      ── AplicarOrdenacao ordena, sempre com desempate por Nome
      ── Skip/Take

5. FavoritoRepository.IdsPorUsuario  ── uma consulta para a página inteira

6. ProdutoMapper.ToDTO(produtos, favoritados) → CatalogoDTO

7. O controller decide a representação:
      X-Requested-With: XMLHttpRequest  →  PartialView("_ResultadoCatalogo")
      caso contrário                    →  View(catalogo)
```

O passo 7 é o coração do "um endereço, duas representações" — ver §7.3.

---

## 3. Areas: por que `Admin` é separada

Uma *area* do ASP.NET Core é uma subdivisão da aplicação com sua própria árvore
de `Controllers/`, `Views/` e `Models/`. Serve para separar públicos.

```
DocesCabana.MVC/
├── Controllers/              ← a loja (público)
│   ├── HomeController.cs
│   ├── CatalogoController.cs
│   ├── ProdutoController.cs        ← detalhe do produto, para o cliente
│   └── …
└── Areas/
    └── Admin/                ← a administração
        ├── Controllers/
        │   ├── ProdutoController.cs     ← cadastro, para a dona da loja
        │   └── AdministradorController.cs
        └── Views/
            ├── _ViewImports.cshtml
            ├── _ViewStart.cshtml
            ├── Produto/Cadastro.cshtml
            └── Administrador/{Index,Cadastro}.cshtml
```

**Existem dois `ProdutoController` e isso é correto.** Um em
`Controllers/`, outro em `Areas/Admin/Controllers/`. Eles não colidem porque a
area é o qualificador que o framework oferece: `/Produto/Detalhes` e
`/Admin/Produto/Cadastro` são endereços distintos, para públicos distintos.

Isso precisou de emenda constitucional. O Princípio IV exige nome de classe
único por conceito de negócio na base inteira — a `010` renomeou
`AdminController` justamente porque colidia com `AdministradorController`. A
emenda **1.4.1** acrescentou a ressalva: a unicidade é escopada **por area**,
não pela solução inteira.

**Para escrever um link para dentro ou fora de uma area**, o atributo `asp-area`
é obrigatório e não pode ser omitido:

```html
<a asp-area="Admin" asp-controller="Produto" asp-action="Cadastro">Cadastrar</a>
<a asp-area=""      asp-controller="Catalogo" asp-action="Index">Catálogo</a>
```

`asp-area=""` explícito é o que tira você da area atual. Omitir o atributo faz
o gerador de endereço **herdar a area ambiente**, e o link aponta para o lugar
errado — exatamente a classe de armadilha descrita em §7.6.

---

## 4. Os quatro ViewComponents, por dentro

Um `ViewComponent` é um pedaço de tela com lógica própria, invocável de
qualquer view. Diferente de uma partial, ele **pode receber dependências por
injeção** e não participa do `ModelState` do formulário da página.

A convenção de pasta é imposta pelo framework:
`Views/Shared/Components/{Nome}/Default.cshtml`.

### 4.1 `Header` — o único com injeção de dependência

```csharp
public async Task<IViewComponentResult> InvokeAsync()
{
    ViewData["ItensCarrinho"] = await ContarItensDoCarrinho();
    ViewData["TermoDeBusca"] = Request.Query["termo"].ToString();
    var categorias = await _categoriaService.ListarComSubcategorias();
    return View(categorias);
}
```

Três coisas acontecem aqui:

**Lê a query string diretamente.** O cabeçalho aparece em toda página, então não
existe um "termo atual" que alguém possa passar de fora. Ele só existe quando a
página *é* o resultado de uma busca. Ler `Request.Query["termo"]` é o caminho
honesto — o componente não tem um dono que saiba disso por ele.

**Carrega a taxonomia inteira a cada requisição.** `ListarComSubcategorias()`
roda em toda página do site. São 4 categorias e 31 subcategorias, mais uma
consulta agregada de contagem por subcategoria. Aceitável nesta escala,
mas é o candidato número um a cache no dia em que a loja crescer.

**A contagem do carrinho é do próprio componente, desde a `017`.** Antes, era
um parâmetro `itensCarrinho` que ninguém passava — ficava sempre em zero. Hoje
o componente injeta `ICarrinhoService` e conta sozinho: do banco, pela claim
do usuário, quando autenticado; da sessão (`HttpContext.Session.Ler()`), para
o visitante — a mesma soma que `TotalDeItens` usa, sem precisar buscar produto
nenhum, porque contar não valida disponibilidade.

O menu suspenso em si **não tem JavaScript** — abre por `:hover` e
`:focus-within` no CSS, o que o mantém acessível por teclado de graça. Só as 8
subcategorias com mais produto disponível aparecem; a lista completa mora na
barra lateral do catálogo.

### 4.2 `VitrineProdutos` — onde mora o corte

```csharp
public const int LimitePadrao = 8;

public IViewComponentResult Invoke(IEnumerable<ProdutoDTO> produtos, int limite = LimitePadrao)
    => View(produtos.Take(limite).ToList());
```

Três decisões pequenas e deliberadas:

**O limite mora no componente, não em quem chama.** Qualquer página que use a
vitrine herda o corte sem precisar lembrar de aplicá-lo. Foi a correção da `013`
para a home que renderizava 99 cartões.

**`.ToList()` não é decoração.** `IEnumerable` pode ser enumerado mais de uma
vez, e a view enumera duas (uma para os cartões, outra para os pontos
indicadores). Sem materializar, a consulta rodaria duas vezes.

**`LimitePadrao` é público desde a `019`.** `HomeController.Index` já pede só
os destaques ao armazenamento (`ProdutoService.BuscarDestaquesDaVitrine`, ver
§6.4), e precisa pedir exatamente o que o componente vai exibir — a mesma
constante nos dois lados evita que "quantos pedimos" e "quantos exibimos"
divirjam em silêncio. O `.Take(limite)` continua no componente mesmo assim:
é rede de segurança contra um consumidor futuro que esqueça o próprio corte.

Oito produtos = cinco posições de rolagem com quatro cartões visíveis no
desktop. O número não é arbitrário: é `8 − 4 + 1`.

### 4.3 `EstrelasNota` — preenchimento fracionário em SVG

O componente recebe uma nota decimal e desenha cinco estrelas, sendo que uma
delas pode ficar **parcialmente** pintada. O algoritmo, por estrela `i`:

```csharp
var preenchimento = Math.Clamp((notaLimitada - (i - 1)) * 100, 0, 100);
```

Para nota `4,5`:

| Estrela | `(4,5 − (i−1)) × 100` | Depois do `Clamp` |
|---|---|---|
| 1 | 450 | **100%** |
| 2 | 350 | **100%** |
| 3 | 250 | **100%** |
| 4 | 150 | **100%** |
| 5 | 50 | **50%** |

O preenchimento vira um `<linearGradient>` com **dois `stop` no mesmo `offset`**
— um opaco, um transparente. Isso produz um corte reto, não um degradê:

```html
<stop offset="50%" stop-color="currentColor"></stop>
<stop offset="50%" stop-color="transparent"></stop>
```

**Por que cada gradiente ganha um `Guid.NewGuid()`:** IDs de SVG são globais no
documento. Se duas fileiras de estrelas aparecessem na mesma página com IDs
iguais, a segunda referenciaria o gradiente da primeira e mostraria a nota
errada. O GUID garante unicidade sem o componente precisar saber quantas vezes
foi invocado.

O texto `4,5 de 5 estrelas` fica num `<span class="somente-leitor-de-tela">`, e
os SVGs são `aria-hidden` — quem usa leitor de tela ouve a nota, não cinco
descrições de polígono.

> ⚠️ O comentário dentro deste arquivo descreve o algoritmo **errado** — diz que
> a 5ª estrela fica em 0% para nota 4,5. O código está certo; o comentário, não.
> Ver §9.

### 4.4 `CardProduto` — o cartão, e dois truques dentro dele

```csharp
public IViewComponentResult Invoke(ProdutoDTO produto, string rotuloBotaoCarrinho = "Adicionar")
```

O rótulo do botão é parâmetro porque o catálogo pede *"Adicionar ao carrinho"*
(fiel à referência visual) e o carrossel, mais estreito, mantém *"Adicionar"* —
sem que o carrossel precise saber que o parâmetro existe.

**Truque 1: o botão de favorito não fica dentro de um `<form>` próprio.**

O cartão pode estar dentro do `<form method="get">` do catálogo, e **HTML não
aceita form dentro de form** — o navegador ignora o aninhamento e submete o de
fora. A saída é o atributo `form=` do HTML5:

```html
<button type="submit" form="formulario-favorito" name="produtoId" value="@Model.ProdutoId">
```

Ele se associa a um `#formulario-favorito` que vive no `_Layout`, fora de
qualquer outro formulário. Funciona não importa onde o cartão esteja no
documento — **inclusive sem JavaScript nenhum**.

E o `produtoId` vai no `name`/`value` **do próprio botão**, não num `<input
type="hidden">` à parte. Um hidden com `form=` levaria o `produtoId` de *todos*
os cartões da página na mesma submissão; o do botão só viaja quando é ele o
acionado.

**Truque 2: reescrita de URL do Google Drive.**

As imagens de demonstração são links de compartilhamento do Drive, que não
servem imagem — servem uma página HTML. `ObterUrlImagem` extrai o ID e monta a
URL de miniatura, com dois padrões de fallback:

```
/d/{id}/…            →  regex  /d/([a-zA-Z0-9_-]+)
…?id={id}            →  regex  id=([a-zA-Z0-9_-]+)
qualquer outra coisa →  devolve a URL intacta
```

É código de demonstração. Quando a loja tiver hospedagem de imagem de verdade,
este método deixa de fazer sentido.

**A caixa alta saiu daqui na `015`.** Era um `.ToUpper()` na view. Hoje quem
decide é o CSS: o carrossel aplica `text-transform: uppercase` sobre si mesmo;
o catálogo não aplica nada e mostra a caixa real do nome.

---

## 5. Página a página

| Endereço | Controlador → Serviço | O que a tela faz |
|---|---|---|
| `/` | `Home.Index` → `IProdutoService.BuscarDestaquesDaVitrine` | Carrossel do topo + vitrine dos 8 produtos mais bem avaliados (`019`; até então pedia o catálogo inteiro) |
| `/Catalogo`<br>`/Catalogo/{apelido}` | `Catalogo.Index` → `ICatalogoService.Montar` | Barra lateral, filtros, ordenação, paginação, busca |
| `/Produto/Detalhes/{id}` | `Produto.Detalhes` → `IProdutoService.BuscarDetalhe` | Imagem, descrição, nota média, histograma, avaliações |
| `/Favorito` | `Favorito.Index` → `IFavoritoService.ListarDoUsuario` | Grade dos favoritos. `[Authorize]` |
| `/Carrinho`<br>`/Carrinho/ConfirmarEsvaziar`<br>`/Carrinho/CadastrarEndereco` | `Carrinho.Index/Acrescentar/AlterarQuantidade/Remover/Esvaziar/ConfirmarEsvaziar/CadastrarEndereco` → `ICarrinhoService`, `IFreteService`, `IPedidoService`, `IEnderecoService` | Itens em cartão, resumo com cupom desabilitado e destaque que troca entre subtotal e total a pagar quando há entrega calculada (`021`), item indisponível sinalizado, esvaziar com confirmação. Cotação de frete por CEP (`020`, §6.10) — só oferecida havendo item disponível, só os disponíveis entram na cotação. Os passos do fechamento (`022`, §6.10) vivem na mesma tela: `Index` aceita `passo`/`enderecoId`/`servicoDeEntregaId` e monta o passo ativo via `IPedidoService.MontarPasso`; `CadastrarEndereco` cadastra sem sair do fechamento (`[Authorize]`, diferente das outras ações desta tela). Sem `[Authorize]` na classe — quem não entrou usa o carrinho da sessão, fundido ao de conta no primeiro request autenticado (`FiltroFusaoDeCarrinho`) |
| `/Pedido/Confirmacao/{id}` | `Pedido.Fechar/Confirmacao` → `IPedidoService` | `Fechar` (`[HttpPost]`, `[Authorize]`) grava o pedido e redireciona para o comprovante (POST-Redirect-Get); recusa reexibe `Carrinho/Index` com `ModelState` inválido. `Confirmacao` (`[HttpGet]`) mostra o comprovante; pedido alheio ou inexistente devolve 404 (`022`, §6.10) |
| `/Conta` | `Conta.Index/AlterarDados` → `IUsuarioService` | Dados pessoais — CPF como texto, o resto editável. `[Authorize]` na classe |
| `/Conta/Enderecos`<br>`/Conta/NovoEndereco`<br>`/Conta/EditarEndereco/{id}` | `Conta.Enderecos/NovoEndereco/EditarEndereco/ExcluirEndereco/TornarPrincipal` → `IEnderecoService` | CRUD de endereço, exatamente um principal (RN-01 a RN-04). Busca por CEP no navegador (ViaCEP); `IEnderecoRepository` nunca busca por id sozinho, só pelo par `(enderecoId, usuarioId)` — é o que torna endereço alheio inalcançável por desenho, não por checagem avulsa |
| `/Autenticacao/Login` | `Autenticacao.Login` → `IUsuarioService` | Entrar, com endereço de retorno |
| `/Autenticacao/Cadastro` | `Autenticacao.Cadastro` | Criar conta de cliente |
| `/Autenticacao/EsqueceuSenha`<br>`/RedefinirSenha` | `Autenticacao` + `IEmailService` | Recuperação por token enviado por e-mail |
| `/Institucional/QuemSomos`<br>`/Privacidade` | `Institucional` | Conteúdo estático |
| `/Home/NaoEncontrado` | `Home.NaoEncontrado` | Alvo da reexecução de 404 |
| `/Home/AcessoNegado` | `Home.AcessoNegado` | Alvo do `AccessDeniedPath` do Identity |
| `/Admin/Produto/Cadastro` | `Admin.Produto` → `IProdutoService` | Cadastro de produto. `[Authorize(Roles = Administrador)]` |
| `/Admin/Administrador`<br>`/Admin/Administrador/Cadastro` | `Admin.Administrador` → `IAdministradorService` | Listar e cadastrar administradores. `[Authorize(Roles = Administrador)]` |

Os dois controladores da area exigem **papel**, não só autenticação — a
anotação está na classe, não em cada ação, então nenhuma ação nova nasce
desprotegida por esquecimento. Quem está logado como cliente e tenta abrir
`/Admin/...` cai em `/Home/AcessoNegado`, não na tela de login.

### Os dois layouts

| Layout | Quem usa | Diferença |
|---|---|---|
| `_Layout` | quase tudo | Cabeçalho completo, rodapé, modal de login, `#formulario-favorito` |
| `_LayoutNaoAutenticado` | telas de autenticação | Cabeçalho reduzido — sem menu de categorias nem ações de usuário |

O `_Layout` é onde vivem os dois formulários que os botões de toda a página
referenciam por `form=`: `#formulario-favorito` (coração) e
`#formulario-carrinho` (adicionar ao carrinho, spec 017) — o mesmo truque para
o mesmo problema: o cartão de produto pode estar dentro do `<form method="get">`
do catálogo, e HTML não aceita form aninhado.

---

## 6. Os algoritmos que sustentam as telas

### 6.1 Os dois carrosséis, que são coisas diferentes

Isto confunde: existem **dois** arquivos de carrossel, com mecânicas opostas.

| | `carrossel.js` | `components/vitrine-produtos.js` |
|---|---|---|
| Onde | Banner do topo da home | Prateleira de produtos |
| Carregado por | `Views/Home/_Carrossel.cshtml` | os dois layouts |
| Mecânica | Troca classe `.ativo` | `transform: translateX(...)` |
| Unidade | Um slide inteiro | Um cartão |
| Autoplay | Sim, 5 s | Não |
| Circular | Sim, dá a volta | Não, para nas pontas |
| Responsivo | Não precisa | Sim, muda quantos cabem |

`_Carrossel.cshtml` mora em `Views/Home/`, não em `Views/Shared/`, e carrega o
próprio script. É a regra que a `010` escreveu no Princípio IV: **tela parcial
de uso único mora na pasta do controlador dono**; `Views/Shared/` é reservado ao
que é reaproveitado por mais de uma página.

**`carrossel.js`** gera as bolinhas dinamicamente (uma por slide), e o laço
circular é feito na entrada de `mostrarSlide`:

```js
if (indice >= slides.length)      indiceAtivo = 0;
else if (indice < 0)              indiceAtivo = slides.length - 1;
else                              indiceAtivo = indice;
```

Todo clique manual chama `reiniciarAutoplay()`, que zera o `setInterval` — sem
isso, clicar faltando meio segundo para o autoplay disparar causaria dois
avanços seguidos.

**`vitrine-produtos.js`** é mais interessante. Ele **mede o DOM** em vez de
assumir valores do CSS:

```js
const larguraItem = itens[0].getBoundingClientRect().width;
// o espaçamento é deduzido da distância entre dois itens reais
gap = itens[1].getBoundingClientRect().left - itens[0].getBoundingClientRect().right;
```

Quantos cabem depende da largura da janela, em degraus:

```
≤ 480px → 1 item      ≤ 768px → 2 itens
≤ 1024px → 3 itens    acima   → 4 itens
```

E daí sai o limite de rolagem:

```js
const indiceMaximo = Math.max(0, itens.length - itensVisiveis);
const quantidadeMover = idx * (larguraItem + gap);
trilha.style.transform = `translateX(-${quantidadeMover}px)`;
```

Com 8 itens e 4 visíveis, `indiceMaximo = 4` — cinco posições (0 a 4). As
bolinhas além de `indiceMaximo` são **escondidas com `display: none`**, não
removidas do DOM — por isso os testes E2E precisam do seletor `:visible`, que é
próprio do Playwright e não CSS padrão.

O estado vive em `container.dataset.indiceAtual`, ou seja, **no próprio HTML**,
não numa variável de módulo. É o que permite mais de uma vitrine na mesma
página sem elas se atrapalharem.

### 6.2 `catalogo.js` — atualização sem recarga

O script intercepta duas coisas: o `submit` do formulário de filtros e o clique
nos links de paginação. Em ambos, busca só o bloco do resultado.

```js
var parametros = new URLSearchParams(new FormData(formulario));
var url = formulario.action.split("?")[0] + "?" + parametros.toString();
```

**O endereço é montado a partir do próprio formulário, nunca à mão.** Assim só
existe uma regra de serialização, e o endereço que o histórico guarda é
garantidamente o mesmo que o formulário produziria. É também o que fez a busca
da `016` funcionar sem uma linha nova: o termo é um campo escondido do
formulário, então entra na serialização sozinho.

Quatro detalhes que não são óbvios:

**`replaceWith`, não `innerHTML`.** O servidor devolve o próprio
`#resultado-catalogo` no HTML, então o script troca o elemento inteiro,
mantendo os dois lados idênticos.

**O foco vai para o resultado.** Sem `resultado.focus()`, quem navega por
teclado é jogado para o início do documento a cada filtro.

**O indicador de carregamento só aparece depois de 200 ms.** Resposta rápida
não pisca a tela.

**`popstate` faz recarga completa, não busca parcial.** As caixas de
subcategoria vivem na barra lateral, *fora* do bloco trocado; refazer só o
resultado deixaria as caixas com o estado anterior ao "voltar". A recarga
resolve isso de graça, sem duplicar em JavaScript a lógica de estado que o
Razor já sabe fazer.

E o `catch` cai para `window.location.href = url` — se a atualização parcial
falhar, a pessoa recebe o resultado do jeito que sempre funcionou.

### 6.3 `favorito.js` — e a intenção que sobrevive ao login

O caminho comum: intercepta o `submit` do `#formulario-favorito`, identifica
qual botão disparou via `evento.submitter`, e posta por `fetch`.

```js
var dados = new FormData(formulario, botao);   // o 2º argumento inclui o botão
```

O construtor de dois argumentos do `FormData` inclui o `name`/`value` do
elemento que submeteu — sem ele, o `produtoId` não viajaria.

**Quando o visitante clica:** o servidor responde `401`, e o script guarda o
produto pretendido em `sessionStorage`, reescreve o link do modal com o endereço
de retorno e abre o modal. Depois do login, `concluirFavoritoPendente()` roda no
`DOMContentLoaded`, vê a intenção pendente e a conclui.

`sessionStorage` é por aba e morre com ela — a intenção não vaza para outra aba
nem sobrevive dias.

**Trocar o ícone exigiu substituir o elemento inteiro.** O kit do FontAwesome
converte todo `<i class="fa-...">` em `<svg>` ao carregar a página, e a tag
original deixa de existir. Trocar classe num `<i>` que já virou `<svg>` não faz
nada. A solução é criar um `<i>` novo a cada alternância e deixar o observador
de mutações do próprio kit reconvertê-lo.

### 6.4 A consulta do catálogo

Toda a filtragem se compõe num método só, `ProdutoRepository.ConstruirConsulta`:

```csharp
.Where(p => p.Status != ProdutoStatus.Inativo)          // sempre
if (CategoriaId)      .Where(… Subcategoria.CategoriaId == …)
if (SubcategoriaIds)  .Where(… SubcategoriaIds.Contains(p.SubcategoriaId))   // OR entre elas
if (ApenasSemAcucar)  .Where(p => p.SemAcucar)                               // AND com o resto
if (TermoNormalizado) .Where(p => p.NomeNormalizado.Contains(termo))
```

Produto inativo é descartado **primeiro e sempre**, em todo caminho de consulta
do catálogo — inclusive na busca.

A ordenação tem duas sutilezas:

```csharp
OrdenacaoCatalogo.MelhorAvaliados => consulta
    .OrderByDescending(p => _context.Avaliacoes
        .Where(a => a.ProdutoId == p.ProdutoId)
        .Average(a => (double?)a.Nota) ?? -1)
    .ThenBy(p => p.Nome),
```

**O `?? -1`** joga produto sem avaliação nenhuma para o fim, em vez de descartá-lo
da consulta. O cast para `double?` é o que permite a média ser nula.

**Todo ramo termina em `ThenBy(p => p.Nome)`.** Sem desempate determinístico,
`Skip`/`Take` pode repetir ou pular produto entre páginas — dois produtos de
mesmo preço poderiam trocar de lugar entre a consulta da página 1 e a da 2.

**A mesma consulta serve a vitrine da home, desde a `019`.**
`ProdutoService.BuscarDestaquesDaVitrine` chama `BuscarPaginaDoCatalogo` com um
`FiltroCatalogoDTO` vazio (nenhuma categoria, nenhuma subcategoria, sem termo)
e `Ordenacao = MelhorAvaliados`, pedindo `pagina: 1` e `tamanhoDaPagina` igual
ao limite da vitrine. Nenhuma consulta nova, nenhum critério duplicado — a
home ganha o mesmo LIMIT/OFFSET que o catálogo, e herda de graça a exclusão de
produto inativo e o desempate por nome.

### 6.5 Busca: por que existe uma coluna normalizada

O banco é SQLite. O `Contains` do EF Core é traduzido para `instr`, que é
**sensível a maiúsculas e a acento**. Sem tratamento, `"cafe"` não encontraria
`"Café"` — nem `"brigadeiro"` encontraria `"Brigadeiro"`.

A solução tem duas metades que precisam casar:

```
Produto.NomeNormalizado  ← TextoHelper.Normalizar(Nome)   gravado no banco
termo do usuário         ← TextoHelper.Normalizar(termo)  calculado na hora
                            ↓
                    comparação entre dois textos já normalizados
```

`TextoHelper.Normalizar` decompõe em `FormD` (que separa a letra do acento),
descarta os caracteres da categoria `NonSpacingMark`, recompõe em `FormC`, baixa
a caixa e colapsa espaços.

`NomeNormalizado` tem `private set` e é derivado nos **dois únicos pontos** que
alteram o nome — o construtor e `AlterarNome`. Não existe caminho que o deixe
divergir.

`Apelido.De` reaproveita o mesmo normalizador e só acrescenta os hifens:
`"Bolachas / Rosquinhas"` → `bolachas-rosquinhas`.

### 6.6 Avaliações: média, histograma e relevância

**A média** (`AvaliacaoService.ResumirPorProduto`):

```csharp
var soma = distribuicao.Sum(kv => kv.Key * kv.Value);   // Σ (nota × quantidade)
media = Math.Round((decimal)soma / total, 1, MidpointRounding.AwayFromZero);
```

`AwayFromZero` é deliberado: o padrão do .NET é *banker's rounding*, que
arredonda 4,25 para 4,2. Numa nota exibida ao cliente, isso surpreende.

**Sem avaliação, a média é `null`, não zero.** Zero é uma nota; ausência de nota
é outra coisa.

**O histograma sempre tem as cinco chaves**, mesmo quando o repositório só
devolve as notas que existem — senão a barra de "2 estrelas" sumiria da tela em
vez de aparecer vazia.

**A ordenação por relevância** é `Votos.Count` decrescente, empate pela mais
recente. Contar `Votos` é seguro como "pessoas distintas" porque a chave
composta de `VotoUtil` impede o par `(Avaliação, Usuário)` repetido no banco.

**O voto é um interruptor**, e a regra vive na entidade:

```csharp
public bool AlternarVotoUtil(Guid usuarioId)
{
    if (usuarioId == UsuarioId)
        throw new InvalidOperationException("Você não pode marcar como útil a própria avaliação.");

    var votoExistente = _votos.FirstOrDefault(v => v.UsuarioId == usuarioId);
    if (votoExistente is not null) { _votos.Remove(votoExistente); return false; }

    _votos.Add(new VotoUtil(AvaliacaoId, usuarioId));
    return true;
}
```

Por isso `BuscarComVotos` é o único método do repositório de avaliação **sem
`AsNoTracking()`** — a coleção precisa ficar rastreada para o `ChangeTracker`
perceber o item acrescentado ou removido em memória.

### 6.7 CPF: dígitos verificadores por módulo 11

`CpfHelper.DigitoVerificadorValido` implementa o algoritmo oficial:

```
1. Rejeita todos os dígitos iguais (111.111.111-11 passaria na conta)
2. 1º dígito: Σ (dígito[i] × peso[i]), pesos 10…2, sobre os 9 primeiros
              resto = soma % 11;  dígito = resto < 2 ? 0 : 11 - resto
3. 2º dígito: mesma conta com pesos 11…2, sobre os 10 primeiros
```

> ⚠️ **Esta implementação tem um defeito real.** Ela usa o primeiro dígito
> *calculado* para derivar o segundo e nunca compara o calculado com o
> digitado — só o último dígito é conferido. Ver §9.

### 6.8 Máscaras de entrada

`autenticacao.js` formata telefone, data e CPF conforme a pessoa digita. As três
seguem o mesmo formato: descartam tudo que não é dígito, cortam no comprimento
máximo, e remontam por fatias.

```js
const digitos = value.replace(/\D/g, "").slice(0, 11);
formatado  = "(" + digitos.slice(0, 2);
formatado += ") " + digitos.slice(2, 7);
formatado += "-" + digitos.slice(7, 11);
```

São **conveniência visual, não validação**. A validação de verdade acontece nas
duas barreiras do servidor (§7.1) — a máscara pode ser contornada desligando o
JavaScript, e o sistema continua correto.

### 6.9 Geração da massa de demonstração

`DbInitializer` semeia 4 categorias, 31 subcategorias e 100 produtos, em rodízio
pelas subcategorias para que toda subcategoria tenha ao menos um produto.

As avaliações usam `Random` com **semente fixa** (`20260820`), para que recriar
a base produza sempre as mesmas notas nos mesmos produtos. A distribuição é
enviesada para cima, porque loja real não tem notas uniformes entre 1 e 5:

```
< 0.45 → 5 estrelas      < 0.75 → 4      < 0.90 → 3
< 0.97 → 2               resto  → 1
```

E ~30% dos produtos ficam **sem avaliação nenhuma** — é o único jeito de
exercitar, em demonstração, o ramo do `?? -1` da ordenação.

`GerarAvaliacoesMock` não toca o banco de propósito: é função pura, então dá
para chamá-la duas vezes com a mesma semente e comparar o resultado num teste
de unidade, sem SQLite em memória.

### 6.10 Cotação de frete (`020`)

Todo produto tem peso e três dimensões (`Produto.Peso/Altura/Largura/
Comprimento`, obrigatórios desde esta feature, sem valor padrão — quem
cadastra decide). `DbInitializer` semeia um valor fixo por categoria (Adega
mais pesada e compacta, Souvenir mais leve e volumosa — de propósito, para o
peso cubado e o peso real puderem divergir em sentidos opostos num mesmo
carrinho).

O contrato fica em `IFreteService.Cotar(cepDestino, itens)`, implementado por
`FreteServiceMelhorEnvio` (`Infrastructure/Services/`) — um `HttpClient`
tipado (`AddHttpClient<IFreteService, FreteServiceMelhorEnvio>`) contra a API
do MelhorEnvio, sandbox por padrão. A conversão de/para o formato da API
(`snake_case`, `Services/MelhorEnvio/*MelhorEnvio.cs`) fica isolada num
subpasta própria — se o formato divergir da documentação, o conserto é
local.

**`Cotar` nunca lança.** Falha de rede, timeout, CEP não atendido, credencial
ausente ou inválida são condição esperada, não exceção (Princípio VIII — cada
camada tem um dono de erro, e aqui o dono é o próprio serviço): o método
sempre devolve uma `CotacaoDeFreteDTO`, com `Opcoes` vazia e `Mensagem`
preenchida quando a cotação não foi possível. `CarrinhoController.Index`
nunca precisa de `try/catch` para isso. Isso inclui a própria configuração:
`FreteSettings.UserAgent` tem um valor-padrão não vazio de propósito —
`HttpHeaders.UserAgent.ParseAdd("")` lança `FormatException`, e deixar essa
falha escapar do adaptador quebraria a garantia acima por um detalhe de
configuração, não por falha de transporte de verdade.

A cotação é sobre `decimal`, e o app roda fixo em `pt-BR`, onde `.` é
separador de milhar, não decimal — todo `decimal.Parse`/`TryParse` sobre
strings vindas da API usa `CultureInfo.InvariantCulture` explicitamente, ou
`"37.79"` vira `3779`, não `37,79`. É o defeito mais fácil de não notar nesta
integração: passa em qualquer asserção relacional (`preço > 0`) e só aparece
comparando o valor final com o que a documentação mostra.

A credencial (`FreteSettings:Token`) nunca é versionada — *user secrets* em
desenvolvimento, variável de ambiente em produção e no E2E (RN-05).

### 6.11 Fechamento de pedido (`022`)

Os passos do fechamento (carrinho → conta → endereço → pagamento) vivem
dentro da própria tela do carrinho (`Carrinho/Index`, aceita `passo`) — "a
coluna esquerda troca de parcial, o resumo à direita permanece" é a mesma
ideia de "um endereço, duas representações" que o projeto usa desde a `014`.
`IPedidoService.MontarPasso(passo, carrinho, usuarioId, enderecoId,
servicoDeEntregaId)` monta o que cada passo precisa; o carrinho em si
(sessão ou persistido) é resolvido por `CarrinhoController`, não pelo
serviço — `IPedidoService` não conhece `HttpContext`.

**O endereço escolhido e a opção de entrega escolhida viajam pela
querystring entre os passos, nunca em sessão.** Guardar a cotação em sessão
foi cogitado e recusado ao especificar (replicaria uma cotação que pode
envelhecer, sem necessidade). Isso faz o passo de Pagamento montar seu
formulário com campos ocultos resolvidos no servidor a partir do que já
está na URL — nenhum JavaScript sincroniza valor nenhum entre um rádio e um
campo oculto, o que é o que faz o caminho sem script (RF-05/CA-23)
funcionar por desenho, não por acaso.

**Fechar (`PedidoService.Fechar`) confere antes de gravar, sempre pela
mesma regra: o que a tela exibiu volta como alegação, o servidor
recalcula.** Nove passos, na ordem:

```
1. carrega o carrinho do usuário (o de agora, não o exibido)
2. carrinho vazio                        → recusa
3. algum item indisponível               → recusa, nomeando o item
4. soma os produtos pelo preço de agora
   ≠ valor exibido                       → recusa, devolve o atual
5. re-cota o frete para o endereço (nunca confia na cotação anterior)
   sem cotação, ou opção escolhida sumiu → recusa
   preço ≠ valor exibido                 → recusa, devolve o atual
6. monta Pedido (raiz do agregado) com os itens, ao preço de agora
7. monta Pagamento (Pendente)
8. esvazia o carrinho (sem IUnitOfWork próprio — ver abaixo)
9. UM SalvarAlteracoes
```

Nenhuma recusa lança exceção — todas voltam em
`ResultadoDoFechamentoDTO.Sucesso == false` (Princípio VIII); é erro
esperado do usuário (preço mudou, item saiu do catálogo, frete indisponível
no momento), não falha do sistema.

**`Pedido` é a raiz do agregado** — decisão que a modelagem original
(spec `003`) tinha adiado por escrito. A coleção de itens é exposta como
`IReadOnlyCollection<ItemPedido>`, mapeada pelo campo de apoio privado
(`EF Core` resolve por convenção de nome, `_itens` → `Itens`, sem
configuração extra). `Pagamento` não tem navegação em `Pedido` (é 1:1
configurado do lado de `Pagamento`, igual antes desta feature) — por isso
`IPedidoRepository.AdicionarComPagamento(pedido, pagamento)` adiciona os
dois ao mesmo `DbContext` sem chamar `SalvarAlteracoes`; quem decide o
commit é `PedidoService.Fechar`, e é por isso que passo 8 (esvaziar o
carrinho) usa `IItemCarrinhoRepository` direto, não
`ICarrinhoService.Esvaziar()` — esse método chama `SalvarAlteracoes` por
conta própria, o que quebraria a garantia de "um só" (RF-20/RN-07).

**A vitrine da home e a ordenação "mais vendidos" do catálogo** somam
`ItemPedido.Quantidade` por produto, excluindo pedido cancelado — mesma
forma de subconsulta que `MelhorAvaliados` usa desde a `014`
(`(int?)`/`?? 0` para produto sem venda ir para o fim, não sumir da
consulta). Sem pedidos semeados (`DbInitializer`), a ordenação empataria
os cem produtos em zero e a home mostraria ordem alfabética sob o título
"mais vendidos" — por isso a semeadura de pedidos existe, com situações
variadas e um pedido cancelado.

---

## 7. Padrões que se repetem

### 7.1 Validação em duas barreiras

| Barreira | Onde | Protege | Erro vira |
|---|---|---|---|
| Entrada | `Application/Validators/*Validator.cs` | o **usuário** | mensagem no campo |
| Invariante | construtor/métodos da entidade | o **dado** | exceção |

Duplicar a regra nas duas é esperado. Ter em **apenas uma** é o defeito: só no
validator significa que a API interna aceita lixo; só no domínio significa que a
pessoa recebe uma tela de erro em vez de uma mensagem de campo.

Os validators são registrados por varredura de assembly — criar o arquivo
`*Validator.cs` já o coloca no pipeline.

### 7.2 O repositório não persiste

```csharp
// Repository<T> — só registra a intenção no ChangeTracker
public async Task Adicionar(T entity) => await _context.Set<T>().AddAsync(entity);
public void Atualizar(T entity)       => _context.Set<T>().Update(entity);
public void Remover(T entity)         => _context.Set<T>().Remove(entity);

// UnitOfWork — este sim grava
public Task<int> SalvarAlteracoes(CancellationToken ct = default)
    => _context.SaveChangesAsync(ct);
```

**Um caso de uso que escreve e não chama o `IUnitOfWork` não salvou nada.** Quem
decide quando o lote está pronto é a camada de aplicação. Não existe transação
explícita separada: `SaveChangesAsync` já é atômico — um lote com uma alteração
inválida não persiste nenhuma das outras.

### 7.3 Um endereço, duas representações

O `CatalogoController.Index` devolve a página inteira ou só o bloco do
resultado, conforme o cabeçalho `X-Requested-With`. **Nenhuma rota nova, nenhuma
regra de filtro duplicada.**

### 7.4 O caminho degradado *é* o código real

Todo recurso interativo funciona sem JavaScript, e o caminho sem script não é
uma promessa paralela — é o mesmo código:

| Recurso | Sem JavaScript |
|---|---|
| Filtro do catálogo | `<noscript>` mostra o botão "Aplicar" |
| Paginação | Links comuns, com endereço próprio |
| Favoritar | `<form>` de verdade, POST-Redirect-Get |
| "Ver todas" as subcategorias | `<details>` nativo |
| Busca | `<form method="get">` comum |
| Remover a busca | Um link, não um botão de script |

A exceção declarada: o **convite ao visitante** que tenta favoritar só existe com
script — e é coerente, porque o convite em si é recurso de script.

### 7.5 POST-Redirect-Get

Sucesso de POST redireciona. Existe um desvio justificado: o caminho assíncrono
do favorito não redireciona, porque um `fetch` não cria entrada no histórico —
não há o que recarregar, logo não há o que repetir. O caminho sem script, que é
o que um navegador pode de fato recarregar, redireciona normalmente.

### 7.6 Valores ambientes de rota: a armadilha

Quando você escreve `asp-controller`/`asp-action` **sem** especificar os demais
parâmetros de rota, o gerador de endereço do ASP.NET Core **reaproveita os
valores da rota atual**. Isso já mordeu duas vezes:

```html
<!-- ERRADO: dentro de /Catalogo/doces, isto vira action="/Catalogo/doces" -->
<form asp-controller="Catalogo" asp-action="Index" method="get">

<!-- CERTO: null explícito sobrepõe o valor ambiente -->
<form asp-controller="Catalogo" asp-action="Index" asp-route-apelido="@((string?)null)" method="get">
```

**Ausência de atributo herda; `null` explícito sobrepõe.** Vale igualmente para
`asp-area`.

### 7.7 Português é a língua ubíqua

Classes, métodos, propriedades, rotas, views, mensagens, nomes de teste e
comentários em português. Ficam em inglês só os termos impostos pelo framework
(`Controller`, `IActionResult`, `Task`, `Repository`, `DTO`, `Id`) e o
vocabulário herdado do Identity (`UserName`, `PhoneNumber`, `Email`).

Nome de teste segue `Dado_..._Quando_..._Entao_...`.

### 7.8 As camadas de teste

| Projeto | Camada | Ferramenta |
|---|---|---|
| `DocesCabana.Tests` | unidade + integração | xUnit, Moq, SQLite em memória |
| `DocesCabana.Tests.E2E` | ponta a ponta | Playwright, sobre a aplicação de verdade |

O E2E sobe a MVC num processo filho, apontada para um SQLite descartável e um
adaptador de e-mail que escreve em arquivo — nunca toca a base de
desenvolvimento. Uma instância é compartilhada pela suíte inteira; cada teste
ganha um contexto de navegador novo, para cookie de um não vazar para outro.

**Duas armadilhas de teste que já custaram tempo:**

`ToBeVisibleAsync` **ignora `opacity: 0`**. Para provar que algo está realmente
visível, é preciso ler `getComputedStyle(el).opacity`.

`WaitForLoadStateAsync(NetworkIdle)` marca o fim da **rede**, não o fim do
`.then()` que atualiza a tela. Leitura única logo depois pega o estado antigo.
A saída é usar asserções com retry automático (`Expect(...).ToHaveURLAsync`,
`ToHaveCountAsync`) em vez de ler uma vez só.

---

## 8. Onde o erro é tratado

### 8.1 Quem lança o quê

| Camada | Lança |
|---|---|
| Domínio | `ArgumentException`, `ArgumentNullException`, `InvalidOperationException` |
| Aplicação | `KeyNotFoundException` para recurso ausente; propaga o resto |
| MVC | **nada** — não faz `try/catch` em ação |

### 8.2 `FilterException`, ramo a ramo

Registrado como filtro global. Em `OnActionExecuting` ele guarda o primeiro
argumento da ação (normalmente o DTO do formulário) em `HttpContext.Items`, para
poder redesenhar a tela preenchida se algo falhar.

Em `OnActionExecuted`, cinco caminhos:

```
1. KeyNotFoundException
   → NotFoundResult (404 sem corpo)
   → o middleware de status reexecuta em /Home/NaoEncontrado

2. InvalidOperationException em VotarUtil
   → volta para o Referer (a ação não tem view própria)

3. InvalidOperationException no controlador Carrinho (spec 017)
   → produto que deixou de estar disponível entre a tela carregar e o
     clique (RN-06): a mensagem explica o motivo
   → assíncrono (X-Requested-With) → BadRequestObjectResult com a mensagem
   → comum                         → volta para o Referer (idem VotarUtil:
                                      nenhuma das ações de escrita tem view
                                      própria para redesenhar)

4. Qualquer exceção num POST
   → InvalidOperationException  vira mensagem no ModelState
   → outras                     viram "Um erro interno ocorreu…"
   → redesenha a view da própria ação, com o model recuperado

5. Exceção num GET que não seja KeyNotFoundException
   → não tratada aqui; sobe para o UseExceptionHandler
```

### 8.3 `UseStatusCodePagesWithReExecute` — e a armadilha dele

```csharp
app.UseStatusCodePagesWithReExecute("/Home/NaoEncontrado");
```

Ele intercepta **qualquer** resposta 4xx/5xx **sem corpo** e reexecuta a
requisição contra aquele caminho.

> ⚠️ Isso engoliu um `401` na `015`. A ação de favoritar devolvia
> `Unauthorized()`, que não escreve corpo — o middleware transformou em 404
> antes de chegar ao script, que ficou sem como distinguir "precisa entrar" de
> "sumiu". A correção foi devolver `StatusCode(401, new { autenticado = false })`,
> **com corpo**, o que evita a reexecução.

Regra prática: se você precisa que um código de status chegue intacto ao
cliente, **escreva um corpo na resposta.**

### 8.4 Segurança na borda

- `[HttpPost]` **sempre** com `[ValidateAntiForgeryToken]`
- Ação que muda estado é `async Task<IActionResult>` e **aguarda** o serviço
- Guarda de `ModelState` antes de qualquer efeito colateral
- Área administrativa com `[Authorize]` explícito
- Endereço de retorno passa por `Url.IsLocalUrl` — sem isso a tela de login
  vira trampolim para outro site
- Senha do administrador semeado vem de *user secret*, nunca literal no código
- Mensagens de recuperação de senha são idênticas para login existente e
  inexistente (evita enumeração de conta)
- Política do Identity: senha de 6+ com maiúscula, minúscula, dígito e símbolo;
  bloqueio por 15 min após 5 tentativas

---

## 9. Dívidas conhecidas e achados desta leitura

### 9.1 Achados desta leitura

**✅ `CpfHelper` aceitava CPF com o primeiro dígito verificador errado — resolvido na `019`.**

O método calculava o primeiro dígito, **usava o valor calculado** para derivar
o segundo, e no fim conferia apenas o último:

```csharp
primeirosDigitos += digito;                     // ← usava o CALCULADO, não o digitado
…
return digitos.EndsWith(digito.ToString());     // ← só o 2º dígito era conferido
```

Verificado por teste: `52998224795`, `52998224705` e `52998224715` são todos
inválidos e **os três passavam**. Na prática, qualquer valor no 10º dígito era
aceito, desde que o 11º casasse com o recalculado — cerca de 10% dos CPFs
malformados entravam.

Os testes existentes só corrompiam o **segundo** dígito (`529.982.247-26`), que
era justamente o caso que o código pegava. Por isso nunca falharam.

A `019` extraiu `CalcularDigito` e passou a conferir os dois dígitos contra o
que a pessoa digitou (`DocesCabana.Domain/Helpers/CpfHelper.cs`).

**✅ A home carregava o catálogo inteiro para mostrar 8 produtos — resolvido na `019`.**

`HomeController.Index` chamava `BuscarTodosProdutos()`, que fazia
`_context.Set<Produto>().AsNoTracking().ToListAsync()` — **os 100 produtos** —
filtrava os inativos em memória e mapeava ~99 DTOs. Só então `VitrineProdutos`
aplicava `.Take(8)` e descartava 91.

A `013` corrigiu o sintoma visual (o corte mora no componente, que é o lugar
certo). A `019` corrigiu a consulta: `ProdutoService.BuscarDestaquesDaVitrine`
reaproveita `BuscarPaginaDoCatalogo` com filtro vazio, ordenação por avaliação
e `tamanhoDaPagina` igual ao limite da vitrine — o Skip/Take vira LIMIT/OFFSET
no banco, e só os 8 produtos exibidos chegam à memória. A mesma consulta já
respeita RN-02 (produto inativo não aparece em listagem nenhuma) e agora marca
os favoritos do usuário autenticado, corrigindo também o achado do carrossel
que nunca refletia favorito real (`015`, tabela 9.2).

**✅ O comentário do `EstrelasNota` descrevia o algoritmo errado — resolvido na `019`.**

Dizia que nota 4,5 deixava "a 5ª estrela em 0%". O código deixa em 50%, que é
o correto. O comentário parecia ter sido editado no meio e ficara contraditório
consigo mesmo (*"a 5ª estrela fica 0%, e a 5ª... a 4ª fica 100%, a 5ª fica 0%"*).
Reescrito para descrever o comportamento real
(`DocesCabana.MVC/Views/Shared/Components/EstrelasNota/Default.cshtml`).

### 9.2 Dívidas já registradas nas specs

| Dívida | Desde | Situação |
|---|---|---|
| Estouro horizontal do cabeçalho a 375px | `009` | Presente em toda página; declarado fora de escopo em `009`, `013`, `015` e `016` |
| Carrossel da home não reflete favorito real | `015` | ✅ Resolvido na `019` — `BuscarDestaquesDaVitrine` marca os favoritos do usuário autenticado |
| Ordem das categorias no cabeçalho é a do banco | `013` | Sem critério definido; repetido em `014`, `015`, `016` |
| `.linha-dupla` não empilha em tela estreita | `016` | Corrigido só no cadastro de produto; cadastro de cliente e de administrador seguem como sempre foram |
| Atalho "Conta" desabilitado no cabeçalho | `014` | A página não existe |
| `Promocao` existe e nunca foi usada | `003` | Entidade completa, sem nenhum consumidor |
| `Estoque` existe e nunca foi usada | `003` | `ProdutoStatus.ForaDeEstoque` é marcado à mão |
| Escrever avaliação | `014` | Barreira de dados fechada (índice único); falta a verificação no serviço e a tela |

### 9.3 O que existe no modelo e ainda não tem comportamento

Restam duas das quinze tabelas modeladas sem nenhum código que as use:
`Estoque` e `Promocao`. `Pedido`, `ItemPedido` e `Pagamento` ganharam
comportamento completo na `022` (fechamento de pedido) — `ItemCarrinho`
entrou no modelo pela `017`, que também deu tela e fluxo completo ao
carrinho. `Endereco` tem entidade, tabela e tela desde a `018`
(`Conta > Endereços`).

---

## Para onde ir depois

- **As decisões e o porquê de cada uma:** `specs/README.md` e as 16 pastas de
  spec. Cada uma tem `spec.md` (o quê), `plan.md` (como e por que assim),
  `tasks.md` (o diário da execução) e `checklist.md` (o que foi provado e como).
- **As regras inegociáveis:** `.specify/memory/constitution.md`, com o histórico
  de emendas ao final — cada emenda diz qual feature a motivou.
- **O modelo de dados:** `ModelagemBancoTCC.dbml`.
