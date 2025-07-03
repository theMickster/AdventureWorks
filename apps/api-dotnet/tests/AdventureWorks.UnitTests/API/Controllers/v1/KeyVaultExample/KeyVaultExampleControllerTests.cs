using AdventureWorks.API.Controllers.v1.KeyVaultExample;
using AdventureWorks.Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace AdventureWorks.UnitTests.API.Controllers.v1.KeyVaultExample;

[ExcludeFromCodeCoverage]
public sealed class KeyVaultExampleControllerTests : UnitTestBase
{
    private readonly Mock<IOptionsSnapshot<AkvExampleSettings>> _mockOptionsSnapshotConfig = new();
    private readonly KeyVaultExampleController _sut;

    public KeyVaultExampleControllerTests()
    {
        _sut = new KeyVaultExampleController(_mockOptionsSnapshotConfig.Object);
    }

    [Fact]
    public void get_returns_ok()
    {
        _mockOptionsSnapshotConfig.Setup(i => i.Value)
            .Returns(new AkvExampleSettings{MyFavoriteComedicMovie = "Hello World"});

        var result = _sut.Get();

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }
    }

    [Fact]
    public void get_returns_not_found()
    {
        _mockOptionsSnapshotConfig.Setup(i => i.Value)
            .Returns((AkvExampleSettings)null!);

        var result = _sut.Get();

        var objectResult = result as ObjectResult;

        using (new AssertionScope())
        {
            objectResult.Should().NotBeNull();

            objectResult!.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
