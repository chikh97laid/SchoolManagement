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
                opt => opt.MapFrom(
                    src => src.Enrollment.StudentId));
    }
}