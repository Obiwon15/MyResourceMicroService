using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Resourceedge.Appraisal.API.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Controllers.V2
{
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController, ApiVersion("2")]
    public class AppraisalReportController : ControllerBase
    {
        private readonly IServiceFactory _serviceFactory;
        private readonly IAppraisalResult _appraisalResult;
        private readonly IAppraisalFinalResult _finalResultRepo;
        private readonly IAppraisalConfig _appraisalConfigRepo;
        private readonly IWebHostEnvironment hostingEnvironment;

        public AppraisalReportController(IServiceFactory serviceFactory, IWebHostEnvironment _hostingEnvironment)
        {
            _serviceFactory = serviceFactory;
            hostingEnvironment = _hostingEnvironment;
            _appraisalResult = serviceFactory.GetServices<IAppraisalResult>();
            _finalResultRepo = serviceFactory.GetServices<IAppraisalFinalResult>();
            _appraisalConfigRepo = serviceFactory.GetServices<IAppraisalConfig>();
        }

        [HttpGet("DownloadResult")]
        public async Task<IActionResult> DownloadResult(string reviewId)
        {
            var configDetails = await _appraisalConfigRepo.GetSingleReview(reviewId);
            if (configDetails == null)
            {
                return NotFound(new { message = "Appraisal configuration not found" });
            }


            string sWebRootFolder = $"{hostingEnvironment.ContentRootPath}/AppraisalResult";
            string sFileName = @$"Result_For_{configDetails.configAppraisal.Name}.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            var memory = new MemoryStream();
            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
                await _finalResultRepo.GenerateExcelForAppraisalResult(fs, ObjectId.Parse(reviewId), configDetails.configAppraisal.Year.ToString());
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
