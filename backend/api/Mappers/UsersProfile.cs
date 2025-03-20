using System;
using api.DTO.UsersDTO;
using api.Helpers.Instances;
using api.Models;
using AutoMapper;

namespace api.Mappers;

public class UsersProfile : Profile
{
    private readonly IHasher _hasher;

    public UsersProfile(IHasher hasher)
    {
        _hasher = hasher;
        ConfigureUserModelToUserDTOMapping();
        ConfigureUserDTOToUserModelMapping();
    }

    private void ConfigureUserModelToUserDTOMapping()
    {
        CreateMap<User, UserCreatedDTO>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
    }

    private void ConfigureUserDTOToUserModelMapping()
    {
        CreateMap<UserCreateDTO, User>()
            .ForMember(
                dest => dest.Password,
                opt => opt.MapFrom(src => _hasher.HashPassword(src.Password))
            );
    }
}
