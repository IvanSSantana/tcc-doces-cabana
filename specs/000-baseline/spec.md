# Especificação — Baseline do sistema (estado atual)

**ID:** `000-baseline` · **Criada em:** 2026-08-07 · **Status:** Implementada (parcial)

> Esta spec é **retroativa**: descreve o que já existe no código em 2026-08-07.
> Ela não é para implementar — é o contexto contra o qual as próximas specs são
> escritas, para que ninguém especifique algo que já está pronto nem assuma
> pronto algo que não está.

---

## 1. Contexto e problema

A Doces Cabana é uma loja de doces de Barra Bonita que hoje vende por canais
informais, sem catálogo próprio nem registro estruturado de pedidos. O projeto é
o TCC que constrói o e-commerce da loja.

## 2. Objetivo

Sistema de e-commerce que permita ao cliente descobrir produtos, criar conta e
comprar; e à loja administrar catálogo e pedidos.

## 3. Quem é afetado

| Perfil | Como interage |
|---|---|
| Cliente (visitante) | Navega a vitrine sem autenticação |
| Cliente autenticado | Faz login, recupera senha; comprará e favoritará |
| Administrador da loja | Administrará catálogo, estoque, promoções e pedidos |

---

## 4. Capacidades já implementadas

### 4.1 Autenticação e conta — **pronto**

- **RF-B01** — Visitante cria conta com nome, e-mail, celular, data de nascimento,
  CPF e senha.
- **RF-B02** — Cadastro é recusado quando e-mail **ou** CPF já pertencem a uma conta.
- **RF-B03** — Usuário entra com e-mail ou CPF e senha, com opção "lembrar-me".
- **RF-B04** — Após N tentativas falhas a conta é bloqueada temporariamente.
- **RF-B05** — Usuário solicita redefinição de senha e recebe link por e-mail.
- **RF-B06** — A mensagem de solicitação é idêntica para login existente e
  inexistente, para não revelar quais contas existem.
- **RF-B07** — Usuário redefine a senha com token válido.
- **RF-B08** — Usuário encerra a sessão.

**Regras:** senha com mínimo de 6 caracteres contendo minúscula, maiúscula, número
e caractere especial; CPF validado por dígito verificador; celular em formato
brasileiro; data de nascimento não futura e no máximo 120 anos atrás.

### 4.2 Catálogo — **parcial**

- **RF-B09** — A página inicial exibe uma vitrine de produtos.
- **RF-B10** — Cada produto exibe nome, preço, imagem e status.
- **RF-B11** — O cabeçalho exibe as categorias navegáveis.
- **RF-B12** — Produto tem status *Ativo*, *Inativo* ou *Fora de estoque*.
- **RF-B13** — Produto inativo ou fora de estoque não pode entrar em promoção.

**Regras:** nome com no mínimo 3 caracteres; preço maior que zero; imagem com URL
absoluta `http`/`https`; produto sempre pertence a uma subcategoria.

### 4.3 Infraestrutura — **pronto**

- Persistência em SQLite via EF Core, com migrations aplicadas. SQL Server é o
  banco alvo do deploy — a troca não aconteceu ainda; a spec `002-revisao-tecnica`
  removeu a única configuração de coluna presa ao dialeto SQLite, então trocar o
  provider passa a custar uma linha mais a regeração das migrations.
- Contas gerenciadas pelo ASP.NET Identity com chave `Guid`. Bloqueio temporário
  após tentativas de senha malsucedidas está ativo (5 tentativas, 15 minutos).
- Envio de e-mail por SMTP.
- Massa inicial de dados criada na subida da aplicação (`DbInitializer`), só
  fora de produção.
- Cultura fixada em `pt-BR`.
- Erros não tratados capturados por filtro global.

---

## 5. Modelado no banco, ainda sem comportamento

O arquivo [`ModelagemBancoTCC.dbml`](../../ModelagemBancoTCC.dbml) descreve o
esquema completo pretendido. Destes, **existem como entidade e persistência
apenas `Produto` e `Usuario`**. As demais tabelas estão modeladas no papel e
aguardam spec própria:

| Tabela modelada | Feature futura |
|---|---|
| `Categoria`, `Subcategoria` | Navegação por categoria |
| `Estoque` | Controle de estoque |
| `Promocao`, `Promocao_Produto_FK` | Promoções |
| `Endereco` | Endereço de entrega |
| `Favoritos` | Lista de favoritos |
| `Avaliacao` | Avaliação de produto |
| `Pedido`, `Produto_Pedido_FK` | Carrinho e pedido |
| `Pagamento` | Pagamento |

O `ProdutoDTO` já carrega `EstaFavorito` e `PromocaoId`, mas nada consome esses
campos ainda — são ganchos, não funcionalidade.

---

## 6. Dívidas conhecidas nesta baseline

Registradas aqui para não serem confundidas com comportamento intencional. Cada
uma vira tarefa em alguma spec futura.

| # | Dívida | Princípio ferido | Status |
|---|---|---|---|
| D-01 | Nenhuma escrita chama `IUnitOfWork`; o `Repository<T>` só registra no `ChangeTracker`, então cadastros não são persistidos | VI | Aberta — endereçada pela spec [`001-cadastro-produto-admin`](../001-cadastro-produto-admin/spec.md) |
| D-02 | `AdminController` não exige autorização — a área administrativa está aberta a qualquer visitante | VII | Aberta — endereçada pela spec `001` |
| D-03 | `AdminController.Cadastro` (POST) não aguarda o serviço, não valida `ModelState` e não tem `[ValidateAntiForgeryToken]` | VII | Aberta — endereçada pela spec `001` |
| D-04 | O formulário de cadastro de produto posta para `asp-action="Cadastrar"`, ação que não existe no controller | — | Aberta — endereçada pela spec `001` |
| D-05 | O campo Promoção do formulário é preenchido com `PromocaoTipo` (um enum) onde se espera o identificador de uma promoção | — | Aberta — endereçada pela spec `001` |
| D-06 | Não há `ProdutoDTOValidator`; o produto só é validado pelo domínio, então o erro chega ao usuário como exceção | III | **Resolvida** pela spec [`002-revisao-tecnica`](../002-revisao-tecnica/spec.md) |
| D-07 | `Endereco` está no `.dbml` mas não existe como entidade | — | Aberta — sem spec própria ainda no backlog |

---

## 7. Fora de escopo desta spec

Tudo. Esta spec não pede implementação — ela apenas descreve o estado de partida.

---

## Checklist de qualidade da spec

- [x] Não há detalhe de implementação nos requisitos
- [x] Requisitos verificáveis
- [x] Dívidas separadas do comportamento intencional
- [x] Sem marcações `[NECESSITA ESCLARECIMENTO]`
