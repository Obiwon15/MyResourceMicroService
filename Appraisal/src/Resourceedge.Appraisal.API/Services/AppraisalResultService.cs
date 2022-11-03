using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using Resourceedge.Appraisal.API.DBQueries;
using Resourceedge.Appraisal.API.Helpers;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.DBContexts;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using Resourceedge.Email.Api.Interfaces;
using Resourceedge.Email.Api.Model;
using Resourceedge.Email.Api.SGridClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Services
{
    public class AppraisalResultService : IAppraisalResult
    {
        public readonly IMongoCollection<AppraisalResult> Collection;
        public readonly IMongoCollection<KeyResultArea> KraCollection;
        public readonly IMongoCollection<AppraisalConfig> AppraisalConfigCollection;
        private readonly IDbContext _context;
        private readonly IMapper _mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IEmailSender sender;
        private readonly IAppraisalConfig _appraisalConfig;

        public AppraisalResultService(IDbContext context, IMapper mapper, ISGClient _client, IEmailSender _sender, IServiceFactory serviceFactory, IAppraisalConfig appraisalConfig)
        {
            Collection = context.Database.GetCollection<AppraisalResult>($"{nameof(AppraisalResult)}s");
            KraCollection = context.Database.GetCollection<KeyResultArea>($"{nameof(KeyResultArea)}s");
            _context = context;
            _mapper = mapper;
            sender = _sender;
            AppraisalConfigCollection = _context.Database.GetCollection<AppraisalConfig>($"{nameof(AppraisalConfig)}s");
            _serviceFactory = serviceFactory;
            _appraisalConfig = appraisalConfig;
        }

        public IEnumerable<AppraisalResult> Get(ObjectId AppraisalConfigId, ObjectId CycleId, int? EmployeeId)
        {
            var configDetails = AppraisalConfigCollection.AsQueryable().FirstOrDefault(x => x.Id == AppraisalConfigId && x.Cycles.Any(x => x.Id == CycleId));
            if (configDetails is null) { return new List<AppraisalResult>(); }

            var result = Collection.AsQueryable().Where(a => a.myId == EmployeeId && a.AppraisalConfigId == AppraisalConfigId && a.AppraisalCycleId == CycleId);
            return result.ToList();
        }

        public async Task<IEnumerable<AppraisalResult>> Get(string reviewId, int employeeId)
        {
            if (employeeId == null || reviewId == null)
            {
                throw new InvalidOperationException("Invalid request, Employee or Review Id cannot be null");
            }

            ObjectId cycleId = new ObjectId(reviewId);
            AppraisalConfig configDetails = AppraisalConfigCollection.AsQueryable().FirstOrDefault(x => x.Id == cycleId);
            if (configDetails is null)
            {
                configDetails = AppraisalConfigCollection.AsQueryable().Where(x => x.Cycles.Any(x => x.Id == cycleId)).FirstOrDefault();
                if (configDetails is null)
                    return new List<AppraisalResult>();
            }

            var result = Collection.AsQueryable().Where(a => a.myId == employeeId && a.AppraisalCycleId == cycleId);
            return result.ToList();
        }


        public void InsertResult(AppraisalResult entity)
        {
            Collection.InsertOne(entity);
        }

        public async Task<IEnumerable<AppraisalPerforanceForCreationDto>> ValidateSubmission(int empId, string reviewId, IEnumerable<AppraisalPerformanceDto> appraisalForCreation)
        {
            if (!appraisalForCreation.Any())
            {
                throw new InvalidOperationException("No Result Sent");
            }

            ObjectId appraisalReview = ObjectId.Parse(reviewId);
            var configExist = await CheckAppraisalConfigurationDetails(appraisalReview);
            if (!configExist)
            {
                throw new InvalidOperationException("Invalid Configuration details");
            }

            if (!appraisalForCreation.Any(x => x.whoami.Equals("Appraiser", StringComparison.CurrentCultureIgnoreCase)))
            {
                bool hasDoneAppraisal = await HasPaticipatedInAppraisal(empId, appraisalReview);
                if (hasDoneAppraisal)
                {
                    throw new InvalidOperationException("You have already participated in this appraisal");
                }
            }

            return _mapper.Map<IEnumerable<AppraisalPerforanceForCreationDto>>(appraisalForCreation);
        }

        public async Task<string> SubmitAppraisal(int empId, ObjectId reviewId, IEnumerable<AppraisalPerforanceForCreationDto> entities)
        {
            var keyResultAreaService = _serviceFactory.GetServices<IKeyResultArea>();
            var employee = await keyResultAreaService.GetEmployee(empId);
            var employeeNameArray = employee.FullName.Trim().Split(' ').Select(x => x.Substring(0, 1));
            var employeeInitials = string.Join("", employeeNameArray);

            string subject = "";
            SingleEmailDto email = new SingleEmailDto();
            List<SingleEmailDto> emailDto = new List<SingleEmailDto>();

            if (employee != null)
            {
                subject = $"{employee.FullName} has performed Appraisal ";

                try
                {
                    foreach (var entity in entities)
                    {
                        var keyResultArea = GetOnlyApplicableKeyoutcomesForAppraisal(entity.KeyResultAreaId, empId, entity.KeyOutcomeScore.Select(x => x.KeyOutcomeId.ToString()).ToList()).FirstOrDefault();
                        if (keyResultArea != null)
                        {
                            if (String.IsNullOrWhiteSpace(entity.whoami))
                            {
                                var myAppraisal = _mapper.Map<AppraisalResult>(entity);
                                myAppraisal.AppraisalCycleId = reviewId;
                                myAppraisal.NextAppraisee = "Appraiser";
                                myAppraisal.KeyResultArea = keyResultArea;

                                if (!myAppraisal.KeyOutcomeScore.All(x => x.EmployeeScore == null))
                                {
                                    var average = myAppraisal.KeyOutcomeScore.Where(x => x.EmployeeScore != null).Average(x => x.EmployeeScore.Value);
                                    myAppraisal.EmployeeCalculation.ScoreTotal = myAppraisal.KeyOutcomeScore.Where(x => x.EmployeeScore != null).Sum(x => x.EmployeeScore).Value;
                                    myAppraisal.EmployeeCalculation.Average = average;
                                    myAppraisal.EmployeeCalculation.WeightContribution = (average * (Convert.ToDouble(myAppraisal.KeyResultArea.Weight)) / 100);
                                }

                                InsertResult(myAppraisal);

                                email.ReceiverFullName = keyResultArea.AppraiserDetails.Name;
                                email.ReceiverEmailAddress = keyResultArea.AppraiserDetails.Email;
                                email.HtmlContent = await sender.FormatEmail(employee.FullName, keyResultArea.AppraiserDetails.Name, employeeInitials, reviewId.ToString());

                                if (email.HtmlContent == null)
                                {
                                    email.HtmlContent = @$"<b>Dear {myAppraisal.KeyResultArea.AppraiserDetails.Name},</b> <br /> <p>{employee.FullName} has successfully participated in this quarter appraisal, Please attend to it. Kindly login to the portal to view. <br /><br /> Thank you.</p>";
                                }
                                emailDto.Add(email);
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Oops something went wrong, {ex.Message}");
                }
                finally
                {
                    var _finalResultRepo = _serviceFactory.GetServices<IAppraisalFinalResult>();
                    await _finalResultRepo.CalculateResult(empId, reviewId);
                    var emailDtos = AppraisalResultExtension.FormatEmailForAppraisal(emailDto);

                    if (emailDtos.Any())
                    {
                        emailDtos.ForEach(async e => await sender.SendToSingleEmployee(subject, e));
                    }
                }
                return "Appraisal Submitted Successfully";
            }
            return "Oops Something went wrong, could not submit appraisal";
        }

        public async Task<string> AppraiseEmployee(int empId, ObjectId reviewId, IEnumerable<AppraisalPerforanceForCreationDto> entities)
        {
            string subject = "";
            List<SingleEmailDto> emailDtos = new List<SingleEmailDto>();
            var keyResultAreaService = _serviceFactory.GetServices<IKeyResultArea>();
            var employee = await keyResultAreaService.GetEmployee(entities.FirstOrDefault().myId);
            var reviewName = (await _appraisalConfig.GetSingleReviewWithoutParticipant(reviewId)).Name;

            if (employee != null)
            {
                try
                {
                    foreach (var entity in entities)
                    {
                        var filter = Builders<AppraisalResult>.Filter.Where(a => a.myId == entity.myId && a.AppraisalCycleId == reviewId
                                                                            && a.KeyResultArea.Id == entity.KeyResultAreaId);
                        var result = Collection.Find(filter).FirstOrDefault();

                        if (result != null)
                        {

                            if (entity.whoami.ToUpper() == "APPRAISER")
                            {
                                if (result.KeyResultArea.AppraiserDetails.EmployeeId != empId)
                                    continue;
                                if (result.NextAppraisee.ToLower() == "hod")
                                    continue;

                                foreach (var item in entity.KeyOutcomeScore)
                                {
                                    if (result.KeyOutcomeScore.Any(a => a.KeyOutcomeId == item.KeyOutcomeId))
                                    {
                                        foreach (var item1 in result.KeyOutcomeScore)
                                        {
                                            if (item.KeyOutcomeId == item1.KeyOutcomeId)
                                            {
                                                result.KeyOutcomeScore.FirstOrDefault(x => x.KeyOutcomeId == item1.KeyOutcomeId).AppraisalScore = item.EmployeeScore;
                                            }
                                        }
                                    }
                                }
                                result.IsAccepted = true;
                                result.NextAppraisee = "Hod";

                                if (!result.KeyOutcomeScore.All(x => x.AppraisalScore == null))
                                {
                                    result.AppraiseeCalculation.ScoreTotal = result.KeyOutcomeScore.Where(x => x.AppraisalScore != null).Sum(x => x.AppraisalScore.Value);
                                    var average = result.KeyOutcomeScore.Where(x => x.AppraisalScore != null).Average(x => x.AppraisalScore.Value);
                                    result.AppraiseeCalculation.Average = average;
                                    result.AppraiseeCalculation.WeightContribution = (average * (Convert.ToDouble(result.KeyResultArea.Weight) / 100));
                                    result.AppraiseeFeedBack = entity.AppraiseeFeedBack;

                                    if (result.KeyResultArea.HodDetails.EmployeeId == result.KeyResultArea.AppraiserDetails.EmployeeId)
                                    {
                                        result.HodAccept.IsAccepted = true;
                                        result = result.HodApproval("");

                                        result.FinalCalculation.ScoreTotal = result.KeyOutcomeScore.Where(x => x.AppraisalScore != null).Sum(x => x.AppraisalScore.Value);
                                        result.FinalCalculation.Average = average;
                                        result.FinalCalculation.WeightContribution = (average * (Convert.ToDouble(result.KeyResultArea.Weight) / 100));
                                    }
                                }
                                else
                                {
                                    if (result.KeyResultArea.HodDetails.EmployeeId == result.KeyResultArea.AppraiserDetails.EmployeeId)
                                    {
                                        result.HodAccept.IsAccepted = true;
                                        result = result.HodApproval("");
                                    }
                                }

                                var entityToUpdate = result.ToBsonDocument();
                                var update = new BsonDocument("$set", entityToUpdate);
                                Collection.FindOneAndUpdate(filter, update, options: new FindOneAndUpdateOptions<AppraisalResult> { ReturnDocument = ReturnDocument.After });

                                subject = await FormatAppraiseEmployeeResponseMail(reviewId, result, employee, emailDtos, reviewName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Oops something went wrong, {ex.Message}");
                }
                finally
                {
                    if (emailDtos.Any())
                    {
                        var _finalResultRepo = _serviceFactory.GetServices<IAppraisalFinalResult>();
                        await _finalResultRepo.CalculateResult(employee.EmployeeId, reviewId);
                        await SendSubmittedAppraisalEmail(subject, emailDtos);
                    }

                }
                return "Supervisor Appraisal submitted successfully";
            }
            return "Oops something went wrong, Appraisal not submitted";

        }

        private async Task<string> FormatAppraiseEmployeeResponseMail(ObjectId reviewId, AppraisalResult result,
            OldEmployeeForViewDto employee, List<SingleEmailDto> emailDtos, string reviewName)
        {
            string subject;
            if (result.HodAccept.IsAccepted != null && result.HodAccept.IsAccepted.Value)
            {
                //send emails to employee and appraiser
                subject = $"{result.KeyResultArea.HodDetails.Name} Approved Review";

                var employeeEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = employee.FullName,
                    ReceiverEmailAddress = employee.Email,
                    HtmlContent = await sender.FormatEmailEmployeeReviewApproved(employee.FullName.Split(" ")[0],
                        result.KeyResultArea.HodDetails.Name, reviewId.ToString())
                };
                emailDtos.Add(employeeEmailDto);

                var appraiserEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = result.KeyResultArea.AppraiserDetails.Name,
                    ReceiverEmailAddress = result.KeyResultArea.AppraiserDetails.Email,
                    HtmlContent = await sender.FormatEmailAppraiserReviewApproved(
                        result.KeyResultArea.AppraiserDetails.Name.Split(' ')[0],
                        result.KeyResultArea.HodDetails.Name, employee.FullName, reviewName)
                };
                emailDtos.Add(appraiserEmailDto);
            }
            else
            {
                //send emails to employee and linemanager
                subject = "Appraisal Review Successful";
                var employeeNameArray = employee.FullName.Trim().Split(' ').Select(x => x.Substring(0, 1));
                var employeeInitials = string.Join("", employeeNameArray);

                var employeeEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = employee.FullName,
                    ReceiverEmailAddress = employee.Email,
                    HtmlContent = await sender.FormatEmailEmployeeReviewedByAppraiser(employee.FullName.Split(' ')[0],
                        result.KeyResultArea.AppraiserDetails.Name, reviewId.ToString())
                };
                emailDtos.Add(employeeEmailDto);


                var lineManagerEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = result.KeyResultArea.HodDetails.Name,
                    ReceiverEmailAddress = result.KeyResultArea.HodDetails.Email,
                    HtmlContent = await sender.FormatEmailLineManagerNewSubmission(
                        result.KeyResultArea.HodDetails.Name.Split(" ")[0], employee.FullName, employeeInitials,
                        result.KeyResultArea.AppraiserDetails.Name, reviewId.ToString())
                };
                emailDtos.Add(lineManagerEmailDto);
            }

            return subject;
        }

        public async Task EmployeeAcceptOrReject(int employeeId, ObjectId appraisalResultId, AcceptanceStatus status)
        {
            string year = DateTime.UtcNow.Year.ToString();
            var appraisalResult = Collection.AsQueryable().Where(x => x.AppraisalCycleId == appraisalResultId && x.myId == employeeId && x.Year == year).ToList();

            if (appraisalResult.Any())
            {
                appraisalResult.ForEach(async x =>
                {
                    if (x.myId != employeeId)
                    {
                        throw new InvalidOperationException("Employee do not match");
                    }

                    x.EmployeeAccept = new AcceptanceStatus()
                    {
                        IsAccepted = status.IsAccepted.Value
                    };
                    x = x.CompleteAppraisal(status.Reason ?? "");

                    var entityToUpdate = x.ToBsonDocument();
                    var update = new BsonDocument("$set", entityToUpdate);
                    var filter = Builders<AppraisalResult>.Filter.Where(y => y.Id == x.Id && x.myId == employeeId);
                    await Collection.UpdateOneAsync(filter, update);
                });
            }
            else
            {
                throw new InvalidOperationException("Appraisal Result not found");
            }
        }

        public async Task<bool> HodApprovalOrReject(OldEmployeeForViewDto Hod, OldEmployeeForViewDto employee, HodApprovalDto approvalDto, ObjectId reviewId)
        {
            try
            {
                var filter = Builders<AppraisalResult>.Filter.Where(a => a.Id == ObjectId.Parse(approvalDto.AppraisalResultId));
                var appraisalResult = Collection.Find(filter).FirstOrDefault();
                var reviewName = (await _appraisalConfig.GetSingleReviewWithoutParticipant(reviewId)).Name;
                var subject = "";
                var emailDtos = new List<SingleEmailDto>();

                if (appraisalResult != null)
                {
                    if (appraisalResult.KeyResultArea.HodDetails.EmployeeId == Hod.EmployeeId)
                    {
                        appraisalResult.HodAccept = new AcceptanceStatus()
                        {
                            IsAccepted = approvalDto.Status
                        };
                        appraisalResult = appraisalResult.HodApproval((approvalDto.reason) ?? "");

                        if (appraisalResult.HodAccept.IsAccepted.Value)
                        {
                            var average = appraisalResult.AppraiseeCalculation.Average;
                            appraisalResult.FinalCalculation.ScoreTotal = appraisalResult.AppraiseeCalculation.ScoreTotal;
                            appraisalResult.FinalCalculation.Average = average;
                            appraisalResult.FinalCalculation.WeightContribution = (average * (Convert.ToDouble(appraisalResult.KeyResultArea.Weight)) / 100);
                        }

                        var newAppraisalResult = appraisalResult;
                        var entityToUpdate = newAppraisalResult.ToBsonDocument();
                        var update = new BsonDocument("$set", entityToUpdate);

                        var res = await Collection.UpdateOneAsync(filter, update);

                        return await SendHodResponseMail(employee, approvalDto, reviewId, appraisalResult, subject, reviewName, emailDtos);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error occured, Message: {ex.Message}");
            }
        }

        private async Task<bool> SendHodResponseMail(OldEmployeeForViewDto employee, HodApprovalDto approvalDto, ObjectId reviewId,
            AppraisalResult appraisalResult, string subject, string reviewName, List<SingleEmailDto> emailDtos)
        {
            if (appraisalResult.HodAccept.IsAccepted.Value)
            {
                subject = "Review Accepted";

                var appraiserEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = appraisalResult.KeyResultArea.AppraiserDetails.Name,
                    ReceiverEmailAddress = appraisalResult.KeyResultArea.AppraiserDetails.Email,
                    HtmlContent = await sender.FormatEmailAppraiserReviewApproved(
                        appraisalResult.KeyResultArea.AppraiserDetails.Name.Split(" ")[0],
                        appraisalResult.KeyResultArea.HodDetails.Name, employee.FullName, reviewName)
                };
                emailDtos.Add(appraiserEmailDto);

                var employeeEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = employee.FullName,
                    ReceiverEmailAddress = employee.Email,
                    HtmlContent = await sender.FormatEmailEmployeeReviewApproved(
                        employee.FullName.Split(" ")[0], appraisalResult.KeyResultArea.HodDetails.Name, reviewId.ToString())
                };
                emailDtos.Add(employeeEmailDto);
            }
            else
            {
                subject = "Review Rejected";
                var employeeNameArray = employee.FullName.Trim().Split(' ').Select(x => x.Substring(0, 1));
                var employeeInitials = string.Join("", employeeNameArray);

                var hodNameArray = appraisalResult.KeyResultArea.HodDetails.Name.Trim().Split(' ').Select(x => x.Substring(0, 1));
                var hodInitials = string.Join("", hodNameArray);

                var appraiserEmailDto = new SingleEmailDto
                {
                    ReceiverFullName = appraisalResult.KeyResultArea.AppraiserDetails.Name,
                    ReceiverEmailAddress = appraisalResult.KeyResultArea.AppraiserDetails.Email,
                    HtmlContent = await sender.FormatEmailAppraiserReviewRejected(
                        appraisalResult.KeyResultArea.AppraiserDetails.Name.Split(' ')[0], employee.FullName, employeeInitials,
                        appraisalResult.KeyResultArea.HodDetails.Name, hodInitials, reviewName, reviewId.ToString(),
                        approvalDto.reason)
                };
                emailDtos.Add(appraiserEmailDto);
            }

            if (emailDtos.Any())
            {
                emailDtos.ForEach(async e => await sender.SendToSingleEmployee(subject, e));
            }

            return true;
        }

        public async Task<string> ApproveAppraisalHod(int hodId, AppraisalPerformanceParam performanceParam, HodApprovalDto approvalDtos)
        {
            ObjectId cycleId = ObjectId.Parse(performanceParam.reviewId);
            var configExist = await CheckAppraisalConfigurationDetails(cycleId);
            if (!configExist)
            {
                throw new InvalidOperationException("Invalid Configuration details");
            }

            IKeyResultArea _resultAreaRepo = _serviceFactory.GetServices<IKeyResultArea>();
            var employee = await _resultAreaRepo.GetEmployee(performanceParam.empId);
            if (employee == null)
            {
                throw new InvalidOperationException("Employee does not exist");
            }

            var hod = await _resultAreaRepo.GetEmployee(hodId);
            if (hod == null)
            {
                throw new InvalidOperationException("Hod details not found");
            }

            var result = await HodApprovalOrReject(hod, employee, approvalDtos, cycleId);
            if (result)
            {
                if (await CheckToCalculateHodResult(performanceParam))
                {
                    var reviewName = (await _appraisalConfig.GetSingleReviewWithoutParticipant(cycleId)).Name;
                    var _finalResultRepo = _serviceFactory.GetServices<IAppraisalFinalResult>();
                    await _finalResultRepo.CalculateResult(performanceParam.empId, cycleId);
                    var appraisalResult = await _finalResultRepo.GetFinalAppraisalResult(performanceParam.empId, cycleId);
                    if (appraisalResult.FinalResult != null)
                        await SendOutEmail(employee, "Appraisal Completed", appraisalResult.FinalResult.Value,
                            reviewName, performanceParam.reviewId);
                }
                return "Appraisal Approved successfully !";
            }
            else
            {
                return "Appraisal Approval Failed!, Try Again";
            }
        }

        public async Task<IEnumerable<AppraisalForApprovalDto>> GetEmployeesToAppraise(int employeeId, string appraisalConfigurationId, string appraisalCycleId, string whoAmI)
        {
            try
            {
                var year = DateTime.Now.Year;
                var pipeline = SupervisorEmployeeQuery.GetEmployeesToAppraise(employeeId, appraisalConfigurationId, appraisalCycleId);
                var lookupResult = KraCollection.Aggregate<AppraisalForApprovalDto>(pipeline);

                var finalResultToReturn = await GenerateDistinctArrayForEmployeeKRA(lookupResult);
                if (finalResultToReturn.Count > 0)
                {
                    //get Equivalent Employee Details for each one
                    IEnumerable<string> IdsToSend = finalResultToReturn.Select(x => x.EmployeeDetail.EmployeeId.ToString()).Distinct();

                    //Get team services needed for the request
                    var teamService = _serviceFactory.GetServices<ITeamRepository>();
                    var returnedEmployees = await teamService.FetchEmployeesDetailsFromEmployeeService(IdsToSend);
                    if (returnedEmployees.Any())
                    {
                        foreach (var employee in returnedEmployees)
                        {
                            var currentEmployee = finalResultToReturn.FirstOrDefault(x => x.EmployeeDetail.EmployeeId == employee.EmployeeId);
                            currentEmployee.EmployeeDetail.Email = employee.Email;
                            currentEmployee.EmployeeDetail.EmpStaffId = employee.StaffId;
                            currentEmployee.EmployeeDetail.FullName = employee.FullName;
                            currentEmployee.EmployeeDetail.Company = employee.Subgroup.Name;
                        }
                    }
                }
                return finalResultToReturn;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<IEnumerable<AppraisalForApprovalDto>> GetEmployeesToAppraise(int employeeId, string appraisalCycleId, string whoAmI)
        {
            try
            {
                var year = DateTime.Now.Year;
                BsonDocument[] pipeline;
                if (whoAmI.Equals("Appraiser", StringComparison.OrdinalIgnoreCase))
                    pipeline = SupervisorEmployeeQuery.GetEmployeesToAppraiseForAppraiser(employeeId, appraisalCycleId);
                else
                    pipeline = SupervisorEmployeeQuery.GetEmployeesToAppraiseForHod(employeeId, appraisalCycleId);

                var lookupResult = KraCollection.Aggregate<AppraisalForApprovalDto>(pipeline);

                var finalResultToReturn = await GenerateDistinctArrayForEmployeeKRA(lookupResult);
                if (finalResultToReturn.Count > 0)
                {
                    //get Equivalent Employee Details for each one
                    IEnumerable<string> IdsToSend = finalResultToReturn.Select(x => x.EmployeeDetail.EmployeeId.ToString()).Distinct();

                    //Get team services needed for the request
                    var teamService = _serviceFactory.GetServices<ITeamRepository>();
                    var returnedEmployees = await teamService.FetchEmployeesDetailsFromEmployeeService(IdsToSend);
                    if (returnedEmployees.Any())
                    {
                        foreach (var employee in returnedEmployees)
                        {
                            var currentEmployee = finalResultToReturn.FirstOrDefault(x => x.EmployeeDetail.EmployeeId == employee.EmployeeId);
                            currentEmployee.EmployeeDetail.Email = employee.Email;
                            currentEmployee.EmployeeDetail.EmpStaffId = employee.StaffId;
                            currentEmployee.EmployeeDetail.FullName = employee.FullName;
                            currentEmployee.EmployeeDetail.Company = employee.Subgroup.Name;
                        }
                    }
                }
                return finalResultToReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<AppraisalForApprovalDto>> GenerateDistinctArrayForEmployeeKRA(IAsyncCursor<AppraisalForApprovalDto> model)
        {
            var computedArray = new List<AppraisalForApprovalDto>();
            await model.ForEachAsync(item =>
            {
                if (!computedArray.Any(x => x.EmployeeDetail.EmployeeId == item.EmployeeDetail.EmployeeId))
                {
                    computedArray.Add(item);
                }
                else
                {
                    var oldResult = computedArray.FirstOrDefault(x => x.EmployeeDetail.EmployeeId == item.EmployeeDetail.EmployeeId);
                    if (oldResult != null)
                    {
                        var oldKra = oldResult.Kra_Details.ToList();
                        oldKra.AddRange(item.Kra_Details);
                        oldResult.Kra_Details = oldKra;
                    }
                }
            });

            return computedArray;
        }

        public async Task<bool> HasPaticipatedInAppraisal(int employeeId, ObjectId reviewId)
        {
            var result = Collection.AsQueryable().Any(x => x.myId == employeeId && x.AppraisalCycleId == reviewId);
            return await Task.FromResult(result);
        }

        public async Task<bool> CheckAppraisalConfigurationDetails(ObjectId reviewId)
        {
            var result = AppraisalConfigCollection.AsQueryable().Any(x => x.Id == reviewId);

            //Consider old model
            if (!result)
            {
                result = AppraisalConfigCollection.AsQueryable().Any(x => x.Year == DateTime.Now.Year && x.Cycles.Any(y => y.Id == reviewId));
            }
            return await Task.FromResult(result);
        }


        public IEnumerable<KeyResultArea> GetAcceptedKRAForAppraisal(int empId, AppraisalConfig config, string resultId = null)
        {
            if (resultId != null)
            {
                ObjectId Id = new ObjectId(resultId);
                return KraCollection.AsQueryable().Where(x => x.EmployeeId == empId && x.Id == Id && x.Approved == true && x.Status.Employee == true && x.Year == DateTime.Now.Year).ToList();
            }

            Func<KeyOutcome, bool> function = (x) => GetApplicableKeyOutcomes(x, config);
            var year = DateTime.Now.Year;
            return KraCollection.AsQueryable().Where(x => x.EmployeeId == empId && x.Approved == true && x.Status.Employee == true && x.Year == year).ToList()
                .Select(x => new KeyResultArea
                {
                    keyOutcomes = x.keyOutcomes.Where(y => GetApplicableKeyOutcomes(y, config)),
                    EmployeeId = x.EmployeeId,
                    AppraiserDetails = x.AppraiserDetails,
                    Approved = x.Approved,
                    HodDetails = x.HodDetails,
                    IsActive = x.IsActive,
                    Id = x.Id,
                    Name = x.Name,
                    Status = x.Status,
                    UserId = x.UserId,
                    Weight = x.Weight,
                    Year = x.Year
                }).ToList();
        }

        public async Task<IEnumerable<KeyResultArea>> GetAcceptedKRAForAppraisal(AppraisalPerformanceParam param)
        {
            var configDetails = await GetAppraisalConfiguration(param.reviewId);
            if (configDetails == null)
            {
                throw new InvalidOperationException("Appraisal configuration not found");
            }

            Func<KeyOutcome, bool> function = (x) => GetApplicableKeyOutcomes(x, configDetails);
            var year = DateTime.Now.Year;
            return await Task.FromResult(KraCollection.AsQueryable().Where(x => x.EmployeeId == param.empId && x.Approved == true && x.Status.Employee == true && x.Year == year).ToList()
                .Select(x => new KeyResultArea
                {
                    keyOutcomes = x.keyOutcomes.Where(y => GetApplicableKeyOutcomes(y, configDetails)),
                    EmployeeId = x.EmployeeId,
                    AppraiserDetails = x.AppraiserDetails,
                    Approved = x.Approved,
                    HodDetails = x.HodDetails,
                    IsActive = x.IsActive,
                    Id = x.Id,
                    Name = x.Name,
                    Status = x.Status,
                    UserId = x.UserId,
                    Weight = x.Weight,
                    Year = x.Year
                }).ToList());
        }

        private bool GetApplicableKeyOutcomes(KeyOutcome keyOutcome, AppraisalConfig review)
        {
            DateTime parsedDate;
            var regex = new Regex("(continuously)|(yearly)|(annually)|(weekly)|(quaterly)|(continuous)|(ongoing)|(quarterly)");
            var passedRegex = regex.IsMatch(keyOutcome.TimeLimit.ToLower());
            if (passedRegex) // not a date object
            {
                return true;
            }

            string date = DateTime.Parse(keyOutcome.TimeLimit).ToString("MM/dd/yyyy");

            var validDate = DateTime.TryParseExact(date, "MM/dd/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out DateTime defaultParsedDate);
            if (validDate)
            {
                if (defaultParsedDate >= review.Period.From)
                {
                    return defaultParsedDate <= review.Period.To;
                }
            }
            else
            {
                var splitedTimelimit = keyOutcome.TimeLimit.Split('/');
                if (splitedTimelimit.Length < 3)
                {
                    long.TryParse(keyOutcome.TimeLimit, out long timeStamp);
                    var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(timeStamp / 1000d)).ToLocalTime();
                    Console.WriteLine(dt);

                    return dt <= review.Period.To;
                }

                var dateString = $"{splitedTimelimit[1]}/{splitedTimelimit[0]}/{splitedTimelimit[2]}";
                var isvalid = DateTime.TryParseExact(dateString.Split(' ')[0], "dd/MM/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out parsedDate);
                if (!isvalid)
                {
                    isvalid = DateTime.TryParseExact(dateString, "MM/dd/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out parsedDate);
                }

                if (isvalid)
                {
                    if (parsedDate >= review.Period.From)
                    {
                        if (parsedDate <= review.Period.To)
                        {
                            return true;
                        }
                    }

                }
            }

            //unknown timelimit supplied so we return false
            return false;
        }

        public async Task<AppraisalConfig> GetAppraisalConfiguration(string configid)
        {
            var result = AppraisalConfigCollection.AsQueryable().FirstOrDefault(x => x.Id == ObjectId.Parse(configid));
            if (result == null)
            {
                var oldConfig = AppraisalConfigCollection.AsQueryable().FirstOrDefault(x => x.Year == DateTime.Now.Year && x.Cycles.Any(y => y.Id == ObjectId.Parse(configid)));
                var cycle = oldConfig.Cycles.FirstOrDefault(x => x.Id == ObjectId.Parse(configid));
                result = oldConfig;
                result.Id = cycle.Id;
                result.isActive = cycle.isActive;
                result.Completed = cycle.Completed;
                result.Name = cycle.Name;
                result.Year = oldConfig.Year;
                result.ReviewType = new ReviewType
                {
                    Name = "Performance Based Review"
                };
                result.Period = new PeriodInReview
                {
                    From = cycle.StartDate.AddMonths(3),
                    To = cycle.StopDate
                };

                result.Duration = new Duration
                {
                    StartDate = cycle.StartDate,
                    StopDate = cycle.StopDate
                };
            }
            return await Task.FromResult(result);
        }

        public IEnumerable<KeyResultArea> GetOnlyApplicableKeyoutcomesForAppraisal(ObjectId kraId, int EmployeeId, IList<string> keyoutcomeIds)
        {
            var pipelineObj = AppraisalQueries.GetApplicableKeyOutcomes(kraId, EmployeeId, keyoutcomeIds);
            var result = KraCollection.Aggregate<KeyResultArea>(pipelineObj).ToList();
            return result;
        }

        public async Task<bool> UpdateKeyResultAreaForExistingResult(string cycleId)
        {
            try
            {
                var result = Collection.AsQueryable().Where(x => x.AppraisalCycleId == ObjectId.Parse(cycleId)).ToList();
                result.ForEach(async x => await UpdateAppraisalResult(x));

                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task UpdateAppraisalResult(AppraisalResult appraisalResult)
        {
            if (appraisalResult.KeyOutcomeScore.Count != appraisalResult.KeyResultArea.keyOutcomes.Count())
            {
                appraisalResult.KeyResultArea = GetOnlyApplicableKeyoutcomesForAppraisal(appraisalResult.KeyResultArea.Id,
                      appraisalResult.myId, appraisalResult.KeyOutcomeScore.Select(x => x.KeyOutcomeId.ToString()).ToList()).FirstOrDefault();

                var modifiedResult = appraisalResult.ToBsonDocument();
                var filter = Builders<AppraisalResult>.Filter.Where(x => x.Id == appraisalResult.Id);
                await Collection.ReplaceOneAsync(filter, appraisalResult);
            }
        }

        public async Task<bool> RestAppraisal(int empId, int appraiserId, ObjectId cycleId)
        {
            try
            {
                //var pipelineObj = AppraisalQueries.GetAppraisalResultForReset(empId, appraiserId, cycleId);
                //var result = Collection.Aggregate<AppraisalResult>(pipelineObj).ToList();

                var filter = Builders<AppraisalResult>.Filter.Where(x => x.myId == empId &&
                                                            x.KeyResultArea.HodDetails.EmployeeId == appraiserId
                                                            && x.AppraisalCycleId == cycleId);

                var appraisalResult = Collection.Find(filter).ToList();
                if (appraisalResult.Any())
                {
                    appraisalResult.ForEach(x => x.ResetForHod(Collection));
                }
                else
                {
                    filter = Builders<AppraisalResult>.Filter.Where(x => x.myId == empId &&
                                                           x.KeyResultArea.AppraiserDetails.EmployeeId == appraiserId
                                                           && x.AppraisalCycleId == cycleId);
                    appraisalResult = Collection.Find(filter).ToList();
                    appraisalResult.ForEach(x => x.ResetForAppraiser(Collection));
                }

                return await Task.FromResult(true);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetEmployeeAppraisal(int empId, ObjectId cycleId)
        {
            try
            {
                var filter = Builders<AppraisalResult>.Filter.Where(a => a.myId == empId && a.AppraisalCycleId == cycleId);

                var result = await Collection.DeleteManyAsync(filter);
                if (result.IsAcknowledged)
                {
                    var finalAppraisalService = _serviceFactory.GetServices<IAppraisalFinalResult>(typeof(IAppraisalFinalResult));
                    await finalAppraisalService.ResetEmployeeFinalAppraisal(empId, cycleId);
                }
                return result.IsAcknowledged;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private async Task SendOutEmail(OldEmployeeForViewDto employee, string subject, double finalResult, string reviewName, string reviewId)
        {
            SingleEmailDto emailDto = new SingleEmailDto()
            {
                ReceiverFullName = employee.FullName,
                ReceiverEmailAddress = employee.Email,
                HtmlContent = await sender.FormatEmailAppraisalScore(employee.FullName.Split(' ')[0], finalResult.ToString(), reviewName, reviewId),
            };

            if (emailDto.HtmlContent == null)
            {
                emailDto.HtmlContent = @$"<b>Dear {employee.FullName},</b> <br /> <br /> <p>Your Appraisal Has been accepted. Kindly login to the portal and View. <br /> Thank you.</p>";
            }

            await sender.SendToSingleEmployee(subject, emailDto);
        }

        public async Task SendSubmittedAppraisalEmail(string subject, List<SingleEmailDto> emailDto)
        {
            var emailDtos = AppraisalResultExtension.FormatEmailForAppraisal(emailDto);

            if (emailDtos.Any())
            {
                emailDtos.ForEach(async e => await sender.SendToSingleEmployee(subject, e));
            }
        }

        public async Task<bool> IsAnyAppriasalResultRejected(int EmployeeId, ObjectId CycleId)
        {
            var result = Collection.AsQueryable().Any(x => x.myId == EmployeeId && x.AppraisalCycleId == CycleId && x.IsAccepted == false);
            return await Task.FromResult(result);
        }

        public async Task<AppraisalReport> GetEmployeesParticipatingInAppraisal(ObjectId cycleId, PaginationResourceParameter param)
        {
            AppraisalReport report = null;
            var _appraisalConfigRepo = _serviceFactory.GetServices<IAppraisalConfig>();
            var _finalAppraisalResult = _serviceFactory.GetServices<IAppraisalFinalResult>();
            var appraisalReview = await _appraisalConfigRepo.GetSingleReviewWithoutParticipant(cycleId);

            var result = (await _finalAppraisalResult.FilterEmployeeParticipants(cycleId, appraisalReview.Participants)).AsQueryable();
            if (result.Any())
            {
                IQueryable<AppraisalsForReviewDto> searchedResult = result;
                if (!string.IsNullOrWhiteSpace(param.SearchQuery))
                    searchedResult = searchedResult.Where(c => c.FullName.Contains(param.SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList().AsQueryable();
                if (!string.IsNullOrWhiteSpace(param.Company))
                    searchedResult = searchedResult.Where(c => c.Company.Equals(param.Company, StringComparison.OrdinalIgnoreCase)).ToList().AsQueryable();
                if (!string.IsNullOrWhiteSpace(param.Status))
                    searchedResult = searchedResult.Where(c => c.Status.Equals(param.Status, StringComparison.OrdinalIgnoreCase)).ToList().AsQueryable();
                if (!string.IsNullOrWhiteSpace(param.Score))
                {
                    switch (param.Score)
                    {
                        case "3.50-above":
                            searchedResult = searchedResult.Where(c => c.FinalResult >= 3.5).ToList().AsQueryable();
                            break;
                        case "2.50-3.49":
                            searchedResult = searchedResult.Where(c => c.FinalResult >= 2.5 && c.FinalResult <= 3.49).ToList().AsQueryable();
                            break;
                        case "1.50-2.49":
                            searchedResult = searchedResult.Where(c => c.FinalResult >= 1.5 && c.FinalResult <= 2.49).ToList().AsQueryable();
                            break;
                        case "1.50-below":
                            searchedResult = searchedResult.Where(c => c.FinalResult <= 1.5).ToList().AsQueryable();
                            break;
                        default:
                            break;
                    }
                }

                report = new AppraisalReport
                {
                    EmployeeReport = PagedList<AppraisalsForReviewDto>.Create(searchedResult, param.PageNumber, param.PageSize),
                    Pagination = new PaginationResourceResponse(searchedResult.Count(), param.PageNumber, param.PageSize)
                };
                return report;
            }
            return report;
        }


        public async Task<DirectReportDto> SearchEmployeeToAppraise(int employeeId, string appraisalCycleId, string whoAmI, PaginationResourceParameter pagingParam)
        {
            var result = await GetEmployeesToAppraise(employeeId, appraisalCycleId, whoAmI);
            int allEmployeeCount = result.Count();

            var Dto = _mapper.Map<IEnumerable<AppraisalForApprovalViewDto>>(result);
            Dto.ToList().ForEach(x =>
            {
                x.Kra_Details.ToList().ForEach(y =>
                {
                    y.KeyResultArea.whoami = y.KeyResultArea.Appraiser.EmployeeId == employeeId ? "appraiser" : "hod";
                });
            });

            if (!string.IsNullOrWhiteSpace(pagingParam.SearchQuery))
            {
                var searchedResult = Dto.Where(x => x.EmployeeDetail.FullName.Contains(pagingParam.SearchQuery, StringComparison.CurrentCultureIgnoreCase) || x.EmployeeDetail.Email.Contains(pagingParam.SearchQuery, StringComparison.CurrentCultureIgnoreCase)).AsQueryable();
                return new DirectReportDto
                {
                    DirectReport = PagedList<AppraisalForApprovalViewDto>.Create(searchedResult, pagingParam.PageNumber, pagingParam.PageSize),
                    Pagination = new PaginationResourceResponse(searchedResult.Count(), pagingParam.PageNumber, pagingParam.PageSize)
                };
            }

            return new DirectReportDto
            {
                DirectReport = PagedList<AppraisalForApprovalViewDto>.Create(Dto.AsQueryable(), pagingParam.PageNumber, pagingParam.PageSize),
                Pagination = new PaginationResourceResponse(allEmployeeCount, pagingParam.PageNumber, pagingParam.PageSize)
            };
        }

        public async Task<bool> CheckToCalculateHodResult(AppraisalPerformanceParam performanceParam)
        {
            var result = Collection.AsQueryable().Where(x => x.myId == performanceParam.empId && x.AppraisalCycleId == ObjectId.Parse(performanceParam.reviewId)).ToList();
            return result.All(x => x.NextAppraisee.Equals("Done", StringComparison.OrdinalIgnoreCase));
        }

        public ArrayList GetEmployeeReviews(int employeeId)
        {
            var activeAppraisals = AppraisalConfigCollection.AsQueryable().Where(a => a.isActive == true).Select(a => a.Id).ToArray();

            var filterBuilder = Builders<AppraisalResult>.Filter;
            var filter = filterBuilder.Where(r => r.myId == employeeId) &
                         filterBuilder.In("AppraisalCycleId", activeAppraisals);
            var employeeResult = Collection.Find(filter).ToList();

            var pendingApprovals = new List<AppraisalResult>();
            var pendingReviews = new List<AppraisalResult>();
            var completedReviews = new List<AppraisalResult>();
            var resultToReturn = new ArrayList();

            foreach (var result in employeeResult)
            {
                switch (result.NextAppraisee.ToLower())
                {
                    case "appraiser":
                        pendingReviews.Add(result);
                        break;
                    case "hod":
                        pendingApprovals.Add(result);
                        break;
                    case "done":
                        completedReviews.Add(result);
                        break;
                }
            }

            resultToReturn.Add(pendingApprovals);
            resultToReturn.Add(pendingReviews);
            resultToReturn.Add(completedReviews);

            return resultToReturn;
        }

        public async Task<List<AppraisalResultForViewDto>> GetRejectedEmployeeAppraisal(int EmployeeId, ObjectId CycleId)
        {
            var configDetails = await GetAppraisalConfiguration(CycleId.ToString());
            if (configDetails == null)
            {
                throw new InvalidOperationException("Appraisal configuration not found");
            }

            IKeyResultArea resultAreaRepo = _serviceFactory.GetServices<IKeyResultArea>();
            var employee = await resultAreaRepo.GetEmployee(EmployeeId);
            if (employee == null)
            {
                throw new InvalidOperationException("Employee does not exist");
            }

            var emptyList = new List<AppraisalResultForViewDto>();
            var result = await IsAnyAppriasalResultRejected(EmployeeId, CycleId);
            if (result == true)
            {
                var appraisalToReturn = (await Collection.FindAsync(x => x.myId == EmployeeId && x.AppraisalCycleId == CycleId && x.IsAccepted == false)).ToList();
                var apprasalDtoToReturn = _mapper.Map<List<AppraisalResultForViewDto>>(appraisalToReturn);
                return apprasalDtoToReturn;
            }
            return emptyList;
        }

    }

}
