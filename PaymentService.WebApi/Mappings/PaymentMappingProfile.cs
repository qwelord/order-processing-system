using AutoMapper;
using PaymentService.DataAccess.Entities;
using PaymentService.WebApi.DTOs;

namespace PaymentService.WebApi.Mappings;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<ProcessPaymentDto, Payment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => PaymentStatus.Completed))
            .ForMember(dest => dest.ProcessedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}