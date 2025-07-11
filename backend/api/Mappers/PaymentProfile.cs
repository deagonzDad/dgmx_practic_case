using api.DTO.PaymentDTO;
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
        CreateMap<Payment, CreatedPaymentDTO>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.TotalAmount))
            // .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
            .ForMember(dest => dest.AmountNight, opt => opt.MapFrom(src => src.AmountPerNight))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
            .ForMember(
                dest => dest.Method,
                opt => opt.MapFrom(src => src.PaymentMethod.ToString())
            );
    }

    public void ConfigureDTOToModelMapping()
    {
        CreateMap<CreateReservationDTO, Payment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.InDate))
            .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.Method));
    }
}
