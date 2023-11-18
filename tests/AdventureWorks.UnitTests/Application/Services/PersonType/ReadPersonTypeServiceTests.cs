using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Services.PersonType;
using AdventureWorks.Application.Services.PersonType;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Profiles.Person;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.PersonType;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonTypeServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonTypeRepository> _mockRepository = new();
    private ReadPersonTypeService _sut;

    public ReadPersonTypeServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(PersonTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPersonTypeService(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadPersonTypeService)
                .Should().Implement<IReadPersonTypeService>();

            typeof(ReadPersonTypeService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPersonTypeService(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPersonTypeService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("personTypeRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((PersonTypeEntity)null!);

        var result = await _sut.GetByIdAsync(12);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new PersonTypeEntity { PersonTypeId = 1, PersonTypeName = "Home", PersonTypeCode = "hello", PersonTypeDescription = "hello world"});

        var result = await _sut.GetByIdAsync(1);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Home");
            result!.Code.Should().Be("hello");
            result!.Description.Should().Be("hello world");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<PersonTypeEntity>)null!);

        var result = await _sut.GetListAsync();
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<PersonTypeEntity>());

        result = await _sut.GetListAsync();
        result.Should().BeEmpty();
    }


    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<PersonTypeEntity>
            {
                new() {PersonTypeId = 1, PersonTypeName = "Home", PersonTypeDescription = "test"}
                ,new() {PersonTypeId = 2, PersonTypeName = "Billing", PersonTypeDescription = "test02"}
                ,new() {PersonTypeId = 3, PersonTypeName = "Mailing", PersonTypeDescription = "test03"}
            });

        var result = await _sut.GetListAsync();
        result.Count.Should().Be(3);
    }
}
