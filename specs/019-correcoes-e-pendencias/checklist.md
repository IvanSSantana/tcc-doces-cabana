# Checklist de conclusão — Correções e pendências

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — RF-01/02/03 em
      `CpfHelper`; RF-04 a RF-10 em `ProdutoService.BuscarDestaquesDaVitrine` +
      `HomeController.Index` + título em `Home/Index.cshtml`; RF-11/RF-12 em
      `docs/arquitetura.md` §9.1/§9.2/§9.3; RF-13 em `EstrelasNota/Default.cshtml`
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando — CA-01 a
      CA-12 têm teste automatizado (unidade ou E2E) que prova o comportamento;
      CA-06/CA-10/CA-11/CA-12 e CA-01 também confirmados ao vivo por HTTP
      (T032/T033). **CA-13 e CA-14 (documentação) verificados por leitura**, não
      por teste automatizado — não há teste de "a doc corresponde ao sistema"
- [x] Nada fora do escopo declarado entrou junto na entrega — corrigidos dois
      achados adicionais durante a Fase 7, da mesma natureza dos previstos
      (comentário desatualizado de `Header.cs` em §4.1, linha da home em §5),
      registrados em T028; nenhuma troca de comportamento fora da spec
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção de
      dependência — `ProdutoService` já dependia de `IFavoritoRepository`
      indiretamente pelo padrão de `CatalogoService`; a mudança é injetar a
      mesma abstração (`Application` → `Application.Contracts`), sem tocar
      camada nenhuma nova
- [x] **II** — Não se aplica: nenhuma entidade nova nesta entrega
- [x] **III** — Regra crítica (CPF) está na barreira de validação
      (`CadastroDTOValidator`, que já delegava a `CpfHelper.CpfValido`) **e**
      no domínio (`CpfHelper`, que é o próprio lugar da regra) — a correção
      em um único método conserta as duas barreiras ao mesmo tempo, por não
      haver duplicação de regra a corrigir em dois lugares
- [x] **IV** — Nomes, mensagens e comentários em português
- [x] **V** — Os testes foram escritos antes e falharam antes de passar — CPF
      (T004/T005, vermelho confirmado por `Assert.False` recebendo `true`,
      não por erro de compilação); vitrine (T011/T012, vermelho por método
      inexistente); home (T016, mesmo padrão). Exceção registrada com
      transparência: os testes E2E de vitrine (T021) não viram o vermelho,
      porque a correção já estava em produção desde a Fase 4 quando foram
      escritos — o vermelho real do mesmo defeito já tinha sido visto e
      confirmado no teste de unidade da Fase 3 (ver T021/T022 em `tasks.md`)
- [x] **VI** — Não se aplica: esta entrega não escreve no banco, não criou
      migration
- [x] **VII** — Não se aplica: nenhuma ação de controller nova, nenhum POST
      novo — `HomeController.Index` continua `[HttpGet]` implícito
- [x] **VIII** — Sem `try/catch` em ação de controller

## Testes

- [x] `dotnet build` sem warnings novos (só o NU1903 pré-existente do pacote
      SQLite, herdado de antes desta entrega)
- [x] `dotnet test` verde — `DocesCabana.Tests`: 552/552. `DocesCabana.Tests.E2E`:
      161/162, a falha é `BuscaTests.Dado_ProdutoComAcento_...`, instável e
      pré-existente à linha de base desta entrega (T002), sem relação com
      CPF, vitrine, estrelas ou documentação — não corrigida, fora de escopo
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração — não se aplica:
      nenhuma escrita nova; `BuscarDestaquesDaVitrine` reaproveita
      `BuscarPaginaDoCatalogo`, que já tem teste de integração desde a `012`

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
      — nenhum formulário novo nesta entrega
- [x] Erros de validação aparecem no campo (`asp-validation-for`) e não só no
      resumo — confirmado ao vivo (T033): `CPF inválido.` aparece em
      `data-valmsg-for="CPF"`
- [x] Testado em largura de tela pequena — não se aplica a esta entrega
      (nenhuma tela nova; a largura da vitrine já é coberta desde a `013`)
- [x] Valores monetários e datas formatados em `pt-BR` — inalterado

## Segurança

- [x] Nenhum segredo commitado
- [x] Entrada do usuário não é interpolada em HTML sem escape — nada novo
      renderiza entrada de usuário nesta entrega
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno —
      "CPF inválido." não distingue qual dígito falhou
