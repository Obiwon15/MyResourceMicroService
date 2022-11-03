using AutoMapper;
using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using System;

namespace Resourceedge.Appraisal.Domain.Profiles
{
    public class AppraisalConfigProfile : Profile
    {
        public AppraisalConfigProfile()
        {
            CreateMap<AppraisalConfigForCreationDto, AppraisalConfig>()
                .ForMember(dest => dest.TotalCycle, to => to.MapFrom(src => src.Total));
            CreateMap<AppraisalConfig, AppraisalConfigForCreationDto>()
                .ForMember(dest => dest.Total, to => to.MapFrom(src => src.TotalCycle));
            CreateMap<AppraisalConfig, AppraisalCongifurationForViewDto>()
                .ForMember(dest => dest.Total, to => to.MapFrom(src => src.TotalCycle))
                .ForMember(dest => dest.ConfigId, to => to.MapFrom(src => src.Id));

            CreateMap<ConfigAppraisalDto, AppraisalConfig>()
                .ForMember(des => des.CreateAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(des => des.Year, opt => opt.MapFrom(src => DateTime.UtcNow.Year))
                .ForMember(des => des.Completed, opt => opt.AllowNull());

            CreateMap<AppraisalConfig, ConfigAppraisalViewDto>();
            CreateMap< ConfigAppraisalViewDto, AppraisalConfig>();
            CreateMap<AppraisalCycle, ConfigAppraisalViewDto>();

            CreateMap<AppraisalConfigForUpdateDto, AppraisalConfig>()
                .ForMember(des => des.Id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<AppraisalConfig, AppraisalConfigForUpdateDto>()
                .ForMember(des => des.Id, opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<ReviewTypeDto, ReviewType>()
                .ForMember(des => des.Id, opt => opt.MapFrom(src => ObjectId.Parse(src.Id)));
            CreateMap<ReviewType, ReviewTypeDto>()
                .ForMember(des => des.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }

    public class AppraisalCycleClassProfile : Profile
    {
        public AppraisalCycleClassProfile()
        {

            CreateMap<AppraisalCycleClass, AppraisalCycle>().ForMember(dest => dest.StartDate, to => to.MapFrom(src => src.Start))
                .ForMember(dest => dest.StopDate, to => to.MapFrom(src => src.Stop));

            CreateMap<AppraisalCycle, AppraisalCycleClass>().ForMember(dest => dest.Start, to => to.MapFrom(src => src.StartDate))
                  .ForMember(dest => dest.Stop, to => to.MapFrom(src => src.StartDate));
        }
    }
}
