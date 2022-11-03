using MongoDB.Bson;
using System;

namespace Resourceedge.Appraisal.API.DBQueries
{
    public static class SupervisorEmployeeQuery
    {
        public static BsonDocument[] GetEmployeesToAppraise(int employeeId, string appraisalConfigurationId, string appraisalCycleId)
        {
            var year = DateTime.Now.Year;
            var firstMatch = new BsonDocument
            {
                {
                    "$match", new BsonDocument
                    {
                        {
                            "$and", new BsonArray
                            {
                                new BsonDocument
                                {
                                    {
                                        "$or", new BsonArray
                                        {
                                            new BsonDocument
                                            {
                                                {"HodDetails.EmployeeId", employeeId }
                                            },
                                            new BsonDocument
                                            {
                                                {"AppraiserDetails.EmployeeId", employeeId }
                                            }
                                        }
                                    },
                                },
                                new BsonDocument
                                {
                                    {
                                        "Year", new BsonDocument
                                        {
                                            {"$eq",  year}
                                    }   }
                                }
                            }

                        }
                    }

                },
            };

            var project = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {
                            "EmployeeDetail", new BsonDocument
                            {
                                { "EmployeeId" , "$EmployeeId" }
                            }
                        }
                    }
                }
            };

            var lookup = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument{
                        {"from", "AppraisalResults" },
                        {"localField", "_id"},
                        {"foreignField", "KeyResultArea._id" },
                        {"as", "Kra_Details" }
                    }
                }
            };


            var project2 = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {
                            "Kra_Details", new BsonDocument
                            {
                                {
                                    "$filter" , new BsonDocument
                                    {
                                        { "input", "$Kra_Details"},
                                        {"as", "item" },
                                        {
                                            "cond", new BsonDocument
                                            {
                                                {
                                                    "$and",new BsonArray
                                                    {
                                                        new BsonDocument
                                                        {
                                                            {"$eq",
                                                                new BsonArray(new dynamic[] { "$$item.AppraisalConfigId", ObjectId.Parse(appraisalConfigurationId)})
                                                            }
                                                        },
                                                          new BsonDocument
                                                          {
                                                               {"$eq",
                                                                  new BsonArray(new dynamic[] { "$$item.AppraisalCycleId", ObjectId.Parse(appraisalCycleId)})
                                                               }
                                                          }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        { "EmployeeDetail", "$EmployeeDetail"},
                        {"appraisalconfiguration", ObjectId.Parse(appraisalConfigurationId) },
                        {"appraisalCycle",  ObjectId.Parse(appraisalCycleId)}
                    }
                }
            };

            var lookup2 = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument
                    {
                       {"from", "FinalAppraisalResults" },
                        {"let",  new BsonDocument
                            {
                                {"config", "$appraisalconfiguration" },
                                {"cycle","$appraisalCycle" },
                                {"employeeId", "$EmployeeDetail.EmployeeId" }
                            } 
                        },
                        {
                            "pipeline", new BsonArray
                             {
                                new BsonDocument
                                {
                                    {"$match", new BsonDocument
                                        {
                                            {"$expr" , new BsonDocument
                                                {
                                                    {
                                                        "$and", new BsonArray
                                                        {
                                                            new BsonDocument 
                                                            {{
                                                                "$eq" , new BsonArray(new dynamic[] { "$$config", "$AppraisalConfigId"})
                                                            }},
                                                            new BsonDocument {{
                                                                    "$eq", new BsonArray(new dynamic[] { "$$cycle", "$AppraisalCycleId"})
                                                                }},
                                                             new BsonDocument {{
                                                                    "$eq", new BsonArray(new dynamic[] { "$$employeeId", "$EmployeeId"})
                                                                }}
                                                        }
                                                    }
                                                }
                                            } }
                                        }
                                }
                            } 
                        },
                        //{"localField", "EmployeeDetail.EmployeeId"},
                        //{"foreignField", "EmployeeId" },
                        {"as", "Temp_Appraisal_Result" }
                    }
                }
            };

            var project3 = new BsonDocument(allowDuplicateNames: true)
            {
                {
                    "$project", new BsonDocument(allowDuplicateNames: true)
                    {
                        {
                            "Temp_Appraisal_Result", new BsonDocument
                            {
                                {
                                    "$filter" , new BsonDocument
                                    {
                                        { "input", "$Temp_Appraisal_Result"},
                                        {"as", "item" },
                                        {
                                            "cond", new BsonDocument
                                            {
                                                {
                                                    "$and",new BsonArray
                                                    {
                                                        new BsonDocument
                                                        {
                                                            {"$eq",
                                                                new BsonArray(new dynamic[] { "$$item.AppraisalConfigId", ObjectId.Parse(appraisalConfigurationId)})
                                                            }
                                                        },
                                                          new BsonDocument
                                                          {
                                                               {"$eq",
                                                                  new BsonArray(new dynamic[] { "$$item.AppraisalCycleId", ObjectId.Parse(appraisalCycleId)})
                                                               }
                                                          }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                         { "EmployeeDetail", "$EmployeeDetail"},
                        {"Kra_Details", "$Kra_Details" },
                        {
                            "Appraisal_Result", new BsonDocument
                            {
                                 {
                                    "$arrayElemAt", new BsonArray(new dynamic[] {"$Temp_Appraisal_Result", 0 })
                            }
                        }
                       }
                    }
                }
            };

            var project4 = new BsonDocument
            {
                {
                    "$project", new BsonDocument{
                       { "Appraisal_Result", new BsonDocument{
                         { "EmployeeId" , "$EmployeeDetail.EmployeeId" },
                           {"AppraisalConfigId",  ObjectId.Parse(appraisalConfigurationId) },
                           {"AppraisalCycleId", ObjectId.Parse(appraisalCycleId) },
                           { "EmployeeResult", "$Appraisal_Result.EmployeeResult"},
                           {"FinalResult",  "$Appraisal_Result.FinalResult" },
                           {"AppraiseeResult",  "$Appraisal_Result.AppraiseeResult" },
                        }
                     },
                        { "EmployeeDetail", "$EmployeeDetail"},
                        {"Kra_Details", "$Kra_Details" },
                    }

                }
            };

            return new[] { firstMatch, project, lookup, project2, lookup2, project3, project4 };
        }


        public static BsonDocument[] GetEmployeesToAppraiseForHod(int employeeId, string appraisalCycleId)
        {
            var year = DateTime.Now.Year;
            var firstMatch = new BsonDocument
            {
                {
                    "$match", new BsonDocument
                    {
                        {
                            "$and", new BsonArray
                            {
                                new BsonDocument
                                {

                                    {
                                        "HodDetails.EmployeeId", employeeId
                                    }

                                },
                                new BsonDocument
                                {
                                    {
                                        "Year", new BsonDocument
                                        {
                                            {"$eq",  year}
                                    }   }
                                }
                            }

                        }
                    }

                },
            };

            var project = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {
                            "EmployeeDetail", new BsonDocument
                            {
                                { "EmployeeId" , "$EmployeeId" }
                            }
                        }
                    }
                }
            };

            var lookup = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument{
                        {"from", "AppraisalResults" },
                        {"localField", "_id"},
                        {"foreignField", "KeyResultArea._id" },
                        {"as", "Kra_Details" }
                    }
                }
            };


            var project2 = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {
                            "Kra_Details", new BsonDocument
                            {
                                {
                                    "$filter" , new BsonDocument
                                    {
                                        { "input", "$Kra_Details"},
                                        {"as", "item" },
                                        {
                                            "cond", new BsonDocument
                                            {
                                                {
                                                    "$eq",new BsonArray(
                                                    new dynamic[]
                                                    {
                                                            "$$item.AppraisalCycleId", ObjectId.Parse(appraisalCycleId)
                                                    })
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        { "EmployeeDetail", "$EmployeeDetail"},
                        {"appraisalCycle",  ObjectId.Parse(appraisalCycleId)}
                    }
                }
            };

            var lookup2 = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument
                    {
                       {"from", "FinalAppraisalResults" },
                        {"let",  new BsonDocument{
                            {"config", "$appraisalconfiguration" },
                            {"cycle","$appraisalCycle" },
                            {"employeeId", "$EmployeeDetail.EmployeeId" }
                        } },
                        {"pipeline", new BsonArray
                         {
                            new BsonDocument
                            {
                                {"$match", new BsonDocument
                                {
                                    {"$expr" , new BsonDocument{
                                        {
                                            "$and", new BsonArray
                                            {
                                                new BsonDocument {{
                                                        "$eq", new BsonArray(new dynamic[] { "$$cycle", "$AppraisalCycleId"})
                                                    }},
                                                 new BsonDocument {{
                                                        "$eq", new BsonArray(new dynamic[] { "$$employeeId", "$EmployeeId"})
                                                 }}
                                            }
                                        }
                                    }
                                    } }
                                }
                            }
                        } },
                        //{"localField", "EmployeeDetail.EmployeeId"},
                        //{"foreignField", "EmployeeId" },
                        {"as", "Temp_Appraisal_Result" }
                    }
                }
            };

            var project3 = new BsonDocument(allowDuplicateNames: true)
            {
                {
                    "$project", new BsonDocument(allowDuplicateNames: true)
                    {
                        {
                            "Temp_Appraisal_Result", new BsonDocument
                            {
                                {
                                    "$filter" , new BsonDocument
                                    {
                                        { "input", "$Temp_Appraisal_Result"},
                                        {"as", "item" },
                                        {
                                            "cond", new BsonDocument
                                            {
                                                {
                                                    "$eq", new BsonArray(
                                                        new dynamic[]
                                                        {
                                                            "$$item.AppraisalCycleId", ObjectId.Parse(appraisalCycleId)
                                                        })
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                         { "EmployeeDetail", "$EmployeeDetail"},
                        {"Kra_Details", "$Kra_Details" },
                        {
                            "Appraisal_Result", new BsonDocument
                            {
                                 {
                                    "$arrayElemAt", new BsonArray(new dynamic[] {"$Temp_Appraisal_Result", 0 })
                            }
                        }
                       }
                    }
                }
            };

            var project4 = new BsonDocument
            {
                {
                    "$project", new BsonDocument{
                       { "Appraisal_Result", new BsonDocument{
                         { "EmployeeId" , "$EmployeeDetail.EmployeeId" },
                           {"AppraisalCycleId", ObjectId.Parse(appraisalCycleId) },
                           { "EmployeeResult", "$Appraisal_Result.EmployeeResult"},
                           {"FinalResult",  "$Appraisal_Result.FinalResult" },
                           {"AppraiseeResult",  "$Appraisal_Result.AppraiseeResult" },
                        }
                     },
                        { "EmployeeDetail", "$EmployeeDetail"},
                        {"Kra_Details", "$Kra_Details" },
                    }

                }
            };

            return new[] { firstMatch, project, lookup, project2, lookup2, project3, project4 };
        }

        public static BsonDocument[] GetEmployeesToAppraiseForAppraiser(int employeeId, string appraisalCycleId)
        {
            var year = DateTime.Now.Year;
            var firstMatch = new BsonDocument
            {
                {
                    "$match", new BsonDocument
                    {
                        {
                            "$and", new BsonArray
                            {
                                new BsonDocument
                                {
                                     {
                                        "AppraiserDetails.EmployeeId", employeeId
                                     }

                                },
                                new BsonDocument
                                {
                                    {
                                        "Year", new BsonDocument
                                        {
                                            {"$eq",  year}
                                    }   }
                                }
                            }

                        }
                    }

                },
            };

            var project = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {
                            "EmployeeDetail", new BsonDocument
                            {
                                { "EmployeeId" , "$EmployeeId" }
                            }
                        }
                    }
                }
            };

            var lookup = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument{
                        {"from", "AppraisalResults" },
                        {"localField", "_id"},
                        {"foreignField", "KeyResultArea._id" },
                        {"as", "Kra_Details" }
                    }
                }
            };


            var project2 = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        {
                            "Kra_Details", new BsonDocument
                            {
                                {
                                    "$filter" , new BsonDocument
                                    {
                                        { "input", "$Kra_Details"},
                                        {"as", "item" },
                                        {
                                            "cond", new BsonDocument
                                            {
                                                {
                                                    "$eq",new BsonArray(
                                                    new dynamic[]
                                                    {
                                                            "$$item.AppraisalCycleId", ObjectId.Parse(appraisalCycleId)
                                                    })
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        { "EmployeeDetail", "$EmployeeDetail"},
                        {"appraisalCycle",  ObjectId.Parse(appraisalCycleId)}
                    }
                }
            };

            var lookup2 = new BsonDocument
            {
                {
                    "$lookup", new BsonDocument
                    {
                       {"from", "FinalAppraisalResults" },
                        {"let",  new BsonDocument{
                            {"config", "$appraisalconfiguration" },
                            {"cycle","$appraisalCycle" },
                            {"employeeId", "$EmployeeDetail.EmployeeId" }
                        } },
                        {"pipeline", new BsonArray
                         {
                            new BsonDocument
                            {
                                {"$match", new BsonDocument
                                {
                                    {"$expr" , new BsonDocument{
                                        {
                                            "$and", new BsonArray
                                            {
                                                new BsonDocument {{
                                                        "$eq", new BsonArray(new dynamic[] { "$$cycle", "$AppraisalCycleId"})
                                                    }},
                                                 new BsonDocument {{
                                                        "$eq", new BsonArray(new dynamic[] { "$$employeeId", "$EmployeeId"})
                                                 }}
                                            }
                                        }
                                    }
                                    } }
                                }
                            }
                        } },
                        //{"localField", "EmployeeDetail.EmployeeId"},
                        //{"foreignField", "EmployeeId" },
                        {"as", "Temp_Appraisal_Result" }
                    }
                }
            };

            var project3 = new BsonDocument(allowDuplicateNames: true)
            {
                {
                    "$project", new BsonDocument(allowDuplicateNames: true)
                    {
                        {
                            "Temp_Appraisal_Result", new BsonDocument
                            {
                                {
                                    "$filter" , new BsonDocument
                                    {
                                        { "input", "$Temp_Appraisal_Result"},
                                        {"as", "item" },
                                        {
                                            "cond", new BsonDocument
                                            {
                                                {
                                                    "$eq", new BsonArray(
                                                        new dynamic[]
                                                        {
                                                            "$$item.AppraisalCycleId", ObjectId.Parse(appraisalCycleId)
                                                        })
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                         { "EmployeeDetail", "$EmployeeDetail"},
                        {"Kra_Details", "$Kra_Details" },
                        {
                            "Appraisal_Result", new BsonDocument
                            {
                                 {
                                    "$arrayElemAt", new BsonArray(new dynamic[] {"$Temp_Appraisal_Result", 0 })
                            }
                        }
                       }
                    }
                }
            };

            var project4 = new BsonDocument
            {
                {
                    "$project", new BsonDocument{
                       { "Appraisal_Result", new BsonDocument{
                         { "EmployeeId" , "$EmployeeDetail.EmployeeId" },
                           {"AppraisalCycleId", ObjectId.Parse(appraisalCycleId) },
                           { "EmployeeResult", "$Appraisal_Result.EmployeeResult"},
                           {"FinalResult",  "$Appraisal_Result.FinalResult" },
                           {"AppraiseeResult",  "$Appraisal_Result.AppraiseeResult" },
                        }
                     },
                        { "EmployeeDetail", "$EmployeeDetail"},
                        {"Kra_Details", "$Kra_Details" },
                    }

                }
            };

            return new[] { firstMatch, project, lookup, project2, lookup2, project3, project4 };
        }

        public static BsonDocument[] GetParticipants(ObjectId cycleId)
        {
            var year = DateTime.Now.Year.ToString();
            var match = new BsonDocument
            {
                {
                    "$match" ,
                    new BsonDocument
                    {
                        {$"AppraisalCycleId", cycleId },
                        {$"Year",  year}
                    }
                }
            };

            var project = new BsonDocument
            {
                {
                    "$project", new BsonDocument{
                        { "EmployeeId" , "$EmployeeId" },
                        {"FinalResult" , "$FinalResult" },
                        {"EmployeeResult", "$EmployeeResult" },
                        {"AppraiseeResult","$AppraiseeResult" }
                    }
                }
            };
            return new[] { match, project };
        }
    }
}


