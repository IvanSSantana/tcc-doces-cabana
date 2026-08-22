# Plano Técnico — Busca e endereços do catálogo

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-22
**Status:** Executado

---

## 1. Resumo da abordagem

**A busca entra como mais um filtro do catálogo, não como caminho paralelo.**
`ConstruirConsulta` no repositório já compõe categoria, subcategoria e "sem
açúcar" num só lugar; o termo vira mais um `Where` ali dentro. Nenhuma rota
nova, nenhuma tela nova, nenhuma segunda regra de ordenação ou paginação — e o
`catalogo.js` da `014` continua funcionando sem uma linha alterada, porque ele
monta o endereço a partir do próprio formulário.

**O acento é resolvido no dado, não na consulta.** `Produto` ganha
`NomeNormalizado`, derivado do próprio nome no construtor e em `AlterarNome`.
A busca normaliza o termo do mesmo jeito e compara os dois lados já
normalizados. Isso resolve acento **e caixa** de uma vez: no SQLite, `Contains`
é traduzido para `instr`, que é sensível a maiúsculas — sem a coluna, "Brigadeiro"
não seria encontrado por "brigadeiro", muito menos "Café" por "cafe".

**O normalizador de acento desce para o `Domain`.** Hoje ele vive dentro de
`Application/Servicos/Apelido.cs`, e quem precisa dele agora é a entidade
`Produto`, que não pode enxergar a `Application`. Vira `Domain/Helpers/TextoHelper.cs`
— só BCL, ao lado de `CepHelper` e `CpfHelper` — e o `Apelido` passa a consumi-lo.
É extração, não reescrita: o comportamento é o mesmo, e os testes que já cobrem
`Apelido.De` continuam valendo sem mudança.

**O endereço legível separa o que foi pedido do que é consultado.** Nasce
`CriteriosDoCatalogoDTO`, que carrega o que veio da URL no vocabulário de quem
lê — apelido de categoria, apelidos de subcategoria, termo. `CatalogoService`
resolve isso contra a taxonomia que ele já carrega em toda requisição e produz
o `FiltroCatalogoDTO` de sempre, com Guids, para o repositório. **O repositório
não muda de contrato**: ele continua recebendo identificadores, sem saber que
apelido existe.

**A extração do CSS de formulário é movimentação, não redesenho.** As regras
compartilhadas saem de `pages/autenticacao.css` para `components/formulario.css`
sem alteração de valor; as seis telas que as usam passam a linkar o componente.
Aí a tela de cadastro de produto ganha o contêiner e o título que lhe faltavam,
mais o arranjo de duas colunas nos campos curtos.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` tocada. `TextoHelper` desce para `Domain` usando só `System.Globalization`/`System.Text` (BCL); `Apelido` fica na `Application` e passa a consumi-lo — a seta aponta para dentro |
| II | Domínio rico e auto-validante | ⬜ OK | `NomeNormalizado` tem `private set` e é derivado do próprio nome nos dois únicos pontos que o alteram (construtor e `AlterarNome`). Ninguém de fora atribui, e ele não pode divergir |
| III | Validação nas duas barreiras | ⬜ OK (parcial) | O termo é o único dado digitado desta feature, e não é persistido nem valida nada: é recorte de consulta. Ver §10 |
| IV | Nomenclatura em português | ⬜ OK | `TextoHelper.Normalizar`, `CriteriosDoCatalogoDTO`, `NomeNormalizado`, `EnderecoDoCatalogo`, `formulario.css` |
| V | Testes escritos antes | ⬜ OK | Cada fase tem sua fase vermelha própria; a extração do CSS tem teste de não-regressão **antes** de a extração começar |
| VI | Repositório + commit via UnitOfWork | ⬜ OK | Busca é só leitura. A única escrita é o preenchimento retroativo de `NomeNormalizado`, que fecha com `IUnitOfWork.SalvarAlteracoes`. Migration `AddProdutoNomeNormalizado` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK / n/a | Nenhuma ação nova de escrita. A busca é `GET` porque é consulta — antiforgery e PRG não se aplicam. `Admin/Produto/Cadastro` não tem sua ação tocada, só a marcação |
| VIII | Tratamento de erro por camada | ⬜ OK | Categoria desconhecida segue lançando `KeyNotFoundException` na aplicação. Subcategoria irreconhecível **não** é erro (RN-04): é filtro descartado |

## 3. Direção visual

Três acréscimos, nenhuma cor nem fonte nova.

**Barra de pesquisa** — a caixa já existe e já tem o desenho certo. Ela ganha um
`<form>` em volta e o termo de volta dentro dela quando há busca ativa. Nada
muda visualmente em repouso.

**Etiqueta do termo, acima da grade**, ao lado da contagem que já está lá:

```
INÍCIO › TODOS › RESULTADOS PARA “BRIGADEIRO”

┌──────────────────┐
│ brigadeiro   ×   │   3 produtos          Ordenar por ▾
└──────────────────┘
```

A etiqueta usa `--cor-primaria` como borda e texto, o mesmo verde dos rótulos de
formulário. O `×` é um link, não um botão de script: ele aponta para o mesmo
endereço sem o termo, e por isso continua funcionando sem JavaScript (RF-10).
A trilha reaproveita o destaque que a `015` definiu — caixa alta com o último
item em laranja —, então o item de resultado nasce destacado sem regra nova.

**Cadastro de produto** — a tela passa a ser a irmã do cadastro de administrador:

```
HOJE                                DEPOIS
                                    ┌─────────────────────────────┐
Nome do Produto                     │  Cadastrar Produto          │
[____________________________]      │                             │
Preço                               │  Nome do Produto            │
[____________________________]      │  [_______________________]  │
Status                              │                             │
[____________________________]      │  Preço        Status        │
...                                 │  [_________]  [_________]   │
sem título, sem contenção,          │                             │
largura da janela inteira           │  ... largura contida        │
                                    └─────────────────────────────┘
```

Pares que dividem linha: **Preço + Status** e **Subcategoria + Sem açúcar**.
Nome, Imagem e Descrição ocupam a linha inteira. Abaixo de 768px tudo empilha,
que é o que `.linha-dupla` já faz no cadastro de administrador.

## 4. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/TextoHelper.cs` | **criar** | `Normalizar(texto)` — minúsculas, sem acento, aparado. Recebe o corpo que hoje está em `Apelido.RemoverAcentos` |
| `Entities/Produto.cs` | alterar | `NomeNormalizado` com `private set`, derivado no construtor e em `AlterarNome` |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Servicos/Apelido.cs` | alterar | Passa a chamar `TextoHelper.Normalizar`; a lógica de hífen fica |
| `DTOs/SubcategoriaDTO.cs` | alterar | Ganha `Apelido`, derivado como o da categoria |
| `DTOs/CriteriosDoCatalogoDTO.cs` | **criar** | O que veio do endereço: apelido da categoria, apelidos de subcategoria, termo, sem açúcar, ordenação |
| `DTOs/FiltroCatalogoDTO.cs` | alterar | Ganha `TermoNormalizado`; segue voltada ao repositório, com Guids |
| `DTOs/CatalogoDTO.cs` | alterar | `SubcategoriasMarcadas` passa a ser de apelidos; ganha `Termo` para a tela reexibir |
| `Mappings/CategoriaMapper.cs` | alterar | Preenche o apelido de cada subcategoria |
| `Contracts/Services/ICatalogoService.cs` | alterar | `Montar` recebe `CriteriosDoCatalogoDTO` |
| `Services/CatalogoService.cs` | alterar | Resolve apelidos de subcategoria contra a categoria atual, normaliza o termo, monta o `FiltroCatalogoDTO` |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/ProdutoRepository.cs` | alterar | Um `Where` a mais em `ConstruirConsulta`, sobre `NomeNormalizado` |
| `DatabaseContext/Configurations/ProdutoConfiguration.cs` | alterar | Mapeia `NomeNormalizado`: obrigatório, 255, padrão vazio |
| `Migrations/` | **criar** | `AddProdutoNomeNormalizado` |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/CatalogoController.cs` | alterar | Aceita `termo` e subcategorias por apelido; monta `CriteriosDoCatalogoDTO` |
| `Helpers/EnderecoDoCatalogo.cs` | **criar** | Monta o endereço do catálogo preservando categoria, subcategorias, sem açúcar, ordenação, termo e página |
| `Helpers/DbInitializer.cs` | alterar | Preenchimento retroativo de `NomeNormalizado` em base já existente |
| `ViewComponents/Header.cs` | alterar | Lê o termo vigente do endereço e o entrega à view |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | A caixa de pesquisa vira `<form method="get">` para o catálogo, com o termo reexibido |
| `Views/Catalogo/Index.cshtml` | alterar | Campo oculto do termo dentro do formulário; item de resultado na trilha |
| `Views/Catalogo/_BarraLateral.cshtml` | alterar | Caixas passam a valer apelido; links de categoria carregam o termo |
| `Views/Catalogo/_ResultadoCatalogo.cshtml` | alterar | Etiqueta do termo; mensagem de vazio própria para busca |
| `Views/Catalogo/_Paginacao.cshtml` | alterar | Passa a usar `EnderecoDoCatalogo` |
| `Areas/Admin/Views/Produto/Cadastro.cshtml` | alterar | Contêiner, título, pares em linha dupla, folha de estilo correta |
| `wwwroot/css/components/formulario.css` | **criar** | Regras compartilhadas de formulário, movidas sem alteração de valor |
| `wwwroot/css/pages/autenticacao.css` | alterar | Fica só com o que é da tela de login |
| `wwwroot/css/pages/cadastro_produto.css` | alterar | Arranjo próprio da tela; a regra da caixa de seleção continua |
| `wwwroot/css/pages/catalogo.css` | alterar | Etiqueta do termo |
| `wwwroot/css/components/header.css` | alterar | A caixa de pesquisa dentro de um formulário |

As outras cinco telas que hoje linkam `autenticacao.css` — `Autenticacao/Login`,
`Cadastro`, `EsqueceuSenha`, `RedefinirSenha` e `Admin/Administrador/Cadastro` —
passam a linkar também `components/formulario.css`. Nenhuma delas muda de
aparência (RF-18).

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Helpers/TextoHelperTests.cs` | **criar** | Acento, caixa e espaço das pontas |
| `Units/Entities/ProdutoTests.cs` | alterar | `NomeNormalizado` nasce do nome e acompanha `AlterarNome` |
| `Units/Servicos/ApelidoTests.cs` | alterar | Passa a incluir a unicidade dos apelidos de subcategoria **dentro de cada categoria** (RN-03) |
| `Units/Services/CatalogoServiceTests.cs` | alterar | Resolução de apelido para identificador; apelido desconhecido ignorado; termo normalizado repassado |
| `Units/Controllers/CatalogoControllerTests.cs` | alterar | Termo e apelidos chegam ao serviço |
| `Integration/Repositories/CatalogoRepositoryIntegrationTests.cs` | alterar | Busca por acento, por caixa, por trecho no meio e produto inativo fora |
| `E2E/Paginas/PaginaCatalogo.cs` | alterar | Etiqueta do termo, mensagem de vazio de busca |
| `E2E/Paginas/PaginaInicial.cs` | alterar | Barra de pesquisa do cabeçalho |
| `E2E/Paginas/PaginaCadastroProduto.cs` | alterar | Título e contêiner |
| `E2E/Fluxos/BuscaTests.cs` | **criar** | CA-01 a CA-11 |
| `E2E/Fluxos/CatalogoTests.cs` | alterar | CA-12 a CA-16 |
| `E2E/Fluxos/CadastroDeProdutoTests.cs` | alterar | CA-17 e CA-18 |
| `E2E/Fluxos/FormularioTests.cs` | **criar** | CA-19 — não-regressão do desenho das cinco telas de formulário |

## 5. Contratos

```csharp
namespace DocesCabana.Domain.Helpers;

public static class TextoHelper
{
    // "Café Especial " -> "cafe especial"
    public static string Normalizar(string texto);
}
```

```csharp
// O que o endereço pediu, no vocabulário de quem lê o endereço.
// Nada aqui é identificador: a tradução é trabalho do CatalogoService,
// que é o único que conhece a categoria atual e a taxonomia inteira.
public record CriteriosDoCatalogoDTO(
    string? ApelidoDaCategoria,
    IReadOnlyCollection<string> ApelidosDeSubcategoria,
    string? Termo,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao);

// O que a consulta precisa. Continua com Guids — o repositório não sabe
// que apelido existe, e não deve saber.
public record FiltroCatalogoDTO(
    Guid? CategoriaId,
    IReadOnlyCollection<Guid> SubcategoriaIds,
    bool ApenasSemAcucar,
    OrdenacaoCatalogo Ordenacao,
    string? TermoNormalizado);

Task<CatalogoDTO> Montar(CriteriosDoCatalogoDTO criterios, int pagina, Guid? usuarioId = null);
```

**Por que dois registros e não um.** A alternativa era um só, com um campo de
apelidos que o repositório precisasse lembrar de ignorar. Um campo que alguém
tem de lembrar de ignorar é um defeito esperando data. Dois registros com
públicos distintos tornam impossível passar apelido ao banco por descuido — e o
`FiltroCatalogoDTO` já era, desde a `012`, o registro voltado ao repositório,
com o comentário no topo dizendo exatamente isso.

## 6. Modelo de dados

- **Entidade:** `Produto` ganha `NomeNormalizado` — texto, obrigatório, 255,
  padrão `''`. Não é entrada: é derivado de `Nome` e mantido por ele.
- **Relacionamentos:** nenhum novo.
- **Migration:** `dotnet ef migrations add AddProdutoNomeNormalizado --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`
- **Impacto em dados existentes:** **precisa de preenchimento retroativo.** A
  coluna nasce vazia nas linhas que já existem, e nenhuma delas seria
  encontrável pela busca até ser preenchida.

**O preenchimento é feito em C#, não em SQL.** O SQLite não tem função para
remover acento — é justamente por isso que a coluna existe. A rotina roda no
`DbInitializer`, depois das migrations: carrega os produtos cujo
`NomeNormalizado` está vazio, chama `AlterarNome(produto.Nome)` — que recalcula
o derivado pelo mesmo caminho de sempre, sem API nova só para migração — e fecha
com `IUnitOfWork.SalvarAlteracoes`. É idempotente e não faz nada numa base
recém-criada, onde o construtor já preencheu tudo.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — helper | `TextoHelperTests` | Acento, caixa e espaço das pontas, incluindo os nomes reais da loja |
| Unidade — entidade | `ProdutoTests` | RN-02: o derivado nasce do nome e nunca diverge dele |
| Unidade — apelido | `ApelidoTests` | RN-03: dentro de cada categoria, os apelidos das subcategorias são distintos |
| Unidade — serviço | `CatalogoServiceTests` | Resolução de apelido, RN-04 (desconhecido ignorado), normalização do termo |
| Unidade — controller | `CatalogoControllerTests` | O que veio da URL chega ao serviço sem deformação |
| Integração | `CatalogoRepositoryIntegrationTests` | RF-03 e RN-06 contra SQLite de verdade — é o único nível que prova o `instr` |
| E2E | `BuscaTests` | O resto: só o navegador diz se o termo sobreviveu a paginar, filtrar e trocar de categoria |
| E2E | `CatalogoTests` | O endereço legível, inclusive o "Cappuccino" das duas categorias |
| E2E | `CadastroDeProdutoTests` | O desenho da tela e o comportamento em tela estreita |

Mapeamento critério → teste:

| Critério | Teste |
|---|---|
| CA-01 | `Dado_BarraDePesquisa_Quando_BuscarPeloNome_Entao_DeveMostrarOProduto` |
| CA-02 | `Dado_CatalogoDeUmaCategoria_Quando_BuscarProdutoDeOutra_Entao_DeveEncontrar` |
| CA-03 | `Dado_ProdutoComAcento_Quando_BuscarSemAcentoEEmOutraCaixa_Entao_DeveEncontrar` |
| CA-04 | `Dado_ResultadoDeBusca_Quando_OlharATela_Entao_DeveTerOrdenacaoPaginacaoEBarraLateral` |
| CA-05 | `Dado_BuscaFeita_Quando_OrdenarFiltrarEPaginar_Entao_OTermoDeveSobreviver` |
| CA-06 | `Dado_BuscaFeita_Quando_OlharABarraDePesquisa_Entao_DeveConterOTermo` |
| CA-07 | `Dado_BuscaComCategoriaEscolhida_Quando_DesfazerABusca_Entao_DeveManterACategoria` |
| CA-08 | `Dado_TermoSemResultado_Quando_OlharOResultado_Entao_DeveMencionarOTermoBuscado` |
| CA-09 | `Dado_BarraVazia_Quando_Submeter_Entao_DeveMostrarOCatalogoCompleto` |
| CA-10 | `Dado_JavaScriptDesligado_Quando_Buscar_Entao_DeveMostrarOResultado` |
| CA-11 | `Dado_ProdutoForaDoCatalogoPublico_Quando_BuscarPeloNomeExato_Entao_NaoDeveAparecer` |
| CA-12 | `Dado_Catalogo_Quando_MarcarSubcategoria_Entao_OEnderecoDeveConterONomeLegivel` |
| CA-13 | `Dado_DuasSubcategoriasMarcadas_Quando_OlharOEndereco_Entao_AmbasDevemAparecerPorNome` |
| CA-14 | `Dado_ApelidoDeSubcategoriaInexistente_Quando_AbrirOCatalogo_Entao_DeveMostrarACategoriaInteira` |
| CA-15 | `Dado_MesmoNomeEmDuasCategorias_Quando_FiltrarEmCadaUma_Entao_NaoDevemSeConfundir` |
| CA-16 | `Dado_MenuDoCabecalho_Quando_EscolherSubcategoria_Entao_OEnderecoDeveSerLegivel` |
| CA-17 | `Dado_Administrador_Quando_AbrirOCadastroDeProduto_Entao_DeveTerTituloEContencao` |
| CA-18 | `Dado_TelaEstreita_Quando_AbrirOCadastroDeProduto_Entao_NaoDeveTransbordar` |
| CA-19 | `Dado_TelasDeFormulario_Quando_CompararComOAnterior_Entao_DevemContinuarIguais` |

**CA-19 é escrito antes da extração do CSS, não depois.** É a única ordem que
faz o teste significar alguma coisa: escrito depois, ele confirmaria o resultado
da mudança em vez de guardar o estado anterior. Ele mede largura do campo, altura,
raio da borda e cor do rótulo nas quatro telas de autenticação e no cadastro de
administrador, com os valores lidos do navegador **antes** de qualquer arquivo
de CSS ser tocado.

**CA-15 é o teste que sustenta a decisão da RN-03.** Se um dia alguém tornar o
apelido global, ele quebra — que é exatamente o alarme que se quer.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Coluna `Apelido` em `Subcategoria`, com índice único | Não existe tela para criar ou renomear subcategoria: o valor só nasceria na carga inicial. E o índice teria de ser composto por categoria, porque "Cappuccino" existe em duas — o que é a `RN-03` escrita em DDL. Revisitar quando houver CRUD de subcategoria |
| Apelido de subcategoria único na loja inteira | Exigiria renomear "Cappuccino" numa das duas categorias, ou desempatar com sufixo. Troca um problema de endereço por uma decisão sobre o nome que a loja usa de verdade |
| Comparar o texto como está, sem coluna normalizada | No SQLite, `Contains` vira `instr`, que é sensível a caixa **e** a acento. "Brigadeiro" não seria encontrado por "brigadeiro". Falharia no primeiro uso |
| Registrar uma função de remoção de acento na conexão SQLite | O EF Core precisaria saber traduzi-la, e a suíte de integração em memória teria de registrá-la também. Mais peças móveis que a coluna, pelo mesmo alcance, e amarrado a um provider que o projeto planeja trocar |
| `COLLATE NOCASE` na coluna | Resolve caixa e não resolve acento — metade do problema, com uma decisão de esquema presa ao SQLite |
| Página `/Busca` própria | Duplicaria grade, ordenação, paginação, cartão e o caminho assíncrono que o catálogo já tem. Busca é recorte de catálogo, e a tela devia dizer isso |
| Sugestão enquanto digita | Exige endpoint próprio, atraso, navegação por teclado — e uma página de resultado por baixo de qualquer forma. Entrega própria, depois desta |
| Guardar o termo em sessão em vez de no endereço | Quebraria a `RN-05`: colar o endereço noutra aba mostraria outro resultado, e o botão voltar deixaria de funcionar |
| Um só registro de filtro, com apelidos e Guids juntos | O repositório teria de lembrar de ignorar um campo. Ver §5 |
| Deixar `autenticacao.css` como está e só linká-lo no cadastro de produto | Resolve a tela hoje e espalha o nome errado para uma terceira tela. O Princípio IV cobra que o nome diga o que a coisa é |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **A extração do CSS muda alguma tela sem querer** — uma regra deixada para trás, uma ordem de arquivo diferente | Média | Alto | CA-19 é escrito e verde **antes** da extração, medindo valores resolvidos no navegador em cinco telas. As regras são movidas literalmente, sem reescrita |
| **O preenchimento retroativo não roda na base do responsável**, e a busca não acha nada de produto antigo | Média | Alto | A rotina roda no `DbInitializer`, no mesmo ponto que já semeia; é idempotente; e um teste de integração prova que produto carregado com o derivado vazio passa a ser encontrável depois dela |
| **Trocar `subcategorias` de Guid para texto quebra teste existente** que constrói endereço à mão | Alta | Baixo | É quebra de compilação ou de asserção, não de produção. A varredura por `subcategorias=` faz parte das tarefas, e os testes da `012`/`014` que passam pela barra lateral não constroem endereço à mão |
| **O termo se perde num caminho esquecido** — um link de categoria, a paginação, o formulário assíncrono | Média | Médio | Todo endereço do catálogo passa a sair de `EnderecoDoCatalogo`; o formulário carrega o termo como campo oculto, então o `catalogo.js` o inclui sozinho ao serializar. CA-05 exercita os quatro caminhos |
| **Termo com caractere de curinga** (`%`, `_`) alterando a consulta | Baixa | Médio | `Contains` no SQLite vira `instr`, que é literal — não há curinga a escapar. Um teste de integração fixa esse comportamento, para que trocar de provider não o quebre em silêncio |
| **Termo absurdamente longo** vindo pela URL | Baixa | Baixo | Nome de produto tem 255; termo maior que isso não pode casar com nada e devolve "nada encontrado", que é a resposta correta. Ver §10 |
| **Renumeração da cadeia deixa referência obsoleta** — foi o que aconteceu nas duas primeiras vezes | Alta | Baixo | Tarefa própria, com varredura por `spec 0NN` na base inteira, incluindo a spec e o plano desta feature |
| **O apelido de subcategoria colide numa categoria** depois de alguém acrescentar uma nova | Baixa | Alto | O teste da `RN-03` percorre a taxonomia real e falha no dia em que isso acontecer, antes de virar endereço ambíguo em produção |

## 10. Desvios constitucionais justificados

**Princípio III — o termo de busca não tem validador de entrada.**

O princípio pede a regra nas duas barreiras: validator para proteger o usuário,
invariante para proteger o dado. O termo de busca não chega a nenhum dos dois
lados desse quadro. Ele não é persistido, não compõe entidade nenhuma e não tem
formato a respeitar — qualquer texto é um termo legítimo, inclusive vazio
(RF-09), inclusive sem resultado (RF-08). Não existe mensagem de campo a
devolver porque não existe campo a corrigir: a resposta a um termo ruim é uma
grade vazia com explicação, que é comportamento especificado, não erro.

A única fronteira que o termo cruza é a consulta, e ali ele é parâmetro, nunca
texto concatenado — o `Contains` do EF Core produz parâmetro ligado, e no SQLite
vira `instr`, que não interpreta curinga. Um validador de comprimento foi
considerado e descartado: nome de produto tem no máximo 255 caracteres, então um
termo maior simplesmente não casa com nada, e recusá-lo com mensagem seria
inventar um erro onde o sistema já responde corretamente.

A alternativa conforme — criar `BuscaDTO` com `BuscaDTOValidator` — foi
descartada por adicionar uma classe, um validador e um `ModelState` a um caminho
`GET` que não tem view de retorno com campo para marcar. O princípio existe para
que dado inválido não entre no sistema; aqui nada entra.
