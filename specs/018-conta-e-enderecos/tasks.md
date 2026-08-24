# Tarefas — Conta e endereços

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
> **A Fase 5 (isolamento entre pessoas) vem antes da Fase 6 (as telas).** A
> RN-05 é o único requisito desta feature que separa "funciona" de "é seguro",
> e é o tipo de coisa que se esquece de acrescentar depois que a tela já está
> bonita. O contrato do repositório nasce sem busca só por identificador, e o
> teste que prova isso é escrito antes de existir view.
>
> **A Fase 3 (invariantes do principal) vem antes de tudo que as consome.** As
> quatro regras — nasce principal, marcar desmarca, excluir promove, excluir o
> último não deixa órfão — são o coração da feature. Falhar acima com elas
> verdes significa "a tela está errada"; sem essa ordem, significa qualquer
> coisa.

---

## Fase 1 — Preparação e linha de base

- [x] **T001** — Branch `018-conta-e-enderecos` já existia a partir de `main` (feita ao criar a pasta da spec). Trazida em dia com `main` (que ganhou a `017` desde então) por fast-forward, sem conflito.
- [x] **T002** — `dotnet build`: sucesso, 0 erro. `dotnet test DocesCabana.Tests`: **475/475**. `dotnet test DocesCabana.Tests.E2E`: **136/136**. Números maiores que os 403/117 previstos no texto original desta tarefa — o plano foi escrito antes de a `017` (carrinho) ser mergeada em `main`; esse é o baseline real de hoje.
- [x] **T003** — Localizado: `Dado_ClienteAutenticado_Quando_OlharOCabecalho_Entao_NaoDeveOferecerContaClicavel` em `DocesCabana.Tests.E2E/Fluxos/CatalogoTests.cs:654`. Verifica que o texto "Conta" aparece no cabeçalho mas não é um link (`<a>`). Vai quebrar de propósito na Fase 7 (T041) — reescrito, não removido.
- [x] **T004** — Confirmado: tabela `Endereco` tem **0 linhas** no banco de desenvolvimento (`DocesCabana.MVC/docescabana.db`, verificado por consulta direta). A migration da Fase 2 nasce sem preenchimento retroativo, como o plano previa.

## Fase 2 — Domínio e esquema

- [x] **T005** — `DocesCabana.Tests/Units/Entities/EnderecoTests.cs`: 11 testes novos — `Padrao` nasce falso e `DataCadastro` é marcada na criação; `MarcarComoPadrao`/`DesmarcarComoPadrao` alternam; `AtualizarDados` recusa exatamente o que o construtor recusa (CEP curto, número zero, rua em branco) e não deixa o endereço parcialmente alterado quando recusa. Falhou por não compilar (membros inexistentes), como esperado.
- [x] **T006** — Confirmado: os 11 erros de compilação são todos `CS1061` (membro não existe) — nenhum erro alheio.
- [x] **T007** — `DocesCabana.Domain/Entities/Endereco.cs`: `Padrao` e `DataCadastro` com `private set`; `AtualizarDados` (mesmas validações do construtor, valida tudo antes de atribuir), `MarcarComoPadrao`, `DesmarcarComoPadrao`.
- [x] **T008** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/EnderecoConfiguration.cs`: `Padrao` (obrigatório, padrão `false`) e `DataCadastro` (obrigatório) mapeados.
- [x] **T009** — Migration `AddEnderecoPadraoEDataCadastro` gerada e conferida: `AddColumn<bool> Padrao` e `AddColumn<DateTime> DataCadastro`, sem preenchimento retroativo necessário (tabela vazia, T004).
- [x] **T010** — `dotnet test DocesCabana.Tests --filter EnderecoTests`: **20/20 verdes** (9 preexistentes + 11 novos).

## Fase 3 — As invariantes do endereço principal

- [x] **T011** `[P]` — `DocesCabana.Tests/Units/Services/EnderecoServiceTests.cs` (criado): 13 testes — RN-02 (o primeiro nasce principal); RN-03 (marcar desmarca o anterior); RN-04 (excluir o principal promove o mais antigo dos restantes; excluir o não-principal preserva o principal; excluir o único não deixa nenhum com principal); `Editar`/`ListarDoUsuario`/`BuscarDoUsuario` e `KeyNotFoundException` para endereço inexistente do usuário. Falhou por falta dos tipos, como esperado.
- [x] **T012** `[P]` — `DocesCabana.Tests/Integration/Repositories/EnderecoIntegrationTests.cs` (criado): 5 testes — persistência real; `BuscarPorUsuario` ordena por `DataCadastro`; a consulta de um usuário não traz endereço de outro; `Buscar` pelo par não encontra endereço alheio; ciclo adicionar/remover. Falhou por falta de `EnderecoRepository`, como esperado.
- [x] **T013** — Confirmado: todos os erros de build eram `CS0246` (tipo não encontrado) — `EnderecoDTO`, `IEnderecoRepository`, `EnderecoService`.
- [x] **T014** `[P]` — `DocesCabana.Application/DTOs/EnderecoDTO.cs` e `Mappings/EnderecoMapper.cs` (criados). DTO sem `UsuarioId` — o dono nunca vem do formulário, sempre da claim de quem está autenticado.
- [x] **T015** — `DocesCabana.Application/Contracts/Repositories/IEnderecoRepository.cs` e `DocesCabana.Infrastructure/Repositories/EnderecoRepository.cs` (criados). **Sem `BuscarPorId(enderecoId)` sozinho** — só o par `(enderecoId, usuarioId)`.
- [x] **T016** — `DocesCabana.Application/Contracts/Services/IEnderecoService.cs` e `Services/EnderecoService.cs` (criados): o CRUD e as quatro invariantes de coleção, com commit por `IUnitOfWork.SalvarAlteracoes`.
- [x] **T017** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: `IEnderecoRepository`/`EnderecoRepository` e `IEnderecoService`/`EnderecoService` registrados.
- [x] **T018** — `dotnet test DocesCabana.Tests`: **502/502 verdes** (475 do baseline + 27 novos desta fase: 11 em `EnderecoTests`, 11 em `EnderecoServiceTests` + 5 em `EnderecoIntegrationTests`).

## Fase 4 — Validação de entrada

- [x] **T019** `[P]` — `DocesCabana.Tests/Units/Validators/EnderecoDTOValidatorTests.cs` (criado): 9 testes — um válido, um por regra obrigatória, CEP com 8 dígitos, número maior que zero. Falhou por falta do tipo, como esperado.
- [x] **T020** `[P]` — `DocesCabana.Tests/Units/Validators/DadosPessoaisDTOValidatorTests.cs` (criado): 6 testes, incluindo dois que **comparam o comportamento com `CadastroDTOValidator`** para o mesmo celular e a mesma data inválidos (plano §9, risco 7). Falhou por falta do tipo, como esperado.
- [x] **T021** — Confirmado: falhas de compilação por tipo inexistente (`EnderecoDTOValidator`, `DadosPessoaisDTO`, `DadosPessoaisDTOValidator`) — nenhuma alheia.
- [x] **T022** `[P]` — `DocesCabana.Application/DTOs/DadosPessoaisDTO.cs` (criado). CPF viaja só para exibição, nunca é campo de formulário.
- [x] **T023** `[P]` — `DocesCabana.Application/Validators/EnderecoDTOValidator.cs` (criado).
- [x] **T024** — `DocesCabana.Application/Validators/DadosPessoaisDTOValidator.cs` (criado): reaproveita as regras (mesma lógica, mesma mensagem) de `CadastroDTOValidator` para Nome, Celular e DataNascimento. Um bug pego no caminho: o primeiro rascunho de dois testes da comparação passava `dataNascimento: default` para um parâmetro `DateTime?` do helper de teste, que o operador `??` convertia de volta para uma data válida — corrigido atribuindo `dto.DataNascimento = default` diretamente, não pela fábrica.
- [x] **T025** — `dotnet test DocesCabana.Tests`: **519/519 verdes** (502 + 17 novos). Validators registrados sozinhos pelo assembly scan, confirmado — nenhum registro manual foi necessário.

## Fase 5 — Isolamento entre pessoas

- [x] **T026** — `DocesCabana.Tests/Units/Services/EnderecoServiceTests.cs`: buscar (`BuscarDoUsuario`), editar e excluir um endereço de outra pessoa já tinham teste desde a Fase 3 (T011) — do ponto de vista do serviço, "inexistente para o usuário" e "de outra pessoa" são exatamente a mesma coisa, porque o repositório busca sempre pelo par e devolve `null` nos dois casos. Faltava só `TornarPrincipal`: acrescentado `Dado_EnderecoDeOutraPessoa_Quando_TentarTornarPrincipal_Entao_DeveLancarKeyNotFoundExceptionSemAlterarNada`, que também confirma que `BuscarPorUsuario`/`SalvarAlteracoes` nunca rodam.
- [x] **T027** — Confirmado: o teste novo **passou de primeira**, sem alterar `EnderecoService`. Registrado o motivo: o desenho do repositório (T015 — `Buscar` sempre pelo par, sem `BuscarPorId` sozinho) já tornava a violação impossível antes de este teste existir; ele fixa a garantia, não a cria.
- [x] **T028** — Confirmado sem alteração: os quatro caminhos (`BuscarDoUsuario`, `Editar`, `Excluir`, `TornarPrincipal`) já passam pelo par — `Buscar(enderecoId, usuarioId)` ou `BuscarPorUsuario(usuarioId)` seguido de filtro, nunca por um identificador sozinho.
- [x] **T029** — `dotnet test DocesCabana.Tests`: **520/520 verdes**. RN-05 provada em unidade, antes de existir qualquer tela.

## Fase 6 — As telas da conta

- [x] **T030** — `DocesCabana.Tests/Units/Controllers/ContaControllerTests.cs` (criado): 14 testes — `[Authorize]` na classe, guarda de `ModelState` antes de qualquer efeito (dados pessoais e endereço), CA-07 (CPF re-preenchido do banco ao redesenhar), redirecionamento no sucesso das 6 ações de escrita, e endereço alheio propaga `KeyNotFoundException` (não é tratado no controlador — Princípio VIII). Falhou por falta de `ContaController`, como esperado.
- [x] **T031** — `DocesCabana.Tests.E2E/Paginas/PaginaConta.cs` (criado) e `Fluxos/ContaTests.cs` (criado): 16 testes cobrindo CA-02 a CA-17 — dados pessoais, o CRUD inteiro de endereço, as quatro regras do principal vistas pela tela, e o endereço alheio (com identificador real de uma segunda conta, não um Guid aleatório — provar isolamento de verdade, não só "id inexistente"). Falhou por falta de controlador/views, como esperado.
- [x] **T032** — Confirmado: build falhava só por `ContaController` não existir.
- [x] **T033** — `DocesCabana.MVC/Controllers/ContaController.cs` (criado): `[Authorize]` na classe (RF-03); sete ações (`Index`, `AlterarDados`, `Enderecos`, `NovoEndereco` GET/POST, `EditarEndereco` GET/POST, `ExcluirEndereco`, `TornarPrincipal`); `[ValidateAntiForgeryToken]` em todas as escritas; PRG no sucesso.
- [x] **T034** `[P]` — `DocesCabana.MVC/Views/Conta/_MenuDaConta.cshtml` (criado): duas entradas (Dados pessoais, Endereços) e "Meus pedidos" já reservado, desabilitado, para a `019`.
- [x] **T035** `[P]` — `DocesCabana.MVC/Views/Conta/Index.cshtml` (criado): dados pessoais. **CPF como texto**, não como campo desabilitado (plano §3, RN-08).
- [x] **T036** `[P]` — `DocesCabana.MVC/Views/Conta/Enderecos.cshtml` (criado): lista, marcação do principal (`★ PRINCIPAL`), convite quando vazia (RF-14). O principal não oferece "Tornar principal".
- [x] **T037** — `DocesCabana.MVC/Views/Conta/FormularioEndereco.cshtml` (criado): cadastro e edição na mesma view (a presença de um `EnderecoId` real distingue os dois), CEP como primeiro campo. A referência a `conta.js` (Fase 8) foi propositalmente deixada de fora por ora — `asp-append-version` falharia contra um arquivo que ainda não existe.
- [x] **T038** — `DocesCabana.MVC/wwwroot/css/pages/conta.css` (criado): menu lateral e cartões de endereço.
- [x] **T039** — `dotnet test DocesCabana.Tests`: **534/534 verdes**. `dotnet test DocesCabana.Tests.E2E`: **16/16 em `ContaTests`**; suíte inteira, **152/152 verdes**. Três bugs reais encontrados e corrigidos no caminho (nenhum era falha do teste): (1) `DadosPessoaisDTO.DataNascimento` sem `[DisplayFormat]` fazia o Input Tag Helper renderizar a data com hora ("06/06/1994 00:00:00") ao pré-preencher o formulário — só aparecia porque, diferente do cadastro, aqui o campo nasce com valor; (2) `DadosPessoaisDTO.CPF` (string não anulável, nunca postado pelo form) era tratado como implicitamente `[Required]` pelo ASP.NET Core, invalidando o `ModelState` em silêncio sem span nenhum para mostrar o erro — corrigido com `[ValidateNever]`; (3) o teste de "corrigir celular" esperava o valor formatado de volta, mas `Usuario.AtualizarDados` grava só os dígitos (mesma convenção do CPF) — ajustado o teste, não a aplicação.

## Fase 7 — O atalho do cabeçalho

- [x] **T040** — `DocesCabana.Tests.E2E/Fluxos/ContaTests.cs`: `Dado_ClienteAutenticado_Quando_AcionarOAtalhoConta_Entao_DeveChegarNaAreaDeConta` (CA-01). Falhou por timeout esperando um link "Conta" que não existia (só o `<span>` apagado), como esperado.
- [x] **T041** — **Reescrito**: `Dado_ClienteAutenticado_Quando_OlharOCabecalho_Entao_DeveOferecerContaClicavel` (era `...NaoDeveOferecerContaClicavel`) — prova que o atalho funciona. Correção esperada, não regressão.
- [x] **T042** — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: o `<span>` apagado virou `<a asp-controller="Conta" asp-action="Index">`.
- [x] **T043** — `dotnet test DocesCabana.Tests.E2E`: **18/18 verdes em `ContaTests` + o teste reescrito de `CatalogoTests`**; suíte inteira, **153/153 verdes**.

## Fase 8 — Busca por CEP e máscaras

- [x] **T044** — `DocesCabana.Tests.E2E/Fluxos/ContaTests.cs`: CA-18, CA-19 e CA-20, **com `Page.RouteAsync` interceptando o ViaCEP** — um devolvendo endereço conhecido, outro abortando a requisição. Nenhum dos três toca a rede de verdade (plano §7). CA-18/CA-19 falharam (campos continuavam vazios); CA-20 passou de primeira — sem script nenhum ainda, o formulário já era preenchível à mão, exatamente o piso que RN-07 exige.
- [x] **T045** — Confirmado: as duas falhas eram por os campos não preencherem (script inexistente), não por erro alheio.
- [x] **T046** — `DocesCabana.MVC/wwwroot/js/pages/conta.js` (criado): busca por CEP (`fetch` ao ViaCEP no blur do campo) e máscaras de celular, data de nascimento e CEP. `TelefoneHelper`/`CepHelper` do lado do servidor já tiram a formatação na gravação — o script só cuida da digitação. Falha, `erro:true` do ViaCEP (CEP bem formado mas inexistente) ou timeout deixam os campos como estavam, sem mensagem alarmante (RN-07). Também escrito o teste que faltava para CA-21 (`Dado_JavaScriptDesligado_Quando_CadastrarEndereco_Entao_DeveFuncionar`, ausente do arquivo até aqui apesar de mapeado no plano) — usa o cliente do seed pelo mesmo motivo já registrado em `017`/T041: o cadastro de conta depende de JavaScript para as máscaras de celular/CPF/data.
- [x] **T047** — `dotnet test DocesCabana.Tests.E2E`: **21/21 verdes em `ContaTests`** (16 da Fase 6 + CA-01 + CA-18/19/20/21); suíte inteira, **157/157 verdes** — CA-21 continua verde depois do script.

## Fase 9 — Documentação de apoio

- [x] **T048** — `ModelagemBancoTCC.dbml`: `Padrao` (bit, RN-01) e `DataCadastro` (datetime2, critério de promoção da RN-04) acrescentadas à tabela `Endereco`.
- [x] **T049** — `docs/arquitetura.md`: `/Conta` e as rotas de endereço acrescentadas à tabela de páginas (§5) — não existe uma "lista de controladores" separada no guia, a própria tabela de páginas cumpre esse papel (mesmo tratamento dado a `CarrinhoController` na `017`).
- [x] **T050** — `grep -rn "spec 018\|spec 019"`: 10 arquivos, todos referências novas e corretas, escritas por esta própria execução. Confirmado: `018` já era o número reservado a endereços (traçado na `016`), esta feature não desloca a cadeia.

## Fase 10 — Fechamento

- [x] **T051** — `dotnet clean` + `dotnet build`: sucesso, **0 erro** (só o aviso pré-existente `NU1903`, alheio a esta feature). `dotnet test DocesCabana.Tests`: **534/534**. `dotnet test DocesCabana.Tests.E2E`: **157/157**, sem flake nesta rodada.
- [ ] **T052** — Subir a aplicação e percorrer **cada critério de aceite** no navegador. **Não executado por este agente** — exige um humano olhando (menu lateral, cartão do endereço principal, CPF como texto). Um smoke test (`dotnet run` + `curl`) confirmou `302` (desafio de login) em `/Conta` para visitante anônimo — prova que a rota e o `[Authorize]` funcionam, não que a tela esteja correta visualmente. Ver `checklist.md`, seção "Verificação manual pendente".
- [ ] **T053** — Percorrer ao vivo o ciclo inteiro do principal. **Não executado por este agente**, mesmo motivo do T052 — coberto por `ContaTests.cs` (CA-08 a CA-16, automatizado e verde), mas não repetido manualmente num navegador.
- [ ] **T054** — Testar a busca por CEP **contra o ViaCEP de verdade**. **Não executado por este agente** — os testes automatizados interceptam a rota de propósito (plano §7); esta é a única lacuna que a automação não pode fechar por desenho, precisa de um humano com acesso à internet real testando à mão.
- [x] **T055** — `checklist.md` preenchido, registrando o que foi provado por teste (a maioria) e o que só a verificação ao vivo mostraria (T052/T053/T054, listados numa seção própria), mais os três achados corrigidos no caminho.
- [x] **T056** — Status da spec → *Implementada*; status do plano → *Executado*; linha da `018` em `specs/README.md` (índice e cadeia da loja) com link para spec/plan/tasks/checklist; parágrafo de fechamento acrescentado à narrativa "Ordem executada".
- [x] **T057** — Backlog de `specs/README.md`: linha "Página de conta do cliente" removida (esta feature a encerra). Três linhas novas registram o que ela **não** encerra: trocar senha, trocar e-mail e "Meus pedidos" — as três já apontam `018` como pré-requisito cumprido. Conferidas as dívidas da baseline (`specs/000-baseline/spec.md` §6) — `D-07` (`Endereco` sem entidade) já estava marcada resolvida pela `003`; nada novo a riscar.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T040, T042 |
| RF-02 | T031, T034 |
| RF-03 | T030, T033 |
| RF-04 | T031, T035 |
| RF-05 | T031, T033, T035 |
| RF-06 | T031, T035 |
| RF-07 | T019, T020, T023, T024, T030 |
| RF-08 | T012, T031, T036 |
| RF-09 | T011, T016, T037 |
| RF-10 | T005, T007, T016, T037 |
| RF-11 | T011, T016, T036 |
| RF-12 | T011, T016, T036 |
| RF-13 | T031, T036 |
| RF-14 | T031, T036 |
| RF-15 | T026, T027, T028, T031 |
| RF-16 | T044, T046 |
| RF-17 | T044, T046 |
| RF-18 | T044, T046 |
| RF-19 | T039, T047 |
| RN-01 | T011, T012 |
| RN-02 | T011, T016 |
| RN-03 | T011, T016 |
| RN-04 | T011, T016 |
| RN-05 | T026, T028, T015 |
| RN-06 | T031, T035 |
| RN-07 | T044, T046, T047 |
| RN-08 | T035, T036, T042 |
| CA-01 | T040 |
| CA-02 a CA-17 | T031 |
| CA-18 a CA-20 | T044 |
| CA-21 | T039, T047 |
