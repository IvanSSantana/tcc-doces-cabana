# Tarefas — Catálogo

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `012-catalogo` a partir de `main`, depois da `011` mergeada.
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial.

## Fase 2 — Testes (devem falhar)

*Escreva, rode, veja vermelho. Só então passe para a Fase 3.*

- [ ] **T003** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: `SemAcucar` nasce `false`; o método de intenção alterna; construtor aceita o valor.
- [ ] **T004** `[P]` — `DocesCabana.Tests/Units/Services/CatalogoServiceTests.cs`: apelido desconhecido lançando `KeyNotFoundException` (RF-07), soma de subcategorias (RN-03), sem açúcar combinando com subcategoria (RF-14, CA-10), inativo fora (RF-25), página fora do intervalo limitada (RF-21), estado vazio (RF-27).
- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Services/ApelidoTests.cs`: `"Empório"` vira `"emporio"`, `"Bolachas / Rosquinhas"` vira algo estável, e os apelidos das quatro categorias semeadas são distintos (plano §9, risco 5).
- [ ] **T006** `[P]` — `DocesCabana.Tests/Units/Controllers/CatalogoControllerTests.cs`: `Index` devolve `ViewResult`; sem parâmetros usa `NomeAZ` e página 1; `ordenacao` inválida cai no padrão em vez de `MaisVendidos`.
- [ ] **T007** `[P]` — `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs`: teste novo provando que `BuscarTodosProdutos` não devolve produto inativo (defeito da spec §10).
- [ ] **T008** `[P]` — `DocesCabana.Tests/Integration/Repositories/CatalogoRepositoryIntegrationTests.cs`: filtro combinado, as quatro ordenações e a paginação em SQLite — **incluindo percorrer todas as páginas e conferir que cada produto aparece exatamente uma vez** (CA-16, plano §9, riscos 2 e 3).
- [ ] **T009** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaCatalogo.cs`: objeto de página com `Abrir`, `Trilha`, `Categorias`, `CaixasDeSubcategoria`, `VerTodas`, `CaixaSemAcucar`, `SeletorDeOrdenacao`, `Cards`, `Paginacao`, `MensagemVazia`. Locators escopados em `.pagina-catalogo`.
- [ ] **T010** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: CA-01 a CA-26 do mapeamento do plano §7.
- [ ] **T011** — Confirmar que T003–T010 falham pelo motivo certo — tipo ou rota inexistente —, nunca por erro de compilação alheio.

## Fase 3 — Domínio

- [ ] **T012** — `DocesCabana.Domain/Entities/Produto.cs`: `SemAcucar` com `private set`, parâmetro opcional **por último** no construtor (para não quebrar chamadas posicionais existentes) e método de intenção para alternar.
- [ ] **T013** — Rodar `dotnet test DocesCabana.Tests`: T003 passa.

## Fase 4 — Aplicação

- [ ] **T014** `[P]` — `DocesCabana.Application/Enums/OrdenacaoCatalogo.cs`.
- [ ] **T015** `[P]` — `DocesCabana.Application/Servicos/Apelido.cs`: função pura nome → apelido (minúsculas, sem acento, espaço vira hífen).
- [ ] **T016** `[P]` — `DocesCabana.Application/DTOs/`: `CategoriaDTO`, `PaginaDeProdutosDTO`, `CatalogoDTO`, `FiltroCatalogoDTO`; `SemAcucar` em `ProdutoDTO`.
- [ ] **T017** `[P]` — `DocesCabana.Application/Contracts/Repositories/ICategoriaRepository.cs`.
- [ ] **T018** `[P]` — `DocesCabana.Application/Contracts/Services/ICategoriaService.cs` e `ICatalogoService.cs`.
- [ ] **T019** — `DocesCabana.Application/Contracts/Repositories/IProdutoRepository.cs`: `BuscarPaginaDoCatalogo` e `ContarNoCatalogo`.
- [ ] **T020** — `DocesCabana.Application/Mappings/CategoriaMapper.cs` e ajuste de `ProdutoMapper` para `SemAcucar`.
- [ ] **T021** — `DocesCabana.Application/Services/CategoriaService.cs`: casa apelido em memória sobre as categorias já carregadas.
- [ ] **T022** — `DocesCabana.Application/Services/CatalogoService.cs`: compõe o `CatalogoDTO`, calcula total de páginas e limita a página pedida ao intervalo válido.
- [ ] **T023** — `DocesCabana.Application/Services/ProdutoService.cs`: `BuscarTodosProdutos` para de devolver inativo (spec §10).
- [ ] **T024** — Rodar `dotnet test DocesCabana.Tests`: T004, T005, T006 e T007 passam.

## Fase 5 — Infraestrutura

- [ ] **T025** — `DocesCabana.Infrastructure/Repositories/CategoriaRepository.cs`: `Include` das subcategorias, `AsNoTracking`.
- [ ] **T026** — `DocesCabana.Infrastructure/Repositories/ProdutoRepository.cs`: `BuscarPaginaDoCatalogo` e `ContarNoCatalogo`. **Toda ordenação termina com `Nome` como desempate** — sem isso `Skip`/`Take` é indefinido (plano §9, riscos 2 e 3).
- [ ] **T027** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/ProdutoConfiguration.cs`: mapear `SemAcucar` com padrão `false`.
- [ ] **T028** — Migration: `dotnet ef migrations add AddProdutoSemAcucar --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`. **Inspecionar o arquivo gerado antes de confiar nele.**
- [ ] **T029** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: registrar `ICategoriaRepository`, `ICategoriaService` e `ICatalogoService`.
- [ ] **T030** — `ModelagemBancoTCC.dbml`: acrescentar `SemAcucar` em `Produto`.
- [ ] **T031** — Rodar `dotnet test DocesCabana.Tests`: T008 passa contra SQLite de verdade.

## Fase 6 — Dados

- [ ] **T032** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: taxonomia da tabela do plano §6 — 4 categorias, 31 subcategorias — e 100 produtos, 25 por categoria. Dez dos 25 de Doces marcados como sem açúcar, nas subcategorias vindas de "Doces Zero". Ao menos um produto inativo e um fora de estoque.
- [ ] **T033** — Apagar o banco de desenvolvimento, subir a aplicação do zero e conferir as 4 categorias, as 31 subcategorias e os 100 produtos (plano §9, risco 1).

## Fase 7 — Apresentação

- [ ] **T034** — `DocesCabana.MVC/Program.cs`: rota `Catalogo/{apelido?}` **antes** da rota padrão.
- [ ] **T035** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: `Index` `GET` pública, sem `[Authorize]`, saneando ordenação inválida e página fora do intervalo.
- [ ] **T036** — `DocesCabana.MVC/Views/Catalogo/_BarraLateral.cshtml`: categorias com a atual destacada; oito caixas de subcategoria visíveis e as demais dentro de um `<details>` "Ver todas"; caixa "Sem açúcar" em bloco separado (RN-04).
- [ ] **T037** `[P]` — `DocesCabana.MVC/Views/Catalogo/_Paginacao.cshtml`: controles numerados preservando categoria, filtros e ordenação em cada link.
- [ ] **T038** — `DocesCabana.MVC/Views/Catalogo/Index.cshtml`: trilha, `<form method="get">` envolvendo barra lateral e ordenação com `onchange="this.form.submit()"` e botão em `<noscript>`, grade, paginação e estado vazio. "Mais vendidos" no seletor com `disabled` (RF-16).
- [ ] **T039** — `DocesCabana.MVC/wwwroot/css/pages/catalogo.css`: tokens em `.pagina-catalogo` — **nunca em `:root`**; grade de 3 colunas colapsando em coluna única; reset defensivo do seletor `section` que `header.css` vaza (plano §9, risco 8).
- [ ] **T040** — `DocesCabana.MVC/ViewComponents/Header.cs` e `Views/Shared/Components/Header/Default.cshtml`: as categorias reais com menu suspenso de até 8 subcategorias (RF-03, RF-04). Mata os 4 `href="#"`. **O botão "Favoritos" continua sem destino — é outra entrega, e fica registrado no checklist.**
- [ ] **T041** — `DocesCabana.MVC/wwwroot/css/components/header.css`: estados do menu suspenso conforme o plano §3 — categoria aberta em coral com seta para cima, painel bege, cartão coral. Abrir por `:hover` **e** `:focus-within`; em tela estreita, virar lista expansível sem depender de `:hover` (plano §9, risco 4).
- [ ] **T042** `[P]` — `DocesCabana.MVC/Views/Shared/Components/CardProduto/Default.cshtml`: os três controles com `disabled` e rótulo de indisponível (RF-24).
- [ ] **T043** `[P]` — `DocesCabana.MVC/wwwroot/js/components/card-produto.js`: **apagar** `adicionarAoCarrinho`, `alternarFavorito` e `alterarQuantidade` — o teatro descrito na spec §10. Remover o `<script>` do layout se ficar órfão.
- [ ] **T044** `[P]` — `DocesCabana.MVC/wwwroot/css/components/card-produto.css`: estado desabilitado dos três controles.
- [ ] **T045** — `DocesCabana.MVC/Views/Home/_Categorias.cshtml`: os blocos passam a ligar ao catálogo da categoria (RF-05). **As imagens continuam as atuais e não correspondem mais às categorias — registrado no backlog, não corrigido aqui.**
- [ ] **T046** — `DocesCabana.MVC/Areas/Admin/Controllers/ProdutoController.cs`: seletor de subcategoria qualificado por categoria (RF-28, CA-24).
- [ ] **T047** — `DocesCabana.MVC/Areas/Admin/Views/Produto/Cadastro.cshtml`: campo de sem açúcar (RF-29).
- [ ] **T048** — Rodar `dotnet test DocesCabana.Tests`: tudo verde.

## Fase 8 — Verificação ao vivo

- [ ] **T049** — Subir a aplicação e percorrer CA-01 a CA-24 manualmente, com atenção a CA-06 (duas subcategorias somam, não intersectam), CA-10 (sem açúcar combina com subcategoria), CA-16 (percorrer as 3 páginas de uma categoria e conferir que nada repete) e CA-20/CA-21. Conferir também que a **vitrine da home parou de listar inativo** (spec §10).
- [ ] **T050** — Repetir o essencial com **JavaScript desligado no navegador** (CA-25): filtrar, ordenar e paginar precisam funcionar.
- [ ] **T051** — Rodar `dotnet test DocesCabana.Tests.E2E` inteiro: verde, incluindo os fluxos herdados que passam pelo cabeçalho, pela página inicial e pelo cadastro de produto — os três alterados aqui.

## Fase 9 — Fechamento

- [ ] **T052** — `dotnet build` sem warnings novos; as duas suítes verdes.
- [ ] **T053** — Preencher o checklist em `checklist.md`, registrando o que ficou provado por teste, o que por verificação manual, e o que não foi verificado.
- [ ] **T054** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`, incluindo os itens novos de backlog (catálogo real da loja, imagens da página inicial, revisão da ordenação inicial, sem glúten e sem lactose).

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T022, T035, T038 |
| RF-02 | T005, T015, T021, T034 |
| RF-03 | T010, T040 |
| RF-04 | T010, T040 |
| RF-05 | T010, T045 |
| RF-06 | T038 |
| RF-07 | T004, T021, T022 |
| RF-08 | T036 |
| RF-09 | T036 |
| RF-10 | T010, T036 |
| RF-11 | T010, T036 |
| RF-12 | T004, T022, T026 |
| RF-13 | T004, T026 |
| RF-14 | T004, T022, T026, T036 |
| RF-15 | T038 |
| RF-16 | T010, T038 |
| RF-17 | T006, T035 |
| RF-18 | T010, T037, T038 |
| RF-19 | T008, T022, T026 |
| RF-20 | T037 |
| RF-21 | T004, T022, T035 |
| RF-22 | T036, T037, T038, T041, T050 |
| RF-23 | T038 |
| RF-24 | T042, T043, T044 |
| RF-25 | T004, T007, T022, T023, T026 |
| RF-26 | T038 |
| RF-27 | T004, T038 |
| RF-28 | T010, T046 |
| RF-29 | T012, T047 |
| RN-01 | T004, T022, T026 |
| RN-02 | T036, T046 |
| RN-03 | T004, T026 |
| RN-04 | T012, T026, T036 |
| RN-05 | T008, T026 |
| RN-06 | T026, T036, T040 |
| RN-07 | T038 |
| CA-01 a CA-26 | T010, T049, T050 (mapa detalhado no plano §7) |
