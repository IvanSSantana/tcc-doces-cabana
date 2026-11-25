# Plano — Desativar admin, alterar CPF e bloquear endereço duplicado

**ID:** 039-desativar-admin-cpf-e-endereco · **Criado em:** 2026-11-25
**Status:** Executado

---

## 1. Objetivo

Três ajustes de manutenção de conta. (a) O admin principal — o admin semeado,
`admin@docescabana.com.br` — passa a desativar e reativar outros admins na tela
`Admin/Administrador`, para tirar de uso contas criadas por engano ou que ninguém
usa mais. (b) Qualquer usuário altera o próprio CPF em "Minha conta", com
validação de formato e de unicidade. (c) O cadastro e a edição de endereço
recusam um endereço com o mesmo CEP, número e complemento de outro endereço da
mesma conta.
O motivo é manutenção, não segurança.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | Admin principal = a conta com o e-mail `admin@docescabana.com.br`. A constante passa de `DbInitializer.EmailAdministrador` para `Domain/Constants/AdministradorPrincipal.Email` | Debate: opção A. A conta já existe no seed e não precisa de migration. A constante precisa ser vista pela `Infrastructure` (seed e serviço). O `Domain` é o mesmo lugar onde `Papeis` vive (spec 005) | Coluna "principal" transferível (precisa de migration e de tela, é demais para manutenção). Qualquer admin desativar qualquer outro (um admin apagaria todos) |
| D2 | "Excluir" vira **desativar/reativar**. Não há exclusão física nem edição de dados de outro admin | Debate: o usuário pediu explicitamente "Esqueça a opção de editar. Desativar/Reativar é suficiente". `Pedido`, `Endereco` e `Avaliacao` apontam para `Usuario` com `Restrict`, e a exclusão física quebraria o histórico | Exclusão física com recusa quando houver histórico. Rebaixar a cliente (a pessoa continuaria entrando) |
| D3 | A desativação usa o lockout do Identity: `SetLockoutEnabledAsync(true)` + `SetLockoutEndDateAsync(AdministradorPrincipal.FimDaDesativacao)` + `UpdateSecurityStampAsync`. A reativação usa `SetLockoutEndDateAsync(null)` + `ResetAccessFailedCountAsync` | Debate: sem migration. O `PasswordSignInAsync` já recusa a conta com lockout ativo | Coluna `Ativa` em `ContaDeAcesso` (exigiria migration e mudança no login) |
| D4 | `FimDaDesativacao = new DateTimeOffset(9999, 12, 1, 0, 0, 0, TimeSpan.Zero)`. A conta está **desativada** quando `LockoutEnd >= FimDaDesativacao` | Separa a desativação do bloqueio temporário de 15 minutos por senha errada. Não usa `DateTimeOffset.MaxValue` porque o Postgres guarda microssegundos: o valor volta truncado e a comparação de igualdade falharia | `LockoutEnd == DateTimeOffset.MaxValue` |
| D5 | `SecurityStampValidatorOptions.ValidationInterval = 1 minuto` | Um admin desativado com sessão aberta (ou "lembrar-me") perde a sessão em até 1 minuto, em vez dos 30 minutos padrão. Custo: uma consulta por usuário logado a cada minuto | Manter os 30 min padrão (o admin desativado continuaria usando o painel por até meia hora) |
| D6 | Nova saída de login `ResultadoDoLogin.Desativado`, com a mensagem "Conta desativada. Procure o administrador da loja." | A mensagem atual, "Conta bloqueada. Tente novamente mais tarde.", engana quem foi desativado: tentar mais tarde não resolve | Reaproveitar `Bloqueado` |
| D7 | Autorização em duas barreiras. O controller devolve `Forbid()` quando quem executa não é principal. O serviço lança `InvalidOperationException` se quem executa não for principal ou se o alvo for o próprio principal | Princípio III aplicado à autorização. O botão escondido na view não é proteção | Só esconder o botão |
| D8 | A tela continua listando os admins desativados, com a coluna "Situação" | Sem a linha na lista, não haveria onde clicar para reativar | Filtrar os desativados |
| D9 | O CPF é alterado em `AlterarDadosUsuario`, junto com nome, celular e nascimento, por um novo método de domínio `Usuario.AlterarCpf` | Mesmo formulário e mesmo POST. `ContaController.AlterarDados` é o único chamador. `ValidarCPF` já existe na entidade | Tela e POST separados só para o CPF |
| D10 | CPF já usado por outra conta: o controller checa `CpfEmUsoPorOutraConta` antes e mostra `MensagensCadastro.DadosJaAssociados` no campo. O serviço repete a checagem e lança `InvalidOperationException` | Princípio VIII: erro esperado vira `ModelState`. É a mesma mensagem genérica do cadastro, que não confirma de quem é o CPF. O índice único existente em `Usuario.CPF` cobre a corrida entre duas requisições | Deixar o `DbUpdateException` do índice estourar |
| D11 | O endereço duplicado é checado pelo `EnderecoService`, comparando `CepHelper.ApenasDigitos(CEP)`, `Numero` e `Complemento` (D12) com os endereços da conta e ignorando o próprio `EnderecoId` na edição. O controller pergunta antes (`EnderecoJaCadastrado`) e mostra o erro. `Cadastrar` e `Editar` repetem a checagem e lançam `InvalidOperationException` | O validator FluentValidation não acessa o banco. A regra envolve vários endereços, então não cabe na entidade `Endereco`. O padrão "controller pergunta, serviço garante" é o mesmo de `ContaJaExiste` | Índice único `(UsuarioId, CEP, Numero)`: a migration falharia se o banco já tiver duplicados, e o índice impediria corrigir isso depois sem script |
| D12 | O complemento **entra** na comparação, normalizado: `Trim`, sem diferenciar maiúsculas de minúsculas, e nulo igual a vazio. "Apto 101" e "apto 101 " são o mesmo endereço; "Apto 101" e "Apto 102" não | Debate: em prédio, dois apartamentos têm o mesmo CEP e número. Sem a normalização, um espaço ou uma letra maiúscula furaria a regra | Só CEP e número (recusaria apartamentos diferentes do mesmo prédio). Comparação literal do complemento |

**Fora de escopo:** editar os dados de outro admin, excluir conta fisicamente,
transferir o posto de principal, restringir quem cadastra admin (qualquer admin
continua cadastrando), N+1 de `ListarAdministradores`, desativar clientes.

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | A constante vai para `Domain`. `UserManager` continua só em `Infrastructure/Identity/Services`. O MVC só vê `IAdministradorService` e os DTOs |
| II | Domínio rico e auto-validante | ✅ OK | `Usuario.AlterarCpf` valida antes de atribuir. A desativação é estado de credencial do Identity, não do domínio |
| III | Validação nas duas barreiras | ✅ OK | CPF: `DadosPessoaisDTOValidator` + `Usuario.AlterarCpf`. Duplicidade de CPF e de endereço: controller (mensagem) + serviço (exceção) + índice único (só para o CPF) |
| IV | Nomenclatura em português | ✅ OK | `Desativar`, `Reativar`, `EhPrincipal`, `AlterarCpf`, `EnderecoJaCadastrado` |
| V | Testes escritos antes | ✅ OK | Ordem de execução abre cada passo pelo teste |
| VI | Repositório + commit via UnitOfWork | ✅ OK | CPF e endereço gravam por `IUnitOfWork.SalvarAlteracoes`. O lockout é gravado pelo `UserManager`, como já é hoje no login |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `Desativar` e `Reativar` são `[HttpPost][ValidateAntiForgeryToken]`, aguardam o serviço e redirecionam. A área segue com `[Authorize(Roles = Papeis.Administrador)]`, mais a checagem do principal |
| VIII | Tratamento de erro por camada | ✅ OK | Sem `try/catch` em ação. O alvo inexistente gera `KeyNotFoundException` |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Constants/AdministradorPrincipal.cs` | criar | `Email = "admin@docescabana.com.br"`, `FimDaDesativacao` (D4) |
| `Entities/Usuario.cs` | alterar | `public void AlterarCpf(string cpf)`: chama `ValidarCPF` e atribui `CpfHelper.ApenasDigitos(cpf)` |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `DTOs/AdministradorDTO.cs` | criar | `Id`, `Nome`, `Email`, `Ativo`, `Principal` |
| `Contracts/Services/IAdministradorService.cs` | alterar | `ListarAdministradores` devolve `List<AdministradorDTO>`. Novos `EhPrincipal`, `Desativar`, `Reativar` |
| `Contracts/Services/IUsuarioService.cs` | alterar | Novo `CpfEmUsoPorOutraConta` |
| `Contracts/Services/IEnderecoService.cs` | alterar | Novo `EnderecoJaCadastrado` |
| `Enums/ResultadoDoLogin.cs` | alterar | Novo valor `Desativado` (D6) |
| `Constants/MensagensEndereco.cs` | criar | `EnderecoRepetido = "Você já tem um endereço com este CEP, número e complemento."` |
| `Validators/DadosPessoaisDTOValidator.cs` | alterar | Regra para `CPF`: `NotEmpty` ("CPF é obrigatório!") e `Must(CpfHelper.CpfValido)` ("CPF inválido.") |
| `Services/EnderecoService.cs` | alterar | `EnderecoJaCadastrado` + guarda em `Cadastrar` e `Editar` (D11) |

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `Identity/Services/AdministradorService.cs` | alterar | Monta `AdministradorDTO` com `Ativo` (D4) e `Principal`. Implementa `EhPrincipal`, `Desativar` e `Reativar` com as guardas de D7 |
| `Identity/Services/UsuarioService.cs` | alterar | `AlterarDadosUsuario` chama `AlterarCpf` depois de checar a colisão (D10). `CpfEmUsoPorOutraConta`. `RealizarLogin` devolve `Desativado` quando `IsLockedOut` e `LockoutEnd >= FimDaDesativacao` |
| `DependencyInjections/IdentityDependencyInjection.cs` | alterar | `services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(1))` (D5) |
| `DatabaseContext/Seed/DbInitializer.cs` | alterar | `EmailAdministrador` passa a valer `AdministradorPrincipal.Email` |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Areas/Admin/Controllers/AdministradorController.cs` | alterar | `Index` põe `ViewData["EhPrincipal"]`. Novos `POST Desativar(Guid id)` e `POST Reativar(Guid id)`: `Forbid()` se quem executa não é principal; senão, chamam o serviço, gravam `TempData["Confirmacao"]` e redirecionam para `Index` |
| `Areas/Admin/Views/Administrador/Index.cshtml` | alterar | `@model List<AdministradorDTO>`. Coluna "Situação" (Ativo/Desativado). Coluna "Ações", só para o principal e nunca na linha dele, com um `<form method="post">` por linha e `confirm()` |
| `wwwroot/css/pages/administradores.css` | alterar | Estilo do selo de situação e do botão de ação da linha |
| `Controllers/AutenticacaoController.cs` | alterar | Mensagem para `ResultadoDoLogin.Desativado` |
| `Controllers/ContaController.cs` | alterar | `AlterarDados`: remove a linha que sobrescreve `dto.CPF` quando o `ModelState` é inválido; checa `CpfEmUsoPorOutraConta` (erro no campo `CPF`); repassa `CPF` no `UsuarioDTO`. `NovoEndereco`/`EditarEndereco`: checam `EnderecoJaCadastrado` (erro em `Numero`) antes de chamar o serviço |
| `Views/Conta/Index.cshtml` | alterar | Troca o `<span class="cpf-somente-leitura">` por `<input asp-for="CPF">` + `<span asp-validation-for="CPF">`, mesmo markup do cadastro |

### Testes
| Arquivo | Ação | O quê |
|---|---|---|
| `Tests/Units/Entities/UsuarioTests.cs` | alterar | `AlterarCpf` com CPF válido, inválido e vazio |
| `Tests/Units/Validators/DadosPessoaisDTOValidatorTests.cs` | alterar/criar | Regra de CPF válido e inválido |
| `Tests/Units/Services/AdministradorServiceTests.cs` | alterar | Desativar/reativar, guardas de D7, `Ativo`/`Principal` na listagem |
| `Tests/Units/Services/UsuarioServiceTests.cs` | alterar | CPF alterado, colisão de CPF, login `Desativado` vs `Bloqueado` |
| `Tests/Units/Services/EnderecoServiceTests.cs` | alterar | Duplicado no cadastro, duplicado na edição, edição do próprio endereço sem mudar CEP/número/complemento, CEP com máscara, complemento diferente passa, complemento com caixa/espaço diferente é recusado, nulo igual a vazio |
| `Tests/Units/Controllers/Admin/AdministradorControllerTests.cs` | alterar | `Forbid` para não principal, redirecionamento para o principal |
| `Tests/Units/Controllers/ContaControllerTests.cs` | alterar | CPF em uso gera erro no campo; endereço duplicado gera erro e volta a view |
| `Tests/Integration/...` (Identity) | criar ou alterar | Desativar grava o `LockoutEnd` no Postgres e `Ativo` volta `false` (prova D4). Login recusado. Reativar libera |

## 5. Contratos e modelo de dados

```csharp
// Domain/Constants/AdministradorPrincipal.cs
public static class AdministradorPrincipal
{
    public const string Email = "admin@docescabana.com.br";
    public static readonly DateTimeOffset FimDaDesativacao;
}

// Domain/Entities/Usuario.cs
public void AlterarCpf(string cpf);

// Application/Contracts/Services/IAdministradorService.cs
Task<List<AdministradorDTO>> ListarAdministradores();
Task<bool> EhPrincipal(Guid usuarioId);
Task Desativar(Guid administradorId, Guid executorId);
Task Reativar(Guid administradorId, Guid executorId);

// Application/Contracts/Services/IUsuarioService.cs
Task<bool> CpfEmUsoPorOutraConta(string cpf, Guid usuarioId);

// Application/Contracts/Services/IEnderecoService.cs
Task<bool> EnderecoJaCadastrado(EnderecoDTO dto, Guid usuarioId);

// Application/Enums/ResultadoDoLogin.cs
Sucesso, Bloqueado, Desativado, Falhou

// MVC/Areas/Admin/Controllers/AdministradorController.cs
[HttpPost][ValidateAntiForgeryToken] Task<IActionResult> Desativar(Guid id);
[HttpPost][ValidateAntiForgeryToken] Task<IActionResult> Reativar(Guid id);
```

Sem migration: o lockout usa as colunas `LockoutEnd`/`LockoutEnabled` que já
existem em `AspNetUsers`, e o índice único de `Usuario.CPF` já existe.

## 6. Ordem de execução

- [x] 1. Teste de `Usuario.AlterarCpf` → criar `AdministradorPrincipal` e `AlterarCpf`. Apontar `DbInitializer.EmailAdministrador` para a constante.
- [x] 2. Teste do validator (CPF) → regra em `DadosPessoaisDTOValidator`.
- [x] 3. Testes de `EnderecoService` (duplicado, edição do próprio, máscara) → `MensagensEndereco`, `EnderecoJaCadastrado` e guardas.
- [x] 4. Testes de `ContaController` (endereço duplicado) → checagem em `NovoEndereco`/`EditarEndereco`.
- [x] 5. Testes de `UsuarioService` (alterar CPF, colisão) → `CpfEmUsoPorOutraConta` + `AlterarDadosUsuario`.
- [x] 6. Testes de `ContaController` (CPF) → `AlterarDados` + `Views/Conta/Index.cshtml`.
- [x] 7. Testes de `AdministradorService` (listagem, desativar, reativar, guardas) → `AdministradorDTO`, contrato e implementação.
- [x] 8. Teste de `UsuarioService.RealizarLogin` (`Desativado` vs `Bloqueado`) → enum, serviço e mensagem no `AutenticacaoController`.
- [ ] 9. Teste de integração de desativar/reativar no Postgres (D4).
- [x] 10. `ValidationInterval` de 1 minuto em `IdentityDependencyInjection`.
- [x] 11. Testes de `AdministradorController` → ações, `Index.cshtml` e CSS.
- [ ] 12. `dotnet build` e `dotnet test` completos. Fumaça manual: o principal desativa um admin, esse admin perde a sessão em até 1 minuto e não entra mais; o principal o reativa e ele entra.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| CPF inválido não entra no domínio | `Dado_CpfInvalido_Quando_AlterarCpf_Entao_DeveLancarArgumentException` |
| CPF com máscara é gravado só com dígitos | `Dado_CpfComMascara_Quando_AlterarCpf_Entao_DeveGuardarApenasDigitos` |
| CPF de outra conta é recusado com mensagem no campo | `Dado_CpfDeOutraConta_Quando_AlterarDados_Entao_DeveRetornarViewComErroNoCpf` |
| Manter o próprio CPF não acusa colisão | `Dado_ProprioCpf_Quando_CpfEmUsoPorOutraConta_Entao_DeveRetornarFalse` |
| Endereço com o mesmo CEP, número e complemento é recusado | `Dado_MesmoCepNumeroEComplemento_Quando_Cadastrar_Entao_DeveLancarInvalidOperationException` |
| Outro apartamento do mesmo prédio passa | `Dado_MesmoCepENumeroComComplementoDiferente_Quando_EnderecoJaCadastrado_Entao_DeveRetornarFalse` |
| Caixa e espaço no complemento não furam a regra | `Dado_ComplementoComCaixaEEspacoDiferentes_Quando_EnderecoJaCadastrado_Entao_DeveRetornarTrue` |
| Complemento nulo e vazio são o mesmo | `Dado_ComplementoNuloEVazio_Quando_EnderecoJaCadastrado_Entao_DeveRetornarTrue` |
| CEP com máscara conta como o mesmo CEP | `Dado_CepComMascaraIgualAoExistente_Quando_EnderecoJaCadastrado_Entao_DeveRetornarTrue` |
| Editar o próprio endereço sem mudar o CEP e o número passa | `Dado_EdicaoDoProprioEndereco_Quando_EnderecoJaCadastrado_Entao_DeveRetornarFalse` |
| Só o principal desativa | `Dado_ExecutorNaoPrincipal_Quando_Desativar_Entao_DeveRetornarForbid` (controller) e `..._DeveLancarInvalidOperationException` (serviço) |
| O principal não se desativa | `Dado_AlvoPrincipal_Quando_Desativar_Entao_DeveLancarInvalidOperationException` |
| A desativação sobrevive ao Postgres | Integração: desativar → `ListarAdministradores` devolve `Ativo == false` |
| Admin desativado ouve a mensagem certa | `Dado_ContaDesativada_Quando_RealizarLogin_Entao_DeveRetornarDesativado` |
| Bloqueio por senha errada continua com a mensagem antiga | `Dado_BloqueioTemporario_Quando_RealizarLogin_Entao_DeveRetornarBloqueado` |
| Sessão aberta cai | Fumaça manual (passo 12). Sem E2E: um E2E não prova nada aqui que unidade + integração + fumaça não provem (Princípio V) |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| R1. O CPF também é login (`ResolverUsuario` aceita CPF). Quem troca o CPF passa a entrar com o novo | Médio | É o comportamento esperado. Na fumaça manual, entrar com o CPF novo |
| R2. `ValidationInterval` de 1 minuto gera uma consulta extra por usuário logado a cada minuto | Baixo | O volume do TCC é pequeno. Se pesar, sobe para 5 min (D5) |
| R3. Se o e-mail do principal for trocado direto no banco, ninguém mais desativa admins | Médio | Não existe tela para trocar e-mail. O valor fica numa constante só |
| R4. Complemento escrito de outro jeito ("Apto 101" vs "Ap. 101") é tratado como endereço diferente | Baixo | A normalização (D12) cobre caixa e espaço. Abreviação fica de fora: tentar adivinhar recusaria endereços legítimos |
| R5. Endereços duplicados já gravados continuam no banco. Editar um deles é recusado, porque o outro ainda tem o mesmo CEP, número e complemento | Baixo | O cliente exclui um dos dois. Sem índice no banco (D11), a regra vale só para gravações novas |
| R6. O admin principal com lockout ativo por senha errada fica bloqueado 15 min, e não existe outro principal | Baixo | Comportamento atual, não muda |

Sem desvio constitucional.

## 9. Resultado da execução

Executado em 2026-11-25.

**Passou**
- `dotnet build` da solução: 0 erros, 0 avisos.
- Testes de unidade (`FullyQualifiedName!~Integration`): 1207 aprovados, 0 falhas.
- `dotnet ef migrations has-pending-model-changes`: nenhuma mudança de modelo. As propriedades novas de `ContaDeAcesso` (somente leitura) não viraram colunas. Sem migration, como previsto.

**Não rodou**
- Passo 9 e a parte de integração do passo 12. O Docker Desktop não estava em execução, então os 103 testes de integração (os 2 novos de `DesativacaoDeContaIntegrationTests` e os 101 já existentes) falharam ao subir o contêiner, com "Não foi possível subir o Postgres de teste". A falha é de ambiente, não de código. Falta rodar `dotnet test` com o Docker ligado.
- E2E (`DocesCabana.Tests.E2E`) não foi executado. Esta feature não criou nenhum E2E (seção 7). O fluxo `AreaAdministrativaTests` continua achando a linha do admin pelo e-mail.
- Fumaça manual do passo 12.

**Desvios do desenho, todos pequenos**
- `ContaDeAcesso` (não listado na seção 4) ganhou `Desativada` e `EhAdministradorPrincipal`, propriedades calculadas. Assim a regra de D4 fica num lugar só, usada por `AdministradorService`, `UsuarioService.RealizarLogin` e pelo mapper.
- Criado `Infrastructure/Identity/Mappings/AdministradorMapper.cs` (Princípio VI: tradução por mapper estático), em vez de montar o DTO dentro do serviço.
- A mensagem de CPF vazio é "O CPF é obrigatório!" (a do plano dizia "CPF é obrigatório!"), para seguir a regra vizinha do `CadastroDTOValidator`, que também foi copiada nas checagens de tamanho e de 11 dígitos. Usa `Cascade(CascadeMode.Stop)` para o `CpfHelper` nunca receber nulo.
- `wwwroot/js/pages/conta.js` ganhou a máscara de CPF. `.cpf-somente-leitura` saiu de `conta.css` porque ficou sem uso.
- `Admin/Administrador/Index.cshtml` reaproveita `.selo-status`, `.acoes-produto` e `.botao-icone` de `gestao-produtos.css`, como a tela de cupons. O `confirm()` não cita o nome do admin: um apóstrofo no nome quebraria a string JavaScript.
- `EnderecoServiceTests`: os testes de "cadastrar o segundo endereço" e "editar" passaram a usar dados não repetidos. A intenção deles não mudou, mas sob a regra nova os dados antigos colidiam.
- `ContaControllerTests`: o teste que exigia recarregar o CPF do banco com `ModelState` inválido virou "volta com o CPF digitado", porque o campo agora é editável.

**Verificar manualmente**
1. Ligar o Docker e rodar `dotnet test` completo.
2. Como `admin@docescabana.com.br`: desativar outro admin. O admin desativado, com sessão aberta, deve cair em até 1 minuto; ao tentar entrar, deve ver "Conta desativada. Procure o administrador da loja.". Depois reativar e conferir que ele entra.
3. Como admin não principal: a coluna "Ações" não aparece.
4. Em "Minha conta": trocar o CPF por um válido e entrar com o CPF novo; tentar um CPF de outra conta e ver o erro no campo.
5. Em "Endereços": cadastrar dois endereços com o mesmo CEP e número, com complementos "Apto 101" e "Apto 102" (deve aceitar); repetir "apto 101" (deve recusar).
