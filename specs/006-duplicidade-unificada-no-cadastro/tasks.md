# Tarefas — Duplicidade unificada no cadastro

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

**Específico desta feature:** duas armadilhas.

1. O texto da mensagem **não muda** — só a casa dele. `"Os dados informados já
   estão associados a uma conta existente."` sai de dois literais e vira uma
   constante com o mesmo conteúdo, caractere por caractere. Se o texto mudar por
   acidente, os dois cadastros continuam consistentes entre si e ninguém percebe
   que a mensagem que o usuário lê ficou diferente da que a spec pede.
2. A T012 **mexe num caminho que hoje funciona** — o cadastro de cliente. Ele não
   está quebrado; está sendo trocado por um equivalente. Qualquer sinal de
   regressão ali é motivo para parar, não para seguir e "conferir no fim".

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `006-duplicidade-unificada-no-cadastro` a partir de
      `main` (com a `005` já integrada).
- [x] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado inicial
      verde (250 na última medição) como linha de base da T016.

## Fase 2 — Testes (devem falhar)

*Escreva, rode, veja vermelho. Só então passe para a Fase 3.*

- [x] **T003** `[P]` — `DocesCabana.Tests/Units/Services/UsuarioServiceCadastroTests.cs`:
      acrescentar — `ContaJaExiste` devolve `true` quando o e-mail tem dono,
      `true` quando o CPF tem dono, `false` quando nenhum dos dois; e a corrida
      de CPF (`SalvarAlteracoes` lançando `DbUpdateException` com o CPF já no
      repositório) apaga a conta criada **e** lança com a mensagem amigável, não
      com erro interno. **Prova RN-01, RF-06, CA-05.**
- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Controllers/AdministradorControllerTests.cs`:
      acrescentar — com CPF já usado e com e-mail já usado, o POST devolve
      `ViewResult` com o dto, o `ModelState` traz
      `MensagensCadastro.DadosJaAssociados` e `CadastrarAdministrador` **não** é
      chamado. **Prova RF-01 a RF-05, CA-01, CA-02.** É o teste que faltava na
      `005` e que teria pego o defeito.
- [x] **T005** `[P]` — `DocesCabana.Tests/Units/Controllers/AutenticacaoControllerTests.cs`:
      ajustar `Dado_UsuarioExistente_...` e
      `Dado_DadosValidos_Quando_CadastroPost_...` para mockar `ContaJaExiste` em
      vez de `BuscarPorLogin`, e desdobrar o primeiro em dois casos — e-mail
      repetido e CPF repetido —, ambos asseverando contra a constante.
      **Prova CA-03, CA-04, e a não-regressão da CA-06.**
- [x] **T006** — Rodar `dotnet test` e confirmar que T003–T005 falham pelo motivo
      esperado — ausência de `ContaJaExiste` e de `MensagensCadastro`, não erro
      de compilação alheio.

## Fase 3 — Aplicação

- [x] **T007** — `DocesCabana.Application/Mensagens/MensagensCadastro.cs`
      (criar): `public const string DadosJaAssociados` com **exatamente** o texto
      que já está hoje em `UsuarioService` e em `AutenticacaoController`.
      Copie o literal existente; não redigite.

## Fase 4 — Serviço

- [x] **T008** — `DocesCabana.Infrastructure/Identity/Services/IUsuarioService.cs`:
      acrescentar `Task<bool> ContaJaExiste(string email, string cpf);`.
- [x] **T009** — `DocesCabana.Infrastructure/Identity/Services/UsuarioService.cs`:
      implementar `ContaJaExiste` sobre `BuscarPorLogin`; trocar o literal da
      mensagem pela constante; acrescentar o `catch (DbUpdateException)` que
      confirma a colisão por consulta **antes** do `DeleteAsync` e traduz para a
      mensagem amigável (plano §4). O teste da corrida da T003 é o que prova que
      a compensação continua removendo a conta nesse caminho novo — risco 1 do
      plano §8.
- [x] **T010** — Rodar `dotnet test`: T003 passa.

## Fase 5 — Apresentação

- [x] **T011** — `DocesCabana.MVC/Controllers/AdministradorController.cs`:
      guarda de duplicidade após a de `ModelState` e antes de cadastrar —
      `ModelState.AddModelError(string.Empty, MensagensCadastro.DadosJaAssociados)`
      e `return View(dto)`. **É a correção do defeito.**
- [x] **T012** — `DocesCabana.MVC/Controllers/AutenticacaoController.cs`: trocar
      a dupla chamada a `BuscarPorLogin` por `ContaJaExiste` e o literal pela
      constante. Comportamento observável idêntico — mesma mensagem, mesma
      chave de `ModelState`, mesma view.
- [x] **T013** — Rodar `dotnet test`: T004 e T005 passam.

## Fase 6 — Fechamento

- [x] **T014** — Fumaça manual, com a aplicação rodando:
      - Cadastrar administrador com CPF já usado: mensagem correta, formulário
        de volta com o que foi digitado (**CA-01**).
      - Idem com e-mail já usado (**CA-02**).
      - Cadastrar cliente com CPF já usado e com e-mail já usado: mesma
        mensagem, sem regressão (**CA-03**, **CA-04**).
      - Depois da recusa por CPF, **consultar o banco** e confirmar que o e-mail
        da tentativa não ficou; então cadastrar de novo com esse mesmo e-mail e
        um CPF livre, e ver funcionar (**CA-05**). Conferir no banco, não na
        tela — foi exatamente essa conferência que faltou na `005`.
      - Cadastro válido nas duas portas segue criando conta (**CA-06**).
- [x] **T015** — `specs/005-gestao-de-administradores/plan.md`: corrigir a §3
      (RN-05 é provada em `UsuarioServiceCadastroTests`, não em
      `AdministradorServiceTests`) e a §6 (remover os dois nomes de teste que
      nunca existiram e apontar para os que existem). Dívida registrada na
      auditoria da `005`.
- [x] **T016** — `dotnet build` sem avisos novos e `dotnet test` verde, com
      contagem maior que a da T002.
- [x] **T017** — Preencher `checklist.md`.
- [x] **T018** — Atualizar a spec para *Implementada*, o plano para *Executado*,
      e `specs/README.md`: linha da `006` e renumeração da linha do Playwright
      para `007`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 — recusa CPF repetido em qualquer porta | T003, T004, T005, T009, T011, T012 |
| RF-02 — recusa e-mail repetido em qualquer porta | T004, T005, T009, T011, T012 |
| RF-03 — mesma mensagem em toda porta | T007, T011, T012 |
| RF-04 — nunca "erro interno" por dado repetido | T003, T009 |
| RF-05 — formulário volta com o que foi digitado | T004, T005, T011, T012 |
| RF-06 — nenhuma credencial órfã | T003, T009, T014 |
| RN-01 — e-mail e CPF únicos no sistema | T003, T009 |
| RN-02 — não revela qual campo repetiu | T007 (mensagem única e genérica) |
| RN-03 — regra vale para porta futura | T008, T009 (a regra mora no serviço) |
| CA-01 | T004, T014 |
| CA-02 | T004, T014 |
| CA-03 | T005, T014 |
| CA-04 | T005, T014 |
| CA-05 | T003, T014 |
| CA-06 | T005, T014 |
