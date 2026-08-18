# Especificação — Organização de nomenclatura

**ID:** `010-organizacao-de-nomenclatura` · **Branch:** `010-organizacao-de-nomenclatura`
**Criada em:** 2026-08-18 · **Status:** Implementada

---

> **Nota sobre o formato.** O template proíbe detalhe de implementação porque a
> spec normalmente descreve comportamento novo para o usuário. Esta feature é
> como a `002-revisao-tecnica`: não muda nada que o cliente veja, só paga dívida
> de organização interna. A seção 5 é o acréscimo que a `002` já introduziu a
> este template — requisitos de **qualidade interna**, sem manifestação visível,
> mas com teste ou verificação que os prova.

---

## 1. Contexto e problema

Uma auditoria de nomenclatura e organização (`/analisar`, 2026-08-18) encontrou
dois pontos de atrito acumulados desde o baseline pré-constituição:

Primeiro, existem dois controladores cujo nome é quase sinônimo em português —
um deles gerencia cadastro de produto, o outro gerencia cadastro de
administrador — mas o nome de nenhum dos dois diz o que ele gerencia. Quem lê
só os nomes não tem como adivinhar qual é qual, e essa ambiguidade sobrevive
desde antes da constituição existir: o vocabulário de domínio consolidado a
partir da spec `004` (o termo "Administrador" para pessoa, papel e serviço)
nunca foi retroaplicado ao controlador mais antigo, que ainda carrega o nome
genérico com que nasceu.

Segundo, dois arquivos de tela parcial de uso único moram numa pasta reservada
para o que é reaproveitado por mais de uma tela — também herança do baseline.
Passaram a destoar quando as duas features mais recentes assentaram, na
prática, um padrão diferente para esse mesmo tipo de arquivo, sem que esse
padrão fosse escrito em lugar nenhum: cada feature nova tem que redescobri-lo
por imitação.

## 2. Objetivo

Dar a cada controlador administrativo um nome que diga o que ele gerencia,
realinhar os dois arquivos de tela ao padrão que a base já pratica, e registrar
esse padrão por escrito para que a próxima feature não precise adivinhá-lo.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Nenhuma mudança perceptível |
| Cliente autenticado | Nenhuma mudança perceptível |
| Administrador da loja | O endereço da tela de cadastro de produto muda; quem tinha o endereço antigo salvo precisa do novo |
| Desenvolvedor do TCC | Lê o nome de um controlador e sabe o que ele gerencia sem abrir o arquivo; encontra a tela parcial de uma página junto do controlador dono, não numa pasta compartilhada que não reflete o uso real |

## 4. Histórias de usuário

> **HU-01** — Como **desenvolvedor do TCC**, quero que o nome de cada
> controlador administrativo diga o que ele gerencia, para não precisar abrir o
> arquivo só para descobrir a diferença entre os dois.
>
> **HU-02** — Como **desenvolvedor do TCC**, quero que uma tela parcial usada
> por uma página só fique junto do controlador dono dela, para não procurar em
> duas pastas diferentes dependendo de quando o arquivo foi criado.
>
> **HU-03** — Como **desenvolvedor do TCC**, quero encontrar essa regra
> escrita em algum lugar, para segui-la na próxima feature sem precisar
> perguntar ou adivinhar pelo exemplo mais recente.

## 5. Requisitos de qualidade interna

*Não observáveis pelo usuário final, verificáveis por inspeção ou por teste
automatizado. Cada um cita o princípio da constituição que o motiva.*

- **RQ-01** *(Princípio IV)* — O controlador que hoje gerencia o cadastro de
  produto administrativo DEVE ter um nome que expresse essa responsabilidade,
  distinto e não sinônimo do nome do controlador que gerencia administradores.
- **RQ-02** *(Princípio IV / organização de arquivos)* — Uma tela parcial usada
  por uma única página DEVE morar na pasta do controlador que a usa; a pasta
  compartilhada de telas é reservada para o que é reaproveitado por mais de uma
  página.
- **RQ-03** *(Princípio IV)* — A regra de RQ-02 DEVE estar escrita na
  constituição do projeto, não apenas praticada por imitação entre features.

## 6. Regras de negócio

Nenhuma — esta feature não introduz nem altera regra de domínio.

## 7. Critérios de aceite

### CA-01 — O endereço antigo de cadastro de produto não existe mais
- **Dado** que o endereço antigo da tela de cadastro de produto existia antes
  desta feature
- **Quando** um administrador autenticado tenta acessá-lo diretamente
- **Então** recebe "não encontrado", e não a tela de cadastro

### CA-02 — O cadastro de produto continua funcionando no novo endereço
- **Dado** que sou administrador autenticado
- **Quando** acesso o novo endereço da tela de cadastro de produto e envio
  dados válidos
- **Então** o produto é cadastrado, exatamente como antes desta feature

### CA-03 — Nada muda para quem não é administrador
- **Dado** que não sou administrador
- **Quando** tento acessar o novo endereço da tela de cadastro de produto
- **Então** sou tratado exatamente como era antes desta feature (redirecionado
  ao login, ou recebendo acesso negado, conforme o caso)

## 8. Fora de escopo

- **Qualquer mudança visual nas duas telas envolvidas.** Nenhum HTML de tela
  muda além do necessário para o arquivo trocar de pasta.
- **Renomear o controlador de administradores.** Ele já tem nome adequado; só
  o outro lado da ambiguidade muda.
- **Revisar todo o restante da nomenclatura do projeto.** Esta feature resolve
  só os dois achados da auditoria que a originou, não uma varredura completa.
- **Redirecionamento do endereço antigo para o novo.** O endereço antigo nunca
  foi divulgado a cliente nenhum — é rota interna de área administrativa —
  então deixar de existir (CA-01) é suficiente; não é criado um "redirecionar
  permanentemente" para ele.

## 9. Dependências

- **Depende de:** nenhuma spec específica — é reorganização do que já existe
  desde o baseline (`000`) e a `005-gestao-de-administradores` (que introduziu
  o segundo controlador e, com ele, a ambiguidade).
- **Bloqueia:** nada.

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
- [x] Nada aqui conflita com `.specify/memory/constitution.md` — RQ-03 pede uma
      emenda ao Princípio IV; a emenda em si é assunto do plano
