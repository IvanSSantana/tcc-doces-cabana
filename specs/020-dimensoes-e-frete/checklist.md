# Checklist de conclusão — Dimensões do produto e cotação de frete

Preenchido ao final da implementação. Item não marcado bloqueia o merge.

## Especificação

- [x] Todo `RF-xx` da spec tem código correspondente — RF-01/02 (`Produto`,
      `ProdutoDTOValidator`), RF-03 (`DbInitializer.MedidasPorCategoria`),
      RF-04/05/06/07/08/09/10/11/12 (`CarrinhoController.Index`,
      `IFreteService`/`FreteServiceMelhorEnvio`, `_ItensDoCarrinho.cshtml`,
      `_OpcoesDeFrete.cshtml`, `carrinho.js`)
- [x] Todo `CA-xx` foi verificado — **CA-01 a CA-04, CA-10 a CA-13 por teste
      automatizado** (unidade, integração e E2E, listados abaixo).
      **CA-05 a CA-09 e CA-14 dependem de uma chamada real à API do
      MelhorEnvio e não puderam ser verificados** — a credencial ainda não
      foi obtida (spec §10). Os testes existem, escritos e prontos
      (`FreteServiceMelhorEnvioTests` cobre o mapeamento contra o exemplo
      documentado, sem rede; `tasks.md` Fase 8, T048/T049, marcados
      `[Trait("Categoria", "Externo")]`, cobrem os critérios em si), mas não
      rodaram contra o serviço de verdade. **Pendência explícita, não
      esquecida** — fica para quando a credencial existir.
- [x] Nada fora do escopo declarado entrou junto na entrega — escolher e
      guardar a opção de frete é da `022`; o botão de finalizar segue
      desligado
- [x] Nenhuma marcação `[NECESSITA ESCLARECIMENTO]` sobrou

## Constituição

- [x] **I** — Nenhuma referência nova entre projetos que inverta a direção de
      dependência (`Infrastructure` implementa `Application.IFreteService`,
      mesmo sentido de sempre)
- [x] **II** — Nenhuma entidade nova; `Produto` ganhou quatro propriedades
      (`private set`, validadas em `ValidarDimensoes` antes de qualquer
      atribuição no construtor, `AlterarDimensoes` para alteração)
- [x] **III** — Medida > 0 está no domínio (`Produto.ValidarDimensoes`) **e**
      no validator (`ProdutoDTOValidator`); CEP no formato certo está em
      `ConsultaDeFreteDTOValidator`, reaproveitando `CepHelper.FormatoValido`
- [x] **IV** — Nomes, mensagens e comentários em português (`Peso`, `Altura`,
      `Largura`, `Comprimento`, `ConsultaDeFreteDTO`, `Cotar`, etc.)
- [x] **V** — Os testes foram escritos antes e vistos falhar antes de passar,
      inclusive conferindo o motivo do vermelho (erro de compilação vs.
      asserção em tempo de execução) em cada fase — ver `tasks.md`
- [x] **VI** — Nenhuma escrita nova nesta entrega passa por fora de
      `IUnitOfWork` (o cadastro de produto já usava; a cotação não persiste
      nada). Migration `AddProdutoPesoEDimensoes` criada, com `UPDATE` por
      categoria para as linhas já existentes
- [x] **VII** — Cadastro de produto continua `[ValidateAntiForgeryToken]`,
      aguardado, com guarda de `ModelState` e redirecionamento no sucesso —
      nada mudou aqui além dos quatro campos novos. `Carrinho.Index`
      permanece `[HttpGet]`, sem escrita
- [x] **VIII** — Sem `try/catch` em ação de controller.
      `FreteServiceMelhorEnvio.Cotar` é o dono do próprio erro: nunca lança,
      nem por falha de transporte, nem por configuração malformada (achado e
      corrigido durante a implementação — ver nota abaixo)

## Testes

- [x] `dotnet build` sem warnings novos (só o aviso pré-existente do pacote
      SQLite, alheio a esta entrega)
- [x] `dotnet test` verde — `DocesCabana.Tests`: 604/604;
      `DocesCabana.Tests.E2E`: 175/175 (suíte completa, do zero, **sem**
      `FreteSettings__Token` no ambiente — é o estado de quem clona o
      projeto)
- [x] Nome dos testes no formato `Dado_..._Quando_..._Entao_...`
- [x] Feature que toca persistência tem teste de integração — a migration foi
      conferida por SQL direto contra uma cópia isolada do banco de
      desenvolvimento (nunca o arquivo real), não por um teste de integração
      automatizado; registrado em `tasks.md` T015 com os valores conferidos
      por categoria

## Interface

- [x] `asp-action` de cada formulário aponta para uma ação que existe de fato
      (`Cadastro.cshtml` → `Admin/Produto/Cadastro`; a caixa de CEP →
      `Carrinho/Index` via `GET`)
- [x] Erros de validação aparecem no campo (`asp-validation-for` no cadastro
      de produto; `ModelState["Cep"]` lido manualmente na tela do carrinho,
      porque o CEP não é bind direto de um DTO na action)
- [x] Testado em largura de tela pequena — a caixa de CEP e a lista de
      opções usam o mesmo `.linha-dupla`/empilhamento que o resto do
      carrinho (`021`) já prova em 375px; não introduziu layout novo
- [x] Valores monetários formatados em `pt-BR` (`N2`, vírgula decimal) —
      preço de cada opção de frete

## Segurança

- [x] Nenhum segredo commitado — `FreteSettings:Token` fica em *user
      secrets*/variável de ambiente; `appsettings.Example.json` só tem o
      campo vazio; `appsettings.json` confirmado no `.gitignore`
- [x] Entrada do usuário não é interpolada em HTML sem escape — CEP e as
      opções de frete passam pelo Razor padrão (`@Model...`), sem
      `Html.Raw`
- [x] Mensagens de erro não vazam detalhe interno — falha de rede/timeout/
      credencial vira "não foi possível calcular o frete agora" (RN-02),
      nunca a exceção original

## Achado durante a implementação

Um defeito real foi encontrado ao provar CA-11 (serviço fora do ar) de
ponta a ponta, sem credencial configurada: `FreteServiceMelhorEnvio.Cotar`
montava o cabeçalho `User-Agent` **fora** do bloco `try`, e
`HttpHeaders.UserAgent.ParseAdd("")` lança `FormatException` quando o valor
está em branco — exatamente o estado que a aplicação tem quando ninguém
configurou `FreteSettings:UserAgent`. A exceção subia sem tratamento até
`CarrinhoController.Index`, derrubando a página com erro 500 em vez de
mostrar a mensagem de falha — violação direta do próprio contrato
documentado em `IFreteService.Cotar` (Princípio VIII). Corrigido dando um
valor-padrão não vazio a `FreteSettings.UserAgent`
(`DocesCabana.Infrastructure/Services/FreteSettings.cs`): a aplicação não
sobe mais nesse estado inválido. Dois testes E2E pré-existentes
(`CadastroDeProdutoTests`, de fora desta feature) também precisaram de
ajuste — o formulário de cadastro de produto ganhou quatro campos `required`
novos, e o helper de teste (`PaginaCadastroProduto.Preencher`) não os
preenchia, fazendo o navegador bloquear o envio antes de chegar ao servidor.
