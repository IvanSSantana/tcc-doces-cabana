# Tarefas — Refinamento do catálogo

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Quatro frentes independentes.** Cada uma tem seu ciclo vermelho → verde
> próprio, e as fases 2 a 6 podem ser reordenadas entre si. O que **não** pode
> mudar de lugar é a Fase 2: ela mexe na ordenação padrão, que é o risco de
> maior probabilidade do plano (§9), e precisa ser confirmada sozinha antes de
> qualquer outra mudança entrar e confundir o diagnóstico.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `014-refinamento-do-catalogo` a partir de `main`.
- [x] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 349 e 57 verdes, herdados da `013`) **e o tempo de subida da suíte E2E** — é a linha de base do risco de lentidão do seed (plano §9).
- [x] **T003** — Subir a aplicação e registrar, no catálogo de "Doces": a largura do cartão, a largura da coluna da grade, e a diferença de altura entre os botões "Adicionar" de um produto de nome curto e um de nome longo. São os números que a Fase 5 tem que zerar.

## Fase 2 — Ordenação inicial e índice único

- [x] **T004** `[P]` — `DocesCabana.Tests/Integration/Repositories/AvaliacaoIntegrationTests.cs` (criar): a segunda avaliação da mesma pessoa sobre o mesmo produto é recusada (RF-15, RN-01, CA-17).
- [x] **T005** `[P]` — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: catálogo aberto sem escolher ordenação vem por melhor avaliação, e duas páginas consecutivas não repetem nem escondem produto (RF-16, RN-04, CA-18, CA-19).
- [x] **T006** — Confirmar que T004 e T005 falham pelo motivo certo — duplicidade aceita, seletor marcando "Nome (A-Z)" — e não por erro de compilação.
- [x] **T007** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/AvaliacaoConfiguration.cs`: índice único em `(UsuarioId, ProdutoId)`.
- [x] **T008** — Gerar a migration `AddUniqueIndexAvaliacaoUsuarioProduto` e **ler o arquivo gerado antes de aplicar** — confirmar que cria só o índice, sem tocar em coluna nem em dado.
- [x] **T009** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: padrão de `ordenacao` passa a `MelhorAvaliados`; `SanearOrdenacao` passa a mandar `MaisVendidos` para `MelhorAvaliados` em vez de `NomeAZ`.
- [x] **T010** — **Rodar as duas suítes inteiras.** É o ponto de conferência do risco 1 do plano: qualquer teste que assumia ordem alfabética aparece aqui. Corrigir o *teste* quando a premissa dele é que mudou; corrigir o *código* se algo realmente quebrou — e registrar no checklist qual foi qual.

## Fase 3 — Seed de avaliações

- [x] **T011** — `DocesCabana.Tests/Units/Helpers/GeradorDeAvaliacoesTests.cs` (criar): a maior parte dos produtos recebe avaliação, parte nenhuma, e duas gerações com a mesma semente produzem o mesmo resultado (RF-12, RF-13, RF-14, CA-14, CA-15, CA-16).
- [x] **T012** — Confirmar que T011 falha por não existir o gerador, e não por outra razão.
- [x] **T013** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: elenco de clientes fictícios de 3 para 8, com CPFs válidos e distintos.
- [x] **T014** — `DbInitializer.cs`: `GerarAvaliacoesMock` determinística — ~70 dos 100 produtos com 1 a 4 avaliações, notas enviesadas para cima, parte sem comentário, semente fixa. Sem acesso a banco dentro dela, para poder ser chamada duas vezes no teste.
- [x] **T015** — `DbInitializer.cs`: ligar o gerador ao semeador, respeitando RN-01 (uma avaliação por pessoa por produto) — a regra tem que valer na geração, não só no índice.
- [x] **T016** — Rodar `dotnet test DocesCabana.Tests`: T011 passa. Conferir o tempo de subida da E2E contra T002.

## Fase 4 — Cartão na grade

- [x] **T017** — `DocesCabana.Tests.E2E/Paginas/PaginaCatalogo.cs` e `Fluxos/CatalogoTests.cs`: cartão preenche a coluna, botões da mesma linha alinhados, etiqueta dentro da imagem (RF-08, RF-09, RF-10, CA-10, CA-11, CA-12).
- [x] **T018** — Confirmar que T017 falha com os números medidos em T003, não com outros.
- [x] **T019** — `DocesCabana.MVC/wwwroot/css/components/card-produto.css`: tirar `width: 85%` da classe base.
- [x] **T020** — `DocesCabana.MVC/wwwroot/css/components/vitrine-produtos.css`: devolver a largura ao container do carrossel, para o carrossel não mudar de aparência (RF-11).
- [x] **T021** — `card-produto.css`: `margin-top: auto` nas ações, alinhando os botões na base independentemente do tamanho do nome (RF-09).
- [x] **T022** — `card-produto.css`: etiqueta de fora de estoque sobre a imagem (RF-10).
- [x] **T023** — Conferir contra T003 ao vivo: cartão preenchendo a coluna, botões alinhados, **e o carrossel da página inicial idêntico ao de antes** (CA-13).

## Fase 5 — Atualização sem recarga

- [x] **T024** — `DocesCabana.Tests.E2E/Paginas/PaginaCatalogo.cs`: ações que esperam a troca parcial em vez de navegação, e localizadores para contagem, foco e estado de carregamento.
- [x] **T025** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: CA-01 a CA-06, CA-09 e CA-21 — sem recarga, endereço em sincronia, botão voltar, rolagem preservada, topo ao paginar, anúncio, troca de categoria.
- [x] **T026** — `CatalogoTests.cs`: **reescrever o teste de RF-05 para desligar o JavaScript de verdade** (`JavaScriptEnabled = false`) e clicar o botão "Aplicar" do `<noscript>` (CA-07). O teste atual navega por endereço com script ligado e não prova o requisito — plano §7.
- [x] **T027** — `CatalogoTests.cs`: CA-08 — abortar a requisição parcial com interceptação de rota e verificar que a página completa carrega.
- [x] **T028** — Confirmar que T025 a T027 falham pelo motivo certo: recarga acontecendo, `<noscript>` inalcançável, nenhuma recuperação de falha.
- [x] **T029** — `DocesCabana.MVC/Views/Catalogo/_ResultadoCatalogo.cshtml` (criar): contagem, grade ou mensagem de vazio, e paginação — recortados de `Index.cshtml` sem alteração de conteúdo.
- [x] **T030** — `Views/Catalogo/Index.cshtml`: incluir a partial; contagem ganha `aria-live="polite"` e o cabeçalho do resultado ganha `tabindex="-1"` para receber foco (RF-04, risco 2).
- [x] **T031** — `Controllers/CatalogoController.cs`: desvio que devolve a partial em requisição assíncrona e a página inteira nas demais.
- [x] **T032** — `DocesCabana.MVC/wwwroot/js/pages/catalogo.js` (criar): interceptar troca de filtro, de ordenação e de página; montar o endereço **a partir do próprio formulário**, nunca à mão (risco 3); buscar e trocar o conteúdo.
- [x] **T033** — `catalogo.js`: `pushState` a cada troca e `popstate` refazendo o caminho, para o botão voltar funcionar (RF-02, CA-03).
- [x] **T034** — `catalogo.js`: mover o foco para o cabeçalho do resultado após a troca (RF-18, CA-21). **Risco 2 do plano** — sem isto, quem usa teclado é jogado para o começo do documento ao paginar.
- [x] **T035** — `catalogo.js`: preservar a rolagem ao filtrar e ordenar; ir ao início da lista ao paginar (RF-03, CA-04, CA-05).
- [x] **T036** — `catalogo.js`: em falha da busca, navegar para o endereço e deixar o servidor entregar a página completa (RF-06, CA-08).
- [x] **T037** — `catalogo.js` e `wwwroot/css/pages/catalogo.css`: estado de carregamento com atraso curto, para não piscar em resposta rápida (risco 7).
- [x] **T038** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 5 verde.

## Fase 6 — Cabeçalho

- [x] **T039** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs` ou fluxo de autenticação: cliente autenticado não recebe atalho para conta inexistente (RF-17, CA-20). Ver falhar.
- [x] **T040** — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: "Conta" desabilitado com rótulo "ainda não disponível", mesmo padrão dos três controles do cartão (spec `012`, RF-24).

## Fase 7 — Verificação ao vivo

- [x] **T041** — Subir a aplicação e percorrer o catálogo à mão: filtrar, ordenar, paginar, trocar de categoria, usar o botão voltar, e conferir o endereço colado numa aba nova. Os testes provam número; só o uso prova que a transição não incomoda.
- [x] **T042** — Percorrer o catálogo **só com o teclado**, do primeiro filtro até a paginação, conferindo que o foco nunca é jogado para o começo do documento (RF-18, CA-21; risco 2).
- [x] **T043** — Desligar o JavaScript no navegador de verdade e repetir filtrar, ordenar e paginar (CA-07). O teste automatizado cobre isso, mas este é o requisito que já foi dado como provado uma vez sem estar.
- [x] **T044** — Conferir a grade a 375px: cartões, etiqueta e paginação sem rolagem horizontal no conteúdo do catálogo.

## Fase 8 — Fechamento

- [x] **T045** — `dotnet build` sem warnings novos; as duas suítes verdes; tempo de subida da E2E comparado a T002.
- [x] **T046** — Preencher `checklist.md`, registrando o que ficou provado por teste, o que por verificação manual, o que não foi verificado, e **quais testes existentes mudaram de premissa na T010**.
- [x] **T047** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`. A renumeração da cadeia (`015` estoque, `016` carrinho, `017` endereço, `018` fechamento, `019` pagamento) foi feita quando a spec nasceu, junto das referências obsoletas; aqui só se **confere**, varrendo `spec 0NN` e `` `0NN` `` na base inteira. Foi essa varredura que escapou na `013` — e o que escapou dela foi a própria spec que estava sendo escrita.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T025, T031, T032 |
| RF-02 | T025, T032, T033 |
| RF-03 | T025, T035 |
| RF-04 | T025, T030 |
| RF-05 | T026, T029, T043 |
| RF-06 | T027, T036 |
| RF-07 | T025, T029 |
| RF-08 | T017, T019 |
| RF-09 | T017, T021 |
| RF-10 | T017, T022 |
| RF-11 | T020, T023 |
| RF-12 | T011, T013, T014 |
| RF-13 | T011, T014 |
| RF-14 | T011, T014 |
| RF-15 | T004, T007, T008 |
| RF-16 | T005, T009 |
| RF-17 | T039, T040 |
| RF-18 | T025, T034, T042 |
| RN-01 | T004, T007, T015 |
| RN-02 | T011, T014 |
| RN-03 | T005, T039, T040 |
| RN-04 | T005, T010 |
| CA-01 | T025, T031, T032 |
| CA-02 | T025, T032 |
| CA-03 | T025, T033 |
| CA-04 | T025, T035, T041 |
| CA-05 | T025, T035 |
| CA-06 | T025, T030 |
| CA-07 | T026, T043 |
| CA-08 | T027, T036 |
| CA-09 | T025, T031 |
| CA-10 | T017, T019, T023 |
| CA-11 | T017, T021, T023 |
| CA-12 | T017, T022, T023 |
| CA-13 | T020, T023 |
| CA-14 | T011, T014 |
| CA-15 | T011, T014 |
| CA-16 | T011, T014 |
| CA-17 | T004, T007, T008 |
| CA-18 | T005, T009 |
| CA-19 | T005, T010 |
| CA-20 | T039, T040 |
| CA-21 | T025, T034, T042 |
