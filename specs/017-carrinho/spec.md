# Especificação — Carrinho

**ID:** `017-carrinho` · **Branch:** `017-carrinho`
**Criada em:** 2026-08-23 · **Status:** Rascunho

---

## 1. Contexto e problema

**O carrinho é a última promessa da loja que não existe.** O seletor de
quantidade e o botão "Adicionar ao carrinho" estão em todo cartão de produto
desde a `012`, desabilitados de propósito, porque fingiam funcionar sem gravar
nada. A `015` ligou o coração — o único dos três que não dependia de mais nada.
Os outros dois continuam ali, apagados, esperando esta feature.

**O atalho do cabeçalho não leva a lugar nenhum.** `<a href="#">Meu carrinho</a>`
é o último link morto do site, no mesmo padrão que a `009` fechou no rodapé, a
`012` nos atalhos de categoria e a `015` no botão de favoritos.

**Não existe onde guardar um carrinho.** As catorze tabelas modeladas em
`ModelagemBancoTCC.dbml` cobrem produto, pedido, item de pedido, pagamento,
endereço, avaliação e favorito — nenhuma delas é carrinho. `ItemPedido` existe,
mas ele é o item de um pedido já fechado: exige `PedidoId`, e um pedido exige
endereço de entrega e valor total. Não serve para uma intenção que ainda está
sendo montada.

**A página do produto tem um seletor de quantidade que não vai a lugar nenhum.**
A `008` construiu o controle de 1 a 99, funcionando, com o número mudando na
tela — e nada acontece com esse número.

## 2. Objetivo

Dar à loja um carrinho de verdade: montar, ver, alterar e remover, sobrevivendo
ao logout e ao fechamento do navegador, montável por quem ainda não entrou e
preservado quando ele entra.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Monta carrinho sem entrar; ao entrar, encontra tudo que escolheu somado ao que já tinha guardado |
| Cliente autenticado | Monta, altera e remove; o carrinho continua lá na próxima visita, em qualquer aparelho |
| Administrador da loja | Não é afetado nesta entrega |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero juntar vários produtos antes de decidir
> comprar, em vez de decidir um a um.
>
> **HU-02** — Como **cliente**, quero que o que eu escolhi continue lá quando eu
> voltar amanhã, de outro computador.
>
> **HU-03** — Como **visitante**, quero escolher produtos antes de criar conta,
> e não perder essa escolha quando eu entrar.
>
> **HU-04** — Como **cliente**, quero mudar a quantidade ou tirar um item sem a
> página inteira piscar.
>
> **HU-05** — Como **cliente**, quero saber de longe quantos itens já coloquei,
> sem abrir o carrinho.
>
> **HU-06** — Como **cliente**, quero entender por que um item que escolhi não
> está mais somando no total, em vez de ver o valor mudar sozinho.

## 5. Requisitos funcionais

### Montar o carrinho

- **RF-01** — O cliente DEVE poder acrescentar um produto ao carrinho a partir
  do cartão de produto, onde quer que o cartão apareça.
- **RF-02** — O cliente DEVE poder acrescentar um produto a partir da página do
  produto, na quantidade que escolheu no seletor.
- **RF-03** — Acrescentar um produto que já está no carrinho DEVE somar à
  quantidade existente, não criar uma segunda linha.
- **RF-04** — O sistema NÃO DEVE permitir acrescentar produto indisponível.

### Ver e alterar

- **RF-05** — O cliente DEVE poder ver, numa tela própria, todos os itens do
  carrinho com nome, imagem, preço unitário, quantidade e valor da linha.
- **RF-06** — O cliente DEVE poder alterar a quantidade de um item.
- **RF-07** — O cliente DEVE poder remover um item.
- **RF-08** — A tela DEVE apresentar o subtotal dos itens que contam.
- **RF-09** — O carrinho sem nenhum item DEVE oferecer caminho para o catálogo,
  em vez de uma área vazia.

### Permanência

- **RF-10** — O carrinho do cliente autenticado DEVE sobreviver ao logout, ao
  fechamento do navegador e à troca de aparelho.
- **RF-11** — O visitante DEVE poder montar carrinho sem entrar.
- **RF-12** — Ao entrar, o carrinho montado como visitante DEVE se juntar ao
  carrinho que a pessoa já tinha guardado.
- **RF-13** — Concluída a junção, o carrinho de visitante DEVE deixar de
  existir.

### Indicação no cabeçalho

- **RF-14** — O cabeçalho DEVE indicar quantos itens há no carrinho.
- **RF-15** — O atalho do cabeçalho DEVE levar à tela do carrinho.

### Produto que deixou de estar disponível

- **RF-16** — Produto que deixou de estar disponível para compra DEVE continuar
  aparecendo na tela do carrinho, sinalizado, com o motivo distinguível entre
  "saiu do catálogo" e "fora de estoque".
- **RF-17** — Item sinalizado NÃO DEVE somar no subtotal.

### Sem JavaScript, e com ele

- **RF-18** — Acrescentar, alterar quantidade e remover DEVEM funcionar sem
  JavaScript.
- **RF-19** — Havendo JavaScript, alterar quantidade e remover NÃO DEVEM
  recarregar a página inteira.

### Pendência anunciada

- **RF-20** — A tela DEVE apresentar o caminho para concluir a compra,
  sinalizado como ainda indisponível enquanto o fechamento não existir.

## 6. Regras de negócio

- **RN-01** — Um produto aparece no máximo uma vez no carrinho de uma pessoa.
  Pedir de novo o que já está lá **soma** à quantidade — diferente do favorito,
  que é interruptor.
- **RN-02** — A quantidade de um item vai de 1 a 99. É o mesmo limite que a
  `008` fixou no seletor da página do produto (RN-10 de lá): uma regra só no
  sistema inteiro. Reduzir abaixo de 1 remove o item.
- **RN-03** — O carrinho é privado: cada pessoa vê apenas o seu, e ninguém vê o
  de outra.
- **RN-04** — O preço mostrado é sempre o preço atual do produto. **O carrinho é
  intenção, não contrato** — quem congela preço é o pedido, e isso acontece no
  fechamento. Um carrinho pode ficar semanas parado, e honrar preço de semanas
  atrás é compromisso comercial que a loja não assumiu.
- **RN-05** — Ao juntar o carrinho de visitante com o carrinho guardado, as
  quantidades do mesmo produto **se somam**, limitadas ao máximo da RN-02. Nada
  que a pessoa escolheu é descartado em silêncio.
- **RN-06** — **"Disponível para compra" é um estado só, com dois motivos de
  recusa.** Produto que saiu do catálogo e produto fora de estoque são igualmente
  incompráveis: nenhum dos dois entra no carrinho (RF-04), e nenhum dos dois
  soma no subtotal se já estiver lá (RF-17). O que os distingue é a **mensagem**,
  não o efeito — quem vê "fora de estoque" sabe que vale esperar; quem vê "saiu
  do catálogo" sabe que não vale.
- **RN-07** — Item indisponível **não é apagado** do carrinho. Voltando a ficar
  disponível, ele volta a contar sozinho. É o mesmo princípio da RN-03 da `015`
  — esconder não é esquecer — só que aqui nem esconder se faz: um subtotal que
  muda sozinho entre duas visitas é pior que um item sinalizado.
- **RN-08** — Um controle oferecido ao cliente entrega o que anuncia. Regra
  herdada: desabilitou os três controles do cartão na `012`, tirou "Mais
  vendidos" da página inicial na `013`, desligou o atalho "Conta" na `014` — e é
  por ela que o fechamento aparece sinalizado aqui, em vez de prometer.

## 7. Critérios de aceite

### CA-01 — Acrescentar do cartão
- **Dado** que estou autenticado e vejo um produto no catálogo
- **Quando** aciono o botão de carrinho do cartão
- **Então** o produto passa a estar no meu carrinho

### CA-02 — Acrescentar da página do produto, na quantidade escolhida
- **Dado** que estou na página de um produto e escolhi quantidade 3
- **Quando** acrescento ao carrinho
- **Então** o carrinho passa a ter aquele produto com quantidade 3

### CA-03 — Acrescentar o que já está soma
- **Dado** que já tenho um produto no carrinho com quantidade 2
- **Quando** acrescento o mesmo produto com quantidade 3
- **Então** ele passa a ter quantidade 5, numa linha só

### CA-04 — Produto indisponível não entra
- **Dado** que um produto está fora do catálogo público ou fora de estoque
- **Quando** tento acrescentá-lo
- **Então** ele não entra no carrinho, e recebo uma explicação

### CA-05 — Ver o carrinho
- **Dado** que tenho produtos no carrinho
- **Quando** abro a tela do carrinho
- **Então** vejo cada um com nome, imagem, preço, quantidade e valor da linha

### CA-06 — Alterar a quantidade
- **Dado** que estou na tela do carrinho
- **Quando** altero a quantidade de um item
- **Então** o valor da linha e o subtotal acompanham

### CA-07 — Remover um item
- **Dado** que estou na tela do carrinho
- **Quando** removo um item
- **Então** ele sai da lista e deixa de contar no subtotal

### CA-08 — Reduzir abaixo de um remove
- **Dado** que um item está com quantidade 1
- **Quando** reduzo a quantidade
- **Então** o item sai do carrinho

### CA-09 — O limite superior é respeitado
- **Dado** que um item está com quantidade 99
- **Quando** tento aumentar
- **Então** ele continua em 99

### CA-10 — Carrinho vazio convida
- **Dado** que não tenho nenhum item
- **Quando** abro o carrinho
- **Então** encontro uma explicação e um caminho para o catálogo

### CA-11 — O carrinho sobrevive à saída
- **Dado** que montei um carrinho e saí da conta
- **Quando** entro de novo
- **Então** o carrinho está como eu deixei

### CA-12 — O visitante monta carrinho
- **Dado** que não estou autenticado
- **Quando** acrescento produtos ao carrinho
- **Então** eles entram, e consigo vê-los e alterá-los

### CA-13 — O que o visitante montou se junta ao que estava guardado
- **Dado** que eu tinha um produto guardado com quantidade 3, e como visitante
  acrescentei o mesmo produto com quantidade 2
- **Quando** entro
- **Então** o carrinho tem aquele produto com quantidade 5

### CA-14 — O carrinho de visitante não sobra depois
- **Dado** que entrei e meu carrinho de visitante foi juntado
- **Quando** saio e volto como visitante
- **Então** o carrinho de visitante está vazio

### CA-15 — O cabeçalho indica a quantidade
- **Dado** que tenho itens no carrinho
- **Quando** olho o cabeçalho de qualquer página
- **Então** vejo quantos itens são

### CA-16 — O atalho leva ao carrinho
- **Dado** que estou em qualquer página
- **Quando** aciono o atalho de carrinho do cabeçalho
- **Então** chego à tela do carrinho

### CA-17 — Produto indisponível fica visível e não soma
- **Dado** que tenho um produto no carrinho e ele deixa de estar disponível
- **Quando** abro o carrinho
- **Então** ele aparece sinalizado, e o subtotal considera apenas os demais

### CA-18 — Os dois motivos de indisponibilidade se distinguem
- **Dado** que tenho um item que saiu do catálogo e outro que está fora de
  estoque
- **Quando** abro o carrinho
- **Então** os dois estão sinalizados e nenhum soma, mas as mensagens dizem
  coisas diferentes

### CA-19 — O item indisponível volta a contar
- **Dado** que um item do carrinho está sinalizado como indisponível
- **Quando** o produto volta a ficar disponível
- **Então** ele volta a somar no subtotal, sem eu ter feito nada

### CA-20 — Funciona sem JavaScript
- **Dado** que o navegador está com JavaScript desligado
- **Quando** acrescento, altero quantidade e removo
- **Então** os três funcionam

### CA-21 — Alterar não recarrega a página
- **Dado** que estou na tela do carrinho, com JavaScript
- **Quando** altero a quantidade de um item
- **Então** o restante da página não é recarregado

### CA-22 — O carrinho é privado
- **Dado** que duas pessoas diferentes têm carrinho
- **Quando** cada uma abre o seu
- **Então** nenhuma vê item da outra

### CA-23 — O fechamento é anunciado, não oferecido
- **Dado** que tenho itens no carrinho
- **Quando** olho o caminho para concluir a compra
- **Então** ele está visível e sinalizado como ainda indisponível

## 8. Fora de escopo

- **Fechamento do pedido.** Gerar `Pedido`, `ItemPedido` e `Pagamento` é a
  `019`. Aqui o caminho existe, sinalizado.
- **Endereço de entrega.** É a `018`.
- **Frete.** Depende de endereço para ser calculado, e a integração com o
  serviço de cálculo é decisão da `019`. Nenhum valor de frete aparece nesta
  tela.
- **Estoque real.** A recusa de produto indisponível usa o
  `ProdutoStatus.ForaDeEstoque` que existe hoje, marcado à mão. Contagem de
  unidades e teto de quantidade por estoque são a `020`.
- **Cupom de desconto.** A entidade `Promocao` existe desde a `003` e continua
  sem uso.
- **Salvar para depois, lista de desejos, comprar de novo.** Nenhum foi pedido;
  favoritos já cobre a intenção de "guardar para olhar depois".
- **Carrinho abandonado, lembrete por e-mail.** Ideia adjacente, sem pedido.
- **Página de conta do cliente.** O atalho "Conta" segue desabilitado como a
  `014` deixou.

## 9. Dependências

- **Depende de:** `003`, que criou `Produto` e o modelo de dados; `008`, que
  fixou o limite de 1 a 99 no seletor de quantidade; `012`, que entregou o
  cartão com os dois controles desabilitados; `015`, que estabeleceu o padrão de
  POST com atualização no lugar, o endereço de retorno no login e o piso sem
  JavaScript para uma ação de gravação.
- **Bloqueia:** a `019` (fechamento), que consome o carrinho para gerar o
  pedido. A `018` (endereços) é independente e pode ser feita em paralelo.

## 10. Decisões e pendências

**A cadeia da loja desloca pela quinta vez.** Esta feature toma o `017`, que a
cadeia reservava a Estoque. Passa a ser: Carrinho `017`, Endereços `018`,
Fechamento `019`, Estoque `020`, Processamento de pagamento `021`. Como nas
quatro vezes anteriores, a varredura de `spec 0NN` na base inteira — **inclusive
nesta spec** — entra como tarefa, não como boa intenção.

**O carrinho é uma tabela, não sessão pura.** Foi decisão explícita do
responsável, mudando uma inclinação anterior. A consequência é que o modelo de
dados do TCC ganha a décima quinta tabela, e o diagrama precisa ser atualizado
junto. O ganho é o que a `HU-02` pede: o carrinho segue a pessoa entre aparelhos
e sobrevive a qualquer tempo.

**O carrinho do visitante vive na sessão, e isso tem preço.** Foi a alternativa
escolhida entre três. A sessão do servidor não está ligada hoje, então esta
feature a introduz — e o armazenamento padrão é em memória, o que significa que
**o carrinho de um visitante desaparece se a aplicação reiniciar**. Para quem
não tem conta, é consequência aceita e registrada; para quem tem, a RF-10
garante o contrário.

**Ao juntar, as quantidades se somam.** Entre somar, deixar a sessão vencer,
deixar o banco vencer e pegar a maior das duas, a soma foi escolhida por não
descartar nada em silêncio — e por ser o que as lojas grandes fazem, logo o que
a pessoa já espera. O teto da RN-02 corta o excesso.

**Não existe coluna de preço na tabela.** Decisão registrada na RN-04. O
contraste com `ItemPedido.PrecoUnitario` é proposital: aquele congela porque
pedido é contrato; este não congela porque carrinho é intenção.

**Produto indisponível fica visível, ao contrário dos favoritos.** A `015`
decidiu esconder favorito indisponível, e estava certa: aquela lista é um
lembrete, e um lembrete a menos não confunde ninguém. Aqui é diferente — o
carrinho tem um total, e um item que some sem avisar faz o total mudar entre
duas visitas sem explicação. Sinalizar é o que respeita a RN-06.

**A junção não mora no controlador de autenticação.** A `015` evitou de
propósito acoplar `AutenticacaoController` ao domínio de favoritos, e a mesma
razão vale aqui. Como a junção é do lado do servidor, ela precisa acontecer
quando uma requisição autenticada encontra carrinho pendente na sessão — não
dentro da ação de login. Assim funciona por qualquer caminho de entrada, sem o
login saber que carrinho existe.

**Achado ao levantar esta feature: o parâmetro `itensCarrinho` tem a forma
errada.** `HeaderViewComponent.InvokeAsync` recebe `int itensCarrinho = 0`
desde sempre, e o `_Layout` invoca `Component.InvokeAsync("Header")` sem
argumento nenhum. Nenhum layout teria como saber essa contagem — ela depende de
quem está vendo. O componente precisa buscá-la sozinho, como já faz com as
categorias e com o termo de busca. O parâmetro sai.

**Achado: o atalho "Meu carrinho" é `href="#"`.** Último link morto do site.
Esta feature o liga.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** —
pendência herdada da `013` §10, repetida na `014`, `015` e `016`, ainda sem
critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — a tabela nova é citada na seção 10 como decisão tomada,
      não como requisito
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — RF-04 e
      CA-04 cobrem produto indisponível recusado; RF-16/RF-17 e CA-17 a CA-19,
      produto que fica indisponível depois de entrar e volta; CA-08 e CA-09, os
      dois limites de quantidade; CA-22, acesso ao carrinho alheio
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — a pendência da seção
      10 é decisão de negócio herdada e registrada, não indefinição desta
      feature
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
