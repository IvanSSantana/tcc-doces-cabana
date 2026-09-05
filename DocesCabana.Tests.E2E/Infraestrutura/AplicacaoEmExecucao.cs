using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace DocesCabana.Tests.E2E.Infraestrutura;

/// <summary>
/// Sobe a aplicação MVC de verdade num processo filho, apontada para um
/// SQLite descartável e para o adaptador de e-mail em arquivo — sem tocar a
/// base usada no dia a dia do desenvolvimento (RF-04, RN-04, RN-05 da 007).
/// Uma instância é compartilhada pela suíte inteira, via <see cref="ColecaoE2E"/>.
/// </summary>
public sealed class AplicacaoEmExecucao : IAsyncDisposable
{
    public const string EmailAdministrador = "admin@docescabana.com.br";
    public const string CpfAdministrador = "52998224725";
    public const string SenhaAdministrador = "SenhaE2E@2026";

    // Elenco de clientes fictícios do seed (DbInitializer, spec 014) — usado
    // pelos testes que precisam de um cliente autenticado sem criar conta
    // nova a cada execução.
    public const string EmailClienteSeed = "cliente1.seed@docescabana.com.br";
    public const string SenhaClienteSeed = "SenhaSeed@123";

    private const int TimeoutDeSubidaSegundos = 60;

    private readonly string _pastaTemporaria;
    private Process? _processo;
    private readonly StringBuilder _saidaPadrao = new();
    private readonly StringBuilder _saidaDeErro = new();

    public string UrlBase { get; }
    public string PastaDeEmails { get; }

    // Exposto para o teste que não tem tela administrativa para exercitar
    // (spec 017, plano §7 — mudar o status de um produto para testar o item
    // do carrinho que fica indisponível): uma conexão isolada e de curta
    // duração, sem transação aberta, não disputa lock com o SQLite da
    // aplicação em execução.
    public string CaminhoDoBanco { get; }

    private AplicacaoEmExecucao(string urlBase, string pastaTemporaria, string pastaDeEmails, string caminhoDoBanco)
    {
        UrlBase = urlBase;
        _pastaTemporaria = pastaTemporaria;
        PastaDeEmails = pastaDeEmails;
        CaminhoDoBanco = caminhoDoBanco;
    }

    public static async Task<AplicacaoEmExecucao> Subir()
    {
        var porta = ObterPortaLivre();
        var pastaTemporaria = Path.Combine(Path.GetTempPath(), $"doces-cabana-e2e-{Guid.NewGuid():N}");
        var pastaDeEmails = Path.Combine(pastaTemporaria, "emails");
        Directory.CreateDirectory(pastaDeEmails);

        var caminhoDoBanco = Path.Combine(pastaTemporaria, "e2e.db");
        var caminhoDaDll = LocalizarDllDaMvc();
        var pastaDaMvc = Path.GetDirectoryName(LocalizarProjetoDaMvc())!;

        var aplicacao = new AplicacaoEmExecucao($"http://127.0.0.1:{porta}", pastaTemporaria, pastaDeEmails, caminhoDoBanco);

        var infoProcesso = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{caminhoDaDll}\"",
            WorkingDirectory = pastaDaMvc,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        infoProcesso.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        infoProcesso.Environment["ASPNETCORE_URLS"] = aplicacao.UrlBase;
        infoProcesso.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={caminhoDoBanco}";
        infoProcesso.Environment["Admin__SenhaInicial"] = SenhaAdministrador;
        infoProcesso.Environment["EmailSettings__Adaptador"] = "Arquivo";
        infoProcesso.Environment["EmailSettings__PastaDeSaida"] = pastaDeEmails;

        // Frete (spec 020) — sem simulador, IFreteService só tem a
        // implementação real. Sem a credencial no ambiente de quem executa,
        // aponta para um endereço que recusa conexão: a cotação falha de
        // forma determinística, sem depender de rede nem do MelhorEnvio
        // estar no ar — os testes sem [Trait("Categoria", "Externo")]
        // continuam verdes em qualquer máquina. Com a credencial presente
        // (FreteSettings__Token no ambiente de fora), repassa a URL real e o
        // token, para os testes marcados como externos (Fase 8, T048/T049).
        var tokenDoAmbiente = Environment.GetEnvironmentVariable("FreteSettings__Token");
        if (!string.IsNullOrWhiteSpace(tokenDoAmbiente))
        {
            infoProcesso.Environment["FreteSettings__UrlBase"] = "https://sandbox.melhorenvio.com.br";
            infoProcesso.Environment["FreteSettings__Token"] = tokenDoAmbiente;
            infoProcesso.Environment["FreteSettings__UserAgent"] =
                Environment.GetEnvironmentVariable("FreteSettings__UserAgent") ?? "Doces Cabana (testes-e2e@docescabana.com.br)";
        }
        else
        {
            infoProcesso.Environment["FreteSettings__UrlBase"] = "http://localhost:9";
        }

        // Armazenamento de imagem (spec 027) — mesmo mecanismo do frete.
        // Sem SupabaseSettings__ChaveDeServico no ambiente de quem executa,
        // sobe sem credencial de propósito: o adaptador recusa sem tocar a
        // rede (RN-03), e os testes sem [Trait("Categoria", "Externo")]
        // continuam determinísticos em qualquer máquina. Com a credencial
        // presente, repassa também UrlBase/Bucket/Pasta se o ambiente os
        // definir, para os testes marcados como externos (Fase 8).
        var chaveDeServicoDoAmbiente = Environment.GetEnvironmentVariable("SupabaseSettings__ChaveDeServico");
        if (!string.IsNullOrWhiteSpace(chaveDeServicoDoAmbiente))
        {
            infoProcesso.Environment["SupabaseSettings__ChaveDeServico"] = chaveDeServicoDoAmbiente;
            infoProcesso.Environment["SupabaseSettings__UrlBase"] =
                Environment.GetEnvironmentVariable("SupabaseSettings__UrlBase") ?? "https://mjnlzsucdsxqahabsniy.supabase.co";
            infoProcesso.Environment["SupabaseSettings__Bucket"] =
                Environment.GetEnvironmentVariable("SupabaseSettings__Bucket") ?? "images";
            infoProcesso.Environment["SupabaseSettings__Pasta"] =
                Environment.GetEnvironmentVariable("SupabaseSettings__Pasta") ?? "public";
        }

        var processo = new Process { StartInfo = infoProcesso, EnableRaisingEvents = true };
        processo.OutputDataReceived += (_, e) => { if (e.Data is not null) aplicacao._saidaPadrao.AppendLine(e.Data); };
        processo.ErrorDataReceived += (_, e) => { if (e.Data is not null) aplicacao._saidaDeErro.AppendLine(e.Data); };

        processo.Start();
        processo.BeginOutputReadLine();
        processo.BeginErrorReadLine();
        aplicacao._processo = processo;

        await aplicacao.EsperarFicarPronta(processo);

        return aplicacao;
    }

    private async Task EsperarFicarPronta(Process processo)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var prazo = DateTime.UtcNow.AddSeconds(TimeoutDeSubidaSegundos);

        while (DateTime.UtcNow < prazo)
        {
            if (processo.HasExited)
                throw new InvalidOperationException(MontarMensagemDeFalha(
                    $"O processo da aplicação encerrou sozinho (código {processo.ExitCode}) antes de responder."));

            try
            {
                var resposta = await http.GetAsync(UrlBase);
                if (resposta.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException) { /* ainda subindo — tenta de novo */ }
            catch (TaskCanceledException) { /* timeout da requisição individual — tenta de novo */ }

            await Task.Delay(200);
        }

        throw new InvalidOperationException(MontarMensagemDeFalha(
            $"A aplicação não respondeu em {UrlBase}/ dentro de {TimeoutDeSubidaSegundos}s."));
    }

    private string MontarMensagemDeFalha(string motivo) =>
        $"""
        {motivo}

        --- stdout ---
        {_saidaPadrao}
        --- stderr ---
        {_saidaDeErro}
        """;

    private static int ObterPortaLivre()
    {
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var porta = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return porta;
    }

    private static string LocalizarDllDaMvc()
    {
        var caminhoDoProjeto = LocalizarProjetoDaMvc();
        var pastaDoProjeto = Path.GetDirectoryName(caminhoDoProjeto)!;
        var configuracao = ObterConfiguracaoDeBuild();

        var caminhoDaDll = Path.Combine(pastaDoProjeto, "bin", configuracao, "net10.0", "DocesCabana.MVC.dll");

        if (!File.Exists(caminhoDaDll))
            throw new FileNotFoundException(
                $"DocesCabana.MVC.dll não encontrada em '{caminhoDaDll}'. Rode 'dotnet build' na solução antes do E2E — a referência de projeto garante a ordem, não que o build tenha acontecido numa configuração diferente.",
                caminhoDaDll);

        return caminhoDaDll;
    }

    private static string LocalizarProjetoDaMvc()
    {
        var raiz = LocalizarRaizDoRepositorio();
        return Path.Combine(raiz, "DocesCabana.MVC", "DocesCabana.MVC.csproj");
    }

    private static string LocalizarRaizDoRepositorio()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);

        while (pasta is not null && !pasta.GetFiles("tcc-doces-cabana.sln").Any())
            pasta = pasta.Parent;

        if (pasta is null)
            throw new InvalidOperationException(
                $"Não encontrei 'tcc-doces-cabana.sln' subindo a partir de '{AppContext.BaseDirectory}'.");

        return pasta.FullName;
    }

    // O executável de teste roda em bin/<Configuração>/net10.0/; a mesma
    // configuração é usada para achar a dll da MVC, para não depender de uma
    // combinação Debug/Release que ninguém buildou.
    private static string ObterConfiguracaoDeBuild()
    {
        var segmentos = AppContext.BaseDirectory
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(s => s.Length > 0)
            .ToArray();

        var indiceBin = Array.LastIndexOf(segmentos, "bin");
        if (indiceBin >= 0 && indiceBin + 1 < segmentos.Length)
            return segmentos[indiceBin + 1];

        return "Debug";
    }

    public async ValueTask DisposeAsync()
    {
        if (_processo is { HasExited: false })
        {
            try
            {
                _processo.Kill(entireProcessTree: true);
                await _processo.WaitForExitAsync();
            }
            catch (InvalidOperationException) { /* já tinha encerrado */ }
        }

        _processo?.Dispose();

        try
        {
            if (Directory.Exists(_pastaTemporaria))
                Directory.Delete(_pastaTemporaria, recursive: true);
        }
        catch (IOException) { /* melhor esforço — não derruba a suíte por isso */ }
    }
}
