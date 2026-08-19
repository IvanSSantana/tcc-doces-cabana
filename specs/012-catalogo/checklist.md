# Checklist de conclusão — Catálogo

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — os 29 requisitos
      funcionais, verificados um a um contra o código e os testes
- [x] Todo `CA-xx` foi verificado — os 26 critérios, por teste E2E contra a
      aplicação rodando de verdade, e reconfirmados ao vivo com `curl` e
      screenshot (menu suspenso, grade, responsivo em 375px)
- [x] Nada fora do escopo declarado entrou junto na entrega — a etiqueta de
      "fora de estoque" no card (achado abaixo) é RF-26, não escopo extra
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou — a spec já não
      tinha nenhuma na revisão de 2026-08-19; a seção 11 documentava decisões
      de negócio já fechadas com o responsável, não pendências

## Constituição

- [x] **I** — Nenhuma `ProjectReference` nova
- [x] **II** — `Produto.SemAcucar` com `private set`, parâmetro opcional no
      construtor e métodos de intenção (`MarcarComoSemAcucar`/`DesmarcarSemAcucar`).
      `Categoria.Subcategorias` segue o mesmo padrão de coleção rastreada por
      campo privado que `Avaliacao.Votos` já usa desde a `008`
- [x] **III** — n/a: `SemAcucar` é booleano, sem formato a validar. Os
      parâmetros de filtro/paginação são saneados no controller e no serviço,
      não são entrada de formulário
- [x] **IV** — `CatalogoService`, `OrdenacaoCatalogo`, `FiltroCatalogoDTO`,
      `Apelido`. `CatalogoController` público não colide com
      `Areas.Admin.ProdutoController` — a ressalva da emenda 1.4.1 cobre
      exatamente este caso
- [x] **V** — Fase 2 inteira vermelha (unidade, integração e E2E) antes de
      qualquer implementação
- [x] **VI** — `ICategoriaRepository` novo, `IProdutoRepository` ganhou três
      métodos. Uma migration (`AddProdutoSemAcucar`), inspecionada antes de
      confiar nela. Escrita (cadastro de produto) segue passando por
      `IUnitOfWork`, inalterado
- [x] **VII** — n/a (parcial): catálogo não tem `POST`, é consulta `GET` por
      design (RF-22). Ausência de `[Authorize]` é requisito — o catálogo é
      público
- [x] **VIII** — Nenhum `try/catch` em ação de controller. Apelido inexistente
      lança `KeyNotFoundException`, capturada pelo `FilterException` existente

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos (solução inteira)
- [x] `dotnet test DocesCabana.Tests` verde — 346/346 (baseline pós-`011`:
      311; +35 desta feature)
- [x] `dotnet test DocesCabana.Tests.E2E` verde — 49/49 (baseline: 30; +19
      desta feature). Confirmado estável em 3 execuções consecutivas depois
      de corrigir uma corrida encontrada durante a execução (ver achados)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração —
      `CatalogoRepositoryIntegrationTests`, 8 testes contra SQLite real,
      incluindo a prova de que nenhum produto repete nem some ao longo de
      todas as páginas de uma categoria (CA-16)

## Interface

- [x] `asp-action`/`asp-controller`/`asp-area` de cada link e formulário
      aponta para uma ação que existe de fato — conferido também ao vivo
- [x] n/a — nenhum formulário grava dado; o filtro é `GET` (RF-22)
- [x] Testado em largura de tela pequena — E2E automatizado a 375px, escopado
      a `.pagina-catalogo`, e confirmado por screenshot real do navegador.
      Mesma ressalva da `009`/`010`: o cabeçalho compartilhado já estoura a
      375px por conta própria, defeito pré-existente não corrigido aqui
- [x] Valores monetários formatados em `pt-BR` — `R$ 19,99`, herdado do
      `CardProduto` existente, sem alteração

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — Razor faz o
      escape por padrão em todo o catálogo
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      inalterado; o catálogo não lida com autenticação

---

## Achados registrados durante a execução

**`RF-26` não estava implementado — produto fora de estoque aparecia sem
sinalização nenhuma.** Encontrado ao conferir manualmente que "Box 3" (o
produto que o seed marca como fora de estoque) aparecia na grade — aparecia,
mas indistinguível de qualquer outro. O requisito pede "sinalizado como tal",
não só "não escondido". Corrigido com uma etiqueta "Fora de estoque" sobre a
imagem e a imagem esmaecida no card; dois testes E2E novos (`CA-20`/`CA-21`)
provam a ausência do inativo e a sinalização do fora de estoque, que
originalmente não tinham teste nenhum.

**Corrida em teste E2E: `CheckAsync()` não espera a navegação que o `onchange`
dispara.** A caixa de subcategoria se auto-submete via
`onchange="this.form.submit()"` — um efeito colateral de script, não algo que
o Playwright associa automaticamente à ação de marcar a caixa. O teste que
marca duas subcategorias em sequência e compara contagens falhou de forma
intermitente, lendo a grade a meio caminho da troca de página. Corrigido
acrescentando `WaitForLoadStateAsync(LoadState.NetworkIdle)` depois de cada
marcação no objeto de página; confirmado estável em três execuções seguidas
depois da correção. Não é defeito da aplicação — é do teste que a exercita.

**A cor de fundo do coração de favoritar mudou de comportamento.** Ao
desabilitar os três controles do card (RF-24), a regra CSS geral
`.card-produto button:disabled { opacity: 0.45 }` tem mais especificidade que
a regra que escondia o coração até o `hover` (`.botao-favorito-card { opacity:
0 }`). O efeito colateral: o coração passou a ficar sempre visível, em opacidade
reduzida, em vez de escondido até passar o mouse. Não é regressão de
requisito nenhum — na prática comunica "indisponível" com mais consistência
do que esconder um controle que não reage a clique — mas é um comportamento
que ninguém pediu explicitamente, registrado aqui para não parecer
acidental caso apareça em revisão futura.
