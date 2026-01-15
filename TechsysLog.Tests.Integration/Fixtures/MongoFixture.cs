using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using System.ComponentModel;
using Xunit;

namespace TechsysLog.Tests.Integration.Fixtures;

public sealed class MongoFixture : IAsyncLifetime
{
    private readonly IContainer _container;

    public string ConnectionString { get; private set; } = default!;

    public MongoFixture()
    {
        _container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("mongo:7")
            .WithPortBinding(27017, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var hostPort = _container.GetMappedPublicPort(27017);
        ConnectionString = $"mongodb://localhost:{hostPort}";
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
