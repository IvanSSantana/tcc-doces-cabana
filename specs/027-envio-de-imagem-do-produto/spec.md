# Especificação — Envio de imagem do produto

**ID:** `027-envio-de-imagem-do-produto` · **Branch:** `027-envio-de-imagem-do-produto`
**Criada em:** 2026-09-05 · **Status:** Rascunho

---

## 1. Contexto e problema

**A loja não consegue publicar uma foto.** Para cadastrar um produto, quem
administra precisa subir a imagem em algum outro lugar, gerar um endereço para
ela e colar esse endereço no formulário. O sistema não recebe imagem — recebe
texto.

**O endereço colado hoje é emprestado, e é de outra coisa.** Os cem produtos da
massa de demonstração apontam para seis links de *pré-visualização* do Google
Drive, em rodízio. Link de pré-visualização é uma página, não um arquivo de
imagem: funciona por acidente, e para de funcionar quando o dono do arquivo
mexe na permissão, move a pasta ou some com a conta. Nada no sistema percebe.

**Nada garante que o que foi colado seja imagem.** O único critério hoje é ser
um endereço `http` bem formado. Endereço de página, de arquivo de texto ou de
imagem que nunca existiu passa igual, e o defeito só aparece na vitrine.

## 2. Objetivo

Fazer a loja publicar a foto do produto pelo próprio sistema, e guardá-la onde
ela não dependa de endereço emprestado.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Administrador da loja | Escolhe um arquivo do computador ao cadastrar o produto, em vez de caçar um endereço em outro serviço |
| Cliente | Vê a mesma imagem de sempre — o que muda é de onde ela vem |
| Quem desenvolve o projeto | Passa a precisar da credencial do armazenamento para cadastrar produto; sem ela, o cadastro recusa explicando o motivo |

## 4. Histórias de usuário

> **HU-01** — Como **dona da loja**, quero escolher a foto do produto direto do
> meu computador, sem depender de endereço gerado em outro serviço.
>
> **HU-02** — Como **dona da loja**, quero saber na hora que o arquivo não
> serve, em vez de descobrir depois que a imagem não aparece na vitrine.
>
> **HU-03** — Como **dona da loja**, quero que o produto não seja cadastrado
> pela metade quando o envio da imagem falhar.
>
> **HU-04** — Como **quem desenvolve**, quero que o sistema diga que falta
> credencial, em vez de falhar de um jeito que eu precise investigar.

## 5. Requisitos funcionais

### O envio

- **RF-01** — O cadastro de produto DEVE receber um arquivo de imagem, e NÃO
  DEVE mais pedir um endereço digitado.
- **RF-02** — O arquivo DEVE ser obrigatório: não se cadastra produto sem
  imagem.
- **RF-03** — Arquivo que não esteja entre os formatos de imagem aceitos DEVE
  ser recusado.
- **RF-04** — Arquivo acima do tamanho máximo aceito DEVE ser recusado.
- **RF-05** — A recusa DEVE aparecer no próprio campo do arquivo, e o restante
  do que já foi preenchido no formulário NÃO DEVE ser perdido.

### O armazenamento

- **RF-06** — A imagem aceita DEVE ser guardada no serviço de armazenamento da
  loja, e o produto DEVE registrar o endereço dela.
- **RF-07** — O nome com que o arquivo é guardado NÃO DEVE ser o nome que veio
  do computador de quem enviou.
- **RF-08** — Falhando o envio, o produto NÃO DEVE ser cadastrado, e quem
  cadastra DEVE ser informado.
- **RF-09** — A credencial do serviço de armazenamento NÃO DEVE ser versionada.

### A massa de demonstração

- **RF-10** — Os produtos de demonstração DEVEM apontar para imagens no mesmo
  serviço de armazenamento que os produtos cadastrados pela loja.
- **RF-11** — Semear a base NÃO DEVE exigir credencial.

## 6. Regras de negócio

- **RN-01** — Produto sem imagem não existe. Regra herdada do cadastro de
  produto: a imagem já era obrigatória, e o que muda aqui é só como ela chega.
- **RN-02** — O nome de arquivo de quem envia não é confiável. Ele decidiria
  parte do endereço público, e pode trazer caminho, acento ou o nome de um
  arquivo que já está lá. Quem nomeia é o sistema.
- **RN-03** — Falha de serviço externo é condição esperada, não exceção. Regra
  herdada da cotação de frete: o envio que não completa recusa com mensagem, e
  não derruba a tela.
- **RN-04** — Credencial de serviço externo não é versionada. Regra
  constitucional, aplicada aqui pela segunda vez.
- **RN-05** — Imagem de produto é pública por natureza. A loja existe para que
  ela seja vista por quem nem entrou; não há o que proteger, e endereço com
  prazo de validade transformaria "mostrar a foto" num problema que volta
  sozinho.

## 7. Critérios de aceite

### CA-01 — O cadastro pede arquivo, não endereço
- **Dado** que sou administrador
- **Quando** abro o cadastro de produto
- **Então** encontro um campo para escolher arquivo, e nenhum campo pedindo o
  endereço da imagem

### CA-02 — Produto sem imagem não é cadastrado
- **Dado** que preenchi o formulário sem escolher arquivo
- **Quando** envio
- **Então** o produto não é cadastrado, e a falta do arquivo é apontada

### CA-03 — Arquivo que não é imagem é recusado
- **Dado** que escolhi um arquivo que não é de um formato de imagem aceito
- **Quando** envio
- **Então** o produto não é cadastrado, e o motivo aparece no campo do arquivo

### CA-04 — Arquivo grande demais é recusado
- **Dado** que escolhi uma imagem acima do tamanho máximo
- **Quando** envio
- **Então** o produto não é cadastrado, e o motivo aparece no campo do arquivo

### CA-05 — A recusa preserva o resto do formulário
- **Dado** que preenchi nome, preço e medidas e o arquivo foi recusado
- **Quando** a tela volta
- **Então** o que preenchi continua lá, e só o arquivo precisa ser escolhido de
  novo

### CA-06 — A imagem enviada aparece no produto
- **Dado** que cadastrei um produto com uma imagem
- **Quando** vejo esse produto no catálogo
- **Então** a imagem exibida é a que enviei

### CA-07 — O nome do arquivo é trocado
- **Dado** que enviei um arquivo com um nome qualquer
- **Quando** o produto é gravado
- **Então** o endereço guardado não contém o nome do arquivo que enviei

### CA-08 — Falha no envio não cadastra
- **Dado** que o serviço de armazenamento recusou ou não respondeu
- **Quando** envio o formulário
- **Então** o produto não é cadastrado, e sou informado de que não foi possível
  enviar a imagem

### CA-09 — Sem credencial, o cadastro explica e recusa
- **Dado** que o serviço de armazenamento não está configurado
- **Quando** tento cadastrar um produto
- **Então** o cadastro é recusado com mensagem, e a tela continua utilizável

### CA-10 — Os produtos de demonstração têm imagem
- **Dado** uma base recém-semeada
- **Quando** abro o catálogo
- **Então** os produtos exibem imagens vindas do serviço de armazenamento da
  loja

### CA-11 — Semear não exige credencial
- **Dado** que o serviço de armazenamento não está configurado
- **Quando** a base é criada do zero
- **Então** a semeadura completa normalmente

## 8. Fora de escopo

- **Mais de uma imagem por produto.** Galeria é item próprio do backlog.
- **Trocar a imagem de um produto já cadastrado.** Não existe edição de produto
  no sistema — é item do backlog, e enquanto não existir, a imagem é escolhida
  uma vez, no cadastro.
- **Apagar o arquivo do armazenamento** quando o produto sai do ar. Pelo mesmo
  motivo: não existe exclusão de produto, e produto inativado hoje não apaga
  nada.
- **Redimensionar, comprimir ou gerar miniatura.** A imagem é guardada como
  veio.
- **Inspecionar o conteúdo do arquivo byte a byte.** A verificação é por
  formato declarado e tamanho; a tela é administrativa e exige papel de
  administrador.
- **Migrar automaticamente as imagens que estão no Drive.** As seis imagens de
  demonstração são subidas à mão, uma vez.
- **Imagens do bloco de categorias da página inicial.** Item separado do
  backlog.
- **Trocar o banco para Postgres.** Entrega própria, a seguinte.

## 9. Dependências

- **Depende de:** o cadastro de produto e a área administrativa, que criaram a
  tela onde o envio acontece; e a massa de demonstração, que define as imagens
  a substituir.
- **Bloqueia:** nada formalmente. Mas destrava o catálogo real da loja, item do
  backlog — hoje ele esbarra em não haver como publicar 390 fotos.

## 10. Decisões e pendências

**O envio acontece pelo formulário, não pelo painel do serviço.** Decisão do
responsável ao especificar. Subir pelo painel e colar o endereço seria trocar
um serviço de terceiro por outro sem resolver nada: continuaria não havendo
envio, continuaria dependendo de alguém colar o endereço certo, e continuaria
sem verificação de que o arquivo é imagem.

**O armazenamento é público.** Decisão tomada ao especificar, com medição: os
endereços assinados fornecidos pelo responsável têm 384 caracteres — contra um
limite de 255 na coluna que os guarda — e expiram em 150 dias, em 2027-02-02.
Guardar endereço que expira significaria assiná-lo de novo a cada exibição, uma
chamada por produto por listagem. Público, o mesmo arquivo tem endereço de 92
caracteres e nenhuma validade. Foto de vitrine não pede sigilo (RN-05).

**Não há segundo modo de armazenamento para desenvolvimento.** Recomendou-se um
adaptador local — como o e-mail já tem, gravando em pasta em vez de enviar por
SMTP — e o responsável escolheu não ter. Consequência aceita e registrada: sem
credencial configurada, não se cadastra produto, e o teste de ponta a ponta que
prova o cadastro sai da suíte padrão para a categoria que exige credencial,
como a cotação de frete já fez.

**A troca do banco para Postgres é entrega própria, a seguinte.** As duas
nasceram do mesmo pedido — "migrar para o Supabase" — mas não dependem uma da
outra: o armazenamento funciona com o banco atual, e o banco funciona com as
imagens atuais. Juntas ficariam na faixa de 80 a 100 tarefas, a mesma que fez o
fechamento de pedido ser quebrado em três.

**⚠️ A massa de demonstração passa a depender de um serviço externo.** Se o
armazenamento sair do ar ou os arquivos forem apagados, os cem produtos de
demonstração ficam sem imagem. É o mesmo risco que os links do Drive já
carregam hoje, movido para um lugar que a loja controla — não é regressão, mas
também não é imunidade.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** —
pendência herdada, repetida em todas as entregas desde a de correções da página
inicial, ainda sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só na seção 10
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-02
      cobre arquivo ausente; CA-03 e CA-04, arquivo inválido; CA-08, falha do
      serviço; CA-09, ausência de credencial
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as duas pendências da
      seção 10 são risco declarado e pendência herdada
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
