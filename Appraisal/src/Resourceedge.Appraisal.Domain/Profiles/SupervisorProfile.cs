using AutoMapper;
using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Profiles
{
    public class SupervisorProfile : Profile
    {
        public SupervisorProfile()
        {
            CreateMap<ChangeSupervisorDto, ChangeSupervisorDtoForView>()
                .ForMember(des => des.keyresultareaid, opt => opt.MapFrom(src => src.KeyResultAreaId.ToString()));
            CreateMap<ChangeSupervisorDtoForView, ChangeSupervisorDto>()
                .ForMember(des => des.KeyResultAreaId, opt => opt.MapFrom(src => ObjectId.Parse(src.keyresultareaid)));
        }
    }
}
