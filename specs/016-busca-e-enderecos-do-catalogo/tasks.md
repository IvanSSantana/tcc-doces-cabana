# Tarefas — Busca e endereços do catálogo

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Duas ordens não são negociáveis.**
>
> **T004 é a primeira tarefa de código, antes de qualquer arquivo de CSS ser
> tocado.** Ele guarda o estado atual das cinco telas de formulário lido do
> navegador. Escrito depois da extração, confirmaria o resultado da mudança em
> vez de guardar o que existia antes — e RF-18 deixaria de ser verificável.
>
> **A Fase 3 (normalização) vem antes da Fase 4 (esquema), que vem antes da
> Fase 5 (busca).** Cada uma depende do derivado da anterior existir. Falhar na
> Fase 5 com as duas anteriores verdes significa "a consulta está errada"; sem
> essa ordem, significa qualquer uma de três coisas.
>
> As Fases 2, 7 e 8 são independentes entre si e das demais — podem ser
> reordenadas se houver motivo.

---

## Fase 1 — Preparação e linha de base

- [x] **T001** — Criar branch `016-busca-e-enderecos-do-catalogo` a partir de `main`. *(feita ao criar a pasta da spec)*
- [x] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 376 e 93 verdes, herdados da `015`). **376 e 93 verdes** — um teste (`PaginasInstitucionaisTests`) falhou por `ERR_NO_BUFFER_SPACE` na primeira rodada, ambiental; isolado, passou.
- [x] **T003** — Subir a aplicação e **capturar as cinco telas de formulário** — `Autenticacao/Login`, `Cadastro`, `EsqueceuSenha`, `RedefinirSenha` e `Admin/Administrador/Cadastro` — mais o `Admin/Produto/Cadastro` como está hoje, a 1440px e a 375px. São a linha de base da Fase 7. Superado por T004, que mede o mesmo estado por asserção automatizada em vez de captura visual.
- [x] **T004** — `DocesCabana.Tests.E2E/Fluxos/FormularioTests.cs` (criar): CA-19 — nas cinco telas acima, largura e altura do campo de texto, raio da borda, cor do rótulo e espaçamento entre campos, lidos do navegador. **Verde contra o código intocado** (5/5).

## Fase 2 — Endereço legível de subcategoria

- [x] **T005** `[P]` — `DocesCabana.Tests/Units/Servicos/ApelidoTests.cs`: RN-03 — percorrer a taxonomia real e verificar, **categoria por categoria**, que os apelidos das subcategorias dela são distintos entre si. **Passou de imediato** — `Apelido.De` já era genérico; não havia código novo para este teste dirigir, só cobertura nova sobre a taxonomia real (registrado, não escondido).
- [x] **T006** `[P]` — `DocesCabana.Tests/Units/Services/CatalogoServiceTests.cs`: apelido de subcategoria vira identificador antes de chegar ao repositório; apelido desconhecido na categoria é ignorado e o catálogo dela é montado inteiro (RN-04); mesmo apelido em categorias diferentes não se confunde (RN-03). Reescrito para `CriteriosDoCatalogoDTO`.
- [x] **T007** — Confirmado: falha de compilação por `CriteriosDoCatalogoDTO` não existir — motivo certo.
- [x] **T008** — `SubcategoriaDTO.cs` e `SubcategoriaMapper.cs` (criado, extraído de `CategoriaMapper`): apelido derivado do nome.
- [x] **T009** — `CriteriosDoCatalogoDTO.cs` (criado): sem termo ainda.
- [x] **T010** — `CatalogoDTO.cs`: `SubcategoriasMarcadas` agora `IReadOnlyCollection<string>`; `Termo` também adiantado (nulo até a Fase 5) para não deixar `_Paginacao.cshtml` com referência quebrada.
- [x] **T011** — `ICatalogoService`/`CatalogoService`: `Montar(CriteriosDoCatalogoDTO, int, Guid?)`; resolve apelidos de subcategoria contra `categoriaAtual.Subcategorias`. Repositório intocado.
- [x] **T012** — `EnderecoDoCatalogo.cs` (criado): parâmetros explícitos (categoria, subcategorias, sem açúcar, ordenação, termo, página) — não recebe `CatalogoDTO` inteiro, para servir tanto a paginação (tudo) quanto o cabeçalho (só categoria+ordenação, na Fase 6).
- [x] **T013** — `CatalogoController.cs`: `subcategorias` agora `string[]?`.
- [x] **T014** `[P]` — `_BarraLateral.cshtml`: `value="@subcategoria.Apelido"`, comparação por apelido.
- [x] **T015** `[P]` — `_Paginacao.cshtml`: usa `EnderecoDoCatalogo.Montar`.
- [x] **T016** `[P]` — `Header/Default.cshtml`: `asp-route-subcategorias="@subcategoria.Apelido"`.
- [x] **T017** — `CatalogoTests.cs`: 5 novos testes (CA-12 a CA-16). Achado de teste corrigido no caminho: duas leituras de `Pagina.Url` logo após `NetworkIdle` pegavam o endereço antigo — mesma corrida `NetworkIdle`-vs-`.then()` já documentada na `015` (a requisição de rede carregava os dois apelidos corretamente; só o `pushState`, que roda dentro do `.then()`, ainda não tinha aplicado). Trocado por `ToHaveURLAsync` com regex (retry automático). O clique dentro do submenu do cabeçalho também precisou de `Force: true` — a transição de opacidade do CSS (`012`) deixa o link "instável" para a checagem padrão durante os 0.15s da animação.
- [x] **T018** — `dotnet test` nas duas suítes: **382 unidade / 45 `CatalogoTests` E2E, verdes**. Nenhum teste anterior construía endereço de subcategoria à mão além dos já corrigidos em `PaginaCatalogo.cs`/`CatalogoTests.cs`.

## Fase 3 — Normalização de texto no domínio

- [x] **T019** `[P]` — `TextoHelperTests.cs` (criado): 7 casos, incluindo os nomes reais da loja. Ver falhar.
- [x] **T020** `[P]` — `ProdutoTests.cs`: nasce do nome, acompanha `AlterarNome`. Ver falhar.
- [x] **T021** — Confirmado: falha de compilação (`TextoHelper`/`NomeNormalizado` não existem) — motivo certo.
- [x] **T022** — `TextoHelper.cs` (criado): corpo extraído de `Apelido.RemoverAcentos`.
- [x] **T023** — `Apelido.cs`: consome `TextoHelper.Normalizar`. Extração confirmada — `ApelidoTests` (10 casos, incluindo os da Fase 2) seguem verdes sem alteração própria.
- [x] **T024** — `Produto.cs`: `NomeNormalizado` derivado no construtor e em `AlterarNome`.
- [x] **T025** — `dotnet test DocesCabana.Tests`: **390 verdes** (era 382 ao fim da Fase 2 + 8 novos: 7 `TextoHelperTests` + 2 `ProdutoTests` − 1 duplicidade de contagem). Suíte E2E completa também conferida: **103/103**.

## Fase 4 — Esquema e preenchimento retroativo

- [x] **T026** — `CatalogoRepositoryIntegrationTests.cs`: dois testes — derivado vazio fica encontrável após o preenchimento; base já correta não muda nada (idempotência). Ver falhar.
- [x] **T027** — `ProdutoConfiguration.cs`: `NomeNormalizado` obrigatório, 255, `HasDefaultValue("")`.
- [x] **T028** — Migration `20260822142339_AddProdutoNomeNormalizado` — `AddColumn` com `defaultValue: ""`. Conferida.
- [x] **T029** — `DbInitializer.cs`: `PreencherNomesNormalizados` (público, por `IServiceProvider`, chamado do `Program.cs` logo após `Migrar`, **fora do gate de "não produção"** — não é dado de demonstração) e uma sobrecarga `internal` por `DocesCabanaDbContext`, testável direto. **Desvio do plano registrado**: usa `context.SaveChangesAsync()` direto, não `IUnitOfWork` — segue a convenção que o próprio `DbInitializer` já usa em `Semear`/`SemearAdministrador` (bootstrap de infraestrutura em `MVC`, não escrita de caso de uso da `Application`); `IUnitOfWork` não está disponível neste arquivo e introduzi-lo aqui quebraria o padrão estabelecido do arquivo inteiro.
- [x] **T030** — `dotnet test DocesCabana.Tests`: **392 verdes**. Verificado ao vivo contra `docescabana.db` real (dev, datado de antes desta migration): `dotnet ef database update` aplicou a coluna (`NomeNormalizado=''` em todas as linhas antigas); um teste temporário (apagado depois) confirmou `PreencherNomesNormalizados` corrigindo todas — "Café 11" → "cafe 11", etc. Base de dev fica migrada e corrigida ao final (backup salvo em `%TEMP%` antes de tocar).

## Fase 5 — Busca no repositório e na aplicação

- [x] **T031** `[P]` — `CatalogoRepositoryIntegrationTests.cs`: 5 novos testes — acento, trecho no meio, produto inativo (RN-06), curinga SQL literal, termo nulo/vazio não filtra. Ver falhar.
- [x] **T032** `[P]` — `CatalogoServiceTests.cs`: termo normalizado repassado; nulo/vazio/só-espaço não filtra; `CatalogoDTO.Termo` devolve o termo cru. Ver falhar.
- [x] **T033** `[P]` — `CatalogoControllerTests.cs`: termo chega ao serviço sem deformação. Ver falhar.
- [x] **T034** — Confirmado: falhas de compilação (parâmetro/propriedade `Termo`/`TermoNormalizado` inexistentes) — motivo certo.
- [x] **T035** — `FiltroCatalogoDTO`/`CriteriosDoCatalogoDTO`: `Termo`/`TermoNormalizado` acrescentados, ambos opcionais com padrão `null` (preserva os construtores de 4 args já em uso pelos testes anteriores).
- [x] **T036** — `ProdutoRepository.ConstruirConsulta`: `Where(p => p.NomeNormalizado.Contains(...))` guardado por `!string.IsNullOrWhiteSpace`.
- [x] **T037** — `CatalogoService.Montar`: normaliza via `TextoHelper`; `CatalogoDTO.Termo` recebe o termo cru.
- [x] **T038** — `CatalogoController.Index`: parâmetro `termo`; `EnderecoDoCatalogo` já suportava (Fase 2).
- [x] **T039** — `dotnet test DocesCabana.Tests`: **403 verdes**. Dois achados de teste corrigidos no caminho (não de aplicação): `"Um"` violava o mínimo de 3 caracteres do nome de produto; o teste de acento passava o termo cru ("CAFE") onde o contrato do repositório espera o termo **já normalizado** pelo serviço — corrigido para `"cafe"`, condizente com `TermoNormalizado`.

## Fase 6 — Busca na tela

- [x] **T040** — `PaginaInicial.cs` e `PaginaCatalogo.cs`: `BarraDePesquisa`, `BotaoPesquisar`, `Buscar(termo)`, `EtiquetaDeBusca`, `BotaoRemoverBusca`.
- [x] **T041** — `BuscaTests.cs` (criado): 12 testes, CA-01 a CA-11. Ver falhar.
- [x] **T042** — `HeaderViewComponent`/`Header/Default.cshtml`: `<form method="get">` para o catálogo, reexibindo `ViewData["TermoDeBusca"]` lido de `Request.Query["termo"]`.
- [x] **T043** — `Catalogo/Index.cshtml`: campo oculto do termo dentro de `#formulario-catalogo`; item de trilha "Resultados para...".
- [x] **T044** `[P]` — `_BarraLateral.cshtml`: migrado para `EnderecoDoCatalogo.Montar` (Todos e cada categoria), termo incluso.
- [x] **T045** `[P]` — `_ResultadoCatalogo.cshtml`: etiqueta removível (link puro) e mensagem de vazio condicional ao termo.
- [x] **T046** `[P]` — `catalogo.css`: `.etiqueta-busca-catalogo`/`.botao-remover-busca`. `header.css` **não precisou de alteração** — `.barra-pesquisa`/`.botao-pesquisar` já eram baseados em classe, não em tipo de elemento; trocar `<div>`/`<input>` solto por `<form>`/`<button type="submit">` não muda a aparência.
- [x] **T047** — `dotnet test DocesCabana.Tests.E2E`: **115/115** (103 + 12 novos). `catalogo.js` de fato não precisou de nenhuma linha — o termo é só mais um campo do formulário que ele já serializa. **Achado de aplicação corrigido no caminho**: o formulário de busca do cabeçalho, sem `asp-route-apelido` explícito, herdava o apelido de categoria **ambiente** da página atual (comportamento padrão do gerador de endereço do ASP.NET Core) — buscar de dentro de `/Catalogo/doces` submetia para `/Catalogo/doces?termo=...` em vez de `/Catalogo?termo=...`, violando RF-02. Corrigido com `asp-route-apelido="@((string?)null)"` explícito, que sobrepõe o valor ambiente em vez de herdá-lo.

## Fase 7 — Estilização do cadastro de produto

- [x] **T048** — `components/formulario.css` (criado): regras movidas literalmente.
- [x] **T049** — `pages/autenticacao.css`: só `#link-esqueceu-senha-login` restou (única regra genuinamente exclusiva do login).
- [x] **T050** — As seis telas linkam `components/formulario.css`.
- [x] **T051** — `dotnet test`: **T004 continuou verde** (5/5) contra o código já extraído — a extração foi literal, sem alteração de valor.
- [x] **T052** — `CadastroDeProdutoTests.cs`: 2 novos testes (CA-17, CA-18). Ver falhar.
- [x] **T053** — `Areas/Admin/Views/Produto/Cadastro.cshtml`: `<main class="container-autenticacao pagina-cadastro-produto">`, título, `.linha-dupla` para os dois pares.
- [x] **T054** — `cadastro_produto.css`: `@media (max-width: 767px)` empilhando os pares, escopado por `.pagina-cadastro-produto`.
- [x] **T055** — `dotnet test`: Fase 7 verde (9/9 em `CadastroDeProdutoTests`+`FormularioTests`), T004 incluído. **Dois achados registrados, não escondidos:**
  - **`.linha-dupla` nunca empilhava em nenhuma largura** — o plano presumiu que "já é o que o cadastro de administrador faz", e não é: não existe media query nenhuma para o componente. Corrigido **só para o cadastro de produto** (`.pagina-cadastro-produto .linha-dupla`, não em `formulario.css`), para não mudar a aparência das outras duas telas por conta própria — RF-18 protege exatamente isso. O comportamento em Cliente/Administrador continua como sempre foi; se um dia for corrigido para eles também, é decisão e feature própria.
  - **Erro de teste (não de aplicação), achado ao investigar o primeiro CA-18**: o teste original comparava a largura do campo contra a largura *total* do container, sem descontar o padding — e a 375px o `root font-size` cai para 14px (`site.css`, breakpoint 768px), então `1.5rem` de padding vale 21px, não 24px. Corrigido para descontar o padding computado antes de comparar.

## Fase 8 — Renumeração da cadeia da loja

- [x] **T056** — Varredura completa. Três referências de código corrigidas (`019`→`020`: `OrdenacaoCatalogo.cs`, `CatalogoController.SanearOrdenacao`, `ProdutoRepository.AplicarOrdenacao`); `specs/012-catalogo/spec.md` (`017`→`018` carrinho, `019`→`020` fechamento); `specs/013-correcoes-da-pagina-inicial/spec.md` (três ocorrências de `019`→`020`); `specs/015-favoritos-e-ajustes-do-catalogo/spec.md` (**achado extra**: já estava desatualizada antes desta feature — dizia "carrinho é `016`" quando o README já apontava `017`; corrigida direto para `018`). Os `tasks.md` de `013`/`014`/`015` **não foram tocados** — são diário de tarefas concluídas, não referência viva (mesmo padrão que `014`/`015` já seguiram: só documentação viva é corrigida a cada deslocamento).
- [x] **T057** — `specs/README.md`: linha do índice, "Ordem executada" (que também não incluía `015` — corrigido), parágrafo narrativo de `016`, tabela da cadeia (Estoque `017`, Carrinho `018`, Endereço `019`, Fechamento `020`, Pagamento `021`), as quatro perguntas em aberto, e a nota de numeração (quarto deslocamento).
- [x] **T058** — Conferido: `SanearOrdenacao` e o comentário equivalente em `ProdutoRepository`/`OrdenacaoCatalogo` corrigidos (T056). Os comentários sobre os controles desabilitados do cartão citam `spec 012`/`spec 015`, não um número de cadeia — nada a corrigir ali.

## Fase 9 — Fechamento

- [x] **T059** — `dotnet build` limpo, e as duas suítes rodadas do zero: **403 unidade, 117 E2E, ambas verdes numa rodada limpa** (sem instabilidade desta vez).
- [x] **T060** — Aplicação subida de verdade (`dotnet run`), verificada por requisição HTTP direta contra o servidor — login real com cookies, formulário de busca reexibindo o termo, cadastro de produto autenticado e estilizado. **Sem captura de tela** — registrado em `checklist.md` como limitação desta rodada, não escondido; o visual em si é coberto pelos testes E2E de estilo computado.
- [x] **T061** — Confirmado ao vivo: "cafe" → 3 produtos (Café), "CACHACA" → 7 produtos (Cachaça), "pelucia" → 5 produtos (Pelúcia). Termo vazio → 99 produtos (catálogo completo). Termo sem correspondência → mensagem mencionando o termo.
- [x] **T062** — `checklist.md` preenchido — achados, o que foi provado por teste, o que só a verificação HTTP ao vivo mostrou.
- [x] **T063** — Spec → *Implementada*; plano → *Executado*; linha em `specs/README.md` com link do checklist.
- [x] **T064** — Linha "Busca por texto" removida do backlog de `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T041, T042 |
| RF-02 | T032, T037, T041 |
| RF-03 | T019, T024, T031, T036 |
| RF-04 | T041, T043 |
| RF-05 | T038, T043, T044 |
| RF-06 | T042 |
| RF-07 | T045 |
| RF-08 | T045 |
| RF-09 | T032, T037 |
| RF-10 | T041, T045 |
| RF-11 | T031, T036 |
| RF-12 | T008, T013, T014 |
| RF-13 | T011, T012, T017 |
| RF-14 | T006, T011 |
| RF-15 | T014, T015, T016 |
| RF-16 | T050, T053, T054 |
| RF-17 | T052, T053 |
| RF-18 | T004, T048, T049, T051 |
| RN-01 | T036 |
| RN-02 | T019, T020, T024, T031 |
| RN-03 | T005, T008, T017 |
| RN-04 | T006, T011 |
| RN-05 | T038, T043, T044 |
| RN-06 | T031, T036 |
| CA-01 a CA-11 | T041 |
| CA-12 a CA-16 | T017 |
| CA-17, CA-18 | T052 |
| CA-19 | T004, T051, T055 |
