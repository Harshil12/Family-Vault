using FamilyVault.Application.DTOs.User;
using FluentValidation;

namespace FamilyVault.Application.Validators.User;

/// <summary>
/// Represents UpdateUserValidators.
/// </summary>
public class UpdateUserValidators : AbstractValidator<UpdateUserRequest>
{
    /// <summary>
    /// Initializes a new instance of UpdateUserValidators.
    /// </summary>
    public UpdateUserValidators()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.")
       .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

        RuleFor(x => x.LastName).MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
       
        RuleFor(x => x.CountryCode).MaximumLength(5).WithMessage("Country code cannot exceed 5 characters.");
        RuleFor(x => x.Mobile).MaximumLength(10).WithMessage("Mobile number cannot exceed 10 characters.");

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
    }   
}
