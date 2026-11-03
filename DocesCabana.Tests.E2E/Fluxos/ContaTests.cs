using DocesCabana.Tests.E2E.Infraestrutura;
using DocesCabana.Tests.E2E.Paginas;
using static Microsoft.Playwright.Assertions;

namespace DocesCabana.Tests.E2E.Fluxos;

public class ContaTests : TesteE2E
{
    private const string SenhaValida = "SenhaForte@2026";

    public ContaTests(FixtureE2E fixture) : base(fixture) { }

    private async Task<(string Email, string Nome, string Celular, string Cpf)> CriarClienteEEntrar()
    {
        var email = GeradorDeDados.EmailUnico("conta");
        var nome = "Cliente Conta E2E";
        var celular = GeradorDeDados.CelularValido();
        var cpf = GeradorDeDados.CpfValido();
        var dados = new DadosDeCadastro(nome, email, celular, "06061994", cpf, SenhaValida);

        var paginaCadastro = new PaginaCadastro(Pagina);
        await paginaCadastro.Abrir(UrlBase);
        await paginaCadastro.Preencher(dados);
        await paginaCadastro.Enviar();
        await Pagina.WaitForURLAsync($"{UrlBase}/Autenticacao/Login");

        var paginaLogin = new PaginaLogin(Pagina);
        await paginaLogin.Entrar(email, SenhaValida);

        return (email, nome, celular, cpf);
    }

    private static DadosDeEndereco EnderecoValido(string rua = "Rua das Flores") =>
        new("17340-000", "São Paulo", "Barra Bonita", "Centro", rua, "123");

    [Fact]
    public async Task Dado_AreaDeConta_Quando_OlharATela_Entao_DeveReunirDadosPessoaisEEnderecos() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(pagina.MenuDados).ToBeVisibleAsync();
            await Expect(pagina.MenuEnderecos).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_Visitante_Quando_TentarAbrirAConta_Entao_DeveSerLevadoAEntrar() =>
        await Executar(async () =>
        {
            var pagina = new PaginaConta(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(Pagina).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Autenticacao/Login"));
        });

    [Fact]
    public async Task Dado_ContaRecemCriada_Quando_AbrirOsDadosPessoais_Entao_DevemVirPreenchidos() =>
        await Executar(async () =>
        {
            var (_, nome, celular, cpf) = await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);

            await pagina.Abrir(UrlBase);

            await Expect(pagina.CampoNome).ToHaveValueAsync(nome);
            await Expect(pagina.CpfSomenteLeitura).ToContainTextAsync(cpf[..3]);
            await Expect(pagina.CampoCelular).Not.ToBeEmptyAsync();
        });

    [Fact]
    public async Task Dado_DadosPessoais_Quando_CorrigirOCelular_Entao_DevePersistir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.Abrir(UrlBase);

            await pagina.CampoCelular.FillAsync("(14) 97777-6666");
            await pagina.BotaoSalvarDados.ClickAsync();

            await pagina.Abrir(UrlBase);
            await Expect(pagina.CampoCelular).ToHaveValueAsync("14977776666");
        });

    [Fact]
    public async Task Dado_DadosPessoais_Quando_TentarAlterarOCpf_Entao_NaoDeveConseguir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.Abrir(UrlBase);

            await Expect(Pagina.Locator("input[name='CPF']")).ToHaveCountAsync(0);
        });

    [Fact]
    public async Task Dado_CelularInvalido_Quando_Salvar_Entao_DeveVoltarComMensagemNoCampoEOsDemaisPreenchidos() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.Abrir(UrlBase);

            await pagina.CampoNome.FillAsync("Nome Ainda Digitado");
            await pagina.CampoCelular.FillAsync("123");
            await pagina.BotaoSalvarDados.ClickAsync();

            await Expect(pagina.ErroDoCelular).ToBeVisibleAsync();
            await Expect(pagina.CampoNome).ToHaveValueAsync("Nome Ainda Digitado");
        });

    [Fact]
    public async Task Dado_NenhumEndereco_Quando_CadastrarOPrimeiro_Entao_DeveNascerPrincipal() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);

            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await Expect(pagina.CartaoPrincipal).ToBeVisibleAsync();
            await Expect(pagina.Cartoes).ToHaveCountAsync(1);
        });

    [Fact]
    public async Task Dado_NenhumEndereco_Quando_AbrirASecao_Entao_DeveConvidarACadastrar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);

            await pagina.AbrirEnderecos(UrlBase);

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
            await Expect(pagina.BotaoNovoEndereco).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_UmEnderecoPrincipal_Quando_CadastrarOSegundo_Entao_OPrimeiroDeveContinuarPrincipal() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Um"));
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Dois"));
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await Expect(pagina.CartaoPrincipal).ToContainTextAsync("Rua Um");
            await Expect(pagina.Cartoes).ToHaveCountAsync(2);
        });

    [Fact]
    public async Task Dado_DoisEnderecos_Quando_MarcarOSegundo_Entao_ElePassaASerPrincipal() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Um"));
            await pagina.BotaoSalvarEndereco.ClickAsync();
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Dois"));
            await pagina.BotaoSalvarEndereco.ClickAsync();

            var segundo = pagina.Cartoes.Filter(new() { HasText = "Rua Dois" });
            await pagina.BotaoTornarPrincipal(segundo).ClickAsync();

            await Expect(pagina.CartaoPrincipal).ToContainTextAsync("Rua Dois");
        });

    [Fact]
    public async Task Dado_EnderecosCadastrados_Quando_OlharALista_Entao_DeveIndicarOPrincipal() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await Expect(pagina.CartaoPrincipal).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_EnderecoCadastrado_Quando_EditarONumero_Entao_DevePersistir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await pagina.BotaoEditar(pagina.Cartoes.First).ClickAsync();
            await pagina.CampoNumero.FillAsync("456");
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await Expect(pagina.Cartoes.First).ToContainTextAsync("456");
        });

    [Fact]
    public async Task Dado_DoisEnderecosEOPrincipalEOPrimeiro_Quando_ExcluirOSegundo_Entao_ElePrincipalDeveContinuar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Um"));
            await pagina.BotaoSalvarEndereco.ClickAsync();
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Dois"));
            await pagina.BotaoSalvarEndereco.ClickAsync();

            var segundo = pagina.Cartoes.Filter(new() { HasText = "Rua Dois" });
            await pagina.BotaoExcluir(segundo).ClickAsync();

            await Expect(pagina.Cartoes).ToHaveCountAsync(1);
            await Expect(pagina.CartaoPrincipal).ToContainTextAsync("Rua Um");
        });

    [Fact]
    public async Task Dado_DoisEnderecos_Quando_ExcluirOPrincipal_Entao_ORestanteDeveAssumir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Um"));
            await pagina.BotaoSalvarEndereco.ClickAsync();
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido("Rua Dois"));
            await pagina.BotaoSalvarEndereco.ClickAsync();

            var principal = pagina.Cartoes.Filter(new() { HasText = "Rua Um" });
            await pagina.BotaoExcluir(principal).ClickAsync();

            await Expect(pagina.Cartoes).ToHaveCountAsync(1);
            await Expect(pagina.CartaoPrincipal).ToContainTextAsync("Rua Dois");
        });

    [Fact]
    public async Task Dado_UnicoEndereco_Quando_Excluir_Entao_AListaDeveFicarVaziaEConvidar() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await pagina.BotaoExcluir(pagina.Cartoes.First).ClickAsync();

            await Expect(pagina.MensagemVazia).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_EnderecoDeOutraPessoa_Quando_TentarAbrirEditarOuExcluir_Entao_NaoDeveConseguir() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await pagina.BotaoEditar(pagina.Cartoes.First).ClickAsync();
            var urlDeEdicaoAlheia = Pagina.Url;

            await Sair();
            await CriarClienteEEntrar();

            await Pagina.GotoAsync(urlDeEdicaoAlheia);

            await Expect(Pagina.GetByRole(Microsoft.Playwright.AriaRole.Heading, new() { Name = "Não encontrado" })).ToBeVisibleAsync();
            await Expect(Pagina.Locator("input[name='Rua']")).ToHaveCountAsync(0);
        });

    [Fact]
    public async Task Dado_ClienteAutenticado_Quando_AcionarOAtalhoConta_Entao_DeveChegarNaAreaDeConta() =>
        await Executar(async () =>
        {
            await CriarClienteEEntrar();
            await Pagina.GotoAsync(UrlBase);

            await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Link, new() { Name = "Conta" }).ClickAsync();

            await Expect(Pagina).ToHaveURLAsync($"{UrlBase}/Conta");
            var pagina = new PaginaConta(Pagina);
            await Expect(pagina.MenuDados).ToBeVisibleAsync();
        });

    [Fact]
    public async Task Dado_FormularioDeEndereco_Quando_InformarCepValido_Entao_DevePreencherOsDemaisCampos() =>
        await Executar(async () =>
        {
            await Pagina.RouteAsync("**/viacep.com.br/ws/**", async rota =>
                await rota.FulfillAsync(new()
                {
                    ContentType = "application/json",
                    Body = """{"cep":"17340-000","logradouro":"Rua das Flores","bairro":"Centro","localidade":"Barra Bonita","uf":"SP"}""",
                }));

            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);

            await pagina.CampoCep.FillAsync("17340-000");
            await pagina.CampoCep.PressAsync("Tab");

            await Expect(pagina.CampoEstado).ToHaveValueAsync("SP");
            await Expect(pagina.CampoCidade).ToHaveValueAsync("Barra Bonita");
            await Expect(pagina.CampoBairro).ToHaveValueAsync("Centro");
            await Expect(pagina.CampoRua).ToHaveValueAsync("Rua das Flores");
        });

    [Fact]
    public async Task Dado_CamposPreenchidosPeloCep_Quando_AlterarARua_Entao_ODigitadoDevePrevalecer() =>
        await Executar(async () =>
        {
            await Pagina.RouteAsync("**/viacep.com.br/ws/**", async rota =>
                await rota.FulfillAsync(new()
                {
                    ContentType = "application/json",
                    Body = """{"cep":"17340-000","logradouro":"Rua das Flores","bairro":"Centro","localidade":"Barra Bonita","uf":"SP"}""",
                }));

            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.CampoCep.FillAsync("17340-000");
            await pagina.CampoCep.PressAsync("Tab");
            await Expect(pagina.CampoRua).ToHaveValueAsync("Rua das Flores");

            await pagina.CampoRua.FillAsync("Rua Digitada Por Mim");

            await Expect(pagina.CampoRua).ToHaveValueAsync("Rua Digitada Por Mim");
        });

    [Fact]
    public async Task Dado_BuscaDeCepIndisponivel_Quando_PreencherAMaoESalvar_Entao_DeveCadastrarNormalmente() =>
        await Executar(async () =>
        {
            await Pagina.RouteAsync("**/viacep.com.br/ws/**", async rota => await rota.AbortAsync());

            await CriarClienteEEntrar();
            var pagina = new PaginaConta(Pagina);
            await pagina.AbrirNovoEndereco(UrlBase);

            await pagina.CampoCep.FillAsync("17340-000");
            await pagina.CampoCep.PressAsync("Tab");

            await pagina.PreencherEndereco(EnderecoValido());
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await Expect(pagina.Cartoes).ToHaveCountAsync(1);
        });

    [Fact]
    public async Task Dado_JavaScriptDesligado_Quando_CadastrarEndereco_Entao_DeveFuncionar() =>
        await Executar(async () =>
        {
            await using var contextoSemScript = await Navegador.NewContextAsync(new() { JavaScriptEnabled = false });
            var paginaSemScript = await contextoSemScript.NewPageAsync();

            var paginaLogin = new PaginaLogin(paginaSemScript);
            await paginaLogin.Abrir(UrlBase);
            await paginaLogin.Entrar(AplicacaoEmExecucao.EmailClienteSeed, AplicacaoEmExecucao.SenhaClienteSeed);

            var pagina = new PaginaConta(paginaSemScript);
            await pagina.AbrirNovoEndereco(UrlBase);
            await pagina.PreencherEndereco(new DadosDeEndereco("17340-000", "São Paulo", "Barra Bonita", "Centro", "Rua Sem Script", "789"));
            await pagina.BotaoSalvarEndereco.ClickAsync();

            await Expect(pagina.Cartoes.Filter(new() { HasText = "Rua Sem Script" })).ToBeVisibleAsync();

            await pagina.BotaoExcluir(pagina.Cartoes.Filter(new() { HasText = "Rua Sem Script" })).ClickAsync();
        });

    private async Task Sair() =>
        await Pagina.Locator("header").GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Sair" }).ClickAsync();
}
