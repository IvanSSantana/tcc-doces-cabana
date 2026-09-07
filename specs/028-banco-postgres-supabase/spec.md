# Especificação — Banco de dados no Postgres do Supabase

**ID:** `028-banco-postgres-supabase` · **Branch:** `028-banco-postgres-supabase`
**Criada em:** 2026-09-07 · **Status:** Rascunho

---

## 1. Contexto e problema

**O sistema conta duas histórias sobre onde ele guarda as coisas.** Desde a
`027`, as imagens dos produtos vivem no armazenamento do Supabase. Os dados —
produtos, contas, pedidos — continuam num arquivo `.db` na pasta do projeto.
Quem lê a arquitetura encontra um serviço em nuvem e um arquivo local lado a
lado, sem nenhum critério que explique por que um dado foi para cada lugar.

**Nada está quebrado por causa disso, e isso é importante dizer.** O arquivo
local atende: a loja funciona, a suíte roda, ninguém disputa escrita. Esta
entrega não conserta um defeito — ela **fecha uma incoerência**. O custo de não
fazer nada é ter que justificar, na defesa do trabalho, por que o banco ficou
de fora quando o resto da stack mudou.

**Há um efeito colateral que vale mais que a coerência.** Os testes que provam
a camada de persistência hoje rodam contra um banco que a loja não usa. Eles
afirmam ordenação, agregação e chave estrangeira — e o fazem contra um motor
diferente do que atende o cliente. São testes que podem passar enquanto a loja
se comporta de outro jeito. Trocar o substrato sem trocar o dos testes
manteria esse ponto cego; esta spec fecha os dois juntos.

## 2. Objetivo

Fazer a loja guardar seus dados no mesmo serviço onde já guarda suas imagens,
sem alterar um único comportamento visível — e fazer os testes de persistência
passarem a provar o banco que a loja de fato usa.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente | Não percebe nada. É o critério de sucesso, não um efeito colateral |
| Administrador da loja | Idem: as mesmas telas, os mesmos dados |
| Quem desenvolve o projeto | Passa a precisar da credencial do banco para rodar a aplicação, e de um ambiente de contêineres para rodar a suíte |

## 4. Histórias de usuário

> **HU-01** — Como **dona da loja**, quero que meus dados fiquem no mesmo
> serviço das minhas fotos, para não depender de um arquivo na máquina de quem
> desenvolve.
>
> **HU-02** — Como **quem desenvolve**, quero que os testes de persistência
> provem o banco que a loja usa, e não um parecido.
>
> **HU-03** — Como **quem desenvolve**, quero rodar a suíte sem rede e sem
> credencial, como sempre pude.
>
> **HU-04** — Como **quem desenvolve**, quero que a falta de um pré-requisito
> me diga o que fazer, em vez de me dar um erro de conexão cru.

## 5. Requisitos funcionais

### O banco

- **RF-01** — A aplicação DEVE guardar seus dados no banco de dados da loja,
  hospedado no mesmo serviço em nuvem que já guarda as imagens.
- **RF-02** — Nenhum comportamento visível da loja DEVE mudar por conta desta
  troca.
- **RF-03** — A credencial de acesso ao banco NÃO DEVE ser versionada.
- **RF-04** — Criar o banco do zero DEVE continuar acontecendo sozinho ao subir
  a aplicação, sem passo manual.
- **RF-05** — A massa de demonstração DEVE continuar sendo semeada como é hoje.

### A suíte

- **RF-06** — A suíte automatizada NÃO DEVE depender de rede, nem tocar o banco
  da loja.
- **RF-07** — Os testes que provam a camada de persistência DEVEM exercitar o
  mesmo motor de banco que a aplicação usa.
- **RF-08** — Cada teste de persistência DEVE continuar recebendo um banco
  virgem, sem interferência dos demais.
- **RF-09** — Faltando o pré-requisito de ambiente para a suíte, a falha DEVE
  dizer o que fazer e o que a suíte não faz.

## 6. Regras de negócio

- **RN-01** — Substrato muda, loja não. Esta entrega não tem permissão para
  alterar comportamento; qualquer diferença observada é defeito a corrigir ou
  decisão a registrar, nunca "efeito da migração".
- **RN-02** — A suíte é determinística e roda offline. Regra herdada da `020` e
  reafirmada na `027`: teste que depende de rede ou de credencial não é teste
  da suíte padrão.
- **RN-03** — Credencial de serviço externo não é versionada. Regra
  constitucional, aplicada aqui pela terceira vez.
- **RN-04** — Teste de persistência prova o banco que a loja usa. É a razão de
  a suíte trocar de motor junto com a aplicação — sem isso, os testes viram
  simulação de um banco que ninguém executa.
- **RN-05** — Erro de ambiente descreve a causa, não o sintoma. Regra herdada
  de dois achados: o cabeçalho vazio da `020` e a configuração vazia da `027`,
  que derrubaram telas inteiras em vez de recusar com mensagem.

## 7. Critérios de aceite

### CA-01 — A loja segue igual
- **Dado** um banco recém-criado no novo serviço
- **Quando** percorro catálogo, produto, carrinho, conta e meus pedidos
- **Então** tudo se comporta como antes da troca

### CA-02 — A base nasce sozinha
- **Dado** um banco vazio e a credencial configurada
- **Quando** subo a aplicação
- **Então** o esquema é criado e a massa de demonstração é semeada, sem passo
  manual

### CA-03 — Nenhum segredo versionado
- **Dado** o repositório recém-clonado
- **Quando** procuro a credencial do banco
- **Então** encontro apenas um exemplo com o campo em branco

### CA-04 — A suíte roda offline
- **Dado** que não tenho credencial do banco da loja configurada
- **Quando** rodo a suíte inteira
- **Então** ela passa, sem tocar o banco da loja e sem depender de rede

### CA-05 — Os testes de persistência provam o motor certo
- **Dado** um teste de integração
- **Quando** ele executa
- **Então** ele exercita o mesmo motor de banco que a aplicação usa

### CA-06 — Cada teste recebe um banco virgem
- **Dado** dois testes de persistência que gravam os mesmos dados
- **Quando** rodam na mesma execução
- **Então** nenhum enxerga o que o outro gravou

### CA-07 — O pré-requisito ausente se explica
- **Dado** que o ambiente de contêineres não está disponível
- **Quando** rodo a suíte
- **Então** recebo uma mensagem dizendo o que ligar, e que a suíte não usa
  banco nenhum da minha máquina nem do serviço da loja

### CA-08 — A ordenação do catálogo é a que a loja mostra
- **Dado** produtos com acento, dígito e maiúscula no nome
- **Quando** um teste afirma a ordem alfabética
- **Então** a ordem afirmada é a que a loja exibe de verdade

## 8. Fora de escopo

- **Qualquer mudança de esquema.** Nenhuma tabela, coluna ou índice novo. Se
  aparecer necessidade, é outra entrega.
- **Trocar a autenticação por Supabase Auth.** O sistema usa ASP.NET Identity
  desde a `004`, com `Usuario` separado de `ContaDeAcesso`; trocar isso é uma
  entrega inteira e ninguém pediu.
- **Row Level Security.** A autorização é feita pela aplicação, por papel; a
  conexão é de serviço. Ligar RLS sem trocar o modelo de acesso não protegeria
  nada e quebraria tudo.
- **Bibliotecas cliente do Supabase.** O acesso continua por EF Core, como
  sempre foi.
- **Migração de dados.** Não há dado a preservar: o único banco que existe é
  local e descartável, reconstruído pela semeadura.
- **Publicar a aplicação.** Hospedar a loja é entrega própria. Esta só troca
  onde o dado mora.
- **Otimizar consulta.** Se alguma ficar lenta no motor novo, vira achado
  registrado, não conserto de improviso.

## 9. Dependências

- **Depende de:** a `027`, que trouxe o serviço em nuvem para o projeto e
  estabeleceu como a credencial dele é configurada e verificada.
- **Bloqueia:** publicar a loja — item de backlog que hoje esbarra em o banco
  ser um arquivo na máquina de quem desenvolve.

## 10. Decisões e pendências

**A motivação é coerência de stack, e isso limita o orçamento.** Decisão do
responsável ao especificar, escolhida entre três alternativas apresentadas
(preparar publicação, exigência acadêmica, coerência). Nada está quebrado hoje:
a consequência direta é que a troca **não tem permissão de tornar a suíte mais
lenta que o aceitável, mais frágil, ou dependente de rede**. Foi essa régua que
descartou as alternativas mais caras.

**A suíte troca de motor junto com a aplicação.** Considerou-se manter os
testes no motor atual — mais rápido e sem pré-requisito novo. Recusado por dois
motivos, e o segundo pesou mais que o primeiro. O primeiro: os testes de
persistência passariam a provar um banco que ninguém executa, incluindo
asserções de ordenação que dependem de regra de ordenação do motor. O segundo:
manter os dois motores obrigaria a aplicação a suportar **dois provedores em
tempo de execução e dois conjuntos de migrations** — complexidade permanente,
paga em toda mudança de esquema futura. O caminho escolhido tem *menos* código,
não mais.

**O pré-requisito da suíte é um ambiente de contêineres.** Decisão do
responsável, com a ressalva de que a ausência dele produza mensagem
explicativa (RF-09, CA-07) e não erro de conexão cru. Consequência aceita: sem
esse ambiente disponível, a suíte não roda.

**O histórico de migrations é descartado e recriado.** Decisão do responsável
ao especificar. As migrations existentes foram geradas para o motor antigo e
não podem ser aplicadas no novo — uma delas chega a manipular dados com
sintaxe que o motor novo interpreta de outro jeito. Como não há dado a
preservar, histórico de evolução não tem função aqui: ele existe para evoluir
banco cujos dados não se pode perder. O histórico segue disponível no
versionamento, e cada spec anterior já registra a migration que criou.

**⚠️ A ordenação alfabética pode mudar, e isso é esperado.** Os dois motores
ordenam texto por critérios diferentes — um por código de caractere, outro por
regra de idioma. Nome com acento, dígito ou maiúscula pode trocar de posição.
A decisão registrada é **não pré-resolver**: roda-se a suíte, observa-se o que
muda, e avalia-se se a ordem nova é melhor (para uma loja brasileira,
provavelmente é) antes de atualizar a asserção — ou de fixar uma regra
explícita, se for pior. É exatamente a diferença que manter os testes no motor
antigo teria escondido (CA-08).

**⚠️ O banco da loja passa a ser estado compartilhado.** Hoje o banco é um
arquivo local que se apaga à vontade. Depois desta entrega, subir a aplicação
escreve no banco em nuvem. A suíte não o toca — ela usa ambiente próprio e
descartável —, então o risco fica contido às execuções manuais de quem
desenvolve. É mudança de hábito, não defeito.

**⚠️ A ordem das categorias no cabeçalho continua sendo a do banco** —
pendência herdada, repetida em todas as entregas desde a de correções da página
inicial, ainda sem critério definido pelo responsável. Segue fora de escopo, e
esta entrega é justamente do tipo que **não** pode resolvê-la por conta própria:
mudar ordenação aqui violaria a RN-01.

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação (classe, tabela, framework, rota)
      nos requisitos — os nomes técnicos aparecem só no plano
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados, não só o caminho feliz — CA-07
      cobre pré-requisito ausente; CA-03, credencial vazada; CA-04, ausência de
      credencial
- [x] Mensagens visíveis ao usuário estão escritas em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida de verdade
- [x] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — as três pendências da
      seção 10 são risco declarado e pendência herdada
- [x] Nada aqui conflita com `.specify/memory/constitution.md`
