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
}
