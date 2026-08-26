# Plano Técnico — Dimensões do produto e cotação de frete

**Spec:** [`spec.md`](./spec.md) · **Status:** Executado
**Criado em:** 2026-08-25

---

## 1. Resumo da abordagem

Duas metades, nesta ordem, e a ordem é dependência técnica, não preferência.

**Fase A — as medidas.** `Produto` ganha `Peso`, `Altura`, `Largura` e
`Comprimento`, validados no construtor como `Preco` já é. Migration com quatro
`UPDATE`, um por categoria, para os cem produtos que já existem. O formulário do
administrador ganha os quatro campos. **Não depende de credencial nenhuma.**

**Fase B — a cotação.** `IFreteService` na `Application`, uma implementação na
`Infrastructure` batendo no MelhorEnvio. A tela do carrinho ganha a caixa de CEP.
**Depende da credencial**, que ainda não existe (spec §10).

A Fase B envia peso e medidas no corpo da requisição — por isso a Fase A vem
antes, mesmo que a credencial chegasse hoje.

### As duas decisões que moldam tudo

**Uma implementação, não duas.** Não há adaptador simulado. O MelhorEnvio já
calcula peso cubado e faz o próprio empacotamento; reimplementar isso do lado de
cá criaria uma segunda resposta para a mesma pergunta, que nunca poderia ser
conferida contra a primeira. Consequência prática: nada de `Pacote`, de
consolidação de volume, de `RegiaoDeEntrega` ou de tabela de preços — tudo isso
foi desenhado e descartado (§8).

**Falha de serviço externo não é exceção.** Serviço fora do ar é condição
esperada, e o Princípio VIII reserva exceção para o que não é. `Cotar` devolve
`CotacaoDeFreteDTO` com `Opcoes` vazia e `Mensagem` preenchida. Nenhuma exceção
atravessa a fronteira, o `FilterException` não ganha ramo novo, e — de graça —
**a aplicação sem credencial se comporta exatamente como a aplicação com o
serviço fora do ar**: a tela diz que não deu para calcular, e o carrinho segue
inteiro.

## 2. Verificação constitucional

| Princípio | Situação |
|---|---|
| **I — Direção de dependência** | ✅ `IFreteService` é declarado na `Application`, implementado na `Infrastructure`. Nenhuma referência de projeto nova. `HttpClient` só aparece na `Infrastructure` |
| **II — Domínio se defende** | ✅ As quatro medidas nascem com `private set`, validadas no construtor, alteráveis só por `AlterarDimensoes` |
| **III — Duas barreiras** | ✅ Medidas: `ProdutoDTOValidator` (entrada) + construtor de `Produto` (invariante). CEP: `ConsultaDeFreteDTOValidator` (entrada) — sem invariante de domínio, porque CEP de consulta não vira estado |
| **IV — Português** | ✅ `IFreteService`, `Cotar`, `OpcaoDeFreteDTO`, `CotacaoDeFreteDTO`, `FreteSettings`. Os campos do JSON do MelhorEnvio são estrangeiros por serem da API, e ficam confinados ao tipo de desserialização |
| **V — Teste antes** | ✅ Ciclo vermelho-verde nas duas fases. Testes de rede separados por `[Trait]` (§6) |
| **VI — Persistência escondida** | ✅ Migration versionada `AddProdutoPesoEDimensoes`, mapeamento em `ProdutoConfiguration`. A cotação não escreve nada, então não há `IUnitOfWork` envolvido |
| **VII — Seguro na borda** | ✅ Credencial em *user secrets*. A cotação é leitura: `[HttpGet]`, sem antiforgery — ver §5 para por que GET |
| **VIII — Dono do erro** | ✅ Domínio lança `ArgumentException` para medida inválida. Falha de rede **não** vira exceção, por ser esperada |

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Mudança |
|---|---|
| `Entities/Produto.cs` | Quatro propriedades novas, quatro validações no construtor, `AlterarDimensoes(...)` |

Nada além disso. `CepHelper` **não** muda — `FormatoValido` já basta, e a
derivação de região que um simulador pediria não existe mais.

### `DocesCabana.Application`

| Arquivo | Mudança |
|---|---|
| `Contracts/Services/IFreteService.cs` | **novo** — o contrato |
| `DTOs/OpcaoDeFreteDTO.cs` | **novo** |
| `DTOs/CotacaoDeFreteDTO.cs` | **novo** |
| `DTOs/ConsultaDeFreteDTO.cs` | **novo** — só o CEP, para a barreira de entrada |
| `Validators/ConsultaDeFreteDTOValidator.cs` | **novo** |
| `DTOs/ProdutoDTO.cs` | Quatro propriedades |
| `Validators/ProdutoDTOValidator.cs` | Quatro regras `GreaterThan(0)` |
| `DTOs/CarrinhoDTO.cs` | Ganha `Cotacao` (anulável) |
| `Mappings/CarrinhoMapper.cs` | Parâmetro opcional com padrão `null` — os testes da `017` seguem compilando sem alteração |
| `Mappings/ProdutoMapper.cs` | As quatro medidas nos dois sentidos |

### `DocesCabana.Infrastructure`

| Arquivo | Mudança |
|---|---|
| `Services/FreteServiceMelhorEnvio.cs` | **novo** — `HttpClient`, `Bearer`, mapeia a resposta |
| `Services/FreteSettings.cs` | **novo** — `UrlBase`, `Token`, `CepDeOrigem`, `UserAgent`, `TimeoutEmSegundos` |
| `Services/MelhorEnvio/` | **nova pasta** — tipos de desserialização do JSON, isolados |
| `DependencyInjections/ApplicationDependencyInjection.cs` | `Configure<FreteSettings>` + `AddHttpClient<IFreteService, FreteServiceMelhorEnvio>` |
| `DatabaseContext/Configurations/ProdutoConfiguration.cs` | Precisão decimal das quatro colunas |
| `Migrations/…_AddProdutoPesoEDimensoes.cs` | **nova** — colunas + quatro `UPDATE` por categoria |

### `DocesCabana.MVC`

| Arquivo | Mudança |
|---|---|
| `Controllers/CarrinhoController.cs` | `Index` passa a aceitar `cep`; `UsuarioAtualId` já existe |
| `Views/Carrinho/_ItensDoCarrinho.cshtml` | Caixa de CEP no `<aside class="resumo-carrinho">`; **corrigir o comentário obsoleto do botão de finalizar** (spec §10) |
| `Views/Carrinho/_OpcoesDeFrete.cshtml` | **nova** — a lista de opções e a mensagem de falha |
| `wwwroot/js/components/carrinho.js` | Intercepta o envio da caixa, reaproveitando o caminho assíncrono da `017` |
| `wwwroot/css/pages/carrinho.css` | Estilo da caixa e da lista |
| `Areas/Admin/Views/Produto/Cadastro.cshtml` | Quatro campos, usando `.linha-dupla` |
| `Helpers/DbInitializer.cs` | Medidas por categoria em `GerarProdutosMock` |
| `appsettings.Example.json` | Seção `FreteSettings` com o token vazio |

## 4. Contratos

```csharp
// Application/Contracts/Services/IFreteService.cs
public interface IFreteService
{
    // Nunca lança por falha de transporte: indisponibilidade do serviço é
    // condição esperada e volta em CotacaoDeFreteDTO.Mensagem (RN-02).
    Task<CotacaoDeFreteDTO> Cotar(string cepDestino, IReadOnlyList<ItemDoCarrinhoDTO> itens);
}

// Application/DTOs/OpcaoDeFreteDTO.cs
public record OpcaoDeFreteDTO(
    int ServicoId,              // é por ele que a 022 casa a re-cotação no fechamento
    string Transportadora,      // "Correios", "Jadlog"
    string Servico,             // "PAC", "SEDEX", ".Package"
    decimal Preco,
    int PrazoMinimoEmDias,
    int PrazoMaximoEmDias);

// Application/DTOs/CotacaoDeFreteDTO.cs
public record CotacaoDeFreteDTO(
    string? CepConsultado,
    IReadOnlyList<OpcaoDeFreteDTO> Opcoes,
    string? Mensagem);      // preenchida só quando não foi possível cotar
```

```csharp
// Domain/Entities/Produto.cs
public decimal Peso { get; private set; }          // kg
public decimal Altura { get; private set; }        // cm
public decimal Largura { get; private set; }       // cm
public decimal Comprimento { get; private set; }   // cm

public void AlterarDimensoes(decimal peso, decimal altura, decimal largura, decimal comprimento);
```

As quatro entram no construtor **sem valor padrão**: um padrão faria a RN-01
depender de quem chama lembrar de passar, que é exatamente o que a invariante
existe para não depender.

### Por que `GET` e não `POST`

Cotar é leitura — não muda estado nenhum. O Princípio VII exige antiforgery em
`[HttpPost]`, não proíbe `GET` para consulta. Consequências que decidem a
escolha:

- Um `<form method="get">` funciona **sem JavaScript** (RF-12/RN-04), e o
  formulário é irmão dos formulários de quantidade, não aninhado neles
- Reaproveita `/Carrinho?cep=...`, sem rota nova
- Cai no "um endereço, duas representações" que a `014` estabeleceu: com script,
  o mesmo endereço devolve só o pedaço que mudou

### O mapeamento contra a API, e três armadilhas

A documentação foi obtida antes de implementar. O endereço é
`POST https://sandbox.melhorenvio.com.br/api/v2/me/shipment/calculate`.

**Cabeçalhos.** `Accept: application/json`, `Content-Type: application/json`,
`Authorization: Bearer <token>` e — **obrigatório** — `User-Agent` com nome da
aplicação e e-mail de contato técnico. Sem ele a API recusa, e nenhum exemplo de
código em C# menciona isso: por isso `UserAgent` é campo de configuração, não
literal no código.

**Ida** (modo `products`, não `volumes` — a API faz o próprio empacotamento):

| Campo | Origem |
|---|---|
| `from.postal_code` | `FreteSettings.CepDeOrigem` = `17340001` |
| `to.postal_code` | o CEP informado |
| `products[].id` | `ProdutoId` |
| `products[].width` / `height` / `length` | `Largura` / `Altura` / `Comprimento`, em cm |
| `products[].weight` | `Peso`, em kg |
| `products[].insurance_value` | **`Produto.Preco`** — a API multiplica pela quantidade |
| `products[].quantity` | a quantidade da linha |
| `options.receipt` / `own_hand` | `false` — os dois só encarecem |
| `services` | **omitido**, para vir tudo que atende o trecho |

**Volta:**

| Nosso campo | JSON |
|---|---|
| `ServicoId` | `id` |
| `Transportadora` | `company.name` |
| `Servico` | `name` |
| `Preco` | **`custom_price`** |
| `PrazoMinimoEmDias` / `PrazoMaximoEmDias` | **`custom_delivery_range.min` / `.max`** |

**Armadilha 1 — os campos óbvios são os errados.** A documentação instrui a usar
`custom_price` e `custom_delivery_time`/`custom_delivery_range`, não `price` e
`delivery_time`: os primeiros refletem taxas e descontos configurados na conta
da loja. Mapear os nomes naturais funcionaria até a loja configurar qualquer
customização, e então passaria a cobrar errado em silêncio.

**Armadilha 2 — o preço vem como texto, e a aplicação é `pt-BR`.**
`"custom_price": "37.79"` é string com ponto decimal, e o `Program.cs` fixa a
cultura em `pt-BR`, onde ponto separa milhar:

```csharp
decimal.Parse("37.79")                               // → 3779,00  ✗
decimal.Parse("37.79", CultureInfo.InvariantCulture) // →   37,79  ✓
```

Um frete de R$ 37,79 viraria R$ 3.779,00 — e passaria em qualquer asserção
relacional do tipo "preço > 0" ou "distante custa mais". **Tem teste próprio.**

**Armadilha 3 — entrada sem preço utilizável.** A documentação mostra só o caso
de sucesso, mas nada garante que toda entrada do vetor traga `custom_price`
válido. Entrada sem preço utilizável é descartada antes de virar opção na tela:
três linhas que protegem a RF-06 (toda opção exibida tem preço e prazo).

**Erros.** `422` devolve `{ message, errors }` para dados inválidos — tratado
como as demais falhas, virando `Mensagem` (RN-02), nunca exceção.

## 5. Modelo de dados

Quatro colunas em `Produto`, `NOT NULL`, `decimal(10,3)` — três casas bastam
para gramas e milímetros.

A migration tem duas partes, e a ordem importa:

1. `AddColumn` com valor padrão temporário, porque `NOT NULL` sobre tabela
   povoada exige um valor
2. Quatro `Sql("UPDATE Produto SET … WHERE SubcategoriaId IN (SELECT … Categoria.Nome = '…')")`

Os valores, decididos ao especificar e fixados aqui para não serem rederivados:

| Categoria | Peso (kg) | A × L × C (cm) | Racional |
|---|---|---|---|
| Adega | 1,200 | 32 × 8 × 8 | garrafa: pesada e compacta |
| Doces | 0,400 | 12 × 15 × 15 | pote ou lata |
| Empório | 0,500 | 14 × 10 × 10 | vidro de geleia, pacote de café |
| Souvenir | 0,300 | 20 × 25 × 30 | pelúcia: leve e volumosa |

Adega e Souvenir são o par que satisfaz o CA-09: a pelúcia pesa **quatro vezes
menos** que a garrafa e ocupa **sete vezes mais volume**. É o caso em que a
cubagem da transportadora domina o peso real. Um valor único para os cem
produtos tornaria o CA-09 impossível de escrever.

`DbInitializer.GerarProdutosMock` já percorre `foreach (var (nomeCategoria, …) in Taxonomia)`
— sabe a categoria no ponto exato em que cria cada produto, então recebe as
mesmas medidas sem nenhuma consulta extra.

## 6. Estratégia de teste

### O que não toca a rede — roda em qualquer máquina

| Camada | Casos |
|---|---|
| `Units/Entities/ProdutoTests.cs` | Cada uma das quatro medidas ≤ 0 recusada no construtor (`Theory`); `AlterarDimensoes` atualiza; medidas válidas constroem |
| `Units/Validators/ProdutoDTOValidatorTests.cs` | Um caso válido e um inválido por medida |
| `Units/Validators/ConsultaDeFreteDTOValidatorTests.cs` | CEP obrigatório; formato inválido recusado; CEP pontuado aceito |
| `Units/Controllers/CarrinhoControllerTests.cs` | CEP na query devolve view com cotação; **CEP inválido nunca chama o serviço** (`Times.Never`); requisição assíncrona devolve parcial; carrinho sem item disponível não cota |
| `Integration/Repositories/` | Base semeada: **todo produto tem as quatro medidas > 0** — prova migration e seed juntos |

`IFreteService` é mockado com `Moq` nos testes de controlador. Isso **não é um
simulador**: é uma linha por teste, e o que se testa ali é o controlador, não o
frete.

### O que toca a rede — `[Trait("Categoria", "Externo")]`

Fora do filtro padrão, seguindo o precedente de `[Trait("Categoria", "E2E")]` em
`TesteE2E.cs`. Quem tem a credencial executa explicitamente.

| Caso | Como |
|---|---|
| Cotação real devolve opções | CEP válido → lista não vazia, cada opção com preço > 0 e prazo > 0 |
| Distância muda o preço (CA-08) | CEP do Sudeste vs. CEP do Norte, mesmo carrinho |
| Volume conta (CA-09) | Carrinho só com Souvenir vs. só com Adega |
| Quantidade muda o preço (CA-07) | Mesmo carrinho com 1 e com 5 unidades |

**Nenhuma asserção sobre valor absoluto.** Tarifa muda, transportadora entra e
sai do catálogo. As asserções são de estrutura e de relação — mesmo critério que
a `019` adotou para as notas da vitrine, onde a ordem relativa é estável e o
nome do produto não é.

### Os caminhos de falha — sem mock, sem credencial

Aqui a ausência de simulador vira vantagem: dá para exercitar o `HttpClient` de
verdade **apontando a configuração para o lugar errado**.

| Caso | Configuração |
|---|---|
| Serviço inalcançável | `UrlBase = "http://localhost:9"` → conexão recusada |
| Credencial inválida | `Token = "invalido"` → 401 |
| Estouro de tempo | `TimeoutEmSegundos = 1` contra endereço que não responde |

Os três exercitam o `try/catch` real e a mensagem real. Um `HttpMessageHandler`
mockado só simularia isso. **E estes rodam sem credencial** — ficam fora do
`[Trait]` de rede.

### Ponta a ponta — `E2E/Fluxos/FreteTests.cs`

`AplicacaoEmExecucao` já injeta configuração de serviço externo por variável de
ambiente (`EmailSettings__Adaptador`, `Admin__SenhaInicial`); `FreteSettings__Token`
entra no mesmo lugar, lido do ambiente de quem roda.

| Caso | Depende de credencial |
|---|---|
| Digitar CEP mostra opções com preço e prazo (CA-05) | sim |
| Visitante calcula sem conta (CA-06) | sim |
| Sem JavaScript funciona (CA-14) | sim |
| CEP inválido mostra mensagem no campo, carrinho intacto (CA-10) | **não** — recusado antes de consultar |
| Carrinho vazio não oferece a caixa (CA-13) | **não** |
| Serviço fora do ar não derruba o carrinho (CA-11) | **não** — `UrlBase` inalcançável |

Três dos seis rodam sem credencial. Não é acaso: são exatamente os que a
barreira de entrada e o tratamento de falha resolvem antes de qualquer rede.

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| **Adaptador simulado ao lado do real** | Segunda implementação da mesma regra, sem como conferir contra a real. ~200 linhas e 15 testes provando que o simulador concorda consigo mesmo. Descartada pelo responsável ao especificar |
| **`Pacote` no domínio, com consolidação e peso cubado** | Existia só para alimentar o simulador. O MelhorEnvio empacota sozinho — ficaria sem consumidor |
| **`CepHelper.RegiaoPor` + `RegiaoDeEntrega`** | Idem: só serviam à tabela de preços do simulador |
| **`HttpMessageHandler` mockado para os caminhos de falha** | Apontar a configuração para endereço inalcançável testa o `HttpClient` de verdade, e é menos código |
| **Medidas anuláveis, como `Descricao`** | Todo cálculo carregaria tratamento de nulo, e o administrador conseguiria salvar produto que a loja não sabe despachar — erro só apareceria na hora de cotar |
| **Valor único de medida para os cem produtos** | O frete variaria só pela quantidade, nunca pelo que os itens são. CA-09 seria inescrevível |
| **`POST` para cotar** | Não muda estado; `GET` reaproveita a rota do carrinho e funciona sem script |
| **Modelo de página novo, separando carrinho de cotação** | O subtotal mora no `<aside>` que o `carrinho.js` da `017` troca inteiro ao mudar quantidade. Tirar o `aside` do parcial quebraria a atualização do subtotal |
| **Cotação também na página do produto** | Dobraria superfície de tela e de E2E. Fora de escopo declarado |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| **A credencial não chega a tempo** | Média | Alto | A Fase A é executável desde já e entrega valor sozinha. Se a credencial nunca vier, a Fase B vira spec própria e nada do que foi feito se perde |
| **O formato da resposta difere do documentado** | ~~Média~~ **Baixa** | Médio | **Reduzido:** a documentação foi obtida antes de implementar, e o mapeamento é testado contra o exemplo documentado, sem credencial. Resta o risco de a documentação estar desatualizada em relação ao serviço — o que a T047 confere |
| **Sandbox devolve transportadora ou preço irreal** | Média | Baixo | As asserções são de relação, não de valor. Preço irreal não quebra teste de "distante custa mais" |
| **A migration falha sobre base povoada** | Baixa | Alto | Duas etapas: coluna com padrão temporário, depois `UPDATE`. Testada pela integração que exige medidas > 0 em todo produto |
| **`CarrinhoDTO` mudar quebra os testes da `017`** | Baixa | Médio | `Cotacao` é anulável e o parâmetro do mapper tem padrão `null` — nenhum teste existente muda |
| **Teste de rede intermitente no dia da banca** | Média | Baixo | Fora do filtro padrão: `dotnet test` continua verde sem rede |

## 9. Desvios constitucionais justificados

*Nenhum.*

A feature não cria entidade nova (II se aplica ao que `Produto` ganha, e é
respeitado), não escreve no banco fora da migration (VI respeitado), não
acrescenta ação que mude estado (VII sem exceção) e não cria caminho de erro
novo — pelo contrário, evita criar um, ao tratar falha esperada como dado em vez
de exceção (VIII reforçado).

---

## Sobre a cadeia da loja

Esta entrega desloca a cadeia pela sétima vez, ao dividir o que era uma spec só.
Depois dela:

| # | Entrega | O que traz |
|---|---|---|
| `020` | Dimensões do produto e cotação de frete | esta |
| `021` | Redesenho do carrinho | `/Carrinho` ganha o desenho do protótipo — itens em cartões, resumo lateral, esvaziar carrinho |
| `022` | Fechamento de pedido | indicador de passos; Conta, Endereço e Pagamento; `Pedido`, `ItemPedido` e `Pagamento`; confirmação; a vitrine passa a "mais vendidos" |
| `023` | Meus pedidos | histórico na área de conta, ligando o atalho que a `018` deixou reservado |
| `024` | Avaliação, promoções, favorito e sugestões | as quatro features que a `019` extraiu do backlog solto |
| `025` | Estoque | substitui o `ProdutoStatus.ForaDeEstoque` marcado à mão |

**O que fica explicitamente para a `022`:** compor `Pedido.Valor` com itens mais
frete, e trocar a ordenação da vitrine para "mais vendidos" com o título junto —
as duas dependem de venda registrada, que é o que a `022` cria.

**⚠️ Ordem de execução sugerida, diferente da numeração.** A `021` reconstrói o
resumo lateral do carrinho, que é exatamente onde a caixa de CEP desta entrega
mora. Executar a `021` **antes** da Fase B da `020` evita construir a caixa duas
vezes. Como a Fase B já está bloqueada pela credencial e a `021` não depende de
nada, isso não custa cronograma. A Fase A da `020` (medidas do produto) é
independente das duas e pode vir primeiro. O `README` das specs já registra
precedente de ordem de execução diferente da numeração.
