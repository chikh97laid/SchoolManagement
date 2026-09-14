using EduCore.Application.Features.StudentManagement.DTOs.Student;
using EduCore.Domain.Entities;
using AutoMapper;

namespace EduCore.Application.Features.StudentManagement.Mapping;

// Ensure the correct 'Profile' type from AutoMapper is referenced
public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentResponse>()
            .ForMember(
                dest => dest.FullName,
                opt => opt.MapFrom(
                    src => $"{src.FirstName} {src.LastName}"))
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(
                    src => src.User != null
                        ? src.User.UserName
                        : null));

        CreateMap<UpdateStudentRequest, Student>();

    }
}