# Especificação — Modelo de dados completo

**ID:** `003-modelo-de-dados-completo` · **Branch:** `003-modelo-de-dados-completo`
**Criada em:** 2026-08-12 · **Status:** Rascunho

---

> **Nota sobre o formato.** Como na `002`, esta feature entrega quase nada de
> visível ao usuário — ela materializa o modelo de dados que o resto do backlog
> precisa. As seções 5 e 8 seguem o template normalmente. A seção 6 registra
> requisitos de **qualidade interna**, e a seção 7 é a mais importante desta
> spec: as invariantes de cada entidade, que viram construtor validante no
> domínio.

---

## 1. Contexto e problema

O arquivo [`ModelagemBancoTCC.dbml`](../../ModelagemBancoTCC.dbml) descreve 13
tabelas. Existem como entidade e persistência **duas**: `Produto` e `Usuario`. As
outras dez estão no papel.

Isso não é só dívida documental — é o que trava o projeto inteiro. O formulário
de cadastro de produto pede a subcategoria como um `Guid` digitado à mão, porque
não existe `Subcategoria` para oferecer numa lista. O campo Promoção é preenchido
com um enum onde se espera o identificador de uma promoção, porque não existe
`Promocao`. Quatro enums — `PromocaoTipo`, `PedidoStatus`, `PagamentoStatus` e
`MetodoPagamento` — foram escritos e nunca usados: estavam esperando as entidades
que esta spec cria. O `ProdutoDTO` carrega `EstaFavorito` e nada o consome.

Enquanto as tabelas não existirem, nem a `001` fecha nem nenhuma das oito
features seguintes do backlog começa.

## 2. Objetivo

Materializar as dez tabelas restantes da modelagem como entidades de domínio
persistidas, de modo que toda feature seguinte encontre o esquema pronto.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma mudança perceptível — a vitrine continua igual |
| Cliente autenticado | Nenhuma mudança perceptível |
| Administrador da loja | Nenhuma mudança perceptível; a `001` é que passa a poder escolher subcategoria e promoção de verdade |

## 4. Histórias de usuário

> **HU-01** — Como **desenvolvedor do TCC**, quero que o banco tenha as tabelas
> que a minha modelagem descreve, para que a próxima feature não precise criar
> esquema antes de criar comportamento.
>
> **HU-02** — Como **desenvolvedor do TCC**, quero que a massa inicial tenha
> categorias e subcategorias de verdade, para que o cadastro de produto possa
> oferecer uma lista em vez de pedir um identificador digitado à mão.
>
> **HU-03** — Como **avaliador da banca**, quero abrir o banco e encontrar o
> esquema que o documento de modelagem promete, para conferir que projeto e
> implementação contam a mesma história.

## 5. Requisitos funcionais

*Esta feature é quase toda interna. O que segue é o pouco que um usuário observa
— na prática, garantias de não-regressão.*

- **RF-01** — A vitrine da página inicial DEVE continuar exibindo os produtos da
  massa inicial, com nome, preço e imagem, exatamente como antes.
- **RF-02** — Todo produto da massa inicial DEVE pertencer a uma subcategoria que
  existe de fato, e essa subcategoria DEVE pertencer a uma categoria que existe.
- **RF-03** — O sistema NÃO DEVE aceitar um produto cuja subcategoria não exista.

## 6. Requisitos de qualidade interna

- **RQ-01** — As dez tabelas ausentes DEVEM existir como entidade de domínio,
  configuração de persistência e tabela no banco: `Categoria`, `Subcategoria`,
  `Estoque`, `Promocao`, `Endereco`, `Favorito`, `Avaliacao`, `Pedido`,
  `ItemPedido` e `Pagamento`.
- **RQ-02** *(Princípio I)* — Nenhuma entidade de domínio DEVE ter propriedade de
  navegação para `Usuario`, que vive na infraestrutura por herdar de
  `IdentityUser<Guid>`. Nesses quatro casos — `Endereco`, `Favorito`,
  `Avaliacao` e `Pedido` — a referência é o identificador, e o relacionamento é
  declarado apenas na configuração de persistência.
- **RQ-10** — Entre entidades que vivem no domínio, o relacionamento DEVE ser
  expresso por propriedade de navegação, não por identificador solto. A restrição
  da RQ-02 é imposta pela arquitetura e vale **somente** para referências a
  usuário; estendê-la ao resto do modelo empobreceria as consultas sem nenhum
  ganho.
- **RQ-11** — A navegação DEVE ser declarada apenas do lado "muitos" para o lado
  "um" (do filho para o pai). NÃO DEVE haver coleção em entidade de domínio nesta
  entrega: gerenciar coleção é decisão de agregado, e agregado é assunto da spec
  que definir a regra — a de pedido, em particular.
- **RQ-03** *(Princípio II)* — Toda entidade nova DEVE ter propriedades com
  `private set`, construtor que valida antes de atribuir, e construtor protegido
  sem parâmetros para o mapeador materializar.
- **RQ-04** — As entidades sem consumidor nesta entrega — `Endereco`, `Favorito`,
  `Avaliacao`, `Pedido`, `ItemPedido`, `Pagamento` — DEVEM receber apenas as
  invariantes verificáveis a partir da modelagem. NÃO DEVEM receber método de
  transição de estado inventado antes da spec da feature correspondente.
- **RQ-05** *(Princípio IV)* — Os nomes `Produto_Pedido_FK`,
  `Promocao_Produto_FK` e `Favoritos` NÃO DEVEM chegar ao código: o primeiro
  descreve mal o que a tabela guarda, o segundo deixa de existir (ver RN-11), e o
  terceiro está no plural onde todas as demais entidades estão no singular.
- **RQ-06** — O arquivo de modelagem DEVE ser atualizado junto com o código, de
  modo que os dois descrevam o mesmo esquema ao final desta feature.
- **RQ-07** *(Princípio VI)* — A mudança de esquema DEVE vir em uma única
  migration versionada, com nome descritivo em inglês.
- **RQ-08** — As configurações de persistência NÃO DEVEM conter sintaxe presa a
  um provider, seguindo o que a `002` estabeleceu.
- **RQ-09** *(Princípio V)* — Toda invariante da seção 7 DEVE ter teste unitário,
  e todo relacionamento novo DEVE ter teste de integração provando que a chave
  estrangeira existe e recusa referência órfã.

## 7. Regras de negócio

*As invariantes abaixo viram validação no construtor de cada entidade.*

### Catálogo

- **RN-01** — Categoria tem nome obrigatório, entre 3 e 100 caracteres.
- **RN-02** — Subcategoria tem nome obrigatório, entre 3 e 100 caracteres, e
  pertence obrigatoriamente a uma categoria.
- **RN-03** — Produto pertence obrigatoriamente a uma subcategoria que existe.

### Estoque

- **RN-04** — Estoque pertence a exatamente um produto, e um produto tem no
  máximo um registro de estoque.
- **RN-05** — Quantidade em estoque nunca é negativa. Retirar mais do que existe
  é recusado.

### Promoção

- **RN-06** — Promoção tem nome obrigatório, no máximo 255 caracteres.
- **RN-07** — Promoção tem data de início e data de fim, e a de fim é
  posterior à de início.
- **RN-08** — Promoção do tipo *Percentual* tem valor entre 1 e 100.
- **RN-09** — Promoção dos demais tipos tem valor maior que zero.
- **RN-10** — Promoção está vigente quando está ativa e a data corrente está
  entre início e fim, inclusive.
- **RN-11** — Um produto está em no máximo **uma** promoção por vez.

### Endereço

- **RN-12** — Endereço pertence obrigatoriamente a um usuário.
- **RN-13** — CEP tem exatamente 8 dígitos, desconsiderada a pontuação.
- **RN-14** — Estado, cidade, bairro e rua são obrigatórios; número é maior que
  zero; complemento é opcional.

### Favorito

- **RN-15** — Favorito liga exatamente um produto a exatamente um usuário, e o
  mesmo par não se repete.

### Avaliação

- **RN-16** — Avaliação pertence a um usuário e a um produto, ambos obrigatórios.
- **RN-17** — Nota é um inteiro de 1 a 5.
- **RN-18** — Comentário é opcional e tem no máximo 255 caracteres.

### Pedido e pagamento

- **RN-19** — Pedido pertence a um usuário e a um endereço de entrega, ambos
  obrigatórios.
- **RN-20** — Pedido nasce com status *Pendente*, pagamento não aprovado, e data
  igual ao momento da criação.
- **RN-21** — Valor do pedido nunca é negativo.
- **RN-22** — Item de pedido tem quantidade maior que zero e preço unitário maior
  que zero, e registra o preço praticado no momento da compra.
- **RN-23** — Pagamento pertence a exatamente um pedido, e um pedido tem no
  máximo um pagamento.
- **RN-24** — Pagamento nasce com status *Pendente* e sem data de pagamento.
- **RN-25** — Valor do pagamento é maior que zero.

## 8. Critérios de aceite

### CA-01 — Vitrine não regride
- **Dado** que a aplicação subiu com o banco recriado
- **Quando** abro a página inicial
- **Então** vejo os mesmos seis produtos da massa inicial, com nome, preço em
  formato brasileiro e imagem

### CA-02 — Produtos ligados a subcategorias reais
- **Dado** que a aplicação subiu com o banco recriado
- **Quando** consulto a subcategoria de qualquer produto da massa inicial
- **Então** ela existe, tem nome, e pertence a uma categoria que também existe

### CA-03 — Produto órfão é recusado
- **Dado** um produto cuja subcategoria não existe no banco
- **Quando** tento gravá-lo
- **Então** a gravação é recusada

### CA-04 — Estoque não fica negativo
- **Dado** um estoque com 3 unidades
- **Quando** tento retirar 5
- **Então** a operação é recusada e a quantidade continua 3

### CA-05 — Promoção com datas invertidas é recusada
- **Dado** que informo data de início posterior à data de fim
- **Quando** crio a promoção
- **Então** a criação é recusada

### CA-06 — Promoção percentual fora da faixa é recusada
- **Dado** uma promoção do tipo *Percentual*
- **Quando** informo valor 0 ou valor 101
- **Então** a criação é recusada

### CA-07 — Vigência da promoção
- **Dado** uma promoção ativa que vai de ontem até amanhã
- **Quando** pergunto se está vigente hoje
- **Então** a resposta é sim; e se ela for desativada, a resposta passa a não

### CA-08 — Nota de avaliação fora da faixa é recusada
- **Dado** uma avaliação
- **Quando** informo nota 0 ou nota 6
- **Então** a criação é recusada

### CA-09 — Pedido nasce pendente
- **Dado** que crio um pedido com usuário e endereço válidos
- **Quando** ele é criado
- **Então** seu status é *Pendente*, o pagamento consta como não aprovado, e a
  data é a do momento da criação

### CA-10 — Item de pedido com quantidade zero é recusado
- **Dado** um item de pedido
- **Quando** informo quantidade 0
- **Então** a criação é recusada

### CA-11 — Esquema e modelagem contam a mesma história
- **Dado** o final desta feature
- **Quando** comparo o `.dbml` com as tabelas geradas pela migration
- **Então** as tabelas, colunas e relacionamentos correspondem, e nenhum nome
  `*_FK` sobrou

### CA-12 — Suíte verde
- **Dado** o final desta feature
- **Quando** executo a suíte de testes
- **Então** todos passam e o total é maior que os 152 de hoje

## 9. Fora de escopo

- **Qualquer tela.** Esta feature não cria nem altera view, controller ou
  serviço de aplicação. É esquema e domínio.
- **As oito features que consomem essas tabelas** — navegação por categoria,
  controle de estoque, promoções, favoritos, endereço, carrinho e pedido,
  pagamento, avaliação. Cada uma vira spec própria, e é lá que entram as regras
  de transição de estado que a RQ-04 proíbe adivinhar aqui.
- **Transições de estado de `Pedido` e `Pagamento`.** Não existe
  `Pedido.Cancelar()` nem `Pagamento.Aprovar()` nesta entrega: as regras de quem
  pode transitar para o quê, e a partir de qual estado, pertencem à spec de
  pedido e à de pagamento.
- **A redundância entre `ProdutoStatus.ForaDeEstoque` e `Estoque.Quantidade == 0`.**
  Os dois passam a existir e podem discordar. Quem concilia é a spec de controle
  de estoque; registrar a tensão aqui basta.
- **Separar a pessoa da credencial.** Hoje `Usuario` mistura dado de negócio
  (nome, CPF, celular, nascimento) com credencial do Identity, e é por isso que
  a RQ-02 existe. Separar os dois eliminaria a exceção constitucional e deixaria
  as quatro entidades navegarem livremente — mas exige migration movendo colunas
  e mexe em toda a camada de autenticação. Vira spec `004`, executada logo após
  esta.
- **Papéis e autorização** — spec `005`.
- **Cadastro de produto pela interface** — spec `001`.
- **Migração de dados existentes.** O banco de desenvolvimento é descartável; a
  massa inicial é recriada.

## 10. Dependências

- **Depende de:** `002-revisao-tecnica` (implementada) — usa a lição de
  configuração neutra de provider e a unidade de trabalho já simplificada.
- **Bloqueia:** `004` (separar pessoa de credencial), `005` (papéis), `001`
  (cadastro de produto) e, por consequência, todo o backlog restante e os
  testes E2E.

**Ordem:** `003` → `004` → `005` → `001` → `006` (E2E em Playwright).

## 11. Pendências

Nenhuma. As duas ambiguidades da modelagem foram resolvidas antes desta spec:

- **Cardinalidade de promoção** — resolvida: um produto está em no máximo uma
  promoção (RN-11). A tabela `Promocao_Produto_FK` do `.dbml` deixa de existir e
  `Produto.PromocaoId`, que já está no código, é a representação definitiva.
- **Tipo do valor de promoção** — resolvida: o `.dbml` declara `smallint`, que
  não representa R$ 4,50. Passa a `decimal(18,2)`, como todo valor monetário do
  sistema.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]`
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
