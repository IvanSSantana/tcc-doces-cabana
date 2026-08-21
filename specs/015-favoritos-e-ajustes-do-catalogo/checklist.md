# Checklist de conclusão — Favoritos e ajustes do catálogo

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — os 27 requisitos,
      verificados um a um contra o código e os testes
- [x] Todo `CA-xx` foi verificado — os 24 critérios, a maioria por teste E2E
      contra a aplicação rodando de verdade, e reconfirmados ao vivo com
      captura de tela (favoritos, cartão do catálogo, mobile 375px, teclado)
- [x] Nada fora do escopo declarado entrou junto na entrega — os dois achados
      corrigidos abaixo (bloco colorido no coração, mensagem de vazio ausente)
      são consequência direta de RF-01/RF-02/RF-11, não escopo extra
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou — a pendência da
      seção 10 (ordem das categorias no cabeçalho) é decisão de negócio
      herdada da `013`/`014`, não indefinição desta feature

## Constituição

- [x] **I** — nenhuma `ProjectReference` nova
- [x] **II** — `Favorito` não mudou; nenhuma entidade nova
- [x] **III** — parcial, com desvio justificado no plano §10: favoritar não
      recebe entrada de usuário para validar (o identificador vem do próprio
      cartão que o sistema imprimiu), só a invariante de aplicação (produto
      existe e está público). A guarda de `returnUrl` é defesa de borda, não
      validação de formulário
- [x] **IV** — `IFavoritoRepository`, `FavoritoService`, `FavoritoController`,
      `favorito.js`, `AlternarFavorito`/`Alternar` em português
- [x] **V** — cada fase teve teste vermelho antes da implementação; a ordem
      das fases 2 a 6 foi respeitada (retorno no login e favoritar simples
      antes da intenção do visitante)
- [x] **VI** — `FavoritoService.Alternar` grava por `IFavoritoRepository` e
      fecha com `IUnitOfWork.SalvarAlteracoes`. **Nenhuma migration** — a
      tabela `Favorito` existe desde a `003`, com a chave composta que já
      garante RN-01 no banco
- [x] **VII** — `[HttpPost]` + `[ValidateAntiForgeryToken]` no alternar;
      `[Authorize]` na lista; PRG no caminho sem script. O caminho assíncrono
      não redireciona — desvio justificado no plano §10 (não há histórico de
      navegador para um `fetch` recarregar)
- [x] **VIII** — produto inexistente ou fora do catálogo público lança
      `KeyNotFoundException`, capturada pelo filtro global

## O que foi provado, e como

| Requisito | Prova |
|---|---|
| RF-01/RF-02 (marcar/indicar) | `FavoritoServiceTests` (unidade) + `FavoritosTests` CA-01/CA-02 (E2E) |
| RF-03/RF-04 (com e sem JS) | CA-04 (sem recarga) e CA-05 (`JavaScriptEnabled = false`, botão associado por `form=` fora de qualquer `<form>` aninhado) |
| RF-05 (toque) | CA-06 — opacidade computada, não `ToBeVisibleAsync` (que ignora `opacity: 0`) |
| RF-06/RF-07 (visitante) | CA-07/CA-08 — 3 execuções seguidas verdes (fluxo mais frágil da feature) |
| RF-08 a RF-12 (lista) | `FavoritosTests` CA-09 a CA-13 |
| RF-13/RF-14 (retorno no login) | `AutenticacaoControllerTests` (unidade) + CA-14/CA-15 |
| RF-15 a RF-18 (cartão) | `CatalogoTests` CA-16/CA-17 (E2E) + captura de tela lado a lado |
| RF-19/RF-20 (trilha) | CA-20 — `text-transform` computado e cor do último item |
| RF-21 a RF-23 (Ver todas) | CA-21/CA-22 — posição via `BoundingBox`, com e sem JavaScript |
| RF-24 a RF-27 (achados) | CA-23 (nenhuma resposta 404 observada) e CA-24 (link de navegação) |

## Achados durante a implementação, registrados aqui em vez de corrigidos em silêncio

**Form aninhado dentro do form do catálogo.** O primeiro desenho do coração
era um `<form>` próprio em volta do botão. Como o cartão pode estar dentro do
`<form method="get" id="formulario-catalogo">`, o navegador ignorava o
aninhamento (HTML não permite form dentro de form) e submetia o formulário
externo — o clique no coração navegava para `/Catalogo/doces` carregando os
campos do favorito como query string. Corrigido com o atributo `form=` do
HTML5: o botão se associa a um `#formulario-favorito` que vive fora de
qualquer form da página (no `_Layout`), funcionando independente de onde o
cartão está no documento.

**`UseStatusCodePagesWithReExecute` engolia o 401.** A ação `Alternar`
devolvia `Unauthorized()` (sem corpo) para o visitante; o middleware da
`008` reexecuta qualquer resposta de erro sem corpo para `/Home/NaoEncontrado`,
transformando o 401 num 404 antes de chegar ao script — que não tinha como
distinguir "precisa entrar" de "sumiu". Corrigido devolvendo
`StatusCode(401, new { autenticado = false })`, que escreve corpo e evita a
reexecução.

**FontAwesome converte `<i>` em `<svg>` e apaga a tag original.** O kit
hospedado (`_Footer.cshtml`) reprocessa qualquer `<i class="fa-...">` da
página, inclusive novos elementos inseridos por script — mas troca classe
num `<i>` que já virou `<svg>` não faz nada. Corrigido substituindo o ícone
inteiro por um `<i>` novo a cada alternância; o observador de mutações do
próprio kit o converte de novo. Os testes E2E precisaram do mesmo ajuste —
buscavam só `i.favoritado`, que nunca resolvia depois da primeira conversão.

**Corrida entre `NetworkIdle` e o `.then()` do fetch.** `WaitForLoadStateAsync`
marca o fim da requisição de rede, não o fim do processamento que troca o
ícone — havia uma folga real entre as duas. Testes que liam o estado uma vez
só, logo após a rede ficar ociosa, pegavam o ícone antigo por uma fração de
segundo. Trocado por asserções com retry automático (`Expect(...).ToHaveCountAsync`).

**Achado de teste, não de aplicação: `IUrlHelper.IsLocalUrl` é membro real da
interface, não extensão.** Um mock sem `Setup` devolve `false` sempre; os
primeiros testes de retorno no login passavam por sorte (o caso "externo"
esperava `false` mesmo). Corrigido configurando o mock com a mesma lógica da
implementação real.

**Dois achados de verificação ao vivo (T059), não capturados por nenhum
teste automatizado até então:**
- O fundo sólido colorido do coração (`background-color: #D93B26` em
  `:has(.favoritado)`) era inofensivo enquanto `EstaFavorito` nunca era
  verdadeiro de verdade (spec 012). Assim que favoritar passou a funcionar,
  virou um bloco colorido atrás do próprio coração. Removido; a opacidade de
  espreitar (0.6 sem hover) continua.
- Desfavoritar o último item da lista de favoritos não mostrava a mensagem
  de vazio: ela só existia na marcação quando o servidor já renderizava a
  lista vazia, e o script apenas removia o cartão, sem inserir a mensagem.
  Corrigido: os dois blocos (grade e mensagem) vivem na marcação o tempo
  todo, alternados por `hidden`; `favorito.js` alterna o atributo quando a
  grade fica sem filhos. Um teste novo (`Dado_UltimoFavorito_Quando_Desfavoritar...`)
  cobre esse caminho, que o CA-11 original não alcançava.

## Verificado ao vivo (T059/T060), não só por teste

- Favoritar e desfavoritar do catálogo, recarregar e continuar marcado, abrir
  a lista, desfavoritar de lá e ver a mensagem de vazio aparecer — tudo
  comparado por captura de tela, antes e depois das duas correções acima.
- Cartão do catálogo comparado à referência visual lado a lado: imagem com
  fundo próprio, nome em caixa normal, preço e seletor na mesma linha, botão
  largo em coral — e o carrossel da home, intocado.
- Teclado: `Tab` alcança o coração, `Enter` aciona, e o foco não se perde.
- 375px: `scrollWidth` do `.pagina-catalogo` igual à largura da tela.

## Não verificado

- **CA-10** (produto que sai do catálogo público some da lista de favoritos e
  volta se reativado) foi verificado em unidade
  (`Dado_ProdutosFavoritadosDeUmUsuario_Quando_ListarDoUsuario...`) mas
  **não de ponta a ponta**: não existe tela administrativa para inativar um
  produto já favoritado e reativá-lo depois, então o caminho completo não foi
  exercitado pelo navegador.
- **O carrossel da página inicial não reflete o estado real de favorito.**
  `HomeController`/`VitrineProdutos` não foram tocados por este plano (fora
  do impacto por camada), então o coração ali sempre carrega `EstaFavorito =
  false` na primeira renderização — funciona (favorita de verdade ao
  clicar), mas o ícone inicial pode não corresponder ao que já está
  favoritado até a pessoa interagir com ele. Registrado para decisão futura,
  não corrigido silenciosamente.
