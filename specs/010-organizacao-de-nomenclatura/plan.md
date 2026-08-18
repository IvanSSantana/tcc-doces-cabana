# Plano Técnico — Organização de nomenclatura

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-18
**Status:** Rascunho

---

## 1. Resumo da abordagem

Duas correções mecânicas, sem lógica nova. A primeira: `AdminController`
(gerencia cadastro de produto desde a `001`) vira `CatalogoController` —
"catálogo" já é o termo que a spec `000-baseline` usa para essa área desde o
início do projeto, e não colide com "Administrador" (pessoa/papel/serviço,
termo fixado pela `004`). O nome de arquivo, classe, pasta de view e teste
mudam juntos; a rota resultante é `/Catalogo/Cadastro`. Nenhuma view
referencia `AdminController` pelo nome explicitamente — todo `asp-action`
existente resolve pelo controlador atual — então o único texto que muda fora
do C# são os dois endereços hardcoded dos testes E2E. A segunda: `_Carrossel`
e `_Categorias`, hoje em `Views/Shared/`, mudam para `Views/Home/`, que é o
único lugar que os usa — mesmo padrão que `Views/Produto/_BlocoAvaliacoes.cshtml`
e `Views/Institucional/_BlocoInstitucional.cshtml` já seguem. Por fim, uma
emenda **PATCH** ao Princípio IV da constituição registra por escrito a regra
que a base já pratica desde a `008`: tela parcial de uso único mora com o
controlador dono; `Views/Shared/` é só para o que é reaproveitado.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` tocada — mudança inteira dentro de `DocesCabana.MVC` e dos dois projetos de teste |
| II | Domínio rico e auto-validante | n/a | Nenhuma entidade tocada |
| III | Validação nas duas barreiras | n/a | Nenhuma regra de validação nova ou alterada |
| IV | Nomenclatura em português | ⬜ OK | É o princípio que esta feature inteira serve — RQ-01, RQ-02 e a emenda de RQ-03 |
| V | Testes escritos antes | ⬜ OK | Fase 2 renomeia e ajusta os testes existentes para o nome novo, roda vermelho pelo motivo certo (classe/rota inexistente), só então a Fase 3 aplica o `git mv` |
| VI | Repositório + commit via UnitOfWork | n/a | Nenhuma persistência tocada |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK | Nenhuma garantia existente é removida — `CatalogoController` herda exatamente o que `AdminController` já tinha (`[Authorize(Roles = Papeis.Administrador)]`, `[ValidateAntiForgeryToken]`, `await`, guarda de `ModelState`, redirecionamento no sucesso). Nada disso é reescrito, só movido |
| VIII | Tratamento de erro por camada | ⬜ OK | O endereço antigo passa a cair no `UseStatusCodePagesWithReExecute` que a `008` instalou — mesmo mecanismo, nenhum código novo de tratamento de erro |

## 3. Impacto por camada

### `DocesCabana.Domain`, `DocesCabana.Application`, `DocesCabana.Infrastructure`

Nenhum arquivo.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/AdminController.cs` | renomear → `Controllers/CatalogoController.cs` | Só o nome do arquivo e da classe (`AdminController` → `CatalogoController`); corpo idêntico |
| `Views/Admin/Cadastro.cshtml` | mover → `Views/Catalogo/Cadastro.cshtml` | Conteúdo idêntico — `asp-action="Cadastro"` já resolve pelo controlador atual, sem referência explícita ao nome antigo |
| `Views/Shared/_Carrossel.cshtml` | mover → `Views/Home/_Carrossel.cshtml` | Conteúdo idêntico |
| `Views/Shared/_Categorias.cshtml` | mover → `Views/Home/_Categorias.cshtml` | Conteúdo idêntico — `<partial name="...">` resolve pelo nome, sem caminho, então `Views/Home/Index.cshtml` não muda |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Controllers/AdminControllerTests.cs` | renomear → `Units/Controllers/CatalogoControllerTests.cs` | Classe `AdminControllerTests` → `CatalogoControllerTests`; `new AdminController(...)` → `new CatalogoController(...)`; asserts inalterados |

### `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Paginas/PaginaCadastroProduto.cs` | alterar | `$"{urlBase}/Admin/Cadastro"` → `$"{urlBase}/Catalogo/Cadastro"` |
| `Fluxos/AreaAdministrativaTests.cs` | alterar | As duas ocorrências de `$"{UrlBase}/Admin/Cadastro"` → `$"{UrlBase}/Catalogo/Cadastro"`; um teste novo prova CA-01 (endereço antigo devolve 404) |

### `.specify/memory/constitution.md`

| Seção | Ação | O quê |
|---|---|---|
| Princípio IV | alterar (emenda PATCH) | Acrescenta a regra de RQ-02: tela parcial de uso único mora com o controlador dono; `Views/Shared/` é reservado ao que é reaproveitado por mais de uma página. Registrada no histórico de emendas (Governança) |

## 4. Contratos

Nenhum. `CatalogoController` mantém a assinatura pública que `AdminController`
já tinha — mesmos dois `Cadastro` (GET e POST), mesmo `Error`.

```csharp
[Authorize(Roles = Papeis.Administrador)]
public class CatalogoController : Controller
{
    [HttpGet] public Task<IActionResult> Cadastro();
    [HttpPost][ValidateAntiForgeryToken] public Task<IActionResult> Cadastro(ProdutoDTO dto);
    public IActionResult Error();
}
```

## 5. Modelo de dados

Não se aplica.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — controller | `Units/Controllers/CatalogoControllerTests.cs` | Os três testes existentes continuam provando GET carrega subcategorias, POST inválido não chama o serviço, POST válido cadastra e redireciona — agora contra a classe renomeada |
| E2E | `Fluxos/AreaAdministrativaTests.cs` | CA-01 (endereço antigo → 404), CA-02 e CA-03 (o resto do fluxo administrativo, inalterado, continua verde contra o endereço novo) |

Mapeamento critério → teste:

| Critério de aceite | Teste que o prova |
|---|---|
| CA-01 | `Dado_EnderecoAntigoDeCadastroDeProduto_Quando_Acessado_Entao_DeveResponder404` (novo) |
| CA-02 | Suíte E2E existente de cadastro de produto (`CadastroDeProdutoTests`), sem alteração de asserção — só o endereço que `PaginaCadastroProduto.Abrir` usa muda |
| CA-03 | `Dado_Visitante_Quando_AbrirAreaAdministrativa_Entao_DeveLevarAoLogin` e `Dado_ClienteComum_Quando_AbrirAreaAdministrativa_Entao_DeveReceberAcessoNegado`, já existentes em `AreaAdministrativaTests.cs`, contra o endereço novo |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Renomear para `ProdutoAdminController` | Mantém "Admin" no nome, a raiz exata da ambiguidade que a feature existe para resolver |
| Renomear para `GestaoDeProdutoController` | "Catálogo" já é o termo que a `000-baseline` usa para esta área desde o primeiro dia do projeto — reaproveitar vocabulário existente bate com o Princípio IV, que pede um nome único por conceito em toda a base (RQ-04 da `002`) |
| Criar um redirecionamento 301 do endereço antigo para o novo | O endereço nunca foi divulgado a cliente nenhum — é rota interna de área administrativa acessada só por quem já sabe o endereço novo. Redirecionar manteria viva uma rota morta sem motivo (fora de escopo, spec §8) |
| Mover `_Carrossel`/`_Categorias` para dentro de `Views/Shared/Components` como `ViewComponent` | Nenhuma delas tem lógica própria (parâmetro, serviço injetado) que justifique a cerimônia de um `ViewComponent` — são HTML estático incluído por posição. `ViewComponent` é para quando a tela precisa computar o que mostra, não só para organizar arquivo |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| Algum lugar esquecido ainda referencia `/Admin/Cadastro` ou `AdminController` por texto | Baixa | Médio | Busca de texto (`grep`) confirmou exatamente 4 arquivos de código afetados antes de iniciar; T002 registra esse levantamento |
| `<partial name="_Carrossel">`/`<partial name="_Categorias">` deixarem de resolver após a mudança de pasta | Baixa | Alto (quebra a página inicial) | `<partial name="...">` do ASP.NET Core busca pelo nome em toda a árvore de `Views/`, não só na pasta do controlador atual — `Views/Home/_Carrossel.cshtml` é resolvido de `Views/Home/Index.cshtml` do mesmo jeito que `Views/Shared/_Carrossel.cshtml` era. Confirmado ao vivo em T0nn, não só por leitura de documentação |

## 9. Desvios constitucionais justificados

*Nenhum.*
