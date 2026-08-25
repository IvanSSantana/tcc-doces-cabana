# Plano Técnico — Meus pedidos

**Spec:** [`spec.md`](./spec.md) · **Status:** Rascunho
**Criado em:** 2026-08-25

---

## 1. Resumo da abordagem

Entrega de leitura. Nada é criado, alterado ou removido — nenhuma escrita, nenhum
`IUnitOfWork`, nenhuma migration. Duas telas sobre dados que o fechamento de
pedido já grava.

**A decisão que carrega o desenho é a assinatura do repositório.** A entrega de
conta e endereços resolveu o mesmo problema com uma escolha que vale copiar:

```csharp
// IEnderecoRepository — sem BuscarPorId(enderecoId) de propósito
Task<Endereco?> Buscar(Guid enderecoId, Guid usuarioId);
```

Não existe método que busque endereço só pelo identificador dele. Assim a RN-01
("pedido alheio é inalcançável") **não pode ser violada por esquecimento** — não
há caminho errado disponível. O mesmo se aplica aqui: `IPedidoRepository` já
ganha, na entrega de fechamento, um `Buscar(pedidoId, usuarioId)`; esta entrega
usa só ele, e nenhum outro é acrescentado.

## 2. Verificação constitucional

| Princípio | Situação |
|---|---|
| **I — Direção de dependência** | ✅ Nenhuma referência nova |
| **II — Domínio se defende** | ✅ Nenhuma entidade muda |
| **III — Duas barreiras** | ✅ Nenhum dado é recebido do usuário além do identificador na rota |
| **IV — Português** | ✅ `PedidoService.ListarDoUsuario`, `BuscarDetalhe`, `ResumoDePedidoDTO` |
| **V — Teste antes** | ✅ Ciclo vermelho-verde |
| **VI — Persistência escondida** | ✅ Só leitura; sem migration, sem `IUnitOfWork` |
| **VII — Seguro na borda** | ✅ `[Authorize]` na classe do controlador, não em cada ação — ação nova não nasce desprotegida por esquecimento |
| **VIII — Dono do erro** | ✅ Pedido inexistente **ou alheio** lança `KeyNotFoundException` da aplicação, que o `FilterException` transforma em "não encontrado" |

## 3. Direção visual

Segue o desenho das telas de conta que a entrega anterior estabeleceu: mesmo
menu lateral, mesmo cartão de conteúdo. A lista reaproveita a linguagem de
cartão que o carrinho passou a usar — um cartão por pedido, com número em
destaque à esquerda e valor à direita.

A situação aparece como etiqueta colorida, e o vocabulário é o do cliente:
"Aguardando pagamento", "Confirmado", "Enviado", "Entregue", "Cancelado" — não
os nomes técnicos do enumerado.

**Pedido cancelado tem tratamento visual distinto** — esmaecido, não vermelho de
erro: cancelamento é um desfecho, não uma falha da tela.

## 4. Impacto por camada

### `DocesCabana.Domain`

Nenhuma mudança.

### `DocesCabana.Application`

| Arquivo | Mudança |
|---|---|
| `Contracts/Services/IPedidoService.cs` | `ListarDoUsuario` e `BuscarDetalhe` |
| `Services/PedidoService.cs` | Implementação |
| `DTOs/ResumoDePedidoDTO.cs` | **novo** — a linha da lista |
| `DTOs/DetalheDePedidoDTO.cs` | **novo** |
| `Mappings/PedidoMapper.cs` | As duas traduções |
| `Contracts/Repositories/IPedidoRepository.cs` | **Nada** — `ListarPorUsuario` e `Buscar(pedidoId, usuarioId)` já vêm do fechamento |

```csharp
Task<IReadOnlyList<ResumoDePedidoDTO>> ListarDoUsuario(Guid usuarioId);

/// <summary>Lança <see cref="KeyNotFoundException"/> para pedido inexistente
/// ou de outra pessoa — os dois casos são indistinguíveis de fora (RN-01).</summary>
Task<DetalheDePedidoDTO> BuscarDetalhe(Guid pedidoId, Guid usuarioId);
```

Os dois casos respondendo igual não é descuido: distinguir "não existe" de "não
é seu" contaria a quem sonda que aquele pedido existe.

### `DocesCabana.Infrastructure`

| Arquivo | Mudança |
|---|---|
| `Repositories/PedidoRepository.cs` | Ajustar as consultas para trazerem o que a lista e o detalhe precisam, sem consulta por item |

O detalhe precisa de itens **com produto** e do endereço de entrega. Uma consulta
com `Include`, não uma por linha — mesma preocupação que o carrinho resolveu ao
trazer `ItemCarrinho` com o produto junto.

### `DocesCabana.MVC`

| Arquivo | Mudança |
|---|---|
| `Controllers/PedidoController.cs` | `Meus` (lista) e `Detalhe` |
| `Views/Pedido/Meus.cshtml` | **nova** |
| `Views/Pedido/Detalhe.cshtml` | **nova** |
| `Views/Conta/_MenuDaConta.cshtml` | O atalho reservado deixa de estar desabilitado |
| `wwwroot/css/pages/pedidos.css` | **nova** |

**`PedidoController` já existe** desde o fechamento, com `Fechar` e
`Confirmacao`. As duas ações novas entram nele — é o mesmo conceito de negócio, e
o Princípio IV pede nome único por conceito. O `[Authorize]` sobe para a classe,
se ainda não estiver lá.

## 5. Sobre a situação do pedido

Ela só avança quando o pagamento for efetuado, e nada no sistema efetua pagamento
até a integração com processadora existir. Consequência para esta tela:

- Pedido criado pela aplicação aparece sempre como **aguardando pagamento**
- Os pedidos **semeados** trazem situações variadas, por representarem compras
  passadas — é o que dá o que mostrar aqui enquanto o gateway não chega

A tradução de `PedidoStatus` para o texto visível mora na view, não na entidade:
é vocabulário de tela, e muda sem que a regra de negócio mude.

## 6. Estratégia de teste

| Camada | Casos |
|---|---|
| `Units/Services/PedidoServiceTests.cs` | Lista devolve só os pedidos do usuário, do mais recente ao mais antigo; **detalhe de pedido alheio lança `KeyNotFoundException`**, igual a inexistente (RN-01, CA-07/CA-08); detalhe traz o preço gravado, não o do produto hoje (RN-02, CA-06) |
| `Units/Controllers/PedidoControllerTests.cs` | `Meus` devolve a view com os resumos; `Detalhe` devolve a view; lista vazia devolve view mesmo assim (CA-04) |
| `Integration/Repositories/` | O detalhe traz itens **e** endereço numa consulta, não uma por item |
| `E2E/Fluxos/MeusPedidosTests.cs` | Atalho da conta habilitado (CA-01); lista mostra os pedidos semeados com situações diferentes (CA-02/CA-03); abrir um leva ao detalhe (CA-05); visitante é levado a entrar (CA-09); quem nunca comprou vê a tela vazia com caminho para o catálogo (CA-04) |

**O cliente sem pedido nenhum já existe na massa de demonstração:** a entrega de
favoritos reservou o oitavo cliente para os testes de lista vazia, justamente
por não ser usado por nenhum outro teste. Mesmo uso aqui — desde que a semeadura
de pedidos do fechamento **não o inclua**, o que é tarefa desta entrega conferir.

## 7. Alternativas descartadas

| Alternativa | Por que não |
|---|---|
| **Tudo expandido numa tela só** | A tela cresceria sem limite com o histórico, e não haveria endereço próprio para o número que a pessoa cita |
| **Só a lista, sem detalhe** | "O que eu comprei?" é justamente a pergunta que leva alguém a abrir um histórico |
| **`BuscarPorId(pedidoId)` no repositório, com checagem de dono no serviço** | A checagem passaria a depender de alguém lembrar. Sem o método, o caminho errado não existe |
| **Responder "acesso negado" para pedido alheio** | Contaria a quem sonda que aquele pedido existe |
| **Controlador próprio para o histórico** | Mesmo conceito de negócio do `PedidoController` que já existe; dois nomes para pedido violam o Princípio IV |
| **Paginação** | Um cliente de doceria não acumula dezenas de pedidos. Acrescentar depois é pequeno; acrescentar agora é complexidade sem consumidor |
| **Traduzir a situação na entidade** | Vocabulário de tela muda sem a regra mudar; o domínio não deve carregar texto de interface |

## 8. Riscos

| Risco | Probabilidade | Impacto | Mitigação |
|---|---|---|---|
| **Consulta por item ao montar o detalhe** | Média | Baixo | `Include` numa consulta só, com teste de integração cobrando isso |
| **A semeadura de pedidos usar o cliente reservado à lista vazia** | Média | Baixo | Tarefa própria conferindo, e o teste de lista vazia falha alto se acontecer |
| **A tela nascer sem nada para mostrar** | Baixa | Médio | Depende da semeadura de pedidos do fechamento; se ela não existir, esta entrega não tem o que demonstrar |
| **Situação sempre igual empobrecer a tela** | Alta | Baixo | Aceito e registrado: os semeados variam, os criados não. Some quando o gateway existir |

## 9. Desvios constitucionais justificados

*Nenhum.*

Entrega de leitura: sem entidade nova, sem escrita, sem migration, sem caminho de
erro novo. A única regra de segurança é implementada por assinatura de método, o
que é mais forte que a checagem que ela substitui.
