using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Controllers.V2
{
    [Authorize]
    [ApiController, ApiVersion("2")]
    [Route("api/v{version:apiVersion}/[controller]/{empId:int}")]
    public class ResultAreaController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IKeyResultArea _resultArea;

        public ResultAreaController(IMapper mapper, IKeyResultArea resultArea)
        {
            _resultArea = resultArea;
            _mapper = mapper;
        }

        [HttpGet("~/api/v{version:apiVersion}/EPA/report")]
        public async Task<IActionResult> EmployeesEPAForHR(int? year, [FromQuery] PaginationResourceParameter param)
        {
            var data = await _resultArea.GetAllUploadedEPA(year, param);
            return Ok(data);
        }


    }
}
