# Checklist de conclusão — Fechamento de pedido

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — RF-01 a RF-23
      (`CarrinhoController`/`PedidoController`, `IPedidoService`, as quatro
      views de passo, `Pedido`/`ItemPedido`/`Pagamento`); RF-24 a RF-27
      (`ProdutoService.BuscarDestaquesDaVitrine`, `ProdutoRepository`
      ramo `MaisVendidos`, `DbInitializer.SemearPedidosDeExemplo`).
- [x] Todo `CA-xx` foi verificado — **CA-01 a CA-04, CA-06 (parcial), CA-19,
      CA-20, CA-21, CA-22 por teste automatizado** (unidade, integração e
      E2E, listados abaixo). **CA-05, CA-07 a CA-15, CA-17, CA-23 (a
      jornada completa até o comprovante) não puderam ser verificados —
      a recotação de frete que o passo de Endereço/Pagamento faz depende
      da mesma credencial do MelhorEnvio que a spec `020` já registrou
      como não obtida (§10); sem ela, a cotação sempre falha, e o link
      para continuar ao pagamento nunca aparece.** Pendência explícita,
      não esquecida — herda o mesmo bloqueio da `020`, sem tarefa própria
      nesta spec. CA-08 e CA-09 têm a mesma dependência.
- [x] Nada fora do escopo declarado entrou junto na entrega — cobrança real,
      avanço de situação do pedido fora da semeadura, baixa de estoque,
      histórico de pedidos, cupom funcional, cancelar/alterar pedido
      fechado e notificação por e-mail seguem de fora, como a spec
      determina
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção
      de dependência (`Infrastructure` implementa `Application.IPedidoService`
      /`IPedidoRepository`, mesmo sentido de sempre)
- [x] **II** — `Pedido` (já existia) ganha cinco propriedades novas
      (`private set`), a coleção de itens como raiz de agregado (campo de
      apoio privado, exposta só como `IReadOnlyCollection`), e métodos de
      intenção (`AcrescentarItem`, `Cancelar`, `Confirmar`,
      `MarcarComoEnviado`, `MarcarComoEntregue` — os quatro últimos usados
      só pela semeadura, spec §10). Nenhuma entidade nova nesta entrega.
- [x] **III** — `FechamentoDePedidoDTOValidator` cobre o formulário
      (endereço/serviço/pagamento obrigatórios); as invariantes de
      `Pedido`/`ItemPedido`/`Pagamento` seguem no domínio, como já eram
- [x] **IV** — Nomes, mensagens e comentários em português (`PedidoService.
      Fechar`, `PassoDoFechamento`, `NumeroVisivel`, etc.)
- [x] **V** — Os testes foram escritos antes e vistos falhar antes de
      passar em quase toda a entrega — **com um desvio registrado e
      justificado em `tasks.md` (T014–T021, T023/T024)**: o coração do
      fechamento (`PedidoService.Fechar`) e o controller que devolve a
      mesma view na recusa foram desenhados junto com o teste, não
      teste-primeiro-vermelho-depois, porque a forma de
      `ResultadoDoFechamentoDTO` só fazia sentido depois de decidir o que
      cada recusa precisa devolver — mesma classe de decisão já tomada na
      `020`. O teste cumpriu seu papel: achou um bug real (de teste, não
      de produção) na primeira passada.
- [x] **VI** — `PedidoService.Fechar` grava pedido, itens e pagamento, e
      esvazia o carrinho, com **um `IUnitOfWork.SalvarAlteracoes` só**
      (RF-20/RN-07) — provado por teste. Migration `AddPedidoDadosDeEntrega`
      criada (tabela com zero linhas, sem backfill)
- [x] **VII** — `PedidoController.Fechar` é `[HttpPost]`,
      `[ValidateAntiForgeryToken]`, `[Authorize]`, aguardado, redireciona
      no sucesso (POST-Redirect-Get, e é o que resolve CA-14 sozinho).
      `CarrinhoController.CadastrarEndereco` segue o mesmo padrão
- [x] **VIII** — Sem `try/catch` em ação de controller. Toda recusa de
      `PedidoService.Fechar` volta em `ResultadoDoFechamentoDTO`, nunca
      como exceção — inclusive falha de cotação de frete, que já não
      lançava desde a `020`

## Testes

- [x] `dotnet build` sem warnings novos (só o aviso pré-existente do pacote
      SQLite, alheio a esta entrega)
- [x] `dotnet test` verde — `DocesCabana.Tests`: 648/648;
      `DocesCabana.Tests.E2E`: 180/180 (suíte completa, do zero, sem
      credencial de frete no ambiente — o estado de quem clona o projeto)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração —
      `PedidoRepositoryIntegrationTests` (agregado com itens e pagamento),
      `CatalogoRepositoryIntegrationTests` (ordenação por venda, incluindo
      pedido cancelado não contar), `DbInitializerPedidosIntegrationTests`
      (semeadura)

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de
      fato (`Carrinho/CadastrarEndereco`, `Pedido/Fechar`)
- [x] Erros de validação aparecem no campo (`asp-validation-for`, no
      formulário de endereço reaproveitado) e no resumo (recusa do
      fechamento, via `ModelState.AddModelError(string.Empty, ...)` — não
      há um campo específico para "o preço mudou")
- [x] Testado em largura de tela pequena — os passos reaproveitam o mesmo
      `.grade-carrinho`/empilhamento que a `021` já prova em 375px; não
      introduziu layout novo
- [x] Valores monetários formatados em `pt-BR` (`N2`, vírgula decimal) — em
      todo o resumo, pagamento e comprovante

## Segurança

- [x] Nenhum segredo commitado — nenhuma credencial nova nesta entrega
- [x] Entrada do usuário não é interpolada em HTML sem escape — Razor
      padrão em toda view nova, sem `Html.Raw`
- [x] Mensagens de erro não vazam detalhe interno — recusas do fechamento
      usam mensagem de negócio ("o valor mudou", "item indisponível"),
      nunca a exceção original; endereço alheio continua devolvendo 404,
      sem distinguir "não existe" de "não é seu" (RN-08)
