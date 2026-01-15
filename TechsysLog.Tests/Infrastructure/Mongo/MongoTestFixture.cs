using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using System.ComponentModel;
using TechsysLog.Infrastructure.Persistence.Mongo;

namespace TechsysLog.Tests.Infrastructure.Mongo;

public sealed class MongoTestFixture : IAsyncLifetime
{
    private readonly IDockerContainer _container;


    public string ConnectionString { get; private set; } = default!;
    public string DatabaseName { get; } = $"techsyslog_tests_{Guid.NewGuid():N}";

    public MongoDbSettings Settings => new()
    {
        ConnectionString = ConnectionString,
        DatabaseName = DatabaseName
    };

    public MongoTestFixture()
    {
        // Mongo padrão
        var mongoConfig = new MongoDbTestcontainerConfiguration
        {
            Database = "admin",
            Username = "root",
            Password = "root"
        };

        _container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo:7")
            .WithPortBinding(27017, true)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", mongoConfig.Username)
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", mongoConfig.Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var hostPort = _container.GetMappedPublicPort(27017);
        ConnectionString = $"mongodb://root:root@localhost:{hostPort}";
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public MongoDbContext CreateContext() => new(Settings);

    public async Task EnsureIndexesAsync(IMongoDbContext ctx)
    {
        var initializer = new MongoIndexInitializer(ctx);
        await initializer.EnsureIndexesAsync(CancellationToken.None);
    }
}
