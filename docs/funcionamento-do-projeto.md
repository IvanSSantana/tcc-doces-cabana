# Como o Doces Cabana funciona

Guia técnico para quem vai mexer no código. Cobre a arquitetura, o caminho de
cada página do servidor até a tela, o front (CSS e JS) e o porquê das decisões.
As regras normativas estão na [constituição](../.specify/memory/constitution.md);
o histórico de cada decisão está em [`specs/`](../specs/).

---

## 1. Arquitetura em uma página

ASP.NET Core MVC (.NET 10) em Clean Architecture de quatro projetos. A
dependência só aponta para dentro:

```
MVC ──────► Application ──────► Domain
 │                                 ▲
 └────────► Infrastructure ────────┘
```

| Projeto | Papel | O que tem |
|---|---|---|
| `DocesCabana.Domain` | Regra de negócio pura. Não referencia nenhum pacote. | Entidades ricas (`Produto`, `Pedido`, `Cupom`, `Promocao`...), enums, value object `FaixaDeRetirada`, helpers (`CpfHelper`, `TextoHelper`, `DescontoHelper`), contratos `IRepository<T>` e `IUnitOfWork`. |
| `DocesCabana.Application` | Casos de uso. Só conhece o Domain. | Services, DTOs, validators (FluentValidation), mappers manuais, contratos de repositório e de serviços externos (`IFreteService`, `IEmailService`, `IArmazenamentoDeImagem`, `ICacheDaAplicacao`). |
| `DocesCabana.Infrastructure` | Implementa os contratos. Único lugar com EF Core, Identity, HTTP externo e SMTP. | `DocesCabanaDbContext` (Postgres/Supabase), repositórios, `UnitOfWork`, migrations, Identity (`ContaDeAcesso`), Melhor Envio, Supabase Storage, e-mail, cache em memória, módulos de DI. |
| `DocesCabana.MVC` | Borda web. | Controllers, Views Razor, ViewComponents, filtros globais, helpers de sessão, `wwwroot` (CSS/JS). |

### Caminho de uma requisição

```
Navegador ─► Controller ─► Service (Application) ─► Repository (Infrastructure) ─► Postgres
                 │               │  entidade ─► Mapper ─► DTO
                 ◄── View Razor ◄┘  (o controller entrega o DTO como Model)
```

1. O **controller** lê rota/query/form, monta um DTO de entrada e chama o service.
   Não tem regra de negócio nem `try/catch`.
2. O **service** carrega entidades pelo repositório, aplica a regra (às vezes
   delegando à própria entidade), converte para DTO com um `*Mapper` estático e
   devolve.
3. Em escrita, o service chama métodos de intenção da entidade
   (`produto.AlterarPreco(...)`) e depois `IUnitOfWork.SalvarAlteracoes()`. O
   repositório **não salva**: só registra no `ChangeTracker`. Por isso um caso
   de uso com várias mudanças é atômico (um único `SaveChangesAsync`).
4. A **view** Razor recebe o DTO e renderiza HTML no servidor.

### Peças transversais

- **Validação em duas barreiras.** O FluentValidation valida o DTO antes da
  action (`ModelState`) e protege o usuário com mensagem no campo. O construtor
  ou método da entidade valida de novo e lança exceção, protegendo o dado. A
  duplicação é proposital.
- **Erros.** O domínio lança `ArgumentException`/`InvalidOperationException`.
  A aplicação lança `KeyNotFoundException`. O filtro global
  [`FilterException`](../DocesCabana.MVC/Filters/FilterException.cs) traduz:
  `KeyNotFound` vira 404. `InvalidOperation` em POST vira erro no `ModelState`
  e a mesma view volta. No carrinho via AJAX vira `400 { erro }`.
- **Fusão de carrinho.** O filtro global
  [`FiltroFusaoDeCarrinho`](../DocesCabana.MVC/Filters/FiltroFusaoDeCarrinho.cs)
  roda em toda action. Se o usuário está logado e há itens no carrinho da
  sessão (montado enquanto era anônimo), ele funde esses itens no carrinho do
  banco e limpa a sessão.
- **Sessão** (`AddSession`, em memória) guarda só três coisas, todas pequenas:
  carrinho anônimo (JSON de `ItemDoCarrinhoDTO`), último CEP consultado e
  código do cupom. Nunca guarda preço ou desconto: esses valores são
  recalculados a cada exibição para não ficarem velhos.
- **Cache em memória** (`ICacheDaAplicacao` → `IMemoryCache`), só para leituras
  quentes da vitrine:
  - `categorias-com-subcategorias` (menu do header e catálogo);
  - `promocoes-vigentes:{dia}` (expira em 1 dia; a chave já muda na virada);
  - `banners-vigentes` (carrossel da home).
  Toda escrita que afeta esses dados remove a chave. Exemplo: salvar um produto
  remove as chaves de categorias e de banners. A cotação de frete tem cache
  próprio de 10 min, como decorator (ver §2.4).
- **Autenticação.** ASP.NET Identity com cookie. `Usuario` (pessoa, no domínio)
  é separado de `ContaDeAcesso` (credencial, `IdentityUser<Guid>`, na
  infraestrutura). Papel `Administrador` protege a área `/Admin` via
  `[Authorize(Roles = ...)]`. O lockout é de 5 tentativas / 15 min. A
  desativação de admin usa `LockoutEnd` muito no futuro mais troca do security
  stamp. Com o intervalo de validação em 1 min, uma sessão aberta cai em até 1
  minuto.
- **Horário.** O banco grava `DateTime` em UTC.
  [`HorarioDeBrasilia`](../DocesCabana.Application/Helpers/HorarioDeBrasilia.cs)
  converte para UTC−3 quando a regra depende do dia local (promoção vigente,
  retirada hoje, painel). O relógio é `TimeProvider`, injetado para ser
  testável.
- **Inicialização** ([`Program.cs`](../DocesCabana.MVC/Program.cs)): aplica as
  migrations, preenche nomes normalizados, semeia a agenda de retirada e, fora
  de produção, semeia dados de exemplo. A cultura é fixa em `pt-BR`.

---

## 2. Lógica (back-end) de cada página

> Convenção usada abaixo: **Controller.Action → Service.Método → Repositório**.
> Toda página é renderizada no servidor. Onde há AJAX, o servidor devolve uma
> *partial view* (HTML) e não JSON. A exceção é o favorito, que devolve JSON.

### 2.1 Home (`/`)

`HomeController.Index` chama dois services:

- `ProdutoService.BuscarDestaquesDaVitrine(limite)` reutiliza a **mesma consulta
  do catálogo** (`ProdutoRepository.BuscarPaginaDoCatalogo`) com filtro vazio e
  ordenação `MaisVendidos`, pegando a primeira página. Ele marca os favoritos do
  usuário (`FavoritoRepository.IdsPorUsuario`) e aplica as promoções vigentes.
  O resultado vai como `Model` (`List<ProdutoDTO>`).
- `BannerService.ListarVigentesDoCarrossel()` lê do cache `banners-vigentes`.
  Se a chave não existe, consulta os banners ativos dentro da validade cujo
  destino (categoria, subcategoria ou produto) ainda existe. Vai em
  `ViewData["Banners"]`.

A view junta a partial `_Carrossel` (banners), `_Categorias` e o ViewComponent
`VitrineProdutos`, que desenha os cards com `CardProduto`.

O **header** é um ViewComponent (`HeaderViewComponent`) presente em todas as
páginas. Ele lê as categorias do cache e conta os itens do carrinho: do banco
se o usuário está logado, da sessão se não está.

### 2.2 Catálogo (`/Catalogo/{apelido?}`)

O catálogo funciona assim: o `CatalogoController.Index` recebe os filtros pela
URL e monta um `CriteriosDoCatalogoDTO`. O `CatalogoService.Montar` resolve
esses critérios e busca no banco, com filtro e paginação, os produtos. Depois
devolve um `CatalogoDTO` que a view renderiza.

Filtros da URL: categoria pelo apelido na rota, `subcategorias[]`,
`semAcucar`, `ordenacao`, `termo` e `pagina`.

Passo a passo do `Montar`:

1. Carrega as categorias com subcategorias do cache e resolve o **apelido**
   (slug) da categoria para o seu Id. Apelido inexistente lança
   `KeyNotFoundException` (404).
2. Filtra as subcategorias marcadas para apenas as que pertencem à categoria
   atual.
3. Normaliza o termo de busca com `TextoHelper.Normalizar` (minúsculas, sem
   acento, sem espaço duplo).
4. Monta um `FiltroCatalogoDTO` (com Ids, não apelidos) e chama o repositório
   duas vezes: `ContarNoCatalogo` (total) e `BuscarPaginaDoCatalogo`
   (`Skip/Take`, 12 por página). A página pedida é saneada com `Clamp` entre 1 e
   o total de páginas.
5. O repositório monta um único `IQueryable`. Ele exclui produtos inativos,
   filtra por categoria via `Subcategoria.CategoriaId` e por
   `NomeNormalizado.Contains(termo)`. Na ordenação, `MelhorAvaliados` e
   `MaisVendidos` são **subconsultas correlacionadas** (média das notas; soma
   das quantidades vendidas em pedidos não cancelados) traduzidas para SQL.
   Nada é ordenado em memória.
6. Marca os favoritos e aplica as promoções vigentes (preço promocional e
   selos) no `ProdutoMapper`.

Renderização: na primeira carga a action devolve a página inteira. Se o pedido
vem com `X-Requested-With: XMLHttpRequest`, devolve só a partial
`_ResultadoCatalogo` (ver o JS em §3.3). As URLs são montadas por
[`EnderecoDoCatalogo`](../DocesCabana.MVC/Helpers/EnderecoDoCatalogo.cs), então
todo filtro é um link real, que pode ser compartilhado e funciona sem JS.

### 2.3 Produto (`/Produto/Detalhes/{id}`)

`ProdutoService.BuscarDetalhe` carrega o produto com a subcategoria
(`AsNoTracking`). Produto inativo responde 404. Em seguida chama o
`AvaliacaoService` para três coisas:

- `ResumirPorProduto`: média e distribuição por nota;
- `ListarPorProduto`: lista ordenada por `Relevantes` (votos "útil") ou
  `MaisRecentes`, limitada por `exibir` (múltiplo de 5, de 5 a 100, saneado no
  controller);
- `VerificarSituacao`: diz se o usuário pode avaliar. A regra é: só quem tem um
  pedido **entregue** com o produto e ainda não avaliou.

As promoções vigentes são aplicadas por último. O resultado é um
`ProdutoDetalheDTO`.

Ações:

- `Avaliar` (POST): em caso de erro devolve a partial do formulário com status
  400, para o modal mostrar sem recarregar. Em caso de sucesso faz PRG e
  redireciona para `#avaliacoes`. Admin recebe `Forbid`.
- `VotarUtil` (POST): alterna o voto do usuário (`VotoUtil`, chave composta
  avaliação + usuário).

### 2.4 Carrinho e fechamento (`/Carrinho`)

É a página mais complexa. Uma única view (`Carrinho/Index`) mostra os quatro
passos do fechamento: **Carrinho → Conta → Endereço → Pagamento**. O passo
"Conta" só aparece para o anônimo.

**Duas fontes de carrinho:**

- Logado: tabela `ItemCarrinho` (chave composta usuário + produto), via
  `CarrinhoService.ObterDoUsuario`.
- Anônimo: lista na sessão. `CarrinhoService.MontarAvulso` busca os produtos
  por Id e monta o mesmo `CarrinhoDTO`.

As duas fontes chegam no mesmo `CarrinhoMapper`. Ele calcula, para cada linha,
a disponibilidade (produto inativo ou fora de estoque fica indisponível e não
entra no total), a melhor promoção que alcança o produto
(`PromocoesVigentes.MelhorPara`, a de maior desconto) e o subtotal.

**Frete.** O CEP digitado fica na sessão. Em cada exibição,
`CotarComCepGuardado` chama `IFreteService.Cotar`. A implementação registrada é
`FreteServiceComCache`, que envolve `FreteServiceMelhorEnvio` (HTTP para a API
da Melhor Envio). A chave de cache é CEP + (produto × quantidade × preço) de
cada linha. Um carrinho diferente nunca recebe o frete de outro. Cotação vazia
não entra no cache.

**Passos.** `PedidoService.MontarPasso` monta o `PassoDoFechamentoDTO`
(em `ViewData["PassoDoFechamento"]`):

- passo Endereço/Pagamento: lista os endereços do usuário, cota o frete do
  endereço escolhido (o padrão, se nenhum foi escolhido) e pré-seleciona a opção
  mais barata;
- retirada na loja: `AgendaDeRetiradaService.ConsultarHoje` devolve as faixas
  de horário ainda disponíveis hoje. Sem faixa, a modalidade volta para entrega;
- cupom (só logado): `CupomService.Avaliar` recalcula o desconto sobre o
  carrinho e o frete atuais.

**Fechar pedido** (`PedidoController.Fechar` → `PedidoService.Fechar`). O
servidor **não confia no que a tela mostrou**. Ele recalcula tudo e compara
com os valores exibidos que vieram no form (`ValorDosProdutosExibido`,
`ValorDoFreteExibido`, `DescontoDoCupomExibido`). Qualquer divergência recusa o
pedido com uma mensagem ("o valor mudou, confira") e a tela volta com o valor
atual. O mesmo vale para item indisponível, faixa de retirada esgotada e opção
de frete que sumiu.

Se tudo confere:

1. cria o `Pedido` (entrega ou `Pedido.ParaRetirada`);
2. copia cada linha como `ItemPedido`, com preço unitário, desconto e rótulo da
   promoção **congelados** (snapshot);
3. aplica o cupom (`CodigoDoCupom` também fica congelado);
4. cria o `Pagamento`;
5. esvazia o carrinho;
6. chama `SalvarAlteracoes()` uma vez. Tudo entra numa única operação atômica.

O fluxo termina em PRG para `/Pedido/Confirmacao/{id}`.

### 2.5 Meus pedidos (`/Pedido/Meus`, `/Pedido/Detalhe/{id}`)

`PedidoService.ListarDoUsuario` e `BuscarDetalhe` sempre filtram pelo usuário
logado. O pedido de outro usuário resulta em `KeyNotFound`, ou seja, 404.
Assim ninguém descobre que o pedido existe. `MarcarComoEntregue` (POST) deixa o
cliente confirmar o recebimento, e é isso que libera a avaliação do produto.

### 2.6 Favoritos (`/Favorito`)

`FavoritoService.Alternar` adiciona ou remove (chave composta produto +
usuário). Com AJAX devolve `{ favoritado: bool }`. Para o anônimo devolve 401,
e o JS abre o modal de login. Sem JS, redireciona de volta (`returnUrl`
validado com `Url.IsLocalUrl`). `ListarDoUsuario` alimenta a página de
favoritos.

### 2.7 Conta e endereços (`/Conta`)

- Dados pessoais: o service checa se CPF ou e-mail já está em uso por outra
  conta. O e-mail do admin principal não pode ser trocado. Depois de salvar,
  `RenovarSessao` reemite o cookie com as claims novas.
- Endereços: CRUD pelo `EnderecoService`. Endereço repetido (mesmo CEP, número
  e complemento) é barrado. Há sempre um endereço **padrão**; ao excluir o
  padrão, outro é promovido.

### 2.8 Autenticação (`/Autenticacao`)

- Login aceita e-mail ou CPF. `UsuarioService.RealizarLogin` traduz o
  `SignInResult` do Identity para o enum `ResultadoDoLogin`. Assim a
  Application não conhece o Identity.
- Cadastro cria `ContaDeAcesso` (credencial) e `Usuario` (pessoa) juntos.
- Esqueceu a senha: gera um token do Identity e envia o link por e-mail. A
  mensagem é **idêntica** para conta existente e inexistente, para evitar
  enumeração. Em dev, `EmailServiceArquivo` grava o e-mail em arquivo em vez de
  enviar por SMTP.

### 2.9 Institucional

`QuemSomos` e `Privacidade`: páginas estáticas, sem service.

### 2.10 Área administrativa (`/Admin/*`)

Todos os controllers têm `[Authorize(Roles = Papeis.Administrador)]`.

| Página | Lógica |
|---|---|
| **Produtos** | `ProdutoService.ListarParaGestao(filtro, pagina)`: ativos/inativos, paginado. Ver, cadastrar e editar acontecem num **modal**: o GET devolve a partial do formulário e o POST com erro devolve a partial com status 400. A imagem é enviada ao Supabase Storage (`ArmazenamentoSupabase`, POST HTTP com nome `Guid`+extensão; aceita JPG/PNG/WEBP até 5 MB). A URL pública fica em `Produto.ImagemUrl`. "Excluir" é **inativação** (soft delete), porque pedidos antigos referenciam o produto. |
| **Promoções** | Tipos: percentual, valor fixo, leve X pague Y. Abrangência: produtos escolhidos, categoria ou subcategoria. Validade em `DateOnly`. O cliente recebe a melhor promoção aplicável por linha. |
| **Cupons** | Código único (normalizado). Tipos: percentual, valor fixo, frete grátis. Regras: valor mínimo, um uso por cliente (checado em `PedidoRepository.ClienteJaUsouCupom`) e restrição a categoria/subcategoria. A gestão mostra quantos usos cada cupom teve. |
| **Banners** | Imagem no Supabase. Destino é **exatamente um** entre categoria, subcategoria e produto. Isso é garantido também no banco pela check constraint `CK_Banner_UmDestino`. Toda escrita invalida o cache do carrossel. |
| **Retirada** | Agenda única (`AgendaDeRetirada`) com horário por dia da semana e dias bloqueados (tabelas filhas de chave composta). A entidade calcula as faixas disponíveis a partir de "agora". |
| **Vendas** | `PainelDeVendasService.Montar(periodo, data)`: busca os pedidos não cancelados do intervalo (convertido de Brasília para UTC) e do intervalo anterior. Calcula receita (sem frete), quantidade de pedidos, ticket médio e variação %. Também monta a série temporal, top 10 produtos, vendas por dia da semana e por hora. Tudo agregado em C# sobre a lista de pedidos do período. |
| **Administradores** | Só o admin principal (e-mail fixo em `AdministradorPrincipal`) cadastra, edita, desativa e reativa os outros administradores. |

---

## 3. Front-end

### 3.1 Renderização: servidor primeiro, JS como melhoria

**Toda página é renderizada no servidor** (Razor). Não há SPA nem framework JS,
e não há JSON para montar a tela. A parte "dinâmica" segue um único padrão:

1. O JS intercepta o `submit` ou o clique num link.
2. Faz `fetch` para **a mesma URL** com o cabeçalho
   `X-Requested-With: XMLHttpRequest`.
3. O controller detecta esse cabeçalho e devolve uma **partial view** (HTML
   pronto).
4. O JS troca só aquele bloco do DOM (`replaceWith`/`innerHTML`) e, quando faz
   sentido, atualiza a URL com `history.pushState`.

**Por quê:** a regra de exibição fica num lugar só (a partial Razor). Ela é a
mesma na carga inicial e na atualização, então não existe template duplicado
em JS. E tudo **funciona sem JavaScript**: todo botão é um `<form>` ou `<a>`
de verdade, e sem JS o navegador faz o POST/GET normal e o servidor responde
com PRG. Em caso de falha no `fetch`, o JS cai para `window.location.href = url`.

Páginas estáticas (sem dado dinâmico): Institucional, Login/Cadastro (exceto
as máscaras), confirmação de pedido.

### 3.2 CSS: organização, grid e flex

- **Sem framework CSS.** Os arquivos são separados por responsabilidade:
  - [`tokens.css`](../DocesCabana.MVC/wwwroot/css/tokens.css): único lugar com
    valores literais de cor, tipografia e espaço (escala de 4px). O resto usa
    `var(--...)`.
  - `base.css`: reset e elementos.
  - `components/`: botão, card, header, formulário, modal etc. São carregados
    no `_Layout`.
  - `pages/`: um arquivo por página, carregado só pela view que o usa (seção
    `Styles`).
- **Breakpoints fixos:** 480 / 768 / 1024 / 1280px, sempre em
  `@media (max-width: ...)`. A convenção está escrita no `tokens.css` porque
  variável CSS não funciona dentro de `@media`.

**Regra prática usada no projeto:**

- **Grid para layout em duas dimensões ou com colunas de largura definida**:
  - estrutura de página com barra lateral: catálogo `260px 1fr`, carrinho
    `1fr 320px` (resumo fixo à direita, `position: sticky`), produto
    `1fr 1fr` (imagem | informações);
  - grades de cards: catálogo `repeat(3, 1fr)`, que vira 2 e depois 1 nos
    breakpoints;
  - linhas internas com colunas alinhadas: item do carrinho `96px 1fr auto`
    (foto | dados | preço), barra de distribuição de notas
    `2.5em 1fr 2em`, cartões do painel `repeat(3, minmax(0, 1fr))`, agenda de
    retirada.
  - `minmax(0, 1fr)` aparece para que texto longo não estoure a coluna.
- **Flex para alinhamento em uma dimensão**: header (logo, busca, ícones),
  botões com ícone, conteúdo interno do card (empurra preço e botão para baixo
  com `margin-top: auto`), rodapé, campos de formulário, chips de filtro,
  trilha do carrossel (`translateX` sobre uma linha flex).

**Por quê:** o grid define a malha e mantém colunas alinhadas entre linhas
diferentes (ex.: todas as linhas do carrinho com o preço na mesma coluna). O
flex distribui itens de tamanho variável numa linha sem precisar saber quantos
são. Na contagem do código, a página do carrinho, a mais estruturada, usa 10
grids e 39 flex.

### 3.3 JavaScript por página

JS puro (vanilla), um arquivo por responsabilidade, em IIFE ou
`DOMContentLoaded`. Os scripts globais estão no `_Layout`; os de página, na
seção `Scripts` da view. Bibliotecas: jQuery Validation Unobtrusive (validação
no cliente gerada pelos adapters do FluentValidation) e Chart.js (só no painel
de vendas).

| Arquivo | Onde | O que faz |
|---|---|---|
| `components/carrinho.js` | global | Intercepta os forms de acrescentar, alterar quantidade, remover e calcular frete. Envia por `fetch` + `FormData`, troca o bloco `_ItensDoCarrinho`, atualiza a bolha do header e mostra o aviso "adicionado ao carrinho". Também cuida da máscara de CEP e do preenchimento automático de endereço via **ViaCEP**, no cadastro de endereço dentro do fechamento. |
| `components/favorito.js` | global | Envia o form do coração por AJAX e alterna o ícone com base no JSON `{favoritado}`. Se a resposta for 401, abre o modal de login. |
| `components/modal-login.js` | global | Abre o `<dialog>` de login (`showModal`). |
| `components/vitrine-produtos.js` | global | Carrossel horizontal dos cards da vitrine: calcula itens visíveis por breakpoint, setas, pontos e `translateX`. Recalcula no `resize`. |
| `carrossel.js` | Home | Carrossel de banners: setas, pontos e troca automática (`setInterval`). Com um banner só, desliga os controles. |
| `pages/catalogo.js` | Catálogo | Filtros, ordenação, busca e paginação sem recarregar. Faz `fetch` da URL, troca `#resultado-catalogo` e chama `pushState`, então o voltar do navegador funciona. Mostra "carregando" só depois de 200 ms para não piscar. Preserva a rolagem. |
| `pages/produto.js` | Produto | Seletor de quantidade (+/−). Modal de avaliação: envia por AJAX, mostra o form de volta em caso de 400, redireciona com âncora `#avaliacoes` em caso de sucesso. Contador de caracteres. |
| `pages/conta.js` | Conta | Máscaras e ViaCEP no formulário de endereço. |
| `autenticacao.js` | Login/Cadastro | Mostrar/ocultar senha. Máscaras de celular, data e CPF. |
| `pages/gestao-produtos.js` | Admin Produtos | Filtro e paginação da tabela via AJAX + `pushState`. Modal único para ver, editar e cadastrar: carrega a partial, envia `multipart` (imagem) e, em caso de 400, re-renderiza o form com os erros. |
| `pages/gestao-promocoes.js` | Admin Promoções | Mostra só os campos do tipo e da abrangência escolhidos. Busca de produtos por nome feita no navegador. |
| `pages/gestao-cupons.js` | Admin Cupons | Mostra os campos conforme o tipo e a restrição. Desabilita a lista escondida para ela não ir no POST. |
| `pages/gestao-banners.js` | Admin Banners | Mostra só o seletor do destino escolhido. |
| `pages/painel-de-vendas.js` | Admin Vendas | Lê o JSON que o Razor deixou em `<script type="application/json" id="dados-do-painel">` e desenha os gráficos com Chart.js. Cartões e tabelas já vêm em HTML, então sem JS a página continua legível. |

---

## 4. Decisões e tipos de dados

| Decisão | Por quê |
|---|---|
| **`Guid` como chave de toda entidade**, gerado no construtor da entidade | O objeto tem identidade antes de ir ao banco (o `Pedido` cria `ItemPedido` e `Pagamento` com o `PedidoId` já conhecido, tudo num `SaveChanges`). Também evita URLs enumeráveis (`/Pedido/Detalhe/1`, `2`, ...). |
| **`decimal` para dinheiro** (`decimal(10,2)` em preço/desconto, `decimal(18,2)` em totais) | `double` tem erro de arredondamento binário. O arredondamento de desconto é explícito (`MidpointRounding.AwayFromZero`, em `DescontoHelper`). |
| **`decimal(10,3)` para peso e dimensões** | A Melhor Envio exige peso em kg e medidas em cm. Três casas cobrem gramas. |
| **Enums como `byte`** (`ProdutoStatus`, `PedidoStatus`, `TipoDeDesconto`...) com valores explícitos | Ocupam 1 byte no banco (`smallint`). O valor numérico fixo impede que reordenar o enum corrompa dados já gravados. |
| **`short` para quantidade** | O carrinho tem um teto (`ItemCarrinho.QuantidadeMaxima`); `int` sobraria. |
| **`DateOnly` / `TimeOnly`** em validade de promoção/cupom, dias bloqueados e faixa de retirada | São datas e horários de calendário, sem fuso. Mapeados para `date`/`time` no Postgres, sem o risco de "virar o dia" na conversão UTC. |
| **`DateTime` em UTC** em `Pedido.Data` | É um instante real. A conversão para Brasília acontece só na apresentação ou no recorte de período. |
| **CPF e celular como `string` só com dígitos** (`varchar(11)`) | CPF pode começar com zero, e não se faz conta com ele. A máscara existe só na tela. CPF tem índice único. |
| **`NomeNormalizado`** gravado no produto | A busca ignora acento e caixa com `Contains` simples no SQL, sem função no banco nem extensão do Postgres. É recalculado pela entidade a cada `AlterarNome`. |
| **Apelido (slug) de categoria na URL** | URL legível (`/Catalogo/bolos`). O service resolve o apelido para Id antes da consulta. |
| **Snapshot no pedido** (preço unitário, desconto e rótulo da promoção, código do cupom, transportadora, prazo) | Mudar o preço ou apagar uma promoção amanhã não pode alterar um pedido de ontem. |
| **Soft delete de produto** (`Status = Inativo`) | Pedidos, avaliações e favoritos apontam para o produto. |
| **Chaves compostas** em `ItemCarrinho`, `Favorito`, `VotoUtil`, `Avaliacao (usuário, produto)` único | A unicidade é regra de negócio (um favorito por produto, uma avaliação por cliente) garantida pelo banco, não só pelo código. |
| **Sessão guarda só identificadores** (itens, CEP, código do cupom) | Preço, frete e desconto mudam. O servidor recalcula e, no fechamento, recusa se diverge do que o cliente viu. |
| **Imagem no Supabase Storage**, não no banco nem no disco | O servidor da aplicação fica sem estado. O banco guarda só a URL pública. |
| **Frete com decorator de cache** (`FreteServiceComCache` envolve `FreteServiceMelhorEnvio`) | Cada ação do carrinho cotaria de novo na API externa. O cache fica fora da implementação real, que continua simples e testável. |
| **Mappers manuais**, sem AutoMapper | O mapeamento fica explícito e fácil de depurar. Vários mappers calculam (promoção, disponibilidade), o que o AutoMapper esconderia. |
| **DTO como `record`** quando é valor imutável de entrada/saída (filtros, cotações, resumos); **`class`** quando é model de formulário com binding | O model binder do MVC e o FluentValidation trabalham melhor com propriedades `get; set;`. Os records dão igualdade por valor e `with`. |
| **Usuário (domínio) separado de ContaDeAcesso (Identity)** | O domínio não depende do ASP.NET. A credencial é detalhe de infraestrutura. |
| **Postgres (Supabase)** com connection string em *user secrets* | Banco gerenciado. Segredo nunca vai para o repositório. Os testes de integração sobem um Postgres descartável com Testcontainers. |

> **Atenção:** a entidade `Estoque` (tabela `Estoque`) existe no modelo, mas
> nenhum service a usa hoje. A disponibilidade de compra é decidida só por
> `Produto.Status` (`Ativo` / `ForaDeEstoque` / `Inativo`). Do mesmo modo,
> `Pagamento` é criado como registro, sem integração com gateway.

---

## 5. Testes

- `DocesCabana.Tests/Units`: entidades (sem mock), services (repositório via
  Moq), validators, controllers, filtros, mappers e helpers.
- `DocesCabana.Tests/Integration`: repositórios contra um Postgres real
  descartável (Testcontainers).
- `DocesCabana.Tests.E2E`: Playwright, só para as jornadas críticas (cadastro,
  login, carrinho, fechamento, cupom, meus pedidos, recuperação de senha,
  admin). Usa page objects em `Paginas/`.
- Nome dos testes: `Dado_..._Quando_..._Entao_...`. O fluxo é TDD (teste
  falhando antes do código).
