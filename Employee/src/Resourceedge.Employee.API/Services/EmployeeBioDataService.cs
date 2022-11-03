using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Resourceedge.Common.Archive;
using Resourceedge.Common.Util;
using Resourceedge.Employee.Domain.DbContext;
using Resourceedge.Employee.Domain.Entities;
using Resourceedge.Employee.Domain.Interfaces;
using Resourceedge.Employee.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Employee.API.Services
{
    public class EmployeeBioDataService : IEmployee
    {
        private readonly ILogger<EmployeeBioData> logger;
        private readonly IMongoCollection<EmployeeBioData> Collection;
        private readonly IMongoCollection<LastEmployeeNumber> LastEmployeeNumberCollection;
        private readonly IQueryable<EmployeeBioData> QueryableCollection;
        private readonly IMapper mapper;

        public EmployeeBioDataService(ILogger<EmployeeBioData> _logger, IDbContext _dbContext, IMapper _mapper)
        {
            if (_logger != null && _dbContext != null)
            {
                this.logger = _logger;
                Collection = _dbContext.Database.GetCollection<EmployeeBioData>("Employees");
                LastEmployeeNumberCollection = _dbContext.Database.GetCollection<LastEmployeeNumber>("LastEmployeeNumber");
                QueryableCollection = Collection.AsQueryable();
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }

            mapper = _mapper;
        }

        public bool InsertEmployeeBioData(EmployeeBioData employeeBioData)
        {
            if (employeeBioData == null)
                return false;

            if (QueryableCollection.Any(x => x.Email == employeeBioData.Email))
                return false;

            Collection.InsertOne(employeeBioData);
            return true;

        }

        public async Task<EmployeeBioData> UpdateEmployeeBioData(int empId, JsonPatchDocument<EmployeeBioDataDto> model)
        {
            var filter = Builders<EmployeeBioData>.Filter.Where(x => x.EmployeeId == empId);
            var empBioData = Collection.Find(filter).FirstOrDefault();

            if (empBioData == null)
                return null;

            var bioDataDto = mapper.Map<EmployeeBioDataDto>(empBioData);
            model.ApplyTo(bioDataDto);

            var bioDataForUpdate = mapper.Map<EmployeeBioData>(bioDataDto);
            bioDataForUpdate.Id = empBioData.Id;
            var update = new BsonDocument("$set", bioDataForUpdate.ToBsonDocument());

            var result = await Collection.FindOneAndUpdateAsync<EmployeeBioData>(filter, update, options: new FindOneAndUpdateOptions<EmployeeBioData> { ReturnDocument = ReturnDocument.After });

            return result;
        }

        public async Task<EmployeeBioDataDto> GetEmployeeBioData(int? empId)
        {
            if (empId == null) return null;

            var filter = Builders<EmployeeBioData>.Filter.Where(e => e.EmployeeId == empId.Value);
            var result = await Collection.FindAsync(filter);

            if (result == null) return null;

            var entityToReturn = mapper.Map<EmployeeBioDataDto>(result.FirstOrDefault());

            return entityToReturn;
        }

        public bool HasFilledBiodata(int empId)
        {
            return QueryableCollection.Any(x => x.EmployeeId == empId);
        }

        public Tuple<PagedList<OldEmployeeForViewDto>, int> Get(int pageNumber, int pageSize, string company, string searchParam)
        {
            var employeeList = company != null && searchParam != null ? QueryableCollection
                        .Where(e => e.Company == company.ToUpper() && e.FullName.Contains(searchParam.ToUpper()))
                        .Select(x => new OldEmployeeForViewDto { Email = x.Email, EmployeeId = x.EmployeeId, FullName = x.FullName, JobRole = x.JobRole, SubGroup = x.Company })
                        .OrderBy(x => x.FullName)
                        : company != null ? QueryableCollection
                        .Where(e => e.Company == company.ToUpper())
                        .Select(x => new OldEmployeeForViewDto { Email = x.Email, EmployeeId = x.EmployeeId, FullName = x.FullName, JobRole = x.JobRole, SubGroup = x.Company })
                        .OrderBy(x => x.FullName)
                        : searchParam != null ? QueryableCollection
                        .Where(e => e.FullName.Contains(searchParam.ToUpper()))
                        .Select(x => new OldEmployeeForViewDto { Email = x.Email, EmployeeId = x.EmployeeId, FullName = x.FullName, JobRole = x.JobRole, SubGroup = x.Company })
                        .OrderBy(x => x.FullName)
                        : QueryableCollection
                        .Select(x => new OldEmployeeForViewDto { Email = x.Email, EmployeeId = x.EmployeeId, FullName = x.FullName, JobRole = x.JobRole, SubGroup = x.Company })
                        .OrderBy(x => x.FullName);

            var employeecount = employeeList.Count();
            var employeePaged = PagedList<OldEmployeeForViewDto>.Create(employeeList, pageNumber, pageSize);

            return Tuple.Create(employeePaged, employeecount);
        }

        public IEnumerable<string> GetOrganisation()
        {
            return QueryableCollection.Select(x => x.Company).Distinct().ToList();
        }

        public async Task<(bool, string)> AddNewEmployee(EmployeeCreateDto employee)
        {
            try
            {
                if (employee == null)
                    return (false, "Empty or Invalid data submitted");

                var employeeToCreate = mapper.Map<EmployeeBioData>(employee);

                //Generate new EmployeeId from last Id of old system
                var lastEmployeeId = LastEmployeeNumberCollection.AsQueryable().FirstOrDefault();
                ++lastEmployeeId.EmployeeId;
                employeeToCreate.EmployeeId = lastEmployeeId.EmployeeId;

                await Collection.InsertOneAsync(employeeToCreate);

                LastEmployeeNumberCollection.UpdateOne(Builders<LastEmployeeNumber>.Filter.Eq("Id", lastEmployeeId.Id),
                                             Builders<LastEmployeeNumber>.Update.Set("EmployeeId", lastEmployeeId.EmployeeId));

                return (true, "Employee Added successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> AddNewEmployeeMany(IEnumerable<EmployeeCreateDto> employees)
        {
            try
            {
                if (!employees.Any())
                    return false;

                var lastEmployeeId = LastEmployeeNumberCollection.AsQueryable().FirstOrDefault();
                ++lastEmployeeId.EmployeeId;

                var employeesToAdd = mapper.Map<IEnumerable<EmployeeBioData>>(employees);
                employeesToAdd.ToList().ForEach(x => x.EmployeeId = ++lastEmployeeId.EmployeeId);

                await Collection.InsertManyAsync(employeesToAdd);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CheckIfExcelFile(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".xlsx" || extension == ".xls");
        }

        public async Task<IEnumerable<EmployeeCreateDto>> ReadExcel(IFormFile file)
        {
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = DateTime.Now.Ticks + extension; //Create a new Name for the file due to security reasons.

                var pathBuilt = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                IList<EmployeeCreateDto> employees = new List<EmployeeCreateDto>();
                ISheet sheet;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files",
                   fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);

                    stream.Position = 0;
                    if (extension == ".xls")
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(0);
                    }
                    else
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream);
                        sheet = hssfwb.GetSheetAt(0);
                    }

                    IRow headerRow = sheet.GetRow(0);
                    int cellCount = headerRow.LastCellNum;
                    if (CheckifHeaderMatch(headerRow).Item1)
                    {
                        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                        {
                            IRow row = sheet.GetRow(i);
                            if (row == null) continue;
                            if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

                            var employee = new EmployeeCreateDto
                            {
                                StaffId = row.GetCell(0).ToString(),
                                FullName = row.GetCell(1).ToString(),
                                Email = row.GetCell(2).ToString(),
                                Company = row.GetCell(3).ToString(),
                                Department = row.GetCell(4).ToString(),
                                Level = row.GetCell(5).ToString(),
                                Location = row.GetCell(6).ToString(),
                                EmploymentType = row.GetCell(7).ToString(),
                                ResumptionDate = row.GetCell(8).ToString(),
                            };

                            employees.Add(employee);
                        }
                                               
                    }
                }
                return employees;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public (bool, string) CheckifHeaderMatch(IRow headerRow)
        {

            return (true, "All column match");
        }
    }
}
