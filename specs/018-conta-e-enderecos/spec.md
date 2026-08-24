# Especificação — Conta e endereços

**ID:** `018-conta-e-enderecos` · **Branch:** `018-conta-e-enderecos`
**Criada em:** 2026-08-23 · **Status:** Implementada

---

## 1. Contexto e problema

**O cliente não tem onde ver nem mudar os próprios dados.** Ele informa nome,
CPF, celular e data de nascimento ao criar a conta, e depois disso não há tela
nenhuma que os mostre de volta. Celular errado no cadastro é celular errado para
sempre.

**O atalho "Conta" do cabeçalho está desabilitado.** A `014` o desligou porque
apontava para uma ação que não existe e levava a erro. Desde então ele aparece
apagado em toda página, para todo cliente autenticado — anunciando uma tela que
nunca chegou.

**Editar os dados já está pronto e nunca foi ligado.** `Usuario.AtualizarDados`
existe no domínio desde a `004`, `IUsuarioService.AlterarDadosUsuario` existe na
aplicação, os dois têm teste de unidade passando — e **nenhum controlador
chama**. Falta apenas a tela.

**Endereço é a quarta entidade que existe e nunca foi usada.** `Endereco` tem
tabela, mapeamento e validação desde a `003`. Ninguém nunca cadastrou um, porque
não há por onde. E `Pedido` exige `EnderecoEntregaId` no construtor — sem
endereço cadastrado, nenhum pedido pode existir.

**A entidade não sabe qual endereço é o principal.** São nove campos, e nenhum
deles distingue o endereço de entrega habitual dos demais. Quem tiver três
endereços vai escolher entre três iguais a cada compra.

## 2. Objetivo

Dar ao cliente uma área de conta com duas coisas: os próprios dados, editáveis,
e os endereços de entrega, com um deles marcado como principal.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente autenticado | Vê e corrige os próprios dados; cadastra, edita, exclui e escolhe endereços |
| Cliente (visitante) | Não alcança a área de conta |
| Administrador da loja | Não é afetado nesta entrega |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero corrigir meu celular sem ter que criar
> outra conta.
>
> **HU-02** — Como **cliente**, quero guardar meu endereço uma vez e não digitar
> de novo a cada compra.
>
> **HU-03** — Como **cliente**, quero ter mais de um endereço — casa e trabalho
> — e dizer qual é o habitual.
>
> **HU-04** — Como **cliente**, quero digitar o CEP e ver o resto do endereço
> aparecer, em vez de preencher tudo à mão.
>
> **HU-05** — Como **cliente**, quero conseguir cadastrar meu endereço mesmo
> quando a busca por CEP não funciona.
>
> **HU-06** — Como **cliente**, quero chegar aos meus dados clicando em "Conta",
> como o cabeçalho promete desde sempre.

## 5. Requisitos funcionais

### Área de conta

- **RF-01** — O cliente autenticado DEVE poder alcançar uma área de conta a
  partir do cabeçalho.
- **RF-02** — A área de conta DEVE reunir, em seções distinguíveis, os dados
  pessoais e os endereços.
- **RF-03** — O visitante NÃO DEVE alcançar a área de conta.

### Dados pessoais

- **RF-04** — O cliente DEVE poder ver os próprios dados: nome, CPF, celular e
  data de nascimento.
- **RF-05** — O cliente DEVE poder alterar nome, celular e data de nascimento.
- **RF-06** — O cliente NÃO DEVE poder alterar o próprio CPF.
- **RF-07** — Dado inválido DEVE ser recusado com mensagem no campo, e o
  formulário DEVE voltar preenchido com o que foi digitado.

### Endereços

- **RF-08** — O cliente DEVE poder ver todos os seus endereços.
- **RF-09** — O cliente DEVE poder cadastrar um endereço.
- **RF-10** — O cliente DEVE poder editar um endereço.
- **RF-11** — O cliente DEVE poder excluir um endereço.
- **RF-12** — O cliente DEVE poder marcar qual endereço é o principal.
- **RF-13** — A lista DEVE indicar qual endereço é o principal.
- **RF-14** — O cliente sem nenhum endereço DEVE receber um convite para
  cadastrar o primeiro, em vez de uma área vazia.
- **RF-15** — O cliente NÃO DEVE alcançar, ver nem alterar endereço de outra
  pessoa.

### Busca por CEP

- **RF-16** — Informado o CEP, o sistema DEVE preencher estado, cidade, bairro e
  rua automaticamente.
- **RF-17** — Os campos preenchidos automaticamente DEVEM continuar editáveis.
- **RF-18** — Falha, demora ou CEP inexistente NÃO DEVEM impedir o cadastro: os
  campos continuam preenchíveis à mão.
- **RF-19** — O cadastro e a edição DEVEM funcionar sem JavaScript.

## 6. Regras de negócio

- **RN-01** — Havendo ao menos um endereço, **exatamente um** é o principal.
  Nunca zero, nunca dois.
- **RN-02** — O primeiro endereço cadastrado torna-se principal
  automaticamente, sem a pessoa precisar escolher.
- **RN-03** — Marcar um endereço como principal desmarca o anterior. É uma
  escolha entre os que existem, não um atributo independente de cada um.
- **RN-04** — Excluir o endereço principal promove outro a principal, desde que
  reste algum. Excluir o último não deixa nada para promover, e aí não há
  principal porque não há endereço.
- **RN-05** — O endereço pertence a quem o cadastrou. Nenhuma pessoa vê, edita
  ou exclui endereço de outra — nem informando o identificador diretamente.
- **RN-06** — O CPF identifica a pessoa e não muda. Nome, celular e data de
  nascimento são correção de dado; CPF seria troca de identidade.
- **RN-07** — O preenchimento automático por CEP é **conveniência, nunca
  requisito**. Todo campo que ele preenche continua editável, e nenhuma falha
  dele impede o cadastro — a pessoa digita e segue.
- **RN-08** — Um controle oferecido ao cliente entrega o que anuncia. Regra
  herdada da `012`, `013`, `014`, `015` e `017` — e é por ela que o atalho
  "Conta" deixa de estar apagado só quando a tela existir de verdade.

## 7. Critérios de aceite

### CA-01 — O atalho do cabeçalho leva à conta
- **Dado** que estou autenticado, em qualquer página
- **Quando** aciono o atalho "Conta"
- **Então** chego à área de conta, e ele não está mais apagado

### CA-02 — A conta reúne as duas seções
- **Dado** que abri a área de conta
- **Quando** olho a tela
- **Então** encontro os meus dados pessoais e os meus endereços, distinguíveis

### CA-03 — O visitante não entra
- **Dado** que não estou autenticado
- **Quando** tento abrir a área de conta
- **Então** sou levado a entrar, e não vejo dado de ninguém

### CA-04 — Os dados aparecem preenchidos
- **Dado** que criei minha conta informando nome, CPF, celular e nascimento
- **Quando** abro os dados pessoais
- **Então** vejo os quatro, com os valores que informei

### CA-05 — Alterar os dados funciona
- **Dado** que estou nos meus dados pessoais
- **Quando** corrijo o celular e confirmo
- **Então** a alteração é gravada, e continua lá quando eu voltar

### CA-06 — O CPF não é alterável
- **Dado** que estou nos meus dados pessoais
- **Quando** tento alterar o CPF
- **Então** não consigo

### CA-07 — Dado inválido volta com mensagem no campo
- **Dado** que estou editando meus dados
- **Quando** informo um celular inválido e confirmo
- **Então** vejo a mensagem no campo do celular, e o resto do formulário
  continua preenchido com o que eu havia digitado

### CA-08 — Cadastrar o primeiro endereço
- **Dado** que não tenho nenhum endereço
- **Quando** cadastro um
- **Então** ele aparece na lista, já marcado como principal

### CA-09 — Lista vazia convida
- **Dado** que não tenho nenhum endereço
- **Quando** abro a seção de endereços
- **Então** encontro um convite para cadastrar o primeiro

### CA-10 — Cadastrar o segundo não rouba o principal
- **Dado** que já tenho um endereço principal
- **Quando** cadastro outro
- **Então** o primeiro continua sendo o principal

### CA-11 — Trocar o principal
- **Dado** que tenho dois endereços
- **Quando** marco o segundo como principal
- **Então** ele passa a ser, e o primeiro deixa de ser

### CA-12 — A lista mostra qual é o principal
- **Dado** que tenho endereços cadastrados
- **Quando** olho a lista
- **Então** consigo ver qual é o principal

### CA-13 — Editar um endereço
- **Dado** que tenho um endereço cadastrado
- **Quando** altero o número e confirmo
- **Então** a alteração é gravada

### CA-14 — Excluir um endereço comum
- **Dado** que tenho dois endereços e o principal é o primeiro
- **Quando** excluo o segundo
- **Então** ele sai da lista e o primeiro continua principal

### CA-15 — Excluir o principal promove outro
- **Dado** que tenho dois endereços
- **Quando** excluo o que é principal
- **Então** ele sai da lista e o restante passa a ser o principal

### CA-16 — Excluir o último não deixa órfão
- **Dado** que tenho um único endereço
- **Quando** o excluo
- **Então** a lista fica vazia e volto a receber o convite para cadastrar

### CA-17 — Endereço alheio é inalcançável
- **Dado** que conheço o identificador do endereço de outra pessoa
- **Quando** tento abri-lo, editá-lo ou excluí-lo
- **Então** não consigo, e nada é alterado

### CA-18 — O CEP preenche o endereço
- **Dado** que estou cadastrando um endereço
- **Quando** informo um CEP válido
- **Então** estado, cidade, bairro e rua aparecem preenchidos

### CA-19 — O que veio do CEP continua editável
- **Dado** que o CEP preencheu os campos
- **Quando** altero a rua
- **Então** consigo, e o que eu digitei é o que fica

### CA-20 — Falha na busca por CEP não impede nada
- **Dado** que a busca por CEP não responde ou não encontra o CEP
- **Quando** preencho os campos à mão e confirmo
- **Então** o endereço é cadastrado normalmente

### CA-21 — Cadastrar funciona sem JavaScript
- **Dado** que o navegador está com JavaScript desligado
- **Quando** preencho todos os campos à mão e confirmo
- **Então** o endereço é cadastrado

## 8. Fora de escopo

- **Trocar a senha pela área de conta.** A troca com senha atual é fluxo próprio
  do Identity, diferente da redefinição por token que a `002` construiu.
  Entrega própria.
- **Ver os próprios pedidos.** Não existem pedidos até a `019`. "Meus pedidos"
  entra na conta quando houver o que listar.
- **Trocar o e-mail.** É a credencial de acesso, não dado de perfil — mexer nela
  envolve confirmação por e-mail e invalidação de sessão.
- **Excluir a própria conta.** Ninguém pediu, e envolve decidir o que acontece
  com pedidos e avaliações já feitos.
- **Apelido do endereço** ("Casa", "Trabalho"). A entidade não tem o campo, e
  nenhuma referência visual o pede.
- **Escolher o endereço no fechamento.** É a `019`. Aqui o endereço é cadastrado
  e marcado; usá-lo numa compra é lá.
- **Frete.** Depende do endereço, mas a cotação é da `019`.
- **Validar se o endereço existe de verdade.** O CEP é conferido no formato, e o
  preenchimento automático é conveniência (RN-07). Ninguém checa se o número
  informado existe naquela rua.

## 9. Dependências

- **Depende de:** `003`, que criou a entidade `Endereco` e sua tabela; `004`,
  que separou `Usuario` de `ContaDeAcesso` e entregou `AtualizarDados` e
  `AlterarDadosUsuario` sem consumidor; `014`, que desligou o atalho "Conta" e
  registrou a dívida que esta feature paga.
- **Bloqueia:** a `019` (fechamento de pedido), que precisa de um endereço
  cadastrado para preencher o `EnderecoEntregaId` que `Pedido` exige no
  construtor. É independente da `017` (carrinho) — as duas podem ser feitas em
  qualquer ordem.

## 10. Decisões e pendências

**A área de conta nasce com duas seções, não com uma.** Foi decisão explícita do
responsável. O peso da segunda é baixo: editar dados pessoais é ligar o que a
`004` já construiu e testou — domínio e aplicação prontos, faltando só a tela. E
resolve o desconforto de um atalho chamado "Conta" levar a uma página que só
tem endereços.

**"Meus pedidos" fica de fora por não existir.** Será a terceira seção quando a
`019` criar pedidos. O menu da conta nasce preparado para crescer.

**`Endereco` ganha duas colunas, não uma.** `Padrao` é a que a RN-01 exige. A
segunda é `DataCadastro`, e ela se justifica sozinha: sem uma ordem estável, a
lista de endereços apareceria na ordem arbitrária que o banco devolvesse, e a
RN-04 não teria como dizer **qual** endereço promover ao excluir o principal.
Com ela, a lista tem ordem e a promoção tem critério — o mais antigo entre os
restantes.

**A regra do "exatamente um principal" não cabe na entidade.** `Endereco`
sozinho não sabe quantos irmãos tem. A RN-01, a RN-03 e a RN-04 são invariantes
de coleção, e vivem no serviço de aplicação, que enxerga todos os endereços da
pessoa. A entidade guarda apenas o próprio estado (`Padrao`) e os métodos que o
alteram.

**`Endereco` precisa ganhar métodos de alteração.** Hoje é uma entidade de
criação apenas — não tem um único `Alterar*`. Pelo Princípio II, estado só muda
por método de intenção, então editar exige acrescentá-los.

**A busca por CEP acontece no navegador, não no servidor.** É a escolha que
torna a RN-07 quase automática: sem JavaScript não há busca, e os campos já
nascem preenchíveis à mão. A alternativa — um endpoint no servidor que consulta
o serviço e devolve — exigiria cliente HTTP, configuração de tempo limite e uma
rota nova, para entregar o mesmo preenchimento e ainda assim precisar do mesmo
piso manual.

**⚠️ Excluir endereço já usado por um pedido é problema da `019`, não desta.**
Hoje não existem pedidos, então toda exclusão funciona. Quando a `019` criar
`Pedido` com chave estrangeira para `Endereco`, excluir um endereço já usado
passará a ser recusado pelo banco — e caberá a ela decidir o que a tela faz
nesse caso. Registrado aqui para não ser descoberto lá.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** —
pendência herdada da `013` §10, repetida na `014`, `015`, `016` e `017`, ainda
sem critério definido pelo responsável. Segue fora de escopo.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — as colunas novas são citadas na seção 10 como decisão
      tomada, não como requisito
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — RF-07 e
      CA-07 cobrem dado inválido; RF-18 e CA-20, falha na busca por CEP; RF-15 e
      CA-17, acesso a endereço alheio; RF-03 e CA-03, visitante na área de
      conta; CA-16, exclusão do último endereço
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as duas pendências da
      seção 10 são, uma, decisão delegada à `019` por não ter efeito hoje, e a
      outra, decisão de negócio herdada
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
