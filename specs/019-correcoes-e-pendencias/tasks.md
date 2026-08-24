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

- [ ] **T001** — Criar branch `019-correcoes-e-pendencias` a partir de `main`. *(feita ao criar a pasta da spec)*
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 534 e 157 verdes, herdados da `018`).
- [ ] **T003** — Localizar os testes que esta entrega vai **quebrar de propósito**: o de `HomeControllerTests` que afirma `BuscarTodosProdutos` chamado uma vez, e qualquer teste E2E que fixe nome de produto da vitrine. Anotá-los aqui — são reescritos na Fase 4, não removidos.

## Fase 2 — A conferência do CPF

- [ ] **T004** — `DocesCabana.Tests/Units/Helpers/CpfHelperTests.cs`: acrescentar os três CPFs com o **primeiro** dígito verificador errado (`52998224795`, `52998224705`, `52998224715`) como inválidos. Ver falhar.
- [ ] **T005** — Confirmar que T004 falha **por aceitar CPF inválido** (`Assert.False` recebendo `true`), não por compilação. É a prova de que o teste viu o bug.
- [ ] **T006** — `DocesCabana.Tests/Units/Helpers/CpfHelperTests.cs`: teste de guarda que percorre os **nove CPFs semeados** — os oito clientes da lista de `DbInitializer.cs` mais o do administrador (mesmo arquivo, na semeadura do admin) — e exige que todos sejam válidos. **Deve passar de primeira**: os nove foram conferidos dígito a dígito ao especificar. Se algum falhar, **pare**: o seed tem CPF inválido e o plano precisa mudar antes de seguir.
- [ ] **T007** `[P]` — `DocesCabana.Tests/Units/Validators/CadastroDTOValidatorTests.cs`: um CPF com o primeiro dígito errado é recusado também na barreira de entrada (RF-01, CA-01). Ver falhar.
- [ ] **T008** — `DocesCabana.Domain/Helpers/CpfHelper.cs`: extrair `CalcularDigito(string parcial, int[] multiplicadores)` e conferir **os dois** dígitos contra os informados (plano §5). Não consertar sem extrair — a duplicação é a causa raiz.
- [ ] **T009** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde.
- [ ] **T010** — Rodar `dotnet test DocesCabana.Tests.E2E`: o cadastro é o caminho de entrada de quase toda a suíte, e `GeradorDeDados.CpfValido` precisa continuar gerando CPF aceito. Falha aqui significa que o gerador calcula errado — **não** que a correção está errada.

## Fase 3 — A consulta da vitrine

- [ ] **T011** `[P]` — `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs`: `BuscarDestaquesDaVitrine` pede ao repositório exatamente o limite recebido (`Verify(..., pagina: 1, tamanhoDaPagina: 8)`, CA-06); ordena por `MelhorAvaliados`; consulta favoritos **só** quando autenticado (CA-12, `Times.Never` para visitante); marca os favoritados (CA-11). Ver falhar.
- [ ] **T012** — Confirmar que T011 falha por não existir o método — e não por erro alheio.
- [ ] **T013** — `DocesCabana.Application/Contracts/Services/IProdutoService.cs` e `Services/ProdutoService.cs`: `BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null)`, reaproveitando `BuscarPaginaDoCatalogo` com filtro vazio (plano §5). **Sem método de repositório novo.** `ProdutoService` passa a receber `IFavoritoRepository`.
- [ ] **T014** — `BuscarTodosProdutos` **não é removido nem alterado** — continua servindo `Areas/Admin`. Confirmar que nenhum outro consumidor da home ficou apontando para ele.
- [ ] **T015** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — A home passa a usar a consulta nova

- [ ] **T016** — **Reescrever** o teste localizado em T003 em `DocesCabana.Tests/Units/Controllers/HomeControllerTests.cs`: passa a provar que `Index` chama `BuscarDestaquesDaVitrine` com a claim de quem vê — `usuarioId` preenchido para autenticado, `null` para visitante. Correção esperada, não regressão. Ver falhar.
- [ ] **T017** — `DocesCabana.MVC/ViewComponents/VitrineProdutos.cs`: extrair `LimitePadrao` como constante pública (hoje é `= 8` no parâmetro). **O `.Take(limite)` fica** — vira rede de segurança (plano §8).
- [ ] **T018** — `DocesCabana.MVC/Controllers/HomeController.cs`: `Index` chama `BuscarDestaquesDaVitrine(VitrineProdutosViewComponent.LimitePadrao, UsuarioAtualId)`; `UsuarioAtualId` copiado de `CatalogoController` (mesmo padrão, mesma leitura de claim).
- [ ] **T019** — `DocesCabana.MVC/Views/Home/Index.cshtml`: título da seção vira **"Bem avaliados"** (RF-08/RN-04, plano §3).
- [ ] **T020** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde.

## Fase 5 — A vitrine vista pela tela

- [ ] **T021** — `DocesCabana.Tests.E2E/Fluxos/PaginaInicialTests.cs`: CA-11 (favoritar na vitrine e recarregar — o coração continua cheio), CA-10 (o título anuncia o critério), CA-07 (ordem relativa por avaliação), CA-09 (produto fora do catálogo não aparece). Ver falhar — CA-11 é o que hoje falha de verdade.
- [ ] **T022** — Confirmar que CA-11 falha **por o coração voltar vazio**, não por seletor errado. É o defeito que a spec descreve.
- [ ] **T023** — Ajustar os testes E2E que a Fase 4 quebrou (localizados em T003), se houver: passam a ler ordem relativa das notas em vez de fixar nome de produto (plano §7).
- [ ] **T024** — Rodar as duas suítes: Fase 5 verde.

## Fase 6 — O comentário das estrelas

- [ ] **T025** — `DocesCabana.MVC/Views/Shared/Components/EstrelasNota/Default.cshtml`, linhas 11-14: reescrever o comentário que se contradiz (*"a 5ª estrela fica 0%, e a 5ª... a 4ª fica 100%, a 5ª fica 0%"*) para descrever o que o código faz — nota 4,5 deixa a 5ª estrela em **50%** (RF-13). **Só o comentário muda**; o algoritmo está correto.

## Fase 7 — Documentação

- [ ] **T026** — `docs/arquitetura.md` §9.1: os três achados desta leitura passam a constar como **resolvidos pela `019`** — `CpfHelper`, a home carregando o catálogo inteiro, e o comentário do `EstrelasNota` (RF-12, CA-14).
- [ ] **T027** — `docs/arquitetura.md` §9.3 (RF-11, CA-13). Conferido ao especificar, para não ter de rederivar: o modelo tem **quinze** tabelas, não catorze (`ItemCarrinho` entrou na `017`). Seguem sem comportamento **cinco**: `Estoque`, `Pedido`, `ItemPedido`, `Pagamento` e `Promocao` — `Endereco` saiu da lista, tem tela desde a `018`. Os dois parágrafos que dizem "`Endereco` … nenhuma tela" e "não existe tabela de carrinho no modelo" saem inteiros.
- [ ] **T028** — `docs/arquitetura.md` §2.1 e §5: conferir se a home, o `ProdutoService` e o fluxo da vitrine seguem descritos como eram — corrigir o que esta entrega mudou.
- [ ] **T029** — `grep -rn "spec 0[0-9][0-9]"` na base inteira — código, comentário, spec antiga, README — e corrigir toda referência que a renumeração tornou obsoleta. **Inclui esta spec e este plano.** Atenção ao comentário de `OrdenacaoCatalogo.MaisVendidos`, que cita a spec que dá sentido a "venda".
- [ ] **T030** — `specs/README.md`: a cadeia passa a ser Correções `019`, Fechamento `020`, Features `021`, Estoque `022`; a nota de numeração registra o sexto deslocamento. Backlog: os quatro itens que a `021` absorve saem da lista solta e passam a apontar para ela.

## Fase 8 — Fechamento

- [ ] **T031** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero.
- [ ] **T032** — Subir a aplicação e conferir ao vivo o que teste automatizado alcança mal: a vitrine mostrando de fato os bem avaliados, o título novo, e o coração cheio depois de recarregar.
- [ ] **T033** — Cadastrar uma conta à mão com um CPF de primeiro dígito errado e confirmar que a mensagem aparece **no campo do CPF**, não como erro geral.
- [ ] **T034** — Preencher `checklist.md`, registrando **o que foi provado por teste e o que só a verificação ao vivo mostrou**.
- [ ] **T035** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`, com o link do checklist.
- [ ] **T036** — Registrar o que esta entrega **não** encerra: a vitrine só vira "mais vendidos" na `020`, e o `OrdenacaoCatalogo.MaisVendidos` segue saneado pelo `CatalogoController` até lá.

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
