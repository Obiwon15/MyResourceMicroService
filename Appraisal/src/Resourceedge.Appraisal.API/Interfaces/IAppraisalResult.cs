using MongoDB.Bson;
using MongoDB.Driver;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Appraisal.Domain.Queries;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Interfaces
{
    public interface IAppraisalResult
    {
        IEnumerable<AppraisalResult> Get(ObjectId AppraisalConfigId, ObjectId CycleId, int? EmployeeId);
        void InsertResult(AppraisalResult entity);
        Task<string> SubmitAppraisal(int empId, ObjectId reviewId, IEnumerable<AppraisalPerforanceForCreationDto> entities);
        Task EmployeeAcceptOrReject(int employeeId, ObjectId appraisalResultId, AcceptanceStatus status);
        Task<bool> HodApprovalOrReject(OldEmployeeForViewDto Hod, OldEmployeeForViewDto employee, HodApprovalDto approvalDto, ObjectId reviewId);
        Task<IEnumerable<AppraisalForApprovalDto>> GetEmployeesToAppraise(int employeeId, string appraisalConfigurationId, string appraisalCycleId, string whoami);
        Task<bool> HasPaticipatedInAppraisal(int employeeId, ObjectId reviewId);
        Task<bool> CheckAppraisalConfigurationDetails(ObjectId reviewId);
       // Task<bool> CheckMultipleAppraisalConfigurationDetails(IEnumerable<AppraisalQueryParam> model);
        Task<string> AppraiseEmployee(int empId, ObjectId reviewId, IEnumerable<AppraisalPerforanceForCreationDto> entities);
        IEnumerable<KeyResultArea> GetAcceptedKRAForAppraisal(int userId, AppraisalConfig configParam, string resultId = null);
        Task<AppraisalConfig> GetAppraisalConfiguration(string configid);
        IEnumerable<KeyResultArea> GetOnlyApplicableKeyoutcomesForAppraisal(ObjectId kraId, int EmployeeId, IList<string> keyoutcomeIds);
        Task<bool> UpdateKeyResultAreaForExistingResult(string cycleId);
        Task UpdateAppraisalResult(AppraisalResult appraisalResult);
        Task<bool> RestAppraisal(int empId, int appraiserId, ObjectId cycleId);
        Task<bool> ResetEmployeeAppraisal(int empId, ObjectId cycleId);
        Task<bool> IsAnyAppriasalResultRejected(int EmployeeId, ObjectId CycleId);

        Task<IEnumerable<AppraisalResult>> Get(string reviewId, int EmployeeId);
        Task<IEnumerable<AppraisalPerforanceForCreationDto>> ValidateSubmission(int empId, string reviewId, IEnumerable<AppraisalPerformanceDto> appraisalForCreation);
        Task<IEnumerable<KeyResultArea>> GetAcceptedKRAForAppraisal(AppraisalPerformanceParam param);
        Task<IEnumerable<AppraisalForApprovalDto>> GetEmployeesToAppraise(int employeeId, string appraisalCycleId, string whoAmI);
        Task<string> ApproveAppraisalHod(int hodId, AppraisalPerformanceParam performanceParam, HodApprovalDto approvalDtos);

        Task<AppraisalReport> GetEmployeesParticipatingInAppraisal(ObjectId cycleId, PaginationResourceParameter param);
        Task<DirectReportDto> SearchEmployeeToAppraise(int employeeId, string appraisalCycleId, string whoAmI, PaginationResourceParameter pagingParam);
        Task<bool> CheckToCalculateHodResult(AppraisalPerformanceParam performanceParam);
        ArrayList GetEmployeeReviews(int employeeId);
        Task<List<AppraisalResultForViewDto>> GetRejectedEmployeeAppraisal(int EmployeeId, ObjectId CycleId);
    }
}
