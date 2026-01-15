using FluentAssertions;
using MongoDB.Driver;
using TechsysLog.Domain.Common.Entities;
using TechsysLog.Infrastructure.Persistence.Mongo.Repositories;

namespace TechsysLog.Tests.Infrastructure.Mongo.Repositories;

public sealed class MongoNotificationRepositoryTests : MongoTestBase
{
    public MongoNotificationRepositoryTests(MongoTestFixture fixture) : base(fixture) { }

    [Fact]
    public async Task create_and_list_by_user_should_work()
    {
        var repo = new MongoNotificationRepository(Ctx);

        await repo.CreateAsync(new Notification("u1", "msg1"), CancellationToken.None);
        await repo.CreateAsync(new Notification("u1", "msg2"), CancellationToken.None);
        await repo.CreateAsync(new Notification("u2", "msg3"), CancellationToken.None);

        var list = await repo.ListByUserAsync("u1", CancellationToken.None);

        list.Should().HaveCount(2);
        list.Select(x => x.UserId).Distinct().Should().ContainSingle().Which.Should().Be("u1");
    }

    [Fact]
    public async Task mark_as_read_should_update_document()
    {
        var repo = new MongoNotificationRepository(Ctx);

        var n = new Notification("u1", "msg");
        await repo.CreateAsync(n, CancellationToken.None);

        // Como o Id do Domain pode não ser o mesmo persistido no seu mapper atual,
        // vamos buscar o id real no banco primeiro.
        var docs = await Ctx.Notifications.Find(x => x.UserId == "u1").ToListAsync();
        docs.Should().HaveCount(1);
        var docId = docs[0].Id;

        await repo.MarkAsReadAsync(docId, "u1", CancellationToken.None);

        var updated = await Ctx.Notifications.Find(x => x.Id == docId).FirstAsync();
        updated.IsRead.Should().BeTrue();
    }
}
