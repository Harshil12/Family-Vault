using FamilyVault.Application.DTOs.Family;
using FluentValidation;

namespace FamilyVault.Application.Validators.Family;

/// <summary>
/// Represents CreateFamilyValidators.
/// </summary>
public class CreateFamilyValidators : AbstractValidator<CreateFamilyRequest>
{
    /// <summary>
    /// Initializes a new instance of CreateFamilyValidators.
    /// </summary>
    public CreateFamilyValidators()
    {
        RuleFor(x => x.FamilyName).NotEmpty().WithMessage("Family name is required.");

        RuleFor(x => x.FamilyName).MaximumLength(100).WithMessage("Family name cannot exceed 100 characters."); 

    }   
}
