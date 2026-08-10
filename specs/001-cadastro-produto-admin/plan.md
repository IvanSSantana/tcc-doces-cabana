# Plano Técnico — Cadastro de produto pelo administrador

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-07
**Status:** Rascunho — bloqueado pela pendência de papéis na seção 10 da spec

---

## 1. Resumo da abordagem

O caminho `Admin/Cadastro → ProdutoService.Cadastrar → ProdutoRepository.Adicionar`
já existe, mas está cortado em três pontos: o serviço nunca faz commit, o
controller não aguarda o serviço, e o formulário posta para uma ação inexistente.
O plano fecha esse caminho de ponta a ponta, adiciona a barreira de validação de
entrada que hoje falta, e protege a rota com autorização por papel.

O trabalho é majoritariamente de **conexão**, não de código novo: `Produto`,
`ProdutoMapper` e `ProdutoRepository` ficam praticamente intocados. A parte nova
é o papel `Administrador`, o `ProdutoDTOValidator` e o carregamento da lista de
subcategorias para o `select`.

Subcategoria é o ponto de atenção: `Produto.SubcategoriaId` é obrigatório e
validado, mas **não existe entidade `Subcategoria`** no domínio. Este plano cria
o mínimo viável dela — entidade, configuração, repositório de leitura e massa
inicial — porque sem isso o RF-07 não tem como ser cumprido.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma referência nova entre projetos |
| II | Domínio rico e auto-validante | ✅ OK | `Subcategoria` nasce no mesmo padrão de `Produto` |
| III | Validação nas duas barreiras | ✅ OK | Corrige a dívida D-06: cria o `ProdutoDTOValidator` que falta |
| IV | Nomenclatura em português | ✅ OK | |
| V | Testes escritos antes | ✅ OK | Fase 2 das tarefas |
| VI | Repositório + commit via UnitOfWork | ✅ OK | Corrige a dívida D-01, causa raiz do RF-02 |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | Corrige as dívidas D-02 e D-03 |
| VIII | Tratamento de erro por camada | ✅ OK | Sem `try/catch` na ação; validator intercepta antes do domínio |

Nenhum desvio. Esta feature **reduz** dívida constitucional em vez de criar.

## 3. Impacto por camada

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Subcategoria.cs` | criar | `SubcategoriaId`, `CategoriaId`, `Nome`; construtor validante; `protected Ctor()` |
| `Entities/Categoria.cs` | criar | `CategoriaId`, `Nome`; mínimo necessário para a subcategoria ter dono |
| `Entities/Produto.cs` | — | intocado |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `Validators/ProdutoDTOValidator.cs` | criar | Espelha RN-01 a RN-04 na barreira de entrada |
| `Services/ProdutoService.cs` | alterar | `Cadastrar` passa a chamar `IUnitOfWork.Commit` e a devolver o DTO da entidade criada, não o DTO recebido |
| `Contracts/Repositories/ISubcategoriaRepository.cs` | criar | `BuscarTodas()` — só leitura nesta feature |
| `Contracts/Services/ISubcategoriaService.cs` | criar | `BuscarTodasSubcategorias()` |
| `Services/SubcategoriaService.cs` | criar | |
| `DTOs/SubcategoriaDTO.cs` | criar | `Id`, `Nome` |
| `Mappings/SubcategoriaMapper.cs` | criar | |
| `DTOs/ProdutoDTO.cs` | alterar | Remover `PromocaoId` do formulário (permanece na classe, sem campo na view) |

**Ponto de desenho — por que o serviço muda a assinatura de retorno:** hoje
`Cadastrar` devolve o mesmo `dto` que recebeu, que tem `Id` vazio. O `Id` real é
gerado dentro do construtor de `Produto`. Devolver `ProdutoMapper.ToDTO(produto)`
é o que permite ao controller confirmar o que de fato foi gravado.

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `DatabaseContext/Configurations/CategoriaConfiguration.cs` | criar | |
| `DatabaseContext/Configurations/SubcategoriaConfiguration.cs` | criar | FK para `Categoria`, `DeleteBehavior.Restrict` |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | `DbSet<Categoria>`, `DbSet<Subcategoria>`; FK de `Produto.SubcategoriaId` |
| `Repositories/SubcategoriaRepository.cs` | criar | |
| `Migrations/` | criar | `AddCategoriaSubcategoria` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registrar `ISubcategoriaRepository` e `ISubcategoriaService` |
| `DependencyInjections/IdentityDependencyInjection.cs` | alterar | Habilitar papéis (`AddRoles<IdentityRole<Guid>>`) se ainda não estiverem |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AdminController.cs` | alterar | `[Authorize(Roles = "Administrador")]` na classe; POST vira `async Task<IActionResult>` com `[ValidateAntiForgeryToken]`, guarda de `ModelState` e `RedirectToAction` no sucesso |
| `Views/Admin/Cadastro.cshtml` | alterar | `asp-action="Cadastro"`; `select` de subcategoria alimentado por `SelectList`; remoção do campo Promoção; resumo de erros mantido |
| `Helpers/DbInitializer.cs` | alterar | Semear papel `Administrador`, um usuário administrador e as categorias/subcategorias iniciais |
| `wwwroot/css/pages/cadastro_produto.css` | criar | A view já referencia este arquivo, que ainda não existe |

## 4. Contratos

```csharp
// Application/Contracts/Services/IProdutoService.cs  (alterado)
Task<ProdutoDTO> Cadastrar(ProdutoDTO dto);   // agora devolve o DTO da entidade criada

// Application/Contracts/Services/ISubcategoriaService.cs  (novo)
public interface ISubcategoriaService
{
    Task<List<SubcategoriaDTO>> BuscarTodasSubcategorias();
}

// Application/Contracts/Repositories/ISubcategoriaRepository.cs  (novo)
public interface ISubcategoriaRepository : IRepository<Subcategoria>
{
}
```

## 5. Modelo de dados

**`Categoria`** — `CategoriaId` (Guid, PK), `Nome` (nvarchar(100), obrigatório).

**`Subcategoria`** — `SubcategoriaId` (Guid, PK), `CategoriaId` (Guid, FK
obrigatória), `Nome` (nvarchar(100), obrigatório).

**`Produto`** — sem mudança de coluna; ganha a FK real
`SubcategoriaId → Subcategoria.SubcategoriaId`, hoje ausente do mapeamento.

Exclusão: `Restrict` em ambas as FKs, conforme `no action` no
[`ModelagemBancoTCC.dbml`](../../ModelagemBancoTCC.dbml).

**Migration:**
```
dotnet ef migrations add AddCategoriaSubcategoria --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC
```

**Impacto em dados existentes:** os produtos da massa inicial hoje têm
`SubcategoriaId` apontando para nada. A migration precisa rodar **depois** de as
subcategorias existirem, ou a massa inicial deve ser recriada. Para o TCC, o
caminho mais simples é recriar o banco de desenvolvimento.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `Units/Entities/SubcategoriaTests.cs` | nome obrigatório, categoria obrigatória |
| Unidade — validator | `Units/Validators/ProdutoDTOValidatorTests.cs` | RN-01 a RN-04, caso válido e inválido por regra |
| Unidade — serviço | `Units/Services/ProdutoServiceTests.cs` | `Cadastrar` chama `Adicionar` **e** `Commit`; devolve DTO com `Id` preenchido |
| Unidade — controller | `Units/Controllers/AdminControllerTests.cs` | `ModelState` inválido devolve `ViewResult` e não chama o serviço; válido redireciona |
| Integração | `Integration/Repositories/ProdutoRepositoryIntegrationTests.cs` | produto gravado é relido do SQLite após commit |

| Critério de aceite | Teste que o prova |
|---|---|
| CA-01 | `Dado_ProdutoValido_Quando_Cadastro_Entao_DeveRedirecionarComMensagemDeSucesso` |
| CA-02 | `Dado_ProdutoAdicionado_Quando_Commit_Entao_DeveEstarPersistidoNoBanco` |
| CA-03 | `Dado_NomeComMenosDeTresCaracteres_Quando_Validar_Entao_DeveRetornarErro` |
| CA-04 | `Dado_PrecoZero_Quando_Validar_Entao_DeveRetornarErro` |
| CA-05 | `Dado_ImagemSemEsquemaHttp_Quando_Validar_Entao_DeveRetornarErro` |
| CA-06, CA-07 | Verificação manual (T027) — teste de autorização exigiria host de integração, fora do escopo |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Fazer `SaveChanges` dentro do `Repository.Adicionar` | Quebra o Princípio VI: cada operação viraria sua própria transação, impossibilitando casos de uso que escrevem em mais de um agregado (pedido + itens + pagamento) |
| Deixar subcategoria como campo `Guid` digitado à mão | Cumpre a letra do RF-01 e viola o RF-07; nenhum administrador digita GUID |
| Validar produto só no domínio, sem `ProdutoDTOValidator` | Viola o Princípio III: o usuário receberia a tela de erro do `FilterException` em vez de mensagem no campo |
| Proteger a rota com checagem manual de usuário no controller | `[Authorize(Roles = ...)]` é o mecanismo do framework; checagem manual é fácil de esquecer na próxima ação |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| A FK nova de `Produto.SubcategoriaId` quebra a massa inicial existente | Alta | Médio | Semear categorias/subcategorias antes dos produtos no `DbInitializer`; recriar o banco de desenvolvimento |
| Papéis do Identity ainda não configurados exigem migration adicional | Média | Médio | Verificar `IdentityDependencyInjection` na T001 antes de planejar a migration |
| A alteração de retorno de `Cadastrar` quebra `ProdutoServiceTests` existentes | Média | Baixo | Os testes atuais cobrem só `BuscarTodos` e `BuscarPorId`; ajustar se necessário |
| Credencial do administrador semeado vazar para produção | Baixa | Alto | Semear apenas fora de produção; senha vinda de *user secret*, nunca literal no código |

## 9. Desvios constitucionais justificados

Nenhum.
