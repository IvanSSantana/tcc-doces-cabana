# Especificação — Gestão de administradores

**ID:** `005-gestao-de-administradores` · **Branch:** `005-gestao-de-administradores`
**Criada em:** 2026-08-12 · **Status:** Implementada

---

> **Nota sobre o escopo.** Esta spec nasceu como "Papéis e cadastro de
> administrador". A spec `001` já entregou a metade dos papéis: criou o papel
> `Administrador`, semeou o primeiro administrador, protegeu a área
> administrativa com `[Authorize]` e configurou a página de acesso negado. Sobra
> daqui para frente apenas a **tela de gestão** — daí o título novo.

---

## 1. Contexto e problema

Existe exatamente um administrador no sistema, e ele nasce da massa inicial com
a senha vinda de um *user secret*. Não há como criar um segundo sem alterar
código e reiniciar a aplicação, nem como saber quem tem acesso administrativo
sem abrir o banco.

Para uma loja com mais de uma pessoa cuidando do catálogo, isso não se sustenta:
ou todos dividem a mesma conta — e ninguém sabe quem cadastrou o quê — ou só uma
pessoa consegue trabalhar.

## 2. Objetivo

Permitir que um administrador veja quem mais tem acesso administrativo e crie
novos administradores pela interface.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma; a área é inacessível e o link não aparece |
| Cliente autenticado | Nenhuma; a área é inacessível e o link não aparece |
| Administrador da loja | Vê a lista de administradores e cadastra novos |

## 4. Histórias de usuário

> **HU-01** — Como **administrador**, quero cadastrar outro administrador pela
> interface, para dividir o trabalho do catálogo sem compartilhar minha senha.
>
> **HU-02** — Como **administrador**, quero ver quem tem acesso administrativo,
> para saber a quem recorrer e perceber se alguém tem acesso que não deveria.
>
> **HU-03** — Como **dono da loja**, quero que só administradores alcancem essa
> tela, para que ninguém se promova sozinho.

## 5. Requisitos funcionais

- **RF-01** — O sistema DEVE apresentar ao administrador a lista de todos os
  administradores, com nome e e-mail de cada um.
- **RF-02** — O sistema DEVE apresentar ao administrador um formulário de
  cadastro de administrador com os campos: nome, e-mail, celular, data de
  nascimento, CPF, senha e confirmação de senha.
- **RF-03** — O sistema DEVE criar a conta já com acesso administrativo, de modo
  que a pessoa cadastrada consiga entrar e usar a área administrativa
  imediatamente.
- **RF-04** — O sistema DEVE aplicar ao cadastro de administrador exatamente as
  mesmas regras e mensagens do cadastro de cliente.
- **RF-05** — O sistema DEVE recusar o cadastro quando o e-mail ou o CPF já
  pertencerem a alguma conta, seja ela de cliente ou de administrador.
- **RF-06** — O sistema DEVE exibir mensagem de confirmação e apresentar a lista
  atualizada após um cadastro bem-sucedido.
- **RF-07** — O sistema NÃO DEVE gravar nada quando qualquer campo estiver
  inválido, e DEVE reapresentar o formulário com os valores digitados e a
  mensagem de erro junto ao campo.
- **RF-08** — O sistema DEVE recusar o acesso a ambas as telas para quem não for
  administrador autenticado.
- **RF-09** — O sistema NÃO DEVE exibir o caminho para a gestão de
  administradores a quem não for administrador.

## 6. Regras de negócio

- **RN-01** — Um administrador é um usuário como qualquer outro, com os mesmos
  dados obrigatórios; o que o distingue é ter acesso administrativo.
- **RN-02** — As regras de nome, CPF, celular, data de nascimento e senha são as
  mesmas do cadastro de cliente. Não existe regra afrouxada para administrador.
- **RN-03** — E-mail e CPF são únicos em todo o sistema, não apenas entre
  administradores.
- **RN-04** — A lista mostra todos os que têm acesso administrativo, incluindo o
  administrador criado pela massa inicial e o próprio usuário autenticado.
- **RN-05** — Se a concessão do acesso administrativo falhar depois de a conta
  ter sido criada, nada permanece — não fica conta pela metade.

## 7. Critérios de aceite

### CA-01 — Listar administradores
- **Dado** que estou autenticado como administrador
- **Quando** abro a gestão de administradores
- **Então** vejo a lista com nome e e-mail de cada administrador, inclusive o meu

### CA-02 — Cadastro bem-sucedido
- **Dado** que estou na tela de cadastro de administrador
- **Quando** preencho dados válidos e envio
- **Então** vejo a mensagem de confirmação, e o novo administrador aparece na
  lista

### CA-03 — O administrador novo consegue trabalhar
- **Dado** que acabei de cadastrar um administrador
- **Quando** ele entra com o e-mail e a senha informados
- **Então** ele acessa a área administrativa sem receber negação de acesso

### CA-04 — E-mail já usado
- **Dado** que já existe uma conta com o e-mail informado
- **Quando** envio o formulário
- **Então** vejo "Os dados informados já estão associados a uma conta
  existente." e nada é gravado

### CA-05 — CPF já usado
- **Dado** que já existe uma conta com o CPF informado, ainda que com outro
  e-mail
- **Quando** envio o formulário
- **Então** o cadastro é recusado e **nenhuma** credencial nova permanece: o
  e-mail dessa tentativa não entra no sistema

### CA-06 — Senha fraca
- **Dado** que estou no formulário
- **Quando** informo a senha "senha123"
- **Então** vejo a mensagem sobre letra maiúscula abaixo do campo Senha e nada é
  gravado

### CA-07 — Visitante bloqueado
- **Dado** que não estou autenticado
- **Quando** acesso a gestão de administradores
- **Então** sou levado à tela de login e não vejo a lista

### CA-08 — Cliente comum bloqueado
- **Dado** que estou autenticado como cliente comum
- **Quando** acesso a gestão de administradores
- **Então** recebo negação de acesso

### CA-09 — Caminho escondido de quem não é administrador
- **Dado** que estou autenticado como cliente comum
- **Quando** olho o cabeçalho do site
- **Então** não vejo nenhum caminho para a gestão de administradores

## 8. Fora de escopo

- **Revogar acesso administrativo.** Decisão registrada: a tela entrega
  cadastrar e listar. Revogar exige definir quem pode revogar quem, e proteger
  contra o caso em que a loja fica sem nenhum administrador capaz de entrar —
  regra que ainda não existe. Vira spec própria.
- **Editar dados de outro administrador**, inclusive redefinir a senha dele.
  Cada um redefine a própria senha pelo fluxo que já existe.
- **Papéis além de `Administrador`.** O sistema tem dois níveis de acesso:
  administrador e todo o resto. Um papel intermediário — "estoquista", por
  exemplo — é feature própria.
- **Registro de quem cadastrou quem.** Não há auditoria nesta entrega.
- **Promover um cliente já cadastrado a administrador.** O formulário cria conta
  nova; converter uma existente pertence à mesma spec futura de revogação.

## 9. Dependências

- **Depende de:** `001-cadastro-produto-admin` (implementada — criou o papel, o
  primeiro administrador e a página de acesso negado) e
  `004-separar-pessoa-de-credencial`, porque cadastrar administrador cria as
  duas metades de um usuário e reaproveita a compensação que a `004` introduz.
- **Bloqueia:** nada. É folha.

## 10. Pendências

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
