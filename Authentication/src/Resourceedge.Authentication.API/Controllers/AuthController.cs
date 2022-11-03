using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Resourceedge.Authentication.Domain.Interfaces;
using Resourceedge.Authentication.Domain.Model;
using System.Linq;
using System.Threading.Tasks;

namespace Resourceedge.Authentication.API.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthInterface AuthRepo;
        private readonly IIdentityServerInteractionService interactionService;
        public AuthController(IAuthInterface auth, IIdentityServerInteractionService _interactionService)
        {
            this.AuthRepo = auth;
            interactionService = _interactionService;
        }

        public IActionResult VerifyEmail(string ReturnUrl)
        {
            ViewBag.Title = "Verify Email";
            return View( "Login",new VerifyEmail { ReturnUrl = ReturnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmail model)
        {
            ViewBag.Title = "Verify Email";

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Email is required";
                return View("Login" ,model);
            }
            var result = await AuthRepo.GetUserbyEmailAsync(model.Email);
            if (!result.Success)
            {
                ViewData["error"] = result.Message;
                return View("Login", model);
            }
            var TempObject = JsonConvert.SerializeObject(new LoginGetViewModel { UserName = model.Email, ReturnUrl = model.ReturnUrl, Name = result.Message });
            TempData["LoginModel"] = TempObject;
            return RedirectToAction("Authenticate", new { ReturnUrl = model.ReturnUrl });
        }

        public IActionResult Authenticate(string ReturnUrl)
        {
            ViewBag.Title = "Authenticate";

            TempData.TryGetValue("LoginModel", out object savedTempObject);
            if (savedTempObject == null)
            {
                return RedirectToAction("VerifyEmail", new { ReturnUrl });
            }

            var loginModel = JsonConvert.DeserializeObject<LoginGetViewModel>((string)savedTempObject);
            return View("verify",new LoginViewModel { ReturnUrl = ReturnUrl, UserName = loginModel.UserName, Name = loginModel.Name,Email = loginModel.Email});
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate(LoginViewModel model)
        {
            ViewBag.Title = "Authenticate";

            if (!ModelState.IsValid)
            {
                ViewBag.Error = "password is required";
                return View("verify", model);
            }
            var result = await AuthRepo.Login(model);
            if (!result.Success)
            {
                ViewBag.Error = "Username or password incorrect";
                return View("verify", model);
            }

            var claims = User.Claims.ToList();
            return Redirect(model.ReturnUrl);
        }
        
        public IActionResult ResetPassword(string Username, string Token, string ReturnUrl)
        {
            ViewBag.Title = "Reset Password";

            if (string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Token))
            {
                return RedirectToAction("PasswordReset", new { ReturnUrl });
            }

            return View("ChangePassword", new ResetPasswordViewModel { Email = Username, Token = Token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            ViewBag.Title = "Reset Password";
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "password is required";
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Password and Confirm Password must match";
                return View(model);
            }

            var result = await AuthRepo.ResetUserPassword(model);
            if (!result.Success)
            {
                ViewBag.Error = "Password Failed to reset, Try Again";
                return Json(new{ success = false, message = result.Message});
            }

            return Json(new { success = true });
        }

        public IActionResult PasswordReset(string ReturnUrl)
        {
            ViewBag.Title = "Password Reset";

            return View("forgotPassword", new VerifyEmail { ReturnUrl = ReturnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> PasswordReset(VerifyEmail model)
        {
            ViewBag.Title = "Password Reset";

            if (!ModelState.IsValid)
            {
                return RedirectToAction("PasswordReset", new { ReturnUrl = "" });
            }

            var isEmailInDb =await AuthRepo.GetUserbyEmailAsync(model.Email);
            if(!isEmailInDb.Success)
            {
                ViewBag.Error = isEmailInDb.Message;
                return RedirectToAction("PasswordReset", new { ReturnUrl = "" });
            }

            var result = await AuthRepo.GetResetPasswordToken(model.Email);
            if (!result.Success)
            {
                ViewBag.Error = "Email does not exist, Please Enter registered Email";
            }

            var callbackUrl = UrlHelperExtensions.ActionLink(this.Url, "ResetPassword", "Auth", new { Username = result.User.Email, Token = result.Token, ReturnUrl = model.ReturnUrl });
            var res = await AuthRepo.SendResetPasswordEmail(result.User, callbackUrl);

            if (!res.Success)
            {
                ViewBag.Error = "Email Failed to send";
            }

            ViewBag.Success = "Email Sent Successfully";
            return Ok(new VerifyEmail { ReturnUrl = model.ReturnUrl });
        }

        [HttpGet]
        public async Task<IActionResult> Signout(string logoutId)
        {
            await AuthRepo.LogoutUser();
            var logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);
             if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            {
                //test the logout later
                return Redirect("https://resourceedge.netlify.app/");
                //return RedirectToAction("Index", "Home");
            }
            return Redirect(logoutRequest.PostLogoutRedirectUri); 
        }
    }
}
