using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.Queries;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Resourceedge.Appraisal.API.Controllers
{
    [Authorize]
    [ApiController, ApiVersion("1")]
    [Route("api/v{version:apiVersion}/finalResult")]
    public class FinalResultController : ControllerBase
    {
        private readonly IAppraisalFinalResult finalResultRepo;
        private readonly IMapper mapper;
        private readonly IAppraisalResult appraisalResult;
        private readonly IWebHostEnvironment hostingEnvironment;

        public FinalResultController(IAppraisalFinalResult _finalResult, IMapper _mapper, IAppraisalResult _appraisalResult, IWebHostEnvironment _hostingEnvironment)
        {
            finalResultRepo = _finalResult;
            mapper = _mapper;
            appraisalResult = _appraisalResult;
            hostingEnvironment = _hostingEnvironment;
        }

        [HttpGet("{cycleId}/{empId}")]
        public IActionResult GetEmployeeResult(string cycleId, int empId)
        {
            ObjectId CycleId = new ObjectId(cycleId);
            var result = finalResultRepo.GetFinalAppraisalResult(empId, CycleId);

            if (result != null)
            {
                return Ok(result);
            }

            return NoContent();
        }

        [HttpGet("{cycleId}")]
        public async Task<IActionResult> AllAppraisalResult([FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }

            var result = await finalResultRepo.GetAllResultByCycle(ObjectId.Parse(configParam.Cycle));
            if (result != null)
            {
                return Ok(result);
            }

            return NoContent();
        }

        [HttpGet("~/api/Report")]
        public async Task<IActionResult> AllAppraisalResultByLocation([FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }

            var result = await finalResultRepo.GetAllResultByCycle(ObjectId.Parse(configParam.Cycle), configDetails.Year.ToString());
            if (result != null)
            {
                return Ok(result);
            }

            return NoContent();
        }

        [HttpGet("~/api/Report/{group}/{pageNumber}/{pageSize}")]
        public async Task<IActionResult> AppraisalResultByGroup(string group, int pageNumber, int pageSize, [FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }

            var result = await finalResultRepo.GetAppraisalResultByGroup(group, pageNumber, pageSize, ObjectId.Parse(configParam.Cycle), configDetails.Year.ToString());
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();
        }

        [HttpGet("~/api/Report/Organisation/Count")]
        public async Task<IActionResult> GetOrganisation([FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }

            var result = await finalResultRepo.GetOrgaization(ObjectId.Parse(configParam.Cycle));
            if (result != null)
            {
                return Ok(result);
            }
            return NoContent();
        }

        [HttpGet("recalculate")]
        public async Task<IActionResult> ReCalculateAppraisal([FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }

            var result = await finalResultRepo.ReCalculateFinalAppraisalResult(ObjectId.Parse(configParam.Cycle));
            if (result)
            {
                return Ok(result);
            }
            return NoContent();
        }

        [HttpGet("DownloadResult")]
        public async Task<IActionResult> DownloadResult([FromQuery] AppraisalQueryParam configParam)
        {
            var configDetails = await appraisalResult.GetAppraisalConfiguration(configParam.Config);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }


            string sWebRootFolder = $"{hostingEnvironment.ContentRootPath}/AppraisalResult";
            string sFileName = @$"Result_For_{configDetails.Name} {configDetails.Cycles.Where(x => x.Id == ObjectId.Parse(configParam.Cycle)).Select(x => x.Name).ToString()}.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            var memory = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
               await finalResultRepo.GenerateExcelForAppraisalResult(fs, ObjectId.Parse(configParam.Cycle), configDetails.Year.ToString());
            }

            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
        }
    }

}

