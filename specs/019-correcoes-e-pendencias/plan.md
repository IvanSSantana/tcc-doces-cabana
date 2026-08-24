# Plano Técnico — Correções e pendências

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-24
**Status:** Rascunho

---

## 1. Resumo da abordagem

**Esta feature não constrói nada novo — ela conserta e reaproveita.** Não há
entidade nova, tabela nova, migration nem serviço novo. Há um método de serviço,
uma função extraída, um controlador que passa a ler a claim que já existe, e
documentação.

**O bug do CPF é de duplicação, não de aritmética.** O cálculo do dígito
verificador está escrito duas vezes no mesmo método, com variáveis
reaproveitadas entre as duas — e foi exatamente essa reescrita que fez o
primeiro dígito calculado sobrescrever o digitado. Extrair `CalcularDigito` e
chamá-la duas vezes elimina a **classe** do erro, não só a instância. Consertar
comparando `digitos[9]` sem extrair a função deixaria a mesma armadilha montada
para a próxima alteração.

**Os dois defeitos da home são um só caminho de código.** `HomeController.Index`
chama `BuscarTodosProdutos`, que materializa os cem produtos e mapeia pela
sobrecarga de `ProdutoMapper.ToDTO` **sem** favoritos. Trazer tudo e não marcar
favorito são o mesmo `return`. Um método novo conserta os dois de uma vez, e
seria artificial separá-los em tarefas distintas.

**A consulta da vitrine já existe.** `IProdutoRepository.BuscarPaginaDoCatalogo`
já exclui inativo no `IQueryable` (RN-01 da `012`), já ordena por avaliação com
desempate por nome, e já faz `Skip`/`Take` que o EF traduz para `LIMIT`/`OFFSET`.
"Os oito primeiros" e "a primeira página de tamanho oito" são a mesma consulta —
reusá-la evita um método de repositório novo que faria o mesmo com outro nome, e
herda a RN-01 em vez de reescrevê-la.

**`BuscarTodosProdutos` não é removido.** Continua servindo `Admin/Produto`. O
que muda é que a home deixa de ser cliente dele.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` nova. `CpfHelper` segue no `Domain`; o método novo, na `Application`; nada novo na `Infrastructure` |
| II | Domínio rico e auto-validante | ⬜ OK | Nenhuma entidade nova. `CpfHelper` é helper estático de domínio, sem estado — a invariante que ele protege (`Usuario.ValidarCPF`) fica mais forte, não mais fraca |
| III | Validação nas duas barreiras | ⬜ OK | `CadastroDTOValidator` já chama `CpfHelper.CpfValido`, e `Usuario` também — corrigir o helper conserta **as duas** barreiras de uma vez, que é o argumento a favor de a regra viver num só lugar |
| IV | Nomenclatura em português | ⬜ OK | `CalcularDigito`, `BuscarDestaquesDaVitrine` |
| V | Testes escritos antes | ⬜ OK | Cada fase tem fase vermelha própria. Os testes de CPF são regressão pura: falham hoje, passam depois |
| VI | Repositório + commit via UnitOfWork | ⬜ OK (não se aplica) | Esta feature não escreve no banco. Nenhuma migration |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK (não se aplica) | Nenhuma ação nova, nenhum POST novo. `Home.Index` continua `GET` anônimo |
| VIII | Tratamento de erro por camada | ⬜ OK | Nenhum caminho de erro novo. CPF inválido continua virando `ArgumentException` no domínio e mensagem de campo no validator |

## 3. Direção visual

**Nenhuma cor, fonte, componente ou arquivo de estilo novo.** A única mudança
visível é o texto do título da vitrine.

```
ANTES                              DEPOIS
┌──────────────────────────┐       ┌──────────────────────────┐
│  Conheça a loja          │       │  Bem avaliados           │
│  ┌────┐┌────┐┌────┐┌────┐│       │  ┌────┐┌────┐┌────┐┌────┐│
│  │ ?  ││ ?  ││ ?  ││ ?  ││       │  │★4.8││★4.7││★4.5││★4.4││
│  └────┘└────┘└────┘└────┘│       │  └────┘└────┘└────┘└────┘│
│  ordem do banco, coração │       │  por avaliação, coração  │
│  sempre vazio            │       │  fiel ao favorito real   │
└──────────────────────────┘       └──────────────────────────┘
```

O cartão em si não muda — é o mesmo `CardProduto` do catálogo, e o coração já
sabe se desenhar cheio quando `EstaFavorito` é verdadeiro. O que muda é que o
valor passa a chegar preenchido.

**O título vira "Bem avaliados"** (RF-08/RN-04). "Conheça a loja" não anuncia
critério nenhum, e a intenção do responsável era "Mais vendidos" — indisponível
até haver venda registrada (spec §10).

## 4. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/CpfHelper.cs` | alterar | Extrair `CalcularDigito`; conferir **os dois** dígitos contra os informados |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Contracts/Services/IProdutoService.cs` | alterar | Assinatura nova — ver §5 |
| `Services/ProdutoService.cs` | alterar | Implementação de `BuscarDestaquesDaVitrine`; passa a depender de `IFavoritoRepository` |

`BuscarTodosProdutos` **não muda** — continua servindo a area administrativa.

### `DocesCabana.Infrastructure`

Nada. A consulta necessária já existe.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/HomeController.cs` | alterar | `Index` chama o método novo com a claim; `UsuarioAtualId` copiado de `CatalogoController` |
| `Views/Home/Index.cshtml` | alterar | Título da seção |
| `Views/Shared/Components/EstrelasNota/Default.cshtml` | alterar | Comentário que se contradiz (RF-13) |

`VitrineProdutosViewComponent` **não muda** — ver §8.

### Documentação

| Arquivo | Ação | O quê |
|---|---|---|
| `docs/arquitetura.md` | alterar | §9.1 (achados resolvidos), §9.3 (telas e tabelas de hoje), §2.1/§5 se houver defasagem |
| `specs/README.md` | alterar | Cadeia renumerada; backlog |

### Testes

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Helpers/CpfHelperTests.cs` | alterar | Casos do primeiro dígito e guarda dos CPFs semeados. Os casos inválidos de hoje corrompem só o segundo dígito |
| `Units/Validators/CadastroDTOValidatorTests.cs` | alterar | Caso do primeiro dígito errado, na barreira de entrada |
| `Units/Services/ProdutoServiceTests.cs` | alterar | `BuscarDestaquesDaVitrine` |
| `Units/Controllers/HomeControllerTests.cs` | **reescrever um teste** | `Dado_ProdutosCadastrados_Quando_Index_...` afirma `Verify(s => s.BuscarTodosProdutos(), Times.Once)` — quebra de propósito. Ver §9 |
| `E2E/Fluxos/PaginaInicialTests.cs` | alterar | Favorito na vitrine, título, ordenação |

## 5. Contratos

```csharp
// ── Domínio ────────────────────────────────────────────────────────────
public static class CpfHelper
{
    // Extraída: o cálculo existia duplicado dentro de
    // DigitoVerificadorValido, com estado compartilhado entre as duas
    // cópias — a causa raiz do defeito.
    private static int CalcularDigito(string parcial, int[] multiplicadores);
}
```

O algoritmo, explícito:

```
digitos = só os dígitos do que foi informado
se comprimento != 11            → inválido
se todos os dígitos iguais      → inválido

d1 = CalcularDigito(digitos[0..9],  [10,9,8,7,6,5,4,3,2])
se digitos[9]  != d1            → inválido      ← a conferência que faltava

d2 = CalcularDigito(digitos[0..10], [11,10,9,8,7,6,5,4,3,2])
devolve digitos[10] == d2
```

`CalcularDigito` é o cálculo padrão de módulo 11: soma dos dígitos pelos pesos,
resto da divisão por 11, e `resto < 2 ? 0 : 11 - resto`.

**A diferença exata em relação a hoje** é a linha `se digitos[9] != d1`. Hoje o
código faz `parcial += d1` — concatena o dígito **calculado** — e segue, de modo
que o dígito **informado** na posição 9 nunca é lido.

```csharp
// ── Aplicação ──────────────────────────────────────────────────────────
public interface IProdutoService
{
    /// <summary>Os produtos em destaque da página inicial: melhor avaliados
    /// primeiro, já marcados com o favorito de quem vê (null = visitante).</summary>
    Task<List<ProdutoDTO>> BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null);
}
```

Implementação, e por que cada linha:

```csharp
public async Task<List<ProdutoDTO>> BuscarDestaquesDaVitrine(int limite, Guid? usuarioId = null)
{
    // Filtro vazio = catálogo inteiro. ConstruirConsulta já aplica
    // Status != Inativo no IQueryable (RN-01 da 012), então a exclusão
    // acontece no SQL, não em memória.
    var filtro = new FiltroCatalogoDTO(
        CategoriaId: null, SubcategoriaIds: [], ApenasSemAcucar: false,
        Ordenacao: OrdenacaoCatalogo.MelhorAvaliados, TermoNormalizado: null);

    // pagina: 1 + tamanhoDaPagina: limite → Skip(0).Take(limite) →
    // LIMIT {limite} OFFSET 0. O banco devolve exatamente `limite` linhas.
    var produtos = await _produtoRepository.BuscarPaginaDoCatalogo(filtro, pagina: 1, tamanhoDaPagina: limite);

    // Visitante não tem favorito: nem consultamos (mesma decisão de
    // CatalogoService.Montar).
    var idsFavoritados = usuarioId.HasValue
        ? await _favoritoRepository.IdsPorUsuario(usuarioId.Value, produtos.Select(p => p.ProdutoId))
        : [];

    return ProdutoMapper.ToDTO(produtos, idsFavoritados);
}
```

Custo: **uma** consulta de `limite` linhas para visitante, **duas** para
autenticado (a segunda é um `WHERE ProdutoId IN (…)` sobre no máximo `limite`
identificadores). Contra uma consulta de cem linhas e noventa e nove DTOs hoje.

```csharp
// ── MVC ────────────────────────────────────────────────────────────────
public async Task<IActionResult> Index()
{
    var produtos = await _produtoService.BuscarDestaquesDaVitrine(
        VitrineProdutosViewComponent.LimitePadrao, UsuarioAtualId);
    return View(produtos);
}
```

`LimitePadrao` vira constante pública do componente (hoje é `= 8` no parâmetro).
O controlador precisa pedir ao banco a mesma quantidade que o componente vai
exibir — sem a constante compartilhada, os dois números podem divergir em
silêncio, e o defeito volta de forma mais sutil que a atual.

## 6. Modelo de dados

**Nenhuma mudança.** Sem entidade nova, sem coluna nova, sem migration, sem
alteração no `ModelagemBancoTCC.dbml`.

É a primeira feature desde a `009` inteiramente sem esquema — e vale registrar
que a consulta que a vitrine passa a usar não é nova: é a mesma que o catálogo
executa a cada filtro.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — helper | `CpfHelperTests` | RN-01: os dois dígitos conferidos. É o arquivo que faltava — hoje o helper só é testado de raspão, pelo validator |
| Unidade — validator | `CadastroDTOValidatorTests` | A correção chega à barreira de entrada, não só ao domínio |
| Unidade — serviço | `ProdutoServiceTests` | Pede `limite` ao repositório; consulta favorito só quando autenticado; marca os certos |
| Unidade — controller | `HomeControllerTests` | Passa a claim quando autenticado, `null` quando visitante |
| E2E | `PaginaInicialTests` | Favorito que sobrevive à recarga, título, e a ordenação vista pela tela |

**O teste que faltava e que teria pego o bug do CPF.** Os testes existentes só
corrompem o **segundo** dígito (`529.982.247-26`) — justamente o caso que o
código pega. Por isso nunca falharam. Os casos novos corrompem o **primeiro**:

| CPF | Dígitos corretos | Hoje | Depois |
|---|---|---|---|
| `52998224795` | `25` | ✅ passa (bug) | ❌ recusado |
| `52998224705` | `25` | ✅ passa (bug) | ❌ recusado |
| `52998224715` | `25` | ✅ passa (bug) | ❌ recusado |
| `52998224726` | `25` | ❌ recusado | ❌ recusado |
| `52998224725` | `25` | ✅ aceito | ✅ aceito |

Mais um teste de guarda que **percorre os nove CPFs semeados** (`DbInitializer`
+ administrador) e exige que todos passem. Os nove foram conferidos ao
especificar e estão corretos; o teste existe para que uma conta de demonstração
nova, com CPF inventado, quebre no `dotnet test` e não na subida da aplicação.

Mapeamento critério → teste:

| Critério | Teste |
|---|---|
| CA-01 | `Dado_CpfComPrimeiroDigitoVerificadorErrado_Quando_Validar_Entao_DeveRecusar` |
| CA-02 | `Dado_CpfComSegundoDigitoVerificadorErrado_Quando_Validar_Entao_DeveRecusar` |
| CA-03 | `Dado_CpfValido_Quando_Validar_Entao_DeveAceitar` |
| CA-04 | `Dado_CpfComDigitosRepetidos_Quando_Validar_Entao_DeveRecusar` |
| CA-05 | `Dado_OsCpfsSemeados_Quando_Validar_Entao_TodosDevemSerValidos` |
| CA-06 | `Dado_UmLimite_Quando_BuscarDestaquesDaVitrine_Entao_DevePedirApenasOLimiteAoRepositorio` |
| CA-07 | `Dado_ProdutosComAvaliacoesDiferentes_Quando_AbrirAPaginaInicial_Entao_OsMelhoresAvaliadosDevemVirPrimeiro` |
| CA-08 | `Dado_ProdutoSemAvaliacao_Quando_BuscarDestaquesDaVitrine_Entao_DeveVirDepoisDosAvaliados` |
| CA-09 | `Dado_ProdutoInativo_Quando_AbrirAPaginaInicial_Entao_NaoDeveAparecerNaVitrine` |
| CA-10 | `Dado_PaginaInicial_Quando_OlharOTituloDaVitrine_Entao_DeveAnunciarOCriterio` |
| CA-11 | `Dado_ProdutoFavoritadoNaVitrine_Quando_RecarregarAPaginaInicial_Entao_DeveContinuarMarcado` |
| CA-12 | `Dado_Visitante_Quando_BuscarDestaquesDaVitrine_Entao_NaoDeveConsultarFavoritos` |
| CA-13, CA-14 | Verificação documental — sem teste automatizado; conferido no checklist |

**CA-07 e CA-09 dependem do seed.** O `DbInitializer` gera avaliações em rodízio
e marca produtos inativos deterministicamente. O teste E2E lê a vitrine e
confere a ordem relativa das notas exibidas, em vez de fixar nomes de produto —
que mudariam se o seed fosse regerado.

**CA-06 é teste de unidade com `Moq`, não E2E.** "Pediu oito, não cem" é
afirmação sobre a chamada ao repositório: `Verify(r => r.BuscarPaginaDoCatalogo(
It.IsAny<FiltroCatalogoDTO>(), 1, 8), Times.Once)`. Nenhum teste de navegador
consegue observar isso.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Consertar o CPF comparando `digitos[9]` sem extrair `CalcularDigito` | Uma linha a menos, e deixa montada a mesma armadilha que causou o defeito: duas cópias do cálculo compartilhando variáveis |
| Método de repositório novo (`BuscarDestaques`) | Faria exatamente o que `BuscarPaginaDoCatalogo` já faz, com outro nome — e precisaria repetir a exclusão de inativo, que é onde a RN-01 poderia se perder |
| Reusar `ICatalogoService.Montar` na home | Traz taxonomia, contagem de páginas e filtros que a home não usa, e `TamanhoDaPagina` é constante `12`, não `8` |
| Tirar o `.Take(limite)` do componente | A consulta passa a limitar, mas o corte no componente é contrato dele desde a `013` ("quem usa a vitrine herda o corte"), tem teste próprio, e vira rede de segurança de graça |
| Ordenar a vitrine por `MaisVendidos` agora | Sem venda registrada, todo produto empata em zero e o desempate por nome assume: sai ordem alfabética com uma subconsulta cara e um título que mente (RN-04). Ver spec §10 |
| Ordenar por `NomeAZ` | Determinístico, mas não é critério — é o alfabeto. A vitrine mostraria sempre os mesmos oito |
| Ordenar aleatoriamente | Quebra a asserção de ordem do E2E e não é reproduzível ao demonstrar o TCC |
| Reauditar o `arquitetura.md` inteiro | Novecentas e oitenta e quatro linhas; releitura completa é entrega própria. Aqui se corrige o que envelheceu e o que esta entrega muda (spec §8) |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Corrigir o CPF quebra o seed e a aplicação não sobe** — `Usuario` valida no construtor | Baixa | **Alto** | Os nove CPFs semeados foram conferidos dígito a dígito **antes** de especificar: todos válidos. O teste de guarda (CA-05) impede que um seed futuro reintroduza o problema |
| **Corrigir o CPF quebra os testes E2E** — cadastro é o caminho de entrada de quase toda suíte | Baixa | Alto | `GeradorDeDados.CpfValido` calcula os dois dígitos corretamente, conferido ao especificar |
| **`HomeControllerTests` quebra ao trocar o método do serviço** — o teste atual afirma `BuscarTodosProdutos` chamado uma vez | Alta | Baixo | Quebra esperada, não regressão. O teste é **reescrito**, não removido: passa a provar que a home pede os destaques com a claim de quem vê. Mesmo tratamento que a `017` deu ao teste dos controles do cartão e a `018` ao do atalho "Conta" |
| **A ordenação por avaliação muda quais produtos aparecem**, e um teste E2E existente depende dos atuais | Média | Médio | A suíte inteira roda no fim da fase; qualquer teste que fixe produto da home é ajustado junto, e passa a ler a ordem relativa em vez do nome |
| **A constante de limite diverge entre controlador e componente** | Média | Baixo | O limite vira constante pública do componente, e o controlador a usa — não há dois números para sincronizar |
| **`MelhorAvaliados` sobre cem produtos é subconsulta por linha** | Baixa | Baixo | É a mesma consulta que o catálogo já executa a cada filtro, com `LIMIT 8`. Se um dia doer, dói no catálogo primeiro |
| **A renumeração da cadeia deixa referência obsoleta** — aconteceu nas cinco vezes anteriores | Alta | Baixo | Tarefa própria varrendo `spec 0NN`, incluindo esta spec e este plano |

## 10. Desvios constitucionais justificados

*Nenhum.*

Esta feature não cria entidade (II não se aplica além do que já vale), não
escreve no banco (VI não se aplica), não acrescenta ação de controlador (VII não
se aplica) e não cria caminho de erro novo (VIII inalterado). O Princípio III
sai **reforçado**: a regra do CPF vive num helper único que as duas barreiras
consomem, então a correção conserta as duas simultaneamente — que é o argumento
prático a favor de a regra não estar duplicada.

---

## Sobre a cadeia da loja

Esta entrega desloca a cadeia pela sexta vez. Depois dela:

| # | Entrega | O que traz |
|---|---|---|
| `019` | Correções e pendências | esta |
| `020` | Fechamento de pedido e frete | `Pedido`, `ItemPedido`, `Pagamento`, MelhorEnvio, `Produto` com peso e dimensões |
| `021` | Avaliação, promoções, favorito e sugestões | CRUD de avaliação, promoções na vitrine, favoritar da página do produto, sugestões na busca |
| `022` | Estoque | substitui o `ProdutoStatus.ForaDeEstoque` marcado à mão |

**Duas coisas ficam explicitamente para a `020`:** trocar a ordenação da vitrine
para "mais vendidos" e o título junto (spec §10), e o `OrdenacaoCatalogo.MaisVendidos`
deixar de ser saneado pelo `CatalogoController` — as duas dependem de venda
registrada, que é o que a `020` cria.

**E uma para a `021`:** o CRUD de avaliação precisa decidir se exige pedido
fechado para avaliar. Se exigir, depende da `020`; se não, é independente.
Decisão do responsável ao especificar.
