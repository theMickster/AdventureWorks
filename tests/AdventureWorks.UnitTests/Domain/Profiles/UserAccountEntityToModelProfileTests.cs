using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Profiles;
using AutoMapper;
using System.Text;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Domain.Models.Shield;

namespace AdventureWorks.UnitTests.Domain.Profiles;

[ExcludeFromCodeCoverage]
public sealed class UserAccountEntityToModelProfileTests : UnitTestBase
{
    private readonly IMapper _mapper;

    public UserAccountEntityToModelProfileTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(UserAccountEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void all_mappings_are_correctly_setup_succeeds() => _mapper.ConfigurationProvider.AssertConfigurationIsValid();

    [Fact]
    public void Map_entities_to_model_succeeds()
    {
        const string passwordExample = "Password1";
        var hash = Encoding.ASCII.GetBytes(passwordExample);

        var entity = new UserAccountEntity
        {
            BusinessEntityId = 725,
            ModifiedDate = new DateTime(2011, 11, 11),
            RecordId = new Guid("d683f2f4-647c-4ef3-b7ca-baf428657973"),
            UserName = "mickey.mantle",
            PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
            Person = new Person
            {
                BusinessEntityId = 725,
                FirstName = "Mickey",
                MiddleName = "Charles",
                LastName = "Mantle",
                Title = "Mr.",
                PersonType = "C"
            },
            PrimaryEmailAddressId = 7,
            EmailAddress = new EmailAddress() {BusinessEntityId = 725, EmailAddressId = 7, EmailAddressName = "mickey.mantle@example.com" }
        };

        var result = _mapper.Map<UserAccountModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(725);
            result.UserName.Should().Be("mickey.mantle");
            result.FirstName.Should().Be("Mickey");
            result.MiddleName.Should().Be("Charles");
            result.LastName.Should().Be("Mantle");
            result.PasswordHash.Should().NotBeNullOrEmpty();
            result.FullName.Should().Be("Mantle, Mickey Charles");
            result.PrimaryEmailAddress.Should().Be("mickey.mantle@example.com");
        }
    }

    [Fact]
    public void Map_entities_to_model_no_middle_name_succeeds()
    {
        const string passwordExample = "Password1";
        var hash = Encoding.ASCII.GetBytes(passwordExample);

        var entity = new UserAccountEntity
        {
            BusinessEntityId = 725,
            ModifiedDate = new DateTime(2011, 11, 11),
            RecordId = new Guid("d683f2f4-647c-4ef3-b7ca-baf428657973"),
            UserName = "mickey.mantle",
            PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
            Person = new Person
            {
                BusinessEntityId = 725,
                FirstName = "Mickey",
                LastName = "Mantle",
                Title = "Mr.",
                PersonType = "C"
            },
            PrimaryEmailAddressId = 7,
            EmailAddress = new EmailAddress() { BusinessEntityId = 725, EmailAddressId = 7, EmailAddressName = "mickey.mantle@example.com" }
        };

        var result = _mapper.Map<UserAccountModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(725);
            result.UserName.Should().Be("mickey.mantle");
            result.FirstName.Should().Be("Mickey");
            result.MiddleName.Should().BeNullOrWhiteSpace();
            result.LastName.Should().Be("Mantle");
            result.PasswordHash.Should().NotBeNullOrEmpty();
            result.FullName.Should().Be("Mantle, Mickey");
        }
    }


    [Fact]
    public void Map_entities_to_model_no_names_succeeds()
    {
        const string passwordExample = "Password1";
        var hash = Encoding.ASCII.GetBytes(passwordExample);

        var entity = new UserAccountEntity
        {
            BusinessEntityId = 725,
            ModifiedDate = new DateTime(2011, 11, 11),
            RecordId = new Guid("d683f2f4-647c-4ef3-b7ca-baf428657973"),
            UserName = "mickey.mantle",
            PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
            Person = new Person
            {
                BusinessEntityId = 725,
                Title = "Mr.",
                PersonType = "C"
            },
            PrimaryEmailAddressId = 7,
            EmailAddress = new EmailAddress() { BusinessEntityId = 725, EmailAddressId = 7, EmailAddressName = "mickey.mantle@example.com" }
        };

        var result = _mapper.Map<UserAccountModel>(entity);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Id.Should().Be(725);
            result.UserName.Should().Be("mickey.mantle");
            result.FirstName.Should().BeNullOrWhiteSpace();
            result.MiddleName.Should().BeNullOrWhiteSpace();
            result.LastName.Should().BeNullOrWhiteSpace();
            result.PasswordHash.Should().NotBeNullOrEmpty();
            result.FullName.Should().BeNullOrWhiteSpace();
        }
    }
}
