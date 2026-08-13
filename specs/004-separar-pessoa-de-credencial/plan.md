# Plano Técnico — Separar pessoa de credencial

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-12
**Status:** Executado

---

## 1. Resumo da abordagem

A classe `Usuario` de hoje vira duas:

| Onde | Nome | O que guarda |
|---|---|---|
| `Domain/Entities` | `Usuario` | `UsuarioId`, `Nome`, `CPF`, `Celular`, `DataNascimento` |
| `Infrastructure/Identity` | `ContaDeAcesso : IdentityUser<Guid>` | e-mail, hash de senha, bloqueio, e o mais que o Identity impõe |

As duas compartilham o identificador: `ContaDeAcesso.Id == Usuario.UsuarioId`. A
conta é a **principal** — é o `UserManager` quem gera o `Guid` — e o `Usuario` é
o dependente, com chave estrangeira apontando para ela.

Feito isso, `Endereco`, `Favorito`, `Avaliacao` e `Pedido` trocam o `Guid` solto
por navegação de verdade para o `Usuario` do domínio, encerrando a RQ-02 da
spec `003`.

O trabalho é feito em cinco blocos: **(A)** as duas classes e suas
configurações; **(B)** a renomeação de `Usuario` para `ContaDeAcesso` nos 21
arquivos que a referenciam; **(C)** o serviço, que passa a compor as duas
metades; **(D)** a navegação das quatro entidades; **(E)** migration, banco e
documentação.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⚠️ Emenda | Esta feature **melhora** a conformidade: o dado de negócio volta ao domínio e quatro entidades param de usar `Guid` solto. Mas a exceção do Princípio I muda de redação — o motivo deixa de ser a entidade. Emenda **MINOR** → constituição 1.2.0. Ver seção 9 |
| II | Domínio rico e auto-validante | ✅ OK | O `Usuario` do domínio nasce com `private set`, construtor que valida antes de atribuir e `protected Ctor()`. As invariantes não são novas — mudam de casa, com os testes junto |
| III | Validação nas duas barreiras | ✅ OK | `CadastroDTOValidator` fica inalterado: continua validando os mesmos campos na entrada. As invariantes de domínio agora ficam no `Usuario` do domínio, e a de e-mail na `ContaDeAcesso` |
| IV | Nomenclatura em português | ✅ OK | A RQ-03 é o que dá ao domínio o termo `Usuario`, que é como o `.dbml` chama o conceito. `ContaDeAcesso` também é português |
| V | Testes escritos antes | ✅ OK | Cada bloco tem teste antes. Os testes que existem migram junto com as invariantes que cobrem |
| VI | Repositório + commit via `UnitOfWork` | ✅ OK | `IUsuarioRepository` novo, com `BuscarPorCpf`. Gravação do `Usuario` via `IUnitOfWork`. Uma migration versionada |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ n/a | Nenhum controller muda de comportamento |
| VIII | Tratamento de erro por camada | ✅ OK | A compensação da RN-08 é um `try/catch` **no serviço**, não em ação de controller — que é exatamente onde o princípio permite |

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Usuario.cs` | **criar** | RN-01 a RN-04. `AtualizarDados(nome, celular, dataNascimento)`. Normaliza CPF e celular para dígitos no construtor |
| `Entities/Endereco.cs` | alterar | RQ-04 — navegação `Usuario? Usuario` sobre o `UsuarioId` que já existe |
| `Entities/Favorito.cs` | alterar | RQ-04 — idem |
| `Entities/Avaliacao.cs` | alterar | RQ-04 — idem |
| `Entities/Pedido.cs` | alterar | RQ-04 — idem |

Nenhuma coluna nova nas quatro: a navegação assenta sobre chave que já existe.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Contracts/Repositories/IUsuarioRepository.cs` | **criar** | `IRepository<Usuario>` mais `Task<Usuario?> BuscarPorCpf(string cpf)` — o login por CPF precisa consultar o domínio, não mais o Identity |
| `DTOs/UsuarioDTO.cs` | — | Inalterado. Continua com `Id`, `Nome`, `Email`, `Celular`, `DataNascimento`, `CPF`; o que muda é de onde cada campo vem |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Identity/Usuario.cs` | **renomear** | → `Identity/ContaDeAcesso.cs`. Perde `Nome`, `CPF`, `DataNascimento`; mantém a validação de e-mail (RN-06); ganha navegação `Usuario? Usuario` — Infraestrutura pode referenciar Domínio |
| `Identity/Services/UsuarioService.cs` | alterar | Compõe as duas metades. Ver seção 4 |
| `Identity/Services/IUsuarioService.cs` | alterar | Só a troca de tipo onde aparece; a assinatura pública não muda |
| `Identity/Mappings/UsuarioMapper.cs` | alterar | `ToDTO(Usuario, ContaDeAcesso)` — o DTO passa a ser composto das duas |
| `Repositories/UsuarioRepository.cs` | **criar** | `BuscarPorCpf` com `FirstOrDefaultAsync` |
| `DatabaseContext/Configurations/UsuarioConfiguration.cs` | reescrever | Passa a configurar o `Usuario` do **domínio**: tabela `Usuario`, índice único de CPF, e a chave estrangeira 1:1 para `ContaDeAcesso` |
| `DatabaseContext/Configurations/ContaDeAcessoConfiguration.cs` | **criar** | Tabela `ContaDeAcesso`, com a navegação para `Usuario` |
| `DatabaseContext/Configurations/{Endereco,Favorito,Avaliacao,Pedido}Configuration.cs` | alterar | `HasOne<Usuario>()` sem navegação vira `HasOne(x => x.Usuario)` apontando para o domínio |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | `IdentityDbContext<ContaDeAcesso, …>`; `DbSet<Usuario>` novo |
| `DependencyInjections/IdentityDependencyInjection.cs` | alterar | `AddIdentity<ContaDeAcesso, IdentityRole<Guid>>` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar `IUsuarioRepository` |
| `Migrations/` | criar | `SepararPessoaDeCredencial` |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AutenticacaoController.cs` | alterar | Só o `using`; nenhum comportamento muda |
| `Helpers/DbInitializer.cs` | alterar | O administrador semeado passa a ser criado em duas metades |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/UsuarioTests.cs` | alterar | Passa a testar o `Usuario` do **domínio**; os casos de e-mail saem daqui |
| `Units/Entities/ContaDeAcessoTests.cs` | **criar** | Recebe os casos de e-mail que saíram do arquivo acima (RN-06) |
| `Units/Mappings/UsuarioMapperTests.cs` | alterar | Assinatura nova do `ToDTO` |
| `Units/Services/UsuarioServiceTests.cs` | alterar | Troca de tipo; caminho de CPF passa pelo repositório |
| `Units/Services/UsuarioServiceLoginTests.cs` | alterar | Idem |
| `Units/Services/UsuarioServiceCadastroTests.cs` | **criar** | RN-08 e CA-04 — a compensação quando a segunda metade falha |
| `Units/Controllers/AutenticacaoControllerTests.cs` | alterar | Só `using`; opera sobre DTO |
| `Integration/InfraestruturaSqliteEmMemoria.cs` | alterar | `SemearUsuario` passa a criar as duas metades e devolver o `Guid` compartilhado |
| `Integration/DatabaseIntegrationTests.cs` | alterar | O teste de atomicidade usa CPF duplicado no `Usuario` do domínio |
| `Integration/Repositories/ModeloDeDadosIntegrationTests.cs` | alterar | Acrescentar CA-05: `Endereco` com `Include(e => e.Usuario)` traz o nome |

## 4. Contratos

```csharp
// Domain/Entities/Usuario.cs — novo
public Usuario(Guid usuarioId, string nome, string cpf, string celular, DateTime dataNascimento);
public void AtualizarDados(string nome, string celular, DateTime dataNascimento);

// Infrastructure/Identity/ContaDeAcesso.cs — antes Usuario
public ContaDeAcesso(string email);
public Usuario? Usuario { get; private set; }   // Infra -> Domain: permitido

// Application/Contracts/Repositories/IUsuarioRepository.cs — novo
public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> BuscarPorCpf(string cpf);
}
```

`IUsuarioService` mantém **todas as assinaturas públicas atuais**. Nenhum
controller muda de forma.

### O ponto delicado: criar as duas metades

`UserManager.CreateAsync` grava por conta própria — ele não participa do
`IUnitOfWork`. Ou seja, o cadastro tem **duas gravações**, e a segunda pode
falhar depois de a primeira ter sucedido. O caso concreto é o CPF repetido: o
e-mail passa pela checagem do Identity, a conta é criada, e só então o índice
único de CPF recusa o `Usuario`.

Sem tratamento, sobra uma credencial órfã — alguém com senha e sem cadastro. A
RN-08 e o CA-04 existem para impedir isso. O serviço compensa:

```csharp
var conta = new ContaDeAcesso(dto.Email!);
var resultado = await _userManager.CreateAsync(conta, dto.Senha!);
// ... trata falha do Identity como hoje (duplicidade, senha fraca)

try
{
    var usuario = new Usuario(conta.Id, dto.Nome!, dto.CPF!, dto.Celular!, dto.DataNascimento!.Value);
    await _usuarioRepository.Adicionar(usuario);
    await _unitOfWork.SalvarAlteracoes();
    return UsuarioMapper.ToDTO(usuario, conta);
}
catch
{
    await _userManager.DeleteAsync(conta);   // desfaz a primeira metade
    throw;
}
```

Vale notar que isto **não** reabre a discussão de transação explícita que a spec
`002` encerrou: o problema aqui não é atomicidade dentro de um `SaveChanges` — é
que o `UserManager` grava fora do nosso controle. Transação de banco não
resolveria sem envolver o Identity na mesma conexão, o que é bem mais invasivo
que uma compensação de quatro linhas.

## 5. Modelo de dados

### Antes

Uma tabela `Usuario` com as colunas do Identity **mais** `NomeCompleto`, `CPF` e
`DataNascimento`.

### Depois

| Tabela | Chave | Colunas |
|---|---|---|
| `ContaDeAcesso` | `Id` | Todas as do `IdentityUser<Guid>`: `UserName`, `NormalizedUserName`, `Email`, `NormalizedEmail`, `PasswordHash`, `SecurityStamp`, `LockoutEnd`, `AccessFailedCount`, … |
| `Usuario` | `UsuarioId` | → `ContaDeAcesso.Id` (1:1, chave compartilhada), `Nome` (255), `CPF` (11, índice único), `Celular` (11), `DataNascimento` (`date`) |

`PhoneNumber` continua existindo em `ContaDeAcesso` porque vem do
`IdentityUser<Guid>` e não dá para removê-la, mas **deixa de ser escrita e
lida** (RQ-07). O celular passa a ser do domínio. A alternativa — manter as
duas — criaria duas fontes de verdade para o mesmo dado.

### Migration

```
dotnet ef migrations add SepararPessoaDeCredencial \
  --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC
```

- **Impacto em dados existentes:** as contas do banco local são perdidas. O
  banco de desenvolvimento é descartável e não versionado, e o administrador
  semeado é recriado na subida — mesmo caminho que a spec `003` seguiu. Não há
  dado de produção.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/UsuarioTests.cs` | RN-01 a RN-04 |
| Unidade — entidade | `Units/Entities/ContaDeAcessoTests.cs` | RN-06 |
| Unidade — serviço | `Units/Services/UsuarioServiceCadastroTests.cs` | RN-08, CA-04 — a conta é apagada quando a segunda metade falha |
| Unidade — serviço | `Units/Services/UsuarioServiceLoginTests.cs` | CA-02, CA-03 — login por e-mail e por CPF, agora pelo repositório |
| Unidade — mapeamento | `Units/Mappings/UsuarioMapperTests.cs` | O DTO composto traz e-mail da conta e celular do domínio |
| Integração | `Integration/Repositories/ModeloDeDadosIntegrationTests.cs` | CA-05 — `Include(e => e.Usuario)` traz o nome; sem `Include` vem `null` |
| Integração | `Integration/DatabaseIntegrationTests.cs` | Atomicidade continua valendo com o CPF único no domínio |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_DadosValidos_Quando_CadastrarUsuario_Entao_DeveCriarAsDuasMetades` |
| CA-02 | `Dado_LoginPorEmail_Quando_RealizarLogin_Entao_DeveAutenticar` (já existe, ajustado) |
| CA-03 | `Dado_LoginPorCpfSemPontuacao_Quando_RealizarLogin_Entao_DeveAutenticar` (já existe, ajustado) |
| CA-04 | `Dado_CpfJaCadastrado_Quando_CadastrarUsuario_Entao_DeveApagarAContaCriada` |
| CA-05 | `Dado_EnderecoComUsuario_Quando_ConsultarComInclude_Entao_DeveTrazerONome` |
| CA-06 | verificação manual, tarefa de fechamento |
| CA-07 | `dotnet test`, tarefa de fechamento |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Domínio `Cliente` + infra `Usuario` | Menor churn, mas administrador também é usuário e não é cliente — `Cliente` mentiria sobre metade das contas, e o `.dbml` continuaria dizendo `Usuario` para uma tabela chamada `Cliente` |
| Domínio `Pessoa` + infra `Usuario` | Neutro e sem renomear o Identity, mas inventa um termo que não existe nem na modelagem nem no vocabulário da loja, contra o Princípio IV |
| Duplicar o e-mail nas duas metades | Mapeamento ficaria trivial, ao custo de duas fontes de verdade que divergem na primeira alteração de cadastro |
| Deixar o celular na conta, junto do `PhoneNumber` | Manteria dado de negócio preso na infraestrutura — exatamente o problema que esta spec existe para resolver |
| Reintroduzir transação explícita para o cadastro | O problema não é atomicidade dentro de um `SaveChanges`, e sim o `UserManager` gravar fora do nosso controle. Transação só ajudaria se o Identity participasse da mesma conexão, o que é muito mais invasivo que a compensação de quatro linhas |
| Mover `IUsuarioService` para a `Application` | A implementação depende de `UserManager` e `SignInManager`. Só sairia da infraestrutura atrás de uma abstração de autenticação própria — custo alto para benefício de pureza. A exceção do Princípio I é **narrada**, não eliminada |
| Manter `Usuario` como nome da classe do Identity e criar o domínio com outro nome | Deixaria o termo do negócio ocupado por uma classe de framework, que é a inversão que o Princípio IV proíbe |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Renomear `Usuario` em 21 arquivos quebrar algo silenciosamente | Média | Alto | A renomeação é a primeira coisa a compilar; 233 testes cobrem. Bloco B é commit isolado, sem mudança de comportamento junto |
| Esquecer de compensar a conta órfã em algum caminho de falha | Média | **Alto** | É a única regra de negócio nova desta spec (RN-08), com teste dedicado (CA-04) e `try/catch` cobrindo tudo entre a criação da conta e o commit |
| Perder as contas do banco local | **Alta** | Baixo | Esperado e aceito: banco descartável, admin recriado na subida. Documentado na seção 5 |
| Login por CPF quebrar ao mudar de fonte | Média | Alto | O CPF sai do Identity e passa ao repositório do domínio. Os testes de CA-02/CA-03 já existem desde a `002` e são executados a cada bloco |
| `PhoneNumber` continuar sendo escrito por engano | Baixa | Médio | Tarefa explícita de busca textual por `PhoneNumber` no fechamento |
| A migration não conseguir renomear a tabela no SQLite | Média | Médio | SQLite reconstrói tabela em vez de renomear coluna; o EF gera isso sozinho. A tarefa confere o arquivo gerado antes de aplicar, como a `003` fez |

## 9. Desvios constitucionais justificados

Nenhum desvio. Uma **emenda**, conforme a Governança item 3 — a constituição
passa de 1.1.0 para **1.2.0** (MINOR, por alteração material do alcance de uma
exceção).

| Princípio | Emenda | Justificativa |
|---|---|---|
| I | Reescrever a exceção: `IUsuarioService` vive na infraestrutura **porque sua implementação depende de `UserManager` e `SignInManager`** — não mais "porque a entidade `Usuario` herda de `IdentityUser<Guid>`". Acrescentar que as entidades de domínio referenciam `Usuario` por navegação normal. | O motivo antigo deixa de ser verdade nesta feature: a entidade `Usuario` passa a ser de domínio e não herda de nada do Identity. Manter o texto velho faria a constituição justificar a exceção por um fato que deixou de existir — e uma exceção mal justificada vira precedente para exceções que ninguém examinou. |
