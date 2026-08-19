# Checklist de conclusão — Área administrativa

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RQ-xx` da spec tem código correspondente — RQ-01 (Area `Admin`
      criada), RQ-02 (os dois controladores dentro dela), RQ-03 (autorização
      preservada), RQ-04 (`CatalogoController` → `Areas.Admin.ProdutoController`)
- [x] Todo `CA-xx` foi verificado — CA-01 a CA-07 por teste E2E, contra a
      aplicação rodando de verdade, e reconfirmados ao vivo com `curl`
      (rotas novas devolvendo 302 sem sessão, rotas antigas devolvendo 404)
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma `ProjectReference` nova
- [x] **II** — n/a: nenhuma entidade de domínio nesta feature
- [x] **III** — n/a: nenhuma validação nova
- [x] **IV** — `Areas.Admin.Controllers.ProdutoController` e
      `AdministradorController`, em português. A emenda 1.4.1 registra a
      ressalva de que a unicidade de nome é escopada por area
- [x] **V** — Fase 2 vermelha (rotas antigas ainda respondendo, testes
      falhando por asserção) antes da Fase 3 mover qualquer arquivo
- [x] **VI** — n/a: nenhuma persistência tocada
- [x] **VII** — `[Authorize(Roles = Papeis.Administrador)]` acompanha as duas
      classes na mudança — RQ-03 e CA-04/CA-05 provam que nada foi perdido
- [x] **VIII** — Nenhum `try/catch` novo. Rotas antigas caem no
      `UseStatusCodePagesWithReExecute` que a `008` já instalou

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos (solução inteira)
- [x] `dotnet test DocesCabana.Tests` verde — 311/311 (mesmo total da `012`
      anterior a esta implementação; renomear/mover teste não muda a contagem)
- [x] `dotnet test DocesCabana.Tests.E2E` verde — 30/30 (baseline: 28; +2
      desta feature — atalho do cabeçalho e saída da área pelo rodapé)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] n/a — feature não toca persistência

## Interface

- [x] `asp-action`/`asp-controller`/`asp-area` de cada link aponta para uma
      ação que existe de fato — conferido também ao vivo (cadastro de
      produto, gestão de administradores, atalho do cabeçalho, rodapé de
      dentro da área administrativa)
- [x] n/a — nenhum campo de formulário novo
- [x] n/a — nenhuma tela redesenhada
- [x] n/a — nenhum valor monetário ou data nesta feature

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — inalterado
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      inalterado

---

## Achados registrados durante a execução

Nenhum. A implementação seguiu o plano sem desvio: os dois riscos mais altos
que o plano previu — link de cliente escapando da area sem `asp-area=""`, e
views de area sem `_ViewImports`/`_ViewStart` próprios — foram mitigados
exatamente como desenhado, e confirmados ao vivo antes do fechamento.
