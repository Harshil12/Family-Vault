using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Application.Validators.FamilyMembers;
using FamilyVault.Application.Validators.User;
using FamilyVault.Domain.Enums;
using FluentAssertions;

namespace FamilyVault.Tests.Application.Validators;

public class RequestValidatorsTests
{
    [Fact]
    public void CreateUserValidators_ShouldReturnError_WhenMobileIsProvidedWithoutCountryCode()
    {
        // Arrange
        var validator = new CreateUserValidators();
        var request = ValidCreateUserRequest();
        request.CountryCode = null;
        request.Mobile = "9876543210";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountryCode");
    }

    [Fact]
    public void CreateUserValidators_ShouldReturnError_WhenCountryCodeDoesNotStartWithPlus()
    {
        // Arrange
        var validator = new CreateUserValidators();
        var request = ValidCreateUserRequest();
        request.CountryCode = "91";
        request.Mobile = "9876543210";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must start with '+'"));
    }

    [Fact]
    public void UpdateUserValidators_ShouldReturnError_WhenCountryCodeIsProvidedWithoutMobile()
    {
        // Arrange
        var validator = new UpdateUserValidators();
        var request = ValidUpdateUserRequest();
        request.CountryCode = "+91";
        request.Mobile = null;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Mobile");
    }

    [Fact]
    public void UpdateUserValidators_ShouldReturnError_WhenMobileIsProvidedWithoutCountryCode()
    {
        // Arrange
        var validator = new UpdateUserValidators();
        var request = ValidUpdateUserRequest();
        request.CountryCode = null;
        request.Mobile = "9876543210";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountryCode");
    }

    [Fact]
    public void CreateFamilyMemberValidators_ShouldReturnError_WhenRelationshipIsSelf()
    {
        // Arrange
        var validator = new CreateFamilyMemberValidators();
        var request = ValidCreateFamilyMemberRequest();
        request.RelationshipType = Relationships.Self;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RelationshipType");
    }

    [Fact]
    public void CreateFamilyMemberValidators_ShouldReturnErrors_WhenPanAadharOrDobIsInvalid()
    {
        // Arrange
        var validator = new CreateFamilyMemberValidators();
        var request = ValidCreateFamilyMemberRequest();
        request.PAN = "BADPAN";
        request.Aadhar = "123";
        request.DateOfBirth = DateTimeOffset.UtcNow.AddDays(1);

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PAN");
        result.Errors.Should().Contain(e => e.PropertyName == "Aadhar");
        result.Errors.Should().Contain(e => e.PropertyName == "DateOfBirth");
    }

    [Fact]
    public void UpdateFamilyMemberValidators_ShouldReturnError_WhenEnumsAreInvalid()
    {
        // Arrange
        var validator = new UpdateFamilyMemberValidators();
        var request = ValidUpdateFamilyMemberRequest();
        request.RelationshipType = (Relationships)200;
        request.BloodGroup = (BloodGroups)200;

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RelationshipType");
        result.Errors.Should().Contain(e => e.PropertyName == "BloodGroup");
    }

    [Fact]
    public void UpdateFamilyMemberValidators_ShouldReturnError_WhenCountryCodeAndMobileCombinationIsInvalid()
    {
        // Arrange
        var validator = new UpdateFamilyMemberValidators();
        var request = ValidUpdateFamilyMemberRequest();
        request.CountryCode = "91";
        request.Mobile = "9876543210";

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CountryCode");
    }

    private static CreateUserRequest ValidCreateUserRequest() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com",
        Username = "john",
        Password = "Password@123",
        CountryCode = "+91",
        Mobile = "9876543210"
    };

    private static UpdateUserRequest ValidUpdateUserRequest() => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com",
        Password = "Password@123",
        CountryCode = "+91",
        Mobile = "9876543210"
    };

    private static CreateFamilyMemberRequest ValidCreateFamilyMemberRequest() => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        FamilyId = Guid.NewGuid(),
        CountryCode = "+91",
        Mobile = "9876543210",
        RelationshipType = Relationships.Spouse,
        DateOfBirth = DateTimeOffset.UtcNow.AddYears(-20),
        BloodGroup = BloodGroups.A_Positive,
        Email = "jane@example.com",
        PAN = "ABCDE1234F",
        Aadhar = "123456789012"
    };

    private static UpdateFamilyMemberRequest ValidUpdateFamilyMemberRequest() => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Jane",
        LastName = "Doe",
        FamilyId = Guid.NewGuid(),
        CountryCode = "+91",
        Mobile = "9876543210",
        RelationshipType = Relationships.Spouse,
        DateOfBirth = DateTimeOffset.UtcNow.AddYears(-20),
        BloodGroup = BloodGroups.A_Positive,
        Email = "jane@example.com",
        PAN = "ABCDE1234F",
        Aadhar = "123456789012"
    };
}
