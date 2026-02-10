using AutoMapper;
using FamilyVault.Application.DTOs.User;
using FamilyVault.Domain.Entities;
namespace FamilyVault.Application.Mappings;

/// <summary>
/// Represents UserMapping.
/// </summary>
public class UserMapping : Profile
{
    /// <summary>
    /// Initializes a new instance of UserMapping.
    /// </summary>
    public UserMapping()
    {
        CreateMap<CreateUserRequest, User>();
        CreateMap<UpdateUserRequest, User>();
        CreateMap<User, UserDto>().ReverseMap();
    }    
}
