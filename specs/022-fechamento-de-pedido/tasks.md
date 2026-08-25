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

- [ ] **T001** — Criar branch `022-fechamento-de-pedido` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build` e as duas suítes; registrar o estado inicial.
- [ ] **T003** — Localizar os testes que esta entrega **quebra de propósito**: o de `CatalogoControllerTests` que afirma o saneamento de `MaisVendidos`, e o de `PaginaInicialTests` que afirma que o título **não** diz "Mais Vendidos". A `019` escreveu os dois sabendo que cairiam aqui — são reescritos na Fase 8, não removidos.

## Fase 2 — `Pedido` vira raiz de agregado

- [ ] **T004** — `DocesCabana.Tests/Units/Entities/PedidoTests.cs`: invariantes das cinco propriedades novas — frete negativo recusado, prazos coerentes, transportadora e serviço obrigatórios. Ver falhar.
- [ ] **T005** `[P]` — Mesmo arquivo: `AcrescentarItem` acumula; a coleção exposta é somente-leitura; **`NumeroVisivel()` tem oito caracteres, é maiúsculo e é estável para o mesmo pedido** (RF-23).
- [ ] **T006** — `DocesCabana.Domain/Entities/Pedido.cs`: `ValorDoFrete`, `Transportadora`, `Servico`, `PrazoMinimoEmDias`, `PrazoMaximoEmDias`; coleção de itens com campo de apoio; `AcrescentarItem`; `NumeroVisivel()` como **método**, não propriedade — propriedade computada o EF tentaria mapear para coluna, mesmo motivo de `Produto.DisponivelParaCompra()`.
- [ ] **T007** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde.

## Fase 3 — Persistência

- [ ] **T008** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/PedidoConfiguration.cs`: colunas novas com precisão, e a coleção de itens pelo campo de apoio.
- [ ] **T009** — Gerar e conferir a migration `AddPedidoDadosDeEntrega`. **Tabela com zero linhas** — sem backfill, sem risco.
- [ ] **T010** `[P]` — `DocesCabana.Application/Contracts/Repositories/IPedidoRepository.cs` e `Infrastructure/Repositories/PedidoRepository.cs`: adicionar, buscar por identificador **com os itens**, e listar por usuário. **Sem repositório para item nem para pagamento** — a raiz do agregado grava os itens junto (plano §3).
- [ ] **T011** — Registrar serviço e repositório em `ApplicationDependencyInjection.cs`.
- [ ] **T012** — `DocesCabana.Tests/Integration/Repositories/`: gravar um pedido com itens e ler de volta com eles. Prova o agregado e a migration.
- [ ] **T013** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — O fechamento

> É o coração da entrega. As cinco recusas do plano §4 têm teste **antes** do
> caminho feliz: é mais fácil escrever o feliz e esquecer que a recusa existe.

- [ ] **T014** — `DocesCabana.Tests/Units/Services/PedidoServiceTests.cs`: carrinho vazio recusa; **item indisponível recusa nomeando o item** (RF-16, CA-18). Ver falhar.
- [ ] **T015** `[P]` — Mesmo arquivo: valor dos produtos divergente do exibido recusa e devolve o atual (RF-15, CA-16); **caso de centavo**, para arredondamento não recusar fechamento legítimo (plano §9).
- [ ] **T016** `[P]` — Mesmo arquivo: cotação indisponível recusa (RF-17, CA-19); opção de entrega escolhida que sumiu da re-cotação recusa; frete divergente recusa (CA-17).
- [ ] **T017** — Mesmo arquivo: caminho feliz — grava pedido, itens e pagamento, esvazia o carrinho e chama `SalvarAlteracoes` **uma vez só** (RF-20, RN-07); **o preço gravado é o de agora, não o exibido** (RF-19, CA-12).
- [ ] **T018** — Confirmar que T014–T017 falham por o serviço não existir — e não por erro alheio.
- [ ] **T019** — `DocesCabana.Application`: `Enums/PassoDoFechamento.cs`, `DTOs/FechamentoDePedidoDTO.cs`, `DTOs/ResultadoDoFechamentoDTO.cs`, `DTOs/ConfirmacaoDePedidoDTO.cs`, `DTOs/PassoDoFechamentoDTO.cs`, `Mappings/PedidoMapper.cs`.
- [ ] **T020** — `DocesCabana.Application/Validators/FechamentoDePedidoDTOValidator.cs`: endereço, serviço e forma de pagamento obrigatórios.
- [ ] **T021** — `DocesCabana.Application/Contracts/Services/IPedidoService.cs` e `Services/PedidoService.cs`: o algoritmo dos nove passos do plano §4, **nesta ordem**. Nenhuma recusa lança exceção (Princípio VIII).
- [ ] **T022** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde.

## Fase 5 — A borda web

- [ ] **T023** — `DocesCabana.Tests/Units/Controllers/PedidoControllerTests.cs`: sucesso redireciona para a confirmação; recusa devolve a view com `ModelState` inválido; **confirmação de pedido alheio não é acessível** (RN-08). Ver falhar.
- [ ] **T024** — `DocesCabana.MVC/Controllers/PedidoController.cs`: `Fechar` (`[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Authorize]`, aguardado, redirecionando) e `Confirmacao` (GET, por identificador). **O redirecionamento é o que resolve o CA-14** — recarregar o comprovante é `GET`.
- [ ] **T025** `[P]` — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`: `passo` inválido cai no primeiro; **passo de conta não é oferecido a quem já entrou** (RF-03, CA-03). Ver falhar.
- [ ] **T026** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `Index` aceita `passo`, mantendo o caminho assíncrono da `017`/`021`.
- [ ] **T027** — Rodar `dotnet test DocesCabana.Tests`: Fase 5 verde.

## Fase 6 — As telas

- [ ] **T028** — `Views/Carrinho/_PassosDoFechamento.cshtml`: o indicador, com o passo ativo destacado (RF-01). **Some o passo de conta para quem já entrou** — e o indicador segue legível com três ou quatro passos.
- [ ] **T029** `[P]` — `Views/Carrinho/_PassoConta.cshtml`: entrar ou criar conta (RF-02). Depois de entrar, **voltar ao passo do carrinho** (RF-04) — a fusão de carrinhos da `017` soma quantidades, e a pessoa precisa ver o que ficou.
- [ ] **T030** — `Views/Carrinho/_PassoEndereco.cshtml`: lista de endereços com o principal marcado (RF-06); **sem nenhum, o formulário da `018` aparece ali mesmo** (RF-07) — reaproveitando `FormularioEndereco.cshtml` como parcial, não copiando.
- [ ] **T031** — Mesmo arquivo: as opções de entrega do endereço escolhido, com transportadora, serviço, preço e faixa de prazo (RF-08/RF-09).
- [ ] **T032** `[P]` — `Views/Carrinho/_PassoPagamento.cshtml`: as quatro formas, **sem coletar nenhum dado** (RF-10/RF-11), com o aviso de que o pagamento será combinado (RF-12).
- [ ] **T033** — `Views/Pedido/Confirmacao.cshtml`: número, itens, valores, prazo e o que acontece a seguir (RF-22). **Sem link para histórico de pedidos** — ele é a entrega seguinte e ainda não existe.
- [ ] **T034** `[P]` — `wwwroot/css/pages/carrinho.css` e o que for preciso para a confirmação.
- [ ] **T035** — `wwwroot/js/components/carrinho.js`: troca de passo sem recarga (RF-05). **Sem script, os passos são navegação `GET` comum** — e é assim que a T036 confere.
- [ ] **T036** — Rodar as duas suítes: Fase 6 verde.

## Fase 7 — Pedidos de demonstração

- [ ] **T037** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: semear pedidos fechados entre os oito clientes fictícios, com produtos e quantidades variados, **situações variadas** (pendente, confirmado, enviado, entregue) e **um pedido cancelado** para o CA-22 ter o que provar (RF-27). As situações variadas representam compras passadas — pedido criado pela aplicação nasce e fica pendente até o gateway existir (spec §10). Criados pelos construtores da aplicação, **não por SQL solto** — é o que garante coerência entre pedido, item e pagamento.
- [ ] **T038** — Conferir na base semeada: os pedidos existem, têm itens, e as quantidades vendidas diferem entre produtos o bastante para uma ordenação ser visível.

## Fase 8 — A vitrine passa a mais vendidos

- [ ] **T039** — `DocesCabana.Tests/Integration/Repositories/`: ordenação por venda — mais vendido primeiro; produto sem venda por último, **não ausente**; **pedido cancelado não conta** (RN-05, CA-22). Ver falhar. É subconsulta traduzida a SQL: teste de unidade não a exercita.
- [ ] **T040** — `DocesCabana.Infrastructure/Repositories/ProdutoRepository.cs`: o ramo `MaisVendidos` de verdade (plano §5), com `(int?)` e `?? 0` para produto sem venda vir por último — mesma forma que `MelhorAvaliados` usa desde a `014`.
- [ ] **T041** — **Reescrever** o teste localizado em T003 em `CatalogoControllerTests`: `MaisVendidos` passa a ser executada, não saneada (RF-26, CA-21). Correção esperada, não regressão.
- [ ] **T042** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: **remover** `SanearOrdenacao`. Existia só para recusar esta ordenação enquanto ela não tinha sentido.
- [ ] **T043** — `DocesCabana.MVC/Controllers/HomeController.cs` e `Application/Services/ProdutoService.cs`: a vitrine passa a pedir ordenação por venda (RF-24).
- [ ] **T044** — `DocesCabana.MVC/Views/Home/Index.cshtml`: título vira **"Mais vendidos"** (RF-25).
- [ ] **T045** — **Reescrever** o outro teste de T003, em `PaginaInicialTests`: o título passa a dizer "Mais vendidos" (CA-20).
- [ ] **T046** — Rodar as duas suítes: Fase 8 verde.

## Fase 9 — Prova de ponta a ponta

- [ ] **T047** — `DocesCabana.Tests.E2E/Fluxos/FechamentoTests.cs` e `Paginas/PaginaFechamento.cs`: a jornada completa — carrinho, endereço, entrega, pagamento, confirmar, comprovante (CA-01, CA-05, CA-07, CA-10, CA-11, CA-15). Ver falhar.
- [ ] **T048** `[P]` — Mesmos arquivos: visitante encontra o passo de entrar (CA-02); entrar devolve ao passo do carrinho (CA-04); sem endereço, cadastra no próprio passo (CA-06); trocar endereço troca as opções (CA-08).
- [ ] **T049** `[P]` — Mesmos arquivos: o carrinho fica vazio depois (CA-13); **recarregar a confirmação não cria segundo pedido** (CA-14); nenhum dado de pagamento é pedido (CA-09).
- [ ] **T050** — Mesmos arquivos: **sem JavaScript, a jornada inteira funciona** (CA-23). É o teste mais frágil da entrega e o que mais prova o desenho.
- [ ] **T051** — Rodar as duas suítes: Fase 9 verde.

## Fase 10 — Fechamento

- [ ] **T052** — `docs/arquitetura.md` §5: as rotas de fechamento e confirmação.
- [ ] **T053** `[P]` — `docs/arquitetura.md` §6: seção nova com o algoritmo das nove etapas e por que a conferência usa alegação em vez de confiar no formulário.
- [ ] **T054** — `docs/arquitetura.md` §9.3: `Pedido`, `ItemPedido` e `Pagamento` **saem** da lista de tabelas sem comportamento. Restam duas: `Estoque` e `Promocao`.
- [ ] **T055** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"` na base inteira.
- [ ] **T056** — `specs/README.md`: a linha da feature e a cadeia.
- [ ] **T057** — `specs/000-baseline/spec.md`: riscar as dívidas que esta entrega resolve, se houver.
- [ ] **T058** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero.
- [ ] **T059** — Subir a aplicação e percorrer uma compra à mão, do catálogo ao comprovante.
- [ ] **T060** — Preencher `checklist.md`.
- [ ] **T061** — Atualizar o status da spec e do plano, e a linha em `specs/README.md`. Registrar o que **não** foi encerrado: nada é cobrado, estoque não baixa, e o histórico de pedidos é a entrega seguinte.

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
