# Plano Técnico — Fechamento de pedido

**Spec:** [`spec.md`](./spec.md) · **Status:** Rascunho
**Criado em:** 2026-08-25

---

## 1. Resumo da abordagem

**Os passos vivem na tela que a `021` construiu.** `/Carrinho` passa a aceitar
qual passo exibir; a coluna esquerda troca de parcial, o resumo à direita
permanece. Sem script, trocar de passo é uma navegação `GET` comum; com script,
o mesmo endereço devolve só o pedaço — o "um endereço, duas representações" que
o projeto usa desde a `014`.

**Exibir é do carrinho; gravar é do pedido.** `CarrinhoController` mostra os
passos. Criar o pedido e exibir o comprovante são de um `PedidoController`
próprio. A separação não é estética: são responsabilidades com públicos e
verbos diferentes, e o Princípio IV pede nome único por conceito de negócio.

**O comprovante é uma página de verdade, alcançada por redirecionamento.**
Fechar é `POST` que redireciona para `/Pedido/Confirmacao/{id}`. Isso é
POST-Redirect-Get do Princípio VII e, de quebra, **resolve o CA-14 sozinho**:
recarregar o comprovante é um `GET`, e nenhum segundo pedido nasce.

**Uma conferência, duas naturezas.** Produtos e entrega são conferidos pela
mesma regra: o que a tela exibiu volta no formulário como **alegação**, o
servidor recalcula, e divergência interrompe. O valor postado nunca é gravado —
adulterá-lo só provoca reexibição.

## 2. Verificação constitucional

| Princípio | Situação |
|---|---|
| **I — Direção de dependência** | ✅ `IPedidoService` e `IPedidoRepository` declarados na `Application`, implementados na `Infrastructure`. Nenhuma referência nova |
| **II — Domínio se defende** | ✅ `Pedido` ganha propriedades com `private set` e métodos de intenção. O número visível é **método**, não propriedade — propriedade computada o EF tentaria mapear para coluna, mesmo motivo de `Produto.DisponivelParaCompra()` |
| **III — Duas barreiras** | ✅ Escolhas do formulário validadas por `FluentValidation`; invariantes do pedido no construtor e nos métodos |
| **IV — Português** | ✅ `PedidoService.Fechar`, `FechamentoDePedidoDTO`, `ConfirmacaoDePedidoDTO`, `NumeroVisivel()` |
| **V — Teste antes** | ✅ Ciclo vermelho-verde. A ordenação por venda ganha teste de integração, por ser subconsulta traduzida a SQL |
| **VI — Persistência escondida** | ✅ Migration versionada; **um único `SalvarAlteracoes`** grava pedido, itens e pagamento — é ele que entrega a RN-07, sem transação explícita, como o princípio determina |
| **VII — Seguro na borda** | ✅ `Fechar` é `[HttpPost]` com `[ValidateAntiForgeryToken]` e `[Authorize]`, aguardado, redirecionando no sucesso |
| **VIII — Dono do erro** | ✅ Divergência e indisponibilidade são **erro esperado do usuário**: viram `ModelState`, não exceção. Serviço de entrega fora do ar já devolve mensagem desde a `020` |

## 3. Impacto por camada

### `DocesCabana.Domain`

| Arquivo | Mudança |
|---|---|
| `Entities/Pedido.cs` | Cinco propriedades novas; coleção de itens; `NumeroVisivel()`; construtor recebendo os dados da entrega |
| `Entities/ItemPedido.cs` | Inalterado — já tem `PrecoUnitario`, que é o congelamento da RN-01 |
| `Entities/Pagamento.cs` | Inalterado |

**A coleção de itens entra aqui, e isto é uma decisão adiada desde a modelagem.**
O comentário em `Pedido.cs` diz, com todas as letras: *"Sem coleção de itens
nesta entrega — quem gerencia o agregado é decisão da spec de carrinho"*. É esta
entrega. `Pedido` vira a raiz do agregado, com coleção somente-leitura e
`AcrescentarItem`. Consequência prática: gravar o pedido grava os itens junto,
e a RN-07 deixa de depender de alguém lembrar de gravar as duas coisas.

### `DocesCabana.Application`

| Arquivo | Mudança |
|---|---|
| `Contracts/Services/IPedidoService.cs` | **novo** |
| `Services/PedidoService.cs` | **novo** — o fechamento inteiro |
| `Contracts/Repositories/IPedidoRepository.cs` | **novo** |
| `DTOs/FechamentoDePedidoDTO.cs` | **novo** — o que o formulário posta |
| `DTOs/ConfirmacaoDePedidoDTO.cs` | **novo** — o comprovante |
| `DTOs/PassoDoFechamentoDTO.cs` | **novo** — o que a tela precisa por passo |
| `Validators/FechamentoDePedidoDTOValidator.cs` | **novo** |
| `Enums/PassoDoFechamento.cs` | **novo** |
| `Mappings/PedidoMapper.cs` | **novo** |
| `Contracts/Repositories/IProdutoRepository.cs` | Nada — a ordenação por venda é interna ao repositório |

### `DocesCabana.Infrastructure`

| Arquivo | Mudança |
|---|---|
| `Repositories/PedidoRepository.cs` | **novo** |
| `Repositories/ProdutoRepository.cs` | `AplicarOrdenacao` ganha o ramo `MaisVendidos` de verdade |
| `DatabaseContext/Configurations/PedidoConfiguration.cs` | Colunas novas, coleção de itens, precisão decimal |
| `Migrations/…_AddPedidoDadosDeEntrega.cs` | **nova** — tabela vazia, sem risco |
| `DependencyInjections/ApplicationDependencyInjection.cs` | Registro do serviço e do repositório |

### `DocesCabana.MVC`

| Arquivo | Mudança |
|---|---|
| `Controllers/CarrinhoController.cs` | `Index` aceita `passo` |
| `Controllers/PedidoController.cs` | **novo** — `Fechar` (POST) e `Confirmacao` (GET) |
| `Views/Carrinho/_PassosDoFechamento.cshtml` | **nova** — o indicador |
| `Views/Carrinho/_PassoConta.cshtml` | **nova** |
| `Views/Carrinho/_PassoEndereco.cshtml` | **nova** — reaproveita o formulário da `018` |
| `Views/Carrinho/_PassoPagamento.cshtml` | **nova** |
| `Views/Pedido/Confirmacao.cshtml` | **nova** |
| `Views/Home/Index.cshtml` | Título vira "Mais vendidos" |
| `Controllers/CatalogoController.cs` | **Remover** `SanearOrdenacao` |
| `Controllers/HomeController.cs` | A vitrine passa a pedir ordenação por venda |
| `wwwroot/js/components/carrinho.js` | Troca de passo sem recarga |
| `Helpers/DbInitializer.cs` | Pedidos de demonstração |

## 4. Contratos

```csharp
// Domain/Entities/Pedido.cs — o que entra
public decimal ValorDoFrete       { get; private set; }
public string  Transportadora     { get; private set; } = default!;
public string  Servico            { get; private set; } = default!;
public int     PrazoMinimoEmDias  { get; private set; }
public int     PrazoMaximoEmDias  { get; private set; }

public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

// Método, não propriedade: propriedade computada o EF tentaria mapear para
// coluna — mesma razão de Produto.DisponivelParaCompra().
public string NumeroVisivel() => PedidoId.ToString("N")[..8].ToUpperInvariant();
```

```csharp
// Application/Contracts/Services/IPedidoService.cs
public interface IPedidoService
{
    Task<PassoDoFechamentoDTO> MontarPasso(PassoDoFechamento passo, Guid? usuarioId, Guid? enderecoId);

    /// <summary>
    /// Fecha o pedido, ou explica por que não. Divergência de valor, item
    /// indisponível e entrega incalculável são erro esperado (RN-02/RN-06) e
    /// voltam no resultado — não como exceção.
    /// </summary>
    Task<ResultadoDoFechamentoDTO> Fechar(Guid usuarioId, FechamentoDePedidoDTO dados);
}

// Application/DTOs/FechamentoDePedidoDTO.cs — o que o formulário posta
public class FechamentoDePedidoDTO
{
    public Guid EnderecoId { get; set; }
    public int ServicoDeEntregaId { get; set; }
    public MetodoPagamento MetodoPagamento { get; set; }

    // Alegações a conferir, nunca gravadas (RN-02). Adulterá-las não dá
    // vantagem: divergir provoca reexibição, coincidir não muda nada.
    public decimal ValorDosProdutosExibido { get; set; }
    public decimal ValorDoFreteExibido { get; set; }
}
```

### A conferência, passo a passo

```
Fechar(usuarioId, dados)
 1. carrega o carrinho do usuário
 2. carrinho vazio                        → recusa
 3. algum item indisponível               → recusa, nomeando o item      (RF-16)
 4. soma os produtos pelo preço de agora
    ≠ ValorDosProdutosExibido             → recusa, devolvendo o atual   (RF-15)
 5. re-cota o frete para o endereço
    não veio cotação                      → recusa                       (RF-17)
    opção escolhida sumiu                 → recusa
    preço ≠ ValorDoFreteExibido           → recusa, devolvendo o atual   (RF-15)
 6. monta Pedido com itens, entrega e valor total
 7. monta Pagamento (Pendente)
 8. esvazia o carrinho
 9. UM SalvarAlteracoes                                                  (RF-20)
```

O passo 9 é o que entrega a RN-07. O Princípio VI é explícito: *"`SalvarAlteracoes`
já é atômico por si — um lote com uma alteração inválida não persiste nenhuma das
outras"*. Nenhuma transação explícita, que a `002` removeu do `IUnitOfWork` por
duplicar essa garantia.

## 5. A ordenação por venda

```csharp
OrdenacaoCatalogo.MaisVendidos => consulta
    .OrderByDescending(p => _context.ItensPedido
        .Where(i => i.ProdutoId == p.ProdutoId && i.Pedido!.Status != PedidoStatus.Cancelado)
        .Sum(i => (int?)i.Quantidade) ?? 0)
    .ThenBy(p => p.Nome),
```

Mesma forma da subconsulta que `MelhorAvaliados` já usa desde a `014` — o
`(int?)` seguido de `?? 0` é o que permite produto sem venda nenhuma vir por
último em vez de sumir da consulta, exatamente como o `?? -1` faz lá.

`CatalogoController.SanearOrdenacao` **é removido** — existia só para recusar
esta ordenação enquanto ela não tinha sentido.

**A semeadura de pedidos não é enfeite.** Sem ela, os cem produtos empatam em
zero, o desempate por nome assume, e a home mostra ordem alfabética sob o título
"mais vendidos" — a RN-04, e o defeito que a `019` recusou cometer. O seed cria
pedidos fechados entre os oito clientes fictícios que já existem, com produtos e
quantidades variados, mais **um pedido cancelado**, para o CA-22 ter o que
provar.

## 6. Modelo de dados

Cinco colunas em `Pedido`, tabela com **zero linhas** — migration sem risco de
dado existente. `ValorDoFrete` como `decimal(10,2)`; os prazos como inteiros; os
dois textos com tamanho limitado.

A coleção de itens é configurada em `PedidoConfiguration` com campo de apoio,
para a entidade não expor `List<>` mutável.

**`Pedido.Valor` continua sendo o total** — produtos mais frete. `ValorDoFrete`
existe separado em vez de derivado por subtração porque, quando cupom ou
promoção entrarem no valor, a subtração deixaria de bater e o erro apareceria
silenciosamente numa tela, não num teste.

## 7. Estratégia de teste

| Camada | Casos |
|---|---|
| `Units/Entities/PedidoTests.cs` | Invariantes das colunas novas; `AcrescentarItem`; **`NumeroVisivel()` tem 8 caracteres, é maiúsculo e é estável para o mesmo pedido** |
| `Units/Services/PedidoServiceTests.cs` | O coração: cada uma das cinco recusas do §4 devolve o motivo certo **sem lançar**; caminho feliz grava pedido, itens e pagamento e chama `SalvarAlteracoes` **uma vez só** (RF-20); preço do item é o de agora, não o exibido (RF-19) |
| `Units/Controllers/PedidoControllerTests.cs` | `Fechar` redireciona para a confirmação no sucesso; recusa devolve a view com `ModelState` inválido; `Confirmacao` de pedido alheio não é acessível |
| `Units/Controllers/CarrinhoControllerTests.cs` | `passo` inválido cai no primeiro; passo de conta não é oferecido a quem já entrou (RF-03) |
| `Units/Controllers/CatalogoControllerTests.cs` | **Reescrever** o teste que hoje prova o saneamento: `MaisVendidos` passa a ser executada, não recusada (RF-26) |
| `Integration/Repositories/` | Ordenação por venda sobre dados reais: mais vendido primeiro; sem venda por último; **pedido cancelado não conta** (CA-22). É subconsulta traduzida a SQL — teste de unidade não a exercita |
| `E2E/Fluxos/FechamentoTests.cs` | Jornada completa; visitante encontra o passo de entrar; entrar devolve ao carrinho (CA-04); sem endereço cadastra ali (CA-06); trocar endereço troca opções (CA-08); recarregar a confirmação não duplica (CA-14); sem JavaScript funciona (CA-23) |

**Testes que esta entrega quebra de propósito:** o de `CatalogoController` que
afirma o saneamento de `MaisVendidos`, e o da `019` que afirma que o título da
vitrine **não** diz "mais vendidos". Os dois são correção esperada, não
regressão — a `019` os escreveu sabendo que cairiam aqui.

## 8. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| **Guardar a cotação em sessão e usá-la no fechamento** | Recomendada e recusada pelo responsável: re-cotar não tem cotação envelhecendo, nem armazenamento, nem pergunta sobre validade |
| **Confiar no valor postado** | Fechar pedido com frete zero pelo inspetor do navegador |
| **Fechar com o valor recotado, sem conferir** | A pessoa pagaria diferente do que revisou, em silêncio |
| **Quatro telas encadeadas** | O estado do pedido teria de atravessar as etapas, em sessão ou na URL |
| **Coletar dados de cartão** | Sem processadora, é risco de segurança sem contrapartida |
| **Coluna sequencial para o número** | O banco não auto-incrementa fora da chave primária; exigiria `max+1` na aplicação, com disputa |
| **Derivar o frete de `Valor` menos os itens** | Quebra silenciosamente quando cupom ou promoção entrarem |
| **Repositórios separados para item e pagamento** | `Pedido` como raiz do agregado grava os itens junto, e a RN-07 deixa de depender de disciplina |
| **Não semear pedidos** | "Mais vendidos" sobre cem produtos empatados em zero é ordem alfabética com título falso |
| **Contar só pedido pago como venda** | Nada é pago nesta fase; daria zero para sempre |

## 9. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| **A re-cotação falha no momento de confirmar** | Média | Médio | Recusa explícita com mensagem (RF-17). É o custo aceito ao escolher re-cotar; a tela de fechamento também cota, então o caso real é "estava no ar ao exibir, caiu ao confirmar" |
| **Divergência de centavos por arredondamento** | Média | Alto | A comparação é sobre `decimal`, nunca `double`, e sobre o valor formatado exibido. Um erro aqui **recusa fechamento legítimo** — teste próprio com valores de centavo |
| **A subconsulta de venda pesa sobre catálogo grande** | Baixa | Baixo | Mesma forma que `MelhorAvaliados` já executa a cada filtro, com `LIMIT` |
| **O seed de pedidos deixa a base incoerente** | Média | Médio | Pedido, item e pagamento criados juntos, pelos mesmos construtores da aplicação — não por SQL solto |
| **A troca de passo sem recarga quebra o resumo** | Média | Médio | O resumo segue dentro do container único que a `021` preservou |
| **Fechar duas vezes por duplo clique** | Média | Alto | POST-Redirect-Get, e o carrinho é esvaziado no mesmo commit: a segunda tentativa encontra carrinho vazio e recusa |

## 10. Desvios constitucionais justificados

*Nenhum.*

`Pedido` ganha comportamento seguindo o Princípio II; a gravação é um
`SalvarAlteracoes` só, como o VI determina; a ação de escrita nasce com
antiforgery, autorização e redirecionamento, como o VII exige; e as recusas são
erro esperado do usuário, que o VIII manda tratar por `ModelState` em vez de
exceção.
