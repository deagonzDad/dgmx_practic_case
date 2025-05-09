using System;
using api.DTO.RoomsDTO;
using api.Models;
using AutoMapper;

namespace api.Mappers;

public class RoomsProfile : Profile
{
    public RoomsProfile()
    {
        ConfigureModelToDTOMapping();
        ConfigureDTOToModelMapping();
    }

    private void ConfigureModelToDTOMapping()
    {
        CreateMap<Room, CreatedRoomDTO>()
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.RoomNumber))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.RoomType))
            .ForMember(dest => dest.ReservationDTOs, opt => opt.Ignore());
    }

    private void ConfigureDTOToModelMapping()
    {
        CreateMap<CreateRoomDTO, Room>()
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => src.Number))
            .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.Type));
    }
}
