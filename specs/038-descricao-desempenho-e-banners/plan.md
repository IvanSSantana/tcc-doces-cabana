# Plano — Quebra de descrição, desempenho e banners do carrossel

**ID:** 038-descricao-desempenho-e-banners · **Criado em:** 2026-11-24
**Status:** Aprovado

---

## 1. Objetivo

Três ajustes pedidos juntos. (1) Descrições longas sem espaço (palavra ou
endereço colado) estouram a largura da página do produto numa linha só: o
texto passa a quebrar dentro do bloco. (2) Quase toda página demora para
renderizar, e as ações do carrinho (trocar quantidade, remover, cotar frete)
são as piores. A causa principal medida na leitura do código é a distância do
banco: o Postgres está em `aws-0-us-east-2` (EUA) e a aplicação roda no
Brasil, então cada consulta paga ~150 ms de ida e volta. Uma ação do carrinho
faz ~10 consultas e ainda chama a API da Melhor Envio. O plano muda o banco
para `sa-east-1` (São Paulo) e corta consultas repetidas. (3) O carrossel da
home deixa de ser três imagens fixas: o administrador cadastra banners com
imagem, período e destino de clique (categoria, subcategoria ou produto).

## 2. Decisões

| # | Decisão | Por quê | Alternativa descartada |
|---|---|---|---|
| D1 | Descrição do produto, comentário de avaliação e descrição no detalhe administrativo ganham `overflow-wrap: anywhere` e `white-space: pre-line` | `.texto-caracteristicas` já tem `pre-line` (respeita Enter), mas não tem `overflow-wrap`: sequência sem espaço não quebra. `.detalhe-produto__campos dd` tem o inverso (quebra palavra, ignora Enter). `.comentario-avaliacao` não tem nenhum dos dois | `word-break: break-all` — quebra no meio de palavras normais também, piora a leitura do texto comum |
| D2 | **Debate:** migrar o banco para um projeto Supabase novo em `sa-east-1`, além de otimizar o código | Latência por consulta domina o tempo de toda página; nenhuma otimização de código chega perto de tirar ~130 ms de cada ida e volta | Só otimizar código — ganho menor, mantém o custo por consulta |
| D3 | Migração preserva dados (`pg_dump`/`pg_restore`) e leva as imagens do Storage junto, com `UPDATE` do prefixo das URLs de imagem | `Produto.ImagemUrl` guarda URL absoluta do projeto antigo; apagar o projeto antigo quebraria todas as imagens. Projeto inativo no plano gratuito é pausado | Recriar banco vazio e semear — perde produtos e imagens cadastrados pelo admin |
| D4 | Cache em memória atrás de contrato da `Application` (`ICacheDaAplicacao`), implementado na `Infrastructure` com `IMemoryCache` | Mantém Princípio I: a `Application` não conhece `Microsoft.Extensions.Caching`. Serviços decidem o que cachear e quando invalidar | Decorators de repositório na `Infrastructure` — esconderiam a regra de invalidação longe do serviço que grava |
| D5 | Cacheados: `CategoriaService.ListarComSubcategorias` (5 min + invalidação em escrita de produto), `PromocaoService.CarregarVigentes` (chave por dia + invalidação em escrita de promoção), banners vigentes da home (chave por dia + invalidação em escrita de banner) | São lidos em quase toda página (cabeçalho, catálogo, carrinho, vitrine) e mudam só por ação do admin. `CarregarVigentes` custa 3 idas ao banco (`AsSplitQuery` + subcategorias) | Cachear também carrinho/agenda — dado por usuário ou por horário; erro de cache ali vira preço ou faixa errada |
| D6 | Cotação de frete cacheada por 10 min, chave = CEP + (produto, quantidade) de cada linha | Hoje toda ação do carrinho com CEP na sessão chama a Melhor Envio, mesmo sem mudar o carrinho (ex.: aplicar cupom, trocar passo) | Cachear só por CEP — devolveria frete de outro conteúdo de carrinho |
| D7 | `CarrinhoService.MontarAvulso` troca a consulta por item por `IProdutoRepository.BuscarPorIds` | N+1: visitante com 5 itens paga 5 idas ao banco. `BuscarPorIds` já existe | — |
| D8 | Medir antes e depois (contagem de comandos SQL e tempo da requisição) em três rotas: `GET /`, `GET /Catalogo`, `POST /Carrinho/AlterarQuantidade` assíncrono | Sem medição não dá para dizer se o pedido "está lento" foi atendido | — |
| D9 | **Debate:** banner tem ordem, ativo/inativo e período | Pedido do responsável: banner sazonal sai sozinho | Só ordem + ativo |
| D10 | Período: `DataInicio` obrigatória, `DataFim` opcional (vazio = sem fim) | Banner institucional permanente não deveria exigir data de fim inventada | Fim obrigatório, como `Promocao` |
| D11 | **Debate:** destino do clique é obrigatório; exatamente um entre categoria, subcategoria e produto | Pedido do responsável | Destino opcional |
| D12 | Destino guardado por FK (`CategoriaId`/`SubcategoriaId`/`ProdutoId`), URL montada na renderização com `EnderecoDoCatalogo` / `Produto/Detalhes` | Renomear categoria muda o apelido; URL gravada quebraria. Mesmo desenho da abrangência de `Promocao` | Gravar URL livre — admin digitaria endereço errado e sem validação |
| D13 | Banner com destino em produto `Inativo` não aparece no carrossel | Página de produto inativo responde 404 (`008`); banner levaria a erro. Produto fora de estoque continua aparecendo (a página existe) | Mostrar e deixar o 404 |
| D14 | **Debate:** sem banner vigente, a home mostra as imagens fixas atuais, sem link | Pedido do responsável. A lista fixa hoje repete as três imagens duas vezes (6 slides); a reserva fica com 3 | Esconder o carrossel |
| D15 | Texto alternativo obrigatório (até 150 caracteres) | Imagem com destino é link; link só com imagem sem `alt` é inacessível. Hoje o `alt` é "Banner 1" | Gerar `alt` do nome do destino — descreve o destino, não a imagem |
| D16 | Imagem enviada por `IArmazenamentoDeImagem` sem mudança, validada pelo `ImagemParaEnvioDTOValidator` existente; formulário informa a proporção do carrossel (3,2 : 1) | Reuso total do caminho da `027` | Pasta própria no bucket — exige mudar o contrato por ganho só organizacional |
| D17 | Ordem é um número digitado; empate desempata por `DataInicio` | Poucos banners; arrastar para ordenar é JS novo sem ganho proporcional | Arrastar e soltar |
| D18 | Gestão em `Areas/Admin/Controllers/BannerController` + link "Banners" no menu admin do cabeçalho | Mesmo desenho de `PromocaoController` (Index, Cadastro, Edicao, Ativar, Desativar) | — |

**Fora de escopo:** imagem separada para celular; agendamento por hora;
cache de agenda de retirada e de carrinho; adiar o script do Font Awesome;
trocar o pooler de Session para Transaction; E2E para banners (não é fluxo
crítico de compra — Princípio V).

## 3. Verificação constitucional

| # | Princípio | Situação | Observação |
|---|---|---|---|
| I | Direção de dependência preservada | ✅ OK | `ICacheDaAplicacao` na `Application`; `IMemoryCache` só na `Infrastructure`. Nenhum `<ProjectReference>` novo |
| II | Domínio rico e auto-validante | ✅ OK | `Banner` com `private set`, construtor valida, métodos de intenção, `protected Banner()` |
| III | Validação nas duas barreiras | ✅ OK | `BannerDTOValidator` (formato, obrigatoriedade, um destino) + invariantes em `Banner` |
| IV | Nomenclatura em português | ✅ OK | `Banner`, `TipoDeDestinoDoBanner`, `ICacheDaAplicacao`. "Banner" é o termo usado pela loja |
| V | Testes escritos antes | ✅ OK | Entidade, validator, serviço, controller e repositório (integração) com teste antes. CSS e migração de região: verificação manual |
| VI | Repositório + commit via UnitOfWork | ✅ OK | `IBannerRepository`; `BannerService` chama `SalvarAlteracoes`. Migration `AddBanners` |
| VII | Antiforgery, `await`, autorização, POST-Redirect-Get | ✅ OK | `BannerController` com `[Authorize(Roles = Papeis.Administrador)]`, POST com `[ValidateAntiForgeryToken]`, PRG |
| VIII | Tratamento de erro por camada | ✅ OK | Domínio lança `ArgumentException`; serviço lança `KeyNotFoundException`; falha de envio de imagem vira `ModelState` |

## 4. Impacto arquivo por arquivo

### `DocesCabana.Domain`
| Arquivo | Ação | O quê |
|---|---|---|
| `Entities/Banner.cs` | criar | Entidade (ver §5) |
| `Enums/TipoDeDestinoDoBanner.cs` | criar | `Categoria`, `Subcategoria`, `Produto` |

### `DocesCabana.Application`
| Arquivo | Ação | O quê |
|---|---|---|
| `Contracts/Services/ICacheDaAplicacao.cs` | criar | Contrato de cache (ver §5) |
| `Contracts/Repositories/IBannerRepository.cs` | criar | Ver §5 |
| `Contracts/Services/IBannerService.cs` | criar | Ver §5 |
| `DTOs/BannerDTO.cs` | criar | Formulário de cadastro/edição |
| `DTOs/BannerDoCarrosselDTO.cs` | criar | Leitura da home: imagem, `alt`, tipo e dados do destino (apelidos, id do produto) |
| `DTOs/ResumoDeBannerDTO.cs` | criar | Linha da listagem administrativa |
| `Validators/BannerDTOValidator.cs` | criar | Texto alternativo 1..150, `DataInicio` obrigatória, `DataFim >= DataInicio` quando informada, `Ordem >= 0`, exatamente um destino coerente com o tipo |
| `Mappings/BannerMapper.cs` | criar | Entidade ↔ DTOs |
| `Services/BannerService.cs` | criar | Gestão + `ListarVigentesDoCarrossel` com cache por dia; invalida a chave em toda escrita |
| `Constants/ChavesDeCache.cs` | criar | Chaves: categorias, promoções vigentes (prefixo + data), banners vigentes (prefixo + data) |
| `Services/CategoriaService.cs` | alterar | `ListarComSubcategorias` via `ICacheDaAplicacao` (5 min) |
| `Services/ProdutoService.cs` | alterar | `Cadastrar`, `Atualizar`, `Inativar` removem a chave de categorias e de banners (contagem por subcategoria e D13 mudam) |
| `Services/PromocaoService.cs` | alterar | `CarregarVigentes` via cache por dia; `Cadastrar`, `Atualizar`, `Ativar`, `Desativar` removem a chave |
| `Services/CarrinhoService.cs` | alterar | `MontarAvulso` usa `BuscarPorIds` (D7) |

### `DocesCabana.Infrastructure`
| Arquivo | Ação | O quê |
|---|---|---|
| `Services/CacheEmMemoria.cs` | criar | `ICacheDaAplicacao` sobre `IMemoryCache` |
| `Services/FreteServiceComCache.cs` | criar | Decorator de `IFreteService`: cacheia só cotação com opções (falha não é cacheada), 10 min (D6) |
| `Repositories/BannerRepository.cs` | criar | Ver §5; leitura vigente com `AsNoTracking` e `Include` de categoria/subcategoria/produto |
| `DatabaseContext/Configurations/BannerConfiguration.cs` | criar | Tabela `Banners`, FKs com `Restrict`, `TextoAlternativo` 150, índice `(Ativo, DataInicio)`, check de exatamente um destino |
| `DatabaseContext/DocesCabanaDbContext.cs` | alterar | `DbSet<Banner> Banners` |
| `DependencyInjections/ApplicationDependencyInjection.cs` | alterar | `AddMemoryCache`, `ICacheDaAplicacao`, `IBannerRepository`, `IBannerService`; `AddFreteService` registra o cliente HTTP e embrulha com `FreteServiceComCache` |
| `Migrations/*_AddBanners.cs` | criar | Migration |

### `DocesCabana.MVC`
| Arquivo | Ação | O quê |
|---|---|---|
| `Areas/Admin/Controllers/BannerController.cs` | criar | `Index`, `Cadastro` (GET/POST), `Edicao` (GET/POST), `Ativar`, `Desativar` (POST). Imagem obrigatória no cadastro, opcional na edição (mesmo fluxo de `ProdutoController`) |
| `Models/FormularioDeBannerViewModel.cs` | criar | `BannerDTO`, categorias com subcategorias, produtos ativos, URL da imagem atual |
| `Areas/Admin/Views/Banner/Index.cshtml` | criar | Lista: miniatura, `alt`, destino, período, ordem, situação, ações |
| `Areas/Admin/Views/Banner/Cadastro.cshtml` | criar | Usa o formulário parcial |
| `Areas/Admin/Views/Banner/Edicao.cshtml` | criar | Usa o formulário parcial |
| `Areas/Admin/Views/Banner/_FormularioBanner.cshtml` | criar | Imagem (dica 3,2 : 1), `alt`, ordem, período, tipo de destino (rádio) + seletor correspondente |
| `wwwroot/css/pages/gestao-banners.css` | criar | Estilo da gestão, nos tokens da `033` |
| `wwwroot/js/pages/gestao-banners.js` | criar | Mostra só o seletor do tipo escolhido; sem JS todos aparecem e o validator decide |
| `Views/Shared/Components/Header/Default.cshtml` | alterar | Link "Banners" no menu admin |
| `Controllers/HomeController.cs` | alterar | Injeta `IBannerService`; passa banners vigentes para a view |
| `Views/Home/Index.cshtml` | alterar | `<partial name="_Carrossel" model="banners" />` |
| `Views/Home/_Carrossel.cshtml` | alterar | Renderiza banners como `<a>` com `<img alt>`; sem banner, as 3 imagens fixas sem link (D14) |
| `wwwroot/js/carrossel.js` | alterar | Seletores passam a aceitar o item como `<a>`; com 1 slide, sem autoplay e sem setas |
| `wwwroot/css/components/carrossel.css` | alterar | Item-link ocupa o slide inteiro |
| `wwwroot/css/pages/produto.css` | alterar | `.texto-caracteristicas` e `.comentario-avaliacao`: `overflow-wrap: anywhere`; comentário ganha `white-space: pre-line` |
| `wwwroot/css/pages/gestao-produtos.css` | alterar | `.detalhe-produto__campos dd`: `white-space: pre-line` |

### Testes
| Arquivo | Ação | O quê |
|---|---|---|
| `DocesCabana.Tests/Units/Entities/BannerTests.cs` | criar | Invariantes |
| `DocesCabana.Tests/Units/Validators/BannerDTOValidatorTests.cs` | criar | Cada `RuleFor` válido e inválido |
| `DocesCabana.Tests/Units/Services/BannerServiceTests.cs` | criar | Gestão, vigência, D13, cache e invalidação |
| `DocesCabana.Tests/Units/Services/CategoriaServiceTests.cs` | alterar | Segunda chamada não consulta o repositório |
| `DocesCabana.Tests/Units/Services/PromocaoServiceTests.cs` | alterar | Cache por dia; escrita invalida |
| `DocesCabana.Tests/Units/Services/ProdutoServiceTests.cs` | alterar | Escrita invalida categorias e banners |
| `DocesCabana.Tests/Units/Services/CarrinhoServiceTests.cs` | alterar | `MontarAvulso` chama `BuscarPorIds` uma vez |
| `DocesCabana.Tests/Units/Services/FreteServiceComCacheTests.cs` | criar | Mesma chave não chama o interno; quantidade diferente chama; falha não fica em cache |
| `DocesCabana.Tests/Units/Controllers/Admin/BannerControllerTests.cs` | criar | `ModelState`, PRG, falha de envio de imagem vira erro de campo |
| `DocesCabana.Tests/Units/Controllers/HomeControllerTests.cs` | alterar | `Index` entrega banners vigentes |
| `DocesCabana.Tests/Integration/Repositories/BannerRepositoryIntegrationTests.cs` | criar | Vigência, ordem, `DataFim` nula, produto inativo, check de destino |

Os testes unitários de serviço usam um `ICacheDaAplicacao` falso em memória
(sem `Moq` de cache), para provar o comportamento e não a chamada.

## 5. Contratos e modelo de dados

```csharp
// Domain/Entities/Banner.cs
public class Banner
{
    public Guid BannerId { get; private set; }
    public string ImagemUrl { get; private set; }
    public string TextoAlternativo { get; private set; }
    public short Ordem { get; private set; }
    public DateOnly DataInicio { get; private set; }
    public DateOnly? DataFim { get; private set; }
    public bool Ativo { get; private set; }
    public Guid? CategoriaId { get; private set; }
    public Guid? SubcategoriaId { get; private set; }
    public Guid? ProdutoId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public Subcategoria? Subcategoria { get; private set; }
    public Produto? Produto { get; private set; }
    public TipoDeDestinoDoBanner TipoDeDestino { get; }   // derivado das FKs

    protected Banner() { }
    public Banner(string imagemUrl, string textoAlternativo, short ordem,
        DateOnly dataInicio, DateOnly? dataFim,
        Guid? categoriaId, Guid? subcategoriaId, Guid? produtoId, Guid id = default);

    public void AlterarImagem(string imagemUrl);
    public void AlterarApresentacao(string textoAlternativo, short ordem);
    public void AlterarPeriodo(DateOnly dataInicio, DateOnly? dataFim);
    public void AlterarDestino(Guid? categoriaId, Guid? subcategoriaId, Guid? produtoId); // exatamente um, nenhum Guid.Empty
    public void Ativar();
    public void Desativar();
    public bool EstaVigente(DateOnly hoje);
}

// Application/Contracts/Services/ICacheDaAplicacao.cs
public interface ICacheDaAplicacao
{
    Task<T> ObterOuCriar<T>(string chave, Func<Task<T>> fabrica, TimeSpan duracao);
    void Remover(string chave);
}

// Application/Contracts/Repositories/IBannerRepository.cs
public interface IBannerRepository : IRepository<Banner>
{
    Task<List<Banner>> ListarVigentes(DateOnly hoje);   // ativos, no período, produto-destino não inativo, por Ordem, DataInicio
    Task<List<Banner>> ListarParaGestao();
}

// Application/Contracts/Services/IBannerService.cs
public interface IBannerService
{
    Task<IReadOnlyList<BannerDoCarrosselDTO>> ListarVigentesDoCarrossel();
    Task<IReadOnlyList<ResumoDeBannerDTO>> ListarParaGestao();
    Task<BannerDTO> BuscarParaEdicao(Guid bannerId);
    Task Cadastrar(BannerDTO dto, string imagemUrl);
    Task Atualizar(BannerDTO dto, string? novaImagemUrl);
    Task Ativar(Guid bannerId);
    Task Desativar(Guid bannerId);
}
```

Migration:
`dotnet ef migrations add AddBanners --project DocesCabana.Infrastructure --startup-project DocesCabana.MVC`

## 6. Ordem de execução

**Fase A — Quebra de linha**

- [ ] 1. CSS de `produto.css` e `gestao-produtos.css` (D1). Verificar à mão com descrição de 300 caracteres sem espaço, e com quebras de linha, em 375 px e 1280 px.

**Fase B — Desempenho no código**

- [ ] 2. Medição de base (D8): rodar em Development com `Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command=Information` (sem commitar) e anotar em §9 a contagem de comandos e o tempo das três rotas.
- [ ] 3. Teste `CarrinhoServiceTests` do `MontarAvulso` com `BuscarPorIds` → falha → implementar D7.
- [ ] 4. `ICacheDaAplicacao` + `CacheEmMemoria` + falso para teste + `ChavesDeCache`.
- [ ] 5. Testes de cache e invalidação em `CategoriaServiceTests`, `PromocaoServiceTests`, `ProdutoServiceTests` → falham → implementar D5.
- [ ] 6. `FreteServiceComCacheTests` → falha → `FreteServiceComCache` e registro no DI (D6).
- [ ] 7. Repetir a medição do passo 2 e anotar.

**Fase C — Banco em São Paulo (manual, sem código)**

- [ ] 8. Criar projeto Supabase em `sa-east-1`, com bucket de mesmo nome e mesma política pública.
- [ ] 9. `pg_dump` do banco antigo e `pg_restore` no novo (esquema + dados, incluindo `__EFMigrationsHistory`).
- [ ] 10. Copiar os objetos do bucket antigo para o novo.
- [ ] 11. `UPDATE` do prefixo de `Produtos.ImagemUrl` (host antigo → novo). Conferir que nenhuma linha ficou com o host antigo.
- [ ] 12. Trocar connection string e `SupabaseSettings` nos *user secrets* (nunca no repositório). Subir a aplicação e conferir home, produto com imagem, carrinho e um pedido.
- [ ] 13. Repetir a medição e anotar. Manter o projeto antigo até a verificação; só então apagar.

**Fase D — Banners do carrossel**

- [ ] 14. `BannerTests` → falha → `Banner` + `TipoDeDestinoDoBanner`.
- [ ] 15. `BannerDTOValidatorTests` → falha → DTOs + `BannerDTOValidator`.
- [ ] 16. `BannerConfiguration`, `DbSet`, `BannerRepository`; `BannerRepositoryIntegrationTests` → verde; migration `AddBanners` (aplicar no banco novo da Fase C).
- [ ] 17. `BannerServiceTests` → falha → `BannerMapper` + `BannerService` + registro no DI.
- [ ] 18. `BannerControllerTests` → falha → `BannerController`, view model, views, CSS e JS da gestão, link no menu admin.
- [ ] 19. `HomeControllerTests` → falha → `HomeController`, `_Carrossel` com reserva (D14), `carrossel.js`, `carrossel.css`.
- [ ] 20. Verificação manual: cadastrar um banner de cada tipo de destino; clicar leva à página certa; inativar o produto de destino tira o banner; sem banner vigente aparecem as 3 imagens fixas; banner com `DataFim` ontem some.
- [ ] 21. Linha da `038` em `specs/README.md` atualizada.
- [ ] 22. `dotnet build` e `dotnet test` completos.

## 7. Testes e verificação

| O que precisa ser provado | Como |
|---|---|
| Banner sem destino, com dois destinos ou com `Guid.Empty` não existe | `Dado_DestinoAusente_Quando_CriarBanner_Entao_DeveLancarArgumentException` e variantes |
| `DataFim` anterior a `DataInicio` é recusada | `Dado_DataFimAnteriorAoInicio_Quando_AlterarPeriodo_Entao_DeveLancarArgumentException` |
| Vigência respeita `DataFim` nula e `Ativo` | `Dado_DataFimNula_Quando_EstaVigente_Entao_DeveSerVerdadeiro` |
| Produto inativo tira o banner do carrossel | `Dado_ProdutoDestinoInativo_Quando_ListarVigentes_Entao_NaoDeveRetornarBanner` (integração) |
| Ordem e desempate | `Dado_BannersComMesmaOrdem_Quando_ListarVigentes_Entao_DeveDesempatarPorDataInicio` (integração) |
| Check de destino no banco | `Dado_BannerComDoisDestinos_Quando_Salvar_Entao_BancoDeveRecusar` (integração) |
| Escrita de banner invalida o cache da home | `Dado_BannerCadastrado_Quando_ListarVigentesDoCarrossel_Entao_DeveReconsultarRepositorio` |
| Categorias e promoções cacheadas e invalidadas | testes de serviço com cache falso |
| Frete: mesma chave não chama a API; falha não fica em cache | `FreteServiceComCacheTests` |
| Visitante: uma consulta para N itens | `Dado_CarrinhoAvulsoComTresItens_Quando_MontarAvulso_Entao_DeveBuscarProdutosUmaVez` |
| Imagem inválida ou falha de envio vira erro de campo | `BannerControllerTests` |
| Ganho de desempenho | medição manual dos passos 2, 7 e 13, registrada em §9 |
| Quebra de linha | verificação manual (layout não tem teste — Princípio V) |

## 8. Riscos e desvios

| Risco ou desvio constitucional | Impacto | Mitigação ou justificativa |
|---|---|---|
| Migração de região perde dado ou imagem | Alto | Projeto antigo fica de pé até o passo 12 passar; conferência de URL antiga = 0 linhas no passo 11 |
| Cache serve dado velho após escrita do admin | Médio | Toda escrita remove a chave; categorias também expiram em 5 min. Cache é por processo — com mais de uma instância, a invalidação não propaga (hoje é uma instância só) |
| Entidade cacheada (`PromocoesVigentes`) presa a `DbContext` descartado | Médio | Leituras cacheadas já usam `AsNoTracking`; `ListarVigentes` de banner nasce assim |
| Frete cacheado diverge do preço real da Melhor Envio | Baixo | 10 min; no `Fechar` do pedido a cotação também passa pelo cache — aceito, mesma janela que o cliente já vê |
| Ganho do código sozinho fica abaixo do esperado | Médio | Por isso a Fase C existe; medição separa o ganho de cada fase |
| Desvio constitucional | — | Nenhum |

## 9. Resultado da execução

*Preenchido na execução.*

| Rota | Base (comandos / ms) | Após código | Após `sa-east-1` |
|---|---|---|---|
| `GET /` | | | |
| `GET /Catalogo` | | | |
| `POST /Carrinho/AlterarQuantidade` (assíncrono) | | | |
