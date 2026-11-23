# Plano — Retirada na loja

**ID:** 036-retirada-na-loja · **Criado em:** 2026-11-20
**Status:** Executado (verificação manual pendente) · **Depende de:** `022`, `035` executadas

---

## 1. Objetivo

Hoje todo pedido sai por transportadora: o passo "Endereço" do fechamento
exige um endereço do cliente e uma cotação de frete. Esta feature dá ao
cliente uma segunda forma de receber, escolhida no próprio passo de endereço:
**retirar na loja, no mesmo dia, numa faixa de horário**, sem custo de frete.
O administrador ganha uma tela para definir o horário de cada dia da semana,
a duração das faixas, o tempo de preparo e os dias sem retirada (feriados).

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | **A escolha entre "Receber em casa" e "Retirar na loja" fica no topo do passo `Endereco`.** As duas opções são links GET (`modalidade=Entrega` ou `modalidade=RetiradaNaLoja`), com `data-navegacao-fechamento`, como a escolha de endereço e de frete já é feita. | Pedido do responsável ("deve aparecer na parte de endereço"). Links GET mantêm o padrão do passo, que funciona sem JavaScript. | Passo novo "Entrega" no indicador de passos: muda `PassoDoFechamento` e os testes do indicador, sem ganho para o cliente. |
| D2 | **A retirada é sempre no dia do pedido.** O cliente escolhe só a faixa de horário. | Decisão do debate ("é um pedido para o dia"). | Escolher dia e faixa: recusado no debate. |
| D3 | **Horário padrão semeado: segunda a sábado, 09:00 às 18:00; domingo fechado. Faixa de 60 min, preparo de 60 min.** O administrador altera tudo isso na tela. | Horário dado no debate. A duração e o preparo ficaram a cargo do admin; 60/60 é só o valor inicial, para a loja não nascer sem faixa. | Duração e preparo fixos em `appsettings`: recusado no debate ("admin define os dois"). |
| D4 | **Geração das faixas:** começam na abertura, de `DuracaoDaFaixa` em `DuracaoDaFaixa` minutos. A última termina no fechamento, mesmo que fique mais curta (09:00–18:00 com 90 min → … 16:30–18:00). Uma faixa só aparece se `Inicio >= agora + Preparo`. | Regra determinística e testável sem banco. Truncar a última faixa evita oferecer horário depois do fechamento. | Faixa que começa antes de `agora + preparo` mas termina depois: o pedido poderia não estar pronto no início da faixa. |
| D5 | **Sem faixa hoje (domingo, dia bloqueado, depois do último início possível), a opção "Retirar na loja" aparece desabilitada**, com o aviso "Retirada indisponível agora. Próxima: {dia}, {dd/MM}, a partir das {HH:mm}." Só "Receber em casa" fica clicável. | Decisão do debate ("opção indisponível"). O aviso diz ao cliente quando voltar. | Agendar para o próximo dia: contradiz D2. |
| D6 | **A próxima abertura procura até 14 dias à frente** o primeiro dia aberto e não bloqueado. Sem nenhum, o aviso é só "Retirada indisponível no momento." | Evita laço infinito se o admin fechar todos os dias. 14 dias cobre feriado prolongado. | Procurar sem limite: laço infinito com a semana toda fechada. |
| D7 | **Sem limite de pedidos por faixa.** | Decisão do debate. | Capacidade por faixa: exigiria contar pedidos agendados com concorrência. |
| D8 | **Loja única. O endereço é conteúdo estático: "Av. Pedro Ometto, 92 - Centro, Barra Bonita - SP, 17340-001", com o link do Google Maps que o rodapé já usa.** Fica num parcial `Views/Shared/_EnderecoDaLoja.cshtml`. | Decisão do debate (loja única, endereço informado). O admin não edita o endereço (não foi escolhido no debate). O parcial é usado por quatro telas, por isso mora em `Shared/` (Princípio IV). | Endereço no banco ou em `appsettings`: configuração sem quem a altere. |
| D9 | **O que o admin edita:** horário de abertura e fechamento de cada dia da semana (ou dia fechado), duração da faixa, preparo, e uma lista de datas sem retirada, com motivo opcional. | Decisão do debate (horário por dia + dias bloqueados; duração e preparo por D3). | Interruptor geral e endereço editável: não escolhidos no debate. |
| D10 | **A configuração é um agregado único, `AgendaDeRetirada`**, com duas coleções próprias: `HorarioDeRetirada` (7 linhas, uma por `DayOfWeek`) e `DiaSemRetirada`. As coleções são mapeadas com `OwnsMany`. A geração das faixas (D4) e a próxima abertura (D6) são métodos da entidade. | A regra de horário é invariante de negócio e cabe inteira no domínio (Princípio II): testável sem mock. `OwnsMany` impede que as linhas filhas sejam alteradas fora da raiz. | Três entidades soltas com repositórios próprios: a regra "abertura antes do fechamento" e a geração de faixas iriam para o serviço. |
| D11 | **Limites de configuração:** duração de 15 a 240 min; preparo de 0 a 480 min; abertura antes do fechamento; dia bloqueado não pode ser no passado nem repetido. | Valores fora disso geram agenda absurda (faixa de 1 min, preparo de dias). | Sem limite: a tela aceitaria uma agenda que não gera faixa nenhuma sem dizer por quê. |
| D12 | **Retirada é grátis.** O pedido de retirada grava `ValorDoFrete = 0`. | Decisão do debate. | — |
| D13 | **Cupom de frete grátis é recusado na retirada:** "Cupom de frete grátis não vale para retirada na loja." O `CupomService.Avaliar` recebe a modalidade. No passo `Carrinho`, a modalidade ainda não foi escolhida e o cupom passa; ao escolher retirada, o resumo mostra o motivo (comportamento D10 da `035`). | Decisão do debate ("cupom de frete não se aplica"). Recusar com motivo segue o padrão de mensagens da `035` (D11 de lá). | Aceitar sem efeito: o cliente acharia que o cupom deu algum desconto. |
| D14 | **Pagamento não muda.** Mesmas formas, mesmo formulário. | Decisão do debate. | "Pagar na retirada": não escolhido. |
| D15 | **`Pedido` ganha `Modalidade` (`ModalidadeDeEntrega`) e `DataDaRetirada`, `InicioDaRetirada`, `FimDaRetirada`.** `EnderecoEntregaId`, `Transportadora`, `Servico` e os dois prazos passam a ser anuláveis. O construtor atual continua sendo o da entrega, com as mesmas validações. A retirada nasce por `Pedido.ParaRetirada(usuarioId, valor, faixa)`. | Um pedido de retirada não tem endereço nem transportadora; gravar valores falsos ("Loja", prazo 0) quebraria as validações e mentiria no histórico. A fábrica nomeada deixa claro qual invariante vale em cada caso. | Tabela separada `RetiradaDoPedido`: mais um `Include` em todas as consultas de pedido por três colunas. |
| D16 | **Linhas antigas de `Pedido` recebem `Modalidade = Entrega`** pelo valor padrão da coluna na migration. | Todo pedido existente foi entrega. | Migration de dados à parte: o valor padrão já resolve. |
| D17 | **`PedidoService.Fechar` reconfere a faixa** contra as faixas disponíveis no momento. Se ela saiu da lista, recusa: "A faixa de retirada escolhida não está mais disponível. Escolha outra." | Mesmo padrão de reconferência do frete e do cupom (`022`, `035`). O cliente pode ter parado no pagamento até a faixa passar. | Confiar na faixa postada: aceitaria faixa já vencida ou adulterada. |
| D18 | **Hora local por `HorarioDeBrasilia.Agora(TimeProvider)`**, novo método ao lado de `Hoje`. | `TimeProvider` já é injetado (`CupomService`, `PromocaoService`); testes controlam o relógio. | `DateTime.Now`: depende do fuso do servidor e não é testável. |
| D19 | **`MontarPasso` ganha dois parâmetros opcionais, `modalidade` e `inicioDaFaixa`**, no fim da assinatura. | Os chamadores atuais continuam compilando sem mudança. | Objeto de parâmetros novo: refatoraria os três chamadores e os testes da `022` sem mudar comportamento. |
| D20 | **Sem E2E nova.** | A disponibilidade depende da hora real do servidor de teste; um E2E seria intermitente (domingo, noite). A regra fica provada em unidade (entidade, serviço, controller) e o banco em integração (Princípio V: E2E só onde as outras camadas não alcançam). | E2E com relógio falso: exigiria trocar `TimeProvider` no host E2E só para este fluxo. |

**Fora de escopo:** capacidade por faixa; mais de uma loja; endereço editável;
interruptor geral de retirada; "pagar na retirada"; status próprio de pedido
("pronto para retirada") e tela do admin para pedidos; aviso por e-mail;
mostrar a modalidade na lista de "Meus pedidos" (só o detalhe e a confirmação
mostram).

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma referência nova entre projetos. Endereço da loja é view. |
| II | Domínio rico e auto-validante | ✅ OK | `AgendaDeRetirada` valida limites e gera faixas; `Pedido.ParaRetirada` valida a faixa. `private set` e `protected` ctor. |
| III | Validação nas duas barreiras | ✅ OK | `AgendaDeRetiradaDTOValidator`, `DiaSemRetiradaDTOValidator` e `FechamentoDePedidoDTOValidator` (por modalidade) espelham as invariantes da entidade. |
| IV | Nomenclatura em português | ✅ OK | `ModalidadeDeEntrega`, `RetiradaNaLoja`, `FaixaDeRetirada`. `DayOfWeek` é tipo do BCL. |
| V | Testes escritos antes | ✅ OK | Ordem de execução põe o teste antes de cada passo. Sem E2E (D20). |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `IAgendaDeRetiradaRepository`; o serviço chama `SalvarAlteracoes`. Mapeamento em `*Configuration`, mapper manual, migration nova. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `RetiradaController` com `[Authorize(Roles = Papeis.Administrador)]`; todo POST com `[ValidateAntiForgeryToken]` e redirect no sucesso. |
| VIII | Tratamento de erro por camada | ✅ OK | Data já bloqueada vira `ModelState` (consulta `DiaJaBloqueado`, como `CodigoJaExiste` da `035`). Faixa vencida vira recusa do fechamento, não exceção. |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/ModalidadeDeEntrega.cs` | Criar | `Entrega = 0`, `RetiradaNaLoja = 1`. |
| `ValueObjects/FaixaDeRetirada.cs` | Criar | `record FaixaDeRetirada(DateOnly Data, TimeOnly Inicio, TimeOnly Fim)`. Pasta nova. |
| `Entities/AgendaDeRetirada.cs` | Criar | Raiz do agregado (D10): limites (D11), `DefinirHorario`, `AlterarFaixas`, `BloquearDia`, `DesbloquearDia`, `FaixasDisponiveis` (D4), `ProximaAbertura` (D6). |
| `Entities/HorarioDeRetirada.cs` | Criar | Filho: `DiaDaSemana`, `Abertura?`, `Fechamento?`, `Aberto`. |
| `Entities/DiaSemRetirada.cs` | Criar | Filho: `Data`, `Motivo?`. |
| `Entities/Pedido.cs` | Alterar | Campos de D15; `EnderecoEntregaId`, `Transportadora`, `Servico`, prazos anuláveis; fábrica `ParaRetirada`. |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `Helpers/HorarioDeBrasilia.cs` | Alterar | `Agora(TimeProvider)` (D18). |
| `Contracts/Repositories/IAgendaDeRetiradaRepository.cs` | Criar | `Obter()`. |
| `Contracts/Services/IAgendaDeRetiradaService.cs` | Criar | Ver §5. |
| `Contracts/Services/ICupomService.cs` | Alterar | `Avaliar` recebe `modalidade` (D13). |
| `Contracts/Services/IPedidoService.cs` | Alterar | `MontarPasso` com `modalidade` e `inicioDaFaixa` (D19). |
| `DTOs/RetiradaDisponivelDTO.cs` | Criar | Faixas de hoje + próxima abertura. |
| `DTOs/FaixaDeRetiradaDTO.cs` | Criar | `record (TimeOnly Inicio, TimeOnly Fim)`. |
| `DTOs/AgendaDeRetiradaDTO.cs` | Criar | Formulário do admin (duração, preparo, 7 horários). |
| `DTOs/HorarioDeRetiradaDTO.cs` | Criar | Uma linha do formulário. |
| `DTOs/DiaSemRetiradaDTO.cs` | Criar | Data + motivo. |
| `DTOs/FechamentoDePedidoDTO.cs` | Alterar | `Modalidade`, `InicioDaFaixa`. |
| `DTOs/PassoDoFechamentoDTO.cs` | Alterar | `Modalidade`, `Retirada`, `InicioDaFaixaSelecionada`; `ValorTotal` com frete 0 na retirada. |
| `DTOs/ConfirmacaoDePedidoDTO.cs` | Alterar | `Modalidade`, campos da retirada; `Transportadora`/`Servico`/prazos anuláveis. |
| `DTOs/DetalheDePedidoDTO.cs` | Alterar | Idem. |
| `Mappings/AgendaDeRetiradaMapper.cs` | Criar | Entidade ↔ DTOs. |
| `Mappings/PedidoMapper.cs` | Alterar | Mapear campos novos. |
| `Validators/AgendaDeRetiradaDTOValidator.cs` | Criar | Duração, preparo, 7 dias, abertura < fechamento quando aberto. |
| `Validators/HorarioDeRetiradaDTOValidator.cs` | Criar | Regra por linha, usada por `RuleForEach`. |
| `Validators/DiaSemRetiradaDTOValidator.cs` | Criar | Data obrigatória e não passada (`TimeProvider` injetado); motivo até 100. |
| `Validators/FechamentoDePedidoDTOValidator.cs` | Alterar | Endereço e serviço só quando `Entrega`; `InicioDaFaixa` obrigatório quando `RetiradaNaLoja` ("Escolha uma faixa de horário!"). |
| `Services/AgendaDeRetiradaService.cs` | Criar | Implementa o contrato. |
| `Services/CupomService.cs` | Alterar | Recusa de D13. |
| `Services/PedidoService.cs` | Alterar | Ramo de retirada em `MontarPasso` e `Fechar` (D17); injeta `IAgendaDeRetiradaService`. |

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `DatabaseContext/Configurations/AgendaDeRetiradaConfiguration.cs` | Criar | Tabela `AgendaDeRetirada`; `OwnsMany` → `HorarioDeRetirada` (chave agenda+dia) e `DiaSemRetirada` (chave agenda+data). |
| `DatabaseContext/Configurations/PedidoConfiguration.cs` | Alterar | FK de endereço opcional; `Transportadora`/`Servico` opcionais; `Modalidade` com padrão `Entrega` (D16); colunas da retirada. |
| `DatabaseContext/DocesCabanaDbContext.cs` | Alterar | `DbSet<AgendaDeRetirada> AgendasDeRetirada`. |
| `DatabaseContext/Seed/DbInitializer.cs` | Alterar | `SemearAgendaDeRetirada` com D3, só se não existir. |
| `Repositories/AgendaDeRetiradaRepository.cs` | Criar | `Obter()` rastreado, com as duas coleções. |
| `DependencyInjections/ApplicationDependencyInjection.cs` | Alterar | Registrar repositório e serviço. |
| `Migrations/*_AddPickupScheduling.cs` | Criar | Gerada pela ferramenta. |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/CarrinhoController.cs` | Alterar | `Index` recebe `modalidade` e `faixa`; repassa a `MontarPassoDoFechamento`. |
| `Controllers/PedidoController.cs` | Alterar | `Fechar` repassa `dados.Modalidade` e `dados.InicioDaFaixa` ao remontar o passo. |
| `Views/Carrinho/_PassoEndereco.cshtml` | Alterar | Seletor de modalidade (D1, D5); conteúdo atual vira o ramo `Entrega`. |
| `Views/Carrinho/_OpcoesDeRetirada.cshtml` | Criar | Endereço da loja, "Hoje, dd/MM", lista de faixas (reusa `.opcao-frete`), "Continuar para pagamento". |
| `Views/Carrinho/_PassoPagamento.cshtml` | Alterar | Ramo retirada: hidden `Modalidade` e `InicioDaFaixa`, resumo "Retirada na loja · hoje HH:mm–HH:mm · Grátis". |
| `Views/Carrinho/_ItensDoCarrinho.cshtml` | Alterar | Linha de entrega no resumo mostra "Retirada — Grátis" na retirada. |
| `Views/Pedido/Confirmacao.cshtml` | Alterar | Ramo retirada: data, faixa, `_EnderecoDaLoja`. |
| `Views/Pedido/Detalhe.cshtml` | Alterar | Bloco "Entrega" vira "Retirada na loja" no ramo retirada. |
| `Views/Shared/_EnderecoDaLoja.cshtml` | Criar | Endereço de D8 + link do Maps. |
| `Areas/Admin/Controllers/RetiradaController.cs` | Criar | `Index` (GET), `SalvarHorarios`, `BloquearDia`, `DesbloquearDia` (POST). |
| `Areas/Admin/Views/Retirada/Index.cshtml` | Criar | Formulário dos 7 dias + duração/preparo; formulário de bloqueio; lista de datas com botão remover. |
| `Models/AgendaDeRetiradaViewModel.cs` | Criar | `Agenda` + `NovoBloqueio`, com prefixos (como `FormularioDeCupomViewModel`). |
| `Views/Shared/Components/Header/Default.cshtml` | Alterar | Item "Retirada na loja" no menu Admin. |
| `wwwroot/css/pages/*.css` | Alterar | Estilo do seletor de modalidade e da opção desabilitada, com os tokens da `033`. |

### Testes
| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Entities/AgendaDeRetiradaTests.cs` | Criar | D4, D6, D11. |
| `Units/Entities/PedidoTests.cs` | Alterar | `ParaRetirada`; cupom com desconto no frete recusado. |
| `Units/Helpers/HorarioDeBrasiliaTests.cs` | Alterar | `Agora`. |
| `Units/Services/AgendaDeRetiradaServiceTests.cs` | Criar | Consulta, gestão, `SalvarAlteracoes`. |
| `Units/Services/PedidoServiceTests.cs` | Alterar | Ramos de retirada. |
| `Units/Services/CupomServiceTests.cs` | Alterar | D13. |
| `Units/Validators/AgendaDeRetiradaDTOValidatorTests.cs` | Criar | Cada `RuleFor`. |
| `Units/Validators/DiaSemRetiradaDTOValidatorTests.cs` | Criar | Cada `RuleFor`. |
| `Units/Validators/FechamentoDePedidoDTOValidatorTests.cs` | Criar | Regras por modalidade. |
| `Units/Mappings/PedidoMapperTests.cs` | Alterar | Pedido de retirada. |
| `Units/Controllers/Admin/RetiradaControllerTests.cs` | Criar | Autorização, `ModelState`, redirect, data repetida. |
| `Units/Controllers/PedidoControllerTests.cs` | Alterar | Repasse da modalidade na recusa. |
| `Integration/Repositories/AgendaDeRetiradaRepositoryIntegrationTests.cs` | Criar | Persistir e reler coleções. |
| `Integration/Repositories/PedidoRepositoryIntegrationTests.cs` | Alterar | Pedido de retirada sem endereço. |

## 5. Contratos e modelo de dados

```csharp
// Domain
public enum ModalidadeDeEntrega { Entrega = 0, RetiradaNaLoja = 1 }
public record FaixaDeRetirada(DateOnly Data, TimeOnly Inicio, TimeOnly Fim);

public class AgendaDeRetirada
{
    public static readonly Guid IdUnico;
    public const int DuracaoMinima = 15, DuracaoMaxima = 240, PreparoMaximo = 480, DiasDeBuscaDaProximaAbertura = 14;
    public Guid AgendaDeRetiradaId { get; private set; }
    public int DuracaoDaFaixaEmMinutos { get; private set; }
    public int PreparoEmMinutos { get; private set; }
    public IReadOnlyCollection<HorarioDeRetirada> Horarios { get; }
    public IReadOnlyCollection<DiaSemRetirada> DiasBloqueados { get; }

    public AgendaDeRetirada(int duracaoDaFaixaEmMinutos, int preparoEmMinutos); // nasce com os 7 dias fechados
    public void AlterarFaixas(int duracaoDaFaixaEmMinutos, int preparoEmMinutos);
    public void DefinirHorario(DayOfWeek dia, TimeOnly? abertura, TimeOnly? fechamento); // ambos nulos = fechado
    public void BloquearDia(DateOnly data, string? motivo, DateOnly hoje);
    public void DesbloquearDia(DateOnly data);
    public IReadOnlyList<FaixaDeRetirada> FaixasDisponiveis(DateTime agoraLocal);
    public DateTime? ProximaAbertura(DateTime agoraLocal);
}

// Pedido (acréscimos)
public ModalidadeDeEntrega Modalidade { get; private set; }
public DateOnly? DataDaRetirada { get; private set; }
public TimeOnly? InicioDaRetirada { get; private set; }
public TimeOnly? FimDaRetirada { get; private set; }
public static Pedido ParaRetirada(Guid usuarioId, decimal valor, FaixaDeRetirada faixa, Guid id = default);

// Application
public interface IAgendaDeRetiradaRepository { Task<AgendaDeRetirada> Obter(); }

public interface IAgendaDeRetiradaService
{
    Task<RetiradaDisponivelDTO> ConsultarHoje();
    Task<AgendaDeRetiradaDTO> ObterParaGestao();
    Task SalvarHorarios(AgendaDeRetiradaDTO dto);
    Task<bool> DiaJaBloqueado(DateOnly data);
    Task BloquearDia(DiaSemRetiradaDTO dto);
    Task DesbloquearDia(DateOnly data);
}

public class RetiradaDisponivelDTO
{
    public DateOnly Data { get; init; }
    public IReadOnlyList<FaixaDeRetiradaDTO> Faixas { get; init; } = [];
    public DateTime? ProximaAbertura { get; init; }
    public bool Disponivel => Faixas.Count > 0;
}

// ICupomService
Task<ResultadoDoCupomDTO> Avaliar(string codigo, Guid usuarioId, CarrinhoDTO carrinho, decimal? valorDoFrete,
    ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega);

// IPedidoService
Task<PassoDoFechamentoDTO> MontarPasso(
    PassoDoFechamento passo, CarrinhoDTO carrinho, Guid? usuarioId, Guid? enderecoId,
    int? servicoDeEntregaId = null, string? codigoDoCupom = null,
    ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega, TimeOnly? inicioDaFaixa = null);

// FechamentoDePedidoDTO (acréscimos)
public ModalidadeDeEntrega Modalidade { get; set; }
public TimeOnly? InicioDaFaixa { get; set; }

// PassoDoFechamentoDTO (acréscimos)
public ModalidadeDeEntrega Modalidade { get; init; }
public RetiradaDisponivelDTO? Retirada { get; init; }
public TimeOnly? InicioDaFaixaSelecionada { get; init; }

// MVC
public Task<IActionResult> Index(string? cep = null, PassoDoFechamento passo = PassoDoFechamento.Carrinho,
    Guid? enderecoId = null, int? servicoDeEntregaId = null,
    ModalidadeDeEntrega modalidade = ModalidadeDeEntrega.Entrega, TimeOnly? faixa = null); // CarrinhoController
```

**Banco:** tabelas `AgendaDeRetirada`, `HorarioDeRetirada`, `DiaSemRetirada`.
`Pedido` ganha `Modalidade` (int, padrão 0), `DataDaRetirada`,
`InicioDaRetirada`, `FimDaRetirada` (anuláveis); `EnderecoEntregaId`,
`Transportadora`, `Servico`, `PrazoMinimoEmDias`, `PrazoMaximoEmDias` passam a
aceitar nulo.

`dotnet ef migrations add AddPickupScheduling --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`

## 6. Ordem de execução

- [x] 1. `ModalidadeDeEntrega`, `FaixaDeRetirada`.
- [x] 2. Teste `AgendaDeRetiradaTests` (falhando) → `AgendaDeRetirada`, `HorarioDeRetirada`, `DiaSemRetirada`.
- [x] 3. Teste `PedidoTests` (retirada) → campos de D15 e `ParaRetirada`.
- [x] 4. Teste `HorarioDeBrasiliaTests.Agora` → `HorarioDeBrasilia.Agora`.
- [x] 5. DTOs e `AgendaDeRetiradaMapper`; contratos `IAgendaDeRetiradaRepository` e `IAgendaDeRetiradaService`.
- [x] 6. Testes dos validators → `AgendaDeRetiradaDTOValidator`, `HorarioDeRetiradaDTOValidator`, `DiaSemRetiradaDTOValidator`, `FechamentoDePedidoDTOValidator`.
- [x] 7. Teste `AgendaDeRetiradaServiceTests` → `AgendaDeRetiradaService`.
- [x] 8. Teste `CupomServiceTests` (D13) → `CupomService.Avaliar`.
- [x] 9. Testes `PedidoServiceTests` (montar passo de retirada, fechar com sucesso, faixa vencida, cupom de frete grátis) → `PedidoService`.
- [x] 10. Teste `PedidoMapperTests` → `PedidoMapper` e DTOs de confirmação e detalhe.
- [x] 11. `AgendaDeRetiradaConfiguration`, `PedidoConfiguration`, `DbSet`, `AgendaDeRetiradaRepository`, DI, migration `AddPickupScheduling`.
- [x] 12. Testes de integração (agenda e pedido de retirada) → ajustes de mapeamento; semente em `DbInitializer`.
- [x] 13. Teste `RetiradaControllerTests` → `RetiradaController`, `AgendaDeRetiradaViewModel`, `Areas/Admin/Views/Retirada/Index.cshtml`, item no menu.
- [x] 14. Teste `PedidoControllerTests` (repasse) → `CarrinhoController.Index`, `PedidoController.Fechar`.
- [x] 15. Views do fechamento: `_EnderecoDaLoja`, `_PassoEndereco`, `_OpcoesDeRetirada`, `_PassoPagamento`, `_ItensDoCarrinho`, `Confirmacao`, `Detalhe`, CSS.
- [x] 16. `dotnet build` e `dotnet test` completos. (1 E2E vermelho anterior à feature; ver §9.)
- [ ] 17. Verificação manual (§7).

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Faixas de 60 min das 09:00 às 18:00 | `Dado_SegundaAs07hComFaixaDe60EPreparoDe60_Quando_FaixasDisponiveis_Entao_DeveRetornarNoveFaixas` |
| Última faixa truncada no fechamento | `Dado_FaixaDe90Minutos_Quando_FaixasDisponiveis_Entao_UltimaFaixaDeveTerminarNoFechamento` |
| Preparo corta faixas próximas | `Dado_SegundaAs10h20ComPreparoDe60_Quando_FaixasDisponiveis_Entao_PrimeiraFaixaDeveComecarAs12h` |
| Domingo, dia bloqueado e fim do dia não têm faixa | `Dado_Domingo_…_Entao_DeveRetornarVazio`, `Dado_DiaBloqueado_…`, `Dado_SegundaAs17h30_…` |
| Próxima abertura pula domingo e bloqueio; limite de 14 dias | `Dado_SabadoAs18h_Quando_ProximaAbertura_Entao_DeveSerSegundaAs9h`, `Dado_SemanaTodaFechada_…_Entao_DeveSerNula` |
| Invariantes da agenda | `Dado_AberturaDepoisDoFechamento_Quando_DefinirHorario_Entao_DeveLancarArgumentException`, idem duração, preparo, data passada, data repetida |
| Pedido de retirada sem endereço e frete 0 | `Dado_FaixaValida_Quando_ParaRetirada_Entao_DeveTerModalidadeRetiradaEFreteZero` |
| Fechar retirada com faixa vencida | `Dado_FaixaQueJaPassou_Quando_Fechar_Entao_DeveRecusarComMensagemDeFaixa` |
| Fechar retirada não cota frete | `Dado_Retirada_Quando_Fechar_Entao_NaoDeveChamarFreteService` |
| Cupom de frete grátis na retirada | `Dado_CupomFreteGratisERetirada_Quando_Avaliar_Entao_DeveRecusar` |
| Validator do fechamento por modalidade | `Dado_RetiradaSemFaixa_Quando_Validar_Entao_DeveFalhar`, `Dado_RetiradaSemEndereco_Quando_Validar_Entao_DevePassar` |
| Admin protegido e PRG | `RetiradaControllerTests`: atributo `Authorize` com papel, redirect no sucesso, `View` com `ModelState` inválido, erro de campo para data repetida |
| Agenda persiste com coleções | `Dado_AgendaComHorariosEBloqueio_Quando_SalvarERecarregar_Entao_DeveManterColecoes` |
| Pedido de retirada persiste sem endereço | `Dado_PedidoDeRetirada_Quando_Salvar_Entao_DevePersistirSemEndereco` |
| Tela, cores e opção desabilitada | Verificação manual: passo Endereço em dia útil (faixas), domingo ou depois das 17h (opção desabilitada com próxima abertura), confirmação e detalhe de pedido de retirada, tela do admin. |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Tornar colunas de `Pedido` anuláveis afeta código que lê `Transportadora`/prazos como obrigatórios | Alto | O compilador aponta cada uso (nullable habilitado). Confirmação e detalhe ganham ramo por modalidade; testes de mapper cobrem os dois casos. |
| Banco de produção sem a linha de `AgendaDeRetirada` (semente não rodou) | Alto | `Obter()` lança `InvalidOperationException` com mensagem clara; o `DbInitializer` semeia na subida. Teste de integração confere a semente. |
| Cliente para no pagamento até a faixa passar | Médio | D17: o fechamento recusa com mensagem e a página remonta com as faixas atuais. |
| Fuso fixo em −3 (`HorarioDeBrasilia`) | Baixo | O Brasil não tem horário de verão desde 2019; mesma premissa da `034`/`035`. |
| `MontarPasso` com oito parâmetros | Baixo | D19: opcionais no fim; não quebra chamadores. Refatorar para objeto fica para quando vier o próximo parâmetro. |
| Sem E2E para o fluxo novo | Médio | D20: unidade cobre a regra e o controller; integração cobre o banco; verificação manual cobre a tela. |

Nenhum desvio constitucional.

## 9. Resultado da execução

**Executada em 2026-11-21.** Passos 1 a 16 concluídos na ordem, com o teste escrito e visto
falhando antes de cada implementação. O passo 17 (verificação manual) fica com o responsável.

**Suítes:**
- `dotnet build`: 0 avisos, 0 erros.
- `DocesCabana.Tests` (unidade + integração): **1118 de 1118 verdes**. São 1027 de unidade, 70 a mais
  que antes, e 6 de integração novos.
- `DocesCabana.Tests.E2E`: **16 de 17**. A falha é
  `CadastroDeProdutoTests.Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar`, com a tela
  dizendo "Armazenamento de imagem não configurado.". É o envio de imagem da `027`, que precisa da
  credencial do Supabase, e a `036` não toca esse caminho. Os E2E de fechamento, cupom e meus pedidos
  passam com o passo Endereço novo.

**O que o plano não previu e foi corrigido no caminho:**
1. **Semente da agenda em produção.** `DbInitializer.Semear` só roda fora de produção
  (`Program.cs`). Com a semente dentro dele, a loja publicada ficaria sem `AgendaDeRetirada`, e o
  passo Endereço lançaria `InvalidOperationException` para todo cliente. Esse era o risco alto do §8.
  `SemearAgendaDeRetirada(IServiceProvider)` passou a ser chamado em `Program.cs` em todo ambiente,
  como `PreencherNomesNormalizados`. A semente só cria a agenda se ela não existir; nunca sobrescreve
  o que o admin salvou.
2. **Retirada escolhida sem faixa disponível.** Um link antigo com `modalidade=RetiradaNaLoja`, aberto
  depois do horário, faria o resumo mostrar "Retirada na loja" sem faixa. `MontarPasso` agora volta
  para `Entrega` quando não há faixa hoje, coerente com D5 (opção desabilitada). Teste
  `Dado_RetiradaIndisponivel_Quando_MontarPasso_Entao_DeveVoltarParaEntregaSemFaixaSelecionada`.
3. **Limite de conexões na suíte de integração.** Os 6 testes de integração novos levaram a suíte além
  do `max_connections` (100) do contêiner (`53300: sorry, too many clients already`). Cada teste cria um
  banco próprio, e o pool do Npgsql mantinha uma conexão ociosa por banco. `PostgresDeTeste.CriarBancoNovo`
  passou a usar `Pooling = false`. É mudança só de infraestrutura de teste, sem efeito na aplicação.
4. Os DTOs de confirmação e detalhe ficaram anuláveis no passo 3, e não no 10, porque o mapper não
  compilava sem isso.

**Verificação manual pendente (passo 17):**
- Passo Endereço num dia útil antes das 17h: o seletor aparece, as faixas aparecem, a escolha da faixa
  é mantida, e "Continuar para pagamento" leva ao pagamento com o resumo "Retirada na loja (hoje,
  HH:mm–HH:mm) Grátis".
- Domingo, ou depois do último início possível: "Retirar na loja" desabilitada, com "Próxima: segunda-feira, dd/MM, a partir das 09:00.".
- Cupom de frete grátis aplicado e retirada escolhida: o resumo mostra o motivo da recusa.
- Confirmação e detalhe de um pedido de retirada: data, faixa e endereço da loja com link do mapa.
- `Admin > Retirada na loja`: salvar horários (inclusive marcar domingo aberto), erro de abertura
  depois do fechamento, bloquear data, data repetida, data passada, remover bloqueio.
- Tema escuro e largura de celular nas telas novas (seletor de modalidade, lista de faixas, tela do admin).
- Aplicar a migration `AddPickupScheduling` no banco do Supabase (a aplicação faz isso na subida via
  `DbInitializer.Migrar`).
