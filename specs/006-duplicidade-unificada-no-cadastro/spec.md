# Especificação — Duplicidade unificada no cadastro

**ID:** `006-duplicidade-unificada-no-cadastro` · **Branch:** `006-duplicidade-unificada-no-cadastro`
**Criada em:** 2026-08-13 · **Status:** Rascunho

---

> **Nota sobre a origem.** Esta spec nasceu da auditoria da `005`, não de uma
> necessidade nova. A `005` entregou o cadastro de administrador com a exigência
> (RF-04) de usar "exatamente as mesmas regras e mensagens do cadastro de
> cliente". A auditoria encontrou uma divergência real e verificada ao vivo: com
> CPF repetido, um cadastro explica o problema e o outro mostra erro interno.
> Esta feature fecha essa lacuna e leva junto duas correções de documentação
> herdadas do mesmo achado — registradas nas tarefas, não como requisitos.

---

## 1. Contexto e problema

O sistema tem hoje duas portas de cadastro: a que o visitante usa para criar a
própria conta e a que o administrador usa para criar outro administrador. Cada
uma checa dado repetido de um jeito diferente, e por isso elas divergiram.

Quando o CPF informado já pertence a alguém, a primeira porta responde *"Os
dados informados já estão associados a uma conta existente."* e a segunda
responde *"Um erro interno ocorreu, tente novamente mais tarde."* — mensagem que
leva quem está cadastrando a tentar de novo, achando que o sistema falhou,
quando na verdade o dado é que precisa mudar. O cadastro é corretamente
recusado nas duas; o que quebra é a explicação.

A causa é a regra morar em dois lugares: quem cadastra cliente confere antes de
gravar, quem cadastra administrador só descobre quando a gravação bate no
limite do sistema. Enquanto forem duas cópias, elas voltam a divergir no
primeiro ajuste que alguém fizer em uma só.

## 2. Objetivo

Fazer com que qualquer cadastro do sistema recuse e-mail ou CPF já usados com a
mesma mensagem, a partir de uma única regra.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma mudança visível: já recebe a mensagem correta e continua recebendo |
| Cliente autenticado | Nenhuma |
| Administrador da loja | Passa a entender por que o cadastro de outro administrador foi recusado, em vez de ver erro interno |

## 4. Histórias de usuário

> **HU-01** — Como **administrador**, quero saber que o cadastro foi recusado
> porque o dado já pertence a alguém, para corrigir o CPF em vez de repetir a
> tentativa achando que o sistema caiu.
>
> **HU-02** — Como **dono da loja**, quero que a recusa por dado repetido seja
> a mesma em toda porta de cadastro, para que arrumar uma não deixe a outra
> para trás.

## 5. Requisitos funcionais

- **RF-01** — O sistema DEVE recusar o cadastro, em qualquer porta de cadastro,
  quando o CPF informado já pertencer a alguma conta.
- **RF-02** — O sistema DEVE recusar o cadastro, em qualquer porta de cadastro,
  quando o e-mail informado já pertencer a alguma conta.
- **RF-03** — O sistema DEVE apresentar a mensagem *"Os dados informados já
  estão associados a uma conta existente."* em qualquer porta de cadastro, tanto
  para e-mail repetido quanto para CPF repetido.
- **RF-04** — O sistema NÃO DEVE apresentar mensagem de erro interno quando a
  recusa se deve a dado repetido.
- **RF-05** — O sistema DEVE reapresentar o formulário com os valores digitados
  quando recusar o cadastro por dado repetido.
- **RF-06** — O sistema NÃO DEVE deixar credencial alguma para trás quando
  recusar um cadastro por dado repetido.

## 6. Regras de negócio

- **RN-01** — E-mail e CPF são únicos em todo o sistema, independentemente do
  tipo de conta e da porta por onde o cadastro entrou.
- **RN-02** — A recusa não revela **qual** dos dois dados está repetido. Essa
  decisão vem da `002` e continua valendo: dizer "esse CPF já existe" confirma
  a terceiros que uma pessoa tem conta na loja.
- **RN-03** — A regra vale para qualquer porta de cadastro, inclusive uma que
  ainda não exista. Uma porta nova nasce com a regra, sem precisar lembrar de
  copiá-la.

## 7. Critérios de aceite

### CA-01 — CPF repetido no cadastro de administrador
- **Dado** que estou autenticado como administrador e já existe conta com o CPF
  que vou informar
- **Quando** envio o formulário de cadastro de administrador
- **Então** vejo *"Os dados informados já estão associados a uma conta
  existente."*, o formulário volta com o que digitei, e nada é gravado

### CA-02 — E-mail repetido no cadastro de administrador
- **Dado** que estou autenticado como administrador e já existe conta com o
  e-mail que vou informar
- **Quando** envio o formulário de cadastro de administrador
- **Então** vejo a mesma mensagem de CA-01 e nada é gravado

### CA-03 — CPF repetido no cadastro de cliente (não pode regredir)
- **Dado** que já existe conta com o CPF que vou informar
- **Quando** me cadastro como cliente
- **Então** vejo a mesma mensagem de CA-01 e nada é gravado

### CA-04 — E-mail repetido no cadastro de cliente (não pode regredir)
- **Dado** que já existe conta com o e-mail que vou informar
- **Quando** me cadastro como cliente
- **Então** vejo a mesma mensagem de CA-01 e nada é gravado

### CA-05 — Nenhuma credencial órfã
- **Dado** que tentei cadastrar com CPF repetido e um e-mail que nunca foi usado
- **Quando** o cadastro é recusado
- **Então** esse e-mail continua livre: consigo usá-lo depois num cadastro com
  CPF válido

### CA-06 — Cadastro válido segue funcionando
- **Dado** que informo e-mail e CPF que ninguém usa
- **Quando** envio o formulário, em qualquer das duas portas
- **Então** a conta é criada normalmente, como antes desta feature

## 8. Fora de escopo

- **Dizer qual campo está repetido.** Seria mais fácil de usar, mas contraria a
  RN-02 e a decisão de enumeração de conta tomada na `002`. Mudar isso é uma
  decisão de segurança própria, não um detalhe desta correção.
- **Unificar as duas telas de cadastro numa só.** Elas têm layouts e destinos
  diferentes de propósito; o que esta spec unifica é a **regra**, não a tela.
- **Alterar as regras de formato** de CPF, e-mail, celular ou senha. Nada nos
  requisitos de preenchimento muda.
- **Revisar as demais mensagens de erro do sistema.** Só a de duplicidade está
  em questão.

## 9. Dependências

- **Depende de:** `005-gestao-de-administradores` (implementada) — é o cadastro
  que ela entregou que está divergindo.
- **Bloqueia:** a spec de testes ponta a ponta em Playwright. Se ela vier
  antes, os testes E2E nascem congelando a mensagem errada como se fosse a
  esperada.

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
