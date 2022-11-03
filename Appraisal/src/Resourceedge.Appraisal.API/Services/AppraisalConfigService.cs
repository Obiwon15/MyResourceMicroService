using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Resourceedge.Appraisal.API.Interfaces;
using Resourceedge.Appraisal.API.ResourceParamters;
using Resourceedge.Appraisal.Domain.DBContexts;
using Resourceedge.Appraisal.Domain.Dtos;
using Resourceedge.Appraisal.Domain.Entities;
using Resourceedge.Appraisal.Domain.Models;
using Resourceedge.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Resourceedge.Email.Api.Interfaces;
using Resourceedge.Email.Api.Model;

namespace Resourceedge.Appraisal.API.Services
{
    public class AppraisalConfigService : IAppraisalConfig
    {
        private readonly IMongoCollection<AppraisalConfig> Collection;
        public readonly IQueryable<AppraisalConfig> QueryableCollection;
        private readonly ILogger<AppraisalConfigService> _logger;
        private readonly IMapper _Mapper;
        private readonly IServiceFactory _serviceFactory;
        private readonly IMongoCollection<ReviewType> ReviewTypeCollection;
        private readonly IEmailSender _sender;


        public AppraisalConfigService(IDbContext context, ILogger<AppraisalConfigService> logger, IMapper mapper, IServiceFactory serviceFactory, IEmailSender sender)
        {
            Collection = context?.Database.GetCollection<AppraisalConfig>($"{nameof(AppraisalConfig)}s") ?? throw new ArgumentNullException(nameof(context));
            QueryableCollection = Collection.AsQueryable<AppraisalConfig>();
            _logger = logger;
            _Mapper = mapper;
            _serviceFactory = serviceFactory;
            _sender = sender;
            ReviewTypeCollection = context.Database.GetCollection<ReviewType>($"{nameof(ReviewType)}s");
        }

        public async Task<bool> Delete(ObjectId id)
        {
            var result = await Collection.DeleteOneAsync(Builders<AppraisalConfig>.Filter.Eq("Id", id));
            return result.DeletedCount > 0 ? true : false;
        }

        public async Task<IEnumerable<AppraisalConfig>> Get(AppraisalConfigParameters param)
        {
            if (param.Year != 0)
            {
                var result = await Collection.FindAsync(x => x.Year == param.Year);
                return result.ToList();
            }
            return await Collection.AsQueryable().ToListAsync();
        }

        public bool Insert(AppraisalConfig entity)
        {
            try
            {
                Collection.InsertOne(entity);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Insert of appraisal configuration failed", ex);
                return false;
            }
        }

        public async Task<AppraisalConfig> Update(ObjectId Id, AppraisalCycle entity)
        {
            try
            {
                var filter = Builders<AppraisalConfig>.Filter.Eq("Id", Id);
                var update = Builders<AppraisalConfig>.Update.Set("Cycles", entity);
                var result = await Collection.FindOneAndUpdateAsync(filter, update, options: new FindOneAndUpdateOptions<AppraisalConfig, AppraisalConfig> { ReturnDocument = ReturnDocument.After });
                if (result != null)
                {
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("update of appraisal configuration failed", ex);
                Console.WriteLine("failed to update  appraisal", ex.Message.ToString());
                return null;
            }
        }

        public AppraisalCycleForAppraisal GetActiveCycle()
        {
            return Collection.Find(c => c.Year == DateTime.Now.Year).ToList()
                .Select(x => new AppraisalCycleForAppraisal
                {
                    ConfigId = x.Id,
                    Cycle = x.Cycles
                   .Where(y => y.isActive == true)
                   .FirstOrDefault()
                }).FirstOrDefault();
        }

        public bool ActivateCycle(string cycleId)
        {
            var filter = Builders<AppraisalConfig>.Filter.Where(c => c.Year == DateTime.Now.Year);
            var appraisalConfig = Collection.Find(filter).First();

            appraisalConfig.Cycles.ForEach(c =>
            {

                if (c.Id == ObjectId.Parse(cycleId))
                {
                    c.isActive = true;
                }
                else
                {
                    c.isActive = false;
                }

            });
            return true;
        }

        public async Task<bool> EnableOrDisableAppraisal(ObjectId id)
        {
            var filter = Builders<AppraisalConfig>.Filter.Where(x => x.Id == id);
            var appraisalConfig = Collection.Find(filter).FirstOrDefault();

            if (appraisalConfig is null)
            {
                var cycleFilter = Builders<AppraisalConfig>.Filter.Where(x => x.Cycles.Any(y => y.Id == id));
                var configuration = (await Collection.FindAsync(cycleFilter)).FirstOrDefault();

                var cycle = configuration?.Cycles.FirstOrDefault(c => c.Id == id);
                if (cycle is null)
                {
                    throw new InvalidOperationException("Review Not Found");
                }

                cycle.isActive = cycle.isActive != true;
                if (cycle.isActive == true && cycle.isArchived == true)
                {
                    cycle.isArchived = false;
                }

                var entityToUpdate = configuration.ToBsonDocument();
                var update = new BsonDocument("$set", entityToUpdate);

                await Collection.UpdateOneAsync(cycleFilter, update);
            }
            else
            {
                appraisalConfig.isActive = appraisalConfig.isActive != true;
                if (appraisalConfig.isActive == true && appraisalConfig.IsArchived == true)
                {
                    appraisalConfig.IsArchived = false;
                }

                var entityToUpdate = appraisalConfig.ToBsonDocument();
                var update = new BsonDocument("$set", entityToUpdate);

                await Collection.UpdateOneAsync(filter, update);

            }
           return await SendMailForEnableOrDisableAppraisal(appraisalConfig);
        }

        private async Task<bool> SendMailForEnableOrDisableAppraisal(AppraisalConfig appraisalConfig)
        {
            string subject;

            if (appraisalConfig.isActive != null && appraisalConfig.isActive.Value)
            {
                subject = $"{appraisalConfig.Name} Has Started";

                if (appraisalConfig.Participants is null)
                {
                    throw new InvalidOperationException("Appraisal does not have any participants");
                }

                var participants = await GetParticipantDetails(appraisalConfig.Participants.Include);
                
                foreach (var participant in participants)
                {
                    var participantEmailDto = new SingleEmailDto
                    {
                        ReceiverFullName = participant.FullName,
                        ReceiverEmailAddress = participant.Email,
                        HtmlContent =
                            await _sender.FormatEmailEmployeeReviewEnabled(participant.FullName.Split(" ")[0],
                                appraisalConfig.Name, appraisalConfig.Duration.StopDate.ToLongDateString(), appraisalConfig.Period.To.ToLongDateString())
                    };

                    await _sender.SendToSingleEmployee(subject, participantEmailDto);
                }

                return true;
            }
            else
            {
                subject = $"{appraisalConfig.Name} Has Ended";

                if (appraisalConfig.Participants is null)
                {
                    throw new InvalidOperationException("Appraisal does not have any participants");
                }

                var participants = await GetParticipantDetails(appraisalConfig.Participants.Include);
                
                foreach (var participant in participants)
                {
                    var participantEmailDto = new SingleEmailDto
                    {
                        ReceiverFullName = participant.FullName,
                        ReceiverEmailAddress = participant.Email,
                        HtmlContent =
                            await _sender.FormatEmailEmployeeReviewDisabled(participant.FullName.Split(" ")[0],
                                appraisalConfig.Name)
                    };

                    await _sender.SendToSingleEmployee(subject, participantEmailDto);
                }

                return true;
            }
        }

        public async Task<bool> ArchiveAppraisal(ObjectId id)
        {
            var filter = Builders<AppraisalConfig>.Filter.Where((x) => x.Id == id);
            var appraisalConfig = Collection.Find(filter).FirstOrDefault();
            
            if (appraisalConfig is null)
            {
                //this block is to consider old appraisal configuration 
                var cycleFilter = Builders<AppraisalConfig>.Filter.Where(x => x.Cycles.Any(y => y.Id == id));
                var configuration = (await Collection.FindAsync(cycleFilter)).FirstOrDefault();

                var cycle = configuration?.Cycles.FirstOrDefault(c => c.Id == id);
                if (cycle is null)
                    return false;

                cycle.isArchived = true;
                cycle.isActive = false;

                var entityToUpdate = configuration.ToBsonDocument();
                var update = new BsonDocument("$set", entityToUpdate);

                await Collection.UpdateOneAsync(cycleFilter, update);
                return true;
            }
            else
            {
                appraisalConfig.IsArchived = true;
                appraisalConfig.isActive = false;
                var entityToUpdate = appraisalConfig.ToBsonDocument();
                var update = new BsonDocument("$set", entityToUpdate);

                await Collection.UpdateOneAsync(filter, update);
                return true;
            }
        }

        public void ActivateAppraisals(object state)
        {
            var filterBuilder = Builders<AppraisalConfig>.Filter;
            var tomorrow = DateTime.Today.AddDays(1);
            var filter = filterBuilder.Gte(x => x.Duration.StartDate, DateTime.Today) &
                         filterBuilder.Lt(x=>x.Duration.StartDate, tomorrow) &
                         filterBuilder.Gt(x => x.Duration.StopDate, DateTime.Today) &
                         filterBuilder.Where(a => a.isActive != true);

            var appraisalConfig = Collection.Find(filter).ToEnumerable().ToArray();
            if (appraisalConfig.Length == 0)
            {
                _logger.LogInformation("appraisal not found");
                return;
            }

            var updateData = new BsonDocument
            {
                {
                    "$set", new BsonDocument
                    {
                        {
                            "isActive", true
                        }
                    }
                }
            };


            Collection.UpdateMany(filter, updateData);
            //remove
            _logger.LogInformation("activated appraisals");
        }

        public void DeActivateAppraisals(object state)
        {
            var filterBuilder = Builders<AppraisalConfig>.Filter;
            var filter = filterBuilder.Lte(x => x.Duration.StopDate, DateTime.Today) &
                         filterBuilder.Where(a => a.isActive != false);

            var appraisalConfig = Collection.Find(filter).ToEnumerable().ToArray();
            if (appraisalConfig.Length == 0)
            {
                _logger.LogInformation("appraisal not found");
                return;
            }

            var updateData = new BsonDocument
            {
                {
                    "$set", new BsonDocument
                    {
                        {
                            "isActive", false
                        }
                    }
                }
            };


            Collection.UpdateMany(filter, updateData);
            //remove
            _logger.LogInformation("deactivated appraisals");
        }

        public bool DeactivateCycle(ObjectId configId, AppraisalCycle cycle)
        {
            try
            {
                var filter = Builders<AppraisalConfig>.Filter.Eq("Id", configId);
                var config = Collection.Find(filter).First();
                if (config != null)
                {
                    var activeCycle = config.Cycles.First(c => c.Id == cycle.Id);
                    activeCycle.isActive = false;

                    var update = config.ToBsonDocument();
                    Collection.UpdateOne(filter, update);

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ActivateCycle(AppraisalConfig appraisalConfig, ObjectId cycleId)
        {
            try
            {
                var filter = Builders<AppraisalConfig>.Filter.Eq("Id", appraisalConfig.Id);
                if (appraisalConfig != null)
                {
                    var activeCycle = appraisalConfig.Cycles.First(c => c.Id == cycleId);
                    activeCycle.isActive = true;

                    var update = appraisalConfig.ToBsonDocument();
                    Collection.UpdateOne(filter, update);

                    return true;
                }

                return false;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<ConfigAppraisalViewDto>> GetReviews(AppraisalConfigParameters param)
        {
            IList<ConfigAppraisalViewDto> configAppraisals = new List<ConfigAppraisalViewDto>();
            if (param.Year != 0)
            {
                var result = (await Collection.FindAsync(x => x.Year == param.Year)).ToList();

                result.ForEach(x =>
                {
                    if (x.Cycles != null)
                    {
                        foreach (var item in x.Cycles)
                        {
                            ConfigAppraisalViewDto config = GetConfigAppraisalViewDto(item, param.Year);
                            configAppraisals.Add(config);
                        }
                    }
                    else
                    {
                        var config = _Mapper.Map<ConfigAppraisalViewDto>(x);
                        configAppraisals.Add(config);
                    }
                });
            }

            return configAppraisals;
        }

        private ConfigAppraisalViewDto GetConfigAppraisalViewDto(AppraisalCycle cycle, int year)
        {
            var config = _Mapper.Map<ConfigAppraisalViewDto>(cycle);
            config.Year = year;
            config.Duration = new Duration
            {
                StartDate = cycle.StartDate,
                StopDate = cycle.StopDate
            };
            config.ReviewType = new ReviewTypeDto
            {
                Name = "Performance Based Review"
            };

            if (config.Participants == null)
            {
                config.Participants = new Participants
                {
                    Include = new List<string> { "Everyone" }
                };
            }

            return config;
        }
        public async Task<ConfigAppraisalWithParticipantDetail> GetSingleReview(string Id)
        {
            var id = ObjectId.Parse(Id);
            var review = (await Collection.FindAsync(x => x.Id == id)).FirstOrDefault();
            if (review == null)
            {
                //this block is to consider old appraisal configuration 
                var configuration = (await Collection.FindAsync(x => x.Cycles.Any(y => y.Id == id))).FirstOrDefault();
                if (configuration != null)
                {
                    var cycle = configuration.Cycles.Where(c => c.Id == id).FirstOrDefault();

                    var config = _Mapper.Map<ConfigAppraisalViewDto>(cycle);
                    config.Year = configuration.Year;
                    config.Duration = new Duration
                    {
                        StartDate = cycle.StartDate,
                        StopDate = cycle.StopDate
                    };
                    config.ReviewType = new ReviewTypeDto
                    {
                        Name = "Performance Based Review"
                    };

                    ConfigAppraisalWithParticipantDetail appraisalWithParticipantDetailForCycel = new ConfigAppraisalWithParticipantDetail
                    {
                        configAppraisal = config,
                        participants = null
                    };

                    return appraisalWithParticipantDetailForCycel;
                }
            }
            var reviewToReturn = _Mapper.Map<ConfigAppraisalViewDto>(review);

            ConfigAppraisalWithParticipantDetail appraisalWithParticipantDetail = new ConfigAppraisalWithParticipantDetail
            {
                configAppraisal = reviewToReturn,
                participants = reviewToReturn.Participants == null ? null : new ParticipantsWithDetail
                {
                    Include = await GetParticipantDetails(reviewToReturn.Participants?.Include),
                    Exclude = await GetParticipantDetails(reviewToReturn.Participants?.Exclude)
                }
            };

            return appraisalWithParticipantDetail;
        }

        public string InsertReview(ConfigAppraisalDto appraisalDto)
        {
            if (appraisalDto == null)
            {
                throw new InvalidOperationException("Invalid Data sumbitted");
            }

            if (string.IsNullOrEmpty(appraisalDto.Name) || appraisalDto.ReviewType == null || appraisalDto.Period == null)
            {
                throw new InvalidOperationException("Title, Type, Duaration, Period in review can not be empty");
            }

            bool isValidReview = IsReviewTypeValid(appraisalDto.ReviewType.Id);
            if (!isValidReview)
            {
                throw new InvalidOperationException("Review type not found");
            }


            var entity = _Mapper.Map<AppraisalConfig>(appraisalDto);
            entity.Participants = appraisalDto.Participants;
            entity.Completed = null;
            entity.isActive = null;

            Collection.InsertOne(entity);
            return $"You have successfully created {appraisalDto.Name}";
        }

        public AppraisalConfig UpdateAppraisalReview(string Id, JsonPatchDocument<AppraisalConfigForUpdateDto> model)
        {
            var id = ObjectId.Parse(Id);

            var filter = Builders<AppraisalConfig>.Filter.Where(x => x.Id == id);
            var review = Collection.Find(filter).FirstOrDefault();

            if (review == null)
                return null;

            var mapreview = _Mapper.Map<AppraisalConfigForUpdateDto>(review);
            model.ApplyTo(mapreview);

            var updatedReview = _Mapper.Map<AppraisalConfig>(mapreview);
            var update = new BsonDocument("$set", updatedReview.ToBsonDocument());

            var result = Collection.FindOneAndUpdate<AppraisalConfig>(filter, update, options: new FindOneAndUpdateOptions<AppraisalConfig> { ReturnDocument = ReturnDocument.After });

            return result;
        }

        public async Task<IEnumerable<NameEmailWithFullName>> GetParticipantDetails(IEnumerable<string> participants)
        {
            IEnumerable<NameEmailWithFullName> employeeDetails = new List<NameEmailWithFullName>();
            if (participants.Any())
            {
                ITeamRepository _teamRepository = _serviceFactory.GetServices<ITeamRepository>();
                if (participants.First().Equals("Everyone", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Participants for everyone not return because of size of data and it's not necessary since we have endpoint to return that
                    var employeeIds = (await _teamRepository.GetEmployeeIDs()).Select(x => x.ToString());

                    var participantsDetail = await _teamRepository.FetchEmployeesDetailsFromEmployeeService(employeeIds);
                    employeeDetails = _Mapper.Map<IEnumerable<NameEmailWithFullName>>(participantsDetail);

                    return employeeDetails;
                }
                else
                {
                    var participantsDetail = await _teamRepository.FetchEmployeesDetailsFromEmployeeService(participants);
                    employeeDetails = _Mapper.Map<IEnumerable<NameEmailWithFullName>>(participantsDetail);
                }

            }
            return employeeDetails;
        }

        public bool InsertReviewType(string name)
        {
            try
            {
                var review = new ReviewType();
                review.Name = name;
                ReviewTypeCollection.InsertOne(review);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ReviewType> GetReviewType(ObjectId Id)
        {
            if (Id != null)
            {
                var result = ReviewTypeCollection.Find(x => x.Id == Id).FirstOrDefault();
                return result;
            }
            return null;
        }

        public async Task<List<ReviewType>> GetReviewTypes()
        {
            var result = ReviewTypeCollection.AsQueryable();
            return result.ToList();
        }

        public async Task<ConfigAppraisalViewDto> GetSingleReviewWithoutParticipant(ObjectId Id)
        {
            var review = (await Collection.FindAsync(x => x.Id == Id)).FirstOrDefault();
            if (review == null)
            {
                //this block is to consider old appraisal configuration 
                var configuration = (await Collection.FindAsync(x => x.Cycles.Any(y => y.Id == Id))).FirstOrDefault();
                if (configuration != null)
                {
                    var cycle = configuration.Cycles.Where(c => c.Id == Id).FirstOrDefault();

                    var config = _Mapper.Map<ConfigAppraisalViewDto>(cycle);
                    config.Year = configuration.Year;
                    config.Duration = new Duration
                    {
                        StartDate = cycle.StartDate,
                        StopDate = cycle.StopDate
                    };
                    config.ReviewType = new ReviewTypeDto
                    {
                        Name = "Performance Based Review"
                    };

                    return config;
                }
            }
            return _Mapper.Map<ConfigAppraisalViewDto>(review);
        }

        public async Task<IEnumerable<ConfigAppraisalViewDto>> GetEmployeeReviews(int employeeId, int? year)
        {
            IList<ConfigAppraisalViewDto> configAppraisals = new List<ConfigAppraisalViewDto>();
            int currentYear = year ?? DateTime.Now.Year;
            var filterBuilder = Builders<AppraisalConfig>.Filter;
            var filter = filterBuilder.Where(a => a.Year == currentYear) &
                         (filterBuilder.AnyEq("Participants.Include", employeeId.ToString()) |
                          filterBuilder.AnyEq("Participants.Include", "everyone") |
                          filterBuilder.Where(x => x.Participants == null));


            var employeeReviews = (await Collection.FindAsync(filter)).ToList();

            employeeReviews.ForEach(x =>
            {
                if (x.Cycles != null)
                {
                    foreach (var item in x.Cycles)
                    {
                        ConfigAppraisalViewDto config = GetConfigAppraisalViewDto(item, currentYear);
                        configAppraisals.Add(config);
                    }
                }
                else
                {
                    var config = _Mapper.Map<ConfigAppraisalViewDto>(x);
                    configAppraisals.Add(config);
                }
            });

            return await GetEmployeeReviewStatus(employeeId, configAppraisals);
        }

        public bool IsReviewTypeValid(string Id)
        {
            ObjectId id = ObjectId.Parse(Id);

            return ReviewTypeCollection.AsQueryable().Any(x => x.Id == id);
        }

        public async Task<IEnumerable<ConfigAppraisalViewDto>> GetEmployeeReviewStatus(int empId, IEnumerable<ConfigAppraisalViewDto> reviews)
        {
            IAppraisalResult _appraisalResult = _serviceFactory.GetServices<IAppraisalResult>();
            reviews.ToList().ForEach(async x =>
            {
                var result = await _appraisalResult.Get(x.Id.ToString(), empId);
                if (!result.Any())
                {
                    x.Status = "";
                }
                else
                {
                    x.Status = (result.All(x => x.NextAppraisee.Contains("Done", StringComparison.CurrentCultureIgnoreCase))) ? "Approved" 
                    : result.Any(x => x.NextAppraisee.Contains("Hod", StringComparison.CurrentCultureIgnoreCase)) ? "Pending Approval" 
                    : result.Any(x => x.NextAppraisee.Contains("Appraiser", StringComparison.CurrentCultureIgnoreCase)) ? "Pending Review" : "" ;
                }
            });

            return await Task.FromResult(reviews);
        }

    }
}
