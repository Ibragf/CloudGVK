namespace BackendGVK.Services.Configs
{
    public class SmtpServerSettings
    {
        public string User { get; set; } = null!;
        public string Password { get; set; } = null!;
        public EmailServerSettings ServerSettings { get; set; } = null!;
    }

    public class EmailServerSettings
    {
        public string? Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
    }
}
