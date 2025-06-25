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
    }

    private void ConfigureModelToDTOMapping()
    {
        CreateMap<Reservation, CreateReservationDTO>();
        // .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => src.CheckInDate))
    }

    private void ConfigureDTOToModelMapping()
    {
        CreateMap<CreateReservationDTO, Reservation>();
    }
}
