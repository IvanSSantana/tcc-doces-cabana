# Plano — Painel de vendas

**ID:** 037-painel-de-vendas · **Criado em:** 2026-11-23
**Status:** Aprovado

---

## 1. Objetivo

O administrador não tem hoje nenhuma visão das vendas: só cadastra produtos,
promoções, cupons e a agenda de retirada. Esta feature dá à área
administrativa um painel de receita com filtro por dia, semana, mês e ano —
cartões de receita, pedidos e ticket médio comparados ao período anterior,
gráfico da receita ao longo do período, produtos mais vendidos e distribuição
por dia da semana e por hora — para entender as vendas, ver o que mais sai e
achar picos e períodos fracos.

Para a receita ter critério, a situação do pedido passa a andar: o pedido
nasce `Confirmado` e o cliente marca como `Entregue` em "Meus pedidos" — o
que, de brinde, destrava a avaliação de produto (`032`), hoje inalcançável
fora do seed.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | O painel mostra **só receita**. Sem despesa, sem saída de caixa. | Decisão do responsável no debate. Não existe entidade de despesa; "fluxo de caixa" com entrada e saída é outra feature. | Cadastro de despesas junto — dobraria o escopo sem ser o que o painel precisa responder. |
| D2 | Pedido novo **nasce `Confirmado`** (construtor e `Pedido.ParaRetirada`). | Decisão do responsável. Sem processadora (`024`), nada tirava o pedido de `Pendente`, e "venda" não tinha critério. | Botão "Confirmar pedido" do cliente levando `Pendente` → `Confirmado` (proposta inicial, descartada pelo responsável). |
| D3 | Em "Meus pedidos", o cliente tem **"Marcar como entregue"**, que leva o pedido para `Entregue`. | Decisão do responsável. Libera também a avaliação, que exige pedido `Entregue` (`PedidoRepository.ExisteEntregueComProduto`). | Botão levando para `Confirmado` — redundante depois de D2. |
| D4 | `Pedido.MarcarComoEntregue` ganha invariante: lança `InvalidOperationException` se o pedido está `Cancelado` ou já `Entregue`. Aceita `Pendente`, `Confirmado` e `Enviado`. | Princípio II — a transição agora é exposta ao cliente, não só ao seed. `Pendente` é aceito porque pedidos antigos do banco continuam nele (D7). | Validar no serviço — a regra protege o dado, mora na entidade. |
| D5 | **Receita = pedidos `Confirmado`, `Enviado` e `Entregue`**; `Cancelado` e `Pendente` ficam fora. | `Pendente` deixa de nascer (D2); os que restam são do seed ou de antes desta feature e nunca foram confirmados. | Contar `Pendente` — inflaria a receita com pedido nunca confirmado. |
| D6 | **Valor da receita = `Pedido.ValorDosProdutos()`** (linhas já com promoção, menos o cupom), **sem frete**. | Decisão do responsável (opção A). Frete é repasse à transportadora, não ganho da loja, e incluí-lo distorce a comparação entre entrega e retirada. | `Pedido.Valor` (total pago) — infla receita de pedido entregue. |
| D7 | **Sem migração de dados** dos pedidos `Pendente` existentes. | Só o seed e testes manuais os produziram; o seed passa a variar status por conta própria. | `UPDATE` via migration — mexeria em dado sem ganho real e sem como desfazer com segurança. |
| D8 | `Pagamento` não muda: continua nascendo `Pendente`. | A situação do pagamento é dona da `024`; mexer aqui seria decidir por ela. | Aprovar o pagamento junto da confirmação — inventaria um "pagou" que ninguém verificou. |
| D9 | **Agregação em C#, no serviço** (caminho 1 do debate): o repositório devolve os pedidos do intervalo com itens e produto; `PainelDeVendasService` converte fuso, agrupa e calcula. | Aprovado no debate. Regra de negócio (fuso, faixas, comparação) fica toda testável em unidade com Moq. Volume de uma doceria — um ano cabe em memória com folga. | GROUP BY no banco (fuso/`date_trunc` no SQL, só provável em integração); endpoint JSON + `fetch` (segundo contrato e página vazia sem JS). |
| D10 | **Filtro com navegação**: escolhe-se `Dia`, `Semana`, `Mes` ou `Ano` e uma data de referência; ◀ ▶ andam para o período anterior/seguinte. O ▶ some quando o período seguinte começa no futuro. Padrão: `Mes` corrente. | Proposta aceita no debate. Período fechado navegável responde "como foi novembro" e "como foi a semana passada". | Intervalo livre de datas — mais formulário e comparação ambígua. |
| D11 | **Agrupamento da série**: `Dia` → 24 horas; `Semana` → 7 dias; `Mes` → dias do mês; `Ano` → 12 meses. Faixas sem venda aparecem com zero. | Proposta aceita no debate. Zero explícito é o que mostra o "período fraco". | Omitir faixas vazias — esconderia exatamente o vale que se procura. |
| D12 | **Semana de segunda a domingo**, datas no **horário de Brasília (UTC−3 fixo)**, reaproveitando `HorarioDeBrasilia`. | `Pedido.Data` é UTC; sem converter, pedido das 22h cai no dia seguinte. O Brasil não tem horário de verão desde 2019, e o helper já usa deslocamento fixo. | `TimeZoneInfo` com id de fuso — depende do SO (Windows × Linux) para nada. |
| D13 | **Comparação com o período imediatamente anterior** de mesmo tamanho (ontem, semana passada, mês passado, ano passado) nos três cartões, com variação percentual; variação some quando o anterior é zero. | Proposta aceita no debate. Percentual sobre zero não tem sentido. | Mesmo período do ano anterior — loja recém-aberta não tem histórico. |
| D14 | **Ticket médio = receita ÷ número de pedidos**; zero pedidos → zero. | Coerente com D6. | — |
| D15 | **Top produtos**: 10 primeiros por quantidade (desempate por receita, depois nome), cada um com sua receita = soma de `CalcularValorDaLinha()`, **sem ratear o cupom**. | Decidido no debate. Cupom é do pedido, não do produto; ratear daria precisão contábil que um painel de tendência não precisa. A soma do top pode passar um pouco da receita quando houve cupom — a tela diz isso numa nota. | Ratear cupom proporcional às linhas. |
| D16 | **Picos**: distribuição por **dia da semana** (7 barras) e por **hora** (24 barras), ambas sobre o período filtrado. No filtro `Dia`, só a de hora aparece (a série principal já é a de hora, então o bloco de picos é omitido inteiro). | Responde "períodos de pico ou fracos" sem tela nova. | Mapa de calor dia × hora — mais bonito, menos legível para quem só quer o pico. |
| D17 | **Chart.js 4.4.1** (build UMD), versionado em `wwwroot/lib/chart.js/` como o jQuery, carregado só na página do painel por `@section Scripts`. | Decisão do responsável de usar biblioteca. Arquivo local não depende de CDN e segue o padrão de `wwwroot/lib`. | CDN — rede em toda abertura e quebra offline; gráfico em SVG à mão — reinventa a biblioteca. |
| D18 | Dados do gráfico vão para a página num `<script type="application/json">` gerado no servidor; `painel-de-vendas.js` só desenha. Cartões e top produtos são HTML puro (funcionam sem JS); cada gráfico tem tabela equivalente em `<details>`. | Página já chega pronta; sem endpoint extra. Tabela dá acessibilidade e piso sem JS. | Endpoint JSON (D9). |
| D19 | Controlador **`Areas/Admin/Controllers/VendasController`**, rota `/Admin/Vendas?periodo=Mes&data=2026-11-01`, só `GET`, `[Authorize(Roles = Papeis.Administrador)]`. Link "Vendas" no submenu Admin do cabeçalho, primeiro da lista. | Princípio IV: nome em português e sem sinônimo na area. "Painel" sozinho é genérico demais para nome de controlador. | `DashboardController` (inglês); `PainelController`. |
| D20 | Ação do cliente: `POST /Pedido/MarcarComoEntregue/{id}` em `PedidoController`, com antiforgery, redireciona para `Meus` (PRG). Pedido de outro usuário ou inexistente → `KeyNotFoundException` → 404. Transição inválida (D4) → `FilterException` volta para a página de origem, mesmo ramo de `VotarUtil`/`Avaliar`. | Princípios VII e VIII; reaproveita ramo de erro existente. | Resposta JSON via `fetch` — sem necessidade. |
| D21 | O botão aparece no cartão de "Meus pedidos" **fora** do link do cartão (o cartão é um `<a>`; botão aninhado em link é HTML inválido), só para pedido `Pendente`, `Confirmado` ou `Enviado`. `ResumoDePedidoDTO` ganha `PodeMarcarComoEntregue`. | Pedido do responsável ("na página de pedidos"). Regra de exibição calculada no mapper, não na view. | Botão só no `Detalhe` — não é onde o responsável pediu. |
| D22 | Repositório ganha `BuscarParaAlterar(pedidoId, usuarioId)` rastreado, sem `Include`. | `Buscar` existente é `AsNoTracking` com itens — `Atualizar` nele marcaria itens como alterados à toa. | Reusar `Buscar` + `Atualizar`. |
| D23 | Sem E2E novo. | Princípio V (1.7.0): nem marcar entregue nem o painel são jornada de fluxo crítico; regra se prova em unidade/integração, autorização em teste de controlador. | E2E do painel — testaria layout e gráfico, que a régua exclui. |

**Fora de escopo:**

- Despesas, saídas, saldo de caixa (D1).
- Gestão de pedidos pelo admin (mudar situação, cancelar, listar todos os pedidos).
- Aprovação de pagamento e qualquer coisa da `024` (D8).
- Exportar (CSV/PDF), intervalo livre de datas, filtro por categoria/produto no painel.
- Migração dos pedidos `Pendente` existentes (D7).
- Cancelamento pelo cliente.

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Serviço e DTOs na `Application`; consulta na `Infrastructure`; nenhum `<ProjectReference>` novo. Chart.js é arquivo estático do MVC. |
| II | Domínio rico e auto-validante | ✅ OK | Status inicial e invariante de `MarcarComoEntregue` na entidade (D2, D4). |
| III | Validação nas duas barreiras | n/a | Nenhum formulário de entrada de dado. O filtro é `GET` com enum e data; valor inválido cai no padrão (`Mes`, hoje) no serviço, sem `ModelState`. |
| IV | Nomenclatura em português | ✅ OK | `VendasController`, `PainelDeVendasService`, `PeriodoDoPainel`, `MarcarComoEntregue`. Enum `Mes` sem acento, como `ForaDeEstoque`. |
| V | Testes escritos antes | ✅ OK | Cada passo de comportamento da seção 6 começa pelo teste. Integração para a nova consulta. |
| VI | Repositório + commit via UnitOfWork | ✅ OK | Marcar como entregue chama `IUnitOfWork.SalvarAlteracoes`. Sem migration (nenhuma mudança de esquema). |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `POST` com `[ValidateAntiForgeryToken]` e PRG (D20); painel com `[Authorize(Roles = Papeis.Administrador)]` (D19). |
| VIII | Tratamento de erro por camada | ✅ OK | Domínio lança `InvalidOperationException`; serviço lança `KeyNotFoundException`; controller sem `try/catch`, `FilterException` ganha o nome da ação na lista existente. |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Pedido.cs` | Alterar | Construtor e `ParaRetirada` atribuem `PedidoStatus.Confirmado`. `MarcarComoEntregue` lança `InvalidOperationException` para `Cancelado` e `Entregue` (D4). |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/PeriodoDoPainel.cs` | Criar | `Dia`, `Semana`, `Mes`, `Ano`. |
| `Helpers/HorarioDeBrasilia.cs` | Alterar | `ParaUtc(DateTime local)` e `DeUtc(DateTime utc)` com o mesmo deslocamento fixo. |
| `Helpers/IntervaloDoPainel.cs` | Criar | Record com `Inicio`/`Fim` locais (fim exclusivo); `static Calcular(PeriodoDoPainel, DateOnly referencia)` (segunda-feira para semana), `Anterior()`, `Seguinte()`, `Rotulo` em pt-BR ("23/11/2026", "17/11 a 23/11/2026", "novembro de 2026", "2026"). |
| `DTOs/PainelDeVendasDTO.cs` | Criar | DTO raiz e os registros aninhados da seção 5. |
| `DTOs/ResumoDePedidoDTO.cs` | Alterar | Novo parâmetro `PodeMarcarComoEntregue`. |
| `Mappings/PedidoMapper.cs` | Alterar | Preenche `PodeMarcarComoEntregue` (`Pendente`/`Confirmado`/`Enviado`). |
| `Contracts/Repositories/IPedidoRepository.cs` | Alterar | `ListarReceitaEntre`, `BuscarParaAlterar` (seção 5). |
| `Contracts/Services/IPedidoService.cs` | Alterar | `MarcarComoEntregue(Guid pedidoId, Guid usuarioId)`. |
| `Contracts/Services/IPainelDeVendasService.cs` | Criar | `Montar` (seção 5). |
| `Services/PedidoService.cs` | Alterar | Implementa `MarcarComoEntregue`: busca por `BuscarParaAlterar`, `KeyNotFoundException` se nulo, chama a entidade, `SalvarAlteracoes`. |
| `Services/PainelDeVendasService.cs` | Criar | Recebe `IPedidoRepository` e `TimeProvider`. Calcula intervalo atual e anterior, busca os dois, converte datas para Brasília, monta cartões, série (D11), top 10 (D15), dia da semana e hora (D16). |

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/PedidoRepository.cs` | Alterar | `ListarReceitaEntre`: `Where(Data >= inicioUtc && Data < fimUtc && Status in {Confirmado, Enviado, Entregue})`, `Include(Itens).ThenInclude(Produto)`, `AsNoTracking`. `BuscarParaAlterar`: rastreado, sem include, filtrando por usuário. |
| `DependencyInjections/ApplicationDependencyInjection.cs` | Alterar | `services.AddScoped<IPainelDeVendasService, PainelDeVendasService>();` ao lado de `IPedidoService`. |
| `DatabaseContext/Seed/DbInitializer.cs` | Alterar | `CriarPedido` passa a partir de `Confirmado` (o `switch` deixa de chamar `Confirmar()` antes de enviar/entregar; `Pendente` sai da lista semeada). Espalhar as datas dos pedidos de exemplo pelos últimos ~60 dias e horas variadas, para o painel ter o que mostrar. |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Areas/Admin/Controllers/VendasController.cs` | Criar | `[Area("Admin")]`, `[Authorize(Roles = Papeis.Administrador)]`. `GET Index(PeriodoDoPainel periodo = PeriodoDoPainel.Mes, DateOnly? data = null)` → `View(await _painel.Montar(periodo, data))`. |
| `Areas/Admin/Views/Vendas/Index.cshtml` | Criar | Abas de período (links `GET`), ◀ rótulo ▶, três cartões, gráfico da série, tabela top 10 com nota do cupom, bloco de picos (omitido em `Dia`), `<details>` com tabela de cada gráfico, `<script type="application/json" id="dados-do-painel">`, `@section Scripts` com Chart.js e o script da página. Estado vazio: "Nenhuma venda neste período." |
| `wwwroot/lib/chart.js/chart.umd.min.js` + `LICENSE.md` | Criar | Chart.js 4.4.1, versionado (D17). |
| `wwwroot/js/pages/painel-de-vendas.js` | Criar | Lê o JSON e desenha os gráficos com as cores dos tokens CSS da `033`; formata R$ em pt-BR. |
| `wwwroot/css/pages/painel-de-vendas.css` | Criar | Grade de cartões, área dos gráficos; responsivo; usa tokens existentes. |
| `Views/Shared/Components/Header/Default.cshtml` | Alterar | Item "Vendas" (`fa-chart-line`) no topo do submenu Admin. |
| `Controllers/PedidoController.cs` | Alterar | `[HttpPost][ValidateAntiForgeryToken] async Task<IActionResult> MarcarComoEntregue(Guid id)` → serviço → `RedirectToAction(nameof(Meus))`. |
| `Views/Pedido/Meus.cshtml` | Alterar | Formulário `POST` com botão "Marcar como entregue" ao lado do link do cartão, quando `PodeMarcarComoEntregue`. Rótulo de `Pendente` continua "Aguardando pagamento" (pedidos antigos). |
| `wwwroot/css/pages/pedidos.css` | Alterar | Posição do botão no cartão. |
| `Filters/FilterException.cs` | Alterar | Incluir `"MarcarComoEntregue"` na lista de ações que voltam ao `Referer` em `InvalidOperationException` (D20). |

### Testes
| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests/Units/Entities/PedidoTests.cs` | Alterar | Nascer `Confirmado` (construtor e retirada) substitui o teste de nascer `Pendente`; casos de `MarcarComoEntregue` (D4). Ajustar os testes que partiam de `Pendente`. |
| `DocesCabana.Tests/Units/Helpers/HorarioDeBrasiliaTests.cs` | Alterar | `ParaUtc`/`DeUtc`, incluindo virada de dia às 21h–23h59. |
| `DocesCabana.Tests/Units/Helpers/IntervaloDoPainelTests.cs` | Criar | Limites de cada período, semana começando na segunda (domingo e segunda como referência), virada de ano, `Anterior`/`Seguinte`, rótulos. |
| `DocesCabana.Tests/Units/Services/PainelDeVendasServiceTests.cs` | Criar | Receita sem frete e com cupom (D6), ticket médio e zero pedidos (D14), comparação e variação sumindo com anterior zero (D13), faixas zeradas (D11), pedido às 23h UTC−3 no dia certo (D12), top 10 com desempate (D15), picos e omissão em `Dia` (D16), referência nula usa hoje do `TimeProvider`, ▶ ausente no período corrente (D10). |
| `DocesCabana.Tests/Units/Services/PedidoServiceTests.cs` | Alterar | `MarcarComoEntregue`: sucesso salva; pedido de outro usuário → `KeyNotFoundException`; fechamento cria pedido `Confirmado`. |
| `DocesCabana.Tests/Units/Mappings/PedidoMapperTests.cs` | Alterar | `PodeMarcarComoEntregue` por situação. |
| `DocesCabana.Tests/Units/Controllers/PedidoControllerTests.cs` | Alterar | `MarcarComoEntregue` redireciona para `Meus`; atributos `HttpPost` + `ValidateAntiForgeryToken`. |
| `DocesCabana.Tests/Units/Controllers/Admin/VendasControllerTests.cs` | Criar | `Authorize` com papel `Administrador`; `Index` devolve `ViewResult` com o DTO; padrão `Mes`. |
| `DocesCabana.Tests/Units/Filters/FilterExceptionTests.cs` | Alterar | `InvalidOperationException` em `MarcarComoEntregue` redireciona ao `Referer`. |
| `DocesCabana.Tests/Integration/Repositories/PedidoRepositoryIntegrationTests.cs` | Alterar | `ListarReceitaEntre` respeita limite inclusivo/exclusivo, exclui `Cancelado`/`Pendente`, traz itens e produto; `BuscarParaAlterar` não acha pedido de outro usuário e persiste a mudança. |

## 5. Contratos e modelo de dados

Sem mudança de esquema, sem migration.

```csharp
// Domain/Entities/Pedido.cs
public void MarcarComoEntregue(); // agora lança InvalidOperationException (Cancelado | Entregue)

// Application/Enums/PeriodoDoPainel.cs
public enum PeriodoDoPainel { Dia, Semana, Mes, Ano }

// Application/Helpers/HorarioDeBrasilia.cs
public static DateTime ParaUtc(DateTime horarioDeBrasilia);
public static DateTime DeUtc(DateTime utc);

// Application/Helpers/IntervaloDoPainel.cs
public sealed record IntervaloDoPainel(PeriodoDoPainel Periodo, DateTime Inicio, DateTime Fim)
{
    public static IntervaloDoPainel Calcular(PeriodoDoPainel periodo, DateOnly referencia);
    public IntervaloDoPainel Anterior();
    public IntervaloDoPainel Seguinte();
    public string Rotulo { get; }
}

// Application/Contracts/Repositories/IPedidoRepository.cs
Task<List<Pedido>> ListarReceitaEntre(DateTime inicioUtc, DateTime fimUtc);
Task<Pedido?> BuscarParaAlterar(Guid pedidoId, Guid usuarioId);

// Application/Contracts/Services/IPedidoService.cs
Task MarcarComoEntregue(Guid pedidoId, Guid usuarioId);

// Application/Contracts/Services/IPainelDeVendasService.cs
Task<PainelDeVendasDTO> Montar(PeriodoDoPainel periodo, DateOnly? referencia);

// Application/DTOs/PainelDeVendasDTO.cs
public record PainelDeVendasDTO(
    PeriodoDoPainel Periodo,
    DateOnly Referencia,
    string RotuloDoPeriodo,
    DateOnly ReferenciaAnterior,
    DateOnly? ReferenciaSeguinte,          // null quando o seguinte começa no futuro
    IndicadorDoPainelDTO Receita,
    IndicadorDoPainelDTO Pedidos,
    IndicadorDoPainelDTO TicketMedio,
    IReadOnlyList<PontoDoGraficoDTO> Serie,
    IReadOnlyList<ProdutoMaisVendidoDTO> MaisVendidos,
    IReadOnlyList<PontoDoGraficoDTO> PorDiaDaSemana, // vazio no período Dia
    IReadOnlyList<PontoDoGraficoDTO> PorHora);       // vazio no período Dia

public record IndicadorDoPainelDTO(decimal Atual, decimal Anterior, decimal? VariacaoPercentual);

public record PontoDoGraficoDTO(string Rotulo, decimal Receita, int Pedidos);

public record ProdutoMaisVendidoDTO(Guid ProdutoId, string Nome, int Quantidade, decimal Receita);

// Application/DTOs/ResumoDePedidoDTO.cs
public record ResumoDePedidoDTO(
    Guid PedidoId, string Numero, DateTime Data, PedidoStatus Status,
    int QuantidadeDeItens, decimal ValorTotal, bool PodeMarcarComoEntregue);

// MVC/Areas/Admin/Controllers/VendasController.cs
[HttpGet] public Task<IActionResult> Index(PeriodoDoPainel periodo = PeriodoDoPainel.Mes, DateOnly? data = null);

// MVC/Controllers/PedidoController.cs
[HttpPost, ValidateAntiForgeryToken] public Task<IActionResult> MarcarComoEntregue(Guid id);
```

## 6. Ordem de execução

- [ ] 1. Teste: `Pedido` nasce `Confirmado` (construtor e `ParaRetirada`); `MarcarComoEntregue` aceita `Pendente`/`Confirmado`/`Enviado` e lança para `Cancelado`/`Entregue`. Ver falhar.
- [ ] 2. Implementar em `Pedido.cs`. Ajustar testes antigos que partiam de `Pendente`. Ver passar.
- [ ] 3. Teste: `PedidoMapper` preenche `PodeMarcarComoEntregue`. Implementar DTO + mapper.
- [ ] 4. Teste de integração: `BuscarParaAlterar` e `ListarReceitaEntre`. Adicionar ao contrato e implementar no `PedidoRepository`.
- [ ] 5. Teste: `PedidoService.MarcarComoEntregue` (sucesso salva; ausente → `KeyNotFoundException`). Implementar.
- [ ] 6. Teste: `PedidoController.MarcarComoEntregue` e o ramo novo do `FilterException`. Implementar ação e ramo.
- [ ] 7. `Meus.cshtml` + `pedidos.css`: formulário do botão fora do link do cartão.
- [ ] 8. Teste: `HorarioDeBrasilia.ParaUtc`/`DeUtc`. Implementar.
- [ ] 9. Teste: `IntervaloDoPainel` (limites, segunda-feira, virada de ano, anterior/seguinte, rótulos). Implementar.
- [ ] 10. Teste: `PainelDeVendasService` (lista da seção 4). Criar `PeriodoDoPainel`, DTOs, contrato e serviço; registrar na injeção de dependência.
- [ ] 11. Teste: `VendasController` (autorização, padrão, `ViewResult`). Implementar.
- [ ] 12. Adicionar Chart.js 4.4.1 em `wwwroot/lib/chart.js/` (arquivo UMD minificado + licença).
- [ ] 13. `Index.cshtml`, `painel-de-vendas.css`, `painel-de-vendas.js`; item "Vendas" no submenu Admin.
- [ ] 14. Seed: pedidos de exemplo partindo de `Confirmado`, datas e horas espalhadas nos últimos ~60 dias.
- [ ] 15. `dotnet build` e `dotnet test` completos (unidade, integração e E2E — o E2E de fechamento/meus pedidos não pode quebrar com a mudança de situação inicial).
- [ ] 16. Verificação manual: painel em cada período, ◀ ▶, estado vazio, tela de 375px, tema; marcar como entregue e conferir que a avaliação do produto libera.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Pedido novo nasce confirmado | `Dado_DadosValidos_Quando_CriarPedido_Entao_DeveNascerConfirmado` (+ variante de retirada) |
| Pedido cancelado não vira entregue | `Dado_PedidoCancelado_Quando_MarcarComoEntregue_Entao_DeveLancarInvalidOperationException` |
| Cliente só altera o próprio pedido | `Dado_PedidoDeOutroUsuario_Quando_MarcarComoEntregue_Entao_DeveLancarKeyNotFoundException` |
| Marcar entregue persiste | `Dado_PedidoConfirmado_Quando_MarcarComoEntregue_Entao_DeveSalvarAlteracoes` (unidade) + integração em `BuscarParaAlterar` |
| Receita exclui frete e desconta cupom | `Dado_PedidoComFreteECupom_Quando_Montar_Entao_ReceitaDeveSerValorDosProdutos` |
| Só situações válidas entram | integração `Dado_PedidosEmTodasAsSituacoes_Quando_ListarReceitaEntre_Entao_DeveTrazerSoConfirmadoEnviadoEntregue` |
| Fuso de Brasília | `Dado_PedidoAs23hDeBrasilia_Quando_Montar_Entao_DeveCairNoMesmoDia` |
| Semana começa na segunda | `Dado_ReferenciaNumDomingo_Quando_CalcularSemana_Entao_InicioDeveSerSegundaAnterior` |
| Faixas vazias aparecem zeradas | `Dado_MesSemVendasEmAlgunsDias_Quando_Montar_Entao_SerieDeveTerTodosOsDiasDoMes` |
| Variação some sem base | `Dado_PeriodoAnteriorSemVendas_Quando_Montar_Entao_VariacaoDeveSerNula` |
| Top 10 e desempate | `Dado_ProdutosEmpatadosNaQuantidade_Quando_Montar_Entao_DeveDesempatarPorReceita` |
| Painel só para administrador | `Dado_VendasController_Quando_InspecionarAtributos_Entao_DeveExigirPapelAdministrador` |
| Botão `POST` seguro e PRG | `Dado_PedidoValido_Quando_MarcarComoEntregue_Entao_DeveRedirecionarParaMeus` + atributos |
| Gráficos, layout, responsividade | verificação manual (passo 16) — sem E2E (D23) |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Nenhum desvio constitucional. | — | — |
| Pedido nasce `Confirmado` mas o detalhe ainda mostra pagamento "Aguardando pagamento" (D8). | Médio — cliente vê informação aparentemente contraditória. | Aceito até a `024`, dona do pagamento. Registrar no backlog do `specs/README.md` se incomodar na verificação manual. |
| O cliente autodeclara a entrega; nada impede marcar antes de receber. | Médio — libera avaliação antes da hora; não afeta a receita (`Confirmado` e `Entregue` já contam). | Aceito: é a decisão do debate (D3), na ausência de gestão de pedidos pelo admin. |
| Mudar a situação inicial quebra testes e E2E que esperavam `Pendente`/"Aguardando pagamento". | Médio — suíte vermelha no meio da execução. | Passo 2 ajusta unidade; passo 15 roda o E2E inteiro. Busca inicial não achou E2E afirmando "Aguardando pagamento". |
| Carregar o ano inteiro em memória (D9). | Baixo — volume de doceria. | Projeção `AsNoTracking`; se um dia pesar, a troca é interna ao repositório/serviço sem mudar o contrato do DTO. |
| Pedidos `Pendente` antigos ficam fora da receita (D5, D7). | Baixo — só seed e testes manuais. | Seed corrigido no passo 14; banco de desenvolvimento pode ser recriado. |
| Soma do top produtos ≠ receita quando há cupom (D15). | Baixo — pode confundir. | Nota na tabela explicando o motivo. |

## 9. Resultado da execução

*Preenchido na execução.*
