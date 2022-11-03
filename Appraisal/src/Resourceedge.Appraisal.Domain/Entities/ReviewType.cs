using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Entities
{
    public class ReviewType
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}
