# Checklist de conclusão — Busca e endereços do catálogo

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — os 18 requisitos,
      verificados um a um contra o código e os testes (tabela abaixo)
- [x] Todo `CA-xx` foi verificado — 17 dos 19 por teste E2E contra a
      aplicação rodando de verdade; CA-01/CA-03/CA-08 (busca, acento, sem
      resultado) e o endereço legível também reconfirmados ao vivo por HTTP
      direto contra o servidor rodando (T060/T061 — **sem captura de tela
      desta vez**: a verificação foi por requisição HTTP e leitura do HTML
      devolvido, não por navegador aberto manualmente; registrado como
      limitação, não escondido)
- [x] Nada fora do escopo declarado entrou junto na entrega — os dois
      achados corrigidos (rota ambiente do formulário de busca, `.linha-dupla`
      nunca empilhando) são consequência direta de RF-01/RF-02 e RF-17,
      não escopo extra
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou — a pendência da
      seção 10 (ordem das categorias no cabeçalho) é decisão de negócio
      herdada da `013`/`014`/`015`, não indefinição desta feature

## Constituição

- [x] **I** — nenhuma `ProjectReference` nova. `TextoHelper` desceu de
      `Application` para `Domain` (só BCL); a seta continua apontando para
      dentro
- [x] **II** — `Produto.NomeNormalizado` tem `private set`, é derivado nos
      dois únicos pontos que mudam o nome (construtor e `AlterarNome`);
      nenhum caminho externo o atribui
- [x] **III** — parcial, com desvio justificado no plano §10: o termo de
      busca não recebe validador — não é persistido, não tem formato a
      violar, e termo vazio/sem resultado é comportamento especificado
      (RF-08/RF-09), não erro
- [x] **IV** — `TextoHelper`, `CriteriosDoCatalogoDTO`, `EnderecoDoCatalogo`,
      `NomeNormalizado`, `components/formulario.css` — tudo em português
      onde é de negócio
- [x] **V** — cada fase teve teste vermelho antes da implementação; a ordem
      das fases 3→4→5 foi respeitada (normalização antes do esquema, antes
      da busca) — a única exceção documentada é T005/T019, testes que já
      nasceram verdes por cobrirem invariante já correta (registrado nas
      próprias tarefas, não escondido)
- [x] **VI** — nenhuma escrita de caso de uso nesta feature (a busca é só
      leitura). A única escrita — `DbInitializer.PreencherNomesNormalizados`
      — é bootstrap de infraestrutura em `MVC`, e segue a convenção que o
      próprio arquivo já usa (`context.SaveChangesAsync()` direto, não
      `IUnitOfWork`); desvio registrado na T029, não silencioso. Migration
      `AddProdutoNomeNormalizado`
- [x] **VII** — nenhuma ação de escrita nova. Busca é `GET`, sem
      antiforgery nem PRG por não alterar estado
- [x] **VIII** — categoria desconhecida continua `KeyNotFoundException`;
      subcategoria desconhecida **não é erro** (RN-04) — filtro descartado,
      não capturado por filtro nenhum porque nunca chega a lançar

## O que foi provado, e como

| Requisito | Prova |
|---|---|
| RF-01 a RF-11 (busca) | `BuscaTests` (E2E, 12 testes) |
| RF-02 (acento/caixa) | `TextoHelperTests`, `CatalogoRepositoryIntegrationTests`, `BuscaTests` |
| RF-12 a RF-15 (endereço legível) | `ApelidoTests`, `CatalogoServiceTests`, `CatalogoControllerTests`, `CatalogoTests` (E2E) |
| RF-16 a RF-18 (cadastro de produto) | `FormularioTests` (não-regressão) + `CadastroDeProdutoTests` (E2E) |
| RN-01 a RN-06 | Unidade + integração, ver tabela do plano §7 |

## Testes

- [x] `dotnet build` sem warnings novos
- [x] `dotnet test` verde — **403 unidade, 117 E2E** (rodada limpa final,
      sem instabilidade)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração —
      `CatalogoRepositoryIntegrationTests` (acento, curinga SQL, preenchimento
      retroativo)

## Interface

- [x] Todo link e formulário aponta para uma ação que existe de fato
- [x] Testado em largura de tela pequena (375px) — `FormularioTests`,
      `CadastroDeProdutoTests`
- [x] Nenhum valor monetário ou data nova introduzida por esta feature

## Segurança

- [x] Nenhum segredo commitado
- [x] Termo de busca vai como parâmetro EF Core (ligado), nunca concatenado —
      confirmado por teste que curinga SQL não é interpretado
- [x] Mensagens de erro (nada encontrado) não vazam detalhe interno

## Achados durante a implementação, registrados aqui em vez de corrigidos em silêncio

**O formulário de busca do cabeçalho herdava o apelido de categoria
ambiente.** `asp-controller`/`asp-action` sem `asp-route-apelido` explícito
faz o gerador de endereço do ASP.NET Core reaproveitar o valor **ambiente**
da rota atual — buscando de dentro de `/Catalogo/doces`, o formulário
submetia para `/Catalogo/doces?termo=...` em vez de `/Catalogo?termo=...`,
violando RF-02 (a busca deveria varrer a loja inteira, não a categoria
aberta). Descoberto pelo primeiro teste de `BuscaTests` que buscava de
dentro de uma categoria. Corrigido com `asp-route-apelido="@((string?)null)"`
explícito — um valor `null` explícito sobrepõe o valor ambiente, ausência
de atributo não.

**`.linha-dupla` nunca empilhava em nenhuma largura de tela.** O plano
presumiu, ao planejar o cadastro de produto, que o componente já empilhava
em tela estreita "como o cadastro de administrador já faz" — não fazia:
não existe (e nunca existiu) `@media` nenhum para `.linha-dupla`. Achado ao
escrever o primeiro teste de CA-18. Corrigido **só para o cadastro de
produto** (`.pagina-cadastro-produto .linha-dupla`, em `cadastro_produto.css`,
não em `formulario.css`) — as outras duas telas que usam `.linha-dupla`
(cadastro de cliente, cadastro de administrador) continuam exatamente como
sempre estiveram, porque RF-18 proíbe esta feature de mudar a aparência
delas por conta própria. Se um dia isso for corrigido para as outras duas,
é decisão e feature própria.

**Achado de teste, não de aplicação: threshold de largura errado no
primeiro CA-18.** O teste original comparava a largura de um campo contra a
largura *total* do container do formulário, sem descontar o padding — e a
375px o `root font-size` cai para 14px (`site.css`, breakpoint em 768px, já
existente desde a baseline), então `1.5rem` de padding vale 21px ali, não
24px. Corrigido para descontar o padding computado antes de comparar.

**Um teste de `spec 015` já estava desatualizado antes desta feature
começar.** `specs/015-favoritos-e-ajustes-do-catalogo/spec.md` dizia
"Carrinho é a `016` da cadeia" quando o `specs/README.md` já apontava
`017` — uma correção que deveria ter acontecido num deslocamento anterior e
não aconteceu. Corrigido para `018` (o valor correto após o deslocamento
desta feature), junto da varredura da Fase 8.

**`ApelidoTests`/RN-03 nasceu verde.** O teste que prova que os apelidos de
subcategoria são distintos dentro de cada categoria real da loja passou de
primeira — `Apelido.De` já era genérico o bastante, herdado da `012`. Não
há violação do Princípio V aqui: o teste não tinha uma linha de produção
para "dirigir" porque a invariante já valia; ele existe para **sustentar**
a RN-03 contra regressão futura (um teste-guarda, não um teste-TDD no
sentido estrito), e isso está registrado na própria tarefa (T005), não
escondido atrás de um "passou de primeira, seguimos em frente".

## Não verificado

- **Nenhuma captura de tela foi feita nesta rodada** (T060/T061 usaram
  verificação HTTP direta contra o servidor rodando — login real, cookies
  reais, HTML devolvido de verdade — em vez de abrir um navegador e olhar).
  Cobre o funcional (contagens de busca corretas, endereço, formulário
  logado), mas não prova visualmente que a etiqueta de busca, o cadastro de
  produto e a trilha ficaram com a aparência pretendida — isso os testes
  E2E de layout (`FormularioTests`, `CadastroDeProdutoTests`,
  `CatalogoTests`) cobrem por medição de estilo computado, não por
  inspeção visual humana.
- **Ordenar por relevância** não foi implementado nem testado — está fora
  de escopo por decisão registrada na spec, seção 10.
