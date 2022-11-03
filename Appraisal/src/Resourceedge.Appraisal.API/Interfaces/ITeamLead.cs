using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Interfaces
{
    public interface ITeamLead
    {

        Task<bool> AddNewTeamLead(TeamLeadDtoForCreation entity);
        Task<TeamLead> GetTeamLead(int Id);
        Task<List<TeamLead>> GetAllTeamLead();
        Task<EmployeeDetailsDto> GetTeamLeadReporters(int empId, PaginationResourceParameter pagingParam);
        Task<bool> TeamLeadAcceptOrRejectEpa(int teamLeadId, int empId, StatusForUpdateDto entity);
        Task<List<KeyResultAreaForViewDto>> GetEmployeeKRAs(int empId, int teamLeadId);
        Task<List<KeyResultAreaForViewDto>> GetEmployeesWithTheirKRAs(int empId);
    }
}
