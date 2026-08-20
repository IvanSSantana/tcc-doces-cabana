# Especificação — Refinamento do catálogo

**ID:** `014-refinamento-do-catalogo` · **Branch:** `014-refinamento-do-catalogo`
**Criada em:** 2026-08-20 · **Status:** Implementada

---

## 1. Contexto e problema

A `012` entregou o catálogo e a `013` corrigiu o que ele quebrou na página
inicial. Sobrou uma lista de incômodos no próprio catálogo, levantada antes de
começar carrinho, favoritos e conta — a ideia é não carregar pendência conhecida
para dentro de uma feature nova.

**Cada mudança de filtro recarrega a página inteira.** Marcar uma subcategoria
reenvia cabeçalho, barra lateral e rodapé, a tela pisca e a posição de rolagem
se perde. O resultado está certo; o caminho até ele incomoda.

**Os produtos não preenchem o espaço da grade.** O cartão de produto foi
desenhado para o carrossel da página inicial e reaproveitado na grade do
catálogo sem revisão: sobra uma faixa vazia à direita de cada um, e produtos de
nome longo empurram preço e botões para baixo, desalinhando a linha. A
sinalização de "fora de estoque" aparece solta, acima da imagem, em vez de
sobre o produto.

**"Melhor avaliados" é uma ordenação que não ordena.** A base de demonstração
tem avaliações em **um único produto** dos cem. Escolher essa ordenação hoje
mostra esse produto no topo e todo o resto em ordem alfabética — a opção está
oferecida ao cliente e não entrega o que promete. É o mesmo defeito que a `013`
corrigiu no título "Mais Vendidos", em outro lugar.

**O atalho "Conta" do cabeçalho leva a lugar nenhum.** Quem entra na loja e
clica nele recebe a página de "não encontrado".

## 2. Objetivo

Deixar o catálogo honesto e agradável — atualização sem recarga, produtos
alinhados na grade, e avaliações suficientes para que a ordenação por nota
signifique alguma coisa — antes de começar a próxima cadeia de entregas.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Filtra e pagina sem a tela piscar; vê a grade alinhada; a ordenação por avaliação passa a produzir uma ordem real |
| Cliente autenticado | O mesmo, e deixa de encontrar um atalho de conta que não abre nada |
| Administrador da loja | Nenhuma mudança |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero marcar um filtro e ver a lista mudar sem
> a página inteira recarregar, para não perder o lugar onde eu estava.
>
> **HU-02** — Como **cliente**, quero poder mandar para alguém o endereço da
> lista que estou vendo, com os filtros que apliquei, e quero que o botão
> voltar do navegador desfaça a última filtragem.
>
> **HU-03** — Como **cliente que navega sem JavaScript**, quero continuar
> conseguindo filtrar, ordenar e paginar.
>
> **HU-04** — Como **cliente**, quero comparar produtos lado a lado sem que
> nomes de tamanhos diferentes desalinhem preços e botões.
>
> **HU-05** — Como **cliente**, quero que "melhor avaliados" me mostre os
> produtos mais bem avaliados, e não uma lista alfabética disfarçada.
>
> **HU-06** — Como **dono da loja**, não quero oferecer no cabeçalho um atalho
> que termina em página de erro.

## 5. Requisitos funcionais

### Atualização da lista sem recarga

- **RF-01** — Ao trocar um filtro, a ordenação ou a página, o sistema DEVE
  atualizar apenas a lista de produtos e seus controles, sem recarregar o
  restante da página.
- **RF-02** — O sistema DEVE manter o endereço da página em sincronia com os
  filtros aplicados, de modo que o endereço continue compartilhável e que o
  botão voltar do navegador percorra as filtragens anteriores.
- **RF-03** — O sistema DEVE preservar a posição de rolagem ao filtrar e ao
  ordenar, e DEVE levar a pessoa ao início da lista ao trocar de página.
- **RF-04** — O sistema DEVE anunciar a mudança do resultado a quem usa leitor
  de tela, informando a nova quantidade de produtos.
- **RF-05** — O sistema DEVE continuar funcionando sem JavaScript: filtrar,
  ordenar e paginar permanecem possíveis com script desligado.
- **RF-06** — Quando a atualização parcial não puder ser concluída, o sistema
  DEVE entregar a página completa do resultado pedido, nunca uma lista vazia,
  congelada ou desatualizada sem aviso.
- **RF-07** — Ao trocar de categoria, o sistema DEVE atualizar também a lista
  de subcategorias oferecida como filtro.
- **RF-18** — Após atualizar a lista, o sistema DEVE deixar o foco do teclado
  numa posição útil junto ao resultado, e NÃO DEVE devolvê-lo ao início do
  documento.

### Apresentação dos produtos na grade

- **RF-08** — Cada produto DEVE ocupar integralmente a largura da coluna que
  lhe cabe na grade, sem faixa vazia ao lado.
- **RF-09** — Produtos de uma mesma linha DEVEM alinhar preço e ações entre si,
  independentemente de quantas linhas o nome ocupe.
- **RF-10** — A sinalização de produto fora de estoque DEVE aparecer sobre a
  imagem do produto.
- **RF-11** — O sistema NÃO DEVE alterar a aparência do produto no carrossel da
  página inicial ao ajustar sua aparência na grade do catálogo.

### Avaliações da base de demonstração

- **RF-12** — A base de demonstração DEVE conter avaliações distribuídas pela
  maior parte do catálogo.
- **RF-13** — A base de demonstração DEVE deixar parte dos produtos sem
  avaliação nenhuma.
- **RF-14** — A base de demonstração DEVE ser reproduzível: recriá-la produz as
  mesmas avaliações, nas mesmas notas, nos mesmos produtos.
- **RF-15** — O sistema NÃO DEVE aceitar mais de uma avaliação da mesma pessoa
  sobre o mesmo produto.
- **RF-16** — A ordenação inicial do catálogo DEVE ser a por melhor avaliação.

### Cabeçalho

- **RF-17** — O sistema NÃO DEVE oferecer atalho para uma área de conta que
  ainda não existe.

## 6. Regras de negócio

- **RN-01** — Uma pessoa avalia um mesmo produto no máximo uma vez. Uma segunda
  avaliação da mesma pessoa sobre o mesmo produto é correção da primeira, não
  um voto a mais — e enquanto não existir tela de edição, é simplesmente
  recusada.
- **RN-02** — Produto sem avaliação nenhuma não é escondido nem tratado como
  nota zero: aparece na lista e vai para o fim da ordenação por avaliação.
  Ausência de nota não é nota ruim.
- **RN-03** — Um controle oferecido ao cliente entrega o que anuncia. Atalho
  que leva a erro e ordenação que não ordena são defeito, não enfeite — é a
  mesma regra que tirou "Mais Vendidos" da página inicial na `013` e que
  desabilitou os três controles do cartão na `012`.
- **RN-04** — Toda ordenação do catálogo termina em desempate por nome, para
  que páginas consecutivas não se sobreponham nem escondam produto. A mudança
  da ordenação inicial não abre exceção a isso.

## 7. Critérios de aceite

### CA-01 — Filtrar não recarrega a página
- **Dado** que estou no catálogo de uma categoria
- **Quando** marco uma subcategoria
- **Então** a lista de produtos muda sem que a página seja recarregada

### CA-02 — O endereço acompanha o filtro
- **Dado** que marquei uma subcategoria e a lista mudou
- **Quando** olho o endereço da página
- **Então** ele contém o filtro aplicado, e abrir esse endereço numa aba nova
  mostra a mesma lista

### CA-03 — O botão voltar desfaz a filtragem
- **Dado** que apliquei um filtro e a lista mudou
- **Quando** uso o botão voltar do navegador
- **Então** volto à lista anterior, com os controles no estado anterior

### CA-04 — A rolagem é preservada ao filtrar
- **Dado** que rolei a página até o meio da lista
- **Quando** troco a ordenação
- **Então** continuo aproximadamente onde estava, sem ser jogado ao topo

### CA-05 — Trocar de página leva ao início da lista
- **Dado** que estou no fim da primeira página
- **Quando** vou para a segunda
- **Então** vejo o início da nova lista, não o fim dela

### CA-06 — A mudança é anunciada
- **Dado** que uso leitor de tela
- **Quando** aplico um filtro
- **Então** sou informado da nova quantidade de produtos

### CA-07 — Sem JavaScript continua funcionando
- **Dado** que o navegador está com JavaScript desligado
- **Quando** marco uma subcategoria e confirmo a filtragem
- **Então** recebo a lista filtrada, e paginação e ordenação também funcionam

### CA-08 — Falha na atualização não deixa tela quebrada
- **Dado** que a atualização parcial não pode ser concluída
- **Quando** isso acontece
- **Então** recebo a página completa do resultado pedido

### CA-09 — Trocar de categoria troca os filtros oferecidos
- **Dado** que estou no catálogo de uma categoria
- **Quando** escolho outra categoria
- **Então** as subcategorias oferecidas passam a ser as da nova categoria

### CA-10 — O produto preenche a coluna
- **Dado** que abro o catálogo
- **Quando** comparo a largura de um produto com a da coluna que ele ocupa
- **Então** são a mesma, sem faixa vazia ao lado

### CA-11 — A linha alinha
- **Dado** que uma linha da grade tem um produto de nome curto e outro de nome
  longo o bastante para ocupar duas linhas
- **Quando** comparo a altura em que estão os botões dos dois
- **Então** estão na mesma altura

### CA-12 — A etiqueta fica sobre a imagem
- **Dado** que um produto está fora de estoque
- **Quando** olho onde a etiqueta aparece
- **Então** ela está sobre a imagem do produto, dentro dos limites dela

### CA-13 — O carrossel não regride
- **Dado** que abro a página inicial
- **Quando** olho os produtos do carrossel
- **Então** eles continuam como estavam antes desta feature

### CA-14 — A maior parte do catálogo tem avaliação
- **Dado** que a base de demonstração foi criada
- **Quando** conto os produtos com pelo menos uma avaliação
- **Então** são a maior parte do catálogo, e não apenas um punhado

### CA-15 — Parte do catálogo não tem avaliação
- **Dado** que a base de demonstração foi criada
- **Quando** conto os produtos sem avaliação nenhuma
- **Então** existem, e ao ordenar por avaliação eles aparecem depois dos
  avaliados, não sumidos

### CA-16 — A base se repete
- **Dado** que a base de demonstração foi criada duas vezes do zero
- **Quando** comparo as avaliações das duas
- **Então** são as mesmas notas, nos mesmos produtos

### CA-17 — Ninguém avalia duas vezes
- **Dado** que uma pessoa já avaliou um produto
- **Quando** uma segunda avaliação dela sobre o mesmo produto é registrada
- **Então** é recusada

### CA-18 — A ordenação inicial é por avaliação
- **Dado** que abro o catálogo sem escolher ordenação
- **Quando** olho o seletor de ordenação e a ordem dos produtos
- **Então** ambos indicam melhor avaliados, e produtos mais bem avaliados vêm
  antes

### CA-19 — Páginas não se sobrepõem
- **Dado** que percorro o catálogo página a página com a ordenação inicial
- **Quando** comparo os produtos de páginas consecutivas
- **Então** nenhum produto aparece em duas páginas nem some entre elas

### CA-21 — O teclado não perde o lugar
- **Dado** que percorro o catálogo apenas com o teclado
- **Quando** troco de página pela paginação
- **Então** o foco fica junto do novo resultado, e não volta ao início do
  documento

### CA-20 — O cabeçalho não oferece conta
- **Dado** que entrei na loja
- **Quando** olho o cabeçalho
- **Então** não há atalho que leve a uma página de conta inexistente

## 8. Fora de escopo

- **Exibir a nota do produto no cartão.** Discutido e descartado: o cartão do
  catálogo difere do da vitrine apenas em estilo, não em conteúdo. Ver seção 10.
- **Mudar a densidade da grade.** Continua com o mesmo número de colunas de
  hoje. Ajustar o cartão para preencher a coluna não é redesenhar a grade.
- **Tela de escrever avaliação.** Continua no backlog, dependente do carrinho —
  esta feature só semeia avaliações e impede duplicidade.
- **Tela de editar ou apagar avaliação.** A recusa da segunda avaliação da
  mesma pessoa (RN-01) é a regra; a tela que permitiria corrigir a primeira é
  entrega própria.
- **Criar a página de conta.** Aqui o atalho só deixa de prometer o que não
  existe.
- **"Mais vendidos".** Continua indisponível até a cadeia da loja registrar
  pedidos.
- **Busca por texto.** O campo do cabeçalho segue sem função, no backlog.
- **Estouro horizontal do cabeçalho em telas estreitas.** Defeito
  pré-existente, registrado desde a `009`.

## 9. Dependências

- **Depende de:** `012-catalogo`, que entregou o catálogo, o cartão e a
  ordenação; `008-pagina-do-produto`, que criou avaliação e voto de útil.
- **Bloqueia:** nada. Antecede a cadeia da loja por escolha, não por
  impedimento técnico.

## 10. Decisões e pendências

**Trocar de categoria recarrega a página; trocar de filtro não.** A barra
lateral só muda de conteúdo quando a categoria muda — ao marcar subcategoria,
ordenar ou paginar, ela já está no estado correto, porque foi o cliente quem a
mexeu. Reconstruí-la nesses casos arrancaria o foco do teclado do controle
recém-usado, sem nada em troca. Trocar de categoria é mudança de contexto
maior, e a recarga completa ali é aceitável.

**O cartão do catálogo difere do da vitrine apenas em estilo.** Foi oferecida a
alternativa de exibir a média de avaliações no cartão do catálogo — o que daria
finalidade visível ao seed e justificaria uma divergência de conteúdo — e foi
descartada pelo responsável. O seed serve à ordenação e à página do produto.

**A ordenação inicial passa a ser "melhor avaliados".** Fecha o item de backlog
aberto pela `012`, que registrou "Nome (A-Z)" como escolha provisória por não
empatar, à espera de dado que sustentasse outra coisa. O dado passa a existir
com esta feature.

**A base de demonstração continua sendo mock.** O catálogo real da loja, com
390 produtos, segue no backlog aguardando exportação. As avaliações semeadas
aqui são de clientes fictícios e não representam opinião de ninguém.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** — pendência
herdada da `013` §10, ainda sem critério definido. Não entrou nesta feature por
seguir sem resposta do responsável.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — RF-06
      e CA-08 cobrem a falha da atualização parcial
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — a pendência da seção
      10 é decisão de negócio em aberto, herdada e registrada, não indecisão
      técnica desta feature
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
