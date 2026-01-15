using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using TechsysLog.Application.Services.Interfaces;

namespace TechsysLog.Tests.Api.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IUserService> UserServiceMock { get; } = new();
    public Mock<IOrderService> OrderServiceMock { get; } = new();
    public Mock<IDeliveryService> DeliveryServiceMock { get; } = new();
    public Mock<INotificationService> NotificationServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove registrations reais e injeta mocks
            services.RemoveAll<IUserService>();
            services.RemoveAll<IOrderService>();
            services.RemoveAll<IDeliveryService>();
            services.RemoveAll<INotificationService>();

            services.AddScoped(_ => UserServiceMock.Object);
            services.AddScoped(_ => OrderServiceMock.Object);
            services.AddScoped(_ => DeliveryServiceMock.Object);
            services.AddScoped(_ => NotificationServiceMock.Object);

            // Override auth: qualquer endpoint [Authorize] passa
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });
        });
    }
}
