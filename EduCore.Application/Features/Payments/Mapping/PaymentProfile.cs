using AutoMapper;
using EduCore.Application.Features.Payments.DTOs;
using EduCore.Domain.Entities;

namespace EduCore.Application.Features.Payments.Mapping
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<CreatePaymentRequest, Payment>()
                            .ForMember(dest => dest.PaymentDate, opt => opt
                            .MapFrom(src => DateOnly.FromDateTime(DateTime.UtcNow)));

            CreateMap<UpdatePaymentRequest, Payment>()
                            .ForMember(dest => dest.PaymentDate, opt => opt
                            .MapFrom(src => DateOnly.FromDateTime(DateTime.UtcNow)));

            CreateMap<Payment, PaymentResponse>();
        }
    }
}
