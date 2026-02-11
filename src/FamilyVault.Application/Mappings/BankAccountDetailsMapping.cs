using AutoMapper;
using FamilyVault.Application.DTOs.BankAccounts;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

/// <summary>
/// Represents BankAccountDetailsMapping.
/// </summary>
public class BankAccountDetailsMapping : Profile
{
    /// <summary>
    /// Initializes a new instance of BankAccountDetailsMapping.
    /// </summary>
    public BankAccountDetailsMapping()
    {
        CreateMap<CreateBankAccountRequest, BankAccountDetails>();
        CreateMap<UpdateBankAccountRequest, BankAccountDetails>();
        CreateMap<BankAccountDetails, BankAccountDetailsDto>().ReverseMap();
    }
}
