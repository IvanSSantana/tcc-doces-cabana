# Tarefas — Envio de imagem do produto

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`… — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Duas coisas não são negociáveis.**
>
> **`enctype="multipart/form-data"` no formulário.** Sem ele o arquivo nunca
> chega, e o sintoma vira "imagem é obrigatória" — uma mensagem que descreve
> outra coisa. É a falha mais fácil de diagnosticar errado desta entrega, e o
> teste da T024 existe para apontar para cá quando acontecer.
>
> **Chave em branco recusa sem tocar a rede.** É isso que faz a suíte de ponta a
> ponta ser determinística e rodar offline. Se em algum momento parecer mais
> simples deixar o `401` do servidor responder, **releia o §5 do plano** — foi
> exatamente esse tipo de atalho que fez a spec `020` derrubar uma página com
> erro 500 por causa de um cabeçalho em branco.

---

## Fase 1 — Preparação

- [ ] **T001** — Criar branch `027-envio-de-imagem-do-produto` a partir de `main`.
- [ ] **T002** — Rodar `dotnet build` e as duas suítes; registrar o estado inicial.
- [ ] **T003** — Conferir no painel que o bucket `images` está **público**, e que as seis imagens respondem em `{UrlBase}/storage/v1/object/public/images/public/{arquivo}` — endereço sem `token`, sem validade. É pré-requisito da Fase 6: as URLs assinadas que existiam antes têm 384 caracteres e expiram em 2027-02-02 (spec §10).

## Fase 2 — O contrato e a verificação do arquivo

> Tudo nesta fase é `Application` pura: nenhuma rede, nenhuma configuração.

- [ ] **T004** — `DocesCabana.Tests/Units/Validators/ImagemParaEnvioDTOValidatorTests.cs`: extensão fora da lista recusada; `Content-Type` fora da lista recusado; acima de 5 MB recusado; caso válido aceito (RF-03/RF-04, CA-03/CA-04). Ver falhar.
- [ ] **T005** `[P]` — `DocesCabana.Tests/Units/Mappings/ProdutoMapperTests.cs` (já existe): `ComImagem` devolve **cópia** com o endereço preenchido, preservando os demais campos. Ver falhar. **Aqui, e não numa pasta `Units/DTOs` nova**: essa pasta não existe na organização de testes, e o precedente do método — `CarrinhoDTO.ComCotacao` — não tem teste próprio, é provado pelo consumidor. Um teste só, no vizinho mais próximo, em vez de inventar uma camada de teste para um método de duas linhas.
- [ ] **T006** — `DocesCabana.Application`: `Contracts/Services/IArmazenamentoDeImagem.cs`, `DTOs/ResultadoDoEnvioDeImagemDTO.cs`, `DTOs/ImagemParaEnvioDTO.cs`, `Validators/ImagemParaEnvioDTOValidator.cs` e `ProdutoDTO.ComImagem`. Formatos e teto de tamanho como **constantes do validador**, não configuração (plano §4). O contrato recebe `Stream`, nunca `IFormFile` (Princípio I).
- [ ] **T007** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde.

## Fase 3 — O adaptador

- [ ] **T008** — `DocesCabana.Tests/Units/Services/ArmazenamentoSupabaseTests.cs`, com `HttpMessageHandler` falso (mesmo padrão de `FreteServiceMelhorEnvioTests`): envio bem-sucedido devolve o endereço público montado corretamente (RF-06); **o caminho enviado usa `Guid` mais a extensão do original, e não contém o nome recebido** (RF-07/RN-02, CA-07); `Authorization: Bearer` e `Content-Type` vão na requisição. Ver falhar.
- [ ] **T009** `[P]` — Mesmo arquivo: `401`, outro `4xx` e `5xx` devolvem falha com mensagem **sem lançar** (RN-03); **chave em branco recusa sem fazer requisição nenhuma** (CA-09) — o handler falso prova isso contando as chamadas.
- [ ] **T010** `[P]` — `DocesCabana.Tests/Units/Services/ArmazenamentoSupabaseCaminhosDeFalhaTests.cs`, sem mock: `HttpClient` real contra `http://localhost:9` (conexão recusada) e contra endereço não roteável com timeout curto. Não lança, devolve falha. Mesmo par que a `020` usa — e pela mesma razão: a suíte padrão não pode depender de rede.
- [ ] **T011** — `DocesCabana.Infrastructure/Services/SupabaseSettings.cs` e `Services/ArmazenamentoSupabase.cs`. `UrlBase`, `Bucket`, `Pasta` e `TimeoutEmSegundos` com padrão; `ChaveDeServico` vazia por padrão, e vazia significa recusar.
- [ ] **T012** — `ApplicationDependencyInjection.cs`: `Configure<SupabaseSettings>` e `AddArmazenamentoDeImagem()` isolado, no formato de `AddFreteService()`. `appsettings.Example.json` ganha a seção com `ChaveDeServico` **vazia** (RF-09/RN-04); conferir que `appsettings.json` segue fora do versionamento.
- [ ] **T013** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — A borda web

- [ ] **T014** — `DocesCabana.Tests/Units/Controllers/Admin/ProdutoControllerTests.cs`: sem arquivo → `ModelState` inválido e o armazenamento **nunca é chamado** (RF-02, CA-02); arquivo recusado pelo validador → idem (CA-03/CA-04); envio falhou → volta a view e `IProdutoService.Cadastrar` **nunca roda** (RF-08, CA-08); sucesso → o endereço devolvido chega ao DTO e a ação redireciona (CA-06). Ver falhar.
- [ ] **T015** — `DocesCabana.MVC/Areas/Admin/Controllers/ProdutoController.cs`: a ordem dos oito passos do plano §5, **nesta ordem**. O `ModelState.Remove` de `ImagemUrl` vai comentado com o motivo e o precedente (`ContaController.AlterarDados`, CA-07 da `018`) — sem isso ele parece gambiarra para quem ler depois.
- [ ] **T016** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde.

## Fase 5 — A tela

- [ ] **T017** — `DocesCabana.MVC/Areas/Admin/Views/Produto/Cadastro.cshtml`: **`enctype="multipart/form-data"` no `<form>`**; campo de arquivo no lugar do campo de endereço (`accept` com os formatos aceitos, `required`); erro lido de `ViewData.ModelState["imagem"]`, como `_ItensDoCarrinho.cshtml` faz com o CEP. O campo de endereço **some do formulário** (RF-01, CA-01).
- [ ] **T018** — Rodar `dotnet test DocesCabana.Tests`: Fase 5 verde. **O E2E ainda não passa aqui, e isso é esperado** — `PaginaCadastroProduto` ainda escreve endereço num campo que deixou de existir; o ajuste é a Fase 7.

## Fase 6 — A massa de demonstração

- [ ] **T019** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: `ImagensDeExemplo` troca os seis links de pré-visualização do Drive pelos seis endereços públicos conferidos na T003 (RF-10). Comentar que são endereços públicos e por isso versionados, como os anteriores eram.
- [ ] **T020** — Apagar a base local, subir a aplicação **sem credencial configurada** e conferir: a semeadura completa (RF-11, CA-11) e os cem produtos exibem imagem no catálogo (CA-10). É a prova de que semear não depende do que a Fase 3 construiu.

## Fase 7 — Ponta a ponta

- [ ] **T021** — `DocesCabana.Tests.E2E/Paginas/PaginaCadastroProduto.cs`: `Preencher` deixa de escrever endereço e passa a anexar arquivo por `SetInputFilesAsync`, com um PNG mínimo **em memória** (`FilePayload`) — sem arquivo no disco, sem fixture para manter.
- [ ] **T022** `[P]` — `DocesCabana.Tests.E2E/Infraestrutura/AplicacaoEmExecucao.cs`: repassar `SupabaseSettings__ChaveDeServico` do ambiente de quem executa, quando presente — mesmo mecanismo que `FreteSettings__Token` já usa. Sem a variável, a aplicação sobe sem credencial de propósito.
- [ ] **T023** — `DocesCabana.Tests.E2E/Fluxos/CadastroDeProdutoTests.cs`: o teste de caminho feliz passa a `[Trait("Categoria", "Externo")]`, com o motivo escrito no arquivo. Os outros três (título e contenção, tela estreita, preço inválido) seguem na suíte padrão sem mudança.
- [ ] **T024** — Mesmo arquivo, **teste novo na suíte padrão**: sem credencial, o cadastro é recusado com a mensagem específica de "armazenamento não configurado" e nenhum produto é criado (CA-09). Afirmar a mensagem, não só a falha — é o que faz este teste apontar para o `enctype` esquecido em vez de esconder o problema.
- [ ] **T025** — Rodar as duas suítes: Fase 7 verde, sem credencial no ambiente.

## Fase 8 — Fechamento

- [ ] **T026** — `docs/arquitetura.md` §5: a linha de `/Admin/Produto/Cadastro` passa a mencionar o envio da imagem.
- [ ] **T027** `[P]` — `docs/arquitetura.md` §6: seção nova sobre o envio — por que o contrato fala `Stream` e não `IFormFile`, por que o arquivo é renomeado, por que chave em branco recusa sem tocar a rede, e por que a credencial nunca chega ao navegador.
- [ ] **T028** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"` na base inteira.
- [ ] **T029** — `specs/README.md`: a linha da feature. Registrar que a `027` **não é elo da cadeia de compra** — fica ao lado da `025`, fora dela — e que a troca do banco para Postgres é a `028`.
- [ ] **T030** — `specs/000-baseline/spec.md`: riscar as dívidas que esta entrega resolve, se houver.
- [ ] **T031** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero, **sem a credencial no ambiente** — é o estado em que qualquer pessoa clona o projeto.
- [ ] **T032** — Com a credencial configurada: rodar a categoria `Externo` e cadastrar um produto à mão, do formulário ao catálogo, conferindo que a imagem enviada é a que aparece (CA-06) e que o endereço gravado não contém o nome do arquivo original (CA-07).
- [ ] **T033** — Preencher `checklist.md`.
- [ ] **T034** — Atualizar o status da spec e do plano, e a linha em `specs/README.md`. Registrar o que **não** foi encerrado: sem credencial não se cadastra produto, e um teste de ponta a ponta vive fora da suíte padrão — consequência aceita ao recusar o adaptador local (spec §10).

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T017, T021 |
| RF-02 | T014, T015 |
| RF-03 | T004, T006 |
| RF-04 | T004, T006 |
| RF-05 | T015, T017 |
| RF-06 | T008, T011 |
| RF-07 | T008, T011 |
| RF-08 | T014, T015 |
| RF-09 | T012 |
| RF-10 | T019 |
| RF-11 | T020 |
| RN-01 | T014 |
| RN-02 | T008, T011 |
| RN-03 | T009, T010 |
| RN-04 | T012 |
| RN-05 | T003, T019 |
| CA-01 | T017 |
| CA-02 | T014, T024 |
| CA-03 | T004, T014 |
| CA-04 | T004, T014 |
| CA-05 | T015, T017 |
| CA-06 | T014, T032 |
| CA-07 | T008, T032 |
| CA-08 | T014, T015 |
| CA-09 | T009, T024 |
| CA-10 | T020, T032 |
| CA-11 | T020 |
