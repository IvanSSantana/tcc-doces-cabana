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

> **Sem Domínio e sem migration.** `Categoria`, `Subcategoria` e `Produto` já
> são o que a feature precisa; a única mudança de dados é o seed (plano §5).

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `012-catalogo` a partir de `main`, depois da `011` mergeada.
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial.

## Fase 2 — Testes (devem falhar)

*Escreva, rode, veja vermelho. Só então passe para a Fase 3.*

- [ ] **T003** `[P]` — `DocesCabana.Tests/Units/Services/CatalogoServiceTests.cs`: filtro por categoria, soma de subcategorias (RN-03), categoria inexistente lançando `KeyNotFoundException` (RF-05), inativo fora (RF-18), estado vazio (RF-20).
- [ ] **T004** `[P]` — `DocesCabana.Tests/Units/Controllers/CatalogoControllerTests.cs`: `Index` devolve `ViewResult`; sem parâmetros usa `MelhorAvaliados`; `ordenacao` inválida cai no padrão em vez de `MaisVendidos` (plano §8, risco 3).
- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs`: teste novo provando que `BuscarTodosProdutos` não devolve produto inativo (defeito da spec §10).
- [ ] **T006** `[P]` — `DocesCabana.Tests/Integration/Repositories/CatalogoRepositoryIntegrationTests.cs`: as quatro ordenações e o filtro em SQLite, **incluindo produto sem nenhuma avaliação indo para o fim em "Melhor avaliados"** (RN-04, plano §8, risco 2).
- [ ] **T007** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaCatalogo.cs`: objeto de página com `Abrir`, `Trilha`, `Categorias`, `CaixasDeSubcategoria`, `SeletorDeOrdenacao`, `Cards`, `MensagemVazia`. Locators escopados em `.pagina-catalogo`.
- [ ] **T008** — `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs`: CA-01 a CA-17 do mapeamento do plano §6.
- [ ] **T009** — Confirmar que T003–T008 falham pelo motivo certo — tipo ou rota inexistente —, nunca por erro de compilação alheio.

## Fase 3 — Aplicação

- [ ] **T010** `[P]` — `DocesCabana.Application/Enums/OrdenacaoCatalogo.cs`.
- [ ] **T011** `[P]` — `DocesCabana.Application/DTOs/CategoriaDTO.cs` e `DTOs/CatalogoDTO.cs`.
- [ ] **T012** `[P]` — `DocesCabana.Application/Contracts/Repositories/ICategoriaRepository.cs`.
- [ ] **T013** `[P]` — `DocesCabana.Application/Contracts/Services/ICategoriaService.cs` e `Contracts/Services/ICatalogoService.cs`.
- [ ] **T014** — `DocesCabana.Application/Contracts/Repositories/IProdutoRepository.cs`: acrescentar `BuscarParaCatalogo`.
- [ ] **T015** — `DocesCabana.Application/Mappings/CategoriaMapper.cs`.
- [ ] **T016** — `DocesCabana.Application/Services/CategoriaService.cs`.
- [ ] **T017** — `DocesCabana.Application/Services/CatalogoService.cs`: compõe o `CatalogoDTO`; `KeyNotFoundException` para categoria inexistente.
- [ ] **T018** — `DocesCabana.Application/Services/ProdutoService.cs`: `BuscarTodosProdutos` para de devolver inativo (spec §10).
- [ ] **T019** — Rodar `dotnet test DocesCabana.Tests`: T003, T004 e T005 passam.

## Fase 4 — Infraestrutura

- [ ] **T020** — `DocesCabana.Infrastructure/Repositories/CategoriaRepository.cs`: `Include` das subcategorias, `AsNoTracking`.
- [ ] **T021** — `DocesCabana.Infrastructure/Repositories/ProdutoRepository.cs`: `BuscarParaCatalogo` com filtro e as quatro ordenações. "Melhor avaliados" por subconsulta de média anulável, nulos por último.
- [ ] **T022** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: registrar `ICategoriaRepository`, `ICategoriaService` e `ICatalogoService`.
- [ ] **T023** — Rodar `dotnet test DocesCabana.Tests`: T006 passa contra SQLite de verdade.

## Fase 5 — Dados

- [ ] **T024** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: taxonomia da tabela do plano §5 — 6 categorias, "Salgados Assados"/"Salgados Fritos" migrando para Padaria, subcategorias novas para Empório, Bomboniere e Souvenir. Os 6 produtos continuam em "Doces de Tacho".
- [ ] **T025** — Apagar o banco de desenvolvimento, subir a aplicação do zero e conferir as 6 categorias com suas subcategorias (plano §8, risco 1). Semear ao menos um produto inativo e um fora de estoque, para que CA-13 e CA-14 tenham o que exercitar.

## Fase 6 — Apresentação

- [ ] **T026** — `DocesCabana.MVC/Controllers/CatalogoController.cs`: `Index` `GET` pública, sem `[Authorize]`, saneando `ordenacao` inválida para `MelhorAvaliados`.
- [ ] **T027** — `DocesCabana.MVC/Views/Catalogo/_BarraLateral.cshtml`: "Todos" + categorias com a atual destacada + caixas de subcategoria (nenhuma quando for "Todos", RF-09).
- [ ] **T028** — `DocesCabana.MVC/Views/Catalogo/Index.cshtml`: trilha, `<form method="get">` envolvendo barra lateral e ordenação com `onchange="this.form.submit()"` e botão em `<noscript>`, grade e estado vazio. "Mais vendidos" no seletor com `disabled` (RF-13).
- [ ] **T029** — `DocesCabana.MVC/wwwroot/css/pages/catalogo.css`: tokens em `.pagina-catalogo` — **nunca em `:root`**; grade de 3 colunas colapsando em coluna única; reset defensivo do seletor `section` que `header.css` vaza (plano §8, risco 6).
- [ ] **T030** — `DocesCabana.MVC/Views/Shared/Components/CardProduto/Default.cshtml`: os três controles com `disabled` e rótulo de indisponível (RF-17).
- [ ] **T031** `[P]` — `DocesCabana.MVC/wwwroot/js/components/card-produto.js`: **apagar** `adicionarAoCarrinho`, `alternarFavorito` e `alterarQuantidade` — o teatro descrito na spec §10. Remover o `<script>` correspondente do layout se ficar órfão.
- [ ] **T032** `[P]` — `DocesCabana.MVC/wwwroot/css/components/card-produto.css`: estado desabilitado dos três controles.
- [ ] **T033** — `DocesCabana.MVC/ViewComponents/Header.cs` e `Views/Shared/Components/Header/Default.cshtml`: o menu passa a listar as categorias reais, cada uma ligando ao catálogo dela (RF-02). Mata os 4 `href="#"`. **O botão "Favoritos" continua sem destino — é outra entrega, e fica registrado no checklist.**
- [ ] **T034** — `DocesCabana.MVC/Views/Home/_Categorias.cshtml`: os blocos passam a ligar ao catálogo da categoria (RF-03), corrigindo o apontamento para uma ação que nunca existiu.
- [ ] **T035** — Rodar `dotnet test DocesCabana.Tests`: tudo verde.

## Fase 7 — Verificação ao vivo

- [ ] **T036** — Subir a aplicação e percorrer CA-01 a CA-16 manualmente, com atenção a CA-05 (duas subcategorias somam, não intersectam), CA-10 (ordenação sobrevive à troca de categoria) e CA-13/CA-14 (inativo some, fora de estoque aparece sinalizado). Conferir também que a **vitrine da home parou de listar inativo** (spec §10).
- [ ] **T037** — Rodar `dotnet test DocesCabana.Tests.E2E` inteiro: verde, incluindo os fluxos herdados que passam pelo cabeçalho e pela página inicial, ambos alterados aqui.

## Fase 8 — Fechamento

- [ ] **T038** — `dotnet build` sem warnings novos; as duas suítes verdes.
- [ ] **T039** — Preencher o checklist em `checklist.md`, registrando o que ficou provado por teste, o que por verificação manual, e o que não foi verificado.
- [ ] **T040** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`.
- [ ] **T041** — **Reapresentar ao responsável as cinco decisões da seção 11 da spec**, agora com a tela pronta para olhar. Nenhuma delas é definitiva até ele confirmar.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T017, T026, T028 |
| RF-02 | T008, T033 |
| RF-03 | T008, T034 |
| RF-04 | T028 |
| RF-05 | T003, T017, T026 |
| RF-06 | T027 |
| RF-07 | T027 |
| RF-08 | T027 |
| RF-09 | T008, T027 |
| RF-10 | T003, T017, T021 |
| RF-11 | T003, T021 |
| RF-12 | T028 |
| RF-13 | T008, T028 |
| RF-14 | T004, T026 |
| RF-15 | T008, T027, T028 |
| RF-16 | T028, T029 |
| RF-17 | T030, T031, T032 |
| RF-18 | T003, T005, T017, T018, T021 |
| RF-19 | T028 |
| RF-20 | T003, T028 |
| RN-01 | T003, T017, T021 |
| RN-02 | T027 |
| RN-03 | T003, T021 |
| RN-04 | T006, T021 |
| RN-05 | T028 |
| CA-01 a CA-17 | T008, T036 (mapa detalhado no plano §6) |
