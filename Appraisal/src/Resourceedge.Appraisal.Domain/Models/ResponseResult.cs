using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Appraisal.Domain.Models
{
    public class ResponseResult
    {
        public bool Success { get; set; }
        public object Data { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
