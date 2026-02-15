using AutoMapper;
using FamilyVault.Application.DTOs.MutualFunds;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

public class MutualFundMapping : Profile
{
    public MutualFundMapping()
    {
        CreateMap<CreateMutualFundHoldingRequest, MutualFundHoldingDetails>();
        CreateMap<UpdateMutualFundHoldingRequest, MutualFundHoldingDetails>();
        CreateMap<MutualFundHoldingDetails, MutualFundHoldingDetailsDto>().ReverseMap();
    }
}
