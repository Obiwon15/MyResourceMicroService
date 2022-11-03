using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Common.Types
{
    public class ResourceedgeException : Exception
    {
        public string Code { get; }

        public ResourceedgeException()
        {
        }

        public ResourceedgeException(string code)
        {
            Code = code;
        }

        public ResourceedgeException(string message, params object[] args)
            : this(string.Empty, message, args)
        {
        }

        public ResourceedgeException(string code, string message, params object[] args)
            : this(null, code, message, args)
        {
        }

        public ResourceedgeException(Exception innerException, string message, params object[] args)
            : this(innerException, string.Empty, message, args)
        {
        }

        public ResourceedgeException(Exception innerException, string code, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            Code = code;
        }
    }
}
