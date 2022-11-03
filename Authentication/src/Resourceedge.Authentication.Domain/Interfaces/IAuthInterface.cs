using Resourceedge.Authentication.Domain.Entities;
using Resourceedge.Authentication.Domain.Model;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Resourceedge.Authentication.Domain.Interfaces
{
    public interface IAuthInterface
    {
        Task<ResponseData> GetUserbyEmailAsync(string email);
        Task<ResponseData> Login(LoginViewModel model);
        Task<ResponseData> AddClaimToUser(string userId, IEnumerable<Claim> claims);
        Task<PasswordResetTokenData> GetResetPasswordToken(string email);
        Task<ResponseData> SendResetPasswordEmail(ApplicationUser currentUser, string url);
        Task<ResponseData> ResetUserPassword(ResetPasswordViewModel model);
        public Task<bool> LogoutUser();
    }
}
