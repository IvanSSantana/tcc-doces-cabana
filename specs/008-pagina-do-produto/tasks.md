# Tarefas — Página do produto

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com outras `[P]` vizinhas (arquivos distintos,
  sem dependência entre si). Sem `[P]` significa: termine a anterior primeiro.
- Toda tarefa nomeia **o arquivo exato** que ela cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Pré-requisito bloqueante.** A feature `004-separar-pessoa-de-credencial`
> precisa estar implementada e em `main` antes da T001. Sem ela, `Avaliacao` não
> navega até `Usuario` e o nome de quem avaliou não chega à tela (RF-13).

---

## Fase 1 — Preparação

- [x] **T001** — Confirmar que a `004` está em `main` e criar a branch `008-pagina-do-produto` a partir dela.
- [x] **T002** — Rodar `dotnet build` e `dotnet test` e registrar o estado inicial (tudo verde antes de começar).

## Fase 2 — Testes (devem falhar)

*Escreva, rode, veja vermelho. Só então passe para a Fase 3.*

- [x] **T003** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: descrição nula aceita, descrição de 4000 caracteres aceita, 4001 lança (RN-01).
- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Entities/AvaliacaoTests.cs`: `AlternarVotoUtil` marca, alterna e desmarca (RN-06); autor votando na própria avaliação lança (RN-07); `TotalUteis` conta pessoas distintas e nunca fica negativo (RN-08); `DataCriacao` é preenchida (RN-09).
- [x] **T005** `[P]` — `DocesCabana.Tests/Units/Entities/VotoUtilTests.cs`: `Guid.Empty` em avaliação ou usuário lança.
- [x] **T006** `[P]` — `DocesCabana.Tests/Units/Validators/ProdutoDTOValidatorTests.cs`: descrição vazia é válida, 4001 caracteres é inválido com a mensagem do RN-01.
- [x] **T007** `[P]` — `DocesCabana.Tests/Units/Mappings/ProdutoDetalheMapperTests.cs`: resumo corta em 160 no fim da palavra com reticências, texto curto sai inteiro sem reticências, descrição nula gera resumo nulo (RN-02, CA-03).
- [x] **T008** `[P]` — `DocesCabana.Tests/Units/Services/AvaliacaoServiceTests.cs`: média com uma casa e média nula sem avaliação (RN-03, CA-07, CA-08); distribuição com as cinco chaves (RN-04); ordenação por relevantes com desempate pela mais recente (RN-05) e por mais recentes (CA-10); `TemMais` e paginação de 5 em 5 (RF-14, RF-15, CA-09); voto marcando e desmarcando (CA-11, CA-12) e voto do próprio autor lançando (CA-14).
- [x] **T009** `[P]` — `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs`: detalhe de produto ativo traz nome, preço e resumo (CA-01); id inexistente lança `KeyNotFoundException` (CA-04); produto inativo lança o mesmo (CA-05).
- [x] **T010** `[P]` — `DocesCabana.Tests/Units/Controllers/ProdutoControllerTests.cs`: `Detalhes` devolve `ViewResult` com o DTO composto; `exibir` fora de faixa é saneado; `VotarUtil` redireciona preservando ordenação e quantidade (RF-17); voto do próprio autor não altera nada (RF-21).
- [x] **T011** — Confirmar que T003–T010 falham pelo motivo certo (e não por erro de compilação alheio).

## Fase 3 — Domínio

- [ ] **T012** `[P]` — `DocesCabana.Domain/Entities/VotoUtil.cs`: chave composta, construtor validante, `protected Ctor()`, navegação `Avaliacao?`.
- [ ] **T013** — `DocesCabana.Domain/Entities/Avaliacao.cs`: acrescentar `DataCriacao`, remover `UpVote`, expor `Votos`, `TotalUteis`, `MarcadaComoUtilPor` e `AlternarVotoUtil` com a guarda de autoria.
- [ ] **T014** `[P]` — `DocesCabana.Domain/Entities/Produto.cs`: `Descricao` opcional, parâmetro no construtor, `AlterarDescricao` e `ValidarDescricao`.
- [ ] **T015** — Rodar `dotnet test`: T003, T004 e T005 passam.

## Fase 4 — Aplicação

- [x] **T016** `[P]` — `DocesCabana.Application/Enums/OrdenacaoAvaliacao.cs`.
- [x] **T017** `[P]` — `DocesCabana.Application/DTOs/AvaliacaoDTO.cs`, `DTOs/ResumoAvaliacoesDTO.cs` e `DTOs/PaginaAvaliacoesDTO.cs`.
- [x] **T018** — `DocesCabana.Application/DTOs/ProdutoDetalheDTO.cs` e `Descricao` em `DTOs/ProdutoDTO.cs`.
- [x] **T019** `[P]` — `DocesCabana.Application/Contracts/Repositories/IAvaliacaoRepository.cs` e `BuscarDetalhePorId` em `Contracts/Repositories/IProdutoRepository.cs`.
- [x] **T020** `[P]` — `DocesCabana.Application/Contracts/Services/IAvaliacaoService.cs` e `BuscarDetalhe` em `Contracts/Services/IProdutoService.cs`.
- [x] **T021** `[P]` — `DocesCabana.Application/Mappings/AvaliacaoMapper.cs`: `ToDTO(Avaliacao, Guid? usuarioAtual)`, resolvendo `MarcadaPeloUsuarioAtual` e `EhDoUsuarioAtual`.
- [x] **T022** — `DocesCabana.Application/Mappings/ProdutoDetalheMapper.cs`: montagem do DTO composto e o resumo de 160 caracteres.
- [x] **T023** `[P]` — `DocesCabana.Application/Mappings/ProdutoMapper.cs`: mapear `Descricao` nos dois sentidos.
- [x] **T024** `[P]` — `DocesCabana.Application/Validators/ProdutoDTOValidator.cs`: `MaximumLength(4000)` na descrição.
- [x] **T025** — `DocesCabana.Application/Services/AvaliacaoService.cs`: resumo, listagem ordenada e `AlternarVotoUtil` com commit via `IUnitOfWork`.
- [x] **T026** — `DocesCabana.Application/Services/ProdutoService.cs`: `BuscarDetalhe`, com `KeyNotFoundException` para inexistente e para inativo.
- [x] **T027** — Rodar `dotnet test`: T006, T007, T008 e T009 passam.

## Fase 5 — Infraestrutura

- [x] **T028** — `DocesCabana.Infrastructure/Repositories/AvaliacaoRepository.cs`: consulta paginada com `Include` de usuário e votos, contagem total e contagem por nota.
- [x] **T029** `[P]` — `DocesCabana.Infrastructure/Repositories/ProdutoRepository.cs`: `BuscarDetalhePorId` com `Include(p => p.Subcategoria)`.
- [x] **T030** — `DatabaseContext/Configurations/VotoUtilConfiguration.cs` novo, `AvaliacaoConfiguration.cs` e `ProdutoConfiguration.cs` alterados, e `DbSet<VotoUtil>` em `DocesCabanaDbContext.cs`.
- [x] **T031** — Migration: `dotnet ef migrations add AddProdutoDescricaoAndAvaliacaoVotes --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`.
- [x] **T032** `[P]` — `DependencyInjections/ApplicationDependencyInjection.cs`: registrar `IAvaliacaoRepository` e `IAvaliacaoService`.
- [x] **T033** `[P]` — `DocesCabana.Tests/Integration/InfraestruturaSqliteEmMemoria.cs`: `SemearAvaliacao`.
- [x] **T034** — `DocesCabana.Tests/Integration/Repositories/AvaliacaoRepositoryIntegrationTests.cs`: ordenação e contagem por nota rodando em SQLite.
- [x] **T035** `[P]` — `ModelagemBancoTCC.dbml`: coluna `Descricao` em `Produto`, `DataCriacao` em `Avaliacao`, remoção de `UpVote` e a tabela `VotoUtil` com sua chave composta e as duas referências.

## Fase 6 — Apresentação

- [x] **T036** — `DocesCabana.MVC/Controllers/ProdutoController.cs`: `Detalhes` (GET público, saneando `ordenacao` e `exibir`) e `VotarUtil` (`[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Authorize]`, redirecionando com a âncora `#avaliacoes`).
- [x] **T037** `[P]` — `DocesCabana.MVC/ViewComponents/EstrelasNota.cs` e `Views/Shared/Components/EstrelasNota/Default.cshtml`: fileira em SVG com preenchimento fracionário e a nota em texto para leitor de tela.
- [x] **T038** `[P]` — `DocesCabana.MVC/wwwroot/css/components/estrelas-nota.css`.
- [x] **T039** — `DocesCabana.MVC/Views/Produto/Detalhes.cshtml`: caminho de navegação, bloco de compra, aviso de fora de estoque e seção de descrição (omitida quando não houver).
- [x] **T040** `[P]` — `DocesCabana.MVC/Views/Produto/_BlocoAvaliacoes.cshtml`: média, histograma, seletor de ordenação, lista, "Ver mais" e o estado vazio.
- [x] **T041** `[P]` — `DocesCabana.MVC/Views/Produto/_CartaoAvaliacao.cshtml`: autor, data, estrelas, comentário e o formulário do voto com `aria-pressed`; para visitante, o botão abre o modal de login.
- [x] **T042** — `DocesCabana.MVC/wwwroot/css/pages/produto.css`: tokens escopados em `.pagina-produto`, as duas grades, as réguas e o colapso para coluna única abaixo de 900px.
- [x] **T043** `[P]` — `DocesCabana.MVC/wwwroot/js/pages/produto.js`: seletor de quantidade limitado a 1–99.
- [x] **T044** `[P]` — `DocesCabana.MVC/Views/Shared/Components/CardProduto/Default.cshtml`: imagem e nome viram link para `Produto/Detalhes`, sem engolir o clique dos botões do card.
- [x] **T045** `[P]` — `DocesCabana.MVC/Views/Admin/Cadastro.cshtml`: `textarea` de descrição com `asp-validation-for`.
- [x] **T046** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: descrição nos produtos semeados e avaliações de exemplo com notas variadas e votos, para a tela ter conteúdo em desenvolvimento.
- [x] **T047** — Rodar `dotnet test`: T010 passa.

## Fase 7 — Fechamento

- [ ] **T048** — `dotnet test` inteiro verde.
- [ ] **T049** — Executar a aplicação e percorrer manualmente CA-01 a CA-15, com atenção a CA-02 (âncora), CA-06 (fora de estoque) e CA-13 (visitante clicando em "Útil").
- [ ] **T050** — Conferir CA-16 e o piso de qualidade: 375px sem rolagem horizontal, navegação inteira por teclado com foco visível, e `prefers-reduced-motion` desligando a rolagem suave.
- [ ] **T051** — Preencher o checklist em `checklist.md`.
- [ ] **T052** — Atualizar o status da spec para *Implementada*, o plano para
      *Executado*, e a linha da `008` em `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T044, T049 |
| RF-02 | T029, T039 |
| RF-03 | T009, T026, T036 |
| RF-04 | T009, T026 |
| RF-05 | T039 |
| RF-06 | T007, T022, T039 |
| RF-07 | T039 |
| RF-08 | T007, T022, T039 |
| RF-09 | T039, T043 |
| RF-10 | T039, T042 |
| RF-11 | T024, T045, T049 |
| RF-12 | T008, T025, T040 |
| RF-13 | T021, T028, T041 |
| RF-14 | T008, T025, T040 |
| RF-15 | T008, T025, T040 |
| RF-16 | T008, T025, T036, T040 |
| RF-17 | T010, T036 |
| RF-18 | T008, T040 |
| RF-19 | T004, T013, T025, T041 |
| RF-20 | T036, T041 |
| RF-21 | T004, T010, T013, T036 |
| RF-22 | T042, T050 |
| RN-01 | T003, T006, T014, T024 |
| RN-02 | T007, T022 |
| RN-03 | T008, T025 |
| RN-04 | T008, T025 |
| RN-05 | T008, T025, T028, T034 |
| RN-06 | T004, T013, T030 |
| RN-07 | T004, T013 |
| RN-08 | T004, T013 |
| RN-09 | T004, T013, T031 |
| RN-10 | T043 |
| RN-11 | T039, T041 |
| RN-12 | T009, T026 |
| CA-01 | T009, T049 |
| CA-02 | T049 |
| CA-03 | T007, T039 |
| CA-04 | T009, T036 |
| CA-05 | T009, T026 |
| CA-06 | T039, T049 |
| CA-07 | T008, T040 |
| CA-08 | T008, T040 |
| CA-09 | T008, T040 |
| CA-10 | T008, T028, T034 |
| CA-11 | T008, T025, T041 |
| CA-12 | T008, T025, T041 |
| CA-13 | T036, T041, T049 |
| CA-14 | T004, T010, T013 |
| CA-15 | T045, T049 |
| CA-16 | T042, T050 |
