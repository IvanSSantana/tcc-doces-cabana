# Plano Técnico — Área administrativa

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-18
**Status:** Rascunho

---

## 1. Resumo da abordagem

Uma *Area* do ASP.NET Core chamada `Admin`, que é o recurso que o próprio
framework oferece para agrupar telas sob um prefixo de rota. `CatalogoController`
e `AdministradorController` mudam para `Areas/Admin/Controllers/`, ganham
`[Area("Admin")]`, e suas views vão para `Areas/Admin/Views/`. O primeiro é
renomeado para `ProdutoController` no caminho (RQ-04): ele gerencia produto, e
"catálogo" precisa ficar livre para a `012`. As rotas passam a ser
`/Admin/Produto/Cadastro` e `/Admin/Administrador`; as antigas deixam de existir
e caem no 404 que a `008` já instalou. Uma rota de area é registrada em
`Program.cs` **antes** da rota padrão. Nenhuma lógica de controller muda —
corpo de ação, autorização e views são idênticos, só mudam de lugar e de nome.

Isso cria um `Areas.Admin.Controllers.ProdutoController` ao lado do
`Controllers.ProdutoController` público que já existe. Não é a colisão que a
constituição 1.4.0 proíbe: são audiências diferentes, e a area é exatamente o
qualificador que o framework oferece para distingui-las — `/Admin/Produto` e
`/Produto` nunca se confundem. O texto do Princípio IV ganha essa ressalva.

A parte não óbvia está nos links: dentro de uma area, o valor `area` entra nos
dados de rota ambientes, então um `asp-controller="Institucional"` num partial
compartilhado (rodapé, modal, cabeçalho) tentaria resolver para
`/Admin/Institucional/...`, que não existe. Todo link de tela de cliente nos
partials compartilhados recebe `asp-area=""` explícito para sair da area — é o
que RF-05 e CA-07 cobram.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` tocada; tudo dentro de `DocesCabana.MVC` e dos testes |
| II | Domínio rico e auto-validante | n/a | Nenhuma entidade tocada |
| III | Validação nas duas barreiras | n/a | Nenhuma validação nova ou alterada |
| IV | Nomenclatura em português | ⬜ OK | `Areas/Admin` é o nome que o framework impõe ao recurso (como `Controller`, `Views`), na mesma categoria dos termos que o Princípio IV já isenta. Controladores e views seguem em português |
| V | Testes escritos antes | ⬜ OK | Fase 2 ajusta os testes e os E2E para as rotas novas e roda vermelho antes de qualquer movimentação de arquivo |
| VI | Repositório + commit via UnitOfWork | n/a | Nenhuma persistência tocada |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK | `[Authorize(Roles = Papeis.Administrador)]` acompanha cada classe na mudança; RQ-03 e CA-04/CA-05 provam que nada foi perdido |
| VIII | Tratamento de erro por camada | ⬜ OK | Rotas antigas caem no `UseStatusCodePagesWithReExecute` existente; nenhum tratamento novo |

## 3. Impacto por camada

### `DocesCabana.Domain`, `DocesCabana.Application`, `DocesCabana.Infrastructure`

Nenhum arquivo.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/CatalogoController.cs` | mover e renomear → `Areas/Admin/Controllers/ProdutoController.cs` | Acrescenta `[Area("Admin")]`; classe vira `ProdutoController` (RQ-04); corpo idêntico |
| `Controllers/AdministradorController.cs` | mover → `Areas/Admin/Controllers/AdministradorController.cs` | Acrescenta `[Area("Admin")]`; corpo idêntico |
| `Views/Catalogo/Cadastro.cshtml` | mover → `Areas/Admin/Views/Produto/Cadastro.cshtml` | Conteúdo idêntico; a pasta acompanha o nome do controlador |
| `Views/Administrador/Index.cshtml` | mover → `Areas/Admin/Views/Administrador/Index.cshtml` | Conteúdo idêntico |
| `Views/Administrador/Cadastro.cshtml` | mover → `Areas/Admin/Views/Administrador/Cadastro.cshtml` | Conteúdo idêntico |
| `Areas/Admin/Views/_ViewStart.cshtml` | **criar** | `Layout = "_Layout"` — o `_ViewStart` da raiz **não** é executado para views de area |
| `Areas/Admin/Views/_ViewImports.cshtml` | **criar** | Cópia do `_ViewImports` da raiz — idem, não é herdado pela area. Sem ele, nenhum tag helper (`asp-for`, `asp-action`) funciona nas telas movidas |
| `Program.cs` | alterar | `MapControllerRoute` de area (`{area:exists}/{controller}/{action}/{id?}`) **antes** da rota padrão |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | `asp-area="Admin"` no atalho de administradores (RF-04); `asp-area=""` nos links de cliente (RF-05) |
| `Views/Shared/_Footer.cshtml` | alterar | `asp-area=""` nos links institucionais (RF-05, CA-07) |
| `Views/Shared/_ModalLogin.cshtml` | alterar | `asp-area=""` nos links de autenticação e política (RF-05) |

### `.specify/memory/constitution.md`

| Seção | Ação | O quê |
|---|---|---|
| Princípio IV | alterar (emenda PATCH) | Ressalva à regra de nome único que a 1.4.0 acabou de introduzir: o escopo de unicidade é a *area*, não a solução inteira. `Admin/Produto` e `/Produto` são telas de públicos distintos e o framework as separa por rota. Correção de alcance de uma regra existente, não regra nova — PATCH, ao contrário da 1.4.0 |

### `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Controllers/CatalogoControllerTests.cs` | mover e renomear → `Units/Controllers/Admin/ProdutoControllerTests.cs` | Acompanha o controlador; asserções inalteradas. A subpasta `Admin/` evita colidir com o teste do `ProdutoController` público, que já existe |
| `Units/Controllers/AdministradorControllerTests.cs` | mover → `Units/Controllers/Admin/AdministradorControllerTests.cs` | Idem |

### `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Paginas/PaginaCadastroProduto.cs` | alterar | `/Catalogo/Cadastro` → `/Admin/Catalogo/Cadastro` |
| `Paginas/PaginaAdministradores.cs` | alterar | `/Administrador` → `/Admin/Administrador` (e o `/Cadastro`) |
| `Fluxos/AreaAdministrativaTests.cs` | alterar | As 4 rotas; o teste de 404 da `010` passa a cobrir as duas rotas antigas (CA-03); teste novo para CA-07 |
| `Fluxos/LoginTests.cs` | alterar | A rota e o `ReturnUrl` esperado (`%2FAdmin%2FAdministrador`) |

## 4. Contratos

Nenhuma assinatura muda. As duas classes mantêm exatamente as ações que têm
hoje; só ganham o atributo de area e mudam de namespace.

```csharp
namespace DocesCabana.MVC.Areas.Admin.Controllers;

// Era Controllers/CatalogoController.cs (010). Renomeado por RQ-04.
[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class ProdutoController : Controller { /* idêntico */ }

[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class AdministradorController : Controller { /* idêntico */ }
```

## 5. Modelo de dados

Não se aplica.

## 6. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — controller | `Units/Controllers/CatalogoControllerTests.cs`, `AdministradorControllerTests.cs` | Que o comportamento das ações não mudou (RF-02) |
| E2E | `Fluxos/AreaAdministrativaTests.cs` | CA-01 a CA-07 |
| E2E | `Fluxos/LoginTests.cs` | Que o `ReturnUrl` continua levando de volta à tela pedida após o login |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `CadastroDeProdutoTests` existente, contra a rota nova (só o objeto de página muda) |
| CA-02 | `Dado_Administrador_Quando_CadastrarOutroAdministrador_Entao_ELeDeveEntrarEUsarAArea`, existente |
| CA-03 | `Dado_EnderecosAntigosDaAreaAdministrativa_Quando_Acessados_Entao_DevemResponder404` (estende o teste que a `010` criou) |
| CA-04 | `Dado_Visitante_Quando_AbrirAreaAdministrativa_Entao_DeveLevarAoLogin`, existente |
| CA-05 | `Dado_ClienteComum_Quando_AbrirAreaAdministrativa_Entao_DeveReceberAcessoNegado`, existente |
| CA-06 | `Dado_Administrador_Quando_UsarOAtalhoDoCabecalho_Entao_DeveChegarNaGestao` (novo) |
| CA-07 | `Dado_AdministradorNaAreaAdministrativa_Quando_ClicarNaPoliticaDoRodape_Entao_DeveSairDaArea` (novo) |

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Prefixo por atributo de rota (`[Route("Admin/[controller]")]`) em cada controller | Dá o mesmo endereço sem dar nada mais: continua sem pasta própria, sem `_ViewImports` próprio e sem um lugar óbvio para a próxima tela administrativa. Areas é o recurso que o framework tem para exatamente isto |
| Renomear os controladores de novo em vez de agrupar | Foi o que a `010` fez, e é o que criou a colisão. Renomear outra vez trata o sintoma; o problema é que telas de dois públicos dividem o mesmo espaço de nomes |
| Mover só o `CatalogoController` | Deixaria a área administrativa em dois lugares — o defeito que RQ-02 nomeia |
| Criar uma tela inicial para `/Admin` | Fora de escopo declarado. O prefixo não precisa de página própria para cumprir seu papel |

## 8. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Link de cliente quebrado dentro da area.** O valor `area` é ambiente: `asp-controller="Institucional"` a partir de uma tela `Admin` gera `/Admin/Institucional/...` ou href vazio | Alta | Alto | `asp-area=""` explícito em todo link de tela de cliente nos partials compartilhados; CA-07 é o teste que trava isso |
| **Views da area sem tag helper.** O `_ViewImports` da raiz não é herdado por areas — sem cópia, `asp-for`/`asp-action` viram atributo literal e o formulário para de funcionar em silêncio | Certa se esquecido | Alto | `Areas/Admin/Views/_ViewImports.cshtml` é tarefa própria (T0nn), e CA-01/CA-02 exercitam os dois formulários de ponta a ponta |
| **Views da area sem layout.** O `_ViewStart` da raiz também não é herdado — as telas renderizariam sem cabeçalho nem rodapé | Certa se esquecido | Médio | `Areas/Admin/Views/_ViewStart.cshtml` é tarefa própria; CA-06/CA-07 dependem do cabeçalho e do rodapé estarem lá |
| **Rota de area registrada depois da padrão.** A rota padrão casaria `/Admin/Catalogo` como controller `Admin`, ação `Catalogo` | Média | Alto | A rota de area vem antes, e CA-01 falha imediatamente se a ordem estiver trocada |
| **`ReturnUrl` do login** passa a conter o prefixo, quebrando a asserção literal do `LoginTests` | Alta | Baixo | Já previsto no impacto; o teste é ajustado na Fase 2 |

## 9. Desvios constitucionais justificados

*Nenhum.*
