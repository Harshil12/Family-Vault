using AutoMapper;
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

/// <summary>
/// Represents DoumnetsDetailsMapping.
/// </summary>
public class DoumnetsDetailsMapping : Profile
{
    /// <summary>
    /// Initializes a new instance of DoumnetsDetailsMapping.
    /// </summary>
    public DoumnetsDetailsMapping()
    {
        CreateMap<CreateDocumentRequest, DocumentDetails>();
        CreateMap<UpdateDocumentRequest, DocumentDetails>();
        CreateMap<DocumentDetails, DocumentDetailsDto>().ReverseMap();
    }
}
