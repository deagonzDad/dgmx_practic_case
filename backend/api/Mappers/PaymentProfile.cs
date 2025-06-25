using System;
using api.DTO.ReservationsDTO;
using api.Models;
using AutoMapper;

namespace api.Mappers;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        ConfigureModelToDTOMapping();
        ConfigureDTOToModelMapping();
    }

    private void ConfigureModelToDTOMapping()
    {
        // CreateMap<Payment, CreateReservationDTO>()
        //     .ForMember(dest => dest., opt => opt.MapFrom(src => src.Amount));
    }

    public void ConfigureDTOToModelMapping()
    {
        CreateMap<CreateReservationDTO, Payment>()
            .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.InDate))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Method));
    }
}
