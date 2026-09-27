using AutoMapper;
using EduCore.Application.Features.Teachers.DTOs;
using EduCore.Domain.Entities;

namespace EduCore.Application.Features.Teachers.Mapping;
public class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<CreateTeacherRequest, Teacher>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.Ignore())
            .ForMember(
                dest => dest.IsActive,
                opt => opt.Ignore());

        CreateMap<UpdateTeacherRequest, Teacher>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.Ignore())
            .ForMember(
                dest => dest.IsActive,
                opt => opt.Ignore());

        CreateMap<Teacher, TeacherResponse>()
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(
                    src => $"{src.User.FirstName}   {src.User.LastName}"))
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(
                    src => src.User != null
                        ? src.User.UserName
                        : null))
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(
                    src => src.User != null
                        ? src.User.Email
                        : null))
            .ForMember(
                dest => dest.PhoneNumber,
                opt => opt.MapFrom(
                    src => src.User != null
                        ? src.User.PhoneNumber
                        : null))
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(
                    src => src.UserId));
    }
}