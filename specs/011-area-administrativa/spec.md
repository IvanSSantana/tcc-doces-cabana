# Especificação — Área administrativa

**ID:** `011-area-administrativa` · **Branch:** `011-area-administrativa`
**Criada em:** 2026-08-18 · **Status:** Rascunho

---

> **Nota sobre o formato.** Como a `002` e a `010`, esta feature não entrega
> comportamento novo ao cliente: reorganiza o que existe. A seção 5 registra
> requisitos de **qualidade interna** (`RQ-xx`), e a 6 os poucos efeitos
> observáveis — que aqui são mudanças de endereço.

---

## 1. Contexto e problema

As duas telas administrativas da loja moram hoje na raiz do site, lado a lado
com as telas de cliente: o cadastro de produto e a gestão de administradores
respondem em endereços que não anunciam serem restritos. Nada quebra por causa
disso hoje, mas o problema aparece agora: a próxima entrega é o catálogo que o
cliente navega, e o nome natural dele — "catálogo" — já está ocupado pela tela
em que o administrador cadastra produto, herdando a decisão da `010`.

O termo tem dono errado. A `000-baseline` usa "catálogo" para a coleção que o
cliente percorre, não para o formulário que a alimenta. E como a tela
administrativa é restrita por papel na classe inteira, ela não pode simplesmente
ganhar uma página pública ao lado.

## 2. Objetivo

Reunir as telas administrativas sob um prefixo de endereço próprio, liberando
para o cliente os nomes que descrevem o que ele vê.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma mudança perceptível |
| Cliente autenticado | Nenhuma mudança perceptível |
| Administrador da loja | Os endereços das duas telas administrativas mudam; quem tinha um salvo precisa do novo. O que as telas fazem é idêntico |
| Desenvolvedor do TCC | Sabe, pelo endereço e pela pasta, se uma tela é restrita antes de abrir o arquivo |

## 4. Histórias de usuário

> **HU-01** — Como **administrador**, quero que as telas administrativas
> fiquem sob um endereço que anuncie que são restritas, para não confundi-las
> com as telas que qualquer cliente alcança.
>
> **HU-02** — Como **desenvolvedor do TCC**, quero que "catálogo" volte a
> nomear o que o cliente navega, para que a próxima entrega possa usar o termo
> certo sem colidir com a tela de cadastro.

## 5. Requisitos de qualidade interna

- **RQ-01** *(Princípio IV)* — Toda tela restrita a administrador DEVE viver
  sob um prefixo de endereço comum que a identifique como administrativa.
- **RQ-02** *(Princípio IV)* — As duas telas administrativas existentes
  (cadastro de produto e gestão de administradores) DEVEM ficar no mesmo lugar;
  deixar uma dentro e outra fora do prefixo é o defeito que esta feature existe
  para evitar.
- **RQ-03** *(Princípio VII)* — Nenhuma garantia de autorização DEVE ser
  perdida na mudança: as telas continuam exigindo o papel de administrador,
  pelo mesmo mecanismo.
- **RQ-04** *(Princípio IV)* — Cada tela administrativa DEVE ser nomeada pelo
  que ela gerencia. A tela de cadastro de produto gerencia **produto**, não
  catálogo — "catálogo" é a coleção que o cliente percorre, e precisa ficar
  livre para ela.

## 6. Requisitos funcionais

- **RF-01** — O sistema DEVE responder "não encontrado" nos endereços antigos
  das duas telas administrativas.
- **RF-02** — O sistema DEVE preservar, nos endereços novos, o comportamento
  atual de cada tela: o que ela mostra, o que ela grava e as mensagens que
  exibe.
- **RF-03** — O sistema DEVE continuar levando ao login quem não está
  autenticado e negando acesso a quem está autenticado sem ser administrador,
  agora nos endereços novos.
- **RF-04** — O sistema DEVE manter funcionando os atalhos que apontam para as
  telas administrativas a partir de qualquer tela do site.
- **RF-05** — O sistema NÃO DEVE quebrar nenhum link para tela de cliente
  exibido enquanto o administrador está numa tela administrativa — em especial
  os do rodapé e do cabeçalho, que aparecem nas duas áreas.

## 7. Regras de negócio

Nenhuma — esta feature não introduz nem altera regra de domínio.

## 8. Critérios de aceite

### CA-01 — Cadastro de produto no endereço novo
- **Dado** que sou administrador autenticado
- **Quando** abro o endereço novo do cadastro de produto e envio dados válidos
- **Então** o produto é cadastrado, com a mesma mensagem de confirmação de antes,
  e o endereço nomeia **produto**, não catálogo

### CA-02 — Gestão de administradores no endereço novo
- **Dado** que sou administrador autenticado
- **Quando** abro o endereço novo da gestão de administradores
- **Então** vejo a lista de administradores, e consigo cadastrar outro

### CA-03 — Endereços antigos não existem mais
- **Dado** que sou administrador autenticado
- **Quando** acesso os endereços antigos das duas telas
- **Então** recebo "não encontrado" nos dois

### CA-04 — Visitante continua sendo mandado ao login
- **Dado** que não estou autenticado
- **Quando** acesso os endereços novos das duas telas
- **Então** sou levado à tela de login, como era antes

### CA-05 — Cliente comum continua recebendo acesso negado
- **Dado** que estou autenticado como cliente comum
- **Quando** acesso os endereços novos das duas telas
- **Então** recebo acesso negado, como era antes

### CA-06 — Atalho do cabeçalho continua funcionando
- **Dado** que sou administrador autenticado, em qualquer tela do site
- **Quando** clico no atalho para a gestão de administradores no cabeçalho
- **Então** chego à tela, no endereço novo

### CA-07 — Links de cliente funcionam de dentro da área administrativa
- **Dado** que sou administrador autenticado, numa tela administrativa
- **Quando** clico em "Política de Privacidade" no rodapé
- **Então** chego à política, e não a um endereço inexistente

## 9. Fora de escopo

- **Qualquer mudança visual nas telas movidas.** O que elas mostram é idêntico.
- **Qualquer tela administrativa nova.** Listagem, edição e exclusão de produto
  seguem no backlog, sem entrar de carona.
- **Um painel ou página inicial da área administrativa.** O prefixo passa a
  existir, mas não ganha uma tela própria de entrada.
- **Rever a autorização em si.** Continua sendo por papel, pelo mesmo
  mecanismo; esta feature só muda onde a tela mora.
- **Redirecionar os endereços antigos para os novos.** Nunca foram divulgados a
  cliente nenhum — são telas restritas, acessadas por quem já vai conhecer o
  endereço novo.

## 10. Dependências

- **Depende de:** `010-organizacao-de-nomenclatura`, que renomeou o
  controlador de cadastro de produto e criou a colisão de nome que esta
  resolve.
- **Bloqueia:** `012-catalogo` — o catálogo do cliente precisa do nome
  "catálogo" livre na raiz.

## 11. Nota: esta é a segunda renomeação do mesmo arquivo

A `010` renomeou esta tela de "Admin" para "Catálogo", para resolver a
ambiguidade com a gestão de administradores. Escolheu "catálogo" por ser o termo
que a `000-baseline` usa — sem saber que a entrega seguinte seria justamente o
catálogo do cliente, que tem direito melhor ao nome.

Renomear duas vezes em duas specs seguidas parece desperdício, e seria, se
fossem duas mudanças independentes. Não são: o arquivo já vai ser movido de
qualquer jeito por RQ-01, e a `010` não tinha como saber. Corrigir agora custa o
mesmo commit; deixar para depois custa outro.

## 12. Pendências

Nenhuma.

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
