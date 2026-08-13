# Especificação — Separar pessoa de credencial

**ID:** `004-separar-pessoa-de-credencial` · **Branch:** `004-separar-pessoa-de-credencial`
**Criada em:** 2026-08-12 · **Status:** Implementada

---

> **Nota sobre o formato.** Como nas specs `002` e `003`, esta feature entrega
> pouco de visível ao usuário — ela move dado de negócio para o lugar certo. A
> seção 5 registra o que o usuário observa, que aqui é sobretudo **não
> regressão**. A seção 6 registra os requisitos de qualidade interna, que são o
> motivo da feature existir.

---

## 1. Contexto e problema

A classe `Usuario` herda de `IdentityUser<Guid>` e por isso vive em
`DocesCabana.Infrastructure`. Só que ela não carrega apenas credencial: carrega
nome, CPF e data de nascimento — dado de negócio, que pertence ao domínio.

Essa mistura tem um custo concreto e já medido. Quatro entidades criadas pela
spec `003` — `Endereco`, `Favorito`, `Avaliacao` e `Pedido` — referenciam
usuário por um `Guid` solto, **sem propriedade de navegação**, porque navegar até
uma classe da infraestrutura faria o domínio depender dela e derrubaria o
Princípio I. A `003` registrou isso como a RQ-02 e o marcou como limitação
imposta, não escolhida.

O mesmo emaranhado é o que sustenta a única exceção que a constituição abre ao
Princípio I: `IUsuarioService` vive na infraestrutura, e os controllers dependem
dela diretamente.

## 2. Objetivo

Separar o que é pessoa do que é credencial, para que as entidades de domínio
possam referenciar o usuário como referenciam qualquer outra entidade.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma mudança perceptível |
| Cliente autenticado | Nenhuma mudança perceptível: cria conta, entra por e-mail ou CPF e recupera senha exatamente como antes |
| Administrador da loja | Nenhuma mudança perceptível |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero que criar conta, entrar e recuperar senha
> continuem funcionando como antes, porque essa mudança não é sobre mim.
>
> **HU-02** — Como **desenvolvedor do TCC**, quero que uma entidade de domínio
> possa navegar até o usuário dono dela, para escrever consulta de endereço, de
> favorito e de pedido sem juntar tabelas na mão.
>
> **HU-03** — Como **desenvolvedor do TCC**, quero que a exceção que a
> constituição abre ao Princípio I fique restrita ao que realmente exige
> infraestrutura, para que ela não sirva de precedente para o que não exige.

## 5. Requisitos funcionais

*Esta feature é uma refatoração. O que segue é garantia de não regressão.*

- **RF-01** — O sistema DEVE continuar criando conta com nome, e-mail, celular,
  data de nascimento, CPF e senha, com as mesmas validações e as mesmas
  mensagens de erro.
- **RF-02** — O sistema DEVE continuar autenticando por e-mail e por CPF, com e
  sem pontuação.
- **RF-03** — O sistema DEVE continuar recusando cadastro cujo e-mail ou CPF já
  pertença a uma conta, com a mesma mensagem.
- **RF-04** — O sistema NÃO DEVE deixar credencial sem dados de pessoa nem
  dados de pessoa sem credencial: se qualquer metade do cadastro falhar, nenhuma
  das duas persiste.
- **RF-05** — O sistema DEVE continuar permitindo redefinição de senha por
  e-mail, com o mesmo fluxo.

## 6. Requisitos de qualidade interna

- **RQ-01** *(Princípio I)* — O dado de negócio do usuário — nome, CPF, celular
  e data de nascimento — DEVE viver em uma entidade do domínio.
- **RQ-02** *(Princípio I)* — A classe que herda do ASP.NET Identity DEVE
  guardar apenas credencial e o que o framework impõe. NÃO DEVE guardar dado de
  negócio.
- **RQ-03** *(Princípio IV)* — O domínio DEVE ficar com o termo do negócio,
  `Usuario`, que é como a modelagem do TCC nomeia o conceito. A classe do
  Identity recebe nome técnico, `ContaDeAcesso`.
- **RQ-04** — `Endereco`, `Favorito`, `Avaliacao` e `Pedido` DEVEM passar a
  referenciar o usuário por propriedade de navegação, encerrando a limitação
  registrada como RQ-02 na spec `003`.
- **RQ-05** *(Princípio I)* — A exceção documentada no Princípio I DEVE ser
  reescrita: o motivo deixa de ser a entidade e passa a ser a dependência de
  `UserManager` e `SignInManager`, que é o que de fato exige infraestrutura.
- **RQ-06** *(Princípio VI)* — A mudança de esquema DEVE vir em uma única
  migration versionada.
- **RQ-07** — A propriedade `PhoneNumber`, herdada do Identity, DEVE deixar de
  ser usada: o celular passa a ser responsabilidade do domínio, e manter os dois
  criaria duas fontes de verdade.
- **RQ-08** *(Princípio V)* — As invariantes que hoje moram na classe do
  Identity DEVEM continuar cobertas por teste depois de mudarem de casa, e a
  navegação nova DEVE ter teste de integração.

## 7. Regras de negócio

### Usuário (domínio)

- **RN-01** — Nome é obrigatório.
- **RN-02** — CPF é obrigatório e válido por dígito verificador, armazenado só
  com dígitos.
- **RN-03** — Celular é obrigatório e válido no formato brasileiro, armazenado
  só com dígitos.
- **RN-04** — Data de nascimento não é futura e não é anterior a 120 anos atrás.
- **RN-05** — Não existem dois usuários com o mesmo CPF.

### Conta de acesso (credencial)

- **RN-06** — E-mail é obrigatório e válido; é ele que identifica a conta no
  login.
- **RN-07** — Toda conta de acesso corresponde a exatamente um usuário, e todo
  usuário a exatamente uma conta. As duas metades compartilham o mesmo
  identificador.

### Criação

- **RN-08** — O cadastro cria as duas metades. Se a segunda falhar, a primeira é
  desfeita — não fica conta órfã.

## 8. Critérios de aceite

### CA-01 — Cadastro continua funcionando
- **Dado** que estou na tela de criar conta
- **Quando** preencho dados válidos e envio
- **Então** a conta é criada e sou levado à tela de login

### CA-02 — Login por e-mail continua funcionando
- **Dado** que tenho conta
- **Quando** entro informando o e-mail e a senha
- **Então** entro no sistema

### CA-03 — Login por CPF continua funcionando
- **Dado** que tenho conta com o CPF 529.982.247-25
- **Quando** entro informando `52998224725` ou `529.982.247-25` e a senha
- **Então** entro no sistema

### CA-04 — Cadastro com CPF repetido não deixa conta órfã
- **Dado** que já existe uma conta com o CPF 529.982.247-25
- **Quando** tento criar outra conta, com e-mail diferente mas o mesmo CPF
- **Então** o cadastro é recusado, e **nenhuma** credencial nova permanece no
  sistema — tentar entrar com o e-mail dessa tentativa não encontra conta

### CA-05 — Entidade de domínio navega até o usuário
- **Dado** um endereço gravado para um usuário
- **Quando** consulto o endereço pedindo o usuário junto
- **Então** recebo o nome do usuário sem precisar de uma segunda consulta

### CA-06 — Recuperação de senha continua funcionando
- **Dado** que tenho conta
- **Quando** solicito redefinição de senha e uso o link recebido
- **Então** consigo definir uma senha nova e entrar com ela

### CA-07 — Suíte verde
- **Dado** o final desta feature
- **Quando** executo a suíte de testes
- **Então** todos passam e o total é maior ou igual aos 233 de hoje

## 9. Fora de escopo

- **Mover `IUsuarioService` para a `Application`.** A implementação depende de
  `UserManager` e `SignInManager`, que são infraestrutura. Esta feature **narra**
  corretamente a exceção do Princípio I, mas não a elimina. Eliminá-la exigiria
  uma abstração de autenticação própria — spec futura, se algum dia valer.
- **Alterar e-mail de uma conta existente.** Nenhuma tela faz isso hoje.
- **Migração de dados.** O banco de desenvolvimento é descartável e é recriado,
  como já foi feito na spec `003`. Não há dado de produção.
- **Gestão de administradores** — spec `005`.
- **Qualquer mudança visual.** Nenhuma tela é redesenhada.

## 10. Dependências

- **Depende de:** `003-modelo-de-dados-completo` (implementada) — é ela que cria
  as quatro entidades cuja limitação de navegação esta spec remove.
- **Bloqueia:** `005-gestao-de-administradores`, que cadastra administrador e
  portanto cria as duas metades.

## 11. Pendências

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
- [x] Nada aqui conflita com `.specify/memory/constitution.md` — a RQ-05 reescreve
      a exceção do Princípio I e vira emenda registrada
