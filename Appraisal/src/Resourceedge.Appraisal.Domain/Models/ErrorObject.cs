using Newtonsoft.Json;
using Resourceedge.Appraisal.Domain.Enums;

namespace Resourceedge.Appraisal.Domain.Models
{
    public class ErrorObject
    {

        public ResponseStatus status { get; set; }
        public string message { get; set; }
        public object data { get; set; }


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

}
