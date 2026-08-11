# Especificação — Revisão técnica da base

**ID:** `002-revisao-tecnica` · **Branch:** `002-revisao-tecnica`
**Criada em:** 2026-08-11 · **Status:** Rascunho

---

> **Nota sobre o formato.** O template proíbe detalhe de implementação porque a
> spec normalmente descreve comportamento novo para o usuário. Esta feature é
> diferente: ela corrige defeitos e paga dívida técnica. As seções 5 e 7 seguem a
> regra normalmente — descrevem o que o usuário observa. A seção 6 é um acréscimo
> a este template: registra requisitos de **qualidade interna**, que não têm
> manifestação visível mas são o motivo da feature existir. Sem essa seção, metade
> do trabalho ficaria sem requisito rastreável.

---

## 1. Contexto e problema

A base cresceu por seis meses guiada pela funcionalidade, e a constituição do
projeto só foi escrita em 2026-08-07 — depois do código. O resultado é uma
distância medida entre o que os princípios exigem e o que existe: entrar com CPF
não funciona apesar de estar documentado como pronto, o bloqueio por tentativas
falhas nunca dispara, e o cadastro de produto perde o status escolhido. Somam-se
duas convenções de nome de teste convivendo, arquivos cujo nome não bate com o
tipo que declaram, e uma abstração de transação que nenhum caso de uso chama.

Corrigir isso agora custa pouco: são cinco projetos e cerca de quarenta arquivos
de código. Cada feature nova do backlog (003 a 011) construída sobre a base atual
multiplica o custo e propaga os mesmos vícios.

## 2. Objetivo

Alinhar a base existente à constituição do projeto, corrigindo os defeitos que a
baseline documenta como funcionalidade pronta e eliminando a inconsistência que
encarece toda feature futura.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma mudança perceptível na navegação da vitrine |
| Cliente autenticado | Passa a conseguir entrar com CPF; passa a ter a conta protegida por bloqueio após tentativas falhas; recebe a mensagem de recuperação de senha como confirmação, não como erro |
| Administrador da loja | O status escolhido no cadastro de produto passa a ser respeitado |

## 4. Histórias de usuário

> **HU-01** — Como **cliente**, quero entrar com meu CPF, para não precisar
> lembrar qual e-mail usei no cadastro.
>
> **HU-02** — Como **cliente**, quero que minha conta trave temporariamente após
> várias senhas erradas, para que ninguém consiga adivinhar minha senha tentando.
>
> **HU-03** — Como **cliente**, quero que a tela de recuperação de senha me diga
> que o e-mail foi enviado, e não me mostre isso com cara de erro, para eu saber
> que deu certo.
>
> **HU-04** — Como **administrador**, quero que o status que escolhi ao cadastrar
> um produto seja o status que ele recebe, para não precisar corrigir depois.
>
> **HU-05** — Como **desenvolvedor do TCC**, quero clonar o repositório em outro
> computador e conseguir rodar a aplicação sem receber um arquivo de configuração
> por fora, para trabalhar em mais de uma máquina.
>
> **HU-06** — Como **desenvolvedor do TCC**, quero uma base que siga uma
> convenção só, para que a próxima feature não gaste tempo decidindo qual dos dois
> padrões existentes seguir.

## 5. Requisitos funcionais

- **RF-01** — O sistema DEVE autenticar o usuário que informa o CPF como login,
  com ou sem pontuação, da mesma forma que autentica quem informa o e-mail.
- **RF-02** — O sistema DEVE bloquear temporariamente a conta após um número
  definido de tentativas de senha malsucedidas, e DEVE informar ao usuário que a
  conta está bloqueada quando ele tentar entrar durante o bloqueio.
- **RF-03** — O sistema DEVE gravar o produto com o status escolhido no
  formulário, e DEVE atribuir o status *Ativo* somente quando nenhum for escolhido.
- **RF-04** — O sistema DEVE apresentar a mensagem de recuperação de senha como
  confirmação, visualmente distinta de uma mensagem de erro.
- **RF-05** — O sistema NÃO DEVE enviar e-mail de recuperação nem consultar conta
  alguma quando o login informado estiver em formato inválido; DEVE recusar o
  envio com a mensagem de erro no campo.
- **RF-06** — O sistema DEVE manter a mensagem de recuperação de senha idêntica
  para login existente e inexistente.
- **RF-07** — O sistema NÃO DEVE deixar um objeto de domínio parcialmente
  preenchido quando a validação do construtor recusar os dados.

## 6. Requisitos de qualidade interna

*Não observáveis pelo usuário final, verificáveis por inspeção ou por teste
automatizado. Cada um cita o princípio da constituição que o motiva.*

- **RQ-01** *(Princípio I)* — Nenhum projeto DEVE declarar dependência de pacote
  que não utiliza, e o projeto de domínio NÃO DEVE depender de nenhum pacote além
  da biblioteca base da plataforma.
- **RQ-02** *(Princípio VI)* — A abstração de transação explícita DEVE ser
  removida. A unidade de trabalho DEVE expor apenas a gravação das alterações
  pendentes, que já é atômica por si.
- **RQ-03** *(Princípio IV)* — O nome de todo arquivo DEVE coincidir com o nome do
  tipo que ele declara, e a pasta de todo arquivo DEVE coincidir com seu namespace.
- **RQ-04** *(Princípio IV)* — Um mesmo conceito de negócio DEVE ter um único nome
  em toda a base; hoje o telefone do cliente aparece com três nomes diferentes.
- **RQ-05** *(Princípio V)* — Todo teste automatizado DEVE seguir a nomenclatura
  `Dado_/Quando_/Entao_`. Cerca de quarenta testes hoje usam outro formato.
- **RQ-06** *(Princípio III)* — O cadastro de produto DEVE ter validação de
  entrada, para que o erro chegue ao administrador junto ao campo em vez de virar
  tela de exceção. *(resolve a dívida D-06 da baseline)*
- **RQ-07** *(Princípio VII)* — Credenciais NÃO DEVEM estar sob controle de
  versão, e ainda assim quem clona o repositório DEVE conseguir subir a aplicação
  sem receber arquivo algum por fora.
- **RQ-08** — As configurações de persistência NÃO DEVEM conter sintaxe que quebre
  no banco de dados alvo do deploy, para que a troca planejada de SQLite para SQL
  Server custe uma linha e a regeração das migrations.
- **RQ-09** — O projeto NÃO DEVE acusar aviso de vulnerabilidade conhecida ao
  compilar.
- **RQ-10** — A spec `000-baseline` DEVE descrever o sistema como ele é: hoje ela
  afirma um banco de dados que o código não usa e lista como abertas dívidas que
  esta feature fecha.
- **RQ-11** — Toda regra de negócio e todo auxiliar de validação DEVE ter teste
  automatizado. Hoje não têm: o cadastro de produto no serviço, a tradução entre
  entidade e DTO, e os auxiliares de CPF e de telefone.

## 7. Regras de negócio

- **RN-01** — Um login é aceito como CPF quando, descartada a pontuação, tem onze
  dígitos e dígito verificador válido; caso contrário é tratado como e-mail.
- **RN-02** — A conta bloqueia após 5 tentativas malsucedidas e permanece
  bloqueada por 15 minutos.
- **RN-03** — Um produto cadastrado sem escolha explícita de status nasce *Ativo*.
- **RN-04** — Um objeto de domínio ou existe válido ou não existe: a validação
  precede qualquer atribuição no construtor.
- **RN-05** — A mensagem de recuperação de senha é "Se existir uma conta com esse
  login, enviamos um e-mail com o link de redefinição." — idêntica nos dois casos.

## 8. Critérios de aceite

### CA-01 — Entrar com CPF sem pontuação
- **Dado** que tenho conta com o CPF 529.982.247-25 e senha correta
- **Quando** informo `52998224725` e minha senha na tela de login
- **Então** entro no sistema e sou levado à página inicial

### CA-02 — Entrar com CPF pontuado
- **Dado** que tenho conta com o CPF 529.982.247-25 e senha correta
- **Quando** informo `529.982.247-25` e minha senha na tela de login
- **Então** entro no sistema e sou levado à página inicial

### CA-03 — Entrar com e-mail continua funcionando
- **Dado** que tenho conta com e-mail e senha correta
- **Quando** informo o e-mail e a senha
- **Então** entro no sistema e sou levado à página inicial

### CA-04 — Bloqueio por tentativas
- **Dado** que tenho conta ativa
- **Quando** erro a senha cinco vezes seguidas e tento uma sexta, agora com a
  senha correta
- **Então** vejo "Conta bloqueada. Tente novamente mais tarde." e não entro

### CA-05 — Produto cadastrado como Inativo permanece Inativo
- **Dado** que estou no formulário de cadastro de produto
- **Quando** preencho dados válidos, escolho o status *Inativo* e envio
- **Então** o produto é gravado com status *Inativo* e não aparece na vitrine

### CA-06 — Produto sem escolha de status nasce Ativo
- **Dado** que estou no formulário de cadastro de produto
- **Quando** preencho dados válidos sem escolher status e envio
- **Então** o produto é gravado com status *Ativo*

### CA-07 — Recuperação de senha com login existente
- **Dado** que existe conta com o login informado
- **Quando** solicito a redefinição de senha
- **Então** vejo "Se existir uma conta com esse login, enviamos um e-mail com o
  link de redefinição." apresentada como confirmação, e recebo o e-mail

### CA-08 — Recuperação de senha com login inexistente
- **Dado** que não existe conta com o login informado
- **Quando** solicito a redefinição de senha
- **Então** vejo exatamente a mesma mensagem de CA-07, apresentada da mesma
  forma, e nenhum e-mail é enviado

### CA-09 — Recuperação de senha com login malformado
- **Dado** que estou na tela de recuperação de senha
- **Quando** envio o login `abc`
- **Então** vejo "O formato do login deve ser um e-mail ou um CPF válido." junto
  ao campo, nenhuma conta é consultada e nenhum e-mail é enviado

### CA-10 — Objeto de domínio não fica meio construído
- **Dado** um cadastro de usuário com nome válido e celular inválido
- **Quando** a criação do usuário é recusada
- **Então** nenhuma propriedade do usuário foi atribuída antes da recusa

### CA-11 — Clone em máquina nova
- **Dado** um clone limpo do repositório em outro computador
- **Quando** sigo as instruções do README e executo a aplicação
- **Então** a aplicação sobe, sem que nenhum arquivo de configuração tenha me
  sido enviado por fora do repositório

### CA-12 — Suíte de testes
- **Dado** a base ao final desta feature
- **Quando** executo a suíte de testes
- **Então** todos passam, o total é maior que os 99 de hoje, e nenhum teste usa
  nomenclatura fora do padrão `Dado_/Quando_/Entao_`

### CA-13 — Compilação limpa
- **Dado** a base ao final desta feature
- **Quando** compilo a solução
- **Então** não há aviso de vulnerabilidade conhecida em pacote

## 9. Fora de escopo

- **As dívidas D-01 a D-05 da baseline.** São o caminho de gravação do cadastro de
  produto e pertencem à spec `001-cadastro-produto-admin`. Esta feature prepara o
  terreno para elas (RQ-02, RQ-06 e RF-03), mas não fecha o cadastro.
- **A dívida D-07** (`Endereco` modelado sem entidade). Vira feature própria no
  backlog; aqui apenas permanece registrada.
- **Troca do banco para SQL Server.** Esta feature torna a troca barata (RQ-08),
  mas não a executa — ela acontece na etapa de deploy.
- **Paginação da vitrine.** A página inicial carrega o catálogo inteiro. Com o
  volume atual não dói; vira spec própria quando o catálogo crescer.
- **Confirmação de e-mail no cadastro.** Existe método de serviço para isso sem
  nenhuma tela que o chame. Fica como está, registrado no backlog.
- **Reescrita do filtro global de exceção.** Ele muda dentro da spec `001`, que é
  quem exercita o caminho de erro do cadastro de produto.
- **Qualquer mudança visual.** Nenhuma tela é redesenhada.

## 10. Dependências

- **Depende de:** `000-baseline` (é a base que está sendo revisada).
- **Bloqueia:** `001-cadastro-produto-admin` e, por consequência, todo o backlog
  de 003 a 011.

**Ordem de execução:** esta feature vem **antes** da `001`. A `001` grava produto
através da unidade de trabalho que a RQ-02 simplifica, precisa da validação de
entrada que a RQ-06 cria, e depende do status que a RF-03 conserta. Executá-la
primeiro significa escrevê-la sobre uma base que muda embaixo.

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
- [x] Nada aqui conflita com `.specify/memory/constitution.md` — a RQ-02 muda o
      texto do Princípio VI e a RQ-03 acrescenta uma regra ao Princípio IV; ambas
      viram emenda registrada, conforme a Governança item 3
