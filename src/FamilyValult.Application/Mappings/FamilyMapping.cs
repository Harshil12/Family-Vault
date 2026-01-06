using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

public class FamilyMapping : Profile
{
    public FamilyMapping()
    {
        CreateMap<CreateFamilyRequest, Family>();
        CreateMap<UpdateFamlyRequest, Family>();
        CreateMap<Family, FamilyDto>().ReverseMap();
    }    
}
