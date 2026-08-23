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

- [ ] **T001** — Criar branch `018-conta-e-enderecos` a partir de `main`. *(feita ao criar a pasta da spec)*
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 403 e 117 verdes, herdados da `016`).
- [ ] **T003** — Localizar o teste da `014` que prova que **o atalho "Conta" está desabilitado**. Anotá-lo aqui: vai quebrar de propósito na Fase 7, e precisa ser **reescrito**, não removido — passa a provar que o atalho leva à conta.
- [ ] **T004** — Confirmar que a tabela `Endereco` está **vazia** no banco de desenvolvimento. É o que permite a migration da Fase 2 nascer sem preenchimento retroativo (plano §6); se houver linha, o plano precisa mudar antes de seguir.

## Fase 2 — Domínio e esquema

- [ ] **T005** — `DocesCabana.Tests/Units/Entities/EnderecoTests.cs`: `AtualizarDados` recusa exatamente o que o construtor recusa (CEP curto, número zero, obrigatório em branco); `MarcarComoPadrao` e `DesmarcarComoPadrao` alternam o estado. Ver falhar.
- [ ] **T006** — Confirmar que T005 falha por não existirem os métodos — e não por erro de compilação alheio.
- [ ] **T007** — `DocesCabana.Domain/Entities/Endereco.cs`: `Padrao` e `DataCadastro` com `private set`; `AtualizarDados`, `MarcarComoPadrao`, `DesmarcarComoPadrao`. **`AtualizarDados` chama as mesmas validações do construtor** — não uma cópia com regras próprias (plano §5).
- [ ] **T008** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/EnderecoConfiguration.cs`: mapear as duas colunas novas.
- [ ] **T009** — Migration: `dotnet ef migrations add AddEnderecoPadraoEDataCadastro --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`. Conferir o arquivo gerado antes de aplicar.
- [ ] **T010** — Rodar `dotnet test DocesCabana.Tests`: T005 passa.

## Fase 3 — As invariantes do endereço principal

- [ ] **T011** `[P]` — `DocesCabana.Tests/Units/Services/EnderecoServiceTests.cs` (criar): RN-02 (o primeiro nasce principal); RN-03 (marcar desmarca o anterior); RN-04 (excluir o principal promove o mais antigo dos restantes); excluir o último deixa a lista vazia sem principal; RN-01 conferida como estado final em cada caso — **nunca dois, nunca zero com endereço existindo**. Ver falhar.
- [ ] **T012** `[P]` — `DocesCabana.Tests/Integration/Repositories/EnderecoIntegrationTests.cs` (criar): persistência real; `BuscarPorUsuario` ordena por `DataCadastro`; a consulta de um usuário não traz endereço de outro. Ver falhar.
- [ ] **T013** — Confirmar que T011 e T012 falham por não existirem serviço nem repositório.
- [ ] **T014** `[P]` — `DocesCabana.Application/DTOs/EnderecoDTO.cs` e `Mappings/EnderecoMapper.cs` (criar).
- [ ] **T015** — `DocesCabana.Application/Contracts/Repositories/IEnderecoRepository.cs` (criar) e `DocesCabana.Infrastructure/Repositories/EnderecoRepository.cs` (criar). **Sem `BuscarPorId(enderecoId)` sozinho** — só o par `(enderecoId, usuarioId)`, que é o desenho que sustenta a RN-05 (plano §5).
- [ ] **T016** — `DocesCabana.Application/Contracts/Services/IEnderecoService.cs` e `Services/EnderecoService.cs` (criar): o CRUD e as quatro invariantes de coleção, com commit por `IUnitOfWork.SalvarAlteracoes`.
- [ ] **T017** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: registrar repositório e serviço.
- [ ] **T018** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — Validação de entrada

- [ ] **T019** `[P]` — `DocesCabana.Tests/Units/Validators/EnderecoDTOValidatorTests.cs` (criar): um caso válido e um inválido por regra. Ver falhar.
- [ ] **T020** `[P]` — `DocesCabana.Tests/Units/Validators/DadosPessoaisDTOValidatorTests.cs` (criar): idem, mais um teste que **compara o comportamento com `CadastroDTOValidator`** para o mesmo valor inválido de celular e de data (plano §9, risco 7). Ver falhar.
- [ ] **T021** — Confirmar que T019 e T020 falham por não existirem os validators.
- [ ] **T022** `[P]` — `DocesCabana.Application/DTOs/DadosPessoaisDTO.cs` (criar).
- [ ] **T023** `[P]` — `DocesCabana.Application/Validators/EnderecoDTOValidator.cs` (criar).
- [ ] **T024** — `DocesCabana.Application/Validators/DadosPessoaisDTOValidator.cs` (criar): **reaproveita as regras de `CadastroDTOValidator`**, não as reescreve.
- [ ] **T025** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde. Conferir que os validators foram registrados sozinhos pelo assembly scan — criar o arquivo basta (Princípio III).

## Fase 5 — Isolamento entre pessoas

- [ ] **T026** — `DocesCabana.Tests/Units/Services/EnderecoServiceTests.cs`: buscar, editar, excluir e tornar principal um endereço **de outra pessoa** lançam `KeyNotFoundException` — nunca devolvem o endereço nem alteram nada. Ver falhar.
- [ ] **T027** — Confirmar que T026 falha, e **confirmar por que**: se já passar, o desenho do repositório (T015) está certo e o teste está apenas fixando a garantia. Registrar qual dos dois foi o caso — não deixar passar como "passou de primeira".
- [ ] **T028** — `DocesCabana.Application/Services/EnderecoService.cs`: garantir que os quatro caminhos passam pelo par `(enderecoId, usuarioId)`.
- [ ] **T029** — Rodar `dotnet test DocesCabana.Tests`: RN-05 provada em unidade, antes de existir qualquer tela.

## Fase 6 — As telas da conta

- [ ] **T030** — `DocesCabana.Tests/Units/Controllers/ContaControllerTests.cs` (criar): guarda de `ModelState` antes de qualquer efeito; redirecionamento no sucesso; endereço alheio recusado; `[Authorize]` no controlador. Ver falhar.
- [ ] **T031** — `DocesCabana.Tests.E2E/Paginas/PaginaConta.cs` (criar) e `Fluxos/ContaTests.cs` (criar): CA-02 a CA-17 — dados pessoais, o CRUD inteiro, as quatro regras do principal vistas pela tela, e o endereço alheio. Ver falhar.
- [ ] **T032** — Confirmar que T030 e T031 falham por não existir controlador nem view.
- [ ] **T033** — `DocesCabana.MVC/Controllers/ContaController.cs` (criar): `[Authorize]` na classe (RF-03); as sete ações; `[ValidateAntiForgeryToken]` em todas as escritas; PRG no sucesso.
- [ ] **T034** `[P]` — `DocesCabana.MVC/Views/Conta/_MenuDaConta.cshtml` (criar): tela parcial de uso único, mora com o controlador dono (Princípio IV). Nasce com duas entradas e espaço para "Meus pedidos" da `019`.
- [ ] **T035** `[P]` — `DocesCabana.MVC/Views/Conta/Index.cshtml` (criar): dados pessoais. **CPF como texto, não como campo desabilitado** (plano §3, RN-08).
- [ ] **T036** `[P]` — `DocesCabana.MVC/Views/Conta/Enderecos.cshtml` (criar): lista, marcação do principal, convite quando vazia (RF-14). O principal **não** oferece "Tornar principal".
- [ ] **T037** — `DocesCabana.MVC/Views/Conta/FormularioEndereco.cshtml` (criar): cadastro e edição na mesma view, com o CEP como primeiro campo.
- [ ] **T038** — `DocesCabana.MVC/wwwroot/css/pages/conta.css` (criar): menu lateral e cartões de endereço, reaproveitando `components/formulario.css` da `016`.
- [ ] **T039** — Rodar as duas suítes: CA-21 (sem JavaScript) já deve passar aqui — as telas ainda não têm script nenhum, e é o melhor momento para provar que o piso existe antes de o script chegar.

## Fase 7 — O atalho do cabeçalho

- [ ] **T040** — `DocesCabana.Tests.E2E/Fluxos/ContaTests.cs`: CA-01 — o atalho leva à conta e não está mais apagado. Ver falhar.
- [ ] **T041** — **Reescrever** o teste localizado em T003: passa a provar que o atalho funciona. Correção esperada, não regressão.
- [ ] **T042** — `DocesCabana.MVC/Views/Shared/Components/Header/Default.cshtml`: o `<span>` apagado vira link para `/Conta`.
- [ ] **T043** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 7 verde.

## Fase 8 — Busca por CEP e máscaras

- [ ] **T044** — `DocesCabana.Tests.E2E/Fluxos/ContaTests.cs`: CA-18, CA-19 e CA-20, **com `Page.RouteAsync` interceptando o ViaCEP** — um devolvendo endereço conhecido, outro devolvendo falha. Nenhum dos três toca a rede de verdade (plano §7). Ver falhar.
- [ ] **T045** — Confirmar que T044 falha por não existir o script.
- [ ] **T046** — `DocesCabana.MVC/wwwroot/js/pages/conta.js` (criar): busca por CEP e máscaras de celular, data e CEP. **Tira a formatação do CEP antes de consultar** (plano §9, risco 4). Falha, demora ou CEP inexistente devolvem os campos ao estado normal, sem mensagem alarmante (RN-07).
- [ ] **T047** — Rodar `dotnet test DocesCabana.Tests.E2E`: Fase 8 verde, **e CA-21 continua verde** — acrescentar o script não pode ter quebrado o piso sem JavaScript.

## Fase 9 — Documentação de apoio

- [ ] **T048** — `ModelagemBancoTCC.dbml`: acrescentar `Padrao` e `DataCadastro` à tabela `Endereco`. Entregável do TCC (plano §6).
- [ ] **T049** — `docs/arquitetura.md`: acrescentar a área de conta à tabela de páginas e o `ContaController` à lista de controladores.
- [ ] **T050** — `grep -rn "spec 0[0-9][0-9]"` na base — conferir se alguma referência ficou obsoleta. **Esta feature não desloca a cadeia** (`018` já era o número reservado a endereços), então o esperado é não achar nada; confirmar em vez de presumir.

## Fase 10 — Fechamento

- [ ] **T051** — `dotnet build` sem aviso e as duas suítes verdes, do zero.
- [ ] **T052** — Subir a aplicação e percorrer **cada critério de aceite** no navegador. Especialmente a aparência do menu lateral, o cartão do endereço principal e o CPF como texto ao lado dos campos editáveis.
- [ ] **T053** — Percorrer ao vivo o ciclo inteiro do principal: cadastrar dois, trocar qual é o principal, excluir o principal e conferir que o outro assumiu, excluir o último e conferir que volta o convite.
- [ ] **T054** — Testar a busca por CEP **contra o ViaCEP de verdade**, uma vez, à mão — os testes automatizados o interceptam de propósito, então esta é a única prova de que a integração real funciona. Registrar o resultado no checklist.
- [ ] **T055** — Preencher `checklist.md`, registrando **o que foi provado por teste e o que só a verificação ao vivo mostrou**.
- [ ] **T056** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`, com o link do checklist.
- [ ] **T057** — Riscar do backlog de `specs/README.md` a linha "Página de conta do cliente", que esta feature encerra. Registrar o que ela **não** encerra: trocar senha, trocar e-mail e "Meus pedidos" seguem sem dono.

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
