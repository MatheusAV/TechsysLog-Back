using FluentAssertions;
using TechsysLog.Infrastructure.Security;
using Xunit;

namespace TechsysLog.Tests.Infrastructure.Security;

public sealed class BcryptPasswordHasherTests
{
    [Fact]
    public void hash_should_create_different_hashes_for_same_password_and_verify_should_work()
    {
        var sut = new BcryptPasswordHasher();

        var password = "123456";

        var h1 = sut.Hash(password);
        var h2 = sut.Hash(password);

        h1.Should().NotBeNullOrWhiteSpace();
        h2.Should().NotBeNullOrWhiteSpace();
        h1.Should().NotBe(h2); // bcrypt usa salt

        sut.Verify(password, h1).Should().BeTrue();
        sut.Verify(password, h2).Should().BeTrue();
        sut.Verify("wrong", h1).Should().BeFalse();
    }
}
