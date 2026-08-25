# Tarefas — Redesenho do carrinho

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`… — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Uma regra não é negociável nesta entrega.**
>
> **O `#itens-carrinho` continua sendo a raiz única do bloco trocado.** As duas
> colunas saem de `display: grid` sobre ele. Separar o resumo num container
> irmão faria a troca sem recarga deixar de ser atômica — por um instante a tela
> mostraria itens novos com subtotal velho. Se em algum momento parecer mais
> fácil separar, **pare e releia o §1 do plano**: é a decisão que mantém
> `carrinho.js` intocado.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `021-redesenho-do-carrinho` a partir de `main`. *(feita ao criar a pasta da spec)*
- [ ] **T002** — Rodar `dotnet build` e as duas suítes; registrar o estado inicial. Anotar em especial os 19 testes E2E de carrinho e os 41 unitários — **é contra eles que o redesenho se mede**.

## Fase 2 — Esvaziar o carrinho

- [ ] **T003** — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`: `Esvaziar` remove todos os itens do usuário e chama `SalvarAlteracoes` **uma vez só** (RF-10); carrinho já vazio não quebra. Ver falhar.
- [ ] **T004** — Confirmar que T003 falha por o método não existir — e não por erro alheio.
- [ ] **T005** — `DocesCabana.Application/Contracts/Services/ICarrinhoService.cs` e `Services/CarrinhoService.cs`: `Esvaziar(Guid usuarioId)`, com o laço sobre `BuscarPorUsuario` (plano §4). **Sem método de repositório novo, e sem `ExecuteDeleteAsync`** — ele grava fora do `IUnitOfWork`, contra o Princípio VI.
- [ ] **T006** `[P]` — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`: `Esvaziar` chama o serviço e redireciona (Princípio VII); **visitante limpa a sessão em vez do banco**, reaproveitando `CarrinhoDaSessao.Limpar` que a `017` já criou; `ConfirmarEsvaziar` devolve a view. Ver falhar.
- [ ] **T007** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `ConfirmarEsvaziar` (GET) e `Esvaziar` (POST, `[ValidateAntiForgeryToken]`, aguardado, redirecionando).
- [ ] **T008** — `DocesCabana.MVC/Views/Carrinho/_ConfirmarEsvaziar.cshtml`: a pergunta, com confirmar e desistir. É o caminho **sem JavaScript** da RF-11 (plano §5).
- [ ] **T009** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde.

## Fase 3 — O resumo do pedido

- [ ] **T010** — `DocesCabana.Tests/Units/Mappings/CarrinhoMapperTests.cs`: sem cotação, o valor em destaque é o subtotal e não inclui entrega (CA-04); com cotação injetada, o destaque é o total a pagar e **inclui** a entrega (CA-05). Ver falhar.
- [ ] **T011** — `DocesCabana.Application/DTOs/CarrinhoDTO.cs`: `Cotacao` anulável. **Conferir antes se a entrega de cotação de frete já o criou** — o plano §6 registra que as duas entregas o preveem, e quem chegar em segundo lugar confere em vez de duplicar.
- [ ] **T012** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — A tela

- [ ] **T013** — `DocesCabana.MVC/Views/Carrinho/_ItensDoCarrinho.cshtml`: reescrever os itens como cartões — miniatura, nome, e os três blocos rotulados de preço unitário, quantidade e subtotal (RF-01/RF-02). **Rótulos repetidos em cada cartão** (plano §3). Manter os controles de quantidade e o remover exatamente como estão por dentro.
- [ ] **T014** — **No mesmo arquivo:** o resumo vira coluna — cupom desabilitado com explicação (RF-08), contagem de produtos, linha de entrega, valor em destaque com o rótulo que troca (RF-05 a RF-07), e o botão de finalizar desabilitado com explicação (RF-09). **A raiz continua sendo o `#itens-carrinho`** — ver o aviso no topo deste arquivo.
- [ ] **T015** — **No mesmo arquivo:** "Esvaziar Carrinho" à esquerda e "Continuar Comprando →" à direita (RF-10/RF-12); o estado vazio segue oferecendo o catálogo (RF-15).
- [ ] **T016** — `DocesCabana.MVC/wwwroot/css/pages/carrinho.css`: reescrever. As duas colunas por `display: grid` sobre o `#itens-carrinho`; empilhamento a 375px (RF-14). Cores só das variáveis que o projeto já define.
- [ ] **T017** — `DocesCabana.MVC/wwwroot/js/components/carrinho.js`: **acrescentar** o diálogo de confirmação, interceptando o link de esvaziar e enviando ao mesmo POST (plano §5). **Não tocar na troca sem recarga** — se ela precisar mudar, a raiz do parcial foi alterada e a T014 saiu do desenho.
- [ ] **T018** — `DocesCabana.Tests.E2E/Paginas/PaginaCarrinho.cs`: atualizar os onze seletores para o desenho novo, e acrescentar os de esvaziar, confirmar e continuar comprando. **É o único arquivo de teste E2E que o redesenho deveria tocar** — se algum dos 19 testes precisar de edição, o objeto de página não está cobrindo o que deveria.
- [ ] **T019** — Rodar as duas suítes. Os 19 testes da `017` devem passar **sem terem sido editados**.

## Fase 5 — Provar o desenho

- [ ] **T020** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs`: cartão com os cinco elementos (CA-01); cupom e finalizar desabilitados e explicados (CA-06/CA-07); rótulo em destaque diz subtotal sem entrega calculada (CA-04). Ver falhar antes da Fase 4, ou confirmar que passam depois dela.
- [ ] **T021** `[P]` — Mesmos arquivos: esvaziar pede confirmação (CA-08); confirmar esvazia e oferece o catálogo (CA-09); desistir não remove nada (CA-10); voltar ao catálogo preserva o carrinho (CA-11).
- [ ] **T022** `[P]` — Mesmos arquivos: sem JavaScript, alterar quantidade, remover e esvaziar funcionam (CA-12); a 375px o resumo empilha abaixo dos itens sem rolagem horizontal (CA-13) — medindo o conteúdo, não o documento, como a `013` fez, porque o estouro do cabeçalho é dívida herdada.
- [ ] **T023** — Rodar as duas suítes: Fase 5 verde.

## Fase 6 — Fechamento

- [ ] **T024** — `docs/arquitetura.md` §5: a linha do carrinho passa a descrever as duas colunas e as ações de esvaziar e continuar comprando.
- [ ] **T025** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"` na base. A segunda varredura é a lição da `019`: a referência obsoleta do botão de finalizar escapou por não conter a palavra "spec". **Conferir em especial o título desse botão**, que agora precisa apontar para a entrega de fechamento.
- [ ] **T026** — `specs/README.md`: registrar a decomposição — `021` redesenho do carrinho, `022` fechamento, `023` meus pedidos, `024` features, `025` estoque; a nota de numeração registra o oitavo deslocamento e o motivo.
- [ ] **T027** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero.
- [ ] **T028** — Subir a aplicação e conferir ao vivo o que teste alcança mal: o cartão contra o protótipo, o alinhamento das duas colunas, e o diálogo de confirmação.
- [ ] **T029** — Preencher `checklist.md`, registrando **o que foi provado por teste e o que só a verificação ao vivo mostrou**.
- [ ] **T030** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha em `specs/README.md`. Registrar o que **não** foi encerrado: o valor da entrega segue ausente até a cotação de frete existir, e o CA-05 só ganha prova de ponta a ponta lá.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T013, T020 |
| RF-02 | T013, T019 |
| RF-03 | T014, T016 |
| RF-04 | T014 |
| RF-05 | T014 |
| RF-06 | T010, T014, T020 |
| RF-07 | T010, T014 |
| RF-08 | T014, T020 |
| RF-09 | T014, T020 |
| RF-10 | T003, T005, T015 |
| RF-11 | T006, T007, T008, T017, T021 |
| RF-12 | T015, T021 |
| RF-13 | T008, T022 |
| RF-14 | T016, T022 |
| RF-15 | T015, T021 |
| RN-01 | T014, T020 |
| RN-02 | T010, T014 |
| RN-03 | T008, T017, T021 |
| RN-04 | T008, T022 |
| RN-05 | T013, T019 |
| CA-01 | T013, T020 |
| CA-02 | T019 |
| CA-03 | T014 |
| CA-04 | T010, T020 |
| CA-05 | T010 |
| CA-06 | T014, T020 |
| CA-07 | T014, T020 |
| CA-08 | T008, T021 |
| CA-09 | T005, T021 |
| CA-10 | T008, T021 |
| CA-11 | T015, T021 |
| CA-12 | T022 |
| CA-13 | T016, T022 |
