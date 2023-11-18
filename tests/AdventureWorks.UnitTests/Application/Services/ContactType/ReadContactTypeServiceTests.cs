using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Services.ContactType;
using AdventureWorks.Application.Services.ContactType;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Profiles.Person;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.ContactType;

[ExcludeFromCodeCoverage]
public sealed class ReadContactTypeServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IContactTypeRepository> _mockRepository = new();
    private ReadContactTypeService _sut;

    public ReadContactTypeServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(ContactTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadContactTypeService(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadContactTypeService)
                .Should().Implement<IReadContactTypeService>();

            typeof(ReadContactTypeService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadContactTypeService(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadContactTypeService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("contactTypeRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ContactTypeEntity)null!);

        var result = await _sut.GetByIdAsync(12);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new ContactTypeEntity { ContactTypeId = 1, Name = "Home" });

        var result = await _sut.GetByIdAsync(1);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Home");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<ContactTypeEntity>)null!);

        var result = await _sut.GetListAsync();
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<ContactTypeEntity>());

        result = await _sut.GetListAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<ContactTypeEntity>
            {
                new() {ContactTypeId = 1, Name = "Home"}
                ,new() {ContactTypeId = 2, Name = "Billing"}
                ,new() {ContactTypeId = 3, Name = "Mailing"}
            });

        var result = await _sut.GetListAsync();
        result.Count.Should().Be(3);
    }
}
