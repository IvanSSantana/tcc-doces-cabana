# Checklist de conclusão — Redesenho do carrinho

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando — **exceto CA-05**
      ("Com entrega calculada, o destaque é total a pagar"), que **não é
      verificável nesta entrega isoladamente**: não há de onde vir uma cotação
      de frete até a spec `020` ser implementada. Coberto por teste de unidade
      (`CarrinhoMapperTests`, injetando a cotação diretamente); a prova de
      ponta a ponta fica registrada como pendência para quando a `020` existir
      (plano §6, risco declarado desde a especificação).
- [x] Nada fora do escopo declarado entrou junto na entrega — os passos do
      fechamento, o cupom funcional e a cotação de frete continuam de fora,
      como a spec determina
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos
- [x] **II** — Nenhuma entidade nova nesta entrega (aplica-se por vacuidade)
- [x] **III** — Não se aplica: `Esvaziar` não recebe dado do usuário para validar
- [x] **IV** — Nomes, mensagens e comentários em português (`Esvaziar`,
      `ConfirmarEsvaziar`, `TemEntregaCalculada`, `ValorTotal`)
- [x] **V** — Todos os testes foram escritos antes e vistos falhar antes de
      passar — inclusive a confirmação de que a falha ocorria pelo motivo certo
      (método inexistente, não erro alheio)
- [x] **VI** — `Esvaziar` grava pelo `IUnitOfWork`, uma chamada só; nenhuma
      migration, pois nenhum esquema mudou
- [x] **VII** — `Esvaziar` é `[HttpPost]` com `[ValidateAntiForgeryToken]`,
      aguardado, redirecionando no sucesso; `ConfirmarEsvaziar` é `[HttpGet]`
      de leitura, sem efeito colateral
- [x] **VIII** — Sem `try/catch` em ação de controller

## Testes

- [x] `dotnet build` sem warnings novos (só o aviso pré-existente do pacote
      SQLite, alheio a esta entrega)
- [x] `dotnet test` verde — 560/560 unitários, 172/172 E2E (suíte completa,
      do zero)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Não há migration nesta entrega — sem persistência nova a testar em
      integração; `Esvaziar` é coberto por unidade com `Moq`

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
- [x] Não há campo de validação de entrada do usuário nesta entrega (cupom é
      decorativo/desabilitado; esvaziar não recebe parâmetro)
- [x] Testado em largura de tela pequena (375px) — verificado com screenshot;
      o conteúdo do carrinho empilha corretamente. O estouro horizontal
      visível vem do cabeçalho compartilhado, dívida herdada desde a `009`,
      fora de escopo desta entrega
- [x] Valores monetários formatados em `pt-BR` (`N2`, vírgula decimal)

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape (Razor escapa
      por padrão; nenhum `@Html.Raw` introduzido)
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      não se aplica a esta entrega (nenhum caminho de erro novo)

## Achados registrados durante a execução

- **Lacuna na spec, resolvida com o responsável antes de implementar:** nem
  `020` nem `021` diziam qual preço, havendo mais de uma opção de frete,
  compõe o "total a pagar" do carrinho. Decidido que é a mais barata (RN-06
  nova, registrada em `spec.md` e `plan.md` §6) — estimativa até o fechamento
  (`022`) escolher de fato.
- **Achado na verificação visual (T028):** o `<dialog>` nativo não centralizava
  por padrão dentro do layout em grid do carrinho. Corrigido com
  centralização explícita em CSS.
- **`OpcaoDeFreteDTO` e `CotacaoDeFreteDTO` foram criados por esta entrega**,
  não pela `020` — a `021` chegou primeiro à implementação. A `020`, ao ser
  implementada, deve conferir que já existem em vez de recriar (plano §6).
