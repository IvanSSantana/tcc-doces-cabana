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

- [ ] **T001** — Criar branch `023-meus-pedidos` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build` e as duas suítes; registrar o estado inicial.
- [ ] **T003** — Conferir que a semeadura de pedidos da entrega anterior **não usa o cliente 8**, reservado desde a entrega de favoritos aos testes de lista vazia. Se usar, corrigir lá — é ele que dá o cenário do CA-04.

## Fase 2 — Consultar

- [ ] **T004** — `DocesCabana.Tests/Units/Services/PedidoServiceTests.cs`: a lista devolve só os pedidos do usuário, do mais recente ao mais antigo (RF-03, CA-03); usuário sem pedido devolve lista vazia, não erro (CA-04). Ver falhar.
- [ ] **T005** `[P]` — Mesmo arquivo: **detalhe de pedido alheio lança `KeyNotFoundException`, igual a pedido inexistente** (RN-01, CA-07/CA-08). Os dois casos respondendo igual é deliberado — distinguir contaria a quem sonda que aquele pedido existe.
- [ ] **T006** `[P]` — Mesmo arquivo: o detalhe traz **o preço gravado no item, não o do produto hoje** (RN-02, CA-06). É o teste que prova o congelamento do fechamento.
- [ ] **T007** — Confirmar que T004–T006 falham por os métodos não existirem.
- [ ] **T008** — `DocesCabana.Application`: `DTOs/ResumoDePedidoDTO.cs`, `DTOs/DetalheDePedidoDTO.cs`, as traduções em `Mappings/PedidoMapper.cs`, e `ListarDoUsuario` / `BuscarDetalhe` em `IPedidoService` e `PedidoService`.
- [ ] **T009** — `DocesCabana.Infrastructure/Repositories/PedidoRepository.cs`: as consultas trazem itens **com produto** e o endereço de entrega, com `Include`. **Nenhum método novo** além do que o fechamento já criou.
- [ ] **T010** — `DocesCabana.Tests/Integration/Repositories/`: o detalhe vem numa consulta só, com itens e endereço — não uma consulta por item.
- [ ] **T011** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde.

## Fase 3 — A borda web

- [ ] **T012** — `DocesCabana.Tests/Units/Controllers/PedidoControllerTests.cs`: `Meus` devolve a view com os resumos; lista vazia devolve a view mesmo assim; `Detalhe` devolve a view. Ver falhar.
- [ ] **T013** — `DocesCabana.MVC/Controllers/PedidoController.cs`: `Meus` e `Detalhe`. **`[Authorize]` na classe**, não em cada ação — ação nova não nasce desprotegida por esquecimento (Princípio VII).
- [ ] **T014** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — As telas

- [ ] **T015** — `DocesCabana.MVC/Views/Pedido/Meus.cshtml`: um cartão por pedido, com número, data, situação, quantidade de itens e total (RF-02); mais recentes primeiro; vazio explica e oferece o catálogo (RF-05).
- [ ] **T016** — `DocesCabana.MVC/Views/Pedido/Detalhe.cshtml`: número, data, situação; itens com nome, quantidade e **preço da compra** (RF-07); endereço, transportadora, serviço e prazo como estavam (RF-08); produtos, entrega e total (RF-09); forma e situação do pagamento (RF-10).
- [ ] **T017** `[P]` — `DocesCabana.MVC/wwwroot/css/pages/pedidos.css`: seguindo o desenho das telas de conta. Situação como etiqueta, com **vocabulário do cliente** — "Aguardando pagamento", não o nome do enumerado. **Cancelado esmaecido, não vermelho**: é desfecho, não falha.
- [ ] **T018** — `DocesCabana.MVC/Views/Conta/_MenuDaConta.cshtml`: o atalho reservado desde a entrega de conta deixa de estar desabilitado (RF-01, CA-01).
- [ ] **T019** — Rodar as duas suítes: Fase 4 verde.

## Fase 5 — Prova de ponta a ponta

- [ ] **T020** — `DocesCabana.Tests.E2E/Fluxos/MeusPedidosTests.cs` e `Paginas/PaginaMeusPedidos.cs`: atalho habilitado (CA-01); a lista mostra os pedidos semeados, **com situações diferentes entre si** (CA-02/CA-03); abrir um leva ao detalhe com itens e valores (CA-05). Ver falhar.
- [ ] **T021** `[P]` — Mesmos arquivos: visitante é levado a entrar (CA-09); **o cliente 8, sem pedido nenhum, vê a tela vazia com caminho para o catálogo** (CA-04).
- [ ] **T022** — Rodar as duas suítes: Fase 5 verde.

## Fase 6 — Fechamento

- [ ] **T023** — `docs/arquitetura.md` §5: as duas rotas novas.
- [ ] **T024** `[P]` — `docs/arquitetura.md`: registrar o padrão de proteção por assinatura de repositório, agora aplicado duas vezes — endereço e pedido. É desenho reutilizável, não detalhe de uma feature.
- [ ] **T025** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"` na base inteira.
- [ ] **T026** — `specs/README.md`: a linha da feature e a cadeia.
- [ ] **T027** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero.
- [ ] **T028** — Subir a aplicação, comprar à mão e conferir que o pedido aparece na lista e no detalhe com o que foi comprado.
- [ ] **T029** — Preencher `checklist.md`.
- [ ] **T030** — Atualizar o status da spec e do plano, e a linha em `specs/README.md`. Registrar o que **não** foi encerrado: a situação do pedido só passa a variar de verdade quando a processadora de pagamento existir.

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
