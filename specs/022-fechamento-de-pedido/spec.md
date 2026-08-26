# Especificação — Fechamento de pedido

**ID:** `022-fechamento-de-pedido` · **Branch:** `022-fechamento-de-pedido`
**Criada em:** 2026-08-25 · **Status:** Implementada

---

## 1. Contexto e problema

**Ninguém consegue comprar.** O carrinho existe, o custo de entrega é calculado,
os endereços estão cadastrados — e o botão de finalizar compra continua
desabilitado desde o dia em que foi criado. Todo o caminho até a compra está
pronto, e o último passo não existe.

**Três tabelas modeladas no início do projeto nunca receberam uma linha.** Pedido,
item de pedido e pagamento foram desenhados junto com o resto do sistema e
seguem vazios. Não é dívida esquecida: é a peça que faltava para o sistema
deixar de ser um catálogo e virar uma loja.

**A vitrine anuncia um critério que não era o pretendido.** A loja queria exibir
os mais vendidos na página inicial. Como não havia venda registrada, a entrega
de correções trocou o título para o critério que tinha dado real — avaliação —
e deixou registrado que a troca aconteceria quando houvesse venda. É aqui.

## 2. Objetivo

Permitir concluir a compra: escolher para onde vai, como chega e como se paga,
e registrar o pedido resultante.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Encontra no fechamento o caminho para entrar ou criar conta, e conclui a compra depois disso |
| Cliente autenticado | Escolhe endereço, entrega e forma de pagamento, e recebe a confirmação do pedido |
| Administrador da loja | Passa a ter pedidos registrados, com o que despachar e para onde |
| Quem visita a página inicial | Vê os produtos mais vendidos, e não mais os mais bem avaliados |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero concluir a compra do que separei, dizendo
> para onde vai e como quero pagar.
>
> **HU-02** — Como **cliente**, quero revisar tudo antes de confirmar, e ter
> certeza de que vou pagar o que revisei.
>
> **HU-03** — Como **cliente**, quero um comprovante com um número que eu
> consiga citar se precisar falar com a loja.
>
> **HU-04** — Como **cliente que ainda não tem conta**, quero criar uma no meio
> da compra, sem perder o que já separei.
>
> **HU-05** — Como **dona da loja**, quero saber o que foi comprado, para onde
> despachar e por qual transportadora.
>
> **HU-06** — Como **cliente**, quero ver na página inicial o que mais sai da
> loja.

## 5. Requisitos funcionais

### Os passos

- **RF-01** — A tela do carrinho DEVE apresentar os passos do fechamento e
  indicar qual está ativo.
- **RF-02** — Quem não está autenticado DEVE encontrar um passo para entrar ou
  criar conta.
- **RF-03** — Quem já está autenticado NÃO DEVE ver esse passo.
- **RF-04** — Depois de entrar, a pessoa DEVE voltar ao passo do carrinho.
- **RF-05** — Trocar de passo NÃO DEVE recarregar a página, e DEVE continuar
  funcionando com JavaScript desligado.

### Endereço e entrega

- **RF-06** — O passo de endereço DEVE listar os endereços da pessoa e permitir
  escolher um, já vindo marcado o principal.
- **RF-07** — Quem não tem endereço cadastrado DEVE poder cadastrar um no
  próprio passo, sem sair do fechamento.
- **RF-08** — Escolhido o endereço, as opções de entrega para ele DEVEM ser
  apresentadas, cada uma com transportadora, serviço, preço e prazo.
- **RF-09** — A pessoa DEVE escolher uma opção de entrega, e o resumo DEVE
  refletir a escolha.

### Pagamento

- **RF-10** — O passo de pagamento DEVE oferecer as formas de pagamento
  aceitas.
- **RF-11** — Nenhum dado de pagamento DEVE ser coletado.
- **RF-12** — A pessoa DEVE ser informada de que o pagamento ainda será
  combinado com a loja.

### Confirmar

- **RF-13** — Antes de confirmar, o resumo DEVE mostrar o valor dos produtos, o
  da entrega e o total.
- **RF-14** — Ao confirmar, o sistema DEVE conferir preço dos itens,
  disponibilidade e custo de entrega.
- **RF-15** — Divergindo qualquer um deles do que foi revisado, o pedido NÃO
  DEVE ser fechado, e a tela DEVE reexibir os valores atuais, sinalizando o que
  mudou.
- **RF-16** — Item indisponível NÃO DEVE entrar em pedido, e sua presença DEVE
  impedir o fechamento.
- **RF-17** — Não sendo possível obter o custo de entrega no momento de
  confirmar, o pedido NÃO DEVE ser fechado, e a pessoa DEVE ser informada.
- **RF-18** — Confirmado, o sistema DEVE registrar o pedido com seus itens, o
  endereço de entrega, a transportadora, o serviço, o prazo, o custo de entrega
  e a forma de pagamento escolhida.
- **RF-19** — O pedido DEVE registrar o preço de cada item como estava no
  momento do fechamento.
- **RF-20** — O pedido, seus itens e o pagamento DEVEM ser registrados juntos:
  ou tudo é gravado, ou nada é.
- **RF-21** — Fechado o pedido, o carrinho DEVE ficar vazio.
- **RF-22** — A pessoa DEVE receber uma confirmação com o número do pedido, os
  itens, os valores, o prazo e o que acontece a seguir.
- **RF-23** — O número do pedido DEVE ser curto o bastante para ser ditado.

### A vitrine

- **RF-24** — A vitrine da página inicial DEVE exibir os produtos mais vendidos.
- **RF-25** — O título da seção DEVE anunciar esse critério.
- **RF-26** — O catálogo DEVE passar a oferecer a ordenação por mais vendidos,
  que hoje é anunciada e recusada.
- **RF-27** — A loja DEVE ter pedidos de demonstração ao ser instalada, para que
  a ordenação por venda tenha o que ordenar.

## 6. Regras de negócio

- **RN-01** — Pedido é registro do que foi combinado, não uma consulta ao
  presente. Preço, custo de entrega, transportadora e prazo ficam gravados como
  estavam no fechamento — mudar o preço de um produto depois não muda o que
  alguém já comprou.
- **RN-02** — A pessoa paga o que revisou. Qualquer valor que mude entre a
  revisão e a confirmação interrompe o fechamento e volta para revisão, em vez
  de ser cobrado em silêncio.
- **RN-03** — Nenhum dado de pagamento é coletado enquanto não houver
  processadora. Guardar número de cartão sem ter como cobrar é assumir risco de
  segurança sem entregar nada em troca.
- **RN-04** — Um título de seção entrega o que anuncia. Regra herdada. É por ela
  que a loja passa a ser instalada com pedidos de demonstração: sem venda
  nenhuma, "mais vendidos" ordenaria cem produtos empatados em zero e exibiria
  ordem alfabética sob um título falso.
- **RN-05** — Pedido cancelado não conta como venda.
- **RN-06** — Item indisponível não entra em pedido. Regra herdada do carrinho,
  que já o exclui de toda soma; aqui ela impede o fechamento.
- **RN-07** — O pedido nasce inteiro ou não nasce. Um pedido sem itens, ou itens
  sem pedido, ou pedido sem forma de pagamento registrada são estados que o
  sistema não deve ser capaz de produzir.
- **RN-08** — Endereço de outra pessoa é inalcançável. Regra herdada da entrega
  de conta e endereços, que segue valendo ao escolher para onde entregar.

## 7. Critérios de aceite

### CA-01 — Os passos aparecem e indicam onde estou
- **Dado** que tenho itens no carrinho
- **Quando** abro a tela
- **Então** vejo os passos do fechamento, com o do carrinho ativo

### CA-02 — Quem não entrou encontra onde entrar
- **Dado** que não estou autenticado
- **Quando** avanço do carrinho
- **Então** encontro o passo de entrar ou criar conta

### CA-03 — Quem já entrou não vê esse passo
- **Dado** que estou autenticado
- **Quando** olho os passos
- **Então** o passo de entrar não está lá

### CA-04 — Entrar devolve ao carrinho
- **Dado** que entrei no meio do fechamento
- **Quando** a tela volta
- **Então** estou no passo do carrinho, vendo o que ficou nele

### CA-05 — O endereço principal já vem marcado
- **Dado** que tenho endereços cadastrados
- **Quando** chego ao passo de endereço
- **Então** o principal já está escolhido

### CA-06 — Sem endereço, cadastro no próprio passo
- **Dado** que não tenho nenhum endereço
- **Quando** chego ao passo de endereço
- **Então** posso cadastrar um ali mesmo, e ele fica escolhido

### CA-07 — As opções de entrega aparecem para o endereço escolhido
- **Dado** que escolhi um endereço
- **Quando** o passo carrega
- **Então** vejo as opções de entrega com transportadora, serviço, preço e prazo

### CA-08 — Trocar o endereço troca as opções
- **Dado** que vi opções para um endereço
- **Quando** escolho outro endereço
- **Então** as opções e o resumo acompanham

### CA-09 — Nenhum dado de pagamento é pedido
- **Dado** que estou no passo de pagamento
- **Quando** escolho uma forma
- **Então** nada além da escolha me é pedido, e sou informado de que o pagamento
  será combinado com a loja

### CA-10 — O resumo mostra a composição antes de confirmar
- **Dado** que escolhi endereço, entrega e forma de pagamento
- **Quando** olho o resumo
- **Então** vejo o valor dos produtos, o da entrega e o total

### CA-11 — O pedido é registrado
- **Dado** que confirmei a compra
- **Quando** o pedido é fechado
- **Então** ele fica registrado com os itens, o endereço, a transportadora, o
  serviço, o prazo, o custo de entrega e a forma de pagamento

### CA-12 — O preço do item fica congelado
- **Dado** que comprei um produto por um preço
- **Quando** o preço do produto muda depois
- **Então** meu pedido continua registrando o preço que paguei

### CA-13 — O carrinho fica vazio
- **Dado** que fechei um pedido
- **Quando** volto ao carrinho
- **Então** ele está vazio

### CA-14 — Recarregar não fecha duas vezes
- **Dado** que acabei de fechar um pedido
- **Quando** recarrego a tela de confirmação
- **Então** nenhum segundo pedido é criado

### CA-15 — A confirmação traz o número e o que vem a seguir
- **Dado** que fechei um pedido
- **Quando** vejo a confirmação
- **Então** encontro um número curto, os itens, os valores, o prazo e a
  informação de que o pagamento será combinado

### CA-16 — Preço divergente interrompe o fechamento
- **Dado** que o preço de um item mudou depois de eu revisar
- **Quando** confirmo
- **Então** o pedido não é fechado, e a tela mostra o valor atual sinalizando a
  mudança

### CA-17 — Entrega divergente interrompe o fechamento
- **Dado** que o custo de entrega mudou depois de eu revisar
- **Quando** confirmo
- **Então** o pedido não é fechado, e a tela mostra o valor atual sinalizando a
  mudança

### CA-18 — Item indisponível impede fechar
- **Dado** que um item do meu carrinho ficou indisponível
- **Quando** confirmo
- **Então** o pedido não é fechado, e sou informado de qual item

### CA-19 — Entrega incalculável impede fechar
- **Dado** que o serviço de entrega está fora do ar
- **Quando** confirmo
- **Então** o pedido não é fechado, e sou informado de que não foi possível
  confirmar a entrega agora

### CA-20 — A vitrine exibe os mais vendidos
- **Dado** que existem pedidos registrados
- **Quando** abro a página inicial
- **Então** os produtos mais vendidos aparecem primeiro, e o título diz isso

### CA-21 — O catálogo oferece a ordenação por mais vendidos
- **Dado** que estou no catálogo
- **Quando** escolho ordenar por mais vendidos
- **Então** a listagem é ordenada por isso, e não por outro critério

### CA-22 — Pedido cancelado não conta como venda
- **Dado** que existe um pedido cancelado
- **Quando** a vitrine ordena por venda
- **Então** os itens daquele pedido não são contados

### CA-23 — O fechamento funciona sem JavaScript
- **Dado** que estou com o JavaScript desligado
- **Quando** percorro os passos e confirmo a compra
- **Então** o pedido é fechado normalmente

## 8. Fora de escopo

- **Cobrar de verdade.** Não há processadora de pagamento. O pedido registra a
  forma escolhida e nasce com pagamento pendente; a cobrança é combinada fora do
  sistema. A integração com processadora é spec própria, logo depois desta.
- **Avançar a situação do pedido.** Nada nesta entrega move um pedido de
  pendente para confirmado, enviado ou entregue — isso depende do pagamento, e
  o pagamento depende do gateway.
- **Baixa de estoque.** A entidade de estoque existe e segue sem comportamento;
  disponibilidade continua sendo o status do produto, marcado à mão. É entrega
  própria, adiante na cadeia.
- **Histórico de pedidos.** Ver os pedidos passados na área de conta é a entrega
  seguinte. A confirmação desta entrega não linka para lá, porque ainda não
  existe.
- **Cupom de desconto.** O campo segue desabilitado, como o redesenho do
  carrinho o deixou.
- **Cancelar ou alterar um pedido já fechado.** Nenhuma das duas coisas existe
  nesta entrega.
- **Comprar etiqueta, imprimir etiqueta e rastrear entrega.** A cotação informa
  preço e prazo; nada nesta entrega gera obrigação com a transportadora.
- **Notificar por e-mail.** A confirmação aparece na tela; nenhum e-mail é
  enviado.
- **Mudar as regras do carrinho.** Quantidade, item indisponível e fusão ao
  entrar seguem exatamente como estão.

## 9. Dependências

- **Depende de:** a entrega de conta e endereços, que criou os endereços a
  escolher; a de cotação de frete, que calcula o custo da entrega; e a de
  redesenho do carrinho, que criou a tela onde os passos moram.
- **Bloqueia:** o histórico de pedidos, que precisa de pedidos para listar.

## 10. Decisões e pendências

**As quatro formas de pagamento são oferecidas, e nenhum dado é coletado.**
Decisão do responsável ao especificar. O pedido grava a forma escolhida e nasce
pendente, e a confirmação diz com todas as letras que o pagamento será
combinado. Coletar dados de cartão sem processadora foi descartado de saída: é
exposição de segurança que não entrega nada em troca (RN-03).

**Preço, disponibilidade e entrega são conferidos ao confirmar, com a mesma
regra.** O que a tela exibiu volta como alegação a ser conferida, nunca como
autoridade — adulterá-lo não dá vantagem, e divergência legítima é detectada em
vez de passar despercebida. Uma regra só para produtos e entrega, em vez de duas
parecidas (RN-02).

**A loja passa a ser instalada com pedidos de demonstração.** Sem isso, a
ordenação por venda empataria os cem produtos em zero e a vitrine exibiria ordem
alfabética sob o título "mais vendidos", ferindo a RN-04 — exatamente o defeito
que a entrega de correções recusou cometer e adiou para esta. Ter a capacidade
de registrar venda não é o mesmo que ter venda.

**Atualização ao implementar: a credencial do MelhorEnvio (spec `020` §10)
ainda não foi obtida, e isso bloqueia mais desta entrega do que o previsto.**
Sem ela, toda recotação de frete falha — inclusive a que os passos de
Endereço e Pagamento fazem para calcular as opções de entrega. Na prática,
é estruturalmente impossível, no ambiente de teste padrão, alcançar o passo
de Pagamento ou confirmar um pedido de verdade: o link para continuar só
aparece quando a cotação tem sucesso. Tudo o que não depende disso foi
implementado e está com as duas suítes verdes (os passos, a navegação
entre eles, o cadastro de endereço, a vitrine e a ordenação por venda). O
caminho de falha de entrega (RF-17) é, por sinal, o único caminho real que
este ambiente consegue exercitar de ponta a ponta — e está provado. A
jornada completa até o comprovante (a maior parte dos critérios de aceite
desta spec) fica pendente da mesma credencial, sem tarefa própria aqui —
o lugar natural é estender a Fase 8 da `020` para cobri-la também.

**⚠️ O passo de pagamento é deliberadamente provisório.** A loja pretende
integrar uma processadora de pagamento (MercadoPago), e isso é **spec própria**,
depois desta — envolve cobrança real, notificação por webhook, reconciliação de
estado e uma exceção constitucional a declarar (o endereço que recebe a
notificação é um `POST` sem antiforgery e sem autenticação, porque quem chama é
o provedor). Quem implementar esta entrega deve saber que **esta tela tem prazo
de validade** e não investir nela além do necessário. O nível de integração —
redirecionar para a tela do provedor ou manter o formulário aqui — decide quanto
desta tela sobrevive, e é decisão da spec do gateway, com a documentação em
mãos.

**A situação do pedido só avança quando o pagamento for efetuado**, e nada nesta
entrega efetua pagamento. Consequência: todo pedido criado pela aplicação nasce
e permanece pendente até o gateway existir. Os pedidos **semeados** são exceção
deliberada: nascem com situações variadas, por representarem compras passadas —
ficção, como todo o resto da massa de demonstração. É o que dá o que mostrar às
telas que dependem de situação, e o pedido cancelado já era necessário para a
RN-05.

**⚠️ Conta como venda todo pedido não cancelado, independente do pagamento.**
Decisão tomada ao especificar, e ela precisa ser revisitada quando houver
cobrança real: como nenhum pagamento é processado nesta fase, contar apenas
pedidos pagos daria zero para sempre e recriaria o problema que a semeadura de
pedidos resolve. Quando a cobrança existir, o critério provavelmente passa a ser
pedido pago.

**O número do pedido é derivado do identificador**, não sequencial. O banco não
auto-incrementa coluna que não seja chave primária, e a chave é um identificador
longo — uma coluna sequencial exigiria calcular o próximo número na aplicação,
com disputa entre pedidos simultâneos. O derivado é curto, ditável, não revela
quantos pedidos a loja já teve, e não tem nada a sincronizar.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** — pendência
herdada, repetida em todas as entregas desde a de correções da página inicial,
ainda sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só na seção 10
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-16 a
      CA-19 cobrem divergência de preço, de entrega, item indisponível e serviço
      fora do ar; CA-14, o duplo fechamento; CA-23, ausência de JavaScript
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as duas pendências da
      seção 10 são decisão registrada a revisitar e pendência herdada
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
