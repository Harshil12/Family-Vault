using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Domain.Enums;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyVault.Application.Validators.Document;

public class CreateDocumentValidators : AbstractValidator<CreateDocumentRequest>
{
    public CreateDocumentValidators()
    {
        RuleFor(x => x.DocumentNumber).NotEmpty().WithMessage("Document number is required.");

        RuleFor(x => x.DocumentType).NotEmpty().WithMessage("DocumentType is required.");

        RuleFor(x => x.FamilyMemberId).NotEmpty().WithMessage("Family member is required.");

        RuleFor(x => x.ExpiryDate).GreaterThan(x => x.IssueDate.GetValueOrDefault())
            .When(x => x.ExpiryDate.HasValue && x.IssueDate.HasValue)
            .WithMessage("Expiry date must be greater than issue date.");

        RuleFor(x => x.ExpiryDate).GreaterThan(DateTimeOffset.UtcNow)
            .When(x => x.ExpiryDate.HasValue)
            .WithMessage("Expiry date must be in the future.");

        RuleFor(x => x.DocumentNumber).Matches(@"^[A-Z]{5}[0-9]{4}[A-Z]$")
            .When(x => x.DocumentType == DocumentTypes.PAN)
            .WithMessage("PAN must be in the corect format");

        RuleFor(x => x.DocumentNumber).Matches(@"^\d{12}$")
            .When(x => x.DocumentType == DocumentTypes.Aadhar)
            .WithMessage("Aadhaar number must be exactly 12 digits.");
    }   
}
