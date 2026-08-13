# Tarefas — Testes E2E com Playwright

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

**Específico desta feature:** três coisas fora do comum.

1. **A emenda vem primeiro.** A T003 altera a constituição *antes* de qualquer
   pacote novo entrar. A ordem não é cerimônia: se o pacote entrar antes, o
   repositório passa a ter um estado em que o código viola a regra vigente, e é
   exatamente isso que a Governança §2 existe para impedir.
2. **Os E2E nascem verdes.** Eles descrevem comportamento que já existe, das
   specs `001` a `006`. O ciclo vermelho-antes vale integralmente para a única
   parte nova de produção — o adaptador de e-mail, Fases 3 e 4. Se um teste de
   fluxo nascer vermelho, **pare**: ou o sistema tem um defeito que ninguém viu,
   ou o teste está errado. Descobrir qual dos dois é o trabalho; marcar `[x]`
   não é.
3. **Nenhuma espera por tempo.** Sem `Thread.Sleep`, sem `WaitForTimeout`.
   Espera-se por condição — elemento visível, URL mudou, arquivo apareceu. Um
   teste que só passa às vezes é defeito, e um E2E instável é pior que E2E
   nenhum, porque ensina todo mundo a ignorar vermelho.

---

## Fase 1 — Preparação

- [x] **T001** — Criar branch `007-testes-e2e-com-playwright` a partir de `main`
      (com a `006` já integrada).
- [x] **T002** — Rodar `dotnet build` e `dotnet test`; registrar o estado inicial
      verde (257 na última medição) como linha de base da T029.

## Fase 2 — Governança

*Sem isto, tudo o que vem depois viola o Princípio V vigente.*

- [x] **T003** — `.specify/memory/constitution.md`: emenda **1.2.0 → 1.3.0**.
      Reescrever a linha de ferramentas do Princípio V conforme o plano §9
      (xUnit + Moq + coverlet para unidade e integração; `Microsoft.Playwright`
      para ponta a ponta; xUnit segue como runner único) e acrescentar a linha
      no histórico de emendas, com data e motivo.

## Fase 3 — Testes da parte nova de produção (devem falhar)

- [x] **T004** `[P]` — `DocesCabana.Tests/Units/Services/EmailServiceArquivoTests.cs`
      (criar): grava o corpo do e-mail num arquivo da pasta configurada; cria a
      pasta se não existir; dois envios geram dois arquivos; sem pasta
      configurada, falha em vez de escolher um lugar sozinho. **Prova RF-06.**
- [x] **T005** `[P]` — `DocesCabana.Tests/Units/DependencyInjections/RegistroDeEmailTests.cs`
      (criar): `Adaptador = "Arquivo"` resolve `EmailServiceArquivo`; ausente,
      vazio, ou com valor desconhecido resolve `EmailService`. **É a trava do
      risco 2 do plano §8** — o dia em que alguém trocar o padrão sem querer,
      este teste fica vermelho.
- [x] **T006** — Rodar `dotnet test` e confirmar que T004 e T005 falham pelo
      motivo esperado — ausência de `EmailServiceArquivo` e da propriedade
      `Adaptador` —, não por erro de compilação alheio.

## Fase 4 — Adaptador de e-mail

- [x] **T007** — `DocesCabana.Infrastructure/Services/EmailSettings.cs`:
      acrescentar `Adaptador` (padrão `"Smtp"`) e `PastaDeSaida` (padrão vazio,
      de propósito — ver T008).
- [x] **T008** — `DocesCabana.Infrastructure/Services/EmailServiceArquivo.cs`
      (criar): um arquivo por e-mail, com destinatário, assunto e corpo. Com
      `PastaDeSaida` vazia, **lançar** — nunca inventar um diretório, que é como
      e-mail vai parar em lugar servido por HTTP (risco 1 do plano §8).
- [x] **T009** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`:
      escolher o adaptador pela configuração. A comparação é
      **`"Arquivo"` explicitamente**; todo o resto cai em `EmailService`.
- [x] **T010** `[P]` — `DocesCabana.MVC/appsettings.Example.json`: documentar as
      duas chaves novas com `"Adaptador": "Smtp"`. Nenhuma credencial real.
- [x] **T011** — Rodar `dotnet test`: T004 e T005 passam; nada mais mudou de cor.

## Fase 5 — Andaime do E2E

*Nenhum teste de fluxo ainda. O objetivo é provar que a aplicação sobe e o
navegador a alcança, antes de empilhar doze fluxos em cima disso.*

- [x] **T012** — `DocesCabana.Tests.E2E/DocesCabana.Tests.E2E.csproj` (criar) e
      `tcc-doces-cabana.sln` (alterar): pacotes `Microsoft.Playwright`, `xunit`,
      `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`; `ProjectReference`
      da MVC com `ReferenceOutputAssembly="false"`, só para ordem de compilação.
- [x] **T013** — `DocesCabana.Tests.E2E/Infraestrutura/AplicacaoEmExecucao.cs`
      (criar): porta livre; pasta temporária com o SQLite e a pasta de e-mails;
      senha do administrador gerada na hora; sobe `dotnet DocesCabana.MVC.dll`
      com `WorkingDirectory` no projeto MVC; espera `GET /` responder `200`;
      derruba com `Kill(entireProcessTree: true)` e apaga a pasta. Em falha de
      subida, a exceção carrega `stdout` e `stderr` capturados.
      **Prova RF-04, RF-08, RN-01, RN-04, RN-05.**
- [x] **T014** — `DocesCabana.Tests.E2E/Infraestrutura/ColecaoE2E.cs` e
      `TesteE2E.cs` (criar): uma instância da aplicação para a suíte inteira,
      via `[CollectionDefinition]`; contexto de navegador novo por teste, para
      que cookie de um não vaze para o outro; rastro do Playwright gravado
      quando o teste falha. A classe base leva `[Trait("Categoria", "E2E")]`,
      que é o que a T029 vai conferir estar realmente separando as suítes.
      **Prova RN-02, RF-08.**
- [x] **T015** `[P]` — `DocesCabana.Tests.E2E/Infraestrutura/GeradorDeDados.cs`
      (criar): e-mail único e CPF com dígito verificador válido por chamada.
      **Prova RN-03.** Sem isto, dois testes que cadastram colidem no índice
      único e o segundo falha por motivo que não tem nada a ver com o que ele
      queria provar.
- [x] **T016** `[P]` — `DocesCabana.Tests.E2E/Infraestrutura/CaixaDeEntrada.cs`
      (criar): espera surgir arquivo na pasta de e-mails e extrai o link de
      redefinição do corpo. Espera por condição, não por tempo.
- [x] **T017** — Instalar o navegador e escrever **um** teste de fumaça: a
      página inicial abre e mostra a logo. Rodar e ver verde.
      **Esta é a tarefa que prova o andaime** — se ela não passar, nada nas
      fases seguintes vai passar, e por motivos que não têm a ver com os fluxos.

      Para a instalação, preferir a via programática
      (`Microsoft.Playwright.Program.Main(["install", "chromium"])`, chamada
      uma vez pela fixture ou por um utilitário do projeto) em vez do
      `playwright.ps1`: o script exige **PowerShell 7 (`pwsh`)**, que não é o
      PowerShell 5.1 que vem no Windows — e "o comando `pwsh` não existe" é uma
      mensagem que não ajuda ninguém a entender que faltou instalar navegador.

## Fase 6 — Objetos de página

*Escopar os seletores no formulário ou no `main`: o `_ModalLogin` do `_Layout`
tem links "Entrar" e "Cadastre-se" que colidem com os da página (risco do §8).*

- [x] **T018** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaLogin.cs` e
      `PaginaCadastro.cs` (criar).
- [x] **T019** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaEsqueceuSenha.cs` e
      `PaginaRedefinirSenha.cs` (criar).
- [x] **T020** `[P]` — `DocesCabana.Tests.E2E/Paginas/PaginaCadastroProduto.cs` e
      `PaginaAdministradores.cs` (criar).

## Fase 7 — Fluxos

- [x] **T021** `[P]` — `DocesCabana.Tests.E2E/Fluxos/CadastroDeClienteTests.cs`
      (criar): **CA-01, CA-02, CA-03**. A duplicidade cobre e-mail e CPF, nas
      duas portas — é a garantia de que a `006` não regride pela tela.
- [x] **T022** `[P]` — `DocesCabana.Tests.E2E/Fluxos/LoginTests.cs` (criar):
      **CA-04, CA-05, CA-06**. O teste de senha errada usa conta descartável
      própria: cinco erros bloqueiam por quinze minutos, e bloquear o
      administrador semeado derruba metade da suíte.
- [x] **T023** — `DocesCabana.Tests.E2E/Fluxos/RecuperacaoDeSenhaTests.cs`
      (criar): **CA-07, CA-08**. A redefinição vai do pedido ao login com a
      senha nova, passando pelo link lido da caixa de entrada, e confirma que a
      senha antiga deixou de valer. **Prova RF-07.**
- [x] **T024** `[P]` — `DocesCabana.Tests.E2E/Fluxos/CadastroDeProdutoTests.cs`
      (criar): **CA-09**, caminho feliz e campo inválido. Usa a subcategoria de
      identificador fixo que o `DbInitializer` semeia desde a `003`.
- [x] **T025** `[P]` — `DocesCabana.Tests.E2E/Fluxos/AreaAdministrativaTests.cs`
      (criar): **CA-10, CA-11, CA-12**.
- [x] **T026** — Rodar `dotnet test DocesCabana.Tests.E2E` inteiro e ver verde.

## Fase 8 — Fechamento

- [x] **T027** — `DocesCabana.Tests.E2E/README.md` (criar): instalação do
      navegador e os dois comandos — `dotnet test --filter "Categoria!=E2E"`
      para o ciclo rápido, `dotnet test DocesCabana.Tests.E2E` para o E2E.
      **Prova RF-05, RF-09.**
- [x] **T028** — Rodar a suíte E2E **duas vezes seguidas**, sem limpar nada
      entre elas, e confirmar mesmo resultado. **Prova CA-13, RN-05.** Se a
      segunda execução falhar, há resíduo — e resíduo é defeito da T013, não
      motivo para rodar `git clean` e seguir.
- [x] **T029** — `dotnet build` sem avisos novos; `dotnet test --filter
      "Categoria!=E2E"` verde com contagem maior que a da T002 (as duas de
      unidade da Fase 3). **Conferir que o filtro de fato exclui os E2E** —
      comparar a contagem com e sem o filtro, e ver a diferença bater com o
      número de testes de fluxo. Marca herdada de classe base é o tipo de coisa
      que se assume que funciona e às vezes não funciona.
- [x] **T030** — Preencher `checklist.md`.
- [x] **T031** — Atualizar a spec para *Implementada*, o plano para *Executado*,
      e a linha da `007` em `specs/README.md`.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 — percorre os fluxos existentes | T021, T022, T023, T024, T025 |
| RF-02 — exercita pela interface | T018, T019, T020 |
| RF-03 — cobre caminhos de erro | T021, T022, T023, T024, T025 |
| RF-04 — base própria e descartável | T013 |
| RF-05 — um comando, instalação documentada | T017, T027 |
| RF-06 — sem serviço externo | T004, T007, T008, T009 |
| RF-07 — redefinição de ponta a ponta | T016, T023 |
| RF-08 — falha diz o que quebrou e onde | T013, T014 |
| RF-09 — fora do ciclo rápido | T014 (marca na base), T027 (comandos), T029 (confere que o filtro exclui) |
| RN-01 — estado conhecido a cada execução | T013 |
| RN-02 — teste não depende de teste | T014 |
| RN-03 — dados não colidem | T015 |
| RN-04 — nenhuma credencial real | T010, T013 |
| RN-05 — execução repetível | T013, T028 |
| CA-01, CA-02, CA-03 | T021, T026 |
| CA-04, CA-05, CA-06 | T022, T026 |
| CA-07, CA-08 | T023, T026 |
| CA-09 | T024, T026 |
| CA-10, CA-11, CA-12 | T025, T026 |
| CA-13 | T028 |
| Emenda 1.3.0 | T003 |
