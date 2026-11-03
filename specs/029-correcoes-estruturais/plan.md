# Plano — Correções estruturais

**ID:** 029-correcoes-estruturais · **Criado em:** 2026-11-03
**Status:** Rascunho

---

## 1. Objetivo

A revisão estrutural de 2026-11-03 apontou pastas duplicadas, tipos em camadas
erradas e dependências de framework vazando para dentro da `Application`. Este
plano corrige os achados 1, 2, 3, 4, 6, 8, 9 e 10 da revisão. O resultado é uma
base em que cada conceito tem uma pasta só, o MVC deixa de importar tipos da
`Infrastructure` nos controllers, e a `Application` deixa de depender do ASP.NET
Core. Nenhum comportamento visível muda.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | `Application/Servicos/Apelido.cs` vira `Application/Helpers/ApelidoHelper.cs` (namespace `DocesCabana.Application.Helpers`). A pasta `Servicos/` é apagada. | `Apelido` é função pura de texto, não serviço. O sufixo `Helper` segue `TextoHelper`, `CepHelper`, `CpfHelper`. O nome novo também desfaz a ambiguidade de `Apelido = Apelido.De(...)` nos mappers, onde propriedade e classe têm o mesmo nome. | Mover para `Domain/Helpers`: slug de URL é preocupação de apresentação/aplicação, não regra de domínio. |
| D2 | `Tests/Units/Servicos/ApelidoTests.cs` vira `Tests/Units/Helpers/ApelidoHelperTests.cs`. | O teste acompanha a classe testada, e `Units/Helpers` já existe. | — |
| D3 | `IUsuarioService` e `IAdministradorService` vão para `Application/Contracts/Services`. As implementações ficam em `Infrastructure/Identity/Services`. | Todos os outros contratos de serviço já moram lá. Controllers deixam de importar `DocesCabana.Infrastructure.Identity.Services`. | Manter a exceção do Princípio I: o único motivo dela era o `SignInResult` do Identity na assinatura, que D4 remove. |
| D4 | `RealizarLogin` passa a retornar o enum novo `Application/Enums/ResultadoDoLogin` (`Sucesso`, `Bloqueado`, `Falhou`). `UsuarioService` traduz o `SignInResult`. | O controller só usa `Succeeded` e `IsLockedOut`. Três valores cobrem o comportamento atual sem perda. Com isso, a `Application` não precisa conhecer o Identity. | DTO com booleanos (`Sucesso`, `Bloqueado`): permite estado inválido (os dois `true`). |
| D5 | Emenda constitucional 1.6.0: remover a "exceção conhecida" do Princípio I. | D3 + D4 eliminam a exceção. Deixá-la escrita convida o próximo desvio. | — |
| D6 | `DadosPessoaisDTO.CPF` vira `string?` e perde `[ValidateNever]`. | O CPF não é postado (é exibido como texto). Sem o atributo, o MVC trata `string` não anulável como `[Required]` implícito e invalida o `ModelState`. `string?` desliga isso sem framework na `Application`. | Mover o atributo para um ViewModel no MVC: cria um tipo novo só para um campo somente-leitura. |
| D7 | `Application.csproj` troca `FluentValidation.AspNetCore` 11.3.1 por `FluentValidation` + `FluentValidation.DependencyInjectionExtensions` 11.11.0. `AddFluentValidationAutoValidation` e `AddFluentValidationClientsideAdapters` saem de `FluentValidationDependencyInjection` e vão para o `Program.cs`. O MVC passa a referenciar `FluentValidation.AspNetCore` 11.3.1. | O auto-validation é integração com o pipeline MVC, então mora no MVC. Na `Application` fica só o scan de validators. Mesma versão que `DocesCabana.Tests` já usa. | Manter tudo na `Application`: mantém a dependência de ASP.NET Core que o Princípio I proíbe. |
| D8 | `MVC/Helpers/DbInitializer.cs` vai para `Infrastructure/DatabaseContext/Seed/DbInitializer.cs` (namespace `DocesCabana.Infrastructure.DatabaseContext.Seed`). `InternalsVisibleTo DocesCabana.Tests` sai do `MVC.csproj` e entra no `Infrastructure.csproj`. | O seed usa `DbContext`, `UserManager` e `RoleManager`: é acesso a dados. O Princípio VI proíbe `DbContext` fora da `Infrastructure`. | Expor os membros `internal` como `public`: eles só existem para teste. |
| D9 | Testes do seed vão de `Units/Helpers/` para `Units/Seed/` (`DbInitializerProdutosTests`, `GeradorDeAvaliacoesTests`). | `Units/Helpers` fica só com helpers de verdade. | — |
| D10 | `MensagemDePostgresIndisponivel`, `PostgresDeTeste` e `InfraestruturaPostgresDescartavel` vão para `Tests/Integration/Infraestrutura/` (namespace `DocesCabana.Tests.Integration.Infraestrutura`). `Units/Infraestrutura/MensagemDePostgresIndisponivelTests.cs` vai junto. | O teste prova a própria infraestrutura de teste de integração: fica ao lado dela. Some a pasta de um arquivo só em `Units`. | Deixar o teste em `Units/`: separa o teste do código que ele cobre. |
| D11 | `EmailSettings`, `FreteSettings` e `SupabaseSettings` vão para `Infrastructure/Settings/` (namespace `DocesCabana.Infrastructure.Settings`). | `Services/` fica só com adaptadores. Os nomes das seções de configuração (`"EmailSettings"` etc.) não mudam, então `appsettings` e variáveis de ambiente do E2E seguem iguais. | — |
| D12 | `Domain/Papeis.cs` vai para `Domain/Constants/Papeis.cs` (`DocesCabana.Domain.Constants`). `Application/Mensagens/MensagensCadastro.cs` vai para `Application/Constants/MensagensCadastro.cs` (`DocesCabana.Application.Constants`). A pasta `Mensagens/` é apagada. | Uma pasta `Constants` por camada, mesmo padrão das pastas técnicas em inglês (`Helpers`, `Enums`, `Mappings`). | Juntar as duas na mesma camada: `Papeis` é usado pela `Infrastructure` e pelo MVC, e `MensagensCadastro` é texto de aplicação. |

**Fora de escopo:** achado 5 (`FilterException`) e achado 7 (separar
`Units/Services` por camada). Nenhuma mudança de comportamento, migration ou
tela.

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | OK | Melhora: `Application` perde ASP.NET Core (D7) e MVC perde imports de `Infrastructure.Identity.Services` e `Infrastructure.DatabaseContext` nos controllers/helpers. Emenda remove a exceção (D5). |
| II | Domínio rico e auto-validante | n/a | Nenhuma entidade muda. |
| III | Validação nas duas barreiras | OK | Validators continuam registrados pelo scan; só muda onde o auto-validation é ligado. |
| IV | Nomenclatura em português | OK | Pasta = namespace = tipo em todos os movimentos. `ResultadoDoLogin` em português. |
| V | Testes escritos antes | OK | D4 é a única mudança de contrato: testes de `RealizarLogin` são reescritos para o enum antes da implementação. O resto é estrutural: a suíte existente é a prova. |
| VI | Repositório + commit via UnitOfWork | OK | Melhora: `DbContext` sai do MVC (D8). |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | n/a | Nenhuma ação muda. |
| VIII | Tratamento de erro por camada | n/a | — |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Papeis.cs` → `Constants/Papeis.cs` | mover | namespace `DocesCabana.Domain.Constants` (D12) |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `Servicos/Apelido.cs` → `Helpers/ApelidoHelper.cs` | mover + renomear | classe `ApelidoHelper` (D1) |
| `Mappings/CategoriaMapper.cs`, `Mappings/SubcategoriaMapper.cs` | alterar | `using` e chamada `ApelidoHelper.De` |
| `Mensagens/MensagensCadastro.cs` → `Constants/MensagensCadastro.cs` | mover | namespace `DocesCabana.Application.Constants` (D12) |
| `Contracts/Services/IUsuarioService.cs` | criar (mover) | vindo da `Infrastructure`; `RealizarLogin` retorna `ResultadoDoLogin` (D3, D4) |
| `Contracts/Services/IAdministradorService.cs` | criar (mover) | vindo da `Infrastructure` (D3) |
| `Enums/ResultadoDoLogin.cs` | criar | enum (D4) |
| `DTOs/DadosPessoaisDTO.cs` | alterar | `CPF` vira `string?`; remover `[ValidateNever]` e o `using` (D6) |
| `DependencyInjections/FluentValidationDependencyInjection.cs` | alterar | fica só `AddValidatorsFromAssemblyContaining` (D7) |
| `DocesCabana.Application.csproj` | alterar | pacotes do D7 |

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `Identity/Services/IUsuarioService.cs`, `IAdministradorService.cs` | apagar | movidos (D3) |
| `Identity/Services/UsuarioService.cs` | alterar | `using` do contrato; traduz `SignInResult` → `ResultadoDoLogin`; `using` de `MensagensCadastro` |
| `Identity/Services/AdministradorService.cs` | alterar | `using` do contrato e de `Papeis` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | `using` dos contratos e de `Settings` |
| `DatabaseContext/Seed/DbInitializer.cs` | criar (mover) | vindo do MVC, namespace novo (D8) |
| `Services/EmailSettings.cs`, `FreteSettings.cs`, `SupabaseSettings.cs` → `Settings/` | mover | namespace `DocesCabana.Infrastructure.Settings` (D11) |
| `Services/EmailService.cs`, `EmailServiceArquivo.cs`, `FreteServiceMelhorEnvio.cs`, `ArmazenamentoSupabase.cs` | alterar | `using DocesCabana.Infrastructure.Settings` |
| `DocesCabana.Infrastructure.csproj` | alterar | `InternalsVisibleTo DocesCabana.Tests` (D8) |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/DbInitializer.cs` | apagar | movido (D8) |
| `Program.cs` | alterar | `using` do seed; `AddFluentValidationAutoValidation()` e `AddFluentValidationClientsideAdapters()` (D7) |
| `DocesCabana.MVC.csproj` | alterar | remove `InternalsVisibleTo`; adiciona `FluentValidation.AspNetCore` 11.3.1 |
| `Controllers/AutenticacaoController.cs` | alterar | `using` dos contratos e de `Constants`; `resultado == ResultadoDoLogin.Sucesso` / `Bloqueado`; remove `using Microsoft.AspNetCore.Identity` se ficar sem uso |
| `Controllers/ContaController.cs` | alterar | `using` do contrato |
| `Areas/Admin/Controllers/AdministradorController.cs` | alterar | `using` dos contratos, `Constants` e `Papeis` |
| `Areas/Admin/Controllers/ProdutoController.cs` | alterar | `using DocesCabana.Domain.Constants` |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | `@using DocesCabana.Domain.Constants` |

### Testes (`DocesCabana.Tests`)
| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Servicos/ApelidoTests.cs` → `Units/Helpers/ApelidoHelperTests.cs` | mover + renomear | D2 |
| `Units/Helpers/DbInitializerProdutosTests.cs`, `GeradorDeAvaliacoesTests.cs` → `Units/Seed/` | mover | namespace `DocesCabana.Tests.Units.Seed` (D9) |
| `Units/Infraestrutura/MensagemDePostgresIndisponivelTests.cs` → `Integration/Infraestrutura/` | mover | D10 |
| `Integration/MensagemDePostgresIndisponivel.cs`, `PostgresDeTeste.cs`, `InfraestruturaPostgresDescartavel.cs` → `Integration/Infraestrutura/` | mover | D10 |
| `Integration/**/*.cs` que usam `InfraestruturaPostgresDescartavel` | alterar | `using DocesCabana.Tests.Integration.Infraestrutura` |
| `Integration/Repositories/CatalogoRepositoryIntegrationTests.cs`, `DbInitializerPedidosIntegrationTests.cs` | alterar | `using` do seed |
| `Units/Controllers/AutenticacaoControllerTests.cs` | alterar | mocks retornam `ResultadoDoLogin` |
| `Units/Services/UsuarioServiceLoginTests.cs` | alterar | asserts no enum; teste novo para conta bloqueada |
| `Units/Services/UsuarioService*Tests.cs`, `AdministradorServiceTests.cs`, `Units/Controllers/ContaControllerTests.cs`, `Units/Controllers/Admin/AdministradorControllerTests.cs` | alterar | `using` dos contratos/`Constants` |
| `Units/DependencyInjections/RegistroDeEmailTests.cs`, `Units/Services/{Frete,Armazenamento,EmailServiceArquivo}*Tests.cs` | alterar | `using DocesCabana.Infrastructure.Settings` |

### Processo
| Arquivo | Ação | O quê |
|---|---|---|
| `.specify/memory/constitution.md` | alterar | 1.6.0: remove a exceção do Princípio I (D5); Princípio V aponta `Integration/Infraestrutura/InfraestruturaPostgresDescartavel.cs` (D10); Princípio VIII sem mudança |

## 5. Contratos e modelo de dados

```csharp
namespace DocesCabana.Application.Enums;
public enum ResultadoDoLogin { Sucesso, Bloqueado, Falhou }

namespace DocesCabana.Application.Contracts.Services;
public interface IUsuarioService
{
    // demais assinaturas inalteradas
    Task<ResultadoDoLogin> RealizarLogin(string login, string senha, bool lembrarMe);
}
public interface IAdministradorService { /* inalterada */ }
```

Sem migration.

## 6. Ordem de execução

Cada passo termina com `dotnet build` e `dotnet test DocesCabana.Tests` com o
mesmo resultado da linha de base (passo 1).

- [ ] 1. Linha de base: com o Docker Desktop ligado, rodar `dotnet test` e anotar o resultado.
- [ ] 2. D12: mover `Papeis` e `MensagensCadastro`; ajustar `using` (código, view, testes).
- [ ] 3. D1 + D2: `ApelidoHelper` e `ApelidoHelperTests`; apagar `Servicos/` nos dois projetos.
- [ ] 4. D11: mover os três `*Settings`; ajustar `using`.
- [ ] 5. D4 (teste antes): reescrever `UsuarioServiceLoginTests` e `AutenticacaoControllerTests` para `ResultadoDoLogin`, incluindo o caso `Bloqueado`; ver falhar na compilação.
- [ ] 6. D3 + D4: criar o enum, mover as interfaces para `Application/Contracts/Services`, traduzir o resultado em `UsuarioService`, ajustar controllers e DI; ver os testes do passo 5 passarem.
- [ ] 7. D6 + D7: `DadosPessoaisDTO`, pacotes e `FluentValidationDependencyInjection`/`Program.cs`. Conferir que `ContaControllerTests` segue verde.
- [ ] 8. D8 + D9: mover `DbInitializer`, `InternalsVisibleTo` e os testes do seed.
- [ ] 9. D10: mover a infraestrutura de teste de integração e o teste dela.
- [ ] 10. D5: emenda 1.6.0 na constituição.
- [ ] 11. `dotnet build` (0 erros) e `dotnet test` completos, incluindo E2E.
- [ ] 12. Conferir com `grep` que não sobrou `Application.Servicos`, `Infrastructure.Identity.Services` no MVC, `Microsoft.AspNetCore` na `Application` nem `DocesCabana.Infrastructure.DatabaseContext` no MVC.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Nenhum comportamento mudou | `dotnet test` (unidade, integração e E2E) igual à linha de base |
| Login bem-sucedido, bloqueado e falho seguem com o mesmo fluxo | `AutenticacaoControllerTests` com `ResultadoDoLogin`; `Dado_ContaBloqueada_Quando_RealizarLogin_Entao_DeveRetornarBloqueado` em `UsuarioServiceLoginTests` |
| Edição de dados pessoais não fica inválida sem o CPF postado | `ContaControllerTests` existentes + E2E de conta |
| Validação automática do FluentValidation continua ligada | Testes de controller e E2E de cadastro/login com campo inválido |
| `Application` sem ASP.NET Core | `grep -r "Microsoft.AspNetCore" DocesCabana.Application` vazio |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Linha de base indisponível sem Docker: 60 testes de integração e os 186 E2E falham hoje por falta do contêiner | Alto: sem ela, não dá para provar que nada mudou | Passo 1 exige o Docker Desktop ligado. Sem ele, a execução para no passo 1. |
| Auto-validation do FluentValidation deixar de rodar ao mudar de projeto | Alto: formulários aceitariam dado inválido em silêncio | Passo 7 confere testes de controller e E2E de formulário. |
| `CPF` anulável quebrar quem lê `dto.CPF` como não nulo | Baixo | O build acusa com aviso de nulabilidade; só `ContaController` e a view usam. |
| `InternalsVisibleTo` na `Infrastructure` expõe também as classes internas do Melhor Envio aos testes | Baixo | Só o projeto de testes enxerga; nenhum uso em produção. |
| Nenhum desvio constitucional | — | — |

## 9. Resultado da execução

*Preenchido na execução.*
