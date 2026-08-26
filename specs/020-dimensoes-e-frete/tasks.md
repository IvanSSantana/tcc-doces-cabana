# Tarefas — Dimensões do produto e cotação de frete

**Spec:** [`spec.md`](./spec.md) · **Plano:** [`plan.md`](./plan.md)

---

## Convenções

- `T001`, `T002`… — ordem de execução. Uma tarefa por commit lógico.
- `[P]` — pode rodar em paralelo com as `[P]` vizinhas (arquivos distintos).
- `🔒` — **bloqueada pela credencial do MelhorEnvio**, que ainda não existe
  (spec §10). Executar todas as demais primeiro.
- Toda tarefa nomeia o arquivo exato que cria ou altera.
- Tarefa de implementação sempre vem **depois** da tarefa de teste que a cobre —
  e o teste precisa ter falhado antes (Princípio V).
- Marque `[x]` só depois de `dotnet test` verde.

> **Só cinco tarefas estão bloqueadas, e todas são de verificação.**
>
> O adaptador do MelhorEnvio é **escrito** sem credencial (Fase 7) — o que a
> credencial destrava é **conferir** que ele funciona (Fase 8). E isso não é
> truque de sequenciamento: sem credencial, cotar cai no mesmo caminho de
> "serviço indisponível" que o plano já desenhou, então a aplicação sobe, o
> carrinho funciona e três critérios de aceite (CA-10, CA-11, CA-13) são
> plenamente verificáveis.
>
> **A ordem Fase A → Fase B não é negociável.** A cotação envia peso e medidas
> no corpo da requisição: as colunas precisam existir e estar preenchidas antes
> de haver o que enviar.

---

## Fase 1 — Preparação e linha de base

- [x] **T001** — Criar branch `020-dimensoes-e-frete` a partir de `main`. *(recriada a partir da `main` pós-`021`, para a implementação)*
- [x] **T002** — Build limpo. 571 unitários / 172 E2E verdes — **não 552/157**: a `021` (redesenho do carrinho) entrou entre a especificação e esta implementação e já mudou a linha de base. Registrado como fato novo, não como erro do plano.
- [x] **T003** — **Achado ao executar: a contagem de 66 estava incompleta.** `grep "new Produto("` não pega `new(...)` de tipo inferido (`ProdutoMapper.ToEntity`, e helpers `CriarProduto` em `CatalogoServiceTests`/`FavoritoServiceTests`/`ProdutoRepositoryIntegrationTests`) — três arquivos de teste a mais que o levantamento original não previa, mais o próprio `ProdutoMapper.cs` em produção. Ajustados todos na Fase 2 (não só a `DbInitializer.cs` de fora — ver nota abaixo).

## Fase 2 — As medidas no domínio

- [x] **T004** — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: `Theory` recusando peso, altura, largura e comprimento ≤ 0 no construtor (RF-01/RN-01, CA-01). Ver falhar.
- [x] **T005** — Confirmado: falha de **compilação** (`CS1503`/`CS7036`, argumento não corresponde) — o construtor ainda não aceita as medidas. É o vermelho legítimo previsto para este estágio.
- [x] **T006** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: medidas válidas constroem e são lidas; `AlterarDimensoes` atualiza as quatro.
- [x] **T007** — `DocesCabana.Domain/Entities/Produto.cs`: as quatro propriedades com `private set`, `ValidarDimensoes` chamada **antes** de qualquer atribuição, e `AlterarDimensoes(...)` (Princípio II, plano §4). Parâmetros inseridos logo após `imagemUrl`, junto dos outros obrigatórios, antes do grupo opcional (`status`, `id`, `descricao`, `semAcucar`). **Sem valor padrão.**
- [x] **T008** — Ajustados **todos** os pontos de construção, não só os 66 originais: `ProdutoMapper.ToEntity` (produção — bloqueava o build antes de qualquer teste rodar) e `ProdutoDTO`/`ProdutoMapper.ToDTO` ganharam as quatro propriedades (T018/T020 da Fase 4, adiantadas aqui por necessidade de compilação — o mapeamento é dependência dura, não op­cional); mais os quatro arquivos de teste que T003 corrigiu ter perdido; mais três literais de `ProdutoDTO` em `ProdutoServiceTests.Cadastrar` que só falhavam **em tempo de execução** (`ArgumentException: Peso deve ser maior que zero`), não em compilação — porque um DTO sem as quatro propriedades novas compila normalmente (elas nascem com `0m`), mas o `Produto` construído a partir dele já não aceita zero. **Decisão do responsável ao especificar, confirmada: sem valor padrão**, mesmo com o volume maior que o previsto.
- [x] **T009** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde (571/571).

## Fase 3 — Persistência e semeadura

- [x] **T010** — **Localização diferente do prevista, decisão registrada:** não `Integration/Repositories/`, mas `Units/Helpers/DbInitializerProdutosTests.cs`, seguindo o precedente já existente de `GerarAvaliacoesMock`/`GeradorDeAvaliacoesTests` — `GerarProdutosMock` é função pura (monta a lista em memória, não toca banco), e `InternalsVisibleTo` para `DocesCabana.Tests` já existe. Tornei `GerarProdutosMock` e `Taxonomia` `internal` (eram `private`) para o teste reaproveitar a taxonomia real em vez de duplicá-la. Prova RF-03/CA-03 (todo produto > 0 nas quatro medidas) e o par Adega/Souvenir do CA-09 (peso e volume invertidos). **Não houve ciclo vermelho-verde real**: a T014 já tinha sido implementada durante o desbloqueio de compilação da Fase 2 (`ProdutoMapper.ToEntity` e `DbInitializer` bloqueavam o build antes de qualquer teste rodar), então o teste nasceu confirmando comportamento já existente, não guiando implementação nova.
- [x] **T011** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/ProdutoConfiguration.cs`: precisão `decimal(10,3)` nas quatro colunas.
- [x] **T012** — Migration `AddProdutoPesoEDimensoes` gerada via `dotnet ef migrations add`.
- [x] **T013** — Migration editada: o EF Core já gera a coluna com padrão temporário (`defaultValue: 0m`) sozinho — a "primeira etapa" do plano §5 não precisou de edição manual. Acrescentados os **quatro `UPDATE` por categoria**, via `Subcategoria`/`Categoria` (`Produto.SubcategoriaId → Subcategoria.CategoriaId → Categoria.Nome`). `Down` desfaz removendo as colunas — sem necessidade de reverter os `UPDATE`, as colunas somem inteiras.
- [x] **T014** — **Feita antecipadamente na Fase 2** (ver nota em T003/T008): `GerarProdutosMock` já passa `MedidasPorCategoria[nomeCategoria]` nos dois pontos de criação (o produto curado "Raspa Tacho" e o laço principal), sem consulta extra — o laço já tinha `nomeCategoria` disponível.
- [x] **T015** — Migration aplicada numa **cópia isolada** do banco de desenvolvimento (`dotnet ef database update` apontando para uma cópia no scratchpad, nunca o arquivo real). Conferido por SQL direto: Adega 25 produtos (peso 1,2 kg, volume 2048 cm³), Doces 25 (0,4 kg, 2700 cm³), Empório 25 (0,5 kg, 1400 cm³), Souvenir 25 (0,3 kg, 15000 cm³) — cem produtos ao todo, e o par Adega/Souvenir com a proporção que o CA-09 pede (Souvenir 4× mais leve, 7× mais volumoso).
- [x] **T016** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde (573/573).

## Fase 4 — O cadastro do administrador

- [x] **T017** `[P]` — `DocesCabana.Tests/Units/Validators/ProdutoDTOValidatorTests.cs`: um caso válido e um inválido por medida (RF-02, CA-01/CA-02), mais um caso "todas válidas". Ver falhar (8 falhas — as 8 combinações de medida × valor inválido).
- [x] **T018** — `DocesCabana.Application/DTOs/ProdutoDTO.cs`: as quatro propriedades. **Feita antecipadamente na Fase 2** (bloqueava o build via `ProdutoMapper.ToEntity`).
- [x] **T019** — `DocesCabana.Application/Validators/ProdutoDTOValidator.cs`: quatro `GreaterThan(0)` com mensagem em português.
- [x] **T020** — `DocesCabana.Application/Mappings/ProdutoMapper.cs`: as quatro medidas nos dois sentidos. **Feita antecipadamente na Fase 2**, pelo mesmo motivo de T018.
- [x] **T021** — `DocesCabana.MVC/Areas/Admin/Views/Produto/Cadastro.cshtml`: os quatro campos em duas `.linha-dupla` (Peso+Altura, Largura+Comprimento) — o mesmo que a `016` consertou nesta tela para empilhar em tela estreita.
- [x] **T022** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde (582/582). **Fim da Fase A** — daqui em diante é cotação.

## Fase 5 — O contrato e a barreira de entrada

- [x] **T023** `[P]` — `DocesCabana.Tests/Units/Validators/ConsultaDeFreteDTOValidatorTests.cs`: CEP obrigatório; formato inválido recusado; CEP pontuado aceito (RF-09, CA-10). Ver falhar.
- [x] **T024** — `DocesCabana.Application/DTOs/ConsultaDeFreteDTO.cs` e `Validators/ConsultaDeFreteDTOValidator.cs`, reaproveitando `CepHelper.FormatoValido` (Princípio III), mesmo padrão de `EnderecoDTOValidator`.
- [x] **T025** `[P]` — `OpcaoDeFreteDTO.cs`/`CotacaoDeFreteDTO.cs`: **já existiam**, criados pela `021` (que chegou primeiro à implementação — plano §6 já previa isso). Conferidos, batem com o plano §4.
- [x] **T026** — `DocesCabana.Application/Contracts/Services/IFreteService.cs`, com o comentário registrando que falha de transporte não lança. **Correção ao plano, registrada:** a assinatura prevista (`IReadOnlyList<ItemDoCarrinhoDTO>`) não tinha de onde vir peso/dimensões — `ItemDoCarrinhoDTO` só carrega `ProdutoId`+`Quantidade` (carrinho de visitante). `LinhaDoCarrinhoDTO` ganhou as quatro medidas (preenchidas em `CarrinhoMapper`, que já recebe `Produto`) e passou a ser o tipo do parâmetro — nenhuma consulta extra, nenhum DTO novo.
- [x] **T027** — `CarrinhoDTO.Cotacao` e `CarrinhoMapper` com parâmetro opcional: **já existiam**, criados pela `021` pelo mesmo motivo de T025. Confirmado: **nenhum teste da `017`/`021` precisou mudar** (588/588 verde sem edição).
- [x] **T028** — Rodar `dotnet test DocesCabana.Tests`: Fase 5 verde (588/588).

## Fase 6 — A tela do carrinho

- [x] **T029** — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`: CEP na query devolve view com cotação; **CEP inválido nunca chama o serviço** (`Times.Never`, CA-10); carrinho sem item disponível não cota (RF-11, CA-13); **carrinho com um item disponível e um indisponível passa ao serviço apenas o disponível** (RF-08/RN-03, CA-12 — `Verify` sobre a lista recebida). `IFreteService` mockado com `Moq`; `IValidator<ConsultaDeFreteDTO>` **real**, não mockado — é lógica pura, mockar esconderia o que os testes de CEP querem provar. Ver falhar.

  > O filtro de disponibilidade fica **aqui**, não no adaptador: quem monta o
  > pedido de cotação é quem sabe o que está disponível, e o adaptador não
  > precisa conhecer `ProdutoStatus`. Também é o que torna a CA-12 verificável
  > sem rede e sem credencial.
- [x] **T030** — Confirmado: falha por o parâmetro `cep` não existir (`CS1739`) — e o construtor por não aceitar `IFreteService`/`IValidator` (`CS1729`).
- [x] **T031** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `Index` aceita `cep = null`, valida antes de cotar (`IValidator<ConsultaDeFreteDTO>.ValidateAsync` + `AddToModelState`), e mantém o caminho assíncrono da `017`. **Correção ao plano registrada em T026:** `Cotar` recebe `IReadOnlyList<LinhaDoCarrinhoDTO>` filtrado por `Disponivel`, não `ItemDoCarrinhoDTO`.
- [x] **T032** — `DocesCabana.MVC/Views/Carrinho/_OpcoesDeFrete.cshtml`: a lista de opções (transportadora, serviço, preço, faixa de prazo — RF-06) e a mensagem de falha.
- [x] **T033** — `DocesCabana.MVC/Views/Carrinho/_ItensDoCarrinho.cshtml`: a caixa de CEP dentro de `<aside class="resumo-carrinho">`, como `<form method="get">` — irmã dos formulários de quantidade, nunca aninhada. Só aparece havendo item disponível (RF-11, `Model.Linhas.Any(l => l.Disponivel)`). Erro de validação lido de `ModelState["Cep"]`.
- [x] **T034** — Comentário do botão de finalizar: **já corrigido na `021`** (aponta para a `022`). Conferido de novo aqui, nada a mudar.
- [x] **T035** `[P]` — `DocesCabana.MVC/wwwroot/css/pages/carrinho.css`: estilo da caixa, da lista de opções e da mensagem de falha.
- [x] **T036** — `DocesCabana.MVC/wwwroot/js/components/carrinho.js`: **`enviarConsulta` nova**, não a `enviar` existente — cotar é `GET`, não `POST` (plano §4), então a função precisa montar a URL com querystring em vez de enviar corpo. Intercepta `.formulario-frete-carrinho` e troca o `#itens-carrinho`. Sem script, o formulário continua funcionando por navegação comum (RF-12/RN-04).

> **Achado ao executar, ordem corrigida:** T037 (E2E) precisa da aplicação
> **de pé**, e ela não sobe sem `IFreteService` resolvível no DI — que só
> nasce na Fase 7. A Fase 7 (T039-T045) foi adiantada para antes de T037;
> só a Fase 8 (que exige a credencial) continua depois. Nenhuma tarefa foi
> pulada, só a ordem de execução mudou — a numeração de `tasks.md` permanece
> como referência do que cada tarefa faz.

- [x] **T037** — `DocesCabana.Tests.E2E/Fluxos/FreteTests.cs` e `Paginas/PaginaCarrinho.cs`: os três critérios que **não** dependem de credencial — CEP inválido mostra mensagem no campo e o carrinho segue inteiro (CA-10); carrinho vazio não oferece a caixa (CA-13); `UrlBase` inalcançável não derruba o carrinho (CA-11). *(executada depois da Fase 7 — ver nota acima)*
  **Bug real encontrado ao rodar o CA-11 sem credencial:** `FreteServiceMelhorEnvio.Cotar` lançava `FormatException` não capturada (`HttpHeaders.UserAgent.ParseAdd("")`, fora do `try`) quando `FreteSettings:UserAgent` ficava vazio — exatamente o estado que `AplicacaoEmExecucao` produz quando não há `FreteSettings__Token` no ambiente. Isso violava o próprio contrato documentado de `IFreteService.Cotar` (Princípio VIII — "nunca lança") e derrubava a página com 500 em vez de mostrar a mensagem de falha (RN-02). Corrigido dando um valor-padrão não vazio a `FreteSettings.UserAgent` (`DocesCabana.Infrastructure/Services/FreteSettings.cs`) — não é mais possível a aplicação subir com esse campo em branco. Também precisou corrigir dois testes E2E pré-existentes (`CadastroDeProdutoTests`, não desta feature) cujo helper `PaginaCadastroProduto.Preencher` não preenchia os quatro campos novos, hoje `required` no formulário — o navegador bloqueava o envio nativo do formulário antes de chegar ao servidor.
- [x] **T038** — Rodar as duas suítes: Fase 6 verde (`DocesCabana.Tests`: 604/604; `DocesCabana.Tests.E2E`: 175/175, sem `FreteSettings__Token` no ambiente).

## Fase 7 — O adaptador, escrito

> Escrito **sem** credencial, contra a documentação oficial já obtida (plano §4).
> O que sai daqui compila, sobe, se comporta corretamente na ausência de token
> **e tem o mapeamento provado contra o exemplo documentado**. O que não sai é
> prova de que a documentação corresponde ao serviço de hoje — isso é a T047.

- [x] **T039** — `DocesCabana.Infrastructure/Services/FreteSettings.cs`: `UrlBase`, `Token`, `CepDeOrigem`, `UserAgent`, `TimeoutEmSegundos`. **`UserAgent` é obrigatório pela API** — é configuração, não literal no código.
- [x] **T040** — **Teste do mapeamento, contra o exemplo da documentação**, com `HttpMessageHandler` falso devolvendo o JSON documentado (PAC, SEDEX e uma entrada sem `custom_price`). Nove testes: `custom_price` não `price`; **armadilha da cultura** (`"37.79"` → `37,79`, não `3779,00`, com `InvariantCulture` explícito); `custom_delivery_range` não `delivery_time`; transportadora/serviço mapeados; entrada sem preço descartada; `Mensagem` nula em resposta válida; `User-Agent`/`Authorization` enviados; `insurance_value` com o preço do produto; `422` vira `Mensagem` sem lançar. Ver falhar (não compilava). **Nada precisou de credencial.**
- [x] **T041** — `DocesCabana.Infrastructure/Services/MelhorEnvio/` (`RequisicaoDeCotacaoMelhorEnvio.cs`, `RespostaDeCotacaoMelhorEnvio.cs`) e `Services/FreteServiceMelhorEnvio.cs`. `insurance_value` recebe o preço do produto; `receipt`/`own_hand` `false`; `services` omitido. As linhas já chegam filtradas por disponibilidade — o adaptador não conhece `ProdutoStatus`. `HttpRequestException`, `TaskCanceledException`, resposta não-2xx e JSON ilegível devolvem `Opcoes = []` e `Mensagem` — nunca lançam.
- [x] **T042** — `ApplicationDependencyInjection.cs`: `Configure<FreteSettings>` e `AddFreteService()` isolado (mesmo padrão de `AddEmailService`) — `AddHttpClient<IFreteService, FreteServiceMelhorEnvio>` com `BaseAddress`/`Timeout` lidos de `FreteSettings` via `IOptions`.
- [x] **T043** `[P]` — `DocesCabana.MVC/appsettings.Example.json`: seção `FreteSettings` com `UrlBase`/`CepDeOrigem`/`UserAgent` preenchidos e `Token` **vazio**. Confirmado: `appsettings.json` (onde o token real vive) está no `.gitignore`, nenhuma credencial versionada.
- [x] **T044** — Testes de falha **sem mock**, apontando a configuração para servidor inalcançável (`http://localhost:9`, conexão recusada) e endereço não roteável com timeout de 1s. **Achado ao executar, escopo ajustado:** o terceiro caso do plano (`Token = "invalido"` → 401) exige um servidor de verdade respondendo — não há como simular 401 sem rede. Rodá-lo aqui, fora do filtro de testes externos, faria a suíte padrão depender da disponibilidade do MelhorEnvio para passar, contra o que o próprio plano promete em §9 ("`dotnet test` continua verde sem rede"). Deferido para a T048 (Fase 8), marcado `[Trait("Categoria", "Externo")]`, onde pertence de verdade.
- [x] **T045** — Rodar `dotnet test DocesCabana.Tests`: Fase 7 verde (604/604). A aplicação sobe e o carrinho funciona sem credencial nenhuma — confirmado na Fase 6 (E2E), executada em seguida por dependência estrutural (nota antes de T037).

## Fase 8 — Verificação contra a API 🔒

> **Bloqueada até a credencial do MelhorEnvio existir** (spec §10). Todas as
> demais tarefas devem estar concluídas antes de chegar aqui.

- [ ] **T046** 🔒 — Guardar a credencial em *user secrets*, nunca no repositório:
      `dotnet user-secrets --project DocesCabana.MVC set "FreteSettings:Token" "<chave>"`.
      Conferir `git status` limpo em seguida.
- [ ] **T047** 🔒 — Primeira chamada real, à mão, com o CEP de origem `17340-001`. **Conferir se a resposta do serviço corresponde à documentação** contra a qual a T040/T041 foram escritas — não é mais descoberta de formato, é conferência de que a documentação não envelheceu. Corrigir o que divergir; a desserialização isolada em `Services/MelhorEnvio/` torna o conserto local.
- [ ] **T048** 🔒 — `DocesCabana.Tests.E2E/Fluxos/FreteTests.cs`, marcados `[Trait("Categoria", "Externo")]`: cotação devolve opções com preço e prazo positivos (CA-05); visitante cota sem conta (CA-06); sem JavaScript funciona (CA-14). **Nenhuma asserção sobre valor absoluto** (plano §6).
- [ ] **T049** 🔒 — Testes de relação, mesmo `[Trait]`: CEP distante custa e demora mais que CEP próximo (CA-08); mais unidades custam mais (CA-07); carrinho só de Souvenir custa mais que carrinho só de Adega, apesar de pesar menos (CA-09) — é a prova de que peso **e** medidas chegam à API corretamente, na unidade certa.
- [ ] **T050** 🔒 — `DocesCabana.Tests.E2E/Infraestrutura/AplicacaoEmExecucao.cs`: passar `FreteSettings__Token` por variável de ambiente, lido do ambiente de quem executa — mesmo lugar onde `EmailSettings__Adaptador` já é passado. Conferir que a suíte **sem** a variável continua verde, com os testes de rede fora do filtro.

## Fase 9 — Documentação e fechamento

- [x] **T051** — `docs/arquitetura.md` §5: a linha do `/Carrinho` na tabela passou a citar `IFreteService` e a cotação de frete (`020`, §6.10).
- [x] **T052** — `docs/arquitetura.md`: seção nova §6.10 — o contrato (nunca lança), a armadilha da cultura no `decimal.Parse`, o peso/dimensões por categoria e onde a credencial vive.
- [x] **T053** — `docs/arquitetura.md` §9.3: conferido, sem mudança — `Estoque`, `Pedido`, `ItemPedido`, `Pagamento`, `Promocao` seguem sem comportamento; a contagem (cinco tabelas) não mudou nesta entrega.
- [x] **T054** — Varredura feita (`spec 0[0-9][0-9]` e `\b02[0-3]\b`). Nenhuma referência obsoleta encontrada introduzida por esta feature — os comentários novos em `_ItensDoCarrinho.cshtml`/`_OpcoesDeFrete.cshtml` citam `020`/`021`/`022` corretamente.
- [x] **T055** — `specs/README.md`: status de `020` passou de *Especificada* para *Implementada* nas duas tabelas (linha de cadeia e linha de "o que destrava"), com link de `checklist`; a nota de "Ordem executada" ganhou `020` ao final (depois de `021`) e uma frase sobre a Fase 8 pendente, sem bloquear o restante.
- [x] **T056** — `dotnet build` sem aviso novo; `DocesCabana.Tests` 604/604 e `DocesCabana.Tests.E2E` 175/175, do zero, **sem** `FreteSettings__Token` no ambiente.
- [x] **T057** — `checklist.md` preenchido, distinguindo o que foi provado sem rede (a maior parte — CA-01 a CA-04, CA-10 a CA-13) do que só será provado contra a API real quando a credencial existir (CA-05 a CA-09, CA-14 — Fase 8).
- [x] **T058** — Status da spec → *Implementada*; status do plano → *Executado*; `specs/README.md` atualizado (T055). Registrado nos dois lugares o que esta entrega **não** encerra: escolher e guardar a opção de frete é da `022` (não `021`, que é o redesenho da tela); o botão de finalizar segue desligado.

> **Fase 8 (T046–T050) permanece bloqueada** pela ausência da credencial do
> MelhorEnvio — cadastro sob verificação, fora do controle desta
> implementação (spec §10). Todas as demais 53 tarefas foram executadas.
> Nenhuma decisão desta entrega depende da credencial; só a confirmação
> final de que a documentação da API não envelheceu, e os critérios de
> aceite que exigem uma resposta real do serviço.

---

## Rastreabilidade

| Requisito | Tarefas |
|---|---|
| RF-01 | T004, T007 |
| RF-02 | T017, T019, T021 |
| RF-03 | T010, T013, T014, T015 |
| RF-04 | T033, T037 |
| RF-05 | T048 |
| RF-06 | T032, T040, T048 |
| RF-07 | T041, T049 |
| RF-08 | T029, T031 |
| RF-09 | T023, T024, T029, T031 |
| RF-10 | T032, T041, T044 |
| RF-11 | T029, T033, T037 |
| RF-12 | T033, T036, T048 |
| RN-01 | T004, T007 |
| RN-02 | T041, T044, T037 |
| RN-03 | T029, T031 |
| RN-04 | T033, T036 |
| RN-05 | T043, T046, T050 |
| RN-06 | T032, T041 |
| CA-01 | T004, T017, T021 |
| CA-02 | T017, T021 |
| CA-03 | T010, T013, T015 |
| CA-04 | T033, T037 |
| CA-05 | T048 |
| CA-06 | T048 |
| CA-07 | T049 |
| CA-08 | T049 |
| CA-09 | T013, T049 |
| CA-10 | T023, T029, T037 |
| CA-11 | T037, T044 |
| CA-12 | T029, T031 |
| CA-13 | T029, T033, T037 |
| CA-14 | T048 |
