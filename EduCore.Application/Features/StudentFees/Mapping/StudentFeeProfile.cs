using AutoMapper;
using EduCore.Application.Features.StudentFees.DTOs;
using EduCore.Domain.Entities;

namespace EduCore.Application.Features.StudentFees.Mapping;

public class StudentFeeProfile : Profile
{
    public StudentFeeProfile()
    {
        CreateMap<CreateStudentFeeRequest, StudentFee>();

        CreateMap<UpdateStudentFeeRequest, StudentFee>();

        CreateMap<StudentFee, StudentFeeResponse>()
            .ForMember(
                dest => dest.StudentId,
                opt => opt.MapFrom(
                    src => src.Enrollment.StudentId))
            .ForMember(
                dest => dest.StudentName,
                opt => opt.MapFrom(
                    src => $"{src.Enrollment.Student.User.FirstName} {src.Enrollment.Student.User.LastName}"))
            .ForMember(
                dest => dest.GroupId,
                opt => opt.MapFrom(
                    src => src.Enrollment.GroupId))
            .ForMember(
                dest => dest.GroupName,
                opt => opt.MapFrom(
                    src => src.Enrollment.Group.Name));
    }
}