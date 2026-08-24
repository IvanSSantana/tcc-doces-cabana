# Tarefas — Correções e pendências

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
> **A Fase 2 (CPF) vem antes de tudo.** É a correção com risco de derrubar a
> aplicação na subida — `Usuario` valida CPF no construtor, e o seed cria nove
> contas. Se algo estiver errado nessa premissa, é melhor descobrir na primeira
> fase, com o resto da base intacta, do que no fim.
>
> **O teste vermelho do CPF é o mais importante desta entrega.** Ele precisa
> falhar **por aceitar um CPF inválido** — não por não compilar. Um teste de
> regressão que nunca viu o bug acontecer não prova que o conserta.

---

## Fase 1 — Preparação e linha de base

- [x] **T001** — Criar branch `019-correcoes-e-pendencias` a partir de `main`. *(feita ao criar a pasta da spec)*
- [x] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 534 e 157 verdes, herdados da `018`). Build limpo; 534/534 unitários; E2E 156/157 (uma falha pré-existente e instável em `BuscaTests`, alheia a esta entrega — voltou a passar depois).
- [x] **T003** — Localizar os testes que esta entrega vai **quebrar de propósito**: `HomeControllerTests.Dado_ProdutosCadastrados_...` (afirma `BuscarTodosProdutos` chamado uma vez). Nenhum teste E2E fixa nome de produto da vitrine — `PaginaInicialTests` já usa contagem/limite, não nome; `CatalogoTests` só verifica ausência do inativo, que continua valendo.

## Fase 2 — A conferência do CPF

- [x] **T004** — `DocesCabana.Tests/Units/Helpers/CpfHelperTests.cs`: acrescentar os três CPFs com o **primeiro** dígito verificador errado (`52998224795`, `52998224705`, `52998224715`) como inválidos. Ver falhar.
- [x] **T005** — Confirmar que T004 falha **por aceitar CPF inválido** (`Assert.False` recebendo `true`), não por compilação. É a prova de que o teste viu o bug.
- [x] **T006** — `DocesCabana.Tests/Units/Helpers/CpfHelperTests.cs`: teste de guarda que percorre os **nove CPFs semeados** — os oito clientes da lista de `DbInitializer.cs` mais o do administrador (mesmo arquivo, na semeadura do admin) — e exige que todos sejam válidos. **Deve passar de primeira**: os nove foram conferidos dígito a dígito ao especificar. Se algum falhar, **pare**: o seed tem CPF inválido e o plano precisa mudar antes de seguir. Passou de primeira.
- [x] **T007** `[P]` — `DocesCabana.Tests/Units/Validators/CadastroDTOValidatorTests.cs`: um CPF com o primeiro dígito errado é recusado também na barreira de entrada (RF-01, CA-01). Ver falhar.
- [x] **T008** — `DocesCabana.Domain/Helpers/CpfHelper.cs`: extrair `CalcularDigito(string parcial, int[] multiplicadores)` e conferir **os dois** dígitos contra os informados (plano §5). Não consertar sem extrair — a duplicação é a causa raiz.
- [x] **T009** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde (547/547).
- [x] **T010** — Rodar `dotnet test DocesCabana.Tests.E2E`: o cadastro é o caminho de entrada de quase toda a suíte, e `GeradorDeDados.CpfValido` precisa continuar gerando CPF aceito. 157/157 verdes.

## Fase 3 — A consulta da vitrine

- [x] **T011** `[P]` — `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs`: `BuscarDestaquesDaVitrine` pede ao repositório exatamente o limite recebido (`Verify(..., pagina: 1, tamanhoDaPagina: 8)`, CA-06); ordena por `MelhorAvaliados`; consulta favoritos **só** quando autenticado (CA-12, `Times.Never` para visitante); marca os favoritados (CA-11). Ver falhar.
- [x] **T012** — Confirmar que T011 falha por não existir o método — e não por erro alheio. (Erro de compilação CS1061/CS1729, como esperado.)
- [x] **T013** — `DocesCabana.Application/Contracts/Services/IProdutoService.cs` e `Services/ProdutoService.cs`: `BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null)`, reaproveitando `BuscarPaginaDoCatalogo` com filtro vazio (plano §5). **Sem método de repositório novo.** `ProdutoService` passa a receber `IFavoritoRepository`.
- [x] **T014** — `BuscarTodosProdutos` **não é removido nem alterado**, conforme o plano. **Correção ao achado ao verificar:** `Areas/Admin/Controllers/ProdutoController.cs` não o chama (só usa `Cadastrar`) — hoje o único consumidor em código de produção é `HomeController.Index`, que a T018 substitui. Depois da Fase 4, o método fica sem uso em produção (só nos testes que o exercitam isoladamente). Mantido mesmo assim, por instrução explícita do plano — não é decisão desta tarefa.
- [x] **T015** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde (550/550).

## Fase 4 — A home passa a usar a consulta nova

- [x] **T016** — **Reescrever** o teste localizado em T003 em `DocesCabana.Tests/Units/Controllers/HomeControllerTests.cs`: passa a provar que `Index` chama `BuscarDestaquesDaVitrine` com a claim de quem vê — `usuarioId` preenchido para autenticado, `null` para visitante. Correção esperada, não regressão. Ver falhar.
- [x] **T017** — `DocesCabana.MVC/ViewComponents/VitrineProdutos.cs`: extrair `LimitePadrao` como constante pública (hoje é `= 8` no parâmetro). **O `.Take(limite)` fica** — vira rede de segurança (plano §8).
- [x] **T018** — `DocesCabana.MVC/Controllers/HomeController.cs`: `Index` chama `BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, UsuarioAtualId)`; `UsuarioAtualId` copiado de `CatalogoController` (mesmo padrão, mesma leitura de claim).
- [x] **T019** — `DocesCabana.MVC/Views/Home/Index.cshtml`: título da seção vira **"Bem avaliados"** (RF-08/RN-04, plano §3).
- [x] **T020** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde (552/552).

## Fase 5 — A vitrine vista pela tela

- [x] **T021** — `DocesCabana.Tests.E2E/Fluxos/PaginaInicialTests.cs`: CA-11 (favoritar na vitrine e recarregar — o coração continua cheio), CA-10 (o título anuncia o critério), CA-07/CA-08 (ordem por nota média, conferida contra a mesma leitura `AVG(Nota) ?? -1` que `ProdutoRepository` usa — sem fabricar dado), CA-09 (produto fora do catálogo não aparece), CA-12 (visitante não vê nada marcado). **Nota sobre "ver falhar":** ao contrário do previsto neste item, os 13 testes passaram de primeira ao rodar — porque a ordem de execução desta spec resolveu o defeito nas Fases 3/4 (serviço e controller) *antes* de a Fase 5 escrever a prova E2E. O vermelho real de CA-11 já foi visto e confirmado lá (T011/T012: `ProdutoServiceTests.Dado_UsuarioAutenticado_Quando_BuscarDestaquesDaVitrine_Entao_DeveMarcarOsFavoritados` falhava por não existir o método, e o comportamento de marcação foi então implementado sob teste). Esta fase prova o mesmo defeito ponta-a-ponta, não repete o ciclo vermelho-verde.
- [x] **T022** — Não reproduzível como descrito, pelo motivo acima: o coração já vinha marcado quando o teste foi escrito, porque a correção já estava em produção desde a Fase 4. A prova de "falha pelo motivo certo" está registrada em T011/T012, não aqui.
- [x] **T023** — Verificado: nenhum teste E2E preexistente fixava nome de produto da vitrine (confirmado em T003), então não havia o que ajustar. `CatalogoTests.Dado_ProdutoInativo_Quando_AbrirCatalogoEVitrine_...` continua válido — testa ausência, não presença por nome.
- [x] **T024** — Rodar as duas suítes: Fase 5 verde (`DocesCabana.Tests` completo + `PaginaInicialTests` 13/13; suíte E2E completa roda ao fim da Fase 8, T031).

## Fase 6 — O comentário das estrelas

- [x] **T025** — `DocesCabana.MVC/Views/Shared/Components/EstrelasNota/Default.cshtml`, linhas 11-14: reescrever o comentário que se contradiz (*"a 5ª estrela fica 0%, e a 5ª... a 4ª fica 100%, a 5ª fica 0%"*) para descrever o que o código faz — nota 4,5 deixa a 5ª estrela em **50%** (RF-13). **Só o comentário muda**; o algoritmo está correto.

## Fase 7 — Documentação

- [x] **T026** — `docs/arquitetura.md` §9.1: os três achados desta leitura passam a constar como **resolvidos pela `019`** — `CpfHelper`, a home carregando o catálogo inteiro, e o comentário do `EstrelasNota` (RF-12, CA-14). Também atualizada a linha da tabela §9.2 sobre o carrossel não refletir favorito real, resolvida pelo mesmo conserto.
- [x] **T027** — `docs/arquitetura.md` §9.3 (RF-11, CA-13). Conferido ao especificar, para não ter de rederivar: o modelo tem **quinze** tabelas, não catorze (`ItemCarrinho` entrou na `017`). Seguem sem comportamento **cinco**: `Estoque`, `Pedido`, `ItemPedido`, `Pagamento` e `Promocao` — `Endereco` saiu da lista, tem tela desde a `018`. Os dois parágrafos que diziam "`Endereco` … nenhuma tela" e "não existe tabela de carrinho no modelo" saíram inteiros.
- [x] **T028** — `docs/arquitetura.md` §2.1 (conferido, já refletia a `017` corretamente — nada a mudar) e §5 (linha da home corrigida: `BuscarTodosProdutos` → `BuscarDestaquesDaVitrine`). **Achado além do previsto pelo plano, corrigido por ser da mesma natureza:** §4.1 (`Header`) ainda mostrava a assinatura antiga do componente, com o parâmetro morto `itensCarrinho` que a `017` já havia substituído por contagem própria — reescrito. §4.2 (`VitrineProdutos`) e §6.4 (consulta do catálogo) ganharam nota sobre `LimitePadrao` e o reaproveitamento da consulta pela vitrine.
- [x] **T029** — `grep -rn "spec 0[0-9][0-9]"` na base inteira — código, comentário, spec antiga, README. Comentário de `OrdenacaoCatalogo.MaisVendidos` conferido: já cita corretamente "até a spec 020 dar sentido a ela" — consistente com a decomposição desta entrega, nada a corrigir ali. Nenhuma referência obsoleta encontrada em código ou em specs antigas (specs concluídas registram a numeração vigente no momento em que foram escritas — padrão já estabelecido pelas rodadas anteriores, conferido contra `016`/`018`). A única desatualização real estava em `specs/README.md`, corrigida em T030.
- [x] **T030** — `specs/README.md`: a cadeia passa a ser Correções `019` (fora da cadeia propriamente, como a `013` foi), Fechamento `020`, Estoque `022`; `021` reservado para a spec de features que absorve os quatro itens do backlog solto; "Pagamento" deixou de ser entrega própria (absorvido pela `020`). Nota de numeração registra o sexto deslocamento. Backlog: os quatro itens que a `021` absorve (CRUD de avaliação, promoções na vitrine, favoritar da página do produto, sugestões na busca) saíram da lista solta com uma nota apontando para ela; o item "Carrossel da home não reflete favorito real" saiu do backlog por estar resolvido por esta própria entrega; "Meus pedidos" passou a depender de `020`, não mais de `019`.

## Fase 8 — Fechamento

- [x] **T031** — `dotnet build` sem aviso novo (só a vulnerabilidade NU1903 pré-existente do pacote SQLite, herdada) e `DocesCabana.Tests` verde do zero (552/552). `DocesCabana.Tests.E2E`: 161/162 — a mesma falha instável e pré-existente de `BuscaTests.Dado_ProdutoComAcento_Quando_BuscarSemAcentoEEmOutraCaixa_Entao_DeveEncontrar` já registrada na linha de base (T002), alheia a esta entrega (busca não foi tocada; passou nas outras duas execuções desta sessão). Não corrigida, por estar fora do escopo desta spec.
- [x] **T032** — Subida real da aplicação (`dotnet run`, ambiente Development), conferida por HTTP: a home devolve exatamente 8 `.card-produto` e o título "Bem avaliados". O que só o olho alcança de verdade (aparência visual do cartão, animação do carrossel, o coração marcado num navegador de fato) fica para a verificação manual do responsável — ver o relato final.
- [x] **T033** — Cadastro via POST real (com token anti-falsificação da própria página) com CPF `529.982.247-95` (primeiro dígito errado): resposta 200 com o formulário de volta e `<span data-valmsg-for="CPF" class="field-validation-error">CPF inválido.</span>` — confirmado no campo do CPF, não em erro geral. Nenhuma conta foi criada.
- [x] **T034** — Preencher `checklist.md`, registrando **o que foi provado por teste e o que só a verificação ao vivo mostrou**.
- [x] **T035** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`, com o link do checklist. Também acrescentado o parágrafo narrativo da `019` e o `019` na "Ordem executada".
- [x] **T036** — Registrado (já constava em `spec.md` §8/§10 e em `plan.md` §9, conferido nesta fase): a vitrine só vira "mais vendidos" na `020`, e o `OrdenacaoCatalogo.MaisVendidos` segue saneado pelo `CatalogoController` até lá — verificado que o comentário em `CatalogoController.SanearOrdenacao` e em `ProdutoRepository`/`OrdenacaoCatalogo` já apontam para a `020` corretamente (T029).

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T004, T007, T008 |
| RF-02 | T004, T008 |
| RF-03 | T006, T008 |
| RF-04 | T011, T013 |
| RF-05 | T011, T013, T021 |
| RF-06 | T011, T013 |
| RF-07 | T013, T021 |
| RF-08 | T019, T021 |
| RF-09 | T011, T013, T016, T018, T021 |
| RF-10 | T011, T016, T018 |
| RF-11 | T027, T028 |
| RF-12 | T026 |
| RF-13 | T025 |
| RN-01 | T004, T008 |
| RN-02 | T013, T021 |
| RN-03 | T011, T017, T018 |
| RN-04 | T019, T036 |
| RN-05 | T026, T027, T028 |
| CA-01 | T004, T007, T033 |
| CA-02 | T004 |
| CA-03 | T004 |
| CA-04 | T004 |
| CA-05 | T006, T010 |
| CA-06 | T011 |
| CA-07 | T021 |
| CA-08 | T011 |
| CA-09 | T021 |
| CA-10 | T021 |
| CA-11 | T011, T021, T022 |
| CA-12 | T011, T016 |
| CA-13 | T027 |
| CA-14 | T026 |
