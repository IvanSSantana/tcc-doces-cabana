# Constituição — Doces Cabana

**Versão:** 1.3.0 · **Ratificada em:** 2026-08-07 · **Última alteração:** 2026-08-13

Este documento define os princípios inegociáveis do projeto. Toda `spec`, `plan` e
`tasks` é validada contra ele antes de virar código. Quando uma decisão técnica
conflita com um princípio daqui, ou o plano muda, ou a constituição é emendada
explicitamente (com registro no histórico ao final) — nunca ignorada em silêncio.

---

## Princípio I — Direção de dependência é sagrada

O projeto é uma Clean Architecture de quatro camadas. As referências entre projetos
só podem apontar para dentro:

```
MVC ──────► Application ──────► Domain
 │                                 ▲
 └────────► Infrastructure ────────┘
```

- `DocesCabana.Domain` **não referencia ninguém**. Sem EF Core, sem ASP.NET, sem
  FluentValidation, sem `Microsoft.*` além do BCL.
- `DocesCabana.Application` referencia apenas `Domain`. Define contratos
  (`Contracts/Repositories`, `Contracts/Services`) que a infraestrutura implementa.
- `DocesCabana.Infrastructure` implementa os contratos da `Application`. É o único
  lugar onde EF Core, Identity e SMTP aparecem.
- `DocesCabana.MVC` só conhece `Application` (DTOs e interfaces de serviço) e os
  módulos de injeção de dependência da `Infrastructure`.

**Exceção conhecida e documentada:** `IUsuarioService` vive em
`Infrastructure/Identity/Services` porque sua implementação depende de
`UserManager` e `SignInManager`, tipos do ASP.NET Identity. A entidade
`Usuario` em si é de domínio — não herda de nada do Identity, que é quem
guarda a credencial (`ContaDeAcesso`, em `Infrastructure/Identity`). Controllers
dependem de `IUsuarioService` diretamente. Qualquer nova feature de autenticação
segue esse mesmo caminho; **nenhuma outra** exceção à direção de dependência é
permitida sem emenda constitucional.

Entidades de domínio referenciam `Usuario` por propriedade de navegação normal,
como referenciam qualquer outra entidade do domínio — a exceção acima é sobre
onde `IUsuarioService` mora, não sobre como o domínio se relaciona com
`Usuario`.

**Como verificar:** olhar os `<ProjectReference>` do `.csproj` alterado. Se uma
tarefa exige uma referência nova, ela viola este princípio até prova em contrário.

---

## Princípio II — O domínio se defende sozinho

Entidades de domínio são ricas, não sacos de propriedades.

- Propriedades têm `private set`. Estado só muda por método de intenção
  (`AlterarPreco`, `AplicarPromocao`, `AtualizarDados`) — nunca por atribuição direta.
- O construtor público **valida antes de atribuir** e lança `ArgumentException` /
  `ArgumentNullException` com mensagem em português. Um objeto que existe é um
  objeto válido.
- Existe um `protected Ctor() { }` sem parâmetros exclusivamente para o EF Core
  materializar a entidade.
- Invariantes que dependem de outro agregado (ex.: "produto inativo não entra em
  promoção") vivem na entidade, não no serviço.

Referência viva: [`Produto.cs`](../../DocesCabana.Domain/Entities/Produto.cs).

---

## Princípio III — Validação em duas barreiras, e elas têm propósitos diferentes

| Barreira | Onde | Para quê | Resultado do erro |
|---|---|---|---|
| Entrada | `Application/Validators/*Validator.cs` (FluentValidation) | Proteger o **usuário**: formato, tamanho, obrigatoriedade, confirmação de senha | `ModelState` inválido → volta a view com mensagem no campo |
| Invariante | Construtor/métodos da entidade | Proteger o **dado**: regra de negócio que não pode ser violada por caminho nenhum | Exceção → capturada pelo `FilterException` |

Duplicar a regra nas duas barreiras é esperado e correto. Ter a regra em **apenas
uma** delas é o defeito: só no validator significa que a API interna aceita lixo;
só no domínio significa que o usuário recebe uma tela de erro em vez de uma
mensagem de campo.

Validators são registrados automaticamente pelo assembly scan em
[`FluentValidationDependencyInjection.cs`](../../DocesCabana.Application/DependencyInjections/FluentValidationDependencyInjection.cs) —
criar o arquivo `*Validator.cs` já o coloca no pipeline.

---

## Princípio IV — Português é a língua ubíqua

Todo o código de negócio é escrito em português: classes, métodos, propriedades,
parâmetros, nomes de teste, mensagens de erro, comentários, rotas e views.

- ✅ `BuscarProdutoPorId`, `AlterarStatus`, `ProdutoStatus.ForaDeEstoque`
- ❌ `GetProductById`, `ChangeStatus`, `ProductStatus.OutOfStock`

Mantêm-se em inglês apenas os termos impostos pelo framework (`Controller`,
`IActionResult`, `Task`, `Repository`, `DTO`, `Id`) e o vocabulário do ASP.NET
Identity herdado (`UserName`, `PhoneNumber`, `Email`).

O nome do arquivo coincide com o nome do tipo que ele declara, e a pasta em que
o arquivo vive coincide com o namespace declarado. `TransactionEf.cs` que
declara `TransactionEF`, ou uma classe `EsqueceuSenhaDTOValidator` num arquivo
`EsqueceuSenhaValidator.cs`, são o defeito que esta regra existe para evitar.

A cultura da aplicação é fixada em `pt-BR` no
[`Program.cs`](../../DocesCabana.MVC/Program.cs) — decimais usam vírgula, datas usam
`dd/MM/yyyy`. Toda feature que formata número ou data respeita isso.

---

## Princípio V — Teste antes, no formato Dado/Quando/Então

Nenhuma tarefa de implementação começa sem o teste correspondente escrito e
**falhando**. O ciclo é: escrever teste → ver falhar → implementar → ver passar.

Nomenclatura obrigatória:

```csharp
[Fact]
public async Task Dado_IdInexistente_Quando_BuscarProdutoPorId_Entao_DeveLancarKeyNotFoundException()
```

Organização em `DocesCabana.Tests`:

- `Units/Entities` — invariantes de domínio, sem mocks.
- `Units/Services` — regra de aplicação, repositório via `Moq`.
- `Units/Validators` — cada `RuleFor` com um caso válido e um inválido.
- `Units/Controllers` — tipo de `IActionResult`, `ModelState`, redirecionamento.
- `Integration/Repositories` — SQLite em memória via
  [`InfraestruturaSqliteEmMemoria`](../../DocesCabana.Tests/Integration/InfraestruturaSqliteEmMemoria.cs).

Ferramentas fixas: xUnit + Moq + coverlet para teste de unidade e de
integração; `Microsoft.Playwright` para teste de ponta a ponta em navegador,
com o xUnit seguindo como runner único. Não introduzir outro framework de
teste, nem um segundo runner.

**Definição de "pronto" para uma feature:** `dotnet test` verde, cobertura das
regras de negócio novas em teste unitário, e ao menos um teste de integração
quando a feature toca persistência.

---

## Princípio VI — Persistência é escondida e o commit é explícito

- Acesso a dados só através de `IRepository<T>` / `I*Repository`. Nenhum
  `DbContext` fora de `Infrastructure`.
- O `Repository<T>` **não persiste**: `Adicionar`, `Atualizar` e `Remover` apenas
  registram a mudança no `ChangeTracker`. A gravação acontece via
  `IUnitOfWork.SalvarAlteracoes`, chamado pela camada de aplicação, que é quem
  decide quando o lote de mudanças está pronto para ir ao banco. Um caso de uso
  que escreve e não chama o `IUnitOfWork` **não salvou nada**. Não existe
  transação explícita separada: `SalvarAlteracoes` já é atômico por si — um
  lote com uma alteração inválida não persiste nenhuma das outras.
- Mudança de esquema exige migration EF Core versionada em
  `Infrastructure/Migrations`, com nome descritivo em inglês (padrão da ferramenta:
  `InitialCreate`, `SanitizingDatabase`).
- Mapeamentos ficam em `DatabaseContext/Configurations/*Configuration.cs`, um por
  entidade, nunca por Data Annotation na entidade de domínio.
- A tradução entidade ↔ DTO é feita por `Mappings/*Mapper.cs` estático e manual.
  Não introduzir AutoMapper.

---

## Princípio VII — Seguro por padrão na borda web

Toda ação de controller nasce com estas garantias, e removê-las precisa de
justificativa escrita na `spec`:

- `[HttpPost]` **sempre** com `[ValidateAntiForgeryToken]`.
- Ação que muda estado é `async Task<IActionResult>` e **aguarda** o serviço.
  Chamada assíncrona não aguardada é bug de corrupção de dados, não estilo.
- `if (!ModelState.IsValid) return View(dto);` antes de qualquer efeito colateral.
- Área administrativa exige autorização explícita (`[Authorize]` com política ou
  papel). Rota administrativa acessível anonimamente é falha de segurança.
- Sucesso de POST redireciona (POST-Redirect-Get); não retorna `View()` direto.
- Segredos (connection string de produção, credenciais SMTP) nunca são commitados.
  Em desenvolvimento vão para *user secrets* ou `appsettings.Development.json`
  não versionado.
- Enumeração de conta é evitada: mensagens de recuperação de senha são idênticas
  para login existente e inexistente.

---

## Princípio VIII — Cada camada tem um dono de erro

- **Domínio** lança `ArgumentException` / `InvalidOperationException`.
- **Aplicação** lança `KeyNotFoundException` para recurso ausente e propaga o resto.
- **MVC** não faz `try/catch` em ação: o filtro global
  [`FilterException`](../../DocesCabana.MVC/Filters/FilterException.cs) captura e
  direciona. Erro esperado do usuário vira `ModelState.AddModelError`, não exceção.

---

## Governança

1. Esta constituição prevalece sobre preferência pessoal e sobre o que "já está
   assim" no código. Código legado que a viola é dívida a ser registrada, não
   precedente a ser seguido.
2. Toda `plan.md` contém uma seção **Verificação Constitucional**. Se algum item
   ficar marcado como violado, a `plan` precisa de uma justificativa explícita
   dizendo por que a alternativa conforme foi descartada.
3. Emendas seguem versionamento semântico:
   - **MAJOR** — remoção ou redefinição incompatível de um princípio.
   - **MINOR** — novo princípio ou expansão material de um existente.
   - **PATCH** — correção de texto, exemplo ou link.
4. Toda emenda registra data e motivo no histórico abaixo.

### Histórico de emendas

| Versão | Data | Alteração |
|---|---|---|
| 1.0.0 | 2026-08-07 | Ratificação inicial, extraída da arquitetura e das convenções já presentes no código. |
| 1.1.0 | 2026-08-11 | Feature `002-revisao-tecnica`. Princípio IV ganha a regra de que nome de arquivo, nome de tipo e pasta/namespace coincidem (RQ-03). Princípio VI perde a menção a transação explícita: `IUnitOfWork` fica só com `SalvarAlteracoes` — a abstração de transação manual foi removida por não ter consumidor e por duplicar a atomicidade que `SaveChangesAsync` já garante (RQ-02). |
| 1.2.0 | 2026-08-13 | Feature `004-separar-pessoa-de-credencial`. A exceção do Princípio I é reescrita: o motivo deixa de ser "a entidade `Usuario` herda de `IdentityUser<Guid>`" (deixou de ser verdade — `Usuario` passou a ser do domínio) e passa a ser a dependência de `UserManager`/`SignInManager`. Acrescentado que entidades de domínio referenciam `Usuario` por navegação normal, encerrando a limitação que a `003` havia registrado como RQ-02. |
| 1.3.0 | 2026-08-13 | Feature `007-testes-e2e-com-playwright`. Princípio V passa a distinguir camada de teste: xUnit + Moq + coverlet continuam fixos para unidade e integração; `Microsoft.Playwright` entra como driver de navegador para teste de ponta a ponta, com o xUnit seguindo como runner único — não introduzido um segundo runner, só um driver para uma camada que a stack anterior não alcançava. |
