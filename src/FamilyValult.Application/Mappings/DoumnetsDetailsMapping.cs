using AutoMapper;
using FamilyVault.Application.DTOs.Documents;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

public class DoumnetsDetailsMapping : Profile
{
    public DoumnetsDetailsMapping()
    {
        CreateMap<CreateDocumentRequest, DocumentDetails>();
        CreateMap<UpdateDocumentRequest, DocumentDetails>();
        CreateMap<DocumentDetails, DocumentDetailsDto>().ReverseMap();
    }
}
