# Especificação — Redesenho do carrinho

**ID:** `021-redesenho-do-carrinho` · **Branch:** `021-redesenho-do-carrinho`
**Criada em:** 2026-08-25 · **Status:** Implementada

---

## 1. Contexto e problema

**A tela do carrinho funciona, mas não é a tela que a loja desenhou.** Ela foi
construída junto com o carrinho, quando ainda não existia protótipo do
fechamento. O desenho definido pela loja mostra outra coisa: duas colunas, os
itens em cartões, e um resumo do pedido fixo ao lado.

**Faltam dois caminhos que o desenho prevê.** Não há como esvaziar o carrinho de
uma vez — só removendo item por item. E não há como voltar ao catálogo de onde
se veio; a pessoa precisa usar o menu ou o botão do navegador.

**O resumo não tem lugar para o frete.** Hoje ele mostra a contagem de itens e o
subtotal. O custo de entrega passa a ser calculado nesta fase do projeto, e não
há onde exibi-lo, nem onde a pessoa entenda que o valor em destaque ainda não
inclui a entrega.

## 2. Objetivo

Dar ao carrinho o desenho definido para o fechamento, com o resumo de pedido
pronto para receber o frete, e as duas ações que faltavam.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Vê o carrinho no desenho novo; pode esvaziá-lo e voltar ao catálogo |
| Cliente autenticado | O mesmo |
| Quem desenvolve o projeto | O resumo passa a ser coluna própria, e é onde o frete e os passos do fechamento vão se encaixar |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero ver de relance o que estou levando e
> quanto vai custar, sem precisar somar nada de cabeça.
>
> **HU-02** — Como **cliente**, quero desistir de tudo de uma vez, sem remover
> item por item.
>
> **HU-03** — Como **cliente**, quero voltar a comprar de onde parei, sem perder
> o que já escolhi.
>
> **HU-04** — Como **cliente**, não quero ler "total a pagar" num valor que
> ainda não inclui a entrega.

## 5. Requisitos funcionais

### Os itens

- **RF-01** — Cada item DEVE ser apresentado como um cartão, com imagem, nome,
  preço unitário, quantidade e o subtotal daquela linha.
- **RF-02** — O cartão DEVE manter o controle de quantidade e a remoção do item,
  que já existem.

### O resumo do pedido

- **RF-03** — O resumo DEVE ficar em coluna própria, ao lado dos itens.
- **RF-04** — O resumo DEVE informar quantos produtos estão no carrinho e quanto
  eles custam somados.
- **RF-05** — O resumo DEVE ter uma linha para o custo de entrega.
- **RF-06** — Enquanto não houver entrega calculada, o valor em destaque DEVE
  ser chamado de subtotal, e a linha de entrega DEVE convidar ao cálculo.
- **RF-07** — Havendo entrega calculada, o valor em destaque DEVE ser chamado de
  total a pagar e DEVE incluir o custo da entrega. Havendo mais de uma opção de
  entrega, a **mais barata** é a que compõe esse valor — é estimativa até o
  fechamento, onde a pessoa escolhe de fato (RN-06).
- **RF-08** — O resumo DEVE apresentar o campo de cupom de desconto
  **desabilitado**, informando que ainda não está disponível.
- **RF-09** — O botão de finalizar compra DEVE continuar visível e desabilitado,
  informando que o fechamento ainda não está disponível.

### As duas ações novas

- **RF-10** — A tela DEVE oferecer esvaziar o carrinho inteiro.
- **RF-11** — Esvaziar o carrinho DEVE pedir confirmação antes de remover.
- **RF-12** — A tela DEVE oferecer voltar ao catálogo, sem perder o carrinho.

### Comportamento geral

- **RF-13** — Tudo DEVE funcionar com JavaScript desligado.
- **RF-14** — Em tela estreita, as duas colunas DEVEM empilhar, sem rolagem
  horizontal.
- **RF-15** — Carrinho vazio DEVE continuar oferecendo caminho para o catálogo.

## 6. Regras de negócio

- **RN-01** — Um controle oferecido ao cliente entrega o que anuncia. Regra
  herdada. É por ela que cupom e finalizar aparecem **desabilitados** em vez de
  parecerem funcionais: um campo que aceita texto e não faz nada mente; um campo
  cinza que explica por que está cinza, não.
- **RN-02** — Um valor em destaque não mente sobre o que representa. Chamar de
  "total a pagar" uma soma que ignora a entrega é afirmar um preço que não é o
  preço. Enquanto a entrega for desconhecida, o valor se chama subtotal.
- **RN-03** — Ação destrutiva e irreversível pede confirmação. Esvaziar o
  carrinho apaga trabalho que a pessoa teve, e fica a um erro de alvo dos
  controles de quantidade.
- **RN-04** — O caminho sem JavaScript é o caminho real, não um consolo. Regra
  herdada.
- **RN-05** — Item indisponível não entra em soma nenhuma. Regra herdada do
  carrinho, que segue valendo no resumo novo.
- **RN-06** — Havendo mais de uma opção de entrega, a mais barata é a que
  compõe o total exibido no carrinho — decidido ao implementar, registrado
  aqui em vez de deixado implícito no código. É estimativa, não escolha:
  quem paga escolhe de fato no fechamento (`022`), e o total pode mudar
  então.

## 7. Critérios de aceite

### CA-01 — Os itens aparecem como cartões
- **Dado** que tenho itens no carrinho
- **Quando** abro a tela
- **Então** cada item aparece com imagem, nome, preço unitário, quantidade e
  subtotal da linha

### CA-02 — Os controles de quantidade e remoção continuam funcionando
- **Dado** que estou na tela do carrinho
- **Quando** altero a quantidade de um item ou o removo
- **Então** o item e o resumo acompanham a mudança

### CA-03 — O resumo mostra produtos e valor
- **Dado** que tenho itens no carrinho
- **Quando** olho o resumo
- **Então** vejo quantos produtos são e quanto custam somados

### CA-04 — Sem entrega calculada, o destaque é subtotal
- **Dado** que ainda não calculei a entrega
- **Quando** olho o resumo
- **Então** o valor em destaque se chama subtotal, e a linha de entrega me
  convida a calculá-la

### CA-05 — Com entrega calculada, o destaque é total a pagar
- **Dado** que a entrega foi calculada
- **Quando** olho o resumo
- **Então** o valor em destaque se chama total a pagar e inclui a entrega

### CA-06 — O cupom aparece desabilitado e explicado
- **Dado** que estou na tela do carrinho
- **Quando** vejo o campo de cupom
- **Então** ele está desabilitado e informa que cupom ainda não está disponível

### CA-07 — Finalizar compra segue anunciado e indisponível
- **Dado** que estou na tela do carrinho
- **Quando** vejo o botão de finalizar compra
- **Então** ele está visível, desabilitado, e informa que o fechamento ainda não
  está disponível

### CA-08 — Esvaziar pede confirmação
- **Dado** que tenho itens no carrinho
- **Quando** peço para esvaziá-lo
- **Então** sou perguntado antes, e nada é removido até eu confirmar

### CA-09 — Esvaziar remove tudo
- **Dado** que confirmei esvaziar o carrinho
- **Quando** a tela recarrega
- **Então** o carrinho está vazio e me oferece caminho para o catálogo

### CA-10 — Desistir de esvaziar não remove nada
- **Dado** que pedi para esvaziar e fui perguntado
- **Quando** desisto
- **Então** o carrinho continua exatamente como estava

### CA-11 — Voltar ao catálogo preserva o carrinho
- **Dado** que tenho itens no carrinho
- **Quando** uso o caminho de voltar a comprar e retorno ao carrinho
- **Então** meus itens continuam lá

### CA-12 — A tela funciona sem JavaScript
- **Dado** que estou com o JavaScript desligado
- **Quando** altero quantidade, removo item ou esvazio o carrinho
- **Então** todas as ações funcionam

### CA-13 — Em tela estreita as colunas empilham
- **Dado** que estou numa tela de 375 pixels de largura
- **Quando** abro o carrinho
- **Então** o resumo aparece abaixo dos itens, sem rolagem horizontal

## 8. Fora de escopo

- **Os passos do fechamento e o indicador de etapas.** Conta, Endereço e
  Pagamento são a entrega seguinte. Um indicador com quatro etapas em que três
  não levam a lugar nenhum feriria a RN-01 — ele chega junto com os passos.
- **Fazer o cupom funcionar.** O campo aparece desabilitado; a regra de desconto
  depende de uma decisão de negócio ainda não tomada, registrada no backlog.
- **Calcular a entrega.** Esta entrega prepara o lugar; quem calcula é a entrega
  de cotação de frete.
- **Ligar o botão de finalizar compra.** É a entrega seguinte.
- **Histórico de pedidos.** Entrega própria, depois do fechamento.
- **Mudar as regras do carrinho.** Quantidade mínima e máxima, item
  indisponível, fusão de carrinhos ao entrar — tudo segue exatamente como está.

## 9. Dependências

- **Depende de:** a entrega do carrinho, que criou a tela, as ações de
  quantidade e remoção e a noção de item indisponível.
- **Bloqueia:** o fechamento de pedido, que constrói os passos dentro desta
  tela; e, na prática, a exibição do frete, que ocupa a linha criada aqui.

## 10. Decisões e pendências

**Esta entrega substitui a tela de carrinho existente; não cria uma segunda.**
Decisão do responsável ao especificar. A alternativa — deixar a tela atual
intacta e pôr o desenho novo só no fechamento — foi descartada porque o sistema
passaria a ter duas telas de carrinho com aparências diferentes, e a pessoa
veria o carrinho mudar de cara ao clicar em finalizar.

**O indicador de passos não entra aqui.** Decorre da RN-01, não de preferência:
os passos ainda não existem.

**A linha de entrega e a troca de rótulo nascem nesta entrega, vazias.** O valor
que as preenche vem da entrega de cotação de frete. Foi decidido assim para que
o resumo seja construído uma vez só, com os dois estados desde o início, em vez
de nascer sem entrega e ser refeito depois.

**⚠️ Ordem de execução, diferente da numeração.** Esta entrega reconstrói o
resumo lateral, que é exatamente onde a caixa de CEP da cotação de frete mora.
Executá-la **antes** da parte de cotação daquela entrega evita construir a caixa
duas vezes. Como a cotação está travada por credencial externa que ainda não
chegou, isso não custa cronograma. O `README` das specs já registra precedente
de ordem de execução diferente da numeração.

**⚠️ O protótipo mostra o menu de categorias como "Doce, Salgado, Adega,
Outros".** A taxonomia real da loja é "Doces, Empório, Adega, Souvenir". O
protótipo envelheceu nesse ponto; o cabeçalho não muda nesta entrega.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** — pendência
herdada, repetida em todas as entregas desde a de correções da página inicial,
ainda sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só na seção 10
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-10
      cobre desistir de esvaziar; CA-12, ausência de JavaScript; CA-13, tela
      estreita; CA-06 e CA-07, controles indisponíveis
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as três pendências da
      seção 10 são de ordem de execução ou herdadas
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
