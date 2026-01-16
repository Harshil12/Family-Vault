using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

public class FamilyMembersMapping : Profile
{
    public FamilyMembersMapping()
    {
        CreateMap<CreateFamilyMememberRequest, FamilyMember>();
        CreateMap<UpdateFamilyMememberRequest, FamilyMember>();
        CreateMap<FamilyMember, FamilyMemberDto>().ReverseMap();
    }    
}
