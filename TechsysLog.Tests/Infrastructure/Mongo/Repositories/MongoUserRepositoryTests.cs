using FluentAssertions;
using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Infrastructure.Persistence.Mongo.Repositories;

namespace TechsysLog.Tests.Infrastructure.Mongo.Repositories;

public sealed class MongoUserRepositoryTests : MongoTestBase
{
    public MongoUserRepositoryTests(MongoTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task create_and_get_by_email_should_work()
    {
        var repo = new MongoUserRepository(Ctx);

        var user = new User("Matheus", "matheus@teste.com", "HASHED");
        await repo.CreateAsync(user, CancellationToken.None);

        var found = await repo.GetByEmailAsync("matheus@teste.com", CancellationToken.None);

        found.Should().NotBeNull();
        found!.Email.Should().Be("matheus@teste.com");
        found.Name.Should().Be("Matheus");
        found.PasswordHash.Should().Be("HASHED");
    }

    [Fact]
    public async Task create_should_enforce_unique_email_index()
    {
        var repo = new MongoUserRepository(Ctx);

        await repo.CreateAsync(new User("A", "dup@teste.com", "H1"), CancellationToken.None);

        Func<Task> act = async () =>
            await repo.CreateAsync(new User("B", "dup@teste.com", "H2"), CancellationToken.None);

        // Mongo lança MongoWriteException para unique index
        await act.Should().ThrowAsync<MongoWriteException>();
    }
}
