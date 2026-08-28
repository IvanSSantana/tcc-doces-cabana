# Tarefas — Meus pedidos

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`… — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Uma coisa não é negociável.**
>
> **Nenhum método novo de repositório que busque pedido só pelo identificador.**
> O fechamento já entrega `Buscar(pedidoId, usuarioId)`, e esta entrega usa só
> ele. É o mesmo desenho que a `018` aplicou a endereço: sem o método errado
> disponível, a RN-01 não pode ser violada por esquecimento. Se em algum momento
> parecer mais prático acrescentar `BuscarPorId(pedidoId)`, **pare e releia o §1
> do plano**.
>
> **Esta entrega depende da anterior.** Sem os pedidos que o fechamento cria e
> semeia, não há o que listar.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `023-meus-pedidos` a partir de `main`.
- [x] **T002** — Build limpo (só o aviso pré-existente do pacote SQLite). Estado inicial: `DocesCabana.Tests` 648/648, `DocesCabana.Tests.E2E` 179/180 (1 falha isolada em `BuscaTests`, instabilidade pré-existente e alheia a esta entrega, já registrada nas specs `020`/`022`).
- [x] **T003** — Conferido em `DbInitializer.SemearPedidosDeExemplo`: usa `usuarioIds[0]` a `usuarioIds[6 % usuarioIds.Count]` (sete pedidos, clientes 1 a 7). `usuarioIds[7]` (cliente 8) nunca é referenciado — segue reservado para a lista vazia, nada para corrigir.

## Fase 2 — Consultar

> **⚠️ Correção ao plano, registrada ao executar.** O plano (§1/§4) e o
> `tasks.md` (nota "não negociável" acima) partiam da premissa de que
> `IPedidoRepository` já tinha `Buscar(pedidoId, usuarioId)` desde a spec
> `022`. Não tinha: o fechamento criou `BuscarPorIdComItens(pedidoId)` —
> um único parâmetro, sem `usuarioId` — e a checagem de dono acontecia
> manualmente em `PedidoService.ObterConfirmacao` (`pedido.UsuarioId !=
> usuarioId`), exatamente o padrão que a `018`/`022` já tinham decidido
> evitar. Corrigido: `BuscarPorIdComItens` foi **substituído** por
> `Buscar(pedidoId, usuarioId)` (mesma assinatura estruturalmente segura de
> `IEnderecoRepository.Buscar`), com o `WHERE` filtrando os dois campos na
> própria consulta — não um método novo além do que o fechamento criou, e
> sim a correção do que ele deveria ter criado. `ObterConfirmacao`
> (spec 022) foi ajustado para usar o novo método, perdendo a checagem
> manual que se tornou redundante. Nenhum método que busque pedido só pelo
> identificador foi introduzido — a regra "não negociável" continua valendo.

- [x] **T004** — `DocesCabana.Tests/Units/Services/PedidoServiceTests.cs`: a lista devolve só os pedidos do usuário, do mais recente ao mais antigo (RF-03, CA-03); usuário sem pedido devolve lista vazia, não erro (CA-04). Visto falhar por `CS1061` (`ListarDoUsuario`/`BuscarDetalhe`/`Buscar` inexistentes).
- [x] **T005** `[P]` — Mesmo arquivo: **detalhe de pedido alheio lança `KeyNotFoundException`, igual a pedido inexistente** (RN-01, CA-07/CA-08). Os dois casos respondendo igual é deliberado — distinguir contaria a quem sonda que aquele pedido existe.
- [x] **T006** `[P]` — Mesmo arquivo: o detalhe traz **o preço gravado no item, não o do produto hoje** (RN-02, CA-06). É o teste que prova o congelamento do fechamento.
- [x] **T007** — Confirmado: T004–T006 falharam por os métodos não existirem (compilação), não por erro alheio.
- [x] **T008** — `DocesCabana.Application`: `DTOs/ResumoDePedidoDTO.cs`, `DTOs/DetalheDePedidoDTO.cs`, as traduções (`ToResumoDTO`/`ToDetalheDTO`) em `Mappings/PedidoMapper.cs`, e `ListarDoUsuario`/`BuscarDetalhe` em `IPedidoService`/`PedidoService`. `ListarDoUsuario` ordena por `Data` descendente no serviço (regra de negócio, não deixada para a ordem que o repositório happens to devolver).
- [x] **T009** — `DocesCabana.Infrastructure/Repositories/PedidoRepository.cs`: `Buscar(pedidoId, usuarioId)` (substitui `BuscarPorIdComItens`, ver correção acima) traz itens **com produto** e o endereço de entrega, com `Include`; `ListarPorUsuario` passa a incluir os itens (para a quantidade do resumo). **Nenhum método que busque pedido só pelo identificador.**
- [x] **T010** — `DocesCabana.Tests/Integration/Repositories/PedidoRepositoryIntegrationTests.cs`: o detalhe vem numa consulta só, com itens, produto de cada item e endereço — não uma consulta por item. Acrescentado também: `Buscar` de pedido de outro usuário devolve nulo (RN-01/CA-07 — a mesma prova que `EnderecoRepositoryIntegrationTests` já fazia para endereço).
- [x] **T011** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde (654/654).

## Fase 3 — A borda web

- [x] **T012** — `DocesCabana.Tests/Units/Controllers/PedidoControllerTests.cs`: `Meus` devolve a view com os resumos; lista vazia devolve a view mesmo assim; `Detalhe` devolve a view. Visto falhar por `CS1061` (ações inexistentes).
- [x] **T013** — `DocesCabana.MVC/Controllers/PedidoController.cs`: `Meus` e `Detalhe`. `[Authorize]` já estava na classe desde a `022` — nenhuma ação nasce desprotegida.
- [x] **T014** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde (657/657).

## Fase 4 — As telas

- [x] **T015** — `DocesCabana.MVC/Views/Pedido/Meus.cshtml`: um cartão por pedido, com número, data, situação, quantidade de itens e total (RF-02); mais recentes primeiro (ordem já vem de `PedidoService.ListarDoUsuario`); vazio explica e oferece o catálogo (RF-05).
- [x] **T016** — `DocesCabana.MVC/Views/Pedido/Detalhe.cshtml`: número, data, situação; itens com nome, quantidade e **preço da compra** (RF-07); endereço, transportadora, serviço e prazo como estavam (RF-08); produtos, entrega e total (RF-09); forma e situação do pagamento (RF-10).
- [x] **T017** `[P]` — `DocesCabana.MVC/wwwroot/css/pages/pedidos.css`: seguindo o desenho das telas de conta. Situação como etiqueta, com **vocabulário do cliente** — "Aguardando pagamento", não o nome do enumerado. **Cancelado esmaecido (cinza), não vermelho**: é desfecho, não falha.
- [x] **T018** — `DocesCabana.MVC/Views/Conta/_MenuDaConta.cshtml`: o atalho reservado desde a `018` deixa de estar desabilitado (RF-01, CA-01). **Achado ao implementar, corrigido:** os dois links existentes ("Dados pessoais", "Endereços") usavam `asp-action` sem `asp-controller` — funcionava enquanto o parcial só era incluído por `ContaController`; incluído agora também por `PedidoController`, resolveriam para lá. Os três links do menu ganharam `asp-controller` explícito.
- [x] **T019** — Rodar as duas suítes: Fase 4 verde (`DocesCabana.Tests` 657/657; `DocesCabana.Tests.E2E`/`ContaTests` 21/21, smoke-test do menu corrigido).

## Fase 5 — Prova de ponta a ponta

- [x] **T020** — `DocesCabana.Tests.E2E/Fluxos/MeusPedidosTests.cs` e `Paginas/PaginaMeusPedidos.cs`: atalho habilitado (CA-01); a lista mostra os pedidos semeados, **com situações diferentes entre si** (CA-02/CA-03); abrir um leva ao detalhe com itens e valores (CA-05). **Achado ao escrever, corrigido:** `DbInitializer.SemearPedidosDeExemplo` dava exatamente um pedido por cliente — nenhum cliente tinha mais de um, então CA-03 ("mais recente primeiro") não tinha o que provar de verdade. O primeiro cliente (`EmailClienteSeed`) passou a ganhar dois pedidos, com datas explicitamente diferentes (30 dias atrás e agora), só para isso — os demais seguem com um cada.
- [x] **T021** `[P]` — Mesmos arquivos: visitante é levado a entrar (CA-09); **o cliente 8, sem pedido nenhum, vê a tela vazia com caminho para o catálogo** (CA-04).

  **Bug real encontrado ao rodar, corrigido:** `<partial name="_MenuDaConta" />` não resolvia quando renderizada a partir de `PedidoController` — o Razor só busca parciais na pasta do controlador atual (`Views/Pedido/`) e em `Views/Shared/`, e `_MenuDaConta.cshtml` vivia em `Views/Conta/`. Página quebrava com `InvalidOperationException` (erro 500) em toda tela nova desta entrega. Corrigido movendo o arquivo para `Views/Shared/` — é exatamente o que o Princípio IV manda para o que é reaproveitado por mais de uma página, e agora é (`Conta` e `Pedido`). Os dois links existentes no menu também precisaram de `asp-controller="Conta"` explícito (T018), pela mesma raiz.
- [x] **T022** — Rodar as duas suítes: Fase 5 verde (`DocesCabana.Tests` 657/657; `DocesCabana.Tests.E2E` 185/185).

## Fase 6 — Fechamento

- [x] **T023** — `docs/arquitetura.md` §5: linha do `/Pedido` expandida com `Meus`/`Detalhe`.
- [x] **T024** `[P]` — `docs/arquitetura.md` nova §6.12: o padrão de proteção por assinatura de repositório, agora aplicado duas vezes (endereço, `018`; pedido, `023`); a tradução de situação na view, não na entidade; e o achado de `_MenuDaConta.cshtml` precisar viver em `Views/Shared/`.
- [x] **T025** — Varredura feita. Nenhuma referência obsoleta encontrada — "Meus pedidos" não aparece mais como "ainda não disponível"/atalho reservado em código nenhum.
- [x] **T026** — `specs/README.md`: status de `023` → *Implementada* nas duas tabelas, com link de `checklist`; nota de "Ordem executada" com `023` ao final, registrando que esta é a única entrega de leitura pura da cadeia e por isso não herda a pendência de credencial das duas anteriores.
- [x] **T027** — `dotnet build` sem aviso novo; `DocesCabana.Tests` 657/657 e `DocesCabana.Tests.E2E` 185/185, do zero.
- [x] **T028** — **Não pôde ser percorrido comprando de verdade** — fechar um pedido depende da mesma credencial do MelhorEnvio que a `022` já registrou como bloqueada (spec `023` não depende disso para ler, mas não há pedido novo para criar sem passar pelo fechamento). Verificado o equivalente possível: a suíte E2E (`MeusPedidosTests`) abre de verdade, em Chromium real, a lista e o detalhe de pedidos **semeados**, confirmando que aparecem com o que foi "comprado" (itens, valores, endereço, transportadora, prazo, pagamento) exatamente como a semeadura gravou.
- [x] **T029** — Preencher `checklist.md`.
- [x] **T030** — Atualizar o status da spec e do plano, e a linha em `specs/README.md` (T026). Registrar o que **não** foi encerrado: a situação do pedido só passa a variar de verdade quando a processadora de pagamento existir; T028 (compra manual de ponta a ponta) segue bloqueado pela mesma credencial que a `022` já bloqueava.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T018, T020 |
| RF-02 | T015, T020 |
| RF-03 | T004, T015 |
| RF-04 | T015, T020 |
| RF-05 | T015, T021 |
| RF-06 | T016 |
| RF-07 | T006, T016 |
| RF-08 | T009, T016 |
| RF-09 | T016 |
| RF-10 | T016 |
| RF-11 | T005, T013 |
| RF-12 | T005 |
| RN-01 | T005, T009 |
| RN-02 | T006 |
| RN-03 | T013 |
| CA-01 | T018, T020 |
| CA-02 | T015, T020 |
| CA-03 | T004, T020 |
| CA-04 | T003, T015, T021 |
| CA-05 | T016, T020 |
| CA-06 | T006 |
| CA-07 | T005 |
| CA-08 | T005 |
| CA-09 | T013, T021 |
