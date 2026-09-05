# Checklist de conclusão — Envio de imagem do produto

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [ ] Todo `CA-xx` foi verificado manualmente na aplicação rodando — **CA-06,
      CA-07 e CA-10 não puderam ser verificados**: dependem do bucket
      `images` estar marcado como público no painel do Supabase, e ele ainda
      não está (`.../object/public/...` devolve `400 Bucket not found`,
      enquanto o mesmo arquivo pelo endereço assinado responde). Os demais
      (CA-01 a CA-05, CA-08, CA-09, CA-11) foram verificados por teste
      automatizado.
- [x] Nada fora do escopo declarado entrou junto na entrega
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção de dependência (`IArmazenamentoDeImagem` fica em `Application`, `ArmazenamentoSupabase` em `Infrastructure`; o contrato fala `Stream`, nunca `IFormFile`)
- [x] **II** — Entidades novas têm `private set`, validam no construtor e têm `protected Ctor()` — nenhuma entidade nova nesta feature
- [x] **III** — Regras críticas estão no validator **e** no domínio (`ImagemParaEnvioDTOValidator` na barreira de entrada; `Produto.ImagemUrl` segue obrigatória no construtor)
- [x] **IV** — Nomes, mensagens e comentários em português
- [x] **V** — Os testes foram escritos antes e falharam antes de passar
- [x] **VI** — Toda escrita chama `IUnitOfWork`; nenhuma migration — não houve mudança de esquema
- [x] **VII** — `[ValidateAntiForgeryToken]` em todo POST, chamadas assíncronas aguardadas, rota administrativa autorizada, POST-Redirect-Get no sucesso
- [x] **VIII** — Sem `try/catch` em ação de controller

## Testes

- [x] `dotnet build` sem warnings novos
- [x] `dotnet test` verde (679 unidade + 185 E2E, `Categoria!=Externo`, sem credencial no ambiente)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração — não se aplica: nenhuma mudança de esquema, e o caminho de gravação já é coberto pelos testes existentes de `ProdutoRepository`

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
- [x] Erros de validação aparecem no campo (`asp-validation-for` ou, para o arquivo, `ViewData.ModelState["imagem"]`) e não só no resumo
- [x] Testado em largura de tela pequena (testes E2E herdados, sem mudança)
- [x] Valores monetários e datas formatados em `pt-BR` — sem mudança nesta feature

## Segurança

- [x] Nenhum segredo commitado (`SupabaseSettings:ChaveDeServico` fica vazio em `appsettings.Example.json`; `appsettings.json` segue fora do versionamento)
- [x] Entrada do usuário não é interpolada em HTML sem escape
- [x] Mensagens de erro não vazam existência de conta nem detalhe interno

## Pendências registradas (não bloqueiam o código, bloqueiam o uso real)

- [ ] **Bucket `images` marcado como público no painel do Supabase** —
      Storage → images → Settings → Public bucket. Sem isso, ninguém
      cadastra produto de verdade (o upload em si até funcionaria com uma
      `service_role` válida, mas os endereços gravados — e os da massa de
      demonstração — não resolvem para ninguém).
- [ ] **`SupabaseSettings__ChaveDeServico` real** para rodar a categoria
      `Externo` e o caminho feliz de ponta a ponta (T032).
