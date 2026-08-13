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

    private const int TimeoutDeSubidaSegundos = 60;

    private readonly string _pastaTemporaria;
    private Process? _processo;
    private readonly StringBuilder _saidaPadrao = new();
    private readonly StringBuilder _saidaDeErro = new();

    public string UrlBase { get; }
    public string PastaDeEmails { get; }

    private AplicacaoEmExecucao(string urlBase, string pastaTemporaria, string pastaDeEmails)
    {
        UrlBase = urlBase;
        _pastaTemporaria = pastaTemporaria;
        PastaDeEmails = pastaDeEmails;
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

        var aplicacao = new AplicacaoEmExecucao($"http://127.0.0.1:{porta}", pastaTemporaria, pastaDeEmails);

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
