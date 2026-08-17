# Tarefas — Páginas institucionais

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md) · **Conteúdo:** [`conteudo-politica.md`](./conteudo-politica.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Implementação sempre depois do teste que a cobre, e o teste precisa ter
  falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Esta feature não tem Domínio, Aplicação nem Infraestrutura.** Não há
> entidade, DTO, validator, repositório nem migration (plano §4 e §6). As fases
> abaixo substituem as do template pelas camadas que a feature realmente toca.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `009-paginas-institucionais` a partir de `main`.
- [x] **T002** — Rodar `dotnet build` e `dotnet test DocesCabana.Tests`, registrar o total de testes como linha de base (esperado: 310 verdes, herdados da `008`).

## Fase 2 — Testes (devem falhar)

*Escreva, rode, veja vermelho. Só então passe para a Fase 3.*

- [x] **T003** `[P]` — `DocesCabana.Tests/Units/Controllers/InstitucionalControllerTests.cs`: `Privacidade()` e `QuemSomos()` devolvem `ViewResult` (CA-08).
- [x] **T004** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaPrivacidade.cs`: *page object* com `Abrir`, `Titulo`, `TitulosDeSecao`, `LinkDoEncarregado`. Todo *locator* escopado em `.pagina-institucional` — nunca na página inteira (plano §9, colisão com o modal de login).
- [x] **T005** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaQuemSomos.cs`: *page object* com `Abrir`, `FraseDeDestaque`, `Blocos`, `Eixo`.
- [x] **T006** — `DocesCabana.Tests.E2E/Fluxos/PaginasInstitucionaisTests.cs`: os oito testes E2E do mapeamento do plano §7 (CA-01 a CA-04, CA-06, CA-07, CA-09, CA-10, CA-12). Links do rodapé escopados em `footer`; o do modal, em `.modal-login`.
- [x] **T007** — Confirmar que T003–T006 falham pelo motivo certo: ação/rota inexistente, e não erro de compilação alheio.

## Fase 3 — Controller, rotas e ligação dos links

- [x] **T008** — `DocesCabana.MVC/Controllers/InstitucionalController.cs`: `Privacidade()` e `QuemSomos()`, `[HttpGet]`, sem dependência injetada, sem parâmetro, sem `[Authorize]` (RF-06). Views vazias por enquanto — o conteúdo entra nas Fases 4 e 5.
- [x] **T009** — Remover o andaime de RF-07: a ação `Privacidade()` de `DocesCabana.MVC/Controllers/HomeController.cs`, o arquivo `DocesCabana.MVC/Views/Home/Privacidade.cshtml` e o teste correspondente em `DocesCabana.Tests/Units/Controllers/HomeControllerTests.cs` (linha 47).
- [x] **T010** `[P]` — `DocesCabana.MVC/Views/Shared/_Footer.cshtml`: trocar os dois `href="#"` por `asp-controller="Institucional"` com `asp-action="QuemSomos"` e `asp-action="Privacidade"` (RF-03, RF-04).
- [x] **T011** `[P]` — `DocesCabana.MVC/Views/Shared/_ModalLogin.cshtml`: mesma ligação no link `.link-privacidade` (RF-05).
- [x] **T012** — Rodar `dotnet test DocesCabana.Tests`: T003 passa. Os E2E de conteúdo seguem vermelhos — é o esperado nesta fase.

## Fase 4 — Política de Privacidade

- [x] **T013** — `DocesCabana.MVC/Views/Institucional/Privacidade.cshtml`: título e parágrafos de abertura, e as 11 seções do [Anexo A](./conteudo-politica.md) na ordem, com a hierarquia de títulos do anexo (RF-08, RF-09, RF-10). Escapar `@` como `@@` no e-mail (plano §9).
- [x] **T014** — Marcar o e-mail do encarregado como link `mailto:` na seção "Contato" (RF-11).
- [x] **T015** — Conferência de transcrição: ler o Anexo A e a view lado a lado, seção por seção, e confirmar que nenhuma frase foi resumida, corrigida ou reordenada (RN-03). Esta tarefa **não é** revisão de código: é conferência de texto legal.
- [x] **T016** — Rodar os E2E: CA-04, CA-05 e CA-09 passam.

## Fase 5 — Quem Somos

- [x] **T017** — `DocesCabana.MVC/Models/BlocoInstitucionalViewModel.cs`: o `record` do plano §5.
- [x] **T018** — `DocesCabana.MVC/Views/Institucional/_BlocoInstitucional.cshtml`: um bloco, com título, texto e imagem. **Ordem no DOM é sempre título → texto → imagem**; quem inverte o lado é a grade, não a marcação — para que a leitura por leitor de tela não dependa do ziguezague visual.
- [x] **T019** — `DocesCabana.MVC/Views/Institucional/QuemSomos.cshtml`: faixa de destaque com a frase e a palavra "infância" em elemento próprio (RF-12), e o contêiner do ziguezague invocando a partial três vezes com `Invertido` em `false, true, false` (RF-13, RF-14, RF-15).
- [x] **T020** — Texto de cada bloco: `Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus tortor ipsum dolor sit.`, igual à referência (RF-13). Comentário no `.cshtml` marcando o ponto como conteúdo de preenchimento, pendente de texto real da loja (spec §8).
- [x] **T021** — Imagem de cada bloco (e da faixa de destaque): retângulo cinza de lugar reservado, sem arquivo de imagem — um `div` com fundo sólido e proporção fixa via CSS, com `alt`/rótulo indicando que é espaço reservado. Comentário marcando o ponto como pendente de foto real (spec §8).
- [x] **T022** — Rodar os E2E: CA-03 e CA-06 passam.

## Fase 6 — Estilo

- [x] **T023** — `DocesCabana.MVC/wwwroot/css/pages/institucional.css`: os cinco tokens do plano §3 declarados em `.pagina-institucional` — **nunca em `:root`** (plano §9).
- [x] **T024** — Estilo da política: coluna de leitura limitada, título de seção em coral na margem, conteúdo recuado sob ele, régua entre seções como borda em CSS (não `<hr>`), corpo em entrelinha `1.7`.
- [x] **T025** — Estilo do Quem Somos: faixa de destaque, grade de duas colunas, `Invertido` trocando a coluna de imagem e texto, e o eixo verde como `::before` do contêiner do ziguezague.
- [x] **T026** — Colapso responsivo: abaixo de 900px as duas páginas viram coluna única e o eixo verde **desaparece** — não vira linha vertical à esquerda (RN-05, RF-16).
- [x] **T027** — Foco de teclado visível em todo link das duas páginas (`:focus-visible`), e `prefers-reduced-motion` respeitado — o que, nesta feature, significa confirmar que não há animação a suprimir (plano §3).
- [x] **T028** — Ligar a folha de estilo nas duas views com `<link ... asp-append-version="true">` no corpo, como `Views/Produto/Detalhes.cshtml` já faz — `_Layout.cshtml` não tem seção de estilos.
- [x] **T029** — Rodar os E2E: CA-07 e CA-10 passam. A suíte inteira, verde.

## Fase 7 — Fechamento

- [x] **T030** — `dotnet build` sem warnings novos e `dotnet test DocesCabana.Tests` verde, comparado com a linha de base de T002.
- [x] **T031** — Suíte E2E completa verde (`dotnet test DocesCabana.Tests.E2E`), incluindo os fluxos herdados da `007` — o rodapé foi alterado e ele aparece em todas as telas.
- [x] **T032** — Subir a aplicação e percorrer manualmente CA-01 a CA-12, com atenção a CA-11, que não tem teste automatizado (plano §7).
- [x] **T033** — Preencher `checklist.md`, registrando explicitamente o que ficou provado por teste, o que por verificação manual, e o que não foi verificado.
- [x] **T034** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md` (índice, nota de ordem executada e a linha do backlog, se couber).

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T003, T008 |
| RF-02 | T003, T008 |
| RF-03 | T006, T010 |
| RF-04 | T006, T010 |
| RF-05 | T006, T011 |
| RF-06 | T003, T008 |
| RF-07 | T006, T009 |
| RF-08 | T004, T013 |
| RF-09 | T004, T013, T015 |
| RF-10 | T013, T024 |
| RF-11 | T004, T014 |
| RF-12 | T005, T019, T025 |
| RF-13 | T005, T017, T018, T019, T020, T021 |
| RF-14 | T006, T018, T019, T025 |
| RF-15 | T006, T019 |
| RF-16 | T006, T026 |
| RF-17 | T006 |
| RN-01 | T003, T008 |
| RN-02 | T008 |
| RN-03 | T013, T015 |
| RN-04 | T009, T010, T011 |
| RN-05 | T025, T026 |
| CA-01 | T006, T010, T032 |
| CA-02 | T006, T011, T032 |
| CA-03 | T006, T010, T022, T032 |
| CA-04 | T004, T013, T016, T032 |
| CA-05 | T004, T014, T016, T032 |
| CA-06 | T005, T019, T022, T032 |
| CA-07 | T006, T025, T029, T032 |
| CA-08 | T003, T006, T008, T032 |
| CA-09 | T006, T009, T016, T032 |
| CA-10 | T006, T026, T029, T032 |
| CA-11 | T027, T032 *(só verificação manual)* |
| CA-12 | T006, T032 |
