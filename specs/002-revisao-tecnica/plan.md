# Plano Técnico — Revisão técnica da base

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-11
**Status:** Rascunho

---

## 1. Resumo da abordagem

O trabalho é feito em cinco blocos independentes entre si, cada um fechando em
`dotnet test` verde: **(A)** correção dos defeitos de autenticação, concentrada em
`UsuarioService` e no registro do Identity; **(B)** correção do domínio e do
mapeamento de produto, que hoje descarta o status e atribui antes de validar;
**(C)** remoção da abstração de transação, reduzindo `IUnitOfWork` a um método;
**(D)** padronização de nomes de arquivo, tipo, namespace e teste, sem mudança de
comportamento; **(E)** configuração, segredos e dependências.

A ordem entre blocos importa em um ponto só: **D vem por último**, porque renomear
arquivos que os blocos anteriores estão editando gera conflito desnecessário. Os
blocos A, B, C e E tocam conjuntos de arquivos disjuntos e podem ser feitos em
qualquer ordem.

Nenhuma migration é criada. A RQ-08 troca `HasColumnType` por API neutra de
provider, o que **não** altera o esquema gerado para SQLite — verificar isso é
uma tarefa explícita (T033).

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | A RQ-01 **restaura** a conformidade: remove o pacote `Microsoft.Extensions.Identity.Stores` do `Domain`, hoje declarado e não usado. Nenhuma `ProjectReference` muda. |
| II | Domínio rico e auto-validante | ✅ OK | A RF-07 restaura a conformidade em `Produto` e `Usuario`: validação passa a preceder toda atribuição. `private set` e `protected Ctor()` são preservados. |
| III | Validação nas duas barreiras | ✅ OK | A RQ-06 cria `ProdutoDTOValidator`, a barreira de entrada que falta. As invariantes de `Produto` permanecem — a duplicação é intencional, conforme o princípio. |
| IV | Nomenclatura em português | ⚠️ Emenda | O princípio não diz nada sobre nome de arquivo × nome de tipo. A RQ-03 acrescenta essa regra. Emenda **MINOR** → constituição 1.1.0. Ver seção 9. |
| V | Testes escritos antes | ✅ OK | Toda correção de defeito é precedida do teste que a prova falhando (Fase 2). A RQ-05 renomeia os testes fora do padrão. |
| VI | Repositório + commit via UnitOfWork | ⚠️ Emenda | A RQ-02 remove a transação explícita. O texto "o commit é explícito" continua valendo — o que sai é a transação manual, não o commit. Emenda **PATCH** → 1.0.1, absorvida na 1.1.0. Ver seção 9. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | A RF-05 acrescenta guarda de `ModelState` em `EsqueceuSenha`; a RF-06 preserva a anti-enumeração; a RQ-07 tira credencial do versionamento. Os defeitos de `AdminController` (D-02, D-03) são da spec `001`. |
| VIII | Tratamento de erro por camada | ✅ OK | Nenhum `try/catch` novo em controller. `FilterException` não é tocado — muda na `001`. |

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Contracts/ITransaction.cs` | **remover** | RQ-02 — abstração sem consumidor |
| `Contracts/IUnitOfWork.cs` | alterar | RQ-02 — fica só `SalvarAlteracoes`; sai `IniciarTransacao`, `ExecutarEmTransacao` e a herança de `IAsyncDisposable` |
| `Entities/Produto.cs` | alterar | RF-03 — construtor recebe `ProdutoStatus status = ProdutoStatus.Ativo`. RF-07 — `ProdutoId` e `Status` passam a ser atribuídos depois das validações |
| `DocesCabana.Domain.csproj` | alterar | RQ-01 — remover `PackageReference` de `Microsoft.Extensions.Identity.Stores` |

> **Por que `IUnitOfWork` deixa de ser `IAsyncDisposable`:** o `UnitOfWork` não é
> dono do `DbContext` — ambos são `Scoped` e o contêiner descarta os dois. Hoje
> `UnitOfWork.DisposeAsync()` descarta um contexto compartilhado com os
> repositórios. Ninguém chama, então não quebrou ainda; é defeito latente.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Mappings/ProdutoMapper.cs` | alterar | RF-03 — `ToEntity` passa a repassar `dto.Status` |
| `Services/ProdutoService.cs` | alterar | RF-03 — `Cadastrar` devolve o DTO mapeado da entidade, não o DTO de entrada |
| `Validators/ProdutoDTOValidator.cs` | **criar** | RQ-06 — nome, preço, imagem e subcategoria, espelhando as invariantes de `Produto` |
| `Validators/EsqueceuSenhaValidator.cs` | **renomear** | RQ-03 → `EsqueceuSenhaDTOValidator.cs` |
| `Validators/LoginDTOValidator.cs` | alterar | RQ-03 — extrair `ValidarEmailOuCpf`, hoje duplicado byte a byte com o validator de recuperação de senha |
| `Validators/EsqueceuSenhaDTOValidator.cs` | alterar | idem, consumir o método extraído |
| `DTOs/ProdutoDTO.cs` | alterar | RQ-03 — `Id` → `ProdutoId`; `EstaFavorito` passa de `set` para `init` |
| `DTOs/EsqueceuSenhaDTO.cs` | **mover** | RQ-03 → `DTOs/Autenticacao/`; remover a propriedade `Id`, nunca lida |
| `DTOs/RedefinirSenhaDTO.cs` | **mover** | RQ-03 → `DTOs/Autenticacao/`; remover a propriedade `Id`, nunca lida |
| `DTOs/Autenticacao/CadastroDTO.cs` | alterar | RQ-04 — `Telefone` → `Celular` |
| `DocesCabana.Application.csproj` | **sem alteração** | `FluentValidation.AspNetCore` está em 11.3.1, que é a última versão publicada — o pacote foi descontinuado. Ver seção 7 |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/TransactionEf.cs` | **remover** | RQ-02 |
| `Repositories/UnitOfWork.cs` | alterar | RQ-02 — fica só `SalvarAlteracoes`; sai `DisposeAsync` |
| `Identity/Services/UsuarioService.cs` | alterar | RF-01 — `RealizarLogin` resolve a entidade uma vez e autentica pelo e-mail dela. Extrair `ResolverUsuario(string login)` privado, consumido por `BuscarPorLogin` e `RealizarLogin`. RF-02 — `lockoutOnFailure: true` |
| `Identity/Usuario.cs` | alterar | RF-07 — `UserName` e `PhoneNumber` atribuídos depois das validações; remover comentários fósseis; `Regex` de e-mail vira `static readonly` compilada, como em `TelefoneHelper` |
| `Identity/Mappings/UsuarioMapper.cs` | alterar | RQ-04 — `dto.Telefone` → `dto.Celular` |
| `DependencyInjections/IdentityDependencyInjection.cs` | alterar | RN-02 — `MaxFailedAccessAttempts = 5`, `DefaultLockoutTimeSpan = 15 min`, `AllowedForNewUsers = true` |
| `DependencyInjections/DbContextDependencyInjection.cs` | alterar | RQ-03 — classe `DatabaseConfig` → `DbContextDependencyInjection` |
| `DatabaseContext/Configurations/ProdutoConfiguration.cs` | alterar | RQ-08 — remover **apenas** `HasColumnType("INTEGER")` do `Status`. O `decimal(18,2)` do `Preco` **fica**. Ver seção 5 |
| `DocesCabana.Infrastructure.csproj` | alterar | RQ-09 — `PackageReference` explícito de `SQLitePCLRaw.lib.e_sqlite3` na menor versão sem advisory |

> **Sobre `ResolverUsuario`:** o defeito da RF-01 é que `RealizarLogin` chama
> `BuscarPorLogin` (que resolve e-mail **ou** CPF, devolvendo um DTO) e em seguida
> `FindByEmailAsync(login)` com o login cru — que devolve `null` quando o login é
> CPF. Além de quebrar o CPF, são três idas ao banco onde uma basta.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AutenticacaoController.cs` | alterar | RF-04/RF-05 — guarda de `ModelState` no POST de `EsqueceuSenha`; mensagem neutra via `TempData["Confirmacao"]`, não via `AddModelError`. Remover o `ILogger` injetado e nunca usado |
| `Controllers/HomeController.cs` | alterar | RQ-03 — remover `using Microsoft.AspNetCore.Authorization` não utilizado |
| `Views/Autenticacao/EsqueceuSenha.cshtml` | alterar | RF-04 — exibir `TempData["Confirmacao"]` com estilo de confirmação |
| `Views/Home/Index.cshtml`, `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | RQ-03 — acompanhar a renomeação `ProdutoDTO.Id` → `ProdutoId`, se referenciada |
| `Helpers/DbInitializer.cs` | alterar | Separar migrar de semear; semeadura só fora de produção |
| `Program.cs` | alterar | Acompanhar a mudança do `DbInitializer`; qualificar a chamada, hoje escrita com namespace completo apesar do `using` já presente |
| `appsettings.json` | **destrackear** | RQ-07 — `git rm --cached`, arquivo local intacto |
| `appsettings.Example.json` | **criar** | RQ-07 — mesma estrutura, `EmailSettings` com valores vazios |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Integration/DatabaseIntegrationTests.cs` | alterar | RQ-02 — os dois testes de transação saem; entra um teste de atomicidade de `SalvarAlteracoes`. Remover `using Microsoft.Data.Sqlite` não utilizado |
| `Integration/Repositories/ProdutoRepositoryIntegrationTests.cs` | alterar | RQ-02 — os três usos de `ExecutarEmTransacao` passam a `SalvarAlteracoes` |
| `Integration/InfraestruturaSqliteEmMemoria.cs` | alterar | RQ-03 — remover `using` não utilizados (`Microsoft.Data.Sqlite` é usado; `System.Threading.Tasks` e `Xunit` são implícitos) |
| `Units/Controllers/AutenticacaoControllerTests.cs` | alterar | RQ-05 — renomear ~17 testes; RF-04/RF-05 — ajustar as asserções de `EsqueceuSenha` para `TempData` |
| `Units/Controllers/HomeControllerTests.cs` | alterar | RQ-05 — renomear 3 testes |
| `Units/Services/UsuarioServiceTests.cs` | alterar | RQ-05 — renomear ~20 testes |
| `Units/Entities/ProdutoTests.cs` | alterar | RF-03/RF-07 — testes de status no construtor e de não-atribuição parcial |
| `Units/Entities/UsuarioTests.cs` | alterar | RF-07 — teste de não-atribuição parcial |
| `Units/Services/ProdutoServiceTests.cs` | alterar | RQ-11 — testes de `Cadastrar` |
| `Units/Services/UsuarioServiceLoginTests.cs` | **criar** | RF-01/RF-02 — login por CPF com e sem pontuação, login por e-mail, `lockoutOnFailure` |
| `Units/Validators/ProdutoDTOValidatorTests.cs` | **criar** | RQ-06 |
| `Units/Validators/RedefinirSenhaDTOValidatorTests.cs` | **criar** | RQ-11 |
| `Units/Validators/EsqueceuSenhaDTOValidatorTests.cs` | **criar** | RQ-11 |
| `Units/Mappings/ProdutoMapperTests.cs` | **criar** | RF-03 — ida e volta preservando `Status` |
| `Units/Mappings/UsuarioMapperTests.cs` | **criar** | RQ-11 |
| `Units/Helpers/CpfHelperTests.cs` | **criar** | RQ-11 — dígito verificador, dígitos repetidos, formato |
| `Units/Helpers/TelefoneHelperTests.cs` | **criar** | RQ-11 — DDD válido e inválido, nono dígito |

### Documentação

| Arquivo | Ação | O quê |
|---|---|---|
| `.specify/memory/constitution.md` | alterar | Emendas do Princípio IV e VI, versão 1.1.0, com linha no histórico |
| `specs/000-baseline/spec.md` | alterar | RQ-10 — §4.3 SQLite em vez de SQL Server; §6 marcar D-06 como resolvida aqui e D-01..D-05 como endereçadas pela `001` |
| `specs/README.md` | alterar | Inserir a linha da `002`; renumerar o backlog 002–010 → 003–011 |
| `README.md` | alterar | RQ-07 — instrução de copiar `appsettings.Example.json` e de usar *user secrets* |

## 4. Contratos

```csharp
// DocesCabana.Domain/Contracts/IUnitOfWork.cs — depois da RQ-02
public interface IUnitOfWork
{
    Task<int> SalvarAlteracoes(CancellationToken cancellationToken = default);
}
```

```csharp
// DocesCabana.Domain/Entities/Produto.cs — construtor, depois da RF-03
public Produto(
    Guid subcategoriaId,
    string nome,
    decimal preco,
    string imagemUrl,
    ProdutoStatus status = ProdutoStatus.Ativo,
    Guid id = default);
```

```csharp
// DocesCabana.Infrastructure/Identity/Services/UsuarioService.cs — novo privado
private async Task<Usuario?> ResolverUsuario(string login);
```

`IProdutoService`, `IProdutoRepository`, `IRepository<T>`, `IUsuarioService` e
`IEmailService` ficam **inalterados**.

## 5. Modelo de dados

Nenhuma entidade nova, nenhuma propriedade nova, nenhuma migration.

A RQ-08 é mais estreita do que parecia no desenho inicial. Verificando o
`DocesCabanaDbContextModelSnapshot`, as duas chamadas de `HasColumnType` do
`ProdutoConfiguration` têm naturezas diferentes:

| Chamada | Coluna no snapshot | No SQL Server | Ação |
|---|---|---|---|
| `Status`: `HasColumnType("INTEGER")` | `INTEGER` | **quebra** — `INTEGER` não é tipo do SQL Server | **remover** |
| `Preco`: `HasColumnType("decimal(18,2)")` | `decimal(18,2)` | válido | **manter** |

`ProdutoStatus` é `enum : byte`. Sem a anotação, o provider SQLite mapeia `byte`
para `INTEGER` — exatamente a coluna de hoje, sem migration. Já no SQL Server ele
mapearia para `tinyint`, que é o correto. A anotação atual é o único ponto que
impede a troca de provider.

O `decimal(18,2)` do `Preco` **fica como está**. É sintaxe válida no SQL Server, e
no SQLite dá à coluna afinidade `NUMERIC` — o comportamento desejado para dinheiro.
Trocar por `HasPrecision(18, 2)`, como o desenho inicial previa, mudaria a coluna
para `TEXT` e com ela a afinidade, afetando comparação e ordenação de preço. Seria
uma migration e uma regressão sutil para resolver um problema que não existe.

A tarefa T033 confirma que a remoção não altera o esquema, gerando uma migration
de verificação, conferindo que sai vazia e descartando-a.

- **Impacto em dados existentes:** nenhum.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/ProdutoTests.cs` | RF-03, RF-07, RN-03, RN-04 |
| Unidade — entidade | `Units/Entities/UsuarioTests.cs` | RF-07, RN-04 |
| Unidade — serviço | `Units/Services/UsuarioServiceLoginTests.cs` | RF-01, RF-02, RN-01 |
| Unidade — serviço | `Units/Services/ProdutoServiceTests.cs` | RF-03 |
| Unidade — mapeamento | `Units/Mappings/ProdutoMapperTests.cs` | RF-03 |
| Unidade — mapeamento | `Units/Mappings/UsuarioMapperTests.cs` | RQ-11 |
| Unidade — auxiliar | `Units/Helpers/CpfHelperTests.cs` | RN-01, RQ-11 |
| Unidade — auxiliar | `Units/Helpers/TelefoneHelperTests.cs` | RQ-11 |
| Unidade — validator | `Units/Validators/ProdutoDTOValidatorTests.cs` | RQ-06 |
| Unidade — validator | `Units/Validators/EsqueceuSenhaDTOValidatorTests.cs` | RF-05 |
| Unidade — controller | `Units/Controllers/AutenticacaoControllerTests.cs` | RF-04, RF-05, RF-06 |
| Integração | `Integration/DatabaseIntegrationTests.cs` | RQ-02 — atomicidade sem transação explícita |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_LoginPorCpfSemPontuacao_Quando_RealizarLogin_Entao_DeveAutenticar` |
| CA-02 | `Dado_LoginPorCpfPontuado_Quando_RealizarLogin_Entao_DeveAutenticar` |
| CA-03 | `Dado_LoginPorEmail_Quando_RealizarLogin_Entao_DeveAutenticar` |
| CA-04 | `Dado_ContaBloqueada_Quando_RealizarLogin_Entao_DeveRetornarLockedOut` |
| CA-05 | `Dado_StatusInativo_Quando_CriarProduto_Entao_DeveManterInativo` |
| CA-06 | `Dado_StatusOmitido_Quando_CriarProduto_Entao_DeveNascerAtivo` |
| CA-07 | `Dado_LoginExistente_Quando_EsqueceuSenha_Entao_DeveGravarConfirmacaoEmTempData` |
| CA-08 | `Dado_LoginInexistente_Quando_EsqueceuSenha_Entao_DeveGravarMesmaConfirmacaoENaoEnviarEmail` |
| CA-09 | `Dado_LoginMalformado_Quando_EsqueceuSenha_Entao_DeveRetornarViewSemConsultarServico` |
| CA-10 | `Dado_CelularInvalido_Quando_CriarUsuario_Entao_NaoDeveAtribuirNenhumaPropriedade` |
| CA-11 | verificação manual, T032 |
| CA-12 | `dotnet test`, T046 |
| CA-13 | `dotnet build`, T047 |

**Sobre CA-04:** `SignInManager` é selado e o teste é unitário com `Moq`. O teste
prova que `PasswordSignInAsync` é chamado com `lockoutOnFailure: true` e que o
controller trata `SignInResult.LockedOut` — não simula cinco tentativas reais. O
bloqueio de ponta a ponta é verificado à mão em T048.

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Manter `ExecutarEmTransacao` como ponto de extensão | Nenhum caso de uso do backlog precisa dela: mesmo pedido + pagamento + baixa de estoque cabem num `SaveChangesAsync`, já atômico. Abstração sem consumidor é custo de leitura sem contrapartida. Volta como emenda se um caso real aparecer. |
| Corrigir `RealizarLogin` fazendo `FindByEmailAsync` cair para busca por CPF | Trata o sintoma. A causa é resolver o usuário duas vezes por caminhos diferentes; um `ResolverUsuario` único elimina o defeito e duas idas ao banco. |
| Trocar para SQL Server agora | Decisão do autor: SQLite em desenvolvimento, SQL Server na etapa de deploy. A RQ-08 reduz a troca futura a uma linha mais a regeração das migrations. |
| Remover `IProdutoRepository`, hoje interface vazia | A spec `003` (listagem, edição e exclusão) coloca consultas nela. Removê-la agora é churn com volta garantida. |
| Renomear tudo em um commit só | Renomeação de arquivo mais edição de conteúdo no mesmo commit produz diff ilegível. Bloco D é o último e separa renomear de editar. |
| Trocar `HasColumnType("decimal(18,2)")` por `HasPrecision(18, 2)` no `Preco` | Era o desenho inicial. Verificado no snapshot: mudaria a coluna de `decimal(18,2)` para `TEXT`, trocando a afinidade SQLite de `NUMERIC` para `TEXT` e afetando comparação e ordenação de preço. Migration e regressão sutil para resolver um problema inexistente — `decimal(18,2)` já é válido nos dois providers. |
| Subir `FluentValidation.AspNetCore` de 11.3.1 | Era o desenho inicial. Verificado no NuGet: **11.3.1 é a última versão publicada** — o pacote foi descontinuado pelo autor, que passou a recomendar registro manual dos validators. Não há para onde subir. Sair dele é mudança de arquitetura de validação, fora do escopo desta feature; fica registrado no backlog. |
| Adotar AutoMapper ao mexer nos mappers | Proibido pelo Princípio VI. |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Renomear `ProdutoDTO.Id` quebra uma view sem erro de compilação (Razor resolve tarde) | Média | Médio | T035 faz busca textual por `.Id` nos `.cshtml` antes de renomear; T048 abre a vitrine e a tela de cadastro |
| Remover `HasColumnType("INTEGER")` alterar o esquema SQLite | Baixa | Alto | T033 gera migration de verificação, confirma que sai vazia e a descarta |
| Fixar `SQLitePCLRaw.lib.e_sqlite3` conflitar com o que o EF Core 10 exige | Média | Médio | T031 verifica com `dotnet list package --vulnerable` **e** com a suíte de integração, que exercita SQLite de verdade |
| Renomear ~40 testes esconder uma quebra real no meio do diff | Média | Alto | Bloco D é o último, roda sobre suíte verde, e a renomeação não toca corpo de teste. T046 compara a contagem antes e depois |
| `TempData` exigir sessão configurada | Baixa | Baixo | O provedor padrão usa cookie e não precisa de configuração; T048 confirma na tela |
| Destrackear `appsettings.json` quebrar o build de quem clona | Média | Médio | `appsettings.Example.json` versionado e instrução no README; T032 valida clonando em pasta temporária |

## 9. Desvios constitucionais justificados

Nenhum desvio. Duas **emendas**, conforme a Governança item 3 — a constituição
passa de 1.0.0 para **1.1.0** (MINOR, por acréscimo de regra ao Princípio IV).

| Princípio | Emenda | Justificativa |
|---|---|---|
| IV | Acrescentar: "o nome do arquivo coincide com o nome do tipo que ele declara, e a pasta coincide com o namespace". | O princípio já governa nomes de tipo, mas é silencioso sobre o arquivo que os contém. O silêncio produziu `TransactionEf.cs`/`TransactionEF`, `EsqueceuSenhaValidator.cs`/`EsqueceuSenhaDTOValidator` e `DbContextDependencyInjection.cs`/`DatabaseConfig`. A regra é a que o autor já seguia na maioria dos arquivos — está sendo escrita, não inventada. |
| VI | Trocar "A gravação acontece via `IUnitOfWork`, chamado pela camada de aplicação, que é quem conhece o limite da transação" por texto que descreve apenas a gravação, sem transação explícita. | A frase permanece verdadeira no essencial: a aplicação continua sendo quem decide quando gravar. O que sai é a transação manual, que nenhum caso de uso chama e que o `SaveChangesAsync` já fornece implicitamente. É PATCH (correção de texto para refletir o código), absorvido na 1.1.0. |
