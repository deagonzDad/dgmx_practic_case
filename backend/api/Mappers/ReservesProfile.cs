using System;
using api.DTO.ReservationsDTO;
using api.Models;
using AutoMapper;

namespace api.Mappers;

public class ReservesProfile : Profile
{
    public ReservesProfile()
    {
        ConfigureModelToDTOMapping();
        ConfigureDTOToModelMapping();
        ConfigureModelToDTOListMapping();
    }

    private void ConfigureModelToDTOMapping()
    {
        CreateMap<Reservation, CreatedReservationDTO>()
            .ForMember(dest => dest.InDate, opt => opt.MapFrom(src => src.CheckInDate))
            .ForMember(dest => dest.OutDate, opt => opt.MapFrom(src => src.CheckOutDate))
            .ForMember(dest => dest.Guests, opt => opt.MapFrom(src => src.NumberOfGuests))
            .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.Id));
    }

    private void ConfigureDTOToModelMapping()
    {
        CreateMap<CreateReservationDTO, Reservation>()
            .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.ClientId))
            .ForMember(dest => dest.NumberOfGuests, opt => opt.MapFrom(src => src.Guest))
            .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.InDate));
    }

    private void ConfigureModelToDTOListMapping()
    {
        CreateMap<Reservation, CreatedReservationListDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => ""))
            .ForMember(dest => dest.RoomNumber, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.OutDate, opt => opt.MapFrom(src => src.CheckOutDate))
            .ForMember(dest => dest.InDate, opt => opt.MapFrom(src => src.CheckInDate))
            .ForMember(dest => dest.Guests, opt => opt.MapFrom(src => src.NumberOfGuests))
            .ForMember(dest => dest.ReservationId, opt => opt.MapFrom(src => src.Id));
    }
}
