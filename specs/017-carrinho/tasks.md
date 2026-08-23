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

- [ ] **T001** — Criar branch `017-carrinho` a partir de `main`. *(feita ao criar a pasta da spec)*
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 403 e 117 verdes, herdados da `016`).
- [ ] **T003** — Localizar os testes que hoje provam que **os controles do cartão estão desabilitados** (`012`/`015`) e os que provam o botão da página do produto. Listá-los aqui: eles vão quebrar de propósito na Fase 8, e precisam ser **reescritos**, não removidos — passam a provar que funcionam.

## Fase 2 — Domínio

- [ ] **T004** `[P]` — `DocesCabana.Tests/Units/Entities/ItemCarrinhoTests.cs` (criar): construtor recusa produto e usuário vazios; quantidade fora de 1–99 recusada nos dois extremos; `Acrescentar` que estouraria o teto para em 99 (RN-02); `AlterarQuantidade` valida igual. Ver falhar.
- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: `DisponivelParaCompra` verdadeiro só para `Ativo`; falso para `Inativo` e para `ForaDeEstoque` (RN-06). Ver falhar.
- [ ] **T006** — Confirmar que T004 e T005 falham por não existir a entidade nem o método — e não por erro de compilação alheio.
- [ ] **T007** — `DocesCabana.Domain/Entities/ItemCarrinho.cs` (criar): `private set`, construtor validante, `protected Ctor()`, constantes `QuantidadeMinima`/`QuantidadeMaxima`, `AlterarQuantidade` e `Acrescentar`.
- [ ] **T008** — `DocesCabana.Domain/Entities/Produto.cs`: `DisponivelParaCompra()`. **Método, não propriedade** — propriedade computada o EF Core tentaria mapear para coluna (plano §1).
- [ ] **T009** — Rodar `dotnet test DocesCabana.Tests`: T004 e T005 passam, e nada mais mudou.

## Fase 3 — Persistência

- [ ] **T010** — `DocesCabana.Tests/Integration/Repositories/CarrinhoIntegrationTests.cs` (criar): a chave composta recusa o par `(usuário, produto)` repetido (RN-01); a busca por usuário não traz item de outro (RN-03); `ContarItens` soma quantidades, não linhas. Ver falhar.
- [ ] **T011** — `DocesCabana.Application/Contracts/Repositories/IItemCarrinhoRepository.cs` (criar): assinaturas do plano §5.
- [ ] **T012** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/ItemCarrinhoConfiguration.cs` (criar) e `DbSet<ItemCarrinho> ItensCarrinho` no contexto: chave composta, FKs com `Restrict` — espelhando `FavoritoConfiguration`.
- [ ] **T013** — `DocesCabana.Infrastructure/Repositories/ItemCarrinhoRepository.cs` (criar). `BuscarPorUsuario` com `Include` do produto; `Buscar` **sem** `AsNoTracking`, porque o item volta para ser alterado.
- [ ] **T014** — Migration: `dotnet ef migrations add AddItemCarrinho --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`. Conferir o arquivo gerado antes de seguir.
- [ ] **T015** — Rodar `dotnet test DocesCabana.Tests`: T010 passa.

## Fase 4 — Regras na aplicação

- [ ] **T016** `[P]` — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs` (criar): acrescentar produto novo; acrescentar o que já está **soma numa linha só** (RN-01); a soma para no teto (RN-02); produto indisponível é recusado com os dois motivos distinguíveis (RN-06); subtotal ignora indisponível (RF-17); preço vem do produto, não de coluna (RN-04). Ver falhar.
- [ ] **T017** — Confirmar que T016 falha por não existir serviço nem DTOs.
- [ ] **T018** `[P]` — `DocesCabana.Application/Enums/MotivoIndisponibilidade.cs` e os três DTOs (`ItemDoCarrinhoDTO`, `LinhaDoCarrinhoDTO`, `CarrinhoDTO`) (criar).
- [ ] **T019** `[P]` — `DocesCabana.Application/Mappings/CarrinhoMapper.cs` (criar): produtos + quantidades → `CarrinhoDTO`, com o subtotal **ignorando** as linhas indisponíveis.
- [ ] **T020** — `DocesCabana.Application/Contracts/Services/ICarrinhoService.cs` e `Services/CarrinhoService.cs` (criar): as cinco operações do carrinho persistido, commit por `IUnitOfWork.SalvarAlteracoes`.
- [ ] **T021** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: registrar repositório e serviço.
- [ ] **T022** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde.

## Fase 5 — A tela, para quem entrou

- [ ] **T023** — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs` (criar): autenticado usa o serviço; requisição assíncrona devolve JSON e a comum redireciona; `Index` exige autenticação. Ver falhar.
- [ ] **T024** — `DocesCabana.Tests.E2E/Paginas/PaginaCarrinho.cs` (criar) e `Fluxos/CarrinhoTests.cs` (criar): CA-05 a CA-11 e CA-17 a CA-19 — ver, alterar, remover, os dois limites, vazio, permanência, e o item indisponível. Ver falhar.
- [ ] **T025** — Confirmar que T023 e T024 falham por não existir controlador nem tela.
- [ ] **T026** — `DocesCabana.MVC/Controllers/CarrinhoController.cs` (criar): `Index`, `Acrescentar`, `AlterarQuantidade`, `Remover`, com `[ValidateAntiForgeryToken]` nas três de escrita. **Sem `[Authorize]` nas de escrita** — elas atendem visitante também (Fase 6); `Index` decide por conta própria qual carrinho montar.
- [ ] **T027** — `DocesCabana.MVC/Views/Carrinho/Index.cshtml` e `_ItensDoCarrinho.cshtml` (criar): a tela e o bloco que a atualização sem recarga substitui. Item indisponível sinalizado, com o motivo (RF-16), fora do subtotal (RF-17), e o fechamento presente e sinalizado (RF-20).
- [ ] **T028** — `DocesCabana.MVC/wwwroot/css/pages/carrinho.css` (criar).
- [ ] **T029** — Rodar as duas suítes: Fase 5 verde para o cliente autenticado.

## Fase 6 — O visitante

- [ ] **T030** — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`: as operações avulsas aplicam **as mesmas regras** das persistidas — somar, limitar, recusar indisponível. Ver falhar.
- [ ] **T031** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs`: CA-12 — visitante acrescenta, vê e altera. Ver falhar.
- [ ] **T032** — Confirmar que T030 e T031 falham porque as operações avulsas não existem e a sessão não está ligada.
- [ ] **T033** — `DocesCabana.Application/Services/CarrinhoService.cs`: as quatro operações avulsas. **Nenhuma regra nova** — as mesmas da Fase 4, aplicadas sobre uma lista em vez do banco.
- [ ] **T034** — `DocesCabana.MVC/Program.cs`: `AddSession` e `UseSession`. **Logo após `UseRouting` e antes de `UseAuthentication`** (plano §9, risco 1) — sessão lida antes do middleware rodar devolve vazio sem erro nenhum.
- [ ] **T035** — `DocesCabana.MVC/Helpers/CarrinhoDaSessao.cs` (criar): leitura e escrita da lista em JSON. **Só isso** — nenhuma regra de negócio mora aqui.
- [ ] **T036** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: as quatro ações passam a escolher entre banco e sessão conforme a autenticação.
- [ ] **T037** — Rodar as duas suítes: Fase 6 verde.

## Fase 7 — A fusão

- [ ] **T038** — `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs`: `Fundir` soma as quantidades do mesmo produto (RN-05), limita ao teto, e traz os produtos que só existiam num dos lados. Ver falhar.
- [ ] **T039** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs`: CA-13 e CA-14 — as quantidades somam ao entrar, e o carrinho avulso não sobra depois. **Escritos por último de propósito** (plano §7). Ver falhar.
- [ ] **T040** — Confirmar que T038 e T039 falham por não existir fusão — e não porque um dos dois lados parou de funcionar.
- [ ] **T041** — `DocesCabana.Application/Services/CarrinhoService.cs`: `Fundir`.
- [ ] **T042** — `DocesCabana.MVC/Filters/FiltroFusaoDeCarrinho.cs` (criar) e registro no `Program.cs`: requisição autenticada com carrinho na sessão funde e **limpa a sessão na mesma requisição** (plano §9, risco 2).
- [ ] **T043** — Rodar as duas suítes. Rodar **três vezes seguidas** os testes de fusão: é o fluxo mais frágil da feature, e falha intermitente aqui é o que mais custa depois.

## Fase 8 — Ligar os controles que estavam apagados

- [ ] **T044** — `DocesCabana.Tests.E2E/Fluxos/CarrinhoTests.cs` e `CatalogoTests.cs`: CA-01, CA-02, CA-15, CA-16 — acrescentar do cartão, acrescentar da página do produto com quantidade, o contador e o atalho do cabeçalho. Ver falhar.
- [ ] **T045** — **Reescrever** os testes listados em T003 que provam controles desabilitados: passam a provar que funcionam. Correção esperada, não regressão.
- [ ] **T046** — `DocesCabana.MVC/Views/Shared/_Layout.cshtml`: `<form id="formulario-carrinho">`, irmão do de favorito, e `carrinho.js`.
- [ ] **T047** — `DocesCabana.MVC/Views/Shared/Components/CardProduto/Default.cshtml`: controles reabilitados; botão associado por `form=` com `produtoId` no próprio `name`/`value`; `<input type="hidden">` da quantidade (plano §3). **Mesma solução de form aninhado que a `015` provou** (plano §9, risco 6).
- [ ] **T048** — `DocesCabana.MVC/Views/Produto/Detalhes.cshtml`: o `<span>` do seletor vira `<input type="number" min="1" max="99">`, e o botão passa a submeter (RF-02, RF-18).
- [ ] **T049** — `DocesCabana.MVC/wwwroot/js/pages/produto.js`: os ± passam a acionar o `<input>`, não o `<span>`.
- [ ] **T050** — `DocesCabana.MVC/ViewComponents/Header.cs`: **remover** o parâmetro `itensCarrinho`, injetar `ICarrinhoService` e contar sozinho — para visitante, contar da sessão (spec §10, achado).
- [ ] **T051** — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: o atalho deixa de ser `href="#"`; bolha com a contagem, escondida quando vazio.
- [ ] **T052** `[P]` — `DocesCabana.MVC/wwwroot/js/components/carrinho.js` (criar): interceptação do formulário, `fetch`, troca do bloco, e os ± acionando o campo escondido do cartão.
- [ ] **T053** `[P]` — `wwwroot/css/components/header.css` e `card-produto.css`: bolha do contador; controles sem aparência de desabilitado.
- [ ] **T054** — Rodar as duas suítes: CA-20 (sem JavaScript) e CA-21 (sem recarga) verdes, além do resto da fase.

## Fase 9 — Renumeração da cadeia da loja

- [ ] **T055** — `grep -rn "spec 0[0-9][0-9]"` na base inteira — código, comentário, spec antiga, README — e corrigir toda referência que a renumeração tornou obsoleta. Inclui **esta spec e este plano**.
- [ ] **T056** — `specs/README.md`: a cadeia passa a ser Carrinho `017`, Endereços `018`, Fechamento `019`, Estoque `020`, Processamento de pagamento `021`; a nota de numeração registra o quinto deslocamento.
- [ ] **T057** — `ModelagemBancoTCC.dbml`: acrescentar a tabela `ItemCarrinho` e as duas referências. O diagrama é entregável do TCC — desatualizá-lo é dívida silenciosa (plano §6).
- [ ] **T058** — `docs/arquitetura.md`: acrescentar o carrinho à tabela de páginas, a sessão ao pipeline do `Program.cs`, e o filtro de fusão à seção de tratamento por camada. O guia vive no repositório justamente para acompanhar.

## Fase 10 — Fechamento

- [ ] **T059** — `dotnet build` sem aviso e as duas suítes verdes, do zero.
- [ ] **T060** — Subir a aplicação e percorrer **cada critério de aceite** no navegador. Especialmente os que teste automatizado alcança mal: a aparência da tela, a bolha do contador, o item indisponível sinalizado, e o cartão com os controles vivos ao lado do coração.
- [ ] **T061** — Percorrer ao vivo o fluxo do visitante inteiro: montar carrinho deslogado, entrar, conferir que as quantidades somaram, sair, e conferir que o carrinho avulso não voltou.
- [ ] **T062** — Preencher `checklist.md`, registrando **o que foi provado por teste e o que só a verificação ao vivo mostrou**.
- [ ] **T063** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`, com o link do checklist.
- [ ] **T064** — Riscar do backlog e da cadeia o que esta feature encerra, e registrar o que ela **não** encerra: o fechamento continua sinalizado como indisponível até a `019`.

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
