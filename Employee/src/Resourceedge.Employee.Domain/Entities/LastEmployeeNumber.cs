using MongoDB.Bson;

namespace Resourceedge.Employee.Domain.Entities
{
    public class LastEmployeeNumber
    {
        public ObjectId Id { get; set; }
        public int EmployeeId { get; set; }
    }
}
