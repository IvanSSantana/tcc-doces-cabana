# Tarefas — Carrinho

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`... — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Três ordens não são negociáveis.**
>
> **A Fase 2 (domínio) vem antes de tudo.** `ItemCarrinho` e
> `Produto.DisponivelParaCompra` são as invariantes que as camadas de cima
> assumem existir. Falhar acima com elas verdes significa "a camada de cima
> está errada"; sem essa ordem, significa qualquer coisa.
>
> **A Fase 6 (visitante) vem depois da Fase 5 (autenticado), e a Fase 7 (fusão)
> depois das duas.** Uma falha na fusão com as duas anteriores verdes significa
> "a fusão não aconteceu". Sem essa ordem, significa uma de três coisas — e foi
> exatamente esse erro de sequência que a `015` evitou de propósito ao deixar
> CA-08 por último.
>
> **A Fase 9 (renumeração) pode vir a qualquer momento**, mas não pode ser
> esquecida: escapou nas duas primeiras vezes que a cadeia deslocou.

---

## Fase 1 — Preparação e linha de base

- [x] **T001** — Criar branch `017-carrinho` a partir de `main`. *(feita ao criar a pasta da spec)*
- [x] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 403 e 117 verdes, herdados da `016`). **Confirmado: 403/403 unidade; 116/117 E2E na primeira rodada, com `Dado_ProdutoComAcento_Quando_BuscarSemAcentoEEmOutraCaixa_Entao_DeveEncontrar` falhando por instabilidade ambiental (busca por CEP/rede) — reexecutado isolado e passou. Não relacionado a esta feature.**
- [x] **T003** — Localizado: **um único teste** prova os controles desabilitados — `Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_QuantidadeECarrinhoDevemEstarDesabilitados` em `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs:291`. Verifica `.botao-adicionar-card` e `.botao-quantidade-card` desabilitados no cartão do catálogo. Não existe teste equivalente para o botão "Adicionar ao carrinho" da página do produto (`Detalhes.cshtml`) — ele nunca foi coberto. Este teste será reescrito na Fase 8 (T045).

## Fase 2 — Domínio

- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Entities/ItemCarrinhoTests.cs` (criar): construtor recusa produto e usuário vazios; quantidade fora de 1–99 recusada nos dois extremos; `Acrescentar` que estouraria o teto para em 99 (RN-02); `AlterarQuantidade` valida igual. Ver falhar.
- [x] **T005** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: `DisponivelParaCompra` verdadeiro só para `Ativo`; falso para `Inativo` e para `ForaDeEstoque` (RN-06). Ver falhar.
- [x] **T006** — Confirmado: falhas de compilação por `ItemCarrinho` e `DisponivelParaCompra` não existirem — motivo certo.
- [x] **T007** — `DocesCabana.Domain/Entities/ItemCarrinho.cs` (criado): `private set`, construtor validante, `protected Ctor()`, constantes `QuantidadeMinima`/`QuantidadeMaxima`, `AlterarQuantidade` e `Acrescentar`.
- [x] **T008** — `DocesCabana.Domain/Entities/Produto.cs`: `DisponivelParaCompra()` como método.
- [x] **T009** — `dotnet test DocesCabana.Tests`: **422 verdes** (403 + 19 novos).

## Fase 3 — Persistência

- [x] **T010** — `CarrinhoIntegrationTests.cs` (criado): 8 testes — chave composta recusa par repetido (RN-01), isolamento entre usuários (RN-03), `Include` do produto, `ContarItens` soma quantidades, `Buscar`/`Adicionar`/`Remover`. Ver falhar.
- [x] **T011** — `IItemCarrinhoRepository.cs` (criado): assinaturas do plano §5.
- [x] **T012** — `ItemCarrinhoConfiguration.cs` (criado) e `DbSet<ItemCarrinho> ItensCarrinho`: chave composta, FKs com `Restrict`.
- [x] **T013** — `ItemCarrinhoRepository.cs` (criado). `Buscar` sem `AsNoTracking`.
- [x] **T014** — Migration `20260823163627_AddItemCarrinho` — chave composta, FKs `Restrict`, índice em `ProdutoId`. Conferida.
- [x] **T015** — `dotnet test DocesCabana.Tests`: **8/8** em `CarrinhoIntegrationTests`.

## Fase 4 — Regras na aplicação

- [x] **T016** `[P]` — `CarrinhoServiceTests.cs` (criado): 17 testes cobrindo acrescentar (novo/soma/teto/indisponível/inexistente), alterar quantidade (atualiza/satura/remove abaixo de 1/inexistente), remover, subtotal ignorando indisponível, preço sempre atual, `ContarItens`. Ver falhar.
- [x] **T017** — Confirmado: falha de compilação por `CarrinhoService` não existir.
- [x] **T018** `[P]` — `MotivoIndisponibilidade.cs` e os três DTOs (criados).
- [x] **T019** `[P]` — `CarrinhoMapper.cs` (criado): `ToDTO` para o carrinho persistido e `Montar` para pares `(Produto, Quantidade)`, reaproveitado pelo avulso na Fase 6.
- [x] **T020** — `ICarrinhoService.cs` (dez métodos, plano §5) e `CarrinhoService.cs` (criados): as cinco operações persistidas implementadas; as quatro avulsas e `Fundir` como `NotImplementedException`, a serem preenchidas nas Fases 6 e 7.
- [x] **T021** — `ApplicationDependencyInjection.cs`: `IItemCarrinhoRepository`/`ItemCarrinhoRepository` e `ICarrinhoService`/`CarrinhoService` registrados.
- [x] **T022** — `dotnet test DocesCabana.Tests`: **447 verdes**, suíte inteira, do zero.

## Fase 5 — A tela, para quem entrou

- [x] **T023** — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs` (criado): 9 testes — autenticado usa o serviço; requisição assíncrona devolve `PartialView`/JSON e a comum redireciona; `Index` exige autenticação; escrita sem sessão devolve 401 (assíncrono) ou redireciona para login. Falhou por falta do tipo `CarrinhoController` antes de T026, como esperado.
- [x] **T024** — `DocesCabana.Tests.E2E/Paginas/PaginaCarrinho.cs` (criado) e `Fluxos/CarrinhoTests.cs` (criado): 10 testes — CA-05 a CA-11 e CA-17 a CA-19 — ver, alterar, remover, os dois limites, vazio, permanência, e o item indisponível (dois motivos). Seed via `fetch` real contra `CarrinhoController.Acrescentar` (`PaginaCarrinho.SemearItem`), já que o cartão/produto ainda não oferece a UI (isso é Fase 8). Status de produto alterado direto no SQLite de teste, na ausência de tela administrativa (mesma limitação já registrada pela `015`/CA-10). Falhou por falta do controlador antes de T026, como esperado.
- [x] **T025** — Confirmado: build antes de T026 falhava só por `CarrinhoController` não existir (unidade e E2E dependiam dele).
- [x] **T026** — `DocesCabana.MVC/Controllers/CarrinhoController.cs` (criado): `Index`, `Acrescentar`, `AlterarQuantidade`, `Remover`, com `[ValidateAntiForgeryToken]` nas três de escrita. **Sem `[Authorize]` nas de escrita** — checam `UsuarioAtualId` manualmente (mesmo padrão de `FavoritoController.Alternar`); `[Authorize]` provisório só em `Index`, até a Fase 6 decidir sozinha qual carrinho montar.
- [x] **T027** — `DocesCabana.MVC/Views/Carrinho/Index.cshtml` e `_ItensDoCarrinho.cshtml` (criados): a tela e o bloco que a atualização sem recarga substitui. Item indisponível sinalizado, com o motivo (RF-16, mensagens distintas para Inativo/ForaDeEstoque), fora do subtotal (RF-17), e o fechamento presente, desabilitado e sinalizado (RF-20/RN-08).
- [x] **T028** — `DocesCabana.MVC/wwwroot/css/pages/carrinho.css` (criado).
- [x] **T029** — `dotnet test DocesCabana.Tests`: **456 verdes** (447 + 9 da `CarrinhoControllerTests`). `dotnet test DocesCabana.Tests.E2E`: **10/10 verdes em `CarrinhoTests`**; suíte E2E inteira, **127/127 verdes** (nenhuma flake nesta rodada). Corrigido no caminho: `AlterarStatusDoProduto` do teste E2E comparava `ProdutoId` sem normalizar caixa (EF Core grava o GUID em maiúsculas no SQLite; `Guid.ToString()` é minúsculo — a comparação `TEXT` é case-sensitive), e dois testes que alteravam o status de um produto global da suíte não o restauravam para `Ativo` ao final, vazando para o `ObterProdutoAtivo()` de testes seguintes (que then recebiam 400 ao semear um produto na prática indisponível). Ambos corrigidos em `CarrinhoTests.cs`.

## Fase 6 — O visitante

- [x] **T030** — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`: 14 testes novos — as operações avulsas aplicam **as mesmas regras** das persistidas — somar, limitar, recusar indisponível. Falharam com `NotImplementedException`, como esperado.
- [x] **T031** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs`: CA-12 — visitante acrescenta, vê e altera. Falhou redirecionando para login (`Index` ainda com `[Authorize]`), como esperado.
- [x] **T032** — Confirmado: T030 falha por `NotImplementedException` nas quatro operações avulsas; T031 falha porque `Index` exige autenticação e a sessão não está ligada.
- [x] **T033** — `DocesCabana.Application/Services/CarrinhoService.cs`: as quatro operações avulsas implementadas. **Nenhuma regra nova** — reaproveitam `ItemCarrinho.QuantidadeMinima/Maxima` e `BuscarProdutoDisponivel`, aplicadas sobre `List<ItemDoCarrinhoDTO>` em vez do banco.
- [x] **T034** — `DocesCabana.MVC/Program.cs`: `AddSession()` e `UseSession()`. **Logo após `UseRouting` e antes de `UseAuthentication`** (plano §9, risco 1).
- [x] **T035** — `DocesCabana.MVC/Helpers/CarrinhoDaSessao.cs` (criado): `Ler`/`Escrever`/`Limpar` — extensões de `ISession`, JSON via `System.Text.Json`. **Só isso** — nenhuma regra de negócio mora aqui.
- [x] **T036** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `[Authorize]` removido de `Index`; as quatro ações passam a escolher entre banco e sessão conforme `UsuarioAtualId`. `CarrinhoControllerTests.cs` atualizado: os dois testes que provavam o desafio de login do visitante (interinos da Fase 5) foram substituídos por testes que provam a sessão — comportamento já previsto pelo T026 original ("elas atendem visitante também (Fase 6)"), não uma surpresa do plano.
- [x] **T037** — `dotnet test DocesCabana.Tests`: **470 verdes**. `dotnet test DocesCabana.Tests.E2E`: **11/11 verdes em `CarrinhoTests`** (10 da Fase 5 + CA-12); suíte E2E inteira, **128/128 verdes**.

## Fase 7 — A fusão

- [x] **T038** — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`: 5 testes novos — `Fundir` soma as quantidades do mesmo produto (RN-05), limita ao teto, traz os produtos que só existiam num dos lados, e não toca o repositório quando a sessão está vazia. Falharam com `NotImplementedException`, como esperado.
- [x] **T039** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs`: `Dado_CarrinhosNosDoisLados_Quando_Entrar_Entao_AsQuantidadesDevemSomar` (CA-13) e `Dado_FusaoConcluida_Quando_VoltarComoVisitante_Entao_OCarrinhoAvulsoDeveEstarVazio` (CA-14), nomes exatos do plano §7. **Escritos por último de propósito.** No caminho, corrigido um problema real de ordenação nos próprios testes (não da feature): depois de `Sair()`, a página de login usa `_LayoutNaoAutenticado`, sem `@Html.AntiForgeryToken()` — `SemearItem` precisa de uma página com o token antes de rodar; adicionada uma navegação para `UrlBase` entre o logout e o próximo `SemearItem`.
- [x] **T040** — Confirmado: ambos falharam porque o item permanecia com a quantidade original (sem soma) e o carrinho avulso continuava com o item depois do login — não porque um dos dois lados (persistido/avulso) parou de funcionar; as suítes das Fases 5 e 6 continuavam verdes.
- [x] **T041** — `DocesCabana.Application/Services/CarrinhoService.cs`: `Fundir` implementado — soma via `ItemCarrinho.Acrescentar` quando o produto já existe no carrinho guardado, cria novo item quando só existia na sessão; nenhuma checagem de disponibilidade (mesma regra do item já guardado que fica indisponível: continua no carrinho, RN-07).
- [x] **T042** — `DocesCabana.MVC/Filters/FiltroFusaoDeCarrinho.cs` (criado), registrado em `Program.cs` como filtro global (`options.Filters.Add<FiltroFusaoDeCarrinho>()`): requisição autenticada com carrinho na sessão funde e **limpa a sessão na mesma requisição** (plano §9, risco 2).
- [x] **T043** — `dotnet test DocesCabana.Tests`: **475 verdes**. Testes de fusão (CA-13/CA-14) rodados **três vezes seguidas, isolados**: verdes nas três. Suíte E2E inteira: **130/130 verdes**.

## Fase 8 — Ligar os controles que estavam apagados

- [x] **T044** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs` e `CatalogoTests.cs`: CA-01, CA-02, CA-15, CA-16 (mais CA-20 e CA-21, escritos junto por serem a mesma fase) — acrescentar do cartão, acrescentar da página do produto com quantidade, o contador e o atalho do cabeçalho, sem JavaScript e sem recarga. Falharam por falta dos controles/rotas/bolha, como esperado.
- [x] **T045** — **Reescrito**: `Dado_Catalogo_Quando_OlharOsControlesDoCard_Entao_QuantidadeECarrinhoDevemFuncionar` (era `...DevemEstarDesabilitados`) — prova que os controles funcionam. Correção esperada, não regressão.
- [x] **T046** — `DocesCabana.MVC/Views/Shared/_Layout.cshtml`: `<form id="formulario-carrinho">`, irmão do de favorito, e `carrinho.js` carregado.
- [x] **T047** — `DocesCabana.MVC/Views/Shared/Components/CardProduto/Default.cshtml`: controles reabilitados; botão "Adicionar" associado por `form=` com `produtoId` no próprio `name`/`value`. **Desvio deliberado do plano**: a quantidade do cartão ficou como `<span data-quantidade-valor>` (não `<input type="hidden">`) — um hidden associado ao formulário compartilhado por `form=` entraria na submissão de TODOS os cartões da grade a cada envio, não só do clicado (achado ao desenhar T052, antes de escrever qualquer view). `carrinho.js` lê o `<span>` do cartão do botão que disparou o envio e substitui `quantidade` no `FormData` antes do `fetch` — preserva a intenção do plano (sem script, o botão soma uma unidade) sem o defeito.
- [x] **T048** — `DocesCabana.MVC/Views/Produto/Detalhes.cshtml`: o `<span>` do seletor virou `<input type="number" min="1" max="99" form="formulario-carrinho">`, e o botão "Adicionar" passa a submeter (RF-02, RF-18). Sem a ambiguidade do cartão — só existe um seletor nesta página.
- [x] **T049** — `DocesCabana.MVC/wwwroot/js/pages/produto.js`: os ± passam a acionar `campo.value` do `<input>`, não `textContent` de um `<span>`.
- [x] **T050** — `DocesCabana.MVC/ViewComponents/Header.cs`: parâmetro `itensCarrinho` removido (estava morto: `ViewData` era escrito e nunca lido pela view); injetado `ICarrinhoService`, conta sozinho — autenticado do banco (`ContarItens`), visitante somando as quantidades da sessão (`CarrinhoDaSessao.Ler()`), sem checar disponibilidade (mesma semântica de `TotalDeItens`).
- [x] **T051** — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: o atalho deixou de ser `href="#"` (`asp-controller="Carrinho" asp-action="Index"`); bolha com a contagem, `hidden` quando zero.
- [x] **T052** `[P]` — `DocesCabana.MVC/wwwroot/js/components/carrinho.js` (criado): uma função trata a resposta dos três caminhos (acrescentar/alterar/remover, mesmo bloco `_ItensDoCarrinho` devolvido pelos três); interceptação por delegação de evento (`document.addEventListener("submit", …)`, mesmo padrão de `favorito.js`); atualiza a bolha do cabeçalho sempre e troca `#itens-carrinho` inteiro só quando a página atual o tem (tela do carrinho); os ± do cartão ajustam o `<span>` local.
- [x] **T053** `[P]` — `wwwroot/css/components/header.css` (bolha `.bolha-carrinho`/`.icone-com-bolha`) e `card-produto.css` (comentário do `:disabled` corrigido — só desabilita quando o produto não está disponível para compra, não mais sempre).
- [x] **T054** — `dotnet test DocesCabana.Tests`: **475 verdes** (sem mudança de contagem — Fase 8 é só MVC/JS/CSS). `dotnet test DocesCabana.Tests.E2E`: **64/64 verdes em `CarrinhoTests` + `CatalogoTests`** (incluindo CA-20 sem JavaScript e CA-21 sem recarga); suíte E2E inteira, **136/136 verdes**. No caminho, uma correção no próprio teste de CA-20 (não da feature): cadastrar uma conta nova sem JavaScript não funciona porque o cadastro depende de máscaras em JS para celular/CPF/data (`autenticacao.js`) — trocado para o cliente do seed (`AplicacaoEmExecucao.EmailClienteSeed`), mesmo padrão já usado por `FavoritosTests`.

## Fase 9 — Renumeração da cadeia da loja

- [x] **T055** — `grep -rn "spec 0[0-9][0-9]"` na base inteira: 100+ arquivos, todos referências corretas a specs já implementadas (012, 014, 015, 016 etc.) — nenhuma obsoleta. Confirmado o que a própria tarefa previa: esta feature não desloca a cadeia (`017` já era o número reservado do Carrinho desde que a cadeia foi traçada na `016`), só troca de posição com "Estoque" dentro da cadeia futura (T056).
- [x] **T056** — `specs/README.md`: a cadeia passa a ser Carrinho `017` (agora Implementada, com link), Endereços `018`, Fechamento `019`, Estoque `020`, Pagamento `021`; a nota de numeração registra o quinto deslocamento (Carrinho trocou de posição com Estoque — RN-06 já cobre indisponibilidade por status, sem depender de tabela de estoque própria); as perguntas em aberto sobre carrinho de visitante e reserva de estoque foram marcadas como resolvidas pela própria `017`; parágrafo de fechamento da `017` acrescentado à narrativa "Ordem executada", registrando os dois desvios do plano.
- [x] **T057** — `ModelagemBancoTCC.dbml`: tabela `ItemCarrinho` acrescentada (mesmo desenho de chave composta do `Favorito`) e as duas referências (`fk_ItemCarrinho_ProdutoId_Produto`, `fk_ItemCarrinho_UsuarioId_Usuario`).
- [x] **T058** — `docs/arquitetura.md`: `/Carrinho` na tabela de páginas (§5); `#formulario-carrinho` ao lado do `#formulario-favorito` na nota sobre os dois layouts; `AddSession`/`UseSession` no pipeline do `Program.cs` (§2.1), com a posição justificada; o ramo do `Carrinho` em `FilterException` na seção de tratamento por camada (§8.2). Conferidas as dívidas da baseline (`specs/000-baseline/spec.md` §6) — nenhuma delas é sobre carrinho; nada a riscar.

## Fase 10 — Fechamento

- [x] **T059** — `dotnet clean` + `dotnet build`: sucesso, **0 erro** (só o aviso pré-existente `NU1903` do pacote `Microsoft.Data.Sqlite`/`SQLitePCLRaw`, alheio a esta feature). `dotnet test DocesCabana.Tests`: **475/475**. `dotnet test DocesCabana.Tests.E2E`: 135/136 na primeira rodada — `LoginTests.Dado_SenhaErrada_Quando_Entrar_Entao_DeveMostrarCredencialIncorreta` (não tocado por esta feature) expirou esperando navegação; reexecutado isolado, passou, e uma segunda rodada completa da suíte inteira deu **136/136** — confirmado instabilidade ambiental, mesmo padrão já registrado em T002, não regressão.
- [ ] **T060** — Subir a aplicação e percorrer **cada critério de aceite** no navegador. **Não executado por este agente** — exige um humano olhando (aparência da tela, a bolha do contador, o item indisponível sinalizado, o cartão com os controles vivos ao lado do coração). Um smoke test (`dotnet run` + `curl`) confirmou `200 OK` sem exceção em `/`, `/Catalogo` e `/Carrinho` — prova que renderiza, não que está correto visualmente. Ver `checklist.md`, seção "Verificação manual pendente".
- [ ] **T061** — Percorrer ao vivo o fluxo do visitante inteiro. **Não executado por este agente**, mesmo motivo do T060 — coberto por `CarrinhoTests.cs` (CA-12/13/14, automatizado e verde), mas não repetido manualmente num navegador.
- [x] **T062** — `checklist.md` preenchido, registrando o que foi provado por teste (a maioria) e o que só a verificação ao vivo mostraria (T060/T061, listados numa seção própria).
- [x] **T063** — Status da spec → *Implementada*; status do plano → *Executado*; linha da `017` em `specs/README.md` com link para spec/plan/tasks/checklist.
- [x] **T064** — Cadeia da loja atualizada (T056): `017` marcada *Implementada*. Backlog: o item "Escrever avaliação de produto" apontava para "carrinho" genérico — corrigido para `017` (já existe) + `019` (o que falta de fato: um pedido fechado para checar elegibilidade). O que esta feature **não** encerra fica sinalizado na própria tela: o botão "Finalizar compra" nasce desabilitado, com `title="Fechamento de pedido ainda não disponível"` — só a `019` liga.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T044, T047 |
| RF-02 | T044, T048 |
| RF-03 | T016, T020 |
| RF-04 | T005, T008, T016, T020 |
| RF-05 | T024, T027 |
| RF-06 | T024, T026, T027 |
| RF-07 | T024, T026 |
| RF-08 | T019, T027 |
| RF-09 | T024, T027 |
| RF-10 | T010, T013, T024 |
| RF-11 | T031, T033, T034, T035 |
| RF-12 | T038, T041, T042 |
| RF-13 | T039, T042 |
| RF-14 | T044, T050, T051 |
| RF-15 | T044, T051 |
| RF-16 | T018, T019, T027 |
| RF-17 | T016, T019 |
| RF-18 | T047, T048, T054 |
| RF-19 | T052, T054 |
| RF-20 | T027 |
| RN-01 | T010, T016, T012 |
| RN-02 | T004, T007 |
| RN-03 | T010, T026 |
| RN-04 | T016, T019 |
| RN-05 | T038, T041 |
| RN-06 | T005, T008, T016 |
| RN-07 | T024, T027 |
| RN-08 | T027 |
| CA-01, CA-02, CA-15, CA-16 | T044 |
| CA-03 a CA-04 | T016 |
| CA-05 a CA-11, CA-17 a CA-19 | T024 |
| CA-12 | T031 |
| CA-13, CA-14 | T039 |
| CA-20, CA-21 | T054 |
| CA-22 | T010, T023 |
| CA-23 | T027 |
