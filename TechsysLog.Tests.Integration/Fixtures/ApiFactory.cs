using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace TechsysLog.Tests.Integration.Fixtures;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _mongoConn;

    public ApiFactory(string mongoConn) => _mongoConn = mongoConn;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = _mongoConn,
                ["MongoDb:DatabaseName"] = $"techsyslog_test_{Guid.NewGuid():N}",

                // JWT fixo para testes
                ["Jwt:Issuer"] = "techsyslog",
                ["Jwt:Audience"] = "techsyslog",
                ["Jwt:SecretKey"] = "THIS_IS_A_TEST_SECRET_KEY_32_CHARS_MIN!!!!",
                ["Jwt:ExpirationMinutes"] = "120"
            };

            cfg.AddInMemoryCollection(dict);
        });
    }
}
