using AutoMapper;
using FamilyVault.Application.DTOs.LifeInsurance;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

public class LifeInsuranceMapping : Profile
{
    public LifeInsuranceMapping()
    {
        CreateMap<CreateLifeInsurancePolicyRequest, LifeInsurancePolicyDetails>();
        CreateMap<UpdateLifeInsurancePolicyRequest, LifeInsurancePolicyDetails>();
        CreateMap<LifeInsurancePolicyDetails, LifeInsurancePolicyDetailsDto>().ReverseMap();
    }
}
