# Checklist de conclusão — Organização de nomenclatura

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RQ-xx` da spec tem código correspondente — RQ-01
      (`CatalogoController`), RQ-02 (`_Carrossel`/`_Categorias` em
      `Views/Home/`), RQ-03 (emenda ao Princípio IV)
- [x] Todo `CA-xx` foi verificado — CA-01 e CA-03 por teste E2E
      (`AreaAdministrativaTests`), reconfirmados ao vivo com `curl`
      (`/Admin/Cadastro` → 404, `/Catalogo/Cadastro` sem sessão → 302 para
      login). CA-02 pela suíte E2E existente de cadastro de produto, que
      passou a usar o endereço novo sem alterar nenhuma asserção
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma `ProjectReference` tocada
- [x] **II** — n/a: nenhuma entidade de domínio
- [x] **III** — n/a: nenhuma validação nova ou alterada
- [x] **IV** — É o princípio que esta feature serve. `AdminController` →
      `CatalogoController`; `_Carrossel`/`_Categorias` realinhadas; as duas
      regras ganham texto próprio na constituição (emenda 1.4.0)
- [x] **V** — T003–T005 escritos e rodados vermelhos (erro de compilação por
      `CatalogoController` inexistente; 200 em vez de 404 na rota antiga)
      antes do `git mv` da Fase 3
- [x] **VI** — n/a: nenhuma persistência tocada
- [x] **VII** — Nenhuma garantia removida: `CatalogoController` herda
      `[Authorize(Roles = Papeis.Administrador)]`, `[ValidateAntiForgeryToken]`,
      `await`, guarda de `ModelState` e POST-Redirect-Get exatamente como
      `AdminController` já tinha — só o nome mudou, confirmado por diff
      (nenhuma linha de corpo alterada além da assinatura da classe)
- [x] **VIII** — Nenhum `try/catch` novo em ação de controller. O endereço
      antigo cai no `UseStatusCodePagesWithReExecute` que a `008` já instalou

## Testes

- [x] `dotnet build` sem warnings novos — 0 avisos (solução inteira)
- [x] `dotnet test DocesCabana.Tests` verde — 311/311 (mesmo total da `009`;
      renomear um teste não muda a contagem)
- [x] `dotnet test DocesCabana.Tests.E2E` verde — 28/28 (baseline: 27; +1 desta
      feature, o teste de CA-01)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] n/a — feature não toca persistência

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato —
      `Views/Catalogo/Cadastro.cshtml` usa `asp-action="Cadastro"` sem
      controlador explícito, resolvido pelo controlador atual (`Catalogo`) sem
      qualquer edição de texto no arquivo
- [x] n/a — nenhum campo de formulário novo ou alterado
- [x] n/a — nenhuma tela redesenhada; confirmado ao vivo que a página inicial
      renderiza carrossel e categorias sem diferença visual após a mudança de
      pasta das partials
- [x] n/a — nenhum valor monetário ou data nesta feature

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — inalterado,
      nenhum HTML novo
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      inalterado

---

## Achados registrados durante a execução

**A emenda constitucional planejada como PATCH era, na verdade, MINOR.** O
plano original (§1, §3) descrevia a mudança no Princípio IV como emenda PATCH.
Ao compará-la com o precedente da própria constituição — a emenda 1.1.0, que
acrescentou ao mesmo princípio a regra de arquivo/tipo/namespace e foi
registrada como MINOR — ficou claro que duas regras normativas novas
constituem "expansão material de um princípio existente" (a própria definição
de MINOR na Governança, item 3), não "correção de texto, exemplo ou link"
(PATCH). Corrigido antes do commit: a constituição foi para 1.4.0, não 1.3.1,
e a spec/plano/tasks desta feature foram ajustados para refletir a
classificação correta. Nenhum código de aplicação foi afetado — o achado é só
sobre o número da versão da constituição.
