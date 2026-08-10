# Especificação — Cadastro de produto pelo administrador

**ID:** `001-cadastro-produto-admin` · **Branch:** `001-cadastro-produto-admin`
**Criada em:** 2026-08-07 · **Status:** Rascunho

> Feature já iniciada no commit `7c4d541` ("Iniciando página de cadastrar produto
> de Admin"). Esta spec formaliza retroativamente o que a tela deve fazer e
> serve de exemplo executável do fluxo SDD.

---

## 1. Contexto e problema

O catálogo hoje só é populado pela massa inicial criada na subida da aplicação.
A loja não tem como incluir um doce novo sem alguém mexer em código e reiniciar o
sistema. Existe uma tela em `Admin/Cadastro` com o formulário desenhado, mas o
caminho de gravação está incompleto: o produto não chega ao banco.

## 2. Objetivo

Permitir que o administrador cadastre um produto novo pela interface web e o veja
na vitrine imediatamente.

## 3. Quem é afetado

| Perfil | Como interage com esta feature |
|---|---|
| Cliente (visitante) | Passa a ver na vitrine os produtos cadastrados pela loja |
| Cliente autenticado | Nenhuma interação direta |
| Administrador da loja | Único perfil que acessa e usa o formulário |

## 4. Histórias de usuário

> **HU-01** — Como **administrador**, quero cadastrar um produto informando nome,
> preço, imagem e subcategoria, para que ele apareça na vitrine sem depender de
> alteração no código.
>
> **HU-02** — Como **administrador**, quero ver a mensagem de erro no campo errado
> quando eu digitar algo inválido, para corrigir sem perder o que já preenchi.
>
> **HU-03** — Como **dono da loja**, quero que só administradores alcancem essa
> tela, para que nenhum visitante insira produto no catálogo.

## 5. Requisitos funcionais

- **RF-01** — O sistema DEVE apresentar ao administrador um formulário de cadastro
  de produto com os campos: nome, preço, status, URL da imagem e subcategoria.
- **RF-02** — O sistema DEVE gravar o produto de forma permanente ao receber o
  formulário válido, de modo que ele continue existindo após reiniciar a aplicação.
- **RF-03** — O sistema DEVE exibir mensagem de confirmação e apresentar o
  formulário limpo após um cadastro bem-sucedido.
- **RF-04** — O sistema DEVE reapresentar o formulário com os valores digitados e
  a mensagem de erro junto ao campo correspondente quando algum dado for inválido.
- **RF-05** — O sistema NÃO DEVE gravar nada quando qualquer campo obrigatório
  estiver inválido.
- **RF-06** — O sistema DEVE recusar o acesso ao formulário e ao envio para quem
  não for administrador autenticado.
- **RF-07** — O sistema DEVE oferecer a subcategoria como escolha entre as
  subcategorias existentes, não como digitação livre de identificador.
- **RF-08** — O produto recém-cadastrado DEVE aparecer na vitrine da página
  inicial quando seu status for *Ativo*.

## 6. Regras de negócio

- **RN-01** — Nome é obrigatório e tem no mínimo 3 caracteres.
- **RN-02** — Preço é obrigatório e maior que zero.
- **RN-03** — Imagem é obrigatória e precisa ser uma URL absoluta `http` ou `https`.
- **RN-04** — Subcategoria é obrigatória e precisa existir.
- **RN-05** — Um produto nasce com status *Ativo* quando o administrador não
  escolhe outro.
- **RN-06** — Preço é exibido e digitado no formato brasileiro, com vírgula
  decimal e duas casas.

## 7. Critérios de aceite

### CA-01 — Cadastro bem-sucedido
- **Dado** que estou autenticado como administrador
- **Quando** preencho nome "Brigadeiro Gourmet", preço 4,50, status *Ativo*, uma
  URL de imagem válida e escolho a subcategoria "Docinhos", e envio
- **Então** vejo a mensagem de confirmação, o formulário volta limpo, e o produto
  aparece na vitrine da página inicial

### CA-02 — Persistência real
- **Dado** que acabei de cadastrar "Brigadeiro Gourmet"
- **Quando** a aplicação é reiniciada
- **Então** "Brigadeiro Gourmet" continua no catálogo

### CA-03 — Nome curto demais
- **Dado** que estou no formulário
- **Quando** envio com nome "Bo"
- **Então** o formulário volta com os demais campos preenchidos e a mensagem
  "Nome deve ter no mínimo 3 caracteres." aparece abaixo do campo Nome, e nada é gravado

### CA-04 — Preço inválido
- **Dado** que estou no formulário
- **Quando** envio com preço 0
- **Então** vejo "Preço deve ser maior que zero." abaixo do campo Preço e nada é gravado

### CA-05 — Imagem inválida
- **Dado** que estou no formulário
- **Quando** envio com imagem "foto.png"
- **Então** vejo "URL da imagem inválida." abaixo do campo Imagem e nada é gravado

### CA-06 — Visitante bloqueado
- **Dado** que não estou autenticado
- **Quando** acesso `/Admin/Cadastro`
- **Então** sou levado à tela de login e não vejo o formulário

### CA-07 — Cliente comum bloqueado
- **Dado** que estou autenticado como cliente comum
- **Quando** acesso `/Admin/Cadastro`
- **Então** recebo negação de acesso e nenhum produto pode ser criado por mim

## 8. Fora de escopo

- Edição e exclusão de produto — spec própria
- Listagem administrativa de produtos com busca e paginação — spec própria
- Upload de arquivo de imagem (esta feature usa **URL**, não upload)
- Cadastro de categoria e subcategoria pela interface
- Cadastro e vínculo de promoção — o campo Promoção sai do formulário nesta
  entrega (ver `[NECESSITA ESCLARECIMENTO]` resolvido na seção 10)
- Controle de estoque

## 9. Dependências

- **Depende de:** `000-baseline` (autenticação e entidade `Produto` existentes);
  existência de subcategorias cadastradas na massa inicial para RF-07
- **Bloqueia:** edição de produto, controle de estoque, promoções

## 10. Pendências

- [x] ~~`[NECESSITA ESCLARECIMENTO: o campo Promoção do formulário atual usa o enum PromocaoTipo, mas o produto guarda o identificador de uma promoção — o que o administrador deveria escolher?]`~~
      **Resolvido:** promoção não existe como cadastro ainda. O campo sai do
      formulário nesta entrega e volta na spec de promoções.
- [ ] `[NECESSITA ESCLARECIMENTO: como um usuário se torna administrador? A massa inicial cria um administrador fixo, ou existirá uma tela de gestão de papéis?]`

---

## Checklist de qualidade da spec

- [x] Não há nenhum detalhe de implementação
- [x] Todo requisito funcional é verificável por um teste
- [x] Todo requisito tem ao menos um critério de aceite correspondente
- [x] Os caminhos de erro estão especificados
- [x] Mensagens visíveis ao usuário estão em português, no texto final
- [x] A seção "Fora de escopo" foi preenchida
- [ ] Não restam marcações `[NECESSITA ESCLARECIMENTO]` — **1 pendente, bloqueia aprovação**
- [x] Nada conflita com a constituição
