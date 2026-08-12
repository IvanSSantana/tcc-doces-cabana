# Plano Técnico — Cadastro de produto pelo administrador

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-07
**Atualizado em:** 2026-08-12 · **Status:** Executado

> **Nota de atualização.** Este plano foi reescrito depois que a spec
> `003-modelo-de-dados-completo` foi implementada. A versão original desta
> spec previa criar `Categoria`/`Subcategoria` aqui — isso já existe. A
> pendência de papéis, que bloqueava a T030 original, foi resolvida: o mínimo
> viável (papel + admin semeado) entra aqui; a tela de gestão fica para a
> spec `005`.

---

## 1. Resumo da abordagem

O caminho `Admin/Cadastro → ProdutoService.Cadastrar → ProdutoRepository.Adicionar`
já existe, mas está cortado em quatro pontos, todos registrados como dívida na
baseline: o serviço nunca faz commit (D-01), o controller não aguarda o serviço
nem valida `ModelState` nem tem antiforgery (D-03), o formulário posta para uma
ação inexistente (D-04), e o campo Promoção é preenchido com um enum onde se
espera o identificador de uma promoção (D-05). A área administrativa também
está aberta a qualquer visitante (D-02). Este plano fecha os cinco pontos.

`Categoria` e `Subcategoria` **já existem** — a `003` os criou com entidade,
configuração e migration. Este plano só consome: o formulário passa a listar
subcategorias reais, e nenhuma migration de catálogo é necessária aqui.

O trabalho novo é: o papel `Administrador` e um admin semeado (RF-06), o
`ProdutoDTOValidator` que falta (D-06, já resolvido pela `002` — nada a fazer
aqui além de confirmar), a correção de `ProdutoService.Cadastrar` para persistir
de fato, e a reconstrução do controller e da view.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma referência nova entre projetos |
| II | Domínio rico e auto-validante | ⬜ n/a | Nenhuma entidade nova; `Produto` já está conforme desde a `002`/`003` |
| III | Validação nas duas barreiras | ✅ OK | `ProdutoDTOValidator` já existe (criado na `002`, dívida D-06 já resolvida) — este plano só confirma que cobre `SubcategoriaId` |
| IV | Nomenclatura em português | ✅ OK | |
| V | Testes escritos antes | ✅ OK | Fase 2 das tarefas |
| VI | Repositório + commit via `UnitOfWork` | ✅ OK | Corrige D-01 |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | Corrige D-02 e D-03 |
| VIII | Tratamento de erro por camada | ✅ OK | Sem `try/catch` na ação; validator intercepta antes do domínio |

Nenhum desvio. Esta feature reduz dívida constitucional.

## 3. Impacto por camada

### `DocesCabana.Domain`

Nenhuma alteração. `Produto`, `Categoria` e `Subcategoria` já existem e já
validam o que o RF-01 a RF-05 exigem.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Contracts/Repositories/ISubcategoriaRepository.cs` | criar | `IRepository<Subcategoria>` — marcador, como `IProdutoRepository` |
| `Contracts/Services/ISubcategoriaService.cs` | criar | `BuscarTodasSubcategorias()` — só leitura |
| `Services/SubcategoriaService.cs` | criar | |
| `DTOs/SubcategoriaDTO.cs` | criar | `SubcategoriaId`, `Nome` |
| `Mappings/SubcategoriaMapper.cs` | criar | |
| `Services/ProdutoService.cs` | alterar | `Cadastrar` passa a chamar `IUnitOfWork.SalvarAlteracoes` após `Adicionar`. **Corrige D-01, causa raiz do RF-02.** O retorno do DTO mapeado da entidade já foi corrigido pela `002` |
| `Validators/ProdutoDTOValidator.cs` | — | Já existe (criado na `002`). Confirmar que cobre `SubcategoriaId != Guid.Empty` — já cobre |

Nomenclatura de `ISubcategoriaRepository`/`ISubcategoriaService` segue o padrão
de `IProdutoRepository`/`IProdutoService`.

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/SubcategoriaRepository.cs` | criar | `Repository<Subcategoria>`, análogo a `ProdutoRepository` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar `ISubcategoriaRepository` e `ISubcategoriaService` |

**Papéis já estão habilitados.** `AddIdentity<Usuario, IdentityRole<Guid>>`
(em `IdentityDependencyInjection.cs`) já registra `RoleManager` e as tabelas
`AspNetRoles`/`AspNetUserRoles` já existem desde a migration
`AddIdentityAndFluentValidation`. Nenhuma migration nova é necessária para o
papel em si — só a semeadura.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AdminController.cs` | alterar | `[Authorize(Roles = "Administrador")]` na classe; POST vira `async Task<IActionResult>` com `[ValidateAntiForgeryToken]`, guarda de `ModelState`, `await _produtoService.Cadastrar(dto)`, sucesso via `TempData` + `RedirectToAction`. GET carrega subcategorias via `ISubcategoriaService` |
| `Views/Admin/Cadastro.cshtml` | alterar | `asp-action="Cadastro"` (corrige D-04); `<select>` de subcategoria com `asp-items` (RF-07); campo Promoção removido (corrige D-05); mensagem de sucesso do `TempData` |
| `Helpers/DbInitializer.cs` | alterar | Semear o papel `Administrador` e um usuário administrador, **fora de produção** (o método já roda condicionalmente desde a `002`). Senha via *user secret*, nunca literal |
| `wwwroot/css/pages/cadastro_produto.css` | criar | A view já referencia este arquivo, que não existe |

## 4. Contratos

```csharp
// Application/Contracts/Services/ISubcategoriaService.cs — novo
public interface ISubcategoriaService
{
    Task<List<SubcategoriaDTO>> BuscarTodasSubcategorias();
}

// Application/Contracts/Repositories/ISubcategoriaRepository.cs — novo
public interface ISubcategoriaRepository : IRepository<Subcategoria>
{
}
```

`IProdutoService.Cadastrar` já tem a assinatura correta desde a `002`
(`Task<ProdutoDTO> Cadastrar(ProdutoDTO dto)`, devolvendo o DTO da entidade
persistida) — nenhuma mudança de contrato aqui, só a implementação passa a
persistir de fato.

## 5. Modelo de dados

Nenhuma mudança de esquema. `Categoria`, `Subcategoria` e a FK real de
`Produto.SubcategoriaId` já existem desde a migration `AddRemainingDomainEntities`
da spec `003`. Nenhuma migration nesta feature.

**Semente de administrador:** um `Usuario` e o papel `Administrador` são
criados pelo `DbInitializer.Semear`, condicionado a `!IsProduction()`, como o
resto da massa inicial. A senha vem de *user secret*
(`Admin:SenhaInicial`) — nunca literal no código.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — serviço | `Units/Services/ProdutoServiceTests.cs` | `Cadastrar` chama `Adicionar` **e** `SalvarAlteracoes` |
| Unidade — serviço | `Units/Services/SubcategoriaServiceTests.cs` | `BuscarTodasSubcategorias` mapeia a lista do repositório |
| Unidade — controller | `Units/Controllers/AdminControllerTests.cs` | `ModelState` inválido devolve `ViewResult` e não chama o serviço; válido chama e redireciona; GET carrega subcategorias na `ViewBag`/`ViewModel` |
| Integração | `Integration/Repositories/ProdutoRepositoryIntegrationTests.cs` | Produto gravado com subcategoria real é relido do SQLite após commit — já existente desde a `003`, sem alteração necessária |

| Critério de aceite | Teste que o prova |
|---|---|
| CA-01 | `Dado_ProdutoValido_Quando_CadastroPost_Entao_DeveRedirecionarComMensagemDeSucesso` |
| CA-02 | Já provado pelo teste de integração existente (persistência real via `SalvarAlteracoes`) |
| CA-03, CA-04, CA-05 | Já cobertos por `ProdutoDTOValidatorTests` desde a `002`; confirmar aqui, não reescrever |
| CA-06, CA-07 | Verificação manual (autorização por atributo não é unit-testável sem host de integração, fora de escopo) |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Recriar `Categoria`/`Subcategoria` nesta spec | Já existem, criadas pela `003`. Recriar duplicaria trabalho e quebraria a migration já aplicada |
| Construir a página de gestão de administradores aqui | É a spec `005`. Esta feature entrega só o papel e um admin semeado — o mínimo que o RF-06 exige |
| Fazer `SaveChanges` dentro de `Repository.Adicionar` | Quebra o Princípio VI — cada operação viraria sua própria transação |
| Deixar subcategoria como campo `Guid` digitado à mão | Cumpre a letra do RF-01 e viola o RF-07 |
| Proteger a rota com checagem manual de usuário no controller | `[Authorize(Roles = ...)]` é o mecanismo do framework |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Confundir subcategoria desta feature com a da `003` e recriar | Baixa | Médio | Este plano já reflete que `Categoria`/`Subcategoria` estão prontas; nenhuma tarefa de domínio para elas |
| Credencial do administrador semeado vazar para produção | Baixa | Alto | Semear apenas fora de produção; senha via *user secret* |
| `SubcategoriaService` sem consumidor além do `<select>` | Baixa | Baixo | É leitura simples; se a `004` (listagem/edição) precisar de mais, estende sem quebrar |

## 9. Desvios constitucionais justificados

Nenhum.
