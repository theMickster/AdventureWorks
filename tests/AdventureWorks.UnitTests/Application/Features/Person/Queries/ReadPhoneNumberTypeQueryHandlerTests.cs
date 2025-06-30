using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

public sealed class ReadPhoneNumberTypeQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPhoneNumberTypeRepository> _mockRepository = new();
    private ReadPhoneNumberTypeQueryHandler _sut;

    public ReadPhoneNumberTypeQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PhoneNumberTypeEntityToPhoneNumberTypeModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPhoneNumberTypeQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPhoneNumberTypeQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPhoneNumberTypeQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("phoneNumberTypeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((PhoneNumberTypeEntity)null!);

        var result = await _sut.Handle(new ReadPhoneNumberTypeQuery { Id = 12 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DateTime.UtcNow });

        var result = await _sut.Handle(new ReadPhoneNumberTypeQuery { Id = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Cell");
        }
    }
}
