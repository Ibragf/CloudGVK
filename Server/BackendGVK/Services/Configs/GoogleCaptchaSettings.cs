namespace BackendGVK.Services.Configs
{
    public class GoogleCaptchaSettings
    {
        public version v3 { get; set; } = null!;
        public version v2 { get; set; } = null!;
    }

    public class version
    {
        public string? SiteKey { get; set; }
        public string? SecretKey { get; set; }
    }
}
