# Checklist de conclusão — Refinamento do catálogo

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — os 18 requisitos,
      verificados um a um contra `catalogo.js`, `_ResultadoCatalogo.cshtml`,
      `CatalogoController.cs`, `card-produto.css` e `DbInitializer.cs`
- [x] Todo `CA-xx` foi verificado — os 21 critérios, por teste E2E/unidade/
      integração contra a aplicação rodando de verdade, mais captura de tela
      e navegação manual (filtro, teclado, JavaScript desligado, 375px)
- [x] Nada fora do escopo declarado entrou junto na entrega — a única
      mudança de marcação fora do que o plano listou foi mover a etiqueta de
      "fora de estoque" para dentro do link da imagem (achado abaixo,
      registrado, não escondido)
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou — a pendência da
      ordem das categorias no cabeçalho, herdada da `013`, continua registrada
      como decisão de negócio em aberto, não indecisão técnica desta feature

## Constituição

- [x] **I** — nenhuma `ProjectReference` tocada
- [x] **II** — `Avaliacao` não mudou; `RN-01` é invariante entre instâncias,
      não vive na entidade (ver desvio abaixo)
- [x] **III** (desvio justificado) — `RN-01` fica só na barreira de dados
      (índice único). Não cabe em validador porque não há formulário de
      avaliação ainda; não cabe na entidade porque depende de consulta a
      outras avaliações, que o Princípio I proíbe dentro do domínio. Registrado
      no plano §10 para a feature que criar a tela de avaliação fechar a
      segunda barreira
- [x] **IV** — `_ResultadoCatalogo.cshtml`, `catalogo.js`, `GerarAvaliacoesMock`,
      `SortearNotaEnviesada`; `InternalsVisibleTo` documentado no `.csproj`
- [x] **V** — Fase 2 vermelha (índice único, ordenação inicial) antes de
      qualquer outra mudança, isolada de propósito — era o risco de maior
      probabilidade do plano (§9)
- [x] **VI** — uma migration (`AddUniqueIndexAvaliacaoUsuarioProduto`), lida
      antes de aplicar: só cria o índice, não toca coluna nem dado
- [x] **VII** — n/a: a atualização parcial é `GET`, mesmo verbo do catálogo
      inteiro; nenhum `POST` novo
- [x] **VIII** — n/a: nenhum caminho de erro novo no servidor; a recuperação
      de falha (RF-06) é só do lado do cliente

## O que foi provado, e como

| Requisito | Prova |
|---|---|
| RF-01/CA-01 | `Dado_CatalogoAberto_Quando_MarcarSubcategoria_Entao_NaoDeveRecarregarAPagina` (marcador em memória sobrevive à troca) |
| RF-02/CA-02/CA-03 | `Dado_FiltroAplicado_Quando_OlharOEndereco...` e `...VoltarNoNavegador...` (pushState + popstate) |
| RF-03/CA-04/CA-05 | `Dado_PaginaRolada_Quando_TrocarAOrdenacao...` e `Dado_FimDaPrimeiraPagina...` |
| RF-04/CA-06 | `Dado_FiltroAplicado_Quando_OResultadoMuda_Entao_DeveSerAnunciado` (`aria-live="polite"`) |
| RF-05/CA-07 | `Dado_JavaScriptDesligado_Quando_FiltrarOrdenarEPaginar...` — contexto com `JavaScriptEnabled = false` de verdade, não mais só navegação direta por URL |
| RF-06/CA-08 | `Dado_AtualizacaoParcialFalha_Quando_Filtrar...` — interceptação de rota aborta a requisição `XMLHttpRequest` |
| RF-07/CA-09 | `Dado_CategoriaAberta_Quando_TrocarDeCategoria...` |
| RF-08 a RF-11/CA-10 a CA-13 | `CatalogoTests` (medição de largura, alinhamento, etiqueta, não regressão do carrossel) |
| RF-12 a RF-15/CA-14 a CA-17 | `GeradorDeAvaliacoesTests` (unidade) + `AvaliacaoIntegrationTests` (índice único) |
| RF-16/RN-04/CA-18/CA-19 | `Dado_CatalogoSemOrdenacaoEscolhida...` e `Dado_OrdenacaoInicial_Quando_PercorrerDuasPaginas...` |
| RF-17/CA-20 | `Dado_ClienteAutenticado_Quando_OlharOCabecalho_Entao_NaoDeveOferecerContaClicavel` |
| RF-18/CA-21 | `Dado_NavegacaoPorTeclado_Quando_TrocarDePaginaPelaPaginacao...` |

## Achados durante a implementação, registrados aqui em vez de corrigidos em silêncio

**A etiqueta "fora de estoque" precisou mudar de lugar na marcação, não só de
CSS.** O plano previa só alterar `card-produto.css`. Na prática, a etiqueta era
irmã anterior de `.container-imagem-card` no HTML, não filha — o
`position: absolute` a ancorava no cartão inteiro, e ela aparecia solta acima
da imagem, não sobre ela. Movida para dentro do link da imagem em
`CardProduto/Default.cshtml`; sem isso, RF-10 não tinha como ser satisfeito
por CSS puro sem um truque frágil entre irmãos.

**O botão voltar do navegador não conseguia restaurar as caixas de
subcategoria.** A primeira versão do `popstate` só rebuscava e trocava
`#resultado-catalogo` — mas as caixas de filtro vivem na barra lateral, fora
dessa área. Voltar restaurava a lista, não os controles, falhando a segunda
metade de CA-03 ("com os controles no estado anterior"). Corrigido trocando o
`popstate` por uma recarga completa: mais simples que sincronizar manualmente
o estado de cada controle da barra lateral, e coerente com a escolha do
próprio plano de não ajaxificar a troca de categoria pela mesma razão.

**Dois testes existentes tinham a ordem alfabética como premissa não
declarada.** `Dado_ProdutoForaDeEstoque_Quando_AbrirOCatalogo...` e o teste
de posição do "Box 3" assumiam a primeira página em "Nome (A-Z)" — a mudança
do padrão para "Melhor avaliados" (RF-16) quebrou os dois. Corrigidos fixando
`?ordenacao=NomeAZ` explicitamente nesses testes, já que a intenção deles
sempre foi achar um produto pelo nome, não exercitar a ordenação padrão.

**Quatro testes de integração de `Avaliacao` usavam um único autor avaliando o
mesmo produto várias vezes** — o índice único (RF-15) passou a recusar isso.
Corrigidos com autores distintos por avaliação; o comportamento que cada teste
prova (ordenar por nota, por data, por votos, contar por nota) não dependia de
ser a mesma pessoa.

**`ToHaveCountAsync` do Playwright contava pontos escondidos com `display:none`
numa tentativa inicial de teste da vitrine** — herdado do mesmo cuidado
registrado na `013`; não se repetiu aqui porque a vitrine não foi tocada por
esta feature, mas o padrão (`:visible`) foi reaproveitado onde fazia sentido.

**Timeouts intermitentes de até 15 minutos durante a suíte E2E** vieram de
processos `dotnet` órfãos de execuções anteriores interrompidas (e, num caso,
de rodar uma instância manual de diagnóstico ao mesmo tempo que a suíte).
Confirmado com `curl` direto contra uma instância isolada (resposta em
40-400ms) que não era defeito de código — encerrar os processos órfãos e
repetir resolveu em todos os casos. Não é um achado de aplicação; registrado
para quem depurar uma lentidão parecida no futuro não perder tempo
desconfiando do código primeiro.

## Verificado ao vivo (T041 a T044), não só por teste

- Filtrar uma subcategoria, trocar a ordenação e paginar, com captura de tela
  antes/depois: a contagem, a grade e a paginação trocam sem piscar o resto
  da página; o checkbox e o seletor de ordenação mantêm o estado.
- 375px: o conteúdo do catálogo (barra lateral, grade, paginação) cabe na
  largura da tela; o cabeçalho compartilhado continua estourando à direita,
  defeito pré-existente da `009`, fora de escopo.
- Teclado e JavaScript desligado: cobertos por teste automatizado
  (`Dado_NavegacaoPorTeclado_Quando_TrocarDePaginaPelaPaginacao...` e
  `Dado_JavaScriptDesligado_Quando_FiltrarOrdenarEPaginar...`), não repetidos
  manualmente à parte — o teste de JS desligado é justamente o que substituiu
  a verificação anterior que tinha sido dada como provada sem estar (plano §7).

## Não verificado

- Comportamento da atualização parcial com conexão lenta ou latência alta
  real (só a falha total foi simulada via interceptação de rota). O
  indicador de carregamento existe e tem atraso mínimo antes de aparecer,
  mas não foi cronometrado contra uma rede de verdade.
- Uso simultâneo por múltiplas abas da mesma sessão (duas abas filtrando o
  catálogo ao mesmo tempo) — não é cenário que a spec pede, mas não foi
  descartado por teste.
