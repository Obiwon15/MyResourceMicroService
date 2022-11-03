using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Operations.Api.Dto
{
    public class OperationDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string Resource { get; set; }
        public string Code { get; set; }
        public string Reason { get; set; }
    }
}
