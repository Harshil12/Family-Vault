using AutoMapper;
using FamilyVault.Application.DTOs.FamilyMembers;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

/// <summary>
/// Represents FamilyMembersMapping.
/// </summary>
public class FamilyMembersMapping : Profile
{
    /// <summary>
    /// Initializes a new instance of FamilyMembersMapping.
    /// </summary>
    public FamilyMembersMapping()
    {
        CreateMap<CreateFamilyMememberRequest, FamilyMember>();
        CreateMap<UpdateFamilyMememberRequest, FamilyMember>();
        CreateMap<FamilyMember, FamilyMemberDto>().ReverseMap();
    }    
}
