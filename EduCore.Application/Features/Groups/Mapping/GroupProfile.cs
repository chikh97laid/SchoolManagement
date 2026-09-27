using AutoMapper;
using EduCore.Application.Features.Groups.DTOs;
using EduCore.Domain.Entities;

namespace EduCore.Application.Features.Groups.Mapping;

public class GroupProfile : Profile
{
    public GroupProfile()
    {
        CreateMap<CreateGroupRequest, Group>();

        CreateMap<UpdateGroupRequest, Group>();

        CreateMap<Group, GroupResponse>()
            .ForMember(
                dest => dest.TeacherName,
                opt => opt.MapFrom(
                    src => src.Teacher.User.FirstName + " " + src.Teacher.User.LastName));
    }
}