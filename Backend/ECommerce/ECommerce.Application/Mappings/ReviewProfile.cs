using AutoMapper;
using ECommerce.Application.DTOs.Review;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings;

public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.Customer.User.FirstName + " " + src.Customer.User.LastName));
    }
}