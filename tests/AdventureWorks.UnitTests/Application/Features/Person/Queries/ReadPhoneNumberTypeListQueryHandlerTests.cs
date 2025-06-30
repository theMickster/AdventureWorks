using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

public sealed class ReadPhoneNumberTypeListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPhoneNumberTypeRepository> _mockRepository = new();
    private ReadPhoneNumberTypeListQueryHandler _sut;

    public ReadPhoneNumberTypeListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PhoneNumberTypeEntityToPhoneNumberTypeModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPhoneNumberTypeListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPhoneNumberTypeListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPhoneNumberTypeListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("phoneNumberTypeRepository");
        }
    }

    [Fact]
    public async Task Handle_returns_empty_list_Async()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<PhoneNumberTypeEntity>());

        var result = await _sut.Handle(new ReadPhoneNumberTypeListQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<PhoneNumberTypeEntity>
            {
                new() { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DateTime.UtcNow },
                new() { PhoneNumberTypeId = 2, Name = "Home", ModifiedDate = DateTime.UtcNow },
                new() { PhoneNumberTypeId = 3, Name = "Work", ModifiedDate = DateTime.UtcNow }
            });

        var result = await _sut.Handle(new ReadPhoneNumberTypeListQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result.Count(x => x.Id == 2 && x.Name == "Home").Should().Be(1);
        }
    }
}
