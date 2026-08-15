# Especificação — Página do produto

**ID:** `008-pagina-do-produto` · **Branch:** `008-pagina-do-produto`
**Criada em:** 2026-08-14 · **Status:** Implementada

---

## 1. Contexto e problema

Hoje o catálogo termina no card da vitrine: nome, foto, preço e um botão. Quem
quer saber o que tem dentro do doce, quanto pesa, se leva amendoim ou o que
outras pessoas acharam dele não tem para onde clicar — o card não leva a lugar
nenhum. É a lacuna mais visível da loja: existe produto, mas não existe página
de produto. Esta feature entrega a tela desenhada na referência visual da loja.

## 2. Objetivo

Dar a cada produto uma página própria que apresente sua imagem, descrição,
preço e as avaliações de quem já comprou.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Abre a página pelo card da vitrine, lê a descrição e as avaliações; ao tentar marcar uma avaliação como útil, é convidado a entrar |
| Cliente autenticado | Tudo o que o visitante faz, e mais: marca e desmarca avaliações como úteis |
| Administrador da loja | Passa a informar uma descrição ao cadastrar o produto, que é o texto exibido nesta página |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero abrir a página de um doce a partir da
> vitrine para ver a foto grande, o preço e a descrição completa antes de decidir.
>
> **HU-02** — Como **cliente**, quero ler as avaliações de quem já comprou, com
> a nota média e a distribuição das notas, para saber se o doce agrada mesmo.
>
> **HU-03** — Como **cliente**, quero ordenar as avaliações pelas mais úteis, as
> mais recentes ou pela nota, para achar rápido o comentário que me interessa.
>
> **HU-04** — Como **cliente autenticado**, quero marcar uma avaliação como útil
> para que ela suba para as próximas pessoas que abrirem a página.
>
> **HU-05** — Como **administrador**, quero escrever a descrição do doce no
> cadastro para que ela apareça na página do produto.

## 5. Requisitos funcionais

### Acesso e identificação do produto

- **RF-01** — O sistema DEVE oferecer uma página própria por produto, alcançável
  a partir do card da vitrine.
- **RF-02** — O sistema DEVE exibir, no topo da página, um caminho de navegação
  com o atalho para a página inicial e o nome da subcategoria do produto.
- **RF-03** — O sistema DEVE responder "não encontrado" quando o identificador
  não corresponder a nenhum produto.
- **RF-04** — O sistema NÃO DEVE exibir ao cliente a página de um produto
  inativo: ela responde "não encontrado" como se o produto não existisse.

### Apresentação do produto

- **RF-05** — O sistema DEVE exibir a imagem, o nome e o preço do produto.
- **RF-06** — O sistema DEVE exibir, ao lado da imagem, um resumo curto da
  descrição e um atalho "Ver mais detalhes" que leva à descrição completa na
  mesma página.
- **RF-07** — O sistema DEVE exibir a descrição completa em uma seção própria,
  intitulada "Características do " seguido do nome do produto.
- **RF-08** — O sistema DEVE omitir o resumo, o atalho e a seção de descrição
  quando o produto não tiver descrição cadastrada — sem deixar espaço vazio nem
  título órfão na tela.
- **RF-09** — O sistema DEVE oferecer um seletor de quantidade com os botões de
  mais e menos e o botão "ADICIONAR AO CARRINHO".
- **RF-10** — O sistema DEVE avisar na página que o produto está fora de estoque
  e impedir a escolha de quantidade quando esse for o status do produto.
- **RF-11** — O administrador DEVE poder informar a descrição do produto no
  formulário de cadastro, e o campo é opcional.

### Avaliações

- **RF-12** — O sistema DEVE exibir a nota média do produto com uma casa decimal,
  a quantidade total de avaliações e a distribuição das notas de 5 a 1 estrelas,
  cada faixa com uma barra proporcional ao total.
- **RF-13** — O sistema DEVE exibir, para cada avaliação, o nome de quem avaliou,
  a data, a nota em estrelas, o comentário e quantas pessoas a marcaram como útil.
- **RF-14** — O sistema DEVE exibir as 5 primeiras avaliações e oferecer
  "Ver mais", que acrescenta mais 5 à lista sem sair da página do produto.
- **RF-15** — O sistema NÃO DEVE exibir o "Ver mais" quando todas as avaliações
  do produto já estiverem na tela.
- **RF-16** — O sistema DEVE oferecer a ordenação das avaliações entre
  *Relevantes*, *Mais recentes*, *Maior nota* e *Menor nota*, começando por
  *Relevantes*.
- **RF-17** — O sistema DEVE preservar a ordenação escolhida e as avaliações já
  carregadas depois de marcar uma avaliação como útil.
- **RF-18** — O sistema DEVE exibir uma mensagem convidando à primeira avaliação
  quando o produto ainda não tiver nenhuma, no lugar da média e do histograma.
- **RF-19** — O cliente autenticado DEVE poder marcar uma avaliação como útil e
  desmarcá-la, e o número exibido reflete a mudança imediatamente.
- **RF-20** — O sistema DEVE convidar o visitante não autenticado a entrar quando
  ele tentar marcar uma avaliação como útil, e NÃO DEVE registrar o voto.
- **RF-21** — O sistema NÃO DEVE deixar ninguém marcar como útil a própria
  avaliação.

### Tela

- **RF-22** — O sistema DEVE apresentar a página em uma coluna única em telas
  estreitas, sem rolagem horizontal.

## 6. Regras de negócio

- **RN-01** — Descrição do produto é opcional e tem no máximo 4000 caracteres.
- **RN-02** — O resumo exibido ao lado da imagem são os primeiros 160 caracteres
  da descrição, cortados no fim de uma palavra e encerrados com reticências.
  Descrição com 160 caracteres ou menos é exibida inteira, sem reticências.
- **RN-03** — A nota média é a média aritmética das notas das avaliações do
  produto, arredondada para uma casa decimal. Produto sem avaliação não tem
  média — não é zero.
- **RN-04** — Cada barra do histograma é a proporção entre as avaliações daquela
  nota e o total de avaliações do produto.
- **RN-05** — *Relevantes* ordena da avaliação com mais marcações de útil para a
  com menos; empate é desfeito pela mais recente.
- **RN-06** — Uma pessoa marca uma avaliação como útil no máximo uma vez. Marcar
  de novo desfaz a marcação anterior.
- **RN-07** — Ninguém marca como útil a própria avaliação.
- **RN-08** — A contagem de marcações de útil de uma avaliação é o número de
  pessoas distintas que a marcaram, e nunca é negativa.
- **RN-09** — Toda avaliação registra a data em que foi escrita.
- **RN-10** — A quantidade escolhida na página é um número inteiro entre 1 e 99,
  e começa em 1.
- **RN-11** — Preço é exibido no formato brasileiro, com vírgula decimal e duas
  casas. Data de avaliação é exibida no formato `26 mar. 2026`.
- **RN-12** — Produto inativo não é visível ao cliente por nenhum caminho.

## 7. Critérios de aceite

### CA-01 — Abrir a página pelo card
- **Dado** que estou na vitrine da página inicial
- **Quando** clico no card do produto "Pé de Moleque Doce de Matar"
- **Então** chego à página desse produto e vejo sua imagem, o nome, o preço
  "R$ 29,99" e o resumo da descrição

### CA-02 — Ver a descrição completa
- **Dado** que estou na página de um produto com descrição longa
- **Quando** clico em "Ver mais detalhes"
- **Então** a página rola até a seção "Características do Pé de Moleque Doce de
  Matar" com o texto completo

### CA-03 — Produto sem descrição
- **Dado** que estou na página de um produto cadastrado sem descrição
- **Quando** a página abre
- **Então** não vejo o resumo, nem o "Ver mais detalhes", nem a seção de
  características — e vejo normalmente imagem, nome, preço e avaliações

### CA-04 — Produto inexistente
- **Dado** que estou navegando na loja
- **Quando** acesso a página de um identificador que não corresponde a produto nenhum
- **Então** recebo a tela de erro de recurso não encontrado

### CA-05 — Produto inativo
- **Dado** que o administrador deixou o produto "Bolo de Teste" como inativo
- **Quando** acesso a página desse produto
- **Então** recebo a mesma resposta de produto não encontrado

### CA-06 — Produto fora de estoque
- **Dado** que o produto está com status fora de estoque
- **Quando** abro sua página
- **Então** vejo o aviso "Fora de estoque" e não consigo escolher quantidade

### CA-07 — Resumo das avaliações
- **Dado** que o produto tem 983 avaliações com média 4,54
- **Quando** abro sua página
- **Então** vejo "4,5", "983 avaliações" e as cinco barras do histograma, com a
  barra de 5 estrelas proporcionalmente maior que a de 1 estrela

### CA-08 — Produto sem avaliação
- **Dado** que o produto ainda não tem avaliação
- **Quando** abro sua página
- **Então** vejo "Este produto ainda não tem avaliações." no lugar da média, do
  histograma e da lista

### CA-09 — Carregar mais avaliações
- **Dado** que o produto tem 8 avaliações e vejo as 5 primeiras
- **Quando** clico em "Ver mais"
- **Então** passo a ver as 8, na mesma ordem, e o "Ver mais" desaparece

### CA-10 — Ordenar por mais recentes
- **Dado** que estou vendo as avaliações ordenadas por relevantes
- **Quando** escolho "Mais recentes"
- **Então** a avaliação de data mais nova passa a ser a primeira da lista

### CA-11 — Marcar como útil
- **Dado** que estou autenticado e vejo uma avaliação de outra pessoa com "Útil (3)"
- **Quando** clico em "Útil"
- **Então** passo a ver "Útil (4)" com o botão marcado, e continuo na mesma
  posição da lista, com a mesma ordenação e as mesmas avaliações carregadas

### CA-12 — Desmarcar como útil
- **Dado** que já marquei aquela avaliação como útil
- **Quando** clico em "Útil" de novo
- **Então** a contagem volta ao valor anterior e o botão deixa de aparecer marcado

### CA-13 — Visitante tentando marcar como útil
- **Dado** que não estou autenticado
- **Quando** clico em "Útil" de uma avaliação
- **Então** sou convidado a entrar e a contagem da avaliação não muda

### CA-14 — Marcar a própria avaliação
- **Dado** que estou autenticado e uma das avaliações da lista é minha
- **Quando** a página abre
- **Então** o botão "Útil" da minha avaliação não está disponível para mim, e um
  envio forçado do voto não altera contagem nenhuma

### CA-15 — Descrição no cadastro
- **Dado** que estou autenticado como administrador
- **Quando** cadastro um produto preenchendo a descrição
- **Então** essa descrição aparece na página do produto recém-criado

### CA-16 — Tela estreita
- **Dado** que abro a página em uma tela de 375 pixels de largura
- **Quando** percorro a página inteira
- **Então** o conteúdo aparece em coluna única, sem rolagem horizontal e sem
  texto cortado

## 8. Fora de escopo

- **Galeria de imagens.** A referência visual mostra miniaturas ao lado da
  imagem principal; esta entrega usa apenas a imagem única que o produto já tem.
  Galeria é feature própria, e depende de o produto passar a guardar mais de uma
  imagem.
- **Escrever avaliação.** A página exibe e ordena avaliações e recebe o voto de
  útil, mas não tem formulário para avaliar — isso é a feature "Avaliação de
  produto" do backlog, que depende do carrinho.
- **Carrinho.** O seletor de quantidade e o botão "ADICIONAR AO CARRINHO"
  aparecem na tela como na referência, mas ainda não têm efeito: o carrinho é
  feature própria, dependente de estoque e endereço. Nada é adicionado, nada é
  guardado.
- **Favoritar o produto** a partir desta página.
- **Navegação por categoria.** O caminho de navegação exibe o nome da
  subcategoria como texto, não como link, porque a página de categoria ainda
  não existe.
- **Denúncia de avaliação, resposta da loja e avaliação com foto.**
- **Produtos relacionados** ao final da página.
- **Edição da descrição** de produtos já cadastrados — depende da tela de edição
  de produto, que é feature própria. Nesta entrega, produtos antigos ficam sem
  descrição até serem recadastrados.

## 9. Dependências

- **Depende de:**
  - `001-cadastro-produto-admin` (implementada) — é onde a descrição passa a ser
    preenchida;
  - `003-modelo-de-dados-completo` (implementada) — `Avaliacao`, `Subcategoria`
    e `Categoria` já existem;
  - `004-separar-pessoa-de-credencial` (implementada) — exibir o nome de quem
    avaliou exige que a avaliação alcance o usuário; a `004` resolveu a
    limitação que a `003` registrou como RQ-02, que impedia isso.
- **Bloqueia:** a feature de escrever avaliação (reaproveita o bloco de
  avaliações desta tela) e a página de carrinho (herda o seletor de quantidade
  e o botão desta tela).

## 10. Pendências

Nenhuma. As três ambiguidades da referência visual foram decididas com a loja
antes desta spec:

- [x] ~~Galeria de imagens~~ — **Resolvido:** sem galeria; apenas a imagem
      principal do produto.
- [x] ~~Voto "Útil"~~ — **Resolvido:** funciona, exige autenticação e alterna.
- [x] ~~"Adicionar ao carrinho"~~ — **Resolvido:** aparece na tela como na
      referência, sem efeito, até a feature de carrinho.

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
