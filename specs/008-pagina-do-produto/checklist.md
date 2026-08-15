# Checklist de conclusão — Página do produto

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado — CA-01 a CA-15 ao vivo, contra a aplicação
      rodando, incluindo consulta direta ao banco para confirmar contagem de
      votos e ausência de efeito em cada caminho recusado (CA-13, CA-14).
      CA-09 (8 avaliações, "Ver mais" aparece e depois some) provado pelos
      dois testes de unidade dedicados — não recriado ao vivo por falta de
      volume de dados semeados, mas a mesma lógica (`TemMais`) foi confirmada
      ao vivo com 3 avaliações (`TemMais = false`). CA-16 (375px, sem rolagem
      horizontal) verificado por revisão estática do CSS — a grade colapsa
      para coluna única abaixo de 900px e nenhuma largura fixa excede o
      viewport —, não por navegador real
- [x] Nada fora do escopo declarado entrou junto na entrega — a correção do
      404 em GET (achado durante a implementação, ver nota abaixo) é
      pré-requisito de RF-03/RF-04, não escopo adicional
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma `ProjectReference` nova. `VotoUtil` é entidade de
      domínio pura; a navegação `Avaliacao → Usuario` já existia desde a `004`
- [x] **II** — `VotoUtil`: `private set`, construtor validante,
      `protected Ctor()`. `Avaliacao.AlternarVotoUtil`/`Produto.AlterarDescricao`
      são métodos de intenção que guardam RN-06/RN-07/RN-01 na entidade
- [x] **III** — Descrição: `ProdutoDTOValidator.MaximumLength(4000)` **e**
      `Produto.ValidarDescricao` no construtor — as duas barreiras, não só uma
- [x] **IV** — Nomes em português em toda a base nova: `ProdutoDetalheDTO`,
      `AvaliacaoService`, `OrdenacaoAvaliacao`, `EstrelasNota`. Preço e data
      formatados em `pt-BR` (`N2`, `d MMM yyyy`)
- [x] **V** — Fase 2 inteira de teste vermelho antes de qualquer Fase 3+;
      confirmado por `dotnet build` mostrando só os erros de tipo ausente
      esperados, nunca erro de compilação alheio
- [x] **VI** — `IAvaliacaoRepository`/`AvaliacaoRepository` novos; o voto
      grava via `IUnitOfWork.SalvarAlteracoes` em `AvaliacaoService`; uma
      migration versionada (`AddProdutoDescricaoAndAvaliacaoVotes`), inspecionada
      antes de confiar nela
- [x] **VII** — `VotarUtil`: `[HttpPost]`, `[ValidateAntiForgeryToken]`,
      `[Authorize]`, `async Task<IActionResult>`, redireciona no sucesso.
      `Detalhes` é `GET` público e não muda estado
- [x] **VIII** — Domínio lança `ArgumentException`/`InvalidOperationException`;
      Aplicação lança `KeyNotFoundException`; nenhum `try/catch` em ação de
      controller. A tradução para 404/redirecionamento amigável vive no
      `FilterException` global, não no controller — ver nota abaixo

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos (solução inteira)
- [x] `dotnet test` verde — 310/310 na suíte rápida (baseline T002: 265);
      suíte E2E da `007` não tocada por esta feature
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração —
      `AvaliacaoRepositoryIntegrationTests`, 4 testes rodando em SQLite de
      verdade, incluindo a ordenação por votos (confirma que o EF Core
      rastreia `Avaliacao.Votos` pelo campo privado sem configuração extra)

## Interface

- [x] `asp-action`/`asp-controller` de cada formulário aponta para uma ação
      que existe de fato — conferido também ao vivo (voto, ordenação,
      cadastro de produto)
- [x] Erros de validação aparecem no campo (`asp-validation-for`) — a
      descrição usa o mesmo padrão do restante do formulário de produto
- [x] Testado em largura de tela pequena — só por revisão estática (ver nota
      de CA-16 acima), não em navegador real
- [x] Valores monetários e datas formatados em `pt-BR` — preço `R$ 19,99`,
      data `26 mar. 2026`, confirmados ao vivo

## Segurança

- [x] Nenhum segredo commitado — a senha das contas de exemplo do seed
      (`SenhaSeed@123`) é dado de desenvolvimento, não credencial real; só
      existe fora de produção (`if (!app.Environment.IsProduction())`)
- [x] Entrada do usuário não é interpolada em HTML sem escape — Razor faz o
      escape por padrão; a descrição do produto (texto livre do
      administrador) passa pelo `@` normal, sem `Html.Raw`
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      inalterado; a página de produto não lida com login

---

## Achados registrados durante a execução

**A ausência de tratamento de 404 em ações `GET`.** O `FilterException`
existente só tratava `POST`. Uma `KeyNotFoundException` numa ação `GET`
(produto inexistente ou inativo — exatamente CA-04/CA-05) propagava sem
nenhuma tela, só um erro cru do Kestrel. Sem essa peça, os dois critérios mais
básicos da spec eram inatingíveis. Resolvido com o padrão idiomático do
ASP.NET Core: `FilterException` devolve `NotFoundResult` para
`KeyNotFoundException` em qualquer método, e
`app.UseStatusCodePagesWithReExecute` reexecuta em `/Home/NaoEncontrado`
(view nova, mesmo estilo mínimo de `AcessoNegado.cshtml`). A mesma mudança
resolveu de brinde o caminho de voto forçado na própria avaliação (RF-21):
como `VotarUtil` não tem view própria, o `InvalidOperationException` desse
caso específico redireciona para o `Referer` em vez de tentar redesenhar uma
view inexistente. Confirmado ao vivo nas duas pontas — CA-04/CA-05 devolvendo
404 com a tela certa, e CA-14 devolvendo 302 sem alterar nenhuma contagem.

**Colisão de CPF no seed.** A conta de exemplo `cliente1.seed` usava o mesmo
CPF do administrador semeado (`52998224725`), travando a subida da aplicação
com violação de índice único assim que o banco de desenvolvimento era
recriado do zero. Encontrado e corrigido durante a própria fumaça manual desta
tarefa (T049) — é exatamente o tipo de defeito que só aparece rodando a
aplicação de verdade, não em teste de unidade.
