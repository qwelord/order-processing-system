using AutoMapper;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
