# Plano Técnico — Testes E2E com Playwright

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-13
**Status:** Rascunho

---

## 1. Resumo da abordagem

Um projeto novo, `DocesCabana.Tests.E2E`, com **xUnit continuando como runner** e
o pacote `Microsoft.Playwright` entrando apenas como a biblioteca que dirige o
navegador. Essa distinção é o que mantém a emenda constitucional pequena: não
há runner novo, há um driver novo.

Uma *fixture* compartilhada sobe a aplicação de verdade num processo filho, em
porta livre, apontada para um SQLite descartável e para um adaptador de e-mail
que grava em disco em vez de mandar por SMTP. É esse adaptador — a única linha
de código de produção que esta feature acrescenta — que permite concluir a
redefinição de senha de ponta a ponta (RF-07) sem depender de serviço externo
(RF-06).

Os testes falam com a tela por papel e rótulo (`GetByRole`, `GetByLabel`), não
por seletor de CSS, e ficam atrás de objetos de página em português. A aplicação
não muda em mais nada: porta, banco, senha do administrador semeado e escolha do
adaptador entram todos por variável de ambiente, porque a `Program.cs` já lê
tudo de configuração.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | `EmailServiceArquivo` fica na Infrastructure implementando o `IEmailService` da Application — mesma direção do `EmailService` que já existe. O projeto E2E referencia a MVC só para garantir ordem de compilação (`ReferenceOutputAssembly="false"`), não para usar tipos |
| II | Domínio rico e auto-validante | ⬜ n/a | Nenhuma entidade nova |
| III | Validação nas duas barreiras | ⬜ n/a | Nenhuma entrada nova de usuário |
| IV | Nomenclatura em português | ✅ OK | `AplicacaoEmExecucao`, `CaixaDeEntrada`, `PaginaLogin`, `GeradorDeDados`; testes em `Dado_..._Quando_..._Entao_...` |
| V | Testes escritos antes | ❌ **Violado** | Duas coisas distintas. (a) **Ferramenta:** o princípio proíbe framework de teste novo, e Playwright é um. Resolvido por emenda 1.3.0 — ver §9. (b) **Ordem:** os testes E2E descrevem comportamento que **já existe**; escrevê-los "vermelhos primeiro" não faz sentido, e um E2E que falha aqui indica defeito, não passo do ciclo. A regra do vermelho-antes continua valendo integralmente para a única parte nova de produção, o `EmailServiceArquivo`, que tem teste de unidade antes |
| VI | Repositório + commit via `UnitOfWork` | ✅ OK | Nenhuma escrita nova e nenhuma migration: o banco do E2E é criado pelas migrations que a própria aplicação roda ao subir |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | Nenhuma ação de controller muda. **Atenção de segurança:** o adaptador de arquivo é opt-in e nunca o padrão, e a pasta de saída fica fora do content root — ver risco 1 e risco 2 na §8 |
| VIII | Tratamento de erro por camada | ⬜ n/a | Nenhum controller muda |

## 3. Impacto por camada

### `DocesCabana.Domain`

Nenhuma alteração.

### `DocesCabana.Application`

Nenhuma alteração. `IEmailService` já tem a assinatura de que precisamos
(`EnviarEmail(string email, string assunto, string corpo)`).

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Services/EmailSettings.cs` | alterar | Duas propriedades: `Adaptador` (padrão `"Smtp"`) e `PastaDeSaida` |
| `Services/EmailServiceArquivo.cs` | **criar** | Implementa `IEmailService` gravando um arquivo por e-mail na `PastaDeSaida` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Escolhe o adaptador pela configuração; **qualquer valor diferente de `"Arquivo"` cai no SMTP** |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `appsettings.Example.json` | alterar | Documentar as duas chaves novas, com `Adaptador: "Smtp"` |

### `DocesCabana.Tests` (suíte rápida, já existente)

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Services/EmailServiceArquivoTests.cs` | **criar** | Grava o corpo; cria a pasta se não existir; um arquivo por envio |
| `Units/DependencyInjections/RegistroDeEmailTests.cs` | **criar** | `"Arquivo"` resolve o adaptador de arquivo; ausente, vazio ou desconhecido resolve o SMTP — é a trava do risco 2 |

### `DocesCabana.Tests.E2E` (projeto novo)

| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests.E2E.csproj` | **criar** | `Microsoft.Playwright`, xUnit, `Microsoft.NET.Test.Sdk`; `ProjectReference` da MVC com `ReferenceOutputAssembly="false"` |
| `Infraestrutura/AplicacaoEmExecucao.cs` | **criar** | Sobe a app em processo filho, porta livre, banco e pasta de e-mails temporários; derruba com `Kill(entireProcessTree: true)` |
| `Infraestrutura/ColecaoE2E.cs` | **criar** | `[CollectionDefinition]` para uma instância só, compartilhada |
| `Infraestrutura/TesteE2E.cs` | **criar** | Base: abre o navegador, cria contexto isolado por teste, guarda rastro em caso de falha |
| `Infraestrutura/GeradorDeDados.cs` | **criar** | E-mail único e CPF com dígito verificador válido, por teste (RN-03) |
| `Infraestrutura/CaixaDeEntrada.cs` | **criar** | Lê a pasta de e-mails e extrai o link de redefinição |
| `Paginas/*.cs` | **criar** | `PaginaLogin`, `PaginaCadastro`, `PaginaEsqueceuSenha`, `PaginaRedefinirSenha`, `PaginaCadastroProduto`, `PaginaAdministradores` |
| `Fluxos/*.cs` | **criar** | Os testes, um arquivo por fluxo — ver §6 |
| `README.md` | **criar** | Instalação do navegador e os dois comandos de execução |

### Raiz

| Arquivo | Ação | O quê |
|---|---|---|
| `tcc-doces-cabana.sln` | alterar | Acrescentar o projeto novo |
| `.specify/memory/constitution.md` | alterar | **Emenda 1.3.0** — ver §9 |

## 4. Contratos

```csharp
// Infrastructure/Services/EmailSettings.cs — acrescentado
public string Adaptador { get; set; } = "Smtp";
public string PastaDeSaida { get; set; } = string.Empty;

// Infrastructure/Services/EmailServiceArquivo.cs — novo
public class EmailServiceArquivo : IEmailService
{
    public Task EnviarEmail(string email, string assunto, string corpo);
}
```

### Como a aplicação sobe

Nada na `Program.cs` muda. Tudo entra por variável de ambiente, porque
`WebApplication.CreateBuilder` já lê o ambiente por cima do `appsettings.json`:

| Variável | Para quê |
|---|---|
| `ASPNETCORE_ENVIRONMENT=Development` | Sem isso a massa inicial não é semeada — a `Program.cs` só semeia fora de produção |
| `ConnectionStrings__DefaultConnection` | SQLite descartável, em pasta temporária (RF-04) |
| `Admin__SenhaInicial` | Senha do administrador semeado, conhecida pelo teste e gerada na hora (RN-04) |
| `EmailSettings__Adaptador=Arquivo` | Liga o adaptador de arquivo |
| `EmailSettings__PastaDeSaida` | Pasta temporária, **fora do content root** |

O processo é iniciado como `dotnet DocesCabana.MVC.dll` com `WorkingDirectory`
na pasta do projeto MVC — assim o content root resolve views e `wwwroot`
corretamente, e não há processo intermediário de `dotnet run` para virar órfão.
A prontidão é aferida por `GET /` até responder `200`, com teto de tempo; se
estourar, a mensagem de falha carrega o que a aplicação escreveu em `stdout` e
`stderr`, que é o que a RF-08 pede.

### Por que um adaptador, e não um servidor SMTP de mentira

A alternativa "fiel" seria subir um servidor SMTP falso e deixar o `EmailService`
real mandar para ele — exercitaria também o código de envio. Sem pacote novo,
isso exige implementar `EHLO`/`MAIL FROM`/`RCPT TO`/`DATA` na mão: umas cem
linhas de protocolo, num projeto que não tem essa complexidade em lugar nenhum.
O adaptador de arquivo é menor, previsível, e o que o E2E precisa provar não é
que o .NET fala SMTP — é que o token é gerado, o link é montado e a senha troca.

O custo é honesto e fica registrado: **o caminho de envio real por SMTP continua
sem cobertura automatizada**, exatamente como está hoje.

## 5. Modelo de dados

Nenhuma mudança de esquema e nenhuma migration. O banco do E2E é um arquivo
SQLite novo a cada execução, criado pelas migrations que a aplicação já roda no
`DbInitializer.Migrar` ao subir, e semeado pelo `DbInitializer.Semear`.

Os identificadores fixos de categoria e subcategoria que a `003` deixou no
`DbInitializer` — com o comentário dizendo, literalmente, que existem *"para que
testes e E2E possam referenciar uma categoria/subcategoria conhecida"* — são o
que torna o cadastro de produto testável sem adivinhar chave.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade | `Units/Services/EmailServiceArquivoTests.cs` | O adaptador novo — única produção nova, com vermelho antes |
| Unidade | `Units/DependencyInjections/RegistroDeEmailTests.cs` | O padrão é SMTP; só `"Arquivo"` troca |
| E2E | `Fluxos/CadastroDeClienteTests.cs` | CA-01, CA-02, CA-03 |
| E2E | `Fluxos/LoginTests.cs` | CA-04, CA-05, CA-06 |
| E2E | `Fluxos/RecuperacaoDeSenhaTests.cs` | CA-07, CA-08 |
| E2E | `Fluxos/CadastroDeProdutoTests.cs` | CA-09 |
| E2E | `Fluxos/AreaAdministrativaTests.cs` | CA-10, CA-11, CA-12 |

`CA-13` (execução repetível) não vira um teste: é provado rodando a suíte duas
vezes seguidas na tarefa de fechamento. Um teste que se auto-verifica quanto a
isso não provaria nada além de si mesmo.

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_DadosInéditos_Quando_CadastrarCliente_Entao_DeveLevarAoLoginEPermitirEntrar` |
| CA-02 | `Dado_EmailJaUsado_Quando_CadastrarCliente_Entao_DeveMostrarMensagemDeDuplicidade` + irmão para CPF e para a porta de administrador |
| CA-03 | `Dado_SenhaFraca_Quando_CadastrarCliente_Entao_DeveMostrarErroDeMaiuscula` |
| CA-04 | `Dado_ContaExistente_Quando_EntrarComEmailEComCpf_Entao_DeveAutenticarNosDois` |
| CA-05 | `Dado_SenhaErrada_Quando_Entrar_Entao_DeveMostrarCredencialIncorreta` |
| CA-06 | `Dado_Autenticado_Quando_Sair_Entao_DevePerderAcessoAAreaAdministrativa` |
| CA-07 | `Dado_LoginExistenteEInexistente_Quando_PedirRedefinicao_Entao_DeveMostrarAMesmaMensagem` |
| CA-08 | `Dado_PedidoDeRedefinicao_Quando_SeguirOLinkETrocarASenha_Entao_DeveEntrarComANovaENaoComAAntiga` |
| CA-09 | `Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar` + irmão para campo inválido |
| CA-10 | `Dado_VisitanteECliente_Quando_AbrirAreaAdministrativa_Entao_DeveLevarAoLoginEDarAcessoNegado` |
| CA-11 | `Dado_Administrador_Quando_CadastrarOutroAdministrador_Entao_ELeDeveEntrarEUsarAArea` |
| CA-12 | `Dado_ClienteComum_Quando_OlharOCabecalho_Entao_NaoDeveVerCaminhoAdministrativo` |
| CA-13 | Tarefa de fechamento: duas execuções seguidas, mesmo resultado |

### Separar do ciclo rápido (RF-09)

Os testes E2E levam a marca `[Trait("Categoria", "E2E")]`, posta na classe base
`TesteE2E`. O ciclo rápido passa a ser `dotnet test --filter "Categoria!=E2E"`,
e o E2E, `dotnet test DocesCabana.Tests.E2E`. Os dois comandos ficam no
`README.md` do projeto novo.

Marca herdada de classe base é justamente o tipo de detalhe que se assume
funcionando e às vezes não funciona, então a tarefa de fechamento compara a
contagem de testes com e sem o filtro em vez de confiar.

**Custo assumido:** quem rodar `dotnet test` puro na raiz continua pegando a
suíte inteira, agora mais lenta. A alternativa — deixar o projeto fora da
solução — resolveria por construção, mas esconde o projeto de quem abre a
solução na IDE, o que é pior num projeto de TCC que alguém vai avaliar lendo.
Fica o filtro documentado.

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| `WebApplicationFactory` / `TestServer` em memória | Não abre socket de verdade; o navegador precisa de uma URL real para navegar |
| Selenium | Exige gerenciar o driver do navegador à parte e não tem espera automática por elemento — mais peça móvel para o mesmo resultado |
| Playwright em Node/TypeScript | Traria npm e um segundo ecossistema para um repositório .NET, com os testes longe do código que descrevem |
| `Microsoft.Playwright.NUnit` ou `.MSTest` | Traria um **runner** novo — aí sim um framework de teste novo, emenda bem maior. O pacote base com xUnit mantém um runner só |
| Rodar contra a base de desenvolvimento | Quebraria RF-04 e RN-05, e faria o E2E apagar dados de quem está desenvolvendo |
| Servidor SMTP de mentira dentro do teste | Cem linhas de protocolo na mão para provar algo que não é o objetivo — ver §4 |
| Endpoint que devolve o último e-mail, para o teste ler | Rota que expõe conteúdo de e-mail é buraco de segurança em produção, mesmo "protegida" |
| Subir a app com `dotnet run` | Cria um processo intermediário que pode virar órfão ao ser derrubado; rodar a dll direto evita isso |
| Um app por teste, em vez de um compartilhado | Cada subida custa segundos e o ganho de isolamento é obtido de forma mais barata com dados únicos por teste (RN-03) |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| A pasta de e-mails cair dentro de `wwwroot` e passar a servir e-mails por HTTP | Baixa | **Alto** | A pasta é criada em diretório temporário do sistema, nunca sob o content root. O `EmailServiceArquivo` não tem valor padrão de pasta: sem configuração explícita ele falha, em vez de escolher um lugar por conta própria |
| O adaptador de arquivo virar o padrão por engano e a loja parar de enviar e-mail em silêncio | Baixa | **Alto** | `Adaptador` nasce `"Smtp"`; **qualquer** valor não reconhecido resolve SMTP, não arquivo. `RegistroDeEmailTests` trava isso, e o `appsettings.Example.json` mostra o valor correto |
| A aplicação não subir e o teste falhar com timeout sem explicação | Média | Médio | `stdout` e `stderr` do processo são capturados e entram na mensagem da falha (RF-08) |
| Bloqueio de conta — 5 tentativas erradas travam por 15 minutos — contaminar testes vizinhos | Média | Médio | O teste de senha errada usa conta descartável própria, nunca o administrador semeado |
| Seletor ambíguo: o `_ModalLogin` do `_Layout` tem links "Entrar" e "Cadastre-se" que colidem com os da página | Média | Baixo | Objetos de página escopam a busca no formulário ou no `main`, não na página inteira |
| Porta escolhida ser tomada entre a escolha e a subida | Baixa | Baixo | Nova tentativa com outra porta |
| Navegador não instalado na primeira execução | **Alta** | Baixo | Instalação pela via programática (`Microsoft.Playwright.Program.Main(["install", "chromium"])`), não pelo `playwright.ps1` — o script exige PowerShell 7 (`pwsh`), que não é o 5.1 que vem no Windows. Documentado no `README.md` do projeto |
| Caminho até a dll da MVC quebrar entre Debug e Release | Média | Médio | Resolvido subindo a partir do diretório do teste até achar o `.sln`, e derivando a configuração do próprio caminho do assembly de teste |
| E2E instável virar ruído e ser ignorado | Média | **Alto** | Espera por condição, nunca por tempo fixo; nenhum `Thread.Sleep`. Um teste que só passa às vezes é tratado como defeito, não como característica |

## 9. Desvios constitucionais justificados

| Princípio | Desvio | Justificativa | Alternativa descartada |
|---|---|---|---|
| V | Introduz `Microsoft.Playwright`, que o princípio proíbe ao dizer "não introduzir framework de teste novo" | O princípio existe para evitar proliferação de stack de teste — três formas de escrever a mesma asserção. Não é esse o caso: xUnit, Moq e coverlet continuam sendo a stack, o runner continua sendo o xUnit, e o Playwright entra como **driver de navegador**, cobrindo uma camada que a stack atual não alcança de jeito nenhum. Sem ele, os requisitos da spec são inatingíveis | Testar a interface por requisição HTTP crua, sem navegador. Foi o que a fumaça manual das specs `004` a `006` fez com `curl` — e não enxerga JavaScript, validação de campo no cliente, nem o cookie de sessão do jeito que o navegador enxerga |

**Emenda proposta — 1.2.0 → 1.3.0 (MINOR, expansão material do Princípio V).**
A linha de ferramentas passa a distinguir a camada:

> Ferramentas fixas: xUnit + Moq + coverlet para teste de unidade e de
> integração; `Microsoft.Playwright` para teste de ponta a ponta em navegador,
> com o xUnit seguindo como runner único. Não introduzir outro framework de
> teste, nem um segundo runner.

E o histórico ganha a linha correspondente. A emenda é tarefa desta feature, não
efeito colateral dela.
