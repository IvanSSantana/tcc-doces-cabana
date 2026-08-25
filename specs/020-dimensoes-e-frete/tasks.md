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

- [ ] **T001** — Criar branch `020-dimensoes-e-frete` a partir de `main`. *(feita ao criar a pasta da spec)*
- [ ] **T002** — Rodar `dotnet build`, `dotnet test DocesCabana.Tests` e `dotnet test DocesCabana.Tests.E2E`; registrar o estado inicial (esperado: 552 e 157 verdes, herdados da `019`).
- [ ] **T003** — Localizar os pontos que esta entrega vai **quebrar de propósito**: todo `new Produto(` do repositório, porque o construtor ganha quatro parâmetros obrigatórios. **Levantado ao especificar, para não rederivar: são 66 chamadas em 12 arquivos** — `DocesCabana.MVC/Helpers/DbInitializer.cs` e onze de teste (`Units/Entities/ProdutoTests`, `Units/Mappings/ProdutoMapperTests`, `Units/Services/ProdutoServiceTests`, `Units/Helpers/GeradorDeAvaliacoesTests`, `Integration/DatabaseIntegrationTests`, `Integration/Repositories/` × 6). Conferir se o número mudou desde então; são ajustados na Fase 2, não removidos.

## Fase 2 — As medidas no domínio

- [ ] **T004** — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: `Theory` recusando peso, altura, largura e comprimento ≤ 0 no construtor (RF-01/RN-01, CA-01). Ver falhar.
- [ ] **T005** — Confirmar que T004 falha **por o construtor aceitar medida inválida**, não só por não compilar. Se a assinatura ainda não existe, o vermelho legítimo é o da compilação — mas o teste precisa ser reexecutado depois de T006 e falhar de novo pelo motivo certo antes de T007.
- [ ] **T006** `[P]` — `DocesCabana.Tests/Units/Entities/ProdutoTests.cs`: medidas válidas constroem e são lidas; `AlterarDimensoes` atualiza as quatro.
- [ ] **T007** — `DocesCabana.Domain/Entities/Produto.cs`: as quatro propriedades com `private set`, as quatro validações **antes** de qualquer atribuição, e `AlterarDimensoes(...)` (Princípio II, plano §4). **Sem valor padrão nos parâmetros** — padrão faria a RN-01 depender de quem chama lembrar.
- [ ] **T008** — Ajustar as 66 chamadas localizadas em T003. Mecânico, mas é o maior volume da entrega inteira. **Decisão do responsável ao especificar: sem valor padrão**, mesmo custando as 66 edições — medida é atributo essencial do produto, como `Nome` e `Preco`, que também não têm padrão; `status`, `descricao` e `semAcucar` têm porque são de fato opcionais. Um helper de teste centralizando a construção foi considerado e descartado: mesmo volume de edição agora, e a convenção do projeto ainda não tem essa infraestrutura.
- [ ] **T009** — Rodar `dotnet test DocesCabana.Tests`: Fase 2 verde.

## Fase 3 — Persistência e semeadura

- [ ] **T010** — `DocesCabana.Tests/Integration/Repositories/`: teste exigindo que **todo produto da base semeada** tenha as quatro medidas > 0 (RF-03, CA-03). Ver falhar.
- [ ] **T011** — `DocesCabana.Infrastructure/DatabaseContext/Configurations/ProdutoConfiguration.cs`: precisão `decimal(10,3)` nas quatro colunas.
- [ ] **T012** — Gerar a migration `AddProdutoPesoEDimensoes`.
- [ ] **T013** — Editar a migration para as duas etapas do plano §5: coluna com padrão temporário (exigência de `NOT NULL` sobre tabela povoada), depois os **quatro `UPDATE` por categoria** com os valores fixados na tabela do plano. Conferir que o `Down` desfaz.
- [ ] **T014** — `DocesCabana.MVC/Helpers/DbInitializer.cs`: `GerarProdutosMock` passa as medidas por categoria — o laço já conhece `nomeCategoria` no ponto de criação, então nenhuma consulta extra é necessária.
- [ ] **T015** — Aplicar a migration numa cópia do banco de desenvolvimento e conferir que os cem produtos ficaram com medidas coerentes com a categoria — Adega pesada e compacta, Souvenir leve e volumosa.
- [ ] **T016** — Rodar `dotnet test DocesCabana.Tests`: Fase 3 verde.

## Fase 4 — O cadastro do administrador

- [ ] **T017** `[P]` — `DocesCabana.Tests/Units/Validators/ProdutoDTOValidatorTests.cs`: um caso válido e um inválido por medida (RF-02, CA-01/CA-02). Ver falhar.
- [ ] **T018** — `DocesCabana.Application/DTOs/ProdutoDTO.cs`: as quatro propriedades.
- [ ] **T019** — `DocesCabana.Application/Validators/ProdutoDTOValidator.cs`: quatro `GreaterThan(0)` com mensagem em português.
- [ ] **T020** — `DocesCabana.Application/Mappings/ProdutoMapper.cs`: as quatro medidas nos dois sentidos.
- [ ] **T021** — `DocesCabana.MVC/Areas/Admin/Views/Produto/Cadastro.cshtml`: os quatro campos, usando `.linha-dupla` — o mesmo que a `016` consertou nesta tela para empilhar em tela estreita.
- [ ] **T022** — Rodar `dotnet test DocesCabana.Tests`: Fase 4 verde. **Fim da Fase A** — daqui em diante é cotação.

## Fase 5 — O contrato e a barreira de entrada

- [ ] **T023** `[P]` — `DocesCabana.Tests/Units/Validators/ConsultaDeFreteDTOValidatorTests.cs`: CEP obrigatório; formato inválido recusado; CEP pontuado aceito (RF-09, CA-10). Ver falhar.
- [ ] **T024** — `DocesCabana.Application/DTOs/ConsultaDeFreteDTO.cs` e `Validators/ConsultaDeFreteDTOValidator.cs`, reaproveitando `CepHelper.FormatoValido` (Princípio III).
- [ ] **T025** `[P]` — `DocesCabana.Application/DTOs/OpcaoDeFreteDTO.cs` e `DTOs/CotacaoDeFreteDTO.cs` (plano §4).
- [ ] **T026** — `DocesCabana.Application/Contracts/Services/IFreteService.cs`, com o comentário registrando que **falha de transporte não lança** — é a decisão que evita um ramo novo no `FilterException`.
- [ ] **T027** — `DocesCabana.Application/DTOs/CarrinhoDTO.cs` ganha `Cotacao` anulável; `Mappings/CarrinhoMapper.cs` ganha parâmetro opcional com padrão `null`. **Conferir que nenhum teste da `017` precisou mudar** — se algum precisou, o padrão `null` não foi aplicado corretamente.
- [ ] **T028** — Rodar `dotnet test DocesCabana.Tests`: Fase 5 verde.

## Fase 6 — A tela do carrinho

- [ ] **T029** — `DocesCabana.Tests/Units/Controllers/CarrinhoControllerTests.cs`: CEP na query devolve view com cotação; **CEP inválido nunca chama o serviço** (`Times.Never`, CA-10); requisição assíncrona devolve parcial; carrinho sem item disponível não cota (RF-11, CA-13); **carrinho com um item disponível e um indisponível passa ao serviço apenas o disponível** (RF-08/RN-03, CA-12 — `Verify` sobre a lista recebida). `IFreteService` mockado com `Moq`. Ver falhar.

  > O filtro de disponibilidade fica **aqui**, não no adaptador: quem monta o
  > pedido de cotação é quem sabe o que está disponível, e o adaptador não
  > precisa conhecer `ProdutoStatus`. Também é o que torna a CA-12 verificável
  > sem rede e sem credencial.
- [ ] **T030** — Confirmar que T029 falha por o parâmetro `cep` não existir — e não por erro alheio.
- [ ] **T031** — `DocesCabana.MVC/Controllers/CarrinhoController.cs`: `Index` aceita `cep`, valida antes de cotar, e mantém o caminho assíncrono da `017` (plano §4, "por que GET").
- [ ] **T032** — `DocesCabana.MVC/Views/Carrinho/_OpcoesDeFrete.cshtml`: a lista de opções (transportadora, serviço, preço, prazo — RF-06) e a mensagem de falha.
- [ ] **T033** — `DocesCabana.MVC/Views/Carrinho/_ItensDoCarrinho.cshtml`: a caixa de CEP no `<aside class="resumo-carrinho">`, entre o subtotal e o botão de finalizar, como `<form method="get">` — irmão dos formulários de quantidade, nunca aninhado. Só aparece havendo item disponível (RF-11).
- [ ] **T034** — **No mesmo arquivo:** corrigir o comentário obsoleto do botão de finalizar, que diz que o fechamento é a `019` (spec §10). Passa a apontar a `021`.
- [ ] **T035** `[P]` — `DocesCabana.MVC/wwwroot/css/pages/carrinho.css`: estilo da caixa e da lista.
- [ ] **T036** — `DocesCabana.MVC/wwwroot/js/components/carrinho.js`: intercepta o envio da caixa e troca o `#itens-carrinho`, reaproveitando o `X-Requested-With` que a `017` já implementou. **Sem script, o formulário precisa continuar funcionando** (RF-12/RN-04).
- [ ] **T037** — `DocesCabana.Tests.E2E/Fluxos/FreteTests.cs` e `Paginas/PaginaCarrinho.cs`: os três critérios que **não** dependem de credencial — CEP inválido mostra mensagem no campo e o carrinho segue inteiro (CA-10); carrinho vazio não oferece a caixa (CA-13); `UrlBase` inalcançável não derruba o carrinho (CA-11). Ver falhar.
- [ ] **T038** — Rodar as duas suítes: Fase 6 verde.

## Fase 7 — O adaptador, escrito

> Escrito **sem** credencial, contra a documentação oficial já obtida (plano §4).
> O que sai daqui compila, sobe, se comporta corretamente na ausência de token
> **e tem o mapeamento provado contra o exemplo documentado**. O que não sai é
> prova de que a documentação corresponde ao serviço de hoje — isso é a T047.

- [ ] **T039** — `DocesCabana.Infrastructure/Services/FreteSettings.cs`: `UrlBase`, `Token`, `CepDeOrigem`, `UserAgent`, `TimeoutEmSegundos`. **`UserAgent` é obrigatório pela API** (nome da aplicação e e-mail de contato) — é configuração, não literal no código.
- [ ] **T040** — **Teste do mapeamento, contra o exemplo da documentação.** Gravar o JSON de resposta documentado como fixture e exigir que ele vire as opções corretas: `custom_price` e não `price`; `custom_delivery_range` e não `delivery_time`; `company.name` como transportadora e `name` como serviço; `id` preservado como `ServicoId`; entrada sem preço utilizável descartada. **Um caso dedicado à armadilha da cultura**: `"37.79"` precisa virar `37,79` e não `3779,00` (plano §4, armadilha 2) — é a única das três que passaria despercebida por toda asserção relacional. Ver falhar. **Nada aqui precisa de credencial.**
- [ ] **T041** — `DocesCabana.Infrastructure/Services/MelhorEnvio/` (tipos de desserialização, isolados nesta pasta) e `Services/FreteServiceMelhorEnvio.cs`, fazendo T040 passar. Monta a requisição no modo `products` conforme a tabela do plano §4: **`insurance_value` recebe o preço do produto**, `options.receipt` e `own_hand` ficam `false`, e `services` é **omitido** para vir tudo que atende o trecho. Peso em kg, medidas em cm (RF-07). As linhas já chegam filtradas por disponibilidade (T029) — o adaptador não conhece `ProdutoStatus`. Trata `HttpRequestException`, `TaskCanceledException`, `422` e JSON ilegível devolvendo `Opcoes = []` e `Mensagem` (RF-10/RN-02) — **nunca lança**.
- [ ] **T042** — `DocesCabana.Infrastructure/DependencyInjections/ApplicationDependencyInjection.cs`: `Configure<FreteSettings>` e `AddHttpClient<IFreteService, FreteServiceMelhorEnvio>` com o timeout da configuração.
- [ ] **T043** `[P]` — `DocesCabana.MVC/appsettings.Example.json`: seção `FreteSettings` com `UrlBase` e `CepDeOrigem` preenchidos e `Token` **vazio** (RN-05). Conferir que nenhum arquivo versionado passou a conter credencial.
- [ ] **T044** — Testes dos caminhos de falha, **sem mock e sem credencial** (plano §6): `UrlBase = "http://localhost:9"` → conexão recusada; `Token = "invalido"` → 401; timeout curto. Os três devem devolver `Mensagem` e nunca lançar.
- [ ] **T045** — Rodar as duas suítes: Fase 7 verde. **A aplicação sobe e o carrinho funciona sem credencial nenhuma** — cotar apenas informa que não foi possível.

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

- [ ] **T051** — `docs/arquitetura.md` §5: a linha do carrinho passa a mencionar a cotação de frete.
- [ ] **T052** — `docs/arquitetura.md` §6: seção nova sobre a integração — o contrato, por que falha de rede não é exceção, e onde a credencial vive.
- [ ] **T053** — `docs/arquitetura.md` §9.3: `Pedido`, `ItemPedido` e `Pagamento` seguem sem comportamento (a `021` os liga); a contagem de tabelas sem uso não muda nesta entrega.
- [ ] **T054** — `grep -rn "spec 0[0-9][0-9]"` **e** `grep -rn "\b0[12][0-9]\b"` na base inteira. A segunda varredura é a lição da T034: a referência obsoleta do botão de finalizar escapou da `019` por não conter a palavra "spec".
- [ ] **T055** — `specs/README.md`: a cadeia passa a ser `020` cotação, `021` fechamento, `022` features, `023` estoque; a nota de numeração registra o sétimo deslocamento e o motivo (divisão de uma spec em duas).
- [ ] **T056** — `dotnet build` sem aviso novo e as duas suítes verdes, do zero, **sem a variável de ambiente da credencial** — é o estado em que qualquer pessoa clona o projeto.
- [ ] **T057** — Preencher `checklist.md`, registrando **o que foi provado por teste sem rede, o que foi provado contra a API e o que só a verificação manual mostrou**.
- [ ] **T058** — Atualizar o status da spec para *Implementada*, o do plano para *Executado*, e a linha da feature em `specs/README.md`. Registrar o que esta entrega **não** encerra: escolher e guardar o frete é da `021`, e o botão de finalizar segue desligado.

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
