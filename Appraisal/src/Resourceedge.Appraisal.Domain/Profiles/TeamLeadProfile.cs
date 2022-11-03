using AutoMapper;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Archive;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Profiles
{
    public class TeamLeadProfile : Profile
    {

        public TeamLeadProfile()
        {
            CreateMap<TeamLead, TeamLeadDtoForCreation>();
            CreateMap<TeamLeadDtoForCreation, TeamLead>();

            CreateMap<OldEmployeeDto, emloyeeKraApprovalDto> ()
                .ForMember(dest => dest.SubGroup, opt => opt.MapFrom(src => src.Subgroup.Name));

        }

    }
}
