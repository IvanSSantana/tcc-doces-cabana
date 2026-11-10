# Plano — Avaliação de produto

**ID:** 032-avaliacao-de-produto · **Criado em:** 2026-11-09
**Status:** Executado

---

## 1. Objetivo

A página do produto já mostra, ordena e pagina avaliações e recebe o voto de
"Útil", mas ninguém consegue escrever uma avaliação: a `008` deixou isso fora de
escopo. Esta feature põe um botão "Avaliar produto" no bloco de avaliações. O
botão abre um modal com nota (1 a 5 estrelas, obrigatória) e comentário
(opcional, até 255 caracteres). O objetivo é **dar confiança a quem está
decidindo comprar**, então só pode avaliar o cliente que tem pedido
**Entregue** com aquele produto. Administradores não avaliam. Cada cliente
avalia um produto uma vez só.

Este é o item "escrever avaliação" do CRUD que o `specs/README.md` reservava
para a `025`. Editar e apagar continuam lá.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | **Só avalia quem tem pedido com status `Entregue` contendo o produto.** | Decisão do debate: a avaliação serve para dar confiança, então vale só a opinião de quem recebeu o doce. | Qualquer pedido não cancelado: funcionaria hoje, mas aceita opinião de quem ainda não recebeu. `Confirmado` em diante: tão inalcançável hoje quanto `Entregue`, e com menos garantia. |
| D2 | **Fica registrado que hoje só usuários do seed conseguem avaliar.** O fechamento cria o pedido como `Pendente` e nada na aplicação avança o status. Só o `DbInitializer` cria pedidos `Entregue`. A regra passa a valer para pedidos reais quando a `024` (pagamento) ou uma gestão de pedidos mover o status. | Decisão do debate, tomada com essa limitação explícita. | Afrouxar a regra até existir o avanço de status: trocaria a confiança por disponibilidade, e o debate escolheu a confiança. |
| D3 | **O modal pede nota obrigatória e comentário opcional.** | Decisão do debate. É exatamente o que a entidade `Avaliacao` já modela (`Nota` 1–5, `Comentario` até 255), e média, estrelas e histograma já dependem da nota. | Comentário obrigatório: regra nova nas duas barreiras, sem pedido. Só comentário: quebraria a média e o histograma. |
| D4 | **O que cada pessoa vê no lugar do botão:** administrador não vê nada. Visitante sem login vê o botão, e o clique abre o `_ModalLogin` (como o "Útil" já faz). Cliente que pode avaliar vê o botão, e o clique abre o modal de avaliação. Cliente que já avaliou vê *"Você já comentou sobre esse produto."*. Cliente que não recebeu o produto vê *"Só quem recebeu este produto pode avaliá-lo."*. | Decisão do debate. | Esconder tudo de quem não pode avaliar: o visitante não descobre que pode avaliar depois de entrar, e o cliente não entende por que o botão não aparece. |
| D5 | **"Já avaliou" é verificado antes de "recebeu".** Quem já tem avaliação vê a mensagem de "já comentou" mesmo sem pedido entregue. | O seed tem avaliações de clientes sem pedido. Para esse cliente, a mensagem verdadeira é "já comentou", não "não recebeu". | Verificar "recebeu" primeiro: mostraria "não recebeu" a quem já tem avaliação publicada. |
| D6 | **A situação é calculada no servidor, num enum da `Application`:** `SituacaoDaAvaliacao { PodeAvaliar, JaAvaliou, NaoRecebeu }`. Ela fica em `ProdutoDetalheDTO.SituacaoDaAvaliacao`, que é `null` para visitante. A view só faz um `switch`. | A regra fica testável em unidade e fora da view. `null` para visitante evita consultas desnecessárias. | Calcular na view ou no controller: espalha a regra e esconde dos testes de serviço. |
| D7 | **O bloqueio do administrador fica na borda web.** A view esconde a área para `User.IsInRole(Papeis.Administrador)`, e a ação `Avaliar` responde `Forbid()` para esse papel, antes de qualquer efeito. | O papel é do Identity, e a `Application` não conhece papéis (Princípio I, emenda 1.6.0). O administrador consegue comprar pelo carrinho, então a regra do pedido entregue não basta para barrá-lo. | Levar o papel para o serviço (por exemplo, `bool ehAdministrador` no `Avaliar`): mistura autorização com regra de negócio e não acrescenta proteção, porque o controller é a única porta. |
| D8 | **"Recebeu" e "já avaliou" são verificados em `AvaliacaoService.Avaliar`, não na entidade.** O serviço lança `InvalidOperationException` com mensagem em português. O índice único `(UsuarioId, ProdutoId)`, que já existe, é a última barreira contra duplicidade. | As duas regras exigem consulta a outros agregados (`Pedido`, outras `Avaliacao`), e a entidade não consulta repositório. A entidade continua protegendo nota e tamanho do comentário. | Construtor `Avaliacao(…, Pedido pedidoEntregue)`: obrigaria o serviço a carregar o pedido inteiro só para provar uma existência, e ainda não cobriria a duplicidade. |
| D9 | **O envio usa o mesmo padrão de modal da `031` (D5 de lá).** O `<dialog>` já vem renderizado na página com o partial `_FormularioAvaliacao`, sem `GET` para carregar. O `POST` é por `fetch` com `FormData` e `X-Requested-With`. Em erro de validação, o servidor responde **400 com o partial preenchido**, e o JS troca o conteúdo do modal. Em sucesso, o servidor faz **redirect**, e o JS navega para `resposta.url`. | Mantém POST-Redirect-Get e a mensagem de campo do FluentValidation (Princípios III e VII). Reaproveita um padrão que a base já tem e testa. O formulário vazio não precisa vir do servidor. | POST tradicional: um erro de validação recarregaria a página com o modal fechado e a mensagem perdida. Montar o modal todo em JS: a validação do servidor não volta para o campo. |
| D10 | **Em sucesso, o redirect vai para `Detalhes/{id}?ordenacao=MaisRecentes#avaliacoes`**, com `TempData["ConfirmacaoAvaliacao"] = "Obrigado pela sua avaliação!"`. | Quem acabou de avaliar quer ver a própria avaliação no topo. Na ordenação padrão (`Relevantes`), uma avaliação sem votos pode cair para fora das 5 primeiras. | Voltar com a ordenação que o cliente estava usando: a avaliação nova pode ficar escondida atrás do "Ver mais". |
| D11 | **Uma `InvalidOperationException` em `Avaliar` redireciona para a página do produto** (pelo referer, com fallback para a Home), no mesmo ramo do `FilterException` que já trata `VotarUtil`. A página recarregada mostra a mensagem certa (D4). | Só acontece por POST adulterado ou por corrida entre duas abas. O ramo genérico de POST tentaria renderizar uma view `Avaliar`, que não existe. | Devolver 400 com a mensagem no modal: precisaria de um ramo de JSON novo no JS para um caso que a tela normal nunca produz. |
| D12 | **O comentário é normalizado no serviço:** só espaços vira `null`, e o resto passa por `Trim()`. | Um comentário só de espaços renderizaria um parágrafo vazio no cartão, que já esconde `IsNullOrWhiteSpace`. Guardar `null` mantém o dado limpo. | Validar como erro: punir quem apagou o texto e deixou espaço não faz sentido, já que o comentário é opcional. |
| D13 | **A nota é escolhida com cinco `<input type="radio" name="Nota">`, desenhados como estrelas só com CSS.** No DTO, a nota é `byte?`, e o validator exige `NotNull` e `InclusiveBetween(1, 5)`. | O radio funciona com teclado e leitor de tela sem JS extra, e `required` no grupo dá a primeira barreira no navegador. `byte?` distingue "não escolheu" de nota 0. | Componente de estrelas em JS: reimplementa acessibilidade que o radio já tem. `byte` não anulável: "sem nota" chegaria como 0 e a mensagem ficaria errada. |
| D14 | **O bloco de avaliações ganha um cabeçalho fixo** ("Avaliações" + a área do botão ou da mensagem), exibido também quando o produto ainda não tem avaliações. | Hoje, sem avaliações, o bloco mostra só "Este produto ainda não tem avaliações.". É justamente aí que o primeiro cliente precisa do botão. | Botão só dentro da grade com avaliações: o primeiro cliente nunca o veria. |
| D15 | **Não há checagem de produto inativo em `Avaliar`.** | `Detalhes` já responde 404 para produto inativo, então o botão é inalcançável. Pela regra D1, quem consegue avaliar recebeu o produto, ou seja, ele existiu. Avaliar um produto inativo via POST adulterado não causa dano. | Injetar `IProdutoRepository` no `AvaliacaoService` só para isso: dependência nova sem caminho real que a exija. |
| D16 | **Nenhum E2E novo.** | O Princípio V exige justificar o que um E2E provaria além das outras camadas. Aqui, o que restaria é o `fetch` do modal, já coberto pelo padrão da `031`. Não há como, pela interface, levar um pedido a `Entregue` de forma determinística (D2), e as avaliações do seed são sorteadas, então um E2E dependeria de sorte. A regra é provada em unidade e integração, e o modal é verificado manualmente (seção 7). | E2E com o cliente do seed: dependeria de ele não ter sido sorteado como avaliador do mesmo produto. |

**Fora de escopo:** editar e apagar avaliação (seguem na `025`); avançar o
status do pedido (`024` ou gestão de pedidos); resposta da loja, denúncia,
foto na avaliação (`008` §8); selo "compra verificada" no cartão; moderação de
conteúdo; e-mail pedindo avaliação após a entrega.

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | Nenhuma `ProjectReference` nova. O enum e o DTO moram na `Application`. O papel de administrador só aparece no MVC (D7). |
| II | Domínio rico e auto-validante | ✅ OK | `Avaliacao` já valida nota, tamanho do comentário e ids no construtor e não muda. As regras que dependem de consulta ficam no serviço (D8). O Princípio II pede invariante na entidade quando ela depende de outro agregado **que a entidade alcança**, e aqui seria preciso uma consulta. |
| III | Validação nas duas barreiras | ✅ OK | Entrada: `NovaAvaliacaoDTOValidator` (nota obrigatória 1–5, comentário ≤ 255). Invariante: construtor de `Avaliacao` (as mesmas regras) + índice único no banco. |
| IV | Nomenclatura em português | ✅ OK | `Avaliar`, `VerificarSituacao`, `SituacaoDaAvaliacao`, `NovaAvaliacaoDTO`, `ExisteEntregueComProduto`, `_FormularioAvaliacao`. O partial de uso único fica em `Views/Produto/`. |
| V | Testes escritos antes | ✅ OK | Cada passo de comportamento da seção 6 começa pelo teste vermelho. Não há E2E novo, com a justificativa em D16. |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `Avaliar` chama `_avaliacaoRepository.Adicionar` + `SalvarAlteracoes`. As consultas novas são métodos de `IAvaliacaoRepository` e `IPedidoRepository`. Sem migration: o índice único já existe. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `Avaliar`: `[HttpPost]`, `[ValidateAntiForgeryToken]`, `[Authorize]`, `Forbid()` para administrador, `ModelState` antes do efeito, `await` no serviço, redirect em sucesso. A resposta 400 com partial é o `return View(dto)` de um modal, como na `031`. |
| VIII | Tratamento de erro por camada | ✅ OK | O serviço lança `InvalidOperationException`, e o `FilterException` a converte em redirect (D11). A ação não tem `try/catch`. Erro de campo vira `ModelState` pelo FluentValidation automático. |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`

Nenhum arquivo.

### `DocesCabana.Application`

| Arquivo | Ação | O quê |
|---|---|---|
| `Enums/SituacaoDaAvaliacao.cs` | criar | `PodeAvaliar = 0`, `JaAvaliou = 1`, `NaoRecebeu = 2` (D6). |
| `DTOs/NovaAvaliacaoDTO.cs` | criar | `ProdutoId`, `Nota` (`byte?`), `Comentario` (`string?`). |
| `Validators/NovaAvaliacaoDTOValidator.cs` | criar | `Nota`: `NotNull` ("Escolha uma nota de 1 a 5.") + `InclusiveBetween(1, 5)`. `Comentario`: `MaximumLength(255)` ("O comentário deve ter no máximo 255 caracteres."). `ProdutoId`: `NotEmpty`. |
| `DTOs/ProdutoDetalheDTO.cs` | alterar | Acrescenta `SituacaoDaAvaliacao? SituacaoDaAvaliacao` (`null` = visitante). |
| `Mappings/ProdutoDetalheMapper.cs` | alterar | `ToDTO` recebe a situação e a repassa. |
| `Contracts/Repositories/IAvaliacaoRepository.cs` | alterar | Acrescenta `ExistePorUsuarioEProduto`. |
| `Contracts/Repositories/IPedidoRepository.cs` | alterar | Acrescenta `ExisteEntregueComProduto`. |
| `Contracts/Services/IAvaliacaoService.cs` | alterar | Acrescenta `VerificarSituacao` e `Avaliar`. |
| `Services/AvaliacaoService.cs` | alterar | Injeta `IPedidoRepository`. `VerificarSituacao` faz já avaliou → `JaAvaliou`, recebeu → `PodeAvaliar`, senão `NaoRecebeu` (D5). `Avaliar` faz as mesmas verificações e lança `InvalidOperationException` ("Você já comentou sobre esse produto." / "Só quem recebeu este produto pode avaliá-lo."). Depois normaliza o comentário (D12), cria a `Avaliacao`, `Adicionar` e `SalvarAlteracoes`. |
| `Services/ProdutoService.cs` | alterar | `BuscarDetalhe`: com `usuarioAtual` presente, chama `_avaliacaoService.VerificarSituacao`. Sem usuário, a situação é `null`. |

### `DocesCabana.Infrastructure`

| Arquivo | Ação | O quê |
|---|---|---|
| `Repositories/AvaliacaoRepository.cs` | alterar | `ExistePorUsuarioEProduto`: `AnyAsync(a => a.UsuarioId == u && a.ProdutoId == p)`. |
| `Repositories/PedidoRepository.cs` | alterar | `ExisteEntregueComProduto`: `AnyAsync(p => p.UsuarioId == u && p.Status == PedidoStatus.Entregue && p.Itens.Any(i => i.ProdutoId == produtoId))`. |

Sem mudança em `ApplicationDependencyInjection`: o container resolve o
construtor novo do `AvaliacaoService` sozinho.

### `DocesCabana.MVC`

| Arquivo | Ação | O quê |
|---|---|---|
| `Controllers/ProdutoController.cs` | alterar | Nova ação `POST Avaliar(NovaAvaliacaoDTO dto)`. Administrador recebe `Forbid()`. Com `ModelState` inválido, responde `PartialView("_FormularioAvaliacao", dto)` com status 400. Em sucesso, `await Avaliar`, grava o `TempData` e redireciona para `Detalhes` com `MaisRecentes` e `#avaliacoes` (D10). |
| `Filters/FilterException.cs` | alterar | O ramo de `InvalidOperationException` de `VotarUtil` passa a valer também para `Avaliar` (D11). |
| `Views/Produto/_BlocoAvaliacoes.cshtml` | alterar | Cabeçalho fixo com título "Avaliações" e a área de ação (D14), mostrado nos dois estados (com e sem avaliações). Exibe `TempData["ConfirmacaoAvaliacao"]` e o `switch` de D4. Com `PodeAvaliar`, renderiza o `<dialog id="modal-avaliacao">` com o `_FormularioAvaliacao`. |
| `Views/Produto/_FormularioAvaliacao.cshtml` | criar | Form `asp-action="Avaliar"` com antiforgery e `ProdutoId` oculto. Tem os cinco radios de estrela (`required`, com rótulo "N estrelas" para leitor de tela), `<textarea name="Comentario" maxlength="255">` com contador, `asp-validation-for` nos dois campos e os botões "Cancelar" e "Enviar avaliação". |
| `wwwroot/js/pages/produto.js` | alterar | Abrir e fechar o `#modal-avaliacao` (botão, "Cancelar", `Esc`). Envio por `fetch` (400 → troca o conteúdo, `redirected` → navega, outra falha → mensagem genérica no modal). Desabilita o botão durante o envio e atualiza o contador de caracteres. Hoje o arquivo sai cedo quando não há seletor de quantidade (`return`), e essa saída precisa ser reestruturada para não impedir o modal num produto fora de estoque. |
| `wwwroot/css/pages/produto.css` | alterar | Cabeçalho do bloco, botão "Avaliar produto", mensagens de situação, modal, estrelas por radio (ordem invertida com `flex-direction: row-reverse` para o hover preencher à esquerda) e contador. Segue as variáveis e o visual do `_ModalLogin`. |

### Testes

| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests/Units/Validators/NovaAvaliacaoDTOValidatorTests.cs` | criar | Casos da seção 7. |
| `DocesCabana.Tests/Units/Services/AvaliacaoServiceTests.cs` | alterar | O construtor ganha o mock de `IPedidoRepository`. Casos de `VerificarSituacao` e `Avaliar`. |
| `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs` | alterar | `BuscarDetalhe` com e sem usuário. |
| `DocesCabana.Tests/Units/Controllers/ProdutoControllerTests.cs` | alterar | Casos de `Avaliar` (administrador, inválido, sucesso). |
| `DocesCabana.Tests/Integration/Repositories/PedidoRepositoryIntegrationTests.cs` | alterar | `ExisteEntregueComProduto` por status, usuário e produto. |
| `DocesCabana.Tests/Integration/Repositories/AvaliacaoRepositoryIntegrationTests.cs` | alterar | `ExistePorUsuarioEProduto`, e `Avaliar` com repositórios reais persistindo a avaliação. |

## 5. Contratos e modelo de dados

```csharp
// Application/Enums/SituacaoDaAvaliacao.cs
public enum SituacaoDaAvaliacao { PodeAvaliar = 0, JaAvaliou = 1, NaoRecebeu = 2 }

// Application/DTOs/NovaAvaliacaoDTO.cs
public class NovaAvaliacaoDTO
{
    public Guid ProdutoId { get; set; }
    public byte? Nota { get; set; }
    public string? Comentario { get; set; }
}

// Application/DTOs/ProdutoDetalheDTO.cs  (acréscimo)
public SituacaoDaAvaliacao? SituacaoDaAvaliacao { get; init; }

// Application/Mappings/ProdutoDetalheMapper.cs
public static ProdutoDetalheDTO ToDTO(
    Produto produto, ResumoAvaliacoesDTO resumoAvaliacoes, PaginaAvaliacoesDTO paginaAvaliacoes,
    SituacaoDaAvaliacao? situacaoDaAvaliacao);

// Application/Contracts/Repositories/IAvaliacaoRepository.cs  (acréscimo)
Task<bool> ExistePorUsuarioEProduto(Guid usuarioId, Guid produtoId);

// Application/Contracts/Repositories/IPedidoRepository.cs  (acréscimo)
Task<bool> ExisteEntregueComProduto(Guid usuarioId, Guid produtoId);

// Application/Contracts/Services/IAvaliacaoService.cs  (acréscimos)
Task<SituacaoDaAvaliacao> VerificarSituacao(Guid produtoId, Guid usuarioId);
Task Avaliar(NovaAvaliacaoDTO dto, Guid usuarioId);

// Application/Services/AvaliacaoService.cs
public AvaliacaoService(IAvaliacaoRepository avaliacaoRepository, IPedidoRepository pedidoRepository, IUnitOfWork unitOfWork);

// MVC/Controllers/ProdutoController.cs
[HttpPost, ValidateAntiForgeryToken, Authorize]
Task<IActionResult> Avaliar(NovaAvaliacaoDTO dto);
```

Não há migration: o índice único `(UsuarioId, ProdutoId)` em `Avaliacao` já
existe (`AvaliacaoConfiguration`).

## 6. Ordem de execução

- [x] 1. Criar `SituacaoDaAvaliacao`, `NovaAvaliacaoDTO` e as assinaturas novas de `IAvaliacaoRepository`, `IPedidoRepository` e `IAvaliacaoService` (compila com `NotImplementedException`). Ajustar o construtor do `AvaliacaoService` e o dos testes existentes.
- [x] 2. Testes de `NovaAvaliacaoDTOValidator` (vermelhos) → criar o validator → verde.
- [x] 3. Testes de integração de `ExisteEntregueComProduto` e `ExistePorUsuarioEProduto` (vermelhos) → implementar nos repositórios → verde.
- [x] 4. Testes de unidade de `VerificarSituacao` (vermelhos) → implementar → verde.
- [x] 5. Testes de unidade de `Avaliar` e o teste de integração que persiste a avaliação (vermelhos) → implementar → verde.
- [x] 6. Testes de `ProdutoService.BuscarDetalhe` com e sem usuário (vermelhos) → acrescentar `SituacaoDaAvaliacao` ao DTO, ao mapper e ao serviço → verde.
- [x] 7. Testes de controller de `Avaliar` (vermelhos) → implementar a ação e criar `_FormularioAvaliacao.cshtml` → verde.
- [x] 8. Ampliar o ramo de `VotarUtil` do `FilterException` para `Avaliar`.
- [x] 9. `_BlocoAvaliacoes.cshtml`: cabeçalho fixo, `switch` da situação, confirmação e `<dialog>`.
- [x] 10. `produto.js` (modal + `fetch` + contador, sem a saída precoce) e `produto.css`.
- [x] 11. `dotnet build` e `dotnet test` completos.
- [x] 12. Verificação manual da seção 7 com a aplicação rodando.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Nota ausente é rejeitada com mensagem | `Dado_NotaNula_Quando_Validar_Entao_DeveFalharEmNota` |
| Nota fora de 1–5 é rejeitada | `Dado_NotaForaDaFaixa_Quando_Validar_Entao_DeveFalharEmNota` (theory 0, 6) |
| Nota 1 e 5 com comentário nulo são aceitas | `Dado_NotaNosLimitesSemComentario_Quando_Validar_Entao_DeveSerValido` (theory 1, 5) |
| Comentário com 256 caracteres é rejeitado e com 255 é aceito | `Dado_ComentarioCom256Caracteres_Quando_Validar_Entao_DeveFalharEmComentario` / `Dado_ComentarioCom255Caracteres_…_DeveSerValido` |
| Só pedido `Entregue` do próprio usuário com o produto conta | integração `Dado_PedidoEntregueComOProduto_Quando_ExisteEntregueComProduto_Entao_DeveSerVerdadeiro` + theory para `Pendente`/`Confirmado`/`Enviado`/`Cancelado` → falso, outro usuário → falso, outro produto → falso |
| Avaliação existente é detectada por usuário e produto | integração `Dado_AvaliacaoDoUsuarioNoProduto_Quando_ExistePorUsuarioEProduto_Entao_DeveSerVerdadeiro` + outro produto → falso |
| "Já avaliou" vence "não recebeu" | `Dado_UsuarioQueJaAvaliouSemPedidoEntregue_Quando_VerificarSituacao_Entao_DeveSerJaAvaliou` |
| Quem recebeu e não avaliou pode avaliar | `Dado_PedidoEntregueSemAvaliacao_Quando_VerificarSituacao_Entao_DeveSerPodeAvaliar` |
| Quem não recebeu não pode | `Dado_SemPedidoEntregue_Quando_VerificarSituacao_Entao_DeveSerNaoRecebeu` |
| Avaliar cria e salva | `Dado_UsuarioQuePodeAvaliar_Quando_Avaliar_Entao_DeveAdicionarESalvar` (verifica nota, comentário e ids da entidade adicionada) |
| Comentário só de espaços vira nulo, e o resto é aparado | `Dado_ComentarioSoDeEspacos_Quando_Avaliar_Entao_DeveGuardarNulo` / `Dado_ComentarioComEspacosNasPontas_Quando_Avaliar_Entao_DeveGuardarAparado` |
| Duplicidade e falta de entrega são barradas sem salvar | `Dado_UsuarioQueJaAvaliou_Quando_Avaliar_Entao_DeveLancarInvalidOperationESemSalvar` / `Dado_UsuarioSemPedidoEntregue_Quando_Avaliar_Entao_DeveLancarInvalidOperationESemSalvar` |
| A avaliação chega ao banco | integração `Dado_UsuarioComPedidoEntregue_Quando_Avaliar_Entao_DevePersistirAvaliacao` |
| Visitante não dispara consulta de situação | `Dado_Visitante_Quando_BuscarDetalhe_Entao_SituacaoDeveSerNulaSemConsultar` |
| Cliente logado recebe a situação no DTO | `Dado_UsuarioAutenticado_Quando_BuscarDetalhe_Entao_DevePreencherSituacao` |
| Administrador é barrado antes do serviço | `Dado_Administrador_Quando_Avaliar_Entao_DeveRetornarForbidSemChamarServico` |
| Erro de campo volta para o modal | `Dado_ModelStateInvalido_Quando_Avaliar_Entao_DeveRetornarParcialComStatus400` |
| Sucesso redireciona para as mais recentes | `Dado_AvaliacaoValida_Quando_Avaliar_Entao_DeveChamarServicoERedirecionarParaMaisRecentes` (verifica `usuarioId`, `TempData` e URL com `#avaliacoes`) |
| Modal, estrelas, contador, mensagens de D4 e redirect de D11 | verificação manual: entrar como um cliente do seed com pedido `Entregue` (`cliente1.seed` → produto mais vendido; `cliente6.seed` → produto pouco vendido), abrir o produto, enviar sem nota (mensagem no campo), enviar com nota (confirmação e avaliação no topo) e recarregar (mensagem "já comentou"). Conferir também: um cliente sem pedido entregue vê o aviso; um visitante abre o modal de login; um administrador não vê nada; um produto fora de estoque abre o modal; e enviar duas vezes de abas diferentes volta para a página sem erro. |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Na prática, só os usuários do seed conseguem avaliar até o status do pedido avançar (D2). | Alto: a feature fica invisível para clientes reais. | Aceito no debate. Fica registrado aqui e no `specs/README.md`. A `024` ou uma gestão de pedidos destrava sem mudar esta feature. |
| As avaliações do seed são sorteadas, então um cliente do seed com pedido entregue pode já ter avaliado o produto. | Baixo: na verificação manual, aparece "já comentou" no lugar do botão. | Se acontecer, testar com o outro cliente do seed da seção 7, ou com outro produto do pedido. |
| Corrida entre duas abas: as duas passam pela verificação e a segunda bate no índice único. O `DbUpdateException` cai no ramo genérico de POST do `FilterException`, que tenta a view `Avaliar`, que não existe. | Baixo: exige dois envios quase simultâneos do mesmo usuário. | O botão de envio é desabilitado durante o `fetch`. O índice garante que o dado nunca duplica. Tratar `DbUpdateException` exigiria conhecer EF no MVC (Princípio I). |
| `produto.js` hoje sai cedo sem o seletor de quantidade, que não existe em produto fora de estoque. | Médio: o modal não abriria nesses produtos. | O passo 10 reestrutura o arquivo, e a verificação manual inclui um produto fora de estoque. |

Nenhum desvio constitucional.

## 9. Resultado da execução

**Suíte:** `dotnet build` com 0 erros. `DocesCabana.Tests`: 709 aprovados, 0 falhas.
E2E: 15 aprovados e 1 falha, `CadastroDeProdutoTests.Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar`.
Esse teste é `[Trait("Categoria", "Externo")]` e falha com "Armazenamento de imagem
não configurado.": exige a `SupabaseSettings__ChaveDeServico` real, que não existe neste
ambiente. O código de cadastro não foi tocado por esta feature.

**Verificação manual (passo 12), feita com um teste Playwright descartável**
(escrito, executado e apagado; não faz parte da entrega, D16 continua valendo).
Contra o app real com o Postgres descartável e o seed, passou: visitante clica em
"Avaliar produto" e abre o modal de login; cliente com pedido `Entregue` abre o modal;
envio sem nota é segurado pelo navegador; contador "15/255"; servidor recusando sem nota
(com o `required` removido) devolve o formulário com a mensagem e preserva o texto
digitado; envio com nota 4 redireciona com "Obrigado pela sua avaliação!", "Você já
comentou sobre esse produto." no lugar do botão e o comentário na lista; produto não
recebido mostra "Só quem recebeu este produto pode avaliá-lo."; administrador vê o
título "Avaliações" e nenhum botão nem aviso.

**O que o plano não previu e foi resolvido na execução (a verificação achou os dois):**
1. **O `fetch` consumia o `TempData`.** O `fetch` do envio segue o redirect e cai na
   `Detalhes` inteira, que lê e apaga o `TempData` antes de o navegador navegar, então a
   confirmação nunca aparecia. A `031` não sofre disso porque o redirect dela cai num
   partial que não lê o `TempData`. Correção dentro da D9: `ProdutoController.Detalhes`
   responde `204 NoContent` quando a requisição traz `X-Requested-With: XMLHttpRequest`,
   sem consultar nem renderizar. Teste:
   `Dado_RequisicaoAjaxDeRedirecionamento_Quando_Detalhes_Entao_DeveRetornarNoContentSemConsultar`.
2. **O `fetch` descarta o `#avaliacoes` da URL de redirect.** O cliente cairia no topo da
   página. Correção: `produto.js` navega para `resposta.url + "#avaliacoes"`.

**Fora do plano, acrescentado:** `DocesCabana.Tests/Units/Filters/FilterExceptionTests.cs`
(3 casos). A mudança no `FilterException` (passo 8) é de comportamento e o filtro não tinha
teste. O parâmetro `situacaoDaAvaliacao` do `ProdutoDetalheMapper.ToDTO` tem valor padrão
`null` (a seção 5 o mostrava sem padrão), para não alterar os testes existentes do mapper.

**Não verificado:** modal em produto *fora de estoque* (a reestruturação do `produto.js`
o cobre por construção, mas nenhum produto assim foi aberto); aparência do modal e das
estrelas (layout, hover, responsividade); teclado e leitor de tela.

**Ponto de atenção:** o `<textarea maxlength="255">` conta quebra de linha como 1 caractere
no navegador, mas o envio manda `
` (2). Um comentário de 255 caracteres com quebras
de linha pode ser recusado pelo servidor com "máximo 255 caracteres". O modal mostra a
mensagem, então não há perda de dado, só um erro possível na borda.
