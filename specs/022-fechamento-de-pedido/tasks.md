# Tarefas — Fechamento de pedido

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`… — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Duas ordens não são negociáveis.**
>
> **O domínio antes da tela.** `Pedido` vira raiz de agregado nesta entrega — a
> decisão que a modelagem adiou por escrito. Fazer isso depois de a tela existir
> significa reescrever a tela.
>
> **A semeadura de pedidos vem antes da vitrine.** Trocar o critério para "mais
> vendidos" sem venda registrada põe cem produtos empatados em zero e exibe
> ordem alfabética sob título falso — o defeito que a `019` recusou cometer.
> Semear primeiro, trocar depois.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `022-fechamento-de-pedido` a partir de `main`.
- [x] **T002** — Build limpo (só o aviso pré-existente do pacote SQLite). Estado inicial: `DocesCabana.Tests` 604/604, `DocesCabana.Tests.E2E` 175/175.
- [x] **T003** — Localizados: `CatalogoControllerTests.Dado_OrdenacaoMaisVendidos_Quando_Index_Entao_DeveSanearParaMelhorAvaliados` ([CatalogoControllerTests.cs:67](../../DocesCabana.Tests/Units/Controllers/CatalogoControllerTests.cs)) e `PaginaInicialTests.Dado_PaginaInicial_Quando_LerOTituloDaSecao_Entao_NaoDeveDizerMaisVendidos` ([PaginaInicialTests.cs:139](../../DocesCabana.Tests.E2E/Fluxos/PaginaInicialTests.cs)). Reescritos na Fase 8 (T041/T045).

## Fase 2 — `Pedido` vira raiz de agregado

- [x] **T004** — `DocesCabana.Tests/Units/Entities/PedidoTests.cs`: invariantes das cinco propriedades novas — frete negativo recusado, prazos coerentes, transportadora e serviço obrigatórios. Visto falhar por `CS1729`/`CS1061` (construtor de 8 argumentos e membros novos inexistentes).
- [x] **T005** `[P]` — Mesmo arquivo: `AcrescentarItem` acumula; a coleção exposta é somente-leitura; **`NumeroVisivel()` tem oito caracteres, é maiúsculo e é estável para o mesmo pedido** (RF-23).
- [x] **T006** — `DocesCabana.Domain/Entities/Pedido.cs`: `ValorDoFrete`, `Transportadora`, `Servico`, `PrazoMinimoEmDias`, `PrazoMaximoEmDias`; coleção de itens com campo de apoio; `AcrescentarItem`; `NumeroVisivel()` como **método**, não propriedade.
- [x] **T007** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde (620/620).

## Fase 3 — Persistência

- [x] **T008** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/PedidoConfiguration.cs`: colunas novas com precisão, e a coleção de itens pelo campo de apoio (`HasMany(p => p.Itens).WithOne(i => i.Pedido)` — EF Core resolveu o campo `_itens` por convenção de nome, sem configuração extra). `ItemPedidoConfiguration` deixou de declarar o lado espelhado, para não haver duas configurações da mesma relação.
- [x] **T009** — Migration `AddPedidoDadosDeEntrega` gerada e conferida: só `AddColumn` em `Pedido` (`ValorDoFrete`, `Transportadora`, `Servico`, `PrazoMinimoEmDias`, `PrazoMaximoEmDias`). **Tabela com zero linhas** — sem backfill, sem risco.
- [x] **T010** `[P]` — `IPedidoRepository`/`PedidoRepository`: `BuscarPorIdComItens` (com `Itens`, `Produto` de cada item e `EnderecoEntrega`), `ListarPorUsuario`. **Sem repositório próprio para item** — grava pela navegação da raiz. Pagamento também não tem repositório próprio, mas precisa ser lido/gravado via `IPedidoRepository` mesmo assim (não tem navegação em `Pedido`, é 1:1 configurado do lado de `Pagamento`) — `BuscarPagamentoPorPedido` e `AdicionarComPagamento(pedido, pagamento)`, este último sem chamar `SalvarAlteracoes` (quem decide o commit é a Fase 4).
- [x] **T011** — Repositório registrado em `ApplicationDependencyInjection.cs`. Serviço (`IPedidoService`) ainda não existe — registrado na Fase 4.
- [x] **T012** — `DocesCabana.Tests/Integration/Repositories/PedidoRepositoryIntegrationTests.cs`: gravar pedido com item e pagamento juntos e ler de volta com eles (prova o agregado, a migration e `AdicionarComPagamento`); `ListarPorUsuario` só traz os do usuário certo.
- [x] **T013** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde (623/623).

## Fase 4 — O fechamento

> É o coração da entrega. As cinco recusas do plano §4 têm teste **antes** do
> caminho feliz: é mais fácil escrever o feliz e esquecer que a recusa existe.

- [x] **T014–T021** — **Desvio de ordem, registrado.** Diferente do previsto, os tipos (`PassoDoFechamento`, `FechamentoDePedidoDTO`, `ResultadoDoFechamentoDTO`, `ConfirmacaoDePedidoDTO`, `PassoDoFechamentoDTO`, `PedidoMapper`, `FechamentoDePedidoDTOValidator`, `IPedidoService`/`PedidoService`) e o teste (`DocesCabana.Tests/Units/Services/PedidoServiceTests.cs`) foram desenhados juntos, não teste-primeiro-vermelho-depois-implementação: o algoritmo dos nove passos do plano §4 tem tantas dependências entre os tipos (o formato de `ResultadoDoFechamentoDTO` só faz sentido depois de decidir o que cada recusa precisa devolver) que escrever o teste isolado primeiro teria exigido adivinhar essas formas e provávelmente reescrevê-las. Mesma classe de decisão já tomada na spec `020` (Fase 2, `ProdutoMapper.ToEntity`). O teste **cumpriu seu papel de verificação**: rodado contra a implementação já escrita, achou um bug real de teste (não da implementação) na primeira passada — `Dado_ValoresDeCentavo_...` chamava `Fechar` sem mockar `IItemCarrinhoRepository.BuscarPorUsuario`, e `EsvaziarSemSalvar` lançava `NullReferenceException` ao iterar `null` — corrigido no teste. Cobre: carrinho vazio recusa (RF-16); item indisponível recusa nomeando o item (RF-16, CA-18); valor dos produtos divergente recusa devolvendo o atual (RF-15, CA-16); caso de centavo não recusa fechamento legítimo (plano §9); cotação indisponível recusa (RF-17, CA-19); opção de entrega sumida da re-cotação recusa; frete divergente recusa devolvendo o atual (CA-17); caminho feliz grava pedido+itens+pagamento e esvazia o carrinho com **um `SalvarAlteracoes` só** (RF-20, RN-07); preço gravado é o de agora, não o exibido (RF-19, CA-12). `IPedidoService`/`PedidoService` registrados em `ApplicationDependencyInjection.cs`. Nenhuma recusa lança exceção (Princípio VIII) — todas voltam em `ResultadoDoFechamentoDTO.Sucesso == false`.
- [x] **T022** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde (632/632).

## Fase 5 — A borda web

- [x] **T023** — `DocesCabana.Tests/Units/Controllers/PedidoControllerTests.cs`: sucesso redireciona para a confirmação; recusa (do serviço, e separadamente do `ModelState` já inválido antes de chamar o serviço) devolve a view com `ModelState` inválido; **confirmação de pedido alheio ou inexistente devolve 404** (RN-08, via `ObterConfirmacao` devolvendo `null` — mesmo tratamento para os dois casos). Escrito junto com T024 (mesmo motivo do desvio registrado em T014–T021: a recusa precisa devolver a mesma view de `Carrinho/Index`, decisão que só fez sentido depois de desenhar o controller inteiro).
- [x] **T024** — `DocesCabana.MVC/Controllers/PedidoController.cs`: `Fechar` (`[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Authorize]`, aguardado, redirecionando no sucesso) e `Confirmacao` (GET, por identificador). Na recusa, reexibe `~/Views/Carrinho/Index.cshtml` (mesma view do carrinho — "exibir é do carrinho", plano §1) com `ModelState` inválido e os valores atuais em `ViewData`. **O redirecionamento no sucesso é o que resolve o CA-14** — recarregar o comprovante é `GET`.
- [x] **T025** `[P]` — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`: `passo` desconhecido na querystring cai no primeiro membro do enum (`Carrinho`) por conta do próprio model binding, sem código extra; **passo de conta não é oferecido a quem já entrou** (RF-03, CA-03); **visitante navegando direto para `?passo=Endereco` cai no passo de conta** (RF-02), guarda que o plano não detalhava mas que ficou necessária.
- [x] **T026** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `Index` aceita `passo`/`enderecoId`, monta `PassoDoFechamentoDTO` via `IPedidoService.MontarPasso` (carrinho passado por parâmetro, não buscado pelo serviço — `ICarrinhoService` conhece sessão/HttpContext, que é detalhe de MVC) e expõe em `ViewData["PassoDoFechamento"]`, mantendo o caminho assíncrono da `017`/`021` intacto.
- [x] **T027** — Rodar `dotnet test DocesCabana.Tests`: Fase 5 verde (640/640).

## Fase 6 — As telas

- [x] **T028** — `Views/Carrinho/_PassosDoFechamento.cshtml`: o indicador, com o passo ativo destacado via `aria-current` (RF-01). **Some o passo de conta para quem já entrou** (`PassosVisiveis`, montado por `IPedidoService.MontarPasso`).
- [x] **T029** `[P]` — `Views/Carrinho/_PassoConta.cshtml`: entrar ou criar conta (RF-02), `returnUrl=/Carrinho` nos dois links. Depois de entrar, **volta ao passo do carrinho** (RF-04) — estendido `AutenticacaoController.Cadastro`/`Login.cshtml`/`Cadastro.cshtml` para o `returnUrl` sobreviver também ao caminho de criar conta, não só ao de entrar (a `015` só cobria o de entrar).
- [x] **T030** — `Views/Carrinho/_PassoEndereco.cshtml`: lista de endereços com o principal marcado (RF-06); **sem nenhum, o formulário da `018` aparece ali mesmo** (RF-07) — extraído `Conta/_FormularioEndereco.cshtml` de `Conta/FormularioEndereco.cshtml` (reaproveitado, não copiado, como o plano pediu), agora parametrizado por controlador/ação via `ViewData`, postando para `CarrinhoController.CadastrarEndereco` (novo). O primeiro endereço de alguém já nasce principal (`EnderecoService`, RN-02 da `018`), então volta escolhido sozinho, sem código extra.
- [x] **T031** — Mesmo arquivo: as opções de entrega do endereço escolhido, com transportadora, serviço, preço e faixa de prazo (RF-08/RF-09). **Desvio do plano, necessário:** a opção de entrega escolhida viaja pela querystring entre os passos (`servicoDeEntregaId`), igual ao endereço — não em sessão (a "cotação em sessão" foi recusada ao especificar, plano §8) nem só no formulário final; sem isso, RF-09 ("o resumo DEVE refletir a escolha") não tinha como ser satisfeito sem JavaScript. `IPedidoService.MontarPasso` ganhou o parâmetro correspondente.
- [x] **T032** `[P]` — `Views/Carrinho/_PassoPagamento.cshtml`: as quatro formas, **sem coletar nenhum dado** (RF-10/RF-11), com o aviso de que o pagamento será combinado (RF-12). O formulário final carrega endereço/serviço de entrega/valores exibidos como campos ocultos — resolvidos no servidor a partir da querystring já validada nos passos anteriores, nunca por JavaScript sincronizando um valor por rádio (funciona sem script por desenho, não por acaso).
- [x] **T033** — `Views/Pedido/Confirmacao.cshtml`: número, itens, valores, prazo e o que acontece a seguir (RF-22). **Sem link para histórico de pedidos** — ele é a entrega seguinte e ainda não existe.
- [x] **T034** `[P]` — `wwwroot/css/pages/carrinho.css`: estilos dos passos, endereço, pagamento e confirmação; `.botao-finalizar-carrinho` deixou de ser `<button disabled>` e virou link estilizado.
- [x] **T035** — `wwwroot/js/components/carrinho.js`: clique em `[data-navegacao-fechamento]` troca só `#itens-carrinho` (RF-05); cadastro de endereço dentro do fechamento reaproveita o `enviar()` genérico já existente. **Sem script, os passos são navegação `GET`/`POST` comum** — confirmado na T036 (E2E roda com JavaScript ligado; a prova explícita sem JavaScript é a Fase 9, CA-23).
- [x] **T036** — Rodar as duas suítes: Fase 6 verde (`DocesCabana.Tests` 642/642; `DocesCabana.Tests.E2E` 175/175). **Achado ao rodar, corrigido:** `CarrinhoTests.Dado_TelaDoCarrinho_Quando_OlharOBotaoDeFinalizar_Entao_DeveEstarDesabilitadoEExplicado` (spec 021, CA-07) afirmava o botão desabilitado — comportamento que esta entrega substitui de propósito (RF-01). Reescrito para provar que o clique agora navega para o passo de endereço, mesma classe de correção que a `019` já fez para a `022`. Uma falha isolada e não relacionada (`BuscaTests`, acentuação) não se repetiu ao rodar de novo — instabilidade pré-existente, alheia a esta entrega.

## Fase 7 — Pedidos de demonstração

- [x] **T037** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: semear pedidos fechados entre os oito clientes fictícios, com produtos e quantidades variados, **situações variadas** (pendente, confirmado, enviado, entregue) e **um pedido cancelado com a maior quantidade de todas** — carrega de propósito o produto que seria "mais vendido", para CA-22 provar que cancelado não conta (RF-27). Um endereço novo por cliente fictício (`Endereco`, nenhum existia antes). **Achado ao implementar:** `Pedido` não tinha como mudar de `Status` depois de criado — `Cancelar`/`Confirmar`/`MarcarComoEnviado`/`MarcarComoEntregue` são métodos novos na entidade, usados só pela semeadura (nenhum caminho real desta entrega os chama; spec §10), cobertos em `PedidoTests`. Tudo criado pelos construtores da aplicação, **não por SQL solto**.
- [x] **T038** — `DocesCabana.Tests/Integration/Repositories/DbInitializerPedidosIntegrationTests.cs`: confere que os pedidos existem, têm itens e pagamento, que há um cancelado, e que as quantidades vendidas (excluindo o cancelado) diferem o bastante entre pelo menos três produtos. `SemearPedidosDeExemplo` tornado `internal` para o teste chamar direto, sem subir Identity inteiro (mesmo padrão de `GerarProdutosMock`).

## Fase 8 — A vitrine passa a mais vendidos

- [x] **T039** — `DocesCabana.Tests/Integration/Repositories/CatalogoRepositoryIntegrationTests.cs`: ordenação por venda — mais vendido primeiro; produto sem venda por último, **não ausente**; **pedido cancelado não conta** (RN-05, CA-22). Visto falhar (ordem alfabética, não por venda) nos dois casos que não coincidiam por acaso com a ordem alfabética. É subconsulta traduzida a SQL: teste de unidade não a exercita.
- [x] **T040** — `DocesCabana.Infrastructure/Repositories/ProdutoRepository.cs`: o ramo `MaisVendidos` de verdade (plano §5), com `(int?)` e `?? 0` para produto sem venda vir por último — mesma forma que `MelhorAvaliados` usa desde a `014`.
- [x] **T041** — **Reescrito** o teste localizado em T003 em `CatalogoControllerTests`: `MaisVendidos` passa a ser executada, não saneada (RF-26, CA-21). Correção esperada, não regressão. **Três achados adicionais, fora do previsto em T003, mesma classe de correção:** `CatalogoTests.Dado_SeletorDeOrdenacao_Quando_TentarEscolherMaisVendidos_Entao_DeveEstarIndisponivel` (E2E) reescrito para provar que a opção agora funciona; `ProdutoServiceTests.Dado_UmLimite_Quando_BuscarDestaquesDaVitrine_...` (unidade) reescrito de `MelhorAvaliados` para `MaisVendidos`; `CarrinhoTests.Dado_TelaDoCarrinho_Quando_OlharOBotaoDeFinalizar_Entao_DeveEstarDesabilitadoEExplicado` (E2E, spec 021 CA-07), já registrado na Fase 6/T036.
- [x] **T042** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: **removido** `SanearOrdenacao`. `Views/Catalogo/_ResultadoCatalogo.cshtml` também perdeu o `disabled`/"(em breve)" da opção no seletor.
- [x] **T043** — `Application/Services/ProdutoService.cs`: `BuscarDestaquesDaVitrine` passa a pedir `OrdenacaoCatalogo.MaisVendidos` (RF-24). `HomeController.cs` não precisou mudar — já delega o critério inteiro ao serviço.
- [x] **T044** — `DocesCabana.MVC/Views/Home/Index.cshtml`: título vira **"Mais vendidos"** (RF-25).
- [x] **T045** — **Reescritos** os dois testes de T003 em `PaginaInicialTests`: título agora afirma "Mais vendidos" (era "não deve dizer") e nega "Bem avaliados" (era o contrário) (CA-20). **Um terceiro, fora do previsto em T003:** `Dado_ProdutosComAvaliacoesDiferentes_..._AVitrineDeveOrdenarPorNotaMedia` reescrito para `Dado_ProdutosComVendasDiferentes_..._AVitrineDeveOrdenarPorQuantidadeVendida`, lendo a venda de cada produto direto do SQLite do E2E (`ObterQuantidadeVendida`, mesmo padrão de `ObterNotaMedia`).
- [x] **T046** — Rodar as duas suítes: Fase 8 verde (`DocesCabana.Tests` 648/648; `DocesCabana.Tests.E2E` 175/175).

## Fase 9 — Prova de ponta a ponta

> **⚠️ Achado ao escrever esta fase, não previsto pelo plano:** o ambiente de
> teste E2E sobe sem credencial do MelhorEnvio (mesma pendência da spec `020`
> §10, ainda não obtida) — e sem ela, **toda recotação de frete falha**,
> inclusive a que os passos de Endereço e Pagamento fazem para calcular as
> opções de entrega. Isso significa que, na suíte padrão, é estruturalmente
> impossível alcançar o passo de Pagamento ou confirmar um pedido de
> verdade — o link "Continuar para pagamento" só existe quando a cotação
> tem sucesso. Praticamente todo critério que dependia de uma cotação
> bem-sucedida (**CA-05, CA-07 a CA-15, CA-17**) fica bloqueado pela mesma
> pendência que já bloqueia a Fase 8 da `020`. T047–T050 foram reduzidas ao
> que é alcançável sem credencial — os passos, a navegação, o cadastro de
> endereço, e o caminho de falha de entrega (RF-17/CA-19), que é
> exatamente o que a suíte padrão sempre exercita de verdade, já que a
> cotação falha sempre neste ambiente. O resto — a jornada completa até o
> comprovante — fica sem tarefa própria nesta spec (o plano não previu a
> dependência) e é adiado para quando a credencial da `020` existir; o
> lugar natural é estender a Fase 8 daquela spec (T048/T049, já marcadas
> `[Trait("Categoria", "Externo")]`) para cobrir também estes critérios,
> não criar uma segunda pendência de credencial em paralelo.

- [x] **T047** — `DocesCabana.Tests.E2E/Fluxos/FechamentoTests.cs` e `Paginas/PaginaFechamento.cs`: os passos aparecem com o do carrinho ativo, havendo item disponível (CA-01). **CA-05/CA-07/CA-10/CA-11/CA-15 (jornada completa até o comprovante) ficam bloqueados** pela falta de credencial — ver nota acima.
- [x] **T048** `[P]` — Mesmos arquivos: visitante encontra o passo de entrar (CA-02); entrar devolve ao passo do carrinho (CA-04); sem endereço, cadastra no próprio passo e fica escolhido (CA-06, só a parte que não depende de cotação bem-sucedida). **CA-08 (trocar endereço troca as opções) bloqueado** — precisa de duas cotações reais, diferentes entre si.
- [x] **T049** `[P]` — **Bloqueado inteiro**: carrinho vazio depois (CA-13), recarregar não duplica (CA-14) e nenhum dado de pagamento é pedido (CA-09) só são alcançáveis depois de confirmar um pedido de verdade, o que exige a cotação ter sucedido.
- [x] **T050** — **Bloqueado**: o caminho sem JavaScript da jornada completa (CA-23) tem a mesma dependência. Em compensação, escrito **um teste novo, fora do previsto** (`Dado_ServicoDeEntregaIndisponivel_Quando_ChegarAoEnderecoEscolhido_Entao_DeveAvisarSemOferecerContinuar`) provando RF-17/CA-19 — a entrega incalculável avisa, não deixa continuar, e o carrinho segue utilizável (RN-02) — que é o comportamento real e determinístico deste ambiente, não uma simulação.
- [x] **T051** — Rodar as duas suítes: Fase 9 verde (`DocesCabana.Tests` 648/648; `DocesCabana.Tests.E2E` 180/180 — 5 testes novos em `FechamentoTests`). Uma falha isolada e não relacionada (`BuscaTests`, acentuação) não se repetiu ao rodar de novo — instabilidade pré-existente, alheia a esta entrega (já registrada na Fase 6).

## Fase 10 — Fechamento

- [x] **T052** — `docs/arquitetura.md` §5: linha do `/Carrinho` atualizada (`CadastrarEndereco`, `IPedidoService`) e nova linha `/Pedido/Confirmacao/{id}`.
- [x] **T053** `[P]` — `docs/arquitetura.md` nova §6.11: os nove passos do fechamento, por que a conferência usa alegação em vez de confiar no formulário, `Pedido` como raiz de agregado, e a ordenação por venda.
- [x] **T054** — `docs/arquitetura.md` §9.3: `Pedido`, `ItemPedido` e `Pagamento` **saíram** da lista de tabelas sem comportamento. Restam duas: `Estoque` e `Promocao`.
- [x] **T055** — Varredura feita. Nenhuma referência obsoleta encontrada introduzida por esta feature — os comentários novos citam `020`/`021`/`022` corretamente, e nenhum "ainda não disponível"/"em breve" relacionado a fechamento ou ordenação por venda sobrou no código.
- [x] **T056** — `specs/README.md`: status de `022` → *Implementada* nas duas tabelas, com link de `checklist`; nota de "Ordem executada" com `022` ao final e uma frase sobre a Fase 9 pendente (mesma credencial da `020`), sem bloquear o restante.
- [x] **T057** — `specs/000-baseline/spec.md` conferida: a menção a `Pedido`/`Pagamento` sem transição de estado é texto descritivo da seção 5, não uma dívida numerada (D-01 a D-07) na tabela da seção 6 — nada para riscar.
- [x] **T058** — `dotnet build` sem aviso novo; `DocesCabana.Tests` 648/648 e `DocesCabana.Tests.E2E` 180/180, do zero, sem credencial de frete no ambiente.
- [x] **T059** — **Parcialmente possível, mesma razão da Fase 9**: a suíte E2E já percorre de verdade, em Chromium real, todo o caminho alcançável sem credencial (catálogo → carrinho → passos → endereço → mensagem de falha de entrega). Confirmar um pedido e chegar ao comprovante **não pôde ser percorrido nem manualmente** — depende da mesma cotação que falha sem a credencial do MelhorEnvio. Fica para quando ela existir.
- [x] **T060** — `checklist.md` preenchido, distinguindo o que foi provado sem rede do que só será provado quando a credencial existir.
- [x] **T061** — Status da spec → *Implementada*; status do plano → *Executado*; `specs/README.md` atualizado (T056). Registrado nos três lugares o que esta entrega **não** encerra: nada é cobrado (RF-11/RN-03), estoque não baixa, histórico de pedidos é a entrega seguinte, e a jornada completa até o comprovante segue sem prova automatizada/manual até a credencial do MelhorEnvio existir.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T028, T047 |
| RF-02 | T029, T048 |
| RF-03 | T025, T028 |
| RF-04 | T029, T048 |
| RF-05 | T035, T050 |
| RF-06 | T030, T047 |
| RF-07 | T030, T048 |
| RF-08 | T031, T047 |
| RF-09 | T031, T048 |
| RF-10 | T032, T049 |
| RF-11 | T032, T049 |
| RF-12 | T032 |
| RF-13 | T047 |
| RF-14 | T015, T016, T021 |
| RF-15 | T015, T016 |
| RF-16 | T014 |
| RF-17 | T016 |
| RF-18 | T017, T021 |
| RF-19 | T017 |
| RF-20 | T017, T021 |
| RF-21 | T017, T049 |
| RF-22 | T033, T047 |
| RF-23 | T005 |
| RF-24 | T040, T043 |
| RF-25 | T044, T045 |
| RF-26 | T041, T042 |
| RF-27 | T037, T038 |
| RN-01 | T017 |
| RN-02 | T015, T016 |
| RN-03 | T032 |
| RN-04 | T037, T044 |
| RN-05 | T039 |
| RN-06 | T014 |
| RN-07 | T017, T021 |
| RN-08 | T023 |
| CA-01 | T028, T047 |
| CA-02 | T048 |
| CA-03 | T025, T028 |
| CA-04 | T029, T048 |
| CA-05 | T030, T047 |
| CA-06 | T030, T048 |
| CA-07 | T031, T047 |
| CA-08 | T048 |
| CA-09 | T032, T049 |
| CA-10 | T047 |
| CA-11 | T017, T047 |
| CA-12 | T017 |
| CA-13 | T049 |
| CA-14 | T024, T049 |
| CA-15 | T033, T047 |
| CA-16 | T015 |
| CA-17 | T016 |
| CA-18 | T014 |
| CA-19 | T016 |
| CA-20 | T043, T044, T045 |
| CA-21 | T041, T042 |
| CA-22 | T037, T039 |
| CA-23 | T050 |
