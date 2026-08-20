# Especificação — Catálogo

**ID:** `012-catalogo` · **Branch:** `012-catalogo`
**Criada em:** 2026-08-18 · **Revisada em:** 2026-08-19 · **Status:** Implementada

---

> **Nota de revisão (2026-08-19).** A primeira versão desta spec foi escrita
> sobre uma taxonomia inventada e um seed de 6 produtos. O catálogo real da
> loja derrubou cinco decisões dela, entre elas "sem paginação". Esta versão
> substitui aquelas escolhas provisórias por decisões tomadas com o
> responsável, registradas na seção 11.

---

## 1. Contexto e problema

A loja tem centenas de produtos, uma página por produto desde a `008` e
categorias modeladas desde a `003` — mas não tem por onde navegar entre elas. A
vitrine da página inicial despeja produtos numa fileira só, sem filtro e sem
ordem; os quatro atalhos de categoria do cabeçalho apontam para lugar nenhum
(`href="#"`), e o bloco de categorias da página inicial aponta para uma tela que
nunca existiu. Quem quer ver só os doces, ou só os vinhos, não tem caminho.

Com esse volume não há como listar tudo de uma vez. Esta feature entrega a tela
desenhada na referência visual da loja — listagem com barra lateral de
categorias, filtro por subcategoria e ordenação — sobre a taxonomia real.

## 2. Objetivo

Dar à loja uma página de catálogo onde o cliente encontra produtos por
categoria, refina por subcategoria e percorre o resultado em páginas.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Navega o catálogo pelo cabeçalho ou pela página inicial, filtra, ordena e pagina; abre a página de um produto a partir dali |
| Cliente autenticado | O mesmo — o catálogo não muda conforme quem está logado |
| Administrador da loja | Passa a escolher a subcategoria num seletor que diz a que categoria cada uma pertence, e a marcar produto como sem açúcar |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero ver só os produtos de uma categoria, para
> não percorrer a loja inteira quando já sei o que procuro.
>
> **HU-02** — Como **cliente**, quero refinar dentro da categoria marcando as
> subcategorias que me interessam, para chegar mais perto do que quero.
>
> **HU-03** — Como **cliente que evita açúcar**, quero filtrar só os doces sem
> açúcar, independente do tipo de doce, para não abrir produto por produto
> para descobrir.
>
> **HU-04** — Como **cliente**, quero escolher a ordem da lista — pelo preço ou
> pelo nome — para comparar do jeito que faz sentido pra mim.
>
> **HU-05** — Como **cliente**, quero percorrer o catálogo em páginas e saber
> quantas existem, para não esperar a categoria inteira carregar de uma vez.
>
> **HU-06** — Como **cliente**, quero que os atalhos de categoria do cabeçalho
> levem a algum lugar, porque hoje eu clico neles e nada acontece.
>
> **HU-07** — Como **administrador**, quero saber a que categoria pertence cada
> subcategoria na hora de cadastrar um produto, porque há nome de subcategoria
> que se repete entre categorias.

## 5. Requisitos funcionais

### Acesso e navegação

- **RF-01** — O sistema DEVE oferecer uma página pública de catálogo que liste
  os produtos disponíveis da loja.
- **RF-02** — O sistema DEVE dar a cada categoria um endereço legível, formado
  a partir do nome dela.
- **RF-03** — O sistema DEVE oferecer, no cabeçalho de toda tela, todas as
  categorias da loja, cada uma abrindo um menu com suas subcategorias.
- **RF-04** — O sistema DEVE exibir, nesse menu, no máximo oito subcategorias:
  as com mais produtos disponíveis.
- **RF-05** — O sistema DEVE ligar o bloco de categorias da página inicial ao
  catálogo da categoria correspondente.
- **RF-06** — O sistema DEVE exibir, no topo da página, uma trilha de navegação
  com a página inicial, o catálogo completo e — quando houver — a categoria
  escolhida.
- **RF-07** — O sistema DEVE responder "não encontrado" quando o endereço não
  corresponder a nenhuma categoria.

### Barra lateral

- **RF-08** — O sistema DEVE exibir, numa barra lateral, a opção "Todos"
  seguida de todas as categorias cadastradas.
- **RF-09** — O sistema DEVE destacar, na barra lateral, a categoria exibida.
- **RF-10** — O sistema DEVE exibir uma caixa de seleção por subcategoria da
  categoria escolhida, começando pelas oito com mais produtos e revelando as
  demais sob demanda, **sem recarregar a página**.
- **RF-11** — O sistema NÃO DEVE exibir caixa de subcategoria nenhuma quando o
  catálogo completo estiver selecionado.
- **RF-12** — O sistema DEVE permitir marcar mais de uma subcategoria ao mesmo
  tempo, exibindo os produtos que pertençam a qualquer uma das marcadas.
- **RF-13** — O sistema DEVE exibir todos os produtos da categoria quando
  nenhuma subcategoria estiver marcada.
- **RF-14** — O sistema DEVE oferecer uma caixa de seleção "sem açúcar",
  apresentada à parte das subcategorias, que restringe o resultado aos produtos
  assim marcados.

### Ordenação

- **RF-15** — O sistema DEVE oferecer um seletor de ordenação com "Mais
  vendidos", "Melhor avaliados", "Menor preço", "Maior preço" e "Nome (A-Z)".
- **RF-16** — O sistema DEVE apresentar "Mais vendidos" marcada como
  indisponível e NÃO DEVE permitir escolhê-la enquanto a loja não registrar
  vendas.
- **RF-17** — O sistema DEVE usar "Nome (A-Z)" como ordem inicial.
- **RF-18** — O sistema DEVE preservar a ordenação escolhida ao trocar de
  categoria, de subcategoria ou de página.

### Paginação

- **RF-19** — O sistema DEVE exibir no máximo doze produtos por página.
- **RF-20** — O sistema DEVE exibir controles de página numerados, indicando a
  página atual e o total, com atalhos para a anterior e a seguinte.
- **RF-21** — O sistema DEVE tratar página fora do intervalo exibindo a
  primeira ou a última válida, conforme o caso, em vez de uma página vazia.
- **RF-22** — O sistema DEVE funcionar sem JavaScript: filtrar, ordenar e
  trocar de página são navegações comuns.

### Grade de produtos

- **RF-23** — O sistema DEVE exibir cada produto como um card com imagem, nome
  e preço, e o card DEVE levar à página daquele produto.
- **RF-24** — O sistema DEVE apresentar os controles de favoritar, de
  quantidade e de adicionar ao carrinho **desabilitados e marcados como
  indisponíveis**, enquanto favoritos e carrinho não existirem.
- **RF-25** — O sistema NÃO DEVE exibir produto inativo em nenhuma listagem.
- **RF-26** — O sistema DEVE exibir produto fora de estoque, sinalizado como
  tal.
- **RF-27** — O sistema DEVE exibir uma mensagem própria quando a combinação de
  filtros não tiver nenhum produto, em vez de uma grade vazia.

### Cadastro de produto

- **RF-28** — O sistema DEVE identificar, no seletor de subcategoria do
  cadastro de produto, a que categoria cada subcategoria pertence.
- **RF-29** — O sistema DEVE permitir que o administrador marque um produto
  como sem açúcar ao cadastrá-lo.

## 6. Regras de negócio

- **RN-01** — Só produto ativo ou fora de estoque aparece para o cliente.
  Produto inativo não existe do lado de fora — nem em listagem, nem por
  endereço direto, como a `008` já estabeleceu para a página do produto.
- **RN-02** — Uma subcategoria pertence a exatamente uma categoria.
  Subcategorias de categorias diferentes podem ter o mesmo nome e são coisas
  distintas.
- **RN-03** — Subcategorias marcadas se somam: o produto aparece se pertencer a
  **qualquer uma** delas, não a todas.
- **RN-04** — "Sem açúcar" é característica do produto, não lugar na
  hierarquia: um produto é "Barras" **e** sem açúcar ao mesmo tempo. Por isso
  esse filtro se combina com os de subcategoria em vez de competir com eles.
- **RN-05** — A ordem inicial não pode produzir empates. Com paginação, ordem
  empatada permite que o mesmo produto apareça em duas páginas ou em nenhuma.
- **RN-06** — "As oito principais" de uma categoria são as oito subcategorias
  com mais produtos disponíveis nela, contadas a partir do catálogo — não uma
  escolha manual.
- **RN-07** — "Mais vendidos" só passa a ser uma ordem possível quando a loja
  registrar pedidos. Até lá é uma opção anunciada, não oferecida.

## 7. Critérios de aceite

### CA-01 — Abrir o catálogo completo
- **Dado** que sou visitante em qualquer tela da loja
- **Quando** abro o catálogo
- **Então** vejo os primeiros doze produtos, a trilha "Home › Todos", e "Todos"
  destacado na barra lateral

### CA-02 — Filtrar por categoria pelo cabeçalho
- **Dado** que estou em qualquer tela da loja
- **Quando** escolho uma categoria no cabeçalho
- **Então** vejo só os produtos dela, a trilha "Home › Todos › {Categoria}", e a
  categoria destacada na barra lateral

### CA-03 — Endereço legível
- **Dado** que abri o catálogo de Empório
- **Quando** olho o endereço da página
- **Então** ele contém o nome da categoria em forma legível, sem acento e em
  minúsculas, e não um identificador interno

### CA-04 — Menu suspenso limitado a oito
- **Dado** que a categoria Doces tem doze subcategorias
- **Quando** abro o menu dela no cabeçalho
- **Então** vejo oito, e são as com mais produtos

### CA-05 — Menu de categoria pequena mostra tudo
- **Dado** que a categoria Adega tem quatro subcategorias
- **Quando** abro o menu dela
- **Então** vejo as quatro, sem corte

### CA-06 — Refinar por duas subcategorias
- **Dado** que estou no catálogo de uma categoria e marquei uma subcategoria
- **Quando** marco também uma segunda
- **Então** vejo os produtos das duas juntos, não a interseção vazia entre elas

### CA-07 — Desmarcar tudo volta à categoria inteira
- **Dado** que tenho subcategorias marcadas
- **Quando** desmarco todas
- **Então** volto a ver todos os produtos da categoria

### CA-08 — Revelar as subcategorias restantes
- **Dado** que estou numa categoria com mais de oito subcategorias
- **Quando** aciono "Ver todas"
- **Então** as demais aparecem sem que a página recarregue

### CA-09 — Filtrar por sem açúcar
- **Dado** que estou no catálogo de Doces
- **Quando** marco "sem açúcar"
- **Então** vejo só os produtos marcados como sem açúcar, de qualquer
  subcategoria

### CA-10 — Sem açúcar combina com subcategoria
- **Dado** que marquei "sem açúcar"
- **Quando** marco também a subcategoria "Barras"
- **Então** vejo só as barras sem açúcar

### CA-11 — O catálogo completo não oferece subcategoria
- **Dado** que estou no catálogo completo
- **Quando** olho a barra lateral
- **Então** vejo as categorias e nenhuma caixa de subcategoria

### CA-12 — Ordenar por menor preço
- **Dado** que estou no catálogo
- **Quando** escolho "Menor preço"
- **Então** os produtos aparecem do mais barato ao mais caro

### CA-13 — "Mais vendidos" não pode ser escolhida
- **Dado** que abro o seletor de ordenação
- **Quando** tento escolher "Mais vendidos"
- **Então** a opção está visível, marcada como indisponível, e não é
  selecionada

### CA-14 — A ordenação sobrevive à troca de categoria e de página
- **Dado** que escolhi "Maior preço"
- **Quando** troco de categoria e depois avanço uma página
- **Então** continuo com "Maior preço" escolhido

### CA-15 — Paginar
- **Dado** que estou numa categoria com mais de doze produtos
- **Quando** olho o fim da grade
- **Então** vejo controles numerados com o total de páginas, e ao ir para a
  segunda vejo produtos diferentes dos da primeira

### CA-16 — Nenhum produto repete nem some entre páginas
- **Dado** que percorro todas as páginas de uma categoria
- **Quando** junto tudo que vi
- **Então** cada produto da categoria aparece exatamente uma vez

### CA-17 — Página fora do intervalo
- **Dado** que uma categoria tem três páginas
- **Quando** peço a página 99
- **Então** vejo a última página válida, não uma grade vazia

### CA-18 — Card leva à página do produto
- **Dado** que estou no catálogo
- **Quando** clico na imagem ou no nome de um produto
- **Então** chego à página daquele produto

### CA-19 — Controles indisponíveis não enganam
- **Dado** que estou no catálogo
- **Quando** tento favoritar, alterar a quantidade ou adicionar ao carrinho
- **Então** nada acontece, e os três controles estão visivelmente marcados como
  indisponíveis

### CA-20 — Produto inativo não aparece
- **Dado** que um produto está inativo
- **Quando** abro o catálogo da categoria dele e a vitrine da página inicial
- **Então** ele não aparece em nenhuma das duas

### CA-21 — Produto fora de estoque aparece sinalizado
- **Dado** que um produto está fora de estoque
- **Quando** abro o catálogo da categoria dele
- **Então** ele aparece, com indicação de que está fora de estoque

### CA-22 — Combinação de filtros sem resultado
- **Dado** que marquei uma subcategoria e "sem açúcar" ao mesmo tempo
- **Quando** nenhum produto atende às duas condições
- **Então** vejo uma mensagem dizendo que não há produtos, e não uma grade vazia

### CA-23 — Endereço de categoria inexistente
- **Dado** que informo uma categoria que não existe
- **Quando** abro o catálogo
- **Então** recebo "não encontrado"

### CA-24 — Seletor de subcategoria do cadastro é inequívoco
- **Dado** que sou administrador cadastrando um produto, e "Cappuccino" existe
  em Doces e em Empório
- **Quando** abro o seletor de subcategoria
- **Então** as duas aparecem identificadas pela categoria a que pertencem

### CA-25 — Sem JavaScript
- **Dado** que o navegador está com JavaScript desligado
- **Quando** filtro, ordeno e troco de página
- **Então** as três coisas funcionam

### CA-26 — Leitura no celular
- **Dado** que abro o catálogo numa tela de 375 pixels de largura
- **Quando** rolo até o fim
- **Então** o conteúdo cabe na largura da tela, sem rolagem horizontal

## 8. Fora de escopo

- **Busca por texto.** O campo de busca do cabeçalho continua sem função — é
  entrega própria.
- **Adicionar ao carrinho, favoritar e o seletor de quantidade funcionarem.**
  São a spec `015` (carrinho) e a de favoritos; aqui os controles aparecem
  desabilitados.
- **"Mais vendidos" funcionando.** Depende de pedidos existirem (spec `017`).
- **Filtro por preço, por avaliação ou por promoção.** Nenhum aparece na
  referência visual.
- **Sem glúten e sem lactose.** A marcação de "sem açúcar" abre a porta para
  elas, mas nenhuma foi pedida.
- **Imagens novas para o bloco de categorias da página inicial.** Os blocos
  passam a apontar para o catálogo, mas continuam com as imagens atuais, que
  não correspondem mais às categorias. Registrado no backlog.
- **Substituir o mock de cem produtos pelo catálogo real.** Registrado no
  backlog, depende de a loja exportar os dados.
- **Reescrever a vitrine da página inicial.** Ela continua como está — exceto
  por parar de listar produto inativo, que é correção de defeito.

## 9. Dependências

- **Depende de:** `011-area-administrativa`, que libera o nome "catálogo" na
  raiz do site.
- **Bloqueia:** `014-carrinho` — o botão "Adicionar ao carrinho" do card do
  catálogo é um dos alvos dela.

## 10. Defeitos encontrados durante a especificação

Três, todos já no ar hoje. Nenhum é escopo que cresceu: os três estão no
caminho exato desta feature, que reusa a mesma consulta, o mesmo card e o mesmo
cadastro.

**A vitrine lista produtos inativos.** A consulta que a alimenta não filtra por
status. Como a `008` fez a página do produto responder "não encontrado" para
produto inativo, o cliente vê o produto na página inicial, clica, e recebe um
404. RF-25 e RN-01 corrigem para as duas listagens de uma vez.

**Os três controles do card são teatro.** O botão "Adicionar ao carrinho" troca
o próprio texto para "Adicionado!" por um segundo e meio e volta — nenhuma
requisição, nada guardado, e o cliente acreditando que adicionou. O coração de
favoritar só troca o ícone na tela: o campo que diria se o produto está
favoritado nunca é preenchido, então recarregar desfaz. O seletor de quantidade
funciona, mas o número não vai a lugar nenhum. RF-24 é a correção.

**O seletor de subcategoria do cadastro é ambíguo.** Ele lista subcategorias
pelo nome, numa lista plana. "Cappuccino" existe em Doces e em Empório, e o
administrador não tem como saber qual é qual — escolher errado põe o produto na
categoria errada, silenciosamente. Hoje não dói porque o seed tem seis
subcategorias de nomes únicos; com a taxonomia real, dói. RF-28 corrige.

## 11. Decisões de negócio registradas

Tomadas com o responsável em 2026-08-19, substituindo as cinco decisões
provisórias da versão anterior desta spec.

**Taxonomia real, com uma fusão.** A loja tem cinco categorias; "Doces
Caseiros" e "Doces Zero" se fundem em **Doces**, e a distinção passa a ser uma
característica do produto (RN-04). Motivo: "zero" descreve o produto, não onde
ele fica — e como "Barras" e "Potes" existiam nas duas, fundi-las sem essa
marcação perderia a informação de que o produto é sem açúcar. Resultado:

| Categoria | Subcategorias |
|---|---|
| **Doces** | Barras, Bolachas / Rosquinhas, Box, Combos, Compotas, Cappuccino, Latas, Palhas, Potes, Quindim, Raspa de Tachos, Sorvetes |
| **Empório** | Café, Cappuccino, Charcutaria, Croissant, Desidratados, Geleias, Manteiga, Mel, Molho, Risotto |
| **Adega** | Cachaça, Licor, Licor Caseiro, Vinhos |
| **Souvenir** | Bijuterias, Canecas, Chaveiros, Kits, Pelúcia |

Quatro categorias, trinta e uma subcategorias. **"Cappuccino" é o único nome
que se repete entre categorias** — é o caso que RF-28 e CA-24 exercitam.

**O cabeçalho lista todas as categorias.** Como sobraram quatro, não há o que
escolher: a ideia anterior de destacar as quatro maiores perdeu objeto.

**Oito subcategorias por menu, as com mais produtos.** Mesma regra no menu do
cabeçalho e na barra lateral, para a loja precisar entender um critério só. Só
Doces (doze) e Empório (dez) chegam a cortar.

**Nome (A-Z) como ordem inicial.** Escolhida por não empatar (RN-05). "Melhor
avaliados", que a versão anterior propunha, empataria quase todo o catálogo em
"sem nota" e faria produto repetir entre páginas. A revisão futura desta
escolha está no backlog.

**Endereço legível sem coluna nova.** `/Catalogo/emporio` em vez de um
identificador interno. O casamento entre endereço e categoria acontece sobre as
quatro categorias que a tela já carrega, sem consulta nem coluna adicional.

**Cem produtos de mock, vinte e cinco por categoria.** A taxonomia é a
verdadeira; os produtos são gerados, distribuídos igualmente. Cada categoria dá
três páginas, então todas exercitam a paginação. Substituir pelo catálogo real
da loja está no backlog. Com esse volume, um filtro de subcategoria devolve dois
ou três produtos: prova a mecânica, não a aparência de loja cheia.

## 12. Pendências

Nenhuma.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]`
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
