using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using Microsoft.Data.Sqlite;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class CarrinhoTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

    public CarrinhoTests(FixtureE2E fixture) : base(fixture) { }

    // Cada teste entra com uma conta nova, com carrinho garantidamente
    // pristino — o carrinho é tabela nova só desta feature, mas usar conta
    // dedicada evita qualquer acoplamento entre testes desta classe (RN-03
    // da 007) e permite asserções exatas (CA-08/CA-09/CA-10 dependem de
    // partir de um estado conhecido).
    private async Task<string> CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("carrinho");
        var dados = new DadosDeCadastro(
            "Cliente Carrinho E2E", email, GeradorDeDados.CelularValido(), "06061994", GeradorDeDados.CpfValido(), SenhaValida);

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, SenhaValida);

        return email;
    }

    // Guid de um produto ativo real, lido do catálogo — evita depender de
    // um identificador fixo, que mudaria se o seed for regerado.
    private async Task<Guid> ObterProdutoAtivo(int indice = 0)
    {
        var pagina = new PaginaCatalogo(Pagina);
        await pagina.Abrir(UrlBase, "doces");
        var texto = await pagina.Cards.Nth(indice).GetAttributeAsync("data-produto-id");
        return Guid.Parse(texto!);
    }

    // Sem tela administrativa para inativar/reativar produto (mesma
    // limitação que a `015` registrou para CA-10 dela), este teste altera o
    // status direto no banco de teste — uma conexão isolada e de curta
    // duração, sem transação aberta, não disputa lock com o SQLite da
    // aplicação em execução.
    private void AlterarStatusDoProduto(Guid produtoId, byte status)
    {
        using var conexao = new SqliteConnection($"Data Source={Aplicacao.CaminhoDoBanco}");
        conexao.Open();
        using var comando = conexao.CreateCommand();
        // EF Core grava o TEXT do Guid em maiúsculas no SQLite; comparação
        // sem UPPER() nos dois lados não bate com produtoId.ToString()
        // (minúsculo), e a comparação de TEXT é case-sensitive por padrão.
        comando.CommandText = "UPDATE Produto SET Status = $status WHERE UPPER(ProdutoId) = UPPER($id)";
        comando.Parameters.AddWithValue("$status", status);
        comando.Parameters.AddWithValue("$id", produtoId.ToString());
        comando.ExecuteNonQuery();
    }

    [Fact]
    public async Task Dado_CarrinhoComItens_Quando_AbrirATela_Entao_DeveMostrarNomeImagemPrecoQuantidadeELinha() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 2);

            await pagina.Abrir(UrlBase);

            var item = pagina.ItemPeloProduto(produtoId);
            await Expect(item).ToBeVisibleAsync();
            await Expect(item.Locator("img")).ToBeVisibleAsync();
            await Expect(item).ToContainTextAsync("R$");
            await Expect(item.Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");
        });

    [Fact]
    public async Task Dado_TelaDoCarrinho_Quando_AumentarQuantidade_Entao_LinhaESubtotalDevemAcompanhar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.AumentarQuantidade(produtoId);

            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");
        });

    [Fact]
    public async Task Dado_TelaDoCarrinho_Quando_RemoverItem_Entao_DeveSairEDeixarDeContar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoUm = await ObterProdutoAtivo(0);
            var produtoDois = await ObterProdutoAtivo(1);
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoUm, 1);
            await pagina.SemearItem(UrlBase, produtoDois, 1);
            await pagina.Abrir(UrlBase);

            await pagina.Remover(produtoUm);

            await Expect(pagina.ItemPeloProduto(produtoUm)).ToHaveCountAsync(0);
            await Expect(pagina.ItemPeloProduto(produtoDois)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_ItemComQuantidadeUm_Quando_Diminuir_Entao_DeveSairDoCarrinho() =>
        await Executar(async () =>
        {
            // RN-02: reduzir abaixo de 1 remove o item.
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.DiminuirQuantidade(produtoId);

            await Expect(pagina.ItemPeloProduto(produtoId)).ToHaveCountAsync(0);
            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_ItemComQuantidadeMaxima_Quando_TentarAumentar_Entao_DeveContinuarNoMaximo() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 99);
            await pagina.Abrir(UrlBase);

            await pagina.AumentarQuantidade(produtoId);

            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("99");
        });

    [Fact]
    public async Task Dado_CarrinhoVazio_Quando_Abrir_Entao_DeveOferecerCaminhoParaOCatalogo() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaCarrinho(Pagina);

            await pagina.Abrir(UrlBase);

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
            await Expect(pagina.MensagemVazia.GetByRole(Microsoft.Playwright.AriaRole.Link)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_CarrinhoMontado_Quando_SairEEntrarDeNovo_Entao_DeveEstarComoFoiDeixado() =>
        await Executar(async () =>
        {
            var email = await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 3);

            await Sair();

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, SenhaValida);

            await pagina.Abrir(UrlBase);

            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("3");
        });

    [Fact]
    public async Task Dado_ItemQueFicouIndisponivel_Quando_AbrirOCarrinho_Entao_DeveAparecerSinalizadoSemSomar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);

            // ProdutoStatus.Inativo = 0 (Domain/Enums/ProdutoStatus.cs).
            AlterarStatusDoProduto(produtoId, 0);

            try
            {
                await pagina.Abrir(UrlBase);

                var item = pagina.ItemPeloProduto(produtoId);
                await Expect(item).ToBeVisibleAsync();
                await Expect(item).ToContainTextAsync("catálogo");
            }
            finally
            {
                // O produto é compartilhado pela suíte inteira (mesma
                // instância da aplicação, RN da ColecaoE2E) — deixá-lo
                // Inativo vazaria para o ObterProdutoAtivo() de outro teste,
                // que poderia escolher um produto ForaDeEstoque/Inativo sem
                // saber e receber 400 ao semear (RN-06).
                AlterarStatusDoProduto(produtoId, 1); // Ativo de novo
            }
        });

    [Fact]
    public async Task Dado_ItensIndisponiveisPorMotivosDiferentes_Quando_Abrir_Entao_AsMensagensDevemDiferir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoInativo = await ObterProdutoAtivo(0);
            var produtoForaDeEstoque = await ObterProdutoAtivo(1);
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoInativo, 1);
            await pagina.SemearItem(UrlBase, produtoForaDeEstoque, 1);

            AlterarStatusDoProduto(produtoInativo, 0);       // Inativo
            AlterarStatusDoProduto(produtoForaDeEstoque, 2); // ForaDeEstoque

            try
            {
                await pagina.Abrir(UrlBase);

                var textoInativo = await pagina.ItemPeloProduto(produtoInativo).InnerTextAsync();
                var textoForaDeEstoque = await pagina.ItemPeloProduto(produtoForaDeEstoque).InnerTextAsync();

                Assert.NotEqual(textoInativo, textoForaDeEstoque);
            }
            finally
            {
                // Mesmo motivo do teste acima: sem restaurar, os dois
                // produtos ficam permanentemente fora do "disponível para
                // compra" e vazam para o restante da suíte.
                AlterarStatusDoProduto(produtoInativo, 1);
                AlterarStatusDoProduto(produtoForaDeEstoque, 1);
            }
        });

    [Fact]
    public async Task Dado_ItemIndisponivel_Quando_OProdutoVoltar_Entao_DeveVoltarASomarSozinho() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);

            AlterarStatusDoProduto(produtoId, 0); // Inativo
            await pagina.Abrir(UrlBase);
            var subtotalIndisponivel = await pagina.Subtotal.InnerTextAsync();

            AlterarStatusDoProduto(produtoId, 1); // Ativo de novo
            await pagina.Abrir(UrlBase);
            var subtotalDisponivel = await pagina.Subtotal.InnerTextAsync();

            Assert.NotEqual(subtotalIndisponivel, subtotalDisponivel);
        });

    [Fact]
    public async Task Dado_NaoAutenticado_Quando_AcrescentarAoCarrinho_Entao_DeveVerEAlterar() =>
        await Executar(async () =>
        {
            // CA-12: sem login — o carrinho avulso vive na sessão, não na
            // conta. Nenhum CriarClienteEEntrar() aqui de propósito.
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);

            await pagina.Abrir(UrlBase);

            var item = pagina.ItemPeloProduto(produtoId);
            await Expect(item).ToBeVisibleAsync();
            await Expect(item.Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("1");

            await pagina.AumentarQuantidade(produtoId);

            await Expect(item.Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");
        });

    // CA-13 e CA-14 são os testes mais frágeis da feature (plano §7):
    // sessão, autenticação e uma transferência de estado entre dois
    // armazenamentos, tudo na mesma jornada.
    [Fact]
    public async Task Dado_CarrinhosNosDoisLados_Quando_Entrar_Entao_AsQuantidadesDevemSomar() =>
        await Executar(async () =>
        {
            var email = await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 3); // guardado, autenticado

            await Sair();
            // Logout cai em /Autenticacao/Login, que usa _LayoutNaoAutenticado
            // (sem @Html.AntiForgeryToken()) — SemearItem precisa de uma
            // página com o token presente antes de rodar o fetch.
            await Pagina.GotoAsync(UrlBase);

            await pagina.SemearItem(UrlBase, produtoId, 2); // avulso, visitante

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, SenhaValida);

            await pagina.Abrir(UrlBase);

            // RN-05: 3 (guardado) + 2 (avulso) = 5.
            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("5");
        });

    [Fact]
    public async Task Dado_FusaoConcluida_Quando_VoltarComoVisitante_Entao_OCarrinhoAvulsoDeveEstarVazio() =>
        await Executar(async () =>
        {
            var email = await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);

            await Sair();
            await Pagina.GotoAsync(UrlBase); // mesmo motivo do teste acima
            await pagina.SemearItem(UrlBase, produtoId, 1); // avulso, visitante

            var paginaLogin = new PaginaLogin(Pagina);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(email, SenhaValida);
            await pagina.Abrir(UrlBase); // dispara o filtro de fusão, que limpa a sessão

            await Sair();
            await pagina.Abrir(UrlBase);

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    // ── Fase 8 — os controles voltam a ligar ao carrinho de verdade ────

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_AcrescentarDoCartao_Entao_DeveEntrarNoCarrinho() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var paginaCatalogo = new PaginaCatalogo(Pagina);
            await paginaCatalogo.Abrir(UrlBase, "doces");
            var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);

            await paginaCatalogo.Cards.First.Locator(".botao-adicionar-card").ClickAsync();
            await Pagina.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

            var pagina = new PaginaCarrinho(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_PaginaDoProduto_Quando_AcrescentarComQuantidadeTres_Entao_DeveEntrarComTres() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();

            await Pagina.GotoAsync($"{UrlBase}/Produto/Detalhes/{produtoId}");
            await Pagina.Locator("[data-quantidade-mais]").ClickAsync();
            await Pagina.Locator("[data-quantidade-mais]").ClickAsync();
            await Expect(Pagina.Locator("[data-quantidade-valor]")).ToHaveValueAsync("3");

            await Pagina.Locator(".botao-adicionar-carrinho").ClickAsync();
            await Pagina.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

            var pagina = new PaginaCarrinho(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("3");
        });

    [Fact]
    public async Task Dado_ItensNoCarrinho_Quando_OlharOCabecalho_Entao_DeveIndicarAQuantidade() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 2);

            await Pagina.GotoAsync(UrlBase);

            var bolha = Pagina.Locator("[data-bolha-carrinho]");
            await Expect(bolha).ToBeVisibleAsync();
            await Expect(bolha).ToHaveTextAsync("2");
        });

    [Fact]
    public async Task Dado_QualquerPagina_Quando_AcionarOAtalhoDeCarrinho_Entao_DeveChegarNaTela() =>
        await Executar(async () =>
        {
            await Pagina.GotoAsync(UrlBase);

            await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Meu carrinho" }).ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Carrinho");
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_AcrescentarAlterarERemover_Entao_OsTresDevemFuncionar() =>
        await Executar(async () =>
        {
            // RF-01/RF-18: sem script, cada um dos três continua um POST
            // comum, resolvido por POST-Redirect-Get (plano §3). Usa o
            // cliente do seed em vez de cadastrar (mesmo motivo de
            // FavoritosTests): o cadastro depende de máscaras em JavaScript
            // (autenticacao.js) para formatar celular/CPF/data — sem script,
            // o formulário nem chegaria a validar.
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();

            var paginaLogin = new PaginaLogin(paginaSemScript);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var paginaCatalogo = new PaginaCatalogo(paginaSemScript);
            await paginaCatalogo.Abrir(UrlBase, "doces");
            var produtoId = Guid.Parse((await paginaCatalogo.Cards.First.GetAttributeAsync("data-produto-id"))!);

            // Acrescentar: sem script, o botão soma uma unidade (plano §3).
            await paginaCatalogo.Cards.First.Locator(".botao-adicionar-card").ClickAsync();
            await Expect(paginaSemScript).ToHaveURLAsync($"{UrlBase}/Carrinho");

            var pagina = new PaginaCarrinho(paginaSemScript);
            var item = pagina.ItemPeloProduto(produtoId);
            await Expect(item.Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("1");

            // Alterar: o botão "mais" já carrega o valor final como
            // name/value (_ItensDoCarrinho.cshtml) — funciona sem script.
            await item.Locator(".botao-quantidade-carrinho.mais").ClickAsync();
            await Expect(item.Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");

            // Remover.
            await item.Locator(".botao-remover-carrinho").ClickAsync();
            await Expect(pagina.ItemPeloProduto(produtoId)).ToHaveCountAsync(0);
            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_TelaDoCarrinho_Quando_AlterarQuantidade_Entao_NaoDeveRecarregarAPagina() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            // Marca um elemento fora do bloco que a atualização substitui —
            // se a página recarregar, ele desaparece e a leitura seguinte
            // falha (mesmo padrão de FavoritosTests).
            await Pagina.EvaluateAsync("() => { document.body.dataset.marcadorDeRecarga = 'presente'; }");

            await pagina.AumentarQuantidade(produtoId);

            var marcador = await Pagina.EvaluateAsync<string?>("() => document.body.dataset.marcadorDeRecarga");
            Assert.Equal("presente", marcador);
            await Expect(pagina.ItemPeloProduto(produtoId).Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");
        });

    // ── Redesenho do carrinho (spec 021) ─────────────────────────────────

    [Fact]
    public async Task Dado_ItemDisponivel_Quando_AbrirOCarrinho_Entao_OCartaoDeveTerImagemNomePrecoQuantidadeESubtotal() =>
        await Executar(async () =>
        {
            // CA-01
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 2);
            await pagina.Abrir(UrlBase);

            var item = pagina.ItemPeloProduto(produtoId);
            await Expect(item.Locator("img")).ToBeVisibleAsync();
            await Expect(item.Locator(".nome-item-carrinho")).ToBeVisibleAsync();
            await Expect(pagina.RotuloColunaPreco(produtoId)).ToContainTextAsync("Preço unitário");
            await Expect(item.Locator(".valor-quantidade-carrinho")).ToHaveTextAsync("2");
            await Expect(pagina.RotuloColunaSubtotal(produtoId)).ToContainTextAsync("Subtotal");
        });

    [Fact]
    public async Task Dado_TelaDoCarrinho_Quando_OlharOCupom_Entao_DeveEstarDesabilitadoEExplicado() =>
        await Executar(async () =>
        {
            // CA-06
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.CampoCupom).ToBeDisabledAsync();
            await Expect(pagina.BotaoAplicarCupom).ToBeDisabledAsync();
            await Expect(Pagina.Locator(".explicacao-cupom-carrinho")).ToContainTextAsync("ainda não está disponível");
        });

    [Fact]
    public async Task Dado_TelaDoCarrinho_Quando_OlharOBotaoDeFinalizar_Entao_DeveLevarAoFechamento() =>
        await Executar(async () =>
        {
            // CA-07 da spec 021 previa o botão desabilitado, "fechamento
            // ainda não disponível" — a spec 022 implementou o fechamento,
            // e o botão agora navega para o primeiro passo dele (RF-01).
            // Reescrito aqui pela mesma razão que a 019 reescreveu os
            // testes que a 022 sabia que ia derrubar: correção esperada,
            // não regressão.
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.BotaoFinalizar).ToBeEnabledAsync();
            await Expect(pagina.BotaoFinalizar).ToHaveAttributeAsync("href", "/Carrinho?passo=Endereco");

            await pagina.BotaoFinalizar.ClickAsync();

            // Com JavaScript, o clique troca só #itens-carrinho — a URL não
            // muda (RF-05); o passo ativo no indicador é a prova de que
            // navegou de verdade.
            await Expect(Pagina.Locator(".passo-fechamento--ativo")).ToHaveTextAsync("Endereço");
        });

    [Fact]
    public async Task Dado_NenhumaEntregaCalculada_Quando_OlharOResumo_Entao_ODestaqueDeveSerSubtotal() =>
        await Executar(async () =>
        {
            // CA-04
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.Subtotal).ToContainTextAsync("Subtotal");
            await Expect(Pagina.Locator(".linha-frete-carrinho")).ToContainTextAsync("Calcule o frete");
        });

    [Fact]
    public async Task Dado_ItemNoCarrinho_Quando_PedirParaEsvaziar_Entao_DevePerguntarAntesDeRemoverNada() =>
        await Executar(async () =>
        {
            // CA-08
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.LinkEsvaziar.ClickAsync();

            await Expect(pagina.DialogoEsvaziar).ToBeVisibleAsync();
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_PerguntaDeEsvaziarAberta_Quando_Confirmar_Entao_DeveRemoverTudoEOferecerOCatalogo() =>
        await Executar(async () =>
        {
            // CA-09
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.LinkEsvaziar.ClickAsync();
            await pagina.BotaoConfirmarEsvaziarNoDialogo.ClickAsync();

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
            await Expect(pagina.MensagemVazia.GetByRole(Microsoft.Playwright.AriaRole.Link)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_PerguntaDeEsvaziarAberta_Quando_Desistir_Entao_NadaDeveSerRemovido() =>
        await Executar(async () =>
        {
            // CA-10
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.LinkEsvaziar.ClickAsync();
            await pagina.BotaoCancelarEsvaziarNoDialogo.ClickAsync();

            await Expect(pagina.DialogoEsvaziar).Not.ToBeVisibleAsync();
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_ItemNoCarrinho_Quando_VoltarAoCatalogoEAoCarrinho_Entao_OItemDevePermanecer() =>
        await Executar(async () =>
        {
            // CA-11
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            await pagina.LinkContinuarComprando.ClickAsync();
            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Catalogo");

            await pagina.Abrir(UrlBase);
            await Expect(pagina.ItemPeloProduto(produtoId)).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_Esvaziar_Entao_DevePerguntarPorPaginaPropriaEFuncionar() =>
        await Executar(async () =>
        {
            // CA-12 (a parte que T022 acrescenta a T003 já não cobre: o
            // esvaziar em si). Mesmo cliente do seed que os demais testes
            // sem script usam, pelo mesmo motivo (cadastro depende de
            // máscara em JavaScript).
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();

            var paginaLogin = new PaginaLogin(paginaSemScript);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaCarrinho(paginaSemScript);
            var produtoId = await ObterProdutoAtivo();
            await pagina.SemearItem(UrlBase, produtoId, 1);
            await pagina.Abrir(UrlBase);

            // Sem script, o link navega para a página própria da RN-04 —
            // não abre diálogo nenhum, porque <dialog>.showModal() nunca é
            // chamado.
            await pagina.LinkEsvaziar.ClickAsync();
            await Expect(paginaSemScript).ToHaveURLAsync($"{UrlBase}/Carrinho/ConfirmarEsvaziar");

            var paginaConfirmar = new PaginaConfirmarEsvaziarCarrinho(paginaSemScript);
            await paginaConfirmar.BotaoConfirmar.ClickAsync();

            await Expect(paginaSemScript).ToHaveURLAsync($"{UrlBase}/Carrinho");
            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_TelaDe375px_Quando_AbrirOCarrinho_Entao_OResumoDeveEmpilharSemRolagemHorizontal() =>
        await Executar(async () =>
        {
            // CA-13
            await CriarClienteEEntrar();
            var produtoId = await ObterProdutoAtivo();
            var pagina = new PaginaCarrinho(Pagina);
            await pagina.SemearItem(UrlBase, produtoId, 1);

            await Pagina.SetViewportSizeAsync(375, 800);
            await pagina.Abrir(UrlBase);

            // Mede o conteúdo, não o documento — o cabeçalho compartilhado já
            // estoura a 375px por conta própria desde a 009 (fora de escopo,
            // mesmo critério que a 013 e a 020 já registraram).
            var larguraDoConteudo = await Pagina.Locator(".pagina-carrinho").EvaluateAsync<double>("el => el.scrollWidth");
            var larguraDaTela = await Pagina.EvaluateAsync<double>("() => window.innerWidth");

            Assert.True(larguraDoConteudo <= larguraDaTela + 1);
        });

    private async Task Sair() =>
        await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Sair" }).ClickAsync();
}
