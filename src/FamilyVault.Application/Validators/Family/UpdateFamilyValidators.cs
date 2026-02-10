using FamilyVault.Application.DTOs.Family;
using FluentValidation;

namespace FamilyVault.Application.Validators.Family;

/// <summary>
/// Represents UpdateFamilyValidators.
/// </summary>
public class UpdateFamilyValidators : AbstractValidator<UpdateFamilyRequest>
{
    /// <summary>
    /// Initializes a new instance of UpdateFamilyValidators.
    /// </summary>
    public UpdateFamilyValidators()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Family ID is required.");
        RuleFor(x => x.FamilyName).NotEmpty().WithMessage("Family name is required.");
        RuleFor(x => x.FamilyName).MaximumLength(100).WithMessage("Family name cannot exceed 100 characters.");
    }   
}

