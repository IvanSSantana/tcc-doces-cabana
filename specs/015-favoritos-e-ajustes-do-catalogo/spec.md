# Especificação — Favoritos e ajustes do catálogo

**ID:** `015-favoritos-e-ajustes-do-catalogo` · **Branch:** `015-favoritos-e-ajustes-do-catalogo`
**Criada em:** 2026-08-21 · **Status:** Implementada

---

## 1. Contexto e problema

O coração do cartão de produto existe desde a `012` e nunca funcionou. Ele foi
desabilitado de propósito naquela feature, junto do seletor de quantidade e do
botão de carrinho, porque os três fingiam funcionar sem gravar nada. Desses
três, o favorito é o único que não depende de mais nada para existir: a tabela
está criada, a entidade está pronta, e ninguém nunca escreveu uma linha nela.

**Não há onde ver o que foi favoritado.** Mesmo que o coração gravasse, a loja
não tem uma tela que liste os favoritos de alguém.

**Entrar na loja custa o lugar onde a pessoa estava.** Quem faz login vai parar
na página inicial, sempre — não existe endereço de retorno em lugar nenhum do
sistema. Isso já incomoda hoje e inviabiliza favoritar a partir do catálogo sem
perder a navegação.

**O cartão do catálogo não corresponde à referência visual.** A `014` corrigiu a
geometria — o cartão passou a preencher a coluna e a alinhar os botões —, mas
não o desenho: a referência mostra imagem sobre fundo próprio, nome em caixa
normal, preço e seletor de quantidade na mesma linha e um botão largo de
carrinho ocupando a base do cartão.

**Dois detalhes da tela de catálogo destoam.** A trilha de navegação aparece em
caixa baixa e sem destaque no item atual, enquanto a referência mostra os nomes
em caixa alta com o último em laranja. E o controle que revela as subcategorias
além das oito principais fica preso acima da lista, em vez de acompanhar o fim
dela e oferecer o caminho de volta.

**Quatro pendências acumuladas, encontradas ao levantar esta feature.** Uma
página que só é alcançável digitando o endereço, um arquivo de script que não
existe sendo pedido em toda página, um elemento vazio no cabeçalho e uma
informação passada a um componente que não a recebe.

## 2. Objetivo

Fazer o favorito funcionar de ponta a ponta — marcar, ver a lista, desmarcar —
e deixar a tela do catálogo igual à referência visual, limpando as pendências
que apareceram no caminho.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Vê o cartão novo e a trilha corrigida; ao tentar favoritar, é convidado a entrar, e o produto fica favoritado assim que ele entra |
| Cliente autenticado | Favorita e desfavorita do próprio catálogo, e tem uma tela com tudo que guardou |
| Administrador da loja | Passa a alcançar o cadastro de produto pelo cabeçalho, sem digitar endereço |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero guardar um produto que me interessou para
> encontrá-lo depois sem ter que procurar de novo.
>
> **HU-02** — Como **cliente**, quero ver numa tela só tudo que guardei.
>
> **HU-03** — Como **visitante**, quero que o produto que tentei guardar esteja
> lá quando eu terminar de entrar, sem ter que procurá-lo de novo.
>
> **HU-04** — Como **cliente**, quero voltar para onde eu estava depois de
> entrar, em vez de ser jogado na página inicial.
>
> **HU-05** — Como **cliente**, quero abrir a lista completa de subcategorias e
> conseguir fechá-la logo onde ela termina, sem subir a página de volta.
>
> **HU-06** — Como **administrador**, quero chegar ao cadastro de produto
> clicando, não decorando endereço.

## 5. Requisitos funcionais

### Favoritar a partir do catálogo

- **RF-01** — O cliente autenticado DEVE poder marcar e desmarcar um produto
  como favorito direto do cartão, no catálogo.
- **RF-02** — O cartão DEVE indicar se aquele produto já está entre os
  favoritos de quem está vendo.
- **RF-03** — Havendo JavaScript, marcar ou desmarcar NÃO DEVE recarregar a
  página.
- **RF-04** — Sem JavaScript, marcar e desmarcar DEVEM continuar funcionando.
- **RF-05** — O controle de favoritar DEVE ser alcançável em tela sensível ao
  toque, onde não existe passagem de mouse.
- **RF-06** — O visitante que tenta favoritar DEVE ser convidado a entrar.
- **RF-07** — Terminado o login, o produto que o visitante tentou favoritar
  DEVE estar favoritado, sem que ele precise pedir de novo.

### Lista de favoritos

- **RF-08** — O cliente autenticado DEVE poder ver, numa tela própria, todos os
  produtos que favoritou.
- **RF-09** — A lista NÃO DEVE exibir produto que deixou de estar disponível ao
  público.
- **RF-10** — Desfavoritar um produto dentro da lista DEVE removê-lo dela
  imediatamente.
- **RF-11** — A lista sem nenhum produto DEVE oferecer caminho para o catálogo,
  em vez de uma área vazia.
- **RF-12** — O visitante NÃO DEVE alcançar a lista de favoritos.

### Retorno depois de entrar

- **RF-13** — Ao entrar, o sistema DEVE devolver a pessoa à página de onde ela
  partiu.
- **RF-14** — O sistema NÃO DEVE aceitar endereço de retorno que aponte para
  fora do próprio site.

### Cartão do catálogo

- **RF-15** — O cartão do catálogo DEVE corresponder à referência visual: a
  imagem sobre fundo próprio, o nome abaixo dela, o preço e o seletor de
  quantidade na mesma linha, e o botão de carrinho ocupando a largura do cartão
  na base.
- **RF-16** — O nome do produto no catálogo DEVE aparecer em caixa normal.
- **RF-17** — O sistema NÃO DEVE alterar a aparência do cartão no carrossel da
  página inicial.
- **RF-18** — O seletor de quantidade e o botão de carrinho DEVEM continuar
  indisponíveis e sinalizados como tal.

### Trilha de navegação

- **RF-19** — Os nomes da trilha DEVEM aparecer em caixa alta.
- **RF-20** — O item mais à direita da trilha DEVE usar a cor de destaque do
  tema, distinguindo-se dos anteriores.

### Subcategorias além das oito principais

- **RF-21** — Ao revelar as subcategorias restantes, o controle que as revela
  DEVE passar para o fim da lista.
- **RF-22** — Com as subcategorias reveladas, o controle DEVE oferecer
  recolhê-las.
- **RF-23** — O controle NÃO DEVE passar a depender de JavaScript.

### Pendências encontradas

- **RF-24** — Nenhuma página DEVE requisitar arquivo que não existe.
- **RF-25** — O cabeçalho NÃO DEVE conter elemento sem função.
- **RF-26** — O administrador DEVE alcançar o cadastro de produto navegando,
  sem digitar endereço.
- **RF-27** — Nenhuma tela DEVE passar ao componente de produto informação que
  ele não recebe.

## 6. Regras de negócio

- **RN-01** — Favorito é um par (pessoa, produto), e o mesmo par existe no
  máximo uma vez. Pedir de novo o que já está favoritado desfaz o favorito —
  é um interruptor, não um contador.
- **RN-02** — A lista de favoritos é privada: cada pessoa vê apenas a sua, e
  ninguém vê a de outra.
- **RN-03** — Produto que sai do catálogo público desaparece da lista de
  favoritos, mas **o favorito não é apagado**. Voltando a ficar disponível, ele
  reaparece na lista de quem o guardou. Esconder não é esquecer.
- **RN-04** — Endereço de retorno só é aceito se apontar para dentro do próprio
  site. Endereço externo é descartado e a pessoa vai para a página inicial —
  aceitar qualquer endereço transformaria a tela de login em trampolim para
  outro site.
- **RN-05** — Um controle oferecido ao cliente entrega o que anuncia. É a mesma
  regra que desabilitou os três controles do cartão na `012`, tirou "Mais
  Vendidos" da página inicial na `013` e desligou o atalho "Conta" na `014` —
  e é por ela que o carrinho continua indisponível aqui.

## 7. Critérios de aceite

### CA-01 — Favoritar do catálogo
- **Dado** que estou autenticado e vejo um produto não favoritado no catálogo
- **Quando** aciono o controle de favoritar
- **Então** ele passa a indicar que o produto está favoritado

### CA-02 — Desfavoritar é o mesmo gesto
- **Dado** que um produto já está favoritado
- **Quando** aciono o mesmo controle
- **Então** ele deixa de estar favoritado

### CA-03 — O favorito sobrevive à recarga
- **Dado** que favoritei um produto
- **Quando** recarrego o catálogo
- **Então** o produto continua indicado como favorito

### CA-04 — Favoritar não recarrega a página
- **Dado** que estou autenticado no catálogo, com JavaScript
- **Quando** favorito um produto
- **Então** o restante da página não é recarregado

### CA-05 — Favoritar funciona sem JavaScript
- **Dado** que o navegador está com JavaScript desligado e estou autenticado
- **Quando** aciono o controle de favoritar
- **Então** o produto fica favoritado e continuo na mesma listagem

### CA-06 — O controle é alcançável no toque
- **Dado** que abro o catálogo numa tela sensível ao toque
- **Quando** olho um cartão
- **Então** o controle de favoritar está visível, sem depender de passagem de
  mouse

### CA-07 — Visitante é convidado a entrar
- **Dado** que não estou autenticado
- **Quando** tento favoritar um produto
- **Então** recebo o convite para entrar, e nada é gravado

### CA-08 — O favorito pretendido se concretiza
- **Dado** que, como visitante, tentei favoritar um produto e fui convidado a
  entrar
- **Quando** termino de entrar
- **Então** volto à listagem e o produto está favoritado

### CA-09 — A lista mostra o que foi guardado
- **Dado** que favoritei alguns produtos
- **Quando** abro a lista de favoritos
- **Então** vejo exatamente esses produtos

### CA-10 — Produto indisponível não aparece
- **Dado** que favoritei um produto e ele deixou de estar disponível ao público
- **Quando** abro a lista de favoritos
- **Então** ele não aparece, e ao voltar a ficar disponível aparece de novo

### CA-11 — Desfavoritar tira da lista
- **Dado** que estou na lista de favoritos
- **Quando** desfavorito um produto
- **Então** ele sai da lista sem que eu precise recarregar

### CA-12 — Lista vazia convida
- **Dado** que não favoritei nada
- **Quando** abro a lista de favoritos
- **Então** encontro uma explicação e um caminho para o catálogo

### CA-13 — Favoritos é privado
- **Dado** que não estou autenticado
- **Quando** tento abrir a lista de favoritos
- **Então** sou levado a entrar, e não vejo lista de ninguém

### CA-14 — Entrar devolve ao lugar
- **Dado** que estou numa página do catálogo e decido entrar
- **Quando** termino de entrar
- **Então** volto àquela página, não à página inicial

### CA-15 — Retorno externo é recusado
- **Dado** que o endereço de retorno aponta para fora do site
- **Quando** termino de entrar
- **Então** vou para a página inicial, e não para o endereço externo

### CA-16 — O cartão corresponde à referência
- **Dado** que abro o catálogo
- **Quando** comparo um cartão com a referência visual
- **Então** a imagem tem fundo próprio, o nome está abaixo dela, preço e
  seletor dividem uma linha, e o botão de carrinho ocupa a largura na base

### CA-17 — O nome vem em caixa normal
- **Dado** que abro o catálogo
- **Quando** leio o nome de um produto
- **Então** ele não está inteiramente em maiúsculas

### CA-18 — O carrossel não regride
- **Dado** que abro a página inicial
- **Quando** olho os cartões do carrossel
- **Então** continuam como estavam antes desta feature

### CA-19 — Carrinho segue indisponível
- **Dado** que abro o catálogo
- **Quando** tento usar o seletor de quantidade ou o botão de carrinho
- **Então** eles não respondem e informam que ainda não estão disponíveis

### CA-20 — A trilha destaca o item atual
- **Dado** que abro o catálogo de uma categoria
- **Quando** olho a trilha de navegação
- **Então** os nomes estão em caixa alta e o item mais à direita usa a cor de
  destaque

### CA-21 — O controle acompanha a lista
- **Dado** que estou numa categoria com mais de oito subcategorias
- **Quando** revelo as restantes
- **Então** o controle passa a aparecer depois da última subcategoria e oferece
  recolher

### CA-22 — Revelar não exige JavaScript
- **Dado** que o navegador está com JavaScript desligado
- **Quando** revelo e recolho as subcategorias
- **Então** funciona nos dois sentidos

### CA-23 — Nenhum arquivo pedido em vão
- **Dado** que percorro as telas da loja
- **Quando** observo o que cada página requisita
- **Então** nenhuma requisição termina em "não encontrado"

### CA-24 — Administrador chega ao cadastro clicando
- **Dado** que entrei como administrador
- **Quando** procuro o cadastro de produto
- **Então** existe um caminho de navegação até ele

## 8. Fora de escopo

- **Carrinho.** O seletor de quantidade e o botão continuam indisponíveis. É a
  `018` da cadeia da loja.
- **Favoritar da página do produto.** Esta entrega cobre o cartão, no catálogo
  e na lista de favoritos. A tela de detalhe é entrega própria.
- **Ordenar ou filtrar a lista de favoritos.** A lista mostra o que foi
  guardado. Filtro e ordenação só se ganharem motivo.
- **Notificar mudança de preço de favorito.** Ideia adjacente, sem pedido.
- **Contador de favoritos no cabeçalho.** O botão "Favoritos" leva à lista;
  quantos são não aparece nele.
- **Unificar o cartão do carrossel com o do catálogo.** Decidido manter os dois
  desenhos — ver seção 10.
- **Listagem, edição e exclusão de produto pelo administrador.** Aqui apenas o
  cadastro existente ganha um caminho de navegação; as demais telas seguem no
  backlog.
- **Página de conta do cliente.** Continua desabilitada no cabeçalho, como a
  `014` deixou.

## 9. Dependências

- **Depende de:** `003`, que criou a entidade e a tabela de favorito; `012`,
  que entregou o catálogo e o cartão; `014`, que corrigiu a geometria do cartão
  e estabeleceu o padrão de atualização sem recarga.
- **Bloqueia:** nada. A `018` (carrinho) reaproveitará o padrão de POST com
  atualização no lugar que esta feature estabelece para o coração.

## 10. Decisões e pendências

**O carrossel da página inicial mantém o cartão atual.** Foi oferecido unificar
os dois desenhos e o responsável preferiu manter a mudança restrita ao
catálogo, como pedido originalmente. A RF-17 preserva o que a `014` garantiu, e
o teste que a protege continua valendo.

**O nome do produto passa a caixa normal no catálogo.** A referência visual
mostra assim, e a fidelidade ao desenho foi o critério pedido. No carrossel o
nome segue em caixa alta, sem mudança visível.

**O controle de favoritar aparece por passagem de mouse no desktop e fica
sempre visível no toque.** Mantém a referência limpa em repouso, sem tornar o
recurso inalcançável em celular.

**A intenção de favoritar do visitante é guardada no próprio navegador.**
Escolhido para não acoplar a tela de login ao favorito nem introduzir um
endereço que altera estado ao ser aberto. A consequência assumida: sem
JavaScript não há convite nem intenção guardada, e a pessoa favorita de novo
depois de entrar — o que é coerente, já que o convite em si é um recurso de
script.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** —
pendência herdada da `013` §10 e repetida na `014` §10, ainda sem critério
definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — RF-14 e
      CA-15 cobrem endereço de retorno hostil; RF-12 e CA-13, acesso indevido à
      lista; RF-09 e CA-10, produto que sai do ar depois de favoritado
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — a pendência da seção
      10 é decisão de negócio herdada e registrada, não indefinição desta
      feature
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
