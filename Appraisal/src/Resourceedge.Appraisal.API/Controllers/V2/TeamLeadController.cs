using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Util;

namespace Resourceedge.Appraisal.API.Controllers.V2
{
    [Authorize]
    [ApiController, ApiVersion("2")]
    [Route("api/v{version:apiVersion}/TeamLead")]
    public class TeamLeadController : BaseController
    {
        private readonly ITeamLead _teamLead;

        public TeamLeadController(IMapper mapper, ITeamLead teamLead)
        {
            _teamLead = teamLead;
        }

        [HttpGet("myteadlead")]
        [ProducesResponseType(typeof(TeamLead), 200)]
        public async Task<IActionResult> GetTeamLead(int empId)
        {
            var data = await _teamLead.GetTeamLead(empId);
            return Ok(data);
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateTeamLead(TeamLeadDtoForCreation model)
        {
            var result = await _teamLead.AddNewTeamLead(model);
            return Ok(result);
        }

        [HttpGet("")]
        [ProducesResponseType(typeof(IEnumerable<TeamLead>), 200)]
        public async Task<IActionResult> TeamLeads()
        {
            var data = await _teamLead.GetAllTeamLead();
            return Ok(data);
        }

        [HttpGet("members/{teamLeadId}")]
        [ProducesResponseType(typeof(IEnumerable<EmployeeDetailsDto>), 200)]
        public async Task<IActionResult> GetTeamLeadReports(int teamLeadId, [FromQuery]PaginationResourceParameter pagingParam)
        {
            var data = await _teamLead.GetTeamLeadReporters(teamLeadId, pagingParam);
            return Ok(data);
        }

        [HttpGet("epa/{teamLeadId}")]
        public async Task<IActionResult> GetTeamLeadReportWithEPAs(int teamLeadId, int empId)
        {
            var data = await _teamLead.GetEmployeeKRAs(teamLeadId, empId);
            return Ok(data);
        }

        [HttpPost("Approve")]
        public async Task<IActionResult> TeamLeadApprove(int teamLeadId, int empId, [FromBody]StatusForUpdateDto entity)
        {
            if (entity == null)
                return BadRequest();

            var result = await _teamLead.TeamLeadAcceptOrRejectEpa(teamLeadId, empId, entity);
            return Ok(result);
        }



    }
}
