using AutoMapper;
using EduCore.Application.Features.Attendances.DTOs;
using EduCore.Domain.Entities;

namespace EduCore.Application.Features.Attendances.Mapping;
public class AttendanceProfile : Profile
{
    public AttendanceProfile()
    {
        CreateMap<CreateAttendanceRequest, Attendance>();

        CreateMap<UpdateAttendanceRequest, Attendance>();

        CreateMap<Attendance, AttendanceResponse>()
            .ForMember(
                dest => dest.StudentId,
                opt => opt.MapFrom(src => src.Enrollment.StudentId))
            .ForMember(
                dest => dest.StudentName,
                opt => opt.MapFrom(src =>
                    $"{src.Enrollment.Student.User.FirstName} {src.Enrollment.Student.User.LastName}"))
            .ForMember(
                dest => dest.GroupId,
                opt => opt.MapFrom(src => src.Enrollment.GroupId))
            .ForMember(
                dest => dest.GroupName,
                opt => opt.MapFrom(src => src.Enrollment.Group.Name));

    }
}