namespace TechsysLog.Infrastructure.Auth
{

    public sealed class JwtSettings
    {
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public int ExpirationMinutes { get; set; } = 60;
    }
}
