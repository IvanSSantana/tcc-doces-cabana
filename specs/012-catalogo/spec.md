# Especificação — Catálogo

**ID:** `012-catalogo` · **Branch:** `012-catalogo`
**Criada em:** 2026-08-18 · **Status:** Rascunho

---

## 1. Contexto e problema

A loja tem produtos, tem uma página por produto desde a `008`, e tem categorias
modeladas desde a `003` — mas não tem por onde navegar entre elas. A vitrine da
página inicial despeja o catálogo inteiro numa fileira só, sem filtro e sem
ordem; os quatro atalhos de categoria do cabeçalho apontam para lugar nenhum
(`href="#"`), e o bloco de categorias da página inicial aponta para uma tela que
nunca existiu. Quem quer ver só os doces, ou só os vinhos, não tem caminho.

Esta feature entrega a tela desenhada na referência visual da loja: a listagem
com barra lateral de categorias, filtro por subcategoria e ordenação.

## 2. Objetivo

Dar à loja uma página de catálogo onde o cliente encontra produtos por
categoria, refina por subcategoria e escolhe a ordem em que os vê.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Navega o catálogo pelo cabeçalho ou pela página inicial, filtra e ordena; abre a página de um produto a partir dali |
| Cliente autenticado | O mesmo — o catálogo não muda conforme quem está logado |
| Administrador da loja | Passa a ver, do lado do cliente, o efeito da categoria que escolheu ao cadastrar cada produto |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero ver só os produtos de uma categoria, para
> não percorrer a loja inteira quando já sei o que procuro.
>
> **HU-02** — Como **cliente**, quero refinar dentro da categoria marcando as
> subcategorias que me interessam, para chegar mais perto do que quero.
>
> **HU-03** — Como **cliente**, quero escolher a ordem da lista — pelo preço,
> pelo nome ou pela avaliação — para comparar do jeito que faz sentido pra mim.
>
> **HU-04** — Como **cliente**, quero saber onde estou na loja pela trilha no
> topo, e voltar um nível com um clique.
>
> **HU-05** — Como **cliente**, quero que os atalhos de categoria do cabeçalho
> levem a algum lugar, porque hoje eu clico neles e nada acontece.

## 5. Requisitos funcionais

### Acesso e navegação

- **RF-01** — O sistema DEVE oferecer uma página pública de catálogo, com
  endereço próprio, que liste os produtos disponíveis da loja.
- **RF-02** — O sistema DEVE oferecer um caminho para o catálogo de cada
  categoria a partir do cabeçalho, presente em toda tela.
- **RF-03** — O sistema DEVE ligar o bloco de categorias da página inicial ao
  catálogo da categoria correspondente.
- **RF-04** — O sistema DEVE exibir, no topo da página, uma trilha de navegação
  com a página inicial, o catálogo completo e — quando houver — a categoria
  escolhida.
- **RF-05** — O sistema DEVE responder "não encontrado" quando a categoria
  pedida não existir.

### Barra lateral de categorias

- **RF-06** — O sistema DEVE exibir, numa barra lateral, a opção "Todos"
  seguida de todas as categorias cadastradas.
- **RF-07** — O sistema DEVE destacar, na barra lateral, a categoria que está
  sendo exibida.
- **RF-08** — O sistema DEVE exibir, abaixo da lista de categorias, uma caixa de
  seleção por subcategoria da categoria escolhida.
- **RF-09** — O sistema NÃO DEVE exibir caixa de subcategoria nenhuma quando o
  catálogo completo estiver selecionado.
- **RF-10** — O sistema DEVE permitir marcar mais de uma subcategoria ao mesmo
  tempo, exibindo os produtos que pertençam a qualquer uma das marcadas.
- **RF-11** — O sistema DEVE exibir todos os produtos da categoria quando
  nenhuma subcategoria estiver marcada.

### Ordenação

- **RF-12** — O sistema DEVE oferecer um seletor de ordenação com as opções
  "Mais vendidos", "Melhor avaliados", "Menor preço", "Maior preço" e
  "Nome (A-Z)".
- **RF-13** — O sistema DEVE apresentar "Mais vendidos" marcada como
  indisponível e NÃO DEVE permitir escolhê-la, enquanto a loja não registrar
  vendas.
- **RF-14** — O sistema DEVE usar "Melhor avaliados" como ordem inicial.
- **RF-15** — O sistema DEVE preservar a ordenação escolhida ao trocar de
  categoria ou de subcategoria.

### Grade de produtos

- **RF-16** — O sistema DEVE exibir cada produto como um card com imagem, nome
  e preço, e o card DEVE levar à página daquele produto.
- **RF-17** — O sistema DEVE apresentar os controles de favoritar, de
  quantidade e de adicionar ao carrinho **desabilitados e marcados como
  indisponíveis**, enquanto favoritos e carrinho não existirem.
- **RF-18** — O sistema NÃO DEVE exibir produto inativo em nenhuma listagem.
- **RF-19** — O sistema DEVE exibir produto fora de estoque, sinalizado como
  tal.
- **RF-20** — O sistema DEVE exibir uma mensagem própria quando a combinação de
  categoria e subcategorias não tiver nenhum produto, em vez de uma grade vazia.

## 6. Regras de negócio

- **RN-01** — Só produto ativo ou fora de estoque aparece para o cliente.
  Produto inativo não existe do lado de fora — nem em listagem, nem por
  endereço direto, como a `008` já estabeleceu para a página do produto.
- **RN-02** — Uma subcategoria pertence a exatamente uma categoria. Não é
  possível, pela tela, combinar subcategorias de categorias diferentes.
- **RN-03** — Subcategorias marcadas se somam: o produto aparece se pertencer a
  **qualquer uma** delas, não a todas.
- **RN-04** — Em "Melhor avaliados", produto sem nenhuma avaliação vai para o
  fim da lista — ausência de nota não é nota baixa, mas também não disputa as
  primeiras posições.
- **RN-05** — "Mais vendidos" só passa a ser uma ordem possível quando a loja
  registrar pedidos. Até lá é uma opção anunciada, não uma opção oferecida.

## 7. Critérios de aceite

### CA-01 — Abrir o catálogo completo
- **Dado** que sou visitante em qualquer tela da loja
- **Quando** abro o catálogo
- **Então** vejo todos os produtos disponíveis, a trilha "Home › Todos", e
  "Todos" destacado na barra lateral

### CA-02 — Filtrar por categoria pelo cabeçalho
- **Dado** que estou em qualquer tela da loja
- **Quando** escolho uma categoria no cabeçalho
- **Então** vejo só os produtos daquela categoria, a trilha "Home › Todos ›
  {Categoria}", e a categoria destacada na barra lateral

### CA-03 — Filtrar por categoria pela página inicial
- **Dado** que estou na página inicial
- **Quando** clico numa das categorias do bloco de categorias
- **Então** chego ao catálogo daquela categoria

### CA-04 — Refinar por uma subcategoria
- **Dado** que estou no catálogo de uma categoria com mais de uma subcategoria
- **Quando** marco uma subcategoria
- **Então** vejo só os produtos dela, e as demais caixas continuam desmarcadas

### CA-05 — Refinar por duas subcategorias
- **Dado** que marquei uma subcategoria
- **Quando** marco também uma segunda
- **Então** vejo os produtos das duas juntos, não a interseção vazia entre elas

### CA-06 — Desmarcar tudo volta à categoria inteira
- **Dado** que tenho subcategorias marcadas
- **Quando** desmarco todas
- **Então** volto a ver todos os produtos da categoria

### CA-07 — O catálogo completo não oferece subcategoria
- **Dado** que estou no catálogo completo
- **Quando** olho a barra lateral
- **Então** vejo as categorias e nenhuma caixa de subcategoria

### CA-08 — Ordenar por menor preço
- **Dado** que estou no catálogo
- **Quando** escolho "Menor preço"
- **Então** os produtos aparecem do mais barato ao mais caro

### CA-09 — "Mais vendidos" não pode ser escolhida
- **Dado** que abro o seletor de ordenação
- **Quando** tento escolher "Mais vendidos"
- **Então** a opção está visível, marcada como indisponível, e não é
  selecionada

### CA-10 — A ordenação sobrevive à troca de categoria
- **Dado** que escolhi "Maior preço" no catálogo completo
- **Quando** troco para uma categoria
- **Então** continuo com "Maior preço" escolhido

### CA-11 — Card leva à página do produto
- **Dado** que estou no catálogo
- **Quando** clico na imagem ou no nome de um produto
- **Então** chego à página daquele produto

### CA-12 — Controles indisponíveis não enganam
- **Dado** que estou no catálogo
- **Quando** tento favoritar, alterar a quantidade ou adicionar ao carrinho
- **Então** nada acontece, e os três controles estão visivelmente marcados como
  indisponíveis

### CA-13 — Produto inativo não aparece
- **Dado** que um produto está inativo
- **Quando** abro o catálogo da categoria dele
- **Então** ele não aparece na grade

### CA-14 — Produto fora de estoque aparece sinalizado
- **Dado** que um produto está fora de estoque
- **Quando** abro o catálogo da categoria dele
- **Então** ele aparece, com indicação de que está fora de estoque

### CA-15 — Categoria sem produto
- **Dado** que uma categoria não tem nenhum produto disponível
- **Quando** abro o catálogo dela
- **Então** vejo uma mensagem dizendo que não há produtos ali, e não uma grade
  vazia

### CA-16 — Categoria inexistente
- **Dado** que informo uma categoria que não existe
- **Quando** abro o catálogo
- **Então** recebo "não encontrado"

### CA-17 — Leitura no celular
- **Dado** que abro o catálogo numa tela de 375 pixels de largura
- **Quando** rolo até o fim
- **Então** o conteúdo cabe na largura da tela, sem rolagem horizontal

## 8. Fora de escopo

- **Paginação.** A referência visual não mostra controle de paginação, e a
  `002` já registrou a paginação da vitrine como spec própria "quando o
  catálogo crescer". O catálogo carrega tudo. **Ver seção 11.**
- **Busca por texto.** O campo de busca do cabeçalho continua sem função — é
  entrega própria.
- **Adicionar ao carrinho, favoritar e o seletor de quantidade funcionarem.**
  São a spec `014` (carrinho) e a de favoritos; aqui os controles aparecem
  desabilitados.
- **"Mais vendidos" funcionando.** Depende de pedidos existirem (spec `016`,
  fechamento).
- **Filtro por preço, por avaliação ou por promoção.** Nenhum aparece na
  referência visual.
- **Endereços amigáveis para categoria.** **Ver seção 11.**
- **Reescrever a vitrine da página inicial.** Ela continua como está — exceto
  por parar de listar produto inativo, que é correção de defeito, não redesenho.

## 9. Dependências

- **Depende de:** `011-area-administrativa`, que libera o nome "catálogo" na
  raiz do site.
- **Bloqueia:** `014-carrinho` — o botão "Adicionar ao carrinho" do card do
  catálogo é um dos alvos dela.

## 10. Defeitos encontrados durante a especificação

Dois, ambos já no ar hoje na vitrine da página inicial. Nenhum é escopo que
cresceu: os dois estão no caminho exato desta feature, que reusa a mesma
consulta e o mesmo card.

**A vitrine lista produtos inativos.** A consulta que a alimenta não filtra por
status. Como a `008` fez a página do produto responder "não encontrado" para
produto inativo, o resultado atual é que o cliente vê o produto na página
inicial, clica, e recebe um 404. RF-18 e RN-01 corrigem para as duas listagens
de uma vez, já que passam pelo mesmo caminho.

**Os três controles do card são teatro.** O botão "Adicionar ao carrinho" troca
o próprio texto para "Adicionado!" por um segundo e meio e volta — nenhuma
requisição, nada guardado, e o cliente acreditando que adicionou. O coração de
favoritar só troca o ícone na tela: o campo que diria se o produto está
favoritado nunca é preenchido por lugar nenhum, então recarregar a página
desfaz. O seletor de quantidade funciona, mas o número não vai a lugar nenhum.
RF-17 é a correção: os três param de mentir até que carrinho e favoritos
existam de verdade. Hoje isso afeta 4 cards na vitrine; sem a correção, o
catálogo multiplicaria por 12.

## 11. Decisões tomadas na ausência do responsável — **revisar**

Estas foram resolvidas sem confirmação, com a orientação de priorizar a
referência visual e a arquitetura vigente. Nenhuma bloqueia a implementação, e
todas são reversíveis a custo baixo — mas todas merecem um "sim" antes de virar
definitivo.

- **⚠️ O cabeçalho passa a listar as 6 categorias reais, e deixa de mostrar
  "Salgado".** A referência visual mostra o cabeçalho antigo (Doce, Salgado,
  Adega, Outros) ao lado de uma barra lateral com seis categorias diferentes —
  as duas metades do mesmo desenho não combinam. Como "Salgados" passou a ser
  subcategoria de Padaria, manter o atalho antigo seria anunciar uma categoria
  que não existe mais. Optou-se pela barra lateral, que é o assunto da tela.
  **A alternativa é manter o cabeçalho como desenhado e aceitar a divergência.**

- **⚠️ Subcategorias inventadas para Empório, Bomboniere e Souvenir.** Essas
  três categorias nunca existiram no banco e a referência visual não mostra as
  subcategorias delas. Foram propostas "Geleias e Conservas / Cafés e Chás",
  "Chocolates / Balas e Gomas" e "Lembrancinhas / Cestas" como dado de
  demonstração. **São nomes inventados; a loja é quem sabe os verdadeiros.**

- **⚠️ A categoria é identificada por seu identificador interno no endereço, não
  por um nome legível.** Segue o padrão que a `008` usa para a página do
  produto e evita uma coluna nova e uma migration. Para uma loja de verdade, um
  endereço legível (`/catalogo/doces`) vale mais — por busca na web e por poder
  ser compartilhado. **Fica registrado como melhoria própria.**

- **⚠️ Sem paginação, o catálogo inteiro carrega de uma vez.** Combina com a
  referência visual e com a decisão da `002`. Com o volume atual (seis
  produtos) não dói. **Se o catálogo real da loja tiver centenas de itens, isto
  precisa mudar antes de ir ao ar.**

- **⚠️ "Melhor avaliados" como ordem inicial.** A referência visual mostra
  "Mais vendidos" como padrão, que é justamente a opção impossível. Escolheu-se
  a mais próxima em espírito. **A alternativa é "Nome (A-Z)", mais barata de
  calcular e mais previsível.**

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [ ] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — **não há marcações,
      mas a seção 11 lista cinco decisões tomadas sem confirmação**
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
