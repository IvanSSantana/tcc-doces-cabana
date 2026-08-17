# Especificação — Páginas institucionais

**ID:** `009-paginas-institucionais` · **Branch:** `009-paginas-institucionais`
**Criada em:** 2026-08-17 · **Status:** Implementada

> As "páginas estáticas" do pedido original estão aqui sob o nome
> **institucionais** — é o termo que o resto do documento usa para o grupo
> (Quem Somos + Política de Privacidade) e o que dá nome à área no código.
> "Estática" descreve como a página é construída, não o que ela é para o
> cliente; a spec fala do segundo.

---

## 1. Contexto e problema

O rodapé do site já promete três coisas — "Quem Somos", "Política de
Privacidade" e "Nosso Endereço" — mas só a terceira leva a algum lugar: as duas
primeiras são links mortos (`href="#"`). O mesmo link morto aparece no modal de
login, no exato momento em que a pessoa entrega e-mail e senha. Uma loja que
pede CPF, data de nascimento e endereço no cadastro e não publica sua política
de privacidade está em desacordo com a LGPD, que a própria política transcrita
no [Anexo A](./conteudo-politica.md) diz cumprir. E não há em lugar nenhum do
site uma página que conte quem é a Doce Cabana — a loja tem uma história de
doces de infância que hoje não é contada.

## 2. Objetivo

Publicar as duas páginas institucionais da loja — Quem Somos e Política de
Privacidade — e ligar a elas todos os pontos do site que hoje apontam para
lugar nenhum.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Abre as duas páginas pelo rodapé de qualquer tela ou pelo link do modal de login, sem precisar de conta |
| Cliente autenticado | Idem — o conteúdo é o mesmo; nenhuma das páginas muda conforme quem está logado |
| Administrador da loja | Nenhuma tela administrativa nova. O texto das páginas é fixo no site e só muda por nova entrega |

## 4. Histórias de usuário

> **HU-01** — Como **visitante**, quero ler a política de privacidade antes de
> criar uma conta, para saber quais dados a loja coleta e o que faz com eles.
>
> **HU-02** — Como **visitante**, quero alcançar a política a partir do rodapé
> de qualquer página e do próprio modal de login, para não precisar procurar.
>
> **HU-03** — Como **visitante**, quero conhecer a história, a missão e a visão
> da Doce Cabana, para decidir se quero comprar de uma loja pequena que não
> conheço.
>
> **HU-04** — Como **titular de dados**, quero encontrar na política o canal de
> contato do encarregado de proteção de dados, para exercer meus direitos
> previstos na LGPD.
>
> **HU-05** — Como **visitante no celular**, quero ler as duas páginas sem
> arrastar a tela para os lados, porque é assim que vou abrir o site.

## 5. Requisitos funcionais

### Acesso e navegação

- **RF-01** — O sistema DEVE oferecer uma página pública de Política de
  Privacidade, alcançável por endereço próprio e estável.
- **RF-02** — O sistema DEVE oferecer uma página pública "Quem Somos",
  alcançável por endereço próprio e estável.
- **RF-03** — O sistema DEVE ligar o link "Política de Privacidade" do rodapé à
  página de RF-01, em todas as telas onde o rodapé aparece.
- **RF-04** — O sistema DEVE ligar o link "Quem Somos" do rodapé à página de
  RF-02, em todas as telas onde o rodapé aparece.
- **RF-05** — O sistema DEVE ligar o link "Política de Privacidade" do modal de
  login à mesma página de RF-01 — não a uma cópia do texto.
- **RF-06** — O sistema NÃO DEVE exigir autenticação para nenhuma das duas
  páginas, nem alterar o que exibe conforme o visitante esteja logado ou não.
- **RF-07** — O sistema DEVE remover a página de privacidade de rascunho que
  existe hoje, cujo conteúdo é o texto de andaime em inglês *"Use this page to
  detail your site's privacy policy"*, para que não haja duas rotas de política
  no site.

### Política de Privacidade

- **RF-08** — O sistema DEVE exibir o título "Política de Privacidade" no topo
  da página.
- **RF-09** — O sistema DEVE exibir o texto integral do [Anexo A](./conteudo-politica.md),
  na mesma ordem e com a mesma hierarquia de títulos, sem omissão nem paráfrase.
- **RF-10** — O sistema DEVE separar visualmente cada uma das 11 seções da
  política das seções vizinhas.
- **RF-11** — O sistema DEVE exibir o e-mail do encarregado de proteção de dados
  na seção "Contato" como um endereço acionável, que abre o cliente de e-mail da
  pessoa já endereçado.

### Quem Somos

- **RF-12** — O sistema DEVE abrir a página com uma faixa de destaque contendo
  uma imagem da loja e a frase "Revivendo os sabores da nossa infância.", com a
  palavra final grafada à mão, em destaque do restante da frase.
- **RF-13** — O sistema DEVE exibir três blocos — Missão, Propósito e Visão —
  cada um com um título grafado à mão, um texto curto e uma imagem. Nesta
  entrega, texto e imagem de cada bloco são conteúdo de preenchimento (texto
  padrão de amostra e retângulo de lugar reservado), como a referência visual
  os define — RF-13 pede a estrutura dos três blocos, não o texto final da
  loja, que é entrega futura (seção 8).
- **RF-14** — O sistema DEVE alternar o lado da imagem e do texto a cada bloco,
  de modo que a leitura desça em ziguezague em torno de um eixo vertical
  contínuo.
- **RF-15** — O sistema DEVE apresentar os três blocos na ordem Missão →
  Propósito → Visão.

### Comportamento comum às duas páginas

- **RF-16** — O sistema DEVE reorganizar as duas páginas em coluna única em
  telas estreitas, sem rolagem horizontal.
- **RF-17** — O sistema NÃO DEVE oferecer nenhum formulário, botão de envio ou
  qualquer outro controle que altere estado nas duas páginas.

## 6. Regras de negócio

- **RN-01** — As duas páginas são públicas e o conteúdo é idêntico para
  visitante, cliente autenticado e administrador. Não existe variação por perfil.
- **RN-02** — Nenhuma das páginas lê ou grava dado de cliente. Abrir qualquer
  uma delas não produz efeito nenhum no sistema.
- **RN-03** — O texto da política é documento legal: a página reproduz o
  [Anexo A](./conteudo-politica.md) palavra por palavra. Corrigir, resumir ou
  "melhorar" o texto durante a implementação é defeito, não zelo — alteração de
  conteúdo passa antes pelo anexo.
- **RN-04** — Existe **uma** página de política no site. Todo link para política
  de privacidade, de onde quer que parta, aponta para ela.
- **RN-05** — O eixo vertical do Quem Somos é estrutura, não enfeite: ele existe
  para amarrar o ziguezague de RF-14. Quando os blocos deixam de alternar
  (coluna única, RF-16), o eixo deixa de ter função e não é exibido.

## 7. Critérios de aceite

### CA-01 — Chegar à política pelo rodapé
- **Dado** que estou em qualquer página da loja como visitante
- **Quando** clico em "Política de Privacidade" no rodapé
- **Então** a página de política abre e mostra o título "Política de Privacidade"

### CA-02 — Chegar à política pelo modal de login
- **Dado** que abri o modal de login
- **Quando** clico em "Política de Privacidade" dentro do modal
- **Então** chego à mesma página de CA-01, e não a outro texto

### CA-03 — Chegar ao Quem Somos pelo rodapé
- **Dado** que estou em qualquer página da loja como visitante
- **Quando** clico em "Quem Somos" no rodapé
- **Então** a página abre e mostra a frase "Revivendo os sabores da nossa infância."

### CA-04 — Política completa e na ordem
- **Dado** que abri a página de política
- **Quando** percorro a página do topo ao fim
- **Então** encontro as 11 seções do Anexo A, na ordem "Definições", "Quais
  dados pessoais coletamos?", "Qual o objetivo do tratamento de dados?", "Quando
  e como coletamos seus dados?", "Compartilhamento de dados pessoais", "Por
  quanto tempo armazenamos os dados?", "Tratamento de dados de menores de
  idade", "Direitos dos titulares de dados", "Como solicitar seus direitos",
  "Atualizações desta Política" e "Contato"

### CA-05 — Contato do encarregado é acionável
- **Dado** que estou na seção "Contato" da política
- **Quando** clico no e-mail do encarregado de proteção de dados
- **Então** meu cliente de e-mail abre com o destinatário já preenchido

### CA-06 — Os três blocos do Quem Somos
- **Dado** que abri a página Quem Somos
- **Quando** desço a página
- **Então** vejo, nesta ordem, os blocos "Missão", "Propósito" e "Visão", cada um
  com título, texto e imagem

### CA-07 — Ziguezague em torno do eixo
- **Dado** que abri o Quem Somos em tela larga
- **Quando** comparo os três blocos
- **Então** o bloco "Propósito" tem imagem e texto trocados de lado em relação a
  "Missão" e "Visão", e um eixo vertical contínuo corre entre as duas colunas

### CA-08 — As duas páginas são públicas
- **Dado** que não estou autenticado
- **Quando** abro cada uma das duas páginas pelo endereço direto
- **Então** ambas abrem normalmente, sem me mandar para a tela de login

### CA-09 — Nenhuma rota de política duplicada
- **Dado** que a página de rascunho de privacidade existia antes desta entrega
- **Quando** abro o endereço antigo dela
- **Então** recebo "não encontrado", e não uma segunda política com texto em
  inglês

### CA-10 — Leitura no celular
- **Dado** que abro cada uma das duas páginas numa tela de 375 pixels de largura
- **Quando** rolo até o fim
- **Então** o conteúdo cabe na largura da tela, sem barra de rolagem horizontal
  em nenhum ponto

### CA-11 — Navegação por teclado
- **Dado** que percorro cada uma das duas páginas usando apenas a tecla Tab
- **Quando** o foco chega a um link
- **Então** o link em foco fica visivelmente marcado

### CA-12 — Nada muda de estado
- **Dado** que abri qualquer uma das duas páginas
- **Quando** procuro por um campo, botão de envio ou formulário
- **Então** não encontro nenhum: as páginas só leem

## 8. Fora de escopo

- **Índice lateral ou sumário navegável na política.** A política é longa e um
  sumário ajudaria, mas a referência visual não tem um e o resto do site não usa
  esse padrão em lugar nenhum. Fica registrado como candidato a feature própria,
  não entra de carona.
- **Edição do conteúdo pelo administrador.** Nenhuma tela de gestão de texto
  institucional. Mudar o texto é entrega nova.
- **Texto definitivo e fotos reais de Missão, Propósito e Visão.** Publicados
  como conteúdo de preenchimento nesta entrega (RF-13), exatamente como a
  referência visual os define. Substituir por texto e fotos da loja é entrega
  futura.
- **Banner de consentimento de cookies.** A política *descreve* o uso de
  cookies; o mecanismo de consentimento é outro assunto, com regra e tela
  próprias.
- **"Nosso Endereço" como página interna.** Continua sendo o link externo para o
  mapa, como já é hoje.
- **Termos de Uso, Trocas e Devoluções, Perguntas Frequentes.** São páginas
  institucionais também, mas não foram pedidas nem desenhadas.
- **Versionamento e data de última atualização da política.** A seção
  "Atualizações desta Política" fala em atualizar; o carimbo de versão exibido
  ao cliente não foi especificado.
- **Tradução ou versão em outro idioma.**

## 9. Dependências

- **Depende de:** o rodapé compartilhado e o modal de login, ambos já existentes
  desde a `000`/`004` — esta feature liga seus links, não os cria.
- **Bloqueia:** nada. É a primeira feature do projeto que não abre caminho para
  outra; entrega valor sozinha.

## 10. Pendências

Nenhuma. As duas questões abertas na versão anterior desta spec — texto real de
Missão/Propósito/Visão e as quatro imagens — foram resolvidas por decisão
explícita: **esta entrega publica o esqueleto exatamente como a referência
visual o mostra**, texto de preenchimento (`Lorem ipsum dolor sit amet,
consectetur adipiscing elit. Phasellus tortor ipsum dolor sit.`) e retângulo
cinza de lugar reservado no lugar de cada imagem, com um comentário no código
identificando cada ponto como pendente de conteúdo real. Trocar o texto e as
imagens por conteúdo definitivo da loja é entrega futura, fora do escopo desta
feature (ver seção 8).

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
