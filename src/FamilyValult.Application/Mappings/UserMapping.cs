using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<CreateUserRequest, FamilyMember>();
        CreateMap<UpdateUserRequest, FamilyMember>();
        CreateMap<User, UserDto>();
    }    
}
