using Resourceedge.Authentication.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Resourceedge.Authentication.Domain.Model
{
    public class ResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class PasswordResetTokenData
    {
        public bool Success { get; set; }
        public ApplicationUser User { get; set; }
        public string Token { get; set; }
    }
}
