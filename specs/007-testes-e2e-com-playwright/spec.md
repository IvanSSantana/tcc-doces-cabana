# Especificação — Testes E2E com Playwright

**ID:** `007-testes-e2e-com-playwright` · **Branch:** `007-testes-e2e-com-playwright`
**Criada em:** 2026-08-13 · **Status:** Rascunho

---

> **Nota sobre o nome.** O nome da ferramenta aparece no título porque foi assim
> que a feature nasceu no backlog, mas os requisitos abaixo não a citam: eles
> descrevem a garantia, não o meio. A escolha técnica está no plano.
>
> **Nota de governança.** Esta feature exige emenda constitucional. O Princípio V
> hoje diz *"Ferramentas fixas: xUnit + Moq + coverlet. Não introduzir framework
> de teste novo."* — e é exatamente o que esta spec pede. A emenda (1.2.0 →
> 1.3.0) está no plano §9 e nas tarefas; não é para ser feita em silêncio.

---

## 1. Contexto e problema

Da `002` à `006`, toda confirmação de que os fluxos funcionam de ponta a ponta
foi feita à mão: subir a aplicação, preencher formulário, ler a tela, conferir o
banco. Isso encontrou defeitos reais — a `006` inteira nasceu de um deles, uma
mensagem de erro trocada que nenhum teste de unidade via.

O problema é que essa conferência não fica. Na entrega seguinte tudo é refeito
do zero, com o roteiro na cabeça de quem está conferindo; o que passou
despercebido uma vez passa de novo. Pior: a auditoria da `005` mostrou que dá
para percorrer um fluxo, ver a tela errada e ainda assim registrar o critério
como aprovado — porque conferir na mão cansa e a atenção é finita.

Os testes que existem hoje enxergam as peças isoladas: o serviço com o
repositório fingido, o controller com o serviço fingido. Nenhum deles atravessa
o navegador, o formulário, o cookie de autenticação e o banco de uma vez — que é
onde os últimos dois defeitos reais moraram.

## 2. Objetivo

Ter um roteiro automatizado que percorre os caminhos reais do usuário num
navegador de verdade, e que qualquer pessoa roda com um comando.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Quem desenvolve e quem avalia o TCC | Beneficiário direto: roda o roteiro e sabe, em minutos, se algum fluxo quebrou |
| Cliente (visitante) | Indiretamente: uma correção numa tela deixa de quebrar outra em silêncio |
| Cliente autenticado | Indiretamente, idem |
| Administrador da loja | Indiretamente, idem |

## 4. Histórias de usuário

> **HU-01** — Como **quem desenvolve**, quero que os caminhos que hoje confiro
> na mão sejam conferidos sozinhos, para que uma regressão apareça antes da
> entrega e não depois dela.
>
> **HU-02** — Como **quem avalia o TCC**, quero ver que o sistema é verificado
> pelos caminhos reais do usuário, e não só por testes que enxergam as peças
> separadas.
>
> **HU-03** — Como **cliente**, quero que um ajuste numa tela não quebre outra
> sem ninguém perceber.

## 5. Requisitos funcionais

- **RF-01** — O sistema DEVE ter um roteiro automatizado que percorre, num
  navegador real, os fluxos que já existem: cadastro de cliente, login e saída,
  recuperação de senha, cadastro de produto e gestão de administradores.
- **RF-02** — O roteiro DEVE exercitar cada fluxo pela interface, como uma
  pessoa faria: preenchendo campos, clicando e lendo o que aparece na tela.
- **RF-03** — O roteiro DEVE cobrir os caminhos de erro de cada fluxo, não só o
  caminho feliz.
- **RF-04** — O roteiro DEVE rodar contra uma base de dados própria e
  descartável, sem tocar a base usada no dia a dia do desenvolvimento.
- **RF-05** — O roteiro DEVE ser executável por um único comando, sem passo
  manual de preparação além da instalação inicial, que DEVE estar documentada.
- **RF-06** — O roteiro NÃO DEVE depender de serviço externo para passar — em
  particular, não DEVE depender de envio real de e-mail.
- **RF-07** — O roteiro DEVE concluir a redefinição de senha de ponta a ponta:
  do pedido, passando pelo link recebido, até entrar com a senha nova.
- **RF-08** — Quando um teste falhar, o roteiro DEVE deixar registrado o que se
  esperava, o que apareceu e em que passo parou, sem exigir que alguém reproduza
  na mão para descobrir.
- **RF-09** — O roteiro NÃO DEVE rodar junto com a suíte rápida de unidade, para
  não alongar o ciclo de desenvolvimento.

## 6. Regras de negócio

- **RN-01** — Cada execução parte de um estado conhecido: a mesma massa inicial
  de catálogo e as mesmas credenciais administrativas.
- **RN-02** — Nenhum teste depende de outro ter rodado antes, nem da ordem em
  que rodam.
- **RN-03** — Dados criados durante a execução não colidem entre si. E-mail e
  CPF são únicos no sistema (RN-01 da `006`), então cada teste que cadastra
  alguém usa dados próprios.
- **RN-04** — Nenhuma credencial real — de e-mail, de banco ou de
  administrador — entra no roteiro ou no repositório.
- **RN-05** — Rodar o roteiro duas vezes seguidas dá o mesmo resultado. Uma
  execução não deixa resíduo que atrapalhe a próxima.

## 7. Critérios de aceite

### CA-01 — Cadastro de cliente bem-sucedido
- **Dado** que estou na tela de cadastro com dados que ninguém usa
- **Quando** preencho tudo e envio
- **Então** sou levado ao login e consigo entrar com o que acabei de cadastrar

### CA-02 — Cadastro recusado por dado repetido
- **Dado** que já existe conta com o e-mail (ou com o CPF) que vou informar
- **Quando** envio o formulário, seja o de cliente, seja o de administrador
- **Então** vejo *"Os dados informados já estão associados a uma conta
  existente."* e nada é criado

### CA-03 — Cadastro recusado por senha fraca
- **Dado** que estou na tela de cadastro
- **Quando** informo a senha "senha123"
- **Então** vejo a mensagem sobre letra maiúscula abaixo do campo Senha

### CA-04 — Login pelos dois caminhos
- **Dado** que tenho uma conta
- **Quando** entro com o e-mail, e depois com o CPF
- **Então** os dois funcionam e chego à página inicial autenticado

### CA-05 — Login com senha errada
- **Dado** que tenho uma conta
- **Quando** entro com a senha errada
- **Então** vejo *"E-mail ou senha incorreto(s)."* e continuo fora

### CA-06 — Sair
- **Dado** que estou autenticado
- **Quando** saio
- **Então** volto à condição de visitante e a área administrativa deixa de
  estar acessível

### CA-07 — Recuperação não revela quem tem conta
- **Dado** que estou na tela de recuperação de senha
- **Quando** informo um login que existe, e depois um que não existe
- **Então** vejo exatamente a mesma mensagem nas duas vezes

### CA-08 — Redefinição de senha de ponta a ponta
- **Dado** que pedi a redefinição para uma conta que existe
- **Quando** abro o link que foi enviado e informo uma senha nova
- **Então** consigo entrar com a senha nova, e não consigo mais com a antiga

### CA-09 — Cadastro de produto pelo administrador
- **Dado** que estou autenticado como administrador
- **Quando** cadastro um produto com dados válidos
- **Então** vejo a confirmação; e, com um campo inválido, vejo o erro no campo
  e nada é cadastrado

### CA-10 — Área administrativa fechada
- **Dado** que sou visitante, e depois cliente comum
- **Quando** tento abrir o cadastro de produto e a gestão de administradores
- **Então** o visitante vai para o login e o cliente comum recebe acesso negado

### CA-11 — Gestão de administradores
- **Dado** que estou autenticado como administrador
- **Quando** abro a gestão, vejo a lista e cadastro um administrador novo
- **Então** ele aparece na lista, e consegue entrar e usar a área
  administrativa

### CA-12 — Caminho administrativo escondido
- **Dado** que estou autenticado como cliente comum
- **Quando** olho o cabeçalho
- **Então** não vejo caminho para a gestão de administradores

### CA-13 — Execução repetível
- **Dado** que o roteiro acabou de rodar inteiro
- **Quando** rodo de novo, sem limpar nada à mão
- **Então** o resultado é o mesmo

## 8. Fora de escopo

- **Mais de um navegador.** Um só basta para provar que os fluxos funcionam;
  cobrir três triplica o tempo de execução em troca de pouco, já que o sistema
  não usa recurso de navegador específico.
- **Teste de carga ou de desempenho.** Outro tipo de ferramenta e outra
  pergunta.
- **Acessibilidade automatizada e comparação visual de telas.** Valiosos, mas
  são features próprias — e comparação visual costuma falhar por diferença de
  fonte entre máquinas, o que traria ruído em vez de sinal.
- **Fluxos que ainda não existem** — carrinho, favoritos, pedido, pagamento,
  avaliação. Não há o que percorrer.
- **Rodar em integração contínua.** O projeto não tem pipeline; montar um é
  feature própria. O roteiro fica preparado para rodar por comando, que é o que
  um pipeline precisaria chamar.
- **Substituir os testes de unidade.** Eles continuam sendo a rede rápida; o
  roteiro é a rede lenta e larga.

## 9. Dependências

- **Depende de:** `001` a `006`, todas implementadas. O roteiro percorre
  exatamente o que elas entregaram — inclusive a mensagem única de duplicidade
  que a `006` acabou de unificar.
- **Bloqueia:** nada. É folha.

## 10. Pendências

Nenhuma.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — o nome da ferramenta aparece só no título e na nota de
      origem, herdado do backlog
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]`
- [ ] Nada aqui conflita com `.specify/memory/constitution.md` — **conflita**:
      Princípio V proíbe framework de teste novo. Resolvido por emenda 1.3.0,
      justificada no plano §9 e executada nas tarefas
