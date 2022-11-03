using MongoDB.Bson;

namespace Resourceedge.Appraisal.API.DBQueries
{


    public class KeyResultAreaQueries
    {



        public static BsonDocument[] GetEmployeesEPA(int year)
        {

            //var aa = 

            var match = new BsonDocument
            {
                {
                    "$match", new BsonDocument
                    {
                        {
                            $"Year", year
                        }
                    }
                },
            };



            var group = new BsonDocument
            {
                {
                    "$group", new BsonDocument
                    {

                        {"_id", "$EmployeeId" },
                        {
                               "details", new BsonDocument
                                {
                                    {"$push","$$ROOT"}
                                }
                        }
                    }
                }
            };

            var arrayFilter1 = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"EmployeeId", "$id" },
                        {"EmployeeDetail", new BsonDocument{
                            {"$first", "$details" }
                        }
                        }
                    }
                }
            };

            var arrayFilter2 = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"EmployeeId", "$id" },
                        {"Approved", "$EmployeeDetail.Approved"},
                        { "Total","$totalNumber"}

                    }
                }
            };

            var teamLeadLookUp = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument
                    {
                        {"from", "TeamLeads" },
                        {"localField", "_id" },
                        {"foreignField", "EmployeeId" },
                        {"as", "TeamLead" }
                    }
                }
            };

            var arrayFilter3 = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {"EmployeeId", 1 },
                        {"Approved", 1},
                        {"Total", 1 },
                        {
                            "TeamLead", new BsonDocument
                            {
                                {"$first", "$TeamLead" }
                            }
                        },

                    }
                }
            };

            //var skip = new BsonDocument
            //{
            //    {
            //        "$skip", toSkip
            //    }
            //};

            //var limit = new BsonDocument
            //{
            //    {
            //        "$limit", toLimit
            //    }
            //};

            var sort = new BsonDocument
            {
                {
                    "$sort", new BsonDocument
                    {
                        {
                            "_id", 1
                        }
                    }
                }
            };

            return new[] { match, group, arrayFilter1, arrayFilter2, teamLeadLookUp, arrayFilter3, sort };
        }
    }
}
