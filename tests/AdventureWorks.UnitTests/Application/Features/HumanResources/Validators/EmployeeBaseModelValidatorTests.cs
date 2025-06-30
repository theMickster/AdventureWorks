using AdventureWorks.Application.Features.HumanResources.Validators;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.UnitTests.Setup.Fixtures;
using FluentValidation.TestHelper;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Validators;

[ExcludeFromCodeCoverage]
public sealed class EmployeeBaseModelValidatorTests : UnitTestBase
{
    private readonly EmployeeBaseModelValidator<EmployeeCreateModel> _sut = new();

    [Theory]
    [InlineData(51)] // 51 characters - too long
    [InlineData(100)] // 100 characters - too long
    public void Validator_should_have_first_name_length_error(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.FirstName = new string('a', length);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorCode("Rule-01");
    }

    [Theory]
    [InlineData(51)] // 51 characters - too long
    [InlineData(100)] // 100 characters - too long
    public void Validator_should_have_last_name_length_error(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.LastName = new string('b', length);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorCode("Rule-02");
    }

    [Theory]
    [InlineData(51)] // 51 characters - too long
    [InlineData(100)] // 100 characters - too long
    public void Validator_should_have_middle_name_length_error(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.MiddleName = new string('c', length);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MiddleName)
            .WithErrorCode("Rule-03");
    }

    [Fact]
    public void Validator_should_not_validate_middle_name_when_null_or_empty()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.MiddleName = null;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.MiddleName);
    }

    [Theory]
    [InlineData(9)] // 9 characters - too long
    [InlineData(20)] // 20 characters - too long
    public void Validator_should_have_title_length_error(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.Title = new string('d', length);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorCode("Rule-04");
    }

    [Fact]
    public void Validator_should_not_validate_title_when_null_or_empty()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.Title = null;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData(11)] // 11 characters - too long
    [InlineData(20)] // 20 characters - too long
    public void Validator_should_have_suffix_length_error(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.Suffix = new string('e', length);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Suffix)
            .WithErrorCode("Rule-05");
    }

    [Fact]
    public void Validator_should_not_validate_suffix_when_null_or_empty()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.Suffix = null;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Suffix);
    }

    [Theory]
    [InlineData(51)] // 51 characters - too long
    [InlineData(100)] // 100 characters - too long
    public void Validator_should_have_job_title_length_error(int length)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.JobTitle = new string('f', length);

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.JobTitle)
            .WithErrorCode("Rule-06");
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Y")]
    [InlineData("A")]
    [InlineData("married")]
    [InlineData("single")]
    public void Validator_should_have_marital_status_invalid_error(string maritalStatus)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.MaritalStatus = maritalStatus;

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus)
            .WithErrorCode("Rule-07");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("S")]
    public void Validator_should_accept_valid_marital_status(string maritalStatus)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.MaritalStatus = maritalStatus;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Theory]
    [InlineData("X")]
    [InlineData("Y")]
    [InlineData("A")]
    [InlineData("male")]
    [InlineData("female")]
    public void Validator_should_have_gender_invalid_error(string gender)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.Gender = gender;

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Gender)
            .WithErrorCode("Rule-08");
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    public void Validator_should_accept_valid_gender(string gender)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.Gender = gender;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.Gender);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validator_should_have_organization_level_negative_error(short organizationLevel)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.OrganizationLevel = organizationLevel;

        var result = _sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.OrganizationLevel)
            .WithErrorCode("Rule-09");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validator_should_accept_valid_organization_level(short organizationLevel)
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.OrganizationLevel = organizationLevel;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.OrganizationLevel);
    }

    [Fact]
    public void Validator_should_not_validate_organization_level_when_null()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();
        model.OrganizationLevel = null;

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.OrganizationLevel);
    }

    [Fact]
    public void Validator_succeeds_when_all_data_is_valid()
    {
        var model = HumanResourcesDomainFixtures.GetValidEmployeeCreateModel();

        var result = _sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
