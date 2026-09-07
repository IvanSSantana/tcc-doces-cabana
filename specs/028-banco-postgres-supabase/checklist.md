# Checklist de conclusão — Banco de dados no Postgres do Supabase

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando — CA-01
      (T028, contra o Postgres real do Supabase), CA-02 (T008), CA-03 (T007),
      CA-04 (T027), CA-05/CA-06 (T013/T015), CA-07 (T009/T011), CA-08 (T014)
- [x] Nada fora do escopo declarado entrou junto na entrega — nenhuma
      entidade, esquema, comportamento visível ou credencial de RLS mudou
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção de dependência — a troca é toda dentro de `Infrastructure`, `Npgsql` entra onde `Sqlite` estava
- [x] **II** — Entidades novas têm `private set`, validam no construtor e têm `protected Ctor()` — não se aplica: nenhuma entidade nova
- [x] **III** — Regras críticas estão no validator **e** no domínio — não se aplica: nenhuma regra nova
- [x] **IV** — Nomes, mensagens e comentários em português — `InfraestruturaPostgresDescartavel`, `PostgresDeTeste`, `MensagemDePostgresIndisponivel`
- [x] **V** — Os testes foram escritos antes e falharam antes de passar, **onde havia comportamento novo a testar** (T009, a mensagem de Docker ausente). Para o resto — troca de provider sobre 245 testes já existentes — o desvio está justificado por escrito no plano §11: a disciplina equivalente foi "suíte verde antes, suíte verde depois, cada vermelho no meio tratado como achado", não presumida em silêncio
- [x] **VI** — Toda escrita chama `IUnitOfWork`; migration criada se o esquema mudou — `InitialCreatePostgres`, esquema idêntico ao anterior
- [x] **VII** — `[ValidateAntiForgeryToken]` em todo POST, chamadas assíncronas aguardadas, rota administrativa autorizada, POST-Redirect-Get no sucesso — nenhuma ação de controller mudou; a connection string com senha foi para *user secrets*
- [x] **VIII** — Sem `try/catch` em ação de controller — nenhuma ação de controller mudou

## Testes

- [x] `dotnet build` sem warnings novos — **0 avisos** (o `NU1903` que existia desde a `020` sumiu de vez, T021)
- [x] `dotnet test` verde — 682 unidade+integração (T013/T015/T027), 185 E2E (T019/T027)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...` — mantido nos 245 testes existentes; os 3 novos (`MensagemDePostgresIndisponivelTests`) seguem o padrão
- [x] Feature que toca persistência tem teste de integração — os 60 já existentes, rodando contra o motor novo

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato — não se aplica: nenhuma view mudou
- [x] Erros de validação aparecem no campo (`asp-validation-for`) e não só no resumo — não se aplica
- [x] Testado em largura de tela pequena — não se aplica: entrega sem interface
- [x] Valores monetários e datas formatados em `pt-BR` — não se aplica: nenhum formato mudou

## Segurança

- [x] Nenhum segredo commitado — `ConnectionStrings:DefaultConnection` com senha em branco no `appsettings.Example.json`; a real fica em *user secrets*, ao lado de `SupabaseSettings:ChaveDeServico`
- [x] Entrada do usuário não é interpolada em HTML sem escape — não se aplica
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno — não se aplica

## Achados registrados durante a execução

- **`.Build()` do Testcontainers valida o Docker, não só `.StartAsync()`** — o primeiro `try/catch` escrito só cobria o segundo, e a mensagem amigável (RF-09) não disparava no caso mais comum (Docker desligado). Corrigido em `PostgresDeTeste` e em `AplicacaoEmExecucao` antes de fechar as respectivas fases.
- **O host direto do Supabase (`db.<ref>.supabase.co`) é IPv6 puro** — confirmado com `Resolve-DnsName` (só `AAAA`, sem `A`) e timeout de TCP real. O pooler em modo Session resolveu, exatamente como o plano previa.
- **Duas instâncias concorrentes de `dotnet run` colidiram aplicando a mesma migration** (`relation "Categoria" already exists`) — sobra de processos de sessões anteriores não encerrados, não um defeito do Postgres ou da migration. Resolvido matando os processos e recriando o schema antes de seguir.
- **Um `ExecuteSqlRawAsync` sem aspas em `CatalogoRepositoryIntegrationTests`** não tinha sido pego pela varredura inicial do plano (que olhou só o E2E) — apareceu como o único vermelho da T013, corrigido no ato.
- **Nenhum teste de ordenação ficou vermelho** — a imagem `postgres:17-alpine` usa collation `C` por padrão, que coincidiu com a ordem que o SQLite já dava para os dados testados. A regra não negociável da T014 não chegou a ser exercitada.
- **`Contains` sobre `NomeNormalizado` segue seguro por um mecanismo diferente** — `instr` (SQLite) virou `LIKE` (Postgres), e o EF Core escapa `%`/`_` nos dois. Confirmado, não assumido: o teste que trava isso passou sem alteração.
