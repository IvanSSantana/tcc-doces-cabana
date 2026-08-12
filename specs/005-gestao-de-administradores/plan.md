# Plano Técnico — Gestão de administradores

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-12
**Status:** Rascunho

---

## 1. Resumo da abordagem

Um controller novo, `AdministradorController`, com duas telas: a lista e o
formulário de cadastro. Ambas protegidas pelo mesmo `[Authorize(Roles = ...)]`
que a `001` já aplicou ao `AdminController`.

Atrás delas, um `IAdministradorService` com dois métodos. O cadastro **não**
reimplementa a criação de usuário: ele reaproveita `IUsuarioService.CadastrarUsuario`,
que desde a `004` já cria as duas metades e compensa se alguma falhar. A única
diferença é que a conta nasce com o papel — e para que a falha na concessão do
papel também seja compensada, a atribuição acontece **dentro** da mesma operação,
via um parâmetro opcional novo em `CadastrarUsuario`.

Há um obstáculo pequeno e concreto no caminho: a constante `PapelAdministrador`
mora hoje em `DbInitializer`, que é da camada MVC. Um serviço da infraestrutura
não pode enxergá-la — a dependência apontaria para fora. A constante muda de casa
antes de qualquer outra coisa.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | `IAdministradorService` fica em `Infrastructure/Identity/Services`, ao lado de `IUsuarioService`, pelo mesmo motivo já documentado na constituição: depende de `UserManager` e `RoleManager`. A constante de papel sobe para o `Domain`, que é o único projeto que todos enxergam |
| II | Domínio rico e auto-validante | ⬜ n/a | Nenhuma entidade nova. Administrador não é um tipo — é um usuário com papel (RN-01) |
| III | Validação nas duas barreiras | ✅ OK | Reaproveita `CadastroDTO` e `CadastroDTOValidator`, que já existem e já cobrem RN-02. Nenhum validator novo — usar outro DTO abriria caminho para as regras divergirem |
| IV | Nomenclatura em português | ✅ OK | |
| V | Testes escritos antes | ✅ OK | Fase 2 e Fase 4 das tarefas |
| VI | Repositório + commit via `UnitOfWork` | ✅ OK | A gravação acontece dentro de `CadastrarUsuario`, que já usa `IUnitOfWork` desde a `004`. Nenhuma migration: papéis e `AspNetUserRoles` existem desde a segunda migration |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `[Authorize(Roles = ...)]` na classe, `[ValidateAntiForgeryToken]` no POST, guarda de `ModelState`, redirecionamento no sucesso |
| VIII | Tratamento de erro por camada | ✅ OK | Sem `try/catch` em ação; a compensação da RN-05 fica no serviço |

Nenhuma emenda constitucional necessária.

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Papeis.cs` | **criar** | `public const string Administrador = "Administrador";`. É vocabulário de negócio e precisa ser visível para MVC e Infrastructure ao mesmo tempo — o `Domain` é o único lugar que satisfaz as duas |

### `DocesCabana.Application`

Nenhuma alteração. `CadastroDTO`, `CadastroDTOValidator` e `UsuarioDTO` servem
como estão.

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Identity/Services/IAdministradorService.cs` | **criar** | `ListarAdministradores` e `CadastrarAdministrador` |
| `Identity/Services/AdministradorService.cs` | **criar** | Ver seção 4 |
| `Identity/Services/IUsuarioService.cs` | alterar | `CadastrarUsuario(CadastroDTO dto, string? papel = null)` — parâmetro opcional, compatível com quem já chama |
| `Identity/Services/UsuarioService.cs` | alterar | Quando `papel` vier preenchido, `AddToRoleAsync` **dentro** do bloco que já compensa (RN-05) |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar `IAdministradorService` |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AdministradorController.cs` | **criar** | `[Authorize(Roles = Papeis.Administrador)]`; `Index` (lista), `Cadastro` GET e POST |
| `Views/Administrador/Index.cshtml` | **criar** | Tabela de nome e e-mail, mensagem de confirmação do `TempData`, link para o cadastro |
| `Views/Administrador/Cadastro.cshtml` | **criar** | Formulário espelhando `Views/Autenticacao/Cadastro.cshtml`, sem o que é específico de auto-cadastro |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Link para a gestão, dentro de um `@if (User.IsInRole(Papeis.Administrador))` — RF-09 |
| `Controllers/AdminController.cs` | alterar | Trocar `DbInitializer.PapelAdministrador` por `Papeis.Administrador` |
| `Helpers/DbInitializer.cs` | alterar | Idem; a constante sai daqui |
| `wwwroot/css/pages/administradores.css` | **criar** | Estilo da tabela, seguindo o padrão das páginas existentes |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Services/AdministradorServiceTests.cs` | **criar** | RF-01, RF-03, RN-04, RN-05 |
| `Units/Controllers/AdministradorControllerTests.cs` | **criar** | RF-06, RF-07 — `ModelState` inválido não chama o serviço; válido redireciona com `TempData` |
| `Units/Services/UsuarioServiceCadastroTests.cs` | alterar | Acrescentar: com `papel` informado, o papel é atribuído; se a atribuição falhar, a conta é desfeita |

## 4. Contratos

```csharp
// Domain/Papeis.cs — novo
public static class Papeis
{
    public const string Administrador = "Administrador";
}

// Infrastructure/Identity/Services/IAdministradorService.cs — novo
public interface IAdministradorService
{
    Task<List<UsuarioDTO>> ListarAdministradores();
    Task<UsuarioDTO> CadastrarAdministrador(CadastroDTO dto);
}

// Infrastructure/Identity/Services/IUsuarioService.cs — alterado
Task<UsuarioDTO> CadastrarUsuario(CadastroDTO usuario, string? papel = null);
```

### Por que o papel entra em `CadastrarUsuario`, e não depois

O caminho óbvio seria `AdministradorService` chamar `CadastrarUsuario` e, em
seguida, `AddToRoleAsync`. O problema é o que acontece se essa segunda chamada
falhar: sobra uma conta criada **sem** acesso administrativo — e como esta spec
não entrega promoção de conta existente (§8 da spec), não haveria como
consertar pela interface. A pessoa teria uma conta de cliente que ninguém pediu.

Passando o papel para dentro de `CadastrarUsuario`, a atribuição acontece no
mesmo bloco que a `004` já protege, e a compensação existente cobre os três
passos de uma vez: conta, usuário e papel. É a RN-05.

```csharp
// UsuarioService.CadastrarUsuario, esboço da parte nova
try
{
    var usuario = new Usuario(conta.Id, /* ... */);
    await _usuarioRepository.Adicionar(usuario);
    await _unitOfWork.SalvarAlteracoes();

    if (!string.IsNullOrWhiteSpace(papel))
    {
        var resultadoPapel = await _userManager.AddToRoleAsync(conta, papel);
        if (!resultadoPapel.Succeeded)
            throw new InvalidOperationException(ObterMensagensErro(resultadoPapel));
    }

    return UsuarioMapper.ToDTO(usuario, conta);
}
catch
{
    await _userManager.DeleteAsync(conta);
    throw;
}
```

### Como a lista é montada

`UserManager.GetUsersInRoleAsync` devolve as **contas**, que depois da `004`
não têm mais nome. O nome vem do `Usuario` do domínio, pelo
`IUsuarioRepository`. O serviço compõe os dois, como o `UsuarioMapper` já faz
para uma conta só.

## 5. Modelo de dados

Nenhuma mudança de esquema e nenhuma migration. As tabelas `AspNetRoles` e
`AspNetUserRoles` existem desde a migration `AddIdentityAndFluentValidation`, e
o papel `Administrador` é criado pela massa inicial desde a spec `001`.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — serviço | `Units/Services/AdministradorServiceTests.cs` | RF-01, RN-04 — a lista traz nome e e-mail compostos das duas metades; RF-03 — o cadastro repassa o papel |
| Unidade — serviço | `Units/Services/UsuarioServiceCadastroTests.cs` | RN-05 — falha ao atribuir papel desfaz a conta |
| Unidade — controller | `Units/Controllers/AdministradorControllerTests.cs` | RF-06, RF-07 |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_DoisAdministradores_Quando_ListarAdministradores_Entao_DeveRetornarNomeEEmailDeCada` |
| CA-02 | `Dado_DadosValidos_Quando_CadastroPost_Entao_DeveRedirecionarComConfirmacao` |
| CA-03 | verificação manual — exige entrar com a conta nova |
| CA-04 | `Dado_EmailJaUsado_Quando_CadastrarAdministrador_Entao_DeveLancarInvalidOperationException` |
| CA-05 | `Dado_CpfJaUsado_Quando_CadastrarAdministrador_Entao_DeveDesfazerAConta` |
| CA-06 | Já coberto por `CadastroDTOValidatorTests` desde a `002`; confirmar, não reescrever |
| CA-07, CA-08, CA-09 | verificação manual — autorização por atributo e `User.IsInRole` na view não são unit-testáveis sem host de integração |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Acrescentar as ações ao `AdminController` | Aquele controller é sobre produto. Gestão de gente e gestão de catálogo na mesma classe é o começo do controller que faz tudo |
| DTO próprio para cadastro de administrador | `CadastroDTO` tem exatamente os campos necessários e já tem validator. Um DTO paralelo faria as regras dos dois cadastros divergirem no primeiro ajuste que alguém fizesse em um só |
| `AddToRoleAsync` depois de `CadastrarUsuario`, no `AdministradorService` | Falha ali deixaria uma conta de cliente que ninguém pediu, sem meio de promover pela interface — promoção está fora do escopo. Ver seção 4 |
| Entidade `Administrador` no domínio | Administrador não é um tipo de coisa, é um usuário com papel (RN-01). Uma entidade separada duplicaria nome, CPF e celular |
| Deixar `PapelAdministrador` no `DbInitializer` | `DbInitializer` é da camada MVC; um serviço da infraestrutura não pode enxergá-lo sem inverter a direção de dependência |
| Incluir revogação nesta entrega | Exige definir quem revoga quem e impedir que a loja fique sem administrador — regra que não existe. Registrado na §8 da spec |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Mover `PapelAdministrador` quebrar a autorização silenciosamente | Média | **Alto** | `[Authorize(Roles = ...)]` compara **string**: se a constante mudar de valor, o atributo passa a exigir um papel que ninguém tem e a área administrativa fica inacessível para todos. O valor literal `"Administrador"` **não muda** — só a casa da constante. Fumaça manual confirma antes do fechamento |
| A lista ficar cara com muitos administradores | Baixa | Baixo | São unidades, não milhares. Uma consulta por papel mais uma por identificadores basta; paginação seria antecipação |
| Alterar a assinatura de `CadastrarUsuario` quebrar chamadas existentes | Baixa | Médio | O parâmetro é opcional e vai por último — quem chama hoje continua compilando. Os testes da `004` cobrem o caminho sem papel |
| O administrador semeado não aparecer na lista | Baixa | Médio | Ele recebe o papel no seed desde a `001`; a fumaça manual da T021 confere que ele consta |

## 9. Desvios constitucionais justificados

Nenhum. `IAdministradorService` na infraestrutura cai na exceção que o Princípio I
já documenta e que a `004` reescreveu: depende de `UserManager` e `RoleManager`.
