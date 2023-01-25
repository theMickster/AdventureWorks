using AdventureWorks.API.Controllers.v1.Address;
using AdventureWorks.API.Controllers.v1.CountryRegion;
using AdventureWorks.Application.Interfaces.Services.CountryRegion;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.API.Controllers.v1.CountryRegion;

public sealed class ReadCountryRegionControllerTests : UnitTestBase
{
    private readonly Mock<ILogger<ReadCountryRegionController>> _mockLogger = new();
    private readonly Mock<IReadCountryRegionService> _mockReadCountryRegionService = new();
    private readonly ReadCountryRegionController _sut;

    public ReadCountryRegionControllerTests()
    {
        _sut = new ReadCountryRegionController(_mockLogger.Object, _mockReadCountryRegionService.Object);
    }

    [Fact]
    public void Controller_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _ = new ReadCountryRegionController(null!, _mockReadCountryRegionService.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("logger");

            _ = ((Action)(() => _ = new ReadCountryRegionController(_mockLogger.Object, null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("readCountryRegionService");
        }
    }

}