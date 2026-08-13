using System.Text.RegularExpressions;

namespace DocesCabana.Tests.E2E.Infraestrutura;

/// <summary>
/// Lê a pasta de e-mails do <c>EmailServiceArquivo</c> e extrai o link de
/// redefinição de senha. Espera por condição — o arquivo aparecer —, nunca
/// por tempo fixo.
/// </summary>
public static class CaixaDeEntrada
{
    private static readonly Regex LinkRegex = new("""href='(?<link>[^']+)'""", RegexOptions.Compiled);

    public static async Task<string> EsperarLinkDeRedefinicao(
        string pastaDeEmails, string destinatario, TimeSpan? tempoLimite = null)
    {
        var prazo = DateTime.UtcNow.Add(tempoLimite ?? TimeSpan.FromSeconds(15));

        while (DateTime.UtcNow < prazo)
        {
            if (Directory.Exists(pastaDeEmails))
            {
                var arquivos = Directory.GetFiles(pastaDeEmails)
                    .OrderByDescending(File.GetLastWriteTimeUtc);

                foreach (var arquivo in arquivos)
                {
                    string conteudo;
                    try
                    {
                        conteudo = await File.ReadAllTextAsync(arquivo);
                    }
                    catch (IOException)
                    {
                        continue; // arquivo ainda sendo escrito — tenta o próximo ciclo
                    }

                    if (!conteudo.Contains($"Para: {destinatario}", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var match = LinkRegex.Match(conteudo);
                    if (match.Success)
                        return match.Groups["link"].Value;
                }
            }

            await Task.Delay(100);
        }

        throw new TimeoutException(
            $"Nenhum e-mail com link de redefinição encontrado para '{destinatario}' em '{pastaDeEmails}' dentro do tempo limite.");
    }
}
