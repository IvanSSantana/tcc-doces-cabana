# Plano Técnico — Refinamento do catálogo

**Spec de origem:** [`spec.md`](./spec.md) · **Criado em:** 2026-08-20
**Status:** Executado

---

## 1. Resumo da abordagem

Quatro frentes independentes que se encontram na mesma página.

**Atualização sem recarga.** O bloco que muda — contagem, grade e paginação —
sai de `Index.cshtml` para uma partial própria. O controller ganha um desvio:
requisição marcada como assíncrona devolve a partial; requisição comum devolve a
página inteira, idêntica à de hoje. Um script intercepta as trocas de filtro,
de ordenação e de página, busca, troca o conteúdo do bloco e sincroniza o
endereço com `history.pushState`. **Sem script, nada intercepta** e o formulário
submete como sempre — o caminho sem JavaScript é literalmente o código atual,
intocado, e é por isso que RF-05 sobrevive por construção e não por disciplina.

**Cartão na grade.** `width: 85%` sai da classe base do cartão e vai para o
carrossel. Largura é responsabilidade do container, não do componente — é essa
inversão, e não um ajuste de número, que conserta o defeito. Para o alinhamento,
`margin-top: auto` nas ações: itens de grade esticam por padrão, então os botões
encostam na base e alinham sozinhos, com nome de uma ou de duas linhas. Nada de
altura fixa arbitrada. O catálogo não passa parâmetro nenhum ao componente; o
que difere é escopado por seletor de ancestral.

**Avaliações.** O elenco de clientes fictícios vai de 3 para 8. Cerca de 70 dos
100 produtos recebem de 1 a 4 avaliações, com notas enviesadas para cima e parte
sem comentário. Um `Random` de semente fixa torna a base reproduzível. A geração
sai para uma função própria, sem acesso a banco, justamente para poder ser
testada duas vezes e comparada (CA-16).

**Índice único, ordenação inicial e cabeçalho.** Um índice único em
`(UsuarioId, ProdutoId)` com migration; a ordenação padrão passa a
`MelhorAvaliados`; o atalho "Conta" é desabilitado.

## 2. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ⬜ OK | Nenhuma `ProjectReference` tocada |
| II | Domínio rico e auto-validante | ⬜ OK | `Avaliacao` não muda. RN-01 é invariante entre instâncias — ver §10 |
| III | Validação nas duas barreiras | ⚠️ | RN-01 fica só na barreira de dados. **Desvio justificado em §10** |
| IV | Nomenclatura em português | ⬜ OK | `_ResultadoCatalogo.cshtml`, `catalogo.js`, `GerarAvaliacoesMock` |
| V | Testes escritos antes | ⬜ OK | Fase 2 vermelha antes de qualquer implementação |
| VI | Repositório + commit via UnitOfWork | ⬜ OK | Uma migration (`AddUniqueIndexAvaliacaoUsuarioProduto`). O seeder segue usando `DbContext` direto, como já fazia |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ⬜ OK (parcial) | Nenhum `POST` novo: a atualização parcial é `GET`, como o catálogo inteiro. Antiforgery não se aplica a leitura. Catálogo segue público por requisito |
| VIII | Tratamento de erro por camada | ⬜ OK | RF-06 é recuperação no cliente; o servidor não ganha caminho de erro novo |

## 3. Direção visual

O cartão não é redesenhado — é corrigido para o contexto em que foi colocado.
Medidas tiradas do navegador, viewport 1440, catálogo de "Doces".

```
HOJE                                        DEPOIS
┌─────────┬─────────┬─────────┐             ┌─────────┬─────────┬─────────┐
│▓▓▓▓▓▓▒▒▒│▓▓▓▓▓▓▒▒▒│▓▓▓▓▓▓▒▒▒│             │▓▓▓▓▓▓▓▓▓│▓▓▓▓▓▓▓▓▓│▓▓▓▓▓▓▓▓▓│
│  nome   │  nome   │  nome   │             │  nome   │  nome   │  nome   │
│  R$     │  R$     │ nome 2ª │             │  R$     │  R$     │ nome 2ª │
│ [-1+][A]│ [-1+][A]│  R$     │             │         │         │  R$     │
│         │         │ [-1+][A]│             │ [-1+][A]│ [-1+][A]│ [-1+][A]│
└─────────┴─────────┴─────────┘             └─────────┴─────────┴─────────┘
   ▒ = 15% de coluna morta                     cartão preenche a coluna
   botões em alturas diferentes                botões na mesma linha de base
```

| Medida | Hoje | Depois |
|---|---|---|
| Largura do cartão na coluna | 85% (≈48px mortos por coluna) | 100% |
| Botões de produtos da mesma linha | desalinhados quando o nome quebra | alinhados na base |
| Etiqueta "fora de estoque" | acima da imagem, sobre fundo transparente | sobre a imagem |
| Aparência no carrossel | — | inalterada (RF-11) |

Nenhuma cor nova, nenhuma fonte nova, nenhuma animação nova. A grade continua
com o mesmo número de colunas (spec §8).

## 4. Impacto por camada

### `DocesCabana.Domain`

Nenhum arquivo. `Avaliacao` já valida nota, comentário e chaves.

### `DocesCabana.Application`

Nenhum arquivo. `OrdenacaoCatalogo` e `CatalogoService` seguem como estão — a
ordenação inicial é um padrão de borda, não regra de aplicação.

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `DatabaseContext/Configurations/AvaliacaoConfiguration.cs` | alterar | Índice único em `(UsuarioId, ProdutoId)` |
| `Migrations/*_AddUniqueIndexAvaliacaoUsuarioProduto.cs` | **criar** | Gerada e inspecionada antes de aplicar |

`ProdutoRepository` **não muda**: `MelhorAvaliados` já está implementado, com o
`?? -1` que joga produto sem nota para o fim (RN-02) e o `ThenBy(Nome)` que
garante RN-04.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/CatalogoController.cs` | alterar | Padrão de `ordenacao` passa a `MelhorAvaliados`; `SanearOrdenacao` passa a mandar `MaisVendidos` para `MelhorAvaliados`; desvio que devolve a partial em requisição assíncrona |
| `Views/Catalogo/_ResultadoCatalogo.cshtml` | **criar** | Contagem, grade (ou mensagem de vazio) e paginação |
| `Views/Catalogo/Index.cshtml` | alterar | Passa a incluir a partial; contagem ganha `aria-live` |
| `wwwroot/js/pages/catalogo.js` | **criar** | Interceptação, busca, troca, `pushState`/`popstate`, foco, rolagem, recuperação de falha |
| `wwwroot/css/components/card-produto.css` | alterar | Tira `width: 85%` da base; `margin-top: auto` nas ações; etiqueta sobre a imagem |
| `wwwroot/css/pages/catalogo.css` | alterar | Estado de carregamento da grade |
| `wwwroot/css/components/vitrine-produtos.css` | alterar | Recebe a largura que saiu do cartão (RF-11) |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | "Conta" desabilitado, padrão dos controles do cartão |
| `Helpers/DbInitializer.cs` | alterar | Elenco de 8 clientes; `GerarAvaliacoesMock` determinística |

### `DocesCabana.Tests` / `DocesCabana.Tests.E2E`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Helpers/GeradorDeAvaliacoesTests.cs` | **criar** | Reprodutibilidade, cobertura, produtos sem nota |
| `Integration/Repositories/AvaliacaoIntegrationTests.cs` | **criar** | Índice único recusa a segunda avaliação |
| `E2E/Paginas/PaginaCatalogo.cs` | alterar | Localizadores novos e ações que esperam a troca parcial |
| `E2E/Fluxos/CatalogoTests.cs` | alterar | CA-01 a CA-12, CA-18 a CA-20; **o teste de RF-05 passa a desligar o JavaScript de verdade** |

## 5. Contratos

```csharp
// Padrão de ordenação muda; a assinatura em si é a mesma.
public async Task<IActionResult> Index(
    string? apelido = null,
    [FromQuery] Guid[]? subcategorias = null,
    [FromQuery] bool semAcucar = false,
    OrdenacaoCatalogo ordenacao = OrdenacaoCatalogo.MelhorAvaliados,
    int pagina = 1);

// Geração determinística, sem banco — é o que torna CA-16 testável.
internal static List<Avaliacao> GerarAvaliacoesMock(
    IReadOnlyList<Produto> produtos,
    IReadOnlyList<Guid> usuarioIds,
    int semente = 20260820);
```

O contrato da atualização parcial é o mesmo endereço do catálogo, com o
cabeçalho que identifica requisição assíncrona. Não há rota nova, não há API
paralela: **um endereço, duas representações**. Isso é o que mantém RF-02 e
RF-05 verdadeiros de graça — o endereço que o script empurra no histórico é o
mesmo que funciona colado numa aba nova.

## 6. Modelo de dados

Uma mudança de esquema: índice único em `Avaliacao (UsuarioId, ProdutoId)`.

Nenhuma coluna nova, nenhuma tabela nova, nenhum dado existente perdido — a
base atual tem três avaliações, de três pessoas distintas, sobre o mesmo
produto, e nenhuma delas colide com o índice.

## 7. Estratégia de teste

| Nível | Arquivo | O que prova |
|---|---|---|
| Unidade | `Units/Helpers/GeradorDeAvaliacoesTests.cs` | RF-12, RF-13, RF-14 — cobertura, lacunas e reprodutibilidade, sem subir banco |
| Integração | `Integration/Repositories/AvaliacaoIntegrationTests.cs` | RF-15/RN-01 — a segunda avaliação é recusada pelo banco |
| E2E | `E2E/Fluxos/CatalogoTests.cs` | O resto: só o navegador sabe se a página recarregou, onde ficou a rolagem e onde foi parar o foco |

Mapeamento critério → teste:

| Critério | Teste que o prova |
|---|---|
| CA-01 | `Dado_CatalogoAberto_Quando_MarcarSubcategoria_Entao_NaoDeveRecarregarAPagina` |
| CA-02 | `Dado_FiltroAplicado_Quando_OlharOEndereco_Entao_DeveConterOFiltro` |
| CA-03 | `Dado_FiltroAplicado_Quando_VoltarNoNavegador_Entao_DeveRestaurarAListaAnterior` |
| CA-04 | `Dado_PaginaRolada_Quando_TrocarAOrdenacao_Entao_DevePreservarARolagem` |
| CA-05 | `Dado_FimDaPrimeiraPagina_Quando_IrParaASegunda_Entao_DeveMostrarOInicioDaLista` |
| CA-06 | `Dado_FiltroAplicado_Quando_OResultadoMuda_Entao_DeveSerAnunciado` |
| CA-07 | `Dado_JavaScriptDesligado_Quando_FiltrarOrdenarEPaginar_Entao_TudoDeveFuncionar` |
| CA-08 | `Dado_AtualizacaoParcialFalha_Quando_FiltrarEntao_DeveCarregarAPaginaCompleta` |
| CA-09 | `Dado_CategoriaAberta_Quando_TrocarDeCategoria_Entao_DeveTrocarAsSubcategorias` |
| CA-10 | `Dado_CatalogoAberto_Quando_MedirOCartao_Entao_DevePreencherAColuna` |
| CA-11 | `Dado_LinhaComNomeCurtoENomeLongo_Quando_CompararOsBotoes_Entao_DevemEstarNaMesmaAltura` |
| CA-12 | `Dado_ProdutoForaDeEstoque_Quando_OlharAEtiqueta_Entao_DeveEstarSobreAImagem` |
| CA-13 | `Dado_PaginaInicial_Quando_OlharOCarrossel_Entao_NaoDeveTerRegredido` |
| CA-14, CA-15 | `GeradorDeAvaliacoesTests` — cobertura e lacunas |
| CA-16 | `Dado_AMesmaSemente_Quando_GerarDuasVezes_Entao_DeveProduzirOMesmoResultado` |
| CA-17 | `Dado_PessoaQueJaAvaliou_Quando_RegistrarSegundaAvaliacao_Entao_DeveRecusar` |
| CA-18 | `Dado_CatalogoSemOrdenacaoEscolhida_Quando_Abrir_Entao_DeveOrdenarPorMelhorAvaliados` |
| CA-19 | `Dado_OrdenacaoInicial_Quando_PercorrerDuasPaginas_Entao_NenhumProdutoDeveSeRepetir` |
| CA-20 | `Dado_ClienteAutenticado_Quando_OlharOCabecalho_Entao_NaoDeveOferecerConta` |

**CA-07 é o critério mais importante desta feature**, e é o que hoje não tem
prova. O teste atual navega direto por endereço com JavaScript ligado — prova o
contrato de URL, não a degradação. O novo abre um contexto com
`JavaScriptEnabled = false`, marca a caixa e **clica o botão "Aplicar" do
`<noscript>`**, que é o caminho que ninguém percorreu até hoje.

**CA-08 usa interceptação de rota** (`Page.RouteAsync`) para abortar a
requisição parcial e verificar que a navegação completa acontece. Sem isso, o
caminho de recuperação seria código que nunca roda.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| htmx | Chega no mesmo lugar com menos código próprio, mas custa uma biblioteca de front-end nova para justificar num projeto que foi conservador em dependência. Oferecida ao responsável e descartada |
| Aplicação de página única | Descartada na origem: a dor relatada é recarga ao filtrar, não arquitetura. Reescreveria o catálogo inteiro para resolver um incômodo de transição |
| Ajaxificar também os links de categoria | Trocar de categoria troca a lista de subcategorias, então a barra lateral teria de ser reconstruída — arrancando o foco do teclado do controle recém-usado. Recarga completa ali é aceitável e mais simples (spec §10) |
| Renderizar a grade em JavaScript, a partir de dados | Duplicaria a montagem do cartão em duas linguagens. Devolver a partial mantém **uma** fonte de renderização, o Razor |
| Rota nova, tipo `/Catalogo/Parcial` | Um endereço com duas representações mantém RF-02 e RF-05 verdadeiros sem esforço. Rota separada abriria espaço para as duas divergirem |
| Componente próprio para o cartão do catálogo | Só o estilo difere; duas marcações seriam duas telas para manter em sincronia |
| Exibir nota no cartão | Daria finalidade visível ao seed, mas muda conteúdo, não estilo. Oferecida e descartada (spec §10) |
| Altura mínima fixa no nome do produto | Resolve o alinhamento com um número chutado, que erra assim que a fonte ou o texto mudarem. `margin-top: auto` alinha sozinho |
| Semear avaliação em todos os produtos | O ramo "produto sem nota vai para o fim" deixaria de existir em demonstração — e catálogo real nenhum é assim |

## 9. Riscos

| Risco | Prob. | Impacto | Mitigação |
|---|---|---|---|
| **Trocar a ordenação padrão quebra testes existentes** que assumem ordem alfabética na primeira página | Alta | Baixo | Rodar a suíte inteira logo após a mudança, isolada em tarefa própria, antes de mexer em qualquer outra coisa |
| **O foco se perde ao trocar o conteúdo.** Os links de paginação vivem *dentro* do bloco substituído: clicar "2" destrói o elemento que tinha o foco, e quem usa teclado é jogado para o começo do documento | Alta | Alto | Após a troca, mover o foco para o cabeçalho do resultado, com `tabindex="-1"` — virou RF-18/CA-21 na spec, para não ser trabalho que nenhum requisito pede. Tarefa própria e com teste, não conferência de passagem |
| **O endereço montado pelo script diverge do que o formulário produziria**, e o resultado colado numa aba nova não bate com o que estava na tela | Média | Alto | Montar o endereço a partir do próprio formulário, não à mão — assim há uma só regra de serialização |
| **O `<noscript>` só existe com script desligado.** Com JavaScript ligado o conteúdo dele não vira elemento, então nenhum teste comum o alcança | Certa | Médio | É exatamente por isso que CA-07 abre um contexto com JavaScript desligado, em vez de tentar clicar o botão no contexto normal |
| **A base maior deixa a suíte E2E mais lenta**: 8 contas passam pelo algoritmo de senha do Identity, que é deliberadamente caro | Média | Baixo | Medir o tempo de subida antes e depois; se pesar, o elenco é o primeiro número a revisar |
| **O índice único falha ao aplicar** se a base de alguém já tiver duplicidade | Baixa | Médio | A base semeada não tem; bases de desenvolvimento local são recriáveis. Inspecionar a migration antes de aplicar |
| **Estado de carregamento pisca** em respostas rápidas, ficando pior que a recarga que veio substituir | Média | Baixo | Só marcar carregamento após um atraso curto; se a resposta chegar antes, nada aparece |

## 10. Desvios constitucionais justificados

**Princípio III — RN-01 fica só na barreira de dados.**

O princípio pede a regra nas duas barreiras: validador de entrada e invariante
de entidade. RN-01 ("uma pessoa avalia um produto no máximo uma vez") não cabe
em nenhuma das duas hoje:

- **Na entidade não cabe** porque é invariante *entre instâncias*. Uma
  `Avaliacao` não enxerga as outras avaliações do mesmo produto; responder
  "essa pessoa já avaliou?" exige consulta, e consulta dentro de entidade é
  justamente o que o Princípio I proíbe.
- **No validador não cabe** porque não existe entrada de usuário para validar:
  a tela de escrever avaliação está no backlog. Criar um validador agora seria
  escrever guarda para um formulário que não existe.

Fica, então, o índice único — que é a barreira que **não pode ser contornada
por caminho nenhum**, inclusive pelo seeder. Quando a tela de escrever
avaliação for construída, ela acrescenta a segunda barreira: verificação no
serviço antes de gravar, para que o cliente receba mensagem de campo em vez de
tela de erro. Registrado aqui para que aquela feature não redescubra a lacuna.
