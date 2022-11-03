using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Resourceedge.Appraisal.API.DBQueries;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.Domain.DBContexts;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Appraisal.Domain.Queries;
using Resourceedge.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Appraisal.API.Services
{
    public class AppraisalFinalResultService : IAppraisalFinalResult
    {
        public readonly IMongoCollection<FinalAppraisalResult> Collection;
        public readonly IMongoCollection<AppraisalResult> AppraisalResultCollection;
        public readonly IQueryable<AppraisalResult> AppraisalResultQueryableCollection;
        private readonly IDbContext context;
        private readonly IServiceFactory serviceFactory;
        private readonly IMapper _mapper;

        public AppraisalFinalResultService(IDbContext _context, IServiceFactory _serviceFactory, IMapper mapper)
        {
            Collection = _context.Database.GetCollection<FinalAppraisalResult>($"{nameof(FinalAppraisalResult)}s");
            AppraisalResultCollection = _context.Database.GetCollection<AppraisalResult>($"{nameof(AppraisalResult)}s");
            AppraisalResultQueryableCollection = AppraisalResultCollection.AsQueryable();
            context = _context;
            serviceFactory = _serviceFactory;
            _mapper = mapper;
        }

        public async Task CalculateResult(int empId, ObjectId cycleId)
        {
            try
            {
                var configService = serviceFactory.GetServices<IAppraisalConfig>();
                var configDetails = await configService.GetSingleReviewWithoutParticipant(cycleId);

                var allAppraisalResult = AppraisalResultQueryableCollection.Where(x => x.myId == empId && x.AppraisalCycleId == cycleId).ToList();

                if (!string.Equals(configDetails.Name, "Quarter 4", StringComparison.OrdinalIgnoreCase))
                {
                    CalculateQuarterlyAppraisal(allAppraisalResult, empId, cycleId);
                }
                else
                {
                    CalculateAnnualAppraisal(allAppraisalResult, empId, cycleId);
                }
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Error occured, message:{ex.Message}");
            }
        }
        public async Task<IEnumerable<FinalAppraisalResultForViewDto>> GetAllResultByCycle(ObjectId cycleId)
        {
            string year = DateTime.Now.Year.ToString();
            var match = new BsonDocument
            {
                {
                    "$match" ,
                    new BsonDocument
                    {
                        {$"AppraisalCycleId", cycleId },
                        {$"Year",  year},
                        //{$"EmployeeAccept.IsAccepted", true }                        
                    }
                }
            };

            var project = new BsonDocument
            {
                {
                    "$project", new BsonDocument{
                        { "EmployeeDetail", new BsonDocument
                            {
                                { "EmployeeId" , "$EmployeeId" }
                             }
                        },
                        {
                            "Result",new BsonDocument
                            {
                                {"EmployeeId" , "$EmployeeId" },
                                {"FinalResult" , "$FinalResult" },
                                {"EmployeeResult", "$EmployeeResult" },
                                {"AppraiseeResult","$AppraiseeResult" }
                            }
                        }
                    }
                }
            };

            var pipeline = new[] { match, project };
            var lookupResult = Collection.Aggregate<FinalAppraisalResultForViewDto>(pipeline);

            var result = lookupResult.ToList();
            var finalResultToReturn = new List<FinalAppraisalResultForViewDto>();
            if (result.Count > 0)
            {
                IEnumerable<string> IdsToSend = result.Select(x => x.EmployeeDetail.EmployeeId.ToString()).Distinct();
                foreach (var item in result)
                {
                    if (!finalResultToReturn.Any(x => x.EmployeeDetail.EmployeeId == item.EmployeeDetail.EmployeeId))
                    {
                        finalResultToReturn.Add(item);
                    }
                }
                var teamRepository = serviceFactory.GetServices<ITeamRepository>();
                var returnedEmployees = await teamRepository.FetchEmployeesDetailsFromEmployeeService(IdsToSend);
                if (returnedEmployees.Any())
                {
                    foreach (var employee in returnedEmployees)
                    {
                        var currentEmployee = finalResultToReturn.FirstOrDefault(x => x.EmployeeDetail.EmployeeId == employee.EmployeeId);
                        currentEmployee.EmployeeDetail.Email = employee.Email;
                        currentEmployee.EmployeeDetail.EmpStaffId = employee.StaffId;
                        currentEmployee.EmployeeDetail.FullName = employee.FullName.ToUpperInvariant();
                        currentEmployee.EmployeeDetail.Company = employee.Subgroup.Name;
                    }
                }
            }
            return finalResultToReturn; ;
        }


        public async Task<IEnumerable<FinalAppraisalResultForViewDto>> GetAllResultByCycle(ObjectId cycleId, string year)
        {
            
            var match = new BsonDocument
            {
                {
                    "$match" ,
                    new BsonDocument
                    {
                        {$"AppraisalCycleId", cycleId },
                        {$"Year",  year},
                        //{$"EmployeeAccept.IsAccepted", true }                        
                    }
                }
            };

            var project = new BsonDocument
            {
                {
                    "$project", new BsonDocument{
                        { "EmployeeDetail", new BsonDocument
                            {
                                { "EmployeeId" , "$EmployeeId" }
                             }
                        },
                        {
                            "Result",new BsonDocument
                            {
                                {"EmployeeId" , "$EmployeeId" },
                                {"FinalResult" , "$FinalResult" },
                                {"EmployeeResult", "$EmployeeResult" },
                                {"AppraiseeResult","$AppraiseeResult" }
                            }
                        }
                    }
                }
            };

            var pipeline = new[] { match, project };
            var lookupResult = Collection.Aggregate<FinalAppraisalResultForViewDto>(pipeline);

            var result = lookupResult.ToList();
            var finalResultToReturn = new List<FinalAppraisalResultForViewDto>();
            if (result.Count > 0)
            {
                IEnumerable<string> IdsToSend = result.Select(x => x.EmployeeDetail.EmployeeId.ToString()).Distinct();
                foreach (var item in result)
                {
                    if (!finalResultToReturn.Any(x => x.EmployeeDetail.EmployeeId == item.EmployeeDetail.EmployeeId))
                    {
                        finalResultToReturn.Add(item);
                    }
                }
                var teamRepository = serviceFactory.GetServices<ITeamRepository>();
                var returnedEmployees = await teamRepository.FetchEmployeesDetailsFromEmployeeService(IdsToSend);
                if (returnedEmployees.Any())
                {
                    foreach (var employee in returnedEmployees)
                    {
                        var currentEmployee = finalResultToReturn.FirstOrDefault(x => x.EmployeeDetail.EmployeeId == employee.EmployeeId);
                        currentEmployee.EmployeeDetail.Email = employee.Email;
                        currentEmployee.EmployeeDetail.EmpStaffId = employee.StaffId;
                        currentEmployee.EmployeeDetail.FullName = employee.FullName.ToUpperInvariant();
                        currentEmployee.EmployeeDetail.Company = employee.Subgroup.Name;
                    }
                }
            }
            return finalResultToReturn; ;
        }

        public async Task<IEnumerable<FinalAppraisalResultForViewDto>> GetAppraisalResultByGroup(string group, int pageNumber, int pageSize, ObjectId cycleId, string year)
        {
            var allResult = await GetAllResultByCycle(cycleId, year);

            var groupResult = allResult.Where(r => r.EmployeeDetail.Company == group).AsQueryable().OrderBy(r => r.EmployeeDetail.FullName);
            return PagedList<FinalAppraisalResultForViewDto>.Create(groupResult, pageNumber, pageSize);
        }

        public async Task<IEnumerable<OrgaizationandCount>> GetOrgaization(ObjectId CycleId)
        {
            var allResult = await GetAllResultByCycle(CycleId);

            return allResult.GroupBy(c => c.EmployeeDetail.Company).Select(x => new OrgaizationandCount { Group = x.Key, Count = x.Count() });
        }

        public async Task<bool> ReCalculateFinalAppraisalResult(ObjectId cycleId)
        {
            try
            {
                var teamRepository = serviceFactory.GetServices<ITeamRepository>();
                var employeeIds = await teamRepository.GetEmployeeIDs();

                employeeIds.ForEach(async x => await CalculateResult(x, cycleId));

                //CalculateResult(23, cycleId);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public async Task<IDictionary<string, IEnumerable<FinalAppraisalResultForViewDto>>> GetResultForDownload(ObjectId cycleId, string year)
        {
            var allResult = await GetAllResultByCycle(cycleId, year);
            var organization =  allResult.GroupBy(c => c.EmployeeDetail.Company).Select(x => new OrgaizationandCount { Group = x.Key, Count = x.Count() });

            IDictionary<string, IEnumerable<FinalAppraisalResultForViewDto>> resultForView = new Dictionary<string, IEnumerable<FinalAppraisalResultForViewDto>>();
            foreach (var item in organization)
            {
                var groupResult = allResult.Where(r => r.EmployeeDetail.Company == item.Group).OrderBy(a => a.EmployeeDetail.FullName);

                resultForView.Add(item.Group, groupResult);
            }

            return resultForView;
        }

        public async Task<bool> ResetEmployeeFinalAppraisal(int empId, ObjectId cycleId)
        {
            try
            {
                var filter = Builders<FinalAppraisalResult>.Filter.Where(a => a.EmployeeId == empId && a.AppraisalCycleId == cycleId);

                var result = await Collection.DeleteOneAsync(filter);
                return result.IsAcknowledged;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public decimal NormalizeResult(decimal totalweight, decimal totalWeightContribution, int rating)
        {
            //get the denominator
            var newRating = ((totalweight / 100) * rating);

            //calculate new result using the denominator
            var newScore = ((totalWeightContribution / newRating) * rating);

            return decimal.Round(newScore,2,MidpointRounding.AwayFromZero);
        }

        public void CalculateQuarterlyAppraisal(List<AppraisalResult> allAppraisalResult, int empId, ObjectId cycleId)
        {
            try
            {
                var appraisalResult = allAppraisalResult.Where(x => x.AppraiseeCalculation.ScoreTotal != null).ToList();
                var empAppraisalResult = allAppraisalResult.Where(x => x.EmployeeCalculation.ScoreTotal != null).ToList();
                var filter = Builders<FinalAppraisalResult>.Filter.Where(x => x.EmployeeId == empId && x.AppraisalCycleId == cycleId);
                var oldFinalResult = Collection.Find(filter).FirstOrDefault();

                if (oldFinalResult == null)
                {
                    var totalWeightAppraised = empAppraisalResult.Sum(a => a.KeyResultArea.Weight);

                    var decimalEmployeeResult = NormalizeResult(totalWeightAppraised, (decimal)empAppraisalResult.Sum(x => x.EmployeeCalculation.WeightContribution), 5);
                    var decimalAppraisalResult = (decimal)((appraisalResult.Any() && appraisalResult.FirstOrDefault().IsAccepted != null) ? NormalizeResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.AppraiseeCalculation.WeightContribution), 5) : 0);
                    var decimalFinalResult = (decimal)((appraisalResult.Any() && appraisalResult.FirstOrDefault().IsAccepted != null) ? NormalizeResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.FinalCalculation.WeightContribution), 5) : 0);

                    var finalResult = new FinalAppraisalResult()
                    {
                        AppraisalConfigId = empAppraisalResult.FirstOrDefault().AppraisalConfigId,
                        AppraisalCycleId = empAppraisalResult.FirstOrDefault().AppraisalCycleId,
                        EmployeeId = empAppraisalResult.FirstOrDefault().myId,
                        EmployeeResult = (double)decimal.Round(decimalEmployeeResult, 2, MidpointRounding.AwayFromZero),
                        AppraiseeResult = (double)decimal.Round(decimalAppraisalResult, 2, MidpointRounding.AwayFromZero),
                        FinalResult = (double)decimal.Round(decimalFinalResult, 2, MidpointRounding.AwayFromZero),
                        Year = DateTime.Now.Year.ToString()
                    };

                    Collection.InsertOne(finalResult);
                }
                else
                {
                    var totalWeightAppraised = appraisalResult.Sum(a => a.KeyResultArea.Weight);

                    oldFinalResult.EmployeeResult = (oldFinalResult.EmployeeResult != 0) ? (double)NormalizeResult(empAppraisalResult.Sum(a => a.KeyResultArea.Weight), (decimal)empAppraisalResult.Sum(x => x.EmployeeCalculation.WeightContribution), 5) : (double)NormalizeResult(empAppraisalResult.Sum(a => a.KeyResultArea.Weight), (decimal)empAppraisalResult.Sum(x => x.EmployeeCalculation.WeightContribution), 5);
                    if (!appraisalResult.Any(x => x.AppraiseeCalculation.WeightContribution == 0))
                    {
                        oldFinalResult.AppraiseeResult = (oldFinalResult.AppraiseeResult != 0) ? (double)NormalizeResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.AppraiseeCalculation.WeightContribution), 5) : (double)NormalizeResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.AppraiseeCalculation.WeightContribution), 5);
                    }
                    if (!appraisalResult.Any(s => s.FinalCalculation.WeightContribution == 0))
                    {
                        oldFinalResult.FinalResult = (appraisalResult.FirstOrDefault().IsAccepted != null) ? (double)NormalizeResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.FinalCalculation.WeightContribution), 5) : (double)NormalizeResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.FinalCalculation.WeightContribution), 5);
                    }

                    var finalResult = oldFinalResult.ToBsonDocument();
                    var update = new BsonDocument("$set", finalResult);

                    Collection.UpdateOne(filter, update);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task CalculateAnnualAppraisal(List<AppraisalResult> appraisalResults, int employeeId, ObjectId cycleId)
        {
            if (employeeId != 0)
            {
                var filter = Builders<FinalAppraisalResult>.Filter.Where(x => x.EmployeeId == employeeId && x.AppraisalCycleId == cycleId);
                var softskillResult = CalculateSoftSkillAppraisal(appraisalResults.Where(x => x.IsSoftSkills == true).ToList());
                var normalAppraisalResult = CalculateNormalAnnualAppraisal(appraisalResults.Where(x => x.IsSoftSkills == false).ToList());

                var oldFinalResult = Collection.Find(filter).FirstOrDefault();
                if (oldFinalResult == null)
                {
                    var finalResult = new FinalAppraisalResult()
                    {
                        AppraisalConfigId = softskillResult.AppraisalConfigId,
                        AppraisalCycleId = softskillResult.AppraisalCycleId,
                        SoftSkillResult = softskillResult.EmployeeResult,
                        NormalResult = normalAppraisalResult.EmployeeResult,
                        EmployeeResult = (double)((decimal)softskillResult.EmployeeResult + (decimal)normalAppraisalResult.EmployeeResult),
                        AppraiseeResult = (double)((decimal)softskillResult.AppraiseeResult + (decimal)normalAppraisalResult.AppraiseeResult),
                        FinalResult = (double)((decimal)softskillResult.FinalResult + (decimal)normalAppraisalResult.FinalResult),
                        Year = DateTime.Now.Year.ToString(),
                        EmployeeId = employeeId
                    };

                    Collection.InsertOne(finalResult);
                }
                else
                {
                    oldFinalResult.AppraisalConfigId = softskillResult.AppraisalConfigId;
                    oldFinalResult.AppraisalCycleId = softskillResult.AppraisalCycleId;
                    oldFinalResult.SoftSkillResult = softskillResult.EmployeeResult;
                    oldFinalResult.NormalResult = normalAppraisalResult.EmployeeResult;
                    oldFinalResult.EmployeeResult = (double)((decimal)softskillResult.EmployeeResult + (decimal)normalAppraisalResult.EmployeeResult);
                    oldFinalResult.AppraiseeResult = (double)((decimal)softskillResult.AppraiseeResult + (decimal)normalAppraisalResult.AppraiseeResult);
                    oldFinalResult.FinalResult = (double)((decimal)softskillResult.FinalResult + (decimal)normalAppraisalResult.FinalResult);

                    var finalResult = oldFinalResult.ToBsonDocument();
                    var update = new BsonDocument("$set", finalResult);

                    Collection.UpdateOne(filter, update);
                }

            }
        }

        public FinalAppraisalResult CalculateSoftSkillAppraisal(List<AppraisalResult> result)
        {
            var empAppraisalResult = result.Where(x => x.EmployeeCalculation.ScoreTotal != null).ToList();
            var empAppraisalResultSoftskils = result.Where(x => x.EmployeeCalculation.ScoreTotal != null).ToList();
            var appraisalResult = result.Where(x => x.AppraiseeCalculation.ScoreTotal != null).ToList();
            var totalWeightAppraised = empAppraisalResult.Sum(a => a.KeyResultArea.Weight);

            var decimalEmployeeResult = NormalizeSoftSkillResult(totalWeightAppraised, (decimal)empAppraisalResult.Sum(x => x.EmployeeCalculation.WeightContribution), empAppraisalResult.Count());
            var decimalAppraisalResult = (decimal)((appraisalResult.Any() && appraisalResult.FirstOrDefault().IsAccepted != null) ? NormalizeSoftSkillResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.AppraiseeCalculation.WeightContribution), empAppraisalResult.Count()) : 0);
            var decimalFinalResult = (decimal)((appraisalResult.Any() && appraisalResult.FirstOrDefault().IsAccepted != null) ? NormalizeSoftSkillResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.FinalCalculation.WeightContribution), empAppraisalResult.Count()) : 0);

            return new FinalAppraisalResult()
            {
                AppraisalConfigId = empAppraisalResult.FirstOrDefault().AppraisalConfigId,
                AppraisalCycleId = empAppraisalResult.FirstOrDefault().AppraisalCycleId,
                EmployeeId = empAppraisalResult.FirstOrDefault().myId,
                EmployeeResult = (double)decimal.Round(decimalEmployeeResult, 2, MidpointRounding.AwayFromZero),
                AppraiseeResult = (double)decimal.Round(decimalAppraisalResult, 2, MidpointRounding.AwayFromZero),
                FinalResult = (double)decimal.Round(decimalFinalResult, 2, MidpointRounding.AwayFromZero),
                Year = DateTime.Now.Year.ToString()
            };

        }

        public FinalAppraisalResult CalculateNormalAnnualAppraisal(List<AppraisalResult> result)
        {
            var empAppraisalResult = result.Where(x => x.EmployeeCalculation.ScoreTotal != null).ToList();
            var appraisalResult = result.Where(x => x.AppraiseeCalculation.ScoreTotal != null).ToList();
            var totalWeightAppraised = empAppraisalResult.Sum(a => a.KeyResultArea.Weight);

            var decimalEmployeeResult = NormalizeNormalAnnualResult(totalWeightAppraised, (decimal)empAppraisalResult.Sum(x => x.EmployeeCalculation.WeightContribution), 5);
            var decimalAppraisalResult = (decimal)((appraisalResult.Any() && appraisalResult.FirstOrDefault().IsAccepted != null) ? NormalizeNormalAnnualResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.AppraiseeCalculation.WeightContribution), 5) : 0);
            var decimalFinalResult = (decimal)((appraisalResult.Any() && appraisalResult.FirstOrDefault().IsAccepted != null) ? NormalizeNormalAnnualResult(totalWeightAppraised, (decimal)appraisalResult.Sum(x => x.FinalCalculation.WeightContribution), 5) : 0);

            return new FinalAppraisalResult()
            {
                AppraisalConfigId = empAppraisalResult.FirstOrDefault().AppraisalConfigId,
                AppraisalCycleId = empAppraisalResult.FirstOrDefault().AppraisalCycleId,
                EmployeeId = empAppraisalResult.FirstOrDefault().myId,
                EmployeeResult = (double)decimal.Round(decimalEmployeeResult, 2, MidpointRounding.AwayFromZero),
                AppraiseeResult = (double)decimal.Round(decimalAppraisalResult, 2, MidpointRounding.AwayFromZero),
                FinalResult = (double)decimal.Round(decimalFinalResult, 2, MidpointRounding.AwayFromZero),
                Year = DateTime.Now.Year.ToString()
            };
        }


        public decimal NormalizeSoftSkillResult(decimal totalweight, decimal totalWeightContribution, int count)
        {
            //calculate new result using the denominator
            var Score = (totalWeightContribution / count);

            return decimal.Round(Score, 2, MidpointRounding.AwayFromZero);
        }

        public decimal NormalizeNormalAnnualResult(decimal totalweight, decimal totalWeightContribution, int rating)
        {
            var newRating = ((totalweight / 100) * 5);

            //var finalRating = ((newRating * 5) / 95);

            var newScore = ((totalWeightContribution / newRating) * rating);

            var finalScore = ((newScore * 95) / 100);

            return decimal.Round(finalScore, 2, MidpointRounding.AwayFromZero);
        }
       
        public async Task<FinalResultDtoForView> GetFinalAppraisalResult(int employeeId, ObjectId reviewId)
        {
            var finalResult = Collection.Find(Builders<FinalAppraisalResult>.Filter.Where(x => x.EmployeeId == employeeId && x.AppraisalCycleId == reviewId)).FirstOrDefault();
            return _mapper.Map<FinalResultDtoForView>(finalResult);
        }

        public async Task<IEnumerable<AppraisalsForReviewDto>> FilterEmployeeParticipants(ObjectId cycleId, Participants participants)
        {
            var AppraisalResultToReturn = new List<AppraisalsForReviewDto>();
            if (cycleId.ToString() != null)
            {
                var pipeline = SupervisorEmployeeQuery.GetParticipants(cycleId);
                var resultToReturn = Collection.Aggregate<AppraisalsForReviewDto>(pipeline);
                var finalResultToReturn = resultToReturn.ToList();

                var teamRepository = serviceFactory.GetServices<ITeamRepository>();
                var returnedEmployees = await teamRepository.GetDistinctEmployeeIncludedinAppraisal(participants);
                var lookupResult = _mapper.Map<IEnumerable<AppraisalsForReviewDto>>(returnedEmployees);
                var result = lookupResult.ToList();

                if (result.Any())
                {
                    foreach (var employee in result)
                    {
                        var currentEmployee = finalResultToReturn.FirstOrDefault(x => x.EmployeeId == employee.EmployeeId);
                        if(currentEmployee != null)
                        {
                            employee.EmployeeResult = currentEmployee.EmployeeResult;
                            employee.AppraiseeResult = currentEmployee.AppraiseeResult;
                            employee.FinalResult = currentEmployee.FinalResult;
                        }                        

                        if (employee.FinalResult > 0)
                            employee.Status = "Completed";
                        else if (employee.AppraiseeResult > 0)
                            employee.Status = "InProgress";
                        else if (employee.EmployeeResult > 0)
                            employee.Status = "Self-Appraised";
                        else
                            employee.Status = "Un-Appraised";
                    }
                }       
                return result;
            }
            return AppraisalResultToReturn;
        }

        public async Task<FileStream> GenerateExcelForAppraisalResult(FileStream fs, ObjectId reviewId, string year)
        {
            IWorkbook workbook;
            workbook = new XSSFWorkbook();

            var result = await GetResultForDownload(reviewId, year);

            foreach (var item in result)
            {

                ISheet excelSheet = workbook.CreateSheet(item.Key);
                IRow row = excelSheet.CreateRow(0);

                row.CreateCell(0).SetCellValue("SN");
                row.CreateCell(1).SetCellValue("FullName");
                row.CreateCell(2).SetCellValue("Email");
                row.CreateCell(3).SetCellValue("EmployeeScore");
                row.CreateCell(4).SetCellValue("AppraiserScore");
                row.CreateCell(5).SetCellValue("FinalScore");
                int count = 0;

                foreach (var finalResult in item.Value)
                {
                    row = excelSheet.CreateRow(++count);
                    row.CreateCell(0).SetCellValue(count);
                    row.CreateCell(1).SetCellValue(finalResult.EmployeeDetail.FullName);
                    row.CreateCell(2).SetCellValue(finalResult.EmployeeDetail.Email);
                    row.CreateCell(3).SetCellValue(finalResult.Result.EmployeeResult);
                    row.CreateCell(4).SetCellValue(finalResult.Result.AppraiseeResult.ToString());
                    row.CreateCell(5).SetCellValue(finalResult.Result.FinalResult.GetValueOrDefault());
                }
            }
            workbook.Write(fs);

            return fs;
        }
    }
}
