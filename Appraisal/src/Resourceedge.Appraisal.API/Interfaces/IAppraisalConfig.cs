using System.Collections;
using Microsoft.AspNetCore.JsonPatch;
using MongoDB.Bson;
using Resourceedge.Appraisal.API.ResourceParamters;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Interfaces
{
    public interface IAppraisalConfig
    {
        public Task<IEnumerable<AppraisalConfig>> Get(AppraisalConfigParameters param);
        public bool Insert(AppraisalConfig entity);
        public Task<AppraisalConfig> Update(ObjectId Id, AppraisalCycle entity);
        public AppraisalCycleForAppraisal GetActiveCycle();
        public bool ActivateCycle(string cycleId);
        public Task<bool> EnableOrDisableAppraisal(ObjectId id);
        public Task<bool> ArchiveAppraisal(ObjectId id);
        Task<IEnumerable<ConfigAppraisalViewDto>> GetReviews(AppraisalConfigParameters param);
        string InsertReview(ConfigAppraisalDto appraisalDto);
        public void ActivateAppraisals(object state);
        public void DeActivateAppraisals(object state);
        AppraisalConfig UpdateAppraisalReview(string Id, JsonPatchDocument<AppraisalConfigForUpdateDto> model);
        bool InsertReviewType(string name);
        Task<ReviewType> GetReviewType(ObjectId Id);
        Task<List<ReviewType>> GetReviewTypes();
        Task<ConfigAppraisalWithParticipantDetail> GetSingleReview(string Id);
        Task<ConfigAppraisalViewDto> GetSingleReviewWithoutParticipant(ObjectId Id);
        Task<IEnumerable<ConfigAppraisalViewDto>> GetEmployeeReviews(int employeeId, int? year);

    }
}
