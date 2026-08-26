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

- [x] **T001** — Criar branch `021-redesenho-do-carrinho` a partir de `main`. *(recriada a partir da `main` pós-merge dos quatro desenhos, para a implementação)*
- [x] **T002** — Rodar `dotnet build` e as duas suítes; registrar o estado inicial. Build limpo; 552/552 unitários; E2E 161/162 (uma falha por timeout de navegação em `PaginasInstitucionaisTests`, instável e alheia a esta entrega).

## Fase 2 — Esvaziar o carrinho

- [x] **T003** — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`: `Esvaziar` remove todos os itens do usuário e chama `SalvarAlteracoes` **uma vez só** (RF-10); carrinho já vazio não quebra. Ver falhar.
- [x] **T004** — Confirmar que T003 falha por o método não existir — e não por erro alheio.
- [x] **T005** — `DocesCabana.Application/Contracts/Services/ICarrinhoService.cs` e `Services/CarrinhoService.cs`: `Esvaziar(Guid usuarioId)`, com o laço sobre `BuscarPorUsuario` (plano §4). **Sem método de repositório novo, e sem `ExecuteDeleteAsync`** — ele grava fora do `IUnitOfWork`, contra o Princípio VI.
- [x] **T006** `[P]` — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`: `Esvaziar` chama o serviço e redireciona (Princípio VII); **visitante limpa a sessão em vez do banco**, reaproveitando `CarrinhoDaSessao.Limpar` que a `017` já criou; `ConfirmarEsvaziar` devolve a view. Ver falhar.
- [x] **T007** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `ConfirmarEsvaziar` (GET) e `Esvaziar` (POST, `[ValidateAntiForgeryToken]`, aguardado, redirecionando).
- [x] **T008** — `DocesCabana.MVC/Views/Carrinho/ConfirmarEsvaziar.cshtml`: a pergunta, com confirmar e desistir. **Sem underscore** — diferente do previsto no plano: é página navegada de verdade (`View()`, sem `PartialView`), não uma partial incluída por outra view, e o ASP.NET Core resolve `View()` pelo nome exato da ação. É o caminho **sem JavaScript** da RF-11 (plano §5).
- [x] **T009** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde (557/557).

## Fase 3 — O resumo do pedido

- [x] **T010** — `DocesCabana.Tests/Units/Mappings/CarrinhoMapperTests.cs`: sem cotação, o valor em destaque é o subtotal e não inclui entrega (CA-04); com cotação injetada, o destaque é o total a pagar e **inclui** a entrega (CA-05). Ver falhar. **Achado ao implementar, resolvido com o responsável em vez de improvisado:** `CotacaoDeFreteDTO` devolve lista de opções, não uma única — decidido que a mais barata compõe o total (RN-06 nova, registrada em spec.md e plan.md §6). Terceiro teste acrescentado para o caso de cotação sem opções (serviço fora do ar).
- [x] **T011** — `DocesCabana.Application/DTOs/CarrinhoDTO.cs`: `Cotacao` anulável, `TemEntregaCalculada` e `ValorTotal` computados. **Nenhuma das duas entregas tinha criado ainda** — `020-dimensoes-e-frete` só tem spec/plano, zero código; `OpcaoDeFreteDTO`/`CotacaoDeFreteDTO` criados aqui, e a `020` deve conferir que já existem ao ser implementada, não recriar. `CarrinhoMapper.Montar`/`ToDTO` ganham parâmetro `cotacao` opcional com padrão `null` — nenhum chamador da `017` precisou mudar.
- [x] **T012** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde (560/560).

## Fase 4 — A tela

- [x] **T013** — `DocesCabana.MVC/Views/Carrinho/_ItensDoCarrinho.cshtml`: reescrever os itens como cartões — miniatura, nome, e os três blocos rotulados de preço unitário, quantidade e subtotal (RF-01/RF-02). **Rótulos repetidos em cada cartão** (plano §3). Manter os controles de quantidade e o remover exatamente como estão por dentro.
- [x] **T014** — **No mesmo arquivo:** o resumo vira coluna — cupom desabilitado com explicação (RF-08), contagem de produtos, linha de entrega, valor em destaque com o rótulo que troca (RF-05 a RF-07), e o botão de finalizar desabilitado com explicação (RF-09). **A raiz continua sendo o `#itens-carrinho`** — ver o aviso no topo deste arquivo. Comentário do botão de finalizar já corrigido aqui (apontava para a `019`, agora aponta para a `022`) — adiantado da T025, conferido de novo lá.
- [x] **T015** — **No mesmo arquivo:** "Esvaziar carrinho" à esquerda e "Continuar comprando →" à direita (RF-10/RF-12); o estado vazio segue oferecendo o catálogo (RF-15).
- [x] **T016** — `DocesCabana.MVC/wwwroot/css/pages/carrinho.css`: reescrever. As duas colunas por `display: grid` sobre o `#itens-carrinho`; empilhamento a 375px (RF-14). Cores só das variáveis que o projeto já define (`--cor-destaque`, `--cor-primaria`), com `color-mix` para a borda coral suavizada do cartão.
- [x] **T017** — `DocesCabana.MVC/wwwroot/js/components/carrinho.js`: **acrescentado** o diálogo de confirmação (`<dialog>` nativo), interceptando o link de esvaziar e enviando ao mesmo POST via o mecanismo genérico de submit já existente (plano §5). **A troca sem recarga não foi tocada** — mesma função `enviar`/`aplicarBloco`.
- [x] **T018** — `DocesCabana.Tests.E2E/Paginas/PaginaCarrinho.cs`: os onze seletores originais mantidos (nenhuma classe usada por eles mudou de nome); acrescentados os de cupom, esvaziar, continuar comprando e o diálogo. Criado também `PaginaConfirmarEsvaziarCarrinho.cs` para o caminho sem JavaScript.
- [x] **T019** — Rodar as duas suítes. **Os 19 testes E2E de carrinho da `017` passaram sem edição nenhuma** (560/560 unitários; 19/19 de `CarrinhoTests`) — só `PaginaCarrinho.cs` mudou, exatamente como o plano previa.

## Fase 5 — Provar o desenho

- [x] **T020** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs`: cartão com os cinco elementos (CA-01); cupom e finalizar desabilitados e explicados (CA-06/CA-07); rótulo em destaque diz subtotal sem entrega calculada (CA-04). Escritos depois da Fase 4 (a alternativa que a própria tarefa previa) — a Fase 2 já tinha provado `Esvaziar` no vermelho, então o risco de teste que passa por acaso estava coberto na camada certa.
- [x] **T021** `[P]` — Mesmos arquivos: esvaziar pede confirmação (CA-08); confirmar esvazia e oferece o catálogo (CA-09); desistir não remove nada (CA-10); voltar ao catálogo preserva o carrinho (CA-11).
- [x] **T022** `[P]` — Mesmos arquivos: sem JavaScript, esvaziar funciona pela página própria da RN-04, sem diálogo (CA-12 — a parte que a T003/T409 já existente não cobria); a 375px o resumo empilha abaixo dos itens sem rolagem horizontal (CA-13) — medindo o conteúdo, não o documento, como a `013` e a `020` já fizeram, porque o estouro do cabeçalho é dívida herdada.
- [x] **T023** — Rodar as duas suítes: Fase 5 verde (560/560 unitários; 29/29 em `CarrinhoTests`, os 19 originais mais os 10 novos).

## Fase 6 — Fechamento

- [x] **T024** — `docs/arquitetura.md` §5: a linha do carrinho passa a descrever os cartões, o resumo com destaque que troca e o cupom desabilitado, e as ações de esvaziar e continuar comprando.
- [x] **T025** — `grep -rn "spec 0[0-9][0-9]"` **e** varredura por número solto na área tocada por esta entrega (`Views/Carrinho`, `CarrinhoController`, `carrinho.js/css`, `CarrinhoService`, `CarrinhoDTO`). Nenhuma referência obsoleta encontrada — o comentário do botão de finalizar já foi corrigido na T014 e aponta para a `022`.
- [x] **T026** — `specs/README.md`: a decomposição já estava registrada (feita ao escrever as quatro specs, antes da implementação) — `021` redesenho do carrinho, `022` fechamento, `023` meus pedidos, `024` features, `025` estoque, nota do oitavo deslocamento. Conferido, nada a mudar aqui; o status da linha vira *Implementada* na T030.
- [x] **T027** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero (560/560 unitários; 172/172 E2E, inclusive o teste antes instável).
- [x] **T028** — Subida real, com screenshot: o cartão bate com o protótipo, o resumo confere (cupom desabilitado e explicado, rótulo "Calcule o frete no carrinho para ver o total" honesto sem entrega calculada). **Achado corrigido nesta verificação:** o `<dialog>` nativo abria ancorado no canto superior esquerdo em vez de centralizado — o `margin: auto` padrão do navegador não é confiável dentro de um ancestral com layout próprio (`.grade-carrinho` em `display: grid`). Corrigido com centralização explícita (`position: fixed` + `translate`). A 375px, o conteúdo do carrinho empilha corretamente (itens acima, resumo abaixo); o estouro do cabeçalho compartilhado é a dívida herdada desde a `009`, fora de escopo, e é o que a medição da CA-13 deliberadamente ignora.
- [x] **T029** — Preencher `checklist.md`, com os três achados da execução registrados na seção final.
- [x] **T030** — Status da spec → *Implementada*, do plano → *Executado*, linha e narrativa em `specs/README.md` atualizadas, `Ordem executada` ganha `021`. `000-baseline/spec.md` conferida — nenhuma dívida baseline referente ao carrinho para riscar (as menções de lá são de `Pedido`/`ItemPedido`, escopo da `022`). **O que não foi encerrado:** CA-05 (destaque "total a pagar") só ganha prova de ponta a ponta quando a `020` existir; a Fase B da `020` (cotação) ainda precisa ser executada, e deve reaproveitar `OpcaoDeFreteDTO`/`CotacaoDeFreteDTO` já criados aqui, não recriá-los.

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
