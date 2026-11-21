# Plano — Cupons de desconto

**ID:** 035-cupons-de-desconto · **Criado em:** 2026-11-18
**Status:** Executado · **Depende de:** `034-promocoes-de-produto` executada

---

## 1. Objetivo

O resumo do carrinho tem, desde a `021`, um campo "Cupom de desconto"
desabilitado, com a mensagem "ainda não está disponível". Esta feature liga
esse campo. O cliente logado digita um código e vê o desconto no resumo, em
todos os passos do fechamento, na confirmação e no detalhe do pedido. O
administrador ganha uma tela para criar cupons: percentual, valor fixo,
"leve X, pague Y" ou frete grátis. Cada cupom pode ter valor mínimo, uso
único por cliente e restrição a uma categoria ou subcategoria.

É a segunda das duas entregas de descontos. Ela se apoia no que a `034`
construiu: o enum `TipoDeDesconto`, o `DescontoHelper`, a data por
`HorarioDeBrasilia` e o carrinho com preço líquido de promoção.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | **O cupom é uma entidade própria (`Cupom`), separada de `Promocao`.** | Decisão do debate ("os dois" conceitos). As regras são diferentes: o cupom tem código, valor mínimo, uso por cliente e frete grátis, e a promoção é automática e por produto. | `Promocao` com código opcional: metade dos campos ficaria sem sentido para cada uso, e as invariantes viveriam em `if`s por modo. |
| D2 | **O cupom aceita os quatro tipos de `TipoDeDesconto`.** "Leve X, pague Y" é contado por produto, como na `034` (D5 de lá): a cada X unidades do mesmo produto elegível, X − Y saem de graça. | Decisão do debate. A mesma contagem nas duas features evita duas regras para o mesmo nome. | Contagem misturando produtos no cupom: o cliente veria "leve 3, pague 2" se comportar de dois jeitos. |
| D3 | **As restrições que o admin configura:** valor mínimo (opcional), uso único por cliente (sim ou não) e restrição a **uma** categoria **ou** uma subcategoria (opcional). O período é por data, como na `034`, e o cupom pode ser ativado e desativado. **Não há limite total de usos.** | Decisão do debate. O limite total não foi escolhido. | Limite total de usos: exigiria controle de concorrência entre pedidos simultâneos, sem pedido do negócio. |
| D4 | **O cupom acumula com a promoção de produto:** ele incide sobre o valor líquido das linhas, depois da promoção. | Decisão do debate. | O cupom ignorar os itens em promoção, ou valer o maior: os dois foram recusados no debate. |
| D5 | **O valor mínimo é conferido sobre os itens elegíveis.** Sem restrição de categoria, todos os itens disponíveis são elegíveis. Com restrição, só os da categoria ou subcategoria contam. O frete nunca conta. | Decisão do debate. | Subtotal do carrinho inteiro: recusado no debate. |
| D6 | **O cálculo:** `Percentual` incide sobre a base elegível. `ValorFixo` é um valor do pedido, limitado à base elegível. "Leve X, pague Y" vale por linha elegível, e cada unidade grátis vale o valor líquido da linha dividido pela quantidade. `FreteGratis` zera **qualquer** opção de entrega escolhida, e o desconto no frete é o preço da opção. Tudo é arredondado por `DescontoHelper.Arredondar`. | Decisão do debate para o frete. No cupom, o valor fixo é do pedido, e não por unidade como na promoção (D6 da `034`): "R$ 15 de desconto" num cupom é lido como R$ 15 no total. O limite pela base impede que os produtos fiquem negativos. | Valor fixo por unidade também no cupom: um cupom de R$ 15 com 10 itens daria R$ 150. |
| D7 | **Um cupom por pedido.** | É o comportamento esperado de loja virtual, e o debate não pediu acúmulo de cupons. | Vários cupons: exigiria uma regra de ordem de aplicação entre cupons. |
| D8 | **Só cliente logado aplica cupom.** O visitante vê o campo desabilitado com "Entre na sua conta para usar um cupom." e um botão "Entrar", que abre o `_ModalLogin`. | Decisão do debate. Assim, o uso por cliente é sempre conferido no momento em que o cupom é aplicado. | Visitante aplica e a conferência fica para o login: recusado no debate. |
| D9 | **O código aplicado fica na sessão (`CupomDaSessao`, como `CepDaSessao`) e é reavaliado a cada exibição do carrinho, a cada passo do fechamento e no fechamento.** | O carrinho muda (quantidade, remoção, promoção que acaba), e o cupom precisa refletir o estado atual. A sessão já guarda o CEP da estimativa. | Gravar no banco: coluna nova para um dado de curta duração. Calcular só no fechamento: o cliente não veria o desconto antes de pagar. |
| D10 | **Um cupom aplicado que deixa de valer continua na sessão, com o motivo no resumo** ("Este cupom vale para compras a partir de R$ 80,00 em Doces."). Ele não dá desconto e não é enviado ao fechamento. O cliente pode removê-lo ou ajustar o carrinho. | O cliente que tirou um item entende o que perdeu e pode voltar o item. | Remover o cupom em silêncio: o cliente perderia o desconto sem saber por quê. |
| D11 | **Mensagens de recusa:** "Cupom não encontrado." (inexistente ou desativado), "Este cupom ainda não está valendo.", "Este cupom expirou.", "Você já usou este cupom.", "Este cupom vale só para itens de {categoria}.", "Este cupom vale para compras a partir de R$ {mínimo}[ em {categoria}]." e "Leve {X} unidades de um mesmo produto para usar este cupom.". | Cada motivo diz ao cliente o que fazer. O código de cupom não é segredo de conta, então distinguir os motivos não abre enumeração de conta (Princípio VII). | Uma mensagem só ("Cupom inválido"): o cliente não saberia se adiciona um item ou desiste. |
| D12 | **O uso único conta os pedidos do cliente com aquele cupom, exceto os `Cancelado`.** | Um pedido cancelado não usou o desconto. | Contar todos: o cliente perderia o cupom por um pedido que não aconteceu. |
| D13 | **`PedidoService.Fechar` reavalia o cupom.** Se ele não vale mais, recusa com o motivo, e o controller tira o cupom da sessão. Se o desconto difere do exibido, recusa com "O desconto do cupom mudou desde a última vez que você revisou. Confira o valor atual.". O formulário de pagamento envia `CodigoDoCupom` e `DescontoDoCupomExibido` (produtos + frete). | É o mesmo padrão de conferência que o fechamento já usa para o subtotal e o frete (`022`). | Confiar no valor exibido: abriria brecha para POST adulterado. |
| D14 | **O pedido guarda o cupom:** `CupomId` (FK `Restrict`, nula), `CodigoDoCupom` (cópia), `DescontoDoCupom` (nos produtos) e `DescontoNoFrete`, gravados por `Pedido.AplicarCupom`. O valor dos produtos do pedido passa a ser derivado dos itens (`ValorDosProdutos()`), e não mais de `Valor − ValorDoFrete`. | Os descontos ficam separados para a confirmação e o detalhe mostrarem cada linha. A FK permite contar os usos (D12, D17). A cópia do código mantém o histórico legível. | Guardar só o total líquido: o detalhe não saberia mostrar "Cupom BEMVINDO10 −R$ 8,00". |
| D15 | **Código do cupom:** de 3 a 20 letras ou números, sem espaço, gravado em maiúsculas e comparado sem diferenciar maiúsculas. É único, com índice no banco e conferência no serviço. **Não muda depois de criado:** na edição, ele aparece só como texto. | Quem divulgou um código não pode ver o código mudar. O pedido já guarda a cópia, mas o cliente que recebeu o código antigo ficaria sem cupom. | Código editável: a divulgação já feita deixaria de funcionar. |
| D16 | **A unicidade do código segue o padrão da `006` (`ContaJaExiste`).** O controller chama `ICupomService.CodigoJaExiste` e, se ele existir, faz `ModelState.AddModelError("Codigo", "Já existe um cupom com este código.")`. `Cadastrar` lança `InvalidOperationException` se o código existir (a barreira do dado), e o índice único é a última barreira. | O FluentValidation da base não consulta repositório. A `006` fixou esse padrão de duplicidade. | Validator assíncrono com repositório: seria um padrão novo na base. |
| D17 | **A tela administrativa fica em `Admin/Cupom`, com o mesmo desenho de `Admin/Promocao` (`034`, D17).** A listagem tem código, desconto, período, restrições, número de usos (pedidos não cancelados) e situação. Cadastro e edição ficam em página própria, e ativar/desativar é por POST. Não há exclusão. O menu "Administração" ganha "Cupons". | Mesma razão da `034`. Os usos mostram ao admin se o cupom está funcionando. | Excluir cupom usado: quebraria a FK do pedido. |
| D18 | **Um cupom de demonstração é semeado: `BEMVINDO10`, 10%, sem mínimo, uso único por cliente e válido por um ano a partir da data do seed.** | Ao contrário de uma promoção (D20 da `034`), um cupom só muda preço quando digitado, então os E2E existentes não são afetados. O E2E de D19 precisa de um código conhecido. | Criar pela tela de admin dentro do E2E: o teste ficaria mais longo e passaria a provar a tela de admin também. |
| D19 | **Um E2E novo: aplicar `BEMVINDO10` no carrinho, ver o desconto no resumo, ir para o passo de Pagamento e ver o desconto mantido.** | O Princípio V pede para dizer o que o E2E prova além das outras camadas: o código guardado na **sessão** sobrevive entre requisições e passos, e o formulário de pagamento envia os campos ocultos do cupom. Unidade e integração não alcançam a sessão nem o pipeline de views. | E2E até o fechamento: depende da cotação de frete real, que a `022` registrou como pendência de credencial. |
| D20 | **Erro de formato no código** (vazio ou mais de 20 caracteres) **e recusa do cupom voltam para a tela do carrinho com a mensagem no campo.** A ação reaproveita o montador do `Index` e faz `return View("Index", modelo)`. Cupom aceito redireciona (PRG). | Princípios III e VII: a mensagem fica no campo e o sucesso redireciona. | `TempData` com redirect também no erro: a mensagem sairia do `ModelState`. |
| D21 | **A subcategoria entra na linha do carrinho:** `LinhaDoCarrinhoDTO.SubcategoriaId`. O mapa subcategoria → categoria é o mesmo que `PromocoesVigentes` já carrega. | O cupom restrito precisa saber a categoria de cada linha. | Recarregar os produtos no `CupomService`: consulta repetida para um dado que o carrinho já tem. |

**Fora de escopo:** limite total de usos (D3); vários cupons no mesmo pedido
(D7); cupom para visitante (D8); cupom personalizado por cliente ou enviado
por e-mail; cupom de primeira compra; relatório de faturamento por cupom
além da contagem de usos; excluir cupom (D17).

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma `ProjectReference` nova. A sessão só aparece no MVC (`CupomDaSessao`). A `Application` recebe o código como `string`. |
| II | Domínio rico e auto-validante | ✅ OK | `Cupom` valida código, tipo e valor, quantidades, período, mínimo e restrição (no máximo uma). Ele também calcula o desconto e confere o mínimo. `Pedido.AplicarCupom` valida que os descontos não passam do valor dos produtos e do frete. |
| III | Validação nas duas barreiras | ✅ OK | Entrada: `CupomDTOValidator` e `AplicacaoDeCupomDTOValidator`. Invariante: construtor e métodos de `Cupom`. A unicidade do código tem as três barreiras de D16. |
| IV | Nomenclatura em português | ✅ OK | `Cupom`, `AplicarCupom`, `CupomDaSessao`, `ResultadoDoCupomDTO`, `CodigoJaExiste`, `ClienteJaUsouCupom`. `Admin/CupomController` não colide com nada. |
| V | Testes escritos antes | ✅ OK | Cada passo de comportamento da seção 6 começa pelo teste vermelho. Há um E2E, com a justificativa em D19. |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `ICupomRepository` novo, consultas novas em `IPedidoRepository` e `SalvarAlteracoes` no serviço. Há migration `AddCupons`. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `AplicarCupom` e `RemoverCupom` têm `[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Authorize]`, `ModelState` antes do efeito e redirect no sucesso (D20). `Admin/CupomController` segue `Admin/PromocaoController`. |
| VIII | Tratamento de erro por camada | ✅ OK | A recusa de cupom é erro esperado do usuário. Ela vira `ResultadoDoCupomDTO` com motivo e depois `ModelState`, nunca exceção. O domínio lança só para dado inválido. A ação não tem `try/catch`. |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/DescontoHelper.cs` | alterar | Recebe `ValidarTipoEValor(tipo, valor, leve, pague)`, extraído de `Promocao` para ser usado também por `Cupom`. |
| `Entities/Promocao.cs` | alterar | Passa a usar `DescontoHelper.ValidarTipoEValor`, sem mudar o comportamento. |
| `Entities/Cupom.cs` | criar | Seção 5. |
| `Entities/Pedido.cs` | alterar | Novos campos `CupomId`, `Cupom`, `CodigoDoCupom`, `DescontoDoCupom` e `DescontoNoFrete`. Novos métodos `AplicarCupom(...)` e `ValorDosProdutos()` (Σ linhas líquidas − desconto do cupom). |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `DTOs/CupomDTO.cs`, `DTOs/ResumoDeCupomDTO.cs` | criar | O formulário e a linha da listagem do admin. |
| `DTOs/AplicacaoDeCupomDTO.cs` | criar | `Codigo`. |
| `DTOs/ResultadoDoCupomDTO.cs` | criar | Seção 5. |
| `Validators/CupomDTOValidator.cs`, `Validators/AplicacaoDeCupomDTOValidator.cs` | criar | Formato do código, tipo e valor, quantidades, período, mínimo positivo e no máximo uma restrição. |
| `Mappings/CupomMapper.cs` | criar | `Rotulo(Cupom)` ("10%", "R$ 15,00", "Leve 3, pague 2", "Frete grátis"), `ToResumoDTO` e `ToDTO`. |
| `Contracts/Repositories/ICupomRepository.cs` | criar | Seção 5. |
| `Contracts/Repositories/IPedidoRepository.cs` | alterar | Acrescenta `ClienteJaUsouCupom` e `ContarUsosPorCupom`. |
| `Contracts/Services/ICupomService.cs`, `Services/CupomService.cs` | criar | `Avaliar` (D5, D6, D10–D12) e a gestão. Recebe `ICupomRepository`, `IPedidoRepository`, `ISubcategoriaRepository`, `TimeProvider` e `IUnitOfWork`. |
| `DTOs/LinhaDoCarrinhoDTO.cs`, `Mappings/CarrinhoMapper.cs` | alterar | `SubcategoriaId` (D21). |
| `DTOs/PassoDoFechamentoDTO.cs` | alterar | `ResultadoDoCupomDTO? Cupom`, `DescontoTotalDoCupom` e o total líquido. |
| `Services/PedidoService.cs` | alterar | Injeta `ICupomService`. `MontarPasso` ganha `codigoDoCupom` e avalia o cupom com o frete de referência: no passo Carrinho, a cotação mais barata do CEP; nos passos Endereço e Pagamento, a opção escolhida. `Fechar` segue D13 e chama `pedido.AplicarCupom`. |
| `DTOs/FechamentoDePedidoDTO.cs` | alterar | `CodigoDoCupom?` e `DescontoDoCupomExibido`. |
| `DTOs/ResultadoDoFechamentoDTO.cs` | alterar | `CupomRecusado` e `DescontoDoCupomAtual?`. |
| `DTOs/ConfirmacaoDePedidoDTO.cs`, `DTOs/DetalheDePedidoDTO.cs`, `Mappings/PedidoMapper.cs` | alterar | `CodigoDoCupom`, `DescontoDoCupom` e `DescontoNoFrete`. `ValorDosProdutos` passa a vir de `pedido.ValorDosProdutos()`, e `ValorTotal` desconta o cupom. |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `DatabaseContext/Configurations/CupomConfiguration.cs` | criar | `Codigo` com até 20 caracteres e índice único. `Valor` e `ValorMinimo` `decimal(10,2)`. Datas como `date`. FKs de categoria e subcategoria com `Restrict`. |
| `DatabaseContext/Configurations/PedidoConfiguration.cs` | alterar | FK `CupomId` com `Restrict`, `CodigoDoCupom` com até 20 caracteres, e os dois descontos `decimal(10,2)` com padrão 0. |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | `DbSet<Cupom> Cupons`. |
| `Repositories/CupomRepository.cs` | criar | Implementa `ICupomRepository`. |
| `Repositories/PedidoRepository.cs` | alterar | `ClienteJaUsouCupom` (status diferente de `Cancelado`) e `ContarUsosPorCupom`. |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registra `ICupomRepository` e `ICupomService`. |
| `DatabaseContext/Seed/DbInitializer.cs` | alterar | Semeia `BEMVINDO10` (D18) se ele ainda não existir. |
| `Migrations/*_AddCupons.cs` | criar | Seção 5. |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/CupomDaSessao.cs` | criar | `LerCupom`, `EscreverCupom` e `LimparCupom` sobre `ISession` (D9). |
| `Controllers/CarrinhoController.cs` | alterar | Novas ações `AplicarCupom(AplicacaoDeCupomDTO)` e `RemoverCupom()` (D20). O montador do `Index` passa o código da sessão a `MontarPasso`. `Esvaziar` também limpa o cupom. |
| `Controllers/PedidoController.cs` | alterar | `Fechar`: limpa o cupom da sessão no sucesso e quando `CupomRecusado`. |
| `Views/Carrinho/_ItensDoCarrinho.cshtml` | alterar | O campo de cupom passa a funcionar: visitante (D8), formulário, cupom aplicado com rótulo e "Remover", e motivo quando não vale (D10). Linha "Cupom {código} −R$" e frete "Grátis" com o valor original riscado. Os totais (hoje nas linhas 143–146 e 198) descontam o cupom. |
| `Views/Carrinho/_PassoPagamento.cshtml` | alterar | Campos ocultos `CodigoDoCupom` e `DescontoDoCupomExibido` (só se o cupom vale). Linha do cupom e total líquido (hoje nas linhas 91–92). |
| `Views/Pedido/Confirmacao.cshtml`, `Views/Pedido/Detalhe.cshtml` | alterar | Linha do cupom e frete grátis. |
| `wwwroot/css/pages/carrinho.css`, `wwwroot/css/pages/pedidos.css` | alterar | Estado do cupom aplicado e recusado, e a linha de desconto. Remove o estilo de "em breve". |
| `Areas/Admin/Controllers/CupomController.cs` | criar | Mesmo desenho de `Admin/PromocaoController`, com a unicidade de D16. |
| `Models/FormularioDeCupomViewModel.cs` | criar | Junta `CupomDTO` às opções de categoria e subcategoria. |
| `Areas/Admin/Views/Cupom/Index.cshtml`, `Cadastro.cshtml`, `Edicao.cshtml`, `_FormularioCupom.cshtml` | criar | D15 e D17. A edição mostra o código como texto. |
| `wwwroot/js/pages/gestao-cupons.js`, `wwwroot/css/pages/gestao-cupons.css` | criar | Mostra e esconde campos por tipo e restrição. O CSS reaproveita o visual de `gestao-promocoes.css`. |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Item "Cupons" no menu de admin. |

### Testes

| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests/Units/Entities/CupomTests.cs` | criar | Seção 7. |
| `DocesCabana.Tests/Units/Entities/PedidoTests.cs` | alterar | `AplicarCupom` e `ValorDosProdutos`. |
| `DocesCabana.Tests/Units/Validators/CupomDTOValidatorTests.cs`, `AplicacaoDeCupomDTOValidatorTests.cs` | criar | Um caso válido e um inválido por `RuleFor`. |
| `DocesCabana.Tests/Units/Services/CupomServiceTests.cs` | criar | Cada motivo de D11, cada tipo de D6, acúmulo (D4), mínimo (D5) e uso (D12). |
| `DocesCabana.Tests/Units/Services/PedidoServiceTests.cs` | alterar | `MontarPasso` com cupom e `Fechar` (D13, D14). |
| `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`, `PedidoControllerTests.cs` | alterar | `AplicarCupom`, `RemoverCupom` e a limpeza da sessão no `Fechar`. |
| `DocesCabana.Tests/Units/Controllers/Admin/CupomControllerTests.cs` | criar | PRG, `ModelState` e código duplicado. |
| `DocesCabana.Tests/Integration/Repositories/CupomRepositoryIntegrationTests.cs` | criar | Busca por código sem diferenciar maiúsculas e índice único. |
| `DocesCabana.Tests/Integration/Repositories/PedidoRepositoryIntegrationTests.cs` | alterar | `ClienteJaUsouCupom` (cancelado não conta), `ContarUsosPorCupom` e pedido com cupom persistido. |
| `DocesCabana.Tests.E2E/Fluxos/CupomTests.cs` + objeto de página do carrinho | criar/alterar | D19. |

## 5. Contratos e modelo de dados

```csharp
// Domain/Entities/Cupom.cs
public Cupom(
    string codigo, TipoDeDesconto tipo, decimal valor, short? quantidadeLeve, short? quantidadePague,
    DateOnly dataInicio, DateOnly dataFim, decimal? valorMinimo, bool umUsoPorCliente,
    Guid? categoriaId, Guid? subcategoriaId, string? descricao = null, Guid id = default);
public static string NormalizarCodigo(string codigo);           // Trim + ToUpperInvariant
public void AlterarDados(TipoDeDesconto tipo, decimal valor, short? quantidadeLeve, short? quantidadePague, string? descricao);
public void AlterarPeriodo(DateOnly dataInicio, DateOnly dataFim);
public void AlterarRestricoes(decimal? valorMinimo, bool umUsoPorCliente, Guid? categoriaId, Guid? subcategoriaId);
public void Ativar(); public void Desativar();
public bool EstaVigente(DateOnly hoje);
public bool Alcanca(Guid subcategoriaId, Guid categoriaId);
public bool AtingeMinimo(decimal baseElegivel);
public decimal CalcularDescontoNosProdutos(IReadOnlyList<(decimal ValorLiquido, short Quantidade)> linhasElegiveis);
public decimal CalcularDescontoNoFrete(decimal valorDoFrete);   // só FreteGratis; senão 0

// Domain/Entities/Pedido.cs  (acréscimos)
public void AplicarCupom(Guid cupomId, string codigo, decimal descontoNosProdutos, decimal descontoNoFrete);
public decimal ValorDosProdutos();

// Application/DTOs/ResultadoDoCupomDTO.cs
public class ResultadoDoCupomDTO
{
    public string Codigo { get; init; } = string.Empty;
    public Guid? CupomId { get; init; }
    public bool Valido { get; init; }
    public string? Motivo { get; init; }
    public string Rotulo { get; init; } = string.Empty;
    public bool FreteGratis { get; init; }
    public decimal DescontoNosProdutos { get; init; }
    public decimal DescontoNoFrete { get; init; }
    public decimal DescontoTotal => DescontoNosProdutos + DescontoNoFrete;
}

// Application/Contracts/Repositories/ICupomRepository.cs
public interface ICupomRepository : IRepository<Cupom>
{
    Task<Cupom?> BuscarPorCodigo(string codigoNormalizado);     // inclui Categoria e Subcategoria
    Task<bool> ExisteCodigo(string codigoNormalizado);
    Task<List<Cupom>> ListarParaGestao();
}

// Application/Contracts/Repositories/IPedidoRepository.cs  (acréscimos)
Task<bool> ClienteJaUsouCupom(Guid usuarioId, Guid cupomId);
Task<Dictionary<Guid, int>> ContarUsosPorCupom();

// Application/Contracts/Services/ICupomService.cs
public interface ICupomService
{
    Task<ResultadoDoCupomDTO> Avaliar(string codigo, Guid usuarioId, CarrinhoDTO carrinho, decimal? valorDoFrete);
    Task<bool> CodigoJaExiste(string codigo);
    Task<IReadOnlyList<ResumoDeCupomDTO>> ListarParaGestao();
    Task<CupomDTO> BuscarParaEdicao(Guid cupomId);
    Task Cadastrar(CupomDTO dto);
    Task Atualizar(CupomDTO dto);
    Task Ativar(Guid cupomId);
    Task Desativar(Guid cupomId);
}

// Application/Contracts/Services/IPedidoService.cs  (alteração)
Task<PassoDoFechamentoDTO> MontarPasso(
    PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId,
    int? servicoDeEntregaId = null, string? codigoDoCupom = null);

// MVC/Controllers/CarrinhoController.cs
[HttpPost, ValidateAntiForgeryToken, Authorize] Task<IActionResult> AplicarCupom(AplicacaoDeCupomDTO dto);
[HttpPost, ValidateAntiForgeryToken, Authorize] IActionResult RemoverCupom();

// MVC/Helpers/CupomDaSessao.cs
public static string? LerCupom(this ISession sessao);
public static void EscreverCupom(this ISession sessao, string codigo);
public static void LimparCupom(this ISession sessao);
```

`Admin/CupomController` tem as mesmas assinaturas de `Admin/PromocaoController`
(`034`, seção 5), com `CupomDTO`.

**Migration:**
`dotnet ef migrations add AddCupons --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`

- `Cupom` (nova): `CupomId`, `Codigo` (`varchar(20)`, índice único),
  `Descricao` (`varchar(255)` nulo), `Tipo`, `Valor` (`decimal(10,2)`),
  `QuantidadeLeve` e `QuantidadePague` (`smallint` nulo), `DataInicio` e
  `DataFim` (`date`), `ValorMinimo` (`decimal(10,2)` nulo),
  `UmUsoPorCliente`, `Ativo`, `CategoriaId` e `SubcategoriaId` (`uuid` nulo,
  FK `Restrict`).
- `Pedido`: `CupomId` (`uuid` nulo, FK `Restrict`, com índice),
  `CodigoDoCupom` (`varchar(20)` nulo), `DescontoDoCupom` e
  `DescontoNoFrete` (`decimal(10,2)`, padrão 0).

## 6. Ordem de execução

- [x] 1. Extrair `DescontoHelper.ValidarTipoEValor` de `Promocao`. Os `PromocaoTests` da `034` precisam continuar verdes sem edição.
- [x] 2. `CupomTests` (código, tipos, período, mínimo, restrição, `Alcanca`, `AtingeMinimo`, cálculos de D6 e `EstaVigente`) (vermelhos) → criar `Cupom` → verde.
- [x] 3. `PedidoTests` de `AplicarCupom` e `ValorDosProdutos` (vermelhos) → implementar → verde.
- [x] 4. `CupomConfiguration`, `PedidoConfiguration` e `DbSet` → gerar a migration `AddCupons` → revisar o SQL.
- [x] 5. Integração de `CupomRepository` e das consultas novas de `PedidoRepository` (vermelhos) → implementar → verde.
- [x] 6. Validators (vermelhos) → implementar → verde.
- [x] 7. `CupomServiceTests` de `Avaliar` (cada motivo, cada tipo, acúmulo, mínimo e uso) e da gestão (vermelhos) → `LinhaDoCarrinhoDTO.SubcategoriaId`, `CupomService`, `CupomMapper` e injeção de dependência → verde.
- [x] 8. `PedidoServiceTests` de `MontarPasso` com cupom e de `Fechar` com cupom válido, recusado e com desconto divergente (vermelhos) → implementar → verde.
- [x] 9. Controller tests de `AplicarCupom`, `RemoverCupom` e da limpeza no `Fechar` (vermelhos) → `CupomDaSessao` e as ações → verde.
- [x] 10. Views do carrinho, do pagamento, da confirmação e do detalhe, e CSS.
- [x] 11. Controller tests de `Admin/CupomController` (vermelhos) → controller, view model, views, JS, CSS e item do menu → verde.
- [x] 12. Escrever o E2E `CupomTests` e vê-lo falhar sem o seed → semear `BEMVINDO10` → verde.
- [x] 13. Linha da `035` no `specs/README.md` atualizada. A pergunta aberta sobre cupom sai de "Perguntas ainda abertas".
- [x] 14. `dotnet build` e `dotnet test` completos (unidade, integração e E2E).
- [ ] 15. Verificação manual da seção 7.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Código normalizado e com formato válido | `Dado_CodigoComEspacosEMinusculas_Quando_CriarCupom_Entao_DeveGuardarMaiusculoSemEspacos` / theory de formato inválido (2 caracteres, 21 caracteres, com espaço no meio, com símbolo) → `ArgumentException` |
| No máximo uma restrição | `Dado_CategoriaESubcategoria_Quando_CriarCupom_Entao_DeveLancarArgumentException` |
| Mínimo positivo quando informado | `Dado_ValorMinimoZero_…_DeveLancarArgumentException` |
| Percentual sobre a base | `Dado_Percentual10ComBase80_Quando_CalcularDescontoNosProdutos_Entao_DeveSer8` |
| Valor fixo limitado pela base | `Dado_ValorFixo15ComBase10_…_DeveSer10` |
| "Leve X, pague Y" por linha com valor líquido | `Dado_Leve3Pague2ComLinhaLiquida27Em3Unidades_…_DeveSer9` |
| Frete grátis zera qualquer opção e só ele mexe no frete | `Dado_FreteGratis_Quando_CalcularDescontoNoFrete_Entao_DeveSerOValorDoFrete` / `Dado_Percentual_…_DeveSerZero` |
| Mínimo sobre os elegíveis | `Dado_BaseElegivelAbaixoDoMinimo_Quando_AtingeMinimo_Entao_DeveSerFalso` |
| `AplicarCupom` não ultrapassa produtos nem frete | `Dado_DescontoMaiorQueProdutos_Quando_AplicarCupom_Entao_DeveLancarArgumentException` / o mesmo para o frete; `Dado_CupomAplicado_Quando_ValorDosProdutos_Entao_DeveDescontar` |
| Cada motivo de recusa | `CupomServiceTests`: um teste por mensagem de D11, com `Valido == false` e `Motivo` exato |
| Desativado é "não encontrado" | `Dado_CupomDesativado_Quando_Avaliar_Entao_MotivoDeveSerNaoEncontrado` |
| Acúmulo com promoção | `Dado_LinhaComPromocaoECupomPercentual_Quando_Avaliar_Entao_BaseDeveSerOValorLiquido` |
| Restrição por categoria e por subcategoria | `Dado_CupomDeCategoriaComItensDeOutra_Quando_Avaliar_Entao_SoDeveDescontarOsDaCategoria` / o mesmo para subcategoria |
| Itens indisponíveis não entram | `Dado_LinhaIndisponivel_Quando_Avaliar_Entao_NaoDeveContarNaBase` |
| Uso único ignora cancelado | integração: `Dado_PedidoCanceladoComCupom_Quando_ClienteJaUsouCupom_Entao_DeveSerFalso` / `Dado_PedidoPendenteComCupom_…_DeveSerVerdadeiro` |
| Código sem diferenciar maiúsculas e único | integração: `Dado_CodigoMinusculo_Quando_BuscarPorCodigo_Entao_DeveEncontrar` (via `NormalizarCodigo`) / `Dado_DoisCuponsComMesmoCodigo_Quando_Salvar_Entao_DeveFalhar` |
| Fechamento com cupom grava tudo | `Dado_CupomValido_Quando_Fechar_Entao_PedidoDeveTerCupomDescontosEValorLiquido` |
| Fechamento recusa cupom que deixou de valer | `Dado_CupomExpiradoNoFechamento_Quando_Fechar_Entao_DeveRecusarComCupomRecusado` |
| Fechamento recusa desconto divergente | `Dado_DescontoExibidoDiferente_Quando_Fechar_Entao_DeveRecusarComDescontoAtual` |
| `MontarPasso` usa o frete certo de referência | `Dado_PassoCarrinhoComFreteGratis_…_DeveUsarCotacaoMaisBarata` / `Dado_PassoPagamentoComFreteGratis_…_DeveUsarOpcaoEscolhida` |
| A borda do carrinho | `Dado_CodigoVazio_Quando_AplicarCupom_Entao_DeveRetornarViewIndexComErroNoCampo` / `Dado_CupomRecusado_…_DeveRetornarViewComMotivoNoCampo` / `Dado_CupomValido_…_DeveGravarNaSessaoERedirecionar` / `Dado_Requisicao_Quando_RemoverCupom_Entao_DeveLimparSessaoERedirecionar` |
| A sessão é limpa após o fechamento | `Dado_FechamentoComSucesso_Quando_Fechar_Entao_DeveLimparCupomDaSessao` / `Dado_CupomRecusado_Quando_Fechar_Entao_DeveLimparCupomDaSessao` |
| Admin: código duplicado vira erro de campo | `Dado_CodigoExistente_Quando_Cadastro_Entao_DeveRetornarViewComErroEmCodigo` |
| Sessão e campos ocultos de ponta a ponta | E2E `Dado_ClienteLogado_Quando_AplicarBemVindo10_Entao_DescontoDeveAparecerNoResumoENoPassoDePagamento` |
| Telas | Verificação manual. Como admin, criar um cupom de frete grátis com mínimo de R$ 50,00 em Doces e um "leve 3, pague 2" restrito a uma subcategoria, e conferir a listagem e a edição com o código só como texto. Como visitante, ver o campo desabilitado e o botão "Entrar". Como cliente: aplicar e ver cada motivo de D11, remover um item até perder o mínimo e ver o motivo (D10), escolher uma entrega mais cara e ver o frete grátis cobrir, fechar e conferir a confirmação, o detalhe e os usos na listagem do admin, e tentar o mesmo cupom de uso único de novo. |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Dois fechamentos simultâneos do mesmo cliente podem usar um cupom de uso único duas vezes. | Baixo: exige duas abas finalizando no mesmo instante. | Aceito. Um índice único não serve, porque um pedido cancelado libera o uso (D12). Fica registrado. |
| `Pagamento` exige valor maior que zero. Um cupom de 100% ou um valor fixo que cobre toda a base deixam só o frete. | Baixo: só quebraria com frete R$ 0,00, que a cotação não devolve hoje. | `Fechar` recusa com "Não foi possível aplicar este cupom a um pedido sem valor." quando o total líquido for 0, antes de criar o pagamento. Coberto por teste no passo 8. |
| `MontarPasso` ganha um parâmetro e uma dependência. | Médio: os testes existentes de `PedidoService` mudam de construtor. | O parâmetro é opcional. O mock de `ICupomService` fica no construtor da classe de teste. |
| O E2E depende do seed `BEMVINDO10` ter vigência na data em que a suíte roda. | Baixo. | O seed calcula a vigência a partir de "hoje" quando o banco é semeado, e o contêiner E2E semeia a cada execução. |
| Dependência da `034`. | Alto se for executada antes: `TipoDeDesconto`, `DescontoHelper`, `HorarioDeBrasilia` e o carrinho líquido ainda não existiriam. | O cabeçalho declara a dependência. `/implementar` desta feature só começa com a `034` executada. |

## 9. Resultado da execução

**Executado em:** 2026-11-20.

**Suíte:** `dotnet build` sem erros nem avisos. `DocesCabana.Tests`: 1018 aprovados
(unidade e integração). `DocesCabana.Tests.E2E`: 16 de 17 aprovados, incluindo o
novo `CupomTests`. O que falhou foi `CadastroDeProdutoTests.Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar`,
que tem `Trait("Categoria", "Externo")` e precisa da credencial do Supabase. Sem
ela, esse teste falha com ou sem esta feature.

**Testes novos:** `CupomTests` (entidade, 45 casos), os casos de cupom em
`PedidoTests`, `CupomDTOValidatorTests`, `AplicacaoDeCupomDTOValidatorTests`,
`CupomServiceTests`, os casos de cupom em `PedidoServiceTests`,
`CarrinhoControllerTests` e `PedidoControllerTests`, `Admin/CupomControllerTests`,
`CupomRepositoryIntegrationTests` (com o seed idempotente de `BEMVINDO10`), os
casos de cupom em `PedidoRepositoryIntegrationTests` e o E2E `Fluxos/CupomTests`.

**Desvios e detalhes que o plano não fixava:**

1. **O E2E não chega ao formulário de pagamento.** No E2E, a cotação de frete não
   responde, por causa da pendência de credencial da `022`. Então o passo de
   Pagamento mostra "Escolha um endereço e uma opção de entrega" e não mostra
   o formulário. O E2E prova que o código fica na sessão entre requisições e
   chega ao passo de Pagamento, com o canhoto e a linha do cupom no resumo. Ele
   **não** prova os campos ocultos `CodigoDoCupom` e `DescontoDoCupomExibido`.
   Esses campos ficam cobertos pela leitura da view e pelos testes de
   `PedidoService.Fechar`. Quando a credencial de frete existir, vale estender
   o E2E até o fechamento.
2. **Mensagens de `DescontoHelper.ValidarTipoEValor`.** As exceções extraídas de
   `Promocao` passaram a dizer "Desconto percentual..." e "Valor do desconto...",
   no lugar de "Promoção percentual..." e "Valor da promoção...". Assim elas
   valem para os dois usos. Os `PromocaoTests` continuaram verdes sem edição.
   A mensagem "Frete grátis só existe como cupom." continua em `Promocao`.
3. **`Pedido.Valor` é o total pago:** produtos + frete − os dois descontos do
   cupom. `ValorDoFrete` guarda o preço cheio da entrega. Os DTOs de confirmação
   e de detalhe ganharam `ValorDosItens`, que é a soma das linhas antes do
   cupom e alimenta a linha "Produtos". `ValorTotal` passou a ser
   `ValorDosProdutos + ValorDoFrete − DescontoNoFrete`.
4. **`PassoDoFechamentoDTO`** ganhou `ValorDoFreteDeReferencia`, `CupomValido`,
   `DescontoTotalDoCupom` e `ValorTotal`. As views do resumo e do pagamento
   passaram a tirar o total daí.
5. **A listagem do admin reaproveita o enum `SituacaoDaPromocao`** (Vigente,
   Agendado, Encerrado, Desativado). Não foi criado um enum só para cupom.
6. **Erro de código duplicado no admin.** A chave do erro é `Cupom.Codigo`,
   porque o formulário usa `[Bind(Prefix = "Cupom")]`, como em
   `Admin/Promocao`. O plano dizia só `"Codigo"`. Na edição, o código vai num
   campo oculto para o validator aceitar, e `CupomService.Atualizar` o ignora.
7. **Restrição no formulário do admin.** O rádio "Vale para" (todos, categoria,
   subcategoria) não é um campo do DTO. O JS desabilita a lista escondida, para
   que ela não seja enviada. Sem JS, as duas listas aparecem, e escolher as
   duas cai no erro do validator.
8. **Novo código recusado com outro cupom já aplicado.** O cupom anterior
   continua na sessão. O motivo da recusa aparece no campo.
9. **`ResultadoDoCupomDTO.ParaRecusa`** é um método de fábrica, no mesmo padrão
   de `ResultadoDoFechamentoDTO`.
10. **Visual do cupom aplicado.** O cupom aparece como um "canhoto" de papel,
    com recortes laterais feitos por `mask`. O rótulo do desconto usa a fonte
    manuscrita da marca (`--fonte-secundaria`). O estilo "em breve" saiu. O
    estilo do campo desabilitado ficou, porque o visitante ainda o vê.
11. **Status da `034`.** O cabeçalho do plano da `034` foi corrigido para
    `Executado`, com aprovação do usuário. O `specs/README.md` já a marcava
    como executada.

**Pendente:** passo 15, a verificação manual da seção 7.
