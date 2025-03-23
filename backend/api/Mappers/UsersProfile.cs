using api.DTO.UsersDTO;
using api.Models;
using AutoMapper;

namespace api.Mappers;

public class UsersProfile : Profile
{
    public UsersProfile()
    {
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
        CreateMap<UserCreateDTO, User>();
    }
}
