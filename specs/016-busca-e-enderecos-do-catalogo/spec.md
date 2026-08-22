# Especificação — Busca e endereços do catálogo

**ID:** `016-busca-e-enderecos-do-catalogo` · **Branch:** `016-busca-e-enderecos-do-catalogo`
**Criada em:** 2026-08-22 · **Status:** Rascunho

---

## 1. Contexto e problema

**A barra de pesquisa do cabeçalho não pesquisa nada.** Ela está em toda página
do site desde o começo, com o texto "Buscar produto..." dentro, e é apenas um
campo solto: não pertence a formulário nenhum, não tem nome, não submete para
lugar nenhum. Digitar e apertar Enter não faz absolutamente nada. É a última
promessa visível do cabeçalho que ainda não foi cumprida — os quatro atalhos
mortos morreram na `012`, o "Favoritos" ganhou destino na `015`, e sobrou esta.

O que torna a lacuna barata de fechar é que a tela de resultado já existe. O
catálogo da `012`, refinado pela `014` e pela `015`, já sabe ordenar, paginar,
filtrar por categoria e subcategoria, desenhar o cartão com favorito e trocar
só o bloco de resultado sem recarregar a página. Uma busca por texto é mais um
filtro entrando nessa mesma máquina.

**O endereço do catálogo mostra identificador técnico quando filtra
subcategoria.** A categoria aparece por nome legível — `/Catalogo/emporio` —
mas a subcategoria aparece como um identificador de 36 caracteres, gerado pelo
banco e sem significado nenhum para quem lê:

```
hoje:     /Catalogo/doces?subcategorias=3f2a91c4-8b7e-4d15-9c02-1a6f5e8d7b30
desejado: /Catalogo/doces?subcategorias=barras
```

Um endereço assim não pode ser lido, ditado, escrito à mão nem reconhecido numa
lista de favoritos do navegador. E, ao contrário da categoria, ele muda a cada
vez que o banco é recriado — o mesmo filtro produz endereços diferentes em
máquinas diferentes.

**A tela de cadastro de produto é a única do sistema sem desenho.** Ela usa os
mesmos nomes de estilo que as outras telas de formulário, mas não carrega a
folha de estilo que os define, e não tem o contêiner nem o título que as demais
têm. O resultado é um formulário cru, sem largura contida e sem cabeçalho, ao
lado de uma tela irmã — o cadastro de administrador — que veste o padrão da
marca corretamente. Não falta referência visual: falta aplicar a que já existe.

## 2. Objetivo

Fazer a barra de pesquisa pesquisar, tornar o endereço do catálogo legível
quando ele filtra subcategoria, e pôr a tela de cadastro de produto no mesmo
padrão visual das demais telas de formulário.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante ou autenticado) | Passa a encontrar produto por nome, de qualquer tela; lê e compartilha endereços de catálogo que fazem sentido |
| Administrador da loja | Cadastra produto numa tela com o mesmo desenho das outras que ele já usa |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero digitar o nome de um produto e encontrá-lo,
> em vez de percorrer categoria por categoria até achar.
>
> **HU-02** — Como **cliente**, quero encontrar "Café" digitando "cafe", sem
> ter que acertar o acento.
>
> **HU-03** — Como **cliente**, quero refinar o que busquei — por categoria,
> por preço, por página — sem que a busca se perca no caminho.
>
> **HU-04** — Como **cliente**, quero saber qual termo produziu a tela que
> estou vendo, e desfazê-lo sem perder os outros filtros.
>
> **HU-05** — Como **cliente**, quero mandar para alguém o endereço de um
> filtro do catálogo e que a pessoa entenda o que vai abrir antes de clicar.
>
> **HU-06** — Como **administrador**, quero cadastrar produto numa tela que
> pareça parte do mesmo sistema que as outras que eu uso.

## 5. Requisitos funcionais

### Busca por produto

- **RF-01** — A barra de pesquisa do cabeçalho DEVE procurar produtos pelo
  nome.
- **RF-02** — A busca DEVE alcançar a loja inteira, qualquer que seja a tela de
  onde foi acionada.
- **RF-03** — A busca DEVE encontrar o produto independentemente de acento e de
  caixa no que foi digitado.
- **RF-04** — O resultado da busca DEVE oferecer os mesmos recursos do
  catálogo: ordenação, paginação, filtro por categoria e por subcategoria, e o
  cartão de produto completo.
- **RF-05** — O termo DEVE sobreviver a qualquer refinamento feito sobre o
  resultado — trocar a ordenação, marcar uma subcategoria, escolher uma
  categoria ou mudar de página.
- **RF-06** — A barra de pesquisa DEVE reexibir o termo que produziu a tela
  atual.
- **RF-07** — O resultado DEVE oferecer um caminho para desfazer a busca que
  preserve os demais filtros aplicados.
- **RF-08** — A busca sem resultado DEVE dizer isso mencionando o termo
  procurado, em vez de aconselhar sobre filtros que não foram usados.
- **RF-09** — Buscar com o campo vazio DEVE levar ao catálogo completo, sem
  erro.
- **RF-10** — A busca DEVE funcionar sem JavaScript.
- **RF-11** — Produto fora do catálogo público NÃO DEVE aparecer no resultado
  da busca.

### Endereço legível de subcategoria

- **RF-12** — O endereço do catálogo DEVE identificar a subcategoria filtrada
  por um nome legível, e não por identificador técnico.
- **RF-13** — O endereço DEVE continuar comportando mais de uma subcategoria ao
  mesmo tempo.
- **RF-14** — Nome de subcategoria que não corresponda a nenhuma da categoria
  aberta DEVE ser ignorado, e o catálogo daquela categoria DEVE ser exibido
  normalmente.
- **RF-15** — Todos os caminhos que hoje levam a uma subcategoria — o menu
  suspenso do cabeçalho e o filtro da barra lateral — DEVEM produzir o endereço
  legível.

### Tela de cadastro de produto

- **RF-16** — A tela de cadastro de produto DEVE apresentar-se no mesmo padrão
  das demais telas de formulário do sistema: título, largura contida, campos,
  botão e mensagens no desenho da marca.
- **RF-17** — Os campos curtos DEVEM dividir a linha em tela larga e empilhar
  em tela estreita.
- **RF-18** — As demais telas de formulário do sistema NÃO DEVEM mudar de
  aparência por causa desta feature.

## 6. Regras de negócio

- **RN-01** — Busca é um filtro entre os outros, não um modo à parte. Ela se
  combina com categoria, subcategoria, "sem açúcar" e ordenação, em vez de
  substituí-los — quem busca e depois clica numa categoria está estreitando o
  resultado, não recomeçando.
- **RN-02** — A comparação do termo ignora acento e caixa **dos dois lados**:
  tanto o que a pessoa digitou quanto o nome guardado do produto. Ignorar de um
  lado só faria "Café" encontrável por "Café" e por mais nada.
- **RN-03** — O nome legível de uma subcategoria é único **dentro da categoria
  dela**, não na loja inteira. Duas categorias podem ter subcategorias de mesmo
  nome — "Cappuccino" existe em Doces e em Empório hoje —, e cada uma só é lida
  sob o endereço da própria categoria, então elas nunca se confundem.
- **RN-04** — Filtro que não pode ser aplicado não impede a página. Valor de
  subcategoria irreconhecível é descartado em silêncio; só a categoria, que é a
  página pedida, produz "não encontrado" quando não existe. É a diferença entre
  pedir uma página que não existe e pedir um recorte que não existe.
- **RN-05** — Toda combinação de busca e filtro é reproduzível pelo endereço:
  abrir o mesmo endereço noutra aba mostra o mesmo resultado. Nenhum estado de
  busca vive escondido no navegador.
- **RN-06** — Produto que saiu do catálogo público não existe do lado de fora,
  em nenhum caminho de consulta — inclusive na busca. É a `RN-01` da `012`,
  aplicada ao caminho novo.

## 7. Critérios de aceite

### CA-01 — A barra de pesquisa encontra
- **Dado** que estou em qualquer página da loja
- **Quando** digito o nome de um produto na barra de pesquisa e submeto
- **Então** vejo uma tela de resultado com esse produto

### CA-02 — A busca varre a loja inteira
- **Dado** que estou no catálogo de uma categoria
- **Quando** busco um produto que pertence a outra categoria
- **Então** ele aparece no resultado

### CA-03 — Acento não atrapalha
- **Dado** que existe um produto cujo nome tem acento
- **Quando** busco o mesmo nome sem acento, ou em caixa diferente
- **Então** o produto aparece no resultado

### CA-04 — O resultado é um catálogo completo
- **Dado** que fiz uma busca com muitos resultados
- **Quando** olho a tela
- **Então** tenho ordenação, paginação, a barra lateral de categorias e o
  cartão de produto com favorito, como em qualquer catálogo

### CA-05 — Refinar não perde a busca
- **Dado** que busquei um termo
- **Quando** troco a ordenação, marco uma subcategoria, escolho uma categoria
  ou avanço de página
- **Então** o resultado continua restrito ao termo buscado

### CA-06 — A barra mostra o que foi buscado
- **Dado** que fiz uma busca
- **Quando** olho a barra de pesquisa do cabeçalho
- **Então** ela contém o termo que produziu a tela

### CA-07 — Desfazer a busca preserva o resto
- **Dado** que busquei um termo e depois escolhi uma categoria
- **Quando** desfaço a busca
- **Então** continuo naquela categoria, agora sem o termo

### CA-08 — Nada encontrado explica o que houve
- **Dado** que busquei um termo que não corresponde a produto nenhum
- **Quando** olho o resultado
- **Então** ele diz que nada foi encontrado para aquele termo, sem me mandar
  desmarcar filtros que eu não marquei

### CA-09 — Busca vazia não quebra
- **Dado** que a barra de pesquisa está vazia
- **Quando** submeto
- **Então** vejo o catálogo completo, sem erro

### CA-10 — A busca funciona sem JavaScript
- **Dado** que o navegador está com JavaScript desligado
- **Quando** busco um produto
- **Então** vejo o resultado normalmente

### CA-11 — Produto indisponível não aparece
- **Dado** que existe um produto fora do catálogo público
- **Quando** busco exatamente o nome dele
- **Então** ele não aparece no resultado

### CA-12 — O endereço mostra o nome da subcategoria
- **Dado** que abro o catálogo de uma categoria
- **Quando** marco uma subcategoria no filtro
- **Então** o endereço passa a conter o nome legível dela, não um identificador
  técnico

### CA-13 — Duas subcategorias cabem no endereço
- **Dado** que marquei duas subcategorias
- **Quando** olho o endereço
- **Então** as duas aparecem por nome, e o resultado soma as duas

### CA-14 — Nome de subcategoria desconhecido é ignorado
- **Dado** que abro o catálogo de uma categoria com um nome de subcategoria que
  não existe nela
- **Quando** a página carrega
- **Então** vejo o catálogo daquela categoria inteiro, sem erro

### CA-15 — Mesmo nome em categorias diferentes não se confunde
- **Dado** que duas categorias têm subcategorias de mesmo nome
- **Quando** abro o filtro dessa subcategoria em cada uma das categorias
- **Então** cada tela mostra apenas os produtos da subcategoria daquela
  categoria

### CA-16 — O menu do cabeçalho leva ao endereço legível
- **Dado** que abro o menu suspenso de uma categoria no cabeçalho
- **Quando** escolho uma subcategoria
- **Então** o endereço que abre contém o nome legível dela

### CA-17 — O cadastro de produto veste o padrão
- **Dado** que entrei como administrador
- **Quando** abro o cadastro de produto
- **Então** a tela tem título, largura contida e campos no mesmo desenho do
  cadastro de administrador

### CA-18 — O formulário se adapta à tela estreita
- **Dado** que abro o cadastro de produto num aparelho estreito
- **Quando** percorro o formulário
- **Então** os campos estão empilhados e nada transborda para os lados

### CA-19 — As outras telas de formulário não regridem
- **Dado** que abro o login, o cadastro de cliente e o cadastro de
  administrador
- **Quando** comparo com o que eram antes desta feature
- **Então** continuam iguais

## 8. Fora de escopo

- **Carrinho, estoque, frete e fechamento de pedido.** Seguem na cadeia da
  loja, agora deslocada — ver seção 10.
- **Sugestão enquanto digita.** A barra busca ao submeter. Lista suspensa com
  resultados parciais é entrega própria, e precisa de uma página de resultado
  por baixo de qualquer forma.
- **Busca em descrição ou em nome de subcategoria.** Só o nome do produto. A
  barra lateral já é o caminho para chegar por categoria e subcategoria.
- **Ordenação por relevância.** O resultado da busca usa as mesmas ordenações
  do catálogo — ver seção 10.
- **Histórico de buscas, buscas populares, correção de digitação.** Nenhum foi
  pedido.
- **Cadastro, edição e exclusão de categoria e subcategoria pelo
  administrador.** É o que justificaria guardar o nome legível como dado
  editável em vez de derivá-lo — ver seção 10.
- **Listagem, edição e exclusão de produto pelo administrador.** Segue no
  backlog. Aqui apenas a tela de cadastro que já existe ganha desenho.
- **Revisão das demais telas de formulário.** Elas passam a compartilhar o
  mesmo desenho de forma explícita, mas não mudam de aparência (RF-18).

## 9. Dependências

- **Depende de:** `012`, que entregou o catálogo, o filtro por subcategoria e o
  nome legível de categoria; `014`, que estabeleceu a atualização do resultado
  sem recarga; `015`, que fechou o desenho do cartão e da trilha.
- **Bloqueia:** nada. A cadeia da loja segue independente desta entrega.

## 10. Decisões e pendências

**A cadeia da loja desloca uma posição.** Esta feature toma o número `016`, que
o `specs/README.md` reservava a Estoque. A cadeia passa a ser: Estoque `017`,
Carrinho `018`, Endereço do usuário `019`, Fechamento de pedido `020`,
Pagamento `021`. É o quarto deslocamento — a `013`, a `014` e a `015` fizeram o
mesmo, sempre por pendências conhecidas que não valia carregar para dentro de
uma entrega nova. O próprio README avisa que cada deslocamento deixa referência
obsoleta em comentário de código e em spec antiga, e que **quem desloca precisa
varrer a base inteira**, inclusive a spec que está escrevendo. Isso entra como
tarefa desta feature, não como boa intenção.

**O carrinho foi discutido e adiado.** Foi a primeira ideia levantada junto com
as três desta entrega, e não cabe com elas: não existe entidade de carrinho, e
quatro perguntas de negócio continuam sem resposta — frete, cupom de desconto,
carrinho de visitante e reserva de estoque. Especificá-lo exige respondê-las
primeiro.

**O nome legível da subcategoria é derivado do nome, não guardado.** Mesma
escolha que a `012` fez para a categoria e pelo mesmo motivo: a comparação
acontece sobre a lista de categorias e subcategorias que a tela já carregou em
toda requisição, então não há consulta nova nem coluna nova. A alternativa —
guardar o nome legível como coluna, com índice único — foi descartada por dois
motivos: não existe tela para o administrador criar ou renomear subcategoria,
então o valor só nasceria na carga inicial de dados; e o índice teria de ser
composto por categoria, porque "Cappuccino" existe em duas. Se um dia houver
CRUD de subcategoria, a decisão merece ser revista, e a `RN-03` é o que
delimita o que precisaria ser garantido.

**A unicidade do nome legível passa a ser garantida por teste.** Como não há
índice, um teste percorre a taxonomia real e verifica, categoria por categoria,
que os nomes legíveis das subcategorias dela são distintos entre si. É esse
teste que sustenta a `RN-03`, e ele falha no dia em que alguém acrescentar uma
subcategoria que colida.

**A busca ignora acento por comparação normalizada dos dois lados.** A
alternativa de simplesmente comparar o texto como está foi descartada: a loja
tem acento em quase todo nome de categoria e de produto — Café, Cachaça,
Empório, Pelúcia —, e uma busca que não encontra "Café" quando se digita "cafe"
falha no primeiro uso real. O custo assumido é guardar, junto de cada produto,
a forma normalizada do nome dele, mantida sempre a partir do próprio nome.

**A busca não ordena por relevância.** O resultado sai nas ordenações que o
catálogo já oferece, com a mesma ordenação padrão. Ordenar por proximidade ao termo
exigiria um critério de pontuação que ninguém pediu, e que competiria com o
seletor de ordenação já presente na tela.

**A busca é pegajosa; a categoria, não.** Buscar leva sempre ao catálogo
completo, sem categoria — a barra vive no cabeçalho, presente inclusive na
página inicial, onde não há categoria nenhuma. Dali em diante o termo acompanha
todo refinamento (RF-05), e sair dele é um gesto explícito (RF-07).

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** —
pendência herdada da `013` §10, repetida na `014` §10 e na `015` §10, ainda sem
critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os endereços que aparecem na seção 1 são o problema
      relatado, visível ao cliente, não a solução técnica
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — RF-08 e
      CA-08 cobrem busca sem resultado; RF-09 e CA-09, busca vazia; RF-14 e
      CA-14, subcategoria irreconhecível; RF-11 e CA-11, produto fora do
      catálogo público alcançado por nome exato
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — a pendência da seção
      10 é decisão de negócio herdada e registrada, não indefinição desta
      feature
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
