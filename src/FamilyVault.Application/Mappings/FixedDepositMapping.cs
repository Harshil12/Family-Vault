using AutoMapper;
using FamilyVault.Application.DTOs.FixedDeposits;
using FamilyVault.Domain.Entities;

namespace FamilyVault.Application.Mappings;

public class FixedDepositMapping : Profile
{
    public FixedDepositMapping()
    {
        CreateMap<CreateFixedDepositRequest, FixedDepositDetails>();
        CreateMap<UpdateFixedDepositRequest, FixedDepositDetails>();
        CreateMap<FixedDepositDetails, FixedDepositDetailsDto>().ReverseMap();
    }
}
