using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Domain.Enums;
using FluentValidation;

namespace FamilyVault.Application.Validators.FamilyMembers;

/// <summary>
/// Represents UpdateFamilyMemberValidators.
/// </summary>
public class UpdateFamilyMemberValidators : AbstractValidator<UpdateFamilyMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of UpdateFamilyMemberValidators.
    /// </summary>
    public UpdateFamilyMemberValidators()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Family member ID is required.");

        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName).MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.FamilyId).NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.Email).EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code cannot exceed 5 characters.");
        RuleFor(x => x.Mobile).MaximumLength(10).WithMessage("Mobile number cannot exceed 10 characters.");
        RuleFor(x => x.PAN).MaximumLength(10).WithMessage("PAN cannot exceed 10 characters.");

        RuleFor(x => x).Custom((request, context) =>
        {
            if (!string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.CountryCode))
            {
                context.AddFailure("CountryCode", "Country code is required when mobile number is provided.");
            }
            if (!string.IsNullOrEmpty(request.CountryCode) && string.IsNullOrEmpty(request.Mobile))
            {
                context.AddFailure("Mobile", "Mobile number is required when country code is provided.");
            }
        });

        RuleFor(x => x).Custom((request, context) =>
        {
            if (!string.IsNullOrEmpty(request.CountryCode) && !request.CountryCode.StartsWith("+"))
            {
                context.AddFailure("CountryCode", "Country code must start with '+'.");
            }
        });

        RuleFor(x => x.PAN).Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]$")
           .WithMessage("PAN must be in the corect format");

        RuleFor(x => x.Aadhar).Matches(@"^\d{12}$")
            .WithMessage("Aadhaar number must be exactly 12 digits.");

        RuleFor(x => x.DateOfBirth).LessThan(DateTimeOffset.UtcNow)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.RelationshipType).IsInEnum()
            .WithMessage("Invalid relationship type.");

        RuleFor(x => x.BloodGroup).IsInEnum()
            .When(x => x.BloodGroup.HasValue)
            .WithMessage("Invalid blood group.");

        RuleFor(x => x).Custom((request, context) =>
        {
            if (request.RelationshipType == Relationships.Self)
            {
                context.AddFailure("RelationshipType", "Relationship type cannot be 'Self' for family members.");
            }
        });
    }   
}
