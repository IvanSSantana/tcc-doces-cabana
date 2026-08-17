# Checklist de conclusão — Páginas institucionais

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado — CA-01, CA-02, CA-03, CA-04, CA-05, CA-06,
      CA-07, CA-09, CA-10 e CA-12 por teste E2E automatizado, contra a
      aplicação rodando de verdade (não `WebApplicationFactory`). CA-08
      (páginas públicas) coberto pelo teste de unidade do controller (sem
      `[Authorize]`) e confirmado ao vivo por `curl` direto, sem sessão. CA-11
      (foco de teclado visível) verificado manualmente e por revisão de CSS
      (`:focus-visible` com contorno de 2px) — não automatizado, pelo mesmo
      motivo registrado na `008`: o Playwright provaria a presença da regra,
      não a percepção visual
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou — as duas pendências
      da versão inicial da spec foram resolvidas por decisão explícita do
      usuário: publicar com o conteúdo de preenchimento da própria referência
      visual (ver nota abaixo)

## Constituição

- [x] **I** — Nenhuma `ProjectReference` nova. `InstitucionalController` não
      injeta nada — nem sequer `Application`
- [x] **II** — n/a: nenhuma entidade de domínio nesta feature
- [x] **III** — n/a: nenhuma entrada de usuário, nenhum formulário (RF-17)
- [x] **IV** — `InstitucionalController`, `QuemSomos`, `BlocoInstitucionalViewModel`
      em português. RF-07 corrigiu uma violação preexistente: a view de
      andaime removida estava em inglês
- [x] **V** — Fase 2 inteira vermelha antes da Fase 3: teste de unidade do
      controller e os 10 testes E2E, todos escritos e rodados falhando antes
      da implementação (rota/ação inexistente)
- [x] **VI** — n/a: nenhuma persistência, nenhuma migration
- [x] **VII** — n/a (parcial): não há `POST` nesta feature. A ausência de
      `[Authorize]` é requisito (RF-06), não omissão — confirmada por
      `curl` sem sessão devolvendo 200 nas duas rotas
- [x] **VIII** — Nenhum `try/catch` em ação de controller. A rota antiga
      (`Home/Privacidade`) cai no `UseStatusCodePagesWithReExecute` que a
      `008` instalou — confirmado ao vivo com `curl` devolvendo 404

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos (solução inteira)
- [x] `dotnet test DocesCabana.Tests` verde — 311/311 (baseline T002: 310;
      +2 testes novos, -1 teste da ação removida)
- [x] `dotnet test DocesCabana.Tests.E2E` verde — 27/27 (baseline: 17 dos
      fluxos da `007`; +10 desta feature)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] n/a — feature não toca persistência, logo não há teste de integração
      de repositório a escrever

## Interface

- [x] `asp-action`/`asp-controller` de cada link aponta para uma ação que
      existe de fato — os três links corrigidos (rodapé × 2, modal de login)
      conferidos também ao vivo
- [x] n/a — não há formulário nem `asp-validation-for` nesta feature (RF-17)
- [x] Testado em largura de tela pequena — E2E automatizado a 375px,
      escopado ao conteúdo desta feature (`.pagina-institucional`), e
      confirmado por screenshot real do navegador
- [x] n/a — nenhum valor monetário ou data nesta feature

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — não há
      entrada de usuário nesta feature; o texto da política é literal, escrito
      pelo desenvolvedor, sem `Html.Raw`
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      inalterado; estas páginas não lidam com autenticação

---

## Achados registrados durante a execução

**O cabeçalho compartilhado já estoura horizontalmente a 375px, antes desta
feature existir.** Ao investigar uma falha inicial do teste de CA-10, a
inspeção elemento a elemento (`scrollWidth` vs. viewport) mostrou que a
`.barra-pesquisa` do cabeçalho — presente em toda página do site, não só nas
duas desta feature — não tem nenhuma regra responsiva em `header.css`; a
`section` global usa `padding: 0 40px` fixo, sem `@media` para telas
estreitas. O conteúdo desta feature (`.pagina-institucional`) não contribui em
nada para o estouro: isolado do cabeçalho, ele cabe perfeitamente na tela — o
screenshot mobile confirma o colapso correto em coluna única e o eixo do
ziguezague desaparecendo, como RN-05 exige. Corrigir o cabeçalho está fora do
escopo declarado desta spec (não é arquivo que a `009` toca) e é problema do
site inteiro, não desta feature — por isso não foi corrigido aqui, apenas
registrado. O teste de CA-10 foi escrito medindo o `scrollWidth` de
`.pagina-institucional`, não do documento inteiro, para não travar esta
entrega num defeito alheio a ela; a medição no documento inteiro teria dado
falso-negativo nesta feature e falso-positivo em qualquer outra página do site
que já existe hoje.

**Conteúdo de preenchimento no Quem Somos é decisão explícita, não pendência.**
A versão inicial desta spec listava duas pendências de
`[NECESSITA ESCLARECIMENTO]`: o texto real de Missão/Propósito/Visão e as
quatro imagens. O usuário decidiu, de forma explícita, publicar com o
"lorem ipsum" e os retângulos cinza exatamente como a referência visual os
define, deixando a substituição por conteúdo definitivo da loja para uma
entrega futura (spec §8). Os pontos ficam marcados com comentário no código-fonte
(`_BlocoInstitucional.cshtml`, `QuemSomos.cshtml`) indicando onde o conteúdo
real deve entrar.
