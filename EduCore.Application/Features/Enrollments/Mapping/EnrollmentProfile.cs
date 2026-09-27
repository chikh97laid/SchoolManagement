using AutoMapper;
using EduCore.Application.Features.Enrollments.DTOs;
using EduCore.Domain.Entities;

namespace EduCore.Application.Features.Enrollments.Mapping;

public class EnrollmentProfile : Profile
{
    public EnrollmentProfile()
    {
        CreateMap<CreateEnrollmentRequest, Enrollment>();

        CreateMap<Enrollment, EnrollmentResponse>()
            .ForMember(
                dest => dest.StudentName,
                opt => opt.MapFrom(
                    src => $"{src.Student.User.FirstName} {src.Student.User.LastName}"))

            .ForMember(
                dest => dest.GroupName,
                opt => opt.MapFrom(
                    src => src.Group.Name))

            .ForMember(
                dest => dest.TeacherId,
                opt => opt.MapFrom(
                    src => src.Group.TeacherId))

            .ForMember(
                dest => dest.TeacherName,
                opt => opt.MapFrom(
                    src => $"{src.Group.Teacher.User.FirstName} {src.Group.Teacher.User.LastName}"));
    }
}