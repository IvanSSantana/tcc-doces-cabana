# Checklist de conclusão — Envio de imagem do produto

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente
- [x] Todo `CA-xx` foi verificado manualmente na aplicação rodando — CA-01 a
      CA-05, CA-08, CA-09 e CA-11 por teste automatizado; CA-06, CA-07 e CA-10
      à mão, com a credencial real e o bucket já público (T032). O cadastro
      manual encontrou e derrubou um defeito que a suíte não pegaria: falta do
      cabeçalho `apikey`, exigido pelo formato novo de chave do Supabase.
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

## Pendências que existiram e foram encerradas

- [x] **Bucket `images` marcado como público no painel do Supabase** — feito
      pelo responsável durante a execução. Os seis endereços da massa de
      demonstração respondem `200`.
- [x] **`SupabaseSettings:ChaveDeServico` real** — configurada em *user
      secrets*. A categoria `Externo` passa, e o cadastro manual completo foi
      feito (T032).

## Dois achados da verificação manual

**1. `Authorization: Bearer` não basta.** O adaptador foi escrito no molde de
`FreteServiceMelhorEnvio`, que autentica só com `Authorization`. Isso basta para
uma chave JWT, e **não** basta para o formato novo do Supabase (`sb_secret_…`),
que exige também o cabeçalho `apikey` — sem ele, o Storage responde
`403 "Invalid Compact JWS"`. Seguir o padrão do vizinho é a regra certa e
continuaria sendo; o que ela não cobre é quando o serviço vizinho e o novo
autenticam de formas diferentes. Só um teste contra o serviço real acha isso.

**2. O E2E não estava isolado dos *user secrets* da máquina.** Descoberto como
consequência do achado anterior: assim que a chave real foi configurada em user
secrets para poder fazer a T032, o teste de "sem credencial" (CA-09) passou a
**cadastrar produto de verdade** — o processo filho roda com
`ASPNETCORE_ENVIRONMENT=Development`, e nesse ambiente o ASP.NET Core carrega os
segredos da máquina de quem executa. O `AplicacaoEmExecucao` controlava as
variáveis de ambiente, mas não o que entrava por trás delas.

Corrigido zerando `SupabaseSettings__ChaveDeServico` explicitamente no ramo
"sem credencial", em vez de apenas omiti-lo — variável de ambiente tem
precedência sobre user secrets. **Omitir não é o mesmo que negar**, e essa
diferença era invisível enquanto ninguém tinha o segredo configurado. A cotação
de frete escapa do mesmo furo por acidente: o ramo sem credencial dela força
`FreteSettings__UrlBase=http://localhost:9`, o que faz a cotação falhar mesmo
se um token vazar dos user secrets.
