using AutoMapper;
using FamilyVault.Application.DTOs.Mediclaim;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

public class MediclaimMapping : Profile
{
    public MediclaimMapping()
    {
        CreateMap<CreateMediclaimPolicyRequest, MediclaimPolicyDetails>();
        CreateMap<UpdateMediclaimPolicyRequest, MediclaimPolicyDetails>();
        CreateMap<MediclaimPolicyDetails, MediclaimPolicyDetailsDto>().ReverseMap();
    }
}
