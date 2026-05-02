using AutoMapper;
using BusinessLogicLayer.DTOs;

namespace BusinessLogicLayer.Mappers
{
    public class ProductDTOToOrderItemResponseMappingProfile : Profile
    {
        public ProductDTOToOrderItemResponseMappingProfile()
        {
            CreateMap<ProductDto, OrderItemResponse>()
              .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductName))
              .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        }
    }
}
