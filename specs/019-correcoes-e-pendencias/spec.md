# Especificação — Correções e pendências

**ID:** `019-correcoes-e-pendencias` · **Branch:** `019-correcoes-e-pendencias`
**Criada em:** 2026-08-24 · **Status:** Rascunho

---

## 1. Contexto e problema

**Um CPF inválido é aceito no cadastro.** A conferência do CPF só olha o último
dígito. O penúltimo — que também é um dígito verificador, e que a pessoa digita
como qualquer outro — nunca é comparado com nada. Na prática, cerca de um em
cada dez CPFs digitados errado passa e vira conta de verdade, com um documento
que não existe.

**A página inicial pede a loja inteira para mostrar oito produtos.** Ela carrega
os cem produtos do catálogo, descarta os indisponíveis, prepara os noventa e
nove restantes para exibição e então mostra oito. Noventa e um são construídos e
jogados fora a cada visita de cada pessoa.

**O coração da vitrine nasce sempre vazio.** Favoritar um produto pela página
inicial funciona, mas ao recarregar a página o coração volta a aparecer vazio —
mesmo estando favoritado. A tela nunca pergunta quais produtos a pessoa
favoritou.

**A vitrine não tem critério de exibição.** Os oito produtos que aparecem são os
oito primeiros que o banco devolver, sem ordem pedida. Não é uma escolha: é o
acaso.

**A documentação de arquitetura envelheceu em pontos que enganam quem lê.** Ela
afirma que endereço não tem tela e que não existe carrinho no modelo — as duas
últimas entregas fizeram exatamente essas coisas. E o texto que explica o
desenho das estrelas de avaliação se contradiz, descrevendo um comportamento
diferente do que a tela faz.

## 2. Objetivo

Corrigir os defeitos conhecidos que as entregas anteriores registraram e não
consertaram, deixando a documentação fiel ao que o sistema faz hoje.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Vê a página inicial mais rápida, com produtos escolhidos por critério; tem o CPF de fato conferido ao criar conta |
| Cliente autenticado | Tudo acima, mais o coração da vitrine refletindo o que ele realmente favoritou |
| Administrador da loja | Tem o CPF conferido ao ser cadastrado |
| Quem desenvolve o projeto | Lê uma documentação que corresponde ao sistema |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero que um CPF digitado errado seja recusado
> na hora, e não vire uma conta com documento inválido.
>
> **HU-02** — Como **cliente**, quero que a página inicial abra rápido, sem o
> sistema preparar dezenas de produtos que ninguém vai ver.
>
> **HU-03** — Como **cliente**, quero que os produtos que favoritei apareçam
> marcados na página inicial, como aparecem no catálogo.
>
> **HU-04** — Como **cliente**, quero que os produtos em destaque na página
> inicial sigam um critério que eu entenda, e que o título diga qual é.
>
> **HU-05** — Como **quem desenvolve o projeto**, quero que a documentação de
> arquitetura descreva o sistema como ele é hoje.

## 5. Requisitos funcionais

### Conferência de CPF

- **RF-01** — O sistema DEVE recusar CPF cujo primeiro dígito verificador não
  corresponda ao número informado.
- **RF-02** — O sistema DEVE continuar recusando CPF cujo segundo dígito
  verificador não corresponda, CPF com quantidade de dígitos diferente de onze,
  e CPF formado por um único dígito repetido.
- **RF-03** — O sistema DEVE continuar aceitando CPF válido, com ou sem
  pontuação.

### Vitrine da página inicial

- **RF-04** — A página inicial DEVE pedir ao armazenamento apenas os produtos
  que vai exibir.
- **RF-05** — A vitrine DEVE exibir os produtos mais bem avaliados primeiro.
- **RF-06** — Produto sem nenhuma avaliação DEVE aparecer depois dos avaliados,
  e não ser excluído da vitrine.
- **RF-07** — A vitrine NÃO DEVE exibir produto fora do catálogo.
- **RF-08** — O título da seção DEVE anunciar o critério de exibição.
- **RF-09** — Para o cliente autenticado, a vitrine DEVE indicar quais produtos
  ele favoritou.
- **RF-10** — Para o visitante, nenhum produto DEVE aparecer marcado como
  favorito.

### Documentação

- **RF-11** — A documentação de arquitetura DEVE descrever as telas e as tabelas
  que existem hoje.
- **RF-12** — Os defeitos corrigidos por esta entrega DEVEM constar como
  resolvidos, e não como pendentes.
- **RF-13** — O texto que explica o preenchimento das estrelas de avaliação DEVE
  descrever o comportamento real da tela.

## 6. Regras de negócio

- **RN-01** — Um CPF só é válido se **os dois** dígitos verificadores
  informados corresponderem aos calculados. Conferir um e ignorar o outro não é
  validação parcial: é aceitar um em cada dez documentos inválidos.
- **RN-02** — Produto fora do catálogo não existe do lado de fora, em nenhuma
  listagem. Regra herdada, e é ela que a vitrine passa a respeitar no
  armazenamento em vez de na tela.
- **RN-03** — A quantidade de produtos exibidos na vitrine é decidida por quem
  exibe, e o pedido ao armazenamento respeita essa quantidade. Pedir mais do que
  se vai mostrar é desperdício, não folga.
- **RN-04** — Um título de seção entrega o que anuncia. Regra herdada das
  entregas anteriores, e é por ela que a vitrine não se chama "mais vendidos"
  enquanto não houver venda registrada no sistema.
- **RN-05** — Documentação que descreve o sistema errado é pior que documentação
  ausente: a ausente faz procurar, a errada faz confiar.

## 7. Critérios de aceite

### CA-01 — CPF com o primeiro dígito verificador errado é recusado
- **Dado** que estou criando uma conta
- **Quando** informo um CPF cujo penúltimo dígito não confere
- **Então** o cadastro é recusado com mensagem de CPF inválido

### CA-02 — CPF com o segundo dígito verificador errado continua recusado
- **Dado** que estou criando uma conta
- **Quando** informo um CPF cujo último dígito não confere
- **Então** o cadastro é recusado com mensagem de CPF inválido

### CA-03 — CPF válido continua aceito
- **Dado** que estou criando uma conta
- **Quando** informo um CPF válido
- **Então** o cadastro é aceito

### CA-04 — Dígitos repetidos continuam recusados
- **Dado** que estou criando uma conta
- **Quando** informo um CPF formado por um único dígito repetido onze vezes
- **Então** o cadastro é recusado

### CA-05 — As contas de demonstração continuam válidas
- **Dado** que o sistema semeia contas de demonstração
- **Quando** a aplicação sobe
- **Então** todas são criadas sem erro de CPF

### CA-06 — A vitrine pede só o que exibe
- **Dado** que a loja tem cem produtos
- **Quando** abro a página inicial
- **Então** o sistema pede oito produtos ao armazenamento, não cem

### CA-07 — A vitrine ordena por avaliação
- **Dado** que os produtos têm avaliações diferentes entre si
- **Quando** abro a página inicial
- **Então** os mais bem avaliados aparecem primeiro

### CA-08 — Produto sem avaliação aparece por último
- **Dado** que existe produto sem nenhuma avaliação
- **Quando** abro a página inicial
- **Então** ele pode aparecer na vitrine, depois dos avaliados

### CA-09 — Produto fora do catálogo não aparece
- **Dado** que existe produto fora do catálogo
- **Quando** abro a página inicial
- **Então** ele não está na vitrine

### CA-10 — O título anuncia o critério
- **Dado** que abri a página inicial
- **Quando** olho o título da seção de produtos
- **Então** ele diz que são os produtos bem avaliados

### CA-11 — O favorito aparece marcado na vitrine
- **Dado** que favoritei um produto que está na vitrine
- **Quando** recarrego a página inicial
- **Então** ele continua marcado como favorito

### CA-12 — O visitante não vê nada marcado
- **Dado** que não estou autenticado
- **Quando** abro a página inicial
- **Então** nenhum produto aparece marcado como favorito

### CA-13 — A documentação corresponde ao sistema
- **Dado** que leio a documentação de arquitetura
- **Quando** procuro pelas telas e tabelas que existem
- **Então** encontro a área de conta e o carrinho descritos, e nenhuma
  afirmação de que não existem

### CA-14 — Os defeitos constam como resolvidos
- **Dado** que leio a seção de achados da documentação
- **Quando** procuro os defeitos que esta entrega corrige
- **Então** estão marcados como resolvidos, com referência a esta entrega

## 8. Fora de escopo

- **Oferecer "mais vendidos" como critério.** Depende de venda registrada, que
  não existe até a entrega de fechamento de pedido. A vitrine passa a "mais
  vendidos" lá, e esta entrega deixa isso registrado para não se perder.
- **Revisão completa da documentação de arquitetura.** São quase mil linhas, e
  reler a base inteira é entrega própria. Aqui só se corrige o que envelheceu e
  o que esta entrega altera.
- **Permitir escolher a ordenação da vitrine.** A página inicial não ganha
  controle de ordenação — o critério é fixo e anunciado no título.
- **Mudar a quantidade de produtos da vitrine.** Continuam oito.
- **Formatar o CPF automaticamente enquanto se digita** em telas que ainda não
  fazem isso. A conferência muda; a digitação não.
- **Corrigir o texto do comentário sobre estrelas em outros lugares.** Só o
  trecho que se contradiz é reescrito.
- **Rever os demais achados registrados na documentação** que não estejam
  listados aqui.

## 9. Dependências

- **Depende de:** as entregas de catálogo, favoritos, carrinho e conta, que
  criaram tanto o comportamento correto a reaproveitar quanto os defeitos a
  corrigir. Nenhuma capacidade nova é necessária.
- **Bloqueia:** nada formalmente. É deliberadamente independente: limpa o
  terreno antes da entrega de fechamento de pedido, sem que esta precise
  esperar.

## 10. Decisões e pendências

**A vitrine ordena por avaliação, não por venda.** Decisão explícita do
responsável, tomada ao especificar esta entrega. A intenção original era "mais
vendidos", mas nenhuma venda é registrada no sistema hoje — as tabelas de pedido
existem e nunca receberam uma linha. Ordenar por venda hoje empataria todos os
produtos em zero, o resultado sairia em ordem alfabética, e a seção anunciaria
um critério que não aplica, ferindo a RN-04. Por isso a vitrine ordena por
avaliação, que tem dado real, e o título diz isso.

**A troca para "mais vendidos" é tarefa da entrega de fechamento de pedido.** É
ela que passa a registrar venda, e é pequena: trocar o critério e o título.
Registrado aqui para não se perder na renumeração.

**Corrigir o CPF não invalida nenhuma conta existente.** Verificado antes de
especificar: as nove contas de demonstração semeadas têm CPF correto nos dois
dígitos, e o gerador de dados dos testes calcula os dois. A conferência só roda
ao criar ou alterar conta, nunca ao ler — então nenhuma conta já gravada é
afetada.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** — pendência
herdada, repetida em todas as entregas desde a de correções da página inicial,
ainda sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só na seção 10, como decisão
      tomada, e mesmo lá em linguagem de negócio
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-01,
      CA-02 e CA-04 cobrem CPF inválido; CA-09, produto fora do catálogo;
      CA-12, visitante sem favorito; CA-08, produto sem avaliação
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — a única pendência da
      seção 10 é decisão de negócio herdada, já fora de escopo
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
