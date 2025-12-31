using FamilyVault.Application.DTOs.Family;
using FamilyVault.Application.DTOs.User;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyVault.Application.Validators.Family;

public class CreateFamilyValidators : AbstractValidator<CreateFamlyRequest>
{
    public CreateFamilyValidators()
    {
        RuleFor(x => x.FamilyName).NotEmpty().WithMessage("Family name is required.");

        RuleFor(x => x.FamilyName).MaximumLength(100).WithMessage("Family name cannot exceed 100 characters."); 

    }   
}
