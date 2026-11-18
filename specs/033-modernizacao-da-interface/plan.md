# Plano — Modernização da interface

**ID:** 033-modernizacao-da-interface · **Criado em:** 2026-11-18
**Status:** Aprovado

---

## 1. Objetivo

A interface cresceu feature a feature, e cada página trouxe seus próprios
valores: há cerca de 290 cores fixas no CSS, cinco cremes quase iguais, 15
raios, 12 sombras, 13 breakpoints e cerca de 40 classes `.botao-*`, cada uma
com seu desenho. O resultado parece montado aos pedaços, e cada tela nova
copia a anterior em vez de usar uma base. Esta feature moderniza a loja e a
área administrativa **sem trocar a identidade**: Inter, Nothing You Could Do, o
verde `#055C40` e o vermelho `#D93B26` continuam. O que muda é a fundação
(tokens completos), os componentes (botão, campo, painel, cartão, título) e o
acabamento de todas as telas, com uma assinatura visual nova: o título
manuscrito vira **etiqueta de confeitaria**.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | **Escopo: loja + área administrativa**, num plano único executado em fases (seção 6). | Decisão do debate. Os tokens e componentes só cumprem o papel se todas as telas os usarem. Uma área de fora continuaria com a paleta própria (`--gestao-*`). | Só a loja: o admin ficaria com outra linguagem visual. Só as telas-chave: o resto herdaria tokens, mas manteria os botões e campos locais. |
| D2 | **Profundidade: fundação + componentes.** A estrutura e a composição de cada página continuam; mudam o acabamento, o ritmo e a hierarquia. | Decisão do debate. Reorganizar layouts (home nova, catálogo novo) é outra feature, e fica mais barata depois que a base existir. | Incluir layouts: plano grande demais e com risco de mexer em markup que o E2E usa. Só fundação: o usuário quase não veria diferença. |
| D3 | **Direção "etiqueta de confeitaria".** O `.titulo` (Nothing You Could Do) passa a ser alinhado à esquerda, com um traço feito à mão em `--cor-destaque` por baixo (SVG em `mask-image`, revelado uma vez com `clip-path` em 600 ms). Cartões viram "papel": superfície branca sobre fundo creme, borda de 1px, sombra mínima. O verde fica no cabeçalho e nas ações principais. | Decisão do debate. O manuscrito é o traço mais característico da marca, e hoje ele aparece como um título centralizado qualquer. A etiqueta é o único elemento ousado; o resto fica contido. | "Vitrine de balcão" (fotos de ponta a ponta, faixas verdes): mais genérica de e-commerce e menos artesanal. "Refino neutro": sem identidade nova. |
| D4 | **Os neutros viram uma rampa única e quente**, derivada só de valores que já existem no CSS: `#FFFFFF` (superfície), `#FDFBF8` (fundo da página), `#F2EFE9` (superfície rebaixada: busca, campos), `#EAE3D8` (divisor), `#DAD4CE` (borda), `#6D6460` (texto suave, contraste 5,5:1 sobre o fundo) e `#1E1E1E` (tinta). Os tons por página (`#FDF5F1`, `#FDF6F4`, `#FCF5F3`, `#FAFAFA`) deixam de existir. | Decisão do debate. Nenhum valor novo entra na paleta: a rampa só escolhe, entre os existentes, um por função. | Manter um creme por página: deixa a variação sem motivo que o debate quis eliminar. |
| D5 | **Os tokens moram em `wwwroot/css/tokens.css`, carregado primeiro pelos dois layouts.** Grupos: cor, tipografia, espaço (base 4px), raio (`--raio-pequeno` 8px, `--raio-medio` 12px, `--raio-grande` 20px, `--raio-pilula` 999px), sombra (3 níveis), movimento (duração e curva) e largura de conteúdo. | Um arquivo só de variáveis deixa claro onde a decisão de design mora. O `site.css` atual mistura tokens, reset e sobras do Bootstrap. | Manter tudo em `site.css`: o arquivo continuaria acumulando regras de página. |
| D6 | **Os tokens antigos ganham nome claro, com alias durante a migração:** `--cor-texto` (que é `#FFFFFF`) vira `--cor-sobre-primaria`, e `--cor-texto-secundario` vira `--cor-tinta`. Os aliases antigos são removidos no último passo. `--cor-primaria`, `--cor-destaque`, `--cor-botao-padrao`, `--fonte-principal` e `--fonte-secundaria` ficam com o nome atual. | "Texto" para branco confunde quem lê o CSS. O alias deixa cada fase compilar e funcionar sozinha. | Renomear tudo de uma vez: um passo gigante, difícil de revisar. |
| D7 | **As paletas locais (`--catalogo-*`, `--produto-*`, `--gestao-*`, `--institucional-*`, `--texto-rodape`) são removidas**, e cada página passa a usar os tokens globais. | São cópias dos tokens globais com outro nome, ou os cremes que D4 elimina. | Mantê-las apontando para os globais: dois nomes para a mesma coisa (Princípio IV, unicidade de nome). |
| D8 | **Botão vira componente: `.botao` + modificador** (`--primario`, `--secundario`, `--perigo`, `--fantasma`, `--icone`, `--pequeno`), em `components/botao.css`, carregado globalmente. Cada view troca o desenho local pelo componente. A classe antiga só fica no markup se JS ou E2E a usar (`.botao-fechar`, `.botao-favorito-card`) ou se ela ainda carregar regra de posição da página. | Hoje há cerca de 40 classes de botão por página, com raios, alturas e hovers diferentes. | Unificar por CSS sem mudar o markup (seletor agrupando as 40 classes): mantém a fragmentação escondida. |
| D9 | **Superfície vira componente: `.painel`** (fundo branco, borda, raio médio, sombra 1), em `components/painel.css`, global. É usado por conta, pedidos, resumo do carrinho e gestão. | Esses quatro arquivos desenham a mesma caixa com valores diferentes. | Um painel por página: é o problema atual. |
| D10 | **`components/formulario.css` passa a ser o componente de campo, carregado globalmente** pelos dois layouts, e é reescrito sobre os tokens. As views deixam de linká-lo individualmente. | Ele já é usado em 9 views, e os modais da gestão e do carrinho têm campos também. Carregar uma vez evita campo sem estilo quando uma página esquece o link. | Manter o link por view: é a fonte do esquecimento. |
| D11 | **O cartão de produto troca o hover.** Sai a borda preta com sombra `rgba(0,0,0,.74)`; entra papel branco permanente com sombra 1, que sobe para sombra 2 com `translateY(-2px)` no hover. A foto amplia 3% dentro do quadro. O coração do favorito continua aparecendo no hover em dispositivos com mouse. | A sombra atual é a mais pesada do site e destoa do resto. O cartão sempre visível como papel é a direção D3. | Manter o cartão transparente: na grade, os produtos ficam soltos no fundo. |
| D12 | **Foco visível global com `:focus-visible`** (contorno 2px `--cor-primaria`, deslocamento 2px; branco sobre o cabeçalho verde). Sai o anel azul `#258cfb` e o seletor inválido `.botao:ativo:focus`. **`prefers-reduced-motion` também vira regra global** em `base.css`, e as 8 cópias por arquivo saem. | O foco atual só cobre classes do Bootstrap que o layout principal nem carrega. | Manter por arquivo: oito cópias que já divergiram. |
| D13 | **O `html` deixa de ser 14px no celular e fica 16px em toda largura.** Os títulos usam `clamp()`. | Com 14px, todo `rem` encolhe no celular, e campo com fonte menor que 16px faz o iOS dar zoom ao focar. | Manter 14px: preserva o tamanho atual, mas mantém o zoom indesejado. |
| D14 | **Quatro breakpoints oficiais: 480, 768, 1024 e 1280px**, documentados em comentário no `tokens.css`. Os 13 atuais são reescritos para o mais próximo. | Variável CSS não funciona em `@media`, então a convenção precisa ser escrita. Treze cortes diferentes geram faixas onde metade da página já mudou e a outra não. | Manter os cortes atuais: cada página continua quebrando num ponto diferente. |
| D15 | **O Bootstrap sai do `_LayoutNaoAutenticado`**, e a pasta `wwwroot/lib/bootstrap` é apagada. As regras `.form-floating` e `.form-control` do `site.css` saem junto. | Nenhuma view usa classe do Bootstrap (verificado com grep). Hoje ele só aplica o *reboot* dele nas telas de autenticação, que por isso renderizam diferente do resto. | Carregar o Bootstrap nos dois layouts: 200 KB para nada. |
| D16 | **Limpeza de sobras do template:** apagar `Views/Shared/_Layout.cshtml.css` (azul `#1b6ec2`, `.footer` absoluto) e o link `DocesCabana.MVC.styles.css`. Tirar do `site.css` o `margin-bottom: 60px` do `body` e o `position: relative` do `html`. Corrigir o `_HeaderNaoAutenticado.cshtml`, que tem um `<head>` dentro do `body` e um `</html>` solto. Trocar o título da aba de `DocesCabana.MVC` para `Doces Cabana`. | São restos do *scaffold* que nenhuma feature assumiu. | Deixar para outra feature: eles atrapalham a fundação (o azul vaza no `a` do layout). |
| D17 | **A regra global `section { max-width; padding }` sai do `header.css` e vai, igual, para o `base.css`.** | Muitas views dependem dela sem saber. Mudar o seletor agora exigiria tocar todas as `<section>` sem ganho visual. Ela só muda de casa porque não é do cabeçalho. | Trocar por classe `.conteudo` em toda view: churn grande fora do objetivo. |
| D18 | **A fonte do Google sai do `@import` do `site.css` e vira `<link>` nos dois layouts**, depois do `preconnect` que já existe. O peso 600 do Nothing You Could Do deixa de ser pedido: a fonte só tem 400, e o 600 gera negrito falso. | O `@import` dentro do CSS atrasa o download da fonte. | Manter: a página pisca com a fonte de sistema por mais tempo. |
| D19 | **`cadastro_produto.css` (28 linhas, usado só em `Admin/Produto/Index`) é fundido em `gestao-produtos.css` e apagado.** | É o único arquivo CSS com `_` no nome e só existe para uma tela que já tem CSS próprio. | Renomear para kebab-case: continuaria sendo um arquivo para 28 linhas da mesma tela. |
| D20 | **Classes e atributos que JS ou E2E usam não mudam de nome.** Os componentes entram como classes adicionais no mesmo elemento. Lista mínima a preservar: `.container-autenticacao`, `.barra-pesquisa`, `.pagina-conta`, `.resumo-erros`, `.mensagem-erro`, `.resumo-sucesso`, `.mensagem-sucesso`, `.card-produto`, `.botao-fechar`, `.botao-favorito-card`, `.valor-quantidade-carrinho`, `.total-itens-carrinho`, `.total-detalhe-pedido`, `.pedidos-vazio`, `.formulario-endereco`, classes do carrossel e todos os `data-*`. | O E2E e os scripts dependem desses nomes. Mudar o visual não pode quebrar comportamento. | Renomear e atualizar testes e JS: o risco sobe sem ganho para quem usa. |
| D21 | **Nenhum teste novo.** A verificação é a suíte atual verde, uma varredura por `grep` (seção 7) e uma revisão visual por tela, antes e depois. | Não há mudança de comportamento. O Princípio V exclui layout, responsividade e foco por teclado do E2E. | Um teste xUnit que varre os `.css` atrás de cor fixa: não cabe em nenhuma pasta que o Princípio V lista, e o `grep` da seção 7 prova o mesmo. |

**Fora de escopo:** reorganizar a composição das páginas (home com abertura
nova, catálogo e produto redesenhados) — fica para uma feature depois desta;
modo escuro; trocar o kit do Font Awesome; tirar o `style="display: none"` do
`#requisitos-senha` (o JS o alterna); a validação no cliente quebrada do
`Admin/Administrador/Cadastro` (ver seção 8, risco R5).

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Só `DocesCabana.MVC` (views e `wwwroot`) muda. Nenhuma `ProjectReference` nova. |
| II | Domínio rico e auto-validante | n/a | O domínio não é tocado. |
| III | Validação nas duas barreiras | n/a | Nenhuma regra de validação muda. Os campos só ganham estilo novo. |
| IV | Nomenclatura em português | ✅ OK | Arquivos, classes e tokens novos em português: `tokens.css`, `base.css`, `botao.css`, `painel.css`, `.botao--primario`, `--cor-sobre-primaria`, `--raio-medio`. D7 elimina pares de nomes para o mesmo token. D19 tira o único CSS com `_` no nome. |
| V | Testes escritos antes | ✅ OK | Não há comportamento novo para testar antes (D21). Cada fase termina com `dotnet test` completo, inclusive E2E, provando que o comportamento não mudou. |
| VI | Repositório + commit via UnitOfWork | n/a | Sem persistência. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | Nenhuma ação de controller muda. Os formulários mantêm `@Html.AntiForgeryToken()` e os mesmos `asp-action`. Só mudam classes. |
| VIII | Tratamento de erro por camada | ✅ OK | Nenhuma ação muda. As telas de erro (`Error`, `NaoEncontrado`, `AcessoNegado`) só ganham estilo. |

## 4. Impacto arquivo por arquivo

Caminhos relativos a `DocesCabana.MVC/`. Os outros projetos não mudam.

### Fundação

| Arquivo | Ação | O quê |
|---|---|---|
| `wwwroot/css/tokens.css` | Criar | Todos os tokens (D4, D5, D6) + comentário com os breakpoints (D14). Aliases `--cor-texto` e `--cor-texto-secundario` até o passo 9. |
| `wwwroot/css/base.css` | Criar | Reset, `body` (fundo `--neutro-fundo`, Inter, cor `--cor-tinta`), `html` 16px (D13), `section` global (D17), `:focus-visible` (D12), `prefers-reduced-motion` global, `.titulo` etiqueta com modificador `.titulo--verde` (D3), `.visualmente-oculto`. |
| `wwwroot/css/site.css` | Apagar | O conteúdo útil passa para `tokens.css` e `base.css`. As regras do Bootstrap e as sobras do template saem (D15, D16, D18). |
| `Views/Shared/_Layout.cshtml` | Alterar | `<link>` da fonte (D18). Ordem dos estilos: `tokens`, `base`, `botao`, `painel`, `formulario`, depois os componentes atuais. Remove o link `DocesCabana.MVC.styles.css`. Título da aba `Doces Cabana`. |
| `Views/Shared/_LayoutNaoAutenticado.cshtml` | Alterar | Mesma ordem de estilos. Remove o CSS e o JS do Bootstrap e o `jquery.min.js` (D15; nenhuma tela desse layout chama `_ValidationScriptsPartial`). Título da aba. |
| `Views/Shared/_Layout.cshtml.css` | Apagar | D16. |
| `Views/Shared/_HeaderNaoAutenticado.cshtml` | Alterar | Tirar o `<head>` e o `</html>` soltos. O link do `header-nao-autenticado.css` passa para o layout. |
| `wwwroot/lib/bootstrap/` | Apagar | D15. |

### Componentes

| Arquivo | Ação | O quê |
|---|---|---|
| `wwwroot/css/components/botao.css` | Criar | `.botao` e modificadores (D8): altura 44px, raio `--raio-pilula`, estados `:hover`, `:active`, `:disabled`, `[aria-busy]`. |
| `wwwroot/css/components/painel.css` | Criar | `.painel`, `.painel__cabecalho`, `.painel--rebaixado` (D9). |
| `wwwroot/css/components/formulario.css` | Reescrever | Os 20 hex viram tokens. Campo com fundo `--neutro-rebaixado`, borda `--neutro-borda`, foco verde. Mensagens de erro e sucesso usando `--cor-destaque` e `--cor-primaria` com fundo claro. Classes atuais mantidas (D20). |
| `wwwroot/css/components/card-produto.css` | Reescrever | Cartão-papel e hover novo (D11). Os 17 hex viram tokens. |
| `wwwroot/css/components/estrelas-nota.css` | Alterar | Cor das estrelas via token. |
| `wwwroot/css/components/vitrine-produtos.css` | Alterar | Setas como `.botao--icone`, espaçamento por token. |
| `Views/Shared/Components/CardProduto/Default.cshtml` | Alterar | Botões de quantidade e de sacola com as classes do componente `.botao`. |
| `Views/Shared/Components/VitrineProdutos/Default.cshtml` | Alterar | Setas com `.botao .botao--icone` (as classes do carrossel ficam). |

### Casca (cabeçalho, rodapé, modais)

| Arquivo | Ação | O quê |
|---|---|---|
| `wwwroot/css/components/header.css` | Alterar | O logo sai do `12vh` e passa a altura fixa com `clamp()`. Tokens no lugar dos 10 hex. A regra `section` sai (D17). A busca usa `--neutro-rebaixado` e raio pílula. O submenu usa `.painel` e sombra 2. |
| `wwwroot/css/components/header-nao-autenticado.css` | Alterar | Mesma altura de logo do cabeçalho completo. |
| `wwwroot/css/components/footer.css` | Alterar | `--texto-rodape` sai (D7). Tokens. |
| `wwwroot/css/components/modal-login.css` | Alterar | Raio grande, sombra 3, `::backdrop` com tinta translúcida. Tokens. |
| `wwwroot/css/components/aviso-carrinho.css` | Alterar | Tokens; botões pelo componente. |
| `Views/Shared/Components/Header/Default.cshtml` | Alterar | Classes de componente nos botões de ação. As classes atuais ficam. |
| `Views/Shared/_Footer.cshtml` | Alterar | Remove o `<link>` próprio (vai para o layout). |
| `Views/Shared/_ModalLogin.cshtml` | Alterar | Botões com `.botao`. |

### Páginas da loja

| Arquivo | Ação | O quê |
|---|---|---|
| `Views/Home/Index.cshtml` | Alterar | Tira o `style="color: var(--cor-primaria)"` e usa `.titulo--verde`. |
| `wwwroot/css/components/carrossel.css` | Alterar | Tokens (9 hex), setas e indicadores com foco visível. |
| `wwwroot/css/components/categorias.css` | Alterar | Cartões de categoria como papel, raio médio. |
| `wwwroot/css/pages/catalogo.css` | Alterar | Remove `--catalogo-*` (D7). Breakpoints D14. Barra lateral em `.painel`. |
| `Views/Catalogo/_BarraLateral.cshtml`, `_Paginacao.cshtml`, `_ResultadoCatalogo.cshtml` | Alterar | Classes de componente em botões e na paginação. |
| `wwwroot/css/pages/produto.css` | Alterar | Remove `--produto-*` (D7). Fundo `#FDF5F1` sai (D4). Tokens nos 21 hex. Modal de avaliação com raio grande e sombra 3. |
| `Views/Produto/Detalhes.cshtml`, `_BlocoAvaliacoes.cshtml`, `_CartaoAvaliacao.cshtml`, `_FormularioAvaliacao.cshtml` | Alterar | Botões pelo componente; cartões de avaliação como `.painel`. |
| `wwwroot/css/pages/carrinho.css` | Alterar | O maior arquivo (941 linhas, 43 hex). Tokens; resumo e passos do fechamento em `.painel`; botões pelo componente; breakpoints D14. Remove o que o componente passa a cobrir. |
| `Views/Carrinho/Index.cshtml`, `_ItensDoCarrinho.cshtml`, `_PassosDoFechamento.cshtml`, `_PassoConta.cshtml`, `_PassoEndereco.cshtml`, `_PassoPagamento.cshtml`, `_OpcoesDeFrete.cshtml`, `ConfirmarEsvaziar.cshtml` | Alterar | Classes de componente. As classes `formulario-*-carrinho` e os `data-*` ficam (D20). |
| `Views/Pedido/Confirmacao.cshtml`, `Meus.cshtml`, `Detalhe.cshtml` | Alterar | `.painel` e `.botao`. |
| `wwwroot/css/pages/pedidos.css` | Alterar | Tokens (17 hex). Selo de status com raio pílula. |
| `wwwroot/css/pages/conta.css` | Alterar | Tokens (17 hex); blocos em `.painel`. |
| `Views/Conta/Index.cshtml`, `Enderecos.cshtml`, `FormularioEndereco.cshtml`, `_FormularioEndereco.cshtml`, `Views/Shared/_MenuDaConta.cshtml` | Alterar | Componentes. Os links do `formulario.css` saem (D10). |
| `wwwroot/css/pages/favoritos.css`, `Views/Favorito/Index.cshtml` | Alterar | Tokens; estado vazio com `.botao--primario` para o catálogo. |
| `wwwroot/css/pages/autenticacao.css` | Alterar | Tokens. Revisar o espaçamento depois que o *reboot* do Bootstrap sair (D15). |
| `Views/Autenticacao/Login.cshtml`, `Cadastro.cshtml`, `EsqueceuSenha.cshtml`, `RedefinirSenha.cshtml` | Alterar | `.botao` nos botões de envio. Os links do `formulario.css` saem. `.container-autenticacao` fica. |
| `wwwroot/css/pages/institucional.css`, `Views/Institucional/QuemSomos.cshtml`, `Privacidade.cshtml`, `_BlocoInstitucional.cshtml` | Alterar | Remove `--institucional-*` (D7). A régua `#E8A79A` passa a ser a etiqueta (D3). |
| `Views/Home/AcessoNegado.cshtml`, `NaoEncontrado.cshtml`, `Views/Shared/Error.cshtml` | Alterar | `.titulo` e um `.botao--primario` de volta para a loja. |

### Área administrativa

| Arquivo | Ação | O quê |
|---|---|---|
| `wwwroot/css/pages/gestao-produtos.css` | Alterar | Remove `--gestao-*` (D7). Absorve `cadastro_produto.css` (D19). Os 41 hex viram tokens. `.botao-principal-gestao`, `.botao-secundario-gestao` e `.botao-perigo-gestao` dão lugar ao componente. Tabela em `.painel`. |
| `wwwroot/css/pages/cadastro_produto.css` | Apagar | D19. |
| `wwwroot/css/pages/administradores.css` | Alterar | Tokens (10 hex); `.botao-novo-administrador` dá lugar ao componente. |
| `Areas/Admin/Views/Produto/Index.cshtml`, `_ResultadoDaGestao.cshtml`, `_FormularioProduto.cshtml`, `_DetalheProduto.cshtml`, `_PaginacaoDaGestao.cshtml` | Alterar | Componentes. O link do `cadastro_produto.css` sai. `data-formulario-produto` e `data-inativar-id` ficam. |
| `Areas/Admin/Views/Administrador/Index.cshtml`, `Cadastro.cshtml` | Alterar | Componentes; os links do `formulario.css` saem. |

### Testes

| Arquivo | Ação | O quê |
|---|---|---|
| — | — | Nenhum (D21). A suíte atual, inclusive `DocesCabana.Tests.E2E`, precisa continuar verde sem alteração. |

## 5. Contratos e modelo de dados

Sem contrato C# novo, sem migration. O "contrato" desta feature é o conjunto
de tokens e componentes CSS:

```css
/* tokens.css — nomes (valores em D4, D5, D6) */
--cor-primaria; --cor-primaria-escura; --cor-primaria-clara;
--cor-destaque; --cor-destaque-clara; --cor-perigo;
--cor-sobre-primaria; --cor-tinta; --cor-botao-padrao;
--neutro-superficie; --neutro-fundo; --neutro-rebaixado;
--neutro-divisor; --neutro-borda; --neutro-texto-suave;
--fonte-principal; --fonte-secundaria;
--texto-pequeno; --texto-base; --texto-grande; --texto-titulo; --texto-etiqueta;
--espaco-1 … --espaco-16;
--raio-pequeno; --raio-medio; --raio-grande; --raio-pilula;
--sombra-1; --sombra-2; --sombra-3;
--duracao-rapida; --duracao; --curva;
--largura-conteudo;

/* componentes */
.titulo, .titulo--verde
.botao, .botao--primario, .botao--secundario, .botao--perigo,
.botao--fantasma, .botao--icone, .botao--pequeno
.painel, .painel__cabecalho, .painel--rebaixado
```

Os valores de `--cor-primaria-escura` (`#033626`), `--cor-primaria-clara`
(`#F0F7F4`), `--cor-destaque-clara` (`#FDF2F2`) e `--cor-perigo` (`#B3261E`)
já existem hoje no CSS. Nenhuma cor nova entra na paleta (D4).

## 6. Ordem de execução

Cada passo termina com o site compilando e as telas tocadas conferidas no
navegador. Os passos que mexem em markup terminam com o E2E.

- [x] 1. Capturar a tela de cada página em 1280px e em 390px (estado "antes"). As capturas ficam locais e não são versionadas.
- [x] 2. Criar `tokens.css` e `base.css`; apagar `site.css`; ajustar os dois layouts (fonte, ordem dos estilos, título da aba); apagar `_Layout.cshtml.css` e o link `styles.css`; corrigir `_HeaderNaoAutenticado`.
- [x] 3. Tirar o Bootstrap do `_LayoutNaoAutenticado` e apagar `wwwroot/lib/bootstrap`. Conferir as 4 telas de autenticação. `dotnet test` (E2E de login, cadastro e recuperação).
- [ ] 4. Criar `botao.css` e `painel.css`; reescrever `formulario.css` e passar a carregá-lo pelos layouts; aplicar `.titulo` etiqueta. *(Feito e conferido no navegador; build verde. Falta rodar o E2E para marcar.)*
- [ ] 5. *(Em andamento: `card-produto.css` reescrito; o resto da casca não foi tocado.)* Casca: cabeçalho, cabeçalho não autenticado, rodapé, modal de login, aviso do carrinho, cartão de produto, vitrine. `dotnet test`.
- [ ] 6. Páginas da loja, uma por vez: home (carrossel, categorias), catálogo, produto, carrinho e fechamento, pedidos, conta, favoritos, autenticação, institucional, telas de erro. Remover as paletas locais de cada uma. `dotnet test` ao fim.
- [ ] 7. Área administrativa: fundir `cadastro_produto.css` em `gestao-produtos.css`; migrar gestão de produtos e administradores. `dotnet test` (E2E da área administrativa e do cadastro de produto).
- [ ] 8. Breakpoints: reescrever todo `@media` para 480, 768, 1024 ou 1280. Conferir cada página em 390, 768 e 1280px.
- [ ] 9. Remover os aliases `--cor-texto` e `--cor-texto-secundario`; rodar a varredura da seção 7.
- [ ] 10. Capturar o estado "depois" e comparar com o passo 1, tela a tela.
- [ ] 11. `dotnet build` e `dotnet test` completos.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Nenhum comportamento mudou | `dotnet test` completo verde, inclusive `DocesCabana.Tests.E2E`, sem alterar nenhum teste. |
| Nenhuma cor fixa fora dos tokens | `grep -rniE "#[0-9a-f]{3,8}\b" DocesCabana.MVC/wwwroot/css --include=*.css \| grep -v tokens.css` sem resultado. |
| Nenhuma paleta local sobrou | `grep -rnE "\-\-(catalogo\|produto\|gestao\|institucional)-\|--texto-rodape\|--cor-texto" DocesCabana.MVC/wwwroot/css DocesCabana.MVC/Views DocesCabana.MVC/Areas` sem resultado. |
| Só os quatro breakpoints oficiais | `grep -rhoE "@media[^{]+" DocesCabana.MVC/wwwroot/css \| sort -u` lista só 480, 768, 1024, 1280, `hover` e `prefers-reduced-motion`. |
| Bootstrap removido | `grep -rn "bootstrap" DocesCabana.MVC/Views DocesCabana.MVC/Areas` sem resultado. |
| Foco por teclado visível em todas as telas | Verificação manual: navegar com Tab pelo cabeçalho, cartão, carrinho, formulário de conta e gestão. |
| Movimento reduzido respeitado | Verificação manual com "Reduzir movimento" do sistema ligado: a etiqueta aparece sem animação, e o cartão não sobe no hover. |
| Responsivo | Verificação manual de cada página em 390, 768 e 1280px, comparando com as capturas do passo 1. |

## 8. Riscos e desvios

Nenhum desvio constitucional.

| Risco | Impacto | Mitigação |
|---|---|---|
| R1. Renomear ou remover uma classe que o JS ou o E2E usa. | Alto: um fluxo crítico (carrinho, fechamento, gestão) quebra em silêncio na tela, mesmo com o CSS certo. | D20 lista o que fica. Os componentes entram como classe adicional. Antes de remover uma classe, rodar `grep` em `wwwroot/js` e `DocesCabana.Tests.E2E`. E2E ao fim de cada fase. |
| R2. Tirar o Bootstrap muda o espaçamento das telas de autenticação (o *reboot* zerava margens e definia `line-height`). | Médio: login e cadastro com aparência quebrada. | O passo 3 é isolado e conferido no navegador antes de seguir. O `base.css` já traz o reset que a tela precisa. |
| R3. Passar o `html` de 14px para 16px no celular (D13) aumenta tudo que usa `rem`. | Médio: elementos estouram em 390px. | O passo 8 confere todas as páginas em 390px. |
| R4. `carrinho.css` (941 linhas) e `gestao-produtos.css` (575) concentram metade do CSS, e a migração pode apagar uma regra de layout junto com a de cor. | Alto: o fechamento de pedido é o fluxo que gera receita. | Migrar esses dois arquivos em commits próprios, por bloco de seletor, com o E2E de carrinho, fechamento e área administrativa ao fim de cada um. |
| R5. **Achado fora de escopo:** `Admin/Administrador/Cadastro` chama `_ValidationScriptsPartial`, mas o `_Layout` não carrega jQuery. A validação no cliente dessa tela já não funciona hoje (a do servidor funciona). | Baixo: não piora com esta feature. | Registrar em `specs/README.md` como pendência. Não corrigir aqui. |
| R6. O traço da etiqueta (SVG em `mask-image`) não aparece em navegador sem suporte a `mask`. | Baixo: o título continua legível, só sem o traço. | `@supports (mask-image: url())` envolve o `::after`. |

## 9. Resultado da execução

*Preenchido na execução.*
