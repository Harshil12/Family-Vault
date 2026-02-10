using AutoMapper;
using FamilyVault.Application.DTOs.Family;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

/// <summary>
/// Represents FamilyMapping.
/// </summary>
public class FamilyMapping : Profile
{
    /// <summary>
    /// Initializes a new instance of FamilyMapping.
    /// </summary>
    public FamilyMapping()
    {
        CreateMap<CreateFamilyRequest, Family>();
        CreateMap<UpdateFamilyRequest, Family>();
        CreateMap<Family, FamilyDto>().ReverseMap();
    }    
}

