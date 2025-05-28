using AutoMapper;
using FCG.Domain.Entities;
using FCG.Service.DTO.Response;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IdentityUser, DTO.Response.RegisterUserResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ReverseMap();

            CreateMap<Game, DTO.Response.GameResponseDto>()
                .ReverseMap();

            CreateMap<Game, DTO.Request.GameRequestDto>()
                .ReverseMap();

            CreateMap<Client, DTO.Response.RegisterClientResponseDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ReverseMap(); 




        }
    }   
    
}
