# Plano Técnico — Carrinho

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-23
**Status:** Rascunho

---

## 1. Resumo da abordagem

**A tabela é a irmã de `Favorito`, com uma coluna a mais.** `ItemCarrinho` tem
chave primária composta `(UsuarioId, ProdutoId)` — que é, ela própria, a
garantia da RN-01 no banco — e `Quantidade`. Nenhuma coluna de preço (RN-04),
nenhuma tabela-cabeçalho `Carrinho`: uma pessoa tem exatamente um carrinho, e um
cabeçalho sem estado próprio só acrescentaria uma junção a toda consulta.

**"Disponível para compra" vira conceito de domínio.** A RN-06 diz que produto
inativo e produto fora de estoque são igualmente incompráveis, e que só a
mensagem os distingue. Isso é invariante, não regra de tela: `Produto` ganha
`DisponivelParaCompra()`, e é ele quem o serviço consulta. Método, não
propriedade — propriedade computada o EF Core tentaria mapear para coluna.

**Dois armazenamentos, um conjunto de regras.** O carrinho do visitante vive na
sessão (decisão da spec §10) e o do cliente no banco. Para as regras não
existirem em dois lugares, `ICarrinhoService` oferece as operações do visitante
como **transformações sobre uma lista** que o chamador guarda onde quiser: o
serviço valida, aplica RN-01 e RN-02, devolve a lista nova, e a `MVC` só faz a
leitura e a escrita da sessão. Nenhuma regra de negócio mora na `MVC`.

**A junção mora num filtro, não no login.** Um filtro global vê toda requisição
autenticada; se houver carrinho pendente na sessão, funde e limpa. Assim
funciona por qualquer caminho de entrada — modal, página de login, retorno de
`returnUrl` — sem o `AutenticacaoController` conhecer o domínio de carrinho, do
mesmo jeito que a `015` evitou acoplá-lo a favoritos.

**A tela reaproveita a máquina da `014`/`015` inteira.** Botão associado por
`form=` a um formulário no layout, POST com antiforgery, redirecionamento no
caminho comum e `fetch` com troca de bloco no caminho com script. Nada de padrão
novo.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` nova. `ItemCarrinho` no `Domain`; contratos na `Application`; repositório na `Infrastructure`; sessão só na `MVC` |
| II | Domínio rico e auto-validante | ⬜ OK | `ItemCarrinho` com `private set`, construtor validante, `protected Ctor()` e `AlterarQuantidade`. Os limites 1–99 são constantes do domínio, não números soltos na tela |
| III | Validação nas duas barreiras | ⬜ OK (parcial) | Quantidade é validada no domínio e saneada na borda. Não há formulário com campos de texto a validar — ver §10 |
| IV | Nomenclatura em português | ⬜ OK | `ItemCarrinho`, `ICarrinhoService`, `CarrinhoDaSessao`, `FiltroFusaoDeCarrinho`, `DisponivelParaCompra`, `carrinho.js` |
| V | Testes escritos antes | ⬜ OK | Cada fase tem fase vermelha própria |
| VI | Repositório + commit via UnitOfWork | ⬜ OK | Toda escrita passa por `IItemCarrinhoRepository` e fecha com `IUnitOfWork.SalvarAlteracoes`. Migration `AddItemCarrinho` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK | `[HttpPost]` + `[ValidateAntiForgeryToken]` nas três ações de escrita; PRG no caminho comum; o assíncrono não redireciona pelo mesmo motivo já justificado na `015` |
| VIII | Tratamento de erro por camada | ⬜ OK | Produto inexistente ou incomprável vira `KeyNotFoundException`/`InvalidOperationException` na aplicação, capturadas pelo filtro global |

## 3. Direção visual

Duas telas mudam e uma nasce. Nenhuma cor e nenhuma fonte nova — a tela do
carrinho usa os mesmos tokens do catálogo.

```
Meu carrinho
┌──────────────────────────────────────┐  ┌──────────────────┐
│ ┌────┐  Raspa Tacho                  │  │ Resumo           │
│ │img │  R$ 19,99                     │  │                  │
│ └────┘  [− 3 +]      R$ 59,97    ✕   │  │ 4 itens          │
├──────────────────────────────────────┤  │ Subtotal         │
│ ┌────┐  Café Especial                │  │ R$ 79,96         │
│ │img │  R$ 19,99                     │  │                  │
│ └────┘  [− 1 +]      R$ 19,99    ✕   │  │ ┌──────────────┐ │
├──────────────────────────────────────┤  │ │  Finalizar   │ │
│ ┌────┐  Palhas 4                     │  │ │  (em breve)  │ │
│ │img │  Fora de estoque              │  │ └──────────────┘ │
│ └────┘  não entra no total       ✕   │  └──────────────────┘
└──────────────────────────────────────┘
```

O item indisponível fica na lista, com a imagem esmaecida e o motivo no lugar
onde estaria o seletor de quantidade — a diferença entre "fora de estoque" e
"saiu do catálogo" é a mensagem, como a RN-06 exige.

**O contador do cabeçalho** é uma bolha sobre o ícone do carrinho, escondida
quando o carrinho está vazio — não um "0" desenhado.

**O seletor de quantidade passa a ser um campo de verdade.** Hoje o da página do
produto é um `<span>` que o JavaScript muda, e o do cartão é um `<span>` com
dois botões desabilitados. Nenhum dos dois sobrevive à RF-18. Os dois viram um
`<input>` que carrega o valor de fato:

| Tela | Antes | Depois |
|---|---|---|
| Página do produto | `<span>1</span>` + dois `<button>` | `<input type="number" min=1 max=99>` visível, com os ± acionando-o por script |
| Cartão | `<span>1</span>` + dois `<button disabled>` | `<input type="hidden" value="1">` + os ± reabilitados, acionando-o por script |

No cartão, sem script, os ± não fazem nada e o botão acrescenta **uma** unidade —
que é o comportamento honesto para um controle de grade. Na página do produto,
onde há espaço, o campo numérico funciona com ou sem script. A RF-02 é
satisfeita pela página do produto; a RF-01, pelo cartão.

## 4. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/ItemCarrinho.cs` | **criar** | `UsuarioId`, `ProdutoId`, `Quantidade`; `AlterarQuantidade`, `Acrescentar`; constantes `QuantidadeMinima`/`QuantidadeMaxima` |
| `Entities/Produto.cs` | alterar | `DisponivelParaCompra()` — RN-06 como invariante, não regra de tela |

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `DTOs/ItemDoCarrinhoDTO.cs` | **criar** | O par produto+quantidade que atravessa a fronteira, sem preço |
| `DTOs/LinhaDoCarrinhoDTO.cs` | **criar** | Uma linha já resolvida para a tela: nome, imagem, preço, quantidade, valor da linha, disponibilidade e motivo |
| `DTOs/CarrinhoDTO.cs` | **criar** | `Linhas`, `Subtotal`, `TotalDeItens` |
| `Enums/MotivoIndisponibilidade.cs` | **criar** | `Nenhum`, `ForaDoCatalogo`, `ForaDeEstoque` |
| `Contracts/Repositories/IItemCarrinhoRepository.cs` | **criar** | Ver §5 |
| `Contracts/Services/ICarrinhoService.cs` | **criar** | Ver §5 |
| `Services/CarrinhoService.cs` | **criar** | Todas as regras, para os dois armazenamentos |
| `Mappings/CarrinhoMapper.cs` | **criar** | Produtos + quantidades → `CarrinhoDTO`, com subtotal ignorando indisponível |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/ItemCarrinhoRepository.cs` | **criar** | Consulta por usuário com `Include` do produto; busca de par; contagem |
| `DatabaseContext/Configurations/ItemCarrinhoConfiguration.cs` | **criar** | Chave composta, FKs com `Restrict` — igual a `FavoritoConfiguration` |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | `DbSet<ItemCarrinho> ItensCarrinho` |
| `Migrations/` | **criar** | `AddItemCarrinho` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | Registro do repositório e do serviço |

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Program.cs` | alterar | `AddSession` + `UseSession`; registro do filtro de fusão |
| `Filters/FiltroFusaoDeCarrinho.cs` | **criar** | Requisição autenticada com carrinho na sessão → funde e limpa |
| `Helpers/CarrinhoDaSessao.cs` | **criar** | Leitura e escrita da lista em JSON na sessão. **Só isso** — nenhuma regra |
| `Controllers/CarrinhoController.cs` | **criar** | `Index`, `Acrescentar`, `AlterarQuantidade`, `Remover` |
| `Views/Carrinho/Index.cshtml` | **criar** | Tela do carrinho |
| `Views/Carrinho/_ItensDoCarrinho.cshtml` | **criar** | Bloco que a atualização sem recarga substitui |
| `Views/Shared/_Layout.cshtml` | alterar | `<form id="formulario-carrinho">`, irmão do de favorito; `carrinho.js` |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Atalho deixa de ser `href="#"`; bolha com a contagem |
| `ViewComponents/Header.cs` | alterar | **Remove** `itensCarrinho`; injeta `ICarrinhoService` e conta sozinho |
| `Views/Shared/Components/CardProduto/Default.cshtml` | alterar | Controles reabilitados, associados ao formulário do layout |
| `Views/Produto/Detalhes.cshtml` | alterar | Seletor vira campo numérico; botão passa a submeter |
| `wwwroot/js/components/carrinho.js` | **criar** | Interceptação, `fetch`, troca do bloco, ± acionando o campo |
| `wwwroot/js/pages/produto.js` | alterar | Os ± passam a acionar o `<input>`, não um `<span>` |
| `wwwroot/css/pages/carrinho.css` | **criar** | Tela do carrinho |
| `wwwroot/css/components/header.css` | alterar | Bolha do contador |
| `wwwroot/css/components/card-produto.css` | alterar | Controles deixam de ter aparência de desabilitado |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/ItemCarrinhoTests.cs` | **criar** | RN-01, RN-02, invariantes do construtor |
| `Units/Entities/ProdutoTests.cs` | alterar | `DisponivelParaCompra` nos três estados |
| `Units/Services/CarrinhoServiceTests.cs` | **criar** | Acrescentar/somar/limitar, recusa de indisponível, subtotal, fusão |
| `Units/Controllers/CarrinhoControllerTests.cs` | **criar** | Visitante × autenticado, assíncrono × comum, antiforgery |
| `Integration/Repositories/CarrinhoIntegrationTests.cs` | **criar** | Chave composta recusa par repetido; carrinho de um não vaza para outro |
| `E2E/Paginas/PaginaCarrinho.cs` | **criar** | Objeto de página |
| `E2E/Fluxos/CarrinhoTests.cs` | **criar** | CA-01 a CA-23 |

## 5. Contratos

```csharp
// ── Domínio ────────────────────────────────────────────────────────────
public class ItemCarrinho
{
    public const short QuantidadeMinima = 1;
    public const short QuantidadeMaxima = 99;   // RN-02, mesma da 008

    public Guid UsuarioId  { get; private set; }
    public Guid ProdutoId  { get; private set; }
    public short Quantidade { get; private set; }

    public void AlterarQuantidade(short quantidade);   // valida 1..99
    public void Acrescentar(short quantidade);         // soma e limita ao teto
}

// Produto — RN-06 como invariante
public bool DisponivelParaCompra() => Status == ProdutoStatus.Ativo;
```

```csharp
// ── Aplicação ──────────────────────────────────────────────────────────
public interface IItemCarrinhoRepository
{
    Task<List<ItemCarrinho>> BuscarPorUsuario(Guid usuarioId);   // com Include do produto
    Task<ItemCarrinho?> Buscar(Guid usuarioId, Guid produtoId);
    Task<int> ContarItens(Guid usuarioId);
    Task Adicionar(ItemCarrinho item);
    void Remover(ItemCarrinho item);
}

public interface ICarrinhoService
{
    // Carrinho persistido — de quem entrou
    Task<CarrinhoDTO> ObterDoUsuario(Guid usuarioId);
    Task Acrescentar(Guid usuarioId, Guid produtoId, short quantidade);
    Task AlterarQuantidade(Guid usuarioId, Guid produtoId, short quantidade);
    Task Remover(Guid usuarioId, Guid produtoId);
    Task<int> ContarItens(Guid usuarioId);

    // Carrinho avulso — de quem ainda não entrou. As mesmas regras, aplicadas
    // sobre uma lista que o chamador guarda onde quiser. O serviço não sabe
    // que existe sessão; a MVC não sabe que existe regra.
    Task<CarrinhoDTO> MontarAvulso(IReadOnlyList<ItemDoCarrinhoDTO> itens);
    Task<IReadOnlyList<ItemDoCarrinhoDTO>> AcrescentarAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade);
    IReadOnlyList<ItemDoCarrinhoDTO> AlterarQuantidadeAvulsa(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId, short quantidade);
    IReadOnlyList<ItemDoCarrinhoDTO> RemoverAvulso(
        IReadOnlyList<ItemDoCarrinhoDTO> itens, Guid produtoId);

    // A ponte entre os dois — RN-05
    Task Fundir(Guid usuarioId, IReadOnlyList<ItemDoCarrinhoDTO> itensDaSessao);
}
```

**Por que a interface é grande, e por que não deveria ser menor.** Dez métodos é
mais que qualquer serviço desta base. É o custo direto da decisão de deixar o
visitante montar carrinho na sessão: existem dois armazenamentos, e eles não têm
a mesma forma — um é consultável e transacional, o outro é uma lista que viaja
inteira. A alternativa considerada foi uma abstração de armazenamento com duas
implementações, escolhida por injeção conforme a autenticação. Ela deixaria a
interface com cinco métodos e acrescentaria uma fábrica, um contrato novo e uma
implementação na `MVC` que precisaria ser registrada por requisição — mais peças
móveis para esconder uma assimetria que é real. Ver §8.

## 6. Modelo de dados

- **Entidade:** `ItemCarrinho` — `UsuarioId` (Guid), `ProdutoId` (Guid),
  `Quantidade` (smallint, 1–99).
- **Chave primária composta `(UsuarioId, ProdutoId)`** — é ela que garante a
  RN-01 no nível que nenhum caminho de código contorna, mesmo padrão de
  `Favorito`.
- **Relacionamentos:** `ItemCarrinho → Produto` e `ItemCarrinho → Usuario`, com
  `OnDelete(DeleteBehavior.Restrict)` nos dois — igual a `Favorito`.
- **Migration:** `dotnet ef migrations add AddItemCarrinho --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`
- **Impacto em dados existentes:** **nenhum.** Tabela nova, vazia.
- **`ModelagemBancoTCC.dbml`:** ganha a décima quinta tabela e as duas
  referências. O diagrama é entregável do TCC, e desatualizá-lo é dívida
  silenciosa — entra como tarefa.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade — entidade | `ItemCarrinhoTests` | RN-02 nos dois extremos; `Acrescentar` que estoura o teto para em 99 |
| Unidade — entidade | `ProdutoTests` | `DisponivelParaCompra` nos três estados |
| Unidade — serviço | `CarrinhoServiceTests` | RN-01 (soma, não duplica), RN-04 (preço atual), RN-05 (fusão), RN-06 (recusa), subtotal ignorando indisponível |
| Unidade — controller | `CarrinhoControllerTests` | Visitante usa sessão, autenticado usa banco; assíncrono devolve JSON, comum redireciona |
| Integração | `CarrinhoIntegrationTests` | RN-01 no banco (chave composta) e RN-03 (carrinho de um não traz o de outro) |
| E2E | `CarrinhoTests` | O resto — só o navegador sabe se a página recarregou, se a sessão sobreviveu e se a fusão aconteceu no login |

Mapeamento critério → teste:

| Critério | Teste |
|---|---|
| CA-01 | `Dado_ClienteAutenticado_Quando_AcrescentarDoCartao_Entao_DeveEntrarNoCarrinho` |
| CA-02 | `Dado_PaginaDoProduto_Quando_AcrescentarComQuantidadeTres_Entao_DeveEntrarComTres` |
| CA-03 | `Dado_ProdutoJaNoCarrinho_Quando_AcrescentarDeNovo_Entao_DeveSomarNumaLinhaSo` |
| CA-04 | `Dado_ProdutoIndisponivel_Quando_TentarAcrescentar_Entao_DeveRecusarComExplicacao` |
| CA-05 | `Dado_CarrinhoComItens_Quando_AbrirATela_Entao_DeveMostrarNomeImagemPrecoQuantidadeELinha` |
| CA-06 | `Dado_TelaDoCarrinho_Quando_AlterarQuantidade_Entao_LinhaESubtotalDevemAcompanhar` |
| CA-07 | `Dado_TelaDoCarrinho_Quando_RemoverItem_Entao_DeveSairEDeixarDeContar` |
| CA-08 | `Dado_ItemComQuantidadeUm_Quando_Reduzir_Entao_DeveSairDoCarrinho` |
| CA-09 | `Dado_ItemComQuantidadeMaxima_Quando_TentarAumentar_Entao_DeveContinuarNoMaximo` |
| CA-10 | `Dado_CarrinhoVazio_Quando_Abrir_Entao_DeveOferecerCaminhoParaOCatalogo` |
| CA-11 | `Dado_CarrinhoMontado_Quando_SairEEntrarDeNovo_Entao_DeveEstarComoFoiDeixado` |
| CA-12 | `Dado_Visitante_Quando_AcrescentarAoCarrinho_Entao_DeveConseguirVerEAlterar` |
| CA-13 | `Dado_CarrinhosNosDoisLados_Quando_Entrar_Entao_AsQuantidadesDevemSomar` |
| CA-14 | `Dado_FusaoConcluida_Quando_VoltarComoVisitante_Entao_OCarrinhoAvulsoDeveEstarVazio` |
| CA-15 | `Dado_ItensNoCarrinho_Quando_OlharOCabecalho_Entao_DeveIndicarAQuantidade` |
| CA-16 | `Dado_QualquerPagina_Quando_AcionarOAtalhoDeCarrinho_Entao_DeveChegarNaTela` |
| CA-17 | `Dado_ItemQueFicouIndisponivel_Quando_AbrirOCarrinho_Entao_DeveAparecerSinalizadoSemSomar` |
| CA-18 | `Dado_ItensIndisponiveisPorMotivosDiferentes_Quando_Abrir_Entao_AsMensagensDevemDiferir` |
| CA-19 | `Dado_ItemIndisponivel_Quando_OProdutoVoltar_Entao_DeveVoltarASomarSozinho` |
| CA-20 | `Dado_JavaScriptDesligado_Quando_AcrescentarAlterarERemover_Entao_OsTresDevemFuncionar` |
| CA-21 | `Dado_TelaDoCarrinho_Quando_AlterarQuantidade_Entao_NaoDeveRecarregarAPagina` |
| CA-22 | `Dado_DuasPessoasComCarrinho_Quando_CadaUmaAbrirOSeu_Entao_NenhumaDeveVerAOutra` |
| CA-23 | `Dado_CarrinhoComItens_Quando_OlharOFechamento_Entao_DeveEstarSinalizadoComoIndisponivel` |

**CA-13 e CA-14 são os testes mais frágeis desta feature**, e os que mais
importa acertar: envolvem sessão, autenticação e uma transferência de estado
entre dois armazenamentos. São escritos **por último**, depois de CA-11
(persistência) e CA-12 (sessão do visitante) estarem verdes — para que uma falha
neles signifique "a fusão não aconteceu", e não "um dos dois lados não
funcionava".

**CA-19 não tem tela administrativa que o exercite.** Não existe caminho pela
interface para inativar um produto e reativá-lo. O teste manipula o estado do
produto diretamente pelo banco de teste, e isso fica registrado no checklist
como cobertura parcial — a mesma limitação que a `015` registrou no CA-10 dela.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| Carrinho só na sessão, sem tabela | Decisão explícita do responsável em contrário. Não sobreviveria à HU-02 (outro aparelho, outro dia) |
| Tabela-cabeçalho `Carrinho` + `ItemCarrinho` | Uma pessoa tem exatamente um carrinho, e o cabeçalho não teria estado próprio — só acrescentaria uma junção a toda consulta e uma linha órfã a toda exclusão |
| Coluna de preço no item | Ver RN-04. Congelar preço é papel do pedido, e um carrinho de semanas viraria compromisso comercial que a loja não assumiu |
| Abstração de armazenamento com duas implementações | Deixaria `ICarrinhoService` com cinco métodos em vez de dez, ao custo de uma fábrica, um contrato novo e uma implementação registrada por requisição na `MVC`. Esconde uma assimetria que é real: um armazenamento é consultável e transacional, o outro é uma lista que viaja inteira |
| Fundir dentro da ação de login | Acopla `AutenticacaoController` ao domínio de carrinho — exatamente o que a `015` evitou com favoritos. E só funcionaria pelo caminho de login que fosse alterado, deixando o modal ou o retorno de fora |
| Fundir por middleware em vez de filtro | Middleware roda antes do roteamento e não tem acesso fácil a serviços por escopo nem ao resultado da autorização. O filtro roda depois de `UseAuthentication`, que é exatamente quando se sabe quem é a pessoa |
| Apagar item que ficou indisponível | Destrói a intenção de quem escolheu. Produto que volta ao catálogo em dois dias já não estaria lá — e a RN-07 diz o contrário |
| Esconder item indisponível, como nos favoritos | Favoritos é lembrete; carrinho tem total. Item que some sem avisar faz o subtotal mudar entre duas visitas sem explicação |
| Manter o `<span>` do seletor de quantidade | Não sobrevive à RF-18: sem script não há como o valor chegar ao servidor. O campo numérico funciona nos dois casos |
| Preencher o parâmetro `itensCarrinho` do `Header` | O parâmetro é insolúvel por quem chama: nenhum layout sabe quem está vendo. O componente precisa contar sozinho, como já faz com categorias e com o termo de busca |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **A ordem de `UseSession` no pipeline quebra a sessão em silêncio** — sessão lida antes do middleware rodar devolve vazio sem erro | Média | Alto | Entra logo após `UseRouting` e antes de `UseAuthentication`; CA-12 falha imediatamente se estiver fora de ordem |
| **A fusão roda duas vezes e duplica quantidade** — duas requisições autenticadas concorrentes com a mesma sessão | Baixa | Alto | O filtro limpa a sessão **na mesma requisição** em que funde, e a soma é limitada ao teto da RN-02. CA-13 e CA-14 juntos cobrem os dois lados |
| **O carrinho do visitante desaparece ao reiniciar a aplicação** | Alta em desenvolvimento | Baixo | Consequência aceita e registrada na spec §10. Sessão em memória é o padrão; trocar por armazenamento distribuído é decisão de implantação, não desta feature |
| **O contador do cabeçalho custa uma consulta por página** — o `Header` aparece em toda tela | Alta | Médio | `ContarItens` é um `COUNT` por índice de chave primária. Fica ao lado da consulta de taxonomia que o `Header` já faz em toda requisição; se um dia doer, os dois viram cache juntos |
| **Reabilitar os controles do cartão quebra o teste da `012`/`015`** que prova que eles estão desabilitados | Alta | Baixo | É quebra esperada: aquele teste existia para provar que a promessa não era feita. Ele é reescrito, não removido — passa a provar que agora funciona |
| **A grade do catálogo ganha um formulário por cartão** e volta o problema de form aninhado da `015` | Média | Alto | Mesma solução já provada: `form=` apontando para um formulário único no `_Layout`, com `produtoId` no `name`/`value` do próprio botão |
| **Renumeração da cadeia deixa referência obsoleta** — aconteceu nas duas primeiras vezes | Alta | Baixo | Tarefa própria, varrendo `spec 0NN` na base inteira, incluindo esta spec e este plano |

## 10. Desvios constitucionais justificados

**Princípio III — não há validador de entrada para o carrinho.**

O princípio pede a regra nas duas barreiras: validator para proteger o usuário,
invariante para proteger o dado. As três ações de escrita desta feature recebem
um identificador de produto que o próprio sistema imprimiu e um número de
quantidade. Não há formato a conferir, nem mensagem de campo a devolver: a tela
do carrinho não tem campo de texto livre.

A quantidade **é** validada nas duas pontas, só que de formas diferentes do par
validator/entidade: o `<input type="number" min max>` limita na borda, o
controlador saneia o que chegar mesmo assim, e `ItemCarrinho.AlterarQuantidade`
recusa fora do intervalo com exceção. Um `CarrinhoDTOValidator` no meio
acrescentaria uma terceira checagem do mesmo número, sem uma view com campo onde
pendurar a mensagem.

A alternativa conforme — criar o DTO de entrada e o validator — foi descartada
por adicionar classe, validador e `ModelState` a três ações que não têm
formulário com campos, e cuja única entrada numérica já é recusada pelo domínio.

---

## Sobre as duas specs seguintes

Decididas nesta mesma conversa, registradas aqui para não serem redecididas:

**`018` — Endereços.** CRUD completo sobre a entidade `Endereco`, que existe
desde a `003` e nunca foi usada, com um endereço marcado como padrão. ViaCEP
preenche rua, bairro, cidade e UF, com digitação manual como piso — serviço fora
do ar, timeout ou JavaScript desligado não impedem ninguém de cadastrar.
Primeira dependência de rede externa do projeto.

**`019` — Fechamento.** `Produto` ganha `Peso`, `Altura`, `Largura` e
`Comprimento`, com migration, campos no cadastro e valores para os 100 produtos
semeados. Frete cotado pelo MelhorEnvio, com CEP de origem em configuração e
token como segredo — e o frete só é calculável **depois** do endereço escolhido.
O fechamento grava `Pedido` (`Status = Pendente`, `PagamentoAprovado = false`,
`Valor = itens + frete`), um `ItemPedido` por linha com `PrecoUnitario`
congelado, e um `Pagamento` com o método escolhido e status `Pendente`. O
carrinho é esvaziado, o que também impede fechar o mesmo pedido duas vezes por
recarregar a página.
