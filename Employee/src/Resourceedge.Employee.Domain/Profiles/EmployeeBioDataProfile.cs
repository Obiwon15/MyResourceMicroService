using AutoMapper;
using MongoDB.Bson;
using Resourceedge.Common.Archive;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Resourceedge.Employee.Domain.Profiles
{
    public class EmployeeBioDataProfile : Profile
    {
        public EmployeeBioDataProfile()
        {
            CreateMap<EmployeeBioData, EmployeeBioDataDto>()
               
                .ForMember(dest => dest.OfficalEmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.WorkPhone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.CreatedAt, to => to.Ignore());
            CreateMap<EmployeeBioDataDto, EmployeeBioData>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.WorkPhone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.OfficalEmail));

            CreateMap<EmployeeCreateDto, EmployeeBioData>();
        }
    }
}
