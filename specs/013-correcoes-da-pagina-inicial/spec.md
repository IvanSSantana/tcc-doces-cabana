# Especificação — Correções da página inicial

**ID:** `013-correcoes-da-pagina-inicial` · **Branch:** `013-correcoes-da-pagina-inicial`
**Criada em:** 2026-08-19 · **Status:** Implementada

---

> **Nota sobre o formato.** Esta feature corrige o que a `012` entregou fora do
> desenho, não acrescenta comportamento. As seções seguem o template normal,
> mas a 1 descreve o estado defeituoso medido, não uma necessidade nova.

---

## 1. Contexto e problema

A `012` entregou o menu suspenso do cabeçalho e passou a alimentar a vitrine da
página inicial com o catálogo inteiro. Duas coisas saíram erradas, ambas
visíveis na página inicial.

**O menu suspenso não reproduz a referência visual.** O painel bege que deveria
atravessar a faixa de conteúdo mede 200 pixels onde a faixa mede 1400; a
categoria aberta continua com o fundo verde da barra em vez de virar uma aba
bege presa ao painel; e o cartão coral preenche o painel em vez de ficar
recuado dentro dele. O efeito é um menu apertado, sem a relação de "aba e
painel" que o desenho estabelece.

**A vitrine mostra 99 bolinhas de navegação.** A régua de pontos atravessa a
tela inteira. A causa direta é que a vitrine gera um ponto por produto, e a
página inicial passa para ela todos os 99 produtos disponíveis da loja. Mas o
número de pontos é sintoma: a página inicial carrega 99 cards de produto de uma
vez, sob uma seção intitulada "Mais Vendidos" — que é exatamente a ordenação
que a `012` declarou impossível até a `016` registrar pedidos, e que aparece
desabilitada no seletor do catálogo por esse motivo.

## 2. Objetivo

Fazer o menu suspenso do cabeçalho corresponder à referência visual, e devolver
à vitrine da página inicial um tamanho e um título que o sistema sustenta.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Vê o menu do cabeçalho como foi desenhado, e uma vitrine com um punhado de produtos em vez de uma régua de bolinhas atravessando a tela |
| Cliente autenticado | O mesmo |
| Administrador da loja | Nenhuma mudança |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero que o menu de categorias abra como foi
> desenhado, para que a loja pareça acabada.
>
> **HU-02** — Como **cliente**, quero conseguir contar as posições da vitrine
> com um olhar, para saber quanto ainda falta percorrer.
>
> **HU-03** — Como **dono da loja**, não quero anunciar "mais vendidos" antes
> de a loja saber o que vende mais, para não afirmar ao cliente algo que não
> posso sustentar.

## 5. Requisitos funcionais

### Menu suspenso do cabeçalho

- **RF-01** — O sistema DEVE dar à categoria aberta o mesmo fundo do painel,
  de modo que ela se leia como uma aba presa a ele, em vez de manter o fundo da
  barra de navegação.
- **RF-02** — O sistema DEVE fazer o painel do menu ocupar a largura da faixa de
  conteúdo do cabeçalho, não a largura do item de categoria.
- **RF-03** — O sistema DEVE posicionar o cartão de subcategorias recuado dentro
  do painel, com folga visível acima, abaixo e dos dois lados.
- **RF-04** — O sistema NÃO DEVE passar a depender de JavaScript: o menu
  continua abrindo por passagem de mouse e por foco de teclado.
- **RF-05** — O sistema DEVE manter, em telas estreitas, o comportamento que
  não depende de passagem de mouse.

### Vitrine da página inicial

- **RF-06** — O sistema DEVE limitar a quantidade de produtos exibidos na
  vitrine da página inicial.
- **RF-07** — O sistema NÃO DEVE permitir que a vitrine receba mais produtos do
  que exibe, independente de quem a invoque.
- **RF-08** — O sistema DEVE exibir um ponto de navegação por posição
  alcançável da vitrine, nunca um por produto.
- **RF-09** — O sistema NÃO DEVE intitular a seção como "Mais Vendidos"
  enquanto a loja não registrar vendas.

## 6. Regras de negócio

- **RN-01** — Todo ponto de navegação visível corresponde a uma posição que a
  vitrine alcança. Ponto que não leva a lugar nenhum é defeito, não decoração.
- **RN-02** — Um título de seção só afirma o que o sistema sabe. "Mais
  vendidos" volta a ser um título possível quando a `016` registrar pedidos —
  é a mesma regra que mantém a opção desabilitada no seletor do catálogo
  (RN-07 da `012`).

## 7. Critérios de aceite

### CA-01 — A categoria aberta vira aba
- **Dado** que abro o menu de uma categoria no cabeçalho
- **Quando** comparo o fundo da categoria aberta com o do painel
- **Então** são o mesmo, e diferentes do fundo das categorias fechadas

### CA-02 — O painel atravessa a faixa
- **Dado** que abro o menu de uma categoria
- **Quando** meço a largura do painel
- **Então** ela é a mesma da faixa de conteúdo do cabeçalho, não a do item

### CA-03 — O cartão fica recuado no painel
- **Dado** que abro o menu de uma categoria
- **Quando** comparo as bordas do cartão de subcategorias com as do painel
- **Então** o cartão está inteiramente dentro do painel, com folga visível dos
  quatro lados

### CA-04 — O teclado continua abrindo o menu
- **Dado** que percorro o cabeçalho com a tecla Tab
- **Quando** o foco chega a uma categoria
- **Então** o menu dela abre, sem precisar de mouse

### CA-05 — A vitrine é limitada
- **Dado** que a loja tem dezenas de produtos disponíveis
- **Quando** abro a página inicial
- **Então** a vitrine exibe no máximo o limite definido, não o catálogo inteiro

### CA-06 — Os pontos correspondem a posições reais
- **Dado** que estou na página inicial
- **Quando** conto os pontos de navegação visíveis
- **Então** o número é igual ao de posições que a vitrine alcança, e clicar no
  último leva ao fim da lista

### CA-07 — A seção não promete ranking
- **Dado** que abro a página inicial
- **Quando** leio o título da seção de produtos
- **Então** ele não diz "Mais Vendidos"

### CA-08 — Nada regride em tela pequena
- **Dado** que abro a página inicial numa tela de 375 pixels de largura
- **Quando** rolo até o fim
- **Então** o conteúdo cabe na largura da tela, sem rolagem horizontal

## 8. Fora de escopo

- **Fazer "Mais vendidos" funcionar.** Depende da `016`, que registra pedidos.
  Aqui a seção só deixa de prometer o que não entrega.
- **Curadoria da vitrine.** Quais produtos aparecem na página inicial continua
  sendo o que a consulta devolve, agora limitado. Escolher a dedo é entrega
  própria.
- **Redesenhar a vitrine.** O carrossel continua o que é — setas, pontos,
  quatro cards por vez. Só o tamanho da entrada e a contagem de pontos mudam.
- **Trocar as imagens de produto.** As seis imagens repetidas entre os cem
  produtos do mock são consequência do seed de demonstração, já registrada no
  backlog da `012`.
- **Corrigir o estouro horizontal do cabeçalho a 375px.** Defeito
  pré-existente, registrado desde a `009`, e não é o que esta feature toca.
- **Reordenar as categorias no cabeçalho.** Hoje saem na ordem do banco. **Ver
  seção 10.**

## 9. Dependências

- **Depende de:** `012-catalogo`, que entregou o menu e a vitrine no estado que
  esta corrige.
- **Bloqueia:** nada.

## 10. Decisões e pendências

**Título novo da seção: "Conheça a loja".** Convida sem afirmar ranking, e
serve a uma loja que vende doces, empório, adega e souvenir — títulos como
"Nossos doces" excluiriam três quartos do catálogo. É uma linha de texto,
trivial de trocar.

**Limite da vitrine: oito produtos.** Com quatro cards visíveis no desktop, dá
cinco posições de rolagem — um número que se conta de relance, que era o que a
referência visual mostrava. Em telas estreitas, onde cabe um card por vez, dá
oito pontos.

**⚠️ A ordem das categorias no cabeçalho é a do banco** — hoje sai Doces,
Adega, Souvenir, Empório, sem critério nenhum. A referência visual tem uma
ordem deliberada, mas com nomes de categoria que não existem mais (Doce,
Salgado, Adega, Outros), então não há de onde copiar. Deixei fora de escopo por
não saber qual ordem a loja quer; **se você tiver uma preferência, é uma linha
de código e entra aqui.**

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [ ] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — **não há marcações,
      mas a seção 10 registra uma pendência aberta (ordem das categorias)**
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
