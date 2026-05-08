using AutoMapper;
using IdentityService.Core.DTOs;
using IdentityService.Core.Models;

namespace IdentityService.Core.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserType>();
    }
}
