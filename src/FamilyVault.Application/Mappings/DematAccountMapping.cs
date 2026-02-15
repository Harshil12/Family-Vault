using AutoMapper;
using FamilyVault.Application.DTOs.DematAccounts;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

public class DematAccountMapping : Profile
{
    public DematAccountMapping()
    {
        CreateMap<CreateDematAccountRequest, DematAccountDetails>();
        CreateMap<UpdateDematAccountRequest, DematAccountDetails>();
        CreateMap<DematAccountDetails, DematAccountDetailsDto>().ReverseMap();
    }
}
