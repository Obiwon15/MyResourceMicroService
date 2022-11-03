using AutoMapper;
using MongoDB.Driver;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.DBContexts;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using Resourceedge.Email.Api.Interfaces;
using Resourceedge.Email.Api.Model;
using Resourceedge.Worker.Auth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Services
{
    public class TeamLeadService : ITeamLead
    {
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        public readonly IMongoCollection<TeamLead> Collection;
        public readonly IMongoCollection<KeyResultArea> KraCollection;
        private readonly IEmailSender _sender;

        public TeamLeadService(IDbContext context, IMapper mapper, IServiceFactory serviceFactory, IEmailSender sender)
        {
            Collection = context.Database.GetCollection<TeamLead>($"{nameof(TeamLead)}s");
            KraCollection = context.Database.GetCollection<KeyResultArea>($"{nameof(KeyResultArea)}s");
            _context = context;
            _mapper = mapper;
            _serviceFactory = serviceFactory;
            _sender = sender;
        }

        public async Task<bool> AddNewTeamLead(TeamLeadDtoForCreation entity)
        {
            var employee = await IsValidEmployee(entity.EmployeeId);
            if (employee == null)
                throw new InvalidOperationException("Invalid data sent or Employee does not exist");

            var teamLead = await IsValidEmployee(entity.TeamLeadId);
            if (teamLead == null)
                throw new InvalidOperationException("Invalid data sent or Employee does not exist");

            if (await DoesEmployeeHaveTeadLead(entity.EmployeeId))
                throw new InvalidOperationException("Employee already has a team lead");

            var entityToAdd = _mapper.Map<TeamLead>(entity);
            entityToAdd.FullName = teamLead.FullName;
            entityToAdd.Email = teamLead.Email;

            Collection.InsertOne(entityToAdd);

            await AddReviwerToClaim(entityToAdd);

            return true;

        }

        public async Task<TeamLead> GetTeamLead(int Id)
        {
            if (Id != 0)
            {
                var result = (await Collection.FindAsync(x => x.EmployeeId == Id)).FirstOrDefault();
                return result;
            }
            return null;
        }

        public async Task<List<TeamLead>> GetAllTeamLead()
        {
            var result = Collection.AsQueryable().ToList();
            return await Task.FromResult(result);
        }

        public async Task<List<KeyResultAreaForViewDto>> GetEmployeesWithTheirKRAs(int employeeId)
        {
            var _resultAreaRepo = _serviceFactory.GetServices<IKeyResultArea>();

            var EmployeeKras = _resultAreaRepo.GetPersonalkpis(employeeId);
            var mappedKra = _mapper.Map<IEnumerable<KeyResultAreaForViewDto>>(EmployeeKras);

            return mappedKra.ToList();
        }

        public async Task<EmployeeDetailsDto> GetTeamLeadReporters(int teamLeadId, PaginationResourceParameter pagingParam)
        {
            var result = Collection.AsQueryable().Where(r => r.TeamLeadId == teamLeadId).Select(y => y.EmployeeId);

            var teamRepo = _serviceFactory.GetServices<ITeamRepository>();
            var employeeDetails = (await teamRepo.FetchEmployeesDetailsFromEmployeeService(result.Select(z => z.ToString()))).ToList();

            var detailsToReturn = _mapper.Map<List<emloyeeKraApprovalDto>>(employeeDetails);


            foreach (var item in detailsToReturn)
            {
                if (await IsValidTeamLead(item.EmployeeId, teamLeadId))
                {
                    var employeeKras = (await GetEmployeesWithTheirKRAs(item.EmployeeId)).ToList();
                    if (employeeKras.Any())
                    {
                        item.Status = employeeKras.All(x => x.Approved == null) ? "Pending Approval" : employeeKras.Any(x => x.Approved == false) ? "Rejected" : "Approved";

                    }
                }
                else
                {
                    item.Status = "Awaiting Submission";
                }
            }

            if (!string.IsNullOrWhiteSpace(pagingParam.SearchQuery))
            {
                var searchedResult = detailsToReturn.Where(x => x.FullName.Contains(pagingParam.SearchQuery, StringComparison.CurrentCultureIgnoreCase) || x.Email.Contains(pagingParam.SearchQuery, StringComparison.CurrentCultureIgnoreCase)).AsQueryable();
                return new EmployeeDetailsDto
                {
                    EmployeesWithDetails = PagedList<emloyeeKraApprovalDto>.Create(detailsToReturn.AsQueryable(), pagingParam.PageNumber, pagingParam.PageSize),
                    Pagination = new PaginationResourceResponse(searchedResult.Count(), pagingParam.PageNumber, pagingParam.PageSize)
                };
            }
            return new EmployeeDetailsDto
            {
                EmployeesWithDetails = PagedList<emloyeeKraApprovalDto>.Create(detailsToReturn.AsQueryable(), pagingParam.PageNumber, pagingParam.PageSize),
                Pagination = new PaginationResourceResponse(detailsToReturn.Count(), pagingParam.PageNumber, pagingParam.PageSize)
            };
        }



        public async Task<List<KeyResultAreaForViewDto>> GetEmployeeKRAs(int teamLeadId, int empId)
        {
            if (await (IsValidTeamLead(empId, teamLeadId)))
            {
                var SingleEmployeeWithKRA = (await GetEmployeesWithTheirKRAs(empId)).ToList();
                return SingleEmployeeWithKRA;

            }
            return new List<KeyResultAreaForViewDto>();
        }

        public async Task<bool> TeamLeadAcceptOrRejectEpa(int teamLeadId, int empId, StatusForUpdateDto entity)
        {
            if (!await IsValidTeamLead(empId, teamLeadId))
            {
                throw new InvalidOperationException("You are not the reviewer for this employee");
            }

            IKeyResultArea kraRepo = _serviceFactory.GetServices<IKeyResultArea>();
            await kraRepo.ApproveOrRejectEPA(empId, entity);

            var employee = await kraRepo.GetEmployee(empId);
            var teamLead = await GetTeamLead(teamLeadId);
            var firstName = employee.FullName.Split(" ")[0];

            if (entity.Approve != null && entity.Approve.Value)
            {
                try
                {
                    const string subject = "EPA ACCEPTED";
                    var emailDto = new SingleEmailDto
                    {
                        ReceiverEmailAddress = employee.Email,
                        ReceiverFullName = employee.FullName,
                        HtmlContent = await _sender.FormatEmailEpaAccept(firstName)
                    };

                    var res = await _sender.SendToSingleEmployee(subject, emailDto);
                    return res == HttpStatusCode.Accepted;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                try
                {
                    const string subject = "EPA REJECTED";
                    var teamLeadNameArray = teamLead.FullName.Trim().Split(' ').Select(x => x.Substring(0, 1));
                    var teamLeadInitials = string.Join("", teamLeadNameArray);

                    var emailDto = new SingleEmailDto
                    {
                        ReceiverEmailAddress = employee.Email,
                        ReceiverFullName = employee.FullName,
                        HtmlContent = await _sender.FormatEmailEpaReject(firstName, teamLead.FullName, teamLeadInitials, entity.Comment)
                    };

                    var res = await _sender.SendToSingleEmployee(subject, emailDto);
                    return res == HttpStatusCode.Accepted;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public async Task<bool> SendEmail(OldEmployeeForViewDto employee, TeamLead teamLead, string subject, string msg)
        {

            var emailDto = new SingleEmailDto
            {
                ReceiverEmailAddress = employee.Email,
                ReceiverFullName = employee.FullName,
                HtmlContent = await _sender.FormatEmail(teamLead.FullName, employee.FullName, msg)
            };

            if (emailDto.HtmlContent is null)
            {
                emailDto.HtmlContent = msg;
            }

            var res = await _sender.SendToSingleEmployee(subject, emailDto);
            return res == HttpStatusCode.Accepted;
        }

        public async Task<OldEmployeeForViewDto> IsValidEmployee(int Id)
        {
            ITeamRepository _teamRepo = _serviceFactory.GetServices<ITeamRepository>();
            return await _teamRepo.GetEmployee(Id);
        }

        public async Task<bool> DoesEmployeeHaveTeadLead(int empId)
        {
            return Collection.AsQueryable().Any(x => x.EmployeeId == empId && x.Year == DateTime.Now.Year);
        }

        public async Task<bool> IsValidTeamLead(int empId, int teamLeadId)
        {
            return Collection.AsQueryable().Any(x => x.EmployeeId == empId && x.TeamLeadId == teamLeadId);
        }

        public Dictionary<string, List<Claim>> FormatReviewerForClaims(TeamLead teamlead, Dictionary<string, List<Claim>> superviorsAndClaims)
        {
            if (teamlead.Email != null)
            {
                if (superviorsAndClaims.ContainsKey(teamlead.Email))
                {
                    //Add claims for Supervisor
                    var currentClaims = superviorsAndClaims[teamlead.Email];
                    var exisitingClam = currentClaims.FirstOrDefault(X => X.Value == "reviewer");
                    if (exisitingClam == null)
                    {
                        superviorsAndClaims[teamlead.Email].Add(new Claim("privilege_appraisal", "appraireviewerser"));
                    }
                }
                else
                {
                    superviorsAndClaims[teamlead.Email] = new List<Claim> { new Claim("privilege_appraisal", "reviewer") };
                }
            }

            return superviorsAndClaims;
        }

        public async Task AddReviwerToClaim(TeamLead entityToAdd)
        {

            var superviorsAndClaims = new Dictionary<string, List<Claim>>();

            superviorsAndClaims = FormatReviewerForClaims(entityToAdd, superviorsAndClaims);

            AuthService authService = _serviceFactory.GetServices<AuthService>();
            await authService.AddUserClaimsByEmail(superviorsAndClaims);
        }
    }
}
