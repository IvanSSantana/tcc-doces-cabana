# Checklist de conclusão — Cadastro de produto pelo administrador

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando —
      CA-01 a CA-07, todos ao vivo via curl contra a aplicação rodando (T022)
- [x] Nada fora do escopo declarado entrou junto na entrega — a exceção
      registrada é a correção do `AccessDeniedPath`, encontrada pela própria
      fumaça manual do T022 (ver nota abaixo)
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

> **Nota sobre a correção fora do plano original:** o T022 revelou que
> `[Authorize(Roles = "Administrador")]`, ao negar acesso a um usuário
> autenticado sem o papel, redirecionava para `/Account/AccessDenied` — o
> caminho padrão do ASP.NET Core Identity, que esta aplicação nunca
> implementou. O resultado era **404**, não uma negação de acesso de
> verdade, o que não cumpre o CA-07 ("recebo negação de acesso"). Corrigido
> configurando `options.AccessDeniedPath = "/Home/AcessoNegado"` e criando a
> ação e a view correspondentes. Reverificado ao vivo: cliente comum agora
> vê "Acesso Negado" (HTTP 200), não 404.

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos
- [ ] **II** — n/a nesta feature (nenhuma entidade nova; `Produto`,
      `Categoria` e `Subcategoria` já existiam desde a `002`/`003`)
- [x] **III** — `ProdutoDTOValidator` (criado na `002`) cobre RN-01 a RN-04 na
      barreira de entrada; `Produto.cs` cobre as mesmas regras no domínio
- [x] **IV** — Nomes, mensagens e comentários em português
- [x] **V** — Testes escritos e vermelhos antes da implementação (T006)
- [x] **VI** — `ProdutoService.Cadastrar` chama `IUnitOfWork.SalvarAlteracoes`;
      nenhuma migration nesta feature (nenhuma mudança de esquema)
- [x] **VII** — `[ValidateAntiForgeryToken]` no POST; `await` em toda chamada
      assíncrona; `[Authorize(Roles = "Administrador")]` na classe;
      POST-Redirect-Get no sucesso
- [x] **VIII** — Nenhum `try/catch` em ação de controller

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos
- [x] `dotnet test` verde — 233/233 (baseline: 227)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração — o teste de
      integração de `ProdutoRepositoryIntegrationTests` (desde a `003`) já
      cobre a gravação com subcategoria real; não precisou de teste novo

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
      — corrigido de `asp-action="Cadastrar"` (inexistente) para
      `asp-action="Cadastro"`
- [x] Erros de validação aparecem no campo (`asp-validation-for`) — confirmado
      ao vivo para nome, preço e imagem (CA-03/04/05)
- [ ] Testado em largura de tela pequena — não verificado nesta rodada
- [x] Valores monetários e datas formatados em `pt-BR` — confirmado ao vivo:
      "R$ 4,50" no card do produto recém-cadastrado

## Segurança

- [x] Nenhum segredo commitado — senha do admin semeado vem de
      `dotnet user-secrets` (`Admin:SenhaInicial`), não do código
- [x] Entrada do usuário não é interpolada em HTML sem escape — Razor escapa
      por padrão; nenhum HTML manual nesta feature
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      inalterado nesta feature (já resolvido na `002`)
