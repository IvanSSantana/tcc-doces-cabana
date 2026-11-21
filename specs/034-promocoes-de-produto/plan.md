# Plano — Promoções de produto

**ID:** 034-promocoes-de-produto · **Criado em:** 2026-11-18
**Status:** Executado

---

## 1. Objetivo

A entidade `Promocao` existe desde a `003` e nenhum código a usa. O
`Produto.PromocaoId` também nunca foi preenchido. Esta feature dá ao
administrador uma tela para criar promoções e dá ao cliente o preço
promocional na vitrine, no carrinho e no pedido. Uma promoção pode ser
percentual, de valor fixo por unidade ou do tipo "leve X, pague Y". Ela vale
para produtos escolhidos, para uma categoria ou para uma subcategoria. O
desconto é aplicado sozinho, sem código.

É a primeira das duas entregas de descontos. A `035` (cupons de desconto) se
apoia no cálculo de preço e no registro do desconto no pedido que esta feature
constrói. Com isso, a pergunta que o `specs/README.md` deixava aberta fica
respondida: cupom e promoção na vitrine são dois conceitos.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | **Os descontos são divididos em duas features:** `034` (promoções de produto) e `035` (cupons). | Decisão do debate. Cada uma entrega valor sozinha. A `035` depende do cálculo de preço por linha que esta feature cria. | Um plano só: a execução ficaria longa e só haveria entrega no fim. |
| D2 | **A promoção de produto aceita três tipos: `Percentual`, `ValorFixo` e `CompreXLeveY`.** A entidade recusa `FreteGratis` ("Frete grátis só existe como cupom."). | Decisão do debate. Os quatro tipos existem, e frete grátis é coisa do pedido inteiro, não de um produto. "Leve X, pague Y" vale nos dois (promoção e cupom). | Frete grátis como promoção de produto: não há regra para um carrinho com metade dos itens em frete grátis. |
| D3 | **Abrangência exclusiva:** cada promoção vale para **produtos escolhidos**, para **uma categoria** ou para **uma subcategoria**. Só uma das três formas por promoção. | Decisão do debate (o admin escolhe os produtos ou a categoria, e o nível pode ser categoria ou subcategoria). A forma exclusiva deixa a tela e a regra simples de ler. A abrangência por categoria alcança também os produtos cadastrados depois. | Misturar produtos e categorias na mesma promoção: a mesma oferta pode ser feita com duas promoções, e o conflito se resolve por D4. |
| D4 | **Se um produto cair em mais de uma promoção vigente, vale a que dá o maior desconto na linha,** calculado com a quantidade real do carrinho. Na vitrine, o cálculo usa uma unidade. | Decisão do debate. A comparação pela quantidade real faz "leve 3, pague 2" vencer um 10% só quando o cliente leva 3. | A escolha individual sempre vence: um produto escolhido com 5% perderia para a categoria com 20%. Impedir sobreposição no admin: obrigaria o admin a conferir todas as promoções a cada cadastro. |
| D5 | **"Leve X, pague Y" é contado por produto:** a cada X unidades do mesmo produto, X − Y saem de graça. Exemplo: leve 3, pague 2 com 7 unidades dá 2 grátis. A entidade exige X ≥ 2 e 1 ≤ Y < X. | Decisão do debate. O desconto fica preso a uma linha só, que é como o carrinho e o pedido mostram o preço. | Misturar produtos elegíveis (o mais barato sai grátis): o desconto não pertence a uma linha e exigiria um rateio. |
| D6 | **O valor fixo é descontado por unidade. Um produto com preço menor ou igual ao valor não é alcançado pela promoção.** | Uma promoção de R$ 10,00 numa categoria com um doce de R$ 8,00 daria o doce de graça. O erro é fácil de cometer numa abrangência por categoria. | Limitar o desconto ao preço: o produto sairia a R$ 0,00 e o `ItemPedido` exige preço positivo. |
| D7 | **O período é por data (`DateOnly`), sem hora.** A promoção vale do primeiro ao último minuto dos dias escolhidos, no horário de Brasília (UTC−3 fixo, já que o Brasil não tem horário de verão desde 2019). Início e fim podem ser o mesmo dia. | O admin pensa em dias ("de 20 a 25 de novembro"). Uma data sem hora evita a conversão de fuso no banco (`date` no Postgres). O deslocamento fixo dispensa a base de fusos do sistema operacional. | `DateTime` em UTC com hora: o admin precisaria digitar hora, e a comparação de "hoje" mudaria de dia às 21h. |
| D8 | **A data de "hoje" vem de `TimeProvider` (BCL do .NET 8+), injetado no `PromocaoService`.** `HorarioDeBrasilia.Hoje(TimeProvider)` converte. | Os testes de vigência controlam o relógio com `Mock<TimeProvider>` (`GetUtcNow` é virtual), sem pacote novo. | `DateTime.UtcNow` direto no serviço: os testes de vigência dependeriam do dia em que rodam. |
| D9 | **`Produto.PromocaoId`, `Produto.Promocao`, `AplicarPromocao` e `RemoverPromocao` saem do produto.** A relação vira muitos-para-muitos (`PromocaoProduto`), com navegação só em `Promocao.Produtos`. A invariante "produto inativo ou fora de estoque não entra em promoção" muda para `Promocao`, nos métodos que definem os produtos escolhidos. | Com D3 e D4, um produto pode estar em várias promoções, e uma FK no produto só guarda uma. A invariante continua na entidade (Princípio II), agora na que define a relação. | Manter a FK no produto: impediria D4 e não serviria para a abrangência por categoria. |
| D10 | **O enum `PromocaoTipo` passa a se chamar `TipoDeDesconto`.** Os valores não mudam. | A `035` usa o mesmo enum em `Cupom`. `PromocaoTipo` num cupom leria errado (Princípio IV: nome único por conceito). A coluna guarda o número, então não precisa de migration. | Dois enums com os mesmos valores: duplica o conceito. |
| D11 | **O cálculo mora na entidade:** `Promocao.Alcanca(...)` e `Promocao.CalcularDesconto(precoUnitario, quantidade)`. A conta de "leve X, pague Y" e o arredondamento (2 casas, `AwayFromZero`) ficam em `Domain/Helpers/DescontoHelper.cs`, que a `035` reaproveita. | É regra de negócio pura, testável sem mock (Princípio II). | Calcular no serviço: a regra sairia do domínio e se repetiria na `035`. |
| D12 | **A escolha da melhor promoção é feita por `PromocoesVigentes` (`Application/Helpers`).** É um objeto em memória, carregado uma vez por requisição por `IPromocaoService.CarregarVigentes()`, com as promoções vigentes e o mapa subcategoria → categoria. Catálogo, vitrine da home, favoritos, página do produto e carrinho consultam esse objeto. | O número de promoções vigentes é pequeno. Uma consulta por requisição atende a todas as telas, sem uma consulta por produto. O mapa resolve a abrangência por categoria sem `Include` novo nas consultas de produto. | Calcular o preço promocional em SQL: a regra de D4 e D6 ficaria duplicada em LINQ traduzível e em C#. |
| D13 | **O carrinho já sai com o preço líquido.** `LinhaDoCarrinhoDTO` ganha `DescontoDaPromocao` e `RotuloDaPromocao`. `ValorDaLinha` e `CarrinhoDTO.Subtotal` passam a descontar a promoção. | O fechamento já compara o subtotal exibido com o recalculado (`PedidoService.Fechar`). Uma promoção que começa ou termina entre a revisão e o "Finalizar" cai na mensagem que já existe ("O valor dos produtos mudou…"), sem código novo. | Calcular o desconto só no fechamento: o cliente veria um valor no carrinho e pagaria outro. |
| D14 | **`ItemPedido` guarda uma cópia do desconto:** `DescontoDaPromocao` e `RotuloDaPromocao` (por exemplo, "Leve 3, pague 2"), sem FK para `Promocao`. | Editar ou desativar uma promoção nunca altera um pedido já feito. Confirmação, "Meus pedidos" e detalhe mostram o que foi cobrado. | FK de `ItemPedido` para `Promocao`: editar a promoção mudaria o histórico, e desativá-la precisaria de cuidado com pedidos antigos. |
| D15 | **Na vitrine (cartão e página do produto), a promoção percentual ou de valor fixo aparece como preço cheio riscado + preço promocional, mais um selo ("-10%" / "-R$ 5,00").** "Leve X, pague Y" aparece só como selo ("Leve 3, pague 2"), sem riscado, porque com uma unidade não há desconto. Se o produto tiver as duas, aparecem os dois. | Mostra ao cliente as duas ofertas. O carrinho aplica a maior (D4). | Mostrar só a melhor para uma unidade: o cliente nunca descobriria o "leve 3". |
| D16 | **A ordenação por preço do catálogo continua usando o preço cheio.** | A ordenação é feita em SQL (`ProdutoRepository.AplicarOrdenacao`), e o preço promocional é calculado em memória (D12). Fica registrado como limitação na seção 8. | Ordenar em memória: quebraria a paginação no banco. |
| D17 | **A tela administrativa fica em `Admin/Promocao`:** listagem com situação (Vigente, Agendada, Encerrada, Desativada), cadastro e edição em página própria, e ativar/desativar por POST. **Não há exclusão.** | A lista de produtos escolhidos (até 100 caixas de marcação) não cabe bem no modal que a `031` usa para produto. Com D14, desativar resolve tudo o que excluir resolveria, sem apagar histórico de gestão. | Modal como na `031`: rolagem dentro de rolagem com a lista de produtos. Excluir: a confirmação extra não acrescenta nada que desativar não faça. |
| D18 | **O seletor de produtos lista só os produtos `Ativo`, agrupados por subcategoria, com um campo de busca que filtra por nome no navegador.** Sem JavaScript, a lista aparece inteira e funciona. Na edição, um produto escolhido que deixou de estar `Ativo` sai da lista e, ao salvar, sai da promoção. Um aviso no formulário explica isso. | É coerente com a invariante de D9. A busca no navegador basta para 100 produtos, sem endpoint novo. | Listar também produtos inativos: a entidade recusaria salvar. |
| D19 | **O menu "Administração" do cabeçalho ganha o item "Promoções".** | É o único caminho de navegação da área administrativa (`Header/Default.cshtml`). | Link só na tela de produtos: a tela ficaria escondida. |
| D20 | **Não há promoção de demonstração no seed.** | Os E2E usam o seed da aplicação (`AplicacaoEmExecucao`). Uma promoção semeada mudaria preços que `FechamentoTests` e outros conferem. A demonstração se faz criando uma promoção pela tela. | Semear e ajustar os E2E: acopla os testes de fluxo a um dado de demonstração. |
| D22 | **A promoção percentual aceita de 1 a 99.** Decidido na execução. | Com 100%, o desconto do item seria igual ao bruto, e o `ItemPedido` recusa isso (D14). O produto nunca sai de graça, como em D6. | Aceitar desconto igual ao bruto no `ItemPedido`: item a R$ 0,00 no pedido. |
| D21 | **Nenhum E2E novo.** | O Princípio V pede para justificar o E2E. Cálculo, escolha da melhor promoção, carrinho e fechamento são provados em unidade e integração (`PedidoService.Fechar` com repositórios mockados e o repositório com Postgres real). A tela de admin é um CRUD sem cookie, sessão ou e-mail. | E2E de compra com promoção: repetiria o caminho feliz de `FechamentoTests` mudando só o valor. |

**Fora de escopo:** cupom por código (`035`); frete grátis (`035`);
ordenação e filtro do catálogo por preço promocional (D16); limite de
estoque ou de quantidade por promoção; promoção só para clientes logados;
página "Ofertas" que lista os produtos em promoção; contagem regressiva na
vitrine; excluir promoção (D17).

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma `ProjectReference` nova. `TimeProvider` é BCL. `PromocoesVigentes` e `HorarioDeBrasilia` moram na `Application`. |
| II | Domínio rico e auto-validante | ✅ OK | `Promocao` valida tipo, valor, quantidades, período e abrangência no construtor e nos métodos de intenção. Ela também guarda a invariante de produto inativo ou fora de estoque (D9). `ItemPedido` valida o desconto entre 0 e o valor bruto da linha. **Emenda PATCH 1.7.1:** o Princípio II cita `AplicarPromocao` como exemplo de método de intenção, e esse método deixa de existir. O exemplo passa a ser `AlterarAbrangencia`. |
| III | Validação nas duas barreiras | ✅ OK | Entrada: `PromocaoDTOValidator` (as mesmas regras de tipo, valor, quantidades, período e abrangência). Invariante: construtor e métodos de `Promocao`. |
| IV | Nomenclatura em português | ✅ OK | `TipoDeDesconto`, `AbrangenciaDaPromocao`, `PromocoesVigentes`, `DescontoHelper`, `HorarioDeBrasilia`, `SituacaoDaPromocao`, `AlterarAbrangencia`. `Admin/PromocaoController` não colide com nada. |
| V | Testes escritos antes | ✅ OK | Cada passo de comportamento da seção 6 começa pelo teste vermelho. Não há E2E novo, com a justificativa em D21. |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `IPromocaoRepository` novo. `PromocaoService` grava por `SalvarAlteracoes`. Há migration `AddPromocoesDeProduto`, e o mapeamento fica em `PromocaoConfiguration`/`ItemPedidoConfiguration`. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `Admin/PromocaoController` tem `[Authorize(Roles = Papeis.Administrador)]` na classe. Os POSTs (`Cadastro`, `Edicao`, `Ativar`, `Desativar`) têm `[ValidateAntiForgeryToken]`, `ModelState` antes do efeito, `await` e redirect. |
| VIII | Tratamento de erro por camada | ✅ OK | O domínio lança `ArgumentException`/`InvalidOperationException`. O serviço lança `KeyNotFoundException` para promoção ou produto ausente. O controller não tem `try/catch`. |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/PromocaoTipo.cs` → `Enums/TipoDeDesconto.cs` | renomear | Mesmos valores (D10). Atualizar todas as referências. |
| `Enums/AbrangenciaDaPromocao.cs` | criar | `ProdutosEscolhidos = 0`, `Categoria = 1`, `Subcategoria = 2`. |
| `Helpers/DescontoHelper.cs` | criar | `Arredondar(decimal)`, `CalcularLeveXPagueY(precoUnitario, quantidade, leve, pague)` (D5, D11). |
| `Entities/Promocao.cs` | alterar | `DataInicio`/`DataFim` passam a `DateOnly`, e fim igual ao início é aceito (D7). Novos campos `QuantidadeLeve`, `QuantidadePague`, `CategoriaId`, `SubcategoriaId`, `Categoria`, `Subcategoria`, `Produtos` (campo `_produtos`) e `Abrangencia`. O construtor recebe a abrangência e exige exatamente uma forma (D3). Recusa `FreteGratis` (D2). Novos métodos `AlterarDados`, `AlterarAbrangencia`, `Alcanca` e `CalcularDesconto` (D4–D6, D11). `EstaVigente(DateOnly)`. |
| `Entities/Produto.cs` | alterar | Remove `PromocaoId`, `Promocao`, `AplicarPromocao` e `RemoverPromocao` (D9). |
| `Entities/ItemPedido.cs` | alterar | Novos campos `DescontoDaPromocao` (0 ≤ desconto < bruto) e `RotuloDaPromocao` (≤ 100). Novo método `CalcularValorDaLinha()` (D14). |
| `Entities/Pedido.cs` | alterar | `AcrescentarItem` ganha `descontoDaPromocao = 0m` e `rotuloDaPromocao = null`. |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/HorarioDeBrasilia.cs` | criar | `static DateOnly Hoje(TimeProvider)`: UTC − 3 h (D7, D8). |
| `Helpers/PromocoesVigentes.cs` | criar | `MelhorPara(produtoId, subcategoriaId, preco, quantidade)` → `DescontoAplicado?` (D4). `ParaVitrine(produtoId, subcategoriaId, preco)` → `(decimal? PrecoPromocional, IReadOnlyList<string> Selos)` (D15). `static Nenhuma`. |
| `DTOs/DescontoAplicadoDTO.cs` | criar | `record DescontoAplicadoDTO(decimal Valor, string Rotulo)`. |
| `Enums/SituacaoDaPromocao.cs` | criar | `Vigente`, `Agendada`, `Encerrada`, `Desativada`. |
| `DTOs/PromocaoDTO.cs` | criar | O DTO do formulário (seção 5). |
| `DTOs/ResumoDePromocaoDTO.cs` | criar | A linha da listagem: nome, rótulo do desconto, período, abrangência legível, situação e `Ativa`. |
| `Validators/PromocaoDTOValidator.cs` | criar | As regras de D2, D3, D5 e D7, com mensagens de campo. |
| `Mappings/PromocaoMapper.cs` | criar | `Rotulo(Promocao)` ("-10%", "-R$ 5,00", "Leve 3, pague 2", em pt-BR), `ToResumoDTO(Promocao, DateOnly hoje)` e `ToDTO(Promocao)`. |
| `Contracts/Repositories/IPromocaoRepository.cs` | criar | Assinaturas da seção 5. |
| `Contracts/Repositories/IProdutoRepository.cs` | alterar | Acrescenta `BuscarPorIds`. |
| `Contracts/Services/IPromocaoService.cs` | criar | Assinaturas da seção 5. |
| `Services/PromocaoService.cs` | criar | Recebe `IPromocaoRepository`, `IProdutoRepository`, `ISubcategoriaRepository`, `TimeProvider` e `IUnitOfWork`. Tem `CarregarVigentes` (D12) e a gestão: listar, buscar, cadastrar, atualizar, ativar e desativar (D17, D18). |
| `DTOs/ProdutoDTO.cs` | alterar | Remove `PromocaoId`. Acrescenta `PrecoPromocional` (`decimal?`) e `SelosDePromocao` (`IReadOnlyList<string>`). `ComImagem` copia os dois. |
| `DTOs/ProdutoDetalheDTO.cs` | alterar | Acrescenta `PrecoPromocional` e `SelosDePromocao`. |
| `Mappings/ProdutoMapper.cs` | alterar | `ToDTO` recebe um `PromocoesVigentes` opcional (padrão `Nenhuma`) e preenche os campos da vitrine. Remove `PromocaoId`. |
| `Mappings/ProdutoDetalheMapper.cs` | alterar | Recebe `PromocoesVigentes` e preenche os campos da vitrine. |
| `Services/CatalogoService.cs` | alterar | Injeta `IPromocaoService`. Carrega as promoções vigentes uma vez e passa ao mapper. |
| `Services/ProdutoService.cs` | alterar | Injeta `IPromocaoService`. `BuscarDestaquesDaVitrine` e `BuscarDetalhe` passam as promoções vigentes. A gestão (`ListarParaGestao`, `BuscarProdutoPorId`) segue sem promoção. |
| `Services/FavoritoService.cs` | alterar | Injeta `IPromocaoService`. A lista de favoritos passa as promoções vigentes. |
| `DTOs/LinhaDoCarrinhoDTO.cs` | alterar | Acrescenta `DescontoDaPromocao` e `RotuloDaPromocao?`. `ValorDaLinha` é líquido (D13). |
| `Mappings/CarrinhoMapper.cs` | alterar | `ToDTO`/`Montar` recebem `PromocoesVigentes` e aplicam `MelhorPara` por linha disponível. |
| `Services/CarrinhoService.cs` | alterar | Injeta `IPromocaoService`. `ObterDoUsuario` e `MontarAvulso` passam as promoções vigentes. |
| `Services/PedidoService.cs` | alterar | `Fechar`: `AcrescentarItem` com `linha.DescontoDaPromocao` e `linha.RotuloDaPromocao`. O resto não muda, porque o subtotal já é líquido. |
| `DTOs/ConfirmacaoDePedidoDTO.cs`, `DTOs/DetalheDePedidoDTO.cs` | alterar | Os registros de item ganham `DescontoDaPromocao` e `RotuloDaPromocao?`. |
| `Mappings/PedidoMapper.cs` | alterar | `ValorDaLinha` passa a vir de `CalcularValorDaLinha()` e preenche os campos novos. |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `DatabaseContext/Configurations/PromocaoConfiguration.cs` | alterar | `DataInicio`/`DataFim` como `date`. FKs `CategoriaId`/`SubcategoriaId` com `Restrict`. `HasMany(p => p.Produtos).WithMany().UsingEntity("PromocaoProduto")` com o campo `_produtos`. |
| `DatabaseContext/Configurations/ProdutoConfiguration.cs` | alterar | Remove `PromocaoId` e a relação com `Promocao`. |
| `DatabaseContext/Configurations/ItemPedidoConfiguration.cs` | alterar | `DescontoDaPromocao` `decimal(10,2)` com padrão 0 e `RotuloDaPromocao` com até 100 caracteres. |
| `Repositories/PromocaoRepository.cs` | criar | Implementa `IPromocaoRepository`. `ListarVigentes` usa `Ativa && DataInicio <= hoje && DataFim >= hoje` e inclui os ids de `Produtos`. |
| `Repositories/ProdutoRepository.cs` | alterar | `BuscarPorIds`. |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registra `IPromocaoRepository`, `IPromocaoService` e `TimeProvider.System` (singleton). |
| `Migrations/*_AddPromocoesDeProduto.cs` | criar | Seção 5. |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Areas/Admin/Controllers/PromocaoController.cs` | criar | `Index`, `Cadastro` (GET/POST), `Edicao` (GET/POST), `Ativar` (POST) e `Desativar` (POST). Monta categorias, subcategorias e produtos ativos para o formulário (D18). |
| `Models/FormularioDePromocaoViewModel.cs` | criar | Junta `PromocaoDTO` às opções de categoria, subcategoria e produto, na mesma pasta de `GestaoDeProdutosViewModel`. |
| `Areas/Admin/Views/Promocao/Index.cshtml` | criar | Tabela com nome, desconto, período (`dd/MM/yyyy`), abrangência, situação e ações, mais o botão "Nova promoção". Usa o visual da gestão de produtos. |
| `Areas/Admin/Views/Promocao/Cadastro.cshtml`, `Edicao.cshtml` | criar | As duas incluem `_FormularioPromocao`. |
| `Areas/Admin/Views/Promocao/_FormularioPromocao.cshtml` | criar | Nome e descrição. Tipo em radio, que mostra o campo de valor ou os campos "leve"/"pague". Datas de início e fim. Abrangência em radio (produtos, categoria ou subcategoria), com a caixa de cada forma. Seletor de produtos agrupado, com busca (D18). Sem JS, todos os campos ficam visíveis. |
| `wwwroot/js/pages/gestao-promocoes.js` | criar | Mostra e esconde campos por tipo e abrangência, e filtra os produtos por nome. |
| `wwwroot/css/pages/gestao-promocoes.css` | criar | Estilo das telas novas, com os tokens de `tokens.css`. |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | O submenu de admin ganha "Promoções" (D19). |
| `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | Preço riscado, preço promocional e selos (D15). |
| `wwwroot/css/components/card-produto.css` | alterar | Classes `preco-card--original`, `preco-card--promocional` e `selo-promocao`. |
| `Views/Produto/Detalhes.cshtml` + `wwwroot/css/pages/produto.css` | alterar | O mesmo tratamento de preço e selos. |
| `Views/Carrinho/_ItensDoCarrinho.cshtml` + `wwwroot/css/pages/carrinho.css` | alterar | A linha mostra o rótulo da promoção e o desconto. O valor da linha é líquido. |
| `Views/Pedido/Confirmacao.cshtml`, `Views/Pedido/Detalhe.cshtml` + `wwwroot/css/pages/pedidos.css` | alterar | O item mostra o rótulo e o desconto quando houver. |

### Testes

| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests/Units/Entities/PromocaoTests.cs` | alterar | Reescrito para a entidade nova (seção 7). Recebe os casos de produto inativo e fora de estoque que hoje estão em `ProdutoTests`. |
| `DocesCabana.Tests/Units/Entities/ProdutoTests.cs` | alterar | Remove os 4 testes de `AplicarPromocao`. |
| `DocesCabana.Tests/Units/Entities/ItemPedidoTests.cs` | alterar | Desconto: limites e `CalcularValorDaLinha`. |
| `DocesCabana.Tests/Units/Helpers/DescontoHelperTests.cs` | criar | Leve X, pague Y e arredondamento. |
| `DocesCabana.Tests/Units/Helpers/PromocoesVigentesTests.cs` | criar | Melhor promoção, abrangência e vitrine. |
| `DocesCabana.Tests/Units/Helpers/HorarioDeBrasiliaTests.cs` | criar | Virada de dia às 21h UTC−3. |
| `DocesCabana.Tests/Units/Validators/PromocaoDTOValidatorTests.cs` | criar | Um caso válido e um inválido por `RuleFor`. |
| `DocesCabana.Tests/Units/Services/PromocaoServiceTests.cs` | criar | `CarregarVigentes` com relógio controlado, e a gestão. |
| `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`, `PedidoServiceTests.cs`, `CatalogoServiceTests.cs`, `ProdutoServiceTests.cs`, `FavoritoServiceTests.cs` | alterar | Construtores com o mock de `IPromocaoService` (`Nenhuma` por padrão). Casos novos de desconto no carrinho e no pedido. |
| `DocesCabana.Tests/Units/Controllers/Admin/PromocaoControllerTests.cs` | criar | `ModelState` inválido devolve a view, sucesso redireciona e ativar/desativar redireciona. Seguir a pasta dos testes de `Admin/ProdutoController`. |
| `DocesCabana.Tests/Integration/Repositories/PromocaoRepositoryIntegrationTests.cs` | criar | Vigência por data, inclusão de produtos e persistência da relação. |
| `DocesCabana.Tests/Integration/Repositories/PedidoRepositoryIntegrationTests.cs` | alterar | Item com desconto e rótulo persiste e volta. |
| `DocesCabana.Tests/Integration/Repositories/ModeloDeDadosIntegrationTests.cs` | alterar | Ajustar se ele constrói `Promocao` ou `Produto` com promoção. |

## 5. Contratos e modelo de dados

```csharp
// Domain/Entities/Promocao.cs
public Promocao(
    string nome, TipoDeDesconto tipo, decimal valor, short? quantidadeLeve, short? quantidadePague,
    DateOnly dataInicio, DateOnly dataFim,
    Guid? categoriaId, Guid? subcategoriaId, IEnumerable<Produto>? produtos,
    string? descricao = null, Guid id = default);
public AbrangenciaDaPromocao Abrangencia { get; }
public IReadOnlyCollection<Produto> Produtos { get; }
public void AlterarDados(string nome, string? descricao, TipoDeDesconto tipo, decimal valor, short? quantidadeLeve, short? quantidadePague);
public void AlterarPeriodo(DateOnly dataInicio, DateOnly dataFim);
public void AlterarAbrangencia(Guid? categoriaId, Guid? subcategoriaId, IEnumerable<Produto>? produtos);
public bool EstaVigente(DateOnly hoje);
public bool Alcanca(Guid produtoId, Guid subcategoriaId, Guid categoriaId);
public decimal CalcularDesconto(decimal precoUnitario, short quantidade); // 0 quando não há desconto (D5, D6)

// Domain/Helpers/DescontoHelper.cs
public static decimal Arredondar(decimal valor);
public static decimal CalcularLeveXPagueY(decimal precoUnitario, short quantidade, short leve, short pague);

// Domain/Entities/ItemPedido.cs
public ItemPedido(Guid pedidoId, Guid produtoId, short quantidade, decimal precoUnitario,
    decimal descontoDaPromocao = 0m, string? rotuloDaPromocao = null, Guid id = default);
public decimal CalcularValorDaLinha();

// Domain/Entities/Pedido.cs
public void AcrescentarItem(Guid produtoId, short quantidade, decimal precoUnitario,
    decimal descontoDaPromocao = 0m, string? rotuloDaPromocao = null);

// Application/Helpers/PromocoesVigentes.cs
public sealed class PromocoesVigentes
{
    public static PromocoesVigentes Nenhuma { get; }
    public PromocoesVigentes(IEnumerable<Promocao> vigentes, IReadOnlyDictionary<Guid, Guid> categoriaPorSubcategoria);
    public DescontoAplicadoDTO? MelhorPara(Guid produtoId, Guid subcategoriaId, decimal precoUnitario, short quantidade);
    public (decimal? PrecoPromocional, IReadOnlyList<string> Selos) ParaVitrine(Guid produtoId, Guid subcategoriaId, decimal preco);
}

// Application/Helpers/HorarioDeBrasilia.cs
public static DateOnly Hoje(TimeProvider relogio);

// Application/DTOs/PromocaoDTO.cs
public class PromocaoDTO
{
    public Guid PromocaoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoDeDesconto Tipo { get; set; }
    public decimal? Valor { get; set; }
    public short? QuantidadeLeve { get; set; }
    public short? QuantidadePague { get; set; }
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public AbrangenciaDaPromocao Abrangencia { get; set; }
    public Guid? CategoriaId { get; set; }
    public Guid? SubcategoriaId { get; set; }
    public List<Guid> ProdutoIds { get; set; } = [];
}

// Application/Contracts/Repositories/IPromocaoRepository.cs
public interface IPromocaoRepository : IRepository<Promocao>
{
    Task<List<Promocao>> ListarVigentes(DateOnly hoje);
    Task<List<Promocao>> ListarParaGestao();          // inclui Categoria, Subcategoria e Produtos; mais recentes primeiro
    Task<Promocao?> BuscarComProdutos(Guid promocaoId);
}

// Application/Contracts/Repositories/IProdutoRepository.cs  (acréscimo)
Task<List<Produto>> BuscarPorIds(IEnumerable<Guid> ids);

// Application/Contracts/Services/IPromocaoService.cs
public interface IPromocaoService
{
    Task<PromocoesVigentes> CarregarVigentes();
    Task<IReadOnlyList<ResumoDePromocaoDTO>> ListarParaGestao();
    Task<PromocaoDTO> BuscarParaEdicao(Guid promocaoId);
    Task Cadastrar(PromocaoDTO dto);
    Task Atualizar(PromocaoDTO dto);
    Task Ativar(Guid promocaoId);
    Task Desativar(Guid promocaoId);
}

// MVC/Areas/Admin/Controllers/PromocaoController.cs
[Area("Admin"), Authorize(Roles = Papeis.Administrador)]
Task<IActionResult> Index();
Task<IActionResult> Cadastro();                         // GET
[HttpPost, ValidateAntiForgeryToken] Task<IActionResult> Cadastro(PromocaoDTO dto);
Task<IActionResult> Edicao(Guid id);                    // GET
[HttpPost, ValidateAntiForgeryToken] Task<IActionResult> Edicao(PromocaoDTO dto);
[HttpPost, ValidateAntiForgeryToken] Task<IActionResult> Ativar(Guid id);
[HttpPost, ValidateAntiForgeryToken] Task<IActionResult> Desativar(Guid id);
```

Os construtores de `CarrinhoService`, `CatalogoService`, `ProdutoService` e
`FavoritoService` ganham `IPromocaoService`.

**Migration:**
`dotnet ef migrations add AddPromocoesDeProduto --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`

- `Produto`: remove a FK, o índice e a coluna `PromocaoId`.
- `Promocao`: `DataInicio` e `DataFim` passam de `timestamp with time zone`
  para `date`. A tabela está vazia em todo ambiente, porque nenhum código a
  preenche nem o seed a usa. Acrescenta `QuantidadeLeve` (`smallint` nulo),
  `QuantidadePague` (`smallint` nulo), `CategoriaId` (`uuid` nulo, FK) e
  `SubcategoriaId` (`uuid` nulo, FK).
- `PromocaoProduto` (nova): `PromocaoId` e `ProdutosProdutoId`, chave
  composta, as duas FKs em cascata a partir de `Promocao`.
- `ItemPedido`: `DescontoDaPromocao` `decimal(10,2)` não nulo com padrão 0
  (os pedidos existentes ficam com 0) e `RotuloDaPromocao` `varchar(100)`
  nulo.

## 6. Ordem de execução

- [x] 1. Renomear `PromocaoTipo` → `TipoDeDesconto` em toda a base. Rodar `dotnet build` e `dotnet test` para confirmar que tudo continua verde.
- [x] 2. Testes de `DescontoHelper` (vermelhos) → implementar → verde.
- [x] 3. Reescrever `PromocaoTests` (construtor, tipos, quantidades, período, abrangência, invariante de produto, `Alcanca`, `CalcularDesconto` e `EstaVigente`) (vermelhos) → reescrever `Promocao` e criar `AbrangenciaDaPromocao` → verde.
- [x] 4. Remover `PromocaoId`, `AplicarPromocao` e `RemoverPromocao` de `Produto`, os testes correspondentes de `ProdutoTests` e `PromocaoId` de `ProdutoDTO`/`ProdutoMapper`.
- [x] 5. Testes de `ItemPedido` com desconto (vermelhos) → implementar `ItemPedido` e `Pedido.AcrescentarItem` → verde.
- [x] 6. Configurações EF (`Promocao`, `Produto`, `ItemPedido`) → gerar a migration `AddPromocoesDeProduto` → revisar o SQL gerado (a conversão de `timestamptz` para `date` e a remoção da FK do produto).
- [x] 7. Testes de integração de `PromocaoRepository` e do item com desconto em `PedidoRepository` (vermelhos) → implementar `PromocaoRepository` e `ProdutoRepository.BuscarPorIds` → verde.
- [x] 8. Testes de `HorarioDeBrasilia` e `PromocoesVigentes` (vermelhos) → implementar → verde.
- [x] 9. Testes de `PromocaoDTOValidator` (vermelhos) → implementar → verde.
- [x] 10. Testes de `PromocaoService` (vigentes com relógio mockado, cadastrar, atualizar, ativar/desativar e `KeyNotFoundException`) (vermelhos) → implementar o serviço e o mapper. Registrar a injeção de dependência e `TimeProvider.System` → verde.
- [x] 11. Testes de `CarrinhoService` e `CarrinhoMapper` com promoção (linha líquida, subtotal, maior desconto) (vermelhos) → implementar → verde.
- [x] 12. Testes de `PedidoService.Fechar` gravando o desconto no item (vermelhos) → implementar → verde.
- [x] 13. Testes de vitrine em `CatalogoService`, `ProdutoService` (destaques e detalhe) e `FavoritoService` (vermelhos) → implementar os DTOs e mappers → verde.
- [x] 14. Testes de `Admin/PromocaoController` (vermelhos) → controller, view model, views, JS e CSS → verde.
- [x] 15. Item "Promoções" no menu de admin.
- [x] 16. Vitrine: cartão, página do produto, carrinho, confirmação e detalhe do pedido (views e CSS).
- [x] 17. Emenda PATCH 1.7.1 da constituição (exemplo do Princípio II) e linha da `034` no `specs/README.md`.
- [ ] 18. `dotnet build` e `dotnet test` completos (unidade, integração e E2E).
- [ ] 19. Verificação manual da seção 7.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| "Leve X, pague Y" por produto | `Dado_Leve3Pague2Com7Unidades_Quando_CalcularLeveXPagueY_Entao_DeveDescontar2Unidades` + theory (2 → 0, 3 → 1, 6 → 2) |
| Arredondamento em 2 casas | `Dado_ValorComTerceiraCasa5_Quando_Arredondar_Entao_DeveArredondarParaCima` |
| Tipos aceitos e recusa de frete grátis | `Dado_TipoFreteGratis_Quando_CriarPromocao_Entao_DeveLancarArgumentException` |
| Percentual entre 1 e 100 e valor fixo positivo | theories de valor inválido por tipo (já existem e são adaptadas) |
| Quantidades de "leve X, pague Y" | `Dado_PagueMaiorOuIgualALeve_Quando_CriarPromocao_Entao_DeveLancarArgumentException` / `Dado_LeveMenorQue2_…` / `Dado_LeveSemQuantidades_…` |
| Período de um dia é aceito e fim antes do início é recusado | `Dado_InicioIgualAoFim_Quando_CriarPromocao_Entao_DeveCriar` / `Dado_FimAntesDoInicio_…_DeveLancarArgumentException` |
| Exatamente uma forma de abrangência | `Dado_NenhumaAbrangencia_…` / `Dado_CategoriaESubcategoria_…` / `Dado_ListaDeProdutosVazia_…` → `ArgumentException` |
| Produto inativo ou fora de estoque não entra | `Dado_ProdutoInativo_Quando_CriarPromocaoPorProdutos_Entao_DeveLancarInvalidOperationException` / `Dado_ProdutoForaDeEstoque_…` (vieram de `ProdutoTests`) |
| `Alcanca` por produto, categoria e subcategoria | theory com as três abrangências, um caso que alcança e um que não alcança |
| Valor fixo não alcança produto barato | `Dado_ValorFixoMaiorOuIgualAoPreco_Quando_CalcularDesconto_Entao_DeveSerZero` |
| Percentual e valor fixo por unidade | `Dado_Percentual10Com3Unidades_Quando_CalcularDesconto_Entao_DeveDescontar10PorCentoDaLinha` / `Dado_ValorFixo2Com3Unidades_…_DeveDescontar6` |
| Vigência inclui o primeiro e o último dia e respeita `Ativa` | theory em `EstaVigente` (véspera, início, fim, dia seguinte, desativada) |
| Desconto do item entre 0 e o bruto | `Dado_DescontoNegativo_…` / `Dado_DescontoIgualAoBruto_…` → `ArgumentException`; `Dado_ItemComDesconto_Quando_CalcularValorDaLinha_Entao_DeveSubtrairDesconto` |
| "Hoje" muda à meia-noite de Brasília | `Dado_UtcAs2h59_Quando_Hoje_Entao_DeveSerDiaAnterior` / `Dado_UtcAs3h00_…_DeveSerMesmoDia` |
| O maior desconto vence pela quantidade real | `Dado_Percentual10ELeve3Pague2Com1Unidade_Quando_MelhorPara_Entao_DeveSerPercentual` / `…Com3Unidades_…_DeveSerLeve3Pague2` |
| A categoria é resolvida pelo mapa de subcategoria | `Dado_PromocaoPorCategoria_Quando_MelhorParaProdutoDeSubcategoriaDaCategoria_Entao_DeveAplicar` |
| Vitrine com riscado e selos | `Dado_PercentualELeve_Quando_ParaVitrine_Entao_DeveTerPrecoPromocionalEDoisSelos` / `Dado_SoLeve_…_DeveTerSeloSemPrecoPromocional` / `Dado_SemPromocao_…_DeveSerNulo` |
| O validator cobre cada regra | `PromocaoDTOValidatorTests`: um caso válido e um inválido por `RuleFor` |
| Só promoções vigentes entram, com relógio controlado | `Dado_PromocaoEncerradaOntem_Quando_CarregarVigentes_Entao_NaoDeveAplicar` (`Mock<TimeProvider>`) |
| Cadastro e edição gravam, e produto ausente é recusado | `Dado_PromocaoValida_Quando_Cadastrar_Entao_DeveAdicionarESalvar` / `Dado_ProdutoIdInexistente_Quando_Cadastrar_Entao_DeveLancarKeyNotFoundException` / `Dado_PromocaoInexistente_Quando_Atualizar_…` |
| O carrinho sai líquido | `Dado_ProdutoEmPromocao_Quando_ObterDoUsuario_Entao_LinhaESubtotalDevemSerLiquidos` e o mesmo para `MontarAvulso` |
| O pedido guarda o desconto do item | `Dado_CarrinhoComPromocao_Quando_Fechar_Entao_ItemDeveTerDescontoERotulo` / `…_Entao_ValorDoPedidoDeveSerLiquidoMaisFrete` |
| Promoção que muda entre a revisão e o fechamento é barrada | `Dado_SubtotalExibidoSemPromocaoQueComecou_Quando_Fechar_Entao_DeveRecusarComValorAtual` |
| Catálogo, destaques, detalhe e favoritos mostram a promoção | um caso por serviço: `…_Entao_DevePreencherPrecoPromocional` |
| Persistência da relação e da vigência | integração: `Dado_PromocaoPorProdutos_Quando_Salvar_Entao_DeveVoltarComProdutos` / `Dado_PromocoesEmVariosPeriodos_Quando_ListarVigentes_Entao_DeveTrazerSoAsDeHoje` / `Dado_ItemComDesconto_Quando_Salvar_Entao_DeveVoltarComDescontoERotulo` |
| A borda admin segue PRG e validação | `Dado_ModelStateInvalido_Quando_Cadastro_Entao_DeveRetornarViewComOpcoes` / `Dado_PromocaoValida_Quando_Cadastro_Entao_DeveRedirecionarParaIndex` / `Dado_Id_Quando_Desativar_Entao_DeveChamarServicoERedirecionar` |
| Telas e vitrine de ponta a ponta | Verificação manual. Como admin, criar três promoções: 10% na subcategoria Brigadeiros, "leve 3, pague 2" num produto escolhido e R$ 50,00 numa categoria. Conferir a listagem e a situação. No catálogo, conferir o riscado e os selos, e que o produto barato não recebe os R$ 50,00. No carrinho, trocar de 1 para 3 unidades e ver a promoção trocar. Fechar o pedido e conferir a confirmação e "Meus pedidos". Desativar a promoção e ver que o pedido continua igual. Conferir também o formulário sem JS e a busca de produtos com JS. |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| A ordenação "menor/maior preço" do catálogo usa o preço cheio (D16). | Médio: um produto em promoção pode aparecer fora da ordem que o preço exibido sugere. | Registrado como limitação. A correção (preço efetivo em SQL ou coluna calculada) fica para quando houver pedido. |
| A migration muda o tipo de `DataInicio`/`DataFim` e remove `Produto.PromocaoId`. | Alto se houver dado: perda das colunas. | A tabela `Promocao` está vazia e a coluna `PromocaoId` é nula em todo produto, porque nenhum código as preenche. Conferir no Supabase com `select count(*) from "Promocao"` e `select count(*) from "Produto" where "PromocaoId" is not null` antes de aplicar. |
| Quatro serviços ganham uma dependência nova no construtor. | Médio: muitos testes de unidade existentes precisam do mock novo. | Passo 1 de cada teste: um campo `Mock<IPromocaoService>` que devolve `PromocoesVigentes.Nenhuma`, criado no construtor da classe de teste. |
| Os E2E conferem preços do seed. | Baixo: nenhuma promoção é semeada (D20), então os preços não mudam. | O passo 18 roda a suíte E2E completa. |
| Emenda constitucional PATCH 1.7.1. | Baixo: troca o exemplo `AplicarPromocao` do Princípio II por `AlterarAbrangencia`. | Registrar no histórico de emendas no passo 17. |
| Valor fixo maior que o preço deixa o produto fora da promoção sem aviso (D6). | Baixo: o admin pode não entender por que um produto da categoria não ganhou desconto. | O formulário explica a regra ao lado do campo de valor fixo. |

## 9. Resultado da execução

*Preenchido na execução.*
