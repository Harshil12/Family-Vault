using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<CreateUserRequest, User>();
        CreateMap<UpdateUserRequest, User>();
        CreateMap<User, UserDto>().ReverseMap();
    }    
}
