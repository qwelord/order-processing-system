using AutoMapper;
using OrderService.DataAccess.Entities;
using OrderService.WebApi.DTOs;

namespace OrderService.WebApi.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderItem, OrderItemResponseDto>();
        CreateMap<Order, OrderResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
