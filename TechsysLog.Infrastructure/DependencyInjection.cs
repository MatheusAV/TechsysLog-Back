using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechsysLog.Application.Abstractions.Auth;
using TechsysLog.Application.Abstractions.Cep;
using TechsysLog.Application.Abstractions.Realtime;
using TechsysLog.Application.Abstractions.Security;
using TechsysLog.Domain.Repositories;
using TechsysLog.Infrastructure.Auth;
using TechsysLog.Infrastructure.Cep;
using TechsysLog.Infrastructure.Persistence.Mongo;
using TechsysLog.Infrastructure.Persistence.Mongo.Repositories;
using TechsysLog.Infrastructure.Realtime;
using TechsysLog.Infrastructure.Security;
using TechsysLog.Realtime.Publishers;

namespace TechsysLog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Mongo settings
        var mongoSettings = config.GetSection("MongoDb").Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("Config MongoDb inválida.");

        services.AddSingleton(mongoSettings);
        services.AddSingleton<IMongoDbContext, MongoDbContext>();
        services.AddSingleton<IMongoIndexInitializer, MongoIndexInitializer>();

        // Repositories
        services.AddScoped<IUserRepository, MongoUserRepository>();
        services.AddScoped<IOrderRepository, MongoOrderRepository>();
        services.AddScoped<IDeliveryRepository, MongoDeliveryRepository>();
        services.AddScoped<INotificationRepository, MongoNotificationRepository>();

        // Security
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        // JWT
        var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Config Jwt inválida.");
        services.AddSingleton(jwtSettings);
        services.AddSingleton<ITokenService, JwtTokenService>();

        // CEP provider (HttpClient)
        services.AddHttpClient<ICepService, ViaCepService>();

        // Realtime (stub)
        services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

        return services;
    }
}
