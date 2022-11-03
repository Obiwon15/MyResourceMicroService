using MongoDB.Bson;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Appraisal.Domain.Queries;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Models;
using Resourceedge.Common.Util;
using Resourceedge.Email.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.JsonPatch;

namespace Resourceedge.Appraisal.API.Interfaces
{
    public interface IKeyResultArea : IGenericRepository<KeyResultArea>
    {

        Task<KeyResultArea> QuerySingleByUserId(ObjectId id, int UserId);
        Task<KeyResultArea> QuerySingleUserKeyOutcome(ObjectId id, ObjectId outcomeId, int empId);
        void AddKeyOutcomes(KeyResultArea entity);
         Task<bool> AddKeyOutcomes(IEnumerable<KeyResultArea> entity);
        public IEnumerable<KeyResultArea> GetPersonalkpis(int empId, string resultArea = null);
        public KeyResultArea Update(ObjectId Id, KeyResultAreaForUpdateMainDto entity);
        Task<long> UpdateKeyOutcome(ObjectId Id, ObjectId outcomeId, int empId, KeyOutcomeForUpdateDto entity);
        Task<KeyResultArea> HodApproval(int empId, int memberId, ObjectId keyResultAreaId, string whoami, StatusForUpdateDto entity);
        Task<long> EmployeeApproval(int empId, StatusForUpdateDto entity);
        IEnumerable<KeyResultArea> GetKeyResultAreasForAppraiser(int appraiserId, int employeeId);
        void SendApprovalNotification(IEnumerable<KeyResultArea> keyAreas);
        Task<OldEmployeeForViewDto> GetEmployee(int empId);
        bool HasUploadedEpa(int employeeId);
        Task<KeyResultArea> DeleteKeyOutcome(ObjectId id, KeyResultArea entity);
        Task<KeyResultArea> QuerySingleByKeyOutcome(ObjectId id);
        //IEnumerable<KeyResultArea> GetAcceptedAppraisal(int userId, AppraisalQueryParam configParam, string resultId = null);

        //To be deleted later
        //Task<IEnumerable<NameEmailWithType>> GetAllSupervisorsForClaims();
        Task<bool> ChangeSupervisor(int empId, int newSupervisorId, ObjectId keyResultAreaId, string whotoUpdate);
        string CheckForSupervisor(int empId, int oldSupervisorId, ObjectId keyResultAreaId);
        Task DeleteEpa(int empId);
        Task<bool> ChangeSupervisor(int empId, ChangeSupervisorDto supervisorDto);
        Task<bool> RestoreEPA(int empId, AppraisalQueryParam appraisalQuery);
        Task ApproveEpa(int empId, KeyResultArea kra, StatusForUpdateDto status);
        Task<bool> CreateKeyResultArea(string empId, IEnumerable<KeyResultAreaDtoForCreation> model);
        Task<(KeyResultAreaForViewDto, bool)> UpdateKeyResultArea(int empId, string KeyResultAreaId,
            JsonPatchDocument<KeyResultAreaForUpdateDto> entityForUpdate);
        Task ApproveOrRejectEPA(int empId, StatusForUpdateDto status);
        Task<EmployeesEPAWithPagination> GetAllUploadedEPA(int? year, PaginationResourceParameter pagination);
    }
}
