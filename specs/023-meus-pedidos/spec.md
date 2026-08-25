# Especificação — Meus pedidos

**ID:** `023-meus-pedidos` · **Branch:** `023-meus-pedidos`
**Criada em:** 2026-08-25 · **Status:** Rascunho

---

## 1. Contexto e problema

**Um pedido fechado some de vista.** A pessoa confirma a compra, vê o
comprovante e, ao sair daquela tela, não tem mais como voltar a ele. O número do
pedido só existe enquanto a aba estiver aberta.

**O atalho já está na tela, desabilitado.** O menu da área de conta nasceu com o
lugar reservado para os pedidos, cinza, esperando existir pedido. Existe agora.

**O comprovante não responde tudo.** Ele diz o que acabou de acontecer. Não diz
o que foi comprado semana passada, nem se aquele pedido já saiu para entrega.

## 2. Objetivo

Dar a quem comprou um lugar para reencontrar suas compras: o que foi comprado,
por quanto, para onde vai e em que pé está.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente autenticado | Encontra na área de conta a lista das suas compras e abre qualquer uma para ver o detalhe |
| Cliente (visitante) | Não alcança a tela — é área de conta |
| Quem desenvolve o projeto | O atalho reservado no menu da conta deixa de estar desabilitado |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero rever o que comprei, sem depender de ter
> guardado o comprovante.
>
> **HU-02** — Como **cliente**, quero saber em que pé está cada compra.
>
> **HU-03** — Como **cliente**, quero reencontrar o número do pedido para citar
> quando falar com a loja.
>
> **HU-04** — Como **cliente**, quero ver por quanto comprei cada coisa na
> época, e não o preço de hoje.

## 5. Requisitos funcionais

### A lista

- **RF-01** — A área de conta DEVE oferecer o acesso aos pedidos, e o atalho
  DEVE deixar de estar desabilitado.
- **RF-02** — A lista DEVE apresentar, por pedido, o número, a data, a situação,
  a quantidade de itens e o valor total.
- **RF-03** — A lista DEVE apresentar os pedidos mais recentes primeiro.
- **RF-04** — Cada pedido da lista DEVE levar ao seu detalhe.
- **RF-05** — Quem nunca comprou DEVE encontrar uma tela que explique isso e
  ofereça caminho para o catálogo.

### O detalhe

- **RF-06** — O detalhe DEVE apresentar o número, a data e a situação do pedido.
- **RF-07** — O detalhe DEVE apresentar cada item com nome, quantidade e o preço
  pelo qual foi comprado.
- **RF-08** — O detalhe DEVE apresentar o endereço de entrega, a transportadora,
  o serviço e o prazo, como estavam no fechamento.
- **RF-09** — O detalhe DEVE apresentar o valor dos produtos, o da entrega e o
  total.
- **RF-10** — O detalhe DEVE apresentar a forma de pagamento escolhida e a
  situação do pagamento.

### Acesso

- **RF-11** — Só o dono do pedido DEVE alcançá-lo.
- **RF-12** — Pedido inexistente DEVE responder como não encontrado.

## 6. Regras de negócio

- **RN-01** — Pedido alheio é inalcançável. Regra herdada da entrega de conta e
  endereços, e implementada pelo mesmo desenho: a busca nunca acontece por
  identificador de pedido sozinho, sempre pelo par pedido-e-dono. Assim a regra
  não depende de alguém lembrar de conferir.
- **RN-02** — O pedido mostra o que foi combinado, não o presente. Preço, frete,
  transportadora e prazo são os gravados no fechamento; se o produto mudou de
  preço depois, o pedido não muda.
- **RN-03** — Nada nesta tela altera pedido. Ver é a única coisa que se faz
  aqui.

## 7. Critérios de aceite

### CA-01 — O atalho da conta funciona
- **Dado** que estou autenticado
- **Quando** abro o menu da área de conta
- **Então** o acesso aos pedidos está disponível, não mais desabilitado

### CA-02 — A lista mostra minhas compras
- **Dado** que já fiz pedidos
- **Quando** abro a lista
- **Então** vejo cada um com número, data, situação, quantidade de itens e total

### CA-03 — Os mais recentes vêm primeiro
- **Dado** que fiz pedidos em datas diferentes
- **Quando** abro a lista
- **Então** o mais recente aparece no topo

### CA-04 — Quem nunca comprou entende o que está vendo
- **Dado** que nunca fiz pedido
- **Quando** abro a lista
- **Então** ela me explica isso e me oferece o catálogo

### CA-05 — O detalhe traz o pedido inteiro
- **Dado** que abri um pedido
- **Quando** vejo o detalhe
- **Então** encontro os itens com preço da época, o endereço, a transportadora,
  o prazo, os valores e a forma de pagamento

### CA-06 — O preço do item é o da compra
- **Dado** que comprei um produto e o preço dele mudou depois
- **Quando** abro o detalhe daquele pedido
- **Então** vejo o preço pelo qual comprei

### CA-07 — Pedido de outra pessoa não é alcançável
- **Dado** que estou autenticado
- **Quando** tento abrir o pedido de outra pessoa
- **Então** recebo "não encontrado", e não o conteúdo dele

### CA-08 — Pedido inexistente responde não encontrado
- **Dado** que estou autenticado
- **Quando** peço um pedido que não existe
- **Então** recebo "não encontrado"

### CA-09 — Visitante não alcança a tela
- **Dado** que não estou autenticado
- **Quando** tento abrir a lista de pedidos
- **Então** sou levado a entrar

## 8. Fora de escopo

- **Cancelar ou alterar um pedido.** Ver é tudo que se faz aqui (RN-03).
- **Repetir uma compra.** Recolocar no carrinho o que já foi comprado é outra
  feature.
- **Avançar a situação do pedido.** Depende do pagamento, e o pagamento depende
  da processadora — spec própria.
- **Rastrear a entrega.** Nada nesta entrega consulta a transportadora.
- **Segunda via do comprovante em arquivo.** O detalhe é a tela; não há geração
  de documento.
- **Paginação e filtro.** A lista mostra tudo, do mais recente ao mais antigo.
  Um cliente de doceria não acumula dezenas de pedidos; se isso mudar, é
  alteração própria e pequena.
- **Ver pedidos pela área administrativa.** Esta é a tela do cliente. A da loja é
  outra feature.

## 9. Dependências

- **Depende de:** a entrega de fechamento de pedido, que cria os pedidos a
  listar e semeia os de demonstração; e a de conta e endereços, que criou a área
  onde a tela vive e deixou o atalho reservado.
- **Bloqueia:** nada.

## 10. Decisões e pendências

**A lista resume e o detalhe aprofunda.** Decisão do responsável ao especificar.
Mostrar tudo expandido na mesma tela foi descartado porque a tela cresceria sem
limite com o histórico, e não haveria endereço próprio para apontar quando
alguém cita um número de pedido.

**A proteção de acesso é por desenho, não por checagem.** O mesmo que a entrega
de conta e endereços fez com endereço: a busca recebe o par pedido-e-dono, e não
existe caminho que busque só por identificador de pedido. Uma regra que não pode
ser violada por esquecimento é melhor que uma regra que depende de lembrar.

**⚠️ A situação do pedido é informativa e, por ora, quase sempre "pendente".**
Ela só avança quando o pagamento for efetuado, e nada no sistema efetua
pagamento até a integração com processadora existir. Os pedidos **semeados**
nascem com situações variadas, por representarem compras passadas — é o que dá
o que mostrar a esta tela enquanto o gateway não chega. Pedido criado pela
aplicação permanece pendente.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** — pendência
herdada, repetida em todas as entregas desde a de correções da página inicial,
ainda sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só na seção 10
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-04
      cobre lista vazia; CA-07, pedido alheio; CA-08, pedido inexistente; CA-09,
      visitante
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as duas pendências da
      seção 10 são dependência declarada de outra entrega e pendência herdada
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
