# Plano — Enxugamento da suíte de testes

**ID:** 030-enxugamento-da-suite-de-testes · **Criado em:** 2026-11-08
**Status:** Executado

---

## 1. Objetivo

A suíte E2E chegou a 182 testes e domina o tempo de `dotnet test`. Cada teste
abre um contexto de navegador e, em geral, cria uma conta e faz login. A maior
parte deles prova layout, funcionamento sem JavaScript ou regras que unidade e
integração já cobrem. Este plano reduz os E2E a **16 jornadas de fluxo
crítico** e faz uma limpeza leve (~31 testes) nas suítes de unidade e
integração. A régua nova entra na constituição para a suíte não voltar a crescer.
Nenhum código de produção muda.

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | Escopo B: enxugar os E2E com rigor e fazer limpeza leve em unidade e integração. | O tempo está nos E2E. Unidade e integração rodam rápido e são a rede de segurança das regras críticas. Ainda assim, há testes de valor baixo nelas (cópia campo a campo, seed, infraestrutura de teste). | A (só E2E): deixa peso morto conhecido. C (as três suítes com o mesmo rigor): tira cobertura de invariante de domínio, que o Princípio II exige. |
| D2 | Régua do E2E: cada fluxo crítico mantém **1 jornada de caminho feliz + no máximo 1 caso de proteção**. A regra de negócio em si se prova em unidade ou integração. | O E2E só deve provar o que as outras camadas não alcançam: autenticação e cookie reais, sessão, e-mail com token, pipeline MVC completo. | Manter 1 E2E por regra: é o modelo atual, que gerou os 182. |
| D3 | Fluxos críticos com E2E: login e controle de acesso, cadastro de cliente, recuperação de senha, carrinho (incluindo a fusão visitante→cliente), fechamento, meus pedidos, cadastro de produto pelo admin, isolamento de endereço entre clientes e uma jornada de catálogo/busca. | Fechado no debate: são a porta de entrada, a venda, o pós-venda, o abastecimento da loja e a segurança. | — |
| D4 | Fluxos sem E2E: favoritos, página inicial/vitrine, páginas institucionais, funcionamento sem JavaScript, layout, responsividade (375px) e foco por teclado. | Fechado no debate. Favoritos e vitrine têm regra coberta em `FavoritoServiceTests`, `CatalogoServiceTests` e `CatalogoRepositoryIntegrationTests`. O resto não é regra de negócio. | Manter os testes sem JS: o funcionamento sem JS não é requisito declarado. |
| D5 | Lista final dos 16 E2E: ver §4 (Testes). 14 são testes existentes mantidos sem alteração e 2 são jornadas novas que consolidam testes cortados. | Reaproveitar o que já está estável. Só consolida onde a jornada não existe num teste só. | Escrever todas as jornadas do zero: mais risco de instabilidade sem ganho. |
| D6 | Jornada nova de carrinho em `CarrinhoTests`: `Dado_ClienteAutenticado_Quando_AcrescentarAlterarERemover_Entao_OCarrinhoDeveAcompanhar`. Acrescenta pelo cartão do catálogo, abre o carrinho, aumenta a quantidade para 2, remove o item e vê a mensagem de vazio. Usa só membros existentes de `PaginaCatalogo` e `PaginaCarrinho`. | Substitui três testes (`AcrescentarDoCartao`, `AumentarQuantidade`, `RemoverItem`) com um cadastro e um login só. | Manter os três: três cadastros e três logins para uma jornada. |
| D7 | Jornada nova de catálogo em `CatalogoTests`: `Dado_Visitante_Quando_BuscarPeloNomeEAbrirOProduto_Entao_DeveChegarNaPaginaDele`. Busca "Raspa Tacho" pela `PaginaInicial`, espera 1 cartão e clica no nome. A URL deve casar com `/Produto/Detalhes/`. | Junta `BuscaTests.Dado_BarraDePesquisa_Quando_BuscarPeloNome_...` e `CatalogoTests.Dado_Catalogo_Quando_ClicarNumProduto_...` numa jornada de descoberta. | Manter os dois testes separados. |
| D8 | `CadastroDeProdutoTests` mantém `Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar` (`[Trait("Categoria","Externo")]`, exige credencial real do Supabase) e `Dado_SemCredencialDeArmazenamento_..._SemCadastrar`. | Sem credencial, o caminho feliz não roda. O teste "sem credencial" é o único que percorre o formulário inteiro no ambiente padrão. | Manter só o caminho feliz: o fluxo do admin ficaria sem E2E na execução padrão. |
| D9 | `FechamentoTests` mantém `Dado_ClienteSemEndereco_..._EFicarEscolhido` (jornada) e `Dado_ServicoDeEntregaIndisponivel_..._SemOferecerContinuar` (proteção). | O ambiente E2E não tem frete, então nenhum pedido chega ao fim. A jornada vai até o passo de endereço. A proteção garante que não se avança sem frete. | Manter `EntreiNoMeioDoFechamento`: a sobrevivência do carrinho ao login já está provada pela fusão (D3). |
| D10 | Arquivos E2E apagados por inteiro: `BuscaTests`, `FavoritosTests`, `PaginaInicialTests`, `PaginasInstitucionaisTests`, `FormularioTests`, `FreteTests`, `FumacaTests`. | Nenhum teste deles sobra (D4, D7). `FumacaTests` é redundante: toda jornada já prova que a aplicação está no ar. `FreteTests` está coberto por D9 e por `CarrinhoControllerTests`. | — |
| D11 | Page objects sem referência depois do corte são apagados: `PaginaFavoritos`, `PaginaPrivacidade`, `PaginaQuemSomos`, `PaginaConfirmarEsvaziarCarrinho`. Nos page objects mantidos, membros que ficarem sem uso também saem, assim como helpers privados sem uso nos arquivos de teste (ex.: `CarrinhoTests.AlterarStatusDoProduto`). | Código morto em teste também é dívida. A busca por referência é mecânica (`grep` + `dotnet build`). | Deixar os arquivos "para o futuro": YAGNI. |
| D12 | Unidade e integração: apagar por inteiro `Units/Controllers/InstitucionalControllerTests.cs`, `Units/Mappings/UsuarioMapperTests.cs`, `Units/Mappings/ProdutoMapperTests.cs`, `Units/Helpers/TextoHelperTests.cs`, `Units/Services/SubcategoriaServiceTests.cs`, `Units/Seed/GeradorDeAvaliacoesTests.cs`, `Units/Seed/DbInitializerProdutosTests.cs`, `Integration/Repositories/DbInitializerPedidosIntegrationTests.cs`, `Integration/Repositories/ProdutoRepositoryIntegrationTests.cs` e `Integration/Infraestrutura/MensagemDePostgresIndisponivelTests.cs`. | Cada um prova algo que não é regra de negócio: ação que só devolve `View`, cópia campo a campo, repasse puro ao repositório, dado de demonstração, CRUD genérico do `Repository<T>` (já coberto por `DatabaseIntegrationTests`) ou texto de mensagem da infraestrutura de teste. A normalização de `TextoHelper` está coberta por `CatalogoServiceTests` e pela busca sem acento em `CatalogoRepositoryIntegrationTests`. Se o seed quebrar, a subida da aplicação no E2E falha. | Apagar validators e invariantes de entidade: o Princípio V exige caso válido e inválido por `RuleFor`, e o Princípio II exige domínio auto-validante. |
| D13 | `Integration/Repositories/ModeloDeDadosIntegrationTests.cs`: saem só os 4 testes de `Include`/navegação (`..._ConsultarSemInclude_...`, `..._ConsultarComInclude_...`). Os 3 de restrição de unicidade e chave estrangeira ficam. | Os testes de `Include` provam comportamento do EF Core, não do negócio. As restrições protegem regra de negócio no banco. | Apagar o arquivo inteiro: perde a prova das restrições. |
| D14 | Ficam todos os outros testes de unidade e integração, inclusive `RegistroDeEmailTests`, `CarrinhoMapperTests`, `ProdutoDetalheMapperTests`, `HomeControllerTests` e `VitrineProdutosTests`. | `RegistroDeEmail` impede que a produção grave e-mail em arquivo por engano. `CarrinhoMapper` calcula o total com frete. Os outros têm lógica própria e custam milissegundos. | — |
| D15 | Emenda constitucional **1.7.0** (MINOR): o Princípio V ganha a régua do E2E de D2 e D4, e a lista de organização ganha a pasta `DocesCabana.Tests.E2E/Fluxos`. | Decidido no debate, para a régua valer nos próximos planos. Expande materialmente um princípio existente. | Registrar só neste plano: ninguém consulta plano antigo ao planejar feature nova. |

**Fora de escopo:** paralelizar a execução dos E2E, reaproveitar sessão
autenticada entre testes (`storageState`) e mexer na subida de
`AplicacaoEmExecucao`. São otimizações de infraestrutura que podem virar outra
feature se 16 jornadas ainda forem lentas. Nenhum código de produção, migration
ou tela muda.

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | OK | Só projetos de teste mudam. |
| II | Domínio rico e auto-validante | n/a | Todos os testes de invariante de entidade ficam (D12). |
| III | Validação nas duas barreiras | n/a | Todos os testes de validator ficam. |
| IV | Nomenclatura em português | OK | As 2 jornadas novas seguem `Dado_/Quando_/Entao_`. |
| V | Testes escritos antes | OK | Não há comportamento novo. As 2 jornadas consolidam testes que já passam contra o código atual. O princípio é emendado (D15) sem conflito com o ciclo teste→implementação. |
| VI | Repositório + commit via UnitOfWork | n/a | `DatabaseIntegrationTests` continua provando o commit explícito. |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | n/a | A autorização continua provada em E2E (acesso admin, isolamento de endereço) e em unidade. |
| VIII | Tratamento de erro por camada | OK | Sem mudança. |

## 4. Impacto arquivo por arquivo

### Testes — `DocesCabana.Tests.E2E/Fluxos` (de 182 para 16)

| Arquivo | Ação | Testes que ficam |
|---|---|---|
| `LoginTests.cs` | Reduzir 3→2 | `Dado_ContaExistente_Quando_EntrarComEmailEComCpf_Entao_DeveAutenticarNosDois`, `Dado_Autenticado_Quando_Sair_Entao_DevePerderAcessoAAreaAdministrativa` |
| `AreaAdministrativaTests.cs` | Reduzir 10→3 | `Dado_Visitante_Quando_AbrirAreaAdministrativa_Entao_DeveLevarAoLogin`, `Dado_ClienteComum_Quando_AbrirAreaAdministrativa_Entao_DeveReceberAcessoNegado`, `Dado_Administrador_Quando_CadastrarOutroAdministrador_Entao_ELeDeveEntrarEUsarAArea` |
| `CadastroDeClienteTests.cs` | Reduzir 5→1 | `Dado_DadosIneditos_Quando_CadastrarCliente_Entao_DeveLevarAoLoginEPermitirEntrar` |
| `RecuperacaoDeSenhaTests.cs` | Reduzir 2→1 | `Dado_PedidoDeRedefinicao_Quando_SeguirOLinkETrocarASenha_Entao_DeveEntrarComANovaENaoComAAntiga` |
| `CarrinhoTests.cs` | Reduzir 29→2 | **novo** `Dado_ClienteAutenticado_Quando_AcrescentarAlterarERemover_Entao_OCarrinhoDeveAcompanhar` (D6), `Dado_CarrinhosNosDoisLados_Quando_Entrar_Entao_AsQuantidadesDevemSomar` |
| `FechamentoTests.cs` | Reduzir 5→2 | `Dado_ClienteSemEndereco_Quando_ChegarAoPassoDeEndereco_Entao_DeveCadastrarAliMesmoEFicarEscolhido`, `Dado_ServicoDeEntregaIndisponivel_Quando_ChegarAoEnderecoEscolhido_Entao_DeveAvisarSemOferecerContinuar` |
| `MeusPedidosTests.cs` | Reduzir 5→1 | `Dado_UmPedido_Quando_Abrir_Entao_DeveLevarAoDetalheComItensEValores` |
| `ContaTests.cs` | Reduzir 21→1 | `Dado_EnderecoDeOutraPessoa_Quando_TentarAbrirEditarOuExcluir_Entao_NaoDeveConseguir` |
| `CadastroDeProdutoTests.cs` | Reduzir 5→2 | `Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar` (Externo), `Dado_SemCredencialDeArmazenamento_Quando_CadastrarProduto_Entao_DeveRecusarComMensagemEspecificaSemCadastrar` |
| `CatalogoTests.cs` | Reduzir 45→1 | **novo** `Dado_Visitante_Quando_BuscarPeloNomeEAbrirOProduto_Entao_DeveChegarNaPaginaDele` (D7) |
| `BuscaTests.cs` | Apagar | — |
| `FavoritosTests.cs` | Apagar | — |
| `PaginaInicialTests.cs` | Apagar | — |
| `PaginasInstitucionaisTests.cs` | Apagar | — |
| `FormularioTests.cs` | Apagar | — |
| `FreteTests.cs` | Apagar | — |
| `FumacaTests.cs` | Apagar | — |

### Testes — `DocesCabana.Tests.E2E/Paginas`

| Arquivo | Ação | O quê |
|---|---|---|
| `PaginaFavoritos.cs` | Apagar | Sem referência após o corte |
| `PaginaPrivacidade.cs` | Apagar | Sem referência após o corte |
| `PaginaQuemSomos.cs` | Apagar | Sem referência após o corte |
| `PaginaConfirmarEsvaziarCarrinho.cs` | Apagar | Sem referência após o corte |
| `PaginaInicial.cs`, `PaginaCatalogo.cs`, `PaginaCarrinho.cs`, `PaginaConta.cs`, `PaginaFechamento.cs`, `PaginaMeusPedidos.cs`, `PaginaAdministradores.cs`, `PaginaCadastro.cs`, `PaginaLogin.cs`, `PaginaCadastroProduto.cs`, `PaginaEsqueceuSenha.cs`, `PaginaRedefinirSenha.cs` | Ajustar | Remover membros sem uso (D11) |

### Testes — `DocesCabana.Tests`

| Arquivo | Ação | O quê |
|---|---|---|
| `Units/Controllers/InstitucionalControllerTests.cs` | Apagar | 2 testes, ações só devolvem `View` |
| `Units/Mappings/UsuarioMapperTests.cs` | Apagar | 1 teste, cópia campo a campo |
| `Units/Mappings/ProdutoMapperTests.cs` | Apagar | 4 testes, cópia campo a campo |
| `Units/Helpers/TextoHelperTests.cs` | Apagar | 1 teste, coberto pela busca |
| `Units/Services/SubcategoriaServiceTests.cs` | Apagar | 2 testes, repasse puro |
| `Units/Seed/GeradorDeAvaliacoesTests.cs` | Apagar | 8 testes, dado de demonstração |
| `Units/Seed/DbInitializerProdutosTests.cs` | Apagar | 2 testes, dado de demonstração. A pasta `Units/Seed/` fica vazia e sai |
| `Integration/Repositories/DbInitializerPedidosIntegrationTests.cs` | Apagar | 1 teste, dado de demonstração |
| `Integration/Repositories/ProdutoRepositoryIntegrationTests.cs` | Apagar | 3 testes, CRUD genérico |
| `Integration/Infraestrutura/MensagemDePostgresIndisponivelTests.cs` | Apagar | 3 testes, texto da infraestrutura de teste. A classe `MensagemDePostgresIndisponivel` fica (é usada) |
| `Integration/Repositories/ModeloDeDadosIntegrationTests.cs` | Reduzir 7→3 | Remover os 4 testes de `Include` (D13) |
| `DocesCabana.Infrastructure/DocesCabana.Infrastructure.csproj` | Verificar | Se nenhum teste restante usar membro `internal` do seed, avaliar se o `InternalsVisibleTo DocesCabana.Tests` ainda é necessário. Remover só se o build continuar verde |

### Documentação

| Arquivo | Ação | O quê |
|---|---|---|
| `.specify/memory/constitution.md` | Emendar | Versão 1.7.0, régua do E2E no Princípio V e linha no histórico (D15) |
| `specs/README.md` | Ajustar | Linha da `030` |

## 5. Contratos e modelo de dados

Nenhum contrato, entidade ou migration muda.

Texto da emenda, ao fim do Princípio V, antes da "Definição de pronto":

> `DocesCabana.Tests.E2E/Fluxos` — ponta a ponta com Playwright, **só para
> jornada de fluxo crítico**: um caminho feliz e no máximo um caso de proteção
> por fluxo. Regra de negócio se prova em unidade ou integração, não em E2E.
> Layout, responsividade, foco por teclado e funcionamento sem JavaScript não
> têm E2E. Um E2E novo precisa dizer, no plano, o que ele prova que as outras
> camadas não alcançam (cookie, sessão, e-mail, pipeline MVC completo).

## 6. Ordem de execução

- [x] 1. Escrever a jornada nova de catálogo (D7) em `CatalogoTests.cs` e rodá-la isolada (`--filter`). Deve passar contra o código atual.
- [x] 2. Escrever a jornada nova de carrinho (D6) em `CarrinhoTests.cs` e rodá-la isolada. Deve passar.
- [x] 3. Reduzir os 10 arquivos de fluxo mantidos à lista do §4 e apagar os 7 arquivos de fluxo.
- [x] 4. Apagar os 4 page objects sem referência. Remover membros e helpers privados sem uso (`grep` por nome em `DocesCabana.Tests.E2E`). `dotnet build`.
- [x] 5. `dotnet test DocesCabana.Tests.E2E --list-tests`: devem ser 16 testes. Rodar a suíte E2E completa.
- [x] 6. Apagar os 10 arquivos de unidade/integração de D12 e os 4 testes de D13. Verificar o `InternalsVisibleTo`. `dotnet build`.
- [x] 7. Emendar a constituição (D15).
- [x] 8. `dotnet build` e `dotnet test` completos.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| A busca leva ao produto e à página dele | `Dado_Visitante_Quando_BuscarPeloNomeEAbrirOProduto_Entao_DeveChegarNaPaginaDele` |
| Acrescentar, alterar e remover funcionam no carrinho real | `Dado_ClienteAutenticado_Quando_AcrescentarAlterarERemover_Entao_OCarrinhoDeveAcompanhar` |
| Nenhuma regra crítica perdeu cobertura | Para cada E2E cortado com regra de negócio, a regra tem teste equivalente: teto 99, item indisponível, fusão e esvaziar em `CarrinhoServiceTests`; principal de endereço em `EnderecoServiceTests`; ordenação, paginação, filtros, inativo e busca sem acento em `CatalogoRepositoryIntegrationTests` e `CatalogoServiceTests`; duplicidade de cadastro em `AutenticacaoControllerTests` e `UsuarioServiceCadastroTests`; pedidos e isolamento em `PedidoServiceTests`; favoritos em `FavoritoServiceTests` |
| A suíte E2E tem exatamente 16 testes | `dotnet test DocesCabana.Tests.E2E --list-tests` |
| A régua está na constituição | Leitura do Princípio V e do histórico, versão 1.7.0 |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Regressão de layout ou do funcionamento sem JS passa despercebida | Médio | Decisão consciente (D4): não é regra de negócio. Fica para verificação manual na apresentação. |
| Regressão de view/JS em fluxo cortado (ex.: o controle de favoritar some da tela) | Médio | Serviço e controller continuam testados. Só a ligação view↔controller perde a prova automática, e um defeito ali é visível no primeiro uso manual. |
| O caminho feliz do cadastro de produto só roda com credencial real (`Externo`) | Médio | D8 mantém o caso "sem credencial", que percorre o formulário inteiro na execução padrão. |
| A jornada consolidada fica instável por fazer mais passos num teste só | Baixo | Usa só métodos de page object que já passam hoje. Os passos 2 e 3 a rodam isolada antes do corte. |
| 16 jornadas continuarem lentas por causa da subida (contêiner + processo) | Baixo | Fora de escopo. Se incomodar depois, vira outra feature de infraestrutura. |

Nenhum desvio constitucional.

## 9. Resultado da execução

**Passou** (`dotnet build` sem avisos; `dotnet test --filter "Categoria!=Externo"`):

| Suíte | Antes | Depois |
|---|---|---|
| E2E | 182 testes | 16 listados, 15 executados e aprovados em 42 s |
| Unidade e integração | 684 (13 s) | 647 aprovados em 13 s |

**Feito além da lista dos passos:**
- 84 membros mortos saíram dos page objects e dos testes (`AlterarStatusDoProduto`, `MensagemDeDuplicidade` e propriedades/métodos de `Pagina*`). Os `using Npgsql` e `using Microsoft.Playwright` ficaram sem uso em `CarrinhoTests` e `CatalogoTests` e foram removidos.
- O comentário do `InternalsVisibleTo` no `Infrastructure.csproj` citava `GerarAvaliacoesMock`, cujo teste foi apagado. Agora cita `PreencherNomesNormalizados`.

**Divergências do plano:**
- O `InternalsVisibleTo DocesCabana.Tests` **ficou**: `CatalogoRepositoryIntegrationTests` usa `DbInitializer.PreencherNomesNormalizados`, que é `internal`. Retirar o atributo quebra o build.
- Unidade e integração perderam 37 testes, não os 31 previstos, porque os arquivos apagados tinham linhas de `[Theory]` contadas separadamente.
- A medição de tempo antes/depois foi retirada do plano a pedido do responsável. Os 42 s e 13 s acima são só o tempo da execução final.
- A primeira tentativa de baseline E2E travou com 12 falhas em `CatalogoTests` e foi cancelada. Depois do corte, `CatalogoTests` passou sem falhas, o que sugere que elas vieram daquela execução quebrada (Docker). Não foi investigado a fundo.

**Verificação manual pendente:**
- `Dado_Administrador_Quando_CadastrarProduto_Entao_DeveConfirmar` (`Categoria=Externo`) exige a credencial real do Supabase e não foi executado.
- Layout, funcionamento sem JavaScript e a ligação view↔controller de favoritos, página inicial e páginas institucionais deixaram de ter teste automatizado (D4). Conferir na aplicação rodando antes da apresentação.
