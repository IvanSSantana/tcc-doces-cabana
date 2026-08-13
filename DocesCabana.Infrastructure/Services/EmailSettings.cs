namespace DocesCabana.Infrastructure.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = "no-reply@docescabana.com.br";
    public string SenderName { get; set; } = "Doces Cabana";
    public bool EnableSsl { get; set; } = true;

    // "Smtp" é o único valor que liga o envio real; qualquer outra coisa —
    // ausente, vazia ou desconhecida — também cai no SMTP. Só o valor exato
    // "Arquivo" liga o adaptador de teste (spec 007).
    public string Adaptador { get; set; } = "Smtp";

    // Vazio de propósito: o adaptador de arquivo se recusa a inventar um
    // diretório — ver EmailServiceArquivo.
    public string PastaDeSaida { get; set; } = string.Empty;
}
