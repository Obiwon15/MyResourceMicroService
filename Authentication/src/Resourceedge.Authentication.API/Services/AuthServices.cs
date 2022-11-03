using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Resourceedge.Authentication.Domain.Entities;
using Resourceedge.Authentication.Domain.Extensions;
using Resourceedge.Authentication.Domain.Interfaces;
using Resourceedge.Authentication.Domain.Model;
using Resourceedge.Email.Api.Model;
using Resourceedge.Email.Api.SGridClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Resourceedge.Email.Api.Interfaces;

namespace Resourceedge.Authentication.API.Services
{
    public class AuthServices : IAuthInterface
    {
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly EdgeDbContext context;
        private readonly ILogger<AuthServices> Logger;
        private readonly IEmailService sender;

        public AuthServices(SignInManager<ApplicationUser> _signInManager, UserManager<ApplicationUser> usermanager,
            EdgeDbContext _context, ILogger<AuthServices> logger, ISGClient _client, IEmailService _sender)
        {
            SignInManager = _signInManager;
            this.UserManager = usermanager;
            context = _context;
            this.Logger = logger;
            sender = _sender;
        }

        public async Task<ResponseData> AddClaimToUser(string userId, IEnumerable<System.Security.Claims.Claim> claims)
        {
            var result = await UserManager.AddMultipleEdgeClaimAsync(userId, claims, context);
            if (!result)
            {
                return new ResponseData { Success = false, Message = "current specified user does not exist" };
            }

            return new ResponseData { Success = true, Message = "claim(s) added for user" };
        }

        public async Task<ResponseData> GetUserbyEmailAsync(string email)
        {
            try
            {
                TextInfo ti = new CultureInfo("en-US", false).TextInfo;
                var currentUser = await UserManager.FindByEmailAsync(email);
                if (currentUser == null)
                {
                    return new ResponseData { Success = false, Message = "Email does not exist, kindly see your HR for registration" };
                }
                return new ResponseData { Success = true, Message = ti.ToTitleCase(currentUser.FullName.ToLower()) };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseData> Login(LoginViewModel model)
        {
            var currentuser = await UserManager.FindByEmailAsync(model.UserName);
            if (currentuser == null)
            {
                return new ResponseData { Success = false, Message = "Email or password incorrect" };
            }

            var result = await SignInManager.PasswordSignInAsync(currentuser.UserName, model.Password, false, false);
            if (result.Succeeded)
            {
                return new ResponseData { Success = result.Succeeded, Message = "sign in successful" };
            }
            return new ResponseData { Success = false, Message = "Email or password incorrect" };
        }

        public async Task<PasswordResetTokenData> GetResetPasswordToken(string email)
        {
            try
            {
                var currentUser = await UserManager.FindByEmailAsync(email);
                if (currentUser == null)
                {
                    return new PasswordResetTokenData { Success = false, User = null, Token = "Email does not exist, kindly see your HR for registration" };
                }

                var token = await UserManager.GeneratePasswordResetTokenAsync(currentUser);


                return new PasswordResetTokenData { Success = true, User = currentUser, Token = token };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<ResponseData> SendResetPasswordEmail(ApplicationUser currentUser, string url)
        {
            try
            {
                string subject = "Reset Password";
                SingleEmailDto emailDto = new SingleEmailDto()
                {
                    ReceiverEmailAddress = currentUser.Email,
                    ReceiverFullName = currentUser.FullName,
                    HtmlContent = await sender.FormatEmail(currentUser.FirstName, url)
                };

                if (emailDto.HtmlContent == null)
                {
                    emailDto.HtmlContent = url;
                }
                var res = await sender.SendToSingleEmployee(subject, emailDto);
                if (res == HttpStatusCode.Accepted)
                    return new ResponseData { Success = true, Message = res.ToString() };
                else
                    return new ResponseData { Success = false, Message = res.ToString() };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ResponseData> ResetUserPassword(ResetPasswordViewModel model)
        {
            try
            {
                var currentUser = await UserManager.FindByEmailAsync(model.Email);
                if (currentUser == null)
                {
                    return new ResponseData { Success = false, Message = "user not found" };

                }

                var res = await UserManager.ResetPasswordAsync(currentUser, model.Token, model.Password);
                if (res.Succeeded)
                {
                    try
                    {
                        const string subject = "Reset Password";
                        SingleEmailDto emailDto = new SingleEmailDto
                        {
                            ReceiverEmailAddress = currentUser.Email,
                            ReceiverFullName = currentUser.FullName,
                            HtmlContent = await sender.FormatSuccessfulResetPasswordEmail(currentUser.FirstName)
                        };

                        var response = await sender.SendToSingleEmployee(subject, emailDto);
                        return response == HttpStatusCode.Accepted
                            ? new ResponseData { Success = true, Message = res.ToString() }
                            : new ResponseData { Success = false, Message = res.ToString() };
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }

                return new ResponseData { Success = false, Message = res.ToString() };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> LogoutUser()
        {
            await SignInManager.SignOutAsync();
            return await Task.FromResult(true);
        }
    }
}
