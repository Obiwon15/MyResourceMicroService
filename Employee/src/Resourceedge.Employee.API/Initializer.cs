using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Resourceedge.Common.Archive;
using Resourceedge.Employee.Domain.DbContext;
using Resourceedge.Employee.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Resourceedge.Employee.API
{
    public class Initializer
    {

        public static void SeedEmployeeDb(IApplicationBuilder service)
        {
            var dbContext = service.ApplicationServices.GetRequiredService(typeof(IDbContext)) as IDbContext;
            if (dbContext != null)
            {
                var collection = dbContext.Database.GetCollection<Resourceedge.Common.Archive.OldEmployee>($"{nameof(OldEmployee)}s");
                if (!collection.AsQueryable().Any())
                {
                    var jsonString = File.ReadAllText("Employees.json");
                    var ParsedJson = JsonSerializer.Deserialize<IEnumerable<OldEmployee>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    collection.InsertMany(ParsedJson);
                }

                var biodataCollection = dbContext.Database.GetCollection<EmployeeBioData>("Employees");
                if (!biodataCollection.AsQueryable().Any())
                {
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<OldEmployee, EmployeeBioData>()
                                    .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.EmployeeId))
                                    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmpEmail))
                                    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                                    .ForMember(dest => dest.StaffId, opt => opt.MapFrom(src => src.EmpStaffId))
                                    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.OfficeNumber))
                                    .ForMember(dest => dest.Company, opt => opt.MapFrom(src => src.Subgroup.Name)));

                    var mapper = new Mapper(config);

                    var ParsedJson = collection.AsQueryable();
                    var biodata = mapper.Map<IEnumerable<EmployeeBioData>>(ParsedJson);

                    biodataCollection.InsertMany(biodata);
                }

                var lastEmployeeCollection = dbContext.Database.GetCollection<LastEmployeeNumber>("LastEmployeeNumber");
                if (!lastEmployeeCollection.AsQueryable().Any())
                {
                    var employeeId = new LastEmployeeNumber { EmployeeId = 328 };

                    lastEmployeeCollection.InsertOne(employeeId);
                }
            }


        }
    }
}
