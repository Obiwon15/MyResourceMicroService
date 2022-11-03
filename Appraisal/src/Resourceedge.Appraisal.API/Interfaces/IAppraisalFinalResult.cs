using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Interfaces
{
    public interface IAppraisalFinalResult
    {
        Task CalculateResult(int empId, ObjectId cycleId);
        Task<IEnumerable<FinalAppraisalResultForViewDto>> GetAppraisalResultByGroup(string group, int pageNumber, int pageSize, ObjectId cycleId, string year);
        Task<IEnumerable<OrgaizationandCount>> GetOrgaization(ObjectId CycleId);
        Task<bool> ReCalculateFinalAppraisalResult(ObjectId cycleId);
        Task<IDictionary<string, IEnumerable<FinalAppraisalResultForViewDto>>> GetResultForDownload(ObjectId cycleId, string year);
        Task<bool> ResetEmployeeFinalAppraisal(int empId, ObjectId cycleId);
        Task<FinalResultDtoForView> GetFinalAppraisalResult(int employeeId, ObjectId reviewId);
        FinalAppraisalResult CalculateNormalAnnualAppraisal(List<AppraisalResult> result);
        Task<IEnumerable<AppraisalsForReviewDto>> FilterEmployeeParticipants(ObjectId cycleId, Participants participants);
        Task<FileStream> GenerateExcelForAppraisalResult(FileStream fs, ObjectId reviewId, string year);
        Task<IEnumerable<FinalAppraisalResultForViewDto>> GetAllResultByCycle(ObjectId cycleId, string year);
        Task<IEnumerable<FinalAppraisalResultForViewDto>> GetAllResultByCycle(ObjectId cycleId);
        //Task<IDictionary<string, IEnumerable<FinalAppraisalResultForViewDto>>> GetResultForDownload(ObjectId cycleId);
       // Task<FileStream> GenerateExcelForAppraisalResult(FileStream fs, ObjectId reviewId);
    }
}
